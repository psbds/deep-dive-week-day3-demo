using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using static BotLuis.CognitiveModels.FlightBooking;

namespace BotLuis.Dialogs
{
	public class MainDialog : ComponentDialog
	{
		private readonly LuistIntentRecognizer _luisRecognizer;
		protected readonly ILogger Logger;

		// Dependency injection uses this constructor to instantiate MainDialog
		public MainDialog(LuistIntentRecognizer luisRecognizer, BookingDialog bookingDialog, ILogger<MainDialog> logger) : base(nameof(MainDialog))
		{
			_luisRecognizer = luisRecognizer;
			Logger = logger;

			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(bookingDialog);
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				ActStepAsync,
				FinalStepAsync,
			}));

			InitialDialogId = nameof(WaterfallDialog);
		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
		}

		private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
			switch (luisResult.GetTopScoringIntent().intent)
			{
				case "search":
					var entities = JsonConvert.DeserializeObject<_Entities>(luisResult.Entities.ToString());
					var destination = entities._instance.city?.Length > 0 ? entities._instance.city[0].Text : null;
					return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(destination), cancellationToken);

				case "check":
					if (!BookingDetails.Bookings.ContainsKey(stepContext.Context.Activity.From.Id) || BookingDetails.Bookings[stepContext.Context.Activity.From.Id].Count == 0)
					{
						var message = MessageFactory.Text("Você não tem nenhuma viagem planejada", InputHints.IgnoringInput);
						await stepContext.Context.SendActivityAsync(message, cancellationToken);
					}
					else
					{
						var message = MessageFactory.Text("Aqui estão suas viagens planejadas", InputHints.IgnoringInput);
						await stepContext.Context.SendActivityAsync(message, cancellationToken);

						foreach (var booking in BookingDetails.Bookings[stepContext.Context.Activity.From.Id])
						{
							var bookingMessage = MessageFactory.Text($"> {booking.Origin} para {booking.Destination}", InputHints.IgnoringInput);
							await stepContext.Context.SendActivityAsync(bookingMessage, cancellationToken);
						}
					}
					break;

				default:
					var didntUnderstandMessageText = $"Desculpe, não entendi.";
					var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
					await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
					break;
			}

			return await stepContext.NextAsync(null, cancellationToken);
		}



		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var message = MessageFactory.Text("No que mais posso ajudar?", InputHints.IgnoringInput);
			await stepContext.Context.SendActivityAsync(message, cancellationToken);
			return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
		}
	}
}

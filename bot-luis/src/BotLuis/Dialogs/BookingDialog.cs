// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.10.3

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace BotLuis.Dialogs
{
	public class BookingDialog : ComponentDialog
	{

		public BookingDialog() : base(nameof(BookingDialog))
		{
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				DestinationStepAsync,
				OriginStepAsync,
				ConfirmStepAsync,
				FinalStepAsync,
			}));

			InitialDialogId = nameof(WaterfallDialog);
		}

		private async Task<DialogTurnResult> DestinationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var bookingDetails = (BookingDetails)stepContext.Options;

			if (bookingDetails.Destination == null)
			{
				var promptMessage = MessageFactory.Text("Para onde você deseja viajar?", InputHints.ExpectingInput);
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
			}

			return await stepContext.NextAsync(bookingDetails.Destination, cancellationToken);
		}

		private async Task<DialogTurnResult> OriginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var bookingDetails = (BookingDetails)stepContext.Options;

			bookingDetails.Destination = (string)stepContext.Result;

			if (bookingDetails.Origin == null)
			{
				var promptMessage = MessageFactory.Text("De onde você deseja partir?", InputHints.ExpectingInput);
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
			}

			return await stepContext.NextAsync(bookingDetails.Origin, cancellationToken);
		}


		private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var bookingDetails = (BookingDetails)stepContext.Options;
			bookingDetails.Origin = (string)stepContext.Result;

			var messageText = $"Por Favor confirme, você quer viajar de {bookingDetails.Origin} para {bookingDetails.Destination}. Esta correto?";
			var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

			return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			if ((bool)stepContext.Result)
			{
				var bookingDetails = (BookingDetails)stepContext.Options;
				var messageText = $"Você reservou uma passagem de {bookingDetails.Origin} para {bookingDetails.Destination}";
				var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
				await stepContext.Context.SendActivityAsync(message, cancellationToken);


				if (!BookingDetails.Bookings.ContainsKey(stepContext.Context.Activity.From.Id))
				{
					BookingDetails.Bookings.Add(stepContext.Context.Activity.From.Id, new List<BookingDetails>());
				}

				BookingDetails.Bookings[stepContext.Context.Activity.From.Id].Add(bookingDetails);

				return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
			}

			return await stepContext.EndDialogAsync(null, cancellationToken);
		}
	}
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.10.3

using System.Collections.Generic;

namespace BotLuis
{
	public class BookingDetails
	{
		public static Dictionary<string, List<BookingDetails>> Bookings = new Dictionary<string, List<BookingDetails>>();

		public BookingDetails(string destination)
		{
			Destination = destination;
		}

		public BookingDetails()
		{
		}

		public string Destination { get; set; }

		public string Origin { get; set; }
	}
}

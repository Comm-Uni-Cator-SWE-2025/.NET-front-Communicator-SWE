using MeetAnalyticsApp.Models;
using System;
using System.Threading.Tasks;

namespace MeetAnalyticsApp.Services
{
    /// <summary>
    /// Provides meeting analytics data such as user counts and previous meeting summaries.
    /// This is a mock service — replace it with real API integration later.
    /// </summary>
    public class MeetAnalyticsService
    {
        private readonly Random _random = new();

        /// <summary>
        /// Retrieves the latest meeting statistics including number of present users,
        /// logged-out users, and a previous meeting summary.
        /// </summary>
        /// <returns>A <see cref="MeetStats"/> object filled with mock data.</returns>
        public Task<MeetStats> FetchStatsAsync()
        {
            var stats = new MeetStats
            {
                UsersPresent = _random.Next(50, 2000),
                UsersLoggedOut = _random.Next(0, 1000),
                PreviousSummary = "Participants reported high satisfaction; action items assigned to design and backend."
            };

            return Task.FromResult(stats);
        }

        /// <summary>
        /// Retrieves sample chat or announcement messages.
        /// This mock method returns pre-defined messages.
        /// </summary>
        /// <returns>An array of message strings.</returns>
        public Task<string[]> FetchMessagesAsync()
        {
            string[] messages =
            {
                "Welcome everyone!",
                "Please mute when not speaking.",
                "QA will begin regression testing tomorrow.",
                "Designer will provide mockups by Wednesday.",
                "Client meeting scheduled for Friday at 3 PM."
            };

            return Task.FromResult(messages);
        }
    }
}

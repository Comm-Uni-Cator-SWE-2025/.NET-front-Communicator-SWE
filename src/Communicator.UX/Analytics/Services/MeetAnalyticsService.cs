using System.Collections.Generic;
using System.Threading.Tasks;
using Communicator.UX.Analytics.Models;

namespace Communicator.UX.Analytics.Services;

public class MeetAnalyticsService
{
    public Task<MeetStats> FetchStatsAsync()
    {
        return Task.FromResult(new MeetStats
        {
            UsersPresent = 120,
            UsersLoggedOut = 15,
            PreviousSummary = "The previous meeting was very productive and tasks were assigned."
        });
    }

    public Task<List<string>> FetchMessagesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "Welcome to the meeting",
            "Design update will be shared soon",
            "Team is progressing well"
        });
    }
}

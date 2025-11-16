using Communicator.Meeting;

namespace Communicator.Analytics;

public class UserAnalytics
{
    private readonly List<UserProfile> _users = [];

    public void FetchUsersFromCloud()
    {
        _users.Add(new UserProfile {
            DisplayName = "Rahul Sharma",
            LogoUrl = "https://picsum.photos/200",
            JoinTime = "10:45 AM"
        });

        _users.Add(new UserProfile {
            DisplayName = "Aisha Gupta",
            LogoUrl = "https://picsum.photos/201",
            JoinTime = "10:46 AM"
        });

        _users.Add(new UserProfile {
            DisplayName = "Sisha Gupta",
            LogoUrl = "https://picsum.photos/201",
            JoinTime = "10:46 AM"
        });
        _users.Add(new UserProfile {
            DisplayName = "Rahul Sharma",
            LogoUrl = "https://picsum.photos/200",
            JoinTime = "10:45 AM"
        });
    }

    public List<UserProfile> GetAllUsers()
    {
        return _users;
    }
}


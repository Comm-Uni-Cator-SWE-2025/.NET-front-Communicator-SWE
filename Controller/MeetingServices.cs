using System.Collections.Generic;

namespace Controller
{
    public class MeetingServices
    {
        private readonly Dictionary<string, MeetingSession> _meetings = new Dictionary<string, MeetingSession>();

        public MeetingSession CreateMeeting(UserProfile user)
        {
            if (user.Role != "instructor")
            {
                System.Console.WriteLine($"User is not an instructor: {user.Role}");
                return null;
            }
            var meeting = new MeetingSession(user.Email);
            _meetings.Add(meeting.MeetingId, meeting);
            return meeting;
        }

        public bool JoinMeeting(UserProfile user, string meetingId, ClientNode clientNode, ClientNode deviceNode, IController networkController)
        {
            networkController.AddUser(deviceNode, clientNode);
            return _meetings.ContainsKey(meetingId);
        }
    }
}

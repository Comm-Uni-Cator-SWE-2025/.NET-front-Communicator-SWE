// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Controller;

public class MeetingServices
{
    private readonly Dictionary<string, MeetingSession> _meetings = new Dictionary<string, MeetingSession>();

    public MeetingSession? CreateMeeting(User user)
    {
        var meeting = new MeetingSession(user.Email);
        _meetings.Add(meeting.MeetingId, meeting);
        return meeting;
    }

    public bool JoinMeeting(User user, string meetingId, ClientNode clientNode, ClientNode deviceNode, IController networkController)
    {
        networkController.AddUser(deviceNode, clientNode);
        return _meetings.ContainsKey(meetingId);
    }
}

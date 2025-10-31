using System;

namespace Controller
{
    public class MeetingSession
    {
        public string MeetingId { get; }
        public string CreatedBy { get; }
        public long CreatedAt { get; }

        public MeetingSession(string createdBy)
        {
            MeetingId = Guid.NewGuid().ToString();
            CreatedBy = createdBy;
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}

using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using System;

namespace Communicator.App.Tests.UI.Pages
{
    public class HomePage
    {
        private readonly Window _window;

        public HomePage(Window window)
        {
            _window = window;
        }

        public TextBox MeetingLinkInput => _window.FindFirstDescendant(cf => cf.ByAutomationId("MeetingLinkInput"))?.AsTextBox();
        public Button JoinMeetingButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("JoinMeetingButton"))?.AsButton();
        public Button CreateMeetingButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("CreateMeetingButton"))?.AsButton();

        public bool IsVisible()
        {
            return MeetingLinkInput != null && JoinMeetingButton != null;
        }

        public MeetingPage JoinMeeting(string meetingId)
        {
            MeetingLinkInput.Text = meetingId;
            JoinMeetingButton.Click();

            var meetingPage = new MeetingPage(_window);
            Retry.WhileFalse(() => meetingPage.IsVisible(), TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
            return meetingPage;
        }

        public MeetingPage CreateMeeting()
        {
            CreateMeetingButton.Click();
            
            var meetingPage = new MeetingPage(_window);
            Retry.WhileFalse(() => meetingPage.IsVisible(), TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
            return meetingPage;
        }
    }
}

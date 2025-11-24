using FlaUI.Core.AutomationElements;

namespace Communicator.App.Tests.UI.Pages
{
    public class MeetingPage
    {
        private readonly Window _window;

        public MeetingPage(Window window)
        {
            _window = window;
        }

        public Button LeaveMeetingButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("LeaveMeetingButton"))?.AsButton();
        public Button ToggleMuteButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleMuteButton"))?.AsButton();
        public Button ToggleCameraButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleCameraButton"))?.AsButton();
        public Button RaiseHandButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("RaiseHandButton"))?.AsButton();
        public Button ToggleScreenShareButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleScreenShareButton"))?.AsButton();
        public Button ToggleChatButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleChatButton"))?.AsButton();
        public Button ToggleParticipantsButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("ToggleParticipantsButton"))?.AsButton();

        public bool IsVisible()
        {
            return LeaveMeetingButton != null;
        }

        public void LeaveMeeting()
        {
            LeaveMeetingButton?.Click();
        }

        public void ToggleMute()
        {
            ToggleMuteButton?.Click();
        }

        public void ToggleCamera()
        {
            ToggleCameraButton?.Click();
        }
    }
}

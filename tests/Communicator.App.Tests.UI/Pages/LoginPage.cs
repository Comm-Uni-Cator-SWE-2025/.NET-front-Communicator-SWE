using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using System;

namespace Communicator.App.Tests.UI.Pages
{
    public class LoginPage
    {
        private readonly Window _window;

        public LoginPage(Window window)
        {
            _window = window;
        }

        public Button SignInButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("SignInButton"))?.AsButton();
        public Button DebugLoginButton => _window.FindFirstDescendant(cf => cf.ByAutomationId("DebugLoginButton"))?.AsButton();

        public bool IsVisible()
        {
            return SignInButton != null;
        }

        public HomePage LoginAsDebugUser()
        {
            var debugButton = DebugLoginButton;
            if (debugButton == null)
            {
                throw new InvalidOperationException("Debug Login button not found. Ensure the app is built in Debug mode.");
            }
            debugButton.Click();
            
            // Wait for Home Page to appear
            var homePage = new HomePage(_window);
            Retry.WhileFalse(() => homePage.IsVisible(), TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
            
            return homePage;
        }
    }
}

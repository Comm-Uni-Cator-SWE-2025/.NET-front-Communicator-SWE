using System;

namespace Controller
{
    public class MockController : IController
    {
        private readonly AuthService _authService;
        private UserProfile? _currentUser;

        public MockController()
        {
            _authService = new AuthService();
            SeedUsers();
        }

        private void SeedUsers()
        {
            _authService.Register("lecturer@iitpkd.ac.in", "password", "Lecturer");
            _authService.Register("student@smail.iitpkd.ac.in", "password", "Student");
        }

        public bool Login(string email, string password)
        {
            var user = _authService.Login(email, password);
            if (user == null)
            {
                return false;
            }

            _currentUser = user;
            return true;
        }

        public bool SignUp(string displayName, string email, string password)
        {
            var user = _authService.Register(email, password, displayName);
            if (user == null)
            {
                return false;
            }

            _currentUser = user;
            return true;
        }

        public UserProfile? GetUser() => _currentUser;

        public void AddUser(ClientNode deviceNode, ClientNode clientNode)
        {
            // Dummy implementation
        }
    }
}

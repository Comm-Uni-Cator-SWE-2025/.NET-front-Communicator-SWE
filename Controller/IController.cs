namespace Controller
{
    public interface IController
    {
        void AddUser(ClientNode deviceNode, ClientNode clientNode);
        UserProfile? GetUser();
        bool Login(string email, string password);
        bool SignUp(string displayName, string email, string password);
    }
}

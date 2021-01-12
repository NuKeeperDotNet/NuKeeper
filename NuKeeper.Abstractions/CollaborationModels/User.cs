namespace NuKeeper.Abstractions.CollaborationModels
{
    public class User
    {
        public static readonly User Default = new User("user@email.com", "", "");

        public User(string login, string name, string email)
        {
            Login = login;
            Name = name;
            Email = email;
        }

        public string Login { get; }
        public string Name { get; }
        public string Email { get; }
    }
}

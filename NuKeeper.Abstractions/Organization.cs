namespace NuKeeper.Abstractions
{
    public class Organization
    {
        public Organization(string name, string login)
        {
            Name = name;
            Login = login;
        }

        public string Name { get; }
        public string Login { get; }
    }
}

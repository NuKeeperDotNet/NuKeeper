namespace NuKeeper.Abstractions.CollaborationModels
{
    public class Organization
    {
        public Organization(string name, string login)
        {
            Name = name;
            Login = login;
        }

        public string Name { get; set; }
        public string Login { get; set; }
    }
}

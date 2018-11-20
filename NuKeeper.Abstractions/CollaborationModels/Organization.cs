namespace NuKeeper.Abstractions.CollaborationModels
{
    public class Organization
    {
        public Organization(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}

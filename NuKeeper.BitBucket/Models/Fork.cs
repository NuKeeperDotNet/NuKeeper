namespace NuKeeper.BitBucket.Models
{
    public class Fork
    {
        public string scm { get; set; }
        public bool? has_wiki { get; set; }
        public string description { get; set; }
        public Links links { get; set; }
        public string fork_policy { get; set; }
        public string language { get; set; }
        public string created_on { get; set; }
        public Parent parent { get; set; }
        public string full_name { get; set; }
        public bool? has_issues { get; set; }
        public Owner owner { get; set; }
        public string updated_on { get; set; }
        public ulong? size { get; set; }
        public bool? is_private { get; set; }
        public string name { get; set; }
    }
}

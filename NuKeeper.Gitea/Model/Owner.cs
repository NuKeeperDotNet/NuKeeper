namespace NuKeeper.Gitea.Model
{
    public class Owner
    {
        public int id { get; set; }
        public string login { get; set; }
        public string full_name { get; set; }
        public string email { get; set; }
        public string avatar_url { get; set; }
        public string language { get; set; }
        public bool is_admin { get; set; }
        public string username { get; set; }
    }
}

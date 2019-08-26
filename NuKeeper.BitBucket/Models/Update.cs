namespace NuKeeper.BitBucket.Models
{
    public class Update
    {
        public string description { get; set; }
        public string title { get; set; }
        public Source destination { get; set; }
        public string reason { get; set; }
        public Source source { get; set; }
        public string state { get; set; }
        public User author { get; set; }
        public string date { get; set; }
    }
}

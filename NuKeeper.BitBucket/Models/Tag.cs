namespace NuKeeper.BitBucket.Models
{
    public class Tag
    {
        public string name { get; set; }
        public object tagger { get; set; }
        public object date { get; set; }
        public object message { get; set; }
    }
}
namespace NuKeeper.BitBucket.Models
{
    public class Comment
    {
        public Parent parent { get; set; }
        public Links links { get; set; }
        public Content content { get; set; }
        public string created_on { get; set; }
        public User user { get; set; }
        public string updated_on { get; set; }
        public int? id { get; set; }
    }
}

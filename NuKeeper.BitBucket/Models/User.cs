namespace NuKeeper.BitBucket.Models
{
    public class User
    {
        public string username { get; set; }
        public string kind { get; set; }
        public string website { get; set; }
        public string display_name { get; set; }
        public Links links { get; set; }
        public string created_on { get; set; }
        public string location { get; set; }
    }
}
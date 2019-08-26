namespace NuKeeper.BitBucket.Models
{
    public class Source
    {
        public Commit commit { get; set; }
        public Repository repository { get; set; }
        public Branch branch { get; set; }
    }
}

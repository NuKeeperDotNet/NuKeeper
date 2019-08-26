using System.Collections.Generic;

namespace NuKeeper.BitBucket.Models
{
    public class Commit
    {
        public string hash { get; set; }
        public Links links { get; set; }
        public Repository repository { get; set; }
        public Author author { get; set; }
        public List<User> participants { get; set; }
        public List<Parent> parents { get; set; }
        public string date { get; set; }
        public string message { get; set; }
    }
}

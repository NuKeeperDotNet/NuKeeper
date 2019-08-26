using System.Collections.Generic;

namespace NuKeeper.BitBucket.Models
{
    public class BranchRestriction
    {
        public List<object> groups { get; set; }
        public int? id { get; set; }
        public string kind { get; set; }
        public List<Link> links { get; set; }
        public string pattern { get; set; }
        public List<User> users { get; set; }
    }
}

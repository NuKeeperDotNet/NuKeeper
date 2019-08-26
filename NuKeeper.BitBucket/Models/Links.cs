using System.Collections.Generic;

namespace NuKeeper.BitBucket.Models
{
    public class Links
    {
        public Link self { get; set; }
        public Link repositories { get; set; }
        public Link link { get; set; }
        public Link followers { get; set; }
        public Link avatar { get; set; }
        public Link following { get; set; }
        public List<Link> clone { get; set; }
        public Link html { get; set; }
    }
}

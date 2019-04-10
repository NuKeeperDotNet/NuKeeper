using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class ForkInfo
    {
        public ForkInfo(string organizationName)
        {
            Organization = organizationName;
        }

        [JsonProperty("organization")]
        public string Organization { get; set; }
    }
}

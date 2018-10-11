using System.Globalization;
using Newtonsoft.Json.Serialization;

namespace NuKeeper
{
#pragma warning disable CA1308

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower(CultureInfo.InvariantCulture);
        }
    }
}

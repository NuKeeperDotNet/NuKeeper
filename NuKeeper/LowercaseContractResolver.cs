using System;
using System.Globalization;
using Newtonsoft.Json.Serialization;

namespace NuKeeper
{
#pragma warning disable CA1308

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return propertyName.ToLower(CultureInfo.InvariantCulture);
        }
    }
}

using System;

namespace NuKeeper.Configuration
{
    public static class EnumParser
    {
        public static T? Parse<T>(string value) where T: struct 
        {
            var success = Enum.TryParse(value, true, out T result);
            if (success)
            {
                return result;
            }
            return null;
        }
    }
}

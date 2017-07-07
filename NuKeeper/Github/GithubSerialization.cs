using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NuKeeper.Github
{
    public static class GithubSerialization
    {
        private static readonly JsonSerializerSettings SerializerSettings;

        static GithubSerialization()
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            };
        }

        public static string SerializeObject(object input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return JsonConvert.SerializeObject(input, SerializerSettings);
        }

        public static T DeserializeObject<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, SerializerSettings);
        }
    }
}
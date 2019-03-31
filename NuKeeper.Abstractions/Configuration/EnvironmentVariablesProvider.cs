using System;

namespace NuKeeper.Abstractions.Configuration
{
    public class EnvironmentVariablesProvider : IEnvironmentVariablesProvider
    {
        public string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        public string GetUserName()
        {
            return Environment.UserName;
        }
    }
}

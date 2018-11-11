using System;
using NUnit.Framework;
using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;

namespace Nukeeper.AzureDevOps.Tests
{
    public class AzureDevOpsPlatformTests
    {
        [Test]
        public void Initialise()
        {
            var platform = new AzureDevOpsPlatform(Substitute.For<INuKeeperLogger>());
            platform.Initialise(new AuthSettings(new Uri("https://uri.com"),"token"));
        }
    }
}

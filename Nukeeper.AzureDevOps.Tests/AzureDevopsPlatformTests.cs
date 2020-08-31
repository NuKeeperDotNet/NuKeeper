using System;
using System.Net.Http;
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
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(new HttpClient());

            var platform = new AzureDevOpsPlatform(Substitute.For<INuKeeperLogger>(), httpClientFactory);
            platform.Initialise(new AuthSettings(new Uri("https://uri.com"), "token"));
        }
    }
}

using System;
using NuKeeper.Abstractions.Formats;
using NUnit.Framework;

namespace NuKeeper.Abstractions.Tests.Formats
{
    [TestFixture]
    public class UriFormatsTests
    {
        [Test]
        public void TrailingSlashIsKeptWhenPresent()
        {
            var input = new Uri("http://test.com/api/path/");

            var output = UriFormats.EnsureTrailingSlash(input);

            Assert.That(output.ToString(), Is.EqualTo("http://test.com/api/path/"));
        }

        [Test]
        public void TrailingSlashIsAddedWhenMissing()
        {
            var input = new Uri("http://test.com/api/path");

            var output = UriFormats.EnsureTrailingSlash(input);

            Assert.That(output.ToString(), Is.EqualTo("http://test.com/api/path/"));
        }

    }
}

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
        
        [Test]
        public void IsLocalUri()
        {
            var input = ".";
            var output = input.ToUri();

            Assert.That(output.IsFile, Is.EqualTo(true));
        }
        
        [Test]
        public void IsRemoteUri()
        {
            var input = "https://www.google.com";
            var output = input.ToUri();
            
            Assert.That(output.Host, Is.EqualTo("www.google.com"));
        }
        
        [Test]
        public void IsNonExistingUri()
        {
            var input = "../../../invalidpath/test/1234/abcde";
            
            Assert.That(() => input.ToUri(), 
                Throws.Exception
                    .TypeOf<NuKeeperException>());
        }
    }
}

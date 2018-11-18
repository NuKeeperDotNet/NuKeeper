using NuKeeper.Abstractions.Formats;
using NUnit.Framework;

namespace NuKeeper.Abstractions.Tests.Formats
{
    [TestFixture]
    public class HasherTests
    {
        [Test]
        public void CanHash()
        {
            var output = Hasher.Hash("test");

            Assert.That(output, Is.EqualTo("098F6BCD4621D373CADE4E832627B4F6"));
        }

        [Test]
        public void HashIsSameTwice()
        {
            const string input = "testabcd1234";
            var output1 = Hasher.Hash(input);
            var output2 = Hasher.Hash(input);

            Assert.That(output1, Is.EqualTo(output2));
        }
    }
}

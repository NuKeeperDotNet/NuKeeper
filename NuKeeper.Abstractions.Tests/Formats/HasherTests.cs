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

            AssertHashFormat(output);
            Assert.That(output, Is.EqualTo("098F6BCD4621D373CADE4E832627B4F6"));
        }

        [Test]
        public void HashIsSameTwice()
        {
            const string input = "testabcd1234";
            var output1 = Hasher.Hash(input);
            var output2 = Hasher.Hash(input);

            AssertHashFormat(output1);
            AssertHashFormat(output2);

            Assert.That(output1, Is.EqualTo(output2));
        }

        private static void AssertHashFormat(string hash)
        {
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash, Is.Not.Empty);
            Assert.That(hash.Length, Is.EqualTo(32));
        }
    }
}

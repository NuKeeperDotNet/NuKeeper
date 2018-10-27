using NUnit.Framework;
using NuKeeper.Abstract.Engine;

namespace NuKeeper.Tests.Engine
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
    }
}

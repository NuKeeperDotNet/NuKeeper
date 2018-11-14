using NuKeeper.Abstractions.Formats;
using NUnit.Framework;

namespace NuKeeper.Abstractions.Tests.Formats
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase("foobar", "foo")]
        [TestCase("foobar", "FOO")]
        [TestCase("FOOBAR", "foo")]
        [TestCase("FoObAr", "foo")]
        [TestCase("foobar", "bar")]
        [TestCase("foobar", "ob")]
        public void DoesContainOrdinal(string value, string substring)
        {
            Assert.That(value.ContainsOrdinal(substring));
        }

        [TestCase("foobar", "x")]
        [TestCase("", "bar")]
        [TestCase(null, "a")]
        [TestCase("foobar", "foobarfish")]
        public void DoesNotContainOrdinal(string value, string substring)
        {
            Assert.That(value.ContainsOrdinal(substring), Is.False);
        }
    }
}

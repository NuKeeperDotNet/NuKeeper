using System;
using NUnit.Framework;

namespace NuKeeper.Tests
{
    [TestFixture]
    public class DurationParserTests
    {
        [Test]
        public void NullStringIsNotParsed()
        {
            var value = DurationParser.Parse(null);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void EmptyStringIsNotParsed()
        {
            var value = DurationParser.Parse(string.Empty);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void BadStringIsNotParsed()
        {
            var value = DurationParser.Parse("ghoti");
            Assert.That(value, Is.Null);
        }

        [Test]
        public void UnknownDurationTypeIsNotParsed()
        {
            var value = DurationParser.Parse("37x");
            Assert.That(value, Is.Null);
        }

        [TestCase("1d", 1)]
        [TestCase("3d", 3)]
        [TestCase("12d", 12)]
        [TestCase("123d", 123)]
        public void DaysAreParsed(string input, int expectedDays)
        {
            var parsed = DurationParser.Parse(input);
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.Value, Is.EqualTo(TimeSpan.FromDays(expectedDays)));
        }

        [TestCase("1h", 1)]
        [TestCase("3h", 3)]
        [TestCase("12h", 12)]
        [TestCase("123h", 123)]
        public void HoursAreParsed(string input, int expectedHours)
        {
            var parsed = DurationParser.Parse(input);
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.Value, Is.EqualTo(TimeSpan.FromHours(expectedHours)));
        }

        [TestCase("1w", 1)]
        [TestCase("3w", 3)]
        [TestCase("12w", 12)]
        [TestCase("123w", 123)]
        public void WeeksAreParsed(string input, int expectedWeeks)
        {
            var parsed = DurationParser.Parse(input);
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.Value, Is.EqualTo(TimeSpan.FromDays(expectedWeeks * 7)));
        }
    }
}

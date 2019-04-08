using System;
using NuKeeper.Abstractions.Formats;
using NUnit.Framework;

namespace NuKeeper.Abstractions.Tests.Formats
{
    [TestFixture]
    public class TimespanFormatTests
    {
        [Test]
        public void TestSomeSeconds()
        {
            var duration = new TimeSpan(0, 0, 32);

            var result = TimeSpanFormat.Ago(duration);

            Assert.That(result, Is.EqualTo("32 seconds ago"));
        }

        [Test]
        public void TestSomeMinutes()
        {
            var duration = new TimeSpan(0, 12, 3);

            var result = TimeSpanFormat.Ago(duration);

            Assert.That(result, Is.EqualTo("12 minutes ago"));
        }

        [Test]
        public void TestAnHour()
        {
            var duration = new TimeSpan(1, 1, 1);

            var result = TimeSpanFormat.Ago(duration);

            Assert.That(result, Is.EqualTo("1 hour ago"));
        }

        [Test]
        public void TestSomeHours()
        {
            var duration = new TimeSpan(9, 12, 3);

            var result = TimeSpanFormat.Ago(duration);

            Assert.That(result, Is.EqualTo("9 hours ago"));
        }

        [Test]
        public void TestSomeDays()
        {
            var duration = new TimeSpan(12, 9, 12, 3);

            var result = TimeSpanFormat.Ago(duration);

            Assert.That(result, Is.EqualTo("12 days ago"));
        }

        [Test]
        public void TestTwoDatesSeperatedByOneDays()
        {
            var end = DateTime.UtcNow;
            var start = end.AddDays(-1);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("1 day ago"));
        }

        [Test]
        public void TestTwoDatesSeperatedByTwoDays()
        {
            var end = DateTime.UtcNow;
            var start = end.AddDays(-2);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("2 days ago"));
        }

        [Test]
        public void TestTwoDatesYearsApart()
        {
            var end = DateTime.UtcNow;
            var start = end.AddYears(-2);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("2 years ago"));
        }

        [Test]
        public void TestTwoDatesMonthsApart()
        {
            var end = DateTime.UtcNow;
            var start = end.AddMonths(-4);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("4 months ago"));
        }

        [Test]
        public void TestMonthStart()
        {
            var end = new DateTime(2018, 2, 1);
            var start = end.AddDays(-1);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("1 day ago"));
        }

        [Test]
        public void TestYearStart()
        {
            var end = new DateTime(2018, 1, 1);
            var start = end.AddDays(-1);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("1 day ago"));
        }

        [Test]
        public void TestTwoDatesTenMonthsApart()
        {
            var end = new DateTime(2018, 4, 5);
            var start = end.AddMonths(-10);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("10 months ago"));
        }

        [Test]
        public void TestTwoDatesFourteenApart()
        {
            var end = new DateTime(2018, 3, 4);
            var start = end.AddMonths(-14);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("1 year and 2 months ago"));
        }


        [Test]
        public void TestTwoDatesYearsAndMonthsApart()
        {
            var end = DateTime.UtcNow;
            var start = end
                .AddYears(-2)
                .AddMonths(-10);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("2 years and 10 months ago"));
        }

        [Test]
        public void TestTwoDatesThreeYearsAndOneMonthApart()
        {
            var end = DateTime.UtcNow;
            var start = end
                .AddYears(-3)
                .AddMonths(-1);

            var result = TimeSpanFormat.Ago(start, end);

            Assert.That(result, Is.EqualTo("3 years and 1 month ago"));
        }
    }
}

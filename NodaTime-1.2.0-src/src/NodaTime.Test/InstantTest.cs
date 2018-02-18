// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using NUnit.Framework;
using NodaTime.Calendars;
using NodaTime.Text;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class InstantTest
    {
        private const long Y2002Days =
            365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 +
            365 + 366 + 365 + 365 + 365 + 366 + 365;

        private const long Y2003Days =
            365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 +
            365 + 366 + 365 + 365 + 365 + 366 + 365 + 365;

        // 2002-04-05
        private const long TestTime1 =
            (Y2002Days + 31L + 28L + 31L + 5L - 1L) * NodaConstants.MillisecondsPerStandardDay + 12L * NodaConstants.MillisecondsPerHour +
            24L * NodaConstants.MillisecondsPerMinute;

        // 2003-05-06
        private const long TestTime2 =
            (Y2003Days + 31L + 28L + 31L + 30L + 6L - 1L) * NodaConstants.MillisecondsPerStandardDay + 14L * NodaConstants.MillisecondsPerHour +
            28L * NodaConstants.MillisecondsPerMinute;

        private Instant one = new Instant(1L);
        private readonly Instant onePrime = new Instant(1L);
        private Instant negativeOne = new Instant(-1L);
        private Instant threeMillion = new Instant(3000000L);
        private Instant negativeFiftyMillion = new Instant(-50000000L);

        private readonly Duration durationNegativeEpsilon = Duration.FromTicks(-1L);
        private readonly Offset offsetOneHour = Offset.FromHours(1);

        [Test]
        public void TestInstantOperators()
        {
            const long diff = TestTime2 - TestTime1;

            var time1 = new Instant(TestTime1);
            var time2 = new Instant(TestTime2);
            Duration duration = time2 - time1;

            Assert.AreEqual(diff, duration.Ticks);
            Assert.AreEqual(TestTime2, (time1 + duration).Ticks);
            Assert.AreEqual(TestTime1, (time2 - duration).Ticks);
        }

        [Test]
        public void FromUtcNoSeconds()
        {
            Instant viaUtc = DateTimeZone.Utc.AtStrictly(new LocalDateTime(2008, 4, 3, 10, 35, 0)).ToInstant();
            Assert.AreEqual(viaUtc, Instant.FromUtc(2008, 4, 3, 10, 35));
        }

        [Test]
        public void FromUtcWithSeconds()
        {
            Instant viaUtc = DateTimeZone.Utc.AtStrictly(new LocalDateTime(2008, 4, 3, 10, 35, 23)).ToInstant();
            Assert.AreEqual(viaUtc, Instant.FromUtc(2008, 4, 3, 10, 35, 23));
        }

        [Test]
        public void InUtc()
        {
            ZonedDateTime viaInstant = Instant.FromUtc(2008, 4, 3, 10, 35, 23).InUtc();
            ZonedDateTime expected = DateTimeZone.Utc.AtStrictly(new LocalDateTime(2008, 4, 3, 10, 35, 23));
            Assert.AreEqual(expected, viaInstant);
        }

        [Test]
        public void InZone()
        {
            DateTimeZone london = DateTimeZoneProviders.Tzdb["Europe/London"];
            ZonedDateTime viaInstant = Instant.FromUtc(2008, 6, 10, 13, 16, 17).InZone(london);

            // London is UTC+1 in the Summer, so the above is 14:16:17 local.
            LocalDateTime local = new LocalDateTime(2008, 6, 10, 14, 16, 17);
            ZonedDateTime expected = london.AtStrictly(local);

            Assert.AreEqual(expected, viaInstant);
        }

        [Test]
        public void WithOffset()
        {
            // Jon talks about Noda Time at Leetspeak in Sweden on October 12th 2013, at 13:15 UTC+2
            Instant instant = Instant.FromUtc(2013, 10, 12, 11, 15);
            Offset offset = Offset.FromHours(2);
            OffsetDateTime actual = instant.WithOffset(offset);
            OffsetDateTime expected = new OffsetDateTime(new LocalDateTime(2013, 10, 12, 13, 15), offset);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WithOffset_NonIsoCalendar()
        {
            // October 12th 2013 ISO is 1434-12-07 Islamic
            CalendarSystem calendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base15, IslamicEpoch.Civil);
            Instant instant = Instant.FromUtc(2013, 10, 12, 11, 15);
            Offset offset = Offset.FromHours(2);
            OffsetDateTime actual = instant.WithOffset(offset, calendar);
            OffsetDateTime expected = new OffsetDateTime(new LocalDateTime(1434, 12, 7, 13, 15, calendar), offset);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FromTicksSinceUnixEpoch()
        {
            Instant actual = Instant.FromTicksSinceUnixEpoch(12345L);
            Instant expected = new Instant(12345L);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FromMillisecondsSinceUnixEpoch_Valid()
        {
            Instant actual = Instant.FromMillisecondsSinceUnixEpoch(12345L);
            Instant expected = new Instant(12345L * NodaConstants.TicksPerMillisecond);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FromMillisecondsSinceUnixEpoch_TooLarge()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Instant.FromMillisecondsSinceUnixEpoch(long.MaxValue / 100));
        }

        [Test]
        public void FromMillisecondsSinceUnixEpoch_TooSmall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Instant.FromMillisecondsSinceUnixEpoch(long.MinValue / 100));
        }

        [Test]
        public void FromSecondsSinceUnixEpoch_Valid()
        {
            Instant actual = Instant.FromSecondsSinceUnixEpoch(12345L);
            Instant expected = new Instant(12345L * NodaConstants.TicksPerSecond);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FromSecondsSinceUnixEpoch_TooLarge()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Instant.FromSecondsSinceUnixEpoch(long.MaxValue / 1000000));
        }

        [Test]
        public void FromSecondsSinceUnixEpoch_TooSmall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Instant.FromSecondsSinceUnixEpoch(long.MinValue / 1000000));
        }

        [Test]
        public void InZoneWithCalendar()
        {
            CalendarSystem copticCalendar = CalendarSystem.GetCopticCalendar(4);
            DateTimeZone london = DateTimeZoneProviders.Tzdb["Europe/London"];
            ZonedDateTime viaInstant = Instant.FromUtc(2004, 6, 9, 11, 10).InZone(london, copticCalendar);

            // Date taken from CopticCalendarSystemTest. Time will be 12:10 (London is UTC+1 in Summer)
            LocalDateTime local = new LocalDateTime(1720, 10, 2, 12, 10, 0, copticCalendar);
            ZonedDateTime expected = london.AtStrictly(local);
            Assert.AreEqual(expected, viaInstant);
        }

        [Test]
        public void Max()
        {
            Instant x = new Instant(100);
            Instant y = new Instant(200);
            Assert.AreEqual(y, Instant.Max(x, y));
            Assert.AreEqual(y, Instant.Max(y, x));
            Assert.AreEqual(x, Instant.Max(x, Instant.MinValue));
            Assert.AreEqual(x, Instant.Max(Instant.MinValue, x));
            Assert.AreEqual(Instant.MaxValue, Instant.Max(Instant.MaxValue, x));
            Assert.AreEqual(Instant.MaxValue, Instant.Max(x, Instant.MaxValue));
        }

        [Test]
        public void Min()
        {
            Instant x = new Instant(100);
            Instant y = new Instant(200);
            Assert.AreEqual(x, Instant.Min(x, y));
            Assert.AreEqual(x, Instant.Min(y, x));
            Assert.AreEqual(Instant.MinValue, Instant.Min(x, Instant.MinValue));
            Assert.AreEqual(Instant.MinValue, Instant.Min(Instant.MinValue, x));
            Assert.AreEqual(x, Instant.Min(Instant.MaxValue, x));
            Assert.AreEqual(x, Instant.Min(x, Instant.MaxValue));
        }

        [Test]
        public void ToDateTimeUtc()
        {
            Instant x = Instant.FromUtc(2011, 08, 18, 20, 53);
            DateTime expected = new DateTime(2011, 08, 18, 20, 53, 0, DateTimeKind.Utc);
            DateTime actual = x.ToDateTimeUtc();
            Assert.AreEqual(expected, actual);

            // Kind isn't checked by Equals...
            Assert.AreEqual(DateTimeKind.Utc, actual.Kind);
        }

        [Test]
        public void ToDateTimeOffset()
        {
            Instant x = Instant.FromUtc(2011, 08, 18, 20, 53);
            DateTimeOffset expected = new DateTimeOffset(2011, 08, 18, 20, 53, 0, TimeSpan.Zero);
            Assert.AreEqual(expected, x.ToDateTimeOffset());
        }

        [Test]
        public void FromDateTimeOffset()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(2011, 08, 18, 20, 53, 0, TimeSpan.FromHours(5));
            Instant expected = Instant.FromUtc(2011, 08, 18, 15, 53);
            Assert.AreEqual(expected, Instant.FromDateTimeOffset(dateTimeOffset));
        }

        [Test]
        public void FromDateTimeUtc_Invalid()
        {
            Assert.Throws<ArgumentException>(() => Instant.FromDateTimeUtc(new DateTime(2011, 08, 18, 20, 53, 0, DateTimeKind.Local)));
            Assert.Throws<ArgumentException>(() => Instant.FromDateTimeUtc(new DateTime(2011, 08, 18, 20, 53, 0, DateTimeKind.Unspecified)));
        }

        [Test]
        public void FromDateTimeUtc_Valid()
        {
            DateTime x = new DateTime(2011, 08, 18, 20, 53, 0, DateTimeKind.Utc);
            Instant expected = Instant.FromUtc(2011, 08, 18, 20, 53);
            Assert.AreEqual(expected, Instant.FromDateTimeUtc(x));
        }

        /// <summary>
        /// Using the default constructor is equivalent to January 1st 1970, midnight, UTC, ISO Calendar
        /// </summary>
        [Test]
        public void DefaultConstructor()
        {
            var actual = new Instant();
            Assert.AreEqual(NodaConstants.UnixEpoch, actual);
        }

        [Test]
        public void XmlSerialization()
        {
            var value = new LocalDateTime(2013, 4, 12, 17, 53, 23, 123, 4567).InUtc().ToInstant();
            TestHelper.AssertXmlRoundtrip(value, "<value>2013-04-12T17:53:23.1234567Z</value>");
        }

        [Test]
        [TestCase("<value>2013-15-12T17:53:23Z</value>", typeof(UnparsableValueException), Description = "Invalid month")]
        public void XmlSerialization_Invalid(string xml, Type expectedExceptionType)
        {
            TestHelper.AssertXmlInvalid<Instant>(xml, expectedExceptionType);
        }

        [Test]
        public void BinarySerialization()
        {
            TestHelper.AssertBinaryRoundtrip(new Instant(12345L));
            TestHelper.AssertBinaryRoundtrip(Instant.MinValue);
            TestHelper.AssertBinaryRoundtrip(Instant.MaxValue);
        }

        [Test]
        [TestCase("1990-01-01T00:00:00Z", false, Description = "Before interval")]
        [TestCase("2000-01-01T00:00:00Z", true, Description = "Start of interval")]
        [TestCase("2010-01-01T00:00:00Z", true, Description = "Within interval")]
        [TestCase("2020-01-01T00:00:00Z", false, Description = "End instant of interval")]
        [TestCase("2030-01-01T00:00:00Z", false, Description = "After interval")]
        public void Contains(string candidateText, bool expectedResult)
        {
            var start = Instant.FromUtc(2000, 1, 1, 0, 0);
            var end = Instant.FromUtc(2020, 1, 1, 0, 0);
            var interval = new Interval(start, end);
            var candidate = InstantPattern.ExtendedIsoPattern.Parse(candidateText).Value;
            Assert.AreEqual(expectedResult, interval.Contains(candidate));
        }

        [Test]
        public void Contains_EndOfTime()
        {
            var interval = new Interval(NodaConstants.UnixEpoch, Instant.MaxValue);
            Assert.IsTrue(interval.Contains(Instant.MaxValue));
        }

        [Test]
        public void Contains_EmptyInterval()
        {
            var instant = NodaConstants.UnixEpoch;
            var interval = new Interval(instant, instant);
            Assert.IsFalse(interval.Contains(instant));
        }

        [Test]
        public void Contains_EmptyInterval_EndOfTime()
        {
            var instant = Instant.MaxValue;
            var interval = new Interval(instant, instant);
            Assert.IsTrue(interval.Contains(instant));
        }
    }
}

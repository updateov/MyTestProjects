// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using NUnit.Framework;
using NodaTime.Test.TimeZones.IO;
using NodaTime.TimeZones;

namespace NodaTime.Test.TimeZones
{
    [TestFixture]
    public class ZoneYearOffsetTest
    {
        private const long TicksPerStandardYear = NodaConstants.TicksPerStandardDay * 365;
        private const long TicksPerLeapYear = NodaConstants.TicksPerStandardDay * 366;

        private Offset oneHour = Offset.FromHours(1);
        private Offset twoHours = Offset.FromHours(2);
        // private Offset minusOneHour = Offset.FromHours(-1);

        [Test]
        public void Construct_InvalidMonth_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 0, 1, 1, true, LocalTime.Midnight), "Month 0");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 34, 1, 1, true, LocalTime.Midnight), "Month 34");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, -3, 1, 1, true, LocalTime.Midnight), "Month -3");
        }

        [Test]
        public void Construct_InvalidDayOfMonth_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 0, 1, true, LocalTime.Midnight), "Day of Month 0");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 32, 1, true, LocalTime.Midnight), "Day of Month 32");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 475, 1, true, LocalTime.Midnight),
                          "Day of Month 475");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, -32, 1, true, LocalTime.Midnight),
                          "Day of Month -32");
        }

        [Test]
        public void Construct_InvalidDayOfWeek_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, -1, true, LocalTime.Midnight), "Day of Week -1");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, 8, true, LocalTime.Midnight), "Day of Week 8");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, 5756, true, LocalTime.Midnight),
                          "Day of Week 5856");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, -347, true, LocalTime.Midnight),
                          "Day of Week -347");
        }

        [Test]
        public void Construct_ValidMonths()
        {
            for (int month = 1; month <= 12; month++)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, month, 1, 1, true, LocalTime.Midnight), "Month " + month);
            }
        }

        [Test]
        public void Construct_ValidDays()
        {
            for (int day = 1; day <= 31; day++)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, 1, day, 1, true, LocalTime.Midnight), "Day " + day);
            }
            for (int day = -1; day >= -31; day--)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, 1, day, 1, true, LocalTime.Midnight), "Day " + day);
            }
        }

        [Test]
        public void Construct_ValidDaysOfWeek()
        {
            for (int dayOfWeek = 0; dayOfWeek <= 7; dayOfWeek++)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, 1, 1, dayOfWeek, true, LocalTime.Midnight), "Day of week " + dayOfWeek);
            }
        }

        [Test]
        public void MakeInstant_Defaults_Epoch()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = NodaConstants.UnixEpoch;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_Year_1971()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1971, Offset.Zero, Offset.Zero);
            var expected = new Instant(365L * NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_SavingOffsetIgnored_Epoch()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, twoHours, oneHour);
            var expected = NodaConstants.UnixEpoch;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_SavingIgnored()
        {
            var offset = new ZoneYearOffset(TransitionMode.Standard, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, twoHours, oneHour);
            var expected = new Instant((1L - 1) * NodaConstants.TicksPerStandardDay - twoHours.Ticks);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_SavingAndOffset()
        {
            var offset = new ZoneYearOffset(TransitionMode.Wall, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, twoHours, oneHour);
            var expected = new Instant((1L - 1) * NodaConstants.TicksPerStandardDay - (twoHours.Ticks + oneHour.Ticks));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_Milliseconds()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, new LocalTime(0, 0, 0, 1));
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = new Instant((1L - 1) * NodaConstants.TicksPerStandardDay + NodaConstants.TicksPerMillisecond);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_WednesdayForward()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, (int)DayOfWeek.Wednesday, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = new Instant((7L - 1) * NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_WednesdayBackward()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 15, (int)DayOfWeek.Wednesday, false, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = new Instant((14L - 1) * NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_JanOne()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = new Instant((1L - 1) * NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_JanMinusTwo()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, -2, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = new Instant((30L - 1) * NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_JanFive()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 5, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = new Instant((5L - 1) * NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_Feb()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 2, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            var expected = new Instant((32L - 1) * NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Next_JanOne()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            var expected = new Instant(baseTicks + (1 * TicksPerStandardYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Next_OneSecondBeforeJanOne()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero) - Duration.Epsilon;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            var expected = NodaConstants.UnixEpoch;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NextTwice_JanOne()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            var expected = new Instant(baseTicks + (2 * TicksPerStandardYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Next_Feb29_FourYears()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 2, 29, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1972, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            var expected = new Instant(baseTicks + (3 * TicksPerStandardYear) + TicksPerLeapYear);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NextTwice_Feb29_FourYears()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 2, 29, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1972, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            var expected = new Instant(baseTicks + (2 * ((3 * TicksPerStandardYear) + TicksPerLeapYear)));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Previous_JanOne()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Previous(actual, Offset.Zero, Offset.Zero);
            var expected = new Instant(baseTicks - (1 * TicksPerStandardYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Previous_OneSecondAfterJanOne()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero) + Duration.Epsilon;
            actual = offset.Previous(actual, Offset.Zero, Offset.Zero);
            var expected = NodaConstants.UnixEpoch;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void PreviousTwice_JanOne()
        {
            var offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Previous(actual, Offset.Zero, Offset.Zero);
            actual = offset.Previous(actual, Offset.Zero, Offset.Zero);
            var expected = new Instant(baseTicks - (TicksPerStandardYear + TicksPerLeapYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Next_WednesdayForward()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)IsoDayOfWeek.Wednesday, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(2006, Offset.Zero, Offset.Zero); // Nov 1 2006
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero); // Oct 31 2007
            var expected = new Instant(baseTicks + (1 * TicksPerStandardYear) - NodaConstants.TicksPerStandardDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NextTwice_WednesdayForward()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)IsoDayOfWeek.Wednesday, true, LocalTime.Midnight);
            var actual = offset.MakeInstant(2006, Offset.Zero, Offset.Zero); // Nov 1 2006
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero); // Oct 31 2007
            actual = offset.Next(actual, Offset.Zero, Offset.Zero); // Nov 5 2008
            var expected = new Instant(baseTicks + TicksPerStandardYear + TicksPerLeapYear + (4 * NodaConstants.TicksPerStandardDay));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_LastSundayInOctober()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 10, -1, (int)IsoDayOfWeek.Sunday, false, LocalTime.Midnight);
            var actual = offset.MakeInstant(1996, Offset.Zero, Offset.Zero);
            Assert.AreEqual(Instant.FromUtc(1996, 10, 27, 0, 0), actual);
        }

        [Test]
        public void Serialization()
        {
            var dio = DtzIoHelper.CreateNoStringPool();
            var expected = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)IsoDayOfWeek.Wednesday, true, LocalTime.Midnight);
            dio.TestZoneYearOffset(expected);

            dio.Reset();
            expected = new ZoneYearOffset(TransitionMode.Utc, 10, -31, (int)IsoDayOfWeek.Wednesday, true, LocalTime.Midnight);
            dio.TestZoneYearOffset(expected);
        }

        [Test]
        public void IEquatable_Tests()
        {
            var value = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)IsoDayOfWeek.Wednesday, true, LocalTime.Midnight);
            var equalValue = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)IsoDayOfWeek.Wednesday, true, LocalTime.Midnight);
            var unequalValue = new ZoneYearOffset(TransitionMode.Utc, 9, 31, (int)IsoDayOfWeek.Wednesday, true, LocalTime.Midnight);

            TestHelper.TestEqualsClass(value, equalValue, unequalValue);
        }

        [Test]
        public void Next_WithAddDay()
        {
            // Last Thursday in October, then add 24 hours. The last Thursday in October 2013 is the 31st, so
            // we should get the start of November 1st.
            var offset = new ZoneYearOffset(TransitionMode.Utc, 10, -1, (int) IsoDayOfWeek.Thursday, false, LocalTime.Midnight, true);
            var instant = Instant.FromUtc(2013, 10, 31, 12, 0);
            var expectedNext = Instant.FromUtc(2013, 11, 1, 0, 0);
            var actualNext = offset.Next(instant, Offset.Zero, Offset.Zero);
            Assert.AreEqual(expectedNext, actualNext);
        }

        [Test]
        public void Previous_WithAddDay()
        {
            // Last Thursday in October, then add 24 hours. The last Thursday in October 2013 is the 31st, so
            // the previous transition is the start of Friday October 26th 2012.
            var offset = new ZoneYearOffset(TransitionMode.Utc, 10, -1, (int)IsoDayOfWeek.Thursday, false, LocalTime.Midnight, true);
            var instant = Instant.FromUtc(2013, 10, 31, 12, 0);
            var expectedPrevious = Instant.FromUtc(2012, 10, 26, 0, 0);
            var actualPrevious = offset.Previous(instant, Offset.Zero, Offset.Zero);
            Assert.AreEqual(expectedPrevious, actualPrevious);
        }
    }
}

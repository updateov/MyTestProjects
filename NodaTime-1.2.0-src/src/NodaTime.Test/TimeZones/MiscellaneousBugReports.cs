// Copyright 2010 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;

namespace NodaTime.Test.TimeZones
{
    /// <summary>
    /// Fixture for miscellaneous bug reports and oddities which don't really fit anywhere else.
    /// Quite often the cause of a problem is nowhere near the test code; it's still useful
    /// to have the original test which showed up the problem, as a small contribution
    /// to regression testing.
    /// </summary>
    [TestFixture]
    public class MiscellaneousBugReports
    {
        [Test]
        public void Niue()
        {
            DateTimeZone niue = DateTimeZoneProviders.Tzdb["Pacific/Niue"];
            var offset = niue.GetUtcOffset(niue.AtStrictly(new LocalDateTime(2010, 1, 1, 0, 0, 0)).ToInstant());
            Assert.AreEqual(Offset.FromHours(-11), offset);
        }

        [Test]
        public void Kiritimati()
        {
            DateTimeZone kiritimati = DateTimeZoneProviders.Tzdb["Pacific/Kiritimati"];
            var offset = kiritimati.GetUtcOffset(kiritimati.AtStrictly(new LocalDateTime(2010, 1, 1, 0, 0, 0)).ToInstant());
            Assert.AreEqual(Offset.FromHours(14), offset);
        }

        [Test]
        public void Pyongyang()
        {
            DateTimeZone pyongyang = DateTimeZoneProviders.Tzdb["Asia/Pyongyang"];
            var offset = pyongyang.GetUtcOffset(pyongyang.AtStrictly(new LocalDateTime(2010, 1, 1, 0, 0, 0)).ToInstant());
            Assert.AreEqual(Offset.FromHours(9), offset);
        }

        [Test]
        public void Khartoum()
        {
            DateTimeZone khartoum = DateTimeZoneProviders.Tzdb["Africa/Khartoum"];
            Assert.IsNotNull(khartoum);
            Instant utc = Instant.FromUtc(2000, 1, 1, 0, 0, 0);
            ZonedDateTime inKhartoum = new ZonedDateTime(utc, khartoum);
            LocalDateTime expectedLocal = new LocalDateTime(2000, 1, 1, 2, 0);
            Assert.AreEqual(expectedLocal, inKhartoum.LocalDateTime);

            // Khartoum changed from +2 to +3 on January 15th 2000
            utc = Instant.FromUtc(2000, 1, 16, 0, 0, 0);
            inKhartoum = new ZonedDateTime(utc, khartoum);
            expectedLocal = new LocalDateTime(2000, 1, 16, 3, 0);
            Assert.AreEqual(expectedLocal, inKhartoum.LocalDateTime);
        }

        /// <summary>
        /// Tbilisi used daylight saving time for winter 1996/1997 too.
        /// </summary>
        [Test]
        public void Tbilisi()
        {
            var zone = DateTimeZoneProviders.Tzdb["Asia/Tbilisi"];
            Instant summer1996 = Instant.FromUtc(1996, 6, 1, 0, 0);
            var interval = zone.GetZoneInterval(summer1996);
            Assert.AreEqual(new LocalDateTime(1996, 3, 31, 1, 0), interval.IsoLocalStart);
            Assert.AreEqual(new LocalDateTime(1997, 10, 26, 0, 0), interval.IsoLocalEnd);
        }
    }
}
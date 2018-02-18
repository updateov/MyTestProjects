// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Properties;
using NodaTime.Testing.TimeZones;
using NodaTime.Text;
using NodaTime.TimeZones;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace NodaTime.Test.Text
{
    [TestFixture]
    public class ZonedDateTimePatternTest : PatternTestBase<ZonedDateTime>
    {
        // Three zones with a deliberately leading-substring-matching set of names.
        // Transition is at 1am local time, going forward an hour.
        private static readonly SingleTransitionDateTimeZone TestZone1 = new SingleTransitionDateTimeZone(
            Instant.FromUtc(2010, 1, 1, 0, 0), Offset.FromHours(1), Offset.FromHours(2), "ab");
        // Transition is at 2am local time, going back an hour.
        private static readonly SingleTransitionDateTimeZone TestZone2 = new SingleTransitionDateTimeZone(
            Instant.FromUtc(2010, 1, 1, 0, 0), Offset.FromHours(2), Offset.FromHours(1), "abc");
        private static readonly SingleTransitionDateTimeZone TestZone3 = new SingleTransitionDateTimeZone(
            Instant.FromUtc(2010, 1, 1, 0, 0), Offset.FromHours(1), Offset.FromHours(2), "abcd");
        private static readonly IDateTimeZoneProvider TestProvider =
            new FakeDateTimeZoneSource.Builder { TestZone1, TestZone2, TestZone3 }.Build().ToProvider();
        private static readonly DateTimeZone FixedPlus1 = FixedDateTimeZone.ForOffset(Offset.FromHours(1));
        private static readonly DateTimeZone FixedWithMinutes = FixedDateTimeZone.ForOffset(Offset.FromHoursAndMinutes(1, 30));
        private static readonly DateTimeZone FixedWithSeconds = FixedDateTimeZone.ForOffset(Offset.FromMilliseconds(5000));
        private static readonly DateTimeZone FixedWithMilliseconds1 = FixedDateTimeZone.ForOffset(Offset.FromMilliseconds(1500));
        private static readonly DateTimeZone FixedWithMilliseconds2 = FixedDateTimeZone.ForOffset(Offset.FromMilliseconds(1510));
        private static readonly DateTimeZone FixedWithMilliseconds3 = FixedDateTimeZone.ForOffset(Offset.FromMilliseconds(1512));
        private static readonly DateTimeZone FixedMinus1 = FixedDateTimeZone.ForOffset(Offset.FromHours(-1));

        private static readonly DateTimeZone France = DateTimeZoneProviders.Tzdb["Europe/Paris"];

        private static readonly ZonedDateTime SampleZonedDateTimeCoptic = LocalDateTimePatternTest.SampleLocalDateTimeCoptic.InUtc();

        // The standard example date/time used in all the MSDN samples, which means we can just cut and paste
        // the expected results of the standard patterns.
        private static readonly ZonedDateTime MsdnStandardExample = LocalDateTimePatternTest.MsdnStandardExample.InUtc();
        private static readonly ZonedDateTime MsdnStandardExampleNoMillis = LocalDateTimePatternTest.MsdnStandardExampleNoMillis.InUtc();

        internal static readonly Data[] InvalidPatternData = {
            new Data { Pattern = "dd MM yyyy HH:MM:SS", Message = Messages.Parse_RepeatedFieldInPattern, Parameters = { 'M' } },
            // Note incorrect use of "y" (year) instead of "Y" (year of era)
            new Data { Pattern = "dd MM yyyy HH:mm:ss gg", Message = Messages.Parse_EraWithoutYearOfEra },
            // Era specifier and calendar specifier in the same pattern.
            new Data { Pattern = "dd MM YYYY HH:mm:ss gg c", Message = Messages.Parse_CalendarAndEra },
            new Data { Pattern = "g", Message = Messages.Parse_UnknownStandardFormat, Parameters = { 'g', typeof(ZonedDateTime) } },
        };

        internal static Data[] ParseFailureData = {
            // Skipped value
            new Data { Pattern = "yyyy-MM-dd HH:mm z", Text = "2010-01-01 01:30 ab", Message = Messages.Parse_SkippedLocalTime},
            // Ambiguous value
            new Data { Pattern = "yyyy-MM-dd HH:mm z", Text = "2010-01-01 01:30 abc", Message = Messages.Parse_AmbiguousLocalTime },

            // Invalid offset within a skipped time
            new Data { Pattern = "yyyy-MM-dd HH:mm z o<g>", Text = "2010-01-01 01:30 ab +01", Message = Messages.Parse_InvalidOffset},
            // Invalid offset within an ambiguous time (doesn't match either option)
            new Data { Pattern = "yyyy-MM-dd HH:mm z o<g>", Text = "2010-01-01 01:30 abc +05", Message = Messages.Parse_InvalidOffset},
            // Invalid offset for an unambiguous time
            new Data { Pattern = "yyyy-MM-dd HH:mm z o<g>", Text = "2005-01-01 01:30 ab +02", Message = Messages.Parse_InvalidOffset},

            // Failures copied from LocalDateTimePatternTest
            new Data { Pattern = "dd MM yyyy HH:mm:ss", Text = "Complete mismatch", Message = Messages.Parse_MismatchedNumber, Parameters = { "dd" }},
            new Data { Pattern = "(c)", Text = "(xxx)", Message = Messages.Parse_NoMatchingCalendarSystem },
            // 24 as an hour is only valid when the time is midnight
            new Data { Pattern = "yyyy-MM-dd HH:mm:ss", Text = "2011-10-19 24:00:05", Message = Messages.Parse_InvalidHour24 },
            new Data { Pattern = "yyyy-MM-dd HH:mm:ss", Text = "2011-10-19 24:01:00", Message = Messages.Parse_InvalidHour24 },
            new Data { Pattern = "yyyy-MM-dd HH:mm", Text = "2011-10-19 24:01", Message = Messages.Parse_InvalidHour24 },
            new Data { Pattern = "yyyy-MM-dd HH:mm", Text = "2011-10-19 24:00", Template = new LocalDateTime(1970, 1, 1, 0, 0, 5).InZoneStrictly(TestZone1), Message = Messages.Parse_InvalidHour24},
            new Data { Pattern = "yyyy-MM-dd HH", Text = "2011-10-19 24", Template = new LocalDateTime(1970, 1, 1, 0, 5, 0).InZoneStrictly(TestZone1), Message = Messages.Parse_InvalidHour24},

            // Redundant specification of fixed zone but not enough digits - we'll parse UTC+01:00:00 and unexpectedly be left with 00
            new Data { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+01:00:00.00", Message = Messages.Parse_ExtraValueCharacters, Parameters = { ".00" }},

            // Can't parse a pattern with a time zone abbreviation.
            new Data { Pattern = "yyyy-MM-dd HH:mm x", Text = "ignored", Message = Messages.Parse_FormatOnlyPattern },

            // Can't parse using a pattern that has no provider
            new Data { ZoneProvider = null, Pattern = "yyyy-MM-dd z", Text = "ignored", Message = Messages.Parse_FormatOnlyPattern },
        };

        internal static Data[] ParseOnlyData = {
            // Template value time zone is from a different provider, but it's not part of the pattern.
            new Data(2013, 1, 13, 16, 2, France) { Pattern = "yyyy-MM-dd HH:mm", Text = "2013-01-13 16:02", Template = NodaConstants.UnixEpoch.InZone(France) },

            // Skipped value, resolver returns start of second interval
            new Data(TestZone1.Transition.InZone(TestZone1)) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2010-01-01 01:30 ab", Resolver = Resolvers.CreateMappingResolver(Resolvers.ThrowWhenAmbiguous, Resolvers.ReturnStartOfIntervalAfter) },

            // Skipped value, resolver returns end of first interval
            new Data(TestZone1.Transition.Minus(Duration.Epsilon).InZone(TestZone1)) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2010-01-01 01:30 ab", Resolver = Resolvers.CreateMappingResolver(Resolvers.ThrowWhenAmbiguous, Resolvers.ReturnEndOfIntervalBefore) },

            // Parse-only tests from LocalDateTimeTest.
            new Data(2011, 10, 19, 16, 05, 20) { Pattern = "dd MM yyyy", Text = "19 10 2011", Template = new LocalDateTime(2000, 1, 1, 16, 05, 20).InUtc() },
            new Data(2011, 10, 19, 16, 05, 20) { Pattern = "HH:mm:ss", Text = "16:05:20", Template = new LocalDateTime(2011, 10, 19, 0, 0, 0).InUtc() },

            // Parsing using the semi-colon "comma dot" specifier
            new Data(2011, 10, 19, 16, 05, 20, 352) { Pattern = "yyyy-MM-dd HH:mm:ss;fff", Text = "2011-10-19 16:05:20,352" },
            new Data(2011, 10, 19, 16, 05, 20, 352) { Pattern = "yyyy-MM-dd HH:mm:ss;FFF", Text = "2011-10-19 16:05:20,352" },

            // 24:00 meaning "start of next day"
            new Data(2011, 10, 20) { Pattern = "yyyy-MM-dd HH:mm:ss", Text = "2011-10-19 24:00:00" },
            new Data(2011, 10, 20, 0, 0, TestZone1) { Pattern = "yyyy-MM-dd HH:mm:ss", Text = "2011-10-19 24:00:00", Template = new LocalDateTime(1970, 1, 1, 0, 5, 0).InZoneStrictly(TestZone1) },
            new Data(2011, 10, 20) { Pattern = "yyyy-MM-dd HH:mm", Text = "2011-10-19 24:00" },
            new Data(2011, 10, 20) { Pattern = "yyyy-MM-dd HH", Text = "2011-10-19 24" },

            // Redundant specification of offset
            new Data(2013, 01, 13, 15, 44, FixedPlus1) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+01:00" },
            new Data(2013, 01, 13, 15, 44, FixedPlus1) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+01:00:00" },
            new Data(2013, 01, 13, 15, 44, FixedPlus1) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+01:00:00.000" },
        };

        internal static Data[] FormatOnlyData = {
            new Data(2011, 10, 19, 16, 05, 20) { Pattern = "ddd yyyy", Text = "Wed 2011" },

            // Time zone isn't in the provider
            new Data(2013, 1, 13, 16, 2, France) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 16:02 Europe/Paris" },

            // Ambiguous value - would be invalid if parsed with a strict parser.
            new Data(TestZone2.Transition.Plus(Duration.FromMinutes(30)).InZone(TestZone2)) { Pattern = "yyyy-MM-dd HH:mm", Text = "2010-01-01 01:30" },

            // Winter
            new Data(2013, 1, 13, 16, 2, France) { Pattern = "yyyy-MM-dd HH:mm x", Text = "2013-01-13 16:02 CET" },
            // Summer
            new Data(2013, 6, 13, 16, 2, France) { Pattern = "yyyy-MM-dd HH:mm x", Text = "2013-06-13 16:02 CEST" },

            new Data(2013, 6, 13, 16, 2, France) { ZoneProvider = null, Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-06-13 16:02 Europe/Paris" },

            // Standard patterns (can't parse because there's no DateTimeZoneProvider)
            new Data(MsdnStandardExampleNoMillis) { Pattern = "G", Text = "2009-06-15T13:45:30 UTC (+00)", Culture = Cultures.FrFr },
            new Data(MsdnStandardExample) { Pattern = "F", Text = "2009-06-15T13:45:30.09 UTC (+00)", Culture = Cultures.FrFr },
        };

        internal static Data[] FormatAndParseData = {

            // Zone ID at the end
            new Data(2013, 01, 13, 15, 44, TestZone1) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 ab" },
            new Data(2013, 01, 13, 15, 44, TestZone2) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 abc" },
            new Data(2013, 01, 13, 15, 44, TestZone3) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 abcd" },
            new Data(2013, 01, 13, 15, 44, FixedPlus1) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+01" },
            new Data(2013, 01, 13, 15, 44, FixedMinus1) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC-01" },
            new Data(2013, 01, 13, 15, 44, DateTimeZone.Utc) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC" },

            // Zone ID at the start
            new Data(2013, 01, 13, 15, 44, TestZone1) { Pattern = "z yyyy-MM-dd HH:mm", Text = "ab 2013-01-13 15:44" },
            new Data(2013, 01, 13, 15, 44, TestZone2) { Pattern = "z yyyy-MM-dd HH:mm", Text = "abc 2013-01-13 15:44" },
            new Data(2013, 01, 13, 15, 44, TestZone3) { Pattern = "z yyyy-MM-dd HH:mm", Text = "abcd 2013-01-13 15:44" },
            new Data(2013, 01, 13, 15, 44, FixedPlus1) { Pattern = "z yyyy-MM-dd HH:mm", Text = "UTC+01 2013-01-13 15:44" },
            new Data(2013, 01, 13, 15, 44, FixedMinus1) { Pattern = "z yyyy-MM-dd HH:mm", Text = "UTC-01 2013-01-13 15:44" },
            new Data(2013, 01, 13, 15, 44, DateTimeZone.Utc) { Pattern = "z yyyy-MM-dd HH:mm", Text = "UTC 2013-01-13 15:44" },

            // More precise fixed zones.
            new Data(2013, 01, 13, 15, 44, FixedWithMinutes) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+01:30" },
            new Data(2013, 01, 13, 15, 44, FixedWithSeconds) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+00:00:05" },
            new Data(2013, 01, 13, 15, 44, FixedWithMilliseconds1) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+00:00:01.500" },
            new Data(2013, 01, 13, 15, 44, FixedWithMilliseconds2) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+00:00:01.510" },
            new Data(2013, 01, 13, 15, 44, FixedWithMilliseconds3) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 15:44 UTC+00:00:01.512" },

            // Valid offset for an unambiguous time
            new Data(new LocalDateTime(2005, 1, 1, 1, 30).InZoneStrictly(TestZone1)) { Pattern = "yyyy-MM-dd HH:mm z o<g>", Text = "2005-01-01 01:30 ab +01"},
            // Valid offset (in the middle of the pattern) for an unambiguous time
            new Data(new LocalDateTime(2005, 1, 1, 1, 30).InZoneStrictly(TestZone1)) { Pattern = "yyyy-MM-dd o<g> HH:mm z", Text = "2005-01-01 +01 01:30 ab"},
            
            // Ambiguous value, resolver returns later value.
            new Data(TestZone2.Transition.Plus(Duration.FromMinutes(30)).InZone(TestZone2)) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2010-01-01 01:30 abc", Resolver = Resolvers.LenientResolver },

            // Ambiguous value, resolver returns earlier value.
            new Data(TestZone2.Transition.Plus(Duration.FromMinutes(-30)).InZone(TestZone2)) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2010-01-01 01:30 abc", Resolver = Resolvers.CreateMappingResolver(Resolvers.ReturnEarlier, Resolvers.ThrowWhenSkipped) },

            // Ambiguous local value, but with offset for later value (smaller offset).
            new Data(TestZone2.Transition.Plus(Duration.FromMinutes(30)).InZone(TestZone2)) { Pattern = "yyyy-MM-dd HH:mm z o<g>", Text = "2010-01-01 01:30 abc +01"},

            // Ambiguous local value, but with offset for earlier value (greater offset).
            new Data(TestZone2.Transition.Plus(Duration.FromMinutes(-30)).InZone(TestZone2)) { Pattern = "yyyy-MM-dd HH:mm z o<g>", Text = "2010-01-01 01:30 abc +02"},

            // Specify the provider
            new Data(2013, 1, 13, 16, 2, France) { Pattern = "yyyy-MM-dd HH:mm z", Text = "2013-01-13 16:02 Europe/Paris", ZoneProvider = DateTimeZoneProviders.Tzdb},

            // Tests without zones, copied from LocalDateTimePatternTest
            // Calendar patterns are invariant
            new Data(MsdnStandardExample) { Pattern = "(c) yyyy-MM-ddTHH:mm:ss.FFFFFFF", Text = "(ISO) 2009-06-15T13:45:30.09", Culture = Cultures.FrFr },
            new Data(MsdnStandardExample) { Pattern = "yyyy-MM-dd(c)THH:mm:ss.FFFFFFF", Text = "2009-06-15(ISO)T13:45:30.09", Culture = Cultures.EnUs },
            new Data(SampleZonedDateTimeCoptic) { Pattern = "(c) yyyy-MM-ddTHH:mm:ss.FFFFFFF", Text = "(Coptic 1) 1976-06-19T21:13:34.1234567", Culture = Cultures.FrFr },
            new Data(SampleZonedDateTimeCoptic) { Pattern = "yyyy-MM-dd'C'c'T'HH:mm:ss.FFFFFFF", Text = "1976-06-19CCoptic 1T21:13:34.1234567", Culture = Cultures.EnUs },
            
            // Use of the semi-colon "comma dot" specifier
            new Data(2011, 10, 19, 16, 05, 20, 352) { Pattern = "yyyy-MM-dd HH:mm:ss;fff", Text = "2011-10-19 16:05:20.352" },
            new Data(2011, 10, 19, 16, 05, 20, 352) { Pattern = "yyyy-MM-dd HH:mm:ss;FFF", Text = "2011-10-19 16:05:20.352" },
            new Data(2011, 10, 19, 16, 05, 20, 352) { Pattern = "yyyy-MM-dd HH:mm:ss;FFF 'end'", Text = "2011-10-19 16:05:20.352 end" },
            new Data(2011, 10, 19, 16, 05, 20) { Pattern = "yyyy-MM-dd HH:mm:ss;FFF 'end'", Text = "2011-10-19 16:05:20 end" },
        };

        internal static IEnumerable<Data> ParseData = ParseOnlyData.Concat(FormatAndParseData);
        internal static IEnumerable<Data> FormatData = FormatOnlyData.Concat(FormatAndParseData);

        public sealed class Data : PatternTestData<ZonedDateTime>
        {
            // Default to the start of the year 2000 UTC
            protected override ZonedDateTime DefaultTemplate
            {
                get { return ZonedDateTimePattern.DefaultTemplateValue; }
            }

            internal ZoneLocalMappingResolver Resolver { get; set; }
            internal IDateTimeZoneProvider ZoneProvider { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Data" /> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public Data(ZonedDateTime value)
                : base(value)
            {
                Resolver = Resolvers.StrictResolver;
                ZoneProvider = TestProvider;
            }

            public Data(int year, int month, int day)
                : this(new LocalDateTime(year, month, day, 0, 0).InUtc())
            {
            }

            // Coincidentally, we don't specify time zones in tests other than the
            // ones which just go down to the date and hour/minute.
            public Data(int year, int month, int day, int hour, int minute, DateTimeZone zone)
                : this(new LocalDateTime(year, month, day, hour, minute).InZoneStrictly(zone))
            {
            }

            public Data(int year, int month, int day, int hour, int minute, int second)
                : this(new LocalDateTime(year, month, day, hour, minute, second).InUtc())
            {
            }

            public Data(int year, int month, int day, int hour, int minute, int second, int millis)
                : this(new LocalDateTime(year, month, day, hour, minute, second, millis).InUtc())
            {
            }


            public Data()
                : this(ZonedDateTimePattern.DefaultTemplateValue)
            {
            }

            internal override IPattern<ZonedDateTime> CreatePattern()
            {
                return ZonedDateTimePattern.Create(Pattern, Culture, Resolver, ZoneProvider, Template);
            }
        }
    }
}

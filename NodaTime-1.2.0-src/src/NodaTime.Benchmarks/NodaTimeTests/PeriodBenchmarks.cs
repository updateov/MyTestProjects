﻿// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using NodaTime.Benchmarks.Framework;

namespace NodaTime.Benchmarks.NodaTimeTests
{
    internal class PeriodBenchmarks
    {
        private static readonly LocalDate SampleStartDate = new LocalDate(2012, 3, 26);
        private static readonly LocalDate SampleEndDate = new LocalDate(2017, 2, 13);
        private static readonly LocalTime SampleStartTime = new LocalTime(13, 25, 10);
        private static readonly LocalTime SampleEndTime = new LocalTime(18, 10, 25);
        private static readonly LocalDateTime SampleStartDateTime = SampleStartDate + SampleStartTime;
        private static readonly LocalDateTime SampleEndDateTime = SampleEndDate + SampleEndTime;

        [Benchmark]
        public void Between_LocalDate()
        {
            Period.Between(SampleStartDate, SampleEndDate);
        }

        [Benchmark]
        public void Between_LocalDate_Years()
        {
            Period.Between(SampleStartDate, SampleEndDate, PeriodUnits.Years);
        }

        [Benchmark]
        public void Between_LocalDate_Months()
        {
            Period.Between(SampleStartDate, SampleEndDate, PeriodUnits.Months);
        }

        [Benchmark]
        public void Between_LocalDate_Days()
        {
            Period.Between(SampleStartDate, SampleEndDate, PeriodUnits.Days);
        }

        [Benchmark]
        public void Between_LocalTime()
        {
            Period.Between(SampleStartTime, SampleEndTime);
        }

        [Benchmark]
        public void Between_LocalDateTime()
        {
            Period.Between(SampleStartDateTime, SampleEndDateTime);
        }

        [Benchmark]
        public void Between_LocalDateTime_Ticks()
        {
            Period.Between(SampleStartDateTime, SampleEndDateTime, PeriodUnits.Ticks);
        }

        [Benchmark]
        public void PlusYears()
        {
            SampleStartDate.PlusYears(3);
        }

        [Benchmark]
        public void PlusMonths()
        {
            SampleStartDateTime.PlusMonths(3);
        }

        [Benchmark]
        public void PlusDays()
        {
            SampleStartDateTime.PlusDays(3);
        }

        [Benchmark]
        public void PlusWeeks()
        {
            SampleStartDateTime.PlusWeeks(3);
        }

        [Benchmark]
        public void PlusHours()
        {
            SampleStartDateTime.PlusHours(3);
        }

        [Benchmark]
        public void PlusMinutes()
        {
            SampleStartDateTime.PlusMinutes(3);
        }

        [Benchmark]
        public void PlusSeconds()
        {
            SampleStartDateTime.PlusSeconds(3);
        }

        [Benchmark]
        public void PlusMilliseconds()
        {
            SampleStartDateTime.PlusMilliseconds(3);
        }

        [Benchmark]
        public void PlusTicks()
        {
            SampleStartDateTime.PlusTicks(3);
        }
    }
}

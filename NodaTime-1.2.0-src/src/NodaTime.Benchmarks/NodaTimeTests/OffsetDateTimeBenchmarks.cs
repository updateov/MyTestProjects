// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using NodaTime.Benchmarks.Framework;

namespace NodaTime.Benchmarks.NodaTimeTests
{
    internal class OffsetDateTimeBenchmarks
    {
        private static readonly Offset OneHourOffset = Offset.FromHours(1);
        private static readonly LocalDateTime SampleLocal = new LocalDateTime(2009, 12, 26, 10, 8, 30);
        private static readonly OffsetDateTime SampleEarlier = new OffsetDateTime(SampleLocal, -OneHourOffset);
        private static readonly OffsetDateTime Sample = new OffsetDateTime(SampleLocal, Offset.Zero);
        private static readonly OffsetDateTime SampleLater = new OffsetDateTime(SampleLocal, OneHourOffset);

        private static readonly IComparer<OffsetDateTime> LocalComparer = OffsetDateTime.Comparer.Local;
        private static readonly IComparer<OffsetDateTime> InstantComparer = OffsetDateTime.Comparer.Instant;
        
        [Benchmark]
        public void Construction()
        {
            new OffsetDateTime(SampleLocal, OneHourOffset).Consume();
        }

        [Benchmark]
        public void TickOfDay()
        {
            Sample.TickOfDay.Consume();
        }

        [Benchmark]
        public void TickOfSecond()
        {
            Sample.TickOfSecond.Consume();
        }

        [Benchmark]
        public void WeekOfWeekYear()
        {
            Sample.WeekOfWeekYear.Consume();
        }

        [Benchmark]
        public void WeekYear()
        {
            Sample.WeekYear.Consume();
        }

        [Benchmark]
        public void ClockHourOfHalfDay()
        {
            Sample.ClockHourOfHalfDay.Consume();
        }

        [Benchmark]
        public void Era()
        {
            Sample.Era.Consume();
        }

        [Benchmark]
        public void CenturyOfEra()
        {
            Sample.YearOfCentury.Consume();
        }

        [Benchmark]
        public void YearOfCentury()
        {
            Sample.YearOfCentury.Consume();
        }

        [Benchmark]
        public void YearOfEra()
        {
            Sample.YearOfEra.Consume();
        }

        [Benchmark]
        public void LocalComparer_Compare()
        {
            LocalComparer.Compare(Sample, SampleEarlier);
            LocalComparer.Compare(Sample, Sample);
            LocalComparer.Compare(Sample, SampleLater);
        }

        [Benchmark]
        public void InstantComparer_Compare()
        {
            InstantComparer.Compare(Sample, SampleEarlier);
            InstantComparer.Compare(Sample, Sample);
            InstantComparer.Compare(Sample, SampleLater);
        }
    }
}
// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Benchmarks.Framework;

namespace NodaTime.Benchmarks.NodaTimeTests
{
    internal class UtcZonedDateTimeBenchmarks
    {
        private static readonly LocalDateTime SampleLocal = new LocalDateTime(2009, 12, 26, 10, 8, 30);
        private static readonly ZonedDateTime Sample = DateTimeZone.Utc.AtStrictly(SampleLocal);

        [Benchmark]
        public void Construction()
        {
            DateTimeZone.Utc.AtStrictly(SampleLocal);
        }

        [Benchmark]
        public void Year()
        {
            Sample.Year.Consume();
        }

        [Benchmark]
        public void Month()
        {
            Sample.Month.Consume();
        }

        [Benchmark]
        public void DayOfMonth()
        {
            Sample.Day.Consume();
        }

        [Benchmark]
        public void IsoDayOfWeek()
        {
            Sample.IsoDayOfWeek.Consume();
        }

        [Benchmark]
        public void DayOfYear()
        {
            Sample.DayOfYear.Consume();
        }

        [Benchmark]
        public void Hour()
        {
            Sample.Hour.Consume();
        }

        [Benchmark]
        public void Minute()
        {
            Sample.Minute.Consume();
        }

        [Benchmark]
        public void Second()
        {
            Sample.Second.Consume();
        }

        [Benchmark]
        public void Millisecond()
        {
            Sample.Millisecond.Consume();
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
    }
}
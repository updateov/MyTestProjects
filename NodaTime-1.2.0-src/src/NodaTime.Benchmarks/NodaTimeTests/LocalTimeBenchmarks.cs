// Copyright 2011 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Benchmarks.Framework;

namespace NodaTime.Benchmarks.NodaTimeTests
{
    internal class LocalTimeBenchmarks
    {
        private readonly LocalTime sample = new LocalTime(10, 8, 30, 300, 1234);
        private static readonly LocalDateTime LocalDateTime = new LocalDateTime(2011, 9, 14, 15, 10, 25);

        [Benchmark]
        public void ConstructionToMinute()
        {
            new LocalTime(15, 10).Consume();
        }

        [Benchmark]
        public void ConstructionToSecond()
        {
            new LocalTime(15, 10, 25).Consume();
        }

        [Benchmark]
        public void ConstructionToMillisecond()
        {
            new LocalTime(15, 10, 25, 500).Consume();
        }

        [Benchmark]
        public void ConstructionToTick()
        {
            new LocalTime(15, 10, 25, 500, 1234).Consume();
        }

        [Benchmark]
        public void ConversionFromLocalDateTime()
        {
            LocalDateTime.TimeOfDay.Consume();
        }

        [Benchmark]
        public void Hour()
        {
            sample.Hour.Consume();
        }

        [Benchmark]
        public void Minute()
        {
            sample.Minute.Consume();
        }

        [Benchmark]
        public void Second()
        {
            sample.Second.Consume();
        }

        [Benchmark]
        public void Millisecond()
        {
            sample.Millisecond.Consume();
        }

        [Benchmark]
        public void ClockHourOfHalfDay()
        {
            sample.ClockHourOfHalfDay.Consume();
        }

        [Benchmark]
        public void TickOfSecond()
        {
            sample.TickOfSecond.Consume();
        }

        [Benchmark]
        public void TickOfDay()
        {
            sample.TickOfDay.Consume();
        }
    }
}
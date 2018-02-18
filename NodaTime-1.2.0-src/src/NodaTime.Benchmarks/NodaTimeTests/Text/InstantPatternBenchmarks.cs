﻿// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Globalization;
using NodaTime.Benchmarks.Framework;
using NodaTime.Text;

namespace NodaTime.Benchmarks.NodaTimeTests.Text
{
    class InstantPatternBenchmarks
    {
        private static readonly Instant Sample = Instant.FromUtc(2011, 8, 24, 12, 29, 30);
        private static readonly InstantPattern GeneralPattern = InstantPattern.CreateWithInvariantCulture("g");
        private static readonly InstantPattern NumberPattern = InstantPattern.CreateWithInvariantCulture("n");
        private static readonly string SampleStringGeneral = GeneralPattern.Format(Sample);
        private static readonly string SampleStringNumber = NumberPattern.Format(Sample);
        private static readonly string SampleStringExtendedIso = InstantPattern.ExtendedIsoPattern.Format(Sample);
        private static readonly CultureInfo MutableInvariantCulture = (CultureInfo) CultureInfo.InvariantCulture.Clone();

        [Benchmark]
        public void NumberPatternFormat()
        {
            NumberPattern.Format(Sample);
        }

        [Benchmark]
        public void GeneralPatternFormat()
        {
            GeneralPattern.Format(Sample);
        }

        [Benchmark]
        public void ExtendedIsoPatternFormat()
        {
            InstantPattern.ExtendedIsoPattern.Format(Sample);
        }
 
        [Benchmark]
        public void NumberPatternParse()
        {
            NumberPattern.Parse(SampleStringNumber);
        }

        [Benchmark]
        public void GeneralPatternParse()
        {
            GeneralPattern.Parse(SampleStringGeneral);
        }

        [Benchmark]
        public void ExtendedIsoPatternParse()
        {
            InstantPattern.ExtendedIsoPattern.Parse(SampleStringExtendedIso);
        }

        [Benchmark]
        public void ParsePatternExtendedIso()
        {
            // Use a mutable culture info to prevent caching
            InstantPattern.Create(InstantPattern.ExtendedIsoPattern.PatternText, MutableInvariantCulture);
        }
    }
}

// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using NodaTime.TimeZones;

namespace NodaTime.Testing.TimeZones
{
    /// <summary>
    /// Time zone with multiple transitions, created via a builder.
    /// </summary>
    public sealed class MultiTransitionDateTimeZone : DateTimeZone
    {
        private readonly ReadOnlyCollection<ZoneInterval> intervals;
        private readonly ReadOnlyCollection<Instant> transitions;

        /// <summary>
        /// Zone intervals within this time zone, in chronological order, spanning the whole time line.
        /// </summary>
        public ReadOnlyCollection<ZoneInterval> Intervals { get { return intervals; } }

        /// <summary>
        /// Transition points between intervals.
        /// </summary>
        public ReadOnlyCollection<Instant> Transitions { get { return transitions; } }

        private MultiTransitionDateTimeZone(string id, IList<ZoneInterval> intervals)
            : base(id, intervals.Count == 1, intervals.Min(x => x.WallOffset), intervals.Max(x => x.WallOffset))
        {
            this.intervals = new ReadOnlyCollection<ZoneInterval>(intervals.ToList());
            transitions = new ReadOnlyCollection<Instant>(intervals.Skip(1).Select(x => x.Start).ToList());
        }

        /// <inheritdoc />
        public override ZoneInterval GetZoneInterval(Instant instant)
        {
            // TODO: We've got this binary search in at least three places now.
            // (PrecalculatedDateTimeZone, BclDateTimeZone, and here.) Maybe we should
            // have a utility method somewhere...
            int lower = 0; // Inclusive
            int upper = intervals.Count; // Exclusive

            while (lower < upper)
            {
                int current = (lower + upper) / 2;
                var candidate = intervals[current];
                if (candidate.Start > instant)
                {
                    upper = current;
                }
                else if (candidate.End <= instant)
                {
                    lower = current + 1;
                }
                else
                {
                    return candidate;
                }
            }
            // Note: this would indicate a bug. The time zone is meant to cover the whole of time.
            throw new InvalidOperationException(string.Format("Instant {0} did not exist in time zone {1}", instant, Id));
        }

        /// <inheritdoc />
        protected override bool EqualsImpl(DateTimeZone zone)
        {
            // Just use reference equality...
            return ReferenceEquals(this, zone);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }

        /// <summary>
        /// Builder to create instances of <see cref="MultiTransitionDateTimeZone"/>. Each builder
        /// can only be built once.
        /// </summary>
        public sealed class Builder : IEnumerable
        {
            private readonly List<ZoneInterval> intervals = new List<ZoneInterval>();
            private Offset currentStandardOffset;
            private Offset currentSavings;
            private string currentName;
            private bool built = false;

            /// <summary>
            /// ID of the time zone which will be built.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Constructs a builder using an ID of "MultiZone", an initial offset of zero (standard and savings),
            /// and an initial name of "First".
            /// </summary>
            public Builder() : this(0, 0)
            {                
            }

            /// <summary>
            /// Constructs a builder using the given first name, standard offset, and a daylight saving
            /// offset of 0. The ID is initially "MultiZone".
            /// </summary>
            /// <param name="firstName">Name of the first zone interval.</param>
            /// <param name="firstOffsetHours">Standard offset in hours in the first zone interval.</param>
            public Builder(int firstOffsetHours, string firstName)
                : this(firstOffsetHours, 0, firstName)
            {
            }

            /// <summary>
            /// Constructs a builder using the given standard offset and saving offset. The ID is initially "MultiZone".
            /// </summary>
            /// <param name="firstStandardOffsetHours">Standard offset in hours in the first zone interval.</param>
            /// <param name="firstSavingOffsetHours">Standard offset in hours in the first zone interval.</param>
            public Builder(int firstStandardOffsetHours, int firstSavingOffsetHours)
                : this(firstStandardOffsetHours, firstSavingOffsetHours, "First")
            {
            }

            /// <summary>
            /// Constructs a builder using the given first name, standard offset, and daylight saving offset.
            /// The ID is initially "MultiZone".
            /// </summary>
            /// <param name="firstStandardOffsetHours">Standard offset in hours in the first zone interval.</param>
            /// <param name="firstSavingOffsetHours">Daylight saving offset in hours in the first zone interval.</param>
            /// <param name="firstName">Name of the first zone interval.</param>
            public Builder(int firstStandardOffsetHours, int firstSavingOffsetHours, string firstName)
            {
                Id = "MultiZone";
                currentName = firstName;
                currentStandardOffset = Offset.FromHours(firstStandardOffsetHours);
                currentSavings = Offset.FromHours(firstSavingOffsetHours);
            }

            /// <summary>
            /// Adds a transition at the given instant, to the specified new standard offset,
            /// with no daylight saving. The name is generated from the transition.
            /// </summary>
            /// <param name="transition">Instant at which the zone changes.</param>
            /// <param name="newStandardOffsetHours">The new standard offset, in hours.</param>
            public void Add(Instant transition, int newStandardOffsetHours)
            {
                Add(transition, newStandardOffsetHours, 0);
            }

            /// <summary>
            /// Adds a transition at the given instant, to the specified new standard offset,
            /// with the new specified daylight saving. The name is generated from the transition.
            /// </summary>
            /// <param name="transition">Instant at which the zone changes.</param>
            /// <param name="newStandardOffsetHours">The new standard offset, in hours.</param>
            /// <param name="newSavingOffsetHours">The new daylight saving offset, in hours.</param>
            public void Add(Instant transition, int newStandardOffsetHours, int newSavingOffsetHours)
            {
                Add(transition, newStandardOffsetHours, newSavingOffsetHours, "Interval from " + transition);
            }

            /// <summary>
            /// Adds a transition at the given instant, to the specified new standard offset,
            /// with the new specified daylight saving. The name is generated from the transition.
            /// </summary>
            /// <param name="transition">Instant at which the zone changes.</param>
            /// <param name="newStandardOffsetHours">The new standard offset, in hours.</param>
            /// <param name="newSavingOffsetHours">The new daylight saving offset, in hours.</param>
            /// <param name="newName">The new zone interval name.</param>
            public void Add(Instant transition, int newStandardOffsetHours, int newSavingOffsetHours, string newName)
            {
                EnsureNotBuilt();
                Instant previousStart = intervals.Count == 0 ? Instant.MinValue : intervals.Last().End;
                // The ZoneInterval constructor will perform validation.
                intervals.Add(new ZoneInterval(currentName, previousStart, transition, currentStandardOffset + currentSavings, currentSavings));
                currentName = newName;
                currentStandardOffset = Offset.FromHours(newStandardOffsetHours);
                currentSavings = Offset.FromHours(newSavingOffsetHours);
            }

            /// <summary>
            /// Builds a <see cref="MultiTransitionDateTimeZone"/> from this builder, invalidating it in the process.
            /// </summary>
            /// <returns>The newly-built zone.</returns>
            public MultiTransitionDateTimeZone Build()
            {
                EnsureNotBuilt();
                built = true;
                Instant previousStart = intervals.Count == 0 ? Instant.MinValue : intervals.Last().End;
                intervals.Add(new ZoneInterval(currentName, previousStart, Instant.MaxValue, currentStandardOffset + currentSavings, currentSavings));
                return new MultiTransitionDateTimeZone(Id, intervals);
            }

            private void EnsureNotBuilt()
            {
                if (built)
                {
                    throw new InvalidOperationException("Cannot use a builder after building");
                }
            }

            /// <summary>
            /// We don't *really* want to implement this, but we want the collection initializer...
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}

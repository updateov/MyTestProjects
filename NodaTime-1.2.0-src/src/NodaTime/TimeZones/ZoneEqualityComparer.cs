// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NodaTime.TimeZones
{
    /// <summary>
    /// Equality comparer for time zones, comparing specific aspects of the zone intervals within
    /// a time zone for a specific interval of the time line.
    /// </summary>
    /// <remarks>
    /// The default behaviour of this comparator is to consider two time zones to be equal if they share the same wall
    /// offsets at all points within a given time interval, regardless of other aspects of each
    /// <see cref="ZoneInterval"/> within the two time zones. This behaviour can be changed using the
    /// <see cref="WithOptions"/> method.
    /// </remarks>
    public sealed class ZoneEqualityComparer : IEqualityComparer<DateTimeZone>
    {
        /// <summary>
        /// Options to use when comparing time zones for equality. Each option makes the comparison more restrictive.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, the comparer only compares the wall offset (total of standard offset and any daylight saving offset)
        /// at every instant within the interval over which the comparer operates. In practice, this is done by comparing each
        /// <see cref="ZoneInterval"/> which includes an instant within the interval (using <see cref="DateTimeZone.GetZoneIntervals(Interval)"/>).
        /// For most purposes, this is all that's required: from the simple perspective of a time zone being just a function from instants to local time,
        /// the default option of <see cref="OnlyMatchWallOffset"/> effectively checks that the function gives the same result across the two time
        /// zones being compared, for any given instant within the interval.
        /// </para>
        /// <para>
        /// It's possible for a time zone to have a transition from one <c>ZoneInterval</c> to another which doesn't adjust the offset: it
        /// might just change the name, or the balance between standard offset to daylight saving offset. (As an example, at midnight local
        /// time on October 27th 1968, the Europe/London time zone went from a standard offset of 0 and a daylight saving offset of 1 hour
        /// to a standard offset of 1 and a daylight saving offset of 0... which left the clocks unchanged.) This transition is irrelevant
        /// to the default options, so the two zone intervals involved are effectively coalesced.
        /// </para>
        /// <para>
        /// The options available change what sort of comparison is performed - which can also change which zone intervals can be coalesced. For
        /// example, by specifying just the <see cref="MatchAllTransitions"/> option, you would indicate that even though you don't care about the name within a zone
        /// interval or how the wall offset is calculated, you do care about the fact that there was a transition at all, and when it occurred.
        /// With that option enabled, zone intervals are never coalesced and the transition points within the operating interval are checked.
        /// </para>
        /// <para>Similarly, the <see cref="MatchStartAndEndTransitions"/> option is the only one where instants outside the operating interval are
        /// relevant. For example, consider a comparer which operates over the interval [2000-01-01T00:00:00Z, 2011-01-01T00:00:00Z). Normally,
        /// anything that happens before the year 2000 (UTC) would be irrelevant - but with this option enabled, the transitions of the first and last zone
        /// intervals are part of the comparison... so if one time zone has a zone interval 1999-09-01T00:00:00Z to 2000-03-01T00:00:00Z and the other has
        /// a zone interval 1999-10-15T00:00:00Z to 2000-03-01T00:00:Z, the two zones would be considered unequal, despite the fact that the only instants observing
        /// the difference occur outside the operating interval.
        /// </para>
        /// </remarks>
        [Flags]
        public enum Options
        {
            /// <summary>
            /// The default comparison, which only cares about the wall offset at any particular
            /// instant, within the interval of the comparer. In other words, if <see cref="DateTimeZone.GetUtcOffset"/>
            /// returns the same value for all instants in the interval, the comparer will consider the zones to be equal.
            /// </summary>
            OnlyMatchWallOffset = 0,

            /// <summary>
            /// Instead of only comparing wall offsets, the standard/savings split is also considered. So when this
            /// option is used, two zones which both have a wall offset of +2 at one instant would be considered
            /// unequal if one of those offsets was +1 standard, +1 savings and the other was +2 standard with no daylight
            /// saving.
            /// </summary>
            MatchOffsetComponents = 1 << 0,

            /// <summary>
            /// Compare the names of zone intervals as well as offsets.
            /// </summary>
            MatchNames = 1 << 1,

            /// <summary>
            /// This option prevents adjacent zone intervals from being coalesced, even if they are otherwise considered
            /// equivalent according to other options.
            /// </summary>
            MatchAllTransitions = 1 << 2,

            /// <summary>
            /// Includes the transitions into the first zone interval and out of the
            /// last zone interval as part of the comparison, even if they do not affect
            /// the offset or name for any instant within the operating interval.
            /// </summary>
            MatchStartAndEndTransitions = 1 << 3,

            /// <summary>
            /// The combination of all available match options.
            /// </summary>
            StrictestMatch = MatchNames | MatchOffsetComponents | MatchAllTransitions | MatchStartAndEndTransitions
        }

        /// <summary>
        /// Checks whether the given set of options includes the candidate one. This would be an extension method, but
        /// that causes problems on Mono at the moment.
        /// </summary>
        private static bool CheckOption(Options options, Options candidate)
        {
            return (options & candidate) != 0;
        }

        private readonly Interval interval;
        private readonly Options options;

        /// <summary>
        /// Returns the interval over which this comparer operates; visible for testing.
        /// </summary>
        internal Interval IntervalForTest { get { return interval; } }
        /// <summary>
        /// Returns the options used by this comparer; visible for testing.
        /// </summary>
        internal Options OptionsForTest { get { return options; } }

        private readonly ZoneIntervalEqualityComparer zoneIntervalComparer;
        
        /// <summary>
        /// Creates a new comparer for the given interval, with the given comparison options.
        /// </summary>
        /// <param name="interval">The interval within the time line to use for comparisons.</param>
        /// <param name="options">The options to use when comparing time zones.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified options are invalid.</exception>
        private ZoneEqualityComparer(Interval interval, Options options)
        {
            this.interval = interval;
            this.options = options;
            if ((options & ~Options.StrictestMatch) != 0)
            {
                throw new ArgumentOutOfRangeException("The value " + options + " is not defined within ZoneEqualityComparer.Options");
            }
            zoneIntervalComparer = new ZoneIntervalEqualityComparer(options, interval);
        }

        /// <summary>
        /// Returns a <see cref="ZoneEqualityComparer"/> for the given interval with the default options.
        /// </summary>
        /// <remarks>
        /// The default behaviour of this comparator is to consider two time zones to be equal if they share the same wall
        /// offsets at all points within a given interval.
        /// To specify non-default options, call the <see cref="WithOptions"/> method on the result
        /// of this method.</remarks>
        /// <param name="interval">The interval over which to compare time zones.</param>
        /// <returns>A ZoneEqualityComparer for the given interval with the default options.</returns>
        public static ZoneEqualityComparer ForInterval(Interval interval)
        {
            return new ZoneEqualityComparer(interval, Options.OnlyMatchWallOffset);
        }

        /// <summary>
        /// Returns a comparer operating over the same interval as this one, but with the given
        /// set of options.
        /// </summary>
        /// <remarks>
        /// This method does not modify the comparer on which it's called.
        /// </remarks>
        /// <param name="options">New set of options, which must consist of flags defined within the <see cref="Options"/> enum.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified options are invalid.</exception>
        /// <returns>A comparer operating over the same interval as this one, but with the given set of options.</returns>
        public ZoneEqualityComparer WithOptions(Options options)
        {
            return this.options == options ? this : new ZoneEqualityComparer(this.interval, options);
        }

        /// <summary>
        /// Compares two time zones for equality according to the options and interval provided to this comparer.
        /// </summary>
        /// <param name="x">The first <see cref="DateTimeZone"/> to compare.</param>
        /// <param name="y">The second <see cref="DateTimeZone"/> to compare.</param>
        /// <returns><c>true</c> if the specified time zones are equal under the options and interval of this comparer; otherwise, <c>false</c>.</returns>
        public bool Equals(DateTimeZone x, DateTimeZone y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }
            // If we ever need to port this to a platform which doesn't support LINQ,
            // we'll need to reimplement this. Until then, it would seem pointless...
            return GetIntervals(x).SequenceEqual(GetIntervals(y), zoneIntervalComparer);
        }

        /// <summary>
        /// Returns a hash code for the specified time zone.
        /// </summary>
        /// <remarks>
        /// The hash code generated by any instance of <c>ZoneEqualityComparer</c> will be equal to the hash code
        /// generated by any other instance constructed with the same options and interval, for the same time zone (or equal ones).
        /// Two instances of <c>ZoneEqualityComparer</c> with different options or intervals may (but may not) produce
        /// different hash codes for the same zone.
        /// </remarks>
        /// <param name="obj">The time zone to compute a hash code for.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(DateTimeZone obj)
        {
            Preconditions.CheckNotNull(obj, "obj");
            unchecked
            {
                int hash = 19;
                foreach (var zoneInterval in GetIntervals(obj))
                {
                    hash = hash * 31 + zoneIntervalComparer.GetHashCode(zoneInterval);
                }
                return hash;
            }
        }

        private IEnumerable<ZoneInterval> GetIntervals(DateTimeZone zone)
        {
            var allIntervals = zone.GetZoneIntervals(interval.Start, interval.End);
            return CheckOption(options, Options.MatchAllTransitions) ? allIntervals : CoalesceIntervals(allIntervals);
        }

        private IEnumerable<ZoneInterval> CoalesceIntervals(IEnumerable<ZoneInterval> zoneIntervals)
        {
            ZoneInterval current = null;
            foreach (var zoneInterval in zoneIntervals)
            {
                if (current == null)
                {
                    current = zoneInterval;
                    continue;
                }
                if (zoneIntervalComparer.EqualExceptStartAndEnd(current, zoneInterval))
                {
                    current = current.WithEnd(zoneInterval.End);
                }
                else
                {
                    yield return current;
                    current = zoneInterval;
                }
            }
            // current will only be null if start == end...
            if (current != null)
            {
                yield return current;
            }
        }

        private sealed class ZoneIntervalEqualityComparer : IEqualityComparer<ZoneInterval>
        {
            private readonly Options options;
            private readonly Interval interval;

            internal ZoneIntervalEqualityComparer(Options options, Interval interval)
            {
                this.options = options;
                this.interval = interval;
            }

            public bool Equals(ZoneInterval x, ZoneInterval y)
            {
                if (!EqualExceptStartAndEnd(x, y))
                {
                    return false;
                }
                return GetEffectiveStart(x) == GetEffectiveStart(y) &&
                    GetEffectiveEnd(x) == GetEffectiveEnd(y);
            }

            public int GetHashCode(ZoneInterval obj)
            {
                int hash = HashCodeHelper.Initialize();
                if (CheckOption(options, Options.MatchOffsetComponents))
                {
                    hash = HashCodeHelper.Hash(hash, obj.StandardOffset);
                    hash = HashCodeHelper.Hash(hash, obj.Savings);
                }
                else
                {
                    hash = HashCodeHelper.Hash(hash, obj.WallOffset);
                }
                if (CheckOption(options, Options.MatchNames))
                {
                    hash = HashCodeHelper.Hash(hash, obj.Name);
                }
                hash = HashCodeHelper.Hash(hash, GetEffectiveStart(obj));
                hash = HashCodeHelper.Hash(hash, GetEffectiveEnd(obj));
                return hash;
            }

            private Instant GetEffectiveStart(ZoneInterval zoneInterval)
            {
                return CheckOption(options, Options.MatchStartAndEndTransitions)
                    ? zoneInterval.Start : Instant.Max(zoneInterval.Start, interval.Start);                
            }

            private Instant GetEffectiveEnd(ZoneInterval zoneInterval)
            {
                return CheckOption(options, Options.MatchStartAndEndTransitions)
                    ? zoneInterval.End : Instant.Min(zoneInterval.End, interval.End);
            }

            /// <summary>
            /// Compares the parts of two zone intervals which are deemed "interesting" by the options.
            /// The wall offset is always compared, regardless of options, but the start/end points are
            /// never compared.
            /// </summary>
            internal bool EqualExceptStartAndEnd(ZoneInterval x, ZoneInterval y)
            {
                if (x.WallOffset != y.WallOffset)
                {
                    return false;
                }
                // As we've already compared wall offsets, we only need to compare savings...
                // If the savings are equal, the standard offset will be too.
                if (CheckOption(options, Options.MatchOffsetComponents) && x.Savings != y.Savings)
                {
                    return false;
                }
                if (CheckOption(options, Options.MatchNames) && x.Name != y.Name)
                {
                    return false;
                }
                return true;
            }
        }
    }
}

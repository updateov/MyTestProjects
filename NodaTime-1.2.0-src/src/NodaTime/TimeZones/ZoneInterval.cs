// Copyright 2010 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Diagnostics;
using System.Text;
using NodaTime.Utility;

namespace NodaTime.TimeZones
{
    /// <summary>
    /// Represents a range of time for which a particular Offset applies.
    /// </summary>
    /// <threadsafety>This type is an immutable reference type. See the thread safety section of the user guide for more information.</threadsafety>
    public sealed class ZoneInterval : IEquatable<ZoneInterval>
    {
        private readonly Instant end;
        private readonly LocalInstant localEnd;
        private readonly LocalInstant localStart;
        private readonly string name;
        private readonly Offset wallOffset;
        private readonly Offset savings;
        private readonly Instant start;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ZoneInterval" /> class.
        /// </summary>
        /// <param name="name">The name of this offset period (e.g. PST or PDT).</param>
        /// <param name="start">The first <see cref="Instant" /> that the <paramref name = "wallOffset" /> applies.</param>
        /// <param name="end">The last <see cref="Instant" /> (exclusive) that the <paramref name = "wallOffset" /> applies.</param>
        /// <param name="wallOffset">The <see cref="WallOffset" /> from UTC for this period including any daylight savings.</param>
        /// <param name="savings">The <see cref="WallOffset" /> daylight savings contribution to the offset.</param>
        /// <exception cref="ArgumentException">If <c><paramref name = "start" /> &gt;= <paramref name = "end" /></c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        public ZoneInterval(string name, Instant start, Instant end, Offset wallOffset, Offset savings)
        {
            Preconditions.CheckNotNull(name, "name");
            Preconditions.CheckArgument(start < end, "start", "The start Instant must be less than the end Instant");
            this.name = name;
            this.start = start;
            this.end = end;
            this.wallOffset = wallOffset;
            this.savings = savings;
            localStart = start == Instant.MinValue ? LocalInstant.MinValue : this.start.Plus(this.wallOffset);
            localEnd = end == Instant.MaxValue ? LocalInstant.MaxValue : this.end.Plus(this.wallOffset);
        }

        
        /// <summary>
        /// Returns a copy of this zone interval, but with the given start instant.
        /// </summary>
        internal ZoneInterval WithStart(Instant newStart)
        {
            return new ZoneInterval(name, newStart, end, wallOffset, savings);
        }

        /// <summary>
        /// Returns a copy of this zone interval, but with the given end instant.
        /// </summary>
        internal ZoneInterval WithEnd(Instant newEnd)
        {
            return new ZoneInterval(name, start, newEnd, wallOffset, savings);
        }

        #region Properties
        /// <summary>
        ///   Gets the standard offset for this period. This is the offset without any daylight savings
        ///   contributions.
        /// </summary>
        /// <remarks>
        ///   This is effectively <c>Offset - Savings</c>.
        /// </remarks>
        /// <value>The base Offset.</value>
        public Offset StandardOffset
        {
            [DebuggerStepThrough] get { return WallOffset - Savings; }
        }

        /// <summary>
        ///   Gets the duration of this period.
        /// </summary>
        /// <remarks>
        ///   This is effectively <c>End - Start</c>.
        /// </remarks>
        /// <value>The Duration of this period.</value>
        public Duration Duration
        {
            [DebuggerStepThrough] get { return End - Start; }
        }

        /// <summary>
        ///   Gets the last Instant (exclusive) that the Offset applies.
        /// </summary>
        /// <value>The last Instant (exclusive) that the Offset applies.</value>
        public Instant End
        {
            [DebuggerStepThrough] get { return end; }
        }

        /// <summary>
        ///   Gets the end time as a LocalInstant.
        /// </summary>
        /// <remarks>
        ///   This is effectively <c>End + Offset</c>.
        /// </remarks>
        /// <value>The ending LocalInstant.</value>
        internal LocalInstant LocalEnd
        {
            [DebuggerStepThrough] get { return localEnd; }
        }

        /// <summary>
        ///   Gets the start time as a LocalInstant.
        /// </summary>
        /// <remarks>
        ///   This is effectively <c>Start + Offset</c>.
        /// </remarks>
        /// <value>The starting LocalInstant.</value>
        internal LocalInstant LocalStart
        {
            [DebuggerStepThrough] get { return localStart; }
        }

        /// <summary>
        /// Returns the local start time of the interval, as LocalDateTime
        /// in the ISO calendar.
        /// </summary>
        public LocalDateTime IsoLocalStart
        {
            [DebuggerStepThrough] get { return new LocalDateTime(localStart); }
        }

        /// <summary>
        /// Returns the local start time of the interval, as LocalDateTime
        /// in the ISO calendar. This does not include any daylight saving 
        /// </summary>
        public LocalDateTime IsoLocalEnd
        {
            [DebuggerStepThrough]
            get { return new LocalDateTime(localEnd); }
        }
        /// <summary>
        ///   Gets the name of this offset period (e.g. PST or PDT).
        /// </summary>
        /// <value>The name of this offset period (e.g. PST or PDT).</value>
        public string Name
        {
            [DebuggerStepThrough] get { return name; }
        }

        /// <summary>
        ///   Gets the offset from UTC for this period. This includes any daylight savings value.
        /// </summary>
        /// <value>The offset from UTC for this period.</value>
        public Offset WallOffset
        {
            [DebuggerStepThrough] get { return wallOffset; }
        }

        /// <summary>
        ///   Gets the daylight savings value for this period.
        /// </summary>
        /// <value>The savings value.</value>
        public Offset Savings
        {
            [DebuggerStepThrough] get { return savings; }
        }

        /// <summary>
        ///   Gets the first Instant that the Offset applies.
        /// </summary>
        /// <value>The first Instant that the Offset applies.</value>
        public Instant Start
        {
            [DebuggerStepThrough] get { return start; }
        }
        #endregion // Properties

        #region Contains
        /// <summary>
        ///   Determines whether this period contains the given Instant in its range.
        /// </summary>
        /// <remarks>
        /// Usually this is half-open, i.e. the end is exclusive, but an interval with an end point of "the end of time" 
        /// is deemed to be inclusive at the end.
        /// </remarks>
        /// <param name="instant">The instant to test.</param>
        /// <returns>
        ///   <c>true</c> if this period contains the given Instant in its range; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        public bool Contains(Instant instant)
        {
            return Start <= instant && (instant < End || End == Instant.MaxValue);
        }

        /// <summary>
        ///   Determines whether this period contains the given LocalInstant in its range.
        /// </summary>
        /// <param name="localInstant">The local instant to test.</param>
        /// <returns>
        ///   <c>true</c> if this period contains the given LocalInstant in its range; otherwise, <c>false</c>.
        /// </returns>
        [DebuggerStepThrough]
        internal bool Contains(LocalInstant localInstant)
        {
            return LocalStart <= localInstant && (localInstant < LocalEnd || End == Instant.MaxValue);
        }
        #endregion // Contains

        #region IEquatable<ZoneInterval> Members
        /// <summary>
        ///   Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///   true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        /// </param>
        [DebuggerStepThrough]
        public bool Equals(ZoneInterval other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Name == other.Name && Start == other.Start && End == other.End && WallOffset == other.WallOffset && Savings == other.Savings;
        }
        #endregion

        #region object Overrides
        /// <summary>
        ///   Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
        /// <filterpriority>2</filterpriority>
        [DebuggerStepThrough]
        public override bool Equals(object obj)
        {
            return Equals(obj as ZoneInterval);
        }

        /// <summary>
        ///   Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///   A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            int hash = HashCodeHelper.Initialize();
            hash = HashCodeHelper.Hash(hash, Name);
            hash = HashCodeHelper.Hash(hash, Start);
            hash = HashCodeHelper.Hash(hash, End);
            hash = HashCodeHelper.Hash(hash, WallOffset);
            hash = HashCodeHelper.Hash(hash, Savings);
            return hash;
        }

        /// <summary>
        ///   Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}: [{1}, {2}) {3} ({4})", Name, Start, End, WallOffset, Savings);
        }
        #endregion // object Overrides
    }
}

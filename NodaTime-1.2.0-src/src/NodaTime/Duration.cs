// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using NodaTime.Text;
using NodaTime.Utility;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace NodaTime
{
    /// <summary>
    /// Represents a fixed (and calendar-independent) length of time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A duration is a length of time defined by an integral number of 'ticks', where a tick is equal to 100
    /// nanoseconds. There are 10,000 ticks in a millisecond.
    /// Although durations are usually used with a positive number of ticks, negative durations are valid, and may occur
    /// naturally when e.g. subtracting an earlier <see cref="Instant"/> from a later one.
    /// </para>
    /// <para>
    /// A duration represents a fixed length of elapsed time along the time line that occupies the same amount of
    /// time regardless of when it is applied.  In contrast, <see cref="Period"/> represents a period of time in
    /// calendrical terms (hours, days, and so on) that may vary in elapsed time when applied.
    /// </para>
    /// <para>
    /// In general, use <see cref="Duration"/> to represent durations applied to global types like <see cref="Instant"/>
    /// and <see cref="ZonedDateTime"/>; use <c>Period</c> to represent a period applied to local types like
    /// <see cref="LocalDateTime"/>.
    /// </para>
    /// </remarks>
    /// <threadsafety>This type is an immutable value type. See the thread safety section of the user guide for more information.</threadsafety>
#if !PCL
    [Serializable]
#endif
    public struct Duration : IEquatable<Duration>, IComparable<Duration>, IComparable, IXmlSerializable, IFormattable
#if !PCL
        , ISerializable
#endif
    {
        #region Readonly static fields
        /// <summary>
        /// Represents the zero <see cref="Duration"/> value. 
        /// This field is read-only.
        /// </summary>
        public static readonly Duration Zero = new Duration(0L);

        /// <summary>
        /// Represents the <see cref="Duration"/> value equals to 1 tick; the smallest amount by which an instant can vary.
        /// This field is read-only.
        /// </summary>
        public static readonly Duration Epsilon = new Duration(1L);

        /// <summary>
        /// Represents the <see cref="Duration"/> value equals to number of ticks in 1 standard week (7 days).
        /// This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 6,048,000,000,000 ticks.
        /// </remarks>
        internal static readonly Duration OneStandardWeek = new Duration(NodaConstants.TicksPerStandardWeek);

        /// <summary>
        /// Represents the <see cref="Duration"/> value equals to number of ticks in 1 day.
        /// This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 864 billion ticks; that is, 864,000,000,000 ticks.
        /// </remarks>
        internal static readonly Duration OneStandardDay = new Duration(NodaConstants.TicksPerStandardDay);

        /// <summary>
        /// Represents the <see cref="Duration"/> value equals to number of ticks in 1 hour.
        /// This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 36 billion ticks; that is, 36,000,000,000 ticks.
        /// </remarks>
        private static readonly Duration OneHour = new Duration(NodaConstants.TicksPerHour);

        /// <summary>
        /// Represents the <see cref="Duration"/> value equals to number of ticks in 1 minute.
        /// This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 600 million ticks; that is, 600,000,000 ticks.
        /// </remarks>
        private static readonly Duration OneMinute = new Duration(NodaConstants.TicksPerMinute);

        /// <summary>
        /// Represents the <see cref="Duration"/> value equals to number of ticks in 1 second.
        /// This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 10 million ticks; that is, 10,000,000 ticks.
        /// </remarks>
        private static readonly Duration OneSecond = new Duration(NodaConstants.TicksPerSecond);

        /// <summary>
        /// Represents the <see cref="Duration"/> value equals to number of ticks in 1 millisecond.
        /// This field is constant.
        /// </summary>
        /// <remarks>
        /// The value of this constant is 10 thousand ticks; that is, 10,000 ticks.
        /// </remarks>
        private static readonly Duration OneMillisecond = new Duration(NodaConstants.TicksPerMillisecond);
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Duration"/> struct.
        /// </summary>
        /// <param name="ticks">The number of ticks.</param>
        internal Duration(long ticks)
        {
            this.ticks = ticks;
        }

        private readonly long ticks;

        /// <summary>
        /// The total number of ticks in the duration.
        /// </summary>
        /// <remarks>
        /// This property effectively represents all of the information within a Duration value; a duration
        /// is simply a number of ticks.
        /// </remarks>
        public long Ticks { get { return ticks; } }

        #region Object overrides
        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Duration)
            {
                return Equals((Duration)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }
        #endregion

        #region Formatting
        /// <summary>
        ///   Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return DurationPattern.BclSupport.Format(this, null, CultureInfo.CurrentCulture);
        }

        /// <summary>
        ///   Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        ///   A <see cref="T:System.String" /> containing the value of the current instance in the specified format.
        /// </returns>
        /// <param name="patternText">The <see cref="T:System.String" /> specifying the pattern to use.
        ///   -or- 
        ///   null to use the default pattern defined for the type of the <see cref="T:System.IFormattable" /> implementation. 
        /// </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider" /> to use to format the value.
        ///   -or- 
        ///   null to obtain the numeric format information from the current locale setting of the operating system. 
        /// </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(string patternText, IFormatProvider formatProvider)
        {
            return DurationPattern.BclSupport.Format(this, patternText, formatProvider);
        }
        #endregion Formatting

        #region Operators
        /// <summary>
        /// Implements the operator + (addition).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the sum of the given values.</returns>
        public static Duration operator +(Duration left, Duration right)
        {
            return new Duration(left.Ticks + right.Ticks);
        }

        /// <summary>
        /// Adds one duration to another. Friendly alternative to <c>operator+()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the sum of the given values.</returns>
        public static Duration Add(Duration left, Duration right)
        {
            return left + right;
        }

        /// <summary>
        /// Returns the result of adding another duration to this one, for a fluent alternative to <c>operator+()</c>.
        /// </summary>
        /// <param name="other">The duration to add</param>
        /// <returns>A new <see cref="Duration" /> representing the result of the addition.</returns>
        public Duration Plus(Duration other)
        {
            return this + other;
        }

        /// <summary>
        /// Implements the operator - (subtraction).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the difference of the given values.</returns>
        public static Duration operator -(Duration left, Duration right)
        {
            return new Duration(left.Ticks - right.Ticks);
        }

        /// <summary>
        /// Subtracts one duration from another. Friendly alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the difference of the given values.</returns>
        public static Duration Subtract(Duration left, Duration right)
        {
            return left - right;
        }

        /// <summary>
        /// Returns the result of subtracting another duration from this one, for a fluent alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="other">The duration to subtract</param>
        /// <returns>A new <see cref="Duration" /> representing the result of the subtraction.</returns>
        public Duration Minus(Duration other)
        {
            return this - other;
        }

        /// <summary>
        /// Implements the operator / (division).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the result of dividing <paramref name="left"/> by
        /// <paramref name="right"/>.</returns>
        public static Duration operator /(Duration left, long right)
        {
            return new Duration(left.Ticks / right);
        }

        /// <summary>
        /// Divides a duration by a number. Friendly alternative to <c>operator/()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the result of dividing <paramref name="left"/> by
        /// <paramref name="right"/>.</returns>
        public static Duration Divide(Duration left, long right)
        {
            return left / right;
        }

        /// <summary>
        /// Implements the operator * (multiplication).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the result of multiplying <paramref name="left"/> by
        /// <paramref name="right"/>.</returns>
        public static Duration operator *(Duration left, long right)
        {
            return new Duration(left.Ticks * right);
        }

        /// <summary>
        /// Implements the operator * (multiplication).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the result of multiplying <paramref name="left"/> by
        /// <paramref name="right"/>.</returns>
        public static Duration operator *(long left, Duration right)
        {
            return new Duration(left * right.Ticks);
        }

        /// <summary>
        /// Multiplies a duration by a number. Friendly alternative to <c>operator*()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the product of the given values.</returns>
        public static Duration Multiply(Duration left, long right)
        {
            return left * right;
        }

        /// <summary>
        /// Multiplies a duration by a number. Friendly alternative to <c>operator*()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the product of the given values.</returns>
        public static Duration Multiply(long left, Duration right)
        {
            return left * right;
        }

        /// <summary>
        /// Implements the operator == (equality).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if values are equal to each other, otherwise <c>false</c>.</returns>
        public static bool operator ==(Duration left, Duration right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator != (inequality).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if values are not equal to each other, otherwise <c>false</c>.</returns>
        public static bool operator !=(Duration left, Duration right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implements the operator &lt; (less than).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is less than the right value, otherwise <c>false</c>.</returns>
        public static bool operator <(Duration left, Duration right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &lt;= (less than or equal).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is less than or equal to the right value, otherwise <c>false</c>.</returns>
        public static bool operator <=(Duration left, Duration right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Implements the operator &gt; (greater than).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is greater than the right value, otherwise <c>false</c>.</returns>
        public static bool operator >(Duration left, Duration right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator &gt;= (greater than or equal).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is greater than or equal to the right value, otherwise <c>false</c>.</returns>
        public static bool operator >=(Duration left, Duration right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Implements the unary negation operator.
        /// </summary>
        /// <param name="duration">Duration to negate</param>
        /// <returns>The negative value of this duration</returns>
        public static Duration operator -(Duration duration)
        {
            return new Duration(-duration.Ticks);
        }

        /// <summary>
        /// Implements a friendly alternative to the unary negation operator.
        /// </summary>
        /// <param name="duration">Duration to negate</param>
        /// <returns>The negative value of this duration</returns>
        public static Duration Negate (Duration duration)
        {
            return -duration;
        }
        #endregion // Operators

        #region IComparable<Duration> Members
        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// The return value has the following meanings:
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>&lt; 0</term>
        /// <description>This object is less than the <paramref name="other"/> parameter.</description>
        /// </item>
        /// <item>
        /// <term>0</term>
        /// <description>This object is equal to <paramref name="other"/>.</description>
        /// </item>
        /// <item>
        /// <term>&gt; 0</term>
        /// <description>This object is greater than <paramref name="other"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(Duration other)
        {
            return Ticks.CompareTo(other.Ticks);
        }

        /// <summary>
        /// Implementation of <see cref="IComparable.CompareTo"/> to compare two offsets.
        /// </summary>
        /// <remarks>
        /// This uses explicit interface implementation to avoid it being called accidentally. The generic implementation should usually be preferred.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is non-null but does not refer to an instance of <see cref="Duration"/>.</exception>
        /// <param name="obj">The object to compare this value with.</param>
        /// <returns>The result of comparing this instant with another one; see <see cref="CompareTo(NodaTime.Duration)"/> for general details.
        /// If <paramref name="obj"/> is null, this method returns a value greater than 0.
        /// </returns>
        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            Preconditions.CheckArgument(obj is Duration, "obj", "Object must be of type NodaTime.Duration.");
            return CompareTo((Duration)obj);
        }
        #endregion

        #region IEquatable<Duration> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter;
        /// otherwise, false.
        /// </returns>
        public bool Equals(Duration other)
        {
            return Ticks == other.Ticks;
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the given number of weeks, assuming a 'standard' week
        /// consisting of seven 24-hour days.
        /// </summary>
        /// <param name="weeks">The number of weeks.</param>
        /// <returns>A <see cref="Duration"/> representing the given number of weeks.</returns>
        public static Duration FromStandardWeeks(long weeks)
        {
            return OneStandardWeek * weeks;
        }

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the given number of days, assuming a 'standard' 24-hour
        /// day.
        /// </summary>
        /// <param name="days">The number of days.</param>
        /// <returns>A <see cref="Duration"/> representing the given number of days.</returns>
        public static Duration FromStandardDays(long days)
        {
            return OneStandardDay * days;
        }

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the given number of hours.
        /// </summary>
        /// <param name="hours">The number of hours.</param>
        /// <returns>A <see cref="Duration"/> representing the given number of hours.</returns>
        public static Duration FromHours(long hours)
        {
            return OneHour * hours;
        }

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the given number of minutes.
        /// </summary>
        /// <param name="minutes">The number of minutes.</param>
        /// <returns>A <see cref="Duration"/> representing the given number of minutes.</returns>
        public static Duration FromMinutes(long minutes)
        {
            return OneMinute * minutes;
        }

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the given number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds.</param>
        /// <returns>A <see cref="Duration"/> representing the given number of seconds.</returns>
        public static Duration FromSeconds(long seconds)
        {
            return OneSecond * seconds;
        }

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the given number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds.</param>
        /// <returns>A <see cref="Duration"/> representing the given number of milliseconds.</returns>
        public static Duration FromMilliseconds(long milliseconds)
        {
            return OneMillisecond * milliseconds;
        }

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the given number of ticks.
        /// </summary>
        /// <param name="ticks">The number of ticks.</param>
        /// <returns>A <see cref="Duration"/> representing the given number of ticks.</returns>
        public static Duration FromTicks(long ticks)
        {
            return new Duration(ticks);
        }

        /// <summary>
        /// Returns a <see cref="Duration"/> that represents the same number of ticks as the
        /// given <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan value to convert</param>
        /// <returns>A new Duration with the same number of ticks as the given TimeSpan.</returns>
        public static Duration FromTimeSpan(TimeSpan timeSpan)
        {
            return FromTicks(timeSpan.Ticks);
        }

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> that represents the same number of ticks as this
        /// <see cref="Duration"/>.
        /// </summary>
        /// <returns>A new TimeSpan with the same number of ticks as this Duration.</returns>
        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(ticks);
        }

        #region XML serialization
        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Preconditions.CheckNotNull(reader, "reader");
            var pattern = DurationPattern.RoundtripPattern;
            string text = reader.ReadElementContentAsString();
            this = pattern.Parse(text).Value;
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            Preconditions.CheckNotNull(writer, "writer");
            writer.WriteString(DurationPattern.RoundtripPattern.Format(this));
        }
        #endregion

#if !PCL
        #region Binary serialization
        private const string TicksSerializationName = "ticks";

        /// <summary>
        /// Private constructor only present for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to fetch data from.</param>
        /// <param name="context">The source for this deserialization.</param>
        private Duration(SerializationInfo info, StreamingContext context)
            : this(info.GetInt64(TicksSerializationName))
        {
        }

        /// <summary>
        /// Implementation of <see cref="ISerializable.GetObjectData"/>.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(TicksSerializationName, ticks);
        }
        #endregion
#endif
    }
}

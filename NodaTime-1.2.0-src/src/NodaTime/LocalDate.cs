// Copyright 2010 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using NodaTime.Calendars;
using NodaTime.Text;
using NodaTime.Utility;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace NodaTime
{
    /// <summary>
    /// LocalDate is an immutable struct representing a date within the calendar,
    /// with no reference to a particular time zone or time of day.
    /// </summary>
    /// <remarks>
    /// <para>Comparisons of dates can be handled in a way which is either calendar-sensitive or calendar-insensitive.
    /// Noda Time implements all the operators (and the <see cref="Equals(NodaTime.LocalDate)"/> method) such that all operators other than <see cref="op_Inequality"/>
    /// will return false if asked to compare two values in different calendar systems.
    /// </para>
    /// <para>
    /// However, the <see cref="CompareTo"/> method (implementing <see cref="IComparable{T}"/>) is calendar-insensitive; it compares the two
    /// dates historically in terms of when they actually occurred, as if they're both converted to some "neutral" calendar system first.
    /// </para>
    /// <para>
    /// It's unclear at the time of this writing whether this is the most appropriate approach, and it may change in future versions. In general,
    /// it would be a good idea for users to avoid comparing dates in different calendar systems, and indeed most users are unlikely to ever explicitly
    /// consider which calendar system they're working in anyway.
    /// </para>
    /// </remarks>
    /// <threadsafety>This type is an immutable value type. See the thread safety section of the user guide for more information.</threadsafety>
#if !PCL
    [Serializable]
#endif
    public struct LocalDate : IEquatable<LocalDate>, IComparable<LocalDate>, IComparable, IFormattable, IXmlSerializable
#if !PCL
        , ISerializable
#endif
    {
        private readonly LocalDateTime localTime;

        /// <summary>
        /// Constructs an instance for the given year, month and day in the ISO calendar.
        /// </summary>
        /// <param name="year">The year. This is the "absolute year", so a value of 0 means 1 BC, for example.</param>
        /// <param name="month">The month of year.</param>
        /// <param name="day">The day of month.</param>
        /// <returns>The resulting date.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The parameters do not form a valid date.</exception>
        public LocalDate(int year, int month, int day)
            : this(year, month, day, CalendarSystem.Iso)
        {
        }

        /// <summary>
        /// Constructs an instance for the given year, month and day in the specified calendar.
        /// </summary>
        /// <param name="year">The year. This is the "absolute year", so, for
        /// the ISO calendar, a value of 0 means 1 BC, for example.</param>
        /// <param name="month">The month of year.</param>
        /// <param name="day">The day of month.</param>
        /// <param name="calendar">Calendar system in which to create the date.</param>
        /// <returns>The resulting date.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The parameters do not form a valid date.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="calendar"/> is null.</exception>
        public LocalDate(int year, int month, int day, CalendarSystem calendar)
            : this(new LocalDateTime(year, month, day, 0, 0, calendar))
        {
        }

        /// <summary>
        /// Constructs an instance for the given era, year of era, month and day in the ISO calendar.
        /// </summary>
        /// <param name="era">The era within which to create a date. Must be a valid era within the ISO calendar.</param>
        /// <param name="yearOfEra">The year of era.</param>
        /// <param name="month">The month of year.</param>
        /// <param name="day">The day of month.</param>
        /// <returns>The resulting date.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The parameters do not form a valid date.</exception>
        public LocalDate(Era era, int yearOfEra, int month, int day)
            : this(era, yearOfEra, month, day, CalendarSystem.Iso)
        {
        }

        /// <summary>
        /// Constructs an instance for the given era, year of era, month and day in the specified calendar.
        /// </summary>
        /// <param name="era">The era within which to create a date. Must be a valid era within the specified calendar.</param>
        /// <param name="yearOfEra">The year of era.</param>
        /// <param name="month">The month of year.</param>
        /// <param name="day">The day of month.</param>
        /// <param name="calendar">Calendar system in which to create the date.</param>
        /// <returns>The resulting date.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The parameters do not form a valid date.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="calendar"/> is null.</exception>
        public LocalDate(Era era, int yearOfEra, int month, int day, CalendarSystem calendar)
            : this(new LocalDateTime(Preconditions.CheckNotNull(calendar, "calendar").GetLocalInstant(era, yearOfEra, month, day), calendar))
        {
        }

        // Visible for extension methods.
        internal LocalDate(LocalDateTime localTime)
        {
            this.localTime = localTime;
        }

        /// <summary>Gets the calendar system associated with this local date.</summary>
        public CalendarSystem Calendar { get { return localTime.Calendar; } }

        /// <summary>Gets the year of this local date.</summary>
        /// <remarks>This returns the "absolute year", so, for the ISO calendar,
        /// a value of 0 means 1 BC, for example.</remarks>
        public int Year { get { return localTime.Year; } }

        /// <summary>Gets the month of this local date within the year.</summary>
        public int Month { get { return localTime.Month; } }

        /// <summary>Gets the day of this local date within the month.</summary>
        public int Day { get { return localTime.Day; } }

        /// <summary>
        /// Gets the week day of this local date expressed as an <see cref="NodaTime.IsoDayOfWeek"/> value,
        /// for calendars which use ISO days of the week.
        /// </summary>
        /// <exception cref="InvalidOperationException">The underlying calendar doesn't use ISO days of the week.</exception>
        /// <seealso cref="DayOfWeek"/>
        public IsoDayOfWeek IsoDayOfWeek { get { return localTime.IsoDayOfWeek; } }

        /// <summary>
        /// Gets the week day of this local date as a number.
        /// </summary>
        /// <remarks>
        /// For calendars using ISO week days, this gives 1 for Monday to 7 for Sunday.
        /// </remarks>
        /// <seealso cref="IsoDayOfWeek"/>
        public int DayOfWeek { get { return localTime.DayOfWeek; } }

        /// <summary>
        /// Gets the "week year" of this local date.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The WeekYear is the year that matches with the <see cref="WeekOfWeekYear"/> field.
        /// In the standard ISO8601 week algorithm, the first week of the year
        /// is that in which at least 4 days are in the year. As a result of this
        /// definition, day 1 of the first week may be in the previous year.
        /// The WeekYear allows you to query the effective year for that day.
        /// </para>
        /// <para>
        /// For example, January 1st 2011 was a Saturday, so only two days of that week
        /// (Saturday and Sunday) were in 2011. Therefore January 1st is part of
        /// week 52 of WeekYear 2010. Conversely, December 31st 2012 is a Monday,
        /// so is part of week 1 of WeekYear 2013.
        /// </para>
        /// </remarks>
        public int WeekYear { get { return localTime.WeekYear; } }

        /// <summary>Gets the week within the WeekYear. See <see cref="WeekYear"/> for more details.</summary>
        public int WeekOfWeekYear { get { return localTime.WeekOfWeekYear; } }

        /// <summary>Gets the year of this local date within the century.</summary>
        /// <remarks>This always returns a value in the range 0 to 99 inclusive.</remarks>
        public int YearOfCentury { get { return localTime.YearOfCentury; } }

        /// <summary>Gets the year of this local date within the era.</summary>
        public int YearOfEra { get { return localTime.YearOfEra; } }

        /// <summary>Gets the era of this local date.</summary>
        public Era Era { get { return localTime.Era; } }

        /// <summary>Gets the day of this local date within the year.</summary>
        public int DayOfYear { get { return localTime.DayOfYear; } }

        /// <summary>
        /// Gets a <see cref="LocalDateTime" /> at midnight on the date represented by this local date.
        /// </summary>
        /// <returns>The <see cref="LocalDateTime" /> representing midnight on tthis local date, in the same calendar
        /// system.</returns>
        public LocalDateTime AtMidnight()
        {
            return localTime;
        }

        /// <summary>
        /// Returns the local date corresponding to the given "week year", "week of week year", and "day of week"
        /// in the ISO calendar system.
        /// </summary>
        /// <param name="weekYear">ISO-8601 week year of value to return</param>
        /// <param name="weekOfWeekYear">ISO-8601 week of week year of value to return</param>
        /// <param name="dayOfWeek">ISO-8601 day of week to return</param>
        /// <returns>The date corresponding to the given week year / week of week year / day of week.</returns>
        public static LocalDate FromWeekYearWeekAndDay(int weekYear, int weekOfWeekYear, IsoDayOfWeek dayOfWeek)
        {
            LocalInstant localInstant = CalendarSystem.Iso.GetLocalInstantFromWeekYearWeekAndDayOfWeek(weekYear, weekOfWeekYear, dayOfWeek);
            return new LocalDate(new LocalDateTime(localInstant));
        }

        /// <summary>
        /// Adds the specified period to the date.
        /// </summary>
        /// <param name="date">The date to add the period to</param>
        /// <param name="period">The period to add. Must not contain any (non-zero) time units.</param>
        /// <returns>The sum of the given date and period</returns>
        public static LocalDate operator +(LocalDate date, Period period)
        {
            Preconditions.CheckNotNull(period, "period");
            Preconditions.CheckArgument(!period.HasTimeComponent, "period", "Cannot add a period with a time component to a date");
            return new LocalDate(date.localTime + period);
        }

        /// <summary>
        /// Adds the specified period to the date. Friendly alternative to <c>operator+()</c>.
        /// </summary>
        /// <param name="date">The date to add the period to</param>
        /// <param name="period">The period to add. Must not contain any (non-zero) time units.</param>
        /// <returns>The sum of the given date and period</returns>
        public static LocalDate Add(LocalDate date, Period period)
        {
            return date + period;
        }

        /// <summary>
        /// Adds the specified period to this date. Fluent alternative to <c>operator+()</c>.
        /// </summary>
        /// <param name="period">The period to add. Must not contain any (non-zero) time units.</param>
        /// <returns>The sum of this date and the given period</returns>
        public LocalDate Plus(Period period)
        {
            return this + period;
        }

        /// <summary>
        /// Combines the given <see cref="LocalDate"/> and <see cref="LocalTime"/> components
        /// into a single <see cref="LocalDateTime"/>.
        /// </summary>
        /// <param name="date">The date to add the time to</param>
        /// <param name="time">The time to add</param>
        /// <returns>The sum of the given date and time</returns>
        public static LocalDateTime operator +(LocalDate date, LocalTime time)
        {
            LocalInstant localDateInstant = date.localTime.LocalInstant;
            LocalInstant localInstant = new LocalInstant(localDateInstant.Ticks + time.TickOfDay);
            return new LocalDateTime(localInstant, date.localTime.Calendar);
        }

        /// <summary>
        /// Subtracts the specified period from the date.
        /// </summary>
        /// <param name="date">The date to subtract the period from</param>
        /// <param name="period">The period to subtract. Must not contain any (non-zero) time units.</param>
        /// <returns>The result of subtracting the given period from the date</returns>
        public static LocalDate operator -(LocalDate date, Period period)
        {
            Preconditions.CheckNotNull(period, "period");
            Preconditions.CheckArgument(!period.HasTimeComponent, "period", "Cannot subtract a period with a time component from a date");
            return new LocalDate(date.localTime - period);
        }

        /// <summary>
        /// Subtracts the specified period from the date. Friendly alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="date">The date to subtract the period from</param>
        /// <param name="period">The period to subtract. Must not contain any (non-zero) time units.</param>
        /// <returns>The result of subtracting the given period from the date.</returns>
        public static LocalDate Subtract(LocalDate date, Period period)
        {
            return date - period;
        }

        /// <summary>
        /// Subtracts the specified period from this date. Fluent alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="period">The period to subtract. Must not contain any (non-zero) time units.</param>
        /// <returns>The result of subtracting the given period from this date.</returns>
        public LocalDate Minus(Period period)
        {
            return this - period;
        }

        /// <summary>
        /// Compares two <see cref="LocalDate" /> values for equality. This requires
        /// that the dates be the same, within the same calendar.
        /// </summary>
        /// <param name="lhs">The first value to compare</param>
        /// <param name="rhs">The second value to compare</param>
        /// <returns>True if the two dates are the same and in the same calendar; false otherwise</returns>
        public static bool operator ==(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime == rhs.localTime;
        }

        /// <summary>
        /// Compares two <see cref="LocalDate" /> values for inequality.
        /// </summary>
        /// <param name="lhs">The first value to compare</param>
        /// <param name="rhs">The second value to compare</param>
        /// <returns>False if the two dates are the same and in the same calendar; true otherwise</returns>
        public static bool operator !=(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime != rhs.localTime;
        }

        /// <summary>
        /// Compares two LocalDate values to see if the left one is strictly earlier than the right
        /// one.
        /// </summary>
        /// <remarks>
        /// This operator always returns false if the two operands have different calendars. See the top-level type
        /// documentation for more information about comparisons.
        /// </remarks>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <returns>true if the <paramref name="lhs"/> is strictly earlier than <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator <(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime < rhs.localTime;
        }

        /// <summary>
        /// Compares two LocalDate values to see if the left one is earlier than or equal to the right
        /// one.
        /// </summary>
        /// <remarks>
        /// This operator always returns false if the two operands have different calendars. See the top-level type
        /// documentation for more information about comparisons.
        /// </remarks>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <returns>true if the <paramref name="lhs"/> is earlier than or equal to <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator <=(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime <= rhs.localTime;
        }

        /// <summary>
        /// Compares two LocalDate values to see if the left one is strictly later than the right
        /// one.
        /// </summary>
        /// <remarks>
        /// This operator always returns false if the two operands have different calendars. See the top-level type
        /// documentation for more information about comparisons.
        /// </remarks>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <returns>true if the <paramref name="lhs"/> is strictly later than <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator >(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime > rhs.localTime;
        }

        /// <summary>
        /// Compares two LocalDate values to see if the left one is later than or equal to the right
        /// one.
        /// </summary>
        /// <remarks>
        /// This operator always returns false if the two operands have different calendars. See the top-level type
        /// documentation for more information about comparisons.
        /// </remarks>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <returns>true if the <paramref name="lhs"/> is later than or equal to <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator >=(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime >= rhs.localTime;
        }

        /// <summary>
        /// Indicates whether this date is earlier, later or the same as another one.
        /// </summary>
        /// <remarks>
        /// The comparison is performed in terms of a calendar-independent notion of date;
        /// the calendar systems of both <see cref="LocalDate" /> values are ignored. When both values use the same calendar,
        /// this is absolutely natural. However, when comparing a value in one calendar with a value in another,
        /// this can lead to surprising results. For example, 1945 in the ISO calendar corresponds to around 1364
        /// in the Islamic calendar, so an Islamic date in year 1400 is "after" a date in 1945 in the ISO calendar.
        /// </remarks>
        /// <param name="other">The other date to compare this one with</param>
        /// <returns>A value less than zero if this date is earlier than <paramref name="other"/>;
        /// zero if this date is the same as <paramref name="other"/>; a value greater than zero if this date is
        /// later than <paramref name="other"/>.</returns>
        public int CompareTo(LocalDate other)
        {
            return localTime.CompareTo(other.localTime);
        }

        /// <summary>
        /// Implementation of <see cref="IComparable.CompareTo"/> to compare two LocalDates.
        /// </summary>
        /// <remarks>
        /// This uses explicit interface implementation to avoid it being called accidentally. The generic implementation should usually be preferred.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is non-null but does not refer to an instance of <see cref="LocalDate"/>.</exception>
        /// <param name="obj">The object to compare this value with.</param>
        /// <returns>The result of comparing this LocalDate with another one; see <see cref="CompareTo(NodaTime.LocalDate)"/> for general details.
        /// If <paramref name="obj"/> is null, this method returns a value greater than 0.
        /// </returns>
        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            Preconditions.CheckArgument(obj is LocalDate, "obj", "Object must be of type NodaTime.LocalDate.");
            return CompareTo((LocalDate)obj);
        }

        /// <summary>
        /// Returns a hash code for this local date.
        /// </summary>
        /// <returns>A hash code for this local date.</returns>
        public override int GetHashCode()
        {
            return localTime.GetHashCode();
        }

        /// <summary>
        /// Compares two <see cref="LocalDate"/> values for equality. This requires
        /// that the dates be the same, within the same calendar.
        /// </summary>
        /// <param name="obj">The object to compare this date with.</param>
        /// <returns>True if the given value is another local date equal to this one; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is LocalDate))
            {
                return false;
            }
            return this == (LocalDate)obj;
        }

        /// <summary>
        /// Compares two <see cref="LocalDate"/> values for equality. This requires
        /// that the dates be the same, within the same calendar.
        /// </summary>
        /// <param name="other">The value to compare this date with.</param>
        /// <returns>True if the given value is another local date equal to this one; false otherwise.</returns>
        public bool Equals(LocalDate other)
        {
            return this == other;
        }

        /// <summary>
        /// Creates a new LocalDate representing the same physical date, but in a different calendar.
        /// The returned LocalDate is likely to have different field values to this one.
        /// For example, January 1st 1970 in the Gregorian calendar was December 19th 1969 in the Julian calendar.
        /// </summary>
        /// <param name="calendarSystem">The calendar system to convert this local date to.</param>
        /// <returns>The converted LocalDate</returns>
        /// <exception cref="ArgumentNullException"><paramref name="calendarSystem"/> is null.</exception>
        public LocalDate WithCalendar(CalendarSystem calendarSystem)
        {
            return new LocalDate(localTime.WithCalendar(calendarSystem));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of years added.
        /// </summary>
        /// <remarks>
        /// If the resulting date is invalid, lower fields (typically the day of month) are reduced to find a valid value.
        /// For example, adding one year to February 29th 2012 will return February 28th 2013; subtracting one year from
        /// February 29th 2012 will return February 28th 2011.
        /// </remarks>
        /// <param name="years">The number of years to add</param>
        /// <returns>The current value plus the given number of years.</returns>
        public LocalDate PlusYears(int years)
        {
            return new LocalDate(localTime.PlusYears(years));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of months added.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method does not try to maintain the year of the current value, so adding four months to a value in 
        /// October will result in a value in the following February.
        /// </para>
        /// <para>
        /// If the resulting date is invalid, the day of month is reduced to find a valid value.
        /// For example, adding one month to January 30th 2011 will return February 28th 2011; subtracting one month from
        /// March 30th 2011 will return February 28th 2011.
        /// </para>
        /// </remarks>
        /// <param name="months">The number of months to add</param>
        /// <returns>The current date plus the given number of months</returns>
        public LocalDate PlusMonths(int months)
        {
            return new LocalDate(localTime.PlusMonths(months));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of days added.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method does not try to maintain the month or year of the current value, so adding 3 days to a value of January 30th
        /// will result in a value of February 2nd.
        /// </para>
        /// </remarks>
        /// <param name="days">The number of days to add</param>
        /// <returns>The current value plus the given number of days.</returns>
        public LocalDate PlusDays(int days)
        {
            return new LocalDate(localTime.PlusDays(days));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of weeks added.
        /// </summary>
        /// <param name="weeks">The number of weeks to add</param>
        /// <returns>The current value plus the given number of weeks.</returns>
        public LocalDate PlusWeeks(int weeks)
        {
            return new LocalDate(localTime.PlusWeeks(weeks));
        }

        /// <summary>
        /// Returns the next <see cref="LocalDate" /> falling on the specified <see cref="IsoDayOfWeek"/>.
        /// This is a strict "next" - if this date on already falls on the target
        /// day of the week, the returned value will be a week later.
        /// </summary>
        /// <param name="targetDayOfWeek">The ISO day of the week to return the next date of.</param>
        /// <returns>The next <see cref="LocalDate"/> falling on the specified day of the week.</returns>
        /// <exception cref="InvalidOperationException">The underlying calendar doesn't use ISO days of the week.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetDayOfWeek"/> is not a valid day of the
        /// week (Monday to Sunday).</exception>
        public LocalDate Next(IsoDayOfWeek targetDayOfWeek)
        {
            // LocalDateTime.Next performs all the validation we need.
            return new LocalDate(localTime.Next(targetDayOfWeek));
        }

        /// <summary>
        /// Returns the previous <see cref="LocalDate" /> falling on the specified <see cref="IsoDayOfWeek"/>.
        /// This is a strict "previous" - if this date on already falls on the target
        /// day of the week, the returned value will be a week earlier.
        /// </summary>
        /// <param name="targetDayOfWeek">The ISO day of the week to return the previous date of.</param>
        /// <returns>The previous <see cref="LocalDate"/> falling on the specified day of the week.</returns>
        /// <exception cref="InvalidOperationException">The underlying calendar doesn't use ISO days of the week.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetDayOfWeek"/> is not a valid day of the
        /// week (Monday to Sunday).</exception>
        public LocalDate Previous(IsoDayOfWeek targetDayOfWeek)
        {
            // LocalDateTime.Next performs all the validation we need.
            return new LocalDate(localTime.Previous(targetDayOfWeek));
        }

        #region Formatting
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// The value of the current instance in the standard format pattern, using the current thread's
        /// culture to obtain a format provider.
        /// </returns>
        public override string ToString()
        {
            return LocalDatePattern.BclSupport.Format(this, null, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats the value of the current instance using the specified pattern.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> containing the value of the current instance in the specified format.
        /// </returns>
        /// <param name="patternText">The <see cref="T:System.String" /> specifying the pattern to use,
        /// or null to use the default format pattern.
        /// </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider" /> to use when formatting the value,
        /// or null to use the current thread's culture to obtain a format provider.
        /// </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(string patternText, IFormatProvider formatProvider)
        {
            return LocalDatePattern.BclSupport.Format(this, patternText, formatProvider);
        }
        #endregion Formatting

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
            var pattern = LocalDatePattern.IsoPattern;
            if (reader.MoveToAttribute("calendar"))
            {
                string newCalendarId = reader.Value;
                CalendarSystem newCalendar = CalendarSystem.ForId(newCalendarId);
                var newTemplateValue = pattern.TemplateValue.WithCalendar(newCalendar);
                pattern = pattern.WithTemplateValue(newTemplateValue);
                reader.MoveToElement();
            }
            string text = reader.ReadElementContentAsString();
            this = pattern.Parse(text).Value;
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            Preconditions.CheckNotNull(writer, "writer");
            if (Calendar != CalendarSystem.Iso)
            {
                writer.WriteAttributeString("calendar", Calendar.Id);
            }
            writer.WriteString(LocalDatePattern.IsoPattern.Format(this));
        }
        #endregion

#if !PCL
        #region Binary serialization
        private const string LocalTicksSerializationName = "ticks";
        private const string CalendarIdSerializationName = "calendar";

        /// <summary>
        /// Private constructor only present for serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to fetch data from.</param>
        /// <param name="context">The source for this deserialization.</param>
        private LocalDate(SerializationInfo info, StreamingContext context)
            : this(new LocalDateTime(new LocalInstant(info.GetInt64(LocalTicksSerializationName)),
                                     CalendarSystem.ForId(info.GetString(CalendarIdSerializationName))))
        {
        }

        /// <summary>
        /// Implementation of <see cref="ISerializable.GetObjectData"/>.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination for this serialization.</param>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(LocalTicksSerializationName, localTime.LocalInstant.Ticks);
            info.AddValue(CalendarIdSerializationName, Calendar.Id);
        }
        #endregion
#endif
    }
}

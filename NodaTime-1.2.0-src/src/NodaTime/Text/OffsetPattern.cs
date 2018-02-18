// Copyright 2011 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Globalization;
using NodaTime.Text.Patterns;
using NodaTime.Utility;
using System.Globalization;

namespace NodaTime.Text
{
    /// <summary>
    /// Represents a pattern for parsing and formatting <see cref="Offset"/> values.
    /// </summary>
    /// <threadsafety>
    /// When used with a read-only <see cref="CultureInfo" />, this type is immutable and instances
    /// may be shared freely between threads. We recommend only using read-only cultures for patterns, although this is
    /// not currently enforced.
    /// </threadsafety>
    public sealed class OffsetPattern : IPattern<Offset>
    {
        /// <summary>
        /// The "general" offset pattern (e.g. +HH, +HH:mm, +HH:mm:ss, +HH:mm:ss.fff) for the invariant culture.
        /// </summary>
        public static readonly OffsetPattern GeneralInvariantPattern = CreateWithInvariantCulture("g");
        /// <summary>
        /// The "general" offset pattern (e.g. +HH, +HH:mm, +HH:mm:ss, +HH:mm:ss.fff) for the invariant culture,
        /// but producing (and allowing) Z as a value for a zero offset.
        /// </summary>
        public static readonly OffsetPattern GeneralInvariantPatternWithZ = CreateWithInvariantCulture("G");

        private const string DefaultFormatPattern = "g";

        internal static readonly PatternBclSupport<Offset> BclSupport = new PatternBclSupport<Offset>(DefaultFormatPattern, fi => fi.OffsetPatternParser);

        private readonly string patternText;
        private readonly NodaFormatInfo formatInfo;
        private readonly IPartialPattern<Offset> pattern;

        /// <summary>
        /// Returns the pattern text for this pattern, as supplied on creation.
        /// </summary>
        public string PatternText { get { return patternText; } }

        /// <summary>
        /// Returns the localization information used in this pattern.
        /// </summary>
        internal NodaFormatInfo FormatInfo { get { return formatInfo; } }

        private OffsetPattern(string patternText, NodaFormatInfo formatInfo, IPartialPattern<Offset> pattern)
        {
            this.patternText = patternText;
            this.formatInfo = formatInfo;
            this.pattern = pattern;
        }

        /// <summary>
        /// Returns the pattern that this object delegates to. Mostly useful to avoid this public class
        /// implementing an internal interface.
        /// </summary>
        internal IPartialPattern<Offset> UnderlyingPattern { get { return pattern; } }

        /// <summary>
        /// Parses the given text value according to the rules of this pattern.
        /// </summary>
        /// <remarks>
        /// This method never throws an exception (barring a bug in Noda Time itself). Even errors such as
        /// the argument being null are wrapped in a parse result.
        /// </remarks>
        /// <param name="text">The text value to parse.</param>
        /// <returns>The result of parsing, which may be successful or unsuccessful.</returns>
        public ParseResult<Offset> Parse(string text)
        {
            return pattern.Parse(text);
        }

        /// <summary>
        /// Formats the given offset as text according to the rules of this pattern.
        /// </summary>
        /// <param name="value">The offset to format.</param>
        /// <returns>The offset formatted according to this pattern.</returns>
        public string Format(Offset value)
        {
            return pattern.Format(value);
        }

        /// <summary>
        /// Creates a pattern for the given pattern text and format info.
        /// </summary>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <param name="formatInfo">Localization information</param>
        /// <returns>A pattern for parsing and formatting offsets.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        internal static OffsetPattern Create(string patternText, NodaFormatInfo formatInfo)
        {
            Preconditions.CheckNotNull(patternText, "patternText");
            Preconditions.CheckNotNull(formatInfo, "formatInfo");
            var pattern = (IPartialPattern<Offset>) formatInfo.OffsetPatternParser.ParsePattern(patternText);
            return new OffsetPattern(patternText, formatInfo, pattern);
        }

        /// <summary>
        /// Creates a pattern for the given pattern text and culture.
        /// </summary>
        /// <remarks>
        /// See the user guide for the available pattern text options.
        /// </remarks>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <param name="cultureInfo">The culture to use in the pattern</param>
        /// <returns>A pattern for parsing and formatting offsets.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        public static OffsetPattern Create(string patternText, CultureInfo cultureInfo)
        {
            return Create(patternText, NodaFormatInfo.GetFormatInfo(cultureInfo));
        }

        /// <summary>
        /// Creates a pattern for the given pattern text in the current thread's current culture.
        /// </summary>
        /// <remarks>
        /// See the user guide for the available pattern text options. Note that the current culture
        /// is captured at the time this method is called - it is not captured at the point of parsing
        /// or formatting values.
        /// </remarks>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <returns>A pattern for parsing and formatting offsets.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        public static OffsetPattern CreateWithCurrentCulture(string patternText)
        {
            return Create(patternText, NodaFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// Creates a pattern for the given pattern text in the invariant culture.
        /// </summary>
        /// <remarks>
        /// See the user guide for the available pattern text options. Note that the current culture
        /// is captured at the time this method is called - it is not captured at the point of parsing
        /// or formatting values.
        /// </remarks>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <returns>A pattern for parsing and formatting offsets.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        public static OffsetPattern CreateWithInvariantCulture(string patternText)
        {
            return Create(patternText, NodaFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Creates a pattern for the same original pattern text as this pattern, but with the specified
        /// culture.
        /// </summary>
        /// <param name="cultureInfo">The culture to use in the new pattern.</param>
        /// <returns>A new pattern with the given culture.</returns>
        public OffsetPattern WithCulture(CultureInfo cultureInfo)
        {
            return Create(patternText, NodaFormatInfo.GetFormatInfo(cultureInfo));
        }
    }
}

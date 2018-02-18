// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

#if !PCL

using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NodaTime.TimeZones.Cldr;
using NodaTime.Utility;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace NodaTime.TimeZones.IO
{
    /// <summary>
    /// Matching class for TzdbResourceWriter in the NodaTime.TzdbCompiler project. This
    /// knows how to read TZDB information from a .NET resource file.
    /// </summary>
    internal sealed class TzdbResourceData : ITzdbDataSource
    {
        private static readonly Regex InvalidResourceNameCharacters = new Regex("[^A-Za-z0-9_/]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// The resource key for the Windows to TZDB ID mapping dictionary.
        /// This resource key contains hyphens, so cannot conflict with a time zone name.
        /// </summary>
        internal const string WindowsToPosixMapKey = "--meta-WindowsToPosix";

        /// <summary>
        /// The resource key for the Windows to TZDB ID mapping version string.
        /// This resource key contains hyphens, so cannot conflict with a time zone name.
        /// </summary>
        internal const string WindowsToPosixMapVersionKey = "--meta-WindowsToPosixVersion";

        /// <summary>
        /// The resource key for the timezone ID alias dictionary.
        /// This resource key contains hyphens, so cannot conflict with a time zone name.
        /// </summary>
        internal const string IdMapKey = "--meta-IdMap";

        /// <summary>
        /// The resource key for the TZDB version ID.
        /// This resource key contains hyphens, so cannot conflict with a time zone name.
        /// </summary>
        internal const string VersionKey = "--meta-VersionId";

        private readonly string tzdbVersion;
        private readonly WindowsZones windowsMapping;
        private readonly IDictionary<string, string> tzdbIdMap;
        private readonly ResourceSet source;

        private TzdbResourceData(ResourceSet source)
        {
            this.source = source;
            tzdbIdMap = CheckKey(LoadDictionary(source, IdMapKey), IdMapKey);
            tzdbVersion = CheckKey(source.GetString(VersionKey), VersionKey);
            var windowsMappingVersion = CheckKey(source.GetString(WindowsToPosixMapVersionKey), WindowsToPosixMapVersionKey);
            var windowsMapping = CheckKey(LoadDictionary(source, WindowsToPosixMapKey), WindowsToPosixMapKey);
            this.windowsMapping = WindowsZones.FromPrimaryMapping(windowsMappingVersion, windowsMapping);
        }

        private static T CheckKey<T>(T value, string key) where T : class
        {
            if (value == null)
            {
                throw new InvalidNodaDataException("Missing or invalid data for resource key " + key);
            }
            return value;
        }

        /// <inheritdoc />
        public string TzdbVersion { get { return tzdbVersion; } }

        /// <inheritdoc />
        public IDictionary<string, string> TzdbIdMap { get { return tzdbIdMap; } }

        /// <inheritdoc />
        public WindowsZones WindowsMapping { get { return windowsMapping; } }

        /// <inheritdoc />
        public DateTimeZone CreateZone(string id, string canonicalId)
        {
            object obj = source.GetObject(NormalizeAsResourceName(canonicalId));
            // We should never be asked for time zones which don't exist.
            Preconditions.CheckArgument(obj != null, "canonicalId", "ID is not one of the recognized time zone identifiers within this resource");
            if (!(obj is byte[]))
            {
                throw new InvalidNodaDataException("Resource key for time zone for ID " + canonicalId + " is not a byte array");
            }
            byte[] bytes = (byte[])obj;
            using (var stream = new MemoryStream(bytes))
            {
                var reader = new LegacyDateTimeZoneReader(stream, null);
                return reader.ReadTimeZone(id);
            }
        }

        /// <summary>
        /// Loads a dictionary of string to string with the given name from the given resource manager.
        /// </summary>
        /// <param name="source">The <see cref="ResourceSet"/> to load from.</param>
        /// <param name="name">The resource name.</param>
        /// <returns>The <see cref="IDictionary{TKey,TValue}"/> or null if there is no such resource.</returns>
        private static IDictionary<string, string> LoadDictionary(ResourceSet source, string name)
        {
            Preconditions.CheckNotNull(source, "source");
            var bytes = source.GetObject(name) as byte[];
            if (bytes != null)
            {
                using (var stream = new MemoryStream(bytes))
                {
                    var reader = new LegacyDateTimeZoneReader(stream, null);
                    return reader.ReadDictionary();
                }
            }
            return null;
        }

        internal static TzdbResourceData FromResourceManager(ResourceManager manager)
        {
            return FromResourceSet(manager.GetResourceSet(CultureInfo.CurrentUICulture, true, true));
        }

        internal static TzdbResourceData FromResourceSet(ResourceSet source)
        {
            Preconditions.CheckNotNull(source, "source");
            return new TzdbResourceData(source);
        }

        internal static ITzdbDataSource FromResource(string baseName, Assembly assembly)
        {
            Preconditions.CheckNotNull(baseName, "baseName");
            Preconditions.CheckNotNull(assembly, "assembly");
            // Special-case the only situation which would ever have worked with the Noda Time assembly,
            // and load the blob version instead.
            if (assembly == typeof(TzdbResourceData).Assembly && baseName == "NodaTime.TimeZones.Tzdb")
            {
                using (Stream stream = assembly.GetManifestResourceStream("NodaTime.TimeZones.Tzdb.nzd"))
                {
                    return TzdbStreamData.FromStream(stream);
                }
            }
            return FromResourceManager(new ResourceManager(baseName, assembly));
        }

        /// <summary>
        /// Normalizes the given name into a valid resource name by replacing invalid
        /// characters with alternatives. This is used to ensure a valid resource name is
        /// used for each time zone resource.
        /// </summary>
        /// <param name="name">The name to normalize.</param>
        /// <returns>The normalized name.</returns>
        internal static string NormalizeAsResourceName(string name)
        {
            Preconditions.CheckNotNull(name, "name");
            name = name.Replace("-", "_minus_");
            name = name.Replace("+", "_plus_");
            name = name.Replace("<", "_less_");
            name = name.Replace(">", "_greater_");
            name = name.Replace("&", "_and_");
            return InvalidResourceNameCharacters.Replace(name, "_");
        }

        /// <summary>
        /// Always returns null - the resource data does not include zone locations.
        /// </summary>
        public IList<TzdbZoneLocation> ZoneLocations
        {
            get { return null; }
        }

        /// <summary>
        /// Always returns null - the resource data does not include any additional mappings.
        /// </summary>
        IDictionary<string, string> ITzdbDataSource.WindowsAdditionalStandardNameToIdMapping
        {
            get { return null; }
        }
    }
}

#endif

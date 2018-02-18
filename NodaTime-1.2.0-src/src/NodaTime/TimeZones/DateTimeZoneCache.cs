// Copyright 2010 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodaTime.Utility;

namespace NodaTime.TimeZones
{
    /// <summary>
    /// Provides an implementation of <see cref="IDateTimeZoneProvider"/> that caches results from an
    /// <see cref="IDateTimeZoneSource"/>.
    /// </summary>
    /// <remarks>
    /// The process of loading or creating time zones may be an expensive operation. This class implements an
    /// unlimited-size non-expiring cache over a time zone source, and adapts an implementation of the
    /// <c>IDateTimeZoneSource</c> interface to an <c>IDateTimeZoneProvider</c>.
    /// </remarks>
    /// <seealso cref="DateTimeZoneProviders"/>
    /// <threadsafety>All members of this type are thread-safe as long as the underlying <c>IDateTimeZoneSource</c>
    /// implementation is thread-safe.</threadsafety>
    public sealed class DateTimeZoneCache : IDateTimeZoneProvider
    {
        private readonly object accessLock = new object();
        private readonly IDateTimeZoneSource source;
        private readonly ReadOnlyCollection<string> ids;
        private readonly IDictionary<string, DateTimeZone> timeZoneMap = new Dictionary<string, DateTimeZone>();
        private readonly string providerVersionId;

        /// <summary>
        /// Creates a provider backed by the given <see cref="IDateTimeZoneSource"/>.
        /// </summary>
        /// <remarks>
        /// Note that the source will never be consulted for requests for the fixed-offset timezones "UTC" and
        /// "UTC+/-Offset" (a standard implementation will be returned instead). This is true even if these IDs are
        /// advertised by the source.
        /// </remarks>
        /// <param name="source">The <see cref="IDateTimeZoneSource"/> for this provider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="InvalidDateTimeZoneSourceException"><paramref name="source"/> violates its contract.</exception>
        public DateTimeZoneCache(IDateTimeZoneSource source)
        {
            this.source = Preconditions.CheckNotNull(source, "source");
            this.providerVersionId = source.VersionId;
            if (providerVersionId == null)
            {
                throw new InvalidDateTimeZoneSourceException("Source-returned version ID was null");
            }
            var providerIds = source.GetIds();
            if (providerIds == null)
            {
                throw new InvalidDateTimeZoneSourceException("Source-returned ID sequence was null");
            }
            var idList = new List<string>(providerIds);
            idList.Sort(StringComparer.Ordinal);
            ids = new ReadOnlyCollection<string>(idList);
            // Populate the dictionary with null values meaning "the ID is valid, we haven't fetched the zone yet".
            foreach (string id in ids)
            {
                if (id == null)
                {
                    throw new InvalidDateTimeZoneSourceException("Source-returned ID sequence contained a null reference");
                }
                timeZoneMap[id] = null;
            }
        }

        /// <summary>
        /// The version ID of this provider. This is simply the <see cref="IDateTimeZoneSource.VersionId"/> returned by
        /// the underlying source.
        /// </summary>
        public string VersionId { get { return providerVersionId; } }

        /// <inheritdoc />
        public DateTimeZone GetSystemDefault()
        {
            TimeZoneInfo bcl = TimeZoneInfo.Local;
            string id = source.MapTimeZoneId(bcl);
            if (id == null)
            {
#if PCL
                throw new DateTimeZoneNotFoundException("TimeZoneInfo name " + bcl.StandardName + " is unknown to source " + providerVersionId);
#else
                throw new DateTimeZoneNotFoundException("TimeZoneInfo ID " + bcl.Id + " is unknown to source " + providerVersionId);
#endif
            }
            return this[id];
        }

        /// <inheritdoc />
        public ReadOnlyCollection<string> Ids { get { return ids; } }

        /// <inheritdoc />
        public DateTimeZone GetZoneOrNull(string id)
        {
            Preconditions.CheckNotNull(id, "id");
            DateTimeZone fixedZone = FixedDateTimeZone.GetFixedZoneOrNull(id);
            if (fixedZone != null)
            {
                return fixedZone;
            }
            lock (accessLock)
            {
                DateTimeZone zone;
                if (!timeZoneMap.TryGetValue(id, out zone))
                {
                    return null;
                }
                if (zone == null)
                {
                    zone = source.ForId(id);
                    if (zone == null)
                    {
                        throw new InvalidDateTimeZoneSourceException("Time zone " + id + " is supported by source " + providerVersionId + " but not returned");
                    }
                    timeZoneMap[id] = zone;
                }
                return zone;
            }
        }

        /// <inheritdoc />
        public DateTimeZone this[string id]
        {
            get
            {
                var zone = GetZoneOrNull(id);
                if (zone == null)
                {
                    throw new DateTimeZoneNotFoundException("Time zone " + id + " is unknown to source " + providerVersionId);
                }
                return zone;
            }
        }
    }
}

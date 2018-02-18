// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.IO;
using NodaTime.Utility;
using NUnit.Framework;
using Newtonsoft.Json;
using NodaTime.Serialization.JsonNet;

namespace NodaTime.Serialization.Test.JsonNet
{
    [TestFixture]
    public class NodaIntervalConverterTest
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Converters = { NodaConverters.IntervalConverter, NodaConverters.InstantConverter },
            DateParseHandling = DateParseHandling.None
        };

        [Test]
        public void Serialize()
        {
            var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
            var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
            var interval = new Interval(startInstant, endInstant);

            var json = JsonConvert.SerializeObject(interval, Formatting.None, settings);

            string expectedJson = "{\"Start\":\"2012-01-02T03:04:05Z\",\"End\":\"2013-06-07T08:09:10Z\"}";
            Assert.AreEqual(expectedJson, json);
        }

        [Test]
        public void Deserialize()
        {
            string json = "{\"Start\":\"2012-01-02T03:04:05Z\",\"End\":\"2013-06-07T08:09:10Z\"}";

            var interval = JsonConvert.DeserializeObject<Interval>(json, settings);

            var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
            var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
            var expectedInterval = new Interval(startInstant, endInstant);
            Assert.AreEqual(expectedInterval, interval);
        }

        [Test]
        public void Deserialize_MissingEnd()
        {
            string json = "{\"Start\":\"2012-01-02T03:04:05Z\"}";

            Assert.Throws<InvalidNodaDataException>(() => JsonConvert.DeserializeObject<Interval>(json, settings));
        }

        [Test]
        public void Deserialize_MissingStart()
        {
            string json = "{\"End\":\"2012-01-02T03:04:05Z\"}";

            Assert.Throws<InvalidNodaDataException>(() => JsonConvert.DeserializeObject<Interval>(json, settings));
        }

        [Test]
        public void Serialize_InObject()
        {
            var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
            var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
            var interval = new Interval(startInstant, endInstant);

            var testObject = new TestObject { Interval = interval };

            var json = JsonConvert.SerializeObject(testObject, Formatting.None, settings);

            string expectedJson = "{\"Interval\":{\"Start\":\"2012-01-02T03:04:05Z\",\"End\":\"2013-06-07T08:09:10Z\"}}";
            Assert.AreEqual(expectedJson, json);
        }

        [Test]
        public void Deserialize_InObject()
        {
            string json = "{\"Interval\":{\"Start\":\"2012-01-02T03:04:05Z\",\"End\":\"2013-06-07T08:09:10Z\"}}";

            var testObject = JsonConvert.DeserializeObject<TestObject>(json, settings);

            var interval = testObject.Interval;

            var startInstant = Instant.FromUtc(2012, 1, 2, 3, 4, 5);
            var endInstant = Instant.FromUtc(2013, 6, 7, 8, 9, 10);
            var expectedInterval = new Interval(startInstant, endInstant);
            Assert.AreEqual(expectedInterval, interval);
        }

        public class TestObject
        {
            public Interval Interval { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Globalization;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Helper
{
	static class PatternsObsoleteXMLFileReader
	{
		public static List<TracingBlock> Read(string fileName)
		{
			var blocks = new List<TracingBlock>();

			var doc = new XmlDocument();
			doc.Load(fileName);

			var fetus = doc.SelectSingleNode("lms-patterns-fetus") as XmlElement;
			if (fetus == null)
			{
				return blocks;
			}

			// Get start time and up-sample-rate
			long start;
			if (!long.TryParse(fetus.GetAttribute("start-date"), out start))
			{
				start = (long)(new DateTime(2001, 1, 1) - new System.DateTime(1970, 1, 1)).TotalSeconds;
			}

			int fhrSample;
			if (!int.TryParse(fetus.GetAttribute("fhr-sample-rate"), out fhrSample))
			{
				fhrSample = 4;
			}

			int upSample;
			if (!int.TryParse(fetus.GetAttribute("up-sample-rate"), out upSample))
			{
				upSample = 1;
			}
		
			if ((upSample != 1) && (upSample != 4))
			{
				Debug.Assert(false);
				return blocks;
			}

			if ((fhrSample != 1) && (fhrSample != 4))
			{
				Debug.Assert(false);
				return blocks;
			}

			// Read everything in one block
			var block = new TracingBlock { Start = new System.DateTime(1970, 1, 1).AddSeconds(start) };

			var fhrs = fetus.SelectNodes("fhr-sample");
			var ups = fetus.SelectNodes("up-sample");

			block.Capacity = ups.Count;

			foreach (XmlElement item in fhrs)
			{
				double value;
				if (!double.TryParse(item.GetAttribute("value"), out value))
				{
					Debug.Assert(false);
					value = TracingBlock.NoData;
				}

				block.HRs.Add(Convert.ToByte(Math.Max(0, Math.Min(255, Math.Floor(value))), CultureInfo.InvariantCulture));
			}

			foreach (XmlElement item in ups)
			{
				double value;
				if (!double.TryParse(item.GetAttribute("value"), out value))
				{
					Debug.Assert(false);
					value = TracingBlock.NoData;
				}

				block.UPs.Add(Convert.ToByte(Math.Max(0, Math.Min(255, Math.Floor(value))), CultureInfo.InvariantCulture));
			}

			// If hhr sample is 1 hz, upsample it
			if (fhrSample == 1)
			{
				var hr = new List<byte>(block.HRs.Count * 4);
				foreach (var p in block.HRs)
				{
					hr.Add(p); hr.Add(p); hr.Add(p); hr.Add(p);
				}
				block.HRs = hr;
			}

			// If up sample is 4 hz, downsample it
			if (upSample == 4)
			{
				var downSampledUps = block.UPs.Where((v, i) => (i % 4) == 0).ToList();
				block.UPs = downSampledUps;
			}

			// Align the FHRs and UPs
			block.AlignSignals();

			// Done
			blocks.Add(block);
			return blocks;
		}
	}
}

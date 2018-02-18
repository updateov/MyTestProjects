using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Helper
{
	static class PatternsXMLFileReader
	{
		public static List<TracingBlock> Read(string fileName)
		{
			var blocks = new List<TracingBlock>();

			var doc = XDocument.Load(fileName);
			var tracings = doc.Root.Descendants("tracings").FirstOrDefault();
			if (tracings == null)
			{
				return blocks;
			}

			var absolutestart = DateTime.ParseExact(tracings.Attribute("starttime").Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);

			var list = new List<TracingBlock>();
			foreach (var elt in doc.Root.Descendants("segment"))
			{
				var type = elt.Parent.Attribute("type").Value.ToUpperInvariant();

				if (string.CompareOrdinal(type, "FHR1") == 0) // FHR start is an index in 250ms
				{
					list.Add (new TracingBlock
									{ 
										Start = absolutestart.AddSeconds(Convert.ToInt64(elt.Attribute("start").Value, CultureInfo.InvariantCulture) / 4d),
										HRs = Convert.FromBase64String(elt.Attribute("data").Value).ToList()
									});
				}
				else if (string.CompareOrdinal(type, "UP") == 0) // UP start is an index in 1s
				{
					list.Add (new TracingBlock
									{ 
										Start = absolutestart.AddSeconds(Convert.ToInt64(elt.Attribute("start").Value, CultureInfo.InvariantCulture)),
										UPs = Convert.FromBase64String(elt.Attribute("data").Value).ToList()
									});
				}
				else
				{
					// Ignore other types
					continue;
				}
			}

			// Order them for easy merging
			list = list.OrderBy(b => b.Start).ToList();

			// Now merge the blocks since UP & HRs are splitted in 2 segments
			var result = new List<TracingBlock>();
			while (list.Count > 0)
			{
				// Find all the blocks that can be merged with the first one in the list
				var mergeable = new List<TracingBlock>();
				mergeable.Add(list[0]);
				list.RemoveAt(0);

				while (true)
				{
					var end = mergeable.Max(b => b.End);
					var candidates = list.Where(b => b.Start <= end).ToList();

					if (candidates.Count == 0)
						break;

					foreach (var b in candidates)
					{
						mergeable.Add(b);
						list.Remove(b);
					}
				}

				// Merge the first block with any block that fits
				if (mergeable.Count == 1)
				{
					result.Add(mergeable.First());
				}
				else
				{
					var start = mergeable.Min(b => b.Start);
					var end = mergeable.Max(b => b.End);

					// Align on second (should not be necessary but who knows
					start = new DateTime((start.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);
					if (end.Ticks % TimeSpan.TicksPerSecond != 0)
					{
						end = new DateTime(((end.Ticks / TimeSpan.TicksPerSecond) + 1) * TimeSpan.TicksPerSecond);
					}

					var hr = new byte[(int)((end - start).TotalSeconds * 4)];
					var up = new byte[(int)((end - start).TotalSeconds)];

					foreach (var block in mergeable)
					{
						if (block.HRs.Count > 0)
						{
							Array.Copy(block.HRs.ToArray(), 0, hr, (int)((block.Start - start).TotalSeconds * 4), block.HRs.Count);
						}
						if (block.UPs.Count > 0)
						{
							Array.Copy(block.UPs.ToArray(), 0, up, (int)((block.Start - start).TotalSeconds), block.UPs.Count);
						}
					}
					result.Add(new TracingBlock { Start = start, HRs = hr.ToList(), UPs = up.ToList() });
				}
			}

			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Helper
{
	static class InFileReader
	{
		public static List<TracingBlock> Read(string fileName)
		{
			List<TracingBlock> blocks = new List<TracingBlock>();

			TracingBlock currentBlock = null;
			DateTime timestamp = new DateTime(2001, 1, 1);

			foreach (var line in File.ReadAllLines(fileName))
			{
				var items = line.Trim().Split(" ".ToCharArray(), StringSplitOptions.None);

				// Skip empty lines
				if (items.GetLength(0) == 0)
				{
					continue;
				}

				// Timestamp
				if (items.GetLength(0) == 2)
				{
					DateTime date;
					if (DateTime.TryParseExact(line.Trim(), new String[] { "dd/MM/yy HH:mm:ss", "dd/MM/yy HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
					{
						// Check for 'overlap'
						Debug.Assert((currentBlock == null) || (currentBlock.End < date));
						timestamp = date;
					}

					continue;
				}

				// Data
				if (items.GetLength(0) == 15)
				{
					// Create a block if necessary
					if (currentBlock == null)
					{
						currentBlock = new TracingBlock { Start = timestamp };
						blocks.Add(currentBlock);
					}

					if (currentBlock == null)
					{
						Debug.Assert(false);
						continue;
					}

					for (int i = 0; i < 15; ++i)
					{
						byte value;
						if (!byte.TryParse(items[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
						{
							Debug.Assert(false);
							value = 255;
						}
						if (i < 12)
						{
							currentBlock.HRs.Add(value);
						}
						else
						{
							currentBlock.UPs.Add(value);
						}
					}

					continue;
				}
			}

			foreach (var b in blocks)
			{
				Debug.Assert(b.UPs.Count * 4 == b.HRs.Count);
			}

			return blocks;
		}

	}
}

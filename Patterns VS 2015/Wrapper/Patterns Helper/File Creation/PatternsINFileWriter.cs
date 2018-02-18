using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.Engine.Data;
using System.Globalization;

namespace PeriGen.Patterns.Helper
{
	public static class PatternsINFileWriter
	{
		/// <summary>
		/// Create a encoded in file for the given tracings
		/// </summary>
		/// <param name="blocks"></param>
		/// <returns></returns>
		public static string Write(IEnumerable<TracingBlock> blocks)
		{
			if ((blocks == null) || (blocks.Count() == 0))
				return string.Empty;

			// Merge block with a 10 minutes max gap
			blocks = PeriGen.Patterns.Engine.Data.TracingBlock.Merge(blocks, 600);
			if (blocks.Count() == 0)
				return string.Empty;

			// Preallocate the string builder to improve performances
			var data = new StringBuilder(21 * blocks.Sum(b => b.TotalSeconds + 1)); // 21 = 3 bytes per points + 1 space between each points, 4 FHRs per seconds and 1 UP per second, + 1 for the EndOfLine
																					// +1 since that adds 21 per block and that account for the 'date' starting each block

			bool firstBlock = true;

			// Scan blocks...
			foreach (var block in blocks)
			{
				block.Trim();

				int position = 0;
				int count = block.TotalSeconds;

				// Line feed for all block but first one
				if (firstBlock)
				{
					firstBlock = false;
				}
				else
				{
					data.AppendLine();
				}

				// Each block starts with a date
				data.AppendLine(block.Start.ToString("dd/MM/yy HH:mm:ss", CultureInfo.InvariantCulture));
				while (position < count)
				{
					if (position != 0)
					{
						data.AppendLine();
					}

					// Each line is 3 seconds of FHR in 4Hz (so that's 12 readings)...
					for (int i = 0; i < 12; ++i)
					{
						data.Append(((position * 4) + i < block.HRs.Count) ? string.Format(CultureInfo.InvariantCulture, "{0:000} ", block.HRs[(position * 4) + i]) : "255 ");
					}

					//... and 3 seconds of UP in 1Hz
					for (int i = 0; i < 3; ++i)
					{
						data.Append((position + i < block.UPs.Count) ? string.Format(CultureInfo.InvariantCulture, "{0:000} ", block.UPs[position + i]) : "255 ");
					}

					position += 3;
				}
			}

			// Done!
			return data.ToString();
		}
	}
}

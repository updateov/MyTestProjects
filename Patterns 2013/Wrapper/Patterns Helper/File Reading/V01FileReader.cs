using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Helper
{
	/// <summary>
	/// Read all tracing data from a V01 file
	/// </summary>
	static class V01FileReader
	{
		/// <summary>
		/// Calculate the base offset for the GE V01 date (a timestamp in the V01 file is the number of seconds elapsed 
		/// since that reference date)
		/// </summary>
		static long V01_offset = (long)(new DateTime(1975, 12, 31, 0, 0, 0).Ticks / TimeSpan.TicksPerSecond);

		/// <summary>
		/// Read all tracings in the given V01 file
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		public static List<TracingBlock> Read(string fileName)
		{
			var blocks = new List<TracingBlock>();

			using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
			using (var br = new BinaryReader(fs))
			{
				// Manually keeping the file length and current position may not be the easiest way to check for EOF but it's WAY faster than any other solution
				long available = fs.Length;

				try
				{
					// Read until the end of the file in one pass but byte per byte to reduce memory overhead
					byte value = 0;
					while (available > 5)
					{
						// Find a header. The code below is not the easiest to read but it is VERY efficient
						if (value != 0x46)
						{
							value = br.ReadByte();
							--available;
							continue;
						}

						value = br.ReadByte();
						--available;
						if (value != 0x57)
						{
							continue;
						}

						value = br.ReadByte();
						--available;
						if (value != 0x61)
						{
							continue;
						}

						value = br.ReadByte();
						--available;
						if (value != 0x76)
						{
							continue;
						}

						value = br.ReadByte();
						--available;
						if (value != 0x65)
						{
							continue;
						}

						// Skip useless data
						br.ReadBytes(4);

						// Retrieve the size of the block
						int block_size = br.ReadUInt16();

						// Skip useless data
						br.ReadBytes(7);

						// Retrieve the time associated to the block, unfortunatly, the time is in Local time !!
						var offset = br.ReadUInt32();
						DateTime block_stamp = new DateTime(TimeSpan.TicksPerSecond * (V01_offset + offset)).ToUniversalTime();

						// Skip useless data
						br.ReadBytes(2);

						TracingBlock current = null;

						// We just read 5 header bytes and skip the block size
						available -= 5 + block_size;

						var H00 = new List<byte>();
						var H01 = new List<byte>();
						var H02 = new List<byte>();

						// Calculate the useful payload of the block
						block_size -= 14; // 14 is the overhead, constant for all block
						while (block_size > 3) // Need at least 3 bytes for a segment header
						{
							Debug.Assert(br.BaseStream.Position < br.BaseStream.Length);

							// Read the channel
							byte segment_channel = br.ReadByte();

							// Read the channel segment size
							int segment_size = br.ReadUInt16();
							Debug.Assert(segment_size > 0);

							if (current == null)
							{
								current = new TracingBlock();
								current.Start = block_stamp;
								current.Capacity = 60;
							}

							// Deal with Channel 00 (FHR)
							if (segment_channel == 0x00)
							{
								H00.AddRange(br.ReadBytes(segment_size));
							}

							// Deal with Channel 01 (FHR)
							else if (segment_channel == 0x01)
							{
								H01.AddRange(br.ReadBytes(segment_size));
							}

							// Deal with Channel 02 (MHR)
							else if (segment_channel == 0x02)
							{
								H02.AddRange(br.ReadBytes(segment_size));
							}

							// Deal with Channel 03 (UP)
							else if (segment_channel == 0x03)
							{
								// Read the UP and immediately downgrade it to 1Hz (it's in 4Hz in the V01 file)
								byte[] ups = br.ReadBytes(segment_size);
								int len = ups.GetLength(0);
								for (int i = 0; i < len; i += 4)
								{
									current.UPs.Add(ups[i]);
								}
							}

							// Ignore others segment
							else
							{
								br.ReadBytes(segment_size);
							}

							// Jump the segment
							block_size -= (3 + segment_size);

							Debug.Assert(block_size >= 0);
						}

						if (H01.Count > 0)
						{
							current.HRs.AddRange(H01);
						}
						else if (H00.Count > 0)
						{
							current.HRs.AddRange(H00);
						}

						Debug.Assert(block_size == 0);
						Debug.Assert(available == (fs.Length - br.BaseStream.Position));

						// Align the FHRs and UPs
						int count = Math.Max(current.HRs.Count, 4 * current.UPs.Count);
						while (current.UPs.Count * 4 < count) { current.UPs.Add(TracingBlock.NoData); }
						while (current.HRs.Count < count) { current.HRs.Add(TracingBlock.NoData); }

						current.Trim();

						// Skip blocks with no FHR and UP
						if (current.TotalSeconds > 0)
						{
							// Add the block to the list
							blocks.Add(current);
						}
					}
				}
				catch (EndOfStreamException)
				{
					// Just ignore that one
					Debug.Assert(false);
				}
			}

			return blocks;
		}
	}

}

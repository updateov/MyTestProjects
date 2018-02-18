using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.Data
{
	public class TracingBlock
	{
		public const byte NoData = 0;

		public TracingBlock()
		{
			this.HRs = new List<byte>();
			this.UPs = new List<byte>();
		}

		public DateTime Start { get; set; }
		public List<byte> HRs { get; set; }
		public List<byte> UPs { get; set; }

		public int Capacity
		{
			get
			{
				return this.UPs.Capacity;
			}
			set
			{
				this.HRs.Capacity = Math.Max(this.HRs.Count, 4 * value);
				this.UPs.Capacity = Math.Max(this.UPs.Count, value);
			}
		}
		public int TotalSeconds
		{
			get
			{
				return Math.Max(this.HRs.Count / 4, this.UPs.Count);
			}
		}

		public DateTime End
		{
			get
			{
				return this.Start.AddSeconds(this.TotalSeconds);
			}
		}

		public void Append(TracingBlock block)
		{
			if (block == null)
				throw new ArgumentNullException("block");

			// Check if block are adjascent
			int gap = (int)(block.Start - this.Start).TotalSeconds - this.TotalSeconds;

			// Fix gap by bridging with NO data
			while (gap > 0)
			{
				this.HRs.Add(TracingBlock.NoData); this.HRs.Add(TracingBlock.NoData); this.HRs.Add(TracingBlock.NoData); this.HRs.Add(TracingBlock.NoData);
				this.UPs.Add(TracingBlock.NoData);
				--gap;
			}

			// Check for overlap
			int overlap = Math.Abs(gap);

			// Too much overlap, just leave NOW
			if ((overlap > 0) && (overlap >= block.TotalSeconds))
			{
				return;
			}

			// Append
			if (overlap > 0)
			{
				this.HRs.AddRange(block.HRs.SkipWhile((x, i) => i < (overlap * 4)));
				this.UPs.AddRange(block.UPs.SkipWhile((x, i) => i < overlap));
			}
			else
			{
				this.HRs.AddRange(block.HRs);
				this.UPs.AddRange(block.UPs);
			}
		}

		public static bool IsNoDataUP(byte data)
		{
			return data < 5 || data == 255;
		}

		public static bool IsNoDataHR(byte data)
		{
			return data == 0 || data == 255;
		}

		/// <summary>
		/// Trim the beginning of the data (no-data)
		/// </summary>
		public void TrimStart()
		{
			while ((this.HRs.Count >= 4) 
				&& IsNoDataUP(this.UPs[0])
				&& IsNoDataHR(this.HRs[0])
				&& IsNoDataHR(this.HRs[1])
				&& IsNoDataHR(this.HRs[2])
				&& IsNoDataHR(this.HRs[3]))
			{
				this.HRs.RemoveRange(0, 4);
				this.UPs.RemoveAt(0);
				this.Start = this.Start.AddSeconds(1);
			}
		}

		/// <summary>
		/// Trim the end of the data (no-data)
		/// </summary>
		public void TrimEnd()
		{
			while ((this.HRs.Count >= 4)
				&& IsNoDataUP(this.UPs[this.UPs.Count - 1])
				&& IsNoDataHR(this.HRs[this.HRs.Count - 1])
				&& IsNoDataHR(this.HRs[this.HRs.Count - 2])
				&& IsNoDataHR(this.HRs[this.HRs.Count - 3])
				&& IsNoDataHR(this.HRs[this.HRs.Count - 4]))
			{
				this.HRs.RemoveRange(this.HRs.Count - 4, 4);
				this.UPs.RemoveAt(this.UPs.Count - 1);
			}
		}

		/// <summary>
		/// Trim the no-data start & end of the data
		/// </summary>
		public void Trim()
		{
			this.TrimStart();
			this.TrimEnd();
		}

		/// <summary>
		/// Merge blocks
		/// </summary>
		/// <param name="blocks">List of block to merged</param>
		/// <param name="maxBridgeableGap">Maximum duration (in seconds) that can be bridged between 2 blocks</param>
		/// <returns></returns>
		public static List<TracingBlock> Merge(IEnumerable<TracingBlock> blocks, int maxBridgeableGap)
		{
			if (blocks == null)
				throw new ArgumentNullException("blocks");

			List<TracingBlock> result = new List<TracingBlock>();

			TracingBlock last = null;
			foreach (TracingBlock block in blocks)
			{
				// Skip empty blocks
				if (block.TotalSeconds == 0)
				{
					continue;
				}

				// First block?
				if (last == null)
				{
					last = block;
					result.Add(block);
					continue;
				}

				// Check if block are adjascent
				int gap = (int)(block.Start - last.Start).TotalSeconds - last.TotalSeconds;

				// A gap that's not bridgeable
				if (gap > maxBridgeableGap)
				{
					last = block;
					result.Add(block);
					continue;
				}

				// Fix gap by bridging with NO data
				while (gap > 0)
				{
					last.HRs.Add(TracingBlock.NoData); last.HRs.Add(TracingBlock.NoData); last.HRs.Add(TracingBlock.NoData); last.HRs.Add(TracingBlock.NoData);
					last.UPs.Add(TracingBlock.NoData);
					--gap;
				}

				// Check for overlap
				int overlap = Math.Abs(gap);

				// Too much overlap, just leave NOW
				if (overlap >= block.TotalSeconds)
				{
					continue;
				}

				// Remove overlap prior to append
				if (overlap > 0)
				{
					block.HRs.RemoveRange(0, overlap * 4);
					block.UPs.RemoveRange(0, overlap);
				}

				// Append
				last.HRs.AddRange(block.HRs);
				last.UPs.AddRange(block.UPs);
			}

			// Optimize memory
			foreach (TracingBlock block in result)
			{
				block.HRs.TrimExcess();
				block.UPs.TrimExcess();
			}

			return result;
		}
	}
}

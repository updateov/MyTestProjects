using System;
using System.Collections.Generic;
using System.Text;

namespace PatternsEngine
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
		public List<byte> HRs { get; private set; }
		public List<byte> UPs { get; private set; }

		public int Capacity
		{
			get
			{
				return this.UPs.Capacity;
			}

			set
			{
				this.HRs.Capacity = 4 * value;
				this.UPs.Capacity = value;
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
				this.HRs.AddRange(block.HRs.GetRange(overlap * 4, block.HRs.Count - (overlap * 4)));
				this.UPs.AddRange(block.UPs.GetRange(overlap, block.UPs.Count - overlap));
			}
			else
			{
				this.HRs.AddRange(block.HRs);
				this.UPs.AddRange(block.UPs);
			}
		}

		/// <summary>
		/// Merge blocks
		/// </summary>
		/// <param name="blocks">List of block to merged</param>
		/// <param name="maxBridgeableGap">Maximum duration (in seconds) that can be bridged between 2 blocks</param>
		/// <returns></returns>
		public static List<TracingBlock> Merge(IEnumerable<TracingBlock> blocks, int maxBridgeableGap)
		{
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

					block.Capacity = 5000;
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
				if ((overlap > 0) && (overlap >= block.TotalSeconds))
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

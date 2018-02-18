using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace PeriGen.Patterns.Engine.Data
{
	/// <summary>
	/// A very simple class to hold UP (uterine pressure) and HR (heart rate) signal used for the pattern's engine
	/// </summary>
	public class TracingBlock
	{
		/// <summary>
		/// No data constant for HR and UP (pen-up)
		/// </summary>
		public const byte NoData = 255;

        /// <summary>
        /// The amount of hours to load in memory from the very end of the tracings file
        /// </summary>
        public const int LastHours = 24;

		/// <summary>
		/// Unique ID
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public TracingBlock()
		{
			this.HRs = new List<byte>();
			this.UPs = new List<byte>();
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="block"></param>
		public TracingBlock(TracingBlock block)
		{
			this.Id = block.Id;
			this.Start = block.Start;
			this.HRs = new List<byte>(block.HRs);
			this.UPs = new List<byte>(block.UPs);
		}

		DateTime _Start;

		/// <summary>
		/// Start time of the block
		/// </summary>
		public DateTime Start 
		{ 
			get
			{
				return _Start;
			}
			set
			{
				// Do this to be sure there is no milliseconds not ticks, align to the second!
				this._Start = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
			}
		}

		/// <summary>
		/// The HR signal
		/// </summary>
		public List<byte> HRs { get; set; }

		/// <summary>
		/// The UP signal
		/// </summary>
		public List<byte> UPs { get; set; }

		/// <summary>
		/// To preallocate some capacity to the HR and UP lists (in second of tracings)
		/// </summary>
		public int Capacity
		{
			get
			{
				return this.UPs.Capacity;
			}
			set
			{
				this.HRs.Capacity = Math.Max(this.HRs.Capacity, checked((int)(4 * value)));
				this.UPs.Capacity = Math.Max(this.UPs.Capacity, value);
			}
		}

		/// <summary>
		/// Total duration of the tracing in seconds
		/// </summary>
		public int TotalSeconds
		{
			get
			{
				return Math.Max((int)Math.Ceiling(this.HRs.Count / 4d), this.UPs.Count);
			}
		}

		/// <summary>
		/// The end time of that block
		/// </summary>
		public DateTime End
		{
			get
			{
				return this.Start.AddSeconds(this.TotalSeconds);
			}
		}

		/// <summary>
		/// Make sure UP and HR signals are the same duration
		/// </summary>
		public void AlignSignals()
		{
			// Preallocate
			this.Capacity = this.TotalSeconds;

			// Make sure this block UP and FHR are aligned
			while (this.UPs.Count * 4 < this.HRs.Count)
			{
				this.UPs.Add(TracingBlock.NoData);
			}
			while (this.UPs.Count * 4 > this.HRs.Count)
			{
				this.HRs.Add(TracingBlock.NoData);
			}
		}

		/// <summary>
		/// Append the given block at the end of the current block. Pad with no-data if necessary
		/// </summary>
		/// <param name="block"></param>
		public void Append(TracingBlock block)
		{
			if (block == null)
				throw new ArgumentNullException("block");

			if (block.TotalSeconds == 0)
				return;

			if (block.End <= this.End)
				return;

			// Align the blocks HR and UP signals
			this.AlignSignals();
			block.AlignSignals();

			// Preallocate to speed up the append
			this.Capacity = (int)Math.Ceiling((block.End - this.Start).TotalSeconds);

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

		/// <summary>
		/// Check if the given data block is no data (pen-up)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsNoDataUP(byte data)
		{
			return data >= 127;
		}

		/// <summary>
		/// Check if the given data block is no data (pen-up)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool IsNoDataHR(byte data)
		{
			return data == 0 || data == 255;
		}

		/// <summary>
		/// Trim the beginning of the data (no-data)
		/// </summary>
		public void TrimStart()
		{
			while ((this.HRs.Count > 4)  // We do not want to trim up to the point where the block is empty, that's why the test is > 4 and not >= 4
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
			while ((this.HRs.Count > 4) // We do not want to trim up to the point where the block is empty, that's why the test is > 4 and not >= 4
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
		/// Check if the block is completly no data
		/// </summary>
		public bool IsCompleteNotData
		{
			get
			{
				return this.TotalSeconds == 0 || (this.UPs.All(b => IsNoDataUP(b)) && this.HRs.All(b => IsNoDataHR(b)));
			}
		}

		/// <summary>
		/// Trim the no-data start and end of the data
		/// </summary>
		public void Trim()
		{
			this.TrimStart();
			this.TrimEnd();
		}

		/// <summary>
		/// Merge blocks
		/// This does not modify the source blocks but create a copy of the blocks
		/// </summary>
		/// <param name="blocks">List of block to merged</param>
		/// <param name="maxBridgeableGap">Maximum duration (in seconds) that can be bridged between 2 blocks</param>
		/// <param name="maxMergedBlockSize">Maximum size of a merged block (if a block is already that big or even bigger, it just won't be merged with anything)</param>
		/// <returns></returns>
		public static List<TracingBlock> Merge(IEnumerable<TracingBlock> blocks, int maxBridgeableGap, int maxMergedBlockSize)
		{
			List<TracingBlock> result = new List<TracingBlock>();

			// Safety first
			if (blocks == null)
				return result;

			TracingBlock last = null;
			foreach (TracingBlock block in blocks)
			{
				// Skip empty blocks
				if (block.TotalSeconds == 0)
				{
					continue;
				}

				block.AlignSignals();

				// First block?
				if (last == null)
				{
					last = new TracingBlock(block);
					result.Add(last);
					continue;
				}

				// Check if block are adjascent
				int gap = (int)(block.Start - last.End).TotalSeconds;

				// A gap that's not bridgeable or if the resulting merged block would be too big...
				if ((gap > maxBridgeableGap) || ((block.End - last.Start).TotalSeconds > maxMergedBlockSize))
				{
					last = new TracingBlock(block);
					result.Add(last);
					continue;
				}

				last.Append(block);
			}

			// Optimize memory
			foreach (TracingBlock block in result)
			{
				block.HRs.TrimExcess();
				block.UPs.TrimExcess();
			}

			// Done
			return result;
		}

		/// <summary>
		/// Merge blocks
		/// This does not modify the source blocks but create a copy of the blocks
		/// </summary>
		/// <param name="blocks">List of block to merged</param>
		/// <param name="maxBridgeableGap">Maximum duration (in seconds) that can be bridged between 2 blocks</param>
		/// <returns></returns>
		public static List<TracingBlock> Merge(IEnumerable<TracingBlock> blocks, int maxBridgeableGap)
		{
			return Merge(blocks, maxBridgeableGap, int.MaxValue);
		}

		/// <summary>
		/// For nice traces
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} Start: {1:s} End: {2:s}", base.ToString(), this.Start, this.End);
		}

		/// <summary>
		/// Split a given block in multiple blocks so that each block duration is less than maxBlockSize
		/// </summary>
		/// <param name="maxBlockSize"></param>
		/// <returns></returns>
		public List<TracingBlock> Split(int maxBlockSize)
		{
			// To make sure HRs and UP are properly aligned...
			this.AlignSignals();

			// Security
			System.Diagnostics.Debug.Assert(maxBlockSize > 0);
			maxBlockSize = Math.Max(1, maxBlockSize);

			// All fits in one block?
			var results = new List<TracingBlock>();
			if (this.TotalSeconds <= maxBlockSize)
			{
				results.Add(this);
				return results;
			}

			// Need to cut!
			var position = 0;
			var duration = this.TotalSeconds;
			var time = this.Start;

			while (position < duration)
			{
				var block_duration = Math.Min(duration - position, maxBlockSize);
				var block = new TracingBlock { Start = time, Capacity = block_duration };

				for (int i = position; i < position + block_duration; ++i)
				{
					block.HRs.Add(this.HRs[(i * 4)]);
					block.HRs.Add(this.HRs[(i * 4) + 1]);
					block.HRs.Add(this.HRs[(i * 4) + 2]);
					block.HRs.Add(this.HRs[(i * 4) + 3]);

					block.UPs.Add(this.UPs[i]);
				}

				// If the block is pure no-data, just skip it...
				if (!block.IsCompleteNotData)
				{
					results.Add(block);
				}

				position += block_duration;
				time = time.AddSeconds(block_duration);
			}

			return results;
		}

		/// <summary>
		/// Enumerate the UP data one by one within the given time
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public IEnumerator<byte> GetUPEnumerator(DateTime start, DateTime end)
		{
			// Align on seconds...
			start = new DateTime((start.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);
			end = new DateTime((end.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);

			var start_index = (int)(start - this.Start).TotalSeconds;
			var end_index = (int)(end - this.Start).TotalSeconds;

			for (var current_index = start_index; current_index < end_index; ++current_index)
			{
				// Outside boundaries of the block?
				if ((current_index < 0) || (current_index >= this.UPs.Count))
				{
					yield return 255;
				}
				else
				{
					yield return this.UPs.ElementAt(current_index);
				}
			}
		}

		/// <summary>
		/// Enumerate the HR data one by one within the given time
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public IEnumerator<byte> GetHREnumerator(DateTime start, DateTime end)
		{
			// Align on seconds...
			start = new DateTime((start.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);
			end = new DateTime((end.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);

			var start_index = (int)((start - this.Start).TotalSeconds * 4);
			var end_index = (int)((end - this.Start).TotalSeconds * 4);

			for (var current_index = start_index; current_index < end_index; ++current_index)
			{
				// Outside boundaries of the block?
				if ((current_index < 0) || (current_index >= this.HRs.Count))
				{
					yield return 255;
				}
				else
				{
					yield return this.HRs.ElementAt(current_index);
				}
			}
		}
	}
}

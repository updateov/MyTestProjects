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
	/// Generic file reader that depending on the file type will call the proper specific reader
	/// </summary>
	public static class TracingFileReader
	{
		public static List<TracingBlock> Read(string fileName)
		{
			List<TracingBlock> blocks = null;

			var ext = new FileInfo(fileName).Extension.ToUpperInvariant();
			if (string.CompareOrdinal(ext, ".IN") == 0)
			{
				blocks = InFileReader.Read(fileName);
			}
			else if (string.CompareOrdinal(ext, ".V01") == 0)
			{
				blocks = V01FileReader.Read(fileName);
			}
			else if (string.CompareOrdinal(ext, ".XML") == 0)
			{
				bool isObsoleteFormat = true;

				using (var s = File.OpenText(fileName))
				{
					char[] buffer = new char[1024];
					s.ReadBlock(buffer, 0, 1024);
					isObsoleteFormat = !new string(buffer).ToUpperInvariant().Contains("PATTERNSARCHIVE");
				}

				if (isObsoleteFormat)
				{
					blocks = PatternsObsoleteXMLFileReader.Read(fileName);
				}
				else
				{
					blocks = PatternsXMLFileReader.Read(fileName);
				}
			}
			else
			{
				// Unknown format
				Debug.Assert(false);
				return null;
			}

			if (blocks != null)
			{
				// Align the blocks because in the file they may be optimized (end trimed)
				foreach (TracingBlock block in blocks)
				{
					while (block.HRs.Count - (4 * block.UPs.Count) > 0)
					{
						block.UPs.Add(255);
					}
					while ((4 * block.UPs.Count) - block.HRs.Count > 0)
					{
						block.HRs.Add(255);
					}
				}
			}

			return blocks;
		}

        public static List<TracingBlock> ReadInFile(String fileName)
        {
            return InFileReader.Read(fileName);
        }

        public static List<TracingBlock> ReadV01File(String fileName)
        {
            return V01FileReader.Read(fileName);
        }
	}
}

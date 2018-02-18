using System.Linq;

namespace PeriGen.Patterns.Chalkboard
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var process = new System.Diagnostics.Process())
			{
				process.StartInfo.UseShellExecute = true;

				string url = string.Empty;
				if (args.Count() > 0)
				{
					url = args[0];
				}

				process.StartInfo.FileName = url;
				process.Start();
			}
		}
	}
}

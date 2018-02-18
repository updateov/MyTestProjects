using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.GE.Service
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Random generator
		/// </summary>
		static Random Generator = new Random((int)DateTime.UtcNow.Ticks);

		/// <summary>
		/// Randomize a list
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IEnumerable<TSource> RandomShuffle<TSource>(this IEnumerable<TSource> source)
		{
			return source.Select(t => new { Index = Generator.Next(), Value = t }).OrderBy(p => p.Index).Select(p => p.Value);
		}
	}
}

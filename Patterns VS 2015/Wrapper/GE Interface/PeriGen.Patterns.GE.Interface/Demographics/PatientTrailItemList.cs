using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.GE.Interface.Demographics
{
	public class PatientTrailItemList : List<PatientTrailItem>
	{
		/// <summary>
		/// Comparison of two lists
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var list = obj as PatientTrailItemList;
			if (list == null)
				return false;

			if (list.Count != this.Count)
				return false;

			for (int i = 0; i < this.Count; ++i)
			{
				if (!this[i].Equals(list[i]))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Hash code for the list
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.Aggregate(0, (x, y) => x.GetHashCode() ^ y.GetHashCode());
		}
	}
}

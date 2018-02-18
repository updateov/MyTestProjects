using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml.Serialization;

namespace PeriGen.Patterns.GE.Interface.Demographics
{
	public class PatientTrailItem
	{
		/// <summary>
		/// The date&time associated to the trail entry
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// The bed location
		/// </summary>
		public string BedName { get; set; }

		/// <summary>
		/// The unit location
		/// </summary>
		public string UnitName { get; set; }

		/// <summary>
		/// Comparison of two items
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var item = obj as PatientTrailItem;
			if (item == null)
				return false;
			
			if (this.Time != item.Time)
				return false;

			if (string.CompareOrdinal(this.BedName, item.BedName) != 0)
				return false;

			if (string.CompareOrdinal(this.UnitName, item.UnitName) != 0)
				return false;

			return true;
		}

		/// <summary>
		/// Hash code for the item
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.Time.GetHashCode();
		}
	}
}

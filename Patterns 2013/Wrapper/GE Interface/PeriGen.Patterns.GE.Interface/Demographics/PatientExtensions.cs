using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PeriGen.Patterns.GE.Interface.Demographics
{
	public static class PatientExtensions
	{
		public static bool CompareFullTrail(this Patient newPatient, Patient oldPatient)
		{
			return oldPatient.TrailList.Equals(newPatient.TrailList);
		}

		public static bool ComparePartialTrail(this Patient newPatient, Patient oldPatient)
		{
			// Get rid of the impossible cases
			if ((newPatient.TrailList.Count == 0)
				|| (oldPatient.TrailList.Count == 0)
				|| (oldPatient.TrailList.Count > newPatient.TrailList.Count))
				return false;

			// First, find where is the most recent column of oldPatient within the newPatient columns
			// It is necessary since OBLink returns a max of 8 columns so the column 8 or the old patient
			// could now be colum 5 of the new one if there was 3 transfers performed on the new one
			var baseTrail = oldPatient.TrailList.Last().Time.Ticks;
			for (int baseIndex = 0; baseIndex < oldPatient.TrailList.Count; ++baseIndex)
			{
				if (newPatient.TrailList[baseIndex].Time.Ticks == baseTrail)
				{
					for (int i = oldPatient.TrailList.Count - 1; (i >= 0) && (baseIndex >= 0); --i, --baseIndex)
					{
						if (!oldPatient.TrailList[i].Equals(newPatient.TrailList[baseIndex]))
							return false;
					}

					// It's a match
					return true;
				}
			}

			// Not found!
			return false;
		}

		public static void SetPropertyValue(this object o, string propertyName, object newValue)
		{
			PropertyInfo pi;
			pi = o.GetType().GetProperty(propertyName);
			if (pi != null && pi.CanWrite) pi.SetValue(o, newValue, null);
		}
	}
}

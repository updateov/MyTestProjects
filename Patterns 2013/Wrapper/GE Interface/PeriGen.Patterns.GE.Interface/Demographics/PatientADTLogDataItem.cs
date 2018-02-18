using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Globalization;
namespace PeriGen.Patterns.GE.Interface.Demographics
{
    public class PatientADTLogItem
    {
		/// <summary>
		/// The date&time associated to the trail entry
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// The ADT log actual text
		/// </summary>
		public string ADTLog { get; set; }
    }
}

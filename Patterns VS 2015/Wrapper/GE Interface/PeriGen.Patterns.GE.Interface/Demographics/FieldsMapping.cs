using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace PeriGen.Patterns.GE.Interface.Demographics
{
	/// <summary>
	/// Store chart fields and mapping with local properties
	/// </summary>
	internal static class FieldsMapping
	{
		/// <summary>
		/// The dictionary of mappings
		/// </summary>
		static Dictionary<string, string> MappingDictionary = null;

		/// <summary>
		/// Retrieve the mapping value associated to the given key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetMappingValue(String key)
		{
			try
			{
				string value;

				if (!MappingDictionary.TryGetValue(key, out value))
					return string.Empty;

				return value;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "Error loading MappingDictionary items. Details: {0}", ex));
				return string.Empty;
			}
		}

		/// <summary>
		/// Initialisation of the dictionary
		/// </summary>
		static FieldsMapping()
		{
			MappingDictionary = new Dictionary<string, string>();
			MappingDictionary.Add("MRN", "MRNCRC");
			MappingDictionary.Add("NAME", "Name");
			MappingDictionary.Add("EDC", "EDDAsString");
			MappingDictionary.Add("BABY", "FetusesAsString");
			MappingDictionary.Add("BED", "BedName");
			MappingDictionary.Add("UNIT", "UnitName");
			MappingDictionary.Add("ADTLOG", "ADTLog");
		}
	}
}

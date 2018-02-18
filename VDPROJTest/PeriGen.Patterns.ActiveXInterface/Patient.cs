using System.Globalization;
using System.Xml.Linq;

namespace PeriGen.Patterns.ActiveXInterface
{
	/// <summary>
	/// The different type of patient status
	/// </summary>
	public enum StatusType : byte
	{
		Invalid = 0,
		Live = 1,
		Unplugged = 2,
		Recovery = 3,
		Error = 4,
		Late = 5
	}

	public class XPatient
	{
		/// <summary>
		/// Unique identifier
		/// </summary>
		public int PatientId { get; set; }

		/// <summary>
		/// MRN
		/// </summary>
		public string MRN { get; set; }

		/// <summary>
		/// Status
		/// </summary>
		public StatusType Status { get; set; }

		/// <summary>
		/// Status
		/// </summary>
		public string StatusDetails { get; set; }

		/// <summary>
		/// First name
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// Last name
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// EDD if known
		/// </summary>
		public System.Nullable<long> EDD { get; set; }

		/// <summary>
		/// Reset date if any
		/// </summary>
		public System.Nullable<long> Reset { get; set; }

		/// <summary>
		/// Number of fetuses if known
		/// </summary>
		public System.Nullable<byte> FetusCount { get; set; }

		/// <summary>
		/// Encode a Patient into an XElement that can be sent to the patterns activex
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public XElement EncodeForActiveX()
		{
			return new XElement("patient",
					new XAttribute("id", this.PatientId),
					new XAttribute("mrn", this.MRN ?? string.Empty),
					new XAttribute("status", ((int)this.Status).ToString(CultureInfo.InvariantCulture)),
					new XAttribute("statusdetails", this.StatusDetails ?? string.Empty),
					new XAttribute("firstname", this.FirstName ?? string.Empty),
					new XAttribute("lastname", this.LastName ?? string.Empty),
					new XAttribute("edd", this.EDD.HasValue ? this.EDD.Value : 0),
					new XAttribute("reset", this.Reset.HasValue ? this.Reset.Value : 0),
					new XAttribute("fetus", this.FetusCount.HasValue ? this.FetusCount.Value : 0));
		}
	}
}

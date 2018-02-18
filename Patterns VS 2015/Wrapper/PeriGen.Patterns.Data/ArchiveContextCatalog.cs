// This file is intended to be edited manually

using System.Globalization;
using System.Xml.Linq;
namespace ArchiveContextCatalog
{
	partial class ArchiveContextCatalog
	{
		/// <summary>
		/// If the DB does not exist, create it now
		/// </summary>
		/// <returns>True is the DB didn't exist yet and was created</returns>
		public bool CreateIfNecessary()
		{
			if (!this.DatabaseExists())
			{
				this.CreateDatabase(false, true);
				this.ExecuteCommand("CREATE UNIQUE INDEX IF NOT EXISTS IDX_PatientId ON Patients (PatientId)");
				this.ExecuteCommand("CREATE INDEX IF NOT EXISTS IDX_EpisodeStatus ON Patients (EpisodeStatus)");

				this.SubmitChanges();
				return true;
			}

			return false;
		}
	}

	partial class Patient
	{
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "pid='{0}' status='{1}' database='{2}'", this.PatientId, this.EpisodeStatus, this.DatabaseFile);
		}
	}

}
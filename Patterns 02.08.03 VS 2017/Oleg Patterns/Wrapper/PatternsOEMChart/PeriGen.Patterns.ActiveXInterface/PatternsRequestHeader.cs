using System;
using System.Globalization;
using System.Xml.Linq;

namespace PeriGen.Patterns.ActiveXInterface
{
	/// <summary>
	/// Allow to perform a data request on one patient, contains id of the patient (either Id or Key) and
	/// the last Id already known of PastEpisodes, Tracings, Artifacts, UserActions
	/// </summary>
	public class PatternsRequestHeader
	{
		/// <summary>
		/// That current instance of server unique ID
		/// </summary>
		static string ServerInstanceUniqueID = ((int)DateTime.UtcNow.Ticks).ToString("x", CultureInfo.InvariantCulture);

		/// <summary>
		/// Patient Key
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Patient Id
		/// </summary>
		public string Id { get; set; }

        /// <summary>
        /// Id of last patient merge
        /// </summary>
        public String LastMergeId { get; set; }

        /// <summary>
        /// epoch time of last patient merge
        /// </summary>
        public String LastMergeTime { get; set; }

        /// <summary>
        /// Last Id of Tracings
        /// </summary>
        public string LastTracing { get; set; }

		/// <summary>
		/// Last Id of Artifacts
		/// </summary>
		public string LastArtifact { get; set; }

        /// <summary>
        /// Last Id of UserActions
        /// </summary>
        public string LastAction { get; set; }

        /// <summary>
		/// Incremental update of the patient data or full download of all data?
		/// </summary>
		public bool Incremental { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public PatternsRequestHeader()
		{
            this.InitializeToZero();
		}

        private void InitializeToZero()
        {
            this.Id = null;
            this.LastMergeId = null;
            this.LastMergeTime = null;
            this.LastTracing = null;
            this.LastArtifact = null;
            this.LastAction = null;
            this.Incremental = false;
        }

		/// <summary>
		/// Reset pointers
		/// </summary>
		public virtual void Reset()
		{
            InitializeToZero();
        }

		/// <summary>
		/// Constructor from an xml element
		/// </summary>
		/// <param name="element"></param>
		public PatternsRequestHeader(XElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			if (element.Attribute("key") != null)
				this.Key = element.Attribute("key").Value;

			if (element.Attribute("id") != null)
				this.Id = element.Attribute("id").Value;

            if (element.Attribute("merge") != null)
                this.LastMergeId = element.Attribute("merge").Value;

            if (element.Attribute("mergetime") != null)
                this.LastMergeTime = element.Attribute("mergetime").Value;

            if (element.Attribute("tracing") != null)
                this.LastTracing = element.Attribute("tracing").Value;

            if (element.Attribute("artifact") != null)
                this.LastArtifact = element.Attribute("artifact").Value;

            if (element.Attribute("action") != null)
				this.LastAction = element.Attribute("action").Value;

			// By default, it is an incremental one if there are already some ids in the header
			this.Incremental =
				(!string.IsNullOrEmpty(this.Id))
					&& (!string.IsNullOrEmpty(this.LastTracing) || 
                        !string.IsNullOrEmpty(this.LastArtifact) || 
                        !string.IsNullOrEmpty(this.LastAction));

			// Make sure the server unique ID is the proper one!
            if ((this.Incremental) && 
                (element.Attribute("serveruid") != null) && 
                (string.CompareOrdinal(element.Attribute("serveruid").Value ?? string.Empty, ServerInstanceUniqueID) != 0))
            {
                InitializeToZero();
            }
		}

		/// <summary>
		/// Serialize as an XElement
		/// </summary>
		/// <returns></returns>
		public virtual XElement SerializeXml()
		{
			return new XElement("request",
				new XAttribute("key", this.Key),
				new XAttribute("id", this.Id ?? "-1"),
                new XAttribute("merge", this.LastMergeId ?? "-1"),
                new XAttribute("mergetime", this.LastMergeTime ?? "-1"),
                new XAttribute("tracing", this.LastTracing ?? "-1"),
                new XAttribute("artifact", this.LastArtifact ?? "-1"),
                new XAttribute("action", this.LastAction ?? "-1"),
				new XAttribute("incremental", this.Incremental ? "1":"0"),
				new XAttribute("serveruid", ServerInstanceUniqueID));
		}
	}

    public class PatternsActionsRequesHeader : PatternsRequestHeader
    {
        /// <summary>
        /// Artifact's start time
        /// </summary>
        public String StartTime { get; set; }

        /// <summary>
        /// Artifact's end time
        /// </summary>
        public String EndTime { get; set; }

        public PatternsActionsRequesHeader()
        {
            StartTime = null;
            EndTime = null;
        }

        public override void Reset()
        {
            StartTime = null;
            EndTime = null;
            base.Reset();
        }

        public PatternsActionsRequesHeader(XElement element)
        {
            if (element.Attribute("startTime") != null)
                StartTime = element.Attribute("startTime").Value;

            if (element.Attribute("endTime") != null)
                EndTime = element.Attribute("endTime").Value;

            Incremental = false;
        }
    }
}

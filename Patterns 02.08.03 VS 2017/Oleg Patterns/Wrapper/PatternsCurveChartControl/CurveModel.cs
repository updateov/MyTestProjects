using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace CurveChartControl
{
	public class CurveModel : INotifyPropertyChanged
	{

		public enum VisitStatusEnum
		{
			Invalid = 0,
			Live = 1,
			Unplugged = 2,
			Recovery = 3,
			Error = 4,
			Late = 5
		}

		public string SnapshotId { get; set; }
		public DateTime? SnapshotLastUpdate { get; set; }

        public VisitStatusEnum VisitStatus { get; set; }

        public String VisitStatusDetail { get; set; }

		public string LaborReferenceMessage { get; set; }

		public bool Valid { get; set; }
		public string Message { get; set; }

		public string PatientId { get; set; }
		public string PatientName { get; set; }
		public string MRN { get; set; }

		public bool? VBAC { get; set; }
		public string VBACLastUpdateDateTime { get; set; }
		public string VBACLastUpdateUserId { get; set; }
		public string VBACLastUpdateUserName { get; set; }
		public Dictionary<string, string> VBACDictionary { get; set; }

		public bool? Vaginal { get; set; }

		public bool Display50Percentile { get; set; }

		/// <summary>
		/// UI requeriments
		/// The value is related with the original question: have previous vaginal deliveries?
		/// YES/NO (True/False)
		/// But for the user it is recomended to show the opposite based in this question: First vaginal delivery?
		/// YES/NO,
		/// So, this property is only used to show the opposite value in the UI.
		/// The calculation still uses the previous meaning of the value.
		/// </summary>
		public bool? VaginalUI
		{
			get
			{
				if (!Vaginal.HasValue) return Vaginal;
				return !Vaginal.Value;
			}
		}

		public string VaginalLastUpdateDateTime { get; set; }
		public string VaginalLastUpdateUserId { get; set; }
		public string VaginalLastUpdateUserName { get; set; }
		public Dictionary<string, string> VaginalDictionary { get; set; }

		public DateTime? Epidural { get; set; }
		public string EpiduralLastUpdateDateTime { get; set; }
		public string EpiduralLastUpdateUserId { get; set; }
		public string EpiduralLastUpdateUserName { get; set; }
		public Dictionary<string, string> EpiduralDictionary { get; set; }

		public DateTime? FirstExam { get; set; }
		public string FirstExamLastUpdateDateTime { get; set; }
		public string FirstExamLastUpdateUserId { get; set; }
		public string FirstExamLastUpdateUserName { get; set; }
		public Dictionary<string, string> FirstExamDictionary { get; set; }

        public bool IsReadOnly { get; set; }

		public DateTime? MembraneRupture { get; set; }
		public string GestationalAge { get; set; }
		public string MembraneStatus { get; set; }
		public string Parity { get; set; }

		public string LastRequestInfo { get; set; }

		public bool IsReviewMode { get; set; }

		public ObservableCollection<Exam> Exams { get; private set; }

		public bool DemoMode = true;

		public CurveModel()
		{
			Reset();

			// No error til the first refresh...
			this.VisitStatus = VisitStatusEnum.Live;
		}

		public void Reset()
		{
			this.Valid = false;

			this.SnapshotId = string.Empty;
			this.SnapshotLastUpdate = null;

			this.Exams = new ObservableCollection<Exam>();

			this.FirstExam = null;
			this.FirstExamDictionary = new Dictionary<string, string>();

			this.GestationalAge = string.Empty;
			this.MembraneStatus = null;
			this.Message = string.Empty;
			this.MRN = null;
			this.Parity = null;
			this.PatientId = string.Empty;
			this.PatientName = string.Empty;
			this.MembraneRupture = null;
			this.VisitStatus = VisitStatusEnum.Live;
			this.VisitStatusDetail = "Invalid data";
			this.DemoMode = true;

			this.Vaginal = null;
			this.VaginalDictionary = new Dictionary<string, string>();

			this.Epidural = null;
			this.EpiduralDictionary = new Dictionary<string, string>();

			this.VBAC = null;
			this.VBACDictionary = new Dictionary<string, string>();

			this.LastRequestInfo = string.Empty;
			this.LaborReferenceMessage = string.Empty;
			this.IsReviewMode = false;
            this.IsReadOnly = false;
		}

		/// <summary>
		/// Helper to raise an error
		/// </summary>
		/// <param name="status"></param>
		/// <param name="details"></param>
		void RaiseError(VisitStatusEnum status, string details)
		{
			this.Reset();
			this.VisitStatus = status;
			this.VisitStatusDetail = details;

			throw new InvalidOperationException(details);
		}

		/// <summary>
		/// Helper to read the data audit for item that can have an audit display (who last changed it and when)
		/// </summary>
		/// <param name="node"></param>
		/// <param name="dictionary"></param>
		/// <param name="codeTitle"></param>
		static void ReadDataAudit(XElement node, Dictionary<string, string> dictionary, string codeTitle)
		{
			dictionary.Clear();

			var lastUpdate = node.GetAttributeAsDateTime("lastUpdate");
			if (lastUpdate.HasValue)
			{
				dictionary.Add("Title", AppResources.LanguageManager.TextTranslated[codeTitle]);
				dictionary.Add("userId", node.GetAttributeAsString("userId"));
				dictionary.Add("userName", node.GetAttributeAsString("userName"));
				dictionary.Add("lastUpdate", lastUpdate.Value.ToLocalTime().ToShortDateString() + " " + lastUpdate.Value.ToLocalTime().ToShortTimeString());
			}
		}

		/// <summary>
		/// Parsing the data returned by the service
		/// </summary>
		/// <param name="data"></param>
		public void ParseCurveData(XElement data)
		{
			if (data == null)
				this.RaiseError(VisitStatusEnum.Error, "Invalid data, missing curve element");

			// Validate version
			if (data.GetAttributeAsInt("invalid_version", 0) != 0)
				this.RaiseError(VisitStatusEnum.Error, AppResources.LanguageManager.TextTranslated["ErrorVersionOutdated"]);

			var patientNode = data.Element("patient");
			if (patientNode == null)
				this.RaiseError(VisitStatusEnum.Error, "Invalid data, missing curve patient element");

			// Check demo mode
			this.DemoMode = data.GetAttributeAsInt("demo_mode", 0) != 0;

			// LastRequest
			this.LastRequestInfo = patientNode.Element("request").ToString();

            // Snapshot Data
            this.IsReviewMode = !string.IsNullOrWhiteSpace(patientNode.Element("request").GetAttributeAsString("snapshot"));

            // Parse status first
            var intStatus = patientNode.GetAttributeAsInt("status", -1);
			if (!Enum.IsDefined(typeof(VisitStatusEnum), intStatus))    
				this.RaiseError(VisitStatusEnum.Error, "Invalid visit status returned");

			this.VisitStatus = (VisitStatusEnum)intStatus;
			this.VisitStatusDetail = patientNode.GetAttributeAsString("statusdetails");

			if (this.VisitStatus == VisitStatusEnum.Error || this.VisitStatus == VisitStatusEnum.Invalid || this.VisitStatus == VisitStatusEnum.Recovery)
				this.RaiseError(VisitStatus, VisitStatusDetail);

			// Patient Header
			this.PatientId = patientNode.GetAttributeAsString("id");
			this.PatientName = patientNode.GetAttributeAsString("lastname") + ", " + patientNode.GetAttributeAsString("firstname");
			this.MRN = patientNode.GetAttributeAsString("mrn");
            this.IsReadOnly = patientNode.GetAttributeAsBoolean("readonly")??false;

			//Curve data
			var snapshotNode = patientNode.Element("snapshot");
			if (snapshotNode != null)
			{
				this.SnapshotId = snapshotNode.GetAttributeAsString("id");
				this.SnapshotLastUpdate = snapshotNode.GetAttributeAsDateTime("update");
				
				var curveNode = snapshotNode.Element("curve");
				if (curveNode == null)
					this.RaiseError(VisitStatusEnum.Error, "Invalid data, missing snapshot curve element");

				this.Valid = curveNode.GetAttributeAsBoolean("valid", false);
				this.Message = curveNode.GetAttributeAsString("message");

				this.LaborReferenceMessage = curveNode.GetAttributeAsString("laborReferenceMessage");

				this.Display50Percentile = curveNode.GetAttributeAsBoolean("display50Percentile", true);

				this.MembraneStatus = curveNode.GetAttributeAsString("membraneStatus");
				this.MembraneRupture = curveNode.GetAttributeAsDateTime("membraneRupture");

				this.Parity = curveNode.GetAttributeAsString("parity");
				this.GestationalAge = patientNode.GetAttributeAsString("ga");

				var node = curveNode.Element("epidural");
				if (node != null)
				{
					this.Epidural = node.GetAttributeAsDateTime("value");
					ReadDataAudit(node, EpiduralDictionary, "Epidural");
				}

				node = curveNode.Element("vbac");
				if (node != null)
				{
					this.VBAC = node.GetAttributeAsBoolean("value");
					ReadDataAudit(node, VBACDictionary, "QuestionVBAC");
				}

				node = curveNode.Element("previousVaginal");
				if (node != null)
				{
					this.Vaginal = node.GetAttributeAsBoolean("value");
					ReadDataAudit(node, VaginalDictionary, "QuestionPreviousVaginalDeliveries");
				}

				node = curveNode.Element("firstExam");
				if (node != null)
				{
					this.FirstExam = node.GetAttributeAsDateTime("value");
					ReadDataAudit(node, FirstExamDictionary, "FirstExam");
				}

				var exams = (from exam in curveNode.Element("exams").Elements() select exam).OrderBy(ex2 => ex2.GetAttributeAsString("time"));
				this.Exams.Clear();
				foreach (var examNode in exams)
				{
					this.Exams.Add(
						new Exam
							{
								Time = examNode.GetAttributeAsDateTime("time").Value,

								Dilatation = examNode.GetAttributeAsDouble("dilatation"),
								Effacement = examNode.GetAttributeAsInt("effacement"),
								Station = examNode.GetAttributeAsInt("station"),

								Presentation = examNode.GetAttributeAsString("presentation"),
								PositionString = examNode.GetAttributeAsString("position"),
								Position = examNode.GetAttributeAsFetalPosition("positionCode"),

								ContractionInterval = examNode.GetAttributeAsDouble("contractionInterval"),

								ExpectedDilatation = examNode.GetAttributeAsDouble("expectedDilatation"),
								LowerExpectedDilatation = examNode.GetAttributeAsDouble("lowerExpectedDilatation"),
								UpperExpectedDilatation = examNode.GetAttributeAsDouble("upperExpectedDilatation"),

								Percentile = examNode.GetAttributeAsDouble("percentile"),

								Status = examNode.GetAttributeAsCurveCalculationStatuses("status"),
								Message = examNode.GetAttributeAsString("message")
							});
				}

				//Set First Exam            
				var firstexam = this.Exams.OrderBy(ex1 => ex1.Time).FirstOrDefault(ex => ex.Time >= FirstExam);
				if (firstexam != null)
				{
					firstexam.IsFirstExam = true;

					//Flag for UI to consider which exam is not considered for calculation
					int index = Exams.IndexOf(firstexam);
					for (int i = 0; i < Exams.Count; i++)
					{
						if (Exams[i].IsFirstExam || i > index)
						{
							Exams[i].IsConsideredForCalculation = true;
						}
						else
						{
							Exams[i].IsConsideredForCalculation = false;
						}
					}
				}
				else
				{
					//No first exam detected, all are considered for calculation						
					foreach (var exam in Exams)
					{
						exam.IsFirstExam = false;
						exam.IsConsideredForCalculation = true;
					}
				}
			}
		}

		#region INotifyPropertyChanged Members

		private void DoPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}

	public class Exam : INotifyPropertyChanged
	{
		public enum FetalPositionEnum
		{
			Unknown = 0,
			LOA = 1,
			LOP = 2,
			LOT = 3,
			OA = 4,
			OP = 5,
			ROA = 6,
			ROP = 7,
			ROT = 8
		}

		/// <summary>
		/// The statuses
		/// </summary>
		public enum CurveCalculationStatuses
		{
			Ignored = 0,
			Valid = 1,
			Info = 2,
			Error = 3
		};

		/// <summary>
		/// The chart control needs a double for the dates and not a datetime
		/// </summary>
		public double TimeOADate 
		{ 
			get 
			{ 
				var value = this.Time.ToOADate();
				if (double.IsNaN(value))
					Debug.Assert(false);
				return value;
			}
		}

		private DateTime _time;
		public DateTime Time
		{
			get
			{
				return _time;
			}
			set
			{
				if (_time != value)
				{
					_time = value;
					DoPropertyChanged("Time");
				}
			}
		}

		public double? Dilatation { get; set; }
		public int? Effacement { get; set; }
		public int? Station { get; set; }

		public string Presentation { get; set; }
		public FetalPositionEnum Position { get; set; }
		public string PositionString { get; set; }

		public double? ContractionInterval { get; set; }

		public string ContractionIntervalWithUnit { get { return this.ContractionInterval.HasValue ? string.Format("{0:0.#} mins", this.ContractionInterval.Value) : string.Empty; } }

		public double? Percentile { get; set; }
		public double? ExpectedDilatation { get; set; }
		public double? LowerExpectedDilatation { get; set; }
		public double? UpperExpectedDilatation { get; set; }

		public CurveCalculationStatuses Status { get; set; }
		public string Message { get; set; }

		private bool _isFirstExam = false;
		public bool IsFirstExam
		{
			get { return _isFirstExam; }
			set
			{
				if (_isFirstExam != value)
				{
					_isFirstExam = value;
					DoPropertyChanged("IsFirstExam");
				}
			}
		}

		private bool _isConsideredForCalculation = true;
		public bool IsConsideredForCalculation
		{
			get { return _isConsideredForCalculation; }
			set
			{
				if (_isConsideredForCalculation != value)
				{
					_isConsideredForCalculation = value;
					DoPropertyChanged("IsConsideredForCalculation");
				}
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;
		private void DoPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}

	public class Snapshot
	{
		public Snapshot()
		{
			this.Id = -1;
			this.Date = DateTime.MinValue;
		}

		public Snapshot(XElement data)
			: this()
		{
			this.Parse(data);
		}

		public int Id { get; set; }
		public DateTime Date { get; set; }

		/// <summary>
		/// Indicates if this item is currently the live one and not the review mode.
		/// It must be set by the UI
		/// </summary>
		public bool IsLive { get; set; }

		/// <summary>
		/// For display in the snapshot selectiondrop down
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Date.ToLocalTime().ToShortDateString() + " " + this.Date.ToLocalTime().ToShortTimeString();
		}

		public void Parse(XElement data)
		{
			this.Id = data.GetAttributeAsInt("id", -1);
			this.Date = data.GetAttributeAsDateTime("updateTime", DateTime.MinValue);
		}
	}
}

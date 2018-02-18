using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.ActiveXInterface;
using PeriGen.Patterns.Service;

namespace PeriGen.Patterns.Curve
{
	/// <summary>
	/// The fetal positions 
	/// </summary>
	public enum FetalPositions
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

	[Flags]
	public enum CurveCalculationReasons
	{
		None = 0x000000,
		MissingParity = 0x000001,				// The patient's parity has not been entered.
		MissingVBAC = 0x000002,					// The VBAC option has not been entered.
		MissingPreviousVaginal = 0x000004,		// The previous vaginal option has not been entered.
		MissingEDD = 0x000008,					// The EDD has not been entered.
		NotSingleton = 0x000010,				// The number of fetuses is not set to Singleton.
		NoContractions = 0x000020,				// No contractions have been detected since the first exam.
		InvalidParity = 0x000040,				// Parity has an invalid value
		MissingDilatation = 0x000080,			// Dilatation is missing from one or more exams.			
		MissingEffacement = 0x000100,			// Effacement is missing from one or more exams.			
		MissingStation = 0x000200,				// Station is missing from one or more exams.				
		MissingPresentation = 0x000400,			// Presentation is missing from one or more exams.		
		InvalidDilatation = 0x000800,			// Dilatation has an invalid value in one or more exams.	
		InvalidEffacement = 0x001000,			// Effacement has an invalid value in one or more exams.	
		InvalidStation = 0x002000,				// Station has an invalid value in one or more exams.		
		DescendingDilatation = 0x004000,		// Descending dilatation.
		NotCephalic = 0x008000,					// Presentation values must all be Cephalic.					
		DilationBelow3cm = 0x010000,			// Dilatation is below 3 cm in one or more exams.			
		Below35week = 0x020000,					// Gestational Age is below 35 weeks.
		ParityNoFirstVaginalNoVBAC = 0x040000,	// Both "First Vaginal Delivery" and "VBAC" are set to "No", therefore the patient's parity must be 1 or more.
		ParityFirstVaginalVBAC = 0x080000,		// Both "First Vaginal Delivery" and "VBAC" are set to "Yes", therefore the patient's parity must be 1 or more.
		ParityFirstVaginalNoVBAC = 0x100000,	// "First Vaginal Delivery" is set to "Yes" and "VBAC" is set to "No", therefore the patient's parity must be 0.
		ParityNoFirstVaginalVBAC = 0x200000,	// "First Vaginal Delivery" is set to "No" and "VBAC" is set to "Yes", therefore the patient's parity must be 2 or more.
		NotEnoughExams = 0x400000,				// Insufficient number of charted vaginal exams.
		ExamInNearFuture = 0x800000,			// Exam is time stamped in the future.
		ExamInFarFuture = 0x1000000,			// Exam is time stamped more than 5 minutes in the future.
		BeforeFirstExam = 0x2000000,			// Before exam marked as first exam
		AlreadyReach10cm = 0x4000000,			// Already reach 10 cm dilatation
	};

	public class CurveExam
	{
		#region Configuration and constants

		static Dictionary<string, FetalPositions> Settings_PositionMappings = PatternsServiceSettings.Instance.PositionMappings.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(v => v.Split("=".ToCharArray())).ToDictionary(v => v.First().ToUpperInvariant(), v => (FetalPositions)int.Parse(v.Last()));

		#endregion

		/// <summary>
		/// Ctr.
		/// </summary>
		/// <param name="exam"></param>
		public CurveExam(DataContextEpisode.PelvicExam exam)
		{
			this.Time = exam.Time.ToDateTime();
			this.Exam = exam;

			this.Status = CurveCalculationStatuses.Valid;
			this.StatusReason = CurveCalculationReasons.None;
						
			this.Presentation = exam.Presentation;

			// Map the position to the code
			if (!string.IsNullOrWhiteSpace(exam.Position))
			{
				FetalPositions fetalPosition;
				if (Settings_PositionMappings.TryGetValue(exam.Position.ToUpperInvariant(), out fetalPosition))
				{
					this.FetalPosition = fetalPosition;
				}
			}
		}

		/// <summary>
		/// The exam time
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// The status code
		/// </summary>
		public CurveCalculationStatuses Status { get; set; }

		/// <summary>
		/// The status reason if applicable
		/// </summary>
		public CurveCalculationReasons StatusReason { get; set; }

		/// <summary>
		/// The underlying exam
		/// </summary>
		public DataContextEpisode.PelvicExam Exam { get; set; }

		/// <summary>
		/// Dilatation value
		/// </summary>
		public double? Dilatation { get; set; }

		/// <summary>
		/// Effacement value
		/// </summary>
		public int? Effacement { get; set; }

		/// <summary>
		/// Station value
		/// </summary>
		public int? Station { get; set; }

		/// <summary>
		/// To handle the carry forward of missing presentation
		/// </summary>
		public string Presentation { get; set; }

		/// <summary>
		/// Fetal position
		/// </summary>
		public FetalPositions FetalPosition { get; set; }

		/// <summary>
		/// Contractions count
		/// </summary>
		public int? ContractionsSinceFirstExam { get; set; }

		/// <summary>
		/// Average Contractions interval in minutes
		/// </summary>
		public double? AverageContractionsInterval { get; set; }

		/// <summary>
		/// Expected dilatation (calculated)
		/// </summary>
		public double? ExpectedDilatation { get; set; }

		/// <summary>
		/// Expected dlatation for the upper percentile limit (calculated)
		/// </summary>
		public double? UpperExpectedDilatation { get; set; }

		/// <summary>
		/// Expected dlatation for the lower percentile limit (calculated)
		/// </summary>
		public double? LowerExpectedDilatation { get; set; }
	
		/// <summary>
		/// Percentile (calculated)
		/// </summary>
		public double? Percentile { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataContextEpisode;
using PeriGen.Patterns.Engine.Data;
using PeriGen.Patterns.ActiveXInterface;
namespace PeriGen.Patterns.Service
{
	namespace Curve
	{		
		public class Constants
			{

				public enum ValidityEnum { Valid, Invalid, InvalidatesLPM, Complete };

				public enum StatusEnum : ulong
				{
					NONE = 0x0,
					DILATATION_MISSING = 0x00000001,
					DILATATION_DESCENDING = 0x00000002,
					NO_CONTRACTIONS = 0x00000004,
					CONTRACTIONS_DESCENDING = 0x00000008,
					ALL_DILATATIONS_BELOW_MINIMUM = 0x00000010,
					MODEL_NOT_SET = 0x0020,
					PROFILE_NOT_SET = 0x00000040,
					UPPER_PERCENTILE_NOT_SET = 0x00000080,
					LOWER_PERCENTILE_NOT_SET = 0x00000100,
					NOT_SINGLETON = 0x00000200,
					EDC_NOT_SET = 0x00000400,
					GESTATIONAL_AGE_BELOW35 = 0x00000800,
					PRESENTATION_NOT_SET = 0x00001000,
					PRESENTATION_NOT_CEPHALIC = 0x00002000,
					PRESENTATION_CEPHALIC = 0x00004000,
					LAST_DILATATION_BELOW_MINIMUM = 0x00008000,
					STATION_MISSING = 0x00010000,
					EFFACEMENT_MISSING = 0x00020000,
					PRESENTATION_MISSING = 0x00040000,
					STARTING_EXAM_DATETIME_INVALID = 0x00080000,
					DECISION_SUPPORT_NOT_INSTALLED = 0x00100000
				}

				public const double LPM_DILATATION_FACTOR_PRIM = 0.80515;
				public const double LPM_EFFACEMENT_FACTOR_PRIM = 0.024109; // for 0 - 100 range
				public const double LPM_STATION_FACTOR_PRIM = -0.03909;
				public const double LPM_EPIDURAL_FACTOR_PRIM = 0.14627;
				public const double LPM_CUMULATIVEUA_FACTOR_PRIM = 0.00447;
				public const double LPM_CONSTANT_PRIM = 0.24952;
				public const double LPM_SE_PRIM = 1.3014;

				public const double LPM_DILATATION_FACTOR_MULT = 0.777;
				public const double LPM_EFFACEMENT_FACTOR_MULT = 0.015503;  // for 0 - 100 range
				public const double LPM_STATION_FACTOR_MULT = -0.20669;
				public const double LPM_EPIDURAL_FACTOR_MULT = 0.23484;
				public const double LPM_CUMULATIVEUA_FACTOR_MULT = 0.00766;
				public const double LPM_CONSTANT_MULT = 1.9914;
				public const double LPM_SE_MULT = 1.3451;

				/// <summary>
				/// Invalid Values
				/// </summary>
				public const double C_INVALID_DILATATION = -1.0;
				public const double C_MINIMUM_DILATATION_FOR_LPM = 3.0;
				public const double C_INVALID_PERCENTILE = -1.0;
				public const int C_INVALID_EFFACEMENT = -1;
				public const double C_INVALID_STATION = int.MaxValue;
				public const int C_INVALID_CONTRACTION_COUNT = 0;
				public const int C_INVALID_PRESENTATION = 0;
				public const int C_NO_CLINICAL_DATA = -1;

			}
		
		public partial class PelvicExamEx : PelvicExam
		{
			public PelvicExamEx() {}

			public PelvicExamEx(PelvicExam item):this()
			{				
				this.Dilatation = item.Dilatation;
				this.Effacement = item.Effacement;
				this.ExamId = item.ExamId;
				this.Position = item.Position;
				this.Presentation = item.Presentation;
				this.Station = item.Station;
				this.UpdateTime = item.UpdateTime;
			}

			public double ExpectedDilatation { get; set; }
			public double ExpectedPercentile { get; set; }

			public int ContractionsCount { get; set; }


			/// <summary>
			/// Validate dilatation value
			/// </summary>
			/// <returns></returns>
			public bool HasValidDilatation()
			{
				return (DilatationValue >= 0.0);
			}

			public double DilatationValue
			{
				get
				{
					double result = 0.0;
					double.TryParse(Dilatation, out result);
					return result;
				}
			}

			/// <summary>
			/// Validate Effacement value
			/// </summary>
			/// <returns></returns>
			public bool HasValidEffacement()
			{
				return (EffacementValue >= 0);

			}

			public int EffacementValue
			{
				get
				{
					int result = 0;
					int.TryParse(Effacement, out result);
					return result;
				}
			}

			/// <summary>
			/// Validate station value
			/// </summary>
			/// <returns></returns>
			public bool HasValidStation()
			{
				return (StationValue >= -5.0 && StationValue <= 5.0);
			}

			public double StationValue
			{
				get
				{
					double result = -1000.0;
					double.TryParse(Station, out result);
					return result;
				}
			}

			/// <summary>
			/// Validate presentation value
			/// </summary>
			/// <returns></returns>
			public bool HasValidPresentation()
			{
				///TODO Curve, meaning of -100451??????
				return (PresentationValue > 0 || PresentationValue == -100451);
			}

			public int PresentationValue
			{
				get
				{
					int result = -1;
					if (Presentation.ToUpperInvariant().StartsWith("CEPHALIC"))  result= 9900227; ///CEPHALIC_VERTEX:
					return result;
				}
			}

			/// TODO Curve Contractions count missing
			public int GetContractionCount()
			{
				return ContractionsCount;
			}

			// =================================================================================================================
			//    !Returns true if usable by CalculateExpectedDilatation(). Copies itself to targetUnit. This is done as certain
			//    values may be absent and need to be set to acceptable values. Since we don't want to modify 'this' object, we use
			//    targetUnit. Params: -lastValidUnit contains the last (full) set of acceptable values. -targetUnit will be set to
			//    the validated values in 'this' object. Rules: -If Dilatation is absent the LPMUnit invalidates LPM -If Effacement
			//    is absent, assume previous value, or 0 if never entered. -If Station is absent, assume == 0. //NOT USED ANY LONGER:
			//    -If Epidural is absent, assume previous value, or 0 if never entered.
			// =================================================================================================================
			public Constants.ValidityEnum IsUsable(PelvicExamEx lastValidUnit, PelvicExamEx targetUnit)
			{
				Constants.ValidityEnum validity = Constants.ValidityEnum.Valid;

				///TODO Curve check clone object a la C++ (*this) ??????
				targetUnit = (PelvicExamEx)this.MemberwiseClone();

				if (!HasValidDilatation())
				{
					return Constants.ValidityEnum.InvalidatesLPM;
				}

				if (!HasValidEffacement())
				{
					targetUnit.Effacement = lastValidUnit.Effacement;
				}

				if (!HasValidStation())
				{
					targetUnit.Station = "0";
				}

				targetUnit.Station = (-1 * (targetUnit.StationValue - 3)).ToString();	// as per Dr Hamilton's Excel spreadsheet

				///TODO Curve check contractions count validatin.....
				//if (!HasValidContractionCount())
				//{
				//    targetUnit.m_nContractionCount = 0;
				//}

				lastValidUnit = targetUnit; // we may now update lastValidUnit with current values
				return validity;
			}

			// =================================================================================================================
			//    Calculates expected dilatation and assigns it to m_dExpectedDilatation. Params: -Unit1 is associated with a
			//    date/time >= that of Unit0 -targetUnit is the Unit whose Expected Dilatation, Percentile, etc. are being calculated
			//    & set.
			// =================================================================================================================
			public void CalculateExpectedDilatation(PelvicExamEx exam0, PelvicExamEx exam1, PelvicExamEx targetExam, bool bPrimip, long timeEpidural)
			{
				double expectedDilatation = -1;
				double expectedPercentile = -1;

				int nEpidural = 0;
				///TODO Curve check time INVALID
				///if (timeEpidural != LMSValue::Time::INVALID)
				if (timeEpidural != 0)
				{
					nEpidural = timeEpidural <= exam1.Time ? 1 : 0;
				}

				var curve = new CurveEngine();
				curve.CalculateExpectedDilatationAndPercentile(exam0.DilatationValue, exam0.EffacementValue, exam0.StationValue, exam1.DilatationValue, exam1.GetContractionCount(), nEpidural, !bPrimip, out expectedDilatation, out expectedPercentile);
				curve = null;

				targetExam.ExpectedDilatation = expectedDilatation;
				targetExam.ExpectedPercentile = expectedPercentile;
			}

		}

		public partial class PelvicExams : List<PelvicExamEx>
		{

			public PelvicExams() { }
			
			public PelvicExams(List<PelvicExam> items):this()
			{
				if(items!=null) items.ForEach(item => this.Add(new PelvicExamEx(item)));
			}
			
			///TODO Curve check use of this variable
			bool m_bHasLPMInfo = false;
			public bool HasLPMInfo { get { return m_bHasLPMInfo; } set { m_bHasLPMInfo = value; } }

			/// TODO Curve set StartingTime DateTime or Long??????		
			long m_timeStartingExam=0;

			/// TODO Curve set m_timeEpidural DateTime or Long??????		
			long m_timeEpidural=0;

			/// <summary>
			/// Pre-check all LPMUnits for anything that *PREVENTS* LPM from being calculated (e.g. missing/descending dilatations)
			/// </summary>
			/// <param name="parity"></param>
			/// <returns></returns>
			public bool IsSetValid(int parity)
			{
				Constants.StatusEnum status = GetStatus();

				return IsValidParity(parity) &&
						IsDecisionSupportInstalled(status) &&
						!MissingDilatation(status) &&
						!MissingStation(status) &&
						!MissingEffacement(status) &&
						!MissingPresentation(status) &&
						!DilatationDescending(status) &&
						!ContractionsDescending(status) &&
						HasContractions(status) &&
						IsSingleton(status) &&
						IsEDCSet(status) &&
						!IsGestationalAgeBelow35(status) &&
						IsPresentationCephalic(status) &&
						IsValidStartingExamDateTime(status);
			}

			/// TODO Curve Review validation functions
			#region Validation functions

			static bool IsDecisionSupportInstalled(Constants.StatusEnum status) { return (status & Constants.StatusEnum.DECISION_SUPPORT_NOT_INSTALLED) == 0; }
			
			static bool MissingDilatation(Constants.StatusEnum status) { return (status & Constants.StatusEnum.DILATATION_MISSING) != 0; } //returns true if one or more dilatations are missing
			
			static bool MissingStation(Constants.StatusEnum status) { return (status & Constants.StatusEnum.STATION_MISSING) != 0; } //returns true if one or more stations are missing
			
			static bool MissingEffacement(Constants.StatusEnum status) { return (status & Constants.StatusEnum.EFFACEMENT_MISSING) != 0; } //returns true if one or more effacements are missing
			
			static bool MissingPresentation(Constants.StatusEnum status) { return (status & Constants.StatusEnum.PRESENTATION_MISSING) != 0; } //returns true if one or more positions are missing
			
			static bool DilatationDescending(Constants.StatusEnum status) { return (status & Constants.StatusEnum.DILATATION_DESCENDING) != 0; } //returns true if one or more dilatations are in descending order
			
			static bool HasContractions(Constants.StatusEnum status) { return (status & Constants.StatusEnum.NO_CONTRACTIONS) == 0; } //returns true if at least one LPMUnit has a ContractionCount > 0
			
			static bool ContractionsDescending(Constants.StatusEnum status) { return (status & Constants.StatusEnum.CONTRACTIONS_DESCENDING) != 0; } //returns true if contraction counts are descending
			
			static bool IsSingleton(Constants.StatusEnum status) { return (status & Constants.StatusEnum.NOT_SINGLETON) == 0; } //returns true if there is one fetus
			
			static bool IsEDCSet(Constants.StatusEnum status) { return (status & Constants.StatusEnum.EDC_NOT_SET) == 0; } //returns true if EDC has been set (required for Gestational Age calculation)
			
			static bool IsGestationalAgeBelow35(Constants.StatusEnum status) { return (status & Constants.StatusEnum.GESTATIONAL_AGE_BELOW35) != 0; } //returns true if Gestational Age is below 35 weeks, or not set
			
			static bool IsPresentationCephalic(Constants.StatusEnum status) { return (status & Constants.StatusEnum.PRESENTATION_NOT_CEPHALIC) == 0; } //returns true if Presentation value is not "Cephalic"
			
			static bool IsValidStartingExamDateTime(Constants.StatusEnum status) { return (status & Constants.StatusEnum.STARTING_EXAM_DATETIME_INVALID) == 0; }

			bool IsValidParity(int parity)
			{
				if (parity == -1)
				{
					return false;
				}

				if (parity == 0)
				{
					return (LPMVBAC != s_nVBAC) && (LPMComparisonModel != s_nCMPreviousVaginalDeliveries);
				}

				return true;

			}

			#endregion

			/// <summary>
			/// Get the status of the collection
			/// </summary>
			/// <returns></returns>
			public Constants.StatusEnum GetStatus()
			{
				///TODO Curve IsDecisionSupportInstalled
				if (!IsDecisionSupportInstalled()) return Constants.StatusEnum.DECISION_SUPPORT_NOT_INSTALLED;

				//initialize status to zero
				Constants.StatusEnum statusResult = Constants.StatusEnum.NONE;

				//initialize defaults
				int previousContractionCount = Constants.C_INVALID_CONTRACTION_COUNT;
				int thisContractionCount = Constants.C_INVALID_CONTRACTION_COUNT;
				double previousDilatation = 0.0;
				double highestDilatation = 0.0;
				bool validPresentationNotCephalic = true;
				bool validPresentation = true;
				bool validDilatation = true;
				bool validEffacement = true;
				bool validStation = true;
				bool atLeastOne = false;

				///TODO Curve DateTime = INVALID is equal to DateTime.MinValue||0 value --- Use Long??????
				long dtVeryFirstExam = 0;

				///Check Items
				foreach (var exam in this)
				{

					if (exam.Time >= dtVeryFirstExam) dtVeryFirstExam = exam.Time;

					if (exam.Time >= m_timeStartingExam)
					{
						atLeastOne = true;

						#region Check Dilatation

						if (exam.HasValidDilatation())
						{

							if (exam.DilatationValue < previousDilatation)
							{
								//Dilatation are in descendig order
								statusResult |= Constants.StatusEnum.DILATATION_DESCENDING;
							}
							highestDilatation = Math.Max(highestDilatation, exam.DilatationValue);
							previousDilatation = exam.DilatationValue;
						}
						else
						{
							validDilatation = false;
						}

						#endregion

						#region Check Effacement

						validEffacement = exam.HasValidEffacement();

						#endregion

						#region Check Station

						validStation = exam.HasValidStation();

						#endregion

						#region Check Presentation

						validPresentation = exam.HasValidPresentation();

						///TODO curve what 9900227 is??????
						if (exam.PresentationValue != 9900227 && exam.PresentationValue != Constants.C_INVALID_PRESENTATION) validPresentationNotCephalic = false;

						#endregion

						#region Check Contractions Count

						thisContractionCount = Math.Max(thisContractionCount, exam.GetContractionCount());
						if (thisContractionCount < previousContractionCount) statusResult |= Constants.StatusEnum.CONTRACTIONS_DESCENDING;
						previousContractionCount = thisContractionCount;

						#endregion

					}

				}

				if (atLeastOne)
				{

					if (previousDilatation >= 0 && previousDilatation < Constants.C_MINIMUM_DILATATION_FOR_LPM) statusResult |= Constants.StatusEnum.LAST_DILATATION_BELOW_MINIMUM;

					if (!validDilatation) statusResult |= Constants.StatusEnum.DILATATION_MISSING;

					if (!validEffacement) statusResult |= Constants.StatusEnum.EFFACEMENT_MISSING;

					if (!validStation) statusResult |= Constants.StatusEnum.STATION_MISSING;

					///TODO Curve 3xCALMInstallationDateTime Presentation Validation
					if (!validPresentation)
					{
						//// For 2.9 visit, we don't complain on this...
						//if ((s_dt3xCALMInstallationDatetime == LMSValue::Time::INVALID) || (dtVeryFirstExam >= s_dt3xCALMInstallationDatetime))
						//{
						//    tStatus |= s_nPRESENTATION_MISSING;
						//}					
						statusResult |= Constants.StatusEnum.PRESENTATION_MISSING;
					}

					if (!validPresentationNotCephalic) statusResult |= Constants.StatusEnum.PRESENTATION_NOT_CEPHALIC;

					// (assumed) all Dilatations are below minimum for LPM calc	
					if (highestDilatation < Constants.C_MINIMUM_DILATATION_FOR_LPM) statusResult |= Constants.StatusEnum.ALL_DILATATIONS_BELOW_MINIMUM;

					///TODO Curve Contractions evaluated as number not as a boolean like C++. Check C++ code to see if validation is ok.
					if (thisContractionCount == Constants.C_INVALID_CONTRACTION_COUNT) statusResult |= Constants.StatusEnum.NO_CONTRACTIONS;

				}

				///TODO Curve review following properties
				if (LPMComparisonModel == Constants.C_NO_CLINICAL_DATA) statusResult |= Constants.StatusEnum.MODEL_NOT_SET;
				if (LPMVBAC == Constants.C_NO_CLINICAL_DATA) statusResult |= Constants.StatusEnum.PROFILE_NOT_SET;
				if (UpperPercentile == Constants.C_NO_CLINICAL_DATA) statusResult |= Constants.StatusEnum.UPPER_PERCENTILE_NOT_SET;
				if (LowerPercentile == Constants.C_NO_CLINICAL_DATA) statusResult |= Constants.StatusEnum.LOWER_PERCENTILE_NOT_SET;
				if (!Singleton) statusResult |= Constants.StatusEnum.NOT_SINGLETON;
				if (!EDC) statusResult |= Constants.StatusEnum.EDC_NOT_SET;
				if (GestationalAgeBelow35) statusResult |= Constants.StatusEnum.GESTATIONAL_AGE_BELOW35;


				///TODO Curve review Validate starting time for CALM 2.9?????

				// // For 2.9 visit, we don't complain on this...
				//if ((s_dt3xCALMInstallationDatetime == LMSValue::Time::INVALID) || (dtVeryFirstExam >= s_dt3xCALMInstallationDatetime))
				//{
				//    if (!ValidateStartingExamDateTime())
				//    {
				//        tStatus |= s_nStartingExamDateTimeInvalid;
				//    }
				//}


				//Return result
				return statusResult;
			}

			///TODO Curve review following properties and constants
			#region Properties to Review

			int LPMComparisonModel = Constants.C_NO_CLINICAL_DATA;
			int LPMVBAC = Constants.C_NO_CLINICAL_DATA;
			int UpperPercentile = Constants.C_NO_CLINICAL_DATA;
			int LowerPercentile = Constants.C_NO_CLINICAL_DATA;
			bool Singleton = false;
			bool EDC = false;
			bool GestationalAgeBelow35 = true;

			const double LPM_PERCENTILE_FACTOR_97 = 1.8810;
			const double LPM_PERCENTILE_FACTOR_95 = 1.6450;
			const double LPM_PERCENTILE_FACTOR_90 = 1.2800;

			const int s_n97thPercentile = 97;
			const int s_n95thPercentile = 95;
			const int s_n90thPercentile = 90;
			const int s_n10thPercentile = 10;
			const int s_n5thPercentile = 5;
			const int s_n3thPercentile = 3;

			const int s_nNoClinicalDat = -1;
			const int s_nNonVBAC = 0;
			const int s_nVBAC = 1;
			const int s_nCMNoPreviousVaginalDeliveries = 0;
			const int s_nCMPreviousVaginalDeliveries = 1;

			const int s_nNoClinicalData_29 = -1;
			const int s_nNonVBAC_29 = -10;
			const int s_nVBAC_29 = -11;
			const int s_nCMNoPreviousVaginalDeliveries_29 = -12;
			const int s_nCMPreviousVaginalDeliveries_29 = -13;
			long s_dt3xCALMInstallationDatetime;

			#endregion

			private bool IsDecisionSupportInstalled()
			{
				return true;
				throw new NotImplementedException("IsDecisionSupportInstalled not implemented");
			}

			public void Calculate(int parity, List<Contraction> contractions, long firstExamTime)
			{
				///TODO Curve --- Set first Exam Time
				m_timeStartingExam = firstExamTime;
				
				///TODO - PelvicExam class must be modified to handle expected dilatation and expected percentile.
				///TODO - PelvicExamEx was created in meantime

				///TODO Curve review validity field
				Constants.ValidityEnum validity = Constants.ValidityEnum.Valid;

				//Initialization
				PelvicExamEx lastValidExam = null;
				PelvicExamEx validExam = null;
				PelvicExamEx validExam0 = lastValidExam;
				PelvicExamEx validExam1 = lastValidExam;

				PelvicExamEx first = null;
				PelvicExamEx second = null;

				HasLPMInfo = false; //Initialize to 0 // remove all previous calculations
				this.ForEach((item) =>
				{
					item.ExpectedDilatation = Constants.C_NO_CLINICAL_DATA;
					item.ExpectedPercentile = Constants.C_NO_CLINICAL_DATA;
				});

				///At least 2 exams for calculation
				if (this.Count < 2) return;

				///TODO CURVE -  If comparison Model and/or profile not set - Calc CANNOT be performed. Where do we pass these parameters...???				
				int LPMComparisonModel = 0;
				int LPMVBAC = 0;
				if (LPMComparisonModel == Constants.C_NO_CLINICAL_DATA || LPMVBAC == Constants.C_NO_CLINICAL_DATA) return;

				// Pre-check all LPMUnits for anything that *PREVENTS* LPM from being calculated (e.g. missing/descending Dilatations)
				if (!IsSetValid(parity)) return;

				// Set for Primip or Multip calculation! :
				bool bPrimip = (LPMComparisonModel == s_nCMNoPreviousVaginalDeliveries);

				bool bValidityState = false;

				///TODO Curve review how to get the first exam
				long startTime = GetStartingExam();


				///TODO Curve Review contractions code....
				//Calculate Contractions count for Exams
				foreach (var exam in this)
				{
					exam.ContractionsCount= contractions.Count(c => c.PeakTime.ToEpoch() >= startTime && c.PeakTime.ToEpoch() < exam.Time);
				}

				// Find valid pairs of LPMUnits and for each pair, calculate expected dilatation:
				foreach (var exam in this)
				{

					// verify that the Unit is lest that first exam
					if (startTime > exam.Time)
					{
						continue;
					}

					// Validate
					validity = exam.IsUsable(lastValidExam, validExam);	// determine whether this LPMUnit is usable
					if (validity == Constants.ValidityEnum.InvalidatesLPM)				// this LPMUnit prevents LPM from being calculated.
					{
						return;
					}

					if (validity == Constants.ValidityEnum.Invalid && bValidityState)	// this LPMUnit is not usable, but previous LPMUnit/s were usable - LPM cannot be calculated.
					{
						return;
					}

					if (validity == Constants.ValidityEnum.Valid)
					{
						// we have a usable LPMUnit!
						bValidityState = true;
					}

					if (validity == Constants.ValidityEnum.Invalid)
					{
						// this LPMUnit is not usable, move on to the next one
						continue;
					}

					// set pointer to first valid LPMUnit
					if (first == null)
					{
						first = exam;
						validExam0 = validExam;
					}
					else
					{
						second = exam;	// set second LPMUnit in pair
						validExam1 = validExam;
						validity = AreValid(validExam0, validExam1);	// check the validity of the pair
						if (validity == Constants.ValidityEnum.InvalidatesLPM)		// pair is invalid and prevents LPM from being calculated
						{
							return;
						}

						if (validity == Constants.ValidityEnum.Complete)
						{
							// we're finished calculating LPM
							break;
						}

						if (validity == Constants.ValidityEnum.Valid)
						{
							second.CalculateExpectedDilatation(validExam0, validExam1, second, bPrimip, m_timeEpidural);
						}

						first = second;	// second LPMUnit becomes first LPMUnit of the next pair...
						validExam0 = validExam;
						second = null;
					}
				}
				HasLPMInfo = true;		// set now contains valid LPM info
			}

			// =================================================================================================================
			// ! Returns validity of a pair of LPMUnit objects, based upon the following: Dilatation of Exam0 must be <=
			// that of Exam1 //Invalidates LPM. The time of Exam1 must be on or after the time of labour onset. Does not
			// invalidate the LPM. Dilatation of Exam1 must be >= 3.0 //Does not invalidate LPM. As per Dr Hamilton.
			// Contractions of Exam1 must be > 0 //Does not invalidate LPM. Dilatations must not both be >= 10.0 //Does not
			// invalidate LPM. Params: -Exam1 is associated with a date/time >= that of Exam
			// =================================================================================================================
			Constants.ValidityEnum AreValid(PelvicExamEx exam0, PelvicExamEx exam1)
			{
				if (exam1.Time <= m_timeStartingExam)
				{
					// This pair is not useable, one of the elements is prior to the start exam.
					return Constants.ValidityEnum.Invalid;
				}

				if (exam0.DilatationValue > exam1.DilatationValue)
				{
					// Dilatations are descending - this invalidates LPM
					return Constants.ValidityEnum.InvalidatesLPM;
				}

				if (exam1.DilatationValue < Constants.C_MINIMUM_DILATATION_FOR_LPM)
				{
					// this pair is not usable, but does not invalidate LPM. As per Dr Hamilton.
					return Constants.ValidityEnum.Invalid;
				}

				///TODO Curve review contraction count
				if (exam1.GetContractionCount() < 1)
				{
					// this pair is not usable, but does not invalidate LPM
					return Constants.ValidityEnum.Invalid;
				}

				if (exam0.DilatationValue >= 10.0 && exam1.DilatationValue >= 10.0)
				{
					// Maximum Dilatation was reached - LPM is complete
					return Constants.ValidityEnum.Complete;
				}

				return Constants.ValidityEnum.Valid;
			}

			private long GetStartingExam()
			{
				///TODO Curve review how to get the first exam....
				return m_timeStartingExam;// this.FirstOrDefault().Time;
			}			

		}

		public partial class CurveEngine
		{
			/// <summary>
			/// Calculate the percentile and the expected dilation
			/// </summary>
			/// <param name="dilatation_0">Exan Dilatation value</param>
			/// <param name="effacement_0">Exam Effacement value</param>
			/// <param name="station_0">Exam Station value</param>
			/// <param name="dilatation">Next Exam Dilatation value</param>
			/// <param name="contractions">Number of contractions since first exam</param>
			/// <param name="epidural">Epidural (administered or not: 1|0 )</param>
			/// <param name="previous_vaginal">Previous vaginal deliveries</param>
			/// <param name="expectedDilatation">Expected Dilatation calculated (output value)</param>
			/// <param name="expectedPercentile">Expected Percentile calculated (output value)</param>
			public void CalculateExpectedDilatationAndPercentile(double dilatation_0, double effacement_0, double station_0, double dilatation, double contractions, double epidural, bool previous_vaginal, out double expectedDilatation, out double expectedPercentile)
			{
				expectedDilatation = -1;
				expectedPercentile = -1;

				bool primip = !previous_vaginal;

				double constant = primip ? Constants.LPM_CONSTANT_PRIM : Constants.LPM_CONSTANT_MULT;
				double dilatationFactor = primip ? Constants.LPM_DILATATION_FACTOR_PRIM : Constants.LPM_DILATATION_FACTOR_MULT;
				double effacementFactor = primip ? Constants.LPM_EFFACEMENT_FACTOR_PRIM : Constants.LPM_EFFACEMENT_FACTOR_MULT;
				double stationFactor = primip ? Constants.LPM_STATION_FACTOR_PRIM : Constants.LPM_STATION_FACTOR_MULT;
				double epiduralFactor = primip ? Constants.LPM_EPIDURAL_FACTOR_PRIM : Constants.LPM_EPIDURAL_FACTOR_MULT;
				double contractionFactor = primip ? Constants.LPM_CUMULATIVEUA_FACTOR_PRIM : Constants.LPM_CUMULATIVEUA_FACTOR_MULT;

				expectedDilatation = constant + (dilatation_0 * dilatationFactor) + (effacement_0 * effacementFactor) + (station_0 * stationFactor) + ((1 - epidural) * epiduralFactor) + ((double)contractions * contractionFactor);
				expectedPercentile = CalculatePercentile(expectedDilatation, dilatation, primip);
			}

			/// <summary>
			/// Calculate the percentile
			/// </summary>
			/// <param name="expectedDilatation">Expected dilatation calculated</param>
			/// <param name="dilatation">Next exam dilatation</param>
			/// <param name="primip"></param>
			/// <returns></returns>
			double CalculatePercentile(double expectedDilatation, double dilatation, bool primip)
			{
				double standardError = primip ? Constants.LPM_SE_PRIM : Constants.LPM_SE_MULT;
				double value = (dilatation - expectedDilatation) / standardError;

				if ((value < -4.0) || (value > 4.0))
				{	// check for out of bounds
					return 0.0;
				}

				return CalculateCumulativeNormalDistribution(value) * 100;
			}

			/// <summary>
			/// Standard normal cumulative distribution function
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			double CalculateCumulativeNormalDistribution(double value)
			{
				double result;
				if (value < -7)
				{
					result = CalculateNormalDistribution(value) / Math.Sqrt(1 + value * value);
				}
				else if (value > 7)
				{
					result = 1 - CalculateCumulativeNormalDistribution(-value);
				}
				else
				{
					result = 0.2316419;

					double[] a = new double[] { 0.31938153, -0.356563782, 1.781477937, -1.821255978, 1.330274429 };

					result = 1 / (1 + result * Math.Abs(value));
					result = 1 - CalculateNormalDistribution(value) * (result * (a[0] + result * (a[1] + result * (a[2] + result * (a[3] + result * a[4])))));
					if (value <= 0)
					{
						result = 1 - result;
					}
				}

				return result;
			}

			/// <summary>
			/// Standard normal density function
			/// </summary>
			/// <param name="t"></param>
			/// <returns></returns>
			double CalculateNormalDistribution(double t)
			{
				return 0.398942280401433 * Math.Exp(-t * t / 2);
			}
		}
	}
	
}

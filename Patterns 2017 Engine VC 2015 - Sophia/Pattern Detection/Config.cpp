/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection REVISION COMMNENTS:
* 07/Dec/2004 RM - Code review modifs on CConfig, CConfigFHR. Copyright LMS
* Medical Systems 2004 by Evonium Inc.
*/
#include "StdAFX.h"
#include "Config.h"
#include <io.h>
#include <fcntl.h>

namespace patterns
{

	//
	// =======================================================================================================================
	//    Definition of CConfig members. ;
	//    ! CConfig::<constructor>
	// =======================================================================================================================
	//
	CConfig::CConfig(void)
	{
	}

	//
	// =======================================================================================================================
	//    ! CConfig::<destructor>
	// =======================================================================================================================
	//
	CConfig::~CConfig(void)
	{
	}

	// Definition of CConfigFHR members. ;
	// ! CONSTANTS: Bounds for BVA checking of all configurable values related to the
	// FHR signal repair process.
	const double CConfigFHR::m_dMaxForMinAmplitude = 240.0;
	const double CConfigFHR::m_dMaxForMaxAmplitude = 240.0;
	const double CConfigFHR::m_dMaxForMaxDiffInSegPts = 50.0;
	const double CConfigFHR::m_dMaxForMaxDiffInArtifPts = 50.0;
	const double CConfigFHR::m_dMinForMinSlope = -10.0;
	const double CConfigFHR::m_dMaxForMaxSlope = 10.0;
	const double CConfigFHR::m_dMaxForMinDiffOverlapFHR = 120.0;

	const double CConfigFHR::m_dMaxForMaxTmWrongFHR = 240.0;
	const double CConfigFHR::m_dMaxForMaxTmMergeRepair = 120.0;
	const double CConfigFHR::m_dMaxForMaxTmCorrelRepair = 180.0;
	const double CConfigFHR::m_dMaxForMaxTmLookAhead = 600.0;
	const double CConfigFHR::m_dMinForTmShortFHR = 30.0;
	const double CConfigFHR::m_dMaxForTmShortFHR = 120.0;

	//
	// =======================================================================================================================
	//    ! CConfigFHR::<constructor>
	// =======================================================================================================================
	//
	CConfigFHR::CConfigFHR(void)
	{
		m_strResourceHandleModuleName = "";

		m_AllowConfiguration = true;

		m_PassedState = No_Raw_Data;

		m_dDefSmpFreq = 4.0;

		m_bShortAppend = false;
		m_lMinSamplesAppend = (long) (60 * m_dDefSmpFreq);
		m_lMaxSamplesAppend = 60 * 60 * 5 * 4; // equivalent of 5 hours at 4 Hz (72000 pts)

		m_bRemoveRepairCandidates = false;						// NN training
		m_bRemoveRepairOutput = true;

		// For removal of repaired accels and decels
		m_dMaxBumpRepOneSlope = 0.25;
		m_dMaxBumpRep2Slopes = 0.4;
		m_dMaxPercSlopeRep = 0.5;
		m_dMaxPercSlopeRep2 = 0.3;

		m_lMinRepLengthFilterVar = 5;							// in seconds - min length to take into account filter end effects when removing rep from filterVar

		m_lMinProlongedLenSec = 120;

		// ! Initialization of all members related to the FHR signal repair process.
		m_dMinAmplitude = 80.0;									// CHE - CHECK_NEED
		m_dMaxAmplitude = 190.0;								// CHE - CHECK_NEED

		m_dMaxDiffInSegPts = 20.0;								// CHE - CHECK_NEED
		m_dMaxDiffInArtifPts = 10.0;							// CHE - CHECK_NEED

		m_dMinSlope = -1.0;										// CHE - CHECK_NEED
		m_dMaxSlope = 1.0;										// CHE - CHECK_NEED

		m_dMinDiffOverlapFHR = 20.0;							// CHE - CHECK_NEED

		m_dMaxTmWrongFHR = 30.0;								// The following values are in seconds. They // CHE - CHECK_NEED
		m_dMaxTmMergeRepair = 60.0;								// should be left as such until the very begin- // CHE - CHECK_NEED
		m_dMaxTmCorrelRepair = 90.0;							// ning of the repair process, when the sampling // CHE - CHECK_NEED
		m_dMaxTmLookAhead = 30.0;								// frequency has its final and definite value. // CHE - CHECK_NEED

		m_dTmShortFHR = 60.0;									// CHE - CHECK_NEED

		m_DegeneratePrecision = 1e-9;							// CHE - CHECK_NEED

		m_bStoreXML = false;

		//m_bStoreXML = true;
		m_iStoreXML = 0;										// needs to be 2 for bump measure dump NN training needs to be 2
		m_iStoreMAT = 0;										// set to 1 to get bump cand arrays, 2 for band pass / ZC dumping NN training needs to be 1
		m_strXMLFilePrefix = "";
		m_bNNtrain = false;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// ! Initialization of all members related to the FIR filtering process.
		static double dFltArray[751] =							// 751 points array - delay = 375
#include "./filter/fir_filt_751.txt"
			m_iFir_Flt_Size = sizeof(dFltArray) / (sizeof(double));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_dFir_Flt = (double *) &dFltArray;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static double dFlt2Array[162] =							// 162 points array - delay = 81
#include "./filter/fir_filt2_162.txt"
			m_iFir_Flt2_Size = sizeof(dFlt2Array) / (sizeof(double));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_dFir_Flt2 = (double *) &dFlt2Array;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static double dFlt4Array[452] =							// 452 points array - delay = 226
#include "./filter/fir_filt4_452.txt"
			m_iFir_Flt4_Size = sizeof(dFlt4Array) / (sizeof(double));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_dFir_Flt4 = (double *) &dFlt4Array;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static double dFlt3Array[449] =							// 449 points array - delay = 224
#include "./filter/fir_filt3_449.txt"
			m_iFir_Flt3_Size = sizeof(dFlt3Array) / (sizeof(double));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_dFir_Flt3 = (double *) &dFlt3Array;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static double dFlt8Array[203] =							// 203 points array - delay = 101
#include "./filter/fir_filt8_203.txt"
			m_iFir_Flt8_Size = sizeof(dFlt8Array) / (sizeof(double));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_dFir_Flt8 = (double *) &dFlt8Array;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static double dFltLP25s[902] =							// 902 points array - delay = 451 (LP 25s - used for accel slope features - same as LP portion of band 1)
#include "./filter/fir_lp25s_1.txt"
			m_iFir_Flt_LP25_Size = sizeof(dFltLP25s) / (sizeof(double));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static double dFltHP25s[247] =							// 247 points array - delay = 123
#include "./filter/fir_hp25s.txt"
			m_iFir_Flt_HP25_Size = sizeof(dFltHP25s) / (sizeof(double));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_dFir_Flt_LP25 = (double *) &dFltLP25s;
		m_filterCoefsHP25.clear();
		for (int i = 0; i < 247; i++)
		{
			m_filterCoefsHP25.push_back(dFltHP25s[i]);
		}

		m_lLongestBPdelay = 0;									// longest BP filter delay in samples
		m_lMaxBumpCandLength = 0;								// maximum bump candidates length in samples

		// ! For external filter files.
		m_dFirFlt = NULL;
		m_d1FirFlt = NULL;
		m_d2FirFlt = NULL;

		m_iFirFlt_Size = 0;
		m_iExtBlockSize = 0;

		m_i1FirFlt_Size = 0;
		m_i1ExtBlockSize = 0;

		m_i2FirFlt_Size = 0;
		m_i2ExtBlockSize = 0;

		m_bUseOOURA = true;
		m_eFFTLibrary = eNONE;

		m_bUS_Standard = false;									// ! US (false) - International case (true).
		m_dFHR_Low = 30.0;										// ! Make modification for US standard
		m_dFHR_High = 210.0;

		m_bUseFileNNET_Multihypothesis = false;
		m_nNumberofExperts = 0;
		m_nnetExperts = NULL;

		m_nNumberofSecondaryExperts = 0;
		m_nnetSecondaryExperts = NULL;

		/*
		* ! The DLL will look in the resources for the topology and nnet experts Accel
		* and Decel Configurations.
		*/
		m_dMAXDELAY = 17.5;										// in minutes
		m_pBands = NULL;
		m_nnetAccelExperts = NULL;
		m_nnetDecelExperts = NULL;
		m_nnetAccelExperts2 = NULL;
		m_nnetDecelExperts2 = NULL;
		SetPathToSaveCandidateBumps(m_strRootPath + "realtime\\bumpCandidates\\");
		m_bUseFileNNET_AccelDecel = false;

		// ! Initialization of all members related to the Band filtering process.
		// m_iBands = 6;
		m_iBands = 4;
		if (m_iBands == 6)
		{
			m_iBandsAccel = m_iBands - 1;
		}
		else
		{
			m_iBandsAccel = m_iBands;
		}

		m_iBandsDecel = m_iBands;

		m_pBands = new CAccelDecelBandPassConfig[m_iBands];

		static double dFltIdent[1] =							// 1 points array - delay = 1
#include "./filter/fir_identity_1.txt"

		static double dFltHP40s[393] =					// 393 points array - delay = 197
#include "./filter/fir_hp40s_1.txt"

		static double dFltHP60s[589] =					// 589 points array - delay = 294
#include "./filter/fir_hp60s_1.txt"

		static double dFltLP40s[602] =					// 602 points array - delay = 301
#include "./filter/fir_lp40s_1.txt"
		
		static double dFltBP90_60s[1082] =				// 1082 points array - delay = 541
#include "./filter/bp3_1081.txt"

		static double dFltBP200_90s[1840] =				// 1840 points array - delay = 920
#include "./filter/bp4_1839b.txt"

		static double dFltBP500_200s[3876] =			// 3876 points array - delay = 1938
#include "./filter/fir_bp500_200s_1.txt"

		static double dFltBP660_400s[4559] =			// 4559 points array - delay = 2279
#include "./filter/fir_bp660_400s_1.txt"

		CAccelDecelBandPassConfig * cpACBPC;
		for (int i = 0; i < m_iBands; i++)
		{
			cpACBPC = &(m_pBands[i]);

			// Populate m_pBands
			if (i == 0)
			{
				cpACBPC->SetLowPassFilterLength(sizeof(dFltLP25s) / (sizeof(double)));
				cpACBPC->SetLowPassFilter((double *) &dFltLP25s);

				cpACBPC->SetHighPassFilterLength(sizeof(dFltHP40s) / (sizeof(double)));
				cpACBPC->SetHighPassFilter((double *) &dFltHP40s);

				cpACBPC->SetAccelMinBumpArea(250);
				cpACBPC->SetDecelMinBumpArea(250);

				cpACBPC->SetAccelMaxBumpCandLength(4.25 * 60);	// in seconds
				cpACBPC->SetDecelMaxBumpCandLength(7.5 * 60);	// in seconds
				cpACBPC->SetPathToSaveBandPass(m_strRootPath + "BandPass_1");
				cpACBPC->SetPathToSaveZeroCrossings(m_strRootPath + "ZC_1");
			}
			else if (i == 1)
			{
				cpACBPC->SetLowPassFilterLength(sizeof(dFltLP40s) / (sizeof(double)));

				cpACBPC->SetLowPassFilter((double *) &dFltLP40s);

				cpACBPC->SetHighPassFilterLength(sizeof(dFltHP60s) / (sizeof(double)));

				cpACBPC->SetHighPassFilter((double *) &dFltHP60s);

				// cpACBPC->SetAccelMinBumpArea(250);
				cpACBPC->SetAccelMinBumpArea(375);
				cpACBPC->SetDecelMinBumpArea(250);
				cpACBPC->SetAccelMaxBumpCandLength(4.25 * 60);	// in seconds
				cpACBPC->SetDecelMaxBumpCandLength(7.5 * 60);	// in seconds
				cpACBPC->SetPathToSaveBandPass(m_strRootPath + "BandPass_2");
				cpACBPC->SetPathToSaveZeroCrossings(m_strRootPath + "ZC_2");
			}
			else if (i == 2)
			{
				cpACBPC->SetLowPassFilterLength(sizeof(dFltBP90_60s) / (sizeof(double)));
				
				cpACBPC->SetLowPassFilter((double *) &dFltBP90_60s);

				cpACBPC->SetHighPassFilterLength(sizeof(dFltIdent) / (sizeof(double)));
				cpACBPC->SetHighPassFilter((double *) &dFltIdent);

				// cpACBPC->SetAccelMinBumpArea(250);
				// cpACBPC->SetDecelMinBumpArea(250);
				cpACBPC->SetAccelMinBumpArea(500);
				cpACBPC->SetDecelMinBumpArea(300);
				cpACBPC->SetAccelMaxBumpCandLength(4.25 * 60);	// in seconds
				cpACBPC->SetDecelMaxBumpCandLength(7.5 * 60);	// in seconds
				cpACBPC->SetPathToSaveBandPass(m_strRootPath + "BandPass_3");
				cpACBPC->SetPathToSaveZeroCrossings(m_strRootPath + "ZC_3");
			}
			else if (i == 3)
			{
				cpACBPC->SetLowPassFilterLength(sizeof(dFltBP200_90s) / (sizeof(double)));

				cpACBPC->SetLowPassFilter((double *) &dFltBP200_90s);

				cpACBPC->SetHighPassFilterLength(sizeof(dFltIdent) / (sizeof(double)));
				cpACBPC->SetHighPassFilter((double *) &dFltIdent);

				// cpACBPC->SetAccelMinBumpArea(250);
				// cpACBPC->SetDecelMinBumpArea(250);
				cpACBPC->SetAccelMinBumpArea(1000);
				cpACBPC->SetDecelMinBumpArea(500);
				cpACBPC->SetAccelMaxBumpCandLength(4.25 * 60);	// in seconds
				cpACBPC->SetDecelMaxBumpCandLength(7.5 * 60);	// in seconds
				cpACBPC->SetPathToSaveBandPass(m_strRootPath + "BandPass_4");
				cpACBPC->SetPathToSaveZeroCrossings(m_strRootPath + "ZC_4");
			}
			else if (i == 4)
			{
				cpACBPC->SetLowPassFilterLength(sizeof(dFltBP500_200s) / (sizeof(double)));

				cpACBPC->SetLowPassFilter((double *) &dFltBP500_200s);

				cpACBPC->SetHighPassFilterLength(sizeof(dFltIdent) / (sizeof(double)));
				cpACBPC->SetHighPassFilter((double *) &dFltIdent);

				// cpACBPC->SetAccelMinBumpArea(250);
				// cpACBPC->SetDecelMinBumpArea(250);
				cpACBPC->SetAccelMinBumpArea(3800);
				cpACBPC->SetDecelMinBumpArea(2000);
				cpACBPC->SetAccelMaxBumpCandLength(4.25 * 60);	// in seconds
				cpACBPC->SetDecelMaxBumpCandLength(7.5 * 60);	// in seconds
				cpACBPC->SetPathToSaveBandPass(m_strRootPath + "BandPass_5");
				cpACBPC->SetPathToSaveZeroCrossings(m_strRootPath + "ZC_5");
			}
			else if (i == 5)
			{
				cpACBPC->SetLowPassFilterLength(sizeof(dFltBP660_400s) / (sizeof(double)));
				cpACBPC->SetLowPassFilter((double *) &dFltBP660_400s);

				cpACBPC->SetHighPassFilterLength(sizeof(dFltIdent) / (sizeof(double)));
				cpACBPC->SetHighPassFilter((double *) &dFltIdent);

				// cpACBPC->SetAccelMinBumpArea(250);
				// cpACBPC->SetDecelMinBumpArea(250);
				cpACBPC->SetAccelMinBumpArea(250);				// band 6 accels are not even considered
				cpACBPC->SetDecelMinBumpArea(2000);
				cpACBPC->SetAccelMaxBumpCandLength(4.25 * 60);	// in seconds
				cpACBPC->SetDecelMaxBumpCandLength(7.5 * 60);	// in seconds
				cpACBPC->SetPathToSaveBandPass(m_strRootPath + "BandPass_6");
				cpACBPC->SetPathToSaveZeroCrossings(m_strRootPath + "ZC_6");
			}
		}

		SetLongestBandPassDelay();						// set longest delay based on loaded filters
		SetLongestBumpCandLength();						// set longest bump cand length based on band pass params

		SetMinBumpCandLength(10.0);						// 10 seconds

		// COLM - cannot set max delay based on filters until fix multiH and accelDecel
		// such that can have -ve indexed baselines
		SetMaxDelayBasedOnBPFilters();					// this sets MAX_DELAY (and thus required signal history and window size)
		UsePendingRT(true);
		SetNumAccelDecelIterations(2);
		
		m_lMinVarLookAhead = 30; // in seconds

		// based on specified filters and max bump candidate length
		m_dBaseLineHistoryKeep = 60.0;					// in minutes - history to keep for multiH/AccelDecel classification
		m_dAccelDecelHistoryKeep = 30.0;				// in minutes - history to keep for merging
		m_dAccExtBuffer = 60.0;							// in seconds
		m_dEarliestEventBegin = 2.0;					// in minutes - earliest start time of event at very beginning of tracing

		m_bUseFileNNET_AccelDecel = false;
		SetPathToSaveCandidateBumps(m_strRootPath + "BumpCandidates");
		SetPathToSaveBumpsClassify(m_strRootPath + "BumpClassify");

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static NNStats AccelStats[21] =					// Array of 20 double pairs
			// include "./AccelNN/accelexpert_stats_20040520T132102_1.txt" #include
			// "./AccelNN/normStatsAccel_20070214T210127.txt"
//#include "./AccelNN/normStatsAccel_20071219T220514.txt"
#include "./AccelNN/normStatsAccel_20080826T000954.txt"
			m_nAccelStats = (sizeof(AccelStats) / (sizeof(NNStats)));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_AccelStats = (NNStats *) &AccelStats;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		static NNStats DecelStats[20] =					// Array of 20 double pairs
			// include "./DecelNN/decelexpert_stats_20040520T132102_1.txt" #include
			// "./DecelNN/normStatsDecel_20070214T210127.txt"
//#include "./DecelNN/normStatsDecel_20071214T152230.txt"
//#include "./DecelNN/normStatsDecel_20080809T163214.txt"
#include "./DecelNN/normStatsDecel_20080823T101415.txt"
			m_nDecelStats = (sizeof(DecelStats) / (sizeof(NNStats)));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_DecelStats = (NNStats *) &DecelStats;
		m_bReplaceBumps = true;

		m_LENGTH_THRESHOLD = 15 * (long) m_dDefSmpFreq; // Min length of Output Object 10 sec
		m_MIN_BAS_LENGTH = 20 * (long) m_dDefSmpFreq;	// Min length for baseline
		m_MIN_BAS_REP_LENGTH = 10 * (long) m_dDefSmpFreq;	// Min length of repair to cause truncation of basleine
		m_LATE_THRESH = 20 * (long) m_dDefSmpFreq;
		m_BEFORE_CONTRACT_THRESH = 10 * (long) m_dDefSmpFreq;
		m_PEAK_PHASE_THRESHOLD = 10 * (long) m_dDefSmpFreq;

		m_MAX_TIME_AFTER_CONTRACTION = 20 * (long) m_dDefSmpFreq;
		m_MAX_TIME_BEFORE_CONTRACTION = 20 * (long) m_dDefSmpFreq;

		m_lDecelRateWindow = 30 * 60 * (long) m_dDefSmpFreq;
		m_lMaxSurrDecelDist = 2 * 60 * (long) m_dDefSmpFreq;

		// m_bRemove1_BL = true;
		// m_bRemoveL_BL = true;
		// m_bRemoveN_BL = true;
		m_bRemove1_BL = false;						// CHE - CHECK_NEED
		m_bRemoveL_BL = false;						// CHE - CHECK_NEED
		m_bRemoveN_BL = false;						// CHE - CHECK_NEED
		m_bMH_Simpl = false;						// CHE - CHECK_NEED
		m_bExcludeLastPositive = true;				// CHE - CHECK_NEED
		m_bNotAllowEmptyContractionAppend = false;	// CHE - CHECK_NEED

		m_bMergeBumpsWithAllBasCand = true;
		m_bDisableRepair = false;					// NN training
		m_dRepairBuffer = 30.0;
		m_dMaxRepairBeforeCommit = 60.0;			// amoutn (in sec.) of repair at end of window before can force commit (consider as long dropout)
		m_dMaxPercRepairForOutput = 0.85;				// maximum percentage repair on events for it to be output at all
		m_lMaxRepairIgnore = 0; // number of samples of consecutive repair that can be ignored for the purposes of determining non-interpretable events

		m_bExtendAccelCandidates = true;			// NN training
		m_bUseAvgBasLevelForYvals = true;			// this is only for accel/decel classification NN training

		// For changing the confidence thresholds in Decel classification
		m_bUseMaxConfidenceDecel = false;			// 'true' for original way
		
		// IF USING 2 ITERATION FAVOUR SENSITIVITY IN FIRST ITERATION

		// IF USING ONLY 1 ITERATION - A = 1.0
		m_dMaxNonDecConfForLate_oneIter = 0.24;				// max nonDecel confidence for a late gradual to be true (if m_bUseMaxConfidenceDecel == false)
		m_dMaxNonDecConfForEarly_oneIter = 0.21;			// for early
		m_dMaxNonDecConfForNonAssoc_oneIter = 0.15;			// for non-associated gradual and indeterminate
		m_dMaxNonDecConfForAbrupt_oneIter = 0.33;

		// IF USING 2 ITERATIONS - A = 1.2
		m_dMaxNonDecConfForLate[0] = 0.46;				// max nonDecel confidence for a late gradual to be true (if m_bUseMaxConfidenceDecel == false)
		m_dMaxNonDecConfForEarly[0] = 0.28;			// for early
		m_dMaxNonDecConfForNonAssoc[0] = 0.23;			// for non-associated gradual and indeterminate
		m_dMaxNonDecConfForAbrupt[0] = 0.46;

		m_dMaxNonDecConfForLate[1] = 0.26;				// max nonDecel confidence for a late gradual to be true (if m_bUseMaxConfidenceDecel == false)
		m_dMaxNonDecConfForEarly[1] = 0.20;			// for early
		m_dMaxNonDecConfForNonAssoc[1] = 0.14;			// for non-associated gradual and indeterminate
		m_dMaxNonDecConfForAbrupt[1] = 0.36;

		m_bUseMaxConfidenceAccel = false;			// true for original way
		m_dMinAccConf[0] = 0.40;
		m_dMinAccConf[1] = 0.52;
		m_dMinAccConf_oneIter = 0.54;

		m_bMapOutputConfidence = true;

		m_dMinAccelCandHeight = 15.0;
		m_dMinAccelCandMeanHeight = 0.0;
		m_dMinAbruptDecelHeight = 15.0;

		m_lBasTrimForVarCalcSec = 5;
		m_lBasSegLenForVarCalcSec = 30;
		m_bConsiderSlopeForBasVar = true;
		m_bCalcBasVar = true;
	}

	//
	// =======================================================================================================================
	//    ! CConfigFHR::<destructor>
	// =======================================================================================================================
	//
	CConfigFHR::~CConfigFHR(void)
	{
		if (m_dFirFlt != NULL)
		{
			delete m_dFirFlt;
			m_dFirFlt = NULL;
		}

		if (m_d1FirFlt != NULL)
		{
			delete m_d1FirFlt;
			m_d1FirFlt = NULL;
		}

		if (m_d2FirFlt != NULL)
		{
			delete m_d2FirFlt;
			m_d2FirFlt = NULL;
		}

		if (m_pBands != NULL)
		{
			delete[] m_pBands;
			m_pBands = NULL;
		}

		if (m_nnetAccelExperts != NULL)
		{
			delete[] m_nnetAccelExperts;
			m_nnetAccelExperts = NULL;
		}

		if (m_nnetDecelExperts != NULL)
		{
			delete[] m_nnetDecelExperts;
			m_nnetDecelExperts = NULL;
		}
		
		if (m_nnetAccelExperts2 != NULL)
		{
			delete [] m_nnetAccelExperts2;
			m_nnetAccelExperts2 = NULL;
		}
		if (m_nnetDecelExperts2 != NULL)
		{
			delete [] m_nnetDecelExperts2;
			m_nnetDecelExperts2 = NULL;
		}

		if (m_nnetExperts != NULL)
		{
			delete[] m_nnetExperts;
			m_nnetExperts = NULL;
		}

		if (m_nnetSecondaryExperts != NULL)
		{
			delete[] m_nnetSecondaryExperts;
			m_nnetSecondaryExperts = NULL;
		}
	}

	/*
	=======================================================================================================================
	! CConfigFHR::ReadINIFile: Sets the CConfig class with values read from an *.ini file whose name is specified.
	\param filename The name of the file to read from. \return A value, taken from the enum type ConfigErrorStatus,
	indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::ReadINIFile(string filename)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int bRet = CES_OK;
		int fh;
		string strPath = filename;
		size_t iLen = strPath.length();
		size_t iPos = strPath.find_last_of('\\');
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		strPath = strPath.substr(0, iPos);
		iPos = strPath.find_last_of('\\');

		strPath = strPath.substr(0, iPos + 1);

		fh = _open(filename.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return Wrong_INInotFound;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Read in input:
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = Wrong_INIReadError;
		}
		else
		{
			/*~~~~~~~~~~~*/
			// put the buffer in a string object
			string strData;
			/*~~~~~~~~~~~*/

			strData.replace(0, bytesread, buffer);
			strData.resize(bytesread, ' ');

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// parse and set strData
			iLen = strData.length();
			iPos = strData.find('\n', 0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (iPos == string::npos)
			{
				return Wrong_INIReadError;
			}

			/*~~~~~~~~~~~*/
			int nCount = 0;
			/*~~~~~~~~~~~*/

			while (iLen > 0)					// (iPos != string::npos) // this is the not found string case
			{
				/*~~~~~~~~~~~*/
				string strLine;
				/*~~~~~~~~~~~*/

				strLine = strData.substr(0, iPos);

				// trim strLine of left and right spaces and any conrol characters \t \r \n
				for (int i = (int) strLine.length(); i >= 0; i--)
				{
					if (strLine[i] == '\n')
					{
						strLine.erase(i, 1);
					}
				}

				for (int i = (int) strLine.length(); i >= 0; i--)
				{
					if (strLine[i] == '\t')
					{
						strLine.erase(i, 1);
					}
				}

				for (int i = (int) strLine.length(); i >= 0; i--)
				{
					if (strLine[i] == '\r')
					{
						strLine.erase(i, 1);
					}
				}

				// strLine.replace(0,'\n',"");
				// strLine.replace(0,'\t'," ");
				// //tabs with space strLine.replace(0,'\r',"");
				// ;
				// trim spaces from start and en
				while (strLine[0] == ' ')
				{
					strLine.erase(0, 1);
				}

				if ((int) strLine.length() > 0) // first trims all and leaves empty line
				{
					while (strLine[(int) strLine.length() - 1] == ' ')
					{
						strLine.erase((int) strLine.length() - 1, 1);
					}
				}

				if (strLine.find("[", 0) == 0 || strLine == "")
				{
					// grouping or empty -> line skip
				}
				else if (strLine.find(";", 0) == 0)
				{
					// comment -> line skip
				}
				else if (strLine.find("SAMPLE_FHR_FREQ", 0) == 0)	// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					int nSize = (int) string("SAMPLE_FHR_FREQ").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					while (strLine[0] == ' ')
					{	// trim the white space
						strLine.erase(0, 1);
					}

					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());
						hr = SetDefSmpFreq(dValue);
					}
				}
				else if (strLine.find("MIN_FHR_VAL", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					int nSize = (int) string("MIN_FHR_VAL").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~*/
						double dMax;
						/*~~~~~~~~*/

						dMax = GetMaxAmplitude();
						hr = SetMinMaxAmplitudes(dValue, dMax);

						// hr = SetDefSmpFreq(dValue);
					}
				}
				else if (strLine.find("MAX_FHR_VAL", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					int nSize = (int) string("MAX_FHR_VAL").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~*/
						double dMin;
						/*~~~~~~~~*/

						dMin = GetMinAmplitude();
						hr = SetMinMaxAmplitudes(dMin, dValue);

						// hr = SetDefSmpFreq(dValue);
					}
				}
				else if (strLine.find("MAX_NEG_SLOPE", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetMinMaxSlopes(dMinSlope, dMaxSlope);
					int nSize = (int) string("MAX_NEG_SLOPE").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~*/
						double dMax;
						/*~~~~~~~~*/

						dMax = GetMaxSlope();
						hr = SetMinMaxSlopes(dValue, dMax);

						// hr = SetDefSmpFreq(dValue);
					}
				}
				else if (strLine.find("MAX_POS_SLOPE", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					int nSize = (int) string("MAX_POS_SLOPE").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~*/
						double dMin;
						/*~~~~~~~~*/

						dMin = GetMinSlope();
						hr = SetMinMaxSlopes(dMin, dValue);

						// hr = SetDefSmpFreq(dValue);
					}
				}
				else if (strLine.find("FHR_LEN_MAX", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					int nSize = (int) string("FHR_LEN_MAX").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());
						hr = SetMaxTmWrongFHR(dValue);
					}
				}
				else if (strLine.find("MIN_FHR_DIFF", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetMinDiffOverlapFHR(dMinDiffOverlapFHR);
					int nSize = (int) string("MIN_FHR_DIFF").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());
						hr = SetMinDiffOverlapFHR(dValue);
					}
				}
				else if (strLine.find("MAX_MERGE_REPAIR", 0) == 0)		// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetMaxTmMergeRepair(dMaxTmMergeRepair / dSmpFreq);
					int nSize = (int) string("MAX_MERGE_REPAIR").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						double dSmpFreq = GetDefSmpFreq();
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						hr = SetMaxTmMergeRepair(dValue / dSmpFreq);
					}
				}
				else if (strLine.find("MAX_CORRELATED_REPAIR", 0) == 0) // != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetMaxTmCorrelRepair(dMaxTmCorrelRepair / dSmpFreq);
					int nSize = (int) string("MAX_CORRELATED_REPAIR").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						double dSmpFreq = GetDefSmpFreq();
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						hr = SetMaxTmCorrelRepair(dValue / dSmpFreq);
					}
				}
				else if (strLine.find("MAX_LOOKAHEAD", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetMaxTmLookAhead(dMaxTmLookAhead / dSmpFreq);
					int nSize = (int) string("MAX_LOOKAHEAD").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						double dSmpFreq = GetDefSmpFreq();
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						hr = SetMaxTmLookAhead(dValue / dSmpFreq);
					}
				}
				else if (strLine.find("DIFF_MAX_IN", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetMaxDiffInSegPts(dMaxDiffInSegPts, dMaxDiffInArtifPts);
					int nSize = (int) string("DIFF_MAX_IN").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~*/
						double dMax;
						/*~~~~~~~~*/

						dMax = GetMaxDiffInArtifPts();
						hr = SetMaxDiffInSegPts(dValue, dMax);
					}
				}
				else if (strLine.find("DIFF_MAX_OUT", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					int nSize = (int) string("DIFF_MAX_OUT").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~*/
						double dMax;
						/*~~~~~~~~*/

						dMax = GetMaxDiffInSegPts();
						hr = SetMaxDiffInSegPts(dMax, dValue);
					}
				}
				else if (strLine.find("SHORT_FHR_LEN", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetTmShortFHR(dTmShortFHR / dSmpFreq);
					int nSize = (int) string("SHORT_FHR_LEN").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						double dSmpFreq = GetDefSmpFreq();
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						hr = SetTmShortFHR(dValue / dSmpFreq);
					}
				}
				else if (strLine.find("BLOCKSIZE", 0) == 0)				// != string::npos)
				{
					// {optimal,decreased,increased,super increased}
				}
				else if (strLine.find("DEGENERATION", 0) == 0)			// != string::npos)
				{
				}
				else if (strLine.find("LOWPASSFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename ReadLowPassFilterFile(string filename);
					int nSize = (int) string("LOWPASSFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						ReadLowPassFilterFile(strLine);
					}
				}
				else if (strLine.find("1STVARPASSFILTER", 0) == 0)		// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename Read1stVarPassFilterFile(string filename);
					int nSize = (int) string("1STVARPASSFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						Read1stVarPassFilterFile(strLine);
					}
				}
				else if (strLine.find("2NDVARPASSFILTER", 0) == 0)		// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename Read2ndVarPassFilterFile(string filename);
					int nSize = (int) string("2NDVARPASSFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						Read2ndVarPassFilterFile(strLine);
					}
				}
				else if (strLine.find("USEOOURA", 0) == 0)				// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// {Y,N}
					int nSize = (int) string("USEOOURA").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						if (strLine.find("N", 0) == string::npos)
						{
							m_bUseOOURA = false;
						}
						else
						{
							m_bUseOOURA = true;
						}
					}
				}
				else if (strLine.find("STOREXML", 0) == 0)				// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// {Y,N}
					int nSize = (int) string("STOREXML").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						if (strLine.find("Y", 0) == string::npos)
						{
							m_bStoreXML = true;
						}
						else
						{
							m_bStoreXML = false;
						}
					}
				}
				else if (strLine.find("PRIMARYNNET", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename UseFileNNET_MultiHypothesis(true);
					// SetPrimaryNNETFile(filename);
					int nSize = (int) string("PRIMARYNNET").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						UseFileNNET_MultiHypothesis(true);
						SetPrimaryNNETFile(strLine);
					}
				}
				else if (strLine.find("SECONDARYNNET", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("SECONDARYNNET").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						UseFileNNET_MultiHypothesis(true);
						SetSecondaryNNETFile(strLine);
					}
				}
				else if (strLine.find("ACCELNNET", 0) == 0)				// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("ACCELNNET").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						UseFileNNET_AccelDecels(true);
						SetAccelNNETFile(strLine);
					}
				}
				else if (strLine.find("DECELNNET", 0) == 0)				// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("DECELNNET").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						UseFileNNET_AccelDecels(true);
						SetDecelNNETFile(strLine);
					}
				}
				else if (strLine.find("MAXDELAY", 0) == 0)				// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// hr = m_pCfgFHR->SetTmShortFHR(dTmShortFHR / dSmpFreq);
					int nSize = (int) string("MAXDELAY").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						/*~~~~~~~~~~~~~~*/
						long hr;
						double dValue = 0;
						/*~~~~~~~~~~~~~~*/

						dValue = atof(strLine.c_str());
						hr = SetMaxDelay(dValue);
					}
				}
				else if (strLine.find("F1LOWFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F1LOWFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandLowFilterFile(0, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F2LOWFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F2LOWFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandLowFilterFile(1, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F3LOWFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F3LOWFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandLowFilterFile(2, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F4LOWFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F4LOWFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandLowFilterFile(3, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F5LOWFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F5LOWFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandLowFilterFile(4, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F6LOWFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F6LOWFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandLowFilterFile(5, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F1HIGHFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F1HIGHFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandHighFilterFile(0, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F2HIGHFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F2HIGHFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandHighFilterFile(1, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F3HIGHFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F3HIGHFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandHighFilterFile(2, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F4HIGHFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F4HIGHFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandHighFilterFile(3, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F5HIGHFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F5HIGHFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandHighFilterFile(4, strLine);
						SetLongestBandPassDelay();
					}
				}
				else if (strLine.find("F6HIGHFILTER", 0) == 0)			// != string::npos)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// = [STRING] <- filename
					int nSize = (int) string("F6HIGHFILTER").length();
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					for (int i = 0; i < nSize; i++)
					{
						strLine.erase(0, 1);
					}

					// now find the equals string
					if (strLine.find("=", 0) == string::npos)
					{
						return Wrong_INIParameter;
					}
					else
					{
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						strLine.replace(0, 1, "");						// remove the "="

						/* trim the string */
						while (strLine[0] == ' ')
						{
							strLine.erase(0, 1);
						}

						if ((int) strLine.length() > 0)					// first trims all and leaves empty line
						{
							while (strLine[(int) strLine.length() - 1] == ' ')
							{
								strLine.erase((int) strLine.length() - 1, 1);
							}
						}

						if (strLine.find(";", 0) != string::npos)
						{
							return Wrong_INIParameter;
						}

						this->SetFBandHighFilterFile(5, strLine);
						SetLongestBandPassDelay();
					}
				}
				else
				{
					// ignore everything else
				}

				// each line is a expert file name
				if (iPos == iLen)
				{
					strData = "";
				}
				else
				{
					strData = strData.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strData.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strData.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strData.length();
					}
				}

				iLen = strData.length();
			}

			/*
			* BLOCKSIZE = {optimal,decreased,increased,super increased} if (((CButton*)
			* GetDlgItem(IDC_RADIO1))->GetCheck() == 1) m_BlocksizeAdjustment = 1;
			* //optimal else if (((CButton*) GetDlgItem(IDC_RADIO2))->GetCheck() == 1)
			* m_BlocksizeAdjustment = -1;
			* //decreased else if (((CButton*) GetDlgItem(IDC_RADIO3))->GetCheck() == 1)
			* m_BlocksizeAdjustment = 2;
			* //increased else if (((CButton*) GetDlgItem(IDC_RADIO4))->GetCheck() == 1)
			* m_BlocksizeAdjustment = 3;
			* //super increased >>bRead_MH_XML = (((CButton*)
			* GetDlgItem(IDC_CHECK19))->GetCheck() > 0);
			* DEGENERATION = [FLOAT] CString sDeg;
			* GetDlgItem(IDC_EDIT15)->GetWindowText(sDeg);
			* m_DegeneratePrecision = atof(sDeg);
			* bRemove1_BL = (((CButton*) GetDlgItem(IDC_CHECK18))->GetCheck() > 0);
			* bRemoveL_BL = (((CButton*) GetDlgItem(IDC_CHECK20))->GetCheck() > 0);
			* bRemoveN_BL = (((CButton*) GetDlgItem(IDC_CHECK21))->GetCheck() > 0);
			* bMH_Simpl = (((CButton*) GetDlgItem(IDC_CHECK22))->GetCheck() > 0);
			* bBL_UnitTest = (((CButton*) GetDlgItem(IDC_CHECK23))->GetCheck() > 0);
			* GetDlgItem(IDC_EDIT16)->GetWindowText(sDeg);
			* m_InitDataPos = atol(sDeg);
			* GetDlgItem(IDC_EDIT17)->GetWindowText(sDeg);
			* m_InitDataSize = atol(sDeg);
			* ??WHAT ELSE?? ;
			* Wrong_INIParameter
			*/
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::ReadLowPassFilterFile: Sets the CConfig class with values read from a Low pass filter file whose name
	is provided. \param filename The name of the file to read from. \return A value, taken from the enum type
	ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	long CConfigFHR::ReadLowPassFilterFile(string filename)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		/*~~~~~~~~~~~~~~*/
		// m_pCfgFHR->m_dFirFlt,m_pCfgFHR->m_iFirFlt_Size double* dFilterData, int&
		// lFilter, CString& strExtr
		long iFilter(0);
		int bRet = CES_OK;
		int fh;
		/*~~~~~~~~~~~~~~*/

		fh = _open(filename.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return Wrong_FileNotFound;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Read in input:
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = Wrong_FileRead;
		}
		else
		{
			/*~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strData;
			// put the buffer in a string object
			string strParse;
			/*~~~~~~~~~~~~*/

			strData.replace(0, bytesread, buffer);
			strData.resize(bytesread, ' ');

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			size_t iLen = strData.length();
			size_t iPos = strData.find('\n', 0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (iPos == string::npos)
			{
				return Wrong_FileRead;
			}

			strParse = strData;

			/*~~~~~~~~~~~~~~*/
			string strLine;
			int nCount = 0;
			double dFilter(0);
			/*~~~~~~~~~~~~~~*/

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strData.substr(0, iPos);
				iFilter++;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strData = "";
				}
				else
				{
					strData = strData.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strData.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strData.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strData.length();
					}
				}

				iLen = strData.length();
			}

			if (m_dFirFlt)
			{
				delete[] m_dFirFlt;
			}

			m_dFirFlt = new double[iFilter];
			m_iFirFlt_Size = iFilter;

			/*~~~~~~~~*/
			int iAr = 0;
			/*~~~~~~~~*/

			iLen = strParse.length();
			iPos = strParse.find('\n', 0);
			if (iPos == string::npos)
			{
				return Wrong_FileRead;
			}

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strParse.substr(0, iPos);
				dFilter = atof(strLine.c_str());
				m_dFirFlt[iAr] = dFilter;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strParse = "";
				}
				else
				{
					strParse = strParse.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strParse.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strParse.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strParse.length();
					}
				}

				iLen = strParse.length();
				iAr++;
			}
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::Read1stVarPassFilterFile: Sets the CConfig class with values read from a first var pass filter file
	whose name is provided. \param filename The name of the file to read from. \return A value, taken from the enum
	type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	long CConfigFHR::Read1stVarPassFilterFile(string filename)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		/*~~~~~~~~~~~~~~*/
		// m_pCfgFHR->m_d1FirFlt,m_pCfgFHR->m_i1FirFlt_Size
		long iFilter(0);
		int bRet = CES_OK;
		int fh;
		/*~~~~~~~~~~~~~~*/

		fh = ::_open(filename.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return Wrong_FileNotFound;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Read in input:
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = Wrong_FileRead;
		}
		else
		{
			/*~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strData;
			// put the buffer in a string object
			string strParse;
			/*~~~~~~~~~~~~*/

			strData.replace(0, bytesread, buffer);
			strData.resize(bytesread, ' ');

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			size_t iLen = strData.length();
			size_t iPos = strData.find('\n', 0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (iPos == string::npos)
			{
				return Wrong_FileRead;
			}

			strParse = strData;

			/*~~~~~~~~~~~~~~*/
			string strLine;
			int nCount = 0;
			double dFilter(0);
			/*~~~~~~~~~~~~~~*/

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strData.substr(0, iPos);
				iFilter++;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strData = "";
				}
				else
				{
					strData = strData.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strData.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strData.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strData.length();
					}
				}

				iLen = strData.length();
			}

			if (m_d1FirFlt)
			{
				delete[] m_d1FirFlt;
			}

			m_d1FirFlt = new double[iFilter];
			m_i1FirFlt_Size = iFilter;

			/*~~~~~~~~*/
			int iAr = 0;
			/*~~~~~~~~*/

			iLen = strParse.length();
			iPos = strParse.find('\n', 0);
			if (iPos == string::npos)
			{
				return Wrong_FileRead;
			}

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strParse.substr(0, iPos);
				dFilter = atof(strLine.c_str());
				m_d1FirFlt[iAr] = dFilter;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strParse = "";
				}
				else
				{
					strParse = strParse.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strParse.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strParse.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strParse.length();
					}
				}

				iLen = strParse.length();
				iAr++;
			}
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::Read2ndVarPassFilterFile: Sets the CConfig class with values read from a second var pass filter file
	whose name is provided. \param filename The name of the file to read from. \return A value, taken from the enum
	type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	long CConfigFHR::Read2ndVarPassFilterFile(string filename)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		/*~~~~~~~~~~~~~~*/
		// m_pCfgFHR->m_d2FirFlt, m_pCfgFHR->m_i2FirFlt_Size
		long iFilter(0);
		int bRet = CES_OK;
		int fh;
		/*~~~~~~~~~~~~~~*/

		fh = _open(filename.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return Wrong_FileNotFound;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Read in input:
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = Wrong_FileRead;
		}
		else
		{
			/*~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strData;
			// put the buffer in a string object
			string strParse;
			/*~~~~~~~~~~~~*/

			strData.replace(0, bytesread, buffer);
			strData.resize(bytesread, ' ');

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			size_t iLen = strData.length();
			size_t iPos = strData.find('\n', 0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (iPos == string::npos)
			{
				return Wrong_FileRead;
			}

			strParse = strData;

			/*~~~~~~~~~~~~~~*/
			string strLine;
			int nCount = 0;
			double dFilter(0);
			/*~~~~~~~~~~~~~~*/

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strData.substr(0, iPos);
				iFilter++;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strData = "";
				}
				else
				{
					strData = strData.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strData.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strData.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strData.length();
					}
				}

				iLen = strData.length();
			}

			if (m_d2FirFlt)
			{
				delete[] m_d2FirFlt;
			}

			m_d2FirFlt = new double[iFilter];
			m_i2FirFlt_Size = iFilter;

			/*~~~~~~~~*/
			int iAr = 0;
			/*~~~~~~~~*/

			iLen = strParse.length();
			iPos = strParse.find('\n', 0);
			if (iPos == string::npos)
			{
				return Wrong_FileRead;
			}

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strParse.substr(0, iPos);
				dFilter = atof(strLine.c_str());
				m_d2FirFlt[iAr] = dFilter;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strParse = "";
				}
				else
				{
					strParse = strParse.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strParse.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strParse.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strParse.length();
					}
				}

				iLen = strParse.length();
				iAr++;
			}
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetDefSmpFreq: Sets the default sampling frequency. \param dDefSmpFreq New default sampling
	frequency. \return A value, taken from the enum type ConfigErrorStatus, indicating either succes or the error that
	happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetDefSmpFreq(double dDefSmpFreq)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if (dDefSmpFreq <= 0.0)
		{
			return Wrong_SetDefSmpFreq;
		}

		m_dDefSmpFreq = dDefSmpFreq;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMinMaxAmplitudes: Sets the minimal and maximal amplitudes for a valid signal. \param dMinAmplitude
	New minimum amplitude. \param dMaxAmplitude New maximum amplitude. \return A value, taken from the enum type
	ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMinMaxAmplitudes(double dMinAmplitude, double dMaxAmplitude)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if (dMinAmplitude < 0.0)
		{
			return Wrong_MinNegAmplitude;
		}

		if (dMinAmplitude > m_dMaxForMinAmplitude)
		{
			return Wrong_MinAmplitude;
		}

		if (dMaxAmplitude < 0.0)
		{
			return Wrong_MaxNegAmplitude;
		}

		if (dMaxAmplitude > m_dMaxForMaxAmplitude)
		{
			return Wrong_MaxAmplitude;
		}

		if (dMinAmplitude >= dMaxAmplitude)
		{
			return Wrong_MinMaxAmplitude;
		}

		m_dMinAmplitude = dMinAmplitude;
		m_dMaxAmplitude = dMaxAmplitude;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetLongestBandPassDelay: Sets the maximum BP delay based on assigned filters \return A value, taken
	from the enum type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetLongestBandPassDelay(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lLPlength;
		long lHPlength;
		long lLPdelay;
		long lHPdelay;
		long lBPdelay;
		CAccelDecelBandPassConfig *cpACBPC;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iBands; i++)
		{
			cpACBPC = &(m_pBands[i]);
			lLPlength = cpACBPC->GetLowPassFilterLength();
			lLPdelay = (long) ceil((double) lLPlength / 2);
			lHPlength = cpACBPC->GetHighPassFilterLength();
			lHPdelay = (long) ceil((double) lHPlength / 2);
			lBPdelay = lLPdelay + lHPdelay;
			m_lLongestBPdelay = max(m_lLongestBPdelay, lBPdelay);
		}

		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetLongestBumpCandLength: Sets the maximum bump candidate length across all bands (in seconds)
	\return A value, taken from the enum type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetLongestBumpCandLength(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dMaxLength = 0.0;
		double dMaxLengthBand = 0.0;
		CAccelDecelBandPassConfig *cpACBPC;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iBands; i++)
		{
			cpACBPC = &(m_pBands[i]);

			// dMaxLength will be in seconds
			dMaxLengthBand = max(cpACBPC->GetAccelMaxBumpCandLength(), cpACBPC->GetDecelMaxBumpCandLength());
			dMaxLength = max(dMaxLength, dMaxLengthBand);
		}

		m_lMaxBumpCandLength = (long) round(GetDefSmpFreq() * dMaxLength);	// in samples

		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMaxDelayBasedOnBPFilters: Sets MAX_DELAY based on longest BP and event lengths Note that MAX_DELAY
	is specified in minutes. \return A value, taken from the enum type ConfigErrorStatus, indicating either succes or
	the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMaxDelayBasedOnBPFilters(void)
	{
		// LXPLXPLXP Remove the max delay extension for now since it cause slight changes to the detection
		// LXPLXPLXP However, this was there so that very long baseline can never go so far in the past as to fo before the 'kept' signal ==> this was causing exception
		//SetMaxDelay(60 + ((m_lLongestBPdelay + m_lMaxBumpCandLength) / (60 * GetDefSmpFreq()))); // 60 is there for extend and baseline/accel/decel history!!! LXP

		// m_lLongestBPdelay is in samples, as is m_lMaxBumpCandLength,
		SetMaxDelay((m_lLongestBPdelay + m_lMaxBumpCandLength) / (60 * GetDefSmpFreq()));

		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::GetReqBaseLinesHistory Return the required history for baselines to keep, in samples.
	=======================================================================================================================
	*/
	long CConfigFHR::GetReqBaseLinesHistory(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long hist = (long) (m_dBaseLineHistoryKeep * 60.0 * GetDefSmpFreq());	// return in samples
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		return hist;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::GetReqAccelDecelHistory Return the required history for accel and decels to keep in buffer, in
	samples.
	=======================================================================================================================
	*/
	long CConfigFHR::GetReqAccelDecelHistory(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long hist = (long) (m_dAccelDecelHistoryKeep * 60.0 * GetDefSmpFreq()); // return in samples
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		return hist;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::GetAccExtBuffer() Return buffer for accel extension - safety margin workaround for dealing with candidate buffers in RT
	=======================================================================================================================
	*/
	long CConfigFHR::GetAccExtBuffer(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long n = (long) (m_dAccExtBuffer * GetDefSmpFreq()); // return in samples
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		return n;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::GetEarliestEventBegin() - absolute index before which a decel/accel will not be accepted for output - buffer at beginning of tracing due to filter effects
	=======================================================================================================================
	*/
	long CConfigFHR::GetEarliestEventBegin()
	{
		long eEvent = (long) (m_dEarliestEventBegin * 60.0 * GetDefSmpFreq()); // in samples
		return(eEvent);
	}


	/*
	=======================================================================================================================
	Return basTrim in samples - this is the amount to trim off baseline ends for the purposes of calculating
	variability
	=======================================================================================================================
	*/
	long CConfigFHR::GetBasTrimForVarCalc(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long basTrim = (long) (m_lBasTrimForVarCalcSec * GetDefSmpFreq());
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		return basTrim;
	}

	/*
	=======================================================================================================================
	Return bas segment length in samples - this is the segment length in which to split baselines for the purposes of
	computing variability
	=======================================================================================================================
	*/
	long CConfigFHR::GetBasSegLenForVarCalc(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long basSeg = (long) (m_lBasSegLenForVarCalcSec * GetDefSmpFreq());
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		return basSeg;
	}

	/*
	=======================================================================================================================
	Set PendingRT flag to b - if using pending RT then set amount of signal to extrapolate equal to the longest band pass filter delay
	=======================================================================================================================
	*/
	void CConfigFHR::UsePendingRT(bool b)
	{
		m_bPendingRT = b;
		if (b)
			SetNumExtrapolatedSignal(GetLongestBandPassDelay());
		else
			SetNumExtrapolatedSignal(0);
	}


	/*
	=======================================================================================================================
	! CConfigFHR::SetMaxDiffInSegPts: Sets the maximal difference between two con- secutive points either in a segment
	(dMaxDiffInSegPts) or in an artifact (dMaxDiffInArtifPts). \param dMaxDiffInSegPts New maximal difference between
	two consecutive points in a segment. \param dMaxDiffInArtifPts New maximal difference between two consecutive
	points in an artifact. \return A value, taken from the enum type ConfigErrorStatus, indicating either succes or the
	error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMaxDiffInSegPts(double dMaxDiffInSegPts, double dMaxDiffInArtifPts)	// = -1
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dMaxDiffInSegPts < 0.0) || (dMaxDiffInSegPts > m_dMaxForMaxDiffInSegPts))
		{
			return Wrong_MaxDiffInSegPts;
		}

		if ((dMaxDiffInArtifPts < 0.0) || (dMaxDiffInArtifPts > m_dMaxForMaxDiffInArtifPts))
		{
			return Wrong_MaxDiffInArtifPts;
		}

		m_dMaxDiffInSegPts = dMaxDiffInSegPts;
		m_dMaxDiffInArtifPts = dMaxDiffInArtifPts;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMaxTmWrongFHR: Sets the maximal time (duration) of a wrong FHR (e.g., for a signal that could be
	the mother's heart rate). \param dMaxTmWrongFHR New maximal time (duration) of a wrong FHR. \return A value, taken
	from the enum type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMaxTmWrongFHR(double dMaxTmWrongFHR)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dMaxTmWrongFHR < 0.0) || (dMaxTmWrongFHR > m_dMaxForMaxTmWrongFHR))
		{
			return Wrong_MaxTmWrongFHR;
		}

		m_dMaxTmWrongFHR = dMaxTmWrongFHR;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMinMaxSlopes: Sets the minimal and maximal slope as involved in some tests to repair the signal.
	\param dMinSlope New minimal slope as involved in some tests to repair the signal. \param dMaxSlope New maximal
	slope as involved in some tests to repair the signal. \return A value, taken from the enum type ConfigErrorStatus,
	indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMinMaxSlopes(double dMinSlope, double dMaxSlope)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dMinSlope < m_dMinForMinSlope) || (dMinSlope > 0.0))
		{
			return Wrong_MinSlope;
		}

		if ((dMaxSlope < 0.0) || (dMaxSlope > m_dMaxForMaxSlope))
		{
			return Wrong_MaxSlope;
		}

		m_dMinSlope = dMinSlope;
		m_dMaxSlope = dMaxSlope;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMinDiffOverlapFHR: Sets the minimal difference between overlaping FHR signals. \param
	dMinDiffOverlapFHR New minimal difference between overlaping FHR signals. \return A value, taken from the enum type
	ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMinDiffOverlapFHR(double dMinDiffOverlapFHR)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dMinDiffOverlapFHR < 0.0) || (dMinDiffOverlapFHR > m_dMaxForMinDiffOverlapFHR))
		{
			return Wrong_MinDiffOverlapFHR;
		}

		m_dMinDiffOverlapFHR = dMinDiffOverlapFHR;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetTmShortFHR: Sets a time (duration) over which it becomes possible to certify a segment is valid.
	Conversely, any segment not lasting this long might be a wrong FHR signal. \param dTmShortFHR New time over which
	it becomes possible to certify a segment is valid \return A value, taken from the enum type ConfigErrorStatus,
	indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetTmShortFHR(double dTmShortFHR)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dTmShortFHR < m_dMinForTmShortFHR) || (dTmShortFHR > m_dMaxForTmShortFHR))
		{
			return Wrong_TmShortFHR;
		}

		m_dTmShortFHR = dTmShortFHR;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMaxTmMergeRepair: Sets the maximal time (duration) that will be allowed between repairs involving
	merging segments. \param dMaxTmMergeRepair New maximal time that will be allowed between repairs involving merging
	segments. \return A value, taken from the enum type ConfigErrorStatus, indicating either succes or the error that
	happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMaxTmMergeRepair(double dMaxTmMergeRepair)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dMaxTmMergeRepair < 0.0) || (dMaxTmMergeRepair > m_dMaxForMaxTmMergeRepair))
		{
			return Wrong_MaxTmMergeRepair;
		}

		m_dMaxTmMergeRepair = dMaxTmMergeRepair;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMaxTmCorrelRepair: Sets the maximal time (duration) between two correlated repairs. \param
	dMaxTmCorrelRepair New maximal time between two correlated repairs. \return A value, taken from the enum type
	ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMaxTmCorrelRepair(double dMaxTmCorrelRepair)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dMaxTmCorrelRepair < 0.0) || (dMaxTmCorrelRepair > m_dMaxForMaxTmCorrelRepair))
		{
			return Wrong_MaxTmCorrelRepair;
		}

		m_dMaxTmCorrelRepair = dMaxTmCorrelRepair;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetDegeneratePrecision: Sets the TBD \param DegeneratePrecision TBD \return A value, taken from the
	enum type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetDegeneratePrecision(double dDegeneratePrecision)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		m_DegeneratePrecision = dDegeneratePrecision;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::SetMaxTmLookAhead: Sets the maximal time (duration) for the look- ahead when looking for the end of
	an artifact. \param dMaxTmLookAhead New maximal time (duration) for the look-ahead when looking for the end of an
	artifact. \return A value, taken from the enum type ConfigErrorStatus, indicating either succes or the error that
	happened.
	=======================================================================================================================
	*/
	int CConfigFHR::SetMaxTmLookAhead(double dMaxTmLookAhead)
	{
		if (!m_AllowConfiguration)
		{
			return -100;
		}

		if ((dMaxTmLookAhead < 0.0) || (dMaxTmLookAhead > m_dMaxForMaxTmLookAhead))
		{
			return Wrong_MaxTmLookAhead;
		}

		m_dMaxTmLookAhead = dMaxTmLookAhead;
		return CES_OK;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::Fmt: Formats a string using the given formatting template and provided value. \param FormatTemplate A
	formatting template (for sprintf). \param d1 Value to format. \return A string resulting from the required
	formating.
	=======================================================================================================================
	*/
	string CConfigFHR::Fmt(const char *FormatTemplate, double d1)
	{
		/*~~~~~~~~~~~*/
		char msg[1024];
		/*~~~~~~~~~~~*/

		sprintf(msg, FormatTemplate, d1);
		return string(msg);
	}

	/*
	=======================================================================================================================
	! CConfigFHR::Fmt: Formats a string using the given formatting template and provided values. \param FormatTemplate
	A formatting template (for sprintf). \param d1 First value to format. \param d2 Second value to format. \return A
	string resulting from the required formating.
	=======================================================================================================================
	*/
	string CConfigFHR::Fmt(const char *FormatTemplate, double d1, double d2)
	{
		/*~~~~~~~~~~~*/
		char msg[1024];
		/*~~~~~~~~~~~*/

		sprintf(msg, FormatTemplate, d1, d2);
		return string(msg);
	}

	/*
	=======================================================================================================================
	! CConfigFHR::ErrorMessage: Provides a human-readable string explaining the specified error. In the case the
	specified value is CES_OK, the function returns an empty string. \param ErrorCode An error code. \return A
	human-readable string explaining the specified error.
	=======================================================================================================================
	*/
	string CConfigFHR::ErrorMessage(int ErrorCode)
	{
		/*~~~~~*/
		string s;
		/*~~~~~*/

		if (ErrorCode == Wrong_SetDefSmpFreq)
		{
			s = "Error in value for SAMPLE_FHR_FREQ. Value must be positive";
		}
		else if (ErrorCode == Wrong_MinNegAmplitude)
		{
			s = "Error in value for MIN_FHR_VAL. Value must be non-negative";
		}
		else if (ErrorCode == Wrong_MinAmplitude)
		{
			s = Fmt("Error in value for MIN_FHR_VAL. Value must be in range 0 to %f", m_dMaxForMinAmplitude);
		}
		else if (ErrorCode == Wrong_MaxNegAmplitude)
		{
			s = "Error in value for MAX_FHR_VAL. Value must be non-negative";
		}
		else if (ErrorCode == Wrong_MaxAmplitude)
		{
			s = Fmt("Error in value for MAX_FHR_VAL. Value must be in range 0 to %f", m_dMaxForMaxAmplitude);
		}
		else if (ErrorCode == Wrong_MinMaxAmplitude)
		{
			s = "Error in values for MIN_FHR_VAL or MAX_FHR_VAL. MIN_FHR_VAL must be < MAX_FHR_VAL";
		}
		else if (ErrorCode == Wrong_MaxDiffInSegPts)
		{
			s = Fmt("Error in value for DIFF_MAX_IN. Value must be in range 0 to %f", m_dMaxForMaxDiffInSegPts);
		}
		else if (ErrorCode == Wrong_MaxDiffInArtifPts)
		{
			s = Fmt("Error in value for DIFF_MAX_OUT. Value must be in range 0 to %f", m_dMaxForMaxDiffInArtifPts);
		}
		else if (ErrorCode == Wrong_MinSlope)
		{
			s = Fmt("Error in value for MAX_NEG_SLOPE. Value must be in range %f to 0", m_dMinForMinSlope);
		}
		else if (ErrorCode == Wrong_MaxSlope)
		{
			s = Fmt("Error in value for MAX_POS_SLOPE. Value must be in range 0 to %f", m_dMaxForMaxSlope);
		}
		else if (ErrorCode == Wrong_MinDiffOverlapFHR)
		{
			s = Fmt("Error in value for MIN_FHR_DIFF. Value must be in range 0 to %f", m_dMaxForMinDiffOverlapFHR);
		}
		else if (ErrorCode == Wrong_MaxTmWrongFHR)
		{
			s = Fmt("Error in value for FHR_LEN_MAX. Value must be in range 0 to %f", m_dMaxForMaxTmWrongFHR);
		}
		else if (ErrorCode == Wrong_MaxTmMergeRepair)
		{
			s = Fmt("Error in value for MAX_MERGE_REPAIR. Value must be in range 0 to %f", m_dMaxForMaxTmMergeRepair * m_dDefSmpFreq);
		}
		else if (ErrorCode == Wrong_MaxTmCorrelRepair)
		{
			s = Fmt("Error in value for MAX_CORRELATED_REPAIR. Value must be in range 0 to %f", m_dMaxForMaxTmCorrelRepair * m_dDefSmpFreq);
		}
		else if (ErrorCode == Wrong_MaxTmLookAhead)
		{
			s = Fmt("Error in value for MAX_LOOKAHEAD. Value must be in range 0 to %f", m_dMaxForMaxTmLookAhead * m_dDefSmpFreq);
		}
		else if (ErrorCode == Wrong_TmShortFHR)
		{
			s = Fmt("Error in value for SHORT_FHR_LEN. Value must be in range %f to %f", m_dMinForTmShortFHR * m_dDefSmpFreq, m_dMaxForTmShortFHR * m_dDefSmpFreq);
		}

		return s;
	}

	/*
	=======================================================================================================================
	GetAccelExperts - get accel NNs for given iteration
	=======================================================================================================================
	*/
	NeuralNet* CConfigFHR::GetAccelExperts(long iteration)
	{
		if (iteration == 0)
			return(m_nnetAccelExperts);
		else if (iteration == 1)
			return(m_nnetAccelExperts2);
		else
			return(NULL);
	}

	/*
	=======================================================================================================================
	GetAccelExperts - get decel NNs for given iteration
	=======================================================================================================================
	*/
	NeuralNet* CConfigFHR::GetDecelExperts(long iteration)
	{
		if (iteration == 0)
			return(m_nnetDecelExperts);
		else if (iteration == 1)
			return(m_nnetDecelExperts2);
		else
			return(NULL);
	}


	/*
	=======================================================================================================================
	! CConfigFHR::LoadAccelDecelTopologyFromResource: Loads Neural Network Topology from Resources (rc2). This function
	given a resource handle, a name and type of resources will attempt to load the information in the rc file. It then
	will call parse to convert this text file into the nnet data structures "Accel_NN_Topology.txt" "Topology",
	"Decel_NN_Topology.txt" "Topology". \param qpszName Resource name. \param qpszType Resource type. \param bAccel
	Accel or Decel? \return true iff everything was properly loaded.
	=======================================================================================================================
	*/
	bool CConfigFHR::LoadAccelDecelTopologyFromResource(const char *qpszName, const char *qpszType, long iteration, bool bAccel)
	{
		/*~~~~~~~~~~~~~*/
		HGLOBAL zhRes = NULL;
		HINSTANCE zhInst;
		char *zpRes = 0;
		/*~~~~~~~~~~~~~*/

		wchar_t wName[1000];
		mbstowcs(wName, qpszName, strlen(qpszName) + 1);//Plus null
		LPWSTR ptrName = wName;

		wchar_t wType[1000];
		mbstowcs(wType, qpszType, strlen(qpszType) + 1);//Plus null
		LPWSTR ptrType = wType;

		zhInst = GetModuleHandleA(GetResourceHandleModuleName().length() == 0 ? NULL : GetResourceHandleModuleName().c_str());

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		LPVOID pdata = NULL;
		HRSRC zRsc = FindResource(zhInst, ptrName, ptrType);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (zRsc)
		{
			zhRes = LoadResource(zhInst, zRsc);
			if (zhRes)
			{
				pdata = LockResource(zhRes);
			}
			else
			{
				return false;
			}
		}
		else
		{
			/*~~~~~~~~~~~~~~~~~~~*/
			// for debugging purpose (if we are in the MFC test app, I would like to see the
			// TRACE of error)
			LPVOID lpMessageBuffer;
			/*~~~~~~~~~~~~~~~~~~~*/

			FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR) & lpMessageBuffer, 0, NULL);

			/*~~~~~~~~~~*/
			CString z_msg;
			/*~~~~~~~~~~*/

			z_msg += (char *) lpMessageBuffer;
			TRACE(z_msg);
			LocalFree(lpMessageBuffer);
		}

		if (!pdata)
		{
			return false;
		}

		/*~~~~~~~~~~~~*/
		// now read out of the resource file
		DWORD dwSizeRes;
		/*~~~~~~~~~~~~*/

		dwSizeRes = SizeofResource(zhInst, zRsc);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		unsigned char *sData = (unsigned char *) pdata;
		//LPTSTR sExpert = (LPTSTR) sData;
		//string strExpert = string(sExpert);
		std::string strExpert(reinterpret_cast< char const* >(sData));
		string strEmpty;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		strEmpty.empty();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRet = ParseAccelDecelTopology(strExpert, strEmpty, iteration, bAccel, true);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		FreeResource(zhRes);
		return bRet;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::ParseAccelDecelTopology: Converts a given text string into the various matrices, vector, and function
	that make up a neural net. \param strExpert TBD \param strPath TBD \param bAccel Accel or Decel? \param bResource
	TBD \return true iff the text was parsed without errors.
	=======================================================================================================================
	*/
	bool CConfigFHR::ParseAccelDecelTopology
		(
		string strExpert,
		string strPath,
		long iteration, 
		bool bAccel,	// false for Decel
		bool bResource
		)					// false for not resource
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bValid = true;
		size_t iLen = strExpert.length();
		size_t iPos = strExpert.find('\n', 0);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (iPos == string::npos)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// first string that it reads is the number of layer
		string strNumberExperts = strExpert.substr(0, iPos);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (bAccel)
		{
			m_nNumberOfAccelExperts = atoi(strNumberExperts.c_str());
			if (m_nNumberOfAccelExperts == 0)
			{
				return false;
			}
			if (iteration == 1)
			{
				m_nnetAccelExperts = new NeuralNet[m_nNumberOfAccelExperts];
			}
			else if (iteration == 2)
			{
				m_nnetAccelExperts2 = new NeuralNet[m_nNumberOfAccelExperts];
			}
		}
		else
		{
			m_nNumberOfDecelExperts = atoi(strNumberExperts.c_str());
			if (m_nNumberOfDecelExperts == 0)
			{
				return false;
			}

			if (iteration == 1)
			{
				m_nnetDecelExperts = new NeuralNet[m_nNumberOfDecelExperts];
			}
			else if (iteration == 2)
			{
				m_nnetDecelExperts2 = new NeuralNet[m_nNumberOfDecelExperts];
			}
		}

		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);	// move past the NumberExperts number
		iPos = strExpert.find('\n', 0);
		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);	// move past the blank line under layer number
		iPos = strExpert.find('\n', 0);

		/*~~~~~~~~~~~*/
		string strLine;
		int nCount = 0;
		/*~~~~~~~~~~~*/

		// here we need the count of experts
		while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
		{
			strLine = strExpert.substr(0, iPos);

			if (bAccel)
			{
				if (bResource)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// this code trims any excessive hard or soft returns
					size_t itPos = strLine.find('\n', 0);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					if (itPos == string::npos)
					{
						itPos = strLine.find('\r', 0);
					}

					if (itPos != string::npos)
					{
						strLine = strLine.substr(0, (int) strLine.length() - 1);
					}

					// load the given resource name
					if (iteration == 1)
					{
						bValid = m_nnetAccelExperts[nCount].LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strLine.c_str(), "NeuralNet");
					}
					else if (iteration == 2)
					{
						bValid = m_nnetAccelExperts2[nCount].LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strLine.c_str(), "NeuralNet");
					}
				}
				else
				{
					/*~~~~~~~~~~~~~~~~~~~~~*/
					string strPass = strPath;
					/*~~~~~~~~~~~~~~~~~~~~~*/

					strPass.append(strLine);
					if (iteration == 1)
					{
						bValid = m_nnetAccelExperts[nCount].LoadFromFile(strPass);
					}
					else if (iteration == 2)
					{
						bValid = m_nnetAccelExperts2[nCount].LoadFromFile(strPass);
					}

					// bValid = m_nnetAccelExperts[nCount].LoadFromFile(strLine);
					if (!bValid)
					{
						return false;
					}
				}

				nCount += 1;

				if (nCount >= m_nNumberOfAccelExperts)
				{
					break;
				}
			}
			else
			{
				if (bResource)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// this code trims any excessive hard or soft returns
					size_t itPos = strLine.find('\n', 0);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					if (itPos == string::npos)
					{
						itPos = strLine.find('\r', 0);
					}

					if (itPos != string::npos)
					{
						strLine = strLine.substr(0, (int) strLine.length() - 1);
					}

					// load the given resource name
					if (iteration == 1)
					{
						bValid = m_nnetDecelExperts[nCount].LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strLine.c_str(), "NeuralNet");
					}
					else if (iteration == 2)
					{
						bValid = m_nnetDecelExperts2[nCount].LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strLine.c_str(), "NeuralNet");
					}
				}
				else
				{
					/*~~~~~~~~~~~~~~~~~~~~~*/
					string strPass = strPath;
					/*~~~~~~~~~~~~~~~~~~~~~*/

					strPass.append(strLine);
					if (iteration == 1)
					{
						bValid = m_nnetDecelExperts[nCount].LoadFromFile(strPass);
					}
					else if (iteration == 2)
					{
						bValid = m_nnetDecelExperts2[nCount].LoadFromFile(strPass);
					}

					if (!bValid)
					{
						return false;
					}
				}

				nCount += 1;

				if (nCount >= m_nNumberOfDecelExperts)
				{
					break;
				}
			}

			// each line is a expert file name
			if (iPos == iLen)
			{
				strExpert = "";
			}
			else
			{
				strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);
			}

			// get a line of up to "\r\n"
			iPos = strExpert.find('\n', 0);
			if (iPos == string::npos)
			{
				iPos = strExpert.find('\r', 0);
				if (iPos == string::npos)
				{
					iPos = strExpert.length();
				}
			}

			iLen = strExpert.length();
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::LoadAccelDecelTopologyFromFile: Loads the Neural Network Topology from a specified file. It then
	parses it to convert its content of this text file into the nnet data structures "Accel_NN_Topology.txt"
	"Topology", "Decel_NN_Topology.txt" "Topology". \param FileName The name of the file to read from. \param bAccel
	Accel or Decel? \return true iff the data could be loaded.
	=======================================================================================================================
	*/
	bool CConfigFHR::LoadAccelDecelTopologyFromFile(string FileName, long iteration, bool bAccel)	// false for Decel
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRet = true;
		int fh;
		string strPath = FileName;
		size_t iLen = strPath.length();
		size_t iPos = strPath.find_last_of('\\');
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		strPath = strPath.substr(0, iPos);
		iPos = strPath.find_last_of('\\');
		strPath = strPath.substr(0, iPos + 1);

		fh = _open(FileName.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Read in input:
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = false;
		}
		else
		{
			/*~~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strExpert;
			/*~~~~~~~~~~~~~*/

			strExpert.replace(0, bytesread, buffer);
			strExpert.resize(bytesread, ' ');

			bRet = ParseAccelDecelTopology(strExpert, strPath, iteration, bAccel, false);
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::ValidateAccelTopology: Validates various aspects of a nnet. The nnet topology for input vector to
	bank of experts, ensure the experts ouput a single result vector. \return true iff the topology was acceptable.
	=======================================================================================================================
	*/
	bool CConfigFHR::ValidateAccelTopology(void)
	{
		/*~~~~~~~~~~~*/
		int iExpIn(0);
		int iExpOut(0);
		/*~~~~~~~~~~~*/

		if (m_nnetAccelExperts == NULL)
		{
			return false;
		}

		// check the experts
		for (int i = 0; i < m_nNumberOfAccelExperts; i++)
		{
			iExpIn = m_nnetAccelExperts[i].getExpectedInputSize();
			iExpOut = m_nnetAccelExperts[i].getExpectedOutputSize();
			if (iExpIn != 20)
			{	// bump input vector
				return false;
			}

			if (iExpOut != 2)
			{	// output size
				return false;
			}
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::ValidateDecelTopology: Validates various aspects of a nnet. The nnet topology for input vector to
	bank of experts, ensure the experts ouput a single result vector. \return true iff the topology was acceptable.
	=======================================================================================================================
	*/
	bool CConfigFHR::ValidateDecelTopology(void)
	{
		/*~~~~~~~~~~~*/
		int iExpIn(0);
		int iExpOut(0);
		/*~~~~~~~~~~~*/

		if (m_nnetDecelExperts == NULL)
		{
			return false;
		}

		// check the experts
		for (int i = 0; i < m_nNumberOfDecelExperts; i++)
		{
			iExpIn = m_nnetDecelExperts[i].getExpectedInputSize();
			iExpOut = m_nnetDecelExperts[i].getExpectedOutputSize();
			if (iExpIn != 20)
			{	// bump input vector
				return false;
			}

			if (iExpOut != 3)
			{	// output size
				return false;
			}
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::LoadBumpClassifyTopology: Loads Neural Network topology for Bump Classification. \return true iff
	loading proceded normally.
	=======================================================================================================================
	*/
	bool CConfigFHR::LoadBumpClassifyTopology(void)
	{
		/*~~~~~~~~~~~~~~~*/
		bool bValid = true;
		/*~~~~~~~~~~~~~~~*/

		// first ask the configuration if they are loading the nnets from files or from
		// resource
		if (m_bUseFileNNET_AccelDecel)
		{
			bValid = LoadAccelDecelTopologyFromFile(m_strAccelNNETFile, 1, true);	// Accel NN
			if (GetNumAccelDecelIterations() > 1)
			{
				bValid = (bValid && (LoadAccelDecelTopologyFromFile(m_strAccelNNETFile2, 2, true)));
			}
			if (!bValid || !ValidateAccelTopology())	// if the topology of the file is not
			{	// in the correct format then we will defualt to the topology from resource
				///;
				///which is validated we should error message somehow
				bValid = LoadAccelDecelTopologyFromResource("AccelNN\\Accel_NN_Topology.txt", "Topology", 1, true);	// Accel NN
				bValid = (bValid && LoadAccelDecelTopologyFromResource("DecelNN\\Decel_NN_Topology.txt", "Topology", 1, false));	// Decel NN
				if (GetNumAccelDecelIterations() > 1)
				{
					bValid = (bValid && LoadAccelDecelTopologyFromResource("AccelNN2\\Accel_NN_Topology.txt", "Topology", 2, true));	// Accel NN			
					bValid = (bValid && LoadAccelDecelTopologyFromResource("DecelNN2\\Decel_NN_Topology.txt", "Topology", 2, false));	// Decel NN
				}
#ifdef _DEBUG
				// developer check that the resource file are populated correctly
				bValid = ValidateAccelTopology();
				bValid = ValidateDecelTopology();
#endif
			}
			else
			{
				// Accel topology passed time to load the Decel
				bValid = LoadAccelDecelTopologyFromFile(m_strDecelNNETFile, 1, false); // Decel NN
				if (GetNumAccelDecelIterations() > 1)
				{
					bValid = (bValid && (LoadAccelDecelTopologyFromFile(m_strDecelNNETFile2, 2, false)));
				}
				if (!bValid || !ValidateDecelTopology())	// if the topology of the file is not
				{	// in the correct format then the Signal will defualt to the topology
					///;
					///from resource which is validated we should error message somehow
					bValid = LoadAccelDecelTopologyFromResource("AccelNN\\Accel_NN_Topology.txt", "Topology", 1, true);	// Accel NN
					bValid = (bValid && LoadAccelDecelTopologyFromResource("DecelNN\\Decel_NN_Topology.txt", "Topology", 1, false));	// Decel NN
					if (GetNumAccelDecelIterations() > 1)
					{
						bValid = (bValid && LoadAccelDecelTopologyFromResource("AccelNN2\\Accel_NN_Topology.txt", "Topology", 2, true));	// Accel NN			
						bValid = (bValid && LoadAccelDecelTopologyFromResource("DecelNN2\\Decel_NN_Topology.txt", "Topology", 2, false));	// Decel NN
					}
#ifdef _DEBUG
					// developer check that the resource file are populated correctly
					bValid = ValidateAccelTopology();
					bValid = ValidateDecelTopology();
#endif
				}
			}
		}
		else
		{
			bValid = LoadAccelDecelTopologyFromResource("AccelNN\\Accel_NN_Topology.txt", "Topology", 1, true);			// Accel NN
			bValid = LoadAccelDecelTopologyFromResource("DecelNN\\Decel_NN_Topology.txt", "Topology", 1, false);		// Decel NN
			if (GetNumAccelDecelIterations() > 1)
			{
				bValid = LoadAccelDecelTopologyFromResource("AccelNN2\\Accel_NN_Topology.txt", "Topology", 2, true);			// Accel NN
				bValid = LoadAccelDecelTopologyFromResource("DecelNN2\\Decel_NN_Topology.txt", "Topology", 2, false);		// Decel NN
			}

#ifdef _DEBUG
			// developer check that the resource file are populated correctly
			bValid = ValidateAccelTopology();
			bValid = ValidateDecelTopology();
#endif
		}

		return bValid;
	}

	//
	// =======================================================================================================================
	//    MultiHypothesis Config protected methods. ;
	//    ! CConfigFHR::LoadTopologyFromResource: Given a resource handle, a name and a type of resources will attempt to
	//    load the information in the rc file. It then will call parse to convert this text file into the nnet data
	//    structures "v5h_NN_Topology", "Topology" "v5i_NN_Topology", "Topology". \param qpszName Resource name. \param
	//    qpszType Resource type. \param bPrimary TBD \return true iff loading proceded normally.
	// =======================================================================================================================
	//
	bool CConfigFHR::LoadTopologyFromResource(const char *qpszName, const char *qpszType, bool bPrimary)
	{
		/*~~~~~~~~~~~~~*/
		HGLOBAL zhRes;
		HINSTANCE zhInst;
		char *zpRes = 0;
		/*~~~~~~~~~~~~~*/

		zhInst = GetModuleHandleA(GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str());

		wchar_t wName[1000];
		mbstowcs(wName, qpszName, strlen(qpszName) + 1);//Plus null
		LPWSTR ptrName = wName;

		wchar_t wType[1000];
		mbstowcs(wType, qpszType, strlen(qpszType) + 1);//Plus null
		LPWSTR ptrType = wType;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		LPVOID pdata = NULL;
		HRSRC zRsc = FindResource(zhInst, ptrName, ptrType);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (zRsc)
		{
			zhRes = LoadResource(zhInst, zRsc);
			if (zhRes)
			{
				pdata = LockResource(zhRes);
			}
			else
			{
				return false;
			}
		}
		else
		{
			/*~~~~~~~~~~~~~~~~~~~*/
			// for debugging purpose (if we are in the MFC test app, I would like to see the
			// TRACE of error)
			LPVOID lpMessageBuffer;
			/*~~~~~~~~~~~~~~~~~~~*/

			FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR) & lpMessageBuffer, 0, NULL);

			/*~~~~~~~~~~*/
			CString z_msg;
			/*~~~~~~~~~~*/

			z_msg += (char *) lpMessageBuffer;
			TRACE(z_msg);
			LocalFree(lpMessageBuffer);
		}

		if (!pdata)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// now read out of the resource file
		DWORD dwSizeRes = SizeofResource(zhInst, zRsc);
		unsigned char *sData = (unsigned char *) pdata;
		//LPTSTR sExpert = (LPTSTR) sData;
		//string strExpert = string(sExpert);
		std::string strExpert(reinterpret_cast< char const* >(sData));
		string strEmpty;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		strEmpty.empty();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bParse = ParseTopology(strExpert, strEmpty, bPrimary);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		FreeResource(zhRes);

		return bParse;
	};

	/*
	=======================================================================================================================
	! CConfigFHR::ParseTopology: Converts a given text string into the various matrices, vector, and function that make
	up a neural net. \param strExpert TBD \param strPath TBD \param bPrimary TBD \param bResource TBD \return true iff
	loading proceded normally.
	=======================================================================================================================
	*/
	bool CConfigFHR::ParseTopology(string strExpert, string strPath, bool bPrimary, bool bResource)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bValid = true;
		size_t iLen = strExpert.length();
		size_t iPos = strExpert.find('\n', 0);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (iPos == string::npos)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string strAbiter = strExpert.substr(0, iPos);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (bPrimary)
		{
			if (bResource)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				// this code trims any excessive hard or soft returns
				size_t itPos = strAbiter.find('\n', 0);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (itPos == string::npos)
				{
					itPos = strAbiter.find('\r', 0);
				}

				if (itPos != string::npos)
				{
					strAbiter = strAbiter.substr(0, strAbiter.length() - 1);
				}

				// load the given resource name
				bValid = m_nnetArbiter.LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strAbiter.c_str(), "NeuralNet");
			}
			else
			{
				/*~~~~~~~~~~~~~~~~~~~~~*/
				string strPass = strPath;
				/*~~~~~~~~~~~~~~~~~~~~~*/

				strPass.append(strAbiter);
				bValid = m_nnetArbiter.LoadFromFile(strPass);

				// bValid = m_nnetArbiter.LoadFromFile(strAbiter);
			}
		}
		else
		{
			if (bResource)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				// this code trims any excessive hard or soft returns
				size_t itPos = strAbiter.find('\n', 0);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (itPos == string::npos)
				{
					itPos = strAbiter.find('\r', 0);
				}

				if (itPos != string::npos)
				{
					strAbiter = strAbiter.substr(0, strAbiter.length() - 1);
				}

				// load the given resource name
				bValid = m_nnetSecondaryArbiter.LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strAbiter.c_str(), "NeuralNet");
			}
			else
			{
				/*~~~~~~~~~~~~~~~~~~~~~*/
				string strPass = strPath;
				/*~~~~~~~~~~~~~~~~~~~~~*/

				strPass.append(strAbiter);
				bValid = m_nnetSecondaryArbiter.LoadFromFile(strPass);

				// bValid = m_nnetSecondaryArbiter.LoadFromFile(strAbiter);
			}
		}

		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);	// move past the arbiter
		iPos = strExpert.find('\n', 0);
		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);	// move past the blank line under layer number
		iPos = strExpert.find('\n', 0);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// first string that it reads is the number of layer
		string strNumberExperts = strExpert.substr(0, iPos);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (bPrimary)
		{
			m_nNumberofExperts = atoi(strNumberExperts.c_str());
			if (m_nNumberofExperts == 0)
			{
				return false;
			}

			m_nnetExperts = new NeuralNet[m_nNumberofExperts];
		}
		else
		{
			m_nNumberofSecondaryExperts = atoi(strNumberExperts.c_str());
			if (m_nNumberofSecondaryExperts == 0)
			{
				return false;
			}

			m_nnetSecondaryExperts = new NeuralNet[m_nNumberofSecondaryExperts];
		}

		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);	// move past the NumberExperts number
		iPos = strExpert.find('\n', 0);
		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);	// move past the blank line under layer number
		iPos = strExpert.find('\n', 0);

		/*~~~~~~~~~~~*/
		string strLine;
		int nCount = 0;
		/*~~~~~~~~~~~*/

		// here we need the count of experts
		while (iLen > 0)
		{
			strLine = strExpert.substr(0, iPos);

			if (bPrimary)
			{
				if (bResource)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// this code trims any excessive hard or soft returns
					size_t itPos = strLine.find('\n', 0);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					if (itPos == string::npos)
					{
						itPos = strLine.find('\r', 0);
					}

					if (itPos != string::npos)
					{
						strLine = strLine.substr(0, (int) strLine.length() - 1);
					}

					// load the given resource name
					bValid = m_nnetExperts[nCount].LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strLine.c_str(), "NeuralNet");
				}
				else
				{
					/*~~~~~~~~~~~~~~~~~~~~~*/
					string strPass = strPath;
					/*~~~~~~~~~~~~~~~~~~~~~*/

					strPass.append(strLine);
					bValid = m_nnetExperts[nCount].LoadFromFile(strPass);

					// bValid = m_nnetExperts[nCount].LoadFromFile(strLine);
				}

				nCount += 1;

				if (nCount >= m_nNumberofExperts)
				{
					break;
				}
			}
			else
			{
				if (bResource)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					// this code trims any excessive hard or soft returns
					size_t itPos = strLine.find('\n', 0);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					if (itPos == string::npos)
					{
						itPos = strLine.find('\r', 0);
					}

					if (itPos != string::npos)
					{
						strLine = strLine.substr(0, (int) strLine.length() - 1);
					}

					// load the given resource name
					bValid = m_nnetSecondaryExperts[nCount].LoadFromResource((GetResourceHandleModuleName().length() == 0?NULL:GetResourceHandleModuleName().c_str()), strLine.c_str(), "NeuralNet");
				}
				else
				{
					/*~~~~~~~~~~~~~~~~~~~~~*/
					string strPass = strPath;
					/*~~~~~~~~~~~~~~~~~~~~~*/

					strPass.append(strLine);
					bValid = m_nnetSecondaryExperts[nCount].LoadFromFile(strPass);

					// bValid = m_nnetSecondaryExperts[nCount].LoadFromFile(strLine);
				}

				nCount += 1;

				if (nCount >= m_nNumberofSecondaryExperts)
				{
					break;
				}
			}

			// each line is a expert file name
			if (iPos == iLen)
			{
				strExpert = "";
			}
			else
			{
				strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);
			}

			// get a line of up to "\r\n"
			iPos = strExpert.find('\n', 0);
			if (iPos == string::npos)
			{
				iPos = strExpert.find('\r', 0);
			}

			if (iPos == string::npos)
			{
				iPos = strExpert.length();
			}

			iLen = strExpert.length();
		}

		return bValid;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::LoadTopologyFromFile: Loads the Neural Network Topology from a specified file. It then parses it to
	convert its content of this text file into the nnet data structures. \param FileName The file to read from. \param
	bPrimary TBD \return true iff loading proceded normally.
	=======================================================================================================================
	*/
	bool CConfigFHR::LoadTopologyFromFile(string FileName, bool bPrimary)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRet = true;
		int fh;
		string strPath = FileName;
		size_t iLen = strPath.length();
		size_t iPos = strPath.find_last_of('\\');
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		strPath = strPath.substr(0, iPos);
		iPos = strPath.find_last_of('\\');
		strPath = strPath.substr(0, iPos + 1);

		fh = _open(FileName.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		/* Read in input: */
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = false;
		}
		else
		{
			/*~~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strExpert;
			/*~~~~~~~~~~~~~*/

			strExpert.replace(0, bytesread, buffer);
			strExpert.resize(bytesread, ' ');
			bRet = ParseTopology(strExpert, strPath, bPrimary, false);
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::ValidateTopology: Validates various aspects of a nnet. The nnet topology for input vector to bank of
	experts, ensure the experts ouput a single result vector. \param bPrimary TBD \return true iff the topology is
	acceptable.
	=======================================================================================================================
	*/
	bool CConfigFHR::ValidateTopology(bool bPrimary)
	{
		/*~~~~~~~~~~~*/
		int iExpIn(0);
		int iExpOut(0);
		/*~~~~~~~~~~~*/

		if (bPrimary)
		{
			if (m_nnetExperts == NULL)
			{
				return false;
			}

			// check the experts
			for (int i = 0; i < m_nNumberofExperts; i++)
			{
				iExpIn = m_nnetExperts[i].getExpectedInputSize();
				iExpOut = m_nnetExperts[i].getExpectedOutputSize();
				if (iExpIn != 55l)
				{	// fhr input vector
					return false;
				}

				if (iExpOut != 1l)
				{	// input vector for arbiter = output size x number of experts
					return false;
				}
			}

			// Experts - fine, check Arbiter now:
			iExpIn = m_nnetArbiter.getExpectedInputSize();
			if (iExpIn != m_nNumberofExperts)
			{
				return false;
			}

			iExpOut = m_nnetArbiter.getExpectedOutputSize();
			if (iExpOut != 1l)
			{
				return false;
			}
		}
		else
		{
			if (m_nnetSecondaryExperts == NULL)
			{
				return false;
			}

			// check the experts
			for (int i = 0; i < m_nNumberofExperts; i++)
			{
				if (m_nnetSecondaryExperts[i].getExpectedInputSize() != 55l)
				{	// fhr input vector
					return false;
				}

				if (m_nnetSecondaryExperts[i].getExpectedOutputSize() != 1l)
				{	// input vector for arbiter = output size x number of experts
					return false;
				}
			}

			if (m_nnetSecondaryArbiter.getExpectedInputSize() != m_nNumberofExperts)
			{
				return false;
			}

			if (m_nnetSecondaryArbiter.getExpectedOutputSize() != 1l)
			{
				return false;
			}
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CConfigFHR::LoadMultihypothesisTopology: Loads the nnet from the resource or files the topology itself is a file
	containing the file names of the nnets.
	=======================================================================================================================
	*/
	bool CConfigFHR::LoadMultihypothesisTopology(void)
	{
		/*~~~~~~~~~~~~~~~*/
		bool bValid = true;
		/*~~~~~~~~~~~~~~~*/

		// first ask the configuration if they are loading the nnets from files or from resource;
		if (IsUsingFileNNET_MultiHypothesis())
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string FileName = GetPrimaryNNETFile();
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			bValid = LoadTopologyFromFile(FileName, true);	// Primary NN
			if (bValid && !ValidateTopology(true))			// if the topology of the file is not in the correct format then the Signal will defualt to the topology is the resource which is validated
			{
				// we should error message somehow
				bValid = LoadTopologyFromResource("all_v5h\\v5h_NN_Topology.txt", "Topology", true);		// Primary NN
				if (bValid)
				{
					bValid = LoadTopologyFromResource("all_v5i\\v5i_NN_Topology.txt", "Topology", false);	// Secondary NN
				}

#ifdef _DEBUG
				// developer check that the resource file are populated correctly
				if (bValid)
				{
					bValid = ValidateTopology(true);
				}

				if (bValid)
				{
					bValid = ValidateTopology(false);
				}

#endif
			}
			else
			{
				// primary topology passed time to load the secondary
				FileName = GetSecondaryNNETFile();
				bValid = LoadTopologyFromFile(FileName, false); // Secondary NN
				if (bValid && !ValidateTopology(false))			// if the topology of the file is not in the correct format then the Signal will defualt to the topology is the resource which is validated
				{
					// we should error message somehow
					bValid = LoadTopologyFromResource("all_v5h\\v5h_NN_Topology.txt", "Topology", true);		// Primary NN
					if (bValid)
					{
						bValid = LoadTopologyFromResource("all_v5i\\v5i_NN_Topology.txt", "Topology", false);	// Secondary NN
					}

#ifdef _DEBUG
					// developer check that the resource file are populated correctly
					if (bValid)
					{
						bValid = ValidateTopology(true);
					}

					if (bValid)
					{
						bValid = ValidateTopology(false);
					}

#endif
				}
			}
		}
		else
		{
			bValid = LoadTopologyFromResource("all_v5h\\v5h_NN_Topology.txt", "Topology", true);				// Primary NN
			if (bValid)
			{
				bValid = LoadTopologyFromResource("all_v5i\\v5i_NN_Topology.txt", "Topology", false);			// Secondary NN
			}

#ifdef _DEBUG
			// developer check that the resource file are populated correctly
			if (bValid)
			{
				bValid = ValidateTopology(true);
			}

			if (bValid)
			{
				bValid = ValidateTopology(false);
			}

#endif
		}

		return bValid;
	}

	/*
	=======================================================================================================================
	! Setup config parameters for NN training
	=======================================================================================================================
	*/
	void CConfigFHR::SetNNtrainingParams(void)
	{
		m_bNNtrain = true;
		m_bRemoveRepairCandidates = false;
		m_iStoreXML = 2;
		m_iStoreMAT = 1;
		m_bDisableRepair = true;
		m_bExtendAccelCandidates = true;
		m_bUseMaxConfidenceDecel = false;
		m_bUseMaxConfidenceAccel = false;
	}

	//
	// =======================================================================================================================
	//    Definition of CAccelDecelBandPassConfig members. ;
	//    ! CAccelDecelBandPassConfig::<constructor>
	// =======================================================================================================================
	//
	CAccelDecelBandPassConfig::CAccelDecelBandPassConfig(void)
	{
	}

	//
	// =======================================================================================================================
	//    ! CAccelDecelBandPassConfig::<destructor>
	// =======================================================================================================================
	//
	CAccelDecelBandPassConfig::~CAccelDecelBandPassConfig(void)
	{
	}

	/*
	=======================================================================================================================
	! CAccelDecelBandPassConfig::ReadLowPassFilterFile: Sets the CConfig class with values read from a Low pass filter
	file whose name is provided. \param filename The name of the file to read from. \return A value, taken from the
	enum type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	long CAccelDecelBandPassConfig::ReadLowPassFilterFile(string filename)
	{
		/*~~~~~~~~~~~~*/
		// m_pCfgFHR->m_dFirFlt,m_pCfgFHR->m_iFirFlt_Size double* dFilterData, int&
		// lFilter, CString& strExtra
		long iFilter(0);
		int bRet = 0;
		int fh;
		/*~~~~~~~~~~~~*/

		fh = _open(filename.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return -1;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Read in input:
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = -2;
		}
		else
		{
			/*~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strData;
			// put the buffer in a string object
			string strParse;
			/*~~~~~~~~~~~~*/

			strData.replace(0, bytesread, buffer);
			strData.resize(bytesread, ' ');

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			size_t iLen = strData.length();
			size_t iPos = strData.find('\n', 0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (iPos == string::npos)
			{
				return -2;
			}

			strParse = strData;

			/*~~~~~~~~~~~~~~*/
			string strLine;
			int nCount = 0;
			double dFilter(0);
			/*~~~~~~~~~~~~~~*/

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strData.substr(0, iPos);
				iFilter++;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strData = "";
				}
				else
				{
					strData = strData.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strData.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strData.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strData.length();
					}
				}

				iLen = strData.length();
			}

			if (m_dLowPassFilter)
			{
				delete[] m_dLowPassFilter;
			}

			m_dLowPassFilter = new double[iFilter];
			m_lLowPassFilterLength = iFilter;

			/*~~~~~~~~*/
			int iAr = 0;
			/*~~~~~~~~*/

			iLen = strParse.length();
			iPos = strParse.find('\n', 0);
			if (iPos == string::npos)
			{
				return -2;
			}

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strParse.substr(0, iPos);
				dFilter = atof(strLine.c_str());
				m_dLowPassFilter[iAr] = dFilter;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strParse = "";
				}
				else
				{
					strParse = strParse.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strParse.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strParse.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strParse.length();
					}
				}

				iLen = strParse.length();
				iAr++;
			}
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	=======================================================================================================================
	! CAccelDecelBandPassConfig::ReadHighPassFilterFile: Sets the CConfig class with values read from a High pass
	filter file whose name is provided. \param filename The name of the file to read from. \return A value, taken from
	the enum type ConfigErrorStatus, indicating either succes or the error that happened.
	=======================================================================================================================
	*/
	long CAccelDecelBandPassConfig::ReadHighPassFilterFile(string filename)
	{
		/*~~~~~~~~~~~~*/
		// m_pCfgFHR->m_dFirFlt,m_pCfgFHR->m_iFirFlt_Size double* dFilterData, int&
		// lFilter, CString& strExtr
		long iFilter(0);
		int bRet = 0;
		int fh;
		/*~~~~~~~~~~~~*/

		fh = _open(filename.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return -1;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Read in input:
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = -2;
		}
		else
		{
			/*~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strData;
			// put the buffer in a string object
			string strParse;
			/*~~~~~~~~~~~~*/

			strData.replace(0, bytesread, buffer);
			strData.resize(bytesread, ' ');

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			size_t iLen = strData.length();
			size_t iPos = strData.find('\n', 0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (iPos == string::npos)
			{
				return -2;
			}

			strParse = strData;

			/*~~~~~~~~~~~~~~*/
			string strLine;
			int nCount = 0;
			double dFilter(0);
			/*~~~~~~~~~~~~~~*/

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strData.substr(0, iPos);
				iFilter++;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strData = "";
				}
				else
				{
					strData = strData.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strData.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strData.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strData.length();
					}
				}

				iLen = strData.length();
			}

			if (m_dHighPassFilter)
			{
				delete[] m_dHighPassFilter;
			}

			m_dHighPassFilter = new double[iFilter];
			m_lHighPassFilterLength = iFilter;

			/*~~~~~~~~*/
			int iAr = 0;
			/*~~~~~~~~*/

			iLen = strParse.length();
			iPos = strParse.find('\n', 0);
			if (iPos == string::npos)
			{
				return -2;
			}

			while (iLen > 0)	// (iPos != string::npos) // this is the not found string case
			{
				strLine = strParse.substr(0, iPos);
				dFilter = atof(strLine.c_str());
				m_dHighPassFilter[iAr] = dFilter;

				// each line is a expert file name
				if (iPos == iLen)
				{
					strParse = "";
				}
				else
				{
					strParse = strParse.substr(iPos + 1, iLen - iPos + 1);
				}

				// get a line of up to "\r\n"
				iPos = strParse.find('\n', 0);
				if (iPos == string::npos)
				{
					iPos = strParse.find('\r', 0);
					if (iPos == string::npos)
					{
						iPos = strParse.length();
					}
				}

				iLen = strParse.length();
				iAr++;
			}
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}
}
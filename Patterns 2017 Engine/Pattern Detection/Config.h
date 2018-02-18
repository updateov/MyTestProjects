/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection CLASSES: File contains
* class declaration for: CConfig : Base functionality for a basic configurator
* utility. CConfigFHR : Configurator specifically for FHR signals. AUTHORS: Mark
* Doubson /FFT and Filters part / MultiHypothesis / BumpClassifications Vladimir
* Khavkine /Bumps Candidates Robert Morin /Repair part Kingsley Woodward /Neuron
* network calculation Jiang Wu /Baseline detection Copyright LMS Medical Systems
* 2004 by Evonium Inc.
*/
#pragma once

// Namespace for string operations
using namespace std;

#include "NN.h"
#include <vector>

namespace patterns
{

	/*
	=======================================================================================================================
	! CLASS: CConfig DESCRIPTION: Base functionality for a basic configurator utility. This should be the base class
	for all configurators in this project.
	=======================================================================================================================
	*/
	class CConfig
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CConfig(void);
		virtual ~CConfig(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	};

	/*
	=======================================================================================================================
	! CLASS: CAccelDecelBandPassConfig DESCRIPTION: Configuration properties for Band Pass Filtering and Zero Cross.
	=======================================================================================================================
	*/
	class CAccelDecelBandPassConfig
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CAccelDecelBandPassConfig(void);
		virtual ~CAccelDecelBandPassConfig(void);

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Member variables
		// -------------------------------------------------------------------------------------------------------------------
		//
	protected:
		string m_strPathToSaveBandPass;
		string m_strPathToSaveZeroCrossings;
		double *m_dLowPassFilter;
		double *m_dHighPassFilter;

		double m_dAccelMinBumpArea; // Min Bump Area to qualify for Bump Candidate
		double m_dDecelMinBumpArea; // Min Bump Area to qualify for Bump Candidate

		double m_dAccelMaxBumpCandLength;
		double m_dDecelMaxBumpCandLength;

		long m_lLowPassFilterLength;
		long m_lHighPassFilterLength;

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Memeber functions
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetLowPassFilterLength(long len)
		{
			m_lLowPassFilterLength = len;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetLowPassFilterLength(void)
		{
			return m_lLowPassFilterLength;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetLowPassFilter(double *pD)
		{
			m_dLowPassFilter = pD;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double *GetLowPassFilter(void)
		{
			return m_dLowPassFilter;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetHighPassFilterLength(long len)
		{
			m_lHighPassFilterLength = len;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetHighPassFilterLength(void)
		{
			return m_lHighPassFilterLength;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetHighPassFilter(double *pD)
		{
			m_dHighPassFilter = pD;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double *GetHighPassFilter(void)
		{
			return m_dHighPassFilter;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetAccelMinBumpArea(double d)
		{
			m_dAccelMinBumpArea = d;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetAccelMinBumpArea(void)
		{
			return m_dAccelMinBumpArea;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetDecelMinBumpArea(double d)
		{
			m_dDecelMinBumpArea = d;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetDecelMinBumpArea(void)
		{
			return m_dDecelMinBumpArea;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetAccelMaxBumpCandLength(double d)
		{
			m_dAccelMaxBumpCandLength = d;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetAccelMaxBumpCandLength(void)
		{
			return m_dAccelMaxBumpCandLength;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetDecelMaxBumpCandLength(double d)
		{
			m_dDecelMaxBumpCandLength = d;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetDecelMaxBumpCandLength(void)
		{
			return m_dDecelMaxBumpCandLength;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetPathToSaveBandPass(string str)
		{
			m_strPathToSaveBandPass = str;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetPathToSaveBandPass(void)
		{
			return m_strPathToSaveBandPass;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetPathToSaveZeroCrossings(string str)
		{
			m_strPathToSaveZeroCrossings = str;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetPathToSaveZeroCrossings(void)
		{
			return m_strPathToSaveZeroCrossings;
		};

		long ReadLowPassFilterFile(string filename);	// now these are similar to that of standard filters
		long ReadHighPassFilterFile(string filename);
	};

	/*
	=======================================================================================================================
	! CLASS: CConfigFHR DESCRIPTION: Configurator utility for all that can be configured in the FHR signal repair
	process.
	=======================================================================================================================
	*/
	class CConfigFHR :
		public CConfig
	{
		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Constructor/destructor
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:
		CConfigFHR(void);
		~ CConfigFHR(void);

		// ! Public class enumerators
		enum FFTLibrary { eNONE = -1, eOOURA, eINTERNAL, eFFTW30 // to be extent... by LMS, developer must add their enum for each class they create or link to third party.
		};

		/*
		* ! Type: ConfigErrorStatus Error statuses that could be returned by one the Set
		* accessor functions for a property related to the FHR signal repair process.
		*/
		enum ConfigErrorStatus   
		{
			CES_OK					= 0,
			Wrong_SetDefSmpFreq		= -1000,
			Wrong_MinNegAmplitude,
			Wrong_MinAmplitude,
			Wrong_MaxNegAmplitude,
			Wrong_MaxAmplitude,
			Wrong_MinMaxAmplitude,
			Wrong_MaxDiffInSegPts,
			Wrong_MaxDiffInArtifPts,
			Wrong_MaxTmWrongFHR,
			Wrong_MinNegSlope,
			Wrong_MinSlope,
			Wrong_MaxNegSlope,
			Wrong_MaxSlope,
			Wrong_MinMaxSlope,
			Wrong_MinDiffOverlapFHR,
			Wrong_MaxTmMergeRepair,
			Wrong_MaxTmCorrelRepair,
			Wrong_MaxTmLookAhead,
			Wrong_TmShortFHR,
			Wrong_INInotFound,
			Wrong_INIReadError,
			Wrong_INIParameter,
			Wrong_FileNotFound,
			Wrong_FileRead
		};

		/* ! Type: PassedState TBD (Mark). */
		enum PassedState { No_Raw_Data = -1, Raw_Data, Repair, LowPass, HighPass, VarPass, MinVar, Baseline, MutliHypothesis, AccelDecel };

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Public members
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:
		// ! Constant bounds for BVA checking of all configurable values related to the
		// FHR signal repair process. CHE - make sure you remove stuff for old repair
		// algorithm - this is now in repair Config (Repair.h) ! Maximum values for the
		// minimum amplitude of an "acceptable" FHR signal
		static const double m_dMaxForMinAmplitude; // XXX

		// ! Maximum values for the maximum amplitude of an "acceptable" FHR signal.
		static const double m_dMaxForMaxAmplitude; // XXX

		// ! Maximum values for the maximum difference between two consecutive points in
		// a segment.
		static const double m_dMaxForMaxDiffInSegPts; // XXX

		// ! Maximum values for the maximum difference between two consecutive points in
		// finding the end of an artifact.
		static const double m_dMaxForMaxDiffInArtifPts; // XXX

		// ! Maximum value for the maximum duration (in seconds)of a wrong FHR (e.g., a
		// mother) FHR.
		static const double m_dMaxForMaxTmWrongFHR;  // XXX

		// ! Extreme values for the minimum acceptable slopes for two consecutive points.
		static const double m_dMinForMinSlope; // XXX

		// ! Extreme values for the maximum acceptable slopes for two consecutive points.
		static const double m_dMaxForMaxSlope; // XXX

		// ! Maximum value for the minimum height difference between two overlaping FHR's.
		static const double m_dMaxForMinDiffOverlapFHR; // XXX

		// ! Maximum value for the maximum duration (in seconds)between two merged
		// reapairs.
		static const double m_dMaxForMaxTmMergeRepair; // XXX

		// ! Maximum value for the maximum duration (in seconds)between correlated repairs.
		static const double m_dMaxForMaxTmCorrelRepair; // XXX

		// ! Maximum value for the maximum duration (in seconds)for look ahead operations.
		static const double m_dMaxForMaxTmLookAhead; // XXX

		static const double m_dMinForTmShortFHR; // XXX
		static const double m_dMaxForTmShortFHR; // XXX

		// ! Lowest valid values in a FHR.
		double m_dFHR_Low;

		// ! Highest valid values in a FHR.
		double m_dFHR_High;

		// ! Filtering process properties.
		bool m_bStoreXML;					// store any XML
		int m_iStoreXML;					// controls level of o/p
		int m_iStoreMAT;					// controls level of .mat o/p (0 for none)
		string m_strXMLFilePrefix;
		bool m_bRemove1_BL; // XXX
		bool m_bRemoveL_BL; // XXX
		bool m_bRemoveN_BL; // XXX
		bool m_bMH_Simpl; // XXX
		bool m_bExcludeLastPositive; // XXX

		bool m_bNNtrain; // are we generating data for NN training?

		string m_strRootPath;				// Path to store output results (txt or xml)
		PassedState m_PassedState;

		double *m_dFirFlt;					// For external Filter File
		double *m_d1FirFlt;					// For external Filter File
		double *m_d2FirFlt;					// For external Filter File

		double *m_dFir_Flt;
		double *m_dFir_Flt2;
		double *m_dFir_Flt3;
		double *m_dFir_Flt4;
		double *m_dFir_Flt8;
		double *m_dFir_Flt_LP25;
		vector<double> m_filterCoefsHP25;

		int m_iFirFlt_Size;					// For external LowPass Filter File
		int m_iExtBlockSize;				// For external LowPass Blocksize adjustment

		int m_i1FirFlt_Size;				// For external 1Var Filter File
		int m_i1ExtBlockSize;				// For external 1Var Blocksize adjustment

		int m_i2FirFlt_Size;				// For external 2Var Filter File
		int m_i2ExtBlockSize;				// For external 2Var Blocksize adjustment

		int m_iFir_Flt_Size;
		int m_iFir_Flt2_Size;
		int m_iFir_Flt3_Size;
		int m_iFir_Flt4_Size;
		int m_iFir_Flt8_Size;
		int m_iFir_Flt_LP25_Size;
		int m_iFir_Flt_HP25_Size;

		bool m_bUseOOURA;
		bool m_bUS_Standard;
		FFTLibrary m_eFFTLibrary;

		bool m_bReplaceBumps;				// Adding Candidate Bump to the array and replace all XXX

		// overlapped Bumps (in Bump Competion between Freq bands)
		bool m_bNotAllowEmptyContractionAppend; // XXX

		bool m_bMergeBumpsWithAllBasCand;	// Merge all baseline candidates w/ bumps for final o/p

		// instead of just set from multiH
		bool m_bDisableRepair;				// in some cases will want to disable repair for NN training
		double m_dRepairBuffer;				// minimum buffer at end of window where repair is considered 'unstable' (in seconds)
		double m_dMaxRepairBeforeCommit;    // amount of repair at end of tracing before which we give up and just update commitIndexes so can commit to output (otherwise have long delays on output occuring right before long dropout)
		double m_dMaxPercRepairForOutput;
		long m_lMaxRepairIgnore; // number of samples of consecutive repair that can be ignored for the purposes of determining non-interpretable events

		bool m_bUseAvgBasLevelForYvals;		// use avg values of fhr samples as baseline Y1, Y2 instead of polyfit
		bool m_bExtendAccelCandidates;

		bool m_bUseMaxConfidenceDecel;		// 'true' for original way
		double m_dMaxNonDecConfForLate[2];		// max nonDecel confidence for a late gradual to be true (if m_bUseMaxConfidenceDecel == false)
		double m_dMaxNonDecConfForEarly[2];	// for early
		double m_dMaxNonDecConfForNonAssoc[2];
		double m_dMaxNonDecConfForAbrupt[2];
		double m_dMaxNonDecConfForLate_oneIter;
		double m_dMaxNonDecConfForEarly_oneIter;
		double m_dMaxNonDecConfForNonAssoc_oneIter;
		double m_dMaxNonDecConfForAbrupt_oneIter;
		bool m_bUseMaxConfidenceAccel;
		double m_dMinAccConf[2];
		double m_dMinAccConf_oneIter;
		double m_bMapOutputConfidence;

		double m_dMinAccelCandHeight;
		double m_dMinAccelCandMeanHeight;
		double m_dMinAbruptDecelHeight;

		long m_lBasTrimForVarCalcSec;
		long m_lBasSegLenForVarCalcSec;
		bool m_bConsiderSlopeForBasVar;
		bool m_bCalcBasVar;

		long m_lNumExtrapSignal; // amount of extrapolated signal at end of window
		bool m_bPendingRT; // are we using extrapolated signal to detect decels/accels at end of window?
		long m_lNumAccelDecelIterations; // number of accel/decel classification iterations


		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Protected members
		// -------------------------------------------------------------------------------------------------------------------
		//
	protected:
		// Repair process related properties. ! Default sampling frequency.
		double m_dDefSmpFreq;

		// ! Minimum amplitude of an "acceptable" FHR signal.
		double m_dMinAmplitude;

		// ! Maximum amplitude of an "acceptable" FHR signal.
		double m_dMaxAmplitude;

		// ! Maximum difference between two consecutive points in a segment.
		double m_dMaxDiffInSegPts; 

		// ! Maximum difference between two consecutive points in finding the end of an
		// artifact.
		double m_dMaxDiffInArtifPts;

		// ! Maximum duration (in seconds) of a wrong FHR (e.g., a mother) FHR.
		double m_dMaxTmWrongFHR;

		// ! Minimum acceptable slopes for two consecutive points.
		double m_dMinSlope;

		// ! Maximum acceptable slopes for two consecutive points.
		double m_dMaxSlope;

		// ! Minimum height difference between two overlaping FHR's.
		double m_dMinDiffOverlapFHR;

		// ! Maximum duration (in seconds) between two merged repairs.
		double m_dMaxTmMergeRepair;

		// ! Maximum duration (in seconds) between correlated repairs.
		double m_dMaxTmCorrelRepair;

		// ! Maximum duration (in seconds) for look ahead operations.
		double m_dMaxTmLookAhead;

		double m_dTmShortFHR;
		double m_DegeneratePrecision;
		bool m_bShortAppend;					// if true allow arbitrarily short appends before proceeding for processing

		long m_lMinSamplesAppend;				// minimum # of accumulated samples required in append before processing
		long m_lMaxSamplesAppend;				// maximum # of samples to process at once (to limit memory utilization)	

		bool m_bRemoveRepairCandidates;			// remove candidates w/ too much repair prior to classification (for NN traning)
		bool m_bRemoveRepairOutput;

		// For removal of repaired accels and decels
		double m_dMaxBumpRepOneSlope;
		double m_dMaxBumpRep2Slopes;
		double m_dMaxPercSlopeRep;
		double m_dMaxPercSlopeRep2;

		long m_lMinRepLengthFilterVar;

		long m_lMinProlongedLenSec;				// minimum length of decel (in seconds) to be called prolonged

		// Multihypothesis memebers
		bool m_bUseFileNNET_Multihypothesis;
		string m_strResourceHandleModuleName;
		string m_strPrimaryNNETFile;
		string m_strSecondaryNNETFile;

		int m_nNumberofExperts;
		NeuralNet *m_nnetExperts;
		NeuralNet m_nnetArbiter;

		int m_nNumberofSecondaryExperts;
		NeuralNet *m_nnetSecondaryExperts;
		NeuralNet m_nnetSecondaryArbiter;

		// Acceleration and deceleration properties and methods.
		struct NNStats
		{
			double dMean;
			double dStandardDev;
		};

		bool m_bUseFileNNET_AccelDecel;			// One flag for all files
		int m_iBands;							// Number of Frequency Bands
		int m_iBandsAccel;						// Number of freq bands used for accel detection
		int m_iBandsDecel;						// Number of bands used for decel detection
		CAccelDecelBandPassConfig *m_pBands;	// Individual Freq Band Configuration
		string m_strPathToSaveCandidateBumps;
		string m_strPathToSaveBumpsClassify;

		long m_lLongestBPdelay;					// longest band-pass filter delay
		long m_lMaxBumpCandLength;				// max bump cand length based on bandpass params -
		double m_dMinBumpCandLength;			// min bump cand length - set by user

		long m_lMinVarLookAhead;				// in seconds

		string m_strAccelNNETFile;				// Neural Network used for Accel Classification
		string m_strDecelNNETFile;				// Neural Network used for Decel Classification
		string m_strAccelNNETFile2;				// Neural Network used for Accel Classification
		string m_strDecelNNETFile2;				// Neural Network used for Decel Classification

		int m_nNumberOfAccelExperts;
		NeuralNet *m_nnetAccelExperts;			// Neural Network used for Accel Classification
		NeuralNet *m_nnetAccelExperts2;			// Neural Network used for Accel Classification (sec. iteration)
		int m_nNumberOfDecelExperts;
		NeuralNet *m_nnetDecelExperts;			// Neural Network used for Decel Classification
		NeuralNet *m_nnetDecelExperts2;			// Neural Network used for Decel Classification (sec. iteration)

		NNStats *m_AccelStats;
		int m_nAccelStats;
		NNStats *m_DecelStats;
		int m_nDecelStats;

		double m_dBaseLineHistoryKeep;			// in minutes - history to keep for multiH/AccelDecel classification
		double m_dAccelDecelHistoryKeep;		// in minutes - history to keep for merging
		double m_dAccExtBuffer;					// safety buffer for accel extension for RT commitment logic
		double m_dEarliestEventBegin;			// in minutes - remove events starting before certain time at very beginning of tracing


		/*
		* ! Minimum length of Output Object after being clipped out 20 seconds - maximum
		* time lag of end of decel to not be considered late.
		*/
		int m_LENGTH_THRESHOLD;
		int m_MIN_BAS_LENGTH;
		long m_MIN_BAS_REP_LENGTH;

		// long LATE_THRESH = 20 * (long)m_dDefSmpFreq;
		long m_LATE_THRESH;

		/*
		* ! 10 seconds - max time lead of beginning of decel to be still associated with
		* contraction. ;
		* long m_BEFORE_CONTRACT_THRESH = 10 * (long)m_dDefSmpFreq;
		*/
		long m_BEFORE_CONTRACT_THRESH;

		/*
		* ! 10 seconds - max time lag of decel peak to contraction peak to be considered
		* in phase. ;
		* long m_PEAK_PHASE_THRESHOLD = 10 * (long)m_dDefSmpFreq;
		*/
		long m_PEAK_PHASE_THRESHOLD;

		long m_MAX_TIME_AFTER_CONTRACTION;		// for contraction association with bump
		long m_MAX_TIME_BEFORE_CONTRACTION;

		double m_dMAXDELAY;						// must set constructor TBD KW 23/NOV/05

		long m_lDecelRateWindow;				// size of window over which to measure decel rate for bump feature
		long m_lMaxSurrDecelDist;				// max. distance of nextDecelX1 or lastDecelX2 for bump feature

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Public functions
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:
		bool m_AllowConfiguration;

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void AllowShortAppend(bool bAllow)
		{
			m_bShortAppend = bAllow;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool IsShortAppendAllowed(void)
		{
			return m_bShortAppend;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetMinSamplesAppend(long nSamples)
		{
			m_lMinSamplesAppend = nSamples;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetMaxSamplesAppend(long nSamples)
		{
			m_lMaxSamplesAppend = nSamples;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMinSamplesAppend(void)
		{
			return m_lMinSamplesAppend;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMaxSamplesAppend(void)
		{
			return m_lMaxSamplesAppend;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool RemoveRepairCandidates(void)
		{
			return m_bRemoveRepairCandidates;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool RemoveRepairOutput(void)
		{
			return m_bRemoveRepairOutput;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetRemoveRepairOutput(bool b)
		{
			m_bRemoveRepairOutput = b;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double MaxBumpRepOneSlope(void)
		{
			return m_dMaxBumpRepOneSlope;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double MaxBumpRep2Slopes(void)
		{
			return m_dMaxBumpRep2Slopes;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double MaxPercSlopeRep(void)
		{
			return m_dMaxPercSlopeRep;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double MaxPercSlopeRep2(void)
		{
			return m_dMaxPercSlopeRep2;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long MinRepLengthFilterVar(void)
		{
			return m_lMinRepLengthFilterVar;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long MinProlongedLength(void)
		{
			return (long) (m_lMinProlongedLenSec * m_dDefSmpFreq);
		}

		int ReadINIFile(string filename);
		long ReadLowPassFilterFile(string filename);
		long Read1stVarPassFilterFile(string filename);
		long Read2ndVarPassFilterFile(string filename);

		//
		// ===============================================================================================================
		//    Repair process related property accessors.
		// ===============================================================================================================
		//
		double GetDefSmpFreq(void) const
		{
			return m_dDefSmpFreq;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMinAmplitude(void) const
		{
			return m_dMinAmplitude;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxAmplitude(void) const
		{
			return m_dMaxAmplitude;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxDiffInSegPts(void) const
		{
			return m_dMaxDiffInSegPts;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxDiffInArtifPts(void) const
		{
			return m_dMaxDiffInArtifPts;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMinSlope(void) const
		{
			return m_dMinSlope;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxSlope(void) const
		{
			return m_dMaxSlope;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMinDiffOverlapFHR(void) const
		{
			return m_dMinDiffOverlapFHR;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxTmWrongFHR(void) const
		{
			return m_dMaxTmWrongFHR;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxTmMergeRepair(void) const
		{
			return m_dMaxTmMergeRepair;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxTmCorrelRepair(void) const
		{
			return m_dMaxTmCorrelRepair;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxTmLookAhead(void) const
		{
			return m_dMaxTmLookAhead;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetTmShortFHR(void) const
		{
			return m_dTmShortFHR;
		}

		// XXX
		int SetDefSmpFreq(double dDefSmpFreq);
		int SetMinMaxAmplitudes(double dMinAmplitude, double dMaxAmplitude);
		int SetMaxDiffInSegPts(double dMaxDiffInSegPts, double dMaxDiffInArtifPts = 0); // Where 0 means "same value as first param.".
		int SetMaxTmWrongFHR(double dMaxTmWrongFHR);
		int SetMinMaxSlopes(double dMinSlope, double dMaxSlope);
		int SetMinDiffOverlapFHR(double dMinDiffOverlapFHR);
		int SetMaxTmMergeRepair(double dMaxTmMergeRepair);
		int SetMaxTmCorrelRepair(double dMaxTmCorrelRepair);
		int SetMaxTmLookAhead(double dMaxTmLookAhead);
		int SetTmShortFHR(double dTmShortFHR);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetDegeneratePrecision(void) const
		{
			return m_DegeneratePrecision;
		}

		int SetDegeneratePrecision(double dDegeneratePrecision);

		// Error message functions.
		string ErrorMessage(int ErrorCode);
		string Fmt(const char *FormatTemplate, double d1);
		string Fmt(const char *FormatTemplate, double d1, double d2);

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		string GetXMLFilePrefix(void)
		{
			return m_strXMLFilePrefix;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetXMLFilePrefix(string strPrefix)
		{
			m_strXMLFilePrefix = strPrefix;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		int GetFFTLibrary(void)
		{
			return (int) m_eFFTLibrary;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetFFTLibrary(int eFFTLibrary)
		{
			m_eFFTLibrary = (FFTLibrary) eFFTLibrary;
		};

		//
		// ===============================================================================================================
		//    NNet_Multihypothesis process related properties.
		// ===============================================================================================================
		//
		void UseFileNNET_MultiHypothesis(bool bUse)
		{
			m_bUseFileNNET_Multihypothesis = bUse;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool IsUsingFileNNET_MultiHypothesis(void)
		{
			return m_bUseFileNNET_Multihypothesis;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetPrimaryNNETFile(void)
		{
			return m_strPrimaryNNETFile;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetResourceHandleModuleName(void)
		{
			return m_strResourceHandleModuleName;
		};
		

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetSecondaryNNETFile(void)
		{
			return m_strSecondaryNNETFile;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetPrimaryNNETFile(string strFileName)
		{
			if (!m_AllowConfiguration)
			{
				return;
			}

			m_strPrimaryNNETFile = strFileName;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetResourceHandleModuleName(string strFileName)
		{
			if (!m_AllowConfiguration)
			{
				return;
			}

			m_strResourceHandleModuleName = strFileName;
		};
		

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetSecondaryNNETFile(string strFileName)
		{
			if (!m_AllowConfiguration)
			{
				return;
			}

			m_strSecondaryNNETFile = strFileName;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetNumberOfExperts(void)
		{
			return m_nNumberofExperts;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		NeuralNet *GetNnetExperts(void)
		{
			return m_nnetExperts;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		NeuralNet *GetNnetArbiter(void)
		{
			return &m_nnetArbiter;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetNumberOfSecondaryExperts(void)
		{
			return m_nNumberofSecondaryExperts;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		NeuralNet *GetNnetSecondaryExperts(void)
		{
			return m_nnetSecondaryExperts;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		NeuralNet *GetNnetSecondaryArbiter(void)
		{
			return &m_nnetSecondaryArbiter;
		};
		bool LoadMultihypothesisTopology(void);

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetPathToTestFiles(string str)
		{
			m_strRootPath = str;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetPathToTestFiles(void)
		{
			return m_strRootPath;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void UseFileNNET_AccelDecels(bool bUse)
		{
			m_bUseFileNNET_AccelDecel = bUse;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool IsUsingFileNNET_AccelDecel(void)
		{
			return m_bUseFileNNET_AccelDecel;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetNumberOfFreqBands(int nBands)
		{
			m_iBands = nBands;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetNumberOfFreqBands(void)
		{
			return m_iBands;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetPathToSaveCandidateBumps(string str)
		{
			m_strPathToSaveCandidateBumps = str;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetPathToSaveCandidateBumps(void)
		{
			return m_strPathToSaveCandidateBumps;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetPathToSaveBumpsClassify(string str)
		{
			m_strPathToSaveBumpsClassify = str;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetPathToSaveBumpsClassify(void)
		{
			return m_strPathToSaveBumpsClassify;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		string GetAccelNNETFile(long iteration = 1)
		{
			string str = m_strAccelNNETFile;

			if (iteration == 2)
			{
				str =  m_strAccelNNETFile2;
			}
			return str;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		string GetDecelNNETFile(long iteration = 1)
		{
			string str = m_strDecelNNETFile;
			if (iteration == 2)
			{
				str = m_strDecelNNETFile2;
			}
			return str;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetAccelNNETFile(string strFileName, long iteration = 1)
		{
			if (!m_AllowConfiguration)
			{
				return;
			}
			if (iteration == 1)
			{
				m_strAccelNNETFile = strFileName;
			}
			else if (iteration == 2)
			{
				m_strAccelNNETFile2 = strFileName;
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetDecelNNETFile(string strFileName, long iteration = 1)
		{
			if (!m_AllowConfiguration)
			{
				return;
			}

			if (iteration == 1)
			{
				m_strDecelNNETFile = strFileName;
			}
			else if (iteration == 2)
			{
				m_strDecelNNETFile2 = strFileName;
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		int GetNumberOfAccelExperts(void)
		{
			return m_nNumberOfAccelExperts;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetNumberOfDecelExperts(void)
		{
			return m_nNumberOfDecelExperts;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		NeuralNet *GetAccelExperts(long iteration = 0);
		NeuralNet *GetDecelExperts(long iteration = 0);
		
		/*
		===============================================================================================================
		===============================================================================================================
		*/

		int GetNumberOfAccelStats(void)
		{
			return m_nAccelStats;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetNumberOfDecelStats(void)
		{
			return m_nDecelStats;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		NNStats *GetAccelStats(void)
		{
			return m_AccelStats;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		NNStats *GetDecelStats(void)
		{
			return m_DecelStats;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetLENGTH_THRESHOLD(void)
		{
			return m_LENGTH_THRESHOLD;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetMinBaseLineLength(void)
		{
			return m_MIN_BAS_LENGTH;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMinBasRepIntLength(void)
		{
			return m_MIN_BAS_REP_LENGTH;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetRepairBuffer(void)
		{
			return m_dRepairBuffer;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMaxRepairForCommitIndexForce(void)
		{
			return (long) ((GetRepairBuffer() + m_dMaxRepairBeforeCommit) * m_dDefSmpFreq);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxPercRepairForOutput(void)
		{
			return(m_dMaxPercRepairForOutput);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetBEFORE_CONTRACT_THRESHOLD(void)
		{
			return m_BEFORE_CONTRACT_THRESH;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetPEAK_PHASE_THRESHOLD(void)
		{
			return m_PEAK_PHASE_THRESHOLD;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetLATE_THRESHOLD(void)
		{
			return m_LATE_THRESH;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMAX_TIME_BEFORE_CONTRACT(void)
		{
			return m_MAX_TIME_BEFORE_CONTRACTION;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMAX_TIME_AFTER_CONTRACT(void)
		{
			return m_MAX_TIME_AFTER_CONTRACTION;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long GetDecelRateWindow(void)
		{
			return m_lDecelRateWindow;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMaxSurrDecelDist(void)
		{
			return m_lMaxSurrDecelDist;
		};

		//
		// ===============================================================================================================
		//    void SetFFilter([in] int, [in] string)
		// ===============================================================================================================
		//
		int SetMaxDelay(double dValue)
		{
			m_dMAXDELAY = dValue;
			return CES_OK;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMaxDelay(void)
		{
			return m_dMAXDELAY;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		int NumBandsAccel(void)
		{
			return m_iBandsAccel;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int NumBandsDecel(void)
		{
			return m_iBandsDecel;
		};

		int SetLongestBandPassDelay(void);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetLongestBandPassDelay(void)
		{
			return m_lLongestBPdelay;
		};

		// in samples
		int SetLongestBumpCandLength(void);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetLongestBumpCandLength(void)
		{
			return m_lMaxBumpCandLength;
		};

		// in seconds
		int SetMaxDelayBasedOnBPFilters(void);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int SetMinBumpCandLength(double dValue)
		{
			m_dMinBumpCandLength = dValue;
			return CES_OK;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMinBumpCandLength(void)
		{
			return m_dMinBumpCandLength;
		};

		long GetFilterVarDelay(void) {return ((m_iFir_Flt8_Size / 2) + (m_iFir_Flt4_Size / 2));}
		long GetMinVarLookAhead(void) {return (long) (m_lMinVarLookAhead * m_dDefSmpFreq);} // return in samples
		long GetMinVarDelay(void) {return (GetFilterVarDelay() + GetMinVarLookAhead());}

		// in seconds

		long GetReqBaseLinesHistory(void);	// return in samples
		long GetReqAccelDecelHistory(void); // return in samples
		long GetAccExtBuffer(void);			// return in samples
		long GetEarliestEventBegin(); // eliminate events before require warmup at very start of tracing (in samples)

		long GetBasTrimForVarCalc(void);	// return in samples
		long GetBasSegLenForVarCalc(void);	// return in samples

		vector<double>& GetFlt_HP25Coeffs() { return m_filterCoefsHP25; }

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool IsBasVarCalcActivated(void)
		{
			return m_bCalcBasVar;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void ActivateBasVarCalc(bool b = true)
		{
			m_bCalcBasVar = b;
		};

		long GetNumExtrapolatedSignal() {return m_lNumExtrapSignal;};
		void SetNumExtrapolatedSignal(long n) { m_lNumExtrapSignal = n;};
		bool UsingPendingRT() {return m_bPendingRT;};
		void UsePendingRT(bool b);
		long GetNumAccelDecelIterations() {return m_lNumAccelDecelIterations;};
		void SetNumAccelDecelIterations(long n) {m_lNumAccelDecelIterations = n;};

		void SetNNtrainingParams(void); // set req params for NN training (only call if want to train)

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void DisableRepair(bool b)
		{
			m_bDisableRepair = b;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetFBandLowFilterFile(int nBand, string strFileName)
		{
			if (!m_AllowConfiguration)
			{
				return;
			}

			if (strFileName == "")
			{
				return;
			}

			if (m_pBands != NULL && nBand >= 0 && nBand <= 5)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				CAccelDecelBandPassConfig *cpACBPC;
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				cpACBPC = &(m_pBands[nBand]);
				cpACBPC->ReadLowPassFilterFile(strFileName);
			}
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetFBandHighFilterFile(int nBand, string strFileName)
		{
			if (!m_AllowConfiguration)
			{
				return;
			}

			if (strFileName == "")
			{
				return;
			}

			if (m_pBands != NULL && nBand >= 0 && nBand <= 5)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				CAccelDecelBandPassConfig *cpACBPC;
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				cpACBPC = &(m_pBands[nBand]);
				cpACBPC->ReadHighPassFilterFile(strFileName);
			}
		};

		bool LoadBumpClassifyTopology(void);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CAccelDecelBandPassConfig *GetBandPassConfig(int ind)
		{
			return &(m_pBands[ind]);
		};

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Protected functions
		// -------------------------------------------------------------------------------------------------------------------
		//
	protected:
		// NNet_Multihypothesis process related properties.
		bool LoadTopologyFromResource(const char *qpszName, const char *qpszType, bool bPrimary);
		bool LoadTopologyFromFile(string FileName, bool bPrimary);
		bool ParseTopology(string strExpert, string strPath, bool bPrimary = true, bool bResource = true);
		bool ValidateTopology(bool bPrimary = true);

		// Accel/Deccel process related properties.
		bool LoadAccelDecelTopologyFromResource(const char *qpszName, const char *qpszType, long iteration, bool bAccel);
		bool LoadAccelDecelTopologyFromFile(string FileName, long iteration, bool bAccel);
		bool ParseAccelDecelTopology(string strExpert, string strPath,  long iteration, bool bAccel = true, bool bResource = true);
		bool ValidateAccelTopology(void);
		bool ValidateDecelTopology(void);
	};
}
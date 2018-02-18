/*
* ! CLASSES: CRepairConfig CRepairInterval CRepairSignal Copyright LMS Medical
* Systems 2006
*/
#pragma once
#include "stdafx.h"
#include "fhrPart.h"
#include "fhrPartSet.h"

namespace patterns
{

	//
	// =======================================================================================================================
	//    include "DigitalSignal.h"
	// =======================================================================================================================
	//
	class CRepairConfig
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CRepairConfig(void);
		~ CRepairConfig(void);

		double m_dSmpFreq;						// sampling frequency

		// MAXIMUM JUMPS, VALUES, SLOPES
		int m_iDiffMaxIn;
		double m_dMaxPosSlopeConst;				// constant part of max slope (not including parametrized part)
		double m_dMaxNegSlopeConst;
		double m_dMinPosSlope;					// absolute minimum allowable slope
		double m_dMinNegSlope;
		double m_dMaxCurrSlope;					// max abs value of current slope estimate for parametrizing allowable entry slope
		int m_iMinFhrVal;						// min fhr value when looking for valid fhr after dropout
		int m_iMaxFhrVal;

		// ARTIFACT VERIFY
		int m_iMaxNonArtifactDiff;				// max jump to be considered for potential non-artifact
		double m_dMaxLenSmallJumpArtifact;		// if longer than this before reverse jump then non artifact (in seconds)
		int m_iMinJumpBackNonArtifact;

		// LOOKING FOR TRUE ARTIFACT START POINT
		int m_iArtifactRewMaxConsecOK;			// number of consec 'OK' samples before give up trying to rewind
		int m_iArtifactRewMinFhrDiff;			// fhr diff value that constitutes not 'OK'
		int m_iArtifactRewFhrDiff;				// min fhr diff value to actually rewind start point of artifact

		int m_iMaxFhrDistFromLastGood;			// can overide iMinFhrVal or iMaxFhrVal

		double m_dSlopeMeasureTime;				// in seconds

		// DROPOUTS
		int m_iDropoutReturnMaxDiff;			// for finding true exit point of dropout
		int m_iDropoutEntryMaxDiff;				// for finding true entry point of dropout
		int m_iRecoveryTimeLookAhead;
		int m_iMinReturnFromDropout;			// min length of return segment (in seconds)
		int m_dSlopeMultFactor;					// mult factor for currSlope in determining parametrized part of allowable slope
		int m_iMinReturnWithInitSlopeFail;
		int m_iMinReturnIgnoreExitSlope;
		int m_iMaxDiffIgnoreExitSlope;
		int m_iMaxGapPostReturnForExitSlopeCheck;
		int m_iMaxDiffFromLastGoodForExitBlock;
		int m_iMaxDropoutLengthIgnore;			// if dropout is less than this can ignore (given certain conditions)
		int m_iMaxFhrDiffIgnoreDropout;			// ignore short dropout if less than this diff at return
		int m_iMaxFhrDiffAlwaysIgnoreDropout;	// ignore short dropout if less than this even if return is short

		// OTHER ARTIFACT
		int m_iMaxArtifactLength;
		int m_iMaxShortRepair;
		int m_iMaxRepairAbsDiff;
		int m_iMaxRepairSlopeDiff;
		int m_iMaxFactorSlopeOverAbs;

		// BUFFER AT END OF WINDOW - for RT want to set this, for BATCH can set to 0
		double m_dRepairBuffer;					// in seconds

		void SetSmpFreq(double dSmpFreq);
	};

	class CRepairSignal
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CRepairSignal(void);
		~ CRepairSignal(void);

		double *m_pSignalToRepair;
		long m_lSignalLength;
		long m_lRealSignalLength;

		// double* m_pRepairedSignal;
		// long m_lRepairedSignalLength;
		fhrPartSet m_pRepairIntervals;

		void SetSignalToRepair(double *pdFHR, long lFhr);
		void SetRealSignalLength(long n) {m_lRealSignalLength = n;}
		void AddRepairInterval(long lX1, long lX2);
		long GetNumRepairIntervals(void);
		fhrPartSet *GetRepairIntervals(void);
		void RepairSignal(long lIndexStart);
		bool IsDropout(double *pSignal, long lIndex);
		long FindTrueDropoutStart(long lIndex);
		long FindTrueArtifactStart(long lIndex);
		bool VerifyArtifact(long lIndex);
		double GetDiffFhr(long lIndex);
		double GetCurrSlope(long lIndex, long lTimeBack);
		double GetSmpFreq(void);
		bool CheckReturnFromDropout(void);
		long FindTrueReturnFromDropout(long lIndex);
		int GetReturnChunkLength(long lIndex);
		bool CheckShortDropout(long lIndex, int iDropoutLength, int iSigTime);
		bool CheckJumpSlope(long lLineBegin, long lLineEnd, double dInstSlope);
		double SlopeBtwnTwoPoints(long lLineBegin, long lLineEnd);
		bool CheckExitSlope(long lIndex, int iSigTime);
		bool ValidReturnSignalForExitSlope(long lIndex);
		bool GetReturnFromInterference(void);
		bool CheckShortRepair(long lIndex, double dCurrDiff);
		bool SignalInRange(long lIndex);
		long GetLastGoodIndex(void);
		void ApplyRepair(void);
		void FillInPoints(double dSlope, double dStartVal);
		void ApplyRepairEnd(void);

		void SetRepairBuffer(double dBufferInSec);
		int GetRepairBuffer(void);	// returns in samples

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	private:
		CRepairConfig *m_pConfig;	// repair config

		bool m_bInArtifact;
		bool m_bDropout;
		bool m_bFailSlope;
		bool m_bApplyRepair;

		double m_dLastGoodFhr;
		long m_lLastGoodIndex;
		double m_dBeforeLastGoodFhr;

		double m_dMaxFhr;
		double m_dMinFhr;

		long m_lLastRepairStart;
		long m_lCurrIndex;
		long m_lLineBegin;
		long m_lLineEnd;
		double m_dCurrSlope;
		double m_dJumpIn;

		int m_iLastDropoutLength;
		int m_iLastSigTime;
	};
}
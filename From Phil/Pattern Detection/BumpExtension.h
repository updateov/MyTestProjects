/*
* ! CLASSES: CBumpExtendSet CBumpExtend CBumpExtendConfig Copyright LMS Medical
* Systems 2006
*/
#pragma once
#include "stdafx.h"
#include "BaseLine.h"
#include "DigitalSignal.h"
#include "fhrPart.h"
#include "fhrPartSet.h"

namespace patterns
{

	class CBumpExtendSet;
	class CBumpExtend;
	class CBumpExtendConfig;

	/*
	=======================================================================================================================
	! CLASS: CBaseLine DESCRIPTION: Description of a baseline structure.
	=======================================================================================================================
	*/
	class CBumpExtend	// : public CObject
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CBumpExtend(void);
		~ CBumpExtend(void);

		bump *m_pBump;
		const double *m_pLpFhr; // low pass fhr buffer
		long m_lNumFhrSamples;	// number of low pass fhr samples in buffer
		baseline m_RefBas;
		double m_dRefBasLevel;	// reference baseline level
		fhrPartSet *m_pRepairIntervals;	// repair intervals so don't extend into repair
		long m_lPrevBumpX2;						// previous bump x2 - can not extend further than this
		CBumpExtendConfig *m_pConfig;			// bump extension config
		fhrPartSet *m_pRefBaseLines;
		double m_dBumpSign;						// +1 or -1 so can use same routines on accel and decel

		void SetBump(bump *Bump);
		void SetBumpSign(double dBumpSign);
		void SetLPfhr(const double *dLpFhr, long lSamples);
		void SetPrevBumpX2(long lX2);
		void SetRefBaseLines(fhrPartSet *pRefBas);
		void SetRepairIntervals(fhrPartSet *pRepInt);
		void SetConfig(CBumpExtendConfig *pConfig);
		bool ExtendBump(void);
		bool ExtendBumpBack(void);
		bool ExtendBumpForward(void);
		bool CheckRepairSegmentBack(long lX1);
		bool CheckRepairSegmentForward(long lX2);
		bool CheckRepairFromIntervals(long lX);
		bool CheckUpToBaseLine(long lX1, long lX2);
		bool CheckLocalMaxBack(long *lX1, int iPushStep);
		bool CheckLocalMaxForward(long *lX2, int iPushStep);
		long GetMaxLPIndexInInterval(long lX1, long lX2);
		bool CheckFlatnessBack(long lX1);
		bool CheckFlatnessForward(long lX2);
		double GetSimpleSlopeBack(long lXPush, long lX1);
		double GetSimpleSlopeForward(long lXPush, long lX2);
		bool GetRefBasBefore(long lX1);
		bool GetRefBasAfter(long lX2);
		bool ResetBasIfOverlap(long lX1);
		bool ResetBasIfOverlapForward(long lX2, long lX1);
		double GetLPSample(long lIndex);
		bool IsFarEnoughBelowRefBas(long lIndex);
		long GetMinRefBasLen(void);
		long GetMinRepairLen(void);
		long GetLocalMaxSearchDist(void);
	};

	class CBumpExtendSet
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CBumpExtendSet(void);
		CBumpExtendSet(double dSmpFreq);
		~ CBumpExtendSet(void);
		
		fhrPartSet *m_pBumps;                  // bump set to extend - these should be of same type (accel or decel)
		long m_lFirstPrevBumpX2;				// x2 of accepted bump prior to first bump in set (so don't extend into it)
		fhrPartSet m_pRefBaseLines;				// local copy
		fhrPartSet m_pRepairIntervals;	// repair intervals so don't extend into repair
		const double *m_pLPfhr;
		long m_nLPfhr;

		// double m_dSmpFreq;
		CBumpExtendConfig *m_pConfig;			// bump extension config

		void SetRefBas(fhrPartSet *pRefBas);
		void SetRefSignal(const double *pFHR, long nFHR);
		void SetBumps(fhrPartSet *Bumps);
		void SetRepairIntervals(fhrPartSet *pRepInts, long lOffset);
		void SetFirstPrevX2(long lX2);
		void Extend(void);
		long GetMinRefBasLen(void);
		void SetMinRefBasLenSec(long n);

		// void SetSmpFreq(double dSmpFreq);
	};

	class CBumpExtendConfig
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CBumpExtendConfig(void);
		~ CBumpExtendConfig(void);

		double m_dSmpFreq;				// sampling frequency

		long m_lMaxLookBack;			// maximum number of seconds can extend backwards
		long m_lMaxLookForward;			// max number of seconds can extend forwards

		double m_dBasLevelPerc;			// perc. of bas level need to reach for stopping condition
		int m_iPushStepStart;			// number of sec. in extension step at start
		int m_iPushGranularity;			// smallest granularity of push step (in samples)
		int m_iMinRefBaseLineLen;		// min length of reference baseline (seconds)
		int m_iMinBasLen;				// min lenght of baseline (for degeneracy) in seconds
		double m_dMaxPercentOverlap;	// max overlap between bump and prev. to allow truncate and keep

		double m_dMaxLocalMaxDiffFromBas;
		int m_iLocalMaxSearchArea;
		int m_iMinFlatLength;
		double m_dMaxFlatSlope;
		int m_iInstSlopeSegLen;
		double m_dMinInstSlope;

		double m_dMinRepairPerc;
		int m_iMinRepairLen;
		bool m_bUseRepairIntervals;

		int m_iFlatnessRewind;
		int m_iLocalMaxRewind;
		int m_iRepairRewind;
		int m_iUptoBasRewind;

		double m_dMaxRefBasLevelOverX1;

		void SetSmpFreq(double dSmpFreq);
		long GetMinRefBasLen(void) {return (long) (m_dSmpFreq * m_iMinRefBaseLineLen);}
		void SetMinRefBasLenSec(long n) {m_iMinRefBaseLineLen = n;}
	};
}
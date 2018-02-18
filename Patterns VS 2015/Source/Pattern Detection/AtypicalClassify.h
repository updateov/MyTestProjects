/*!

CLASSES: 
	CAtypicalClassify
	CAtypicalConfig

Copyright LMS Medical Systems 2006 
*/


#pragma once

#include "stdafx.h"
#include <math.h> 
#include <algorithm> 
#include "BaseLine.h" 
#include "DigitalSignal.h"
#include "fhrPart.h"
#include "fhrPartSet.h"


//class CAtypicalClassify;
//

namespace patterns
{
	class CAtypicalConfig;;

	class CAtypicalClassify 
	{
	public:
		CAtypicalClassify();
		~CAtypicalClassify();

	protected:

		fhrPartSet* m_Bumps; // bumps to classify - more efficient if do as group
		const double* m_pLpFHR; // low pass fhr 
		const double* m_pFHR; // raw (repaired) FHR
		long m_nLP; // number of low pass fhr samples 
		long m_nFHR; // number of raw samples
		double m_dSmpFreq;
		long m_basIndex; // index to speed up baseline search when doing multiple bumps
		long m_decIndex; // index to speed up P2 seach when doing multiple bumps
		long m_repIntIndex;
		double m_dVarScore; // variability score for decels
		fhrPartSet* m_pRepairIntervals; // repair intervals so don't extend into repair
		long m_nRepairIntervals;
		CAtypicalConfig* m_pConfig; // bump extension config
		fhrPartSet* m_pRefBaseLines; // set of reference baselines

	public:

		void SetBumps					(fhrPartSet* Bumps);
		void SetFHR						(const double* dFHR, long n);
		void SetLpFHR					(const double* dLpFHR, long n);
		void SetRefBaseLines			(fhrPartSet* pRefBas);
		void SetRepairIntervals			(fhrPartSet* pRepInt);
		void SetSampFreq				(double f) {m_dSmpFreq = f;};
		void AtypicalClassify			(bool = false);

	protected:
		
		bool AtypicalPreFilter			(decel &d);
		bool CheckTroughBack			(const double *d, long x1, long x2, double minD, long minTroughLen, long* r1, long* r2);
		bool CheckTroughForward			(const double *d, long x1, long x2, double minD, long minTroughLen, long* r1, long* r2);
		bool FindBasAfterGap			(decel &d, long minGap, long maxGap, long* b1, long* b2);
		bool GetBasImmedBeforeAndAfter  (decel &d, long maxDist, baseline* b1, baseline* b2);
		void GetBiggestBump				(const double *d, long x1, long x2, long minHumpLen, long* b1, long* b2);
		double GetDecVarScore			(decel &d);
		double GetDecVarScore			(decel &d, double* zcScoreOut, double* wzcScoreOut);
		int GetDoubleSign				(double d);
		double* GetFhrDiff				(decel &d);
		double* GetFhrDiff				(long x1, long x2);
		double GetRise2Height			(long x1, long x2, long* p);
		double GetRiseVarScore			(long x1, long x2);
		double GetWeightedZC			(double* d, long n, long peakIndex, long* zc, long nzc);
		double GetWeightedZCRise		(double* d, long n, long peakIndex, long* zc, long nzc);
		void GetZC						(double* d, long n, long** zc, long* nzc);
		double* GetZCDev				(double* d, long n, long* zc, long nzc);
		bool HasRepair					(long x1, long x2);
		bool IsBiphasic					(decel &d);
		bool IsLossRise					(decel &d);
		bool IsLossVar					(decel &d);
		bool IsLowerBas					(decel &d);
		bool IsProlSecRise				(decel &d);
		bool IsSlowReturn				(decel &d);
		bool IsSixties					(decel &d);
		double MaxDouble				(double* d, long a, long b);
		double MeanDouble				(double* d, long a, long b);
		double MeanDouble				(double* d, long a, long b, double* t);
		double MeanDouble				(const double* d, long a, long b);
		double MeanDouble				(const double* d, long a, long b, double* t);
		double MinDouble				(double* d, long a, long b);
		double PercDouble				(const double* d, long a, long b, double p);
		void SortDouble					(double* d, long n);
		double SumDouble				(double* d, long a, long b);
	};

	class CAtypicalConfig
	{
	public:
		CAtypicalConfig();
		~CAtypicalConfig();

		double m_dSmpFreq; // sampling frequency
		double m_dMinLengthSec;
		double m_dMinDepth;
		long m_lMinBaseLineLenSamps;
		long m_lLPfilterDelaySamps;

		// Biphasic
		double m_dBiphasicTrim;
		double m_dBiphasicMinHumpLenSec;
		double m_dBiphasicMinTroughLenSec;
		double m_dBiphasicHumpMinHeight;

		// Loss Rise
		double m_dLossRiseMinHumpLenSec;
		double m_dLossRiseMinTroughLenSec;
		double m_dLossRiseMinHeight;
		double m_dLossRiseMinAboveBas;
		double m_dLossRiseMaxIntoBasSec;

		// Prolonged Sec Rise
		double m_dProlongedRiseMinHumpLenSec;
		double m_dProlongedRiseMinTroughLenSec;
		double m_dProlongedRiseMinHeight;
		double m_dProlongedRiseMinAboveBas;

		// Lower Baseline
		double m_dMinLowerBasDiff;
		double m_dMaxPrevBasAgeSec;

		// Slow Return
		double m_dSlowReturnLagSec;

		void SetSmpFreq					(double dSmpFreq);	
		void SetMinBaseLineLen			(long lMinLen);
		void SetLPFilterDelay			(long lDelay);
		double GetMinLength				() {return (long) (m_dSmpFreq * m_dMinLengthSec); };
		long GetMinBiphasicHumpLen		() {return (long) (m_dSmpFreq * m_dBiphasicMinHumpLenSec); };
		long GetMinBiphasicTroughLen	() {return (long) (m_dSmpFreq * m_dBiphasicMinTroughLenSec); };
		long GetLossRiseMinHumpLen		() {return (long) (m_dSmpFreq * m_dLossRiseMinHumpLenSec); };
		long GetLossRiseMinTroughLen	() {return (long) (m_dSmpFreq * m_dLossRiseMinTroughLenSec); };
		long GetLossRiseMaxIntoBas		() {return (long) (m_dSmpFreq * m_dLossRiseMaxIntoBasSec); };
		long GetProlongedRiseMinHumpLen () {return (long) (m_dSmpFreq * m_dProlongedRiseMinHumpLenSec); };
		long GetProlongedRiseMinTroughLen() {return (long) (m_dSmpFreq * m_dProlongedRiseMinTroughLenSec); };
		long GetMaxPrevBasAge			() {return (long) (m_dSmpFreq * m_dMaxPrevBasAgeSec); };
		long GetMinSlowReturnLag		() {return (long) (m_dSmpFreq * m_dSlowReturnLagSec); };
		long GetMinBaseLineLen			() {return (m_lMinBaseLineLenSamps);};

	};

}





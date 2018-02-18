/*
* ! PROJECT: LMS -- Repair Module Copyright LMS Medical Systems 2006 This code is
* based on MATLAB code in src-test\autoMarking\Online\Preprocessing\
* fhr_repair_offline(_new).m getRepairConfig.m
*/
#include "stdafx.h"
#include "Repair.h"

namespace patterns
{

	//
	// =======================================================================================================================
	//    CRepairConfig
	// =======================================================================================================================
	//
	CRepairConfig::CRepairConfig(void)
	{
		m_dSmpFreq = 4.0;						// sampling frequency

		// MAXIMUM JUMPS, VALUES, SLOPES
		m_iDiffMaxIn = 20;
		m_dMaxPosSlopeConst = 2.0;				// constant part of max slope (not including parametrized part)
		m_dMaxNegSlopeConst = 2.0;
		m_dMinPosSlope = 1.5;					// absolute minimum allowable slope
		m_dMinNegSlope = 1.5;
		m_dMaxCurrSlope = 1.0;
		m_iMinFhrVal = 60;						// min fhr value when looking for valid fhr after dropout
		m_iMaxFhrVal = 190;

		// ARTIFACT VERIFY
		m_iMaxNonArtifactDiff = 30;				// max jump to be considered for potential non-artifact
		m_dMaxLenSmallJumpArtifact = 5.0;		// if longer than this before reverse jump then non artifact (in seconds)
		m_iMinJumpBackNonArtifact = 15;

		// LOOKING FOR TRUE ARTIFACT START POINT
		m_iArtifactRewMaxConsecOK = 3;			// number of consec 'OK' samples before give up trying to rewind
		m_iArtifactRewMinFhrDiff = 5;			// fhr diff value that constitutes not 'OK'
		m_iArtifactRewFhrDiff = 10;				// min fhr diff value to actually rewind start point of artifact

		m_iMaxFhrDistFromLastGood = 5;			// can overide iMinFhrVal or iMaxFhrVal

		m_dSlopeMeasureTime = 2.5;				// in seconds

		// DROPOUTS
		m_iDropoutReturnMaxDiff = 30;			// for finding true exit point of dropout
		m_iDropoutEntryMaxDiff = 15;			// for finding true entry point of dropout
		m_iRecoveryTimeLookAhead = 30;
		m_iMinReturnFromDropout = 5;			// min length of return segment (in seconds)
		m_dSlopeMultFactor = 5;					// mult factor for currSlope in determining parametrized part of allowable slope
		m_iMinReturnWithInitSlopeFail = 20;
		m_iMinReturnIgnoreExitSlope = 20;
		m_iMaxDiffIgnoreExitSlope = 15;
		m_iMaxGapPostReturnForExitSlopeCheck = 5;
		m_iMaxDiffFromLastGoodForExitBlock = 15;
		m_iMaxDropoutLengthIgnore = 3;			// if dropout is less than this can ignore (given certain conditions)
		m_iMaxFhrDiffIgnoreDropout = 25;		// ignore short dropout if less than this diff at return
		m_iMaxFhrDiffAlwaysIgnoreDropout = 10;	// ignore short dropout if less than this even if return is short

		// OTHER ARTIFACT
		m_iMaxArtifactLength = 15;				// in seconds
		m_iMaxShortRepair = 3;					// in seconds
		m_iMaxRepairAbsDiff = 10;
		m_iMaxRepairSlopeDiff = 12;
		m_iMaxFactorSlopeOverAbs = 2;

		m_dRepairBuffer = 30.0;					// 30 second buffer at end for redoing in next window when doing RT
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CRepairConfig::~CRepairConfig(void)
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CRepairConfig::SetSmpFreq(double dSmpFreq)
	{
		m_dSmpFreq = dSmpFreq;
	}

	//
	// =======================================================================================================================
	//    CRepairSignal
	// =======================================================================================================================
	//
	CRepairSignal::CRepairSignal(void)
	{
		m_pConfig = new CRepairConfig;

		m_pSignalToRepair = NULL;
		m_lSignalLength = 0;
		m_lRealSignalLength = 0;

		m_bInArtifact = false;
		m_bDropout = false;
		m_bFailSlope = false;
		m_bApplyRepair = false;

		m_dLastGoodFhr = (m_pConfig->m_iMinFhrVal + m_pConfig->m_iMaxFhrVal) / 2.0;
		m_dBeforeLastGoodFhr = m_dLastGoodFhr;
		m_lLastGoodIndex = 0;

		m_dMaxFhr = m_pConfig->m_iMaxFhrVal;
		m_dMinFhr = m_pConfig->m_iMinFhrVal;

		m_lLastRepairStart = -1;
		m_lCurrIndex = 0;
		m_lLineBegin = 0;
		m_lLineEnd = 0;
		m_dCurrSlope = 0.0;
		m_dJumpIn = 0.0;

		m_iLastDropoutLength = 0;
		m_iLastSigTime = 0;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CRepairSignal::~CRepairSignal(void)
	{
		m_pRepairIntervals.clear();
		delete m_pConfig;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CRepairSignal::SetSignalToRepair(double *pdFHR, long lFhr)
	{
		m_pSignalToRepair = pdFHR;
		m_lSignalLength = lFhr;
	}

	/*
	=======================================================================================================================
	void CRepairSignal::SetRepairedSignal(double* pdFhr, long lFHR) { m_pRepairedSignal = pdFHR;
	m_lRepairedSignalLength = lFhr;
	}
	=======================================================================================================================
	*/
	void CRepairSignal::AddRepairInterval(long lX1, long lX2)
	{
		fhrPart *p = new fhrPart(lX1, lX2);
		p->setAbsCoords(false);
		m_pRepairIntervals.add(p);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CRepairSignal::GetNumRepairIntervals(void)
	{
		return m_pRepairIntervals.size();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPartSet *CRepairSignal::GetRepairIntervals(void)
	{
		return (&m_pRepairIntervals);
	}

	/*
	=======================================================================================================================
	! CRepairSignal::RepairSignal Core repair loop. Will repair signal in m_pSignalToRepair starting from sample index
	lIndexStart and write repaired signal to m_pSignalToRepair
	=======================================================================================================================
	*/
	void CRepairSignal::RepairSignal(long lIndexStart)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bUseRepairBuffer = false;					// flag to say whether end of signal repair may change
		// depending on what comes next - in these cases keep the end repair as pending
		long lMinLastGoodIndex = m_lRealSignalLength - GetRepairBuffer();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_lCurrIndex = lIndexStart;

		if (IsDropout(m_pSignalToRepair, m_lCurrIndex)) // case where first sample is dropout and second is OK
		{
			m_bDropout = true;
			m_bInArtifact = true;
			m_lLineBegin = m_lCurrIndex;
		}

		while (m_lCurrIndex < m_lRealSignalLength - 2)
		{
			if (!(m_bInArtifact))						// in seemingly good signal
			{
				if (IsDropout(m_pSignalToRepair, m_lCurrIndex + 1))
				{		// current sample is just prior to dropout
					if (m_lCurrIndex > lMinLastGoodIndex)
					{	// in buffer zone at end of window
						bUseRepairBuffer = true;	// any repair done may change so flag as unstable
					}

					m_bDropout = true;
					m_bInArtifact = true;
					m_lLineBegin = FindTrueDropoutStart(m_lCurrIndex);
					if ((!(bUseRepairBuffer)) && (m_lCurrIndex < m_lRealSignalLength))
					{
						m_lLastGoodIndex = m_lLineBegin;
					}
				}
				else if (abs(GetDiffFhr(m_lCurrIndex)) > m_pConfig->m_iDiffMaxIn)
				{		// current sample may be just prior to interference
					if (m_lCurrIndex > lMinLastGoodIndex)
					{	// in buffer zone at end of window
						bUseRepairBuffer = true;	// any repair done may change so flag as unstable
					}

					m_lLineBegin = FindTrueArtifactStart(m_lCurrIndex);
					if (VerifyArtifact(m_lLineBegin))
					{	// is probably artifact
						if (m_lLineBegin <= m_lLastRepairStart)
						{
							m_lLineBegin = m_lLastRepairStart + 1;	// do not start repair from same place twice - potential to get stuck in loop
						}

						m_lLastRepairStart = m_lLineBegin;
						m_dJumpIn = GetDiffFhr(m_lLineBegin);
						m_bInArtifact = true;
					}
					if ((!(bUseRepairBuffer)) && (m_lCurrIndex < m_lRealSignalLength))
					{
						m_lLastGoodIndex = m_lLineBegin;
					}
				}
				else
				{
					m_bInArtifact = false;					// current sample looks OK
					if ((!(bUseRepairBuffer)) && (m_lCurrIndex < m_lRealSignalLength))
					{
						m_lLastGoodIndex = m_lCurrIndex;	// this is for knowing when to start in a subsequent window
					}
				}

				if (m_bInArtifact)
				{
					m_dBeforeLastGoodFhr = m_dLastGoodFhr;
					if (m_lLineBegin > 0)
					{
						m_dLastGoodFhr = m_pSignalToRepair[m_lLineBegin];
					}

					m_dMaxFhr = max(m_pConfig->m_iMaxFhrVal, max(m_dBeforeLastGoodFhr, m_dLastGoodFhr) + m_pConfig->m_iMaxFhrDistFromLastGood);
					m_dMinFhr = min(m_pConfig->m_iMinFhrVal, min(m_dBeforeLastGoodFhr, m_dLastGoodFhr) - m_pConfig->m_iMaxFhrDistFromLastGood);
					m_dCurrSlope = GetCurrSlope(m_lLineBegin - 1, (long) (m_pConfig->m_dSlopeMeasureTime * GetSmpFreq()));
				}
			}
			else	// in artifact region
			{
				if (m_bDropout)
				{	// in dropout region
					m_bApplyRepair = CheckReturnFromDropout();
				}
				else
				{
					m_bApplyRepair = GetReturnFromInterference();
				}

				if (m_bApplyRepair)
				{
					ApplyRepair();
				}
			}

			// Note that m_lCurrIndex is also advanced in ApplyRepair and sometimes in
			// CheckReturnFromDropout and GetReturnFromInterferenc
			m_lCurrIndex++;
		}

		// If still in artifact at end of signal want to just hold last valid signal
		if (m_bInArtifact)
		{
			ApplyRepairEnd();
		}
	}

	/*
	=======================================================================================================================
	! CRepairSignal::IsDropout Check whether dropout occurs at index lIndex of signal. Currently just checks if signal
	is equal to 0. Returns true if dropout, false otherwise
	=======================================================================================================================
	*/
	bool CRepairSignal::IsDropout(double *pSignal, long lIndex)
	{
		return pSignal[lIndex] == 0 || pSignal[lIndex] == 255;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::FindTrueDropoutStart Check in signal immediately prior to dropout for large jumps in signal level
	in cases where true start of a dropout is actually earlier.
	=======================================================================================================================
	*/
	long CRepairSignal::FindTrueDropoutStart(long lIndex)
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		int iNumConsecOK = 0;
		long i = lIndex - 1;
		long lIndexOut = lIndex;
		/*~~~~~~~~~~~~~~~~~~~~*/

		while ((i >= 0) && (iNumConsecOK <= m_pConfig->m_iArtifactRewMaxConsecOK))
		{
			if (abs(GetDiffFhr(i)) < m_pConfig->m_iDropoutEntryMaxDiff)
			{
				iNumConsecOK++;
			}
			else
			{
				lIndexOut = i;
				iNumConsecOK = 0;
			}

			i--;
		}

		return lIndexOut;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::FindTrueArtifactStart Check in signal immediately prior to current artifact start for large jumps
	in signal level in cases where true start of an artifact is actually earlier than initially identified.
	=======================================================================================================================
	*/
	long CRepairSignal::FindTrueArtifactStart(long lIndex)
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		int iNumConsecOK = 0;
		long i = lIndex - 1;
		long lIndexOut = lIndex;
		double dCurrDiff;
		/*~~~~~~~~~~~~~~~~~~~~*/

		while ((i >= 0) && (iNumConsecOK <= m_pConfig->m_iArtifactRewMaxConsecOK))
		{
			dCurrDiff = abs(GetDiffFhr(i));
			if (dCurrDiff < m_pConfig->m_iArtifactRewMinFhrDiff)
			{
				iNumConsecOK++;
			}
			else if (dCurrDiff > m_pConfig->m_iArtifactRewFhrDiff)
			{
				lIndexOut = i;
				iNumConsecOK = 0;
			}
			else
			{	// not big enough to rewind but big enough to reset OK counter to 0
				iNumConsecOK = 0;
			}

			i--;
		}

		return lIndexOut;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::VerifyArtifact Ensure that apparent 'artifactual' jump in fhr is most likely artifact and not
	possibly just an abrupt deceleration.
	=======================================================================================================================
	*/
	bool CRepairSignal::VerifyArtifact(long lIndex)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dCurrDiff = GetDiffFhr(lIndex);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// for very large jumps, just assume artifact
		if (abs(dCurrDiff) > m_pConfig->m_iMaxNonArtifactDiff)
		{
			return true;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~*/
		bool isDec = dCurrDiff < 0;
		double dDiffSum = 0;
		long lEndIndex;
		int iTimeLook;
		/*~~~~~~~~~~~~~~~~~~~~~~~*/

		// positive spikes are more likely to be artifact (although can sometimes be
		// decel exit)
		if (!(isDec))
		{				// check for large difference in 10 consecutive samples
			lEndIndex = min(lIndex + 10, m_lSignalLength - 2);
			dDiffSum = dCurrDiff;
			for (long i = lIndex + 1; i <= lEndIndex; i++)
			{
				dDiffSum += GetDiffFhr(i);
			}

			if (dDiffSum > 2 * m_pConfig->m_iMaxNonArtifactDiff)
			{
				return true;
			}
		}

		// look for return spike in opposite direction - if none found then potentially
		// legitimate decel or accel (but in some cases long artifact)
		iTimeLook = (int) (m_pConfig->m_dMaxLenSmallJumpArtifact * GetSmpFreq());
		lEndIndex = min(lIndex + iTimeLook, m_lSignalLength - 2);

		for (long i = lIndex + 1; i <= lEndIndex; i++)
		{
			dCurrDiff = GetDiffFhr(i);
			if (isDec && (dCurrDiff > m_pConfig->m_iMinJumpBackNonArtifact))
			{
				return true;
			}
			else if (!(isDec) && (dCurrDiff < -m_pConfig->m_iMinJumpBackNonArtifact))
			{
				return true;
			}
		}

		return false;	// may be legitimate accel or decel
	}

	/*
	=======================================================================================================================
	! CRepairSignal::GetDiffFhr Return difference between next sample (lIndex + 1) and current sample (lIndex)
	=======================================================================================================================
	*/
	double CRepairSignal::GetDiffFhr(long lIndex)
	{
		return m_pSignalToRepair[lIndex + 1] - m_pSignalToRepair[lIndex];
	}

	/*
	=======================================================================================================================
	! CRepairSignal::GetCurrSlope Return average slope over last lTimeBack samples
	=======================================================================================================================
	*/
	double CRepairSignal::GetCurrSlope(long lIndex, long lTimeBack)
	{
		if (lIndex < lTimeBack)
		{
			return 0.0;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long x1 = max(0, lIndex - lTimeBack);
		double dSlope = (m_pSignalToRepair[lIndex] - m_pSignalToRepair[x1]) / (lIndex - x1);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (dSlope > m_pConfig->m_dMaxCurrSlope)
		{
			dSlope = m_pConfig->m_dMaxCurrSlope;
		}
		else if (dSlope < -m_pConfig->m_dMaxCurrSlope)
		{
			dSlope = -m_pConfig->m_dMaxCurrSlope;
		}

		return dSlope;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CRepairSignal::GetSmpFreq(void)
	{
		return m_pConfig->m_dSmpFreq;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::CheckReturnFromDropout This is the main function for dealing with repair segments due to dropout
	Returns true is found valid return from dropout at m_lCurrIndex, otherwise false. In certain cases, m_lCurrIndex
	will be advanced (i.e. if find invalid signal chunk can advance to end of it without considering every sample)
	=======================================================================================================================
	*/
	bool CRepairSignal::CheckReturnFromDropout(void)
	{
		if (IsDropout(m_pSignalToRepair, m_lCurrIndex + 1))
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dCurrFhr = m_pSignalToRepair[m_lCurrIndex + 1];
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// first condition is that there is signal in valid range
		if ((dCurrFhr >= m_dMaxFhr) || (dCurrFhr <= m_dMinFhr))
		{
			return false;
		}

		// check following sample as well
		dCurrFhr = m_pSignalToRepair[m_lCurrIndex + 2];
		if ((dCurrFhr >= m_dMaxFhr) || (dCurrFhr <= m_dMinFhr))
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lIndex = m_lCurrIndex;
		int iDropoutLength = m_iLastDropoutLength;
		int iSigTime = m_iLastSigTime;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (!(m_bFailSlope))	// flag so don't have to repeat loops if failing slope and advancing by one sample
		{
			lIndex = FindTrueReturnFromDropout(m_lCurrIndex);
			m_lCurrIndex = lIndex;						// in case true return is later and fail slope
			iDropoutLength = lIndex - m_lLineBegin - 1; // length of dropout
			m_iLastDropoutLength = iDropoutLength;
			iSigTime = GetReturnChunkLength(lIndex);	// length of return before next dropout
			m_iLastSigTime = iSigTime;
		}
		else			// failed slope criteria - advancing by one sample so increment/decrement dropoutLength/sigTime
		{
			iSigTime--; // leading sample of chunk has been considered dropout
			iDropoutLength++;
			m_iLastDropoutLength = iDropoutLength;
			m_iLastSigTime = iSigTime;
		}

		// check to see if can repair short dropout easily
		if (CheckShortDropout(lIndex, iDropoutLength, iSigTime))
		{
			m_lLineEnd = lIndex + 1;
			return true;
		}

		// if return to valid signal is shortlived, ignore
		if (iSigTime < m_pConfig->m_iMinReturnFromDropout * GetSmpFreq())	// short chunk
		{
			m_lCurrIndex = lIndex + iSigTime - 1;	// can skip ahead to end of chunk
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// if return to valid signal is slightly longer and fhr difference is big, ignore
		double dMinSigTime = (double) min((2 * m_pConfig->m_iMinReturnFromDropout * GetSmpFreq()), (0.5 * iDropoutLength));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (iSigTime < dMinSigTime)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			double dFhrDiff = abs(m_dLastGoodFhr - m_pSignalToRepair[lIndex + 1]);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (dFhrDiff > m_pConfig->m_iMaxFhrDiffIgnoreDropout)
			{
				m_lCurrIndex = lIndex + iSigTime - 1;
				return false;
			}
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lTempLineEnd = lIndex + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// check slope connecting last valid signal to start of return chunk
		if (!(CheckJumpSlope(m_lLineBegin, lTempLineEnd, m_dCurrSlope)))
		{	// failed entry slope criteria
			m_bFailSlope = true;

			if (iSigTime < m_pConfig->m_iMinReturnWithInitSlopeFail * GetSmpFreq()) // shortish chunk - discard
			{
				m_lCurrIndex = lIndex + iSigTime - 1;	// can skip to end of chunk
				m_bFailSlope = false;					// reset flag
			}

			return false;
		}
		else	// entry slope is OK - as additional check look at exit slope
		{
			if (CheckExitSlope(lIndex, iSigTime))
			{
				m_lLineEnd = lTempLineEnd;
				return true;
			}
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::FindTrueReturnFromDropout Checks to see if return from dropout condition was actually a bit
	earlier than iniatially found. This can happen in cases where valid signal immediately after a dropout is either
	very low or very high.
	=======================================================================================================================
	*/
	long CRepairSignal::FindTrueReturnFromDropout(long lIndex)
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		int iNumConsecOK = 0;
		long lIndexOut = lIndex;
		long i = lIndexOut;
		/*~~~~~~~~~~~~~~~~~~~~*/

		while ((i < m_lSignalLength - 1) && (iNumConsecOK <= m_pConfig->m_iArtifactRewMaxConsecOK))
		{
			if (abs(GetDiffFhr(i)) < m_pConfig->m_iDropoutReturnMaxDiff)
			{
				iNumConsecOK++;
			}
			else
			{
				lIndexOut = i;
				iNumConsecOK = 0;
			}

			i++;
		}

		if (lIndex == lIndexOut)	// no advancement - try to rewind
		{
			i = lIndex;
			while ((i > m_lLineBegin) && (!(IsDropout(m_pSignalToRepair, i - 1))) && (abs(GetDiffFhr(i)) < m_pConfig->m_iArtifactRewMinFhrDiff))
			{
				i--;
				lIndexOut = i;
			}
		}

		return lIndexOut;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::GetReturnChunkLength Get length of signal from lIndex until next dropout
	=======================================================================================================================
	*/
	int CRepairSignal::GetReturnChunkLength(long lIndex)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lEndIndex = min((long) (lIndex + m_pConfig->m_iRecoveryTimeLookAhead * GetSmpFreq()), m_lSignalLength - 1);
		int iSigTime = 9999;	// if longer than m_iRecoveryTimeLookAhead then equal some large number
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		for (long i = lIndex + 1; i <= lEndIndex; i++)
		{
			if ((IsDropout(m_pSignalToRepair, i)) || (i == m_lSignalLength - 1))
			{
				iSigTime = i - lIndex + 1;
				break;
			}
		}

		return iSigTime;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::CheckShortDropout Check if conditions are met for repair of a 'short' dropout. Return true if
	conditions are met, false otherwise
	=======================================================================================================================
	*/
	bool CRepairSignal::CheckShortDropout(long lIndex, int iDropoutLength, int iSigTime)
	{
		if (iDropoutLength <= m_pConfig->m_iMaxDropoutLengthIgnore * GetSmpFreq())
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			double dFhrDiff = abs(m_pSignalToRepair[lIndex + 1] - m_dLastGoodFhr);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (dFhrDiff < m_pConfig->m_iMaxFhrDiffAlwaysIgnoreDropout)
			{
				return true;
			}
			else if ((iSigTime >= m_pConfig->m_iMinReturnFromDropout * GetSmpFreq()) && (dFhrDiff < m_pConfig->m_iMaxFhrDiffIgnoreDropout))
			{
				return true;
			}
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::CheckJumpSlope Check if slope between lLineBegin and lLineEnd exceed maximum slope criteria, based
	partially on instantaneous slope dInstSlope. Return true if meets slope criteria, flase otherwise
	=======================================================================================================================
	*/
	bool CRepairSignal::CheckJumpSlope(long lLineBegin, long lLineEnd, double dInstSlope)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dMaxPosSlope = max(m_pConfig->m_dMinPosSlope, m_pConfig->m_dMaxPosSlopeConst + m_pConfig->m_dSlopeMultFactor * dInstSlope);
		double dMaxNegSlope = max(m_pConfig->m_dMinNegSlope, m_pConfig->m_dMaxNegSlopeConst - m_pConfig->m_dSlopeMultFactor * dInstSlope);
		double dSlopeJump = (m_pSignalToRepair[lLineEnd] - m_pSignalToRepair[lLineBegin]) / (lLineEnd - lLineBegin);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		return (dSlopeJump <= dMaxPosSlope) && (dSlopeJump >= (-dMaxNegSlope));
	}

	/*
	=======================================================================================================================
	! CRepairSignal::SlopeBtwnTwoPoints
	=======================================================================================================================
	*/
	double CRepairSignal::SlopeBtwnTwoPoints(long lLineBegin, long lLineEnd)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dSlope = (m_pSignalToRepair[lLineEnd] - m_pSignalToRepair[lLineBegin]) / (lLineEnd - lLineBegin);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		return dSlope;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::CheckExitSlope Check slope from end of current chunk to ensuing chunk, if a valid ensuing chunk is
	found. Return false if valid ensuing chunk is found and slope does not meet criteria, return true otherwise.
	=======================================================================================================================
	*/
	bool CRepairSignal::CheckExitSlope(long lIndex, int iSigTime)
	{
		if (iSigTime >= m_pConfig->m_iMinReturnIgnoreExitSlope * GetSmpFreq())
		{
			return true;	// long return chunk - can ignore exit slope
		}

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		long lNextDrop = lIndex + iSigTime - 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (abs(m_dLastGoodFhr - m_pSignalToRepair[lNextDrop - 1]) <= m_pConfig->m_iMaxDiffIgnoreExitSlope)
		{
			return true;	// small diff between fhr at end of chunk and last good fhr - ignore exit slope
		}

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// Now look for valid block after the next dropout that is not too far
		int iMaxGap = (int) (m_pConfig->m_iMaxGapPostReturnForExitSlopeCheck * GetSmpFreq());
		long lEndIndex = min(m_lSignalLength - 2, lNextDrop + iMaxGap);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = lNextDrop; i <= lEndIndex; i++)
		{
			if (!(IsDropout(m_pSignalToRepair, i)))
			{
				if (ValidReturnSignalForExitSlope(i))
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					double dCurrSlope = GetCurrSlope(lNextDrop - 1, (long) (m_pConfig->m_dSlopeMeasureTime * GetSmpFreq()));
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					if (CheckJumpSlope(lNextDrop - 1, i + 1, dCurrSlope))
					{
						return true;					// exit slope ok
					}
					else
					{
						m_lCurrIndex = lNextDrop - 1;	// skip ahead to end of chunk
						return false;
					}
				}
			}
		}

		return true;	// no exit block found - gap after current block is too long - ignore exit slope
	}

	/*
	=======================================================================================================================
	! CRepairSignal::ValidReturnSignalForExitSlope Check for valid ensuing chunk for the purposes of measuring exit
	slope Returns true if found valid ensuing chunk, false otherwise
	=======================================================================================================================
	*/
	bool CRepairSignal::ValidReturnSignalForExitSlope(long lIndex)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lEndIndex = min(m_lSignalLength - 1, (long) (lIndex + m_pConfig->m_iMaxGapPostReturnForExitSlopeCheck * GetSmpFreq()));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = lIndex; i <= lEndIndex; i++)
		{
			if (abs(m_pSignalToRepair[i] - m_dLastGoodFhr) > m_pConfig->m_iMaxDiffFromLastGoodForExitBlock)
			{
				return false;
			}
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::GetReturnForInterference This is the main function for finding an exit point to any artifact that
	is not dropout. Return true if artifact exit point is found, false if none can be found (rarely happens)
	=======================================================================================================================
	*/
	bool CRepairSignal::GetReturnFromInterference(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lEndIndex = min(m_lSignalLength - 2, (long) (m_lLineBegin + m_pConfig->m_iMaxArtifactLength * GetSmpFreq()));
		double dCurrDiff = m_dJumpIn;
		double dExtrapDiff = 0.0;
		double dMinDiff = 9999.0;
		double dMinSlopeDiff = 9999.0;
		long lDropoutIndex = 0;
		long lMinIndex = m_lLineBegin;
		long lMinSlopeIndex = m_lLineBegin;
		bool bFoundDropout = false;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Loop through signal up to max artifact length to find the best exit point
		// Consider both the absolute difference between exit sample and last good as well
		// as a difference that takes slope into account Favour shorter repairs - if find
		// a small difference early then exit loop
		for (long i = m_lLineBegin + 1; i <= lEndIndex; i++)
		{
			dExtrapDiff += m_dCurrSlope;
			dCurrDiff += GetDiffFhr(i);

			if (IsDropout(m_pSignalToRepair, i + 1))
			{			// flag dropout if found one
				lDropoutIndex = i;
				bFoundDropout = true;
			}

			if (abs(dCurrDiff) < dMinDiff)
			{
				lMinIndex = i;
				dMinDiff = abs(dCurrDiff);
			}

			if (abs(dCurrDiff - dExtrapDiff) < dMinSlopeDiff)
			{
				if (SignalInRange(i + 1))
				{
					lMinSlopeIndex = i;
					dMinSlopeDiff = abs(dCurrDiff - dExtrapDiff);
				}
			}

			if (CheckShortRepair(i, dMinDiff))
			{
				break;	// favour quick repair if can find small difference
			}

			if (CheckShortRepair(i, dMinSlopeDiff))
			{
				break;	// favour quick repair
			}
		}

		// Favour the minimum difference w/ slope taken into account unless it is much
		// larger than the absolute difference
		if (dMinSlopeDiff < m_pConfig->m_iMaxRepairSlopeDiff)
		{
			if (dMinSlopeDiff < ((dMinDiff * m_pConfig->m_iMaxFactorSlopeOverAbs) + 0.0001))
			{			// 0.0001 is just for times where lMinDiff is exactly equal to 0, while lMinSlopeDiff is approx 0
				m_lLineEnd = lMinSlopeIndex + 1;
				return true;
			}
		}

		if (dMinDiff < m_pConfig->m_iMaxRepairAbsDiff)
		{
			m_lLineEnd = lMinIndex + 1;
			return true;
		}

		// if neither the difference w/ slope taken into account or the absolute
		// difference is deemed small enough, check if there was a dropout during the
		// artifact - and if so treat as a dropout
		if (bFoundDropout)
		{
			m_lCurrIndex = lDropoutIndex;	// advance to dropout
			m_bDropout = true;
			return false;					// did not find artifact exit yet
		}

		// best option is just to take minimum difference, even if large unless it occurs
		// immediately after artifact start
		if (lMinSlopeIndex > m_lLineBegin + 1)
		{
			m_lLineEnd = lMinSlopeIndex + 1;
			return true;
		}

		if (lMinIndex > m_lLineBegin + 1)
		{
			m_lLineEnd = lMinIndex + 1;
			return true;
		}

		m_bInArtifact = false;				// don't do anything
		m_lCurrIndex = m_lLineBegin;		// reset curr index to start of line - try to repair from next sample
		return false;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::CheckShortRepair See if early exit point satifies difference requirements - favour short repairs
	Return true if meets short repair criteria, flase otherwise
	=======================================================================================================================
	*/
	bool CRepairSignal::CheckShortRepair(long lIndex, double dCurrDiff)
	{
		if (lIndex - m_lLineBegin < m_pConfig->m_iMaxShortRepair * GetSmpFreq())
		{
			return dCurrDiff < (m_pConfig->m_iMaxRepairAbsDiff / 2.0);
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::SignalInRange
	=======================================================================================================================
	*/
	bool CRepairSignal::SignalInRange(long lIndex)
	{
		return (m_pSignalToRepair[lIndex] > m_pConfig->m_iMinFhrVal) && (m_pSignalToRepair[lIndex] < m_pConfig->m_iMaxFhrVal);
	}

	/*
	=======================================================================================================================
	! CRepairSignal::GetLastGoodIndex Returns index at which last known good signal is. Used for knowing when to start
	repair in current window based on where it finished in last window
	=======================================================================================================================
	*/
	long CRepairSignal::GetLastGoodIndex(void)
	{
		return m_lLastGoodIndex;
	}

	/*
	=======================================================================================================================
	! CRepairSignal::ApplyRepair Apply Repair between points m_lLineBegin and m_lLineEnd by linear interpolation.
	Special case if at very beginning of signal. Also adds to array of repair intervals.
	=======================================================================================================================
	*/
	void CRepairSignal::ApplyRepair(void)
	{
		/*~~~~~~~~~~~~~*/
		double dSlope;			// slope of interpolated line
		double dStartVal;		// starting value of interpolation
		//~~~~~~~~~~~~~

		if (m_lLineBegin == 0)	// at beginning of signal
		{
			dSlope = 0.0;		// will draw straight line
			dStartVal = m_pSignalToRepair[m_lLineEnd];
			AddRepairInterval(m_lLineBegin, m_lLineEnd - 1);
		}
		else
		{
			dSlope = SlopeBtwnTwoPoints(m_lLineBegin, m_lLineEnd);
			dStartVal = m_pSignalToRepair[m_lLineBegin];
			AddRepairInterval(m_lLineBegin + 1, m_lLineEnd - 1);
		}

		FillInPoints(dSlope, dStartVal);

		// reset necessary flags and update m_lCurrIndex
		m_bInArtifact = false;
		m_bDropout = false;
		m_bFailSlope = false;
		m_lCurrIndex = m_lLineEnd - 1;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CRepairSignal::FillInPoints(double dSlope, double dStartVal)
	{
		m_pSignalToRepair[m_lLineBegin] = dStartVal;

		for (long i = m_lLineBegin + 1; i <= m_lLineEnd; i++)
		{
			m_pSignalToRepair[i] = m_pSignalToRepair[i - 1] + dSlope;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CRepairSignal::ApplyRepairEnd(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dSlope = 0.0;
		double dStartVal = m_pSignalToRepair[m_lLineBegin];
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_lLineEnd = m_lRealSignalLength - 1;

		FillInPoints(dSlope, dStartVal);
		AddRepairInterval(m_lLineBegin + 1, m_lLineEnd);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CRepairSignal::SetRepairBuffer(double dBufferInSec)
	{
		m_pConfig->m_dRepairBuffer = dBufferInSec;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	int CRepairSignal::GetRepairBuffer(void)
	{
		return (int) (m_pConfig->m_dRepairBuffer * GetSmpFreq());
	}
}
/* ! PROJECT: LMS -- Bump Extension Copyright LMS Medical Systems 2006 */
#include "stdafx.h"
#include "BumpExtension.h"
#include "DigitalSignal.h"

namespace patterns
{


#define BUMP_EXTEND_UNDEFINED	- 1
#define EXTEND_BACKWARDS		1
#define EXTEND_FORWARDS			2

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	CBumpExtend::CBumpExtend(void)
	{
		m_pLpFhr = NULL;		// pointer to low pass fhr
		m_lNumFhrSamples = 0;	// number of low pass samples
		m_dRefBasLevel = BUMP_EXTEND_UNDEFINED; // reference baseline level
		m_lPrevBumpX2 = BUMP_EXTEND_UNDEFINED;	// x2 of previous bump (limit for backwards extension)
		m_pConfig = NULL;						// pointer to bump extension config
		m_pRefBaseLines = NULL;					// pointer to set of baselines used for reference
		m_dBumpSign = 1.0;						// bump sign (1.0 is decel, -1.0 is accel (this is because alg was originally designed for decel)
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CBumpExtend::~CBumpExtend(void)
	{
		m_pLpFhr = NULL;			// do not delete
		m_pConfig = NULL;			// do not delete (shared among bumps in CBumpExtendSet)
		m_pRefBaseLines = NULL;		// do not delete - shared among all bumps in set
	}

	/*
	=======================================================================================================================
	! CBumpExtend::SetBump Sets pointer to bump to be extended \param pBump - pointer to bump to be extended
	=======================================================================================================================
	*/
	void CBumpExtend::SetBump(bump* b)
	{
		m_pBump = b;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBumpExtend::SetPrevBumpX2(long lPrevX2)
	{
		m_lPrevBumpX2 = lPrevX2;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBumpExtend::SetLPfhr(const double *dLpFhr, long lSamples)
	{
		m_pLpFhr = dLpFhr;
		m_lNumFhrSamples = lSamples;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBumpExtend::SetBumpSign(double dBumpSign)
	{
		m_dBumpSign = dBumpSign;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBumpExtend::SetRefBaseLines(fhrPartSet *pRefBas)
	{
		m_pRefBaseLines = pRefBas;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBumpExtend::SetRepairIntervals(fhrPartSet *pRepInt)
	{
		m_pRepairIntervals = pRepInt;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBumpExtend::SetConfig(CBumpExtendConfig *pConfig)
	{
		m_pConfig = pConfig;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::ExtendBump Extends bump - top-level for single bump
	=======================================================================================================================
	*/
	bool CBumpExtend::ExtendBump(void)
	{
		/*~~~~~*/
		bool bRC;
		/*~~~~~*/

		GetRefBasBefore(m_pBump->getX1());
		bRC = ExtendBumpBack();
		GetRefBasAfter(m_pBump->getX2());
		if (ExtendBumpForward())
		{
			bRC = true;
		}

		return bRC;
	}
	/*
	=======================================================================================================================
	! CBumpExtend::ExtendBumpBack Extends the begin boundary (x1) back in time Stopping conditions for extension are:
	1) Flatness 2) Upto reference baseline level 3) Found local maximum 4) Encounter area of repair Stopping conditions
	can be ignored if current fhr level is sufficiently below a reference level
	=======================================================================================================================
	*/
	bool CBumpExtend::ExtendBumpBack(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iPushStep = (int) (m_pConfig->m_iPushStepStart * m_pConfig->m_dSmpFreq);	// # samples to extend at each iteration
		bool bContinue = true;			// continue extension
		bool bFoundCondition = false;	// found stopping condition
		bool bRC = false;
		long lX1 = m_pBump->getX1();
		long lX2 = m_pBump->getX2();
		long lMinX1 = max(0, lX1 - (long) (m_pConfig->m_lMaxLookBack * m_pConfig->m_dSmpFreq)); // limit of backwards extension
		int iRewind = 0;									// # samples to rewind extension when final stopping criteria met (diff. for diff. stopping criteria)
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		if (m_lPrevBumpX2 >= lX2)
		{													// prior bump (of same type) has been extended past end of current bump (i.e. two bumps become 1)
			return false;
		}

		while (bContinue)									// main extension loop
		{
			iRewind = 0;
			bFoundCondition = false;
			if (lX1 <= lMinX1)
			{												// extended to/past limit
				lX1 = lMinX1 + 1;
				bContinue = false;
			}
			else if (lX1 <= m_lPrevBumpX2)
			{												// extended to boundary of previous bump of same type
				lX1 = m_lPrevBumpX2 + 1;
				bContinue = false;
			}
			else if (CheckRepairSegmentBack(lX1))
			{												// encountered repaired segment
				bFoundCondition = true;
				iRewind = (int) (m_pConfig->m_iRepairRewind * m_pConfig->m_dSmpFreq);
			}
			else if (CheckUpToBaseLine(lX1, lX1 + iPushStep))
			{												// reached reference baseline level
				bFoundCondition = true;
				iRewind = (int) (m_pConfig->m_iUptoBasRewind * m_pConfig->m_dSmpFreq);
			}
			else if (CheckLocalMaxBack(&lX1, iPushStep))
			{												// encountered local maximum
				bContinue = false;							// can get out of loop if find local max
				iRewind = (int) (m_pConfig->m_iLocalMaxRewind * m_pConfig->m_dSmpFreq);
			}
			else if (CheckFlatnessBack(lX1))
			{												// encountered flat area
				bFoundCondition = true;
				iRewind = (int) (m_pConfig->m_iFlatnessRewind * m_pConfig->m_dSmpFreq);
			}

			if (bContinue && (!bFoundCondition))
			{												// no stopping condition encountered
				lX1 -= iPushStep;
				ResetBasIfOverlap(lX1);						// check if bump extends into reference baseline
			}
			else if (bFoundCondition)						// found stopping condition
			{
				if (lX1 < m_pBump->getX1())
				{											// have extended in previous loop
					if (iPushStep > m_pConfig->m_iPushGranularity)
					{
						iPushStep = (int) iPushStep / 2;	// reduce search granularity
					}
					else
					{
						bContinue = false;				// at final granularity - can exit loop
					}

					lX1 = lX1 + iPushStep;				// rewind
				}
				else
				{	// reached stopping condition in first attempt - give up
					lX1 = m_pBump->getX1();	// no extension
					bContinue = false;		// exit loop
					iRewind = 0;
				}
			}
		}

		lX1 += iRewind; // rewind extension based on final stopping criterion
		bRC = ((lX1 < m_pBump->getX1()) || (lX1 == m_lPrevBumpX2 + 1));	// do not rewind bump unless overlaps previous bump
		if (bRC)
		{
			m_pBump->setX1(lX1);
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::ExtendBumpForward Extends the end boundary (x2) forward in time Stopping conditions for extension
	are: 1) Flatness 2) Upto reference baseline level 3) Found local maximum 4) Encounter area of repair Stopping
	conditions can be ignored if current fhr level is sufficiently below a reference level This is essentially a mirror
	image of ExtendBumpBack
	=======================================================================================================================
	*/
	bool CBumpExtend::ExtendBumpForward(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iPushStep = (int) (m_pConfig->m_iPushStepStart * m_pConfig->m_dSmpFreq);
		bool bContinue = true;
		bool bFoundCondition = false;
		bool bRC = false;
		long lX2 = m_pBump->getX2();
		long lMaxX2 = min(m_lNumFhrSamples, lX2 + (long) (m_pConfig->m_lMaxLookForward * m_pConfig->m_dSmpFreq));
		int iRewind = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (m_dRefBasLevel != BUMP_EXTEND_UNDEFINED)
		{
			if (m_dRefBasLevel > GetLPSample(m_pBump->getX1()) + m_pConfig->m_dMaxRefBasLevelOverX1)
			{
				m_dRefBasLevel = GetLPSample(m_pBump->getX1());
			}
		}

		while (bContinue)
		{
			iRewind = 0;
			bFoundCondition = false;
			if (lX2 >= lMaxX2)
			{
				lX2 = lMaxX2 - 1;
				bContinue = false;
			}
			else if (CheckRepairSegmentForward(lX2))
			{
				bFoundCondition = true;
				iRewind = (int) (m_pConfig->m_iRepairRewind * m_pConfig->m_dSmpFreq);
			}
			else if (CheckUpToBaseLine(lX2 - iPushStep, lX2))
			{
				bFoundCondition = true;
				iRewind = (int) (m_pConfig->m_iUptoBasRewind * m_pConfig->m_dSmpFreq);
			}
			else if (CheckLocalMaxForward(&lX2, iPushStep))
			{
				bContinue = false;			// can get out of loop if find local max
				iRewind = (int) (m_pConfig->m_iLocalMaxRewind * m_pConfig->m_dSmpFreq);
			}
			else if (CheckFlatnessForward(lX2))
			{
				bFoundCondition = true;
				iRewind = (int) (m_pConfig->m_iFlatnessRewind * m_pConfig->m_dSmpFreq);
			}

			if (bContinue && (!bFoundCondition))
			{
				lX2 += iPushStep;
				ResetBasIfOverlapForward(lX2, m_pBump->getX1());
			}
			else if (bFoundCondition)		// found stopping condition
			{
				if (lX2 > m_pBump->getX2())	// have extended in previous loop
				{
					if (iPushStep > m_pConfig->m_iPushGranularity)
					{
						iPushStep = (int) iPushStep / 2;
					}
					else
					{
						bContinue = false;
					}

					lX2 = lX2 - iPushStep;
				}
				else						// reached stopping condition in first attempt - give up
				{
					lX2 = m_pBump->getX2();
					bContinue = false;
					iRewind = 0;
				}
			}
		}

		lX2 -= iRewind;
		bRC = (lX2 > m_pBump->getX2());
		if (bRC)
		{
			m_pBump->setX2(lX2);
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetRefBasBefore Given a current x1, this will find the closest baseline starting before that x1 and use it
	as the reference baseline. Baselines will be truncated at the specified x1 should they pass the x1 in question. A
	reference level is then calculated as the average value of low pass samples over the reference baseline. Reference
	baselines must meet specified length requirements in order to be considered.
	=======================================================================================================================
	*/
	bool CBumpExtend::GetRefBasBefore(long lX1)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int nBaseLines = m_pRefBaseLines->size();
		int i = 0, basIndex = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (nBaseLines == 0)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		baseline *pBL = (baseline *) m_pRefBaseLines->getAt(0);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while ((i < nBaseLines) && (pBL->getX1() + GetMinRefBasLen() <= lX1))
		{
			basIndex = i;
			pBL = (baseline *) m_pRefBaseLines->getAt(i++);
		}

		if (pBL->getX1() + GetMinRefBasLen() > lX1)
		{
			if (basIndex > 0)
			{
				pBL = (baseline *) m_pRefBaseLines->getAt(basIndex-1);
			}
			else
			{
				return false;
			}
		}

		m_RefBas = *(pBL);

		if (m_RefBas.getX2() < lX1)
		{
			m_dRefBasLevel = m_RefBas.getLpMean() * m_dBumpSign;	// flip if accel
		}
		else	// need to truncate and recompute
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			double dSignalSum = m_RefBas.getLpMean() * (double) (m_RefBas.length()) * m_dBumpSign;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			while (m_RefBas.getX2() >= lX1)
			{
				if (m_RefBas.getX2() < 0)
				{
//					ASSERT(FALSE); // OUT OF BOUNDS

					// before current window - don't have signal available (this shouldn't happen)
					return false;
				}

				dSignalSum -= GetLPSample(m_RefBas.getX2());	// GetLPSample already flips sign if accel/decel
				m_RefBas.setX2(m_RefBas.getX2() - 1);
			}

			m_dRefBasLevel = dSignalSum / (m_RefBas.length());
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetRefBasAfter Given a current x2, this will find the closest baseline starting after that x2 and use it
	as the reference baseline. Baselines will be truncated if there is overlap with the bump. A
	reference level is then calculated as the average value of low pass samples over the reference baseline. Reference
	baselines must meet specified length requirements in order to be considered.
	=======================================================================================================================
	*/
	bool CBumpExtend::GetRefBasAfter(long lX2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int nBaseLines = m_pRefBaseLines->size();
		int i = nBaseLines - 1, basIndex = nBaseLines;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (nBaseLines == 0)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		baseline *pBL = (baseline *) m_pRefBaseLines->getAt(i);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while ((i > 0) && (pBL->getX2() - GetMinRefBasLen() >= lX2))
		{
			basIndex = i;
			i--;
			pBL = (baseline *) m_pRefBaseLines->getAt(i);
		}

		if (basIndex < nBaseLines)
		{ 
			pBL = (baseline *) m_pRefBaseLines->getAt(basIndex);
		}
		else
		{
			return false; // no ref baseline afterwards
		}

		m_RefBas = *(pBL);

		if (m_RefBas.getX1() > lX2)
		{
			m_dRefBasLevel = m_RefBas.getLpMean() * m_dBumpSign;	// flip if accel
		}
		else	// need to truncate and recompute
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			double dSignalSum = m_RefBas.getLpMean() * (double) (m_RefBas.length()) * m_dBumpSign;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			while (m_RefBas.getX1() <= lX2)
			{
				if (m_RefBas.getX1() < 0)
				{
//					ASSERT(FALSE); // OUT OF BOUND

					// before current window - don't have signal available (this shouldn't happen)
					return false;
				}

				dSignalSum -= GetLPSample(m_RefBas.getX1());	// GetLPSample already flips sign if accel/decel
				m_RefBas.setX1(m_RefBas.getX1() + 1);
			}

			m_dRefBasLevel = dSignalSum / (m_RefBas.length());
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::ResetBasIfOverlap Given an x1, check if the current reference baseline surpasses x1, and if so,
	truncate reference baseline. If reference baseline no longer meets length requirements, then get a new reference
	baseline, and set the reference baseline level to the maximum of the current and newly calculated levels (this is
	to safeguard against erroneous reference baseline levels).
	=======================================================================================================================
	*/
	bool CBumpExtend::ResetBasIfOverlap(long lX1)
	{
		bool bRC = false;

		if (m_dRefBasLevel == BUMP_EXTEND_UNDEFINED)
		{	// no reference baseline
			bRC =  false;
		}
		else if (lX1 > m_RefBas.getX2())
		{
			bRC =  false;
		}
		else 
		{
			double dOldRefBas = m_dRefBasLevel;
			long oldX2 = m_RefBas.getX2();
			long newX2 = lX1 - 1;
			if (newX2 - m_RefBas.getX1() + 1 >= GetMinRefBasLen())
			{
				double dSignalSum = m_dRefBasLevel * m_RefBas.length();
				for (long i = oldX2; i > newX2; i--)
				{
					dSignalSum -= GetLPSample(i);
				}
				m_RefBas.setX2(newX2);
				m_dRefBasLevel = dSignalSum / (double) m_RefBas.length();
				bRC = false;
			}
			else
			{
				GetRefBasBefore(lX1);
				bRC = true;
			}
			m_dRefBasLevel = max(m_dRefBasLevel, dOldRefBas);		
		}
		return(bRC);
	}

	/*
	=======================================================================================================================
	! CBumpExtend::ResetBasIfOverlapForward Given an x2, check if the current reference baseline overlaps extended x2, and if so,
	truncate reference baseline. If reference baseline no longer meets length requirements, then get a new reference
	baseline, and set the reference baseline level to the maximum of the current and newly calculated levels (this is
	to safeguard against erroneous reference baseline levels).
	=======================================================================================================================
	*/
	bool CBumpExtend::ResetBasIfOverlapForward(long lX2, long lX1)
	{
		bool bRC = false;

		if (m_dRefBasLevel == BUMP_EXTEND_UNDEFINED)
		{	// no reference baseline
			bRC =  false;
		}
		else if (lX2 < m_RefBas.getX1())
		{
			bRC =  false;
		}
		else if (m_RefBas.getX2() < lX1) 
		{ // using baseline prior to bump as reference
			bRC = false;
		}
		else 
		{
			double dOldRefBas = m_dRefBasLevel;
			long oldX1 = m_RefBas.getX1();
			long newX1 = lX2 + 1;
			if (m_RefBas.getX2() - newX1 + 1 >= GetMinRefBasLen())
			{
				double dSignalSum = m_dRefBasLevel * m_RefBas.length();
				for (long i = oldX1; i < newX1; i++)
				{
					dSignalSum -= GetLPSample(i);
				}
				m_RefBas.setX1(newX1);
				m_dRefBasLevel = dSignalSum / (double) m_RefBas.length();
				bRC = false;
			}
			else
			{
				if (!(GetRefBasAfter(lX2)))
				{
					GetRefBasBefore(lX1);
				}
				bRC = true;
			}
			m_dRefBasLevel = max(m_dRefBasLevel, dOldRefBas);		
		}
		return(bRC);
	}
	

	/*
	=======================================================================================================================
	! CBumpExtend::CheckRepairSegmentBack Check that not extending back (from lX1) into a repaired segment. The
	repaired segment is characterized by constant slope.
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckRepairSegmentBack(long lX1)
	{	
		return CheckRepairFromIntervals(lX1);
	}

	/*
	=======================================================================================================================
	! CBumpExtend::CheckRepairSegmentForward Check that not extending forward (from lX2) into a repaired segment. The
	repaired segment is characterized by constant slope.
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckRepairSegmentForward(long lX2)
	{
		return CheckRepairFromIntervals(lX2);

	}

	/*
	=======================================================================================================================
	! CBumpExtend::CheckRepairFromIntervals Check if current sample index overlaps a repair interval - intervals shouls
	already have been filtered for length so can skip over very small repairs
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckRepairFromIntervals(long lX)
	{
		for (long i = 0; i < m_pRepairIntervals->size(); i++)
		{
			fhrPart *p = m_pRepairIntervals->getAt(i);
			if ((p->getX1() <= lX) && (p->getX2() >= lX))
			{
				return true;
			}
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::CheckUpToBaseline Check that no sample between indexes lX1 and lX2 is above reference baseline level
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckUpToBaseLine(long lX1, long lX2)
	{
		if (m_dRefBasLevel == BUMP_EXTEND_UNDEFINED)
		{
			return false;
		}

		lX1 = max(0, lX1);	// check boundary condition
		lX2 = min(lX2, m_lNumFhrSamples - 1);

		for (long i = lX1; i <= lX2; i++)
		{
			if (GetLPSample(i) >= (m_dRefBasLevel * m_pConfig->m_dBasLevelPerc))
			{
				return true;
			}
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::CheckLocalMaxBack Check that no local max in area lX1 -> lX1 + iPushStep. Local max is defined as
	highest fhr value in area defined by GetLocalMaxSearchDist() samples on either side of lX1.
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckLocalMaxBack(long *lX1, int iPushStep)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRC = false;
		long lX1Temp = *lX1;
		long lA1 = max(0, lX1Temp - GetLocalMaxSearchDist());
		long lA2 = min(m_lNumFhrSamples - 1, lX1Temp + iPushStep + GetLocalMaxSearchDist());
		long lMaxIndex = GetMaxLPIndexInInterval(lA1, lA2);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if ((lMaxIndex >= lX1Temp) && (lMaxIndex <= lX1Temp + iPushStep))
		{
			bRC = true;
			lX1Temp = lMaxIndex;
			if (IsFarEnoughBelowRefBas(lMaxIndex))
			{
				bRC = false;
			}
		}

		if (bRC)
		{
			*lX1 = lX1Temp + 1;
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::CheckLocalMaxBack Check that no local max in area lX2 - iPushStep -> lX2. Local max is defined as
	highest fhr value in area defined by GetLocalMaxSearchDist() samples on either side of lX1.
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckLocalMaxForward(long *lX2, int iPushStep)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRC = false;
		long lX2Temp = *lX2;
		long lA1 = max(0, lX2Temp - iPushStep - GetLocalMaxSearchDist());
		long lA2 = min(m_lNumFhrSamples - 1, lX2Temp + GetLocalMaxSearchDist());
		long lMaxIndex = GetMaxLPIndexInInterval(lA1, lA2);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if ((lMaxIndex >= lX2Temp - iPushStep) && (lMaxIndex <= lX2Temp))
		{
			bRC = true;
			lX2Temp = lMaxIndex;
			if (IsFarEnoughBelowRefBas(lMaxIndex))
			{
				bRC = false;
			}
		}

		if (bRC)
		{
			*lX2 = lX2Temp - 1;
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetMaxLPIndexInInterval Get the maximum low pass sample in interval specified by lX1 -> lX2
	=======================================================================================================================
	*/
	long CBumpExtend::GetMaxLPIndexInInterval(long lX1, long lX2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dMaxVal = GetLPSample(lX1);
		long lMaxIndex = lX1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = lX1 + 1; i <= lX2; i++)
		{
			if (GetLPSample(i) > dMaxVal)
			{
				dMaxVal = GetLPSample(i);
				lMaxIndex = i;
			}
		}

		return lMaxIndex;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::CheckFlatnessBack Ensure not extending back into flat portion of signal Non-Flatness is defined by a
	minimum difference between samples that are a defined number of samples apart AND by a minimum instantaneous slope
	over a smaller length of signal
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckFlatnessBack(long lX1)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lIndexStart = max(0, lX1 - (long) (m_pConfig->m_iMinFlatLength * m_pConfig->m_dSmpFreq));
		double dSimpSlope = GetSimpleSlopeBack(lIndexStart, lX1);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (dSimpSlope < (m_pConfig->m_dMaxFlatSlope / m_pConfig->m_dSmpFreq))
		{
			if (IsFarEnoughBelowRefBas(lX1))
			{
				return false;
			}

			lIndexStart = max(0, lX1 - m_pConfig->m_iInstSlopeSegLen);

			/*~~~~~~~~~~~~~*/
			double dCurrDiff;
			/*~~~~~~~~~~~~~*/

			for (long i = lX1; i > lIndexStart; i--)
			{
				dCurrDiff = GetLPSample(i - 1) - GetLPSample(i);
				if (dCurrDiff >= m_pConfig->m_dMinInstSlope)
				{
					return false;
				}
			}

			return true;
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::CheckFlatnessForward Ensure not extending forward into flat portion of signal Non-Flatness is
	defined by a minimum difference between samples that are a defined number of samples apart AND by a minimum
	instantaneous slope over a smaller length of signal
	=======================================================================================================================
	*/
	bool CBumpExtend::CheckFlatnessForward(long lX2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lIndexEnd = min(m_lNumFhrSamples - 1, lX2 + (long) (m_pConfig->m_iMinFlatLength * m_pConfig->m_dSmpFreq));
		double dSimpSlope = GetSimpleSlopeForward(lIndexEnd, lX2);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (dSimpSlope < (m_pConfig->m_dMaxFlatSlope / m_pConfig->m_dSmpFreq))
		{
			if (IsFarEnoughBelowRefBas(lX2))
			{
				return false;
			}

			lIndexEnd = min(m_lNumFhrSamples - 1, lX2 + m_pConfig->m_iInstSlopeSegLen);

			/*~~~~~~~~~~~~~*/
			double dCurrDiff;
			/*~~~~~~~~~~~~~*/

			for (long i = lX2; i < lIndexEnd; i++)
			{
				dCurrDiff = GetLPSample(i + 1) - GetLPSample(i);
				if (dCurrDiff >= m_pConfig->m_dMinInstSlope)
				{
					return false;
				}
			}

			return true;
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetSimpleSlopeForward Get Slope between two points
	=======================================================================================================================
	*/
	double CBumpExtend::GetSimpleSlopeForward(long lXPush, long lX2)
	{
		return (GetLPSample(lXPush) - GetLPSample(lX2)) / (double) (lXPush - lX2);
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetSimpleSlopeBack Get Slope between two points
	=======================================================================================================================
	*/
	double CBumpExtend::GetSimpleSlopeBack(long lXPush, long lX1)
	{
		return (GetLPSample(lXPush) - GetLPSample(lX1)) / (double) (lX1 - lXPush);
	}

	/*
	=======================================================================================================================
	! CBumpExtend::IsFarEnoughBelowRefBas Check to see if sample at lIndex is sufficiently below the reference bas
	level to safely ignore a stopping condition
	=======================================================================================================================
	*/
	bool CBumpExtend::IsFarEnoughBelowRefBas(long lIndex)
	{
		if (m_dRefBasLevel == BUMP_EXTEND_UNDEFINED)
		{
			return false;
		}

		return m_dRefBasLevel > (GetLPSample(lIndex) + m_pConfig->m_dMaxLocalMaxDiffFromBas);
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetLPSample Get low pass sample at specified index, multiplied by bumpSign. Note that algorithm was
	designed for decels and when applied to accels, the value of the fhr is simply multiplied by -1 (bumpSign for
	accels) in order to keep algorithms symmetrical
	=======================================================================================================================
	*/
	double CBumpExtend::GetLPSample(long lIndex)
	{
		if ((lIndex >= 0) && (lIndex < m_lNumFhrSamples))
		{
			return m_pLpFhr[lIndex] * m_dBumpSign;
		}
		
		// OUT OF BOUNDS
		throw exception("(GetRefBasBefore) Baseline sample too far in the past!");
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetMinRefBasLen Get the minimum required length of a baseline segment in order for it to be
	considered for reference
	=======================================================================================================================
	*/
	long CBumpExtend::GetMinRefBasLen(void)
	{
		return (m_pConfig->GetMinRefBasLen());
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetMinRepairLen Get the minimum length of segment required for consideration for repair segment
	stopping criteria
	=======================================================================================================================
	*/
	long CBumpExtend::GetMinRepairLen(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lMinRep = (long) (m_pConfig->m_iMinRepairLen * m_pConfig->m_dSmpFreq);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		return lMinRep;
	}

	/*
	=======================================================================================================================
	! CBumpExtend::GetLocalMaxSearchDist Get the size of the area in which to consider a maximum local
	=======================================================================================================================
	*/
	long CBumpExtend::GetLocalMaxSearchDist(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lSearchDist = (long) (m_pConfig->m_iLocalMaxSearchArea * m_pConfig->m_dSmpFreq);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		return lSearchDist;
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet constructor
	=======================================================================================================================
	*/
	CBumpExtendSet::CBumpExtendSet(void)
	{
		m_lFirstPrevBumpX2 = 0; // index of maximum extension back of first bump in set
		m_pLPfhr = NULL;		// point to low pass signal
		m_nLPfhr = 0;			// number of low pass signal samples
		m_pConfig = new CBumpExtendConfig;	// config object
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CBumpExtendSet::CBumpExtendSet(double dSmpFreq)
	{
		m_lFirstPrevBumpX2 = -1;
		m_pLPfhr = NULL;
		m_nLPfhr = 0;
		m_pConfig = new CBumpExtendConfig;
		m_pConfig->SetSmpFreq(dSmpFreq);
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet destructor
	=======================================================================================================================
	*/
	CBumpExtendSet::~CBumpExtendSet(void)
	{

		m_pRefBaseLines.clear();
		m_pRepairIntervals.clear();

		m_pLPfhr = NULL;					// do not delete - this is not a local copy

		// pointer to CFHRSignal lpFhr
		if (m_pConfig)
		{
			delete m_pConfig;
		}

		m_pConfig = NULL;
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet::SetRefBas Take a set of baselines (pRefBas) and copy any baseline that is longer than the minimum
	reference length to a local set of reference baselines
	=======================================================================================================================
	*/
	void CBumpExtendSet::SetRefBas(fhrPartSet *pRefBas)
	{
		m_pRefBaseLines.clear();
		m_pRefBaseLines.addcopy(pRefBas);
		m_pRefBaseLines.filterByLength(GetMinRefBasLen());
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet::SetRefSignal Set the pointer to the low pass fhr signal
	=======================================================================================================================
	*/
	void CBumpExtendSet::SetRefSignal(const double *pFHR, long nFHR)
	{
		m_pLPfhr = pFHR;
		m_nLPfhr = nFHR;
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet::SetBumps Take a set of candidate bumps to be extended and make a local copy
	=======================================================================================================================
	*/
	void CBumpExtendSet::SetBumps(fhrPartSet *bumps)
	{
		m_pBumps = bumps;
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet::SetRepairIntervals Take a set of repair intervals and copy any interval that is longer than the
	max skip over
	=======================================================================================================================
	*/
	void CBumpExtendSet::SetRepairIntervals(fhrPartSet *pRepInt, long lOffset)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~*/
		long lNumLongRepInt = 0;
		long lRepIndex = 0;
		long i = 0;
		/*~~~~~~~*/

		m_pRepairIntervals.clear();
		for (i = 0; i < pRepInt->size(); i++)
		{
			fhrPart *p = pRepInt->getAt(i);
			if (p->length() > ((long) (m_pConfig->m_iMinRepairLen * m_pConfig->m_dSmpFreq)))
			{
				m_pRepairIntervals.addcopy(p);
			}
		}

		m_pRepairIntervals.toRelativeTime(lOffset);

	}

	/*
	=======================================================================================================================
	! CBumpExtendSet::SetFirstPrevX2
	=======================================================================================================================
	*/
	void CBumpExtendSet::SetFirstPrevX2(long lX2)
	{
		m_lFirstPrevBumpX2 = lX2;
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet::Extend Extend all bumps in set.
	=======================================================================================================================
	*/
	void CBumpExtendSet::Extend(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		CBumpExtend *pBumpExtend = new CBumpExtend;
		bump *pLastBumpPos2 = NULL, *pLastBumpNeg2 = NULL;
		long lBumpsOut = 0;
		bool *bKeepBump = new bool[m_pBumps->size()];
		double dPercOverlap = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pBumpExtend->SetLPfhr(m_pLPfhr, m_nLPfhr);
		pBumpExtend->SetRefBaseLines(&m_pRefBaseLines);
		pBumpExtend->SetConfig(m_pConfig);
		pBumpExtend->SetRepairIntervals(&m_pRepairIntervals);

		for (int i = 0; i < m_pBumps->size(); i++)
		{
			bKeepBump[i] = false;
			pBumpExtend->SetPrevBumpX2(m_lFirstPrevBumpX2); // this will be used for first bump
			if (m_pBumps->getAt(i)->isAccel())
			{
				pBumpExtend->SetBumpSign(-1.0);				// everything inverted if accel

				// if (i > 0) // in some cases first bump will have been swallowed by prevX2 so
				// still want to use original m_lFirstPrevBumpX2 {
				if (pLastBumpPos2)
				{
					pBumpExtend->SetPrevBumpX2(pLastBumpPos2->getX2()); // maximum extension backwards
				}

				// else pBumpExtend->SetPrevBumpX2(-1);
				// // no limit dictated by prior event on backwards extension }
			}
			else	// decel
			{
				pBumpExtend->SetBumpSign(1.0);

				// if (i > 0) {
				if (pLastBumpNeg2)
				{
					pBumpExtend->SetPrevBumpX2(pLastBumpNeg2->getX2());
				}
			}

			pBumpExtend->SetBump((bump*) m_pBumps->getAt(i));	// set the bump in BumpExtend object
			pBumpExtend->ExtendBump();				// extend current bump

			// now check for overlap w/ previous bump
			if (m_pBumps->getAt(i)->getX1() > pBumpExtend->m_lPrevBumpX2)
			{		// no overlap
				bKeepBump[i] = true;
			}
			else	// overlap - need to truncate and/or toss
			{
				dPercOverlap = (double) ((pBumpExtend->m_lPrevBumpX2 - m_pBumps->getAt(i)->getX1()) / (m_pBumps->getAt(i)->getX2() - m_pBumps->getAt(i)->getX1() - 1));
				if (dPercOverlap <= m_pConfig->m_dMaxPercentOverlap)
				{	// minimum overlap - just truncate
					bKeepBump[i] = true;
					m_pBumps->getAt(i)->setX1(pBumpExtend->m_lPrevBumpX2 + 1);
				}

				// otherwise too much overlap - throw away current bump (bKeepBump = false)
			}
		}
		for (int i = m_pBumps->size()-1; i >= 0; i--)
		{
			if (!bKeepBump[i])
			{	
				m_pBumps->removeAt(i);
			}
		}

		if (bKeepBump)
		{
			delete [] bKeepBump;
		}
		delete pBumpExtend;
	}

	/*
	=======================================================================================================================
	! CBumpExtendSet::GetMinRefBasLen Get the minimum required baseline length for it to be used as a reference
	baseline
	=======================================================================================================================
	*/
	long CBumpExtendSet::GetMinRefBasLen(void)
	{
		return (m_pConfig->GetMinRefBasLen());
	}
	/*
	=======================================================================================================================
	! CBumpExtendSet::GetMinRefBasLen Get the minimum required baseline length for it to be used as a reference
	baseline
	=======================================================================================================================
	*/
	void CBumpExtendSet::SetMinRefBasLenSec(long n)
	{
		m_pConfig->SetMinRefBasLenSec(n);
	}

	/*
	=======================================================================================================================
	! CBumpExtendConfig constructor
	=======================================================================================================================
	*/
	CBumpExtendConfig::CBumpExtendConfig(void)
	{
		m_dSmpFreq = 4.0;					// sampling frequency

		m_lMaxLookBack = 600;				// maximum number of seconds can extend backwards
		m_lMaxLookForward = 600;			// max number of seconds can extend forwards

		m_dBasLevelPerc = 1.0;				// perc. of bas level need to reach for stopping condition
		m_iPushStepStart = 2;				// number of sec. in extension step at start
		m_iPushGranularity = 1;				// smallest granularity of push step (in samples)
		m_iMinRefBaseLineLen = 60;			// min length of reference baseline (seconds)
		m_iMinBasLen = 20;					// minimum length of any baseline (seconds)
		m_dMaxPercentOverlap = 0.25;		// maximum overlap between successive bump cand. where can still truncate and keep later bump

		m_dMaxLocalMaxDiffFromBas = 25.0;	// difference required between current sample and ref bas level in order to ignore stopping condition
		m_iLocalMaxSearchArea = 5;			// in seconds
		m_iMinFlatLength = 10;				// in seconds
		m_dMaxFlatSlope = 2.0 / (double) m_iMinFlatLength;	// in seconds slope - need to be divided by smpFreq
		m_iInstSlopeSegLen = 10;		// in samples
		m_dMinInstSlope = 0.3;			// samples slope

		m_dMinRepairPerc = 0.9;			// percentage of segment w/ constant slope required to call segment 'repair'
		m_iMinRepairLen = 30;			// in seconds - ignore any repairs shorter than this
		m_bUseRepairIntervals = true;	// instead of looking for constant slope, use explicit repair intervals from repair module

		m_iFlatnessRewind = 1;			// in seconds
		m_iLocalMaxRewind = 1;			// in seconds
		m_iRepairRewind = 0;			// in seconds
		m_iUptoBasRewind = 0;			// in seconds

		m_dMaxRefBasLevelOverX1 = 15.0; // if sample of x1 of extended bump is more than this amount less than ref bas level,
		// set ref bas level to x1 sample (for x2 extension)
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CBumpExtendConfig::~CBumpExtendConfig(void)
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBumpExtendConfig::SetSmpFreq(double dSmpFreq)
	{
		m_dSmpFreq = dSmpFreq;
	}



}
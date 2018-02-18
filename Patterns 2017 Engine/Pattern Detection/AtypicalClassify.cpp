/*!
PROJECT: LMS -- Bump Extension

Copyright LMS Medical Systems 2006 
*/

#include "stdafx.h"
#include "AtypicalClassify.h"

namespace patterns
{
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CAtypicalClassify::CAtypicalClassify()
	{
		m_pLpFHR = NULL; 
		m_pFHR = NULL;
		m_nLP = 0;
		m_nFHR = 0;
		m_dSmpFreq = 4.0;
		m_basIndex = 0;
		m_decIndex = 0;
		m_repIntIndex = 0;
		m_pConfig = new CAtypicalConfig;
		m_pRefBaseLines = NULL; // set of reference baselines

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CAtypicalClassify::~CAtypicalClassify()
	{
		// do not delete m_pRefBaseLines or m_pBumps
		m_pLpFHR = NULL; // do not delete
		if (m_pConfig)
		{
			delete m_pConfig;
		}
		m_pConfig = NULL; 
		
	}

	/*!
	CAtypicalClassify::SetBumps
		Set bumps to be classified

		\param pBump - pointer to bump to be extended

	*/
	void CAtypicalClassify::SetBumps(fhrPartSet* Bumps)
	{
		m_Bumps = Bumps;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CAtypicalClassify::SetLpFHR(const double* dLpFHR, long n)
	{
		m_pLpFHR = dLpFHR;
		m_nLP = n;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CAtypicalClassify::SetFHR(const double* dFHR, long n)
	{
		m_pFHR = dFHR;
		m_nFHR = n;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CAtypicalClassify::SetRefBaseLines(fhrPartSet* pRefBas)
	{
		m_pRefBaseLines = pRefBas;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CAtypicalClassify::SetRepairIntervals(fhrPartSet* pRepInt)
	{
		m_pRepairIntervals = pRepInt;
	}

	/*!
	CAtypicalClassify::AtypicalClassify 
		Do Atypical Classification

	*/
	void CAtypicalClassify::AtypicalClassify(bool extendedClassification)
	{
		long i;

		for (i = 0; i < m_Bumps->size(); i++)
		{
			decel *d = (decel *) m_Bumps->getAt(i);
			d->clearAtypical();
			m_decIndex = i; // to speed up search for surrounding decels in submethods
			if (AtypicalPreFilter(*d))
			{
				m_dVarScore = GetDecVarScore(*d);
				if (IsLossVar(*d))
					d->setLossVar();
				if (IsSixties(*d))
					d->setSixties();

				// The next 6 atypical types are only marked if the extendedClassification is ON
				if (extendedClassification)
				{
					if (IsBiphasic(*d))
						d->setBiphasic();
					if (d->isLate())
						d->setVarLate();
					if (IsLossRise(*d))
						d->setLossRise();
					if (IsLowerBas(*d))
						d->setLowerBas();
					if (IsProlSecRise(*d))
						d->setProlSecRise();
					if (IsSlowReturn(*d))
						d->setSlowReturn();
				}

				if ((d->isAtypical()) && (d->getSubtype() != fhrPart::DecType_PROL))
					d->setSubtype(fhrPart::DecType_ATYP);
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::AtypicalPreFilter(decel &d)
	{

		bool bRC = true;

		if (!(d.isVariable()))
		{
			bRC = false;
		}
		else
		{
			double dMinHeight = 30;
			double dMinMeanHeight = 15;
			long lMinLength = 30 * (long) m_dSmpFreq;
			double dMaxRepair = 0.6;
			long equivLen = (long) ((double) d.length() * (1 -  d.getPercRepair()));
		
			bRC = ((d.getBM()->getMaxHeight() >= dMinHeight) && (d.getBM()->getMeanHeight() >= dMinMeanHeight) && (d.getPercRepair() < dMaxRepair) && (equivLen >= lMinLength));
		}
		return(bRC);
	}

	/* 
		CheckTrough
		Checks to see if trough exists for signal d between indices x1 and x2.  Trough must be of length minTroughLen
		and not have any FHR samples above minD.  Indices of first trough found returned in r1, r2.  Return value is false
		if no trough found, true if yes.  
		*/
	bool CAtypicalClassify::CheckTroughForward(const double *d, long x1, long x2, double minD, long minTroughLen, long* r1, long* r2)
	{
		bool found = false;
		long i = x1, s1, s2;
		long maxi = x2 - minTroughLen + 1;

		while ((!found) && (i < maxi))
		{
			s1 = i;
			s2 = i + minTroughLen - 1;
			found = true;
			for (long k = s1; k <= s2; k++)
			{
				if (d[k] > minD)
				{
					found = false;
					i = k+1;
					break;
				}
			}
		}

		return(found);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::CheckTroughBack(const double *d, long x1, long x2, double minD, long minTroughLen, long* r1, long* r2)
	{
		bool found = false;
		long i = x2, s1, s2;
		long mini = x1 + minTroughLen - 1;

		while((!found) && (i > mini))
		{
			s2 = i;
			s1 = i - minTroughLen + 1;
			found = true;
			for (long k = s2; k >= s1; k--)
			{
				if (d[k] > minD)
				{
					found = false;
					i = k - 1;
					break;
				}
			}
		}

		return(found);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::FindBasAfterGap(decel &d, long minGap, long maxGap, long* b1, long* b2)
	{
		long x2 = d.getX2(), maxX2 = m_nFHR, minX2 = d.getX2() + minGap, 
			n = m_pRefBaseLines->size(), i = m_basIndex, minBasLen = 20 * (long) m_dSmpFreq;
		bool bRC = false;

		if (m_decIndex < m_Bumps->size() - 1)
			maxX2 = m_Bumps->getAt(m_decIndex + 1)->getX1();

		maxX2 = min(maxX2, x2 + maxGap);

		while ((i < n) && (m_pRefBaseLines->getAt(i)->getX1() < x2))
			i++;

		m_basIndex = i;  // as long as decels are ordered this will save time when classifying multiple decels (mostly useful for batch)
		if (i < n) 
		{
			baseline* pb = (baseline *) m_pRefBaseLines->getAt(i);
			if (pb->getX1() < maxX2)
			{
				if (pb->getX1() < minX2)
					pb->setX1(minX2);
				if (pb->length() < minBasLen)
				{
					if ((i < n - 1) && (m_pRefBaseLines->getAt(i+1)->getX1() <= maxX2))
						pb = (baseline *) m_pRefBaseLines->getAt(i+1);
					else
						pb = NULL;
				}
				if (pb)
				{
					(*b1) = pb->getX1();
					(*b2) = pb->getX2();
					bRC = true;
				}
			}
		}

		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::GetBasImmedBeforeAndAfter(decel &d, long maxDist, baseline* b1, baseline* b2)
	{
		long i = m_basIndex - 2, n = m_pRefBaseLines->size(); 
		baseline* pb;
		bool bRC = false;

		if (i < 0) i = 0;

		while ((i < n) && (m_pRefBaseLines->getAt(i)->getX2() < d.getX1() - maxDist))
			i++;

		m_basIndex = i; // update so search is quicker for subsequent decels (assumes decels ordered)

		if (i < n - 1)
		{
			pb = (baseline *) m_pRefBaseLines->getAt(i);
			if (pb->getX2() < d.getX1())
			{
				(*b1) = (*pb);
				pb = (baseline *) m_pRefBaseLines->getAt(i+1);
				if (pb->getX1() <= d.getX2() + maxDist)
				{
					bRC = true;
					(*b2) = (*pb);
				}
			}
		}

		return(bRC);
	}


	/*
		GetBiggestBump
		Find highest mean segment of length minHumpLen in portion of d between indexes x1 and x2
		Return indexes of maximum bump in b1, b2
		Note assumes x1 -> x2 provides
		*/
	void CAtypicalClassify::GetBiggestBump(const double *d, long x1, long x2, long minHumpLen, long* b1, long* b2)
	{
		long i, maxX1 = x2 - minHumpLen + 1;
		double h, maxh = 0.0, toth = 0.0; 

		x2 = x1 + minHumpLen - 1;
		h = MeanDouble(d, x1, x2, &toth); // don't actually need mean height - can just compare total height since same length segment
		maxh = toth;
		(*b1) = x1;
		(*b2) = x2;
		for (i = x1+1; i <= maxX1; i++)
		{
			x2 = i + minHumpLen - 1;
			toth -= d[i - 1];
			toth += d[x2];
			if (toth > maxh)
			{
				maxh = toth;
				(*b1) = i;
				(*b2) = x2;
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::GetDecVarScore(decel &d)
	{
		double dummy1, dummy2;

		return(GetDecVarScore(d, &dummy1, &dummy2));
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::GetDecVarScore(decel &d, double* zcScoreOut, double* wzcScoreOut)
	{
		double maxZCRatio = 0.015;
		double maxWZCRatio = 0.03;
		long len = d.length();
		double effLen = (double) len * (1.0 - d.getPercRepair());
		double* dif = GetFhrDiff(d);  // diff of signal in bump
		long* zc = NULL;
		long nzc = 0;
		double wzc, zcr, wzcr, zcScore, wzcScore;

		GetZC(dif, len - 1, &zc, &nzc);
		
		wzc = GetWeightedZC(dif, len - 1, d.getPeak() - d.getX1() + 1, zc, nzc);

		zcr = (double) nzc / effLen;
		wzcr = (100 * wzc) / (effLen * d.getBM()->getMaxHeight());
		zcScore = zcr / maxZCRatio;
		wzcScore = wzcr / maxWZCRatio;

		(*zcScoreOut) = zcScore;
		(*wzcScoreOut) = wzcScore;

		if (dif)
		{
			delete [] dif;	
		}
		if (zc)
		{
			delete [] zc;
		}

		return(zcScore + wzcScore);
	}

	/*
		GetDoubleSign - return 0 if close to 0, -1 if less than 0, +1 if greater than
		*/
	int CAtypicalClassify::GetDoubleSign(double d)
	{
		int s = 0;
		double minVal = 0.001;

		if (d > minVal)
			s = 1;
		else if (d < -minVal)
			s = -1;

		return(s);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double* CAtypicalClassify::GetFhrDiff(decel &d)
	{
		return(GetFhrDiff(d.getX1(), d.getX2()));
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double* CAtypicalClassify::GetFhrDiff(long x1, long x2)
	{
		long len = x2 - x1 + 1;
		double* d = new double[len - 1];

		for (long i = 1; i < len; i++)
			d[i-1] = m_pFHR[x1 + i] - m_pFHR[x1 + i -1];

		return(d);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::GetRise2Height(long x1, long x2, long* p)
	{
		double maxFHR = m_pFHR[x1];
		long i, maxIndex = x1;

		for (i = x1+1; i <= x2; i++)
		{
			if ((m_pFHR[i]) > maxFHR)
			{
				maxFHR = m_pFHR[i];
				maxIndex = i;
			}
		}

		(*p) = maxIndex;
		return(maxFHR - min(m_pFHR[x1], m_pFHR[x2]));

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::GetRiseVarScore(long x1, long x2)
	{
		double maxZCRatio = 0.015;
		double maxWZCRatio = 0.03;
		long len = x2 - x1 + 1, peakIndex;
		double* d = GetFhrDiff(x1, x2);  
		long* zc = NULL;
		long nzc = 0;
		double wzc, zcr, wzcr, zcScore, wzcScore;
		double h = GetRise2Height(x1, x2, &peakIndex);

		GetZC(d, len - 1, &zc, &nzc);
		wzc = GetWeightedZCRise(d, len - 1, peakIndex - x1 + 1, zc, nzc);

		zcr = (double) nzc / len;
		wzcr = (100 * wzc) / (len * h);
		zcScore = zcr / maxZCRatio;
		wzcScore = wzcr / maxWZCRatio;

		if (d)
		{
			delete [] d;
		}

		return(zcScore + wzcScore);
	}


	/* 
		GetWeightedZC:
		Get sum of magnitudes of deviations upward on downward slope (start to peak) and deviations downwards on upward slope (peak to end)
		d: signal (typically the derivative of FHR)
		n: length of d
		peakIndex: index in d of peak of decel
		zc: indexes of zero crossings
		nzc: length of zc
		returns sum of weighted deviations (indicating magnitude of deviations at zero crossings)
		*/
		
	double CAtypicalClassify::GetWeightedZC(double* d, long n, long peakIndex, long* zc, long nzc)
	{
		double zcw = 0.0, a;
		long i = 0, sIndex = 0;
		// from start to peak look for upward deviations
		while ((i < nzc) && (zc[i] < peakIndex))
		{
			a = MaxDouble(d, sIndex, zc[i]-1);
			if (a > 0)
				zcw += a;
			sIndex = zc[i];
			i = i + 1;
		}

		// post peak
		sIndex = peakIndex;
		while (i < nzc)
		{
			a = MinDouble(d, sIndex, zc[i]-1);
			if (a < 0)
				zcw -= a;
			sIndex = zc[i];
			i = i + 1;
		}

		// last zc to end
		a = MinDouble(d, sIndex, n-1);
		if (a < 0)
			zcw -= a;

		return(zcw);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::GetWeightedZCRise(double* d, long n, long peakIndex, long* zc, long nzc)
	{
		double zcw = 0.0, a;
		long i = 0, sIndex = 0;
		// from start to peak look for down deviations
		while ((i < nzc) && (zc[i] < peakIndex))
		{
			a = MinDouble(d, sIndex, zc[i]);
			if (a < 0)
				zcw -= a;
			sIndex = zc[i] + 1;
			i = i + 1;
		}

		// post peak
		sIndex = peakIndex;
		while (i < nzc)
		{
			a = MaxDouble(d, sIndex, zc[i]);
			if (a > 0)
				zcw += a;
			sIndex = zc[i] + 1;
			i = i + 1;
		}

		// last zc to end
		a = MaxDouble(d, sIndex, n-1);
		if (a > 0)
			zcw += a;

		return(zcw);
	}

		
		
	/*
		GetZC:
		Get zero crossing indexes in signal d (for atypical classification applitcations d is generatlly the derivate of FHR)
		For a zero crossing to be valid, a cumulative deviation of at least minDev above or below zero is required.
		This allows ignoring of very small deviations 
		*/

	void CAtypicalClassify::GetZC(double* d, long n, long** zc, long* nzc)
	{
		int minDev = 3;  // so can ignore minor fluctuations
		long* zcTemp = new long[n];
		long i = 0, k = 0, firstZC = 0, nAll = 0, numKeep = 0, zcIndex = 0;
		int cs = 0;
		double* zcDev = NULL;
		long* zcKeep = NULL;

		// get first non-zero diff
		while ((i < n) && (GetDoubleSign(d[i]) == 0))
		{
			i++;
		}

		cs = GetDoubleSign(d[i]); // get sign
		firstZC = i;

		// get all ZC regardless of deviation
		for (i = firstZC + 1; i < n; i++)
		{
			if ((GetDoubleSign(d[i]) != 0) && ( GetDoubleSign(d[i]) != cs))
			{
				zcTemp[nAll++] = i;
				cs = GetDoubleSign(d[i]);
			}
		}


		if (nAll > 0)
		{
			bool* keep = new bool[nAll];
			for (i = 0; i < nAll; i++)
			{
				keep[i] = false;
			}
			zcDev = GetZCDev(d, n, zcTemp, nAll);
			i = 0;
			while ((i < nAll) && (zcDev[i] < minDev))
			{
				i++;
			}
			k = i + 1;
			while (k <= nAll)
			{
				if (zcDev[k] >= minDev)
				{
					keep[k-1] = true;
					numKeep++;
					k++;
				}
				else
					k += 2;
			}

			zcKeep = new long[numKeep];
			for (i = 0; i < nAll; i++)
			{
				if (keep[i])
				{
					zcKeep[zcIndex++] = zcTemp[i];
				}
			}
			if (keep)
			{
				delete [] keep;
			}
			if (zcDev)
			{
				delete [] zcDev;
			}
		}

		

		if (zcTemp)
		{
			delete [] zcTemp;
		}

		if (*zc)
		{
			delete [] (*zc);
		}
		(*zc) = zcKeep;
		(*nzc) = numKeep;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double* CAtypicalClassify::GetZCDev(double* d, long n, long* zc, long nzc)
	{
		double* zcDev = new double[nzc+1];

		zcDev[0] = abs(SumDouble(d, 0, zc[0] - 1));
		for (long i = 1; i < nzc; i++)
		{
			zcDev[i] = abs(SumDouble(d, zc[i-1], zc[i] - 1));
		}
		zcDev[nzc] = abs(SumDouble(d, zc[nzc-1], n-1));

		return(zcDev);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::HasRepair(long x1, long x2)
	{
		long i = m_repIntIndex;
		bool bRC = false;

		while(( i < m_pRepairIntervals->size()) && (m_pRepairIntervals->getAt(i)->getX2() < x1))
			i++;

		m_repIntIndex = i; // assumes set of decels to be classified are ordered
		
		if (i < m_pRepairIntervals->size())
			bRC = (m_pRepairIntervals->getAt(i)->getX1() <= x2);
		
		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::IsBiphasic(decel &d)
	{
		double minHumpHeight = 12.0;
		long maxMinHumpLen = (long) (10 * m_dSmpFreq);
		long maxMinTroughLen = (long) (10 * m_dSmpFreq);
		long minHumpLen, minTroughLen, minS, sampsTrim;
		long x1, x2, b1, b2, t1, t2, r1, r2, l1, l2;
		long len = d.length();
		double trim = 0.1, bh, minD;
		bool giveUp = false, bRC = false, haveTrough = false;
		
		minHumpLen = min(maxMinHumpLen, len / 5);
		minTroughLen = min(maxMinTroughLen, len / 5);
		minS = minHumpLen + (2 * minTroughLen);
		sampsTrim = (long) round(trim * (double) len);
		x1 = d.getX1() + sampsTrim;
		x2 = d.getX2() - sampsTrim;

		while (!giveUp)
		{
			if ((x2 - x1 + 1) < minS)
				giveUp = true;
			else
			{
				GetBiggestBump(m_pLpFHR, x1, x2, minHumpLen, &b1, &b2);
				bh = MeanDouble(m_pLpFHR, b1, b2);
				minD = bh - minHumpHeight;
				// look to right of bump
				t2 = x2;
				if ((x2 - b2) < minTroughLen)
					t1 = x2 - minTroughLen + 1;
				else
					t1 = b2 + 1;
				haveTrough = CheckTroughForward(m_pLpFHR, t1, t2, minD, minTroughLen, &r1, &r2);
				if (!haveTrough)
				{ // no trough to right - shrink search area
					while (m_pLpFHR[b1] <= minD)
						b1++; // if hump contains samples that might be in another hump's trough
					x2 = b1 - 1;
				}
				else
				{ // found trough to right, need to look to left of hump
					if (b1 - x1 + 1 < minTroughLen)
						b1 = x1 + minTroughLen;
					t1 = x1;
					t2 = b1 - 1;
					haveTrough = CheckTroughBack(m_pLpFHR, t1, t2, minD, minTroughLen, &l1, &l2);
					if (!haveTrough)
					{
						while (m_pLpFHR[b2] <= minD)
							b2--;
						x1 = b2 + 1;
					}
					else
					{
						bRC = true; // found biphasic
						giveUp = true;
					}
				}
			}
		}

		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::IsLossRise(decel &d)
	{
		baseline b1, b2; 
		bool bRC = false, haveBas;
		
		haveBas = GetBasImmedBeforeAndAfter(d, 1, &b1, &b2);
		if (haveBas)
		{
			long inBasCheck = 30 * (long) m_dSmpFreq, minBasLen = 20 * (long) m_dSmpFreq + inBasCheck;
			long x1 = d.getX1() - inBasCheck, x2 = d.getX2() + inBasCheck, xmid = d.midpt();
			double maxOverBas = 3.0, maxBasDiff = 8.0, maxVarScore = 20.0, percBin = 0.96, maxPre, maxPost;
			
			if (x1 < 0) x1 = 0;
			if (x2 >= m_nFHR) x2 = m_nFHR;

			maxPre = PercDouble(m_pFHR, x1, xmid, percBin);
			maxPost = PercDouble(m_pFHR, xmid, x2, percBin);
			if ((maxPre <= b1.getYmean() + maxOverBas) && (maxPost <= b2.getYmean() + maxOverBas))
			{
				if ((b1.getY2() <= b1.getYmean() + maxOverBas) && (b2.getY1() <= b2.getYmean() + maxOverBas))
				{
					if (abs(b1.getYmean() - b2.getYmean()) < maxBasDiff)
						bRC = (m_dVarScore < maxVarScore);
				}
			}
		}

		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::IsLossVar(decel &d)
	{
		double maxScore = 4.0;
		bool bRC = false;

		if (m_dVarScore < maxScore)
		{ // get individual zc and wzc scores
			double totScore, zcScore, wzcScore;
			totScore = GetDecVarScore(d, &zcScore, &wzcScore);
			bRC = ((zcScore < maxScore / 2) && (wzcScore < maxScore / 2));
		}
		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::IsLowerBas(decel &d)
	{
		baseline b1, b2; 
		long maxDist = 20 * (long) m_dSmpFreq, minBasLen = 40 * (long) m_dSmpFreq;
		double minBasDrop = 15.0;
		bool bRC = false, haveBas;

		haveBas = GetBasImmedBeforeAndAfter(d, maxDist, &b1, &b2);
		if (haveBas)
		{
			if (b1.getYmean() - b2.getYmean() > minBasDrop)
				bRC = ((b1.length() >= minBasLen)  && (b2.length() >= minBasLen));
		}
				
		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::IsProlSecRise(decel &d)
	{
		double maxDecVarScore = 15.0, maxRiseVarScore = 8.0, 
			preGapRatio = 0.8, minGapHeight = 10.0;
		long minGap = 20 * (long) m_dSmpFreq, maxGapBeforeBas = 90 * (long) m_dSmpFreq;
		bool bRC = false;

		if (m_dVarScore < maxDecVarScore) // do not consider decels w/ high variability
		{
			long b1, b2;
			bool haveBas = FindBasAfterGap(d, minGap, maxGapBeforeBas, &b1, &b2); // need reference baseline post-rise	
			if (haveBas)
			{  // have reference baseline w/ gap between end of decel and baseline
				double meanb, base, meangap, minGapLevel, riseVarScore, varFactor, primrise;
				long g1, g2, p1, p2;
				meanb = MeanDouble(m_pFHR, b1, b2); // ref level
				g1 = d.getX2() + 1;    
				g2 = d.getX2() + minGap;
				if (g2 < m_nFHR)
				{
					base = (m_pFHR[d.getX1()] + m_pFHR[d.getX2()]) / 2.0; // base level of decel
					meangap = MeanDouble(m_pFHR, g1, g2); // level of gap (to see if rise)
					minGapLevel = max(meanb + minGapHeight, base); // min level of gap to be rise
					if (meangap >= minGapLevel)
					{
						if (!HasRepair(g1, g2))
						{  
							riseVarScore = GetRiseVarScore(g1, g2); // check variability of rise
							varFactor = m_dVarScore / maxDecVarScore;
							if ((varFactor * riseVarScore) < maxRiseVarScore)
							{
								p1 = d.getX1() - minGap;
								p2 = d.getX1() - 1;
								if (p1 >= 0)
								{
									primrise = MeanDouble(m_pFHR, p1, p2);
									bRC = (primrise <= (meanb + (minGapHeight * preGapRatio)));
								}
							}
						}
					}
				}
			}
		}

		return(bRC);

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::IsSlowReturn(decel &d)
	{
		long onset, recov, minRecov = 60 * (long) m_dSmpFreq;
		double roRatio, minROratio = 2.0, maxError = 0.013;
		bool bRC = false;

		onset = d.getPeak() - d.getX1();
		recov = d.getX2() - d.getPeak();
		roRatio = (double) recov / (double) onset;

		if (roRatio > minROratio)
		{
			if (recov > minRecov)
			{
				double recovH = m_pFHR[d.getX2()] - m_pFHR[d.getPeak()];
				double gradStep = recovH / (double) recov;
				double gradError = 0.0, p = m_pFHR[d.getPeak()];
				for (long k = d.getPeak()+1;k <= d.getX2(); k++)
				{
					p += gradStep;
					gradError += ((m_pFHR[k] - p) * (m_pFHR[k] - p));
				}
				gradError = sqrt(gradError) / (recovH * recov * (1 - d.getPercRepair()));
				bRC = (gradError < maxError);
			}
		}

		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CAtypicalClassify::IsSixties(decel &d)
	{
		bool bRC, x, y, z;

		x = (d.getPeakVal() <= 60); // down to 60
		y = (d.getBM()->getMaxHeight() >= 60); // 60 deep
		z = (d.length() >= (long) (60 * m_dSmpFreq));

		bRC = ((x && y) || (x && z) || (y && z));
		return(bRC);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::MaxDouble(double* d, long a, long b)
	{
		double m = 0.0;
		for (long i = a; i <= b; i++)
		{
			if (d[i] > m)
				m = d[i];
		}

		return(m);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::MeanDouble(double* d, long a, long b)
	{
		double dummy;
		return(MeanDouble(d, a, b, &dummy));

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::MeanDouble(const double* d, long a, long b)
	{
		double dummy;
		return(MeanDouble(d, a, b, &dummy));

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::MeanDouble(double* d, long a, long b, double* t)
	{
		*t = 0.0;
		long i, n = b - a + 1;

		for (i = a; i <= b; i++)
			(*t) += d[i];

		return(((*t) / (double) n));
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::MeanDouble(const double* d, long a, long b, double* t)
	{
		*t = 0.0;
		long i, n = b - a + 1;

		for (i = a; i <= b; i++)
			(*t) += d[i];

		return(((*t) / (double) n));
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::MinDouble(double* d, long a, long b)
	{
		double m = 0.0;
		for (long i = a; i <= b; i++)
		{
			if (d[i] < m)
				m = d[i];
		}

		return(m);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::PercDouble(const double* d, long a, long b, double p)
	{
		//assume p is close to 1
		long n = b - a + 1, i, index, nloc;
		index = (long) (round((double) (n-1) * p));
		nloc = n - index;
		double* dloc = new double[nloc];
		double dp;

		for (i = 0; i < nloc; i++)
			dloc[i] = d[a+i];
		SortDouble(dloc, nloc);

		for (i = a + nloc; i <= b; i++)
		{
			if (d[i] > dloc[0])
			{
				dloc[0] = d[i];
				SortDouble(dloc, nloc);
			}
		}

		dp = dloc[0];
		if (dloc)
		{
			delete [] dloc;
		}
		return(dp);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CAtypicalClassify::SortDouble(double* d, long n)
	{
	int i, j, increment;
	double temp;
	increment = n / 2;
	 
	while (increment > 0) {
		for (i = increment; i < n; i++) {
		j = i;
		temp = d[i];
		while ((j >= increment) && (d[j-increment] > temp)) {
			d[j] = d[j - increment];
			j = j - increment;
		}
		d[j] = temp;
		}
	 
		if (increment == 2)
		increment = 1;
		else 
		increment = (int) (increment / 2.2);
	}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CAtypicalClassify::SumDouble(double* d, long a, long b)
	{
		double s = 0.0;
		for (long i = a; i <=b; i++)
			s += d[i];
		return(s);
	}


	/*!
	CAtypicalConfig constructor

	*/

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CAtypicalConfig::CAtypicalConfig()
	{
		m_dSmpFreq = 4.0; // smp freq - should be set from higher level
		m_dMinLengthSec = 15.0; // min decel length in seconds
		m_dMinDepth = 15.0; // min decel depth
		m_lMinBaseLineLenSamps = 80; // min baseline length in samples (set from higher level)
		m_lLPfilterDelaySamps = 81; // lp filter delay (set from higher level)

		// Biphasic
		m_dBiphasicTrim = 0.15;  // percentage of bump on edges to not consider for biphasic search area 
		m_dBiphasicMinHumpLenSec = 3.0; // min lenght of 'hump' in biphasic pattern
		m_dBiphasicMinTroughLenSec = 3.0; // min length of corresponding troughs in biphasic pattern
		m_dBiphasicHumpMinHeight= 12.0; // min height of hump relative to troughs

		// Loss Rise
		m_dLossRiseMinHumpLenSec = 2.5;
		m_dLossRiseMinTroughLenSec = 2.5;
		m_dLossRiseMinHeight = 5.0;
		m_dLossRiseMinAboveBas = 2.0;
		m_dLossRiseMaxIntoBasSec = 10.0;

		// Prolonged Sec Rise
		m_dProlongedRiseMinHumpLenSec = 25.0;
		m_dProlongedRiseMinTroughLenSec = 10.0;
		m_dProlongedRiseMinHeight = 10.0;
		m_dProlongedRiseMinAboveBas = 10.0;

		// Lower Baseline
		m_dMaxPrevBasAgeSec = 180.0;
		m_dMinLowerBasDiff = 12.0;

		// Slow Return
		m_dSlowReturnLagSec = 60.0;

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CAtypicalConfig::~CAtypicalConfig()
	{

	}

	void CAtypicalConfig::SetSmpFreq(double dSmpFreq)
	{
		m_dSmpFreq = dSmpFreq;
	}

	void CAtypicalConfig::SetMinBaseLineLen(long lMinLen)
	{
		m_lMinBaseLineLenSamps = lMinLen;
	}

	void CAtypicalConfig::SetLPFilterDelay(long lDelay)
	{
		m_lLPfilterDelaySamps = lDelay;
	}
}







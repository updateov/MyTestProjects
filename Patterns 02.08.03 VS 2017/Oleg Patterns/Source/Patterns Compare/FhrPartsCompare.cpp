#include "stdafx.h"
#include <string.h>

#include "FhrPartsCompare.h"

namespace patterns
{

	//
	// =======================================================================================================================
	//    class event;
	// =======================================================================================================================
	//
	CFhrPartsCompare::CFhrPartsCompare(void)
	{
		Clear();
		clearMemory = false;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CFhrPartsCompare::~CFhrPartsCompare(void)
	{
		if (m_pbExpMask)
		{
			delete[] m_pbExpMask;
		}

		m_pbExpMask = NULL;

		if (m_pbFPMask)
		{
			delete[] m_pbFPMask;
		}


		m_pbFPMask = NULL;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::Clear(void)
	{
		m_strID = "";
		m_pExpert = NULL;
		m_pTest = NULL;
		m_dMinOverlap = 0.000001;
		m_lEdgeBuffer = 2600;
		m_lNumSamples = 0;
		m_lNumSampsConsider = 0;

		m_iExpert = 0;
		m_iTest = 0;
		m_iDetect = 0;
		m_iMiss = 0;
		m_iImportantMiss = 0;
		m_iTP = 0;
		m_iFP = 0;
		m_lExpSamp = 0;
		m_lSampOver = 0;
		m_lSampNon = 0;

		m_dSens = 0.0;
		m_dPPV = 0.0;
		m_dSampSens = 0.0;
		m_dSampPPV = 0.0;

		m_pbExpMask = NULL;
		m_pbFPMask = NULL;

		m_dMissRate = 0.0;
		m_dFPRate = 0.0;
		m_dExpRate = 0.0;

		m_pMissedParts.clear();
		m_pFalseParts.clear();

		if (clearMemory)
		{
			delete m_pExpert;
			delete m_pTest;
		}

		m_bDoSort = true;

		m_SetCrossTypeDecel = false;
		for (int i = 0; i < 8; ++i) for (int j = 0; j < 8; ++j) m_CrossType[i][j] = 0;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::SetID(string strID)
	{
		m_strID = strID;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::SetExpert(fhrPartSet *pExpParts, long numSamples)
	{
		if (clearMemory)
		{
			if (m_pExpert)
				delete m_pExpert;
		}
		SetClearMemory(false);

		m_pExpert = pExpParts;

		if (m_pbExpMask)
		{
			delete[] m_pbExpMask;
		}

		m_pbExpMask = new bool[m_pExpert->size()];
		for (int i = 0; i < m_pExpert->size(); i++)
		{
			m_pbExpMask[i] = false;
		}

		m_lNumSamples = numSamples;
		m_lNumSampsConsider = m_lNumSamples - (2 * m_lEdgeBuffer);
		TrimEndParts(m_pExpert, m_lEdgeBuffer);
		m_iExpert = m_pExpert->size();
	}

	void CFhrPartsCompare::SetExpert(fetus *f)
	{
		
		fhrPartSet *p = new fhrPartSet;
		FhrPartsFromFetus(p, f);
		SetExpert(p, f->get_number_of_fhr());
		SetClearMemory(true);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::SetTest(fhrPartSet *pTestParts)
	{
		if (clearMemory)
		{
			if (m_pTest)
				delete m_pTest;
		}
		SetClearMemory(false);

		m_pTest = pTestParts;

		if (m_pbFPMask)
		{
			delete[] m_pbFPMask;
		}

		m_pbFPMask = new bool[m_pTest->size()];
		for (int i = 0; i < m_pTest->size(); i++)
		{
			m_pbFPMask[i] = true;
		}

		if (m_lNumSamples > 0)
			TrimEndParts(m_pTest, m_lEdgeBuffer);

		m_iTest = m_pTest->size();
	}

	void CFhrPartsCompare::SetTest(fetus *f)
	{
		
		fhrPartSet *p = new fhrPartSet;
		FhrPartsFromFetus(p, f);
		SetTest(p);
		SetClearMemory(true);
	}

	void CFhrPartsCompare::FhrPartsFromFetus(fhrPartSet *p, fetus *f)
	{
		p->clear();
		for (long i = 0; i < f->GetEventsCount(); i++)
		{
			p->addcopy(const_cast<event *>(&(f->get_event(i))));
		}
	}


	int toIndexDecelType(fhrPart::FhrPartType v)
	{
		switch (v)
		{
			case fhrPart::DecType_VAR: return 1;
			case fhrPart::DecType_GRAD: return 2;
			case fhrPart::DecType_LATE: return 3;
			case fhrPart::DecType_EARLY: return 4;
			case fhrPart::DecType_NONASSOC: return 5;
			case fhrPart::DecType_PROL: return 6;
			case fhrPart::DecType_ATYP: return 7;
			default: return 0;
		}
	}


	string toStringDecelIndex(int v)
	{
		switch (v)
		{
			case 1: return "VAR";
			case 2: return "GRAD";
			case 3: return "LATE";
			case 4: return "EARLY";
			case 5: return "NONASSOC";
			case 6: return "PROL";
			case 7: return "ATYP";
			default: return "UNKNOWN";
		}
	}

	void CFhrPartsCompare::FhrPartsFromFetusContractions(fhrPartSet *p, fetus *f)
	{
		p->clear();
		for (long i = 0; i < f->GetContractionsCount(); i++)
		{
			p->addcopy(const_cast<contraction *>(&(f->get_contraction(i))));
		}
	}

	void CFhrPartsCompare::TrimEndParts(fhrPartSet *p, long lEdgeBuffer)
	{
		if (m_lNumSamples == 0)
		{
			m_lNumSamples = 72000;
			printf("Warning - do not have tracing length information - assuming tracing is 5 hours long for purposes of trimming end parts\n");
		}

		p->sortByEnd();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long x1cut = lEdgeBuffer;
		long x2cut = m_lNumSamples - lEdgeBuffer;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		p->filterEndingAfter(x2cut);
		p->filterStartingBefore(x1cut);

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFhrPartsCompare::is_important_miss(fhrPart *pPart)
	{
		if (pPart->isDecel())
		{
			decel* pDecel = (decel*)pPart;
			return (pDecel->length() > 4*60) || (pDecel->getHeight() > 15);
		}
		else if (pPart->isAccel())
		{
			accel* pAccel = (accel*)pPart;
			return (pAccel->length() > 4*60) || (pAccel->getHeight() > 15);
		}
		return false;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::Compare(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~*/
		fhrPart *pA = NULL;
		fhrPart *pB = NULL;
		long x1;
		long x2;
		long x1b;
		long x2b;
		bool bInLoop;
		bool bOverlap;
		int b = 0;
		double dOverlap = 0.0;
		long bSamples = 0;
		string strDump = m_strID;
		/*~~~~~~~~~~~~~~~~~~~~~*/

		strDump.append("_dump.txt");

		if (m_bDoSort)
		{
			m_pExpert->sortByEnd();
			m_pTest->sortByEnd();
		}

		if (m_pTest->size() > 0)
		{
			pB = m_pTest->getAt(0);
		}

		for (int i = 0; i < m_pExpert->size(); i++)
		{
			bInLoop = true;
			bOverlap = false;
			pA = m_pExpert->getAt(i);
			x1 = pA->getX1();
			x2 = pA->getX2();
			m_lExpSamp += pA->length();

			while (bInLoop)
			{
				while ((b < m_pTest->size()) && ((m_pTest->getAt(b))->getX2() < x1))
					b++;

				if (b < m_pTest->size())
				{
					pB = m_pTest->getAt(b);
					x1b = pB->getX1();
					x2b = pB->getX2();
					if (x1b > x2)
					{
						dOverlap = 0.0;
						bInLoop = false;
					}
					else if ((x1b <= x1) && (x2b >= x2))	// test part contains expert
					{
						m_lSampOver += pA->length();
						dOverlap = ((double) pA->length()) / ((double) pB->length());
						bInLoop = false;
					}
					else if ((x1b <= x1) && (x2b < x2))
					{
						m_lSampOver += (x2b - x1 + 1);
						dOverlap = ((double) (x2b - x1 + 1)) / ((double) max(pA->length(), pB->length()));
						bInLoop = true;						// next test part could also overlap
					}
					else if ((x1b > x1) && (x2b < x2))		// test part inside expert part
					{
						m_lSampOver += pB->length();
						dOverlap = ((double) pB->length()) / ((double) pA->length());
						bInLoop = true;
					}
					else if ((x1b > x1) && (x2b >= x2))
					{
						m_lSampOver += (x2 - x1b + 1);
						dOverlap = ((double) (x2 - x1b + 1)) / ((double) (max(pA->length(), pB->length())));
						bInLoop = false;
					}

					if (dOverlap >= m_dMinOverlap)
					{
						if (m_SetCrossTypeDecel)
						{
							if (pA->isDecel() && pB->isDecel())
							{
								// Prolonged does not exist in the system...
								m_CrossType[toIndexDecelType(((decel*)pA)->getSubtype())][toIndexDecelType(((decel*)pB)->getSubtype())] ++;
							}
						}

						bOverlap = true;
						m_pbFPMask[b] = false;
					}

					if (bInLoop)
						b++;
				}
				else
				{
					bInLoop = false;
				}
			}

			if (bOverlap)
			{
				m_iDetect++;
				m_pbExpMask[i] = true;
			}
			else
			{
				if (is_important_miss(pA))
				{
					m_iImportantMiss++;
				}
				m_iMiss++;
				m_pMissedParts.addcopy(pA);
			}
		}

		for (int i = 0; i < m_pTest->size(); i++)
		{
			bSamples += (m_pTest->getAt(i))->length();
			if (m_pbFPMask[i])
			{
				m_iFP++;
				m_pFalseParts.addcopy(m_pTest->getAt(i));
			}
		}

		m_iTP = m_pTest->size() - m_iFP;
		m_lSampNon = bSamples - m_lSampOver;
		CalcSensPPV();
		CalcRates();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::CalcSensPPV(void)
	{
		m_dSens = (double) m_iDetect / m_iExpert;
		m_dPPV = (double) m_iDetect / (m_iDetect + m_iFP);
		m_dSampSens = (double) m_lSampOver / m_lExpSamp;
		m_dSampPPV = (double) m_lSampOver / (m_lSampOver + m_lSampNon);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::CalcRates(void)
	{
		if (m_lNumSamples > 0)	// have # sample info
		{
			m_dExpRate = (double) m_iExpert / m_lNumSampsConsider;
			m_dMissRate = (double) m_iMiss / m_lNumSampsConsider;
			m_dFPRate = (double) m_iFP / m_lNumSampsConsider;
		}
		else
		{
			m_dExpRate = 0.0;
			m_dMissRate = 0.0;
			m_dFPRate = 0.0;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::Merge(CFhrPartsCompare *pPartsComp)
	{
		// Clear the following fields meant for a single file to avoid possible confusion
		m_strID = "";
		if (m_pbExpMask)
		{
			delete[] m_pbExpMask;
		}

		m_pbExpMask = NULL;

		if (m_pbFPMask)
		{
			delete[] m_pbFPMask;
		}

		m_pbFPMask = NULL;

		m_pMissedParts.clear();
		m_pFalseParts.clear();

		// Now sum the appropriate fields
		if (pPartsComp->m_lNumSamples > 0)
		{
			m_lNumSamples += pPartsComp->m_lNumSamples;
			m_lNumSampsConsider += pPartsComp->m_lNumSampsConsider;
		}
		else
		{	// if don't have # sample info then clear all # samp info
			m_lNumSamples = 0;
			m_lNumSampsConsider = 0;
		}

		m_iExpert += pPartsComp->m_iExpert;
		m_iTest += pPartsComp->m_iTest;
		m_iDetect += pPartsComp->m_iDetect;
		m_iMiss += pPartsComp->m_iMiss;
		m_iImportantMiss += pPartsComp->m_iImportantMiss;
		m_iTP += pPartsComp->m_iTP;
		m_iFP += pPartsComp->m_iFP;
		m_lExpSamp += pPartsComp->m_lExpSamp;
		m_lSampOver += pPartsComp->m_lSampOver;
		m_lSampNon += pPartsComp->m_lSampNon;

		for (int i = 0; i < 8; ++i) for (int j = 0; j < 8; ++j) m_CrossType[i][j] += pPartsComp->m_CrossType[i][j];

		// Now recalc sens/ppv and rates
		CalcSensPPV();
		CalcRates();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::PrintHeaderString(FILE *fid, bool simple)
	{
		// FileID (20) ExpEv(10) Detect(10) Miss(10) Miss/hr(10) ExpSamp(10) SampOver(10)
		// SampNon(10) ... ... SampSens(10) SampPPV(10) TP(10) FP(10) FP/hr(10) Sens(10)
		// PPV(10) Sens+PPPV(10)
		if (simple)
		{
			fprintf
				(
				fid,
				"%20s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s\n",
				"FileID",
				"ExpEv",
				"TestEv",
				"Detect",
				"Miss",
				"High Miss",
				"SampSens",
				"SampPPV",
				"TP",
				"FP",
				"Sens",
				"PPV",
				"Sens+PPV"
				);
		}
		else
		{
			fprintf
				(
				fid,
				"%20s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s %10s\n",
				"FileID",
				"ExpEv",
				"TestEv",
				"Detect",
				"Miss",
				"High Miss",
				"Miss/hr",
				"ExpSamp",
				"SampOver",
				"SampNon",
				"SampSens",
				"SampPPV",
				"TP",
				"FP",
				"FP/hr",
				"Sens",
				"PPV",
				"Sens+PPV"
				);
		}
	}

	void CFhrPartsCompare::PrintCrossTypeDecel(FILE *fid)
	{
		if (m_SetCrossTypeDecel)
		{
			fprintf(fid, "\n\nClassification performance of detected decelerations\n%20s ", "");
			for (int i = 0; i < 8; ++i)
			{
				fprintf(fid, "%10s ", toStringDecelIndex(i).c_str());
			}
			for (int i = 0; i < 8; ++i)
			{
				fprintf(fid, "\n%20s ", toStringDecelIndex(i).c_str());
				for (int j = 0; j < 8; ++j)
				{
					fprintf(fid, "%10d ", m_CrossType[i][j]);
				}
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFhrPartsCompare::PrintOPString(FILE *fid, bool simple)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string strOP;
		double sph = (4.0 * 60.0 * 60.0);	// samps per hour at 4 Hz
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		if (simple)
		{
			fprintf
				(
				fid,
				"%20s %10d %10d %10d %10d %10d %10.2f %10.2f %10d %10d %10.2f %10.2f %10.2f\n",
				m_strID.c_str(),
				m_iExpert,
				m_iTest,
				m_iDetect,
				m_iMiss,
				m_iImportantMiss,
				100.0 * m_dSampSens,
				100.0 * m_dSampPPV,
				m_iTP,
				m_iFP,
				100.0 * m_dSens,
				100.0 * m_dPPV,
				100.0 * (m_dSens + m_dPPV)
				);
		}
		else
		{
			fprintf
				(
				fid,
				"%20s %10d %10d %10d %10d %10d %10.2f %10d %10d %10d %10.2f %10.2f %10d %10d %10.2f %10.2f %10.2f %10.2f\n",
				m_strID.c_str(),
				m_iExpert,
				m_iTest,
				m_iDetect,
				m_iMiss,
				m_iImportantMiss,
				m_dMissRate * sph,
				m_lExpSamp,
				m_lSampOver,
				m_lSampNon,
				100.0 * m_dSampSens,
				100.0 * m_dSampPPV,
				m_iTP,
				m_iFP,
				m_dFPRate * sph,
				100.0 * m_dSens,
				100.0 * m_dPPV,
				100.0 * (m_dSens + m_dPPV)
				);
		}


	}
}
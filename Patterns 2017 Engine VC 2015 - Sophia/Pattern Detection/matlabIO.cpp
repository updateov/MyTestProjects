#include "stdafx.h"

#ifdef MATLAB_OUTPUT

#include "matlabIO.h"

namespace patterns
{

	// ! =CMatSignal::<constructor>
	CMatSignal::CMatSignal()
	{
		m_pSignalDouble = NULL;
		m_pSignalLong = NULL;
		m_lSize = 0;
		m_bSaveAsUD = false;
		m_bIsDouble = true;
	}

	//
	// =======================================================================================================================
	//    ! CMatSignal::<destructor>
	// =======================================================================================================================
	//
	CMatSignal::~CMatSignal(void)
	{
		m_pSignalDouble = NULL;
		m_pSignalLong = NULL;
	}

	/*
	=======================================================================================================================
	! CMatSignal::OutputToFile Writes the signal to the output mat file \return true if success, false if failure A
	note on indexing - signal index in C++ will be one less than in MATLAB (e.g. m_pdLowPassSignal[0] is equivalent to
	filterLP(1). When outputting a signal to .mat format, this is automatically taken care of - array indexing in
	MATLAB starts at 1 instead of 0. When outputting baselines, accels, and decels, sample indexes will need to be
	shifted by 1
	=======================================================================================================================
	*/
	bool CMatSignal::OutputToFile(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		MATFile *pmat = matOpen(m_strOutputFile.c_str(), "w");
		bool bRC = true;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (pmat == NULL)
		{
			return false;
		}

		/*~~~~~~~~~~~~*/
		mxArray *sigOut;
		mxArray *udOut;
		/*~~~~~~~~~~~~*/

		sigOut = mxCreateDoubleMatrix(1, m_lSize, mxREAL);
		if (m_bIsDouble)
		{
			memcpy((void *) (mxGetPr(sigOut)), (void *) m_pSignalDouble, (m_lSize * (sizeof(*m_pSignalDouble))));
		}
		else
		{
			memcpy((void *) (mxGetPr(sigOut)), (void *) m_pSignalLong, (m_lSize * (sizeof(*m_pSignalLong))));
		}

		if (m_bSaveAsUD)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			const char *field_names[] = { "filename", "fhr" };
			int iNumFields = sizeof(field_names) / sizeof(*field_names);
			int iDims[2] = { 1, 1 };
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			udOut = mxCreateStructArray(2, iDims, iNumFields, field_names);

			mxSetField(udOut, 0, "filename", mxCreateString(m_strOutputFile.c_str()));
			mxSetField(udOut, 0, "fhr", sigOut);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			int status = matPutVariable(pmat, "ud_save", udOut);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			bRC = (status == 0);

			// mxDestroyArray(udOut);
		}
		else
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			int status = matPutVariable(pmat, m_strOutputVarName.c_str(), sigOut);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			bRC = (status == 0);
		}

		mxDestroyArray(sigOut);

		// mxDestroyArray(udOut);
		// // get error if try to free this - may cause memory leak
		matClose(pmat);

		return bRC;
	}

/*
=======================================================================================================================
! CMatSignal::ReadFromFile :  Read input signal from .mat file - this is much faster than converting .mat to .xml and reading
in .xml.  Can only read in fhr and up, not events or contractions.
=======================================================================================================================
*/

bool CMatSignal::ReadFromFile(int** pfhr, long* nfhr, int** pup, long* nup)
{
	MATFile *pmat = matOpen(m_strOutputFile.c_str(), "r");
	bool bRC = (pmat != NULL);
	mxArray *pa, *pmatfhr, *pmatup;
	double *dfhr, *dup;

	pa = matGetVariable(pmat, "ud_save");
	pmatfhr = mxGetField(pa, 0, "fhr");
	*nfhr=mxGetNumberOfElements(pmatfhr);
	dfhr=mxGetPr(pmatfhr);
	pmatup = mxGetField(pa, 0, "uc");
	*nup = mxGetNumberOfElements(pmatup);
	dup = mxGetPr(pmatup);

	*pfhr = new int[*nfhr];
	for (long i = 0; i < *nfhr; i++)
	{
		(*pfhr)[i] = (int) (dfhr[i] + 0.5);
	}
	*pup = new int[*nup];
	for (long i = 0; i < *nup; i++)
	{
		(*pup)[i] = (int) (dup[i] + 0.5);
	}

	return(bRC);
}

/*
=======================================================================================================================
! CMatSignal::ToInFileString - convert fhr and up signals to str for .in file which can then be imported to fetus object
=======================================================================================================================
*/

string CMatSignal::ToInFileString(int* pfhr, long nfhr, int* pup, long nup)
{
	bool up4 = (nfhr == nup);
	long nlines = nfhr / 12;  // in format has 12 fhr and 3 up per line
	string str("01/01/00 00:00\r\n");
	long i, j;
	char temp[10];

	for (i = 0; i < nlines; i++)
	{
		for (j = 0 ; j < 12; j++)
		{
			To3DigStr(temp, pfhr[(i*12) + j]);
			
			str += temp;
			str += " ";
		}
		if (up4) // up is specified in 4 Hz - need to downsample
		{
			for (j = 0; j < 12; j += 4)
			{
				To3DigStr(temp, pup[(i*12) + j]);
				str += temp;
				str += " ";
			}
		}
		else
		{
			for (j = 0; j < 3; j++)
			{
				To3DigStr(temp, pup[(i*3) + j]);
				str += temp;
				str += " ";
			}
		}
		str += "\r\n";
	}

	return(str);
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CMatSignal::To3DigStr(char* s, int x)
{
	if (x < 10)
	{
		sprintf(&(s[0]), "%d", 0);
		sprintf(&(s[1]), "%d", 0);
		sprintf(&(s[2]), "%d", x);
	}
	else if (x < 100)
	{
		sprintf(&(s[0]), "%d", 0);
		sprintf(&(s[1]), "%d", x);
	}
	else
	{
		sprintf(s, "%d", x);
	}
}


	//
	// =======================================================================================================================
	//    ! CMatBumpSet::<constructor>
	// =======================================================================================================================
	//
	CMatFhrPartSet::CMatFhrPartSet(void)
	{
		m_pFhrParts = NULL;
		m_bSaveAsUD = true;
	}

	//
	// =======================================================================================================================
	//    ! CMatSignal::<destructor>
	// =======================================================================================================================
	//
	CMatFhrPartSet::~CMatFhrPartSet(void)
	{
		m_pFhrParts = NULL;
	}

	/*
	=======================================================================================================================
	! CMatFhrPartSet::OutputToFile Writes the signal to the output mat file \return true if success, false if failure
	Sample indexes need to be shifted by 1 because MATLAB indexing starts at 1
	=======================================================================================================================
	*/
	bool CMatFhrPartSet::OutputToFile(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		MATFile *pmat = matOpen(m_strOutputFile.c_str(), "w");
		bool bRC = true;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (pmat == NULL)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~*/
		mxArray *udOut;
		mxArray *fhrPartVector;
		/*~~~~~~~~~~~~~~~~~~~*/

		if (m_bSaveAsUD)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			const char *field_names[] = { "filename", "fhr_part_vector" };
			int iNumFields = sizeof(field_names) / sizeof(*field_names);
			int iDims[2] = { 1, 1 };
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			udOut = mxCreateStructArray(2, iDims, iNumFields, field_names);

			mxSetField(udOut, 0, "filename", mxCreateString(m_strOutputFile.c_str()));

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// now create fhr_part_vector
			const char *fhrPartVectorFNs[] = { "fhr_type", "x_beg", "x_end", "y1", "y2", "freqBand", "fhr_part_handle", "part_charact", "confidence", "height", "basVar", "peakVal", "isLate", "isVariable", "percRepair", "lag", "contrIndex", "timestamp", "nonInterp", "pending", "mhClass", "mhType"};
			int iNumFhrPartFields = sizeof(fhrPartVectorFNs) / sizeof(*fhrPartVectorFNs);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			iDims[0] = 1;
			iDims[1] = m_pFhrParts->size();
			fhrPartVector = mxCreateStructArray(2, iDims, iNumFhrPartFields, fhrPartVectorFNs);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			const char *typePrivateFNs[] = { "bas_val" };
			int iNumTPfields = sizeof(typePrivateFNs) / sizeof(*typePrivateFNs);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			iDims[0] = 1;
			iDims[1] = 1;

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// mxArray *typePrivate = mxCreateStructArray(2, iDims, iNumTPfields, typePrivateFNs);
			string strFhrType;
			string strPartChar = "empty";
			fhrPart::FhrPartType subt;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			for (int i = 0; i < m_pFhrParts->size(); i++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				mxArray *mxStart;
				mxArray *mxEnd;
				mxArray *mxFreqBand;
				mxArray *mxConf;
				mxArray *mxHeight;
				mxArray *mxBasVar;
				mxArray *mxPeakVal; // output although currently cannot us when convertion MAT output back to dev XML because of class backwards compatibility issues
				mxArray *mxIsLate;
				mxArray *mxIsVariable;
				mxArray *mxPercRepair, *mxLag, *mxContrIndex, *mxY1, *mxY2, *mxTimestamp, *mxPending, *mxNonInterp, *mxMhClass, *mxMhType;
				
				long x1, x2, freqBand = -1, lag = -1, timestamp = -1, contrIndex = -1, mhType = baseline::MH_NONE;
				double y1 = 0.0, y2 = 0.0, confidence = -1.0, height = -1.0, basVar = -1.0, peakVal = -1.0, percRepair = -1.0;
				bool isLate = false, isVariable = false, nonInterp = false, pending = false, mhClass = false;
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				strPartChar = "empty";
				fhrPart *p = m_pFhrParts->getAt(i);

				x1 = p->getX1() + 1; // shift by one - indexing offset as compared to MATLAB - add one so will match up with output FHR signal
				x2 = p->getX2() + 1;
				timestamp = p->getTimestamp() + 1;
				pending = p->isPending();
				percRepair = p->getPercRepair();
				
				if (p->isBump())
				{
					bump *b = (bump *) m_pFhrParts->getAt(i);
					freqBand = b->getFreqBand();
					confidence = b->getConfidence();
					height = b->getBM()->getMaxHeight();
					peakVal = b->getPeakVal();
					nonInterp = b->isNonInterp();
				}
				if (p->isDecel())
				{
					decel* d = (decel *) m_pFhrParts->getAt(i);
					isLate = d->isLate();
					isVariable = d->isVariable();
					lag = d->getLag();
					contrIndex = d->getContrIndex();
					strFhrType = "DEC";
					subt = d->getSubtype();
					switch (subt)
					{
					case fhrPart::DecType_ATYP:
						strFhrType = "DAV";
						strPartChar = "atyp";
						if (d->isBiphasic()) {strPartChar += "BI";}
						if (d->isLossRise()) {strPartChar += "LR";}
						if (d->isLossVar()) {strPartChar += "LV";}
						if (d->isLowerBas()) {strPartChar += "LB";}
						if (d->isProlSecRise()) {strPartChar += "P2";}
						if (d->isSixties()) {strPartChar += "60";}
						if (d->isSlowReturn()) {strPartChar += "SR";}
						if (d->isLate()) {strPartChar += "LT";}
						break;
					case fhrPart::DecType_EARLY:
						strPartChar = "early";
						break;
					case fhrPart::DecType_GRAD:
						strPartChar = "gradN";
						break;
					case fhrPart::DecType_LATE:
						strPartChar = "late";
						break;
					case fhrPart::DecType_NONASSOC:
						strPartChar = "gradN";
						break;
					case fhrPart::DecType_PROL:
						strPartChar = "prol";
						break;
					case fhrPart::DecType_VAR:
						strFhrType = "DAB";
						strPartChar = "slow";
						break;
					default:
						strFhrType = "OTH";
					}
				}
				if (p->isNonDecel())
				{
					strFhrType = "OTH";
					strPartChar = "nonDecel";
				}
				if (p->isAccel())
				{
					strFhrType = "ACC";
				}
				if (p->isNonAccel())
				{
					strFhrType = "OTH";
					strPartChar = "nonAccel";
				}
				if (p->isBaseline())
				{
					baseline *b = (baseline *) m_pFhrParts->getAt(i);
					strFhrType = "BAS";
					y1 = b->getY1();
					y2 = b->getY2();
					mhClass = b->getMhClass();
					mhType = b->getMhType();
					basVar = b->getVar();
				}

				mxSetField(fhrPartVector, i, "fhr_type", mxCreateString(strFhrType.c_str()));
				mxSetField(fhrPartVector, i, "part_charact", mxCreateString(strPartChar.c_str()));
				mxStart = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxEnd = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxFreqBand = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxConf = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxHeight = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxBasVar = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxPeakVal = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxIsLate = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxIsVariable = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxPercRepair = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxLag = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxContrIndex = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxY1 = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxY2 = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxTimestamp = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxPending = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxNonInterp = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxMhClass = mxCreateDoubleMatrix(1, 1, mxREAL);
				mxMhType = mxCreateDoubleMatrix(1, 1, mxREAL);

				(*(mxGetPr(mxStart))) = x1;
				mxSetField(fhrPartVector, i, "x_beg", mxStart);
				(*(mxGetPr(mxEnd))) = x2;
				mxSetField(fhrPartVector, i, "x_end", mxEnd);
				(*(mxGetPr(mxFreqBand))) = freqBand;
				mxSetField(fhrPartVector, i, "freqBand", mxFreqBand);
				(*(mxGetPr(mxConf))) = confidence;
				mxSetField(fhrPartVector, i, "confidence", mxConf);
				(*(mxGetPr(mxHeight))) = height;
				mxSetField(fhrPartVector, i, "height", mxHeight);
				(*(mxGetPr(mxBasVar))) = basVar;
				mxSetField(fhrPartVector, i, "basVar", mxBasVar);
				(*(mxGetPr(mxPeakVal))) = peakVal;
				mxSetField(fhrPartVector, i, "peakVal", mxPeakVal);
				(*(mxGetPr(mxIsLate))) = (double) isLate;
				mxSetField(fhrPartVector, i, "isLate", mxIsLate);
				(*(mxGetPr(mxIsVariable))) = (double) isVariable;
				mxSetField(fhrPartVector, i, "isVariable", mxIsVariable);
				(*(mxGetPr(mxPercRepair))) = percRepair;
				mxSetField(fhrPartVector, i, "percRepair", mxPercRepair);
				(*(mxGetPr(mxLag))) = lag;
				mxSetField(fhrPartVector, i, "lag", mxLag);
				(*(mxGetPr(mxContrIndex))) = contrIndex;
				mxSetField(fhrPartVector, i, "contrIndex", mxContrIndex);
				(*(mxGetPr(mxY1))) = y1;
				mxSetField(fhrPartVector, i, "y1", mxY1);
				(*(mxGetPr(mxY2))) = y2;
				mxSetField(fhrPartVector, i, "y2", mxY2);
				(*(mxGetPr(mxTimestamp))) = timestamp;
				mxSetField(fhrPartVector, i, "timestamp", mxTimestamp);
				(*(mxGetPr(mxPending))) = pending;
				mxSetField(fhrPartVector, i, "pending", mxPending);
				(*(mxGetPr(mxNonInterp))) = nonInterp;
				mxSetField(fhrPartVector, i, "nonInterp", mxNonInterp);
				(*(mxGetPr(mxMhClass))) = (double) mhClass;
				mxSetField(fhrPartVector, i, "mhClass", mxMhClass);
				(*(mxGetPr(mxMhType))) = mhType;
				mxSetField(fhrPartVector, i, "mhType", mxMhType);
			}

			mxSetField(udOut, 0, "fhr_part_vector", fhrPartVector);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// mxSetField(udOut, 0, "fhr", sigOut);
			int status = matPutVariable(pmat, "ud_save", udOut);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			bRC = (status == 0);

			// mxDestroyArray(udOut);
			// mxDestroyArray(fhrPartVector);
		}

		// else { int status = matPutVariable(pmat, m_strOutputVarName.c_str(), sigOut);
		// bRC = (status == 0);
		// } ;
		// mxDestroyArray(sigOut);
		// mxDestroyArray(udOut);
		// // get error if try to free this - may cause memory leak
		matClose(pmat);

		return bRC;
	}

	//
	// =======================================================================================================================
	//    ! CMatContractionSet::<constructor>
	// =======================================================================================================================
	//
	CMatContractionSet::CMatContractionSet(void)
	{
		m_pContractions = new ContractionDetection[10];
		m_lMaxSize = 10;
		m_lSize = 0;
	}

	//
	// =======================================================================================================================
	//    ! CMatContractionSet::<destructor>
	// =======================================================================================================================
	//
	CMatContractionSet::~CMatContractionSet(void)
	{
		if (m_pContractions)
		{
			delete[] m_pContractions;
		}

		m_pContractions = NULL;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CMatContractionSet::Add(ContractionDetection c)
	{
		if (m_lSize == m_lMaxSize)
		{
			m_lMaxSize = m_lMaxSize * 2;

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			ContractionDetection *pNewC = new ContractionDetection[m_lMaxSize];
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			for (long i = 0; i < m_lSize; i++)
			{
				pNewC[i] = m_pContractions[i];
			}

			if (m_pContractions)
			{
				delete m_pContractions;
			}

			m_pContractions = pNewC;
		}

		m_pContractions[m_lSize++] = c;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CMatContractionSet::GetLastXEnd(void)
	{
		/*~~~~~~~~~*/
		long lX2 = 0;
		/*~~~~~~~~~*/

		if (m_lSize > 0)
		{
			lX2 = m_pContractions[m_lSize - 1].lEnd;
		}

		return lX2;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CMatContractionSet::OutputToFile(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		MATFile *pmat = matOpen(m_strOutputFile.c_str(), "w");
		bool bRC = true;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (pmat == NULL)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		mxArray *udOut;
		mxArray *ucPartVector;
		const char *field_names[] = { "filename", "uc_part_vector" };
		int iNumFields = sizeof(field_names) / sizeof(*field_names);
		int iDims[2] = { 1, 1 };
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		udOut = mxCreateStructArray(2, iDims, iNumFields, field_names);

		mxSetField(udOut, 0, "filename", mxCreateString(m_strOutputFile.c_str()));

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// now create fhr_part_vector
		const char *ucPartVectorFNs[] = { "x_beg", "x_peak", "x_end", "uc_type", "type_private", "uc_part_handle", "uc_part_vector" };
		int iNumUcPartFields = sizeof(ucPartVectorFNs) / sizeof(*ucPartVectorFNs);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		iDims[0] = 1;
		iDims[1] = m_lSize;
		ucPartVector = mxCreateStructArray(2, iDims, iNumUcPartFields, ucPartVectorFNs);

		/*~~~~~~~~~~~~~~~~~~~*/
		string ucType = "CONT";
		/*~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_lSize; i++)
		{
			/*~~~~~~~~~~~~~*/
			mxArray *mxStart;
			mxArray *mxEnd;
			mxArray *mxPeak;
			/*~~~~~~~~~~~~~*/

			mxSetField(ucPartVector, i, "uc_type", mxCreateString(ucType.c_str()));

			// mxSetField(fhrPartVector, i, "part_charact", mxCreateString(strPartChar.c_str()));
			mxStart = mxCreateDoubleMatrix(1, 1, mxREAL);
			mxEnd = mxCreateDoubleMatrix(1, 1, mxREAL);
			mxPeak = mxCreateDoubleMatrix(1, 1, mxREAL);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			double dStart = m_pContractions[i].lStart + 1;	// shift by one - indexing offset as compared to MATLAB
			double dEnd = m_pContractions[i].lEnd + 1;		// shift by one - indexing offset as compared to MATLAB
			double dPeak = m_pContractions[i].lPeak + 1;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			(*(mxGetPr(mxStart))) = dStart;

			// memcpy((void *)(mxGetPr(fieldVal)), &(m_pFhrParts[i].lStart), (sizeof(m_pFhrParts[i].lStart)));
			mxSetField(ucPartVector, i, "x_beg", mxStart);
			(*(mxGetPr(mxEnd))) = dEnd;

			// memcpy((void *)(mxGetPr(fieldVal)), &(m_pFhrParts[i].lEnd), (sizeof(m_pFhrParts[i].lEnd)));
			mxSetField(ucPartVector, i, "x_end", mxEnd);
			(*(mxGetPr(mxPeak))) = dPeak;
			mxSetField(ucPartVector, i, "x_peak", mxPeak);
		}

		mxSetField(udOut, 0, "uc_part_vector", ucPartVector);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int status = matPutVariable(pmat, "ud_save", udOut);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		bRC = (status == 0);

		matClose(pmat);

		return bRC;
	};
}
#endif
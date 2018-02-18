/*
* ! PROJECT: LMS -- BaseLine Detection Copyright LMS Medical Systems 2004 by
* Evonium Inc
*/
#include "stdafx.h"
#include "Repair.h"
#include "BaseLine.h"

#include "DigitalSignal.h"

namespace patterns
{

	// ! Proper definition of a byte.
#ifndef BYTE
	typedef unsigned char BYTE;
#endif

	//
	// =======================================================================================================================
	//    ! CBaseLineConfig::<constructor>
	// =======================================================================================================================
	//
	CBaseLineConfig::CBaseLineConfig(void)
	{
		m_dSmpFreq = 4.0;					// sampling frequency

		// Parameters in seconds
		m_dCommitBuffer = 4.0 * 60.0;			// commit buffer in seconds
		m_iBasTruncateLen = 5 * 60;			// length at which can truncate without waiting for more data (sec)
		m_iMinBasLength = 20;				// minimum baseline length (sec)
		m_iMaxDistForMergeTry = 60;			// max dist btwn bas. to attempt a merge (in seconds)

		// Box width params
		m_pPolyVecMid = new int[2];
		m_pPolyVecMid[0] = 27;
		m_pPolyVecMid[1] = 3;

		m_pPolyVecSmall = new int[2];
		m_pPolyVecSmall[0] = 13;
		m_pPolyVecSmall[1] = 3;

		m_dSmallVarThresh = 0;
		m_dMinBoxWidth = 3;
		m_dMaxBoxWidth = 30;
		m_dWidthMultFactor = 1.05;

		// Box slope params
		m_dCorridorSlopeOffset = 2;
		m_dCorridorSlope = (double) 15.0 / (9.0 * 240.0);
		m_dSlopeMultFactor = 1.25;

		// Advancement of box start index
		m_bAdvOverExtremaDuringBas = true;

		// m_bAdvOverExtremaDuringNonBas = false;
		m_bAdvOverExtremaDuringNonBas = true;
		m_iSlowAdvStep = 1;					// in samples
		m_bSkipToLastValidEnd = false;

		// FlatBox simple
		m_bDoFlatBoxBinarySearch = true;
		m_bAlwaysFlatBoxSimple = false;

		// Extend Right
		m_iNumSuccFailAllow = 1;
		m_bAllowSuccFail = false;
		m_bDoExtendRightBinarySearch = false;

		// CalcExtremaBoxes
		m_bDoBinaryBackForAccepted = true;
		m_bDoBinaryForwardForAccepted = true;

		// Merging
		m_bNoInvalidBits = false;

		m_bUsePrecomputedPolyFitSums = false;

		m_bSkipRepairSegments = true;
		m_dMinRepairSegmentLength = 10.0;	// in seconds
	}

	//
	// =======================================================================================================================
	//    ! CBaseLineConfig::<destructor>
	// =======================================================================================================================
	//
	CBaseLineConfig::~CBaseLineConfig(void)
	{
		delete[] m_pPolyVecMid;
		m_pPolyVecMid = NULL;

		delete[] m_pPolyVecSmall;
		m_pPolyVecSmall = NULL;
	}

	//
	// =======================================================================================================================
	//    ! CLinePointArray::<constructor>
	// =======================================================================================================================
	//
	CLinePointArray::CLinePointArray(void)
	{
		m_pData = NULL;
		m_nSize = m_nMaxSize = m_nGrowBy = 0;
	}

	//
	// =======================================================================================================================
	//    ! CLinePointArray::<destructor>
	// =======================================================================================================================
	//
	CLinePointArray::~CLinePointArray(void)
	{
		for (int i = 0; i < GetSize(); i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~*/
			CLinePoint *pLine = GetAt(i);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (pLine)
			{
				delete pLine;
				pLine = NULL;
			}
		}

		SetSize(0);

		if (m_pData != NULL)
		{
			delete[] m_pData;
		}
	}

	/*
	=======================================================================================================================
	! CLinePointArray::GetSize: Gets the current size of the array. \return The array's current size.
	=======================================================================================================================
	*/
	int CLinePointArray::GetSize(void) const
	{
		return m_nSize;
	}

	/*
	=======================================================================================================================
	! CLinePointArray::GetCount: As GetSize(), gets the current size of the array. \return The array's current size.
	=======================================================================================================================
	*/
	int CLinePointArray::GetCount(void) const
	{
		return m_nSize;
	}

	/*
	=======================================================================================================================
	! CLinePointArray::RemoveAll: Removes all occurrences that were included in the array.
	=======================================================================================================================
	*/
	void CLinePointArray::RemoveAll(void)
	{
		SetSize(0, -1);
	}

	/*
	=======================================================================================================================
	! CLinePointArray::GetAt: Gets the element at the given index. \param nIndex Index of the required element. \return
	Pointer to the required element.
	=======================================================================================================================
	*/
	CLinePoint *CLinePointArray::GetAt(int nIndex)
	{
		return m_pData[nIndex];
	}

	/*
	=======================================================================================================================
	! CLinePointArray::SetAt: Sets the specified element in the array at the given index. \param nIndex Index of the
	required element. \param newElement The element to set in the array.
	=======================================================================================================================
	*/
	void CLinePointArray::SetAt(int nIndex, CLinePoint *newElement)
	{
		m_pData[nIndex] = newElement;
	}

	/*
	=======================================================================================================================
	! CLinePointArray::Add: Adds the specified element to the array. The array's size will increase by one after this
	call. \param newElement The element to set in the array. \return Index where the required element was set.
	=======================================================================================================================
	*/
	int CLinePointArray::Add(CLinePoint *newElement)
	{
		/*~~~~~~~~~~~~~~~~~*/
		int nIndex = m_nSize;
		/*~~~~~~~~~~~~~~~~~*/

		SetAtGrow(nIndex, newElement);
		return nIndex;
	}

	/*
	=======================================================================================================================
	! CLinePointArray::SetSize: Sets the size of the array to one specified. It also sets the size by which the array
	will grow whenever it needs to do so. \param nNewSize The array's requested new size. \param nGrowBy The size by
	which the array should grow whenever it has to. \return The array's previous size.
	=======================================================================================================================
	*/
	int CLinePointArray::SetSize(int nNewSize, int nGrowBy)
	{
		/*~~~~~~~~~~~~~~~~~~~*/
		int nOldSize = m_nSize;
		/*~~~~~~~~~~~~~~~~~~~*/

		if (nGrowBy != -1)
		{
			m_nGrowBy = nGrowBy;				// set new size
		}

		if (nNewSize == 0)						// Array now contains 0 elements
		{
			if (m_pData != NULL)
			{
				delete[] m_pData;
				m_pData = NULL;
			}

			m_nSize = m_nMaxSize = 0;
		}
		else if (m_pData == NULL)
		{
			m_pData = (CLinePoint **) new BYTE[(size_t) nNewSize * sizeof(CLinePoint *)];
			memset((void *) m_pData, 0, (size_t) nNewSize * sizeof(CLinePoint *));

			m_nSize = m_nMaxSize = nNewSize;
		}
		else if (nNewSize <= m_nMaxSize)
		{
			if (nNewSize > m_nSize)
			{
				memset((void *) (m_pData + m_nSize), 0, (size_t) (nNewSize - m_nSize) * sizeof(CLinePoint *));
			}

			m_nSize = nNewSize;
		}
		else									// nNewSize > m_nMaxSize
		{
			nGrowBy = m_nGrowBy;

			if (nGrowBy == 0)
			{
				nGrowBy = m_nSize / 8;
				nGrowBy = (nGrowBy < 4) ? 4 : ((nGrowBy > 1024) ? 1024 : nGrowBy);
			}

			/*~~~~~~~~~~~~*/
			int nNewMax = 0;
			/*~~~~~~~~~~~~*/

			if (nNewSize < m_nMaxSize + nGrowBy)
			{
				nNewMax = m_nMaxSize + nGrowBy; // granularity
			}
			else
			{
				nNewMax = nNewSize;		// no slush
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			CLinePoint **pNewData = (CLinePoint **) new BYTE[(size_t) nNewMax * sizeof(CLinePoint *)];
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			memcpy(pNewData, m_pData, (size_t) m_nSize * sizeof(CLinePoint *));

			memset((void *) (pNewData + m_nSize), 0, (size_t) (nNewSize - m_nSize) * sizeof(CLinePoint *));

			delete[] m_pData;
			m_pData = pNewData;
			m_nSize = nNewSize;
			m_nMaxSize = nNewMax;
		}

		return nOldSize;
	}

	/*
	=======================================================================================================================
	! CLine::operator= : Makes this occurrence equal to the one passed as parameter.
	=======================================================================================================================
	*/
	CLine &CLine::operator=(const CLine &srcObj)
	{
		if (this == &srcObj)
		{
			return *this;
		}

		m_Point1 = srcObj.m_Point1;
		m_Point2 = srcObj.m_Point2;
		m_dSlope = srcObj.m_dSlope;
		m_dYIntercept = srcObj.m_dYIntercept;
		m_nLength = srcObj.m_nLength;
		return *this;
	}

	/*
	=======================================================================================================================
	! CLine::Empty: Resets the current occurrence to it's initial value and frees any allocated dynamic memory.
	=======================================================================================================================
	*/
	void CLine::Empty(void)
	{
		m_Point1.Empty();
		m_Point2.Empty();
		m_dSlope = 0.0;
		m_dYIntercept = 0.0;
		m_nLength = 0;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CLine::CalcLength(void)
	{
		m_nLength = GetX2() - GetX1() + 1;
	}

	/*
	=======================================================================================================================
	! CLine::GetLineCoefficients: TBD
	=======================================================================================================================
	*/
	void CLine::GetLineCoefficients(double *pSignal)
	{
		/*~~~~~~~*/
		double dY1;
		double dY2;
		/*~~~~~~~*/

		if (m_Point2.x == m_Point1.x)
		{
			m_dSlope = 0;
			m_dYIntercept = m_Point1.y;
		}
		else
		{
			PatternsVector::CalPolyFit(pSignal, m_Point1.x, m_Point2.x, dY1, dY2, m_dYIntercept, m_dSlope);
			m_Point1.y = dY1;
			m_Point2.y = dY2;
		}
	}

	/*
	=======================================================================================================================
	! CLine::CreateLine: TBD
	=======================================================================================================================
	*/
	void CLine::CreateLine(double *pSignal)
	{
		CalcLength();
		GetLineCoefficients(pSignal);
	}

	/*
	=======================================================================================================================
	! CLine::CreateLinePrecomp: Uses precomputed signal summations to calculate polyfit. Can save computation time if
	doing many polyfits.
	=======================================================================================================================
	*/
	void CLine::CreateLinePrecomp(double *pSignalSum, double *pSignalCrossCorr, long lOffset)
	{
		CalcLength();

		/*~~~~~~~~~~~~~~~*/
		// x here is the time variable y is the signal variable The beginning of the
		// display window corresponds to index 0 in the signalSum arrays
		long x1;
		long x2;
		long n;
		double x1Intercept;
		double meanX;
		double meanXsq;
		double meanY;
		double meanCrossXY;
		/*~~~~~~~~~~~~~~~*/

		x1 = GetX1() - lOffset;
		x2 = GetX2() - lOffset;
		n = m_nLength;

		meanX = (n + 1) / 2.0;
		meanXsq = (((2 * n) + 1) * (n + 1)) / 6.0;

		if (x1 == 0)
		{
			meanY = pSignalSum[x2] / n;
			meanCrossXY = pSignalCrossCorr[x2] / n;
		}
		else
		{
			meanY = (pSignalSum[x2] - pSignalSum[x1 - 1]) / n;
			meanCrossXY = (pSignalCrossCorr[x2] - pSignalCrossCorr[x1 - 1] - (x1 * (pSignalSum[x2] - pSignalSum[x1 - 1]))) / n;
		}

		m_dSlope = (meanCrossXY - (meanX * meanY)) / (meanXsq - (meanX * meanX));
		x1Intercept = meanY - (m_dSlope * meanX);	// this is actually the value at x1-1

		m_Point1.y = x1Intercept + m_dSlope;		// x1Intercept + (x1 - (x1 - 1))*slope
		m_Point2.y = x1Intercept + (m_dSlope * (x2 - (x1 - 1)));

		m_dYIntercept = x1Intercept - ((x1 - 1) * m_dSlope);

		return;
	}

	/*
	=======================================================================================================================
	! CLineMatch::operator= : Makes this occurrence equal to the one passed as parameter.
	=======================================================================================================================
	*/
	CLineMatch &CLineMatch::operator=(const CLineMatch &srcObj)
	{
		if (this == &srcObj)
		{
			return *this;
		}

		m_dTopDistance = srcObj.m_dTopDistance;
		m_dBottomDistance = srcObj.m_dBottomDistance;

		if (srcObj.m_pLine != NULL)
		{
			if (m_pLine == NULL)
			{
				m_pLine = new CLine;
			}

			if (m_pLine != NULL)
			{
				*(m_pLine) = *(srcObj.m_pLine);
			}
		}

		return *this;
	}

	/*
	=======================================================================================================================
	! CLineMatch::Empty: Resets the current occurrence to it's initial value and frees any allocated dynamic memory.
	=======================================================================================================================
	*/
	void CLineMatch::Empty(void)
	{
		m_dTopDistance = 0.0;
		m_dBottomDistance = 0.0;
		if (m_pLine != NULL)
		{
			m_pLine->Empty();
		}
	}

	/*
	=======================================================================================================================
	! CBox::operator= : Makes this occurrence equal to the one passed as parameter.
	=======================================================================================================================
	*/
	CBox &CBox::operator=(const CBox &srcObj)
	{
		if (this == &srcObj)
		{
			return *this;
		}

		m_nStartIndex = srcObj.m_nStartIndex;	// Begining of box.
		m_nEndIndex = srcObj.m_nEndIndex;		// End of box.
		m_dWidth = srcObj.m_dWidth;				// Width of box.

		if (srcObj.m_pLineMatch != NULL)
		{
			if (m_pLineMatch == NULL)
			{
				m_pLineMatch = new CLineMatch;
			}

			if (m_pLineMatch != NULL)
			{
				*(m_pLineMatch) = *(srcObj.m_pLineMatch);
			}
		}

		return *this;
	}

	/*
	=======================================================================================================================
	! CBox::Empty: Resets the current occurrence to it's initial value and frees any allocated dynamic memory.
	=======================================================================================================================
	*/
	void CBox::Empty(void)
	{
		m_nStartIndex = 0;	// Begining of box.
		m_nEndIndex = 0;	// End of box.
		m_dWidth = 0.0;		// Width of box.
		m_pLineMatch->Empty();
	}

	/*
	=======================================================================================================================
	! CLinePointArray::SetAtGrow: Sets the specified new element at the given index. If, before the call, the array is
	too small to contain the given position, the array is first extended in order to satisfy the request. \param nIndex
	Position at which the new element must be added. \param newElement Pointer to the new element to add in the array.
	=======================================================================================================================
	*/
	void CLinePointArray::SetAtGrow(int nIndex, CLinePoint *newElement)
	{
		if (nIndex >= m_nSize)
		{
			SetSize(nIndex + 1, -1);	// Need to grow. Bad!!!!
		}

		m_pData[nIndex] = newElement;
	}

	/*
	=======================================================================================================================
	! CDisplayWindow::InitWindow: Initializes various window drawing-related parameters.
	=======================================================================================================================
	*/
	void CDisplayWindow::InitWindow(void)
	{
		m_lX1 = 0;	// 1;
		///
		m_lX2 = 0;
		m_lA2 = 0;

		return;
	}

	//
	// =======================================================================================================================
	//    ! CBoxArray::<constructor>
	// =======================================================================================================================
	//
	CBoxArray::CBoxArray(void)
	{
		m_pData = NULL;
		m_nSize = m_nMaxSize = m_nGrowBy = 0;
	}

	//
	// =======================================================================================================================
	//    ! CBoxArray::<destructor>
	// =======================================================================================================================
	//
	CBoxArray::~CBoxArray(void)
	{
		if (m_pData != NULL)
		{
			delete[] m_pData;
		}
	}

	/*
	=======================================================================================================================
	! CBoxArray::GetSize: Gets the current size of the array. \return The array's current size.
	=======================================================================================================================
	*/
	int CBoxArray::GetSize(void) const
	{
		return m_nSize;
	}

	/*
	=======================================================================================================================
	! CBoxArray::GetCount: As GetSize(), gets the current size of the array. \return The array's current size.
	=======================================================================================================================
	*/
	int CBoxArray::GetCount(void) const
	{
		return m_nSize;
	}

	/*
	=======================================================================================================================
	! CBoxArray::RemoveAll: Removes all occurrences that were included in the array.
	=======================================================================================================================
	*/
	void CBoxArray::RemoveAll(void)
	{
		SetSize(0, -1);
	}

	/*
	=======================================================================================================================
	! CBoxArray::GetAt: Gets the element at the given index. \param nIndex Index of the required element. \return
	Pointer to the required element.
	=======================================================================================================================
	*/
	CBox *CBoxArray::GetAt(int nIndex)
	{
		return m_pData[nIndex];
	}

	/*
	=======================================================================================================================
	! CBoxArray::GetLast: Gets the last element. \return Pointer to the last element.
	=======================================================================================================================
	*/
	CBox *CBoxArray::GetLast(void)
	{
		/*~~~~~~~~~~~~~~~~~~*/
		int iSize = GetSize();
		/*~~~~~~~~~~~~~~~~~~*/

		if (iSize > 0)
		{
			return GetAt(GetSize() - 1);
		}
		else
		{
			return NULL;
		}
	}

	/*
	=======================================================================================================================
	! CBoxArray::GetAt: Gets the element at the given index. \param nIndex Index of the required element. \return
	Constant pointer to the required element as a non-modifiable constant.
	=======================================================================================================================
	*/
	const CBox *CBoxArray::GetAt(int nIndex) const
	{
		return m_pData[nIndex];
	}

	/*
	=======================================================================================================================
	! CBoxArray::SetAt: Sets the specified element in the array at the given index. \param nIndex Index of the required
	element. \param newElement The element to set in the array.
	=======================================================================================================================
	*/
	void CBoxArray::SetAt(int nIndex, CBox *newElement)
	{
		m_pData[nIndex] = newElement;
	}

	/*
	=======================================================================================================================
	! CBoxArray::Add: Adds the specified element to the array. The array's size will increase by one after this call.
	\param newElement The element to set in the array. \return Index where the required element was set.
	=======================================================================================================================
	*/
	int CBoxArray::Add(CBox *newElement)
	{
		/*~~~~~~~~~~~~~~~~~*/
		int nIndex = m_nSize;
		/*~~~~~~~~~~~~~~~~~*/

		SetAtGrow(nIndex, newElement);
		return nIndex;
	}

	/*
	=======================================================================================================================
	! CBoxArray::operator[]: As GetAt, gets the element at the given index. \param nIndex Index of the required
	element.
	=======================================================================================================================
	*/
	const CBox *CBoxArray::operator[](int nIndex) const
	{
		return GetAt(nIndex);
	}

	/*
	=======================================================================================================================
	! CBoxArray::SetSize: Sets the size of the array to one specified. It also sets the size by which the array will
	grow whenever it needs to do so. \param nNewSize The array's requested new size. \param nGrowBy The size by which
	the array should grow whenever it has to. \return The array's previous size.
	=======================================================================================================================
	*/
	int CBoxArray::SetSize(int nNewSize, int nGrowBy)
	{
		/*~~~~~~~~~~~~~~~~~~~*/
		int nOldSize = m_nSize;
		/*~~~~~~~~~~~~~~~~~~~*/

		if (nGrowBy != -1)
		{
			m_nGrowBy = nGrowBy;				// set new size
		}

		if (nNewSize == 0)						// Array now contains 0 elements
		{
			if (m_pData != NULL)
			{
				delete[] m_pData;
				m_pData = NULL;
			}

			m_nSize = m_nMaxSize = 0;
		}
		else if (m_pData == NULL)
		{
			m_pData = (CBox **) new BYTE[(size_t) nNewSize * sizeof(CBox *)];
			memset((void *) m_pData, 0, (size_t) nNewSize * sizeof(CBox *));

			m_nSize = m_nMaxSize = nNewSize;
		}
		else if (nNewSize <= m_nMaxSize)
		{
			if (nNewSize > m_nSize)
			{
				memset((void *) (m_pData + m_nSize), 0, (size_t) (nNewSize - m_nSize) * sizeof(CBox *));
			}

			m_nSize = nNewSize;
		}
		else									// nNewSize > m_nMaxSize
		{
			nGrowBy = m_nGrowBy;

			if (nGrowBy == 0)
			{
				nGrowBy = m_nSize / 8;
				nGrowBy = (nGrowBy < 4) ? 4 : ((nGrowBy > 1024) ? 1024 : nGrowBy);
			}

			/*~~~~~~~~~~~~*/
			int nNewMax = 0;
			/*~~~~~~~~~~~~*/

			if (nNewSize < m_nMaxSize + nGrowBy)
			{
				nNewMax = m_nMaxSize + nGrowBy; // granularity
			}
			else
			{
				nNewMax = nNewSize;		// no slush
			}

			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			CBox **pNewData = (CBox **) new BYTE[(size_t) nNewMax * sizeof(CBox *)];
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			memcpy(pNewData, m_pData, (size_t) m_nSize * sizeof(CBox *));

			memset((void *) (pNewData + m_nSize), 0, (size_t) (nNewSize - m_nSize) * sizeof(CBox *));

			delete[] m_pData;
			m_pData = pNewData;
			m_nSize = nNewSize;
			m_nMaxSize = nNewMax;
		}

		return nOldSize;
	}

	/*
	=======================================================================================================================
	! CBoxArray::SetAtGrow: Sets the specified new element at the given index. If, before the call, the array is too
	small to contain the given position, the array is first extended in order to satisfy the request. \param nIndex
	Position at which the new element must be added. \param newElement Pointer to the new element to add in the array.
	=======================================================================================================================
	*/
	void CBoxArray::SetAtGrow(int nIndex, CBox *newElement)
	{
		if (nIndex >= m_nSize)
		{
			SetSize(nIndex + 1, -1);	// Need to grow. Bad!!!!
		}

		m_pData[nIndex] = newElement;
	}

	/*
	=======================================================================================================================
	! CBoxArray::RemoveAt: Removes a given number of consecutive elements from the array, starting from the specified
	index. \param nIndex Position of the first element to be removed. \param nCount The number of consecutive elements
	to remove.
	=======================================================================================================================
	*/
	void CBoxArray::RemoveAt(int nIndex, int nCount)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int nMoveCount = m_nSize - (nIndex + nCount);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (nMoveCount > 0)
		{
			memmove(m_pData + nIndex, m_pData + (nIndex + nCount), (size_t) nMoveCount * sizeof(CBox *));
		}

		m_nSize -= nCount;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CBaseLineDetection::CBaseLineDetection(void)
	{
		m_pDisplayWindow = new CDisplayWindow;
		m_pBasConfig = new CBaseLineConfig;
		m_pExtrema = NULL;
		m_pExtremaMidPts = NULL;
		m_pSignal = NULL;
		m_pSignalSum = NULL;
		m_pSignalCrossCorr = NULL;
		m_nOffset = 0;
		m_iRepStart = 0;
		m_iRepEnd = 0;
		m_bJumpMode = false;

		m_nNextWindowStart = 0;
		m_lLastValidRepairIndex = 0;	// so do not detect baselines in unstable repair signal

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	CBaseLineDetection::~CBaseLineDetection(void)
	{
		m_pBaseLines.clear();
		if (m_pDisplayWindow != NULL)
		{
			delete m_pDisplayWindow;
		}

		if (m_pExtrema != NULL)
		{
			delete m_pExtrema;
		}

		if (m_pExtremaMidPts != NULL)
		{
			delete m_pExtremaMidPts;
		}

		if (m_pBasConfig != NULL)
		{
			delete m_pBasConfig;
		}

		if (m_pSignalSum != NULL)
		{
			delete[] m_pSignalSum;
			m_pSignalSum = NULL;
		}

		if (m_pSignalCrossCorr != NULL)
		{
			delete[] m_pSignalCrossCorr;
			m_pSignalCrossCorr = NULL;
		}

		m_pRepairIntervals.clear();
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::DetectBaseLine: Detects baseline candidates in current window \param pSignal - CFHRSignal
	from higher level \param pBaseLinesFinal - CBaseLines - valid baseline candidates will be returned here \param
	pBaseLinesPending - CBaseLines - pending baselines from previous windows passed here and also pending baselines
	from current window will be returned If m_pRepairIntervals exist, will subdivide search to look between repair
	segments in order to a) avoid considering repaired areas for baseline detection and b) speed up detection When
	there is repair, the overall window will be sub-windowed into blocks of clean signal There will be no commit buffer
	(i.e. all baselines will become final) for all blocks except for the last one If there are no repair Intervals (or
	m_bSkipRepairSegments is not set) then will operate by considering the entire window at once (as before)
	=======================================================================================================================
	*/
	void CBaseLineDetection::DetectBaseLine(CFHRSignal *pSignal, fhrPartSet *pBaseLinesFinal, fhrPartSet *pBaseLinesPending)
	{
		m_pSignal = pSignal;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lNextX1 = max(0, m_nNextWindowStart - m_nOffset);
		fhrPartSet pPendingOut;
		long maxX2 = m_pSignal->GetLowPassCount() - m_pSignal->GetNumExtrapolatedSignal();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// If using precomputed summations for polyfit, calculate these sums
		if (m_pBasConfig->m_bUsePrecomputedPolyFitSums)
		{
			CalcPolyFitPrep();
		}

		for (long i = 0; i <= m_pRepairIntervals.size(); i++)
		{
			if (i > 0)
			{				// only consider pending baselines in window that goes up to first repair (or to end of window if no repair)
				pBaseLinesPending->clear();
			}

			InitWindow();	// Set initial window boundaries ignoring repair
			m_pDisplayWindow->m_lX1 = lNextX1;
			if (i < m_pRepairIntervals.size()) // only consider upto next repair segment
			{
				fhrPart *p = m_pRepairIntervals.getAt(i);
				m_pDisplayWindow->m_lX2 = min(maxX2, p->getX1()) - 1;
				lNextX1 = p->getX2() + 1;	// ensuing block starts from end of repair segment
				if (lNextX1 <= m_pDisplayWindow->m_lA2)
				{
					m_nNextWindowStart = lNextX1 + m_nOffset;	// update global start point for next high level window if past commit buffer
				}

				// m_pDisplayWindow->m_lA2 = m_pDisplayWindow->m_lX2;
			}

			// Core - will add to m_pBaseLines
			if (m_pDisplayWindow->GetLength() > 0)				// deals w/ case where repair is right at beginning of window
			{
				GetCandidateBaselines(pBaseLinesPending);
				if (m_pBaseLines.size() > 0)
				{	// move baselines found in current block to final buffer and populate pending if exist (will only exist if last block in window
					DetectAcceptedParts(m_pDisplayWindow->m_lA2, &m_pBaseLines, pBaseLinesFinal, &pPendingOut);
					m_pBaseLines.clear();
				}
			}
		}

		// Clear pending baselines if exist - will repopulate with pending from current
		// window
		pBaseLinesPending->clear();
		pPendingOut.setClearMemory(false);
		pBaseLinesPending->add(&pPendingOut);

		UpdateWindow(pBaseLinesFinal);

		m_pBaseLines.clear();
	}

	/*
	=======================================================================================================================
	CBaselineDetection::InitWindow - initialize the baseline window struct based on last window and higher level signal
	window This is now based on higher level window parameters Want to process the minimum amount required and do it
	all in one go Use a 'm_nextWindowStart' to determine start of window - this should have been set in the last run of
	baselineDetection (or 0 if first run). This determines the point in the last window for which all previous
	baselines are deemed valid and do not need to be reprocessed. The end of the window should just be the end of
	available signal. This can be m_nPtsCount but will really be the amount of lowPass or minVar signal since these
	will be less.
	=======================================================================================================================
	*/
	void CBaseLineDetection::InitWindow(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dSmpFreq = m_pSignal->GetConfig()->GetDefSmpFreq();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_pDisplayWindow->InitWindow();

		// x1 and x2 delimit the signal to be considered a2 delimits the acceptance zone
		// where baselines can be commited - end of window may not be stable
		m_pDisplayWindow->m_lX1 = max(0, m_nNextWindowStart - m_pSignal->m_OutputAbsStart);

		m_pDisplayWindow->m_lX2 = m_pSignal->GetLowPassCount() - m_pSignal->GetNumExtrapolatedSignal() - 1;

		m_pDisplayWindow->m_lA2 = m_pSignal->m_nPtsCount - m_pSignal->GetNumExtrapolatedSignal() - GetCommitBuffer();

		// Repair is now explicitly taken care of w/ repair intervals so do not need to
		// worry if (m_pDisplayWindow->m_lA2 > m_lLastValidRepairIndex) // do not look
		// into unstable repair m_pDisplayWindow->m_lA2 = m_lLastValidRepairIndex;
		// ;
		// Currently iCommitBuffer takes into account the filter delays and is measured
		// from the end of the window Idealyy the CommitBuffer would be made up of 3
		// parts: the LP delay, the minVar delay (taking into account the varPass delay)
		// and then a true commit buffer that dictates the amount of time at the end of
		// the window given that all underlying signals are valid - for now subtract
		// iCommitBuffer from absolute end of window
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBaseLineDetection::SetRepairIntervals(fhrPartSet *pRepairIntervals)
	{
		if ((pRepairIntervals->size() == 0) || (!(m_pBasConfig->m_bSkipRepairSegments)))
		{
			return;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lNumLongRepIntervals = 0;
		long lFirstIndex = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_pRepairIntervals.clear();
		for (long i = 0; i < pRepairIntervals->size(); i++)
		{
			fhrPart *p = pRepairIntervals->getAt(i);
			if (p->getX2() > m_nNextWindowStart)
			{
				if (p->length() >= m_pBasConfig->GetMinRepSegmentLength())
				{
					m_pRepairIntervals.addcopy(p);
				}
			}
		}

		m_pRepairIntervals.toRelativeTime(m_nOffset);

	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetCandidateBaselines: Gets all candidate baselines for longest box. \param
	pBaseLinesPending: pending baselines from last pass (if any) This will populate m_pBaseLines
	=======================================================================================================================
	*/
	void CBaseLineDetection::GetCandidateBaselines(fhrPartSet *pBaseLinesPending)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long nStart = m_pDisplayWindow->m_lX1;
		long nEnd = m_pDisplayWindow->m_lX2;
		long *pExtrema = NULL;
		int nCount = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pExtrema = new long[nEnd - nStart];

		PeakDetector(nStart, nEnd, pExtrema, nCount);

		if (m_pExtrema)
		{
			delete[] m_pExtrema;
		}

		m_pExtrema = new long[nCount];
		m_nExtremaCount = nCount;

		for (int i = 0; i < nCount; i++)
		{
			m_pExtrema[i] = (long) pExtrema[i];
		}

		GetExtremaMidPts();

		delete[] pExtrema;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// here convert pending baselines into boxes
		CBoxes *pBoxVec = ConvertBaseLinesToBoxVec(pBaseLinesPending);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		CalcExtremaBoxes(pBoxVec);

		// convert boxes to baselines (m_pBaseLines)
		ConvertBoxVecToBaseLines(pBoxVec);

		if (pBoxVec)
		{
			delete pBoxVec;
			pBoxVec = NULL;
		}

		return;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::PeakDetector: Gets all extremas of FHR signal. \param nStart Start of the interval in the FHR
	signal \param nEnd End of the interval in the FHR signal \param pLocalMaxMin Pointer to an array holding all
	extermas. \param nCount Reference to an integer that will be initialized to the number of extremas \return false if
	the interval incorrect, otherwise true
	=======================================================================================================================
	*/
	bool CBaseLineDetection::PeakDetector(long nStart, long nEnd, long *pLocalMaxMin, int &nCount)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dMinDiffNotEqual = 0.0000000001;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (nEnd - nStart < 3)
		{
			return false;
		}
		else
		{
			if (pLocalMaxMin == NULL)
			{
				pLocalMaxMin = new long[nEnd - nStart];
			}

			/*~~~~~~~~~~~*/
			int nIndex = 0;
			/*~~~~~~~~~~~*/

			for (long i = nStart + 1; i < nEnd; i++)
			{
				if (i + 1 > m_pSignal->GetLowPassCount())
				{
					// AfxMessageBox("Error: size is larger than the size of LowPass");
					return false;
				}

				// This is a direct translation from MATLAB - can be problems in flat repair
				// because not exactly equal to each other in flat region ;
				// if (((GetLowPassValue(i) > GetLowPassValue(i - 1)) && (GetLowPassValue(i) >=
				// GetLowPassValue(i + 1))) || ((GetLowPassValue(i) < GetLowPassValue(i - 1)) &&
				// (GetLowPassValue(i) <= GetLowPassValue(i + 1)))) pLocalMaxMin[nIndex++] = i;
				// ;
				// This is the original Evonium approach - caused every sample to be extremum in
				// flat repair regions which makes baseline detection go painfully slow. Need to
				// redefine =lity to consider very small differences as being equal ;
				// if (GetLowPassValue(i) != GetLowPassValue(i - 1) || GetLowPassValue(i) !=
				// GetLowPassValue(i + 1))
				if ((abs(GetLowPassValue(i) - GetLowPassValue(i - 1)) > dMinDiffNotEqual) || (abs(GetLowPassValue(i) - GetLowPassValue(i + 1)) > dMinDiffNotEqual))
				{
					if (GetLowPassValue(i) >= max(GetLowPassValue(i - 1), GetLowPassValue(i + 1)) || GetLowPassValue(i) <= min(GetLowPassValue(i - 1), GetLowPassValue(i + 1)))
					{
						pLocalMaxMin[nIndex++] = i;
					}
				}
			}

			pLocalMaxMin[nIndex] = nEnd;	// add last Point
			nCount = nIndex + 1;
			return true;
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetExtremaMidPts: Get midpts of extrema
	=======================================================================================================================
	*/
	void CBaseLineDetection::GetExtremaMidPts(void)
	{
		if (m_pExtremaMidPts)
		{
			delete[] m_pExtremaMidPts;
		}

		m_nExtremaMidPtsCount = m_nExtremaCount + 2;	// because include start and end index
		m_pExtremaMidPts = new long[m_nExtremaMidPtsCount];

		m_pExtremaMidPts[0] = m_pDisplayWindow->m_lX1;

		for (int i = 1; i < m_nExtremaCount; i++)
		{	// m_pExtremaMidPts[i] = floor((m_pExtrema[i] + m_pExtrema[i+1])
			///2);
			///
			m_pExtremaMidPts[i] = (m_pExtrema[i] + m_pExtrema[i - 1]) / 2;
		}

		m_pExtremaMidPts[m_nExtremaCount] = m_pDisplayWindow->m_lX2;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::CalcExtremaBoxes: Calculates longest boxes \param pBoxVec Contains pending boxes from
	previous window. On output, pointer to an array of boxes in current window (this may include pending)
	=======================================================================================================================
	*/
	void CBaseLineDetection::CalcExtremaBoxes(CBoxes *pBoxVec)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lPos = m_pDisplayWindow->m_lX1;	// starting index of search
		int iNextExtrema = 0;					// index in array of extrema of extremum after last valid box end index
		long lLastValidEndIndex = lPos;			// last valid box end index
		long lLastPosConsider = min(m_pDisplayWindow->m_lX2 - GetMinBasLen(), m_pDisplayWindow->m_lA2);
		// long lLastPosConsider = m_pDisplayWindow->m_lX2 - GetMinBasLen();
		// // look even if definitely going to be pending
		int iStartExtremaIndex = 0;				// index in array of extrema of extremum after last start index
		int iNextExtremaOut = 0;				// intermed iNextExtrema
		long lIndexLastFailed = 0;				// index at which box search failed (used for binary search)
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		if (pBoxVec->GetSize() == 0)			// no pending boxes
		{
			iNextExtrema = UpdateNextExtremaBeyondIndex(iNextExtrema, lPos);
			iStartExtremaIndex = iNextExtrema;
		}
		else	// pending boxes - update appropriate variables so can skip first part of window
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			CBox *pLastBox = pBoxVec->GetLast();
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			lLastValidEndIndex = pLastBox->m_nEndIndex;
			iStartExtremaIndex = UpdateNextExtremaBeyondIndex(iNextExtrema, pLastBox->m_nStartIndex);
			iNextExtrema = UpdateNextExtremaBeyondIndex(iStartExtremaIndex, pLastBox->m_nEndIndex);
		}

		// loop over start indexes
		while (lPos < lLastPosConsider)
		{
			// for when in area where no valid boxes - iNextExtrema will lag
			iNextExtrema = max(iNextExtrema, iStartExtremaIndex);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// Get longest box from current start index
			CBox *pCurrBox = GetBoxWithWidthUpdate(lPos, iNextExtrema, iStartExtremaIndex, lLastValidEndIndex, iNextExtremaOut);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			// If box is valid and extends past last valid box
			if (IsValidBox(pCurrBox, lLastValidEndIndex))
			{
				if (m_pBasConfig->m_bDoBinaryBackForAccepted)
				{
					BinarySearchBackwards(pCurrBox, pBoxVec);						// fine tune
				}

				if (pCurrBox->m_nEndIndex < m_pDisplayWindow->m_lX2)
				{
					if (m_pBasConfig->m_bDoBinaryForwardForAccepted)
					{
						lIndexLastFailed = m_pExtremaMidPts[iNextExtremaOut];
						BinarySearch(pCurrBox, lIndexLastFailed, iNextExtremaOut);	// fine tune
					}
				}

				if (AddBoxCheckOverlap(pBoxVec, pCurrBox))	// add current box to existing boxes. May involve merge.
				{
					iNextExtrema = iNextExtremaOut;
					lLastValidEndIndex = (pBoxVec->GetLast())->m_nEndIndex;
					if (lLastValidEndIndex == m_pDisplayWindow->m_lX2)
					{
						return; // cannot get longer box
					}
				}
				else			// current box could not be added/merged to existing set of boxes
				{
					delete pCurrBox;
					pCurrBox = NULL;
				}
			}
			else				// box not valid or does not extend far enough to be useful
			{
				delete pCurrBox;
				pCurrBox = NULL;
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long lPrevBoxEnd = max(lLastValidEndIndex, lPos);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			UpdateBoxLoc(iStartExtremaIndex, lPos, lPrevBoxEnd);	// update start point of next box search
		}

		return;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetBoxWithWidthUpdate \param lPos - start sample index of box \param iNextExtrema - index in
	array of extrema midpts of next extremum past lLastValidEndIndex \param iStartExtremaIndex - index in array of
	extrema midpts of next extremum past lPos \param lLastValidEndIndex - sample index of end of last valid box \param
	iNextExtremaOut - will be updated version of iNextExtrema once current box is extended Returns CBox* - pointer to
	longest box from given start point
	=======================================================================================================================
	*/
	CBox *CBaseLineDetection::GetBoxWithWidthUpdate(long lPos, int iNextExtrema, int iStartExtremaIndex, long lLastValidEndIndex, int &iNextExtremaOut)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lBoxEnd = max(lPos, lLastValidEndIndex);		// start search w/ endpoint at last Valid
		double dBoxWidth = GetWidthEstimate(lPos, lBoxEnd); // update width criterion
		CBox *pBox = new CBox;
		CLineMatch *pLineMatch = new CLineMatch;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pBox->m_nStartIndex = lPos;
		pBox->m_nEndIndex = lBoxEnd;
		pBox->m_dWidth = dBoxWidth;

		// Stretch box forward in time
		LongestBoxRight(pBox, iNextExtrema, iStartExtremaIndex, iNextExtremaOut, pLineMatch);

		if (pLineMatch)
		{
			delete pLineMatch;
			pLineMatch = NULL;
		}

		return pBox;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::IsValidBox: Checks to see whether box found is to be kept \param pBox - box to check for
	validity \param lPrevEndIndex - sample index of last valid box If current box is not of sufficient length (20 sec),
	does not surpass the specified end index, or does not meet width and slope criteria, return false. Else, return
	true.
	=======================================================================================================================
	*/
	bool CBaseLineDetection::IsValidBox(CBox *pBox, long lPrevEndIndex)
	{
		/*~~~~~~~~~~~~~*/
		bool bRC = false;
		/*~~~~~~~~~~~~~*/

		if (pBox->GetLength() < GetMinBasLen())
		{
			return false;
		}

		if (lPrevEndIndex >= pBox->m_nEndIndex)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		CLineMatch *pLineMatch = new CLineMatch;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// double dY2 = GetLowPassValue(pBox->m_nEndIndex);
		// // COLM - we don't need this
		bRC = CreateLineMatch(pBox, pBox->m_nEndIndex, pLineMatch);

		if (pLineMatch)
		{
			delete pLineMatch;
			pLineMatch = NULL;
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::UpdateBoxLoc Updates starting point for next search (lPos), based on config parameters,
	extrema, and end index of last previous valid box \param iStartExtremaIndex - index in extrema array of extremum
	midpt after current position \param lPos - current search start index \param lPrevBoxEnd - end of previous valid
	box
	=======================================================================================================================
	*/
	void CBaseLineDetection::UpdateBoxLoc(int &iStartExtremaIndex, long &lPos, long lPrevBoxEnd)
	{
		if (lPos < lPrevBoxEnd)					// already have box going past current start index
		{
			if (m_pBasConfig->m_bSkipToLastValidEnd)
			{
				lPos = lPrevBoxEnd + 1;			// most aggressive
				iStartExtremaIndex = UpdateNextExtremaBeyondIndex(iStartExtremaIndex, lPos);
			}
			else if (m_pBasConfig->m_bAdvOverExtremaDuringBas)
			{
				if (lPrevBoxEnd < m_pExtremaMidPts[iStartExtremaIndex])
				{
					if (m_pBasConfig->m_bAdvOverExtremaDuringNonBas)
					{
						lPos = m_pExtremaMidPts[iStartExtremaIndex];
					}
					else
					{
						lPos = lPrevBoxEnd + 1; // note here that lPrevBoxEnd is before next extrema
					}
				}
				else
				{
					lPos = m_pExtremaMidPts[iStartExtremaIndex];
				}
			}
			else
			{
				lPos += m_pBasConfig->m_iSlowAdvStep;
			}
		}
		else	// in are where have no candidate baseline
		{
			if (m_pBasConfig->m_bAdvOverExtremaDuringNonBas)
			{
				lPos = m_pExtremaMidPts[iStartExtremaIndex];
			}
			else
			{
				lPos += m_pBasConfig->m_iSlowAdvStep;
			}
		}

		// Can check if have to advance nextExtrema counter by 1
		if (lPos >= m_pExtremaMidPts[iStartExtremaIndex])
		{
			iStartExtremaIndex++;
		}

		return;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetWidthEstimate: Determines the allowable box width based on ambient variability \param
	lBoxStart Start Index of box \param lBoxEnd End Index of box
	=======================================================================================================================
	*/
	double CBaseLineDetection::GetWidthEstimate(long lBoxStart, long lBoxEnd)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// long lBoxMid = floor((lBoxStart + lBoxEnd) / 2);
		long lBoxMid = (lBoxStart + lBoxEnd) / 2;
		long lLocalMinStart = (long) (m_pSignal->m_pdMinVarSignal[lBoxStart]);
		long lLocalMinMid = (long) (m_pSignal->m_pdMinVarSignal[lBoxMid]);
		long lLocalMinEnd = (long) (m_pSignal->m_pdMinVarSignal[lBoxEnd]);
		double dWidth = 0;
		double dMeanVar = (GetVarValue(lLocalMinStart) + GetVarValue(lLocalMinMid) + GetVarValue(lLocalMinEnd)) / 3;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (dMeanVar < m_pBasConfig->m_dSmallVarThresh)
		{
			dWidth = (m_pBasConfig->m_pPolyVecSmall[0] * dMeanVar) + m_pBasConfig->m_pPolyVecSmall[1];
		}
		else
		{
			dWidth = (m_pBasConfig->m_pPolyVecMid[0] * dMeanVar) + m_pBasConfig->m_pPolyVecMid[1];
		}

		dWidth = max(dWidth, m_pBasConfig->m_dMinBoxWidth);
		dWidth = min(dWidth, m_pBasConfig->m_dMaxBoxWidth);

		dWidth = dWidth * m_pBasConfig->m_dWidthMultFactor; // to stay consistent w/ MATLAB code

		return dWidth;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	int CBaseLineDetection::UpdateNextExtremaBeyondIndex(int iExtremaIndexStart, long lSampleIndex)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iLen = m_nExtremaMidPtsCount;
		int i = iExtremaIndexStart;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while ((i < iLen) && (m_pExtremaMidPts[i] <= lSampleIndex))
			i++;

		return i;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::LongestBoxRight: Extend a box as far as possible in time subject to dynamic box width
	constraints \param pBox Box to be extended \param iNextExtrema Index in extrema array of next extrema midpt after
	current box endpoint \param iStartExtremaIndex Index in extrema array of next extrema midpt after current box start
	\param &iNextExtremaOut Updated index in extrema array of next extrema midpt after extended box endpoint \param
	*pLineMatch Pointer to lineMatch object used for temp storage of intermed results
	=======================================================================================================================
	*/
	void CBaseLineDetection::LongestBoxRight(CBox *pBox, int iNextExtrema, int iStartExtremaIndex, int &iNextExtremaOut, CLineMatch *pLineMatch)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// if (pLineMatch == NULL) pLineMatch = new CLineMatch;
		long lOrigEndIndex = pBox->m_nEndIndex;
		long lBoxIndexLimit = m_pDisplayWindow->m_lX2;
		bool bDoFlatBoxSimple = ((m_pBasConfig->m_bAlwaysFlatBoxSimple) || pBox->GetLength() < GetMinBasLen());
		long lFlatBoxEndIndex = lOrigEndIndex;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		iNextExtremaOut = iNextExtrema;

		if (bDoFlatBoxSimple)
		{
			lFlatBoxEndIndex = FlatBoxSimple(pBox, lBoxIndexLimit, iNextExtremaOut, iStartExtremaIndex);
			if (lFlatBoxEndIndex <= lOrigEndIndex)
			{
				return;
			}

			pBox->m_dWidth = GetWidthEstimate(pBox->m_nStartIndex, lFlatBoxEndIndex);
			if (CreateLineMatch(pBox, lFlatBoxEndIndex, pLineMatch))
			{		// check sloped box based on polyfit
				BoxUpdate(pBox, pLineMatch);
			}
			else	// result of flatBoxSimple does not meet criteria if box sloped
			{
				BoxUpdate(pBox, pLineMatch);

				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				// update endpoint of box (note box is currently invalid)
				long lMaxRewind = max(pBox->m_nStartIndex + GetMinBasLen(), lOrigEndIndex);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (!(RewindBoxOverExtremaUntilFits(pBox, iNextExtremaOut, lMaxRewind, pLineMatch)))
				{
					pBox->m_nEndIndex = lOrigEndIndex;	// just return original box
				}

				return;
			}
		}

		// Now advance over extrema allowing sloped box
		ExtendRight(pBox, lBoxIndexLimit, iNextExtremaOut);
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::ExtendRight: Extends box across successive extrema midpts until fails to meet dynamic width
	and slope criteria \param pBox Box to be extended \param lBoxIndexLimit Sample index of maximum extension \param
	&iNextExtrema Index in array of extrema midpts of next extrema midpt after current end of box
	=======================================================================================================================
	*/
	void CBaseLineDetection::ExtendRight(CBox *pBox, long lBoxIndexLimit, int &iNextExtrema)
	{
		/*~~~~~~~~~~~~~~~~*/
		long lIndexLastFail;
		/*~~~~~~~~~~~~~~~~*/

		if (pBox->m_nEndIndex == lBoxIndexLimit)
		{
			return;
		}

		lIndexLastFail = VisitExtremas(pBox, lBoxIndexLimit, iNextExtrema);

		if (m_pBasConfig->m_bDoExtendRightBinarySearch)
		{
			BinarySearch(pBox, lIndexLastFail, iNextExtrema);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBaseLineDetection::BinarySearch(CBox *pBox, long lIndexLastFailed, int iExtremaMidFail)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lLastExtrema;
		CLineMatch *pLineMatch = new CLineMatch;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if ((iExtremaMidFail > 0) && (iExtremaMidFail < (m_nExtremaMidPtsCount - 1)))
		{
			lLastExtrema = m_pExtrema[iExtremaMidFail - 1];
			if (CreateLineMatch(pBox, lLastExtrema, pLineMatch))
			{
				BoxUpdate(pBox, pLineMatch);
			}
			else
			{
				lIndexLastFailed = lLastExtrema;
			}
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// long lCurrIndex = floor((lIndexLastFailed + pBox->m_nEndIndex) / 2);
		long lCurrIndex = (lIndexLastFailed + pBox->m_nEndIndex) / 2;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while (lCurrIndex > pBox->m_nEndIndex)
		{
			if (CreateLineMatch(pBox, lCurrIndex, pLineMatch))
			{
				BoxUpdate(pBox, pLineMatch);
			}
			else
			{
				lIndexLastFailed = lCurrIndex;
			}

			// lCurrIndex = floor((lIndexLastFailed + pBox->m_nEndIndex) / 2);
			lCurrIndex = (lIndexLastFailed + pBox->m_nEndIndex) / 2;
		}

		if (pLineMatch)
		{
			delete pLineMatch;
			pLineMatch = NULL;
		}

		return;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBaseLineDetection::BinarySearchBackwards(CBox *pBox, CBoxes *pBoxVec)
	{
		/*~~~~~~~~~~*/
		long lSearch1;
		/*~~~~~~~~~~*/

		if (pBoxVec->GetSize() == 0)
		{
			lSearch1 = m_pDisplayWindow->m_lX1;
		}
		else
		{
			lSearch1 = (pBoxVec->GetLast())->m_nEndIndex;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// long lCurrIndex = floor((lSearch1 + pBox->m_nStartIndex) / 2);
		// long lCurrIndex = (lSearch1 + pBox->m_nStartIndex) / 2;
		long lLastFailed = lSearch1;
		long lCurrIndex = lLastFailed;
		CLineMatch *pLineMatch = new CLineMatch;
		CBox *pBoxTemp = new CBox;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		*pBoxTemp = *pBox;

		while (lCurrIndex < pBox->m_nStartIndex)
		{
			pBoxTemp->m_nStartIndex = lCurrIndex;
			if (CreateLineMatch(pBoxTemp, pBoxTemp->m_nEndIndex, pLineMatch))
			{
				pBox->m_nStartIndex = lCurrIndex;
				BoxUpdate(pBox, pLineMatch);
			}
			else
			{
				lLastFailed = lCurrIndex;
			}

			// lCurrIndex = ceil((lLastFailed + pBox->m_nStartIndex) / 2);
			lCurrIndex = (lLastFailed + pBox->m_nStartIndex + 1) / 2;	// should figure out ceil issue
		}

		if (pBoxTemp)
		{
			delete pBoxTemp;
			pBoxTemp = NULL;
		}

		if (pLineMatch)
		{
			delete pLineMatch;
			pLineMatch = NULL;
		}

		return;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::VisitExtremas: Extends box across successive extrema midpts, checking whether meeting width
	and slope requirements
	=======================================================================================================================
	*/
	long CBaseLineDetection::VisitExtremas(CBox *pBox, long lBoxIndexLimit, int &iNextExtrema)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bContinueSearch = true;
		bool bIsValid = false;
		int iNextExtremaTemp = iNextExtrema;
		int iMaxFail = m_pBasConfig->m_iNumSuccFailAllow;
		int iSuccFail = 0;
		long lIndexLastFail = 0;
		CBox *pTestBox = new CBox;
		// CBox *pBoxFail = new CBox;
		CLineMatch *pLineMatch = new CLineMatch;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pBox->m_dWidth = GetWidthEstimate(pBox->m_nStartIndex, pBox->m_nEndIndex);
		bContinueSearch = VisitNextExtrema(pBox, iNextExtremaTemp, bIsValid, lBoxIndexLimit, pLineMatch);

		lIndexLastFail = pLineMatch->GetX2();
		if (bIsValid)
		{
			BoxUpdate(pBox, pLineMatch);
			*pTestBox = *pBox;
		}
		else
		{
			// *pBoxFail = *pBox;
			// BoxUpdate(pBoxFail, pLineMatch);
			if (m_pBasConfig->m_bAllowSuccFail)
			{
				iSuccFail = 1;

				// *pTestBox = *pBoxFail;
			}
			else
			{
				iSuccFail = iMaxFail;	// fails at first advance - give up
			}
		}

		while ((bContinueSearch) && (iSuccFail < iMaxFail))
		{
			if (pTestBox->m_nEndIndex >= lBoxIndexLimit)
			{
				break;
			}

			iNextExtremaTemp++;
			pTestBox->m_dWidth = GetWidthEstimate(pTestBox->m_nStartIndex, pTestBox->m_nEndIndex);
			bContinueSearch = VisitNextExtrema(pBox, iNextExtremaTemp, bIsValid, lBoxIndexLimit, pLineMatch);
			BoxUpdate(pTestBox, pLineMatch);

			if (bIsValid)
			{
				*pBox = *pTestBox;
				iSuccFail = 0;

				// LXP Don't let the engine create HUGE baseline if the signal is quite flat and if we are in batch mode processing hours of tracing at once.
				// Not doing that would cause the engine to try to create a baseline of hours and then the performances would degrade since complexity is
				// not linear
				if (bContinueSearch && ((pBox->m_nEndIndex - pBox->m_nStartIndex) > 4 * 120))
				{
					bContinueSearch = false;
				}

			}
			else
			{
				if (iSuccFail == 0)
				{
					iNextExtrema = iNextExtremaTemp;
					lIndexLastFail = pLineMatch->GetX2();
				}

				iSuccFail++;
			}
		}

		if (pLineMatch)
		{
			delete pLineMatch;
			pLineMatch = NULL;
		}

		if (pTestBox)
		{
			delete pTestBox;
			pTestBox = NULL;
		}

		// delete pBoxFail;
		// pBoxFail = NULL;
		return lIndexLastFail;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::VisitNextExtrema: Checks to see if box is still valid if extended to next extrema midpt
	=======================================================================================================================
	*/
	bool CBaseLineDetection::VisitNextExtrema(CBox *pBox, int iNextExtrema, bool &bIsValid, long lBoxIndexLimit, CLineMatch *pLineMatch)
	{
		/*~~~~~~~~~~~~~*/
		bool bRC = false;
		long lCurrIndex;
		/*~~~~~~~~~~~~~*/

		if (iNextExtrema >= m_nExtremaMidPtsCount)
		{
			lCurrIndex = lBoxIndexLimit;
		}
		else if (m_pExtremaMidPts[iNextExtrema] > lBoxIndexLimit)
		{
			lCurrIndex = lBoxIndexLimit;
		}
		else
		{
			lCurrIndex = m_pExtremaMidPts[iNextExtrema];
			bRC = true;
		}

		bIsValid = CreateLineMatch(pBox, lCurrIndex, pLineMatch);
		return bRC;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::BoxUpdate: Updates a box. \param pBox On input, a box. On output, the updated box. \param
	pLineMatch TBD
	=======================================================================================================================
	*/
	bool CBaseLineDetection::BoxUpdate(CBox *pBox, CLineMatch *pLineMatch)
	{
		if (pBox == NULL || pLineMatch == NULL)
		{
			return false;
		}

		pBox->m_nEndIndex = pLineMatch->m_pLine->m_Point2.x;
		*(pBox->m_pLineMatch) = *(pLineMatch);
		return true;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::CreateLineMatch: Creates a CLineMatch structure based on the specified box and contained
	signal. \param pBox The specified box. \param nFlatBoxEndPointX Ending index of flat box. \param pLineMatch Pointer
	to the newly created CLineMatch. \return true if everything went nicely. Otherwise, false.
	=======================================================================================================================
	*/
	bool CBaseLineDetection::CreateLineMatch(CBox *pBox, long nFlatBoxEndPointX, CLineMatch *pLineMatch)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRet = false;
		CLinePoint Point1(pBox->m_nStartIndex, GetLowPassValue(pBox->m_nStartIndex));
		CLinePoint Point2(nFlatBoxEndPointX, GetLowPassValue(pBox->m_nEndIndex));
		CLine *pLine = new CLine(Point1, Point2);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// CLine* pLine2 = new CLine(Point1, Point2);
		// // just for debug new polyfit
		if (pLine == NULL)
		{
			return false;
		}

		if (m_pBasConfig->m_bUsePrecomputedPolyFitSums)
		{
			pLine->CreateLinePrecomp(m_pSignalSum, m_pSignalCrossCorr, m_pDisplayWindow->m_lX1);
		}
		else
		{
			pLine->CreateLine(m_pSignal->m_pdLowPassSignal);
		}

		if (MatchLineToSignal(pLine, pLineMatch))
		{
			if ((pLineMatch->m_dTopDistance + pLineMatch->m_dBottomDistance) > pBox->m_dWidth)
			{
				bRet = false;
			}
			else
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				double dMaxSlope = (m_pBasConfig->m_dCorridorSlopeOffset / pLine->m_nLength) + (m_pBasConfig->m_dSlopeMultFactor * m_pBasConfig->m_dCorridorSlope);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				bRet = (abs(pLine->m_dSlope) < dMaxSlope);
			}
		}
		else
		{
			bRet = false;
		}

		if (pLine)
		{
			delete pLine;
			pLine = NULL;
		}

		return bRet;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::MatchLineToSignal: Obtains the boundary information between the FHR and a line superimposed
	over the signal. \param pLine: CLine structure \param pLineMatch A CLineMatch occurrence that will either be
	created of reset.
	=======================================================================================================================
	*/
	bool CBaseLineDetection::MatchLineToSignal(CLine *pLine, CLineMatch *pLineMatch)
	{
		/*~~~~~~~~~~~~~~~~~~~*/
		double dTopDistance;
		double dBottomDistance;
		/*~~~~~~~~~~~~~~~~~~~*/

		GetBoundaryInfo(pLine, dTopDistance, dBottomDistance);

		;

		if (pLineMatch == NULL)
		{
			pLineMatch = new CLineMatch;
			if (pLineMatch == NULL)
			{
				return false;
			}
		}
		else
		{
			pLineMatch->Empty();
		}

		*(pLineMatch->m_pLine) = *pLine;
		pLineMatch->m_dTopDistance = dTopDistance;
		pLineMatch->m_dBottomDistance = dBottomDistance;

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBaseLineDetection::GetBoundaryInfo(CLine *pLine, double &dTopDistance, double &dBottomDistance)
	{
		/*~~~~~~~~~~~~~~~~~~~~~*/
		long x1 = pLine->GetX1();
		long x2 = pLine->GetX2();
		double dYline;
		double dYcurve;
		double dYdiff;
		/*~~~~~~~~~~~~~~~~~~~~~*/

		dYline = (pLine->m_dSlope * x1) + pLine->m_dYIntercept;
		dYcurve = GetLowPassValue(x1);
		dTopDistance = dBottomDistance = dYcurve - dYline;

		for (long i = x1 + 1; i <= x2; i++)
		{
			dYline = pLine->m_dSlope * i + pLine->m_dYIntercept;
			dYcurve = GetLowPassValue(i);
			dYdiff = dYcurve - dYline;
			if (dTopDistance < dYdiff)
			{
				dTopDistance = dYdiff;
			}

			if (dBottomDistance > dYdiff)
			{
				dBottomDistance = dYdiff;
			}
		}

		dBottomDistance = fabs(dBottomDistance);
		return;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::FlatBoxSimple: Extend a 'flat' box (no slope) as far as possible while respecting dynamic box
	width constraints \param pBox: box to be extended \param lBoxIndexLimit: limit of extension \param &iNextExtrema:
	index in extrema array of extrema or extrema mid pt after current box end \param iStartExtremaIndex: index in
	extrema array of extrema or extrema midpt after box start
	=======================================================================================================================
	*/
	long CBaseLineDetection::FlatBoxSimple(CBox *pBox, long lBoxIndexLimit, int &iNextExtrema, int iStartExtremaIndex)
	{
		// nextExtrema can be based on extrema midpts - if so convert here to actual
		// extrema index (midpts will lag extrema)
		if (iStartExtremaIndex > 0)
		{
			if ((m_pExtrema[iStartExtremaIndex - 1] >= pBox->m_nStartIndex))
			{
				iStartExtremaIndex--;
			}
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int i = iStartExtremaIndex + 1;
		// get min and max signal vals between box start index and first extremum after
		// start index
		double dMinVal = 240.0;
		double dMaxVal = 0.0;
		double dCurrSamp = 0.0;
		double dCurrWidth = 0.0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long k = pBox->m_nStartIndex; k <= m_pExtrema[iStartExtremaIndex]; k++)
		{
			dCurrSamp = GetLowPassValue(k);
			if (dCurrSamp > dMaxVal)
			{
				dMaxVal = dCurrSamp;
			}

			if (dCurrSamp < dMinVal)
			{
				dMinVal = dCurrSamp;
			}

			dCurrWidth = dMaxVal - dMinVal;
			if (dCurrWidth >= pBox->m_dWidth)
			{
				return k - 1;
			}
		}

		// now can just traverse extrema instead of looking at every sample
		while ((dCurrWidth < pBox->m_dWidth) && (i < m_nExtremaCount) && (m_pExtrema[i] <= lBoxIndexLimit))
		{
			dCurrSamp = GetLowPassValue(m_pExtrema[i]);
			if (dCurrSamp < dMinVal)
			{
				dMinVal = dCurrSamp;
			}

			if (dCurrSamp > dMaxVal)
			{
				dMaxVal = dCurrSamp;
			}

			dCurrWidth = dMaxVal - dMinVal;
			pBox->m_dWidth = GetWidthEstimate(pBox->m_nStartIndex, m_pExtrema[i]);
			i = i + 1;
		}

		if (i == iStartExtremaIndex)
		{
			return pBox->m_nStartIndex;
		}

		iNextExtrema = max(iNextExtrema, i);	// update next extrema out for next step in search

		if (i == m_nExtremaCount)				// passed at last extrema - check at limit of signal
		{
			dCurrSamp = GetLowPassValue(lBoxIndexLimit);
			if (dCurrSamp < dMinVal)
			{
				dMinVal = dCurrSamp;
			}

			if (dCurrSamp > dMaxVal)
			{
				dMaxVal = dCurrSamp;
			}

			dCurrWidth = dMaxVal - dMinVal;
			pBox->m_dWidth = GetWidthEstimate(pBox->m_nStartIndex, lBoxIndexLimit);
			if (dCurrWidth < pBox->m_dWidth)
			{
				return lBoxIndexLimit;
			}
		}

		return m_pExtrema[i - 1];

		// NOTE - Did not implement optional binary search at end of FlatBoxSimple See
		// MATLAB src\core\baselineDetection\FlatBoxSimple if want to implemet
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::RewindBoxOverExtremaUntilFits: Take an invalid box and attempt to rewind it until is is valid
	or until the box endpoint is less than rewindLimit. Rewinding is done across extrema and extrema midpts. \param
	pBox: pointer to invalid box that is to be rewound \param &iNextExtrema: index in extrema array of next extrema
	midpt after current box endpoint \param lMaxRewind: sample index of minimum box endpoint \param pLineMatch:
	CLineMatch object (used for temp storage)
	=======================================================================================================================
	*/
	bool CBaseLineDetection::RewindBoxOverExtremaUntilFits(CBox *pBox, int &iNextExtrema, long lMaxRewind, CLineMatch *pLineMatch)
	{
		/*~~~~~~~~~~~~~~~~~~*/
		bool bIsValid = false;
		/*~~~~~~~~~~~~~~~~~~*/

		if (pBox->m_nEndIndex < lMaxRewind)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iLastExtrema = iNextExtrema - 1;
		bool bUseMidPt = true;
		long lCurrIndex;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while (!(bIsValid))
		{
			if (bUseMidPt)
			{
				if (iLastExtrema < 0)
				{
					return false;
				}

				lCurrIndex = m_pExtremaMidPts[iLastExtrema];
				bUseMidPt = false;
				iLastExtrema--;
			}
			else
			{
				iNextExtrema = iLastExtrema + 1;
				if (iLastExtrema < 0)
				{
					return false;
				}

				lCurrIndex = m_pExtrema[iLastExtrema];
				bUseMidPt = true;
			}

			if (lCurrIndex < lMaxRewind)
			{
				return false;
			}

			pBox->m_dWidth = GetWidthEstimate(pBox->m_nStartIndex, lCurrIndex);
			bIsValid = CreateLineMatch(pBox, lCurrIndex, pLineMatch);
		}

		BoxUpdate(pBox, pLineMatch);
		return true;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::DetectAcceptedParts: TBD \param nA2 TBD \param pBaseLines TBD \param pBaseLinesOut TBD
	=======================================================================================================================
	*/
	void CBaseLineDetection::DetectAcceptedParts(long nA2, fhrPartSet *pBaseLines, fhrPartSet *pBaseLinesOut, fhrPartSet *pBaseLinesPending)
	{
		for (int i = 0; i < pBaseLines->size(); i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			baseline *pBaseLine = (baseline *) pBaseLines->getAt(i);
			baseline *pBaseLineOut = new baseline;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (DetectAcceptedPart(pBaseLine, nA2, pBaseLineOut))
			{
				pBaseLinesOut->add(pBaseLineOut);
				if (pBaseLine->getX2() > pBaseLineOut->getX2())
				{			// truncated
					return; // don't consider rest of baselines - better to restart from a2 in next window
				}
			}
			else
			{
				pBaseLinesPending->add(pBaseLineOut);
			}
		}
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::DetectAcceptedPart: Calculates intersection for each feature with Accept interval. Keep
	baseline if 1) it finishes before a2 2) it starts more than minTruncate before a2 (in which case truncate first
	=======================================================================================================================
	*/
	bool CBaseLineDetection::DetectAcceptedPart(baseline *pBaseLine, long nA2, baseline *pBaseLineOut)
	{
		/*~~~~~~~~~~~~~~~~~~~*/
		bool bAccepted = false;
		/*~~~~~~~~~~~~~~~~~~~*/

		*(pBaseLineOut) = *(pBaseLine);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long iMinTruncate = GetMinTruncateLen();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (pBaseLine->getX2() <= nA2)
		{
			bAccepted = true;
		}
		else if ((nA2 - pBaseLine->getX1()) >= iMinTruncate)	// long enough to cutoff at a2
		{
			bAccepted = true;
			pBaseLineOut->setX2(nA2);						// truncate
		}
		else if ((m_pDisplayWindow->m_lX2 - (GetCommitBuffer()) - pBaseLine->getX1()) >= iMinTruncate)
		{	// this case is for cases of long dropout where there is < 5 minutes of valid baseline before the dropout
			///;
			///Previously was having to wait until end of dropout to commit baseline
			bAccepted = true;
			pBaseLineOut->setX2(nA2);	// truncate
		}
		else
		{
			bAccepted = false;				// pending
		}

		return bAccepted;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::UpdateWindow: This function really only updates the starting point of the next baseline
	detection search in the next high level window. All input baselines should end before or at window.a2
	=======================================================================================================================
	*/
	void CBaseLineDetection::UpdateWindow(fhrPartSet *pBaseLines)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int dNumBaseLines = pBaseLines->size();
		long iMinTruncate = GetMinTruncateLen();
		long lMinNextWindowStart = (long) (m_pDisplayWindow->m_lA2 - iMinTruncate);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_nNextWindowStart = max(m_nNextWindowStart, lMinNextWindowStart + m_pSignal->m_OutputAbsStart);

		if (dNumBaseLines > 0)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			baseline *pLastBaseLine = (baseline *) pBaseLines->getAt(dNumBaseLines - 1);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (pLastBaseLine->getX2() >= m_nNextWindowStart - m_pSignal->m_OutputAbsStart)
			{
				m_nNextWindowStart = pLastBaseLine->getX2() + m_pSignal->m_OutputAbsStart + 1;
			}
		}
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetVarValue: TBD
	=======================================================================================================================
	*/
	double CBaseLineDetection::GetVarValue(long iPoint)
	{
		// Change from Evon - use actual index in array (instead of iPoint - 1)
		return m_pSignal->m_pdVarPassSignal[iPoint];
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetLowPassValue: TBD
	=======================================================================================================================
	*/
	double CBaseLineDetection::GetLowPassValue(long iPoint)
	{
		return m_pSignal->m_pdLowPassSignal[iPoint];
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetSampFreq:
	=======================================================================================================================
	*/
	double CBaseLineDetection::GetSampFreq(void)
	{
		return m_pBasConfig->m_dSmpFreq;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::GetMinBasLen:
	=======================================================================================================================
	*/
	int CBaseLineDetection::GetMinBasLen(void)
	{
		return (int) (ceil(m_pBasConfig->m_dSmpFreq * m_pBasConfig->m_iMinBasLength));
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::ConvertBoxVecToBaseLines:
	=======================================================================================================================
	*/
	void CBaseLineDetection::ConvertBoxVecToBaseLines(CBoxArray *pBoxVec)
	{
		m_pBaseLines.clear();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iNumBox = pBoxVec->GetSize();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < iNumBox; i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			CBox *pCurrBox = pBoxVec->GetAt(i);
			baseline *pNewBL = new baseline;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			pNewBL->setX1(pCurrBox->m_nStartIndex);
			pNewBL->setX2(pCurrBox->m_nEndIndex);

			// Y values are not determined - these will be updated in FHR domain (as opposed
			// to LP) later
			pNewBL->setY1(0);
			pNewBL->setY2(0);

			// pNewBL->pBox2 = pCurrBox;
			m_pBaseLines.add(pNewBL);
		}

		return;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::ConvertBaseLinesToBoxVec
	=======================================================================================================================
	*/
	CBoxes *CBaseLineDetection::ConvertBaseLinesToBoxVec(fhrPartSet *pBaseLines)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		CBoxes *pBoxVec = new CBoxes;
		int iNumBas = pBaseLines->size();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < iNumBas; i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			baseline *pCurrBas = (baseline *) pBaseLines->getAt(i);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (pCurrBas->length() >= GetMinBasLen())
			{
				/*~~~~~~~~~~~~~~~~~~~~~*/
				CBox *pNewBox = new CBox;
				/*~~~~~~~~~~~~~~~~~~~~~*/

				pNewBox->m_nEndIndex = pCurrBas->getX2();
				pNewBox->m_nStartIndex = pCurrBas->getX1();
				pBoxVec->Add(pNewBox);
			}
		}

		return pBoxVec;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::AddBoxCheckOverlap This tries to add a current box to an existing set of boxes, and try to
	merge in such a way that all boxes remain true to height and slope requirements \param pBoxVec - array of current
	valid boxes \param pCurrBox - pointer to box to be added Returns true if pBoxVec was updated (either box added or
	merged), false if not
	=======================================================================================================================
	*/
	bool CBaseLineDetection::AddBoxCheckOverlap(CBoxes *pBoxVec, CBox *pCurrBox)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bAddBox = false;
		int iNumBox = pBoxVec->GetCount();
		CBox *pLastBox;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (iNumBox > 0)
		{
			pLastBox = pBoxVec->GetAt(iNumBox - 1);
		}

		while ((iNumBox > 0) && (pLastBox->m_nStartIndex > pCurrBox->m_nStartIndex))	// can happen in rare cases
		{
			pBoxVec->RemoveAt(iNumBox - 1);
			delete pLastBox;
			iNumBox--;
			if (iNumBox > 0)
			{
				pLastBox = pBoxVec->GetAt(iNumBox - 1);
			}
		}

		if (iNumBox == 0)
		{
			bAddBox = true;
		}
		else
		{
			if (MergeBoxes(pLastBox, pCurrBox))
			{
				return true;			// merged current box to last box
			}
			else						// could not merge the two boxes
			{
				if (pLastBox->m_nEndIndex >= pCurrBox->m_nStartIndex)
				{						// boxes overlap
					bAddBox = TruncateShorter(pLastBox, pCurrBox);
				}
				else
				{
					bAddBox = true;		// no overlap - can just add currBox
				}

				if (!bAddBox)
				{						// truncating shorter did result in valid boxes
					bAddBox = TruncateLonger(pLastBox, pCurrBox);
				}

				if (!bAddBox)
				{
					bAddBox = FindValidSplitPoint(pLastBox, pCurrBox);
				}

				if (!bAddBox)
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
					bool bRC = DoBestMergePossible(pLastBox, pCurrBox, bAddBox);
					/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

					if (bRC && !bAddBox)
					{
						return true;	// merged but no new box added
					}
				}
			}
		}

		if (bAddBox)
		{
			pBoxVec->Add(pCurrBox);
		}

		return bAddBox;
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::MergeBoxes Try to make one box stretching from start index of pBox1 to end index of pBox2
	\param pBox1 - box with earlier start index \param pBox2 - box with later end index Return true if merge
	successful, false otherwise. Merged box will returned as pBox1
	=======================================================================================================================
	*/
	bool CBaseLineDetection::MergeBoxes(CBox *pBox1, CBox *pBox2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRC = false;
		int iMaxDist = (int) (round(m_pBasConfig->m_iMaxDistForMergeTry * GetSampFreq()));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if ((pBox2->m_nStartIndex - pBox1->m_nEndIndex) > iMaxDist)
		{
			return false;	// too far to merge
		}

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		CBox *pTestBox = new CBox;
		CLineMatch *pLineMatch = new CLineMatch;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pTestBox->m_nStartIndex = pBox1->m_nStartIndex;
		pTestBox->m_nEndIndex = pBox2->m_nEndIndex;
		pTestBox->m_dWidth = GetWidthEstimate(pTestBox->m_nStartIndex, pTestBox->m_nEndIndex);

		if (CreateLineMatch(pTestBox, pTestBox->m_nEndIndex, pLineMatch))
		{
			*pBox1 = *pTestBox;
			delete pBox2;
			bRC = true;
		}

		if (pTestBox)
		{
			delete pTestBox;
			pTestBox = NULL;
		}

		if (pLineMatch)
		{
			delete pLineMatch;
			pLineMatch = NULL;
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CBaseLineDetection::TruncateLonger(CBox *pBox1, CBox *pBox2)
	{
		if (pBox1->GetLength() > pBox2->GetLength())
		{
			return TruncateEarlierBox(pBox1, pBox2);
		}
		else
		{
			return TruncateLaterBox(pBox1, pBox2);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CBaseLineDetection::TruncateShorter(CBox *pBox1, CBox *pBox2)
	{
		if (pBox1->GetLength() > pBox2->GetLength())
		{
			return TruncateLaterBox(pBox1, pBox2);
		}
		else
		{
			return TruncateEarlierBox(pBox1, pBox2);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CBaseLineDetection::TruncateEarlierBox(CBox *pBox1, CBox *pBox2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		CBox *pTestBox = new CBox;
		bool bRC = false;
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		*pTestBox = *pBox1;
		pTestBox->m_nEndIndex = pBox2->m_nStartIndex - 1;
		pTestBox->m_dWidth = GetWidthEstimate(pTestBox->m_nStartIndex, pTestBox->m_nEndIndex);

		if (IsValidBox(pTestBox, 0))
		{
			*pBox1 = *pTestBox;
			bRC = true;
		}

		if (pTestBox)
		{
			delete pTestBox;
			pTestBox = NULL;
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CBaseLineDetection::TruncateLaterBox(CBox *pBox1, CBox *pBox2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		CBox *pTestBox = new CBox;
		bool bRC = false;
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		*pTestBox = *pBox2;
		pTestBox->m_nStartIndex = pBox1->m_nEndIndex + 1;
		pTestBox->m_dWidth = GetWidthEstimate(pTestBox->m_nStartIndex, pTestBox->m_nEndIndex);

		if (IsValidBox(pTestBox, 0))
		{
			*pBox2 = *pTestBox;
			bRC = true;
		}

		if (pTestBox)
		{
			delete pTestBox;
			pTestBox = NULL;
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CBaseLineDetection::FindValidSplitPoint(CBox *pBox1, CBox *pBox2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRC = false;
		double pSplitPointsTry[] = { 1 / 2.0, 1 / 3.0, 2 / 3.0, 1 / 4.0, 3 / 4.0, 2 / 5.0, 3 / 5.0, 1 / 4.0, 4 / 5.0 };
		int iNumSplits = sizeof(pSplitPointsTry) / sizeof(*pSplitPointsTry);
		long lX1 = pBox1->m_nStartIndex;
		long lX2 = pBox2->m_nEndIndex;
		CBox *pTestBox1 = new CBox;
		CBox *pTestBox2 = new CBox;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < iNumSplits; i++)
		{
			GetBoxesAtSplitPoint(lX1, lX2, pSplitPointsTry[i], pTestBox1, pTestBox2);
			if ((IsValidBox(pTestBox1, 0)) && (IsValidBox(pTestBox2, 0)))
			{
				*pBox1 = *pTestBox1;
				*pBox2 = *pTestBox2;
				bRC = true;
				break;
			}
		}

		if (pTestBox1)
		{
			delete pTestBox1;
			pTestBox1 = NULL;
		}

		if (pTestBox2)
		{
			delete pTestBox2;
			pTestBox2 = NULL;
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CBaseLineDetection::GetBoxesAtSplitPoint(long lX1, long lX2, double dSplit, CBox *pBox1, CBox *pBox2)
	{
		pBox1->m_nStartIndex = lX1;
		pBox1->m_nEndIndex = (long) (round(lX1 + (dSplit * (lX2 - lX1 + 1))));
		pBox2->m_nStartIndex = pBox1->m_nEndIndex + 1;
		pBox2->m_nEndIndex = lX2;

		pBox1->m_dWidth = GetWidthEstimate(pBox1->m_nStartIndex, pBox1->m_nEndIndex);
		pBox2->m_dWidth = GetWidthEstimate(pBox2->m_nStartIndex, pBox2->m_nStartIndex);

		return;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CBaseLineDetection::DoBestMergePossible(CBox *pBox1, CBox *pBox2, bool &bAddBox)
	{
		bAddBox = false;

		if (m_pBasConfig->m_bNoInvalidBits)
		{
			return false;	// drop current box
		}

		if (pBox1->GetLength() > pBox2->GetLength())
		{					// current box is shorter
			return false;	// drop current box
		}

		// Current box is longer - truncate previous box. If it is 1) degenerate - merge
		// with current box, disregarding slope/width criteria 2) > 20 sec. Just keep even
		// though may not meet slope/width criteria
		pBox1->m_nEndIndex = pBox2->m_nStartIndex - 1;

		if (pBox1->GetLength() > GetMinBasLen())	// not degenerate
		{
			bAddBox = true;
			return true;	// add both boxes to array
		}
		else				// degenerate
		{
			// merge boxes even though does not meet criteria
			pBox1->m_nEndIndex = pBox2->m_nEndIndex;
			delete pBox2;
			return true;
		}
	}

	/*
	=======================================================================================================================
	! CBaseLineDetection::CalcPolyFitPrep Calculates signal summation and signal/time cross correlation for signal in
	current window. These summations then can be used to compute polyfits. Start index of summation corresponds to
	start index of window (i.e. pDisplayWindow->x1 corresponds to time 1)
	=======================================================================================================================
	*/
	void CBaseLineDetection::CalcPolyFitPrep(void)
	{
		/*~~~~~~~~~~~~~*/
		double dCurrSamp;
		/*~~~~~~~~~~~~~*/

		// only want to compute sums in current display window
		if (m_pSignalSum != NULL)
		{
			delete[] m_pSignalSum;
			m_pSignalSum = NULL;
		}

		if (m_pSignalCrossCorr != NULL)
		{
			delete[] m_pSignalCrossCorr;
			m_pSignalCrossCorr = NULL;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lWindowSize = m_pDisplayWindow->GetLength();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_pSignalSum = new double[lWindowSize];
		m_pSignalCrossCorr = new double[lWindowSize];

		m_pSignalSum[0] = GetLowPassValue(m_pDisplayWindow->m_lX1);
		m_pSignalCrossCorr[0] = GetLowPassValue(m_pDisplayWindow->m_lX1);
		for (long i = 1; i < lWindowSize; i++)
		{
			dCurrSamp = GetLowPassValue(m_pDisplayWindow->m_lX1 + i);
			m_pSignalSum[i] = m_pSignalSum[i - 1] + dCurrSamp;
			m_pSignalCrossCorr[i] = m_pSignalCrossCorr[i - 1] + (dCurrSamp * (i + 1));
		}

		return;
	}
}
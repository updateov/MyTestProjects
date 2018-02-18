/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection CLASSES: File contains
* class declaration for: CBaseLine : Description of baseline. CBaseLines : array
* of baselines. CLinePoint : Description of a point in the line. CLine :
* Description of a line. CLineMatch : Description of baseline. CBox : Description
* of a box. CBoxInfo : structure contains box, next extrema and the longest box
* found. CBoxes : array of CBoxInfo. CBaseLineDetection : contains all the
* functions to find the baselines. CBaseLineConfig : contains all baseline
* detection config parameters Copyright LMS Medical Systems 2004 by Evonium Inc.
*/
#pragma once
#include "VectorMatrix.h"
#include "fhrPartSet.h"
#include "fhrPart.h"

namespace patterns
{

	// ! Type of baseline.
	enum BaseLineType { BAS = 0, NOB = 1, NOE = 2 };

	// ! Type of neural net pass.
	enum NNPassType { NONE = 0, PRIM = 1, SEC = 2, PRIM_WEAK = 3, SEC_WEAK = 4 };

	class CBaseLineConfig
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CBaseLineConfig(void);
		~ CBaseLineConfig(void);

		double m_dSmpFreq;					// sampling frequency

		// Parameters in seconds
		double m_dCommitBuffer;				// commit buffer in seconds
		int m_iBasTruncateLen;				// length at which can truncate without waiting for more data
		int m_iMinBasLength;				// minimum baseline length
		int m_iMaxDistForMergeTry;			// max dist btwn bas. to attempt a merge

		// Box width params
		int *m_pPolyVecMid;
		int *m_pPolyVecSmall;
		double m_dSmallVarThresh;
		double m_dMinBoxWidth;
		double m_dMaxBoxWidth;
		double m_dWidthMultFactor;

		// Box slope params
		double m_dCorridorSlopeOffset;
		double m_dCorridorSlope;
		double m_dSlopeMultFactor;

		// Advancement of box start index
		bool m_bAdvOverExtremaDuringBas;
		bool m_bAdvOverExtremaDuringNonBas;
		int m_iSlowAdvStep;
		bool m_bSkipToLastValidEnd;

		// FlatBox simple
		bool m_bDoFlatBoxBinarySearch;
		bool m_bAlwaysFlatBoxSimple;

		// Extend Right
		int m_iNumSuccFailAllow;
		bool m_bAllowSuccFail;
		bool m_bDoExtendRightBinarySearch;

		// CalcExtremaBoxes
		bool m_bDoBinaryBackForAccepted;
		bool m_bDoBinaryForwardForAccepted;

		// Merging
		bool m_bNoInvalidBits;

		// Compute polyfits with precomputed summations
		bool m_bUsePrecomputedPolyFitSums;

		// Skip over repaired segments
		bool m_bSkipRepairSegments;
		double m_dMinRepairSegmentLength;	// min rep segment length to require skip over (seconds)

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetSampFreq(double d)
		{
			m_dSmpFreq = d;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMinRepSegmentLength(void)
		{
			return (long) (m_dMinRepairSegmentLength * m_dSmpFreq);
		};
		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetCommitBufferSec(double s) {m_dCommitBuffer = s;}
		void SetCommitBuffer(long numSamples) {m_dCommitBuffer = ((double) numSamples / m_dSmpFreq);}
		long GetCommitBuffer(void) {return (long) (m_dCommitBuffer * m_dSmpFreq);} // return in samples
		void SetMinTruncateLenSec(long s) {m_iBasTruncateLen = s;}
		long GetMinTruncateLen(void) {return (m_iBasTruncateLen * (long) m_dSmpFreq);} // return in samples

	};

	class CBox;
	class CFHRSignal;

	
	/*
	=======================================================================================================================
	! CLASS: CLinePoint DESCRIPTION: Description of a point in the line.
	=======================================================================================================================
	*/
	class CLinePoint
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CLinePoint(void)
		{
			Empty();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CLinePoint(long xx, double yy)
		{
			x = xx;
			y = yy;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		~CLinePoint(void)
		{
		}

		long x;
		double y;

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		CLinePoint &operator=(const CLinePoint &srcObj)
		{
			if (this == &srcObj)
			{
				return *this;
			}

			x = srcObj.x;
			y = srcObj.y;
			return *this;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void Empty(void)
		{
			x = 0;
			y = 0.0;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetX(void)
		{
			return x;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetY(void)
		{
			return y;
		};
	};

	/*
	=======================================================================================================================
	! CLASS: CLinePointArray DESCRIPTION: Array of line points.
	=======================================================================================================================
	*/
	class CLinePointArray
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		// Construction
		CLinePointArray(void);
		~ CLinePointArray(void);

		// Attributes
		int GetSize(void) const;
		int GetCount(void) const;
		int SetSize(int nNewSize, int nGrowBy = -1);

		// Operations
		void RemoveAll(void);

		// Accessing elements
		CLinePoint *GetAt(int nIndex);
		void SetAt(int nIndex, CLinePoint *newElement);

		// Potentially growing the array
		void SetAtGrow(int nIndex, CLinePoint *newElement);
		int Add(CLinePoint *newElement);

		// Operations that move elements around
		void RemoveAt(int nIndex, int nCount = 1);

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    Implementation
		// -------------------------------------------------------------------------------------------------------------------
		//
	protected:
		CLinePoint **m_pData;	// The actual array of line points.
		int m_nSize;			// Number of elements in the array (i.e., upperBound - 1).
		int m_nMaxSize;			// Maximum size currently allocated.
		int m_nGrowBy;			// Amount by which the array will grow, when it needs to.
	};

	/*
	=======================================================================================================================
	! CLASS: CLine DESCRIPTION: Description of a line.
	=======================================================================================================================
	*/
	class CLine
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CLine(void)
		{
			Empty();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CLine(CLinePoint Point1, CLinePoint Point2)
		{
			Empty();
			m_Point1 = Point1;
			m_Point2 = Point2;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		~CLine(void)
		{
		}

		CLinePoint m_Point1;
		CLinePoint m_Point2;
		double m_dSlope;
		double m_dYIntercept;
		int m_nLength;

		CLine &operator =(const CLine &srcObj);
		void Empty(void);
		void CalcLength(void);
		void GetLineCoefficients(double *pSignal);
		void CreateLine(double *pSignal);
		void CreateLinePrecomp(double *pSignalSum, double *pSignalCrossCorr, long lOffset);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetX1(void)
		{
			return m_Point1.GetX();
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetX2(void)
		{
			return m_Point2.GetX();
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetY1(void)
		{
			return m_Point1.GetY();
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetY2(void)
		{
			return m_Point2.GetY();
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetLength(void)
		{
			return m_nLength;
		};
	};

	/*
	=======================================================================================================================
	! CLASS: CLineMatch DESCRIPTION: Structure containing "Boundary information" of signal // with respect to
	superimposed line. Line info also included.
	=======================================================================================================================
	*/
	class CLineMatch
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CLineMatch(void)
		{
			m_pLine = new CLine;
			Empty();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CLineMatch(CLineMatch &srcObj)
		{
			*this = srcObj;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		~CLineMatch(void)
		{
			if (m_pLine)
			{
				delete m_pLine;
			}
		}

		CLineMatch &operator =(const CLineMatch &srcObj);
		void Empty(void);

		double m_dTopDistance;		// Top vertical distances between line and FHR.
		double m_dBottomDistance;	// Bottom vertical distances between line and FHR.

		CLine *m_pLine;

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long GetX2(void)
		{
			return m_pLine->m_Point2.x;
		};
	};

	/*
	=======================================================================================================================
	! CLASS: CBox DESCRIPTION: Description of a box.
	=======================================================================================================================
	*/
	class CBox
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CBox(void)
		{
			m_pLineMatch = new CLineMatch;
			Empty();
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		~CBox(void)
		{
			if (m_pLineMatch != NULL)
			{
				delete m_pLineMatch;
			}
		};

		CBox &operator =(const CBox &srcObj);
		void Empty(void);

		long m_nStartIndex; // Begining of box.
		long m_nEndIndex;	// End of box.
		double m_dWidth;	// Width of box.

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int GetLength(void)
		{
			return m_nEndIndex - m_nStartIndex + 1;
		};
		CLineMatch *m_pLineMatch;
	};

	class CBoxArray
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CBoxArray(void);
		~ CBoxArray(void);

		int GetSize(void) const;
		int GetCount(void) const;
		int SetSize(int nNewSize, int nGrowBy = -1);
		void RemoveAll(void);

		const CBox *GetAt(int nIndex) const;
		CBox *GetAt(int nIndex);
		void SetAt(int nIndex, CBox *newElement);
		void SetAtGrow(int nIndex, CBox *newElement);
		int Add(CBox *newElement);
		CBox *GetLast(void);

		const CBox *operator [](int nIndex) const;
		void RemoveAt(int nIndex, int nCount = 1);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		CBox **m_pData; // the actual array of data
		int m_nSize;	// # of elements (upperBound - 1)
		int m_nMaxSize; // max allocated
		int m_nGrowBy;	// grow amount
	};

	/*
	=======================================================================================================================
	! CLASS: CBoxes DESCRIPTION: array of CBox.
	=======================================================================================================================
	*/
	class CBoxes :
		public CBoxArray
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CBoxes(void)
		{
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		~CBoxes(void)
		{
			for (int i = 0; i < GetSize(); i++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~*/
				CBox *pBoxVec = GetAt(i);
				/*~~~~~~~~~~~~~~~~~~~~~*/

				if (pBoxVec)
				{
					delete pBoxVec;
					pBoxVec = NULL;
				}
			}

			SetSize(0);
		}
	};

	/*
	=======================================================================================================================
	! CLASS: CDisplayWindow DESCRIPTION: Description of window to display baseline.
	=======================================================================================================================
	*/
	class CDisplayWindow
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CDisplayWindow(void)
		{
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		~CDisplayWindow(void)
		{
		};
		void InitWindow(void);

		long m_lA2;
		long m_lX1;
		long m_lX2;

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long GetLength(void)
		{
			return m_lX2 - m_lX1 + 1;
		};
	};

	/*
	=======================================================================================================================
	! CLASS: CBaseLineDetection DESCRIPTION: contains all the functions to find the baselines.
	=======================================================================================================================
	*/
	class CBaseLineDetection
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CBaseLineDetection(void);
		~ CBaseLineDetection(void);

		CFHRSignal *m_pSignal;
		fhrPartSet m_pBaseLines;
		long *m_pExtrema;	// min/max indexes of fhr (output from PeakDetector)
		int m_nExtremaCount;
		long *m_pExtremaMidPts;
		int m_nExtremaMidPtsCount;

		double *m_pSignalSum;
		double *m_pSignalCrossCorr;

		long m_nOffset;
		long m_iRepStart;
		long m_iRepEnd;
		bool m_bJumpMode;

		long m_nNextWindowStart;

		long m_lLastValidRepairIndex;

		CDisplayWindow *m_pDisplayWindow;
		CBaseLineConfig *m_pBasConfig;

		fhrPartSet m_pRepairIntervals;

		void DetectBaseLine(CFHRSignal *pSignal, fhrPartSet *pBaseLinesFinal, fhrPartSet *pBaseLinesPending);

		void GetCandidateBaselines(fhrPartSet *pBaseLinesPending);
		bool PeakDetector(long nStart, long nEnd, long *pExtrema, int &nCount);
		void GetExtremaMidPts(void);
		void CalcExtremaBoxes(CBoxes *pBoxVecIn);
		void UpdateBoxLoc(int &iStartExtremaIndex, long &lPos, long lPrevBoxEnd);
		int UpdateNextExtremaBeyondIndex(int iExtremaIndexStart, long lSampleIndex);
		void LongestBoxRight(CBox *pBox, int iNextExtrema, int iStartExtremaIndex, int &iNextExtremaOut, CLineMatch *pLineMatch);
		void ExtendRight(CBox *pBox, long lBoxIndexLimit, int &iNextExtrema);
		void BinarySearch(CBox *pBox, long lIndexLastFailed, int iExtremaMidFail);
		void BinarySearchBackwards(CBox *pBox, CBoxes *pBoxVec);
		long VisitExtremas(CBox *pBox, long lBoxIndexLimit, int &iNextExtrema);
		bool VisitNextExtrema(CBox *pBox, int iNextExtrema, bool &bIsValid, long lBoxIndexLimit, CLineMatch *pLineMatch);
		bool BoxUpdate(CBox *pBox, CLineMatch *pLineMatch);
		bool CreateLineMatch(CBox *pBox, long nFlatBoxEndPointX, CLineMatch *pLineMatch);
		bool MatchLineToSignal(CLine *pLine, CLineMatch *pLineMatch);
		void GetBoundaryInfo(CLine *pLine, double &dTopDistance, double &dBottomDistance);
		long FlatBoxSimple(CBox *pBox, long lBoxIndexLimit, int &iNextExtrema, int iStartExtremaIndex);
		void DetectAcceptedParts(long nA2, fhrPartSet *pBaseLines, fhrPartSet *pBaseLinesOut, fhrPartSet *pBaseLinesPending);

		bool DetectAcceptedPart(baseline *pBaseLine, long nA2, baseline *pBaseLineOut);
		void UpdateWindow(fhrPartSet *pBaseLines);
		double GetVarValue(long iPoint);
		double GetLowPassValue(long iPoint);
		void InitWindow(void);
		void SetRepairIntervals(fhrPartSet *pRepairIntervals);

		CBox *GetBoxWithWidthUpdate(long lPos, int iNextExtrema, int iStartExtremaIndex, long lLastValidEndIndex, int &iNextExtremaOut);
		double GetWidthEstimate(long lBoxStart, long lBoxEnd);
		bool RewindBoxOverExtremaUntilFits(CBox *pBox, int &iNextExtrema, long lMaxRewind, CLineMatch *pLineMatch);
		bool IsValidBox(CBox *pBox, long lPrevEndIndex);

		double GetSampFreq(void);
		int GetMinBasLen(void);
		void ConvertBoxVecToBaseLines(CBoxArray *pBoxVec);
		CBoxes *ConvertBaseLinesToBoxVec(fhrPartSet *pBaseLines);

		bool AddBoxCheckOverlap(CBoxes *pBoxVec, CBox *pCurrBox);
		bool MergeBoxes(CBox *pBox1, CBox *pBox2);
		bool TruncateLonger(CBox *pBox1, CBox *pBox2);
		bool TruncateShorter(CBox *pBox1, CBox *pBox2);
		bool TruncateEarlierBox(CBox *pBox1, CBox *pBox2);
		bool TruncateLaterBox(CBox *pBox1, CBox *pBox2);
		bool FindValidSplitPoint(CBox *pBox1, CBox *pBox2);
		void GetBoxesAtSplitPoint(long lX1, long lX2, double dSplit, CBox *pBox1, CBox *pBox2);
		bool DoBestMergePossible(CBox *pBox1, CBox *pBox2, bool &bAddBox);
		void CalcPolyFitPrep(void);
		void CreateLinePrecomp(CLine *pLine);

		void SetSampFreq(double f) {m_pBasConfig->SetSampFreq(f);}
		long GetCommitBuffer(void) {return(m_pBasConfig->GetCommitBuffer());}
		void SetCommitBufferSec(double s) {m_pBasConfig->SetCommitBufferSec(s);}
		void SetCommitBuffer(long numSamples) {m_pBasConfig->SetCommitBuffer(numSamples);}
		long GetMinTruncateLen(void) {return (m_pBasConfig->GetMinTruncateLen());}
		void SetMinTruncateLenSec(long s) {m_pBasConfig->SetMinTruncateLenSec(s);}
	};
}
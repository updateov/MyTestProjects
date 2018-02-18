/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection CLASSES: File contains
* class declaration for: CFHRSegment : A segment of valid FHR signal.
* CFHRArtifact : Artifact (or invalid part) of a FHR signal. Copyright LMS
* Medical Systems 2004 by Evonium Inc.
*/
#pragma once

#include "float.h"

namespace patterns
{

	/*
	* ! CLASS: CFHRSegment DESCRIPTION: A segment of valid FHR signal, that is, a
	* part of a FHR signal in which all samples are inside a (configurable) range.
	* This class holds all about a segment that is required to carry out the required
	* FHR signal repair process.
	*/
	class CFHRArtifact;

	class CFHRSegment
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CFHRSegment(long nOrdinal);
		~ CFHRSegment(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		long m_nOrdinal;
		long m_nStart;
		long m_nLength;
		long m_nFirstOddityPos;

		double m_dMeanHeight;
		double m_dStartHeight;
		double m_dEndHeight;

		CFHRArtifact *m_pNextGap;
		CFHRArtifact *m_pNextArtifact;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetOrdinal(void) const
		{
			return m_nOrdinal;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetStart(void) const
		{
			return m_nStart;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetLength(void) const
		{
			return m_nLength;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetEnd(void) const
		{
			return m_nStart + m_nLength - 1;
		}

		long GetCleanSignalLength(void) const;

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		double GetMeanHeight(void) const
		{
			return m_dMeanHeight;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetStartHeight(void) const
		{
			return m_dStartHeight;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetEndHeight(void) const
		{
			return m_dEndHeight;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CFHRArtifact *GetNextGap(void)
		{
			return m_pNextGap;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CFHRArtifact *GetNextArtifact(void)
		{
			return m_pNextArtifact;
		}

		bool SetPosition(long nStart, long nLength, const double *const pdSignal);
		bool SetFirstOddityPos(long nFirstOddityPos);

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetNextGap(CFHRArtifact *pNextGap)
		{
			m_pNextGap = pNextGap;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetNextArtifact(CFHRArtifact *pNextArtifact)
		{
			m_pNextArtifact = pNextArtifact;
		}

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		void Reset(void);
	};

	/*
	=======================================================================================================================
	! CLASS: CFHRArtifact DESCRIPTION: Artifact (or invalid part) of a FHR signal, that is, mostly, a part of a FHR
	signal between two segments. This class holds all about an artifact that is required to carry out the required FHR
	signal repair process.
	=======================================================================================================================
	*/
	class CFHRArtifact
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CFHRArtifact(void);
		~ CFHRArtifact(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		long m_nLength;
		double m_dHeightDiff;

		double m_dMeanHeight;

		CFHRSegment *m_pNextSegment;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetLength(void) const
		{
			return m_nLength;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetHeightDiff(void) const
		{
			return m_dHeightDiff;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetSlope(void) const
		{
			return (m_nLength < 0) ? DBL_MAX : m_dHeightDiff / (m_nLength + 1);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetMeanHeight(void) const
		{
			return m_dMeanHeight;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		CFHRSegment *GetNextSegment(void) const
		{
			return m_pNextSegment;
		}

		// Set nStartArtifact to zero (or a negative value) to mean the signal starts
		// with a gap. Set nStartRightSegment to nPtsCount (or more) to mean the signal
		// ends with a gap.
		bool SetLengthAndHeightDiff(long nStartArtifact, long nEndArtifact, const double *const pdSignal, long nPtsCount);

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetNextSegment(CFHRSegment *pNextSegment)
		{
			m_pNextSegment = pNextSegment;
		}

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		void Reset(void);
	};
}
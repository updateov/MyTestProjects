/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection Copyright LMS Medical
* Systems 2004 by Evonium Inc.
*/
#include "StdAFX.h"
#include "FHRSegment.h"

namespace patterns
{

	/*
	=======================================================================================================================
	! CFHRSegment::<constructor> \param nOrdinal Ordinal number for this segment.
	=======================================================================================================================
	*/
	CFHRSegment::CFHRSegment(long nOrdinal)
	{
		m_nOrdinal = nOrdinal;
		Reset();
	}

	//
	// =======================================================================================================================
	//    ! CFHRSegment::<destructor>
	// =======================================================================================================================
	//
	CFHRSegment::~CFHRSegment(void)
	{
	}

	/*
	=======================================================================================================================
	! CFHRSegment::GetCleanSignalLength: Gets the length of initial clean signal. Gets the length of the segment from
	its starting point up to either the first oddity or the end of the segment. \return The initial clean signal
	length.
	=======================================================================================================================
	*/
	long CFHRSegment::GetCleanSignalLength(void) const
	{
		if (m_nFirstOddityPos < 0)
		{
			return m_nLength;
		}

		return m_nFirstOddityPos - m_nStart;
	}

	/*
	=======================================================================================================================
	! CFHRSegment::SetPosition: Sets the position of the segment. Sets the position of the segment using the specified
	start and length. Other values are then calculated using the specified signal. \param nStart Start position of the
	segment. \param nLength End position of the segment. \param pdSignal Pointer to the signal containing the segment.
	\return true iff the specified length is greater than zero.
	=======================================================================================================================
	*/
	bool CFHRSegment::SetPosition(long nStart, long nLength, const double *const pdSignal)
	{
		if (nLength <= 0)
		{
			return false;	// The segment length is too small.
		}

		m_nStart = nStart;
		m_nLength = nLength;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// Set the start and end height.
		long nEnd = nStart + nLength - 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_dStartHeight = pdSignal[nStart];
		m_dEndHeight = pdSignal[nEnd];

		// Calculate the mean height.
		m_dMeanHeight = 0.0;
		for (long i = nStart; (i <= nEnd); ++i)
		{
			m_dMeanHeight += (pdSignal[i]);
		}

		m_dMeanHeight /= (nLength);

		return true;
	}

	/*
	=======================================================================================================================
	! CFHRSegment::SetFirstOddityPos: Sets the position of the first oddity. Sets the position of the first oddity in
	the segment to the specified value (nFirstOddityPos). \param nFirstOddityPos Position of the first oddity. \return
	true iff the specified position is valid for the segment.
	=======================================================================================================================
	*/
	bool CFHRSegment::SetFirstOddityPos(long nFirstOddityPos)
	{
		if ((nFirstOddityPos < m_nStart) || (nFirstOddityPos > GetEnd()))
		{
			return false;
		}

		m_nFirstOddityPos = nFirstOddityPos;
		return true;
	}

	/*
	=======================================================================================================================
	! CFHRSegment::Reset: Resets the segment. Sets all properties to essentially indicate this instance contains no
	segment.
	=======================================================================================================================
	*/
	void CFHRSegment::Reset(void)
	{
		m_nStart = m_nLength = 0;
		m_nFirstOddityPos = -1; // i.e., No oddity in this segment.
		m_dMeanHeight = 0.0;
		m_dStartHeight = m_dEndHeight = 0.0;
		m_pNextGap = 0;
		m_pNextArtifact = 0;
	}

	//
	// =======================================================================================================================
	//    ! CFHRArtifact::<default constructor>
	// =======================================================================================================================
	//
	CFHRArtifact::CFHRArtifact(void)
	{
		Reset();
	}

	//
	// =======================================================================================================================
	//    ! CFHRArtifact::<destructor>
	// =======================================================================================================================
	//
	CFHRArtifact::~CFHRArtifact(void)
	{
	}

	/*
	=======================================================================================================================
	! CFHRArtifact::SetLengthAndHeightDiff: Sets length and height difference. For the part of the signal between
	nStartArtifact and nEndArtifact, calculate the length and height difference between its extremeties. \param
	nStartArtifact Start position of the artifact. \param nEndArtifact End position of the artifact. \param pdSignal
	Pointer to the signal containing the artifact. \param nPtsCount Number of points in the signal. \return true iff
	nStartArtifact < nEndArtifact. PRECONDITION: nStartArtifact < 0 shall be interpreted as nStartArtifact == 0 and to
	mean the signal starts with a gap. PRECONDITION: nEndArtifact > (nPtsCount - 1) shall be interpreted as
	nEndArtifact == (nPtsCount - 1) and to mean the signal ends with a gap.
	=======================================================================================================================
	*/
	bool CFHRArtifact::SetLengthAndHeightDiff(long nStartArtifact, long nEndArtifact, const double *const pdSignal, long nPtsCount)
	{
		if (nStartArtifact <= 0)	// The signal starts with a gap.
		{
			if (nEndArtifact < 0)
			{
				return false;
			}

			nStartArtifact = 0;
			m_dHeightDiff = 0;
			m_nLength = nEndArtifact + 1;
		}
		else if (nEndArtifact >= (nPtsCount - 1))	// The signals ends with a gap.
		{
			if (nStartArtifact <= (nPtsCount - 1))
			{
				return false;
			}

			nEndArtifact = (nPtsCount - 1);
			m_dHeightDiff = 0;
			m_nLength = nPtsCount - nStartArtifact;
		}
		else	// The gap is between two segments.
		{
			if (nStartArtifact > nEndArtifact)
			{
				return false;
			}

			m_dHeightDiff = pdSignal[nEndArtifact + 1] - pdSignal[nStartArtifact - 1];
			m_nLength = nEndArtifact - nStartArtifact + 1;
		}

		// Calculate the mean height.
		m_dMeanHeight = 0.0;
		for (long i = nStartArtifact; (i <= nEndArtifact); ++i)
		{
			m_dMeanHeight += (pdSignal[i]);
		}

		if (m_nLength > 1)
		{
			m_dMeanHeight /= (m_nLength);
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CFHRArtifact::Reset: Resets the artifact. Sets all properties to essentially indicate this instance contains no
	artifact.
	=======================================================================================================================
	*/
	void CFHRArtifact::Reset(void)
	{
		m_nLength = 0;
		m_dHeightDiff = 0.0;
		m_dMeanHeight = 0.0;
		m_pNextSegment = 0;
	}
}
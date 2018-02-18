#include "stdafx.h"

#include "Classification.h"

using namespace patterns_classifier;

/*
 =======================================================================================================================
 =======================================================================================================================
 */

CClassification::CClassification(void)
{
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
CClassification::~CClassification(void)
{
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CClassification::reset(void)
{
	m_t.clear();
	m_c.clear();
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
long CClassification::classAtTime(long t, long maxSinceT)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// assume vector is sorted based on time
	long tIndex = indexFromTime(t); // time index equal to or smaller than t
	long outClass = t_notcalc;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	// debugCheck();
	if (tIndex > 0)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long tLast = m_t[tIndex - 1];
		/*~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if ((t - tLast) < maxSinceT)
		{
			outClass = m_c[tIndex - 1];
		}
	}

	return outClass;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
long CClassification::indexFromTime(long t)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~*/
	long n = (long) m_t.size();
	long t0;
	bool done = (n <= 0);
	long i = 0;
	long fLow = 0;
	long fHigh = n - 1;
	/*~~~~~~~~~~~~~~~~~~~~~~~*/

	// check boundaries first
	if (n > 0)
	{
		if (m_t[0] > t)
		{
			done = true;
		}

		if (m_t[n - 1] <= t)
		{
			i = n;
			done = true;
		}
	}

	while (!done)
	{
		i = (fLow + fHigh) / 2;
		t0 = m_t[i];
		if (t0 >= t)
		{
			fHigh = i;
		}

		if (t0 <= t)
		{
			fLow = i;
		}

		if (fLow == fHigh)
		{
			done = true;
		}
		else if (fHigh - fLow == 1)
		{
			i = fHigh;
			done = true;
		}
	}

	return i;
}

/*
 =======================================================================================================================
    Can call this directly if no index to save time
 =======================================================================================================================
 */
void CClassification::setClassAtTime(long t, long c, long index)
{
	m_t.insert(m_t.begin() + index, t);
	m_c.insert(m_c.begin() + index, c);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CClassification::setClassAtTime(long t, long c)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long tIndex = indexFromTime(t);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	setClassAtTime(t, c, tIndex);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
long CClassification::getLastTime(void)
{
	/*~~~~~~~~*/
	long t = -1;
	/*~~~~~~~~*/

	if (!(m_t.empty()))
	{
		t = m_t[m_t.size() - 1];
	}

	return t;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CClassification::debugCheck(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~*/
	long t;
	long c;
	long n = (long) m_t.size();
	/*~~~~~~~~~~~~~~~~~~~~~~~*/

	for (long i = 0; i < n; i++)
	{
		t = m_t[i];
		c = m_c[i];
	}
}
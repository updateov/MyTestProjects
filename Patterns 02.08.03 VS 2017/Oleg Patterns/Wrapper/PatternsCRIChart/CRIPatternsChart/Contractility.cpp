#include "stdafx.h"
#include "Contractility.h"

using namespace patterns;

/*
 =======================================================================================================================
    Construction and destruction.
 =======================================================================================================================
 */
Contractility::Contractility(void)
{
	m_contractilityKey = -1;
	m_start = m_end = 0;
	m_classification = Unknown;
}

Contractility::Contractility(long start, long end, ContractilityClassification classification)
{
	m_start = start;
	m_end = end;
	m_classification = classification;
	m_contractilityKey = -1;
}

Contractility::Contractility(const Contractility& contractilityIndex)
{
	*this = contractilityIndex;
}

Contractility::~Contractility(void)
{
}
/*
 =======================================================================================================================
 =======================================================================================================================
 */

long Contractility::GetStart() const
{
	return m_start;
}

long Contractility::GetEnd() const
{
	return m_end;
}

long Contractility::GetContractilityKey() const
{
	return m_contractilityKey;
}

void Contractility::SetStart(long start)
{
	m_start = start;
}

void Contractility::SetEnd(long end)
{
	m_end = end;
}

void Contractility::SetContractilityKey(long contractilityKey)
{
	m_contractilityKey = contractilityKey;
}
/*
 =======================================================================================================================
    Operators. All operators have the implied meaning and act on all properties.
 =======================================================================================================================
 */
Contractility& Contractility::operator =(const Contractility& contractilityIndex)
{
	m_start = contractilityIndex.m_start;
	m_end = contractilityIndex.m_end;
	m_classification = contractilityIndex.m_classification;
	m_contractilityKey = contractilityIndex.m_contractilityKey;
	return *this;
}

bool Contractility::operator ==(const Contractility& contractilityIndex) const
{
	return m_start == contractilityIndex.m_start && m_end == contractilityIndex.m_end && m_classification == contractilityIndex.m_classification;
		//&& m_contractilityKey == contractilityIndex.m_contractilityKey;
}


Contractility& Contractility::operator -=(long n)
{
	SetStart(GetStart() - n);
	SetEnd(GetEnd() - n);
	return *this;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool Contractility::Intersects(const Contractility& contractilityIndex) const
{
	return GetEnd() >= contractilityIndex.GetStart() && GetStart() <= contractilityIndex.GetEnd();
}

#include "StdAfx.h"
#include "CRICalculation.h"
#include <iostream>
#include <fstream>

namespace patterns
{
	CRICalculation::CRICalculation(void)
	{
		m_contractionsRateLast = 0;
		m_contractions120SecRateLast = 0;
		m_accelerationRateLast = 0;
		m_lateDecelRateLast = 0;
		m_prolongedDecelRateLast = 0;
		m_longAndLargeDecelsRateLast = 0;
		m_lastCalculatedContractility = 0;
	}


	CRICalculation::~CRICalculation(void)
	{
	}

	void CRICalculation::ResetContractionsRates()
	{
		ClearContractionsRate();
		ClearContractionsRateIndex();
		ClearContractions120SecRate();
		ClearContractions120SecRateIndex();
		m_contractionsRateLast = 0;
		m_contractions120SecRateLast = 0;
	}

	void CRICalculation::AppendContractionsRates(long windowSize, long contractionPeakTime)
	{
		long j1 = GetContractionsRateIndexAt(contractionPeakTime);
		long j2 = GetContractionsRateIndexAt(contractionPeakTime + windowSize);
		long pos1 = j1 + 1;
		long pos2 = j2 + 2;

		m_contractionsRate.insert(m_contractionsRate.begin() + pos1, m_contractionsRate[j1] + 1);
		m_contractionsRateIndex.insert(m_contractionsRateIndex.begin() + pos1, contractionPeakTime);

		m_contractionsRate.insert(m_contractionsRate.begin() + pos2, m_contractionsRate[j2 + 1] == 0 ? 0 : m_contractionsRate[j2 + 1] - 1);
		m_contractionsRateIndex.insert(m_contractionsRateIndex.begin() + pos2, contractionPeakTime + windowSize);

		UpdateContractionsRateInRange(pos1, pos2);
	}

	void CRICalculation::AppendContractions120SecRates(long windowSize, long contractionPeakTime)
	{
		long j1 = GetContractions120SecRateIndexAt(contractionPeakTime);
		long j2 = GetContractions120SecRateIndexAt(contractionPeakTime + windowSize);
		long pos1 = j1 + 1;
		long pos2 = j2 + 2;

		m_contractions120SecRate.insert(m_contractions120SecRate.begin() + pos1, m_contractions120SecRate[j1] + 1);
		m_contractions120SecRateIndex.insert(m_contractions120SecRateIndex.begin() + pos1, contractionPeakTime);

		m_contractions120SecRate.insert(m_contractions120SecRate.begin() + pos2, m_contractions120SecRate[j2 + 1] == 0 ? 0 : m_contractions120SecRate[j2 + 1] - 1);
		m_contractions120SecRateIndex.insert(m_contractions120SecRateIndex.begin() + pos2, contractionPeakTime + windowSize);

		UpdateContractions120SecRateInRange(pos1, pos2);
	}


	void CRICalculation::ClearContractionsRate()
	{
		m_contractionsRate.clear();
		m_contractionsRate.insert(m_contractionsRate.end(), 0);
	}

	void CRICalculation::ClearContractionsRateIndex()
	{
		m_contractionsRateIndex.clear();
		m_contractionsRateIndex.insert(m_contractionsRateIndex.end(), 0);
	}

	void CRICalculation::ClearContractions120SecRate()
	{
		m_contractions120SecRate.clear();
		m_contractions120SecRate.insert(m_contractions120SecRate.end(), 0);
	}

	void CRICalculation::ClearContractions120SecRateIndex()
	{
		m_contractions120SecRateIndex.clear();
		m_contractions120SecRateIndex.insert(m_contractions120SecRateIndex.end(), 0);
	}

	void CRICalculation::ResetContractilities()
	{
		Contractility toAdd(0, 0, Contractility::Normal);
		m_contractilities.clear();
		m_contractilities.insert(m_contractilities.end(), toAdd);
		m_lastCalculatedContractility = 0;
	}
		
	
	vector<long> CRICalculation::GetContractionsRate() const
	{
		return m_contractionsRate;
	}
	
	vector<long> CRICalculation::GetContractionsRateIndex() const
	{
		return m_contractionsRateIndex;
	}
	
	vector<long>& CRICalculation::GetContractionsRate()
	{
		return m_contractionsRate;
	}

	long CRICalculation::GetContractionsRateAt(long upi) const
	{
		return m_contractionsRate[GetContractionsRateIndexAt(upi)];
	}

	long CRICalculation::GetContractionsRateIndexAt(long upi) const
	{
		return FindEventRateIndex(upi, m_contractionsRateLast, m_contractionsRateIndex);
	}

	long CRICalculation::GetContractions120SecRateIndexAt(long upi) const
	{
		return FindEventRateIndex(upi, m_contractions120SecRateLast, m_contractions120SecRateIndex);
	}

	void CRICalculation::UpdateContractionsRateInRange(long start, long end)
	{
		for (long j = start + 1; j < end; ++j)
		{
			m_contractionsRate[j]++;
		}
	}

	void CRICalculation::UpdateContractions120SecRateInRange(long start, long end)
	{
		for (long j = start + 1; j < end; ++j)
		{
			m_contractions120SecRate[j]++;
		}
	}

	// =================================================================================================================
	//    Look for index in m_<event>Rate for given fhr index. We first look at the last returned index and, if the given fhr index is
	//    not close, that is if it does not fall between ratesIndex[rateLast - 1] and ratesIndex[rateLast + 1], we go for
	//    the binary search.
	// ===================================================================================================================
	long CRICalculation::FindEventRateIndex(long index, long& rateLast, vector<long>& ratesIndex) const
	{
		if (ratesIndex.size() == 0)
		{
			return -1;
		}

		long a = rateLast;
		long b = rateLast;
		long n = (long)ratesIndex.size();

		if (index < 0)
		{
			index = 0;
		}

		if (index >= ratesIndex.back())
		{
			rateLast = n - 1;
		}
		else if (rateLast > 0 && index < ratesIndex[rateLast - 1])
		{
			a = 0;
		}
		else if (index < ratesIndex[rateLast])
		{
			rateLast--;
		}
		else if (rateLast + 2 < n && index > ratesIndex[rateLast + 2])
		{
			b = n;
		}
		else if (rateLast + 2 < n && index == ratesIndex[rateLast + 2])
		{
			rateLast += 2;
		}
		else if (rateLast + 1 < n && index >= ratesIndex[rateLast + 1])
		{
			rateLast++;
		}

		if (a != b)
		{
			while (a < b - 1)
			{
				if (index == ratesIndex[(a + b) / 2])
				{
					a = b = (a + b) / 2;
				}
				else if (index < ratesIndex[(a + b) / 2])
				{
					b = (a + b) / 2;
				}
				else
				{
					a = (a + b) / 2;
				}
			}

			rateLast = a;
		}

		return rateLast;
	}

	long CRICalculation::GetNumberOfContractilities(void) const
	{
		return (long)m_contractilities.size();
	}

	// ===================================================================================================================
	// ===================================================================================================================
	long CRICalculation::FindContractilityIndex(long i) const
	{
		long numOfContractilities = GetNumberOfContractilities();
		while ((numOfContractilities > 0) && (m_contractilities[numOfContractilities - 1].GetEnd() > i))
		{
			numOfContractilities--;
		}

		return numOfContractilities;
	}

	const Contractility& CRICalculation::GetContractility(long i) const
	{
		static Contractility cdump;
		if (i < 0)
			i = 0;

		long nContractilities = GetNumberOfContractilities();
		if (i >= nContractilities)
			i = nContractilities - 1;

		return i < 0 ? cdump : m_contractilities[i];
	}

	const Contractility& CRICalculation::GetContractilityByIndex(long index) const
	{
		return GetContractility(FindContractilityIndex(index));
	}


	void CRICalculation::AppendContractility(const Contractility& contractility)
	{
		long curContractilityIndex = FindContractilityIndex(contractility.GetStart());		

		if (m_contractilities.size() == 0 || !GetContractility(curContractilityIndex).Intersects(contractility) || curContractilityIndex < 0)
		{
			if(curContractilityIndex >=0)
				m_contractilities.insert(m_contractilities.begin() + curContractilityIndex, contractility);
			else
				m_contractilities.push_back(contractility);
			m_lastCalculatedContractility = contractility.GetEnd();
		}
	}

	void CRICalculation::AppendContractilities(const vector<Contractility>& contractilities)
	{
		if (contractilities.size() == 0)
			return;

		if (m_contractilities.size() == 0)
		{
			// Special case for first injection
			m_contractilities.insert(m_contractilities.end(), contractilities.begin(), contractilities.end());
		}
		else
		{
			for (vector<Contractility>::const_iterator itr = contractilities.begin(); itr != contractilities.end(); ++itr)
			{
				AppendContractility(*itr);
			}
		}
	}


	void CRICalculation::MergeContractilities(const vector<Contractility>& contractilities)
	{	

		if (contractilities.size() == 0)	
			return;
		

		if (m_contractilities.size() == 0)
		{
			// Special case for first injection
			m_contractilities.insert(m_contractilities.end(), contractilities.begin(), contractilities.end());
			
		}
		else if(m_contractilities.back().GetEnd() < contractilities.begin()->GetStart()) //no intersections with old just append new contractilities
		{
			
			AppendContractilities(contractilities);
			
		}
		else if(contractilities.begin()->GetStart() <= m_contractilities.begin()->GetStart())
		{	
			
			m_contractilities.clear();
			AppendContractilities(contractilities);				
		}
		else
		{
			long firstIntersectionIndex = FindContractilityIndex(contractilities.begin()->GetStart());
			
			if(firstIntersectionIndex == GetNumberOfContractilities()) // no intersections with old just append new contractilities
			{
				
				AppendContractilities(contractilities);
				
			}
			else // remove contractilities until intersection and append new contractilities
			{	
				
				int numToErase = GetNumberOfContractilities() - firstIntersectionIndex - 1;

				if(numToErase > 0)
					m_contractilities.erase(m_contractilities.begin() + firstIntersectionIndex + 1, m_contractilities.end());

				if(!m_contractilities.empty() && m_contractilities.back().Intersects(contractilities.front())) // new start contractility intersects with old end contractility
				{
					if(m_contractilities.back().GetStart() >= contractilities.begin()->GetStart())
					{
						m_contractilities.pop_back();
						AppendContractilities(contractilities);
					}
					else
					{
						AppendContractilitiesIntersectedWithEnd(contractilities);
					}
				}
				else
				{
					AppendContractilities(contractilities);
				}

			}
		}
		if(!m_contractilities.empty())
			m_lastCalculatedContractility = m_contractilities.back().GetEnd();
	}
	void CRICalculation::AppendContractilitiesIntersectedWithEnd(const vector<Contractility>& contractilities)
	{
		vector<Contractility>::const_iterator begin = contractilities.begin();
		Contractility oldEndContractility = m_contractilities.back();
		Contractility newBeginContractility = contractilities.front();

		if(oldEndContractility.Intersects(newBeginContractility)) // new start contractility intersects with old end contractility
		{
			if(oldEndContractility.GetClassification() == newBeginContractility.GetClassification()) // the same clasification: just update the end
			{
				if(oldEndContractility.GetStart() != newBeginContractility.GetStart() || oldEndContractility.GetEnd() != newBeginContractility.GetEnd())
				{	
					if(oldEndContractility.GetStart() >= newBeginContractility.GetStart())
					{
						oldEndContractility.SetStart(newBeginContractility.GetStart());
						m_contractilities.back().SetStart(newBeginContractility.GetStart());
					}
					oldEndContractility.SetEnd(newBeginContractility.GetEnd());					
					m_contractilities.back().SetEnd(newBeginContractility.GetEnd());
					begin++;
				}
			}
			else // different classification: break old contractility
			{
				if (oldEndContractility.GetEnd() > newBeginContractility.GetStart() - 1)
				{
					oldEndContractility.SetEnd(newBeginContractility.GetStart() - 1);
				}
			
			//	if(oldEndContractility.GetStart() < newBeginContractility.GetStart() - 1)
			//	{					
			//		m_contractilities.back().SetEnd(newBeginContractility.GetStart() - 1);
		
			//	}

			}

			if(begin != contractilities.end())
			{
				
				m_contractilities.insert(m_contractilities.end(), begin, contractilities.end());
			}
		}
	}

	void CRICalculation::SetContractilityEndValue(long index)
	{
		m_contractilities.back().SetEnd(index);
	}

	Contractility::ContractilityClassification CRICalculation::GetContractilityClassification(long index) const
	{
		long curIndex = FindContractilityIndex(index);
		const Contractility& curContractility = GetContractility(curIndex);
		Contractility::ContractilityClassification toRet = curContractility.GetClassification();
		return toRet;
	}


	bool CRICalculation::ContractilitiesEqual(const CRICalculation& a, bool iincludeoverlap/* = false*/) const
	{
		long i;
		long n = (long)m_contractilities.size();
		bool bEqual = n == (long)a.GetNumberOfContractilities();

		for (i = 0; i < n; i++)
		{
			bEqual = iincludeoverlap ? GetContractilityByIndex(i).Intersects(a.GetContractilityByIndex(i)) : GetContractilityByIndex(i) == a.GetContractilityByIndex(i);
			if (!bEqual)
				break;
		}

		return bEqual;
	}

	CRICalculation& CRICalculation::operator=(const CRICalculation& curCalculation)
	{
		m_contractionsRate.clear();
		m_contractionsRateIndex.clear();
		m_contractions120SecRate.clear();
		m_contractions120SecRateIndex.clear();
		m_contractionsRateLast = 0;
		m_contractions120SecRateLast = 0;

		m_contractilities = curCalculation.m_contractilities;
		m_lastCalculatedContractility = curCalculation.m_lastCalculatedContractility;

		return *this;
	}
}

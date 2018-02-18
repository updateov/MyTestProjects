#pragma once

#include <string>
#include <vector>
#include <map>
#include "Contractility.h"

using namespace std;

namespace patterns
{
	enum EventClassification
	{
		Acceleration,
		LateDeceleration,
		LongAndLargeDeceleration,
		ProlongedDeceleration
	};

	class CRICalculation
	{
	public:
		CRICalculation(void);
		virtual ~CRICalculation(void);

	protected:
		mutable vector<long> m_contractionsRate;
		mutable vector<long> m_contractionsRateIndex;
		mutable vector<long> m_contractions120SecRate;
		mutable vector<long> m_contractions120SecRateIndex;
		mutable long m_contractionsRateLast;
		mutable long m_contractions120SecRateLast;

		mutable long m_accelerationRateLast;
		mutable long m_lateDecelRateLast;
		mutable long m_prolongedDecelRateLast;
		mutable long m_longAndLargeDecelsRateLast;

		vector<Contractility> m_contractilities;
		long m_lastCalculatedContractility;

	public:
		void ResetContractionsRates();
		void AppendContractionsRates(long windowSize, long contractionPeakTime);
		void AppendContractions120SecRates(long windowSize, long contractionPeakTime);

		void ClearContractionsRate();
		void ClearContractionsRateIndex();
		void ClearContractions120SecRate();
		void ClearContractions120SecRateIndex();

		void ResetContractilities();
		
		vector<long> GetContractionsRate() const;
		vector<long> GetContractionsRateIndex() const;

		vector<long>& GetContractionsRate();

		long GetContractionsRateAt(long upi) const;
		long GetContractionsRateIndexAt(long upi) const;
		long GetContractions120SecRateIndexAt(long upi) const;


		void UpdateContractionsRateInRange(long start, long end);
		void UpdateContractions120SecRateInRange(long start, long end);

		long FindEventRateIndex(long index, long& rateLast, vector<long>& ratesIndex) const;

		const vector<Contractility>& GetContractilities() { return m_contractilities; }
		long GetNumberOfContractilities(void) const;
		const Contractility& GetContractility(long) const;
		const Contractility& GetContractilityByIndex(long index) const;
		void AppendContractility(const Contractility&);
		void AppendContractilities(const vector<Contractility>&);
		void MergeContractilities(const vector<Contractility>& contractilities);
		void SetContractilityEndValue(long index);
		long FindContractilityIndex(long) const;
		Contractility::ContractilityClassification GetContractilityClassification(long index) const;

		bool ContractilitiesEqual(const CRICalculation& a, bool iincludeoverlap = false) const;
		CRICalculation& operator= (const CRICalculation&);
	private:
		void AppendContractilitiesIntersectedWithEnd(const vector<Contractility>& contractilities);
	};
};

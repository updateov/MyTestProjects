#pragma once

#include <map>
#include <string>

using namespace std;

namespace patterns
{
	class Contractility
	{
	public:
		enum ContractilityClassification
		{
			Unknown = -1,
			Normal = 0,
			Alert = 1,
			Danger = 2
		};


	protected:
		long m_start;
		long m_end;

		ContractilityClassification m_classification;

		long m_contractilityKey;

	public:
		Contractility(void);
		Contractility(long, long, ContractilityClassification);
		Contractility(const Contractility& contractilityIndex);
		virtual ~Contractility(void);

		long GetStart() const;
		long GetEnd() const;
		long GetContractilityKey() const;

		void SetStart(long);
		void SetEnd(long);
		void SetContractilityKey(long);

		ContractilityClassification GetClassification() const
		{
			return m_classification;
		}

		void SetClassification(ContractilityClassification val)
		{
			m_classification = val;
		}

		long GetLength() const 
		{ 
			return m_end - m_start + 1; 
		}

		Contractility& operator =(const Contractility& contractilityIndex);
		bool operator ==(const Contractility& contractilityIndex) const;
		Contractility& operator -=(long);

		bool Intersects(const Contractility& contractilityIndex) const;
	};
}

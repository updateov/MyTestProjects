#pragma once

#include "FiltersCalculator.h"

class FilterWrapper
{
public:
	FilterWrapper();
	virtual ~FilterWrapper();

	static FilterWrapper* Instance();
	static void Destroy();

	FiltersCalculator* GetCalculator()
	{
		return m_pCalc;
	}

private:
	static FilterWrapper* s_pInstance;

	FiltersCalculator* m_pCalc;
};


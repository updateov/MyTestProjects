#include "stdafx.h"
#include "FilterWrapper.h"

FilterWrapper* FilterWrapper::s_pInstance = NULL;

FilterWrapper::FilterWrapper()
{
	m_pCalc = new FiltersCalculator();
}


FilterWrapper::~FilterWrapper()
{
	delete m_pCalc;
	m_pCalc = NULL;
}

FilterWrapper* FilterWrapper::Instance()
{
	if (s_pInstance == NULL)
	{
		s_pInstance = new FilterWrapper();
	}

	return s_pInstance;
}

void FilterWrapper::Destroy()
{
	if (s_pInstance)
	{
		delete s_pInstance;
		s_pInstance = NULL;
	}
}
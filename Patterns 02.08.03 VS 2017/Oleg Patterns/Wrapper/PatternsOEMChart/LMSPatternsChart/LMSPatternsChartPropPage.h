#pragma once

// LMSPatternsChartPropPage.h : Declaration of the CLMSPatternsChartPropPage property page class.


// CLMSPatternsChartPropPage : See LMSPatternsChartPropPage.cpp for implementation.

class CLMSPatternsChartPropPage : public COlePropertyPage
{
	DECLARE_DYNCREATE(CLMSPatternsChartPropPage)
	DECLARE_OLECREATE_EX(CLMSPatternsChartPropPage)

// Constructor
public:
	CLMSPatternsChartPropPage();

// Dialog Data
	enum { IDD = IDD_PROPPAGE_LMSPATTERNSCHART };

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Message maps
protected:
	DECLARE_MESSAGE_MAP()
};


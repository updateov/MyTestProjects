#pragma once

// LMSPatternsChartPropPage.h : Declaration of the CRIPatternsChartPropPage property page class.


// CRIPatternsChartPropPage : See CRIPatternsChartPropPage.cpp for implementation.

class CRIPatternsChartPropPage : public COlePropertyPage
{
	DECLARE_DYNCREATE(CRIPatternsChartPropPage)
	DECLARE_OLECREATE_EX(CRIPatternsChartPropPage)

// Constructor
public:
	CRIPatternsChartPropPage();

// Dialog Data
	enum { IDD = IDD_PROPPAGE_LMSPATTERNSCHART };

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Message maps
protected:
	DECLARE_MESSAGE_MAP()
};


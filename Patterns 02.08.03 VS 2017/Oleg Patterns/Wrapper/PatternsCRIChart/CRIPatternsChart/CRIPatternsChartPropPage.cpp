// LMSPatternsChartPropPage.cpp : Implementation of the CRIPatternsChartPropPage property page class.

#include "stdafx.h"
#include "CRIPatternsChart.h"
#include "CRIPatternsChartPropPage.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


IMPLEMENT_DYNCREATE(CRIPatternsChartPropPage, COlePropertyPage)



// Message map

BEGIN_MESSAGE_MAP(CRIPatternsChartPropPage, COlePropertyPage)
END_MESSAGE_MAP()



// Initialize class factory and guid

IMPLEMENT_OLECREATE_EX(CRIPatternsChartPropPage, "LMSPATTERNSCHA.CRIPatternsChartPropPage.1",
	0xbde41207, 0x1be0, 0x442c, 0xac, 0x85, 0xef, 0x12, 0xac, 0x26, 0x9c, 0xb8)



// CRIPatternsChartPropPage::CRIPatternsChartPropPageFactory::UpdateRegistry -
// Adds or removes system registry entries for CRIPatternsChartPropPage

BOOL CRIPatternsChartPropPage::CRIPatternsChartPropPageFactory::UpdateRegistry(BOOL bRegister)
{
	if (bRegister)
		return AfxOleRegisterPropertyPageClass(AfxGetInstanceHandle(),
			m_clsid, IDS_LMSPATTERNSCHART_PPG);
	else
		return AfxOleUnregisterClass(m_clsid, NULL);
}



// CRIPatternsChartPropPage::CRIPatternsChartPropPage - Constructor

CRIPatternsChartPropPage::CRIPatternsChartPropPage() :
	COlePropertyPage(IDD, IDS_LMSPATTERNSCHART_PPG_CAPTION)
{
}



// CRIPatternsChartPropPage::DoDataExchange - Moves data between page and properties

void CRIPatternsChartPropPage::DoDataExchange(CDataExchange* pDX)
{
	DDP_PostProcessing(pDX);
}



// CRIPatternsChartPropPage message handlers

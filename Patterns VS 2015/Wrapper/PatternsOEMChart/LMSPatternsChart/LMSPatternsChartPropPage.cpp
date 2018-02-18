// LMSPatternsChartPropPage.cpp : Implementation of the CLMSPatternsChartPropPage property page class.

#include "stdafx.h"
#include "LMSPatternsChart.h"
#include "LMSPatternsChartPropPage.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


IMPLEMENT_DYNCREATE(CLMSPatternsChartPropPage, COlePropertyPage)



// Message map

BEGIN_MESSAGE_MAP(CLMSPatternsChartPropPage, COlePropertyPage)
END_MESSAGE_MAP()



// Initialize class factory and guid

IMPLEMENT_OLECREATE_EX(CLMSPatternsChartPropPage, "LMSPATTERNSCHA.LMSPatternsChaPropPage.1",
	0x86ef25ce, 0x2a8a, 0x4fc0, 0x99, 0x3b, 0xc4, 0x57, 0x53, 0x75, 0x92, 0xdf)



// CLMSPatternsChartPropPage::CLMSPatternsChartPropPageFactory::UpdateRegistry -
// Adds or removes system registry entries for CLMSPatternsChartPropPage

BOOL CLMSPatternsChartPropPage::CLMSPatternsChartPropPageFactory::UpdateRegistry(BOOL bRegister)
{
	if (bRegister)
		return AfxOleRegisterPropertyPageClass(AfxGetInstanceHandle(),
			m_clsid, IDS_LMSPATTERNSCHART_PPG);
	else
		return AfxOleUnregisterClass(m_clsid, NULL);
}



// CLMSPatternsChartPropPage::CLMSPatternsChartPropPage - Constructor

CLMSPatternsChartPropPage::CLMSPatternsChartPropPage() :
	COlePropertyPage(IDD, IDS_LMSPATTERNSCHART_PPG_CAPTION)
{
}



// CLMSPatternsChartPropPage::DoDataExchange - Moves data between page and properties

void CLMSPatternsChartPropPage::DoDataExchange(CDataExchange* pDX)
{
	DDP_PostProcessing(pDX);
}



// CLMSPatternsChartPropPage message handlers

// TracingsContainerWnd.cpp : implementation file
//

#include "stdafx.h"
#include "TracingsContainerWnd.h"
#include "PatternsChart.h"
#include "patterns gui, tracing.h"

// TracingsContainerWnd

IMPLEMENT_DYNAMIC(TracingsContainerWnd, CWnd)

TracingsContainerWnd::TracingsContainerWnd()
{

}

TracingsContainerWnd::~TracingsContainerWnd()
{
}


BEGIN_MESSAGE_MAP(TracingsContainerWnd, CWnd)
	//ON_WM_PAINT()
END_MESSAGE_MAP()


//
//// TracingsContainerWnd message handlers
//void TracingsContainerWnd::OnPaint()
//{
//	CPaintDC dc(this);
//	CRect clientRect;
//	GetClientRect(clientRect);
//	dc.FillRect(clientRect, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
//}

BOOL TracingsContainerWnd::OnWndMsg(UINT message, WPARAM wParam, LPARAM lParam, LRESULT* pResult)
{
	bool bIsValidMsg = false;
	CWnd* pParent = GetParent();
	if (message == WM_COMMAND)
	{
		
		pParent->SendMessageA(message, wParam, lParam);
		
		return TRUE;

	}
	bool bMessageProcessed = false;
	switch (message)
	{

	case SHOW_EXPORT_MSG:
	case FORCE_UPDATE_ADAPTER_MSG:
	case SWITCH_15MIN_30MIN_VIEWS_MSG:
	case MSG_SETSLIDERPOSITION:
	{		
		pParent->PostMessageA(message, wParam, lParam);
		
		return TRUE;
	}
	break;
	default:
		return CWnd::OnWndMsg(message, wParam, lParam, pResult);
	}


	return TRUE;
}

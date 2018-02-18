#include "StdAfx.h"

#include "ExportNativeManager.h"

ExportNativeManager::ExportNativeManager(void)
{
	m_bInitialized = false;
	m_screenWidthInMinutes = 255;
	m_startTime = (DATE)0;
	m_patternsCtrl = NULL;
}

ExportNativeManager::~ExportNativeManager(void)
{
}

void ExportNativeManager::Init(DATE startTime, double screenWidthInMinutes)
{
	m_screenWidthInMinutes = screenWidthInMinutes;
	m_startTime = startTime;
	m_bInitialized = true;
}



void ExportNativeManager::InitNavigationPanel(long parentHandle, int x, int y, int width, int height)
{
	//if(m_pPatternsAdapter)
	//{
	//	VARIANT_BOOL retVal;
	//	m_pPatternsAdapter->InitNavigationPanel(parentHandle, x,  y, width, height, &retVal);
	//}
}

void ExportNativeManager::ResizeNavigationPanel(int x, int y, int width, int height)
{
	//if(m_pPatternsAdapter)
	//	m_pPatternsAdapter->ResizeNavigationPanel(x,  y, width, height);
}

void ExportNativeManager::InitExportButton(long parentHandle, int x, int y, int width, int height)
{
	//VARIANT_BOOL retVal;
	//if(m_pPatternsAdapter)
	//	m_pPatternsAdapter->InitExportButton(parentHandle, x,  y, width, height, &retVal);
}

void ExportNativeManager:: ResizeExportButton(int x, int y, int width, int height)
{
	//if(m_pPatternsAdapter)
	//	m_pPatternsAdapter->ResizeExportButton(x,  y, width, height);
}




HRESULT ExportNativeManager::SetExportableChunks(DATE startTime)
{
	time_t rawtime;
    struct tm * timeinfo;



	return S_OK;
}

void ExportNativeManager::HideControls(bool bHide, bool valid)
{
	if(IsWindow(m_clrWnd))
	{
		
		SendMessage(m_clrWnd, MSG_EXPORT_HIDE_CONTROLS, bHide? 1 : 0, valid? 1 : 0);
	}

}

void ExportNativeManager::SetStartTime(DATE startTime)
{
	m_startTime = startTime;
	if(IsWindow(m_clrWnd))
	{
		SendMessage(m_clrWnd, MSG_EXPORT_SET_START_TIME, 0, (LPARAM)&m_startTime);
	}

}

void ExportNativeManager::BeginUpdateChunks()
{
	if(IsWindow(m_clrWnd))
	{
		SendMessage(m_clrWnd, MSG_EXPORT_BEGIN_UPDATE, 0, 0);
	}

}

void ExportNativeManager::AddChunk(int exportID, int intervalID, DATE startTime, int timeRange, bool isExported, long x1, long x2)
{
	ExportChunkStruct chunk(exportID, intervalID, startTime,timeRange,isExported, x1, x2);
	if(IsWindow(m_clrWnd))
	{
		SendMessage(m_clrWnd, MSG_EXPORT_ADD_CHUNK, 0, (LPARAM)&chunk);
	}

}

void ExportNativeManager::EndUpdateChunks()
{
	if(IsWindow(m_clrWnd))
	{
		SendMessage(m_clrWnd, MSG_EXPORT_END_UPDATE, 0, 0);
	}
	
}

void ExportNativeManager::SetTimeRange(int timeRange)
{
	if(IsWindow(m_clrWnd))
	{
		SendMessage(m_clrWnd, MSG_EXPORT_SET_RANGE, 0, (LPARAM)&timeRange);
	}

}



void ExportNativeManager::SetInitParams(const CString& userID, int episodeID, bool canModify, HWND criHWND)
{
	WPFAdapterInitParams initParams(userID, episodeID, canModify, criHWND);
	if(IsWindow(m_clrWnd))
	{
		SendMessage(m_clrWnd, MSG_EXPORT_SET_INITPARAMS, 0, (LPARAM)&initParams);
	}

}

void ExportNativeManager::SetMontevideoVisible(bool isPressed, bool raiseEvent)
{
	if(IsWindow(m_clrWnd))
	{
		
		SendMessage(m_clrWnd, MSG_EXPORT_SET_MONTEVIDEO, isPressed? 1 : 0, raiseEvent? 1 : 0);
	}

}

void ExportNativeManager::SetPanelTooltip(const CString  message, DATE end36Weeks)
{
	if(IsWindow(m_clrWnd))
	{
		PanelToolTip tooltip(message, end36Weeks);
		SendMessage(m_clrWnd, MSG_EXPORT_SET_TOOLTIP, 0, (LPARAM)&tooltip);
	}

}

void ExportNativeManager::SetRoundExportValue(const RoundExportValueStruct& roundExportVal)
{
	if (IsWindow(m_clrWnd))
	{		
		SendMessage(m_clrWnd, MSG_EXPORT_SET_ROUND_VAL, 0, (LPARAM)&roundExportVal);
	}

}
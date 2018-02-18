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

}



void ExportNativeManager::InitNavigationPanel(long parentHandle, int x, int y, int width, int height)
{

}

void ExportNativeManager::ResizeNavigationPanel(int x, int y, int width, int height)
{

}

void ExportNativeManager::InitExportButton(long parentHandle, int x, int y, int width, int height)
{

}

void ExportNativeManager:: ResizeExportButton(int x, int y, int width, int height)
{

}




HRESULT ExportNativeManager::SetExportableChunks(DATE startTime)
{
	return S_OK;
}

void ExportNativeManager::HideControls(bool bHide, bool valid)
{


}

void ExportNativeManager::SetStartTime(DATE startTime)
{


}

void ExportNativeManager::BeginUpdateChunks()
{

}

void ExportNativeManager::AddChunk(int exportID, int intervalID, DATE startTime, int timeRange, bool isExported, long x1, long x2)
{


}

void ExportNativeManager::EndUpdateChunks()
{

	
}

void ExportNativeManager::SetTimeRange(int timeRange)
{


}



void ExportNativeManager::SetInitParams(const CString& userID, int episodeID, bool canModify, HWND criHWND)
{

}

void ExportNativeManager::SetMontevideoVisible(bool isPressed)
{

}

void ExportNativeManager::SetPanelTooltip(const CString  message, DATE end36Weeks)
{

}


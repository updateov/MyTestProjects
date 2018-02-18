#pragma once

//#ifdef _DEBUG
//	#import "..\PeriGen.Patterns.WebSite\common\Perigen.Patterns.NnetControls.tlb" raw_interfaces_only //, named_guids, no_namespace
//#else
//	#import "..\..\Release\Perigen.Patterns.NnetControls.tlb" raw_interfaces_only //, named_guids, no_namespace
//#endif

//using namespace Perigen_Patterns_NnetControls;
#include "ExportWndMessages.h"

class PatternsChartCtrl;


#define MOUSE_OVER_CALLBACK_MSG (WM_USER + 11)
#define MOUSE_LEAVE_CALLBACK_MSG (WM_USER + 12)
#define TIME_RANGE_CHANGE_CALLBAK_MSG (WM_USER + 13)

#define GET_EXPORTENTRIES_MSG (WM_USER + 15)

class ExportNativeManager 
{
	
public:

	ExportNativeManager(void);
	~ExportNativeManager(void);
public:
	void Init(DATE startTime, double screenWidthInMinutes);
	void OnExportPanelMouseOver();

	HRESULT __stdcall SetExportableChunks(DATE startTime);


	

	void InitNavigationPanel(long parentHandle, int x, int y, int width, int height);
	void ResizeNavigationPanel(int x, int y, int width, int height);

    void InitExportButton(long parentHandle, int x, int y, int width, int height);
	void ResizeExportButton(int x, int y, int width, int height);

	void HideControls(bool bHide = true, bool valid = true);
	void SetMontevideoVisible(bool isPressed);

	
	bool IsInitialized() const
	{return m_bInitialized;}
	void BeginUpdateChunks();
	void AddChunk(int exportID, int intervalID, DATE startTime, int timeRange, bool isExported, long x1, long x2);
	void EndUpdateChunks();
	void SetStartTime(DATE startTime);
	void SetTimeRange(int timeRange);
	
	void SetInitParams(const CString& userID, int episodeID, bool canModify, HWND m_criHWND);
	void SetPanelTooltip(const CString  message, DATE end36Weeks);
public:
	
	void SetPatternsControl(HWND hWnd)
	{m_patternsCtrl = hWnd;}
	void SetCLRWindow(HWND hWnd)
	{m_clrWnd = hWnd;}
protected:
	
private:
	
	
	DATE m_startTime;
	double m_screenWidthInMinutes;
	bool m_bInitialized;
	HWND m_patternsCtrl;
	HWND m_clrWnd;
};


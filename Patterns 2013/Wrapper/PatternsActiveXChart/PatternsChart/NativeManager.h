#pragma once

#ifdef _DEBUG
	#import "..\..\PeriGen.Patterns.WebSite\common\Perigen.Patterns.NnetControls.tlb" raw_interfaces_only //, named_guids, no_namespace
#else
	#import "..\..\..\Release\Perigen.Patterns.NnetControls.tlb" raw_interfaces_only //, named_guids, no_namespace
#endif

using namespace Perigen_Patterns_NnetControls;
class PatternsChartCtrl;
struct EportBarActiveChunck
{
	EportBarActiveChunck(DATE from, DATE to, long id)
	{ m_from = from; m_to = to; m_intervalID = id;}
	DATE m_from;
	DATE m_to;
	long m_intervalID;
};
struct ExportEntriesStruct
{
	ExportEntriesStruct()
	{
		m_meanContructions = -1.0;
		m_meanBaseline = -1.0;
		m_meanBaselineVariability = -1.0;
		m_montevideo = -1.0;
	}
	double m_meanContructions;
	double m_meanBaseline;
	double m_meanBaselineVariability;
	double m_montevideo;
};

#define MOUSE_OVER_CALLBACK_MSG (WM_USER + 11)
#define MOUSE_LEAVE_CALLBACK_MSG (WM_USER + 12)
#define TIME_RANGE_CHANGE_CALLBAK_MSG (WM_USER + 13)
#define GET_EXPORT_ENTRIES_MSG (WM_USER + 14)
#define GET_EXPORTENTRIES_MSG (WM_USER + 15)

class NativeManager : public IPatternEvent
{
	//typedef void (PatternsChartCtrl::*callback_func_ptr)(DATE, DATE);
public:

	NativeManager(void);
	~NativeManager(void);
public:
	void Init(DATE startTime, double screenWidthInMinutes);
	void OnExportPanelMouseOver();

	HRESULT __stdcall QueryInterface(const IID &, void **);
    ULONG __stdcall AddRef(void) { return 1; }
    ULONG __stdcall Release(void) { return 1; }
	HRESULT __stdcall RaiseMouseOverEvent(DATE from, DATE to);
	HRESULT __stdcall RaiseMouseLeaveEvent(DATE from, DATE to);
    HRESULT __stdcall RaiseBtnPressedEvent(DATE from, DATE to, long id);
	HRESULT __stdcall RaiseExportDialogClosedEvent(DATE from, DATE to);
	HRESULT __stdcall RaiseTimeRangeChangedEvent(long timeRange);
	HRESULT __stdcall SetExportableChunks(DATE startTime);
	//HRESULT __stdcall GetExportDialogParams(long intervalID, IExportDialogParams** pRetVal);
	//HRESULT __stdcall GetExportDialogParams(long intervalID, BSTR* pRetVal);

	BSTR GetExportCalculatedEntries(DATE from, DATE to, long id);
	void GetExportCalculatedEntriesEx(DATE from, DATE to, long id, ExportEntriesStruct& entries);

	void InitNavigationPanel(long parentHandle, int x, int y, int width, int height);
	void ResizeNavigationPanel(int x, int y, int width, int height);

    void InitExportButton(long parentHandle, int x, int y, int width, int height);
	void ResizeExportButton(int x, int y, int width, int height);

	void HideControls();
	void SetMontevideoVisible(bool isPressed);

	
	bool IsInitialized() const
	{return m_bInitialized;}
	void BeginUpdateChunks();
	void AddChunk(int exportID, int intervalID, DATE startTime, int timeRange, bool isExported, long x1, long x2);
	void EndUpdateChunks();
	void SetStartTime(DATE startTime);
	void SetTimeRange(int timeRange);
	void SetPluginURL(const CString& serverURL);
	void SetInitParams(const CString& userID, int episodeID, bool canModify, HWND m_criHWND);
	void SetPanelTooltip(const CString  message, DATE end36Weeks);
public:
	
	void SetPatternsControl(HWND hWnd)
	{m_patternsCtrl = hWnd;}
protected:
	
private:
	
	IPatternsWPFAdapterPtr m_pPatternsAdapter;
	DATE m_startTime;
	double m_screenWidthInMinutes;
	bool m_bInitialized;
	HWND m_patternsCtrl;
};


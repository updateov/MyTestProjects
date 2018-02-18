#pragma once

//using namespace Perigen_Patterns_NnetControls;

class NativeManager : public IPatternEvent
{
public:

	NativeManager(void);
	~NativeManager(void);

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

	void InitNavigationPanel(long parentHandle, int x, int y, int width, int height);
	void ResizeNavigationPanel(int x, int y, int width, int height);

    void InitExportButton(long parentHandle, int x, int y, int width, int height);
	void ResizeExportButton(int x, int y, int width, int height);
	void SetTimeRange(int timeRange);

	void HideControls();

	DATE GetStartTime();
private:

	IPatternsWPFAdapterPtr m_pPatternsAdapter;
	DATE m_startTime;
};


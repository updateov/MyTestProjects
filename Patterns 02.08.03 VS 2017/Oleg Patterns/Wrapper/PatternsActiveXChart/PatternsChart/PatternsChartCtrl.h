//#pragma once
#include "afxctl.h"
#include <iostream>
#include <fstream>
#include <objsafe.h>
#include <vector>
#include "tinyxml.h"
#include "popup message.h"
#include "icon_button.h"
#include "patterns, conductor.h"
#include "Patterns gui, tracing.h"
#include "patterns, fetus.h"
#include "utils.h"
#include "PatternsExportableChunkNative.h"
#include "..\..\..\Source\Pattern Detection\ThreadLock.h"
#include "PatternsChart.h"
#include "TracingsContainerWnd.h"

class NativeManager;
class PatternsChartCtrl :	public COleControl
{	
public:
	PatternsChartCtrl(void);
	virtual ~PatternsChartCtrl(void);
	// Overrides
public:
	virtual int OnCreate(LPCREATESTRUCT);
	virtual void OnDestroy();
	virtual void OnSize(UINT, int, int);
	virtual void OnDraw(CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid);
	virtual afx_msg BOOL OnEraseBkgnd(CDC *);
	
	virtual DWORD GetControlFlags();
	afx_msg void OnTimer(UINT_PTR);

	// Implementation
protected:	



	// Message maps
	DECLARE_MESSAGE_MAP()
	long OnForceUpdateAdapterMessage(WPARAM wParam, LPARAM lPararm);
	long OnExportBarMouseOverCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnExportBarMouseLeaveCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnShowExportBarMsg(WPARAM wParam, LPARAM lParam);
	long OnTimeRangeChangeCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnGetExportCalculatedEntriesEx(WPARAM wParam, LPARAM lParam);
	long OnSwitch15Min3MinViewsMsg(WPARAM wParam, LPARAM lParam);
	// Dispatch and event IDs
public:
	enum {
		dispidConnectionData = 1
	};

	static void initialize_bitmaps();
	static void release_bitmaps();


protected:

	enum data_status_code { data_status_no_data = 0, data_status_connected = 1, data_status_error = 2, data_status_recovery = 3};

	virtual void OnConnectionDataChanged(void) = 0;
	virtual void ClearRequest(void);
	virtual void ClearPatient(void);

	// The Configurations items
	CString m_ConnectionData;
	CString m_ServerURL;
	CString m_UserID;
	CString m_UserName;
	CString m_Permissions;
	CString m_patientID;
	HWND m_criHWND;


	int m_cr_stage2;
	int m_RefreshDelay;
	int m_Disclaimer;
	int m_Banner;
	int m_DemoMode;

	data_status_code m_DataStatus;
	string m_DataStatusDetails;

	// For security in threading
	patterns::ThreadLock m_DataLock;

	// Initialization properly done?
	bool m_Initialized;

	// Indicate that the control is in critical error...
	bool m_CriticalError;

	// In order to reduce frequency of download when the server is out of reach
	int m_skip_download;

	// The tracing control
	patterns_gui::tracing* m_Tracing;
	patterns_gui::tracing* m_Tracing30MinView;
	patterns_gui::tracing* m_Tracing15MinView;


	CWnd m_exportBar;
	CWnd m_exportButton;
	CEmptyBarWnd m_emptyBar;
	CEmptyBarWnd m_exportContainer;
	bool m_exportOpen;
	TracingsContainerWnd m_tracingContainer;

	// For message when resolution is too low or any other problem
	popup_message* m_Message;

	// For periodic refresh
	UINT m_RefreshTimer;

	///////////////////////////////////////////////////////////////////////////////////////
	/// Data functions
	patterns::conductor* m_conductor;
	patterns::fetus* get_fetus() {return m_conductor->get_fetus("KEY");}
	input_adapter::patient* get_patient() {return m_conductor->get_input_adapter_ptr()->get_patient("KEY");}

	TiXmlDocument m_RequestHeader;

	virtual void download_patient_data();
	virtual void process_patient_data(TiXmlElement* rootNode) = 0;
	virtual void perform_server_user_action(int actiontype, long artifact, string patient);
	virtual void PerformUserPluginAction(string element, long value, int actiontype);
	///////////////////////////////////////////////////////////////////////////////////////
	/// Utils
	virtual void log_exception(string msg, exception& e);
	virtual void log_exception(string msg);
	virtual void log_error(string msg);

	static void trace(WORD type, LPCTSTR szFormat, ...);

	///////////////////////////////////////////////////////////////////////////////////////
	/// COMMAND BAR
	map<string, icon_button*> m_buttons;
	icon_button* get_button(string name);
	void set_button(string name, icon_button* button);

	virtual void set_data_status(data_status_code status, string message);

	virtual void afx_msg toggle_events();
	virtual void afx_msg toggle_toco();
	virtual void afx_msg toggle_baseline();
	virtual void afx_msg toggle_montevideo();

	bool bplayback;
	bool bplayforward;

	virtual afx_msg void display_data_status(void) = 0;

	virtual afx_msg void toggle_zoom(void);
	virtual afx_msg void pause(void);
	virtual afx_msg void play_back(void);
	virtual afx_msg void play_forward(void);
	virtual afx_msg void go_beginning(void);
	virtual afx_msg void go_end(void);
	virtual afx_msg void go_next_page(void);
	virtual afx_msg void go_previous_page(void);

	virtual afx_msg void on_strike_out_event(void) = 0;
	virtual afx_msg void on_strike_out_contraction(void) = 0;
	virtual afx_msg void on_accept_event(void) = 0;

	virtual void on_show_about(void) = 0;

	virtual void initialize_commandbar();

	virtual void CreateExportBar();
	void adjust_controls();
	void refresh_command_bar(CDC* pdc);
	void refresh_banner(CDC* pdc);
	void RefreshExportBarWnd(CDC* pdc);

	int calculate_required_tracing_width(int height, bool for30MinViews);
	int calculate_required_tracing_height(int width, bool for30MinView);

	bool display_patient_banner() {return m_Banner == 2;} // 2 --> GE 
	
// GDI Object for faster work
protected:
	CPen m_linePen;

// For using the MessageBoxTimeout function from user32.dll
protected:
	typedef int (__stdcall * Type_MessageBoxTimeoutA)(HWND, LPCSTR, LPCSTR, UINT, WORD, DWORD);
	HMODULE m_hUser32;
	Type_MessageBoxTimeoutA MessageBoxTimeoutA;
protected:
	//load resource strings
	virtual CString LoadIconTextZoom() = 0;
	virtual CString LoadIconTextBaselines() = 0;
	virtual CString LoadIconTextToco() = 0;
	virtual CString LoadIconTextEvents() = 0;
	virtual CString LoadIconTextMontevideo() = 0;
	virtual CString LoadIconTextAbout() = 0;
	virtual CString LoadTextAvailableSpaceTooSmall() = 0;
	virtual CString LoadTextAnErrorHappened() = 0;

public:
	void UpdatePatternsAdapter();
	void VisibleChunksToAdapter();
	void ShowExportBar(bool show);
protected:
	int FindIntervalByStartTime(time_t startTime);
	void LoadExportableIntervals(TiXmlElement* intervalsNode);
	void AddExportableInterval(long basetime, string data);
	void Zoom(bool zoom);
	void UpdateExportTimeRange(TiXmlElement* requestNode);
	int FindIntervalByID(long id);
	long GetNumberOfFetuses();	
	date GetValidGAStart();
	void ParseGA(string GA);
	void LoadEnabledPlugins();
	bool IsIntervalExported(long id);
	CString GetExportNotEnabledMessage(DATE& endTime);
protected:
	NativeManager* m_pPatternsAdapter;
	long m_tracingLeftInSeconds;
	long m_tracingRightInSeconds;
	vector<PatternsExportableChunkNative> m_exportableIntervals;
	int m_gaTotalDays;;
	int m_intervalDuration;
	CString m_GA;
	bool m_bExportEnabled;
	bool m_bPluginLoadError;
	bool m_bExportVisible;

	double m_patternsRatioCoefficient;
	int m_tracingViewSizeInMinutes;
	int m_compressedViewSizeInMinutes;

	double m_patterns30MinRatioCoefficient;
	double m_patterns15MinRatioCoefficient;

	bool m_bIs30MinView;
	bool m_lessThanMinimumSpace;

	CStringArray m_supportedPatternsVersions;
	CString m_currentPatternsVersion;
	int m_currentVersionIndex;
	bool m_isInitialRequest;
	bool m_isFirstRequest;
protected:
	void PrepareInitialRequestHeader();
	bool RetryInitialRequest();
	void DownloadData();
	void AddBuildVersionToSupportedVersions(const CString buildFileVersion);
protected:
	void Switch15Min30MinViews(bool externalCall = false, bool left15min = false);

	bool CreateTracing(patterns_gui::tracing* pTracing, bool isVisible, int sizeInMinutes);

	void AdjustSliderAfterResize(bool isLeftPartShown, long leftIndex);
	long OnSetSliderPosMsg(WPARAM wParam, LPARAM lParam);
	long OnGoToSelectedIntervalMsg(WPARAM wParam, LPARAM lParam);

	long OnSwitchToggledCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnViewZoomedCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnSwitchTo15MinViewMsg(WPARAM wParam, LPARAM lParam);
protected:
	
	int m_logo;

	void ShowAboutDialog(const CString& url, bool bCheckList, bool isCheckListApp, int timeout);
protected:
	void CreateExportContainer();
	void ResizeExportContainer();
	void ReplaceChildrenParent(bool bToContainer);
	void ToggleSwitch(bool right);
	void ShowMVU(bool bShow, bool bNotify);
protected:
	bool m_MVUButtonVisible;
};

//std::wstring utf8_to_wstr(const std::string &utf8);
//std::string wstr_to_str( const std::wstring& wstr );
//string& cleanString(string& str);

#pragma once


#include <iostream>
#include <fstream>
#include <objsafe.h>
#include <vector>
#include "..\..\Source\Patterns Application\tinyxml.h"
#include "..\..\Source\Patterns Application\popup message.h"
#include "..\..\Source\Patterns Application\icon_button.h"
#include "ExportWndMessages.h"
#include "patterns, conductor.h"
#include "Patterns gui, tracing.h"
#include "patterns, fetus.h"
#include "..\PatternsActiveXChart\PatternsChart\utils.h"
#include "..\PatternsActiveXChart\PatternsChart\PatternsExportableChunkNative.h"
#include "..\..\Source\Pattern Detection\ThreadLock.h"
#include "..\PatternsActiveXChart\PatternsChart\PatternsChart.h"
#include "base64.h"
#include <objsafe.h>
#include "InitializationWorkerThread.h"
#include "DataStatus.h"
#include "ConnectionWorkerThread.h"

class ExportNativeManager;

#define MSG_ABOUTBTN_CLICKED (WM_USER + 100)

#define MSG_STRIKEOUT_EVENT (WM_USER + 200)
#define MSG_ACCEPT_EVENT (WM_USER + 210)
#define MSG_STRIKEOUT_CONTRACTION (WM_USER + 220)

#define MSG_END_INITIALIZATION (WM_USER + 900)
#define MSG_SWITCH_TOGGLED_MSG (WM_USER + 901)

#define MSG_MOVE_TO_INTERVAL_MSG (WM_USER + 902)
#define MSG_VIEW_ZOOMED (WM_USER + 903)

#define MSG_GET_STRIP_CURRENT_STATUS (WM_USER + 904)
#define MSG_SET_STRIP_STATUS (WM_USER + 905)

#define MSG_MVU_STATE_CHANGED (WM_USER + 906)

#define PATTERNS_CTL_CLASSNAME "PatternsControlWnd"
// ContainerBaseWnd
using namespace patterns;

struct ProductInformationStruct
{
	ProductInformationStruct()
	{
		m_logo = 0;
		m_checklistEnabled = false;
		m_brandMarkTM = false;
	}
	int m_logo;
	bool m_checklistEnabled;
	CString m_brandName;
	CString m_patternsName;
	bool m_brandMarkTM;
};

struct CtrlContextStruct
{
	CtrlContextStruct()
	{
		m_MVU = false;
		m_baselines = 0;
		m_events = false;
		m_zoom = false;
		m_toco = false;
		m_is15MinView = false;
		m_leftHalfShown = false;
		m_compressedStartTime = (DATE)0;
		m_expandedEndTime = (DATE)0;
		m_bScrollToEnd = false;
	}

	bool m_MVU;
	bool m_baselines;
	bool m_events;
	bool m_zoom;
	bool m_toco;
	DATE m_compressedStartTime;
	DATE m_expandedEndTime;
	bool m_is15MinView;
	bool m_leftHalfShown;
	bool m_bScrollToEnd;
};

class ContainerBaseWnd : public CWnd
{
	DECLARE_DYNAMIC(ContainerBaseWnd)

public:
	ContainerBaseWnd();
	virtual ~ContainerBaseWnd();
protected:
	BOOL RegisterWindowClass();
	bool CreateTracing(patterns_gui::tracing* pTracing, bool isVisible, int sizeInMinutes);
public:
	BOOL Create(LPCRECT rect, HWND hParent);
	virtual void SetInitialData(const CString& url, const CString& patientID,  const CString& userID, const CString& userName, const CString& permissions, bool exportSupported, const CString& version);
protected:
	DECLARE_MESSAGE_MAP()
	virtual int OnCreate(LPCREATESTRUCT);
	virtual void OnDestroy();
	virtual void OnSize(UINT, int, int);
	afx_msg void OnTimer(UINT_PTR);
	afx_msg void OnPaint();
	virtual afx_msg BOOL OnEraseBkgnd(CDC *);
/// export bar
	long OnForceUpdateAdapterMessage(WPARAM wParam, LPARAM lPararm);
	long OnExportBarMouseOverCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnExportBarMouseLeaveCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnShowExportBarMsg(WPARAM wParam, LPARAM lParam);
	long OnTimeRangeChangeCallbackMsg(WPARAM wParam, LPARAM lParam);
	long OnGetExportCalculatedEntriesEx(WPARAM wParam, LPARAM lParam);
///
	long OnInitialLoadCompletedMsg(WPARAM wParam, LPARAM lParam);
	long OnWorkerThreadCreation(WPARAM wParam, LPARAM lParam);
	long OnMoveToIntervalMsg(WPARAM wParam, LPARAM lParam);
	long OnSwitch15Min3MinViewsMsg(WPARAM wParam, LPARAM lParam);
	long OnSetSliderPosMsg(WPARAM wParam, LPARAM lParam);
	long OnGoToSelectedIntervalMsg(WPARAM wParam, LPARAM lParam);

	long OnRequestResponseMsg(WPARAM wParam, LPARAM lParam);
	long OnRequestFailureMsg(WPARAM wParam, LPARAM lParam);

	long OnGetStripCurrentStatus(WPARAM wParam, LPARAM lParam);
	long OnSetStripStatus(WPARAM wParam, LPARAM lParam);

	long OnToggleSwitchMsg(WPARAM wParam, LPARAM lParam);

	long OnGoToTime(WPARAM wParam, LPARAM lParam);

	long OnToggleSwitchNotification(WPARAM wParam, LPARAM lParam);

protected:
	virtual void SetDataToWorkerThread() {}
public:
	static void initialize_bitmaps();
	static void release_bitmaps();
protected:

	//enum data_status_code { data_status_no_data = 0, data_status_connected = 1, data_status_error = 2, data_status_recovery = 3};

	virtual void ClearRequest(void);
	virtual void ClearPatient(void);
protected:
	///////////////////////////////////////////////////////////////////////////////////////
	/// Data functions

	patterns::fetus* get_fetus() {return m_conductor->get_fetus("KEY");}
	input_adapter::patient* get_patient() {return m_conductor->get_input_adapter_ptr()->get_patient("KEY");}

	virtual void download_patient_data();
	//virtual void download_demo_data();
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

	virtual void set_data_status(DataStatus::data_status_code status, string message);

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

	void on_show_about(void);

	virtual void initialize_commandbar();

	
	void adjust_controls();
	void refresh_command_bar(CDC* pdc);
	void refresh_banner(CDC* pdc);
	void RefreshExportBarWnd(CDC* pdc);

	int calculate_required_tracing_width(int height, bool for30MinView);
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
	virtual CString LoadIconTextZoom();
	virtual CString LoadIconTextBaselines();
	virtual CString LoadIconTextToco();
	virtual CString LoadIconTextEvents();
	virtual CString LoadIconTextMontevideo();
	virtual CString LoadIconTextAbout();
	virtual CString LoadTextAvailableSpaceTooSmall();
	virtual CString LoadTextAnErrorHappened();
public:
	static int get_timeout_dialog() 
	{
		if (s_TimeoutDialog > 0)
			return s_TimeoutDialog;
		return 24*60; // Disabled is one day timeout
	}
	static void set_timeout_dialog(int value) {s_TimeoutDialog = value;}

protected:
	static int s_TimeoutDialog;
protected:
		// The Configurations items
	//CString m_ConnectionData;
	CString m_ServerURL;
	CString m_UserID;
	CString m_UserName;
	CString m_Permissions;
	HWND m_criHWND;
	bool m_exportSupported;


	int m_cr_stage2;
	int m_RefreshDelay;
	int m_Disclaimer;
	int m_Banner;
	int m_DemoMode;

	DataStatus::data_status_code m_DataStatus;
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

	// For message when resolution is too low or any other problem
	popup_message* m_Message;

	// For periodic refresh
	UINT m_RefreshTimer;

	patterns::conductor* m_conductor;

	TiXmlDocument m_RequestHeader;

	int m_lastMergeID;
protected: 
	virtual void StartInitializationThread() {}
	void StopInitializationThread();
	void DoOnInitializationEnd(bool bSucceed);
	void StartConnectionThread();
	void RequestData();
	void StopConnectionThread();
public:
	void ToggleSwitch(bool right, bool bExternalCall = false);
	bool IsLeftPartShown();

	bool GetProductInformation(ProductInformationStruct& productInfo);
public:
	//export bar
	void UpdatePatternsAdapter();
	void VisibleChunksToAdapter();
	void ShowExportBar(bool show);
	HWND GetExportBarWndHandle()
	{return IsWindow(m_exportBar.m_hWnd)? m_exportBar.m_hWnd : NULL;}
	HWND GetExportButtonWndHandle()
	{return IsWindow(m_exportButton.m_hWnd) ? m_exportButton.m_hWnd : NULL;}

	bool GetExportCalculatedEntries(DATE from, DATE to, long id, ExportEntriesCalcStruct& entries);
	bool GetContractionThresholdExceeded(DATE from, DATE to);
protected:
	//export bar
	int FindIntervalByStartTime(time_t startTime);
	virtual void CreateExportBar();
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
	///////export bar

	void LoadProductInformation();

	int GetRoundBaselineFHRValue() const
	{
		return m_roundBaselineFHRValue;
	}
protected:
	void UpdateRoundExportValue();
protected:
	void Switch15Min30MinViews(bool bNotify = true, bool externalCall = false, bool left15Min = false);
	void AdjustSliderAfterResize(bool isLeftPartShown, long leftIndex);
public:
	void SwitchViews(bool to30Min, bool externalCall = false);
	bool Is30MinView();
protected:
	ExportNativeManager* m_pPatternsAdapter;
	long m_tracingLeftInSeconds;
	long m_tracingRightInSeconds;
	vector<PatternsExportableChunkNative> m_exportableIntervals;
	int m_gaTotalDays;;
	int m_intervalDuration;
	CString m_GA;
	bool m_bExportEnabled;
	bool m_bPluginLoadError;
	bool m_bExportVisible;
	CString m_patientID;
	double m_patternsRatioCoefficient;
	int m_tracingViewSizeInMinutes;
	int m_compressedViewSizeInMinutes;

	double m_patterns30MinRatioCoefficient;
	double m_patterns15MinRatioCoefficient;

	bool m_bIs30MinView;
	bool m_lessThanMinimumSpace;

	InitializationWorkerThread* m_pWorkerThread;
	HWND m_hThreadWnd;
	bool m_bInitialLoadCompleted;

	CString m_str_strikeout_by_label;
	CString m_str_restore_by_label;
	CString m_str_confirm_by_label;
	CString m_str_action_performed_at;

	CString m_patternsVersion;

	CString m_brandName;
	bool m_brandMarkTM;
	int m_logo;
	CString m_patternsAppName;
	CString m_checklistAppName;
	bool m_checklistEnabled;

	CStringArray m_supportedPatternsVersions;
	CString m_currentPatternsVersion;
	int m_currentVersionIndex;
	bool m_isInitialRequest;
	bool m_isFirstRequest;

	ConnectionWorkerThread* m_pConnectionThread;
	bool m_connectionThreadInitialized;

	int m_roundBaselineFHRValue;
	int m_roundByMontevideoUnits;
protected:
	void PrepareInitialRequestHeader();
	bool RetryInitialRequest();
	bool ProcessResponse(string response);
	void AddBuildVersionToSupportedVersions(const CString buildFileVersion);
public:
	void SwitchToLeft15Min();
	void SwitchToRight15Min();
public:
	void ShowMVU(bool bShow, bool bNotify, bool bRaiseEvent);

protected:
	bool m_MVUButtonVisible;
};



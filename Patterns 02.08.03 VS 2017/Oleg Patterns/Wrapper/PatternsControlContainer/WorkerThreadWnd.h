#pragma once
#include "afxwin.h"
#include "..\..\Source\Patterns Application\tinyxml.h"
#include "Patterns gui, tracing.h"
#include "patterns, fetus.h"
#include "patterns, crifetus.h"
#include "Patterns gui, cri tracing.h"
#include "Patterns gui, services.h"
#include "..\PatternsActiveXChart\PatternsChart\utils.h"
#include "..\PatternsActiveXChart\PatternsChart\PatternsExportableChunkNative.h"
#include "ExportWndMessages.h"
#include "patterns, conductor.h"
#include "patterns gui, cri viewer input adapter.h"
#include "patterns, criconductor.h"
#include "..\..\Source\Pattern Detection\ThreadLock.h"
#include "..\PatternsActiveXChart\PatternsChart\PatternsChart.h"
#include "base64.h"
#include <objsafe.h>
#include "DataStatus.h"

#define MSG_END_INITIAL_REQUEST (WM_USER + 501)
#define MSG_INITIALIZATION (WM_USER + 502)
#define MSG_SET_INITIAL_DATA (WM_USER + 503)
#define MSG_CREATE_THREAD_WND (WM_USER + 504)
#define MSG_END_CREATE_THREAD_WND (WM_USER + 505)
using namespace patterns;
using namespace patterns_gui;



class BaseControlData
{
public:
	BaseControlData(const CString& url, const CString& patientID, TiXmlDocument* pRequestHeader,
		const CString& permissions, int* disclaimer, int* banner, bool* criticalError, int* gaTotalDays,
		int* demoMode, int* intervalDuration, CString* ga,
		vector<PatternsExportableChunkNative>* intervals,
		bool exportEnabled, bool displayBanner, HWND control,
		const CString& str_strikeout_by_label,
		const CString& str_restore_by_label,
		const CString& str_confirm_by_label,
		const CString& str_action_performed_at,
		bool* initialRequest, bool* firstRequest,
		CStringArray* pSupportedPatternsVersions,
		CString* currentPatternsVersion,
		int* pCurrentVersionIndex,
		const CString& userID)
	{
		m_ServerURL = url;
		m_patientID = patientID;
		m_pRequestHeader = pRequestHeader;
		m_Permissions = permissions;

		m_Disclaimer = disclaimer;
		m_Banner = banner;
		m_pCriticalError = criticalError;
		m_gaTotalDays = gaTotalDays;
		m_DemoMode = demoMode;
		m_intervalDuration = intervalDuration;
		m_GA = ga;
		m_exportableIntervals = intervals;
		m_bExportEnabled = exportEnabled;
		m_displayPatientBanner = displayBanner;
		m_controlWnd = control;
		m_strikeout_by_label = str_strikeout_by_label;
		m_restore_by_label = str_restore_by_label;
		m_confirm_by_label = str_confirm_by_label;
		m_action_performed_at = str_action_performed_at;

		m_pisInitialRequest = initialRequest;
		m_pisFirstRequest = firstRequest;
		
		m_pSupportedPatternsVersions = pSupportedPatternsVersions;
		m_currentPatternsVersion = currentPatternsVersion;
		m_pCurrentVersionIndex = pCurrentVersionIndex;
		m_userID = userID;

	}
	virtual ~BaseControlData() {}
public:
	CString m_ServerURL;
	CString m_patientID;
	CString m_userID;
	TiXmlDocument* m_pRequestHeader;

	int* m_Disclaimer;
	int* m_Banner;
	int* m_DemoMode;

	bool* m_pCriticalError;

	int* m_gaTotalDays;;
	int* m_intervalDuration;
	CString* m_GA;
	vector<PatternsExportableChunkNative>* m_exportableIntervals;
	bool m_bExportEnabled;
	bool m_displayPatientBanner;
	CString m_Permissions;
	CString m_strikeout_by_label;
	CString m_restore_by_label;
	CString m_confirm_by_label;
	CString m_action_performed_at;


	HWND m_controlWnd;

	CStringArray* m_pSupportedPatternsVersions;
	CString* m_currentPatternsVersion;
	int* m_pCurrentVersionIndex;
	bool* m_pisInitialRequest;
	bool* m_pisFirstRequest;

};

class PatternsInitialData : public BaseControlData
{
public:
	PatternsInitialData(patterns::fetus* fetus,		
		input_adapter::patient* patient,
		const CString& url, const CString& patientID, TiXmlDocument* pRequestHeader,
		const CString& permissions, int* disclaimer, int* banner, bool* criticalError, int* gaTotalDays,
		int* demoMode, int* intervalDuration, CString* ga,
		vector<PatternsExportableChunkNative>* intervals,
		bool exportEnabled, bool displayBanner, HWND control,
		const CString& str_strikeout_by_label,
		const CString& str_restore_by_label,
		const CString& str_confirm_by_label,
		const CString& str_action_performed_at,
		bool* initialRequest, bool* firstRequest,
		CStringArray* pSupportedPatternsVersions,
		CString* currentPatternsVersion,
		int* pCurrentVersionIndex,
		const CString& userID) :
		BaseControlData(url, patientID, pRequestHeader,
			permissions, disclaimer, banner, criticalError, gaTotalDays,
			demoMode, intervalDuration, ga,
			intervals,
			exportEnabled, displayBanner, control,
			str_strikeout_by_label,
			str_restore_by_label,
			str_confirm_by_label,
			str_action_performed_at,
			initialRequest, firstRequest,
			 pSupportedPatternsVersions,
			 currentPatternsVersion,
			pCurrentVersionIndex,
			userID)

	{
		m_pFetus = fetus;		
		m_pPatient = patient;
	}
	~PatternsInitialData() {}
public:
	patterns::fetus* m_pFetus;	
	input_adapter::patient* m_pPatient;
	
};

class ChecklistInitialData : public BaseControlData
{
public:
	ChecklistInitialData(patterns::CRIFetus* fetus,		
		CRIInputAdapter::patient* patient,
		const CString& url, const CString& patientID, TiXmlDocument* pRequestHeader,
		const CString& permissions, int* disclaimer, int* banner, bool* criticalError, int* gaTotalDays,
		int* demoMode, int* intervalDuration, CString* ga,
		vector<PatternsExportableChunkNative>* intervals,
		bool exportEnabled, bool displayBanner, HWND control,
		const CString& str_strikeout_by_label,
		const CString& str_restore_by_label,
		const CString& str_confirm_by_label,
		const CString& str_action_performed_at,
		bool* initialRequest, bool* firstRequest,
		CStringArray* pSupportedPatternsVersions,
		CString* currentPatternsVersion,
		int* pCurrentVersionIndex,
		const CString& userID)
		:
		BaseControlData(url, patientID, pRequestHeader,
			permissions, disclaimer, banner, criticalError, gaTotalDays,
			demoMode, intervalDuration, ga,
			intervals,
			exportEnabled, displayBanner, control,
			str_strikeout_by_label,
			str_restore_by_label,
			str_confirm_by_label,
			str_action_performed_at, 
			initialRequest, firstRequest,
			pSupportedPatternsVersions,
			currentPatternsVersion,
			pCurrentVersionIndex,
			userID)
		
	{
		m_pFetus = fetus;		
		m_pPatient = patient;
	}
	virtual ~ChecklistInitialData() {}
protected:
public:
	patterns::CRIFetus* m_pFetus;	
	CRIInputAdapter::patient* m_pPatient;
};

class InitializationResult
{
public:
	InitializationResult()
	{
		m_bSucceed = false;
		m_status = DataStatus::data_status_no_data;
		m_statusDetails = "";
		m_readOnly = false;
		m_updateBanner = false;
		m_b30MinView = false;
		m_roundBaselineFHRValue = 1;
		m_roundByMontevideoUnits = 1;
		m_showMVU = false;
	}
	~InitializationResult() {}
public:
	CString m_error;
	bool m_bSucceed;
	DataStatus::data_status_code m_status;
	string m_statusDetails;
	bool m_readOnly;
	bool m_updateBanner;
	bool m_b30MinView;
	int m_roundBaselineFHRValue;
	int m_roundByMontevideoUnits;
	bool m_showMVU;

};

class WorkerThreadWnd :
	public CWnd
{
	DECLARE_DYNAMIC(WorkerThreadWnd);
public:
	WorkerThreadWnd();
	virtual ~WorkerThreadWnd();
public: 
	BOOL Create();
protected:
	DECLARE_MESSAGE_MAP()
	afx_msg void OnDestroy();
	long OnInitMsg(WPARAM wParam, LPARAM lParam);
	InitializationResult* DownloadData();
	virtual InitializationResult* LoadData(TiXmlElement* rootNode) { return NULL; };
	void ParseGA(string GA);
	void LoadExportableIntervals(TiXmlElement* intervalsNode);
	void AddExportableInterval(long basetime, string data);
	int FindIntervalByID(long id);
	int FindIntervalByStartTime(time_t startTime);
	void UpdateExportTimeRange(TiXmlElement* requestNode);
	void PostIntializationFailureMessage();

	void PrepareInitialRequestHeader();
	bool RetryInitialRequest(InitializationResult*& pResult);

protected:
	virtual LRESULT WindowProc(UINT message, WPARAM wParam, LPARAM lParam);
public:
	void SetControlData(BaseControlData* pData);
public:
	static void trace(WORD type, LPCTSTR szFormat, ...);
	void log_exception(string msg, exception& e);
	void log_exception(string msg);
	void log_error(string msg);
protected:
	static CString m_className;

	//CString m_ServerURL;
	//CString m_patientID;
	//TiXmlDocument* m_pRequestHeader;
	
	//int* m_Disclaimer;
	//int* m_Banner;
	//int* m_DemoMode;

	CString m_error;
	patterns::ThreadLock m_DataLock;
	//bool* m_pCriticalError;
	
	//int* m_gaTotalDays;;
	//int* m_intervalDuration;
	//CString* m_GA;
	//vector<PatternsExportableChunkNative>* m_exportableIntervals;
	//bool m_bExportEnabled;
	//bool m_displayPatientBanner;
	//CString m_Permissions;

	string m_statusDetails;
	int m_status;

	//CString m_strikeout_by_label;
	//CString m_restore_by_label;
	//CString m_confirm_by_label;
	//CString m_action_performed_at;
	
	
	//HWND m_controlWnd;
	BaseControlData* m_pData;

};

class ChecklistWorkerThreadWnd : public WorkerThreadWnd
{
	DECLARE_DYNAMIC(ChecklistWorkerThreadWnd);
public:
	ChecklistWorkerThreadWnd() {};
	virtual ~ChecklistWorkerThreadWnd() {}

protected:
	DECLARE_MESSAGE_MAP()
	long OnSetDataMsg(WPARAM wParam, LPARAM lParam);
protected:
	virtual InitializationResult*  LoadData(TiXmlElement* rootNode);
	void LoadCRIAlgorithmSettings(TiXmlElement* patientNode);
public:
	void SetControlData(ChecklistInitialData* pData);
protected:
	//patterns::CRIFetus* m_pFetus;
	
	//CRIInputAdapter::patient* m_pPatient;
	
};

class PatternsWorkerThreadWnd : public WorkerThreadWnd
{
	DECLARE_DYNAMIC(PatternsWorkerThreadWnd);
public:
	PatternsWorkerThreadWnd() {};
	virtual ~PatternsWorkerThreadWnd() {};

protected:
	DECLARE_MESSAGE_MAP()
	long OnSetDataMsg(WPARAM wParam, LPARAM lParam);
protected:
	virtual InitializationResult*  LoadData(TiXmlElement* rootNode);
public:
	void SetControlData(PatternsInitialData* pData);
protected:
	//patterns::fetus* m_pFetus;
	
	//input_adapter::patient* m_pPatient;
	
};


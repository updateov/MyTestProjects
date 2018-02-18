// ContainerBaseWnd.cpp : implementation file
//

#include "stdafx.h"

#include "ExportNativeManager.h"
#include "ContainerBaseWnd.h"
#include "base64.h"
#include "patterns gui, services.h"
#include "patterns gui, double buffer.h"
#include "..\..\Source\Patterns Application\PatternsVersionNumber.h"
#include "Resource.h"

using namespace patterns_gui;

// ContainerBaseWnd

int ContainerBaseWnd::s_TimeoutDialog = 1;

IMPLEMENT_DYNAMIC(ContainerBaseWnd, CWnd)
CWinApp theApp;
ContainerBaseWnd::ContainerBaseWnd()
{
	m_MVUButtonVisible = false;
	m_roundBaselineFHRValue = 1;
	m_roundByMontevideoUnits = 1;
	m_pConnectionThread = NULL;
	m_connectionThreadInitialized = false;

	m_currentVersionIndex = 0;
	m_isFirstRequest = true;
	m_isInitialRequest = true;
	m_supportedPatternsVersions.Add(PATTERNS_02_08_02);
	m_supportedPatternsVersions.Add(PATTERNS_02_08_01);
	//m_supportedPatternsVersions.Add(PATTERNS_02_08_00);
	m_supportedPatternsVersions.Add(PATTERNS_02_06_00);
	m_supportedPatternsVersions.Add(PATTERNS_DEV_VERSION);
	

	m_skip_download = 0;
	m_lessThanMinimumSpace = false;

	m_bIs30MinView = false;
	m_pWorkerThread = NULL;
	m_hThreadWnd = NULL;
	m_bInitialLoadCompleted = false;

	m_exportSupported = false;
	m_bExportVisible = false;
	m_bPluginLoadError = false;
	m_bExportEnabled = false;
	m_gaTotalDays = 0;
	m_intervalDuration = 30;
	m_tracingLeftInSeconds = 0;
	m_tracingRightInSeconds = 0;
	HRESULT hr = CoInitialize(NULL);

	m_pPatternsAdapter = NULL;

	m_hUser32 = LoadLibrary("user32.dll");
	MessageBoxTimeoutA = (Type_MessageBoxTimeoutA) GetProcAddress(m_hUser32, "MessageBoxTimeoutA");

	m_RefreshTimer = 0;

	m_Initialized = false;
	m_CriticalError = false;

	m_ServerURL = "";
	m_UserID = "";
	m_UserName = "";
	m_Permissions = "";
	//m_ConnectionData = "";
	m_criHWND = NULL;

	m_DataStatus = DataStatus::data_status_initialization;
	m_DataStatusDetails = "Initializing...";

	m_cr_stage2 = 0;

	m_Banner = 0;

	m_RefreshDelay = 4;

	m_Disclaimer = true;
	m_DemoMode = false;
	

	m_Tracing = NULL;

	m_Message = new popup_message();
	
	m_conductor = NULL;

	// Prepare GDI object
	m_linePen.CreatePen(PS_SOLID, 0, RGB(175, 175, 175));

	m_lastMergeID = -1;

	m_tracingViewSizeInMinutes = m_bIs30MinView? TRACINGS_VIEW_SIZE_30_MINUTES : TRACINGS_VIEW_SIZE_15_MINUTES;
	m_compressedViewSizeInMinutes = DEFAULT_COMPRESSED_VIEW_SIZE_MINUTES;

	m_patternsRatioCoefficient = patterns_ratio_coefficient(m_tracingViewSizeInMinutes, m_compressedViewSizeInMinutes);
	m_patterns15MinRatioCoefficient = patterns_ratio_coefficient(TRACINGS_VIEW_SIZE_15_MINUTES, m_compressedViewSizeInMinutes);
	m_patterns30MinRatioCoefficient = patterns_ratio_coefficient(TRACINGS_VIEW_SIZE_30_MINUTES, m_compressedViewSizeInMinutes);
	m_logo = 0;
	m_brandMarkTM = false;
	m_checklistEnabled = false;

}

ContainerBaseWnd::~ContainerBaseWnd()
{
	m_Initialized = false;

	if (m_hUser32 != NULL)
	{
		FreeLibrary(m_hUser32);
		m_hUser32 = NULL;
	}
	
	if (m_Tracing15MinView)
	{
		m_Tracing15MinView->set(NULL);
	}

	if (m_Tracing30MinView)
	{
		m_Tracing30MinView->set(NULL);
	}

	if (m_conductor != NULL)
	{
		delete m_conductor;
		m_conductor = NULL;
	}

	// Flush command buttons
	for (map<string, icon_button*>::iterator itr = m_buttons.begin(); itr != m_buttons.end(); ++itr)
	{
		delete itr->second;
	}
	m_buttons.clear();

	m_Tracing = NULL;

	if (m_Tracing15MinView != NULL)
	{
		delete m_Tracing15MinView;
		m_Tracing15MinView = NULL;
	}

	if (m_Tracing30MinView != NULL)
	{
		delete m_Tracing30MinView;
		m_Tracing30MinView = NULL;
	}
	

	if (m_Message != NULL)
	{
		delete m_Message;
		m_Message = NULL;
	}
}


BOOL ContainerBaseWnd::RegisterWindowClass()
{
	WNDCLASS wndcls;
	HINSTANCE hInst = AfxGetInstanceHandle();

	if (!(::GetClassInfo(hInst, PATTERNS_CTL_CLASSNAME, &wndcls)))
	{
		// otherwise we need to register a new class
		wndcls.style = CS_DBLCLKS | CS_HREDRAW | CS_VREDRAW;
		wndcls.lpfnWndProc = ::DefWindowProc;
		wndcls.cbClsExtra = wndcls.cbWndExtra = 0;
		wndcls.hInstance = hInst;
		wndcls.hIcon = NULL;
		wndcls.hCursor = AfxGetApp()->LoadStandardCursor(IDC_ARROW);
		wndcls.hbrBackground = (HBRUSH)(COLOR_3DFACE + 1);
		wndcls.lpszMenuName = NULL;
		wndcls.lpszClassName = PATTERNS_CTL_CLASSNAME;

		if (!AfxRegisterClass(&wndcls))
		{
			AfxThrowResourceException();
			return FALSE;
		}
	}

	return TRUE;
}


BOOL ContainerBaseWnd::Create(LPCRECT rect, HWND hParent)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	RegisterWindowClass();
	CWnd* pParent = CWnd::FromHandle(hParent);
	if(pParent != NULL)
	{
		CRect clientRect;
		pParent->GetClientRect(clientRect);
		pParent->ShowWindow(SW_SHOW);
		CRect ctrlRect(rect);
		if (CWnd::Create(PATTERNS_CTL_CLASSNAME, NULL, WS_CHILD | WS_VISIBLE, ctrlRect, pParent, 0))
		{
			
			
			m_str_strikeout_by_label.LoadString(strikeout_by_label);			
			m_str_restore_by_label.LoadString(restore_by_label);
			m_str_confirm_by_label.LoadString(confirm_by_label);
			m_str_action_performed_at.LoadString(action_performed_at);
			
			StartInitializationThread();
			return TRUE;
		}
	}
	return FALSE;
}


void ContainerBaseWnd::SetInitialData(const CString& url, const CString& patientID,  const CString& userID, const CString& userName, const CString& permissions, bool exportSupported, const CString& version)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	m_criHWND = NULL;
	m_patientID = patientID;

	m_exportSupported = exportSupported;
	m_ServerURL = url;
	if (!m_ServerURL.IsEmpty())
	{
		int length = m_ServerURL.GetLength();
		if (m_ServerURL[length - 1] != '/')
			m_ServerURL += '/';
	}

	m_patternsVersion = version;
	if (!m_patternsVersion.IsEmpty())
	{
		m_supportedPatternsVersions.RemoveAll();
		m_supportedPatternsVersions.Add(m_patternsVersion);
		m_currentPatternsVersion = m_patternsVersion;
		m_currentVersionIndex = 0;
	}

	m_UserID = userID;
	m_UserName = userName;
	m_Permissions = permissions;
	m_RefreshDelay = 5;

	m_cr_stage2 = 30;
	m_Banner = 0;


	int timeout = 1;
	
	set_timeout_dialog(timeout);

}

BEGIN_MESSAGE_MAP(ContainerBaseWnd, CWnd)
	ON_WM_PAINT()
	ON_WM_TIMER()
	ON_WM_SIZE()
	ON_WM_ERASEBKGND()
	ON_WM_CREATE()
	ON_WM_DESTROY()

	ON_BN_CLICKED(15, go_beginning)
	ON_BN_CLICKED(45, go_previous_page)
	ON_BN_CLICKED(35, play_back)
	ON_BN_CLICKED(30, pause)
	ON_BN_CLICKED(40, play_forward)
	ON_BN_CLICKED(25, go_next_page)
	ON_BN_CLICKED(20, go_end)

	ON_BN_CLICKED(100, toggle_events)
	ON_BN_CLICKED(105, toggle_montevideo)
	ON_BN_CLICKED(103, toggle_baseline)
	ON_BN_CLICKED(102, toggle_toco)
	ON_BN_CLICKED(61, toggle_zoom)
	ON_BN_CLICKED(63, display_data_status)
	ON_BN_CLICKED(5, on_show_about)

	ON_CONTROL(patterns_gui::tracing::ndeleteevent, 200, on_strike_out_event)
	ON_CONTROL(patterns_gui::tracing::ndeletecontraction, 200, on_strike_out_contraction)
	ON_CONTROL(patterns_gui::tracing::nacceptevent, 200, on_accept_event)
	ON_MESSAGE(FORCE_UPDATE_ADAPTER_MSG, OnForceUpdateAdapterMessage)
	ON_MESSAGE(MOUSE_OVER_CALLBACK_MSG, OnExportBarMouseOverCallbackMsg)
	ON_MESSAGE(MOUSE_LEAVE_CALLBACK_MSG, OnExportBarMouseLeaveCallbackMsg)
	ON_MESSAGE(SHOW_EXPORT_MSG, OnShowExportBarMsg)
	ON_MESSAGE(TIME_RANGE_CHANGE_CALLBAK_MSG, OnTimeRangeChangeCallbackMsg)
	ON_MESSAGE(GET_EXPORTENTRIES_MSG, OnGetExportCalculatedEntriesEx)
	ON_MESSAGE(MSG_END_INITIAL_REQUEST, OnInitialLoadCompletedMsg)
	ON_MESSAGE(MSG_END_CREATE_THREAD_WND, OnWorkerThreadCreation)
	ON_MESSAGE(MSG_MOVE_TO_INTERVAL_MSG, OnMoveToIntervalMsg)
	ON_MESSAGE(SWITCH_15MIN_30MIN_VIEWS_MSG, OnSwitch15Min3MinViewsMsg)
	ON_MESSAGE(MSG_SETSLIDERPOSITION, OnSetSliderPosMsg)
	ON_MESSAGE(MSG_REQUEST_RESPONSE, OnRequestResponseMsg)
	ON_MESSAGE(MSG_REQUEST_FAILURE, OnRequestFailureMsg)
	ON_MESSAGE(MSG_GET_STRIP_CURRENT_STATUS, OnGetStripCurrentStatus)
	ON_MESSAGE(MSG_SET_STRIP_STATUS, OnSetStripStatus)
	ON_MESSAGE(MSG_GOTO_SELECTEDINTERVAL, OnGoToSelectedIntervalMsg)
	ON_MESSAGE(TOGGLE_SWITCH_MSG, OnToggleSwitchMsg)
	ON_MESSAGE(MSG_GOTO_TIME, OnGoToTime)
	ON_MESSAGE(TOGGLE_SWITCH_NOTIFICATION_MSG, OnToggleSwitchNotification)
END_MESSAGE_MAP()



// ContainerBaseWnd message handlers


int ContainerBaseWnd::OnCreate(LPCREATESTRUCT s)
{
		// Prepare the data request header for that patient

	PrepareInitialRequestHeader();
	//m_RequestHeader.Clear();
	//CString version;
	//version.Format("<patients version=\"%s\"><request key=\"0\"/></patients>\r\n", FILEVERSTR);//m_patternsVersion);
	//m_RequestHeader.Parse(version);
	////m_RequestHeader.Parse("<patients version=\"" FILEVERSTR "\"><request key=\"0\"/></patients>\r\n");
	//m_RequestHeader.FirstChildElement("patients")->SetAttribute("user", m_UserID);
	//m_RequestHeader.FirstChild("patients")->FirstChildElement("request")->SetAttribute("key", m_patientID);

	// Check if we are in demo mode
	m_DemoMode = ((m_ServerURL.GetLength() >= 4) && (m_ServerURL.Left(4).CompareNoCase("data") == 0));
	bool isOldVersion = false;
	if (m_patternsVersion.GetLength() >= 5)
	{
		CString majorVersion = m_patternsVersion.Left(8);		
		if (majorVersion.CompareNoCase(PATTERNS_02_04_00) == 0)
		{
			isOldVersion = true;
		}
	}
	if (!isOldVersion)
	{
		LoadProductInformation();
	}
	m_Tracing = m_bIs30MinView ? m_Tracing30MinView : m_Tracing15MinView;
	LoadEnabledPlugins();


	//m_Initialized = true;

	if (CWnd::OnCreate(s) == -1)
		return -1;

	m_Tracing30MinView->SetLogo(m_logo);
	m_Tracing15MinView->SetLogo(m_logo);
	m_Tracing30MinView->SetExportEnabled(m_bExportEnabled);
	m_Tracing15MinView->SetExportEnabled(m_bExportEnabled);
	m_Message->Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	m_Message->set_format(DT_CENTER | DT_VCENTER | DT_WORDBREAK);

	CreateTracing(m_Tracing30MinView, m_bIs30MinView, TRACINGS_VIEW_SIZE_30_MINUTES);
	CreateTracing(m_Tracing15MinView, !m_bIs30MinView, TRACINGS_VIEW_SIZE_15_MINUTES);
	
	//UINT vs15MinVisible = m_bIs30MinView ? 0 : WS_VISIBLE;
	//UINT vs30MinVisible = m_bIs30MinView ? WS_VISIBLE : 0;
	//m_Tracing15MinView->Create(NULL, "", WS_CHILD | WS_TABSTOP | vs15MinVisible | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	//m_Tracing30MinView->Create(NULL, "", WS_CHILD | WS_TABSTOP | vs15MinVisible | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	//m_Tracing15MinView->set_type(tracing::tnormal);
	//m_Tracing15MinView->set_paper(tracing::pusa);
	//m_Tracing15MinView->set_scaling_mode(tracing::spaper);
	//m_Tracing15MinView->show_grid();
	//m_Tracing15MinView->lock_scaling();

	//m_Tracing30MinView->set_type(tracing::tnormal);
	//m_Tracing30MinView->set_paper(tracing::pusa);
	//m_Tracing30MinView->set_scaling_mode(tracing::spaper);
	//m_Tracing30MinView->show_grid();
	//m_Tracing30MinView->lock_scaling();

	//m_Tracing15MinView->set_lengths(TRACINGS_VIEW_SIZE_15_MINUTES * 60, m_compressedViewSizeInMinutes * 60, SLIDER_VIEW_SIZE_SEC);
	//m_Tracing30MinView->set_lengths(TRACINGS_VIEW_SIZE_30_MINUTES * 60, m_compressedViewSizeInMinutes * 60, SLIDER_VIEW_SIZE_SEC);
	//
	//m_Tracing15MinView->show(tracing::winformation);
	//m_Tracing15MinView->show(tracing::whidedisconnected, false);
	//m_Tracing15MinView->show(tracing::wbaselinevariability, true);
	//m_Tracing15MinView->show(tracing::wcrtracing, true);

	m_Tracing30MinView->show(tracing::winformation);
	m_Tracing30MinView->show(tracing::whidedisconnected, false);
	m_Tracing30MinView->show(tracing::wbaselinevariability, true);
	m_Tracing30MinView->show(tracing::wcrtracing, true);

	CreateExportBar();
	initialize_commandbar();

	//// First download of data
	//download_patient_data();

	//// Display the patient
	m_Tracing15MinView->set(get_fetus());
	m_Tracing30MinView->set(get_fetus());
	
	m_Tracing15MinView->zoom(false);
	m_Tracing30MinView->zoom(false);

	m_Tracing->move_to(get_fetus()->get_number_of_fhr() + 1000000);
	m_Tracing->scroll(true);

	//// Reset the timer for refresh
	//if (m_RefreshTimer != 0)
	//{
	//	KillTimer(m_RefreshTimer);
	//	m_RefreshTimer = 0;
	//}

	//if (::IsWindow(m_hWnd))
	//{
	//	m_RefreshTimer = SetTimer(1, m_RefreshDelay * 1000, NULL);
	//	InvalidateRgn(0, FALSE);
	//}

	//m_Initialized = true;

	return 0;
}

bool ContainerBaseWnd::CreateTracing(patterns_gui::tracing* pTracing, bool isVisible, int sizeInMinutes)
{
	UINT vsVisible = isVisible ? 0 : WS_VISIBLE;
	if (pTracing->Create(NULL, "", WS_CHILD | WS_TABSTOP | vsVisible | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200))
	{
		if (!isVisible)
			pTracing->EnableWindow(FALSE);
		pTracing->set_type(CRITracing::tnormal);
		pTracing->set_paper(CRITracing::pusa);
		pTracing->set_scaling_mode(CRITracing::spaper);
		pTracing->show_grid();
		pTracing->lock_scaling();
		pTracing->set_lengths(sizeInMinutes * 60, m_compressedViewSizeInMinutes * 60, SLIDER_VIEW_SIZE_SEC);
		pTracing->show(CRITracing::winformation);
		pTracing->show(CRITracing::whidedisconnected, false);
		pTracing->show(CRITracing::wbaselinevariability, true);
		pTracing->show(CRITracing::wcrtracing, true);
		return true;
	}
	return false;

}
// Cleanup
void ContainerBaseWnd::OnDestroy()
{	
	StopInitializationThread();
	if (m_RefreshTimer != 0)
	{ 
		KillTimer(m_RefreshTimer);
		m_RefreshTimer = 0;
	}
	if(m_pPatternsAdapter)
	{
		delete m_pPatternsAdapter;
		m_pPatternsAdapter = NULL;
		m_exportableIntervals.clear();
	}
	StopConnectionThread();
	CWnd::OnDestroy();
}

void ContainerBaseWnd::RequestData()
{
	if (m_pConnectionThread != NULL && m_connectionThreadInitialized)
	{
		m_pConnectionThread->SendRequest(m_RequestHeader);
	}
	else
	{
	}
}

// Get the data / updates from the server and refresh the display
void ContainerBaseWnd::download_patient_data()
{	
	// We don't come back from a critical error
	if (m_CriticalError)
		return;

	//// Not initialized yet
	if (m_ServerURL.IsEmpty() || m_patientID.IsEmpty())
		return;

	// If there was recently an error talking to patterns server, skip the next xx call to reduce error count
	if (m_skip_download > 0)
	{
		--m_skip_download;
		return;
	}

	RequestData();



}
long ContainerBaseWnd::OnRequestFailureMsg(WPARAM wParam, LPARAM lParam)
{
	if (lParam != NULL)
	{
		CString* pError = (CString*)lParam;
		CString error = *pError;
		delete pError;
		if (!error.IsEmpty())
		{
			string errorStr = (LPCSTR)error;
			log_error(errorStr);
		}
		set_data_status(DataStatus::data_status_error, "");
	}

	return 0;
}

long ContainerBaseWnd::OnRequestResponseMsg(WPARAM wParam, LPARAM lParam)
{
	try
	{
		if (lParam == NULL)
			throw new exception("Invalid data format, unable to parse it as a proper xml");

		string* pResponse = (string*)lParam;
		string response = *pResponse;
		delete pResponse;

		ReplyMessage(1);

	if (!m_DataLock.try_acquire())
		return 0;

		ProcessResponse(response);


	}
	catch (exception& e)
	{
		log_exception("Error while requesting server data", e);
	}
	catch (...)
	{
		log_exception("Unknown error while requesting server data");
	}

	m_DataLock.release();
	return 0;
}

bool ContainerBaseWnd::ProcessResponse(string response)
{
	// Request data from the server
	//string response = utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("data"), utils::to_string(m_RequestHeader), "PATTERNS_CONTROL") + "\r\n";

	// Failed?
	if (response.length() == 2) // 2 because of the + "\r\n"
	{
		m_skip_download = 4;

		ClearPatient();
		ClearRequest();

		set_data_status(DataStatus::data_status_error, "Unable to reach the server");
		return false;
	}
	m_skip_download = 0;
	// Parse the answer
	TiXmlDocument xmlDoc;
	if (xmlDoc.Parse(response.c_str()) == 0)
		throw new exception("Invalid data format, unable to parse it as a proper xml");

	// Fetch the data
	TiXmlElement* rootNode = xmlDoc.RootElement();
	if (rootNode == NULL)
		throw new exception("Response is empty");

	// Check for invalid version
	int invalid_version = 0;
	rootNode->Attribute("invalid_version", &invalid_version);
	if (invalid_version != 0 && m_patternsVersion.IsEmpty())
	{
		if(RetryInitialRequest() && m_currentVersionIndex < m_supportedPatternsVersions.GetSize())
		{
			return true;
		}

	
	}
	// Process the response		
	process_patient_data(rootNode);
	return false;

}
///
/// Clean up the request to the bare minimum: the patient ID
///
void ContainerBaseWnd::ClearRequest()
{
	// In the request header, reset all attribute but the key
	TiXmlElement* patientsNode = m_RequestHeader.FirstChildElement("patients");
	if (patientsNode != NULL)
	{
		TiXmlElement* requestNode = patientsNode->FirstChildElement("request");
		if (requestNode != NULL)
		{
			// Keep only the "key" attribute from the request
			const TiXmlAttribute* pAttribute = requestNode->FirstAttribute();
			while (pAttribute != NULL)
			{
				CString name = pAttribute->Name();
				if (name.CompareNoCase("key") != 0)
				{
					requestNode->RemoveAttribute(name);
					pAttribute = requestNode->FirstAttribute();
				}
				else
				{
					pAttribute = pAttribute->Next();
				}
			}
		}
	}
}

///
/// Flush all current loaded patient data
///
void ContainerBaseWnd::ClearPatient()
{
	// Reset existing data
	patterns::fetus* f = get_fetus();
	if (f)
	{
		f->clear();
	}

	input_adapter::patient* p = get_patient();
	if (p)
	{
		p->reset();
	}
	
}


// Send to the server the request action and update the screen
void ContainerBaseWnd::perform_server_user_action(int actiontype, long artifact, string patient)
{
	try
	{
		TiXmlDocument xmlDoc;

		TiXmlElement* item = new TiXmlElement("action");
		item->SetAttribute("type", actiontype);
		item->SetAttribute("artifact", artifact);
		item->SetAttribute("patient", patient.c_str());
		item->SetAttribute("userid", m_UserID);
		item->SetAttribute("username", m_UserName);
		xmlDoc.LinkEndChild(item);

		utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("useraction"), utils::to_string(xmlDoc), "PATTERNS_CONTROL");

		download_patient_data();
	}
	catch (exception& e)
	{
		log_exception("Error while sending a user action to the server", e);
	}
	catch (...)
	{
		log_exception("Unknown error while sending a user action to the server");
	}
}

/////////////////////////////////////////////////////////////////////
/// COMMAND BAR

void ContainerBaseWnd::set_button(string name, icon_button* button)
{
	m_buttons[name] = button;
}

// Retrieve a specific command bar button by its name
icon_button* ContainerBaseWnd::get_button(string name)
{
	map<string, icon_button*>::iterator i = m_buttons.find(name);
	if (i == m_buttons.end())
	{
		icon_button* button = new icon_button();
		set_button(name, button);
		return button;
	}
	return i->second;
}

// Prepare all bitmaps
void ContainerBaseWnd::initialize_bitmaps()
{
	// Security. Check if already loaded
	if (services::is_bitmap("leftmost"))
		return;

#ifdef _DEBUG
	trace(EVENTLOG_INFORMATION_TYPE, "Intialization of the bitmaps");
#endif

	CRect r;

	r = services::get_bitmap_rectangle("VCR_BUTTONS");
	services::create_bitmap_fragment("VCR_BUTTONS", "leftmost", CRect(0, 0, 18, r.Height()));
	services::create_bitmap_fragment("VCR_BUTTONS", "previouspage", CRect(18, 0, 36, r.Height()));
	services::create_bitmap_fragment("VCR_BUTTONS", "left", CRect(36, 0, 54, r.Height()));
	services::create_bitmap_fragment("VCR_BUTTONS", "pause", CRect(54, 0, 72, r.Height()));
	services::create_bitmap_fragment("VCR_BUTTONS", "right", CRect(72, 0, 90, r.Height()));
	services::create_bitmap_fragment("VCR_BUTTONS", "nextpage", CRect(90, 0, 108, r.Height()));
	services::create_bitmap_fragment("VCR_BUTTONS", "rightmost", CRect(108, 0, 126, r.Height()));
	services::forget_bitmap("VCR_BUTTONS");

	r = services::get_bitmap_rectangle("DATA_STATUS");
	services::create_bitmap_fragment("DATA_STATUS", "connected",	CRect(0, 0,						r.Width(),	r.Height() / 5));
	services::create_bitmap_fragment("DATA_STATUS", "error",		CRect(0, r.Height() / 5,		r.Width(),	(2 * r.Height()) / 5));
	services::create_bitmap_fragment("DATA_STATUS", "recovery",		CRect(0, (2 * r.Height()) / 5,	r.Width(),	(3 * r.Height()) / 5));
	services::create_bitmap_fragment("DATA_STATUS", "no_data",		CRect(0, (3 * r.Height()) / 5,	r.Width(), (4 * r.Height()) / 5));
	services::create_bitmap_fragment("DATA_STATUS", "initialization", CRect(0, (4 * r.Height()) / 5, r.Width(), r.Height()));
	services::forget_bitmap("DATA_STATUS");

	r = services::get_bitmap_rectangle("NAVIGATIONBUTTONS");
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "zoom", CRect(r.Height(), 0, 2 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "about", CRect(4 * r.Height(), 0, 5 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "mask button", CRect(5 * r.Height(), 0, 6 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "action on", CRect(10 * r.Height(), 0, 11 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "action off", CRect(11 * r.Height(), 0, 12 * r.Height(), r.Height()));
	services::forget_bitmap("NAVIGATIONBUTTONS");

	tracing::create_bitmaps();
}
void ContainerBaseWnd::release_bitmaps()
{
#ifdef _DEBUG
	trace(EVENTLOG_INFORMATION_TYPE, "Releasing the bitmaps from memory");
#endif
	services::reset();
}

// Initialize the command bar by creating all the buttons
void ContainerBaseWnd::initialize_commandbar()
{
	// CREATE
	get_button("zoom")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 61);

	get_button("data status")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 63);

	get_button("go to beginning")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 15);
	get_button("page left")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 45);
	get_button("play left")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 35);
	get_button("pause")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 30);
	get_button("play right")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 40);
	get_button("page right")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 25);
	get_button("go to end")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 20);

	m_exportButton.Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 0);

	get_button("baseline")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 103);
	get_button("toco")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 102);
	get_button("events")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 100);
	get_button("montevideo")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 105);

	get_button("about")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 5);

	// DECORATE
	CString s;

	//s.LoadString(icon_text_zoom);
	s = LoadIconTextZoom();
	get_button("zoom")->SetWindowText(s);
	get_button("zoom")->set_icon("zoom");
	get_button("zoom")->set_mask("mask button");

	set_data_status(m_DataStatus, m_DataStatusDetails);

	get_button("go to beginning")->set_icon("leftmost");
	get_button("page left")->set_icon("previouspage");
	get_button("play left")->set_icon("left");
	get_button("pause")->set_icon("pause");
	get_button("play right")->set_icon("right");
	get_button("page right")->set_icon("nextpage");
	get_button("go to end")->set_icon("rightmost");

	//s.LoadString(icon_text_baselines);
	s = LoadIconTextBaselines();
	get_button("baseline")->SetWindowText(s);
	get_button("baseline")->set_icon("action off");
	get_button("baseline")->set_mask("mask button");

	//s.LoadString(icon_text_toco);
	s = LoadIconTextToco();
	get_button("toco")->SetWindowText(s);
	get_button("toco")->set_icon("action on");
	get_button("toco")->set_mask("mask button");

	//s.LoadString(icon_text_events);
	s = LoadIconTextEvents();
	get_button("events")->SetWindowText(s);
	get_button("events")->set_icon("action on");
	get_button("events")->set_mask("mask button");

	//s.LoadString(icon_text_montevideo);
	s = LoadIconTextMontevideo();
	get_button("montevideo")->SetWindowText(s);
	get_button("montevideo")->set_icon("action off");
	get_button("montevideo")->set_mask("mask button");

	//s.LoadString(icon_text_about);
	s = LoadIconTextAbout();
	get_button("about")->SetWindowText(s);
	get_button("about")->set_icon("about");
	get_button("about")->set_mask("mask button");
}

// Toggle events display ON/OFF
void ContainerBaseWnd::toggle_events(void)
{
	bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::whideevents);
	m_Tracing30MinView->show(patterns_gui::CRITracing::whideevents, !isVisible);
	m_Tracing15MinView->show(patterns_gui::CRITracing::whideevents, !isVisible);
	Zoom(false);

	get_button("events")->set_icon((!m_Tracing->is_visible(patterns_gui::tracing::whideevents))?"action on":"action off");
}

// Toggle Hyperstimulation display Large/Small
void ContainerBaseWnd::toggle_toco(void)
{
	bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::wcrtracing);

	m_Tracing30MinView->show(patterns_gui::CRITracing::wcrtracing, !isVisible);
	m_Tracing15MinView->show(patterns_gui::CRITracing::wcrtracing, !isVisible);
	Zoom(false);

	get_button("toco")->set_icon((m_Tracing->is_visible(patterns_gui::tracing::wcrtracing))?"action on":"action off");
}

// Toggle Baseline display ON/OFF
void ContainerBaseWnd::toggle_baseline(void)
{
	bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::wbaselines);
	
	m_Tracing30MinView->show(patterns_gui::CRITracing::wbaselines, !isVisible);
	m_Tracing15MinView->show(patterns_gui::CRITracing::wbaselines, !isVisible);

	Zoom(false);
	get_button("baseline")->set_icon((m_Tracing->is_visible(patterns_gui::tracing::wbaselines))?"action on":"action off");
}

// Toggle Montevideo display ON/OFF
void ContainerBaseWnd::toggle_montevideo(void)
{
	bool wasVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
	ShowMVU(!wasVisible, true, true);
	//m_Tracing30MinView->show(patterns_gui::CRITracing::wmontevideo, !wasVisible);
	//m_Tracing15MinView->show(patterns_gui::CRITracing::wmontevideo, !wasVisible);
	//
	//Zoom(false);
	//bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
	//get_button("montevideo")->set_icon((isVisible)?"action on":"action off");
	//perform_server_user_action(isVisible ? eMVUButtonOn : eMVUButtonOff, MVUButtonDummyArtifactID, get_patient()->get_id());
	//if(m_pPatternsAdapter)
	//{
	//	m_pPatternsAdapter->SetMontevideoVisible(isVisible);
	//}
}

void ContainerBaseWnd::ShowMVU(bool bShow, bool bNotify, bool bRaiseEvent)
{
	bool wasVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
	bool isVisible = wasVisible;
	if (bShow != wasVisible)
	{
		m_Tracing30MinView->show(patterns_gui::tracing::wmontevideo, !wasVisible);
		m_Tracing15MinView->show(patterns_gui::tracing::wmontevideo, !wasVisible);

		//m_Tracing->zoom(false);
		Zoom(false);
		isVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
		m_MVUButtonVisible = isVisible;
		get_button("montevideo")->set_icon((isVisible) ? "action on" : "action off");
	}
	if(bNotify)
		perform_server_user_action(isVisible ? eMVUButtonOn : eMVUButtonOff, MVUButtonDummyArtifactID, get_patient()->get_id());
	if (m_pPatternsAdapter && bShow != wasVisible)
	{
		m_pPatternsAdapter->SetMontevideoVisible(isVisible, bRaiseEvent);
	}
	else if(bRaiseEvent)
	{
		GetParent()->SendMessage(MSG_MVU_STATE_CHANGED, bShow? 1 : 0, 0);
	}
}

// Tracing navigation. Go to the beginning of the tracing.
void ContainerBaseWnd::go_beginning(void)
{
	tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
	m_Tracing->move_to(0);
	pOther->move_to(0);
}

// Tracing navigation. Go to the end of the tracing.
void ContainerBaseWnd::go_end(void)
{
	tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
	m_Tracing->move_to(m_Tracing->get()->get_number_of_fhr() + 100);
	m_Tracing->scroll(true);
	pOther->move_to(m_Tracing->get()->get_number_of_fhr() + 100);
}

// Tracing navigation. Go to the next page A page represent the width of the view.
void ContainerBaseWnd::go_next_page(void)
{
	if (m_Tracing->is_animating())
	{
		m_Tracing->stop_animating();
	}

	m_Tracing->move_page_animating(false);
}

// Tracing navigation. Go to the previous page A page represent the width of the view.
void ContainerBaseWnd::go_previous_page(void)
{
	if (m_Tracing->is_animating())
	{
		m_Tracing->stop_animating();
	}

	m_Tracing->move_page_animating();
}

// Tracing navigation. play back.
void ContainerBaseWnd::play_back(void)
{
	if (m_Tracing->is_animating() && bplayforward)
	{
		m_Tracing->stop_animating();
		bplayforward = false;
	}

	if (m_Tracing->is_animating())
	{
		m_Tracing->set_animation_rate(m_Tracing->get_animation_rate() * 2);
	}
	else
	{
		m_Tracing->set_animation_rate(1);
		m_Tracing->move_to_animating(0);
	}

	bplayback = true;
}

// Tracing navigation. play forward.
void ContainerBaseWnd::play_forward(void)
{
	if (m_Tracing->is_animating() && bplayback)
	{
		m_Tracing->stop_animating();
		bplayback = false;
	}

	if (m_Tracing->is_animating())
	{
		m_Tracing->set_animation_rate(m_Tracing->get_animation_rate() * 2);
	}
	else
	{
		m_Tracing->set_animation_rate(1);
		m_Tracing->move_to_animating(m_Tracing->get()->get_number_of_fhr());
	}

	bplayforward = true;
}

// Pause
void ContainerBaseWnd::pause(void)
{
	m_Tracing->stop_animating();
}

// Zoom / unzoom
void ContainerBaseWnd::toggle_zoom(void)
{
	m_Tracing->zoom(!m_Tracing->is_zoomed());
	ShowExportBar(!m_Tracing->is_zoomed());
}

// Update the data connection status : live / disconnected / error
void ContainerBaseWnd::set_data_status(DataStatus::data_status_code status, string msg)
{
	DataStatus::data_status_code wasStatus = m_DataStatus;
	
	// Once critical, always critical!
	if (!m_CriticalError && !m_bPluginLoadError)
	{
		m_DataStatus = status;
		m_DataStatusDetails = msg;
	}
	bool wasConnected = (wasStatus == DataStatus::data_status_connected || wasStatus == DataStatus::data_status_recovery);
	bool isConnected = (m_DataStatus == DataStatus::data_status_connected || m_DataStatus == DataStatus::data_status_recovery);
	if (::IsWindow(m_hWnd))
	{
		switch (m_DataStatus)
		{
		case DataStatus::data_status_connected:
			get_button("data status")->set_icon("connected");
			break;

		case DataStatus::data_status_no_data:
			get_button("data status")->set_icon("no_data");
			break;

		case DataStatus::data_status_recovery:
			get_button("data status")->set_icon("recovery");
			break;
		case DataStatus::data_status_initialization:
			get_button("data status")->set_icon("initialization");
			break;
		case DataStatus::data_status_error:
		default:
			get_button("data status")->set_icon("error");
			break;
		}
		if(wasConnected != isConnected)
		{
			if(wasConnected)
			{
				ShowExportBar(false);
			}
			else 
			{
				ShowExportBar(true);
			}
		}
	}
}

// Adjust the size and position of the controls based on the current control size and settings
void ContainerBaseWnd::adjust_controls()
{
	m_lessThanMinimumSpace = false;
	CRect rect;
	GetClientRect(&rect);

	int cx = rect.Width();
	int cy = rect.Height();

	m_Tracing->zoom(false);
	m_lessThanMinimumSpace = false;
	int actualExportBarHeight = EXPORT_BAR_HEIGHT;
	int tracing_height = cy - ((display_patient_banner() ? BANNER_HEIGHT : 0) + BOTTOM_HEIGHT + actualExportBarHeight);
	
	int tracing_required_width = this->calculate_required_tracing_width(tracing_height, true);
	if (cx < patterns_minimum_width || tracing_required_width < patterns_minimum_width)
	{
		m_lessThanMinimumSpace = true;
		// Calculate proper minimum resolution for message
		int minimum_height = calculate_required_tracing_height(patterns_minimum_width, true);
		if (display_patient_banner())
		{
			minimum_height += BANNER_HEIGHT;
		}
		minimum_height += (BOTTOM_HEIGHT + actualExportBarHeight);


		CString msg;
		//msg.LoadString(available_space_too_small);
		msg = LoadTextAvailableSpaceTooSmall();

		CString t;
		t.Format(msg, patterns_minimum_width, minimum_height);
		m_Message->set_text(t.GetBuffer(t.GetLength()));

		m_Tracing->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
		if(m_bIs30MinView)
			m_Tracing15MinView->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
		else
			m_Tracing30MinView->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
		m_Message->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
	}
	else
	{
		m_Message->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
		if (tracing_required_width < cx)
		{
			int dY = cy - (tracing_height + BOTTOM_HEIGHT + actualExportBarHeight);
			if(dY < 0)
				dY = 0;
			m_Tracing->SetWindowPos(0, (cx - tracing_required_width) / 2, (display_patient_banner() ? BANNER_HEIGHT : 0) + dY, tracing_required_width, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
			if (m_bIs30MinView)
				m_Tracing15MinView->SetWindowPos(0, (cx - tracing_required_width) / 2, (display_patient_banner() ? BANNER_HEIGHT : 0) + dY, tracing_required_width, tracing_height, SWP_HIDEWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
			else
				m_Tracing30MinView->SetWindowPos(0, (cx - tracing_required_width) / 2, (display_patient_banner() ? BANNER_HEIGHT : 0) + dY, tracing_required_width, tracing_height, SWP_HIDEWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
		}
		else
		{
			m_Tracing->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
			if (m_bIs30MinView)
				m_Tracing15MinView->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_HIDEWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
			else
				m_Tracing30MinView->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_HIDEWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);

		}

	}


	//long top = cy + 4 - BOTTOM_HEIGHT;
	long top = cy + 3 - BOTTOM_HEIGHT - actualExportBarHeight;

	const UINT visible = SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED;
	const UINT hidden = SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE;

	int right = cx;
	int left = 10;
	// Reposition export bar
	CRect tracingRect;
	m_Tracing->GetWindowRect(tracingRect);
	ScreenToClient(tracingRect);

	m_exportBar.SetWindowPos(0, tracingRect.left, top - 3, tracingRect.Width(), EXPORT_BAR_HEIGHT, SWP_SHOWWINDOW | SWP_FRAMECHANGED);
	m_emptyBar.SetWindowPos(0, tracingRect.left, top - 3, tracingRect.Width(), EXPORT_BAR_HEIGHT, SWP_SHOWWINDOW | SWP_FRAMECHANGED);
	if(m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized())
	{
		m_pPatternsAdapter->ResizeNavigationPanel(0, 0, tracingRect.Width(), actualExportBarHeight);
	}

	top+= actualExportBarHeight;
	
	// Reposition buttons

	int button_width = 0;
	int leftBtnW = 0;
	int zoom_button_width = get_button("zoom")->optimal_width() + 5;
	leftBtnW += zoom_button_width;
	int status_button_width = 101;
	leftBtnW += 101;
	
	int rightBtnW = 0;
	int about_button_width = get_button("about")->optimal_width() + 5;
	rightBtnW += about_button_width;
	int montevideo_button_width = get_button("montevideo")->optimal_width() + 5;
	rightBtnW += montevideo_button_width;
	int events_button_width = get_button("events")->optimal_width() + 5;
	rightBtnW += events_button_width;
	int toco_button_width = get_button("toco")->optimal_width() + 5;
	rightBtnW += toco_button_width;
	int baseline_button_width = get_button("baseline")->optimal_width() + 5;
	rightBtnW += baseline_button_width;
	
	bool isNavigationVisible = true;
	bool isLargeExportButtonVisible = false;
	int availableLeft = left + leftBtnW;
	int availableRight = right - rightBtnW;
	int navigationW = (7 * 18);
	int availableW = availableRight - availableLeft - navigationW;
	if (availableW < 0)
	{
		isNavigationVisible = false;
		availableW = availableRight - availableLeft;
	}
	int exportBtnW = 0;
	if (availableW < 10 && !m_lessThanMinimumSpace)
	{
		availableW += 12;
		baseline_button_width -= 2;
		toco_button_width -= 2;
		events_button_width -= 2;
		montevideo_button_width -= 2;
		about_button_width -= 2;
		zoom_button_width -= 2;
		exportBtnW = min(baseline_button_width, availableW - 2);
	}


	if(availableW > 2 * baseline_button_width + 20)
	{
		isLargeExportButtonVisible = true;
		exportBtnW = 2 * baseline_button_width;
	}
	else if (availableW > baseline_button_width + 10)
	{
		exportBtnW = baseline_button_width + 10;
	}
	else 
	{
		exportBtnW = availableW - 2;
	}

	get_button("zoom")->SetWindowPos(0, left, top, zoom_button_width, 18, 0);
	left += zoom_button_width;

	
	get_button("data status")->SetWindowPos(0, left, top + 1, status_button_width, 18, 0);
	left += status_button_width;

	// From the right...
	
	right -= about_button_width;
	get_button("about")->SetWindowPos(0, right, top, about_button_width, 18, 0);

	
	right -= montevideo_button_width;
	get_button("montevideo")->SetWindowPos(0, right, top, montevideo_button_width, 18, right - left >= 10 ? visible:hidden);

	
	right -= events_button_width;
	get_button("events")->SetWindowPos(0, right, top, events_button_width, 18, right - left >= 10 ? visible:hidden);
	
	
	right -= toco_button_width;
	get_button("toco")->SetWindowPos(0, right, top, toco_button_width, 18, right - left >= 10 ? visible:hidden);
	
	
	right -= baseline_button_width;
	get_button("baseline")->SetWindowPos(0, right, top, baseline_button_width, 18, right - left >= 10 ? visible:hidden);
	
	
	// In the middle?
	
	
	//bool isNavigationVisible = (right - left >= navigationW);//(right - left >= (7 * 18) + 20 + button_width);
	bool isExportButtonVisible = !m_lessThanMinimumSpace && exportBtnW >= 10;//(right - left >= navigationW + 20 + button_width);
	int exportBtnLeft = left;

	//UINT flags = (right - left >= (7 * 18) + 20 + button_width) ? visible : hidden;
	UINT flags = isNavigationVisible? visible : hidden;

	get_button("go to beginning")->SetWindowPos(0, left, top, 18, 18, flags);
	left += 18;
	get_button("page left")->SetWindowPos(0, left, top, 18, 18, flags);
	left += 18;
	get_button("play left")->SetWindowPos(0, left, top, 18, 18, flags);
	left += 18;
	get_button("pause")->SetWindowPos(0, left, top, 18, 18, flags);
	left += 18;
	get_button("play right")->SetWindowPos(0, left, top, 18, 18, flags);
	left += 18;
	get_button("page right")->SetWindowPos(0, left, top, 18, 18, flags);
	left += 18;
	get_button("go to end")->SetWindowPos(0, left, top, 18, 18, flags);

	if(IsWindow(m_exportButton.m_hWnd))
	{		

		if (isNavigationVisible)
			exportBtnLeft = availableW > exportBtnW + 10? left + 10 : left + 1;

		if (availableW > exportBtnW + 10 && exportBtnW < baseline_button_width && availableW > baseline_button_width + 10)
		{
			exportBtnW = baseline_button_width;
		}

		
		if(availableW > exportBtnW + 20)
			exportBtnLeft += (isLargeExportButtonVisible)? 28 : 10;
		CRect exportBarRect;
		m_exportBar.GetWindowRect(exportBarRect);
		ScreenToClient(exportBarRect);		
		m_exportButton.SetWindowPos(0, exportBtnLeft, exportBarRect.bottom, exportBtnW, top - exportBarRect.bottom + 18, isExportButtonVisible? visible : hidden);
		
		if(m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized())
		{
			m_pPatternsAdapter->ResizeExportButton(0, 0, exportBtnW, top - exportBarRect.bottom + 18);
		}
	}	

	m_Tracing->refresh_layout();
	if(m_bInitialLoadCompleted)
		ShowExportBar(m_bExportVisible);// && !lessThanMinimumSpace);
	else
	{
		ShowExportBar(false);
	}
}

// On resize of the control, reposition the command bar
void ContainerBaseWnd::OnSize(UINT t, int cx, int cy)
{
	bool isStripEnd = false;
	date startT, endT;

	patterns_gui::tracing* pOtherTracing = m_bIs30MinView ? m_Tracing15MinView : m_Tracing30MinView;
	if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		m_Tracing->GetExpandedViewStartAndEnd(startT, endT);
		pOtherTracing->ShowWindow(SW_SHOWNA);

		patterns::fetus* f = get_fetus();
		if (f != NULL)
		{
			long total = f->get_number_of_fhr() / f->get_hr_sample_rate();
			if (f->get_start_date() != undetermined_date && total <= endT - f->get_start_date() + 25 && m_Tracing->is_scrolling())
				isStripEnd = true;
		}

	}



	CWnd::OnSize(t, cx, cy);
	adjust_controls();
	if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
		if (isStripEnd)
		{
			go_end();
		}
		else
		{
			m_Tracing->MoveToTime(endT, true, false);
			pOtherTracing->MoveToTime(endT, true, false);
		}
		pOtherTracing->RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
		pOtherTracing->ShowWindow(SW_HIDE);
	}

}
void ContainerBaseWnd::AdjustSliderAfterResize(bool isLeftPartShown, long leftIndex)
{
	m_Tracing->move_to(leftIndex);
	if (m_Tracing->IsToggleButtonAvailable())
	{

		m_Tracing->move_page(false);
		if (isLeftPartShown)
		{
			m_Tracing->ToggleSwitchLeft(false);
			long index = m_Tracing->GetSliderLeftIndex();
			if (index <= 0)
				m_Tracing->move_to(0);

		}
	}

}

/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// DRAWING

int ContainerBaseWnd::calculate_required_tracing_width(int height, bool for30MinView)
{
	static int line_height = -1;

	// Calculate a line of text height
	if (line_height < 0)
	{
		CPaintDC dc(this);
		services::select_font(80, "Arial", &dc);
		line_height = dc.GetTextExtent("W").cy;
	}

	// Substract the minimum for the fixed part
	if(for30MinView)
		return (int)((height - ((4 * (line_height + 1)) + patterns_fix_required_height)) / m_patterns30MinRatioCoefficient);
	else
		return (int)((height - ((4 * (line_height + 1)) + patterns_fix_required_height)) / m_patterns15MinRatioCoefficient);
}

int ContainerBaseWnd::calculate_required_tracing_height(int width, bool for30MinView)
{
	static int line_height = -1;

	// Calculate a line of text height
	if (line_height < 0)
	{
		CPaintDC dc(this);
		services::select_font(80, "Arial", &dc);
		line_height = dc.GetTextExtent("W").cy;
	}
	if(for30MinView)
		return (int)((width * m_patterns30MinRatioCoefficient) + (4 * (line_height + 1)) + patterns_fix_required_height);
	else
		return (int)((width * m_patterns15MinRatioCoefficient) + (4 * (line_height + 1)) + patterns_fix_required_height);
}

// Update the banner
void ContainerBaseWnd::refresh_banner(CDC* pdc)
{
	// No banner?
	if (!display_patient_banner())
		return;

	// Grab the location of the banner
	CRect bannerBox;
	GetClientRect(&bannerBox);
	bannerBox.bottom = bannerBox.top + BANNER_HEIGHT;

	// Clear all in white
	pdc->FillRect(bannerBox, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));

	// Draw line
	pdc->SelectObject(&m_linePen);
	pdc->MoveTo(bannerBox.left, bannerBox.bottom - 1);
	pdc->LineTo(bannerBox.right, bannerBox.bottom - 1);

	// Draw third party logo if any
	if (m_Banner == 1) // PeriGen
	{
		CRect thirdPBox = services::get_bitmap_rectangle("PERIGEN_BANNER");
		services::draw_bitmap(pdc, "PERIGEN_BANNER", "", bannerBox.right - thirdPBox.Width(), bannerBox.top, bannerBox.right, bannerBox.top + thirdPBox.Height(), services::ccenter);
	}
	else if (m_Banner == 2) // GE
	{
		CRect thirdPBox = services::get_bitmap_rectangle("GE_BANNER");
		services::draw_bitmap(pdc, "GE_BANNER", "", bannerBox.right - thirdPBox.Width(), bannerBox.top, bannerBox.right, bannerBox.top + thirdPBox.Height(), services::ccenter);
	}

	// In error, don't display the content of the banner at all
	if (m_CriticalError)
		return;

	// Draw the current patient info
	input_adapter::patient* p = get_patient();
	patterns::fetus* f = get_fetus();

	// No patient?
	if ((p == 0) || (f == 0))
		return;

	string banner_text = "Name: " + p->get_name();
	if (p->get_surname().length() > 0) banner_text += ", " + p->get_surname();

	banner_text += " | ID# " + p->get_accountno();

	pdc->SetBkMode(TRANSPARENT);
	pdc->SetTextColor(RGB(101, 92, 62));
	pdc->SetTextAlign(TA_BASELINE);

	pdc->SaveDC();

	services::select_font(80, "Arial Bold", pdc);
	pdc->TextOut(5, 4 + (BANNER_HEIGHT / 2), banner_text.c_str());

	pdc->RestoreDC(-1);
}

// Update all command bar buttons
void ContainerBaseWnd::refresh_command_bar(CDC* pdc)
{
	// Grab the location of the command bar
	CRect barBox;
	GetClientRect(&barBox);
	barBox.top = barBox.bottom - BOTTOM_HEIGHT;

	// Clear all in white
	pdc->FillRect(barBox, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));

	// Draw line
	pdc->SelectObject(&m_linePen);
	pdc->MoveTo(barBox.left, barBox.top);
	pdc->LineTo(barBox.right, barBox.top);

	// Repaint individual button
	for (map<string, icon_button*>::iterator i = m_buttons.begin(); i != m_buttons.end(); ++i)
	{
		i->second->Invalidate(TRUE);
	}
}


void ContainerBaseWnd::OnPaint()
{
	CPaintDC dc(this);
	CRect rcBounds;
	GetClientRect(rcBounds);

	if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		CRect tracingRect;
		m_Tracing->GetWindowRect(tracingRect);
		if (!tracingRect.IsRectEmpty())
		{
			ScreenToClient(tracingRect);
			if (tracingRect.Width() < rcBounds.Width())
			{
				CRect leftRect(0, 0, tracingRect.left, rcBounds.bottom);
				CRect rigntRect(tracingRect.right, 0, rcBounds.right, rcBounds.bottom);
				dc.FillSolidRect(leftRect, RGB(190, 190, 190));
				dc.FillSolidRect(rigntRect, RGB(190, 190, 190));
			}
		}
	}
	refresh_banner(&dc);
	refresh_command_bar(&dc);
}

// Optimization for drawing
BOOL ContainerBaseWnd::OnEraseBkgnd(CDC *)
{
	return TRUE;
}

// Timer refresh...
void ContainerBaseWnd::OnTimer(UINT_PTR timer)
{
	if (m_bInitialLoadCompleted && (m_Initialized) && (!m_DemoMode) && (timer == m_RefreshTimer))
	{
		download_patient_data();
	}

	CWnd::OnTimer(timer);
}

/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// LOGS

// An exception happened...
void ContainerBaseWnd::log_exception(string msg, exception& e)
{
	log_exception(msg + "\r\n\r\n" + e.what());
}

// An exception happened...
void ContainerBaseWnd::log_exception(string msg)
{
	CString text;
	//text.LoadString(an_error_happened);
	text = LoadTextAnErrorHappened();

	CString message = text + "\r\n\r\n" + msg.c_str();
	log_error((string)message);
}

// An error happened...
void ContainerBaseWnd::log_error(string msg)
{
	trace(EVENTLOG_ERROR_TYPE, msg.c_str());

	ClearPatient();
	ClearRequest();

	set_data_status(DataStatus::data_status_error, msg);
}

// Trace information in the Microsoft Windows Event log
void ContainerBaseWnd::trace(WORD type, LPCTSTR szFormat, ...)
{
	static HANDLE m_hEventLog = NULL;
	try
	{
		TCHAR szBuf[0x2000];

		va_list pArg;
		va_start(pArg, szFormat);
		::_vsntprintf_s(szBuf, sizeof(szBuf) / sizeof(TCHAR), szFormat, pArg);
		va_end(pArg);

		::OutputDebugString(szBuf);
		if (szBuf[::_tcslen(szBuf) - 1] != _T('\n'))
		{
			::OutputDebugString(_T("\n"));
		}

		if (m_hEventLog == NULL)
		{
			m_hEventLog = RegisterEventSource(NULL, _T("PeriGen Patterns"));
		}

		if (m_hEventLog != NULL)
		{
			LPCTSTR msg = (LPCTSTR)(&szBuf[0]);
			ReportEvent(m_hEventLog, type, 1, 0, NULL, 1,	0, &msg, NULL);
		}
	}
	catch(...)
	{
		// Just ignore
	}
}

void ContainerBaseWnd::CreateExportBar()
{
	if(!IsWindow(m_exportBar.m_hWnd))
	{
		CRect clientRect;
		GetClientRect(clientRect);
		CRect exportRect(clientRect.left, clientRect.bottom, clientRect.right, clientRect.bottom);
		m_exportBar.Create(NULL, NULL, WS_CHILD , exportRect, this, 0);
		m_emptyBar.Create(NULL, NULL, WS_CHILD | WS_DISABLED, exportRect, this, 0);
	}

}

void ContainerBaseWnd::UpdatePatternsAdapter()
{
	
	if(!m_bInitialLoadCompleted || !m_bExportEnabled  || m_Tracing->is_zoomed())
		return;
	bool valid = (m_gaTotalDays >= 252 && GetNumberOfFetuses() == 1);
	if(m_pPatternsAdapter != NULL)
	{

		m_Tracing->GetCompressedViewBoundsInSeconds(m_tracingLeftInSeconds, m_tracingRightInSeconds);
		if(!m_pPatternsAdapter->IsInitialized())
		{
			m_pPatternsAdapter->SetCLRWindow(GetParent()->m_hWnd);
			double defaultWidthInMinutes = 255;
			long widthInSeconds = m_tracingRightInSeconds - m_tracingLeftInSeconds;

			date startT = m_tracingRightInSeconds;
			SYSTEMTIME t = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(startT));
			COleDateTime oleTime(t);
			DATE startTime = oleTime;	
			m_pPatternsAdapter->Init(startTime, defaultWidthInMinutes);
//			m_pPatternsAdapter->SetPluginURL(m_ServerURL);
			m_pPatternsAdapter->SetMontevideoVisible(m_Tracing->is_visible(patterns_gui::tracing::wmontevideo));
			
			string patientIDStr = get_patient()->get_id();			
			long patientId = (patientIDStr.length() > 0)? atol(patientIDStr.c_str()) : 0;

			m_pPatternsAdapter->SetInitParams(m_UserID, patientId, m_Permissions.CompareNoCase("readonly") != 0, m_criHWND);
			DATE messageEndTime;
			CString eventMesage = m_Tracing->GetExportNotEnabledMessage(messageEndTime);
			m_pPatternsAdapter->SetPanelTooltip(eventMesage, messageEndTime);
			if(m_bExportEnabled && IsWindow(m_exportBar.m_hWnd))
			{				
				CRect exportBarRect;
				m_exportBar.GetWindowRect(exportBarRect);	
				m_pPatternsAdapter->InitNavigationPanel((long)m_exportBar.m_hWnd, 0, 0, exportBarRect.Width(), exportBarRect.Height());
			}

			if(m_bExportEnabled && IsWindow(m_exportButton.m_hWnd))
			{
				if(valid)
				{
					CRect exportBtnRect;
					m_exportButton.GetWindowRect(exportBtnRect);
				
					m_pPatternsAdapter->InitExportButton((long)m_exportButton.m_hWnd, 0, 0, exportBtnRect.Width(), exportBtnRect.Height());
				}
				else
				{
					m_pPatternsAdapter->InitExportButton((long)m_exportButton.m_hWnd, 0, 0, 0, 0);
				}
			}
			m_pPatternsAdapter->SetTimeRange(m_intervalDuration);
			
			

		}
		if(valid)
		{

			long actualWidth = m_tracingRightInSeconds - m_tracingLeftInSeconds;
		
			date startT = m_tracingRightInSeconds + 30;// + 65;
			SYSTEMTIME tStart = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(startT));
			COleDateTime oleTime(tStart);		
			DATE startTime = oleTime;
			if(m_pPatternsAdapter->IsInitialized())
			{
				CRect exportBtnRect;
				m_exportButton.GetWindowRect(exportBtnRect);
				m_pPatternsAdapter->ResizeExportButton(0, 0, exportBtnRect.Width(), exportBtnRect.Height());

				DATE messageEndTime;
				CString eventMesage = m_Tracing->GetExportNotEnabledMessage(messageEndTime);
				m_pPatternsAdapter->SetPanelTooltip(eventMesage, messageEndTime);

				m_pPatternsAdapter->SetStartTime(startTime);
				VisibleChunksToAdapter();
			}
		}

	}
}

void ContainerBaseWnd::VisibleChunksToAdapter()
{
	
	if(m_pPatternsAdapter && m_pPatternsAdapter->IsInitialized() && m_gaTotalDays >= 252 && GetNumberOfFetuses() == 1)
	{

		//m_Tracing->ResetVisibleChunks();
		m_Tracing15MinView->ResetVisibleChunks();
		m_Tracing30MinView->ResetVisibleChunks();
		m_pPatternsAdapter->BeginUpdateChunks();
		vector<PatternsExportableChunkNative>::iterator it;
		patterns::fetus* f = get_fetus();
		long offset = 0;
		
		if(f != NULL && f->get_start_date() != patterns::undetermined_date)
		{
			date tracingStart = (long)f->get_start_date();
			offset = (long)tracingStart;	
		
			for(it = m_exportableIntervals.begin(); it != m_exportableIntervals.end(); it++)
			{
				date validStart = GetValidGAStart();
				if(it->GetStartTime() >= tracingStart 
					&& (it->GetStartTime() >= m_tracingLeftInSeconds && it->GetStartTime() <= m_tracingRightInSeconds
					|| it->GetEndTime() >=  m_tracingLeftInSeconds && it->GetEndTime() <= m_tracingRightInSeconds)
					&& it->GetStartTime() >= validStart)
				{
					DATE startDate = it->GetStartDATE();
					COleDateTime olet(startDate);
					int startMinute = olet.GetMinute();
					long leftInPixels = m_Tracing->GetChunkXFromSeconds(it->GetStartTime() - offset);
					long rightInPixels = m_Tracing->GetChunkXFromSeconds(it->GetEndTime() - offset);
					if(startMinute == 0 || startMinute == 30)
					{					
						//m_Tracing->AddVisibleChunkEnd(leftInPixels);
						m_Tracing15MinView->AddVisibleChunkEnd(leftInPixels);
						m_Tracing30MinView->AddVisibleChunkEnd(leftInPixels);
					}
					olet += COleDateTimeSpan(0, 0, it->GetTimeRange(), 0);
					int endMinute = olet.GetMinute();
					if(endMinute == 0 || endMinute == 30)
					{					
						//m_Tracing->AddVisibleChunkEnd(rightInPixels);
						m_Tracing15MinView->AddVisibleChunkEnd(rightInPixels);
						m_Tracing30MinView->AddVisibleChunkEnd(rightInPixels);
					}				

					m_pPatternsAdapter->AddChunk(it->m_exportID, it->m_id, it->GetStartDATE(), it->GetTimeRange(), it->IsExported(), leftInPixels, rightInPixels);

				
				}
			}		

		}
		m_pPatternsAdapter->EndUpdateChunks();
	}
	
}



void ContainerBaseWnd::LoadExportableIntervals(TiXmlElement* intervalsNode)
{	
	if(m_bExportEnabled)
	{
		TiXmlElement* node = intervalsNode->FirstChildElement("interval");
		if (node != NULL)
		{
			long basetime = patterns::conductor::to_integer(intervalsNode->Attribute("basetime"));
		
			while (node != NULL)
			{	
				string data = node->Attribute("data");
				AddExportableInterval(basetime, data);
				node = node->NextSiblingElement();
			}
		}
	}

}

void ContainerBaseWnd::AddExportableInterval(long basetime, string data)
{
	if(!m_bExportEnabled)
		return;

	if (data.compare(0, 4, "INT|") == 0)
	{
		string interval = data.substr(4);
		vector<string> mri;
		patterns::conductor::to_vector(interval, mri, '|');

		if (mri.size() < 5)
		{
			return;
		}
		
		long id = (conductor::to_integer(mri[0]));
		long startTime = (conductor::to_integer(mri[1]));
		long endTime = (conductor::to_integer(mri[2]));
		long timeRange = (conductor::to_integer(mri[3]));
		long exportID = (conductor::to_integer(mri[4]));

		time_t intervalStartTime = basetime + startTime;
		PatternsExportableChunkNative chunk(intervalStartTime, timeRange, exportID >= 0);
		chunk.m_id = id;
		chunk.m_exportID = exportID;
		if (m_exportableIntervals.size() == 0)
			m_exportableIntervals.push_back(chunk);
		else if (id > m_exportableIntervals.back().m_id)
		{
			int oldIndex = FindIntervalByStartTime(intervalStartTime);
			if (oldIndex == -1)
			{
				m_exportableIntervals.push_back(chunk);
			}
			else if(!m_exportableIntervals.at(oldIndex).IsExported())
			{
				m_exportableIntervals.at(oldIndex) = chunk;
			}
		}
		else
		{
			int oldIndex = FindIntervalByID(id);
			if(oldIndex != -1 && oldIndex < m_exportableIntervals.size())
			{
				m_exportableIntervals.at(oldIndex) = chunk;				
			}
		}
	}
}

int ContainerBaseWnd::FindIntervalByID(long id)
{
	int index = -1;
	for(int i = m_exportableIntervals.size()-1; i >=0; i--)
	{
		if(m_exportableIntervals[i].m_id == id)
		{
			index = i;
			break;
		}
	}
	return index;
}

int ContainerBaseWnd::FindIntervalByStartTime(time_t startTime)
{
	int index = -1;
	for (int i = m_exportableIntervals.size() - 1; i >= 0; i--)
	{
		if (m_exportableIntervals[i].m_startTime == startTime)
		{
			index = i;
			break;
		}
	}
	return index;
}

bool ContainerBaseWnd::IsIntervalExported(long id)
{
	int index = FindIntervalByID(id);
	if(index != -1)
	{
		return (m_exportableIntervals[index].m_exportID >= 0);
	}
	return false;
}
long ContainerBaseWnd::OnForceUpdateAdapterMessage(WPARAM wParam, LPARAM lPararm)
{
	if(m_bExportEnabled)
		UpdatePatternsAdapter();
	return 0;
}


long ContainerBaseWnd::OnExportBarMouseOverCallbackMsg(WPARAM wParam, LPARAM lParam)
{
	if(!m_bExportEnabled)
		return 0;
	EportBarActiveChunck* pChunk = (EportBarActiveChunck*)lParam;
	if(pChunk && m_Tracing)
	{
		tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
		DATE from = pChunk->m_from;
		DATE to = pChunk->m_to;
		long id = pChunk->m_intervalID;
		ReplyMessage(0);

		bool isExported = false;
		if(wParam != 0 && id >=0)
		{
			isExported = IsIntervalExported(id);
	
		}
		m_Tracing->SetHighlightedExportTime(from, to, wParam != 0, isExported);
		pOther->SetHighlightedExportTime(from, to, wParam != 0, isExported);
	}
	return 0;
}

long ContainerBaseWnd::OnExportBarMouseLeaveCallbackMsg(WPARAM wParam, LPARAM lParam)
{
	if(!m_bExportEnabled)
		return 0;	
	EportBarActiveChunck* pChunk = (EportBarActiveChunck*)lParam;
	if(pChunk && m_Tracing)
	{
		tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
		DATE from = pChunk->m_from;
		DATE to = pChunk->m_to;
		ReplyMessage(0);
		m_Tracing->ResetHighlightedExportTime(from, to, wParam != 0);
		pOther->ResetHighlightedExportTime(from, to, wParam != 0);
	}
	return 0;

}
long ContainerBaseWnd::OnShowExportBarMsg(WPARAM wParam, LPARAM lParam)
{
	m_bExportVisible = (wParam == 1);
	if(!m_bExportEnabled)
		return 0;
	bool show = (wParam == 1);
	ShowExportBar(show);	
	return 0;
}
void ContainerBaseWnd::RefreshExportBarWnd(CDC* pdc)
{

	if(IsWindow(m_exportBar.m_hWnd))
	{
		CRect barRect;
		m_exportBar.GetWindowRect(barRect);
		ScreenToClient(barRect);
		pdc->FillRect(barRect, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	}
}
void ContainerBaseWnd::ShowExportBar(bool show)
{	
	m_bExportVisible = show;
	if(m_Tracing != NULL && IsWindow(m_exportBar.m_hWnd) && IsWindow(m_exportButton.m_hWnd) && m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd)
		/*&& m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized()*/)
	{
		tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
		bool valid = (m_bExportEnabled && m_gaTotalDays >= 252 && GetNumberOfFetuses() == 1);
		bool isConnected = (m_DataStatus == DataStatus::data_status_connected || m_DataStatus == DataStatus::data_status_recovery);
		CRect barRect;
		m_exportBar.GetWindowRect(barRect);
		ScreenToClient(barRect);
		CRect btnRect;
		m_exportButton.GetWindowRect(btnRect);
		ScreenToClient(btnRect);
		
		const UINT visible = SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED;
		const UINT hidden = SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE;
		bool bShowExport = !m_Tracing->is_zoomed() && show && isConnected && m_bExportEnabled && !m_lessThanMinimumSpace;
		m_exportBar.ShowWindow(bShowExport? SW_SHOW : SW_HIDE);
		m_exportButton.ShowWindow(!m_Tracing->is_zoomed() && show && isConnected && valid? SW_SHOW : SW_HIDE);			
		m_emptyBar.ShowWindow(!bShowExport? SW_SHOW : SW_HIDE);


		 if(m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized())
		 {
			if(!m_Tracing->is_zoomed() && show && isConnected && m_bExportEnabled)
			{	
				m_emptyBar.SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW);
				m_pPatternsAdapter->HideControls(false, valid);
				m_pPatternsAdapter->ResizeNavigationPanel(0, 0, barRect.Width(), barRect.Height());
				if (valid)
				{
					m_pPatternsAdapter->ResizeExportButton(0, 0, btnRect.Width(), btnRect.Height());
				}
				else
				{
					m_pPatternsAdapter->ResizeExportButton(0, 0, 0, 0);
				}
				
				UpdatePatternsAdapter();		
			}
			else
			{
				m_emptyBar.SetWindowPos(0, barRect.left, barRect.top, barRect.Width(), barRect.Height(), SWP_SHOWWINDOW | SWP_FRAMECHANGED);
				m_Tracing->ResetVisibleChunks();
				pOther->ResetVisibleChunks();
				m_pPatternsAdapter->HideControls();
				
			}
		 }
		
		RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
	}
}
void ContainerBaseWnd::Zoom(bool zoom)
{
	if(m_Tracing != NULL)
	{
		bool wasZoomed = m_Tracing->is_zoomed();
		if(wasZoomed != zoom)
		{			
			//m_Tracing->zoom(zoom);
			m_Tracing15MinView->zoom(zoom);
			m_Tracing30MinView->zoom(zoom);
			ShowExportBar(!m_Tracing->is_zoomed());
		}
	}

}
void ContainerBaseWnd::PerformUserPluginAction(string element, long value, int actiontype)
{
	if(!m_bExportEnabled)
		return;
	try
	{
		TiXmlDocument xmlDoc;
		TiXmlElement* actions = new TiXmlElement("pluginActions");	
		TiXmlElement* item = new TiXmlElement("pluginAction");		
		item->SetAttribute("type", actiontype);		
		item->SetAttribute("patient", get_patient()->get_id().c_str());
		item->SetAttribute("userid", m_UserID);
		item->SetAttribute("username", m_UserName);
		item->SetAttribute(element.c_str(), value);
		actions->LinkEndChild(item);
		xmlDoc.LinkEndChild(actions);

		utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("pluginaction"), utils::to_string(xmlDoc), "PATTERNS_CONTROL");
	
	}
	catch (exception& e)
	{
		log_exception("Error while sending a user action to the server", e);
	}
	catch (...)
	{
		log_exception("Unknown error while sending a user action to the server");
	}

}

long ContainerBaseWnd::OnTimeRangeChangeCallbackMsg(WPARAM wParam, LPARAM lParam)
{
	long timeRange = (int)lParam;
	ReplyMessage(0);
	PerformUserPluginAction(string("intervalduration"), timeRange, 1);
	return 0;
}

void ContainerBaseWnd::UpdateExportTimeRange(TiXmlElement* requestNode)
{
	if(m_bExportEnabled)
	{
		int intervalduration = 30;
		requestNode->Attribute("intervalDuration", &intervalduration);
	
		if(intervalduration != m_intervalDuration)
		{
			m_intervalDuration = intervalduration;

			if(m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized())
			{
				m_pPatternsAdapter->SetTimeRange(m_intervalDuration);
			}
		}
	}
}

long ContainerBaseWnd::GetNumberOfFetuses()
{
	input_adapter::patient* pPatient = get_patient();
	if(pPatient)
	{
		return pPatient->get_number_of_fetuses();
	}
	return 1;
}

date ContainerBaseWnd::GetValidGAStart()
{	
	fetus* curFetus = get_fetus();
	if(curFetus && curFetus->has_cutoff_date())
	{		
		return fetus::convert_to_local(curFetus->get_cutoff_date());
		
	}
	return 0;
}

void ContainerBaseWnd::ParseGA(string GA)
{
	CString gaStr(GA.c_str());

	if(m_GA.Compare(gaStr) != 0)
	{
		m_GA = gaStr;
		long plus = gaStr.Find("+");
		CString weeks = (plus > 0) ? gaStr.Left(plus) : gaStr;
		CString days = (plus >= 0 && plus < gaStr.GetLength() - 1)?
			gaStr.Mid(plus + 1) : "";
		
		weeks.TrimRight();
		long totalDays = 0;
		if(!weeks.IsEmpty())
		{
			int w =  atoi(weeks);
			totalDays = 7*w;
		}
		days.TrimLeft();
		if(!days.IsEmpty())
		{
			int d = atoi(days);
			totalDays += d;
		}
		m_gaTotalDays = totalDays;	

	}
}

void ContainerBaseWnd::LoadEnabledPlugins()
{
	m_bExportEnabled = false;
	m_bPluginLoadError = false;
	if(m_ServerURL.Find("PatternsDataFeed") >= 0)
		return;
	try
	{
	// Request data from the server
		string response = utils::perform_server_request(string((LPCSTR)m_ServerURL) + string("Plugins"), "PATTERNS_CONTROL") + "\r\n";
		
		// Failed?
		if (response.length() == 2) // 2 because of the + "\r\n"
		{				
			set_data_status(DataStatus::data_status_error, "Unable to reach the server");
			m_bPluginLoadError = true;
			return;
		}

		// Parse the answer
		TiXmlDocument xmlDoc;
		if (xmlDoc.Parse(response.c_str()) == 0)
		{
			throw new exception("Invalid data format, unable to parse it as a proper xml");
		}
	

		TiXmlElement* pliginsnode = xmlDoc.FirstChildElement("Plugins");	
		if(pliginsnode == NULL)
		{
			log_error( "Unable to reach the plugin server");
			set_data_status(DataStatus::data_status_error, "Unable to reach the server");
			m_bPluginLoadError = true;
			return;
		}
		TiXmlElement* node = pliginsnode->FirstChildElement("Plugin");
		while( node )
		{
			string value = node->Attribute("Name");
			if(value.c_str() != NULL)
			{				
				if(m_exportSupported && value.compare(EXPORT_PLUGIN_NAME) == 0)
				{
					m_bExportEnabled =  true;
					break;
				}
				else if (value.compare(CHECKLIST_PLUGIN_NAME) == 0)
				{
					m_checklistEnabled = true;
				}
			}
			node=node->NextSiblingElement();
		}

		if(m_Tracing)
		{
			//m_Tracing->SetExportEnabled(m_bExportEnabled);
			m_Tracing15MinView->SetExportEnabled(m_bExportEnabled);
			m_Tracing30MinView->SetExportEnabled(m_bExportEnabled);
		}
		if(m_bExportEnabled && m_pPatternsAdapter == NULL)
		{
			m_pPatternsAdapter = new ExportNativeManager();		
		}
		if(m_bExportEnabled && m_pPatternsAdapter != NULL)
			m_pPatternsAdapter->SetPatternsControl(m_hWnd);
		ShowExportBar(m_bExportEnabled);
	}	
	catch (exception& e)
	{		
		m_CriticalError = true; 
		log_exception("Error while requesting server data", e);
	
	}
	catch (...)
	{
		m_CriticalError = true; 
		log_exception("Unknown error while requesting server data");
		
	}
	
}

void ContainerBaseWnd::LoadProductInformation()
{
	try
	{
		// Request data from the server
		string response = utils::perform_server_request(string((LPCSTR)m_ServerURL) + string("productinfo"), "PATTERNS_CONTROL") + "\r\n";
		// Failed?
		if (response.length() == 2) // 2 because of the + "\r\n"
		{
			set_data_status(DataStatus::data_status_error, "Unable to reach the server");
			m_bPluginLoadError = true;
			return;
		}

		// Parse the answer
		TiXmlDocument xmlDoc;
		if (xmlDoc.Parse(response.c_str()) == 0)
		{
			throw new exception("Invalid data format, unable to parse it as a proper xml");
		}


		TiXmlElement* pluginsnode = xmlDoc.FirstChildElement("product");
		if (pluginsnode == NULL)
		{
			log_error("Unable to reach the server");

			return;
		}

		const char* brand = pluginsnode->Attribute("brand");
		if (brand!= NULL)
		{
			m_brandName = brand;
		}
		const char* brandMarkTM = pluginsnode->Attribute("BrandMarkTM");
		if (brandMarkTM != NULL)
		{
			CString mark(brandMarkTM);
			m_brandMarkTM = mark.CompareNoCase("true") == 0;
		}
		const char* patterns = pluginsnode->Attribute("patterns");
		if (patterns != NULL)
		{
			m_patternsAppName = patterns;
		}


		const char* logo = pluginsnode->Attribute("logo");		
		if (logo != NULL)
		{			
			m_logo = atoi(logo);
		}


	}
	catch (exception& e)
	{
		m_CriticalError = true;
		log_exception("Error while requesting server data", e);

	}
	catch (...)
	{
		m_CriticalError = true;
		log_exception("Unknown error while requesting server data");
	}
}

CString ContainerBaseWnd::GetExportNotEnabledMessage(DATE& endTime)
{
	endTime = DATE(0);
	CString msg = "";
	if(m_Tracing)
	{
		msg = m_Tracing->GetExportNotEnabledMessage(endTime);
	}
	else
	{		
		COleDateTime maxTime;
		maxTime.SetDateTime(9999,12,31,0,0,0);
		endTime = maxTime;
	}
	return msg;
}

long ContainerBaseWnd::OnGetExportCalculatedEntriesEx(WPARAM wParam, LPARAM lParam)
{	
	ExportEntriesCalcStruct* pEntries = (ExportEntriesCalcStruct*)wParam;
	EportBarActiveChunck* pChunk = (EportBarActiveChunck*)lParam;
	if(pEntries && pChunk && m_Tracing)
	{
		
		DATE from = pChunk->m_from;
		DATE to = pChunk->m_to;
		long id = pChunk->m_intervalID;
		CString resultString;
		m_Tracing->DoOnExportDlgCalculatedEntriesRequestEx(from, to, pEntries->m_meanContructions, pEntries->m_meanBaseline, 
			pEntries->m_meanBaselineVariability, pEntries->m_montevideo, pEntries->m_contractionThresholdExceeded);
		
	}
	return 0;
}

/////////////////////////////////////////////////
CString ContainerBaseWnd::LoadIconTextZoom()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(icon_text_zoom);
	return s;
}
CString ContainerBaseWnd::LoadIconTextBaselines()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(icon_text_baselines);
	return s;
}
CString ContainerBaseWnd::LoadIconTextToco()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(icon_text_toco);
	return s;

}
CString ContainerBaseWnd::LoadIconTextEvents()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(icon_text_events);
	return s;

}
CString ContainerBaseWnd::LoadIconTextMontevideo()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(icon_text_montevideo);
	return s;

}
CString ContainerBaseWnd::LoadIconTextAbout()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(icon_text_about);
	return s;

}
CString ContainerBaseWnd::LoadTextAvailableSpaceTooSmall()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(available_space_too_small);
	return s;

}
CString ContainerBaseWnd::LoadTextAnErrorHappened()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;
	s.LoadString(an_error_happened);
	return s;

}

bool ContainerBaseWnd::GetExportCalculatedEntries(DATE from, DATE to, long id, ExportEntriesCalcStruct& entries)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	if(m_Tracing != NULL)
	{
		return m_Tracing->DoOnExportDlgCalculatedEntriesRequestEx(from, to, entries.m_meanContructions,  entries.m_meanBaseline,  entries.m_meanBaselineVariability,  entries.m_montevideo, entries.m_contractionThresholdExceeded);		
		
	}
	return false;
}

bool ContainerBaseWnd::GetContractionThresholdExceeded(DATE from, DATE to)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	if (m_Tracing != NULL)
	{
		return m_Tracing->GetContractionThresholdExceeded(from, to);

	}
	return false;

}
////////////////////////////////////////////////////

long ContainerBaseWnd::OnInitialLoadCompletedMsg(WPARAM wParam, LPARAM lParam)
{
	ReplyMessage(1);
	InitializationResult* pResult = (InitializationResult*)lParam;
	bool bSucceed = false;
	if (pResult != NULL)
	{
		bSucceed = pResult->m_bSucceed;
		if (!pResult->m_bSucceed && !pResult->m_error.IsEmpty())
		{
			string errorStr = (LPCSTR)pResult->m_error;
			log_error(errorStr);
			m_DataStatus = pResult->m_status;
			m_DataStatusDetails = errorStr;
		}
		set_data_status(pResult->m_status, pResult->m_statusDetails);
		//m_Tracing->set_can_delete(pResult->m_readOnly);
		m_Tracing15MinView->set_can_delete(pResult->m_readOnly);
		m_Tracing30MinView->set_can_delete(pResult->m_readOnly);
		if (pResult->m_updateBanner && display_patient_banner())
		{
			// Refresh banner
			CRect rect;
			GetClientRect(&rect);
			rect.bottom = rect.top + BANNER_HEIGHT;

			InvalidateRect(rect);
		}
		go_end();
		if (pResult->m_bSucceed && pResult->m_b30MinView != m_bIs30MinView)
			PostMessage(SWITCH_15MIN_30MIN_VIEWS_MSG, 0, 0);
		if (pResult->m_bSucceed)
		{
			m_roundBaselineFHRValue = pResult->m_roundBaselineFHRValue;
			m_roundByMontevideoUnits = pResult->m_roundByMontevideoUnits;
			ShowMVU(pResult->m_showMVU, false, false);
		}
		delete pResult;
	}

	DoOnInitializationEnd(bSucceed);

	return 0;
}

void ContainerBaseWnd::DoOnInitializationEnd(bool bSucceed)
{
	StartConnectionThread();
	m_bInitialLoadCompleted = true;

	// Reset the timer for refresh
	if (m_RefreshTimer != 0)
	{
		KillTimer(m_RefreshTimer);
		m_RefreshTimer = 0;
	}

	m_RefreshTimer = SetTimer(1, m_RefreshDelay * 1000, NULL);
	InvalidateRgn(0, FALSE);

	m_Initialized = true;
	UpdatePatternsAdapter();

	
	ShowExportBar(m_bExportVisible);
	m_pWorkerThread = NULL;
	m_hThreadWnd = NULL;
	GetParent()->SendMessage(MSG_END_INITIALIZATION, bSucceed? 1 : 0, 0);
	
	if (bSucceed)
	{
		UpdateRoundExportValue();
	}

}

void ContainerBaseWnd::StopInitializationThread()
{
	try
	{
		if (m_pWorkerThread != NULL)
		{
			HANDLE hThread = m_pWorkerThread->m_hThread;
			if (hThread != NULL)
			{
				TerminateThread(hThread, 1);
			}
		}
	}
	catch (...)
	{
		
	}

	m_pWorkerThread = NULL;
	m_hThreadWnd = NULL;
}

long ContainerBaseWnd::OnWorkerThreadCreation(WPARAM wParam, LPARAM lParam)
{
	m_hThreadWnd = (HWND)lParam;
	SetDataToWorkerThread();
	return 1;
}

void ContainerBaseWnd::ToggleSwitch(bool right, bool bExternalCall)
{
	if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		if (!m_Tracing->IsExportDlgOpenFor15MinInterval())
		{
			if (right)
			{
				m_Tracing->ToggleSwitchRight(false);
			}
			else 
			{
				m_Tracing->ToggleSwitchLeft(false);
			}
		}
	}
	if (!bExternalCall)
	{
		WPARAM wParam = (m_Tracing != NULL && m_Tracing->IsLeftPartShown()) ? 1 : 0;
		GetParent()->SendMessage(MSG_SWITCH_TOGGLED_MSG, wParam, 0);
	}
}

bool ContainerBaseWnd::IsLeftPartShown()
{
	if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		return m_Tracing->IsLeftPartShown();
	}
	return false;
}

void ContainerBaseWnd::on_show_about(void)
{
	if (GetParent() != NULL)
	{
		GetParent()->SendMessage(MSG_ABOUTBTN_CLICKED, 0, 0);

		
	}
}

bool ContainerBaseWnd::GetProductInformation(ProductInformationStruct& productInfo)
{
	productInfo.m_brandMarkTM = m_brandMarkTM;
	productInfo.m_brandName = m_brandName;
	productInfo.m_logo = m_logo;
	productInfo.m_patternsName = m_patternsAppName;
	productInfo.m_checklistEnabled = m_checklistEnabled;
	return true;
}

long ContainerBaseWnd::OnMoveToIntervalMsg(WPARAM wParam, LPARAM lParam)
{
	if (lParam != NULL && m_bExportEnabled && m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
		DATE* from = (DATE*)lParam;

		COleDateTime oleStartT(*from);
		SYSTEMTIME sStartT;
		oleStartT.GetAsSystemTime(sStartT);
		

		SYSTEMTIME utcStart = fetus::convert_to_utc(sStartT);
		date intervalStartTime = fetus::convert_to_time_t(utcStart);
		
		int index = FindIntervalByStartTime(intervalStartTime);
		if (index != -1)
		{
			PatternsExportableChunkNative chunk = m_exportableIntervals[index];
			if (m_Tracing->MoveToInterval(*from, chunk.m_timeRange))
			{
				pOther->MoveToInterval(*from, chunk.m_timeRange);
				UpdatePatternsAdapter();
			}
		}
	}
	return 1;
}

void ContainerBaseWnd::Switch15Min30MinViews(bool bNotify, bool externalCall, bool left15min)
{
	bool isscrolling = m_Tracing->is_scrolling();
	bool iszoomed = m_Tracing->is_zoomed();
	long index = m_Tracing->GetSliderLeftIndex();
	bool isLeftPartShown = !m_bIs30MinView && IsLeftPartShown();

	long tracingLeftInSeconds, tracingRightInSeconds;
	m_Tracing->GetCompressedViewBoundsInSeconds(tracingLeftInSeconds, tracingRightInSeconds);
	date compressedStartT = tracingLeftInSeconds;
	SYSTEMTIME t = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(compressedStartT));
	COleDateTime oleEndTime(t);
	DATE startCompressedTime = oleEndTime;

	date startT, endT;
	m_Tracing->GetExpandedViewStartAndEnd(startT, endT);
	SYSTEMTIME tEnd = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(endT));
	COleDateTime oleTimeEnd(tEnd);
	DATE endTime = oleTimeEnd;

	bool isStripEnd = false;
	patterns::fetus* f = get_fetus();
	if (!left15min && f != NULL)
	{
		long total = f->get_number_of_fhr() / f->get_hr_sample_rate();
		if (f->get_start_date() != undetermined_date && total <= endT - f->get_start_date() + 25 && m_Tracing->is_scrolling())
			isStripEnd = true;
	}

	m_bIs30MinView = !m_bIs30MinView;
	m_Tracing = m_bIs30MinView ? m_Tracing30MinView : m_Tracing15MinView;
	tracing* pOther = m_bIs30MinView ? m_Tracing15MinView : m_Tracing30MinView;
	pOther->ShowWindow(SW_HIDE);
	pOther->EnableWindow(FALSE);
	m_Tracing->EnableWindow(TRUE);
	m_Tracing->ShowWindow(SW_SHOW);
	m_Tracing->UpdateWindow();


	long compressedLeft, compressedRight;
	m_Tracing->GetCompressedViewBoundsInSeconds(compressedLeft, compressedRight);

	SetRedraw(FALSE);
	
	SYSTEMTIME sysT;	
	COleDateTime compressedTimeT(compressedLeft);
	compressedTimeT.GetAsSystemTime(sysT);
	SYSTEMTIME utcDest = fetus::convert_to_utc(sysT);
	date destinationTime = fetus::convert_to_time_t(utcDest);
	if (destinationTime != compressedLeft)
	{
		Zoom(iszoomed);
		m_Tracing->MoveCompressedViewToTime(destinationTime);
	}


	bool goToSelectedInterval = false;
	if (!m_bIs30MinView && m_Tracing->IsExportDlgOpenFor15MinInterval())
	{
		goToSelectedInterval = true;
	}

	SYSTEMTIME  sEndT;
	COleDateTime oleEndT(endTime);
	oleEndT.GetAsSystemTime(sEndT);
	SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);
	date newEndTime = fetus::convert_to_time_t(utcEnd);

	if (isLeftPartShown)
		newEndTime += 900;

	if (!goToSelectedInterval)
	{
		if (!m_bIs30MinView)
		{
			if (left15min  && index > 0)
			{
				newEndTime -= 900;
				m_Tracing->ToggleSwitchLeft(!externalCall);
			}
			else
				m_Tracing->ToggleSwitchRight(!externalCall);
		}

		m_Tracing->MoveToTime(newEndTime, true, false);
	}
	else
		m_Tracing->MoveToHighlited15MinExportInterval();

	if (isStripEnd)
	{
		m_Tracing->move_to(m_Tracing->get()->get_number_of_fhr() + 100);
		m_Tracing->scroll(true);
	}
	bool isBeginning = false;
	if (!m_bIs30MinView && !goToSelectedInterval)
	{
		if (index <= 0 && left15min)
		{
			m_Tracing->move_to(0);
			isBeginning = true;
		}
		
	}
	
	SetRedraw(TRUE);
	RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);

	//date startT_, endT_;
	//m_Tracing->GetExpandedViewStartAndEnd(startT_, endT_);
	//SYSTEMTIME tEnd_ = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(endT_));
	//COleDateTime oleTimeEnd_(tEnd_);
	//DATE endTime_ = oleTimeEnd_;
	//	fstream fs;
	//	fs.open("c:\\temp\\15min30minswitch.txt", fstream::app | fstream::in);
	//	fs << "destination time" << oleEndT.Format("%y/%m/%d %H:%M:%S") << "\r\n";
	//	fs << "actual time" << oleTimeEnd_.Format("%y/%m/%d %H:%M:%S") << "\r\n";
	//	fs << "actual time in ms" << (long)endT_ << "\r\n";
	//	
	//	if (newEndTime != endT_ && !isStripEnd && !isBeginning)
	//	{
	//		fs << "move to time in ms" << (long)newEndTime << "\r\n";
	//		m_Tracing->MoveToTime(newEndTime, true, false);
	//		PostMessage(MSG_SETSLIDERPOSITION, (WPARAM)isscrolling, (LPARAM)index);
	//	}
	//	fs.close();
	
	if (goToSelectedInterval)
	{
		PostMessage(MSG_GOTO_SELECTEDINTERVAL, 0, 0);
	}
	
	if (bNotify)
	{
		WPARAM wParam = (m_Tracing != NULL && m_bIs30MinView) ? 1 : 0;
		GetParent()->SendMessage(MSG_VIEW_ZOOMED, wParam, 0);
	}

}

long ContainerBaseWnd::OnSwitch15Min3MinViewsMsg(WPARAM wParam, LPARAM lParam)
{
	Switch15Min30MinViews();
	return 0;
}
long ContainerBaseWnd::OnSetSliderPosMsg(WPARAM wParam, LPARAM lParam)
{
	bool isscrolling = wParam != 0;
	long index = lParam;
	m_Tracing->move_to(index);
	if (!m_bIs30MinView)
	{
		m_Tracing->move_page(false);
	}
	m_Tracing->scroll(isscrolling);
	if (isscrolling)
	{
		m_Tracing->update();
	}
	return 0;
}

long ContainerBaseWnd::OnGoToSelectedIntervalMsg(WPARAM wParam, LPARAM lParam)
{
	m_Tracing->MoveToHighlited15MinExportInterval();
	return 0;
}

void ContainerBaseWnd::AddBuildVersionToSupportedVersions(const CString buildFileVersion)
{
	CString buildMajorVersion = buildFileVersion.GetLength() <= 8 ? buildFileVersion : buildFileVersion.Left(8);
	int index = -1;
	for (int i = 0; i < m_supportedPatternsVersions.GetSize(); i++)
	{
		CString version = m_supportedPatternsVersions[i];
		CString majorVersion = version.GetLength() <= 8 ? version : version.Left(8);
		if (majorVersion.Compare(buildMajorVersion) == 0)
			index = i;
	}
	if (index > 0)
	{
		m_supportedPatternsVersions.RemoveAt(index);
	}
	m_supportedPatternsVersions.InsertAt(0, buildMajorVersion);
}

void ContainerBaseWnd::PrepareInitialRequestHeader()
{
	m_RequestHeader.Clear();
	CString str;
	str.Format("<patients version=\"%s\"><request key=\"0\"/></patients>\r\n", m_currentPatternsVersion);
	//m_RequestHeader.Parse("<patients version=\"" FILEVERSTR "\"><request key=\"0\"/></patients>\r\n");
	m_RequestHeader.Parse(str);
	m_RequestHeader.FirstChildElement("patients")->SetAttribute("user", m_UserID);
	m_RequestHeader.FirstChild("patients")->FirstChildElement("request")->SetAttribute("key", m_patientID);
}

bool ContainerBaseWnd::RetryInitialRequest()
{
	if (m_isInitialRequest)
	{
		m_currentVersionIndex++;
		if (m_currentVersionIndex < m_supportedPatternsVersions.GetSize())
		{			
			m_currentPatternsVersion = m_supportedPatternsVersions[m_currentVersionIndex];
			m_skip_download = 4;
			PrepareInitialRequestHeader();
			RequestData();
			return true;
		}
		else
		{
			m_isInitialRequest = false;
			
		}	

	}
	return false;
}


void ContainerBaseWnd::SwitchViews(bool to30Min, bool externalCall)
{
	if (m_bIs30MinView != to30Min)
		Switch15Min30MinViews(false, externalCall);
}
bool ContainerBaseWnd::Is30MinView()
{
	return m_bIs30MinView;
}

void ContainerBaseWnd::StopConnectionThread()
{
	try
	{
		if (m_pConnectionThread != NULL)
		{
			HANDLE hThread = m_pConnectionThread->m_hThread;

			m_pConnectionThread->StopRequest();
			DWORD waitState = WaitForSingleObject(hThread, 10000);
			if (waitState == WAIT_TIMEOUT)
			{
				TerminateThread(hThread, 1);

			}
			else if (waitState == WAIT_FAILED)
			{
				CString logStr;
				logStr.Format("WAIT_FAILED for worker hThread = 0x%x", hThread);
				trace(EVENTLOG_INFORMATION_TYPE, logStr);
			}
		}
	}
	catch (...)
	{
		CString logStr = "Exception in StopConnectionThread";
		trace(EVENTLOG_INFORMATION_TYPE, logStr);
	}
	m_pConnectionThread = NULL;
}

void ContainerBaseWnd::StartConnectionThread()
{
	try
	{

		m_pConnectionThread = (ConnectionWorkerThread*)AfxBeginThread(RUNTIME_CLASS(ConnectionWorkerThread));
		if (m_pConnectionThread != NULL)
		{
			WaitForSingleObject(m_pConnectionThread->m_startEvent, INFINITE);
			m_pConnectionThread->SetData(m_ServerURL, m_hWnd);
			m_connectionThreadInitialized = true;

		}
		else
		{

			log_error("Unable to start ConnectionWorkerThread");
			set_data_status(DataStatus::data_status_error, "Initialization failure");
		}
	}
	catch (...)
	{
		log_error("Exception while starting ConnectionWorkerThread");
		set_data_status(DataStatus::data_status_error, "Initialization failure");

	}

}

long ContainerBaseWnd::OnGetStripCurrentStatus(WPARAM wParam, LPARAM lParam)
{
	try
	{
		if (lParam != NULL)
		{
			


			CtrlContextStruct* pStripStatus = (CtrlContextStruct*)lParam;
			pStripStatus->m_is15MinView = !Is30MinView();
			pStripStatus->m_leftHalfShown = pStripStatus->m_is15MinView ?
				IsLeftPartShown() : false;
			if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
			{
				pStripStatus->m_MVU = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
				pStripStatus->m_toco = m_Tracing->is_visible(patterns_gui::tracing::wcrtracing);
				pStripStatus->m_events = m_Tracing->is_visible(patterns_gui::tracing::whideevents);
				pStripStatus->m_baselines = m_Tracing->is_visible(patterns_gui::tracing::wbaselines);
				pStripStatus->m_zoom = m_Tracing->is_zoomed();
				long tracingLeftInSeconds, tracingRightInSeconds;
				m_Tracing->GetCompressedViewBoundsInSeconds(m_tracingLeftInSeconds, m_tracingRightInSeconds);
				date compressedEndT = m_tracingLeftInSeconds;
				SYSTEMTIME t = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(compressedEndT));
				COleDateTime oleEndTime(t);
				DATE endCompressedTime = oleEndTime;
				pStripStatus->m_compressedStartTime = endCompressedTime;

				date startT, endT;
				m_Tracing->GetExpandedViewStartAndEnd(startT, endT);
				SYSTEMTIME tEnd = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(endT));
				COleDateTime oleTimeEnd(tEnd);
				DATE endTime = oleTimeEnd;
				pStripStatus->m_expandedEndTime = endTime;
				
				
				//{
				//	fstream fs;
				//	fs.open("c:\\temp\\setstatus.txt", fstream::app | fstream::in);
				//	fs << "original time" << oleTimeEnd.Format("%y/%m/%d %H:%M:%S") << "\r\n";
				//	fs << "original time in ms " << (long)endT << "\r\n";
				//	
				//	fs.close();
				//}

				bool isStripEnd = false;
				patterns::fetus* f = get_fetus();
				if (f != NULL)
				{
					long total = f->get_number_of_fhr() / f->get_hr_sample_rate();
					if (f->get_start_date() != undetermined_date && total <= endT - f->get_start_date() + 25 && m_Tracing->is_scrolling())
						isStripEnd = true;
				}
				pStripStatus->m_bScrollToEnd = isStripEnd;
			}
			return 1L;
		}
	}
	catch (...)
	{

	}
	return 0;
}

long ContainerBaseWnd::OnSetStripStatus(WPARAM wParam, LPARAM lParam)
{
	if (lParam != NULL)
	{
		
		CtrlContextStruct* pStripStatus = (CtrlContextStruct*)lParam;
		CtrlContextStruct stripStatus = *pStripStatus;

		ReplyMessage(0);
		if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
		{
			SetRedraw(FALSE);
			if (stripStatus.m_MVU != m_Tracing->is_visible(patterns_gui::tracing::wmontevideo))
			{
				//toggle_montevideo();
				ShowMVU(stripStatus.m_MVU, false, false);
			}

			if (stripStatus.m_toco != m_Tracing->is_visible(patterns_gui::tracing::wcrtracing))
			{
				toggle_toco();
			}

			if (stripStatus.m_events != m_Tracing->is_visible(patterns_gui::tracing::whideevents))
			{
				toggle_events();
			}

			if (stripStatus.m_baselines != m_Tracing->is_visible(patterns_gui::tracing::wbaselines))
			{
				toggle_baseline();
			}
			
			if (stripStatus.m_is15MinView == Is30MinView())
			{
				m_bIs30MinView = !stripStatus.m_is15MinView;
				m_Tracing = m_bIs30MinView ? m_Tracing30MinView : m_Tracing15MinView;
				tracing* pOther = m_bIs30MinView ? m_Tracing15MinView : m_Tracing30MinView;
				pOther->ShowWindow(SW_HIDE);
				pOther->EnableWindow(FALSE);
				m_Tracing->EnableWindow(TRUE);
				m_Tracing->ShowWindow(SW_SHOW);
				//m_Tracing->UpdateWindow();

			}

			if (!Is30MinView() && stripStatus.m_leftHalfShown != IsLeftPartShown())
			{
				ToggleSwitch(!stripStatus.m_leftHalfShown, true);
			}

			if (pStripStatus->m_bScrollToEnd)
			{
				go_end();
			}
			else
			{
				long compressedLeft, compressedRight;
				m_Tracing->GetCompressedViewBoundsInSeconds(compressedLeft, compressedRight);

				SYSTEMTIME sysT;
				COleDateTime compressedTimeT(stripStatus.m_compressedStartTime);
				compressedTimeT.GetAsSystemTime(sysT);
				SYSTEMTIME utcDest = fetus::convert_to_utc(sysT);
				date destinationTime = fetus::convert_to_time_t(utcDest);
				if (destinationTime != compressedLeft)
				{
					if (stripStatus.m_zoom != m_Tracing->is_zoomed())
					{
						Zoom(stripStatus.m_zoom);
					}

					m_Tracing->MoveCompressedViewToTime(destinationTime);
				}

				date startT, endT;
				m_Tracing->GetExpandedViewStartAndEnd(startT, endT);

				SYSTEMTIME  sEndT;
				COleDateTime oleEndT(stripStatus.m_expandedEndTime);
				oleEndT.GetAsSystemTime(sEndT);
				SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);
				date newEndTime = fetus::convert_to_time_t(utcEnd);

				if (newEndTime != endT)
				{
					m_Tracing->MoveToTime(newEndTime, true, false);
				}
				////////////////////////////////////////////
				SetRedraw(TRUE);
				RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
				//date startT_, endT_;
				//m_Tracing->GetExpandedViewStartAndEnd(startT_, endT_);
				//SYSTEMTIME tEnd_ = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(endT_));
				//COleDateTime oleTimeEnd_(tEnd_);
				//DATE endTime_ = oleTimeEnd_;
				
				//{
				//	fstream fs;
				//	fs.open("c:\\temp\\setstatus.txt", fstream::app | fstream::in);
				//	fs << "destination time" <<oleEndT.Format("%y/%m/%d %H:%M:%S") << "\r\n";
				//	fs << "actual time" << oleTimeEnd_.Format("%y/%m/%d %H:%M:%S") << "\r\n";
				//	fs << "actual time in ms" << (long)endT_ << "\r\n";
				//	fs.close();
				//}
	
				PostMessage(MSG_GOTO_TIME, 0, (LPARAM)newEndTime);
				////////////////////////////////////////
			}

			if (stripStatus.m_zoom != m_Tracing->is_zoomed())
			{
				Zoom(stripStatus.m_zoom);
			}
			SetRedraw(TRUE);
			RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
		}

	}
	return 0;
}

void ContainerBaseWnd::UpdateRoundExportValue()
{
	if (m_pPatternsAdapter != NULL && m_bExportEnabled)
	{
		RoundExportValueStruct rooundExportVal;
		rooundExportVal.m_roundBaselineFHRValue = m_roundBaselineFHRValue;
		rooundExportVal.m_roundByMontevideoUnits = m_roundByMontevideoUnits;
		m_pPatternsAdapter->SetRoundExportValue(rooundExportVal);
	}
}
long ContainerBaseWnd::OnToggleSwitchMsg(WPARAM wParam, LPARAM lParam)
{
	bool right = (wParam == 0);
	if (right)
	{
		m_Tracing->ToggleSwitchRight(false);
	}
	else 
	{
		m_Tracing->ToggleSwitchLeft(false);
	}
	return 0;
}

long ContainerBaseWnd::OnGoToTime(WPARAM wParam, LPARAM lParam)
{
	if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		if (wParam != NULL)
		{
			DATE* pNewTime = (DATE*)wParam;
			DATE newTime = *pNewTime;
			delete pNewTime;
			SYSTEMTIME  sEndT;
			COleDateTime oleEndT(newTime);
			oleEndT.GetAsSystemTime(sEndT);
			SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);
			date newEndTime = fetus::convert_to_time_t(utcEnd);
			m_Tracing->MoveToTime(newEndTime, true, false);
		}
		else
		{
			date endTime = lParam;
			m_Tracing->MoveToTime(endTime, true, false);
			RedrawWindow(NULL, NULL, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
		}
	}

	return 0;
}

void ContainerBaseWnd::SwitchToLeft15Min()
{
	if (Is30MinView())
		Switch15Min30MinViews(false, true, true);
	else
		ToggleSwitch(false, true);
}

void ContainerBaseWnd::SwitchToRight15Min()
{
	if (Is30MinView())
		Switch15Min30MinViews(false, true, false);
	else
		ToggleSwitch(true, true);
}

long ContainerBaseWnd::OnToggleSwitchNotification(WPARAM wParam, LPARAM lParam)
{	
	GetParent()->SendMessage(MSG_SWITCH_TOGGLED_MSG, wParam, 0);
	return 0;
}
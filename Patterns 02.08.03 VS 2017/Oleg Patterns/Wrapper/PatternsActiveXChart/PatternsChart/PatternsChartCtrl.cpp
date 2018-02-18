#include "stdafx.h"
#include "PatternsChartCtrl.h"
#include "patterns gui, services.h"
#include "patterns gui, double buffer.h"
#include "NativeManager.h"
#include "AboutBoxWrapper.h"
#include <fstream>



//#define PATTERNS_02_06_00 "02.06.00"
//#define PATTERNS_02_08_00 "02.08.00"
//#define PATTERNS_02_08_01 "02.08.01"
//#define PATTERNS_02_08_02 "02.08.02"
#define PATTERNS_UDI "*+B087PATTERNS02081/$$7020802D*"
using namespace patterns_gui;

BEGIN_MESSAGE_MAP(PatternsChartCtrl, COleControl)
	ON_WM_TIMER()
	ON_WM_ERASEBKGND()
	ON_WM_CREATE()
	ON_WM_DESTROY()
	ON_WM_SIZE()
	ON_OLEVERB(AFX_IDS_VERB_EDIT, OnEdit)
	ON_OLEVERB(AFX_IDS_VERB_PROPERTIES, OnProperties)

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
	ON_MESSAGE(SWITCH_15MIN_30MIN_VIEWS_MSG, OnSwitch15Min3MinViewsMsg)
	ON_MESSAGE(MSG_SETSLIDERPOSITION, OnSetSliderPosMsg)
	ON_MESSAGE(SWITCH_TOGGLED_CALLBACK_MSG, OnSwitchToggledCallbackMsg)
	ON_MESSAGE(VIEW_ZOOMED_CALLBACK_MSG, OnViewZoomedCallbackMsg)
	ON_MESSAGE(MSG_GOTO_SELECTEDINTERVAL, OnGoToSelectedIntervalMsg)
	ON_MESSAGE(VIEW_SWITCHTO15MIN_CALLBACK_MSG, OnSwitchTo15MinViewMsg)
END_MESSAGE_MAP()


PatternsChartCtrl::PatternsChartCtrl(void)
{		
	m_MVUButtonVisible = false;
	m_exportOpen = false;
	m_logo = 0;
	m_currentVersionIndex = 0;
	m_isFirstRequest = true;
	m_isInitialRequest = true;
	m_supportedPatternsVersions.Add(PATTERNS_02_08_02);
	m_supportedPatternsVersions.Add(PATTERNS_02_08_01);
	//m_supportedPatternsVersions.Add(PATTERNS_02_08_00);
	m_supportedPatternsVersions.Add(PATTERNS_02_06_00);
	m_supportedPatternsVersions.Add(PATTERNS_DEV_VERSION);

	m_skip_download = 0;
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
	m_ConnectionData = "";
	m_criHWND = NULL;

	m_DataStatus = data_status_no_data;
	m_DataStatusDetails = "Initializing...";

	m_bOptimizedDraw = true;


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

	m_tracingViewSizeInMinutes = DEFAUULT_TRACINGS_VIEW_SIZE_MINUTES;
	m_compressedViewSizeInMinutes = DEFAULT_COMPRESSED_VIEW_SIZE_MINUTES;

	m_patternsRatioCoefficient = patterns_ratio_coefficient(m_tracingViewSizeInMinutes, m_compressedViewSizeInMinutes);
}


PatternsChartCtrl::~PatternsChartCtrl(void)
{

	CoUninitialize();

	m_Initialized = false;

	if (m_hUser32 != NULL)
	{
		FreeLibrary(m_hUser32);
		m_hUser32 = NULL;
	}
	
	//if (m_Tracing)
	//{
	//	m_Tracing->set(NULL);
	//}

	if (m_Tracing30MinView)
	{
		m_Tracing30MinView->set(NULL);
	}

	if (m_Tracing15MinView)
	{
		m_Tracing15MinView->set(NULL);
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

	//if (m_Tracing != NULL)
	//{
	//	delete m_Tracing;
	//	m_Tracing = NULL;
	//}

	if (m_Tracing != NULL)
	{
		m_Tracing = NULL;
	}

	if (m_Tracing30MinView != NULL)
	{
		delete m_Tracing30MinView;
		m_Tracing30MinView = NULL;
	}

	if (m_Tracing15MinView != NULL)
	{
		delete m_Tracing15MinView;
		m_Tracing15MinView = NULL;
	}

	if (m_Message != NULL)
	{
		delete m_Message;
		m_Message = NULL;
	}

}



// Flags to customize MFC's implementation of ActiveX controls.
DWORD PatternsChartCtrl::GetControlFlags()
{
	DWORD dwFlags = COleControl::GetControlFlags();

	// The control can receive mouse notifications when inactive.
	dwFlags |= pointerInactive;

	// The control can optimize its OnDraw method, by not restoring the original GDI objects in the device context.
	dwFlags |= canOptimizeDraw;

	// Disables the call to IntersectClipRect made by COleControl and gains a small speed advantage
	dwFlags &= ~clipPaintDC;

	// eliminates extra drawing operations and the accompanying visual flicker. Use when your control draws itself identically in the inactive and active states
	dwFlags |= noFlickerActivate;

	return dwFlags;
}

// CRIPatternsChartCtrl message handlers
int PatternsChartCtrl::OnCreate(LPCREATESTRUCT s)
{
	if (COleControl::OnCreate(s) == -1)
		return -1;
	CRect clientRect;
	GetClientRect(clientRect);	
	m_tracingContainer.Create(NULL, NULL, WS_DISABLED | WS_CHILD, clientRect, this, 0);
	//m_tracingContainer.CreateEx(WS_EX_TRANSPARENT | WS_EX_NOPARENTNOTIFY, AfxRegisterWndClass(0), "",  WS_CHILD | LWS_TRANSPARENT| WS_DISABLED, clientRect, this, 0);
	
	m_Tracing15MinView->SetExportEnabled(m_bExportEnabled);
	m_Tracing30MinView->SetExportEnabled(m_bExportEnabled);

	CreateTracing(m_Tracing30MinView, m_bIs30MinView, TRACINGS_VIEW_SIZE_30_MINUTES);
	CreateTracing(m_Tracing15MinView, !m_bIs30MinView, TRACINGS_VIEW_SIZE_15_MINUTES);

	m_Message->Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	m_Message->set_format(DT_CENTER | DT_VCENTER | DT_WORDBREAK);

	CreateExportBar();
	initialize_commandbar();
	
	m_Tracing15MinView->set(get_fetus());
	m_Tracing30MinView->set(get_fetus());

	//// First download of data
	download_patient_data();

	//// Display the patient

	m_Tracing15MinView->zoom(false);
	m_Tracing30MinView->zoom(false);

	m_Tracing->move_to(get_fetus()->get_number_of_fhr() + 1000000);
	m_Tracing->scroll(true);

	// Reset the timer for refresh
	if (m_RefreshTimer != 0)
	{
		KillTimer(m_RefreshTimer);
		m_RefreshTimer = 0;
	}
		
	if (::IsWindow(m_hWnd))
	{
		m_RefreshTimer = SetTimer(1, m_RefreshDelay * 1000, NULL);
		InvalidateRgn(0, FALSE);
	}

	m_Initialized = true;

	return 0;
}

// Cleanup
void PatternsChartCtrl::OnDestroy()
{	
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
	COleControl::OnDestroy();
}

bool PatternsChartCtrl::CreateTracing(patterns_gui::tracing* pTracing, bool isVisible, int sizeInMinutes)
{
	UINT vsVisible = isVisible ? 0 : WS_VISIBLE;
	if (pTracing->Create(NULL, "", WS_CHILD | WS_TABSTOP | vsVisible | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200))
	{
		if (!isVisible)
			pTracing->EnableWindow(FALSE);
		pTracing->set_type(tracing::tnormal);
		pTracing->set_paper(tracing::pusa);
		pTracing->set_scaling_mode(tracing::spaper);
		pTracing->show_grid();
		pTracing->lock_scaling();
		pTracing->set_lengths(sizeInMinutes * 60, m_compressedViewSizeInMinutes * 60, SLIDER_VIEW_SIZE_SEC);
		pTracing->show(tracing::winformation);
		pTracing->show(tracing::whidedisconnected, false);
		pTracing->show(tracing::wbaselinevariability, true);
		pTracing->show(tracing::wcrtracing, true);
		return true;
	}
	return false;
}


// Get the data / updates from the server and refresh the display
void PatternsChartCtrl::download_patient_data()
{	
	// We don't come back from a critical error
	if (m_CriticalError)
		return;

	// Not initialized yet
	if (m_ServerURL.IsEmpty())
		return;

	// If there was recently an error talking to patterns server, skip the next xx call to reduce error count
	if (m_skip_download > 0)
	{
		--m_skip_download;
		return;
	}

	if (!m_DataLock.try_acquire())
		return;


	try
	{
		DownloadData();
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
}

void PatternsChartCtrl::DownloadData()
{
	// Request data from the server
	string response = utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("data"), utils::to_string(m_RequestHeader)) + "\r\n";

	// Failed?
	if (response.length() == 2) // 2 because of the + "\r\n"
	{
		m_skip_download = 4;

		ClearPatient();
		ClearRequest();

		set_data_status(data_status_error, "Unable to reach the server");
		return;
	}

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
	if (invalid_version != 0)
	{
		if (RetryInitialRequest() && m_currentVersionIndex <= m_supportedPatternsVersions.GetSize())
		{
			return;
		}

	}

	// Process the response		
	process_patient_data(rootNode);

}
///
/// Clean up the request to the bare minimum: the patient ID
///
void PatternsChartCtrl::ClearRequest()
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
void PatternsChartCtrl::ClearPatient()
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
void PatternsChartCtrl::perform_server_user_action(int actiontype, long artifact, string patient)
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

		utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("useraction"), utils::to_string(xmlDoc));

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

void PatternsChartCtrl::set_button(string name, icon_button* button)
{
	m_buttons[name] = button;
}

// Retrieve a specific command bar button by its name
icon_button* PatternsChartCtrl::get_button(string name)
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
void PatternsChartCtrl::initialize_bitmaps()
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
	services::create_bitmap_fragment("DATA_STATUS", "connected",	CRect(0, 0,						r.Width(),	r.Height() / 4));
	services::create_bitmap_fragment("DATA_STATUS", "error",		CRect(0, r.Height() / 4,		r.Width(),	(2 * r.Height()) / 4));
	services::create_bitmap_fragment("DATA_STATUS", "recovery",		CRect(0, (2 * r.Height()) / 4,	r.Width(),	(3 * r.Height()) / 4));
	services::create_bitmap_fragment("DATA_STATUS", "no_data",		CRect(0, (3 * r.Height()) / 4,	r.Width(),	r.Height()));
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
void PatternsChartCtrl::release_bitmaps()
{
#ifdef _DEBUG
	trace(EVENTLOG_INFORMATION_TYPE, "Releasing the bitmaps from memory");
#endif
	services::reset();
}

// Initialize the command bar by creating all the buttons
void PatternsChartCtrl::initialize_commandbar()
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
void PatternsChartCtrl::toggle_events(void)
{
	//m_Tracing->show(patterns_gui::tracing::whideevents, !m_Tracing->is_visible(patterns_gui::tracing::whideevents));
	bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::whideevents);
	m_Tracing30MinView->show(patterns_gui::tracing::whideevents, !isVisible);
	m_Tracing15MinView->show(patterns_gui::tracing::whideevents, !isVisible);

	Zoom(false);
	get_button("events")->set_icon((!m_Tracing->is_visible(patterns_gui::tracing::whideevents))?"action on":"action off");
}

// Toggle Hyperstimulation display Large/Small
void PatternsChartCtrl::toggle_toco(void)
{
	//m_Tracing->show(patterns_gui::tracing::wcrtracing, !m_Tracing->is_visible(patterns_gui::tracing::wcrtracing));
	bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::wcrtracing);
	m_Tracing30MinView->show(patterns_gui::tracing::wcrtracing, !isVisible);
	m_Tracing15MinView->show(patterns_gui::tracing::wcrtracing, !isVisible);	
	
	Zoom(false);
	get_button("toco")->set_icon((m_Tracing->is_visible(patterns_gui::tracing::wcrtracing))?"action on":"action off");
}

// Toggle Baseline display ON/OFF
void PatternsChartCtrl::toggle_baseline(void)
{
	//m_Tracing->show(patterns_gui::tracing::wbaselines, !m_Tracing->is_visible(patterns_gui::tracing::wbaselines));
	bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::wbaselines);
	m_Tracing30MinView->show(patterns_gui::tracing::wbaselines, !isVisible);
	m_Tracing15MinView->show(patterns_gui::tracing::wbaselines, !isVisible);
	Zoom(false);
	get_button("baseline")->set_icon((m_Tracing->is_visible(patterns_gui::tracing::wbaselines))? "action on" : "action off");
}

// Toggle Montevideo display ON/OFF
void PatternsChartCtrl::toggle_montevideo(void)
{
//	m_Tracing->show(patterns_gui::tracing::wmontevideo, !m_Tracing->is_visible(patterns_gui::tracing::wmontevideo));
	bool wasVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
	ShowMVU(!wasVisible, true);
	//m_Tracing30MinView->show(patterns_gui::tracing::wmontevideo, !wasVisible);
	//m_Tracing15MinView->show(patterns_gui::tracing::wmontevideo, !wasVisible);

	////m_Tracing->zoom(false);
	//Zoom(false);
	//bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
	//m_MVUButtonVisible = isVisible;
	//get_button("montevideo")->set_icon((isVisible)?"action on":"action off");
	//perform_server_user_action(isVisible? eMVUButtonOn : eMVUButtonOff, MVUButtonDummyArtifactID, get_patient()->get_id());
	//if(m_pPatternsAdapter)
	//{
	//	m_pPatternsAdapter->SetMontevideoVisible(isVisible);
	//}
}

void PatternsChartCtrl::ShowMVU(bool bShow, bool bNotify)
{
	bool wasVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
	if (bShow != wasVisible)
	{
		m_Tracing30MinView->show(patterns_gui::tracing::wmontevideo, !wasVisible);
		m_Tracing15MinView->show(patterns_gui::tracing::wmontevideo, !wasVisible);

		//m_Tracing->zoom(false);
		Zoom(false);
		bool isVisible = m_Tracing->is_visible(patterns_gui::tracing::wmontevideo);
		m_MVUButtonVisible = isVisible;
		get_button("montevideo")->set_icon((isVisible) ? "action on" : "action off");
		if(bNotify)
			perform_server_user_action(isVisible ? eMVUButtonOn : eMVUButtonOff, MVUButtonDummyArtifactID, get_patient()->get_id());
		if (m_pPatternsAdapter)
		{
			m_pPatternsAdapter->SetMontevideoVisible(isVisible);
		}
	}
}
// Tracing navigation. Go to the beginning of the tracing.
void PatternsChartCtrl::go_beginning(void)
{
	m_Tracing->move_to(0);
	tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
	pOther->move_to(0);
}

// Tracing navigation. Go to the end of the tracing.
void PatternsChartCtrl::go_end(void)
{
	m_Tracing->move_to(m_Tracing->get()->get_number_of_fhr() + 100);
	m_Tracing->scroll(true);
	tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
	pOther->move_to(pOther->get()->get_number_of_fhr() + 100);
}

// Tracing navigation. Go to the next page A page represent the width of the view.
void PatternsChartCtrl::go_next_page(void)
{
	if (m_Tracing->is_animating())
	{
		m_Tracing->stop_animating();
	}

	m_Tracing->move_page_animating(false);
}

// Tracing navigation. Go to the previous page A page represent the width of the view.
void PatternsChartCtrl::go_previous_page(void)
{
	if (m_Tracing->is_animating())
	{
		m_Tracing->stop_animating();
	}

	m_Tracing->move_page_animating();

}

// Tracing navigation. play back.
void PatternsChartCtrl::play_back(void)
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
void PatternsChartCtrl::play_forward(void)
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
void PatternsChartCtrl::pause(void)
{
	m_Tracing->stop_animating();
}

// Zoom / unzoom
void PatternsChartCtrl::toggle_zoom(void)
{
	//m_Tracing->zoom(!m_Tracing->is_zoomed());
	bool isZoomed = m_Tracing->is_zoomed();	
	m_Tracing15MinView->zoom(!isZoomed);
	m_Tracing30MinView->zoom(!isZoomed);
	ShowExportBar(!m_Tracing->is_zoomed());
}

// Update the data connection status : live / disconnected / error
void PatternsChartCtrl::set_data_status(data_status_code status, string msg)
{
	data_status_code wasStatus = m_DataStatus;
	
	// Once critical, always critical!
	if (!m_CriticalError && !m_bPluginLoadError)
	{
		m_DataStatus = status;
		m_DataStatusDetails = msg;
	}
	bool wasConnected = (wasStatus == data_status_connected || wasStatus == data_status_recovery);
	bool isConnected = (m_DataStatus == data_status_connected || m_DataStatus == data_status_recovery);
	if (::IsWindow(m_hWnd))
	{
		switch (m_DataStatus)
		{
		case data_status_connected:
			get_button("data status")->set_icon("connected");
			break;

		case data_status_no_data:
			get_button("data status")->set_icon("no_data");
			break;

		case data_status_recovery:
			get_button("data status")->set_icon("recovery");
			break;

		case data_status_error:
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
void PatternsChartCtrl::adjust_controls()
{
	CRect rect;
	GetClientRect(&rect);
	//if (!m_exportOpen && IsWindow(m_tracingContainer.m_hWnd))
	//{
	//	m_tracingContainer.MoveWindow(rect);
	//}

	int cx = rect.Width();
	int cy = rect.Height();

	m_Tracing->zoom(false);
	
	int actualExportBarHeight = EXPORT_BAR_HEIGHT;
	//int tracing_height = cy - ((display_patient_banner() ? BANNER_HEIGHT : 0) + BOTTOM_HEIGHT + actualExportBarHeight);
	
	//int tracing_required_width = this->calculate_required_tracing_width(tracing_height);
	m_lessThanMinimumSpace = false;

	int tracing_height = cy - ((display_patient_banner() ? BANNER_HEIGHT : 0) + BOTTOM_HEIGHT);
	int tracing30MinRequiredWidth = this->calculate_required_tracing_width(tracing_height, true);
	int tracing15MinRequiredWidth = this->calculate_required_tracing_width(tracing_height, false);
	int tracing_required_width = m_bIs30MinView ? tracing30MinRequiredWidth : tracing15MinRequiredWidth;

	if (cx < patterns_minimum_width || tracing_required_width < patterns_minimum_width)
	{
		m_lessThanMinimumSpace = true;
		// Calculate proper minimum resolution for message
		int minimum_height = calculate_required_tracing_height(patterns_minimum_width, m_bIs30MinView);
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
		if (m_bIs30MinView)
			m_Tracing15MinView->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
		else
			m_Tracing30MinView->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);

		m_Message->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
	}
	else
	{
		m_Message->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);

		if (m_bIs30MinView)
			m_Tracing15MinView->SetWindowPos(0, (cx - tracing15MinRequiredWidth) / 2, (display_patient_banner() ? BANNER_HEIGHT : 0), tracing15MinRequiredWidth, tracing_height, SWP_HIDEWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
		else
			m_Tracing30MinView->SetWindowPos(0, (cx - tracing30MinRequiredWidth) / 2, (display_patient_banner() ? BANNER_HEIGHT : 0), tracing30MinRequiredWidth, tracing_height, SWP_HIDEWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);

		if (tracing_required_width < cx)
		{
			int dY = cy - (tracing_height + BOTTOM_HEIGHT + actualExportBarHeight);
			if(dY < 0)
				dY = 0;
			m_Tracing->SetWindowPos(0, (cx - tracing_required_width) / 2, (display_patient_banner() ? BANNER_HEIGHT : 0) + dY, tracing_required_width, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
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

	button_width = get_button("zoom")->optimal_width() + 5;
	get_button("zoom")->SetWindowPos(0, left, top, button_width, 18, 0);
	left += button_width;

	get_button("data status")->SetWindowPos(0, left, top + 1, 101, 18, 0);
	left += 110;

	// From the right...
	button_width = get_button("about")->optimal_width() + 5;
	right -= button_width;
	get_button("about")->SetWindowPos(0, right, top, button_width, 18, 0);

	button_width = get_button("montevideo")->optimal_width() + 5;
	right -= button_width;
	get_button("montevideo")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);

	button_width = get_button("events")->optimal_width() + 5;
	right -= button_width;
	get_button("events")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);
	
	button_width = get_button("toco")->optimal_width() + 5;
	right -= button_width;
	get_button("toco")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);
	
	button_width = get_button("baseline")->optimal_width() + 5;
	right -= button_width;
	get_button("baseline")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);
	
	
	// In the middle?
	
	bool isLargeExportButtonVisible = false;
	bool isNavigationVisible = (right - left >= (7 * 18));//(right - left >= (7 * 18) + 20 + button_width);
	bool isExportButtonVisible = (right - left >= (7 * 18) + 20 + button_width);
	int exportBtnLeft = left;
	if(isExportButtonVisible)
	{		
		isLargeExportButtonVisible = (right - left >= (7 * 18) + 20 + button_width*2);
	}


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
		if(isNavigationVisible)
			exportBtnLeft = left + 10;

		if(isLargeExportButtonVisible)
		{
			button_width *=2;			
		}
		else
		{
			button_width += 10;
		}
		exportBtnLeft += (isLargeExportButtonVisible)? 28 : 10;
		CRect exportBarRect;
		m_exportBar.GetWindowRect(exportBarRect);
		ScreenToClient(exportBarRect);		
		m_exportButton.SetWindowPos(0, exportBtnLeft, exportBarRect.bottom, button_width, top - exportBarRect.bottom + 18, isExportButtonVisible? visible : hidden);
		
		if(m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized())
		{
			m_pPatternsAdapter->ResizeExportButton(0, 0, button_width, top - exportBarRect.bottom + 18);
		}
	}	

	m_Tracing->refresh_layout();
	ShowExportBar(m_bExportVisible);
	ResizeExportContainer();
}

// On resize of the control, reposition the command bar
//void PatternsChartCtrl::OnSize(UINT t, int cx, int cy)
//{
//	long leftIndex = -1;
//	bool isscrolling = false;
//	bool isLeftPartShown = true;
//	patterns_gui::tracing* pOtherTracing = m_bIs30MinView ? m_Tracing15MinView : m_Tracing30MinView;
//	if ((t == SIZE_RESTORED || t == SIZE_MAXIMIZED) && m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
//	{
//		isscrolling = m_Tracing->is_scrolling();
//		leftIndex = m_Tracing->GetSliderLeftIndex();
//		if (m_Tracing->IsToggleButtonAvailable())
//		{
//			isLeftPartShown = m_Tracing->IsLeftPartShown();
//		}
//	}
//	pOtherTracing->ShowWindow(SW_SHOWNA);
//
//	COleControl::OnSize(t, cx, cy);
//	adjust_controls();	
//
//	bool scrolling = m_Tracing->is_scrolling();
//
//	AdjustSliderAfterResize(isLeftPartShown, leftIndex);
//
//
//	if (pOtherTracing->IsToggleButtonAvailable())
//	{
//		pOtherTracing->move_to(leftIndex);
//		pOtherTracing->move_page(false);
//
//	}
//	else
//	{
//		pOtherTracing->move_to(leftIndex);
//	}
//	pOtherTracing->ShowWindow(SW_HIDE);
//
//}

void PatternsChartCtrl::OnSize(UINT t, int cx, int cy)
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

void PatternsChartCtrl::AdjustSliderAfterResize(bool isLeftPartShown, long leftIndex)
{
	if (m_Tracing->IsToggleButtonAvailable())
	{
		m_Tracing->move_to(leftIndex);

		m_Tracing->move_page(false);
		if (isLeftPartShown)
		{
			m_Tracing->ToggleSwitchLeft();
			long index = m_Tracing->GetSliderLeftIndex();
			if (index <= 0)
				m_Tracing->move_to(0);

		}
	}
	else
	{
		m_Tracing->move_to(leftIndex);
	}


}

/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// DRAWING

int PatternsChartCtrl::calculate_required_tracing_width(int height, bool for30MinView)
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
	//return (int)((height - ((4 * (line_height + 1)) + patterns_fix_required_height)) / m_patternsRatioCoefficient);
	if (for30MinView)
		return (int)((height - ((4 * (line_height + 1)) + patterns_fix_required_height)) / m_patterns30MinRatioCoefficient);
	else
		return (int)((height - ((4 * (line_height + 1)) + patterns_fix_required_height)) / m_patterns15MinRatioCoefficient);

}

int PatternsChartCtrl::calculate_required_tracing_height(int width, bool for30MinView)
{
	static int line_height = -1;

	// Calculate a line of text height
	if (line_height < 0)
	{
		CPaintDC dc(this);
		services::select_font(80, "Arial", &dc);
		line_height = dc.GetTextExtent("W").cy;
	}

	//return (int)((width * m_patternsRatioCoefficient) + (4 * (line_height + 1)) + patterns_fix_required_height);
	if (for30MinView)
		return (int)((width * m_patterns30MinRatioCoefficient) + (4 * (line_height + 1)) + patterns_fix_required_height);
	else
		return (int)((width * m_patterns15MinRatioCoefficient) + (4 * (line_height + 1)) + patterns_fix_required_height);

}

// Update the banner
void PatternsChartCtrl::refresh_banner(CDC* pdc)
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
void PatternsChartCtrl::refresh_command_bar(CDC* pdc)
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

// PatternsChartCtrl::OnDraw - Drawing function
void PatternsChartCtrl::OnDraw(CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid)
{
	if (!pdc)
		return;

	double_buffer dbDC(pdc, &rcBounds);
	dbDC.FillSolidRect(rcBounds, RGB(190,190,190));
	refresh_banner(dbDC);
	refresh_command_bar(dbDC);
	
}

// Optimization for drawing
BOOL PatternsChartCtrl::OnEraseBkgnd(CDC *)
{
	return TRUE;
}

// Timer refresh...
void PatternsChartCtrl::OnTimer(UINT_PTR timer)
{
	if ((m_Initialized) && (!m_DemoMode) && (timer == m_RefreshTimer))
	{
		download_patient_data();
	}

	COleControl::OnTimer(timer);
}

/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// LOGS

// An exception happened...
void PatternsChartCtrl::log_exception(string msg, exception& e)
{
	log_exception(msg + "\r\n\r\n" + e.what());
}

// An exception happened...
void PatternsChartCtrl::log_exception(string msg)
{
	CString text;
	//text.LoadString(an_error_happened);
	text = LoadTextAnErrorHappened();

	CString message = text + "\r\n\r\n" + msg.c_str();
	log_error((string)message);
}

// An error happened...
void PatternsChartCtrl::log_error(string msg)
{
	trace(EVENTLOG_ERROR_TYPE, msg.c_str());

	ClearPatient();
	ClearRequest();

	set_data_status(data_status_error, msg);
}

// Trace information in the Microsoft Windows Event log
void PatternsChartCtrl::trace(WORD type, LPCTSTR szFormat, ...)
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
			m_hEventLog = RegisterEventSource(NULL, _T("PeriGen Patterns ActiveX"));
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

void PatternsChartCtrl::CreateExportBar()
{
	if(!IsWindow(m_exportBar.m_hWnd))
	{
		CRect clientRect;
		GetClientRect(clientRect);
		CRect exportRect(clientRect.left, clientRect.bottom, clientRect.right, clientRect.bottom);
		m_exportBar.Create(NULL, NULL, WS_CHILD , exportRect, this, 0);
		m_emptyBar.Create(NULL, NULL, WS_CHILD | WS_DISABLED, exportRect, this, 0);
	}
	CreateExportContainer();

}

void PatternsChartCtrl::UpdatePatternsAdapter()
{
	if(!m_bExportEnabled  || m_Tracing->is_zoomed())
		return;
	bool valid = (m_gaTotalDays >= 252 && GetNumberOfFetuses() == 1);
	if(m_pPatternsAdapter != NULL)
	{

		m_Tracing->GetCompressedViewBoundsInSeconds(m_tracingLeftInSeconds, m_tracingRightInSeconds);
		if(!m_pPatternsAdapter->IsInitialized())
		{
			double defaultWidthInMinutes = 255;
			long widthInSeconds = m_tracingRightInSeconds - m_tracingLeftInSeconds;

			date startT = m_tracingRightInSeconds;
			SYSTEMTIME t = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(startT));
			COleDateTime oleTime(t);
			DATE startTime = oleTime;	
			m_pPatternsAdapter->Init(startTime, defaultWidthInMinutes);
			m_pPatternsAdapter->SetPluginURL(m_ServerURL);
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
			
			if (m_bExportEnabled && IsWindow(m_exportContainer.m_hWnd))
			{
				CRect containerRect;
				m_exportContainer.GetWindowRect(containerRect);
				m_pPatternsAdapter->InitExportContainer((long)m_exportContainer.m_hWnd, 0, 0, containerRect.Width(), containerRect.Height());
			}

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

void PatternsChartCtrl::VisibleChunksToAdapter()
{
	
	if(m_pPatternsAdapter && m_pPatternsAdapter->IsInitialized() && m_gaTotalDays >= 252 && GetNumberOfFetuses() == 1)
	{
		tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
		
		m_Tracing->ResetVisibleChunks();
		pOther->ResetVisibleChunks();

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
						m_Tracing->AddVisibleChunkEnd(leftInPixels);
						pOther->AddVisibleChunkEnd(leftInPixels);
					}
					olet += COleDateTimeSpan(0, 0, it->GetTimeRange(), 0);
					int endMinute = olet.GetMinute();
					if(endMinute == 0 || endMinute == 30)
					{					
						m_Tracing->AddVisibleChunkEnd(rightInPixels);
						pOther->AddVisibleChunkEnd(rightInPixels);
					}				

					m_pPatternsAdapter->AddChunk(it->m_exportID, it->m_id, it->GetStartDATE(), it->GetTimeRange(), it->IsExported(), leftInPixels, rightInPixels);

				
				}
			}		

		}
		m_pPatternsAdapter->EndUpdateChunks();
	}
	
}



void PatternsChartCtrl::LoadExportableIntervals(TiXmlElement* intervalsNode)
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
int PatternsChartCtrl::FindIntervalByStartTime(time_t startTime)
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

void PatternsChartCtrl::AddExportableInterval(long basetime, string data)
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
		PatternsExportableChunkNative chunk(basetime + startTime, timeRange, exportID >= 0);
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
			else if (!m_exportableIntervals.at(oldIndex).IsExported())
			{
				m_exportableIntervals.at(oldIndex) = chunk;
			}
		}
		else
		{
			int oldIndex = FindIntervalByID(id);
			if (oldIndex != -1 && oldIndex < m_exportableIntervals.size())
			{
				m_exportableIntervals.at(oldIndex) = chunk;
			}
		}
	}
}

int PatternsChartCtrl::FindIntervalByID(long id)
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

bool PatternsChartCtrl::IsIntervalExported(long id)
{
	int index = FindIntervalByID(id);
	if(index != -1)
	{
		return (m_exportableIntervals[index].m_exportID >= 0);
	}
	return false;
}
long PatternsChartCtrl::OnForceUpdateAdapterMessage(WPARAM wParam, LPARAM lPararm)
{
	if(m_bExportEnabled)
		UpdatePatternsAdapter();
	return 0;
}


long PatternsChartCtrl::OnExportBarMouseOverCallbackMsg(WPARAM wParam, LPARAM lParam)
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
		
		if (wParam != 0 && !isExported)
		{
			m_exportOpen = true;
			ResizeExportContainer();
		}
		m_Tracing->SetHighlightedExportTime(from, to, wParam != 0, isExported);
		pOther->SetHighlightedExportTime(from, to, wParam != 0, isExported);
	}
	return 0;
}

long PatternsChartCtrl::OnExportBarMouseLeaveCallbackMsg(WPARAM wParam, LPARAM lParam)
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
		if (wParam != 0 && m_exportOpen)
		{
			m_exportOpen = false;
			ResizeExportContainer();

		}
		m_Tracing->ResetHighlightedExportTime(from, to, wParam != 0);
		pOther->ResetHighlightedExportTime(from, to, wParam != 0);
	}
	return 0;

}
long PatternsChartCtrl::OnShowExportBarMsg(WPARAM wParam, LPARAM lParam)
{
	m_bExportVisible = (wParam == 1);
	if(!m_bExportEnabled)
		return 0;
	bool show = (wParam == 1);
	ShowExportBar(show);	
	return 0;
}
void PatternsChartCtrl::RefreshExportBarWnd(CDC* pdc)
{

	if(IsWindow(m_exportBar.m_hWnd))
	{
		CRect barRect;
		m_exportBar.GetWindowRect(barRect);
		ScreenToClient(barRect);
		pdc->FillRect(barRect, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	}
}
void PatternsChartCtrl::ShowExportBar(bool show)
{	
	m_bExportVisible = show;
	if(m_Tracing != NULL && IsWindow(m_exportBar.m_hWnd) && IsWindow(m_exportButton.m_hWnd) && m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd)
		/*&& m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized()*/)
	{
		tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
		bool valid = (m_bExportEnabled && m_gaTotalDays >= 252 && GetNumberOfFetuses() == 1);
		bool isConnected = (m_DataStatus == data_status_connected || m_DataStatus == data_status_recovery);
		CRect barRect;
		m_exportBar.GetWindowRect(barRect);
		ScreenToClient(barRect);
		CRect btnRect;
		m_exportButton.GetWindowRect(btnRect);
		ScreenToClient(btnRect);
		
		const UINT visible = SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED;
		const UINT hidden = SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE;
		bool bShowExport = !m_Tracing->is_zoomed() && show && isConnected && m_bExportEnabled;
		m_exportBar.ShowWindow(bShowExport? SW_SHOW : SW_HIDE);
		m_exportButton.ShowWindow(!m_Tracing->is_zoomed() && show && isConnected && valid? SW_SHOW : SW_HIDE);			
		m_emptyBar.ShowWindow(!bShowExport? SW_SHOW : SW_HIDE);


		 if(m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized())
		 {
			if(!m_Tracing->is_zoomed() && show && isConnected && m_bExportEnabled)
			{	
				m_emptyBar.SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW);
				m_pPatternsAdapter->ResizeNavigationPanel(0, 0, barRect.Width(), barRect.Height());
				if(valid)
				{
					m_pPatternsAdapter->ResizeExportButton(0,0, btnRect.Width(), btnRect.Height());
				}
				else
				{
					m_pPatternsAdapter->ResizeExportButton(0,0,0,0);
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
void PatternsChartCtrl::Zoom(bool zoom)
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
void PatternsChartCtrl::PerformUserPluginAction(string element, long value, int actiontype)
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

		utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("pluginaction"), utils::to_string(xmlDoc));
	
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

long PatternsChartCtrl::OnTimeRangeChangeCallbackMsg(WPARAM wParam, LPARAM lParam)
{
	long timeRange = (int)lParam;
	ReplyMessage(0);
	PerformUserPluginAction(string("intervalduration"), timeRange, 1);
	return 0;
}

void PatternsChartCtrl::UpdateExportTimeRange(TiXmlElement* requestNode)
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

long PatternsChartCtrl::GetNumberOfFetuses()
{
	input_adapter::patient* pPatient = get_patient();
	if(pPatient)
	{
		return pPatient->get_number_of_fetuses();
	}
	return 0;
}

date PatternsChartCtrl::GetValidGAStart()
{	
	fetus* curFetus = get_fetus();
	if(curFetus && curFetus->has_cutoff_date())
	{		
		return fetus::convert_to_local(curFetus->get_cutoff_date());
		
	}
	return 0;
}

void PatternsChartCtrl::ParseGA(string GA)
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

void PatternsChartCtrl::LoadEnabledPlugins()
{
	m_bExportEnabled = false;
	m_bPluginLoadError = false;
	try
	{
	// Request data from the server
		string response = utils::perform_server_request(string((LPCSTR)m_ServerURL) + string("Plugins")) + "\r\n";
		
		// Failed?
		if (response.length() == 2) // 2 because of the + "\r\n"
		{				
			set_data_status(data_status_error, "Unable to reach the server");
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
			set_data_status(data_status_error, "Unable to reach the server");
			m_bPluginLoadError = true;
			return;
		}
		TiXmlElement* node = pliginsnode->FirstChildElement("Plugin");
		while( node )
		{
			string value = node->Attribute("Name");
			if(value.c_str() != NULL)
			{				
				if( value.compare(EXPORT_PLUGIN_NAME) == 0)
				{
					m_bExportEnabled =  true;
					break;
				}
			}
			node=node->NextSiblingElement();
		}

		if(m_Tracing)
		{
			m_Tracing->SetExportEnabled(m_bExportEnabled);
			tracing* pOther = (m_bIs30MinView) ? m_Tracing15MinView : m_Tracing30MinView;
			pOther->SetExportEnabled(m_bExportEnabled);
		}
		if(m_bExportEnabled && m_pPatternsAdapter == NULL)
		{
			m_pPatternsAdapter = new NativeManager();		
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

CString PatternsChartCtrl::GetExportNotEnabledMessage(DATE& endTime)
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

long PatternsChartCtrl::OnGetExportCalculatedEntriesEx(WPARAM wParam, LPARAM lParam)
{	
	ExportEntriesStruct* pEntries = (ExportEntriesStruct*)wParam;
	EportBarActiveChunck* pChunk = (EportBarActiveChunck*)lParam;
	if(pEntries && pChunk && m_Tracing)
	{
		
		DATE from = pChunk->m_from;
		DATE to = pChunk->m_to;
		long id = pChunk->m_intervalID;
		pEntries->m_is15MinView = !m_bIs30MinView;
		pEntries->m_bIsLeftPartShown = m_Tracing->IsLeftPartShown();
		bool contractionThresholdExceeded = false;
		m_Tracing->DoOnExportDlgCalculatedEntriesRequestEx(from, to, pEntries->m_meanContructions, pEntries->m_meanBaseline, pEntries->m_meanBaselineVariability, pEntries->m_montevideo, contractionThresholdExceeded);
		
	}
	return 0;
}

//void PatternsChartCtrl::Switch15Min30MinViews(bool externalCall)
//{
//	bool isscrolling = m_Tracing->is_scrolling();
//	bool iszoomed = m_Tracing->is_zoomed();
//	long index = m_Tracing->GetSliderLeftIndex();
//	m_bIs30MinView = !m_bIs30MinView;
//	m_Tracing = m_bIs30MinView ? m_Tracing30MinView : m_Tracing15MinView;
//	tracing* pOther = !m_bIs30MinView ? m_Tracing30MinView : m_Tracing15MinView;
//	m_Tracing->zoom(iszoomed);
//	m_Tracing->scroll(isscrolling);
//	m_Tracing->move_to(index);
//	bool goToSelectedInterval = false;
//	if (!m_bIs30MinView && m_Tracing->IsExportDlgOpenFor15MinInterval())
//	{
//		goToSelectedInterval = true;
//	}
//	if (!m_bIs30MinView && !goToSelectedInterval)
//	{		
//		m_Tracing->move_page(false);
//	}
//
//
//	pOther->ShowWindow(SW_HIDE);
//	pOther->EnableWindow(FALSE);
//
//	m_Tracing->EnableWindow(TRUE);
//	m_Tracing->ShowWindow(SW_SHOW);
//	m_Tracing->UpdateWindow();
//	m_Tracing->Invalidate();
//	
//	if (!goToSelectedInterval)
//	{
//		long newindex = m_Tracing->GetSliderLeftIndex();
//
//		PostMessage(MSG_SETSLIDERPOSITION, (WPARAM)isscrolling, (LPARAM)index);
//	}
//	else
//	{
//		PostMessage(MSG_GOTO_SELECTEDINTERVAL, 0, 0);
//	}
//}

void PatternsChartCtrl::Switch15Min30MinViews(bool externalCall, bool left15min)
{
	bool isscrolling = m_Tracing->is_scrolling();
	bool iszoomed = m_Tracing->is_zoomed();
	long index = m_Tracing->GetSliderLeftIndex();
	bool isLeftPartShown = !m_bIs30MinView && m_Tracing->IsLeftPartShown();

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
			if (left15min && index > 0)
			{
				newEndTime -= 900;
				m_Tracing->ToggleSwitchLeft(false);
			}
			else
				m_Tracing->ToggleSwitchRight(false);
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


	if (goToSelectedInterval)
	{
		PostMessage(MSG_GOTO_SELECTEDINTERVAL, 0, 0);
	}

}

long PatternsChartCtrl::OnSwitch15Min3MinViewsMsg(WPARAM wParam, LPARAM lParam)
{
	Switch15Min30MinViews();
	return 0;
}

long PatternsChartCtrl::OnSetSliderPosMsg(WPARAM wParam, LPARAM lParam)
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

long PatternsChartCtrl::OnGoToSelectedIntervalMsg(WPARAM wParam, LPARAM lParam)
{
	m_Tracing->MoveToHighlited15MinExportInterval();
	return 0;
}


void PatternsChartCtrl::AddBuildVersionToSupportedVersions(const CString buildFileVersion)
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

void PatternsChartCtrl::PrepareInitialRequestHeader()
{
	m_RequestHeader.Clear();
	CString str;
	str.Format("<patients version=\"%s\"><request key=\"0\"/></patients>\r\n", m_currentPatternsVersion);
	//m_RequestHeader.Parse("<patients version=\"" FILEVERSTR "\"><request key=\"0\"/></patients>\r\n");
	m_RequestHeader.Parse(str);
	m_RequestHeader.FirstChildElement("patients")->SetAttribute("user", m_UserID);
	m_RequestHeader.FirstChild("patients")->FirstChildElement("request")->SetAttribute("key", m_patientID);
}

bool PatternsChartCtrl::RetryInitialRequest()
{
	if (m_isInitialRequest)
	{
		m_currentVersionIndex++;
		if (m_currentVersionIndex < m_supportedPatternsVersions.GetSize())
		{
			
			m_currentPatternsVersion = m_supportedPatternsVersions[m_currentVersionIndex];

			PrepareInitialRequestHeader();
			DownloadData();
			return true;

		}
		else
		{
			m_isInitialRequest = false;
		}

	}
	return false;

}

void PatternsChartCtrl::ShowAboutDialog(const CString& url, bool bCheckList, bool isChecklistApp, int timeout)
{	
	OpenAboutBox(0, bCheckList, url, PATTERNS_UDI, isChecklistApp, timeout);
}

void PatternsChartCtrl::CreateExportContainer()
{
	if (!IsWindow(m_exportContainer.m_hWnd))
	{
		CRect clientRect;
		GetClientRect(clientRect);
		CRect exportRect(clientRect.left, clientRect.bottom, clientRect.left, clientRect.bottom);		
	
		if (m_exportContainer.Create(NULL, NULL, WS_CHILD | WS_DISABLED, exportRect, this, 0))
		{
			
		}
	}
}

void PatternsChartCtrl::ResizeExportContainer()
{
	if (IsWindow(m_exportContainer.m_hWnd))
	{
		CRect wasExportRect;
		m_exportContainer.GetWindowRect(wasExportRect);
		ScreenToClient(wasExportRect);

		CRect clientRect;
		GetClientRect(clientRect);
		CRect exportRect(clientRect.left, clientRect.bottom, clientRect.left, clientRect.bottom);
		
		if (m_Tracing != NULL && m_exportOpen && m_Tracing15MinView != NULL)
		{
			CRect upRect = m_Tracing15MinView->GetUpRect();
			
			if (!upRect.IsRectEmpty())
			{
				ScreenToClient(upRect);
				exportRect.top = upRect.bottom;		
				exportRect.right = clientRect.right;	
			
			}
		}

		//if (!wasExportRect.EqualRect(exportRect))
		if (wasExportRect.Width() != exportRect.Width() || wasExportRect.Height() != exportRect.Height())
		{
			SetRedraw(FALSE);
			if (exportRect.IsRectEmpty())
			{
				m_tracingContainer.EnableWindow(FALSE);
				m_tracingContainer.MoveWindow(clientRect);
				m_tracingContainer.ShowWindow(SW_HIDE);
				ReplaceChildrenParent(false);
				m_exportContainer.EnableWindow(FALSE);
				m_exportContainer.ShowWindow(SW_HIDE);
			}
			else
			{
				clientRect.bottom = exportRect.top - 1;
				m_tracingContainer.MoveWindow(clientRect);
				m_tracingContainer.EnableWindow(FALSE);
				m_tracingContainer.ShowWindow(SW_SHOW);
				ReplaceChildrenParent(true);

				m_exportContainer.EnableWindow(TRUE);
				m_exportContainer.ShowWindow(SW_SHOW);
				
			}
			
			m_exportContainer.MoveWindow(exportRect);
			SetRedraw(TRUE);
			if (m_pPatternsAdapter != NULL && m_pPatternsAdapter->IsInitialized())
			{
				m_pPatternsAdapter->ResizeExportContainer(0, 0, exportRect.Width(), exportRect.Height());
			}
			
			RedrawWindow(NULL, NULL, RDW_ERASE | RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);
		}
	}
}

void PatternsChartCtrl::ReplaceChildrenParent(bool bToContainer)
{
	CWnd* pParent = bToContainer ? &m_tracingContainer : (CWnd*)this;
	
	for (std::map < string, icon_button*>::iterator it = m_buttons.begin(); it != m_buttons.end(); ++it)
	{
		icon_button* button = it->second;
		if (button != NULL && IsWindow(button->m_hWnd))
		{
			button->SetParent(pParent);
		}
	}
	if (m_Message != NULL && IsWindow(m_Message->m_hWnd))
	{
		m_Message->SetParent(pParent);
	}

	if (m_Tracing30MinView != NULL && IsWindow(m_Tracing30MinView->m_hWnd))
	{
		m_Tracing30MinView->SetParent(pParent);
	}
	if (m_Tracing15MinView != NULL && IsWindow(m_Tracing15MinView->m_hWnd))
	{
		m_Tracing15MinView->SetParent(pParent);
	}
	if (IsWindow(m_exportBar.m_hWnd))
	{
		m_exportBar.SetParent(pParent);
	}
	if (IsWindow(m_emptyBar.m_hWnd))
	{
		m_emptyBar.SetParent(pParent);
	}
	if (IsWindow(m_exportButton.m_hWnd))
	{
		m_exportButton.SetParent(pParent);
	}
}

void PatternsChartCtrl::ToggleSwitch(bool right)
{
	if (m_Tracing != NULL && IsWindow(m_Tracing->m_hWnd))
	{
		if (right)
		{
			m_Tracing->ToggleSwitchRight();
		}
		else
		{
			m_Tracing->ToggleSwitchLeft();
		}
	}
}

long PatternsChartCtrl::OnSwitchToggledCallbackMsg(WPARAM wParam, LPARAM lParam)
{
	bool bRight = (wParam != 0);
	if (bRight == m_Tracing->IsLeftPartShown() && !m_Tracing->IsExportDlgOpenFor15MinInterval())
	{		
		ToggleSwitch(bRight);
	}
	return 0;
}

long PatternsChartCtrl::OnViewZoomedCallbackMsg(WPARAM wParam, LPARAM lParam)
{
	bool to15Min = (wParam != 0);
	if (to15Min == m_bIs30MinView)
	{
		Switch15Min30MinViews(true);
	}
	return 0;
}

long PatternsChartCtrl::OnSwitchTo15MinViewMsg(WPARAM wParam, LPARAM lParam)
{
	bool left = (wParam == 0);
	if (m_bIs30MinView)
	{
		Switch15Min30MinViews(true, left);
	}
	else 
	{
		ToggleSwitch(!left);
	}
	return 0;
}
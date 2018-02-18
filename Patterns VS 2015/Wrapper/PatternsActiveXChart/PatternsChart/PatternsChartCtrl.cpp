#include "stdafx.h"
#include "PatternsChartCtrl.h"
#include "patterns gui, services.h"
#include "patterns gui, double buffer.h"
#include "NativeManager.h"
#include <fstream>



// Convert a UTF-8 encoded string to a wstring
std::wstring utf8_to_wstr(const std::string &utf8)
{
	if (utf8 == "")
	{
		return L"";
	}

	std::vector<wchar_t> wbuff(utf8.length() + 1);
	if (!MultiByteToWideChar(CP_UTF8, 0,  utf8.c_str(), utf8.length(), &wbuff[0], utf8.length() + 1))
	{
		DWORD e = ::GetLastError();
		switch(e)
		{
			case ERROR_INSUFFICIENT_BUFFER:
			case ERROR_INVALID_FLAGS:
			case ERROR_INVALID_PARAMETER:
			case ERROR_NO_UNICODE_TRANSLATION:
				return L"Error";
			default:
				break;
		}
	}

	return &wbuff[0];
}
// Convert a wstring to a UTF-8 encoded string
std::string wstr_to_str( const std::wstring& wstr )
{
	if(wstr == L"")
		return "";

	std::vector<char> buff(wstr.length() + 1);

	size_t size = wstr.length();
	WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), size, &buff[0], size+1, NULL, NULL );
	return &buff[0];
}

// Due to the way data is retrieved from an XML send by a C# application, the accent characters (and all 'special' characters and not
// properly encoded in ANSI, doing a UTF8 to wstring then back to ansi fixes that
string& cleanString(string& str)
{
	wstring ws = utf8_to_wstr(str);
	str = wstr_to_str(ws);
	return str;
}

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
END_MESSAGE_MAP()


PatternsChartCtrl::PatternsChartCtrl(void)
{
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
	
	if (m_Tracing)
	{
		m_Tracing->set(NULL);
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

	if (m_Tracing != NULL)
	{
		delete m_Tracing;
		m_Tracing = NULL;
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
	

	//if(m_pPatternsAdapter == NULL)
	//{
	//	m_pPatternsAdapter = new NativeManager();		
	//}
	//if(m_pPatternsAdapter != NULL)
	//	m_pPatternsAdapter->SetPatternsControl(m_hWnd);
	m_Tracing->SetExportEnabled(m_bExportEnabled);
	m_Message->Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	m_Message->set_format(DT_CENTER | DT_VCENTER | DT_WORDBREAK);
	m_Tracing->Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	m_Tracing->set_type(tracing::tnormal);
	m_Tracing->set_paper(tracing::pusa);
	m_Tracing->set_scaling_mode(tracing::spaper);
	m_Tracing->show_grid();
	m_Tracing->lock_scaling();
	m_Tracing->set_lengths(TRACING_VIEW_SIZE_SEC, COMPRESS_VIEW_SIZE_SEC);
	m_Tracing->show(tracing::winformation);
	m_Tracing->show(tracing::whidedisconnected, false);
	m_Tracing->show(tracing::wbaselinevariability, true);
	m_Tracing->show(tracing::wcrtracing, true);

	CreateExportBar();
	initialize_commandbar();

	//// First download of data
	download_patient_data();

	//// Display the patient
	m_Tracing->set(get_fetus());
	m_Tracing->zoom(false);
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

// Read the 'demo' data
void PatternsChartCtrl::download_demo_data()
{
	try
	{
		// Already loaded?
		if (m_ConnectionData.GetLength() == 0)
			return;

		TiXmlDocument xmlDoc;
		string configuration = base64::base64_decode((LPCTSTR)m_ConnectionData) + "\r\n";

		if (xmlDoc.Parse(configuration.c_str()) == 0)
			throw exception("The connection data is not a valid xml");

		TiXmlElement* node = xmlDoc.FirstChildElement("configuration");
		if (node == NULL)
			throw exception("The connection data does not contain the 'configuration' node");

		TiXmlElement* data = node->FirstChildElement("data");
		if (data == NULL)
			throw exception("The connection data does not contain the 'data' node");

		process_patient_data(data);

		// Flush connection data that is no useless
		m_ConnectionData = "";
	}
	catch (exception& e)
	{
		log_exception("Error while processing demo data", e);
	}
	catch (...)
	{
		log_exception("Unknown error while processing demo data");
	}
}

// Get the data / updates from the server and refresh the display
void PatternsChartCtrl::download_patient_data()
{	
	// We don't come back from a critical error
	if (m_CriticalError)
		return;

	// Not initialized yet
	if (m_ConnectionData.GetLength() == 0)
		return;

	// In demo mode, we don't use a server!
	if (m_DemoMode)
	{
		download_demo_data();
		return;
	}

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

		// Process the response
		process_patient_data(rootNode);
	
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
	get_button("baseline")->set_icon("action on");
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
	m_Tracing->show(patterns_gui::tracing::whideevents, !m_Tracing->is_visible(patterns_gui::tracing::whideevents));
	//m_Tracing->zoom(false);
	Zoom(false);
	get_button("events")->set_icon((!m_Tracing->is_visible(patterns_gui::tracing::whideevents))?"action on":"action off");
}

// Toggle Hyperstimulation display Large/Small
void PatternsChartCtrl::toggle_toco(void)
{
	m_Tracing->show(patterns_gui::tracing::wcrtracing, !m_Tracing->is_visible(patterns_gui::tracing::wcrtracing));
	//m_Tracing->zoom(false);
	Zoom(false);
	get_button("toco")->set_icon((m_Tracing->is_visible(patterns_gui::tracing::wcrtracing))?"action on":"action off");
}

// Toggle Baseline display ON/OFF
void PatternsChartCtrl::toggle_baseline(void)
{
	m_Tracing->show(patterns_gui::tracing::wbaselines, !m_Tracing->is_visible(patterns_gui::tracing::wbaselines));
	//m_Tracing->zoom(false);
	Zoom(false);
	get_button("baseline")->set_icon((m_Tracing->is_visible(patterns_gui::tracing::wbaselines))?"action off":"action on");
}

// Toggle Montevideo display ON/OFF
void PatternsChartCtrl::toggle_montevideo(void)
{
	m_Tracing->show(patterns_gui::tracing::wmontevideo, !m_Tracing->is_visible(patterns_gui::tracing::wmontevideo));
	//m_Tracing->zoom(false);
	Zoom(false);
	get_button("montevideo")->set_icon((m_Tracing->is_visible(patterns_gui::tracing::wmontevideo))?"action on":"action off");

	if(m_pPatternsAdapter)
	{
		m_pPatternsAdapter->SetMontevideoVisible(m_Tracing->is_visible(patterns_gui::tracing::wmontevideo));
	}
}

// Tracing navigation. Go to the beginning of the tracing.
void PatternsChartCtrl::go_beginning(void)
{
	m_Tracing->move_to(0);
}

// Tracing navigation. Go to the end of the tracing.
void PatternsChartCtrl::go_end(void)
{
	m_Tracing->move_to(m_Tracing->get()->get_number_of_fhr() + 100);
	m_Tracing->scroll(true);
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
	m_Tracing->zoom(!m_Tracing->is_zoomed());
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

	int cx = rect.Width();
	int cy = rect.Height();

	m_Tracing->zoom(false);
	
	int actualExportBarHeight = EXPORT_BAR_HEIGHT;
	int tracing_height = cy - ((display_patient_banner() ? BANNER_HEIGHT : 0) + BOTTOM_HEIGHT + actualExportBarHeight);
	
	int tracing_required_width = this->calculate_required_tracing_width(tracing_height);
	if (cx < patterns_minimum_width || tracing_required_width < patterns_minimum_width)
	{
		// Calculate proper minimum resolution for message
		int minimum_height = calculate_required_tracing_height(patterns_minimum_width);
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
		}
		else
		{
			m_Tracing->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
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
}

// On resize of the control, reposition the command bar
void PatternsChartCtrl::OnSize(UINT t, int cx, int cy)
{
	COleControl::OnSize(t, cx, cy);
	adjust_controls();	
}
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// DRAWING

int PatternsChartCtrl::calculate_required_tracing_width(int height)
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
	return (int)((height - ((4 * (line_height + 1)) + patterns_fix_required_height)) / patterns_ratio_coefficient);
}

int PatternsChartCtrl::calculate_required_tracing_height(int width)
{
	static int line_height = -1;

	// Calculate a line of text height
	if (line_height < 0)
	{
		CPaintDC dc(this);
		services::select_font(80, "Arial", &dc);
		line_height = dc.GetTextExtent("W").cy;
	}

	return (int)((width * patterns_ratio_coefficient) + (4 * (line_height + 1)) + patterns_fix_required_height);
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

		m_Tracing->ResetVisibleChunks();
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
					}
					olet += COleDateTimeSpan(0, 0, it->GetTimeRange(), 0);
					int endMinute = olet.GetMinute();
					if(endMinute == 0 || endMinute == 30)
					{					
						m_Tracing->AddVisibleChunkEnd(rightInPixels);
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

		PatternsExportableChunkNative chunk(basetime + startTime, timeRange, exportID >= 0);
		chunk.m_id = id;
		chunk.m_exportID = exportID;
		if(m_exportableIntervals.size() == 0 || id > m_exportableIntervals.back().m_id)
			m_exportableIntervals.push_back(chunk);
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
		
		DATE from = pChunk->m_from;
		DATE to = pChunk->m_to;
		ReplyMessage(0);
		m_Tracing->ResetHighlightedExportTime(from, to, wParam != 0);
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
			m_Tracing->zoom(zoom);
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
		CString resultString;
		m_Tracing->DoOnExportDlgCalculatedEntriesRequestEx(from, to, pEntries->m_meanContructions, pEntries->m_meanBaseline, pEntries->m_meanBaselineVariability, pEntries->m_montevideo);
		
	}
	return 0;
}

//////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CEmptyBarWnd, CWnd)
	ON_WM_PAINT()
END_MESSAGE_MAP()

void CEmptyBarWnd::OnPaint()
{
	CPaintDC dc(this);
	CRect clientRect;
	GetClientRect(clientRect);
	dc.FillRect(clientRect, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
}
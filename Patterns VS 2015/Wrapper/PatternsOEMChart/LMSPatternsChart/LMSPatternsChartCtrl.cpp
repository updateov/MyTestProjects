// LMSPatternsChartCtrl.cpp : Implementation of the CLMSPatternsChartCtrl ActiveX Control class.

#include "stdafx.h"
#include "LMSPatternsChart.h"
#include "LMSPatternsChartCtrl.h"
#include "LMSPatternsChartPropPage.h"
//#include "NativeManager.h"
#include "patterns gui, double buffer.h"
#include "patterns gui, services.h"

//#include "utils.h"
#include "aboutbox.h"
#include <fstream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//// Convert a UTF-8 encoded string to a wstring
//std::wstring utf8_to_wstr(const std::string &utf8)
//{
//	if (utf8 == "")
//	{
//		return L"";
//	}
//
//	std::vector<wchar_t> wbuff(utf8.length() + 1);
//	if (!MultiByteToWideChar(CP_UTF8, 0,  utf8.c_str(), utf8.length(), &wbuff[0], utf8.length() + 1))
//	{
//		DWORD e = ::GetLastError();
//		switch(e)
//		{
//			case ERROR_INSUFFICIENT_BUFFER:
//			case ERROR_INVALID_FLAGS:
//			case ERROR_INVALID_PARAMETER:
//			case ERROR_NO_UNICODE_TRANSLATION:
//				return L"Error";
//			default:
//				break;
//		}
//	}
//
//	return &wbuff[0];
//}
//
//// Convert a wstring to a UTF-8 encoded string
//std::string wstr_to_str( const std::wstring& wstr )
//{
//	if(wstr == L"")
//		return "";
//
//	std::vector<char> buff(wstr.length() + 1);
//
//	size_t size = wstr.length();
//	WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), size, &buff[0], size+1, NULL, NULL );
//	return &buff[0];
//}
//
//// Due to the way data is retrieved from an XML send by a C# application, the accent characters (and all 'special' characters and not
//// properly encoded in ANSI, doing a UTF8 to wstring then back to ansi fixes that
//string& cleanString(string& str)
//{
//	wstring ws = utf8_to_wstr(str);
//	str = wstr_to_str(ws);
//	return str;
//}
//
using namespace patterns_gui;
//
//const int patterns_minimum_width = 700;
//const double patterns_ratio_coefficient =	((210 / 90.) / 17.)		// HR 17 minutes
//										+	((100 / 75.) / 17.)		// UP 17 minutes
//										+	((210 / 90.) / 120.)	// HR 2 hours
//										+	((100 / 75.) / 120.)	// UP 2 hours
//										+	((200 / 100.) / 120.)	// CR 2 hours
//										+	((250 / 100.) / 120.);	// Proportional blank inter space 
//
//const int patterns_fix_required_height = 197;
//
///// <summary>
///// The different type of action
///// </summary>
//enum ActionTypes
//{
//	eActionStrikeoutContraction = 1,
//	eActionStrikeoutEvent = 2,
//	eActionConfirmEvent = 3,
//	eActionUndoStrikeoutEvent = 4,
//	eActionUndoStrikeoutContraction = 5,
//};
//
///// <summary>
///// The status of that patient
///// </summary>
//enum PatientStatus
//{
//	ePatientInvalid = 0,
//	ePatientLive = 1,
//	ePatientUnplugged = 2,
//	ePatientRecovery = 3,
//	ePatientError = 4,
//	ePatientLate = 5,
//};

IMPLEMENT_DYNCREATE(CLMSPatternsChartCtrl, COleControl)
DEFINE_GUID(IID_IDispatchEx, 0xa6ef9860, 0xc720, 0x11d0, 0x93, 0x37, 0x0, 0xa0, 0xc9, 0xd, 0xca, 0xa9);

//Implementation IObjectSafety
BEGIN_INTERFACE_MAP(CLMSPatternsChartCtrl, COleControl)
	INTERFACE_PART(CLMSPatternsChartCtrl, IID_IObjectSafety, ObjectSafety)
END_INTERFACE_MAP()

STDMETHODIMP CLMSPatternsChartCtrl::XObjectSafety::GetInterfaceSafetyOptions(
	REFIID riid,	
	DWORD __RPC_FAR *pdwSupportedOptions,	
	DWORD __RPC_FAR *pdwEnabledOptions)
{
	METHOD_PROLOGUE_EX(CLMSPatternsChartCtrl, ObjectSafety)

		if (!pdwSupportedOptions || !pdwEnabledOptions)
		{
			return E_POINTER;
		}

		*pdwSupportedOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER | INTERFACESAFE_FOR_UNTRUSTED_DATA;
		*pdwEnabledOptions = 0;

		// What interface is being checked out anyhow?
		OLECHAR szGUID[39];
		int i = StringFromGUID2(riid, szGUID, 39);

		if ((riid == IID_IDispatch) || (riid == IID_IDispatchEx))
		{
			// Client wants to know if object is safe for scripting
			*pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER;
			return S_OK;
		}

		if (riid == IID_IPersistPropertyBag	
			|| riid == IID_IPersistStreamInit
			|| riid == IID_IPersistStorage
			|| riid == IID_IPersistMemory)
		{
			// Client wants to know if object is safe for initializing from persistent data
			*pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_DATA;
			return S_OK;
		}

		if (NULL != pThis->GetInterface(&riid))
		{
			*pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER;
			return S_OK;
		}

		trace(EVENTLOG_WARNING_TYPE, "Requested interface is not supported.\n");
		return E_NOINTERFACE;
}

STDMETHODIMP CLMSPatternsChartCtrl::XObjectSafety::SetInterfaceSafetyOptions(
	REFIID riid,	
	DWORD dwOptionSetMask,	
	DWORD dwEnabledOptions)
{
	METHOD_PROLOGUE_EX(CLMSPatternsChartCtrl, ObjectSafety)

		// What is this interface anyway? We can do a quick lookup in the registry under HKEY_CLASSES_ROOT\Interface
		OLECHAR szGUID[39];
	int i = StringFromGUID2(riid, szGUID, 39);

	if (0 == dwOptionSetMask && 0 == dwEnabledOptions)
	{
		// the control certainly supports NO requests through the specified interface so it's safe to return S_OK even if the interface isn't supported.
		return S_OK;
	}

	if ((riid == IID_IDispatch) || (riid == IID_IDispatchEx))
	{
		if (INTERFACESAFE_FOR_UNTRUSTED_CALLER == dwOptionSetMask && INTERFACESAFE_FOR_UNTRUSTED_CALLER == dwEnabledOptions)
		{
			return S_OK;
		}
		return E_FAIL;
	}

	if (riid == IID_IPersistPropertyBag	|| riid == IID_IPersistStreamInit || riid == IID_IPersistStorage || riid == IID_IPersistMemory)
	{
		if (INTERFACESAFE_FOR_UNTRUSTED_DATA == dwOptionSetMask && INTERFACESAFE_FOR_UNTRUSTED_DATA == dwEnabledOptions)
		{
			return NOERROR;
		}

		trace(EVENTLOG_WARNING_TYPE, "Client asking if it's safe to call through IPersist* and it's not!");
		return E_FAIL;
	}

	trace(EVENTLOG_WARNING_TYPE, "We didn't account for the safety of %s, and it's one we support...\n", szGUID);
	return E_FAIL;
}

STDMETHODIMP_(ULONG) CLMSPatternsChartCtrl::XObjectSafety::AddRef()
{
	METHOD_PROLOGUE_EX_(CLMSPatternsChartCtrl, ObjectSafety)
		return (ULONG)pThis->ExternalAddRef();
}

STDMETHODIMP_(ULONG) CLMSPatternsChartCtrl::XObjectSafety::Release()
{
	METHOD_PROLOGUE_EX_(CLMSPatternsChartCtrl, ObjectSafety)
		return (ULONG)pThis->ExternalRelease();
}

STDMETHODIMP CLMSPatternsChartCtrl::XObjectSafety::QueryInterface(REFIID iid, LPVOID* ppvObj)
{
	METHOD_PROLOGUE_EX_(CLMSPatternsChartCtrl, ObjectSafety)
		return (HRESULT)pThis->ExternalQueryInterface(&iid, ppvObj);
}


// Message map

BEGIN_MESSAGE_MAP(CLMSPatternsChartCtrl, PatternsChartCtrl)
	ON_WM_TIMER()
	ON_WM_CREATE()
	ON_WM_DESTROY()
	ON_OLEVERB(AFX_IDS_VERB_EDIT, OnEdit)
	ON_OLEVERB(AFX_IDS_VERB_PROPERTIES, OnProperties)

	ON_BN_CLICKED(63, display_data_status)

	ON_BN_CLICKED(5, on_show_about)

	ON_CONTROL(patterns_gui::tracing::ndeleteevent, 200, on_strike_out_event)
	ON_CONTROL(patterns_gui::tracing::ndeletecontraction, 200, on_strike_out_contraction)
	ON_CONTROL(patterns_gui::tracing::nacceptevent, 200, on_accept_event)
END_MESSAGE_MAP()

// Dispatch map

BEGIN_DISPATCH_MAP(CLMSPatternsChartCtrl, COleControl)
	DISP_PROPERTY_NOTIFY_ID(CLMSPatternsChartCtrl, "ConnectionData", dispidConnectionData, m_ConnectionData, OnConnectionDataChanged, VT_BSTR)
END_DISPATCH_MAP()

// Event map

BEGIN_EVENT_MAP(CLMSPatternsChartCtrl, COleControl)
END_EVENT_MAP()

// Property pages

// TODO: Add more property pages as needed. Remember to increase the count!
BEGIN_PROPPAGEIDS(CLMSPatternsChartCtrl, 1)
PROPPAGEID(CLMSPatternsChartPropPage::guid)
END_PROPPAGEIDS(CLMSPatternsChartCtrl)

// Initialize class factory and guid
IMPLEMENT_OLECREATE_EX(CLMSPatternsChartCtrl, "LMSPATTERNSCHART.LMSPatternsChartCtrl.1", 0x172661, 0x82a6, 0x4c68, 0x85, 0x85, 0xd5, 0x4e, 0x46, 0xa0, 0x7c, 0xcf)

// Type library ID and version
IMPLEMENT_OLETYPELIB(CLMSPatternsChartCtrl, _tlid, _wVerMajor, _wVerMinor)

// Interface IDs
const IID BASED_CODE IID_DLMSPatternsChart = { 0xD497676A, 0xB978, 0x4B6F, { 0x85, 0xC5, 0xA9, 0xE, 0x5A, 0xDF, 0xE1, 0x79 } };
const IID BASED_CODE IID_DLMSPatternsChartEvents = { 0xCFF0436C, 0xFF88, 0x4F96, { 0x8F, 0xCD, 0xAA, 0x7B, 0xD4, 0x25, 0x40, 0xFD } };

// Control type information
static const DWORD BASED_CODE _dwLMSPatternsChartOleMisc = OLEMISC_SETCLIENTSITEFIRST
| OLEMISC_INSIDEOUT
//| OLEMISC_ALWAYSRUN
| OLEMISC_ACTIVATEWHENVISIBLE
//| OLEMISC_IGNOREACTIVATEWHENVISIBLE
| OLEMISC_CANTLINKINSIDE
| OLEMISC_RECOMPOSEONRESIZE;

IMPLEMENT_OLECTLTYPE(CLMSPatternsChartCtrl, IDS_LMSPATTERNSCHART, _dwLMSPatternsChartOleMisc)

// CLMSPatternsChartCtrl::CLMSPatternsChartCtrlFactory::UpdateRegistry -
// Adds or removes system registry entries for CLMSPatternsChartCtrl
BOOL CLMSPatternsChartCtrl::CLMSPatternsChartCtrlFactory::UpdateRegistry(BOOL bRegister)
{
	// TODO: Verify that your control follows apartment-model threading rules.
	// Refer to MFC TechNote 64 for more information.
	// If your control does not conform to the apartment-model rules, then
	// you must modify the code below, changing the 6th parameter from
	// afxRegInsertable | afxRegApartmentThreading to afxRegInsertable.

	if (bRegister)
	{
		return AfxOleRegisterControlClass(
			AfxGetInstanceHandle(),
			m_clsid,
			m_lpszProgID,
			IDS_LMSPATTERNSCHART,
			IDB_LMSPATTERNSCHART,
			afxRegInsertable | afxRegApartmentThreading,
			_dwLMSPatternsChartOleMisc,
			_tlid,
			_wVerMajor,
			_wVerMinor);
	}

	return AfxOleUnregisterClass(m_clsid, m_lpszProgID);
}

// CLMSPatternsChartCtrl::CLMSPatternsChartCtrl - Constructor
CLMSPatternsChartCtrl::CLMSPatternsChartCtrl()
{

	m_cr_limit = 10;
	m_cr_window = 10;

	m_cr_stage1 = 0;

	InitializeIIDs(&IID_DLMSPatternsChart, &IID_DLMSPatternsChartEvents);

	m_Tracing = new tracing();
	m_Tracing->set_can_delete(false); // Readonly by default!

	
	m_conductor = new patterns::conductor();
	m_conductor->set_input_adapter(new viewer_input_adapter());

	// Create the initial patient AND fetus
	patterns::fetus f;
	f.set_up_sample_rate(1);
	f.set_hr_sample_rate(4);

	input_adapter::patient* p = new input_adapter::patient();
	p->set_key("KEY");
	f.set_key("KEY");

	((viewer_input_adapter*)(&m_conductor->get_input_adapter()))->add_patient(p, &f);

}

// CLMSPatternsChartCtrl::~CLMSPatternsChartCtrl - Destructor
CLMSPatternsChartCtrl::~CLMSPatternsChartCtrl()
{
}

// CLMSPatternsChartCtrl::DoPropExchange - Persistence support
void CLMSPatternsChartCtrl::DoPropExchange(CPropExchange* pPX)
{
	ExchangeVersion(pPX, MAKELONG(_wVerMinor, _wVerMajor));
	COleControl::DoPropExchange(pPX);
}

// Flags to customize MFC's implementation of ActiveX controls.
//DWORD CLMSPatternsChartCtrl::GetControlFlags()
//{
//	DWORD dwFlags = COleControl::GetControlFlags();
//
//	// The control can receive mouse notifications when inactive.
//	dwFlags |= pointerInactive;
//
//	// The control can optimize its OnDraw method, by not restoring the original GDI objects in the device context.
//	dwFlags |= canOptimizeDraw;
//
//	// Disables the call to IntersectClipRect made by COleControl and gains a small speed advantage
//	dwFlags &= ~clipPaintDC;
//
//	// eliminates extra drawing operations and the accompanying visual flicker. Use when your control draws itself identically in the inactive and active states
//	dwFlags |= noFlickerActivate;
//
//	return dwFlags;
//}

// CLMSPatternsChartCtrl message handlers
int CLMSPatternsChartCtrl::OnCreate(LPCREATESTRUCT s)
{
	if (PatternsChartCtrl::OnCreate(s) == -1)
		return -1;
	return 0;
	//if (COleControl::OnCreate(s) == -1)
	//	return -1;

	//m_Message->Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	//m_Message->set_format(DT_CENTER | DT_VCENTER | DT_WORDBREAK);
	//
	//m_Tracing->Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 200);
	//m_Tracing->set_type(tracing::tnormal);
	//m_Tracing->set_paper(tracing::pusa);
	//m_Tracing->set_scaling_mode(tracing::spaper);
	//m_Tracing->show_grid();
	//m_Tracing->lock_scaling();
	//m_Tracing->set_lengths(1020, 7200);
	//m_Tracing->show(tracing::winformation);
	//m_Tracing->show(tracing::whidedisconnected, false);
	//m_Tracing->show(tracing::wbaselinevariability, true);
	//m_Tracing->show(tracing::wcrtracing, true);

	//initialize_commandbar();

	//// First download of data
	//download_patient_data();

	//// Display the patient
	//m_Tracing->set(get_fetus());
	//m_Tracing->zoom(false);
	//m_Tracing->move_to(get_fetus()->get_number_of_fhr() + 1000000);
	//m_Tracing->scroll(true);

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

	//// The control is ready to run
	//m_Initialized = true;

	//return 0;
}

// Cleanup
void CLMSPatternsChartCtrl::OnDestroy()
{
	PatternsChartCtrl::OnDestroy();
	//if (m_RefreshTimer != 0)
	//{
	//	KillTimer(m_RefreshTimer);
	//	m_RefreshTimer = 0;
	//}

	//COleControl::OnDestroy();
}

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
/////////////// DATA FUNCTION

// Process the data and adjust the display
void CLMSPatternsChartCtrl::process_patient_data(TiXmlElement* rootNode)
{
	// Check for invalid version
	int invalid_version = 0;
	rootNode->Attribute("invalid_version", &invalid_version);
	if (invalid_version != 0)
	{
		log_error("This version of the application is outdated, answer ‘Yes’ to the first prompt which asked to update or contact a system administrator for additional help.");
		m_CriticalError = true;
		return;
	}

	// Retrieve data
	TiXmlElement* patientNode = rootNode->FirstChildElement("patient");
	if (patientNode == NULL)
		throw new exception("Invalid response, no patient node found");

	patterns::fetus* f = get_fetus();
	input_adapter::patient* p = get_patient();

	// Suspend notifications so that GUI refreshes are done all at once
	f->suspend_notifications(true);

	TiXmlElement* tracingsNode = patientNode->FirstChildElement("tracings");
	TiXmlElement* actionsNode = patientNode->FirstChildElement("actions");
	TiXmlElement* artifactsNode = patientNode->FirstChildElement("artifacts");
	TiXmlElement* intervalsNode = patientNode->FirstChildElement("intervals");	
	TiXmlElement* requestNode = patientNode->FirstChildElement("request");	

	int incremental = 0;
	requestNode->Attribute("incremental", &incremental);

	// Empty all data if requested to
	if (incremental == 0)
	{
		p->reset();
		f->clear();
	}

	//////////////////////////////////////////////////////////////////////////////////
	// Patient data...
	p->set_key("KEY");
	p->set_id((string)patientNode->Attribute("id"));

	const char* gaAttr = patientNode->Attribute("ga");
	if(gaAttr != NULL)
	{
		string ga = cleanString((string)(gaAttr));
		ParseGA(ga);
	}

	bool update_banner = false;
	if (display_patient_banner())
	{
		string firstname = cleanString((string)(patientNode->Attribute("firstname")));
		string lastname = cleanString((string)(patientNode->Attribute("lastname")));
		string mrn = cleanString((string)(patientNode->Attribute("mrn")));

		if (firstname != p->get_surname())
		{
			update_banner = true;
			p->set_surname(firstname);
		}
		if (lastname != p->get_name())
		{
			update_banner = true;
			p->set_name(lastname);
		}
		if (mrn != p->get_accountno())
		{
			update_banner = true;
			p->set_accountno(mrn);
		}
	}

	bool refresh_needed = false;

	int status = 0;
	patientNode->Attribute("status", &status);
	string status_details = cleanString((string)(patientNode->Attribute("statusdetails")));

	// Readonly?
	bool patient_readonly = (patientNode->Attribute("readonly") != NULL) && (((CString)(patientNode->Attribute("readonly"))).CompareNoCase("true") == 0);
	m_Tracing->set_can_delete(!patient_readonly && (m_Permissions.CompareNoCase("readonly") != 0));

	switch (status)
	{
	case ePatientLive:
		if (!p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(true);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		set_data_status(data_status_connected, status_details);
		break;

	case ePatientUnplugged:
		if (p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(false);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		set_data_status(data_status_connected, status_details);
		break;

	case ePatientLate:
		if (!p->get_is_collecting_tracing() || !p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(true);
			p->set_is_late_collecting_tracing(true);
			f->note(subscription::mfetus);
		}
		set_data_status(data_status_connected, status_details);
		break;

	case ePatientRecovery:
		if (p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(false);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		set_data_status(data_status_recovery, status_details);
		break;

	case ePatientInvalid:
		ClearPatient();
		set_data_status(data_status_no_data, status_details);
		break;

	case ePatientError:
	default: 
		ClearPatient();
		set_data_status(data_status_error, status_details);
		break;
	}

	date edd = patterns::conductor::to_integer(patientNode->Attribute("edd"));
	if (p->get_edc() != edd)
	{
		refresh_needed = true;
		p->set_edc(edd);

		if (edd != undetermined_date)
		{
			// Calculate the time edc - 4 weeks
			edd = (edd - (60 * 60 * 24 * 28)) + (2 * 60 * 60); // The 2 hours is there to fix the possible daylight saving issue where a day is 23 hours OR 25

			SYSTEMTIME st = fetus::convert_to_SYSTEMTIME(edd);

			// Round it so that it's sure at midnight
			st.wHour = 0;
			st.wMinute = 0;
			st.wSecond = 0;
			st.wMilliseconds = 0;

			// Done
			edd = fetus::convert_to_utc(fetus::convert_to_time_t(st));
		}

		f->set_cutoff_date(edd);
	}

	date reset = patientNode->Attribute("reset") == 0 ? undetermined_date : patterns::conductor::to_integer(patientNode->Attribute("reset"));
	if (f->get_reset_date() != reset)
	{
		f->set_reset_date(reset);
		refresh_needed = true;
	}

	long fc = patterns::conductor::to_integer(patientNode->Attribute("fetus"));
	if (p->get_number_of_fetuses() != fc)
	{
		refresh_needed = true;
		p->set_number_of_fetuses(fc);
	}

	UpdateExportTimeRange(requestNode);
	if(intervalsNode != NULL)
	{
		LoadExportableIntervals(intervalsNode);
	}

	TiXmlElement* node;

	bool refresh_contractions_rate = false;
	bool some_tracings = false;

	//////////////////////////////////////////////////////////////////////////////////
	// Tracings...
	if (tracingsNode != NULL)
	{
		time_t absolutestart = 0;
		time_t endprevious = 0;

		if (f->get_number_of_up() > 0) // Not an empy fetus... it's an append then
		{
			absolutestart = f->get_start_date();
			endprevious = absolutestart + f->get_number_of_up();
		}

		vector<char> ups;
		vector<char> hrs;

		node = tracingsNode->FirstChildElement("tracing");
		while (node != NULL)
		{

	
			time_t blocktime = patterns::conductor::to_integer(node->Attribute("start"));
			if (absolutestart == 0)
			{
				absolutestart = blocktime;
				endprevious = blocktime;
			}

			string _hr1 = base64::base64_decode(node->Attribute("hr1"));
			string _up = base64::base64_decode(node->Attribute("up"));

			if (blocktime > endprevious)
			{
				ups.insert(ups.end(), (unsigned int)(blocktime - endprevious), 255);
				hrs.insert(hrs.end(), (unsigned int)(4 * (blocktime - endprevious)), 255);
				endprevious = blocktime;
			}

			int overlap = (int)(endprevious - blocktime);		
			long seconds = max(_up.length(), _hr1.length() / 4) - overlap;

			if (seconds > 0)
			{
				
				ups.reserve(ups.size() + seconds);
				hrs.reserve(hrs.size() + 4 * seconds);

				std::copy(_up.begin() + overlap, _up.end(), std::back_inserter(ups));
				std::copy(_hr1.begin() + (4 * overlap), _hr1.end(), std::back_inserter(hrs));

				if (ups.size() < hrs.size() / 4)
				{
					ups.insert(ups.end(), (hrs.size() / 4) - ups.size(), 0);
				}
				if (hrs.size() < 4 * ups.size())
				{
					hrs.insert(hrs.end(), (4 * ups.size()) - hrs.size(), 0);
				}
				endprevious += seconds;
			}
			
			node = node->NextSiblingElement();
		}

		if (ups.size() > 0)
		{
			refresh_needed = true;
			some_tracings = true;

			if (f->get_number_of_up() == 0)
			{
				f->set_start_date(absolutestart);
			}

			f->append_up(ups);
			f->append_fhr(hrs);
		}
	
	}

	//////////////////////////////////////////////////////////////////////////////////
	// Artifacts...
	if (artifactsNode != NULL)
	{
		vector<string> lines;		
		node = artifactsNode->FirstChildElement("artifact");
		if (node != NULL)
		{
	
			long basetime = patterns::conductor::to_integer(artifactsNode->Attribute("basetime"));

			while (node != NULL)
			{
				lines.push_back(node->Attribute("data"));
				node = node->NextSiblingElement();
			}

			if (lines.size() > 0)
			{
				// Must reset the initial date for all events!
				input_adapter::load_saved_data(*f, lines, ((long)f->get_start_date() - basetime));
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////
	// Actions...
	if (actionsNode != NULL)
	{
		node = actionsNode->FirstChildElement("action");
		while (node != NULL)
		{

			refresh_needed = true;

			int artifact_id = patterns::conductor::to_integer(node->Attribute("artifact"));
			int action_type = patterns::conductor::to_integer(node->Attribute("type"));

			string performed = services::date_to_string(patterns::conductor::to_integer(node->Attribute("performed")), patterns_gui::services::fnormal);
			string username = cleanString((string)(node->Attribute("username")));
			string userid = cleanString((string)(node->Attribute("userid")));

			if (userid.length() > 0)
			{
				username = username + " (" + userid + ")";
			}

			CString t;

			t.LoadString(strikeout_by_label);
			string strikeby = (LPCSTR)t;

			t.LoadString(restore_by_label);
			string restoreby = (LPCSTR)t;

			t.LoadString(confirm_by_label);
			string confirmby = (LPCSTR)t;

			t.LoadString(action_performed_at);
			string performedat = (LPCSTR)t;

			switch (action_type)
			{
			case eActionStrikeoutContraction:
				{
					patterns::contraction* ctr = f->get_contraction_for_artifactkey(artifact_id);
					if (ctr != NULL)
					{
						ctr->set_extension("strike_out", strikeby + username + " " + performedat + performed);
						ctr->set_as_strike_out(true);
						refresh_contractions_rate = true;
					}
					break;
				}
			case eActionUndoStrikeoutContraction:
				{
					patterns::contraction* ctr = f->get_contraction_for_artifactkey(artifact_id);
					if (ctr != NULL)
					{
						ctr->set_extension("restored", restoreby + username + " " + performedat + performed);
						ctr->set_as_strike_out(false);
						refresh_contractions_rate = true;
					}
					break;
				}

			case eActionStrikeoutEvent:
				{
					patterns::event* evt = f->get_event_for_artifactkey(artifact_id);
					if (evt != NULL)
					{
						evt->set_extension("strike_out", strikeby + username + " " + performedat + performed);
						evt->set_as_strike_out(true);
					}	
					break;
				}
			case eActionConfirmEvent:
				{
					patterns::event* evt = f->get_event_for_artifactkey(artifact_id);
					if (evt != NULL)
					{
						evt->set_extension("confirmed", confirmby + username + " " + performedat + performed);
						evt->set_as_confirmed(true);
					}
					break;
				}
			case eActionUndoStrikeoutEvent:
				{
					patterns::event* evt = f->get_event_for_artifactkey(artifact_id);
					if (evt != NULL)
					{
						evt->set_extension("restored", restoreby + username + " " + performedat + performed);
						evt->set_as_strike_out(false);
					}
					break;
				}
			}
			node = node->NextSiblingElement();
		}
	}

	//////////////////////////////////////////////////////////////////////////////////
	// Request...
	if (requestNode != NULL)
	{
		TiXmlElement* request = m_RequestHeader.FirstChild("patients")->FirstChildElement("request");
		request->operator =(*requestNode);
	}

	if (refresh_needed)
	{
		if (refresh_contractions_rate)
		{
			f->ResetContracionRate();
		}
		f->note(subscription::mfetus);
	}

	if (update_banner && display_patient_banner())
	{
		// Refresh banner
		CRect rect;
		GetClientRect(&rect);
		rect.bottom = rect.top + BANNER_HEIGHT;

		InvalidateRect(rect);
	}

	// Check for demo mode
	m_Disclaimer = 0;
	rootNode->Attribute("demo_mode", &m_Disclaimer);
	m_Tracing->show(patterns_gui::tracing::wshowdisclaimer, (m_Disclaimer > 0));

	// Suspend notifications so that GUI refreshes are done all at once
	f->suspend_notifications(false);

	if (incremental == 0)
	{
		
		m_Tracing->zoom(false);
		m_Tracing->move_to(f->get_number_of_fhr() + 1000000);
		m_Tracing->scroll(true);
		m_Tracing->GetCompressedViewBoundsInSeconds(m_tracingLeftInSeconds, m_tracingRightInSeconds);
		
	}
	UpdatePatternsAdapter();
}

// Read the 'demo' data
//void CLMSPatternsChartCtrl::download_demo_data()
//{
//	try
//	{
//		// Already loaded?
//		if (m_ConnectionData.GetLength() == 0)
//			return;
//
//		TiXmlDocument xmlDoc;
//		string configuration = base64::base64_decode((LPCTSTR)m_ConnectionData) + "\r\n";
//
//		if (xmlDoc.Parse(configuration.c_str()) == 0)
//			throw exception("The connection data is not a valid xml");
//
//		TiXmlElement* node = xmlDoc.FirstChildElement("configuration");
//		if (node == NULL)
//			throw exception("The connection data does not contain the 'configuration' node");
//
//		TiXmlElement* data = node->FirstChildElement("data");
//		if (data == NULL)
//			throw exception("The connection data does not contain the 'data' node");
//
//		process_patient_data(data);
//
//		// Flush connection data that is no useless
//		m_ConnectionData = "";
//	}
//	catch (exception& e)
//	{
//		log_exception("Error while processing demo data", e);
//	}
//	catch (...)
//	{
//		log_exception("Unknown error while processing demo data");
//	}
//}

// Get the data / updates from the server and refresh the display
//void CLMSPatternsChartCtrl::download_patient_data()
//{
//	// We don't come back from a critical error
//	if (m_CriticalError)
//		return;
//
//	// Not initialized yet
//	if (m_ConnectionData.GetLength() == 0)
//		return;
//
//	// In demo mode, we don't use a server!
//	if (m_DemoMode)
//	{
//		download_demo_data();
//		return;
//	}
//
//	// If there was recently an error talking to patterns server, skip the next xx call to reduce error count
//	if (m_skip_download > 0)
//	{
//		--m_skip_download;
//		return;
//	}
//
//	if (!m_DataLock.try_acquire())
//		return;
//
//	try
//	{
//		// Request data from the server
//		string response = utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("data"), utils::to_string(m_RequestHeader)) + "\r\n";
//
//		// Failed?
//		if (response.length() == 2) // 2 because of the + "\r\n"
//		{
//			m_skip_download = 4;
//
//			ClearPatient();
//			ClearRequest();
//
//			set_data_status(data_status_error, "Unable to reach the server");
//			return;
//		}
//
//		// Parse the answer
//		TiXmlDocument xmlDoc;
//		if (xmlDoc.Parse(response.c_str()) == 0)
//			throw new exception("Invalid data format, unable to parse it as a proper xml");
//
//		// Fetch the data
//		TiXmlElement* rootNode = xmlDoc.RootElement();
//		if (rootNode == NULL)
//			throw new exception("Response is empty");
//
//		// Process the response
//		process_patient_data(rootNode);
//	}
//	catch (exception& e)
//	{
//		log_exception("Error while requesting server data", e);
//	}
//	catch (...)
//	{
//		log_exception("Unknown error while requesting server data");
//	}
//
//	m_DataLock.release();
//}

///
/// Clean up the request to the bare minimum: the patient ID
///
//void CLMSPatternsChartCtrl::ClearRequest()
//{
//	// In the request header, reset all attribute but the key
//	TiXmlElement* patientsNode = m_RequestHeader.FirstChildElement("patients");
//	if (patientsNode != NULL)
//	{
//		TiXmlElement* requestNode = patientsNode->FirstChildElement("request");
//		if (requestNode != NULL)
//		{
//			// Keep only the "key" attribute from the request
//			const TiXmlAttribute* pAttribute = requestNode->FirstAttribute();
//			while (pAttribute != NULL)
//			{
//				CString name = pAttribute->Name();
//				if (name.CompareNoCase("key") != 0)
//				{
//					requestNode->RemoveAttribute(name);
//					pAttribute = requestNode->FirstAttribute();
//				}
//				else
//				{
//					pAttribute = pAttribute->Next();
//				}
//			}
//		}
//	}
//}

///
/// Flush all current loaded patient data
///
//void CLMSPatternsChartCtrl::ClearPatient()
//{
//	// Reset existing data
//	patterns::fetus* f = get_fetus();
//	if (f)
//	{
//		f->clear();
//	}
//
//	input_adapter::patient* p = get_patient();
//	if (p)
//	{
//		p->reset();
//	}
//}


// CLMSPatternsChartCtrl message handlers
void CLMSPatternsChartCtrl::OnConnectionDataChanged(void)
{
	m_DataLock.acquire();

	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Already initialized?
		if (m_Initialized)
		{
			m_Initialized = false;
			m_CriticalError = false;
			m_skip_download = 0;

			// Reset existing data
			ClearPatient();
			ClearRequest();

			set_data_status(data_status_no_data, "");

			// Reset the timer for refresh
			if (m_RefreshTimer != 0)
			{
				KillTimer(m_RefreshTimer);
				m_RefreshTimer = 0;
			}
		}

		// Read the configuration
		TiXmlDocument xmlDoc;
		string configuration = base64::base64_decode((LPCTSTR)m_ConnectionData) + "\r\n";
		if (xmlDoc.Parse(configuration.c_str()) == 0)
			throw exception("The connection data is not a valid xml");

		TiXmlElement* node = xmlDoc.FirstChildElement("configuration");
		if (node == NULL)
			throw exception("The connection data does not contain the 'configuration' node");

		// Validation of mandatory connection data
		if (node == NULL) throw exception("The connection data is invalid, root 'configuration' is missing");
		if (node->Attribute("patient_id") == 0)			throw exception("The connection data is invalid, missing 'patient_id'");
		if (node->Attribute("server_url") == 0)			throw exception("The connection data is invalid, missing 'server_url'");
		if (node->Attribute("user_id") == 0)			throw exception("The connection data is invalid, missing 'user_id'");
		if (node->Attribute("user_name") == 0)			throw exception("The connection data is invalid, missing 'user_name'");
		if (node->Attribute("refresh") == 0)			throw exception("The connection data is invalid, missing 'refresh'");
		if (node->Attribute("cr_limit") == 0)			throw exception("The connection data is invalid, missing 'cr_limit'");
		if (node->Attribute("cr_window") == 0)			throw exception("The connection data is invalid, missing 'cr_window'");
		if (node->Attribute("cr_stage1") == 0)			throw exception("The connection data is invalid, missing 'cr_stage1'");
		if (node->Attribute("cr_stage2") == 0)			throw exception("The connection data is invalid, missing 'cr_stage2'");

		// Retrieve all values
		CString patient_id = node->Attribute("patient_id");
		m_ServerURL = node->Attribute("server_url");
		m_UserID = node->Attribute("user_id");
		m_UserName = node->Attribute("user_name");
		m_Permissions = node->Attribute("permissions");

		node->Attribute("refresh", &m_RefreshDelay);
		node->Attribute("cr_limit", &m_cr_limit);
		node->Attribute("cr_window", &m_cr_window);
		node->Attribute("cr_stage1", &m_cr_stage1);
		node->Attribute("cr_stage2", &m_cr_stage2);
		node->Attribute("banner", &m_Banner);

		int timeout;
		node->Attribute("timeout_dlg", &timeout);
		CLMSPatternsChartApp::set_timeout_dialog(timeout);

		// Apply contraction rate settings
		patterns_gui::tracing::set_cr_maximum(m_cr_limit);
		patterns::fetus::set_cr_window(m_cr_window);
		patterns_gui::tracing::set_cr_threshold(m_cr_stage1);
		patterns_gui::tracing::set_cr_kcrstage(m_cr_stage2);

		// Apply user permission
		m_Tracing->set_can_delete(m_Permissions.CompareNoCase("readonly") != 0);

		// Prepare the data request header for that patient
		m_RequestHeader.Clear();
		m_RequestHeader.Parse("<patients version=\"" FILEVERSTR "\"><request key=\"0\"/></patients>\r\n");
		m_RequestHeader.FirstChildElement("patients")->SetAttribute("user", m_UserID);
		m_RequestHeader.FirstChild("patients")->FirstChildElement("request")->SetAttribute("key", patient_id);

		// Check if we are in demo mode
		m_DemoMode = ((m_ServerURL.GetLength() >= 4) && (m_ServerURL.Left(4).CompareNoCase("data") == 0));

		LoadEnabledPlugins();
		// If the window is already up, that means it is a "re-initialization"
		if (::IsWindow(m_hWnd))
		{
			adjust_controls();

			// Refresh the time
			m_RefreshTimer = SetTimer(1, m_RefreshDelay * 1000, NULL);

			// Refresh the data
			download_patient_data();

			// Reset position
			m_Tracing->zoom(false);
			m_Tracing->move_to(get_fetus()->get_number_of_fhr() + 1000000);
			m_Tracing->scroll(true);
			ShowExportBar(true);
		}

		m_Initialized = true;
	}
	catch (exception& e)
	{
		log_exception("Error during the control initialization", e);
		m_CriticalError = true;
	}
	catch (...)
	{
		log_exception("Unknown error during the control initialization");
		m_CriticalError = true;
	}

	m_DataLock.release();
	InvalidateControl();
	SetModifiedFlag();
}


/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// COMMAND BAR


// User ask to strike an event
void CLMSPatternsChartCtrl::on_strike_out_event(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::tracing::cevent)
	{
		ASSERT(FALSE);
		return;
	}

	const patterns::event& evt = m_Tracing->get()->get_event(m_Tracing->get_selection());
	if (evt.get_artifactkey() == -1)
	{
		ASSERT(FALSE);
		return;
	}

	bool undo = evt.is_strike_out();

	CString s;
	s.LoadString(undo?confirm_undo_strikeout_event:confirm_strikeout_event);

	CString t;
	t.LoadString(question_title);

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, CLMSPatternsChartApp::get_timeout_dialog() * 60000) == IDYES)
	{
		if (m_DemoMode)
		{
			(const_cast<event&>(evt)).set_as_strike_out(!undo);
			get_fetus()->note(subscription::mfetus);
		}
		else
		{
			perform_server_user_action(undo?eActionUndoStrikeoutEvent:eActionStrikeoutEvent, evt.get_artifactkey(), get_patient()->get_id());
		}
	}
}

// User ask to confirm an event
void CLMSPatternsChartCtrl::on_accept_event(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::tracing::cevent)
	{
		ASSERT(FALSE);
		return;
	}

	const patterns::event& evt = m_Tracing->get()->get_event(m_Tracing->get_selection());
	if (evt.get_artifactkey() == -1)
	{
		ASSERT(FALSE);
		return;
	}

	CString s;
	s.LoadString(confirm_confirm_event);

	CString t;
	t.LoadString(question_title);

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, CLMSPatternsChartApp::get_timeout_dialog() * 60000) == IDYES)
	{
		if (m_DemoMode)
		{
			(const_cast<event&>(evt)).set_as_confirmed(true);
			get_fetus()->note(subscription::mfetus);
		}
		else
		{
			perform_server_user_action(eActionConfirmEvent, evt.get_artifactkey(), get_patient()->get_id());
		}
	}
}

// User ask to strike a contraction
void CLMSPatternsChartCtrl::on_strike_out_contraction(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::tracing::ccontraction)
	{
		ASSERT(FALSE);
		return;
	}

	const patterns::contraction& ctr = m_Tracing->get()->get_contraction(m_Tracing->get_selection());
	if (ctr.get_artifactkey() == -1)
	{
		ASSERT(FALSE);
		return;
	}

	bool undo = ctr.is_strike_out();

	CString s;
	s.LoadString(undo?confirm_undo_strikeout_contraction:confirm_strikeout_contraction);

	CString t;
	t.LoadString(question_title);

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, CLMSPatternsChartApp::get_timeout_dialog() * 60000) == IDYES)
	{
		if (m_DemoMode)
		{
			(const_cast<contraction&>(ctr)).set_as_strike_out(!undo);
			get_fetus()->ResetContracionRate();
			get_fetus()->note(subscription::mfetus);
		}
		else
		{
			perform_server_user_action(undo?eActionUndoStrikeoutContraction:eActionStrikeoutContraction, ctr.get_artifactkey(), get_patient()->get_id());
		}
	}
}

// Open the about box
void CLMSPatternsChartCtrl::on_show_about(void)
{
	switch (m_Banner)
	{
		case 1: //Power by PeriGen
		{
			aboutbox_powerby d(this);
			d.DoModal();
			break;
		}
		case 2: // GE
		{
			aboutbox_ge d(this);
			d.DoModal();
			break;
		}

		default: // PeriGen
		{
			aboutbox d(this);
			d.DoModal();
			break;
		}
	}
}

// Open the about box
void CLMSPatternsChartCtrl::display_data_status(void)
{
	if (m_DataStatusDetails.length() > 0)
	{
		if (m_CriticalError || (m_DataStatus == data_status_error))
		{
			CString t;
			t.LoadString(error_title);
			MessageBoxTimeoutA(m_hWnd, m_DataStatusDetails.c_str(), t, MB_ICONERROR | MB_OK | MB_DEFBUTTON1, LANG_NEUTRAL, CLMSPatternsChartApp::get_timeout_dialog() * 60000);
		}
		else
		{
			CString t;
			t.LoadString(information_title);
			MessageBoxTimeoutA(m_hWnd, m_DataStatusDetails.c_str(), t, MB_ICONINFORMATION | MB_OK | MB_DEFBUTTON1, LANG_NEUTRAL, CLMSPatternsChartApp::get_timeout_dialog() * 60000);
		}
	}
}

//// Update the data connection status : live / disconnected / error
//void CLMSPatternsChartCtrl::set_data_status(data_status_code status, string msg)
//{
//	// Once critical, always critical!
//	if (!m_CriticalError)
//	{
//		m_DataStatus = status;
//		m_DataStatusDetails = msg;
//	}
//
//	if (::IsWindow(m_hWnd))
//	{
//		switch (m_DataStatus)
//		{
//		case data_status_connected:
//			get_button("data status")->set_icon("connected");
//			break;
//
//		case data_status_no_data:
//			get_button("data status")->set_icon("no_data");
//			break;
//
//		case data_status_recovery:
//			get_button("data status")->set_icon("recovery");
//			break;
//
//		case data_status_error:
//		default:
//			get_button("data status")->set_icon("error");
//			break;
//		}
//	}
//}

// Adjust the size and position of the controls based on the current control size and settings
//void CLMSPatternsChartCtrl::adjust_controls()
//{
//	CRect rect;
//	GetClientRect(&rect);
//
//	int cx = rect.Width();
//	int cy = rect.Height();
//
//	m_Tracing->zoom(false);
//	
//	int tracing_height = cy - ((display_patient_banner() ? BANNER_HEIGHT : 0) + BOTTOM_HEIGHT);
//	int tracing_required_width = this->calculate_required_tracing_width(tracing_height);
//
//	if (cx < patterns_minimum_width || tracing_required_width < patterns_minimum_width)
//	{
//		// Calculate proper minimum resolution for message
//		int minimum_height = calculate_required_tracing_height(patterns_minimum_width);
//		if (display_patient_banner())
//		{
//			minimum_height += BANNER_HEIGHT;
//		}
//		minimum_height += BOTTOM_HEIGHT;
//
//		CString msg;
//		msg.LoadString(available_space_too_small);
//
//		CString t;
//		t.Format(msg, patterns_minimum_width, minimum_height);
//		m_Message->set_text(t.GetBuffer(t.GetLength()));
//
//		m_Tracing->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
//		m_Message->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
//	}
//	else
//	{
//		m_Message->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
//		if (tracing_required_width < cx)
//		{
//			m_Tracing->SetWindowPos(0, (cx - tracing_required_width) / 2, display_patient_banner() ? BANNER_HEIGHT : 0, tracing_required_width, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
//		}
//		else
//		{
//			m_Tracing->SetWindowPos(0, 0, display_patient_banner() ? BANNER_HEIGHT : 0, cx, tracing_height, SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED);
//		}
//	}
//
//	long top = cy + 4 - BOTTOM_HEIGHT;
//
//	// Reposition buttons
//
//	const UINT visible = SWP_SHOWWINDOW | SWP_NOCOPYBITS | SWP_FRAMECHANGED;
//	const UINT hidden = SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE;
//
//	int right = cx;
//	int left = 10;
//	int button_width = 0;
//
//	button_width = get_button("zoom")->optimal_width() + 5;
//	get_button("zoom")->SetWindowPos(0, left, top, button_width, 18, 0);
//	left += button_width;
//
//	get_button("data status")->SetWindowPos(0, left, top + 1, 101, 18, 0);
//	left += 110;
//
//	// From the right...
//	button_width = get_button("about")->optimal_width() + 5;
//	right -= button_width;
//	get_button("about")->SetWindowPos(0, right, top, button_width, 18, 0);
//
//	button_width = get_button("montevideo")->optimal_width() + 5;
//	right -= button_width;
//	get_button("montevideo")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);
//
//	button_width = get_button("events")->optimal_width() + 5;
//	right -= button_width;
//	get_button("events")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);
//	
//	button_width = get_button("toco")->optimal_width() + 5;
//	right -= button_width;
//	get_button("toco")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);
//	
//	button_width = get_button("baseline")->optimal_width() + 5;
//	right -= button_width;
//	get_button("baseline")->SetWindowPos(0, right, top, button_width, 18, right - left >= 10 ? visible:hidden);
//
//	// In the middle?
//	UINT flags = (right - left >= (7 * 18) + 10) ? visible : hidden;
//
//	get_button("go to beginning")->SetWindowPos(0, left, top, 18, 18, flags);
//	left += 18;
//	get_button("page left")->SetWindowPos(0, left, top, 18, 18, flags);
//	left += 18;
//	get_button("play left")->SetWindowPos(0, left, top, 18, 18, flags);
//	left += 18;
//	get_button("pause")->SetWindowPos(0, left, top, 18, 18, flags);
//	left += 18;
//	get_button("play right")->SetWindowPos(0, left, top, 18, 18, flags);
//	left += 18;
//	get_button("page right")->SetWindowPos(0, left, top, 18, 18, flags);
//	left += 18;
//	get_button("go to end")->SetWindowPos(0, left, top, 18, 18, flags);
//
//	m_Tracing->refresh_layout();
//}
//
//// On resize of the control, reposition the command bar
//void CLMSPatternsChartCtrl::OnSize(UINT t, int cx, int cy)
//{
//	COleControl::OnSize(t, cx, cy);
//	adjust_controls();
//}

/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// DRAWING

//int CLMSPatternsChartCtrl::calculate_required_tracing_width(int height)
//{
//	static int line_height = -1;
//
//	// Calculate a line of text height
//	if (line_height < 0)
//	{
//		CPaintDC dc(this);
//		services::select_font(80, "Arial", &dc);
//		line_height = dc.GetTextExtent("W").cy;
//	}
//
//	// Substract the minimum for the fixed part
//	return (int)((height - ((4 * (line_height + 1)) + patterns_fix_required_height)) / patterns_ratio_coefficient);
//}
//
//int CLMSPatternsChartCtrl::calculate_required_tracing_height(int width)
//{
//	static int line_height = -1;
//
//	// Calculate a line of text height
//	if (line_height < 0)
//	{
//		CPaintDC dc(this);
//		services::select_font(80, "Arial", &dc);
//		line_height = dc.GetTextExtent("W").cy;
//	}
//
//	return (int)((width * patterns_ratio_coefficient) + (4 * (line_height + 1)) + patterns_fix_required_height);
//}
//
//// Update the banner
//void CLMSPatternsChartCtrl::refresh_banner(CDC* pdc)
//{
//	// No banner?
//	if (!display_patient_banner())
//		return;
//
//	// Grab the location of the banner
//	CRect bannerBox;
//	GetClientRect(&bannerBox);
//	bannerBox.bottom = bannerBox.top + BANNER_HEIGHT;
//
//	// Clear all in white
//	pdc->FillRect(bannerBox, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
//
//	// Draw line
//	pdc->SelectObject(&m_linePen);
//	pdc->MoveTo(bannerBox.left, bannerBox.bottom - 1);
//	pdc->LineTo(bannerBox.right, bannerBox.bottom - 1);
//
//	// Draw third party logo if any
//	if (m_Banner == 1) // PeriGen
//	{
//		CRect thirdPBox = services::get_bitmap_rectangle("PERIGEN_BANNER");
//		services::draw_bitmap(pdc, "PERIGEN_BANNER", "", bannerBox.right - thirdPBox.Width(), bannerBox.top, bannerBox.right, bannerBox.top + thirdPBox.Height(), services::ccenter);
//	}
//	else if (m_Banner == 2) // GE
//	{
//		CRect thirdPBox = services::get_bitmap_rectangle("GE_BANNER");
//		services::draw_bitmap(pdc, "GE_BANNER", "", bannerBox.right - thirdPBox.Width(), bannerBox.top, bannerBox.right, bannerBox.top + thirdPBox.Height(), services::ccenter);
//	}
//
//	// In error, don't display the content of the banner at all
//	if (m_CriticalError)
//		return;
//
//	// Draw the current patient info
//	input_adapter::patient* p = get_patient();
//	patterns::fetus* f = get_fetus();
//
//	// No patient?
//	if ((p == 0) || (f == 0))
//		return;
//
//	string banner_text = "Name: " + p->get_name();
//	if (p->get_surname().length() > 0) banner_text += ", " + p->get_surname();
//
//	banner_text += " | ID# " + p->get_accountno();
//
//	pdc->SetBkMode(TRANSPARENT);
//	pdc->SetTextColor(RGB(101, 92, 62));
//	pdc->SetTextAlign(TA_BASELINE);
//
//	pdc->SaveDC();
//
//	services::select_font(80, "Arial Bold", pdc);
//	pdc->TextOut(5, 4 + (BANNER_HEIGHT / 2), banner_text.c_str());
//
//	pdc->RestoreDC(-1);
//}
//
//// Update all command bar buttons
//void CLMSPatternsChartCtrl::refresh_command_bar(CDC* pdc)
//{
//	// Grab the location of the command bar
//	CRect barBox;
//	GetClientRect(&barBox);
//	barBox.top = barBox.bottom - BOTTOM_HEIGHT;
//
//	// Clear all in white
//	pdc->FillRect(barBox, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
//
//	// Draw line
//	pdc->SelectObject(&m_linePen);
//	pdc->MoveTo(barBox.left, barBox.top);
//	pdc->LineTo(barBox.right, barBox.top);
//
//	// Repaint individual button
//	for (map<string, icon_button*>::iterator i = m_buttons.begin(); i != m_buttons.end(); ++i)
//	{
//		i->second->Invalidate(TRUE);
//	}
//}
//
//// CLMSPatternsChartCtrl::OnDraw - Drawing function
//void CLMSPatternsChartCtrl::OnDraw(CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid)
//{
//	if (!pdc)
//		return;
//
//	double_buffer dbDC(pdc, &rcBounds);
//	refresh_banner(dbDC);
//	refresh_command_bar(dbDC);
//}
//
//// Optimization for drawing
//BOOL CLMSPatternsChartCtrl::OnEraseBkgnd(CDC *)
//{
//	return TRUE;
//}
//
//// Timer refresh...
void CLMSPatternsChartCtrl::OnTimer(UINT_PTR timer)
{
	PatternsChartCtrl::OnTimer(timer);
	//if ((m_Initialized) && (!m_DemoMode) && (timer == m_RefreshTimer))
	//{
	//	download_patient_data();
	//}

	//COleControl::OnTimer(timer);
}
//
///////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////
///// LOGS
//
//// An exception happened...
//void CLMSPatternsChartCtrl::log_exception(string msg, exception& e)
//{
//	log_exception(msg + "\r\n\r\n" + e.what());
//}
//
//// An exception happened...
//void CLMSPatternsChartCtrl::log_exception(string msg)
//{
//	CString text;
//	text.LoadString(an_error_happened);
//
//	CString message = text + "\r\n\r\n" + msg.c_str();
//	log_error((string)message);
//}
//
//// An error happened...
//void CLMSPatternsChartCtrl::log_error(string msg)
//{
//	trace(EVENTLOG_ERROR_TYPE, msg.c_str());
//
//	ClearPatient();
//	ClearRequest();
//
//	set_data_status(data_status_error, msg);
//}
//
//// Trace information in the Microsoft Windows Event log
//void CLMSPatternsChartCtrl::trace(WORD type, LPCTSTR szFormat, ...)
//{
//	static HANDLE m_hEventLog = NULL;
//	try
//	{
//		TCHAR szBuf[0x2000];
//
//		va_list pArg;
//		va_start(pArg, szFormat);
//		::_vsntprintf_s(szBuf, sizeof(szBuf) / sizeof(TCHAR), szFormat, pArg);
//		va_end(pArg);
//
//		::OutputDebugString(szBuf);
//		if (szBuf[::_tcslen(szBuf) - 1] != _T('\n'))
//		{
//			::OutputDebugString(_T("\n"));
//		}
//
//		if (m_hEventLog == NULL)
//		{
//			m_hEventLog = RegisterEventSource(NULL, _T("PeriGen Patterns ActiveX"));
//		}
//
//		if (m_hEventLog != NULL)
//		{
//			LPCTSTR msg = (LPCTSTR)(&szBuf[0]);
//			ReportEvent(m_hEventLog, type, 1, 0, NULL, 1,	0, &msg, NULL);
//		}
//	}
//	catch(...)
//	{
//		// Just ignore
//	}
//}
/////////////////////////////////////////////////
CString CLMSPatternsChartCtrl::LoadIconTextZoom()
{
	CString s;
	s.LoadString(icon_text_zoom);
	return s;
}
CString CLMSPatternsChartCtrl::LoadIconTextBaselines()
{
	CString s;
	s.LoadString(icon_text_baselines);
	return s;
}
CString CLMSPatternsChartCtrl::LoadIconTextToco()
{
	CString s;
	s.LoadString(icon_text_toco);
	return s;

}
CString CLMSPatternsChartCtrl::LoadIconTextEvents()
{
	CString s;
	s.LoadString(icon_text_events);
	return s;

}
CString CLMSPatternsChartCtrl::LoadIconTextMontevideo()
{
	CString s;
	s.LoadString(icon_text_montevideo);
	return s;

}
CString CLMSPatternsChartCtrl::LoadIconTextAbout()
{
	CString s;
	s.LoadString(icon_text_about);
	return s;

}
CString CLMSPatternsChartCtrl::LoadTextAvailableSpaceTooSmall()
{
	CString s;
	s.LoadString(available_space_too_small);
	return s;

}
CString CLMSPatternsChartCtrl::LoadTextAnErrorHappened()
{
	CString s;
	s.LoadString(an_error_happened);
	return s;

}
//////////////////////////////////////////////////

// LMSPatternsChartCtrl.cpp : Implementation of the CRIPatternsChartCtrl ActiveX Control class.

#include "stdafx.h"
#include "CRIPatternsChart.h"
#include "CRIPatternsChartCtrl.h"
#include "CRIPatternsChartPropPage.h"

#include "patterns gui, double buffer.h"
#include "patterns gui, services.h"

//#include "criutils.h"
//#include "aboutbox.h"

#include <iostream>
#include <fstream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define MSG_ABOUTBTN_CLICKED (WM_USER + 100)

#define MSG_STRIKEOUT_EVENT (WM_USER + 200)
#define MSG_ACCEPT_EVENT (WM_USER + 210)
#define MSG_STRIKEOUT_CONTRACTION (WM_USER + 220)

//
using namespace patterns_gui;
//

IMPLEMENT_DYNCREATE(CRIPatternsChartCtrl, COleControl)
DEFINE_GUID(IID_IDispatchEx, 0xa6ef9860, 0xc720, 0x11d0, 0x93, 0x37, 0x0, 0xa0, 0xc9, 0xd, 0xca, 0xa9);

//Implementation IObjectSafety
BEGIN_INTERFACE_MAP(CRIPatternsChartCtrl, COleControl)
	INTERFACE_PART(CRIPatternsChartCtrl, IID_IObjectSafety, ObjectSafety)
END_INTERFACE_MAP()

STDMETHODIMP CRIPatternsChartCtrl::XObjectSafety::GetInterfaceSafetyOptions(
	REFIID riid,	
	DWORD __RPC_FAR *pdwSupportedOptions,	
	DWORD __RPC_FAR *pdwEnabledOptions)
{
	METHOD_PROLOGUE_EX(CRIPatternsChartCtrl, ObjectSafety)

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

STDMETHODIMP CRIPatternsChartCtrl::XObjectSafety::SetInterfaceSafetyOptions(
	REFIID riid,	
	DWORD dwOptionSetMask,	
	DWORD dwEnabledOptions)
{
	METHOD_PROLOGUE_EX(CRIPatternsChartCtrl, ObjectSafety)

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

STDMETHODIMP_(ULONG) CRIPatternsChartCtrl::XObjectSafety::AddRef()
{
	METHOD_PROLOGUE_EX_(CRIPatternsChartCtrl, ObjectSafety)
		return (ULONG)pThis->ExternalAddRef();
}

STDMETHODIMP_(ULONG) CRIPatternsChartCtrl::XObjectSafety::Release()
{
	METHOD_PROLOGUE_EX_(CRIPatternsChartCtrl, ObjectSafety)
		return (ULONG)pThis->ExternalRelease();
}

STDMETHODIMP CRIPatternsChartCtrl::XObjectSafety::QueryInterface(REFIID iid, LPVOID* ppvObj)
{
	METHOD_PROLOGUE_EX_(CRIPatternsChartCtrl, ObjectSafety)
		return (HRESULT)pThis->ExternalQueryInterface(&iid, ppvObj);
}

#define BOTTOM_HEIGHT 26
#define BANNER_HEIGHT 36

////////////////////////

// Message map

BEGIN_MESSAGE_MAP(CRIPatternsChartCtrl, PatternsChartCtrl)
	ON_WM_TIMER()
	ON_WM_CREATE()
	ON_WM_DESTROY()
	ON_OLEVERB(AFX_IDS_VERB_EDIT, OnEdit)
	ON_OLEVERB(AFX_IDS_VERB_PROPERTIES, OnProperties)


	ON_BN_CLICKED(63, display_data_status)

	ON_BN_CLICKED(5, on_show_about)

	ON_CONTROL(patterns_gui::CRITracing::ndeleteevent, 200, on_strike_out_event)
	ON_CONTROL(patterns_gui::CRITracing::ndeletecontraction, 200, on_strike_out_contraction)
	ON_CONTROL(patterns_gui::CRITracing::nacceptevent, 200, on_accept_event)
	ON_MESSAGE(MSG_STRIKEOUT_EVENT, OnStrikeoutEventCallback)
	ON_MESSAGE(MSG_ACCEPT_EVENT, OnAcceptEventCallback)
	ON_MESSAGE(MSG_STRIKEOUT_CONTRACTION, OnStrikeoutContractionCallback)
END_MESSAGE_MAP()

// Dispatch map

BEGIN_DISPATCH_MAP(CRIPatternsChartCtrl, COleControl)
	DISP_PROPERTY_NOTIFY_ID(CRIPatternsChartCtrl, "ConnectionData", dispidConnectionData, m_ConnectionData, OnConnectionDataChanged, VT_BSTR)
END_DISPATCH_MAP()

// Event map

BEGIN_EVENT_MAP(CRIPatternsChartCtrl, COleControl)
END_EVENT_MAP()

// Property pages

// TODO: Add more property pages as needed. Remember to increase the count!
BEGIN_PROPPAGEIDS(CRIPatternsChartCtrl, 1)
PROPPAGEID(CRIPatternsChartPropPage::guid)
END_PROPPAGEIDS(CRIPatternsChartCtrl)

// Initialize class factory and guid
IMPLEMENT_OLECREATE_EX(CRIPatternsChartCtrl, "CRIPATTERNSCHART.CRIPatternsChartCtrl.1", 0x822d5cdc, 0x3062, 0x4c71, 0x8f, 0xa5, 0x71, 0x56, 0xe3, 0x8b, 0x2f, 0x1c)

// Type library ID and version
IMPLEMENT_OLETYPELIB(CRIPatternsChartCtrl, _tlid, _wVerMajor, _wVerMinor)

// Interface IDs
const IID BASED_CODE IID_DCRIPatternsChart = { 0x41681E08, 0xE263, 0x42D9, { 0xA3, 0x59, 0x84, 0x3C, 0xBD, 0x34, 0x5, 0x43 } };
const IID BASED_CODE IID_DCRIPatternsChartEvents = { 0x1A73C536, 0xB666, 0x4FDC, { 0x9D, 0x96, 0x89, 0x72, 0x42, 0xEE, 0x30, 0x48 } };
// Control type information
static const DWORD BASED_CODE _dwCRIPatternsChartOleMisc = OLEMISC_SETCLIENTSITEFIRST
| OLEMISC_INSIDEOUT
//| OLEMISC_ALWAYSRUN
| OLEMISC_ACTIVATEWHENVISIBLE
//| OLEMISC_IGNOREACTIVATEWHENVISIBLE
| OLEMISC_CANTLINKINSIDE
| OLEMISC_RECOMPOSEONRESIZE;

IMPLEMENT_OLECTLTYPE(CRIPatternsChartCtrl, IDS_LMSPATTERNSCHART, _dwCRIPatternsChartOleMisc)

// CRIPatternsChartCtrl::CRIPatternsChartCtrlFactory::UpdateRegistry -
// Adds or removes system registry entries for CRIPatternsChartCtrl
BOOL CRIPatternsChartCtrl::CRIPatternsChartCtrlFactory::UpdateRegistry(BOOL bRegister)
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
			_dwCRIPatternsChartOleMisc,
			_tlid,
			_wVerMajor,
			_wVerMinor);
	}

	return AfxOleUnregisterClass(m_clsid, m_lpszProgID);
}

// CRIPatternsChartCtrl::CRIPatternsChartCtrl - Constructor
CRIPatternsChartCtrl::CRIPatternsChartCtrl()
{
	CString buildFileVersion(FILEVERSTR);
	AddBuildVersionToSupportedVersions(buildFileVersion);

	m_currentPatternsVersion = m_supportedPatternsVersions[0];
	m_currentVersionIndex = 0;

	m_maxContractionRateValue = 30;
	m_contractionRateWindowSize = 30;

	m_contractionRateTrigger = 15;	

	m_criHWND = NULL;

	InitializeIIDs(&IID_DCRIPatternsChart, &IID_DCRIPatternsChartEvents);

	m_lessThanMinimumSpace = false;

	m_bIs30MinView = true;

	//m_Tracing = new CRITracing();
	//m_Tracing->set_can_delete(false); // Readonly by default!

	m_Tracing15MinView = new CRITracing();
	m_Tracing15MinView->set_can_delete(false);
	m_Tracing30MinView = new CRITracing();
	m_Tracing30MinView->set_can_delete(false);

	m_Tracing = m_bIs30MinView ? m_Tracing30MinView : m_Tracing15MinView;

	//m_Message = new CRIPopupMessage();
	
	m_conductor = new patterns::CRIConductor();
	m_conductor->set_input_adapter(new CRIViewerInputAdapter());

	// Create the initial patient AND fetus
	patterns::CRIFetus f;
	f.set_up_sample_rate(1);
	f.set_hr_sample_rate(4);

	CRIInputAdapter::patient* p = new CRIInputAdapter::patient();
	p->set_key("KEY");
	f.set_key("KEY");

	((CRIViewerInputAdapter*)(&m_conductor->get_input_adapter()))->add_patient(p, &f);


	m_tracingViewSizeInMinutes = m_bIs30MinView ? TRACINGS_VIEW_SIZE_30_MINUTES : TRACINGS_VIEW_SIZE_15_MINUTES;
	m_compressedViewSizeInMinutes = DEFAULT_COMPRESSED_VIEW_SIZE_MINUTES;

	//m_patternsRatioCoefficient = patterns_ratio_coefficient(m_tracingViewSizeInMinutes, m_compressedViewSizeInMinutes);
	m_patterns15MinRatioCoefficient = patterns_ratio_coefficient(TRACINGS_VIEW_SIZE_15_MINUTES, m_compressedViewSizeInMinutes);
	m_patterns30MinRatioCoefficient = patterns_ratio_coefficient(TRACINGS_VIEW_SIZE_30_MINUTES, m_compressedViewSizeInMinutes);
	m_patternsRatioCoefficient = m_bIs30MinView ? m_patterns30MinRatioCoefficient : m_patterns15MinRatioCoefficient;

}

// CRIPatternsChartCtrl::~CRIPatternsChartCtrl - Destructor
CRIPatternsChartCtrl::~CRIPatternsChartCtrl()
{
}

// CRIPatternsChartCtrl::DoPropExchange - Persistence support
void CRIPatternsChartCtrl::DoPropExchange(CPropExchange* pPX)
{
	ExchangeVersion(pPX, MAKELONG(_wVerMinor, _wVerMajor));
	COleControl::DoPropExchange(pPX);
}


// CRIPatternsChartCtrl message handlers
int CRIPatternsChartCtrl::OnCreate(LPCREATESTRUCT s)
{
	if(PatternsChartCtrl::OnCreate(s) == -1)
		return -1;
	return 0;

}

// Cleanup
void CRIPatternsChartCtrl::OnDestroy()
{
	if(m_waitTimer != 0)
	{
		KillTimer(m_waitTimer);
		m_waitTimer = 0;
	}
	PatternsChartCtrl::OnDestroy();
}

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
/////////////// DATA FUNCTION
patterns::CRIFetus* CRIPatternsChartCtrl::GetFetus()
{
	return dynamic_cast<CRIFetus*>(get_fetus());
}
// Process the data and adjust the display
void CRIPatternsChartCtrl::process_patient_data(TiXmlElement* rootNode)
{
	bool bDefaut30MinView = m_bIs30MinView;
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

	patterns::CRIFetus* f = GetFetus();
	CRIInputAdapter::patient* p = get_patient();

	// Suspend notifications so that GUI refreshes are done all at once
	f->suspend_notifications(true);

	TiXmlElement* tracingsNode = patientNode->FirstChildElement("tracings");
	TiXmlElement* actionsNode = patientNode->FirstChildElement("actions");
	TiXmlElement* artifactsNode = patientNode->FirstChildElement("artifacts");
	TiXmlElement* contractilitiesNode = patientNode->FirstChildElement("contractilities");
	TiXmlElement* requestNode = patientNode->FirstChildElement("request");	
	TiXmlElement* intervalsNode = patientNode->FirstChildElement("intervals");	

	int incremental = 0;
	requestNode->Attribute("incremental", &incremental);

	if(incremental == 0)
	{
		LoadCRIAlgorithmSettings(patientNode);
	}

	// Empty all data if requested to
	if (incremental == 0)
	{
		p->reset();
		f->clear();
		f->ResetContractilities();
		m_exportableIntervals.clear();
		m_Tracing15MinView->ResetExportData();
		m_Tracing30MinView->ResetExportData();

		if (m_isFirstRequest)
		{
			m_isFirstRequest = false;
			const char* default15MinView = rootNode->Attribute("Default15MinView");
			if (default15MinView != NULL)
			{
				CString defaultView(default15MinView);
				bDefaut30MinView = defaultView.CompareNoCase("true") != 0;
			}
		}

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
	//m_Tracing->set_can_delete(!patient_readonly && (m_Permissions.CompareNoCase("readonly") != 0));
	m_Tracing15MinView->set_can_delete(!patient_readonly && m_Permissions.CompareNoCase("readonly") != 0);
	m_Tracing30MinView->set_can_delete(!patient_readonly && m_Permissions.CompareNoCase("readonly") != 0);


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

	date edd = patterns::CRIConductor::to_integer(patientNode->Attribute("edd"));
	if (p->get_edc() != edd)
	{
		refresh_needed = true;
		p->set_edc(edd);

		if (edd != undetermined_date)
		{
			// Calculate the time edc - 4 weeks
			edd = (edd - (60 * 60 * 24 * 28)) + (2 * 60 * 60); // The 2 hours is there to fix the possible daylight saving issue where a day is 23 hours OR 25

			SYSTEMTIME st = CRIFetus::convert_to_SYSTEMTIME(edd);

			// Round it so that it's sure at midnight
			st.wHour = 0;
			st.wMinute = 0;
			st.wSecond = 0;
			st.wMilliseconds = 0;

			// Done
			edd = CRIFetus::convert_to_utc(CRIFetus::convert_to_time_t(st));
		}

		f->set_cutoff_date(edd);
	}

	date reset = patientNode->Attribute("reset") == 0 ? undetermined_date : patterns::CRIConductor::to_integer(patientNode->Attribute("reset"));
	if (f->get_reset_date() != reset)
	{
		f->set_reset_date(reset);
		refresh_needed = true;
	}

	long fc = patterns::CRIConductor::to_integer(patientNode->Attribute("fetus"));
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
			time_t blocktime = patterns::CRIConductor::to_integer(node->Attribute("start"));
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
	
			long basetime = patterns::CRIConductor::to_integer(artifactsNode->Attribute("basetime"));

			while (node != NULL)
			{
				lines.push_back(node->Attribute("data"));
				node = node->NextSiblingElement();
			}

			if (lines.size() > 0)
			{
				// Must reset the initial date for all events!
				CRIInputAdapter::load_saved_data(*f, lines, ((long)f->get_start_date() - basetime));
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////
	// Actions...
	long lastMVUActionTime = 0;
	bool mvuIsVisible = m_MVUButtonVisible;
	if (actionsNode != NULL)
	{
		node = actionsNode->FirstChildElement("action");
		while (node != NULL)
		{

			refresh_needed = true;

			int artifact_id = patterns::CRIConductor::to_integer(node->Attribute("artifact"));
			int action_type = patterns::CRIConductor::to_integer(node->Attribute("type"));
			long performedatTime = patterns::CRIConductor::to_integer(node->Attribute("performed"));
			//string performed = services::date_to_string(patterns::CRIConductor::to_integer(node->Attribute("performed")), patterns_gui::services::fnormal);
			string performed = services::date_to_string(performedatTime, patterns_gui::services::fnormal);
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
			case eMVUButtonOff:
				{
					if (lastMVUActionTime < performedatTime)
					{
						lastMVUActionTime = performedatTime;
						mvuIsVisible = false;
					}
				}
				break;
			case eMVUButtonOn:
				{
					if (lastMVUActionTime < performedatTime)
					{
						lastMVUActionTime = performedatTime;
						mvuIsVisible = true;
					}
				}
				break;
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
	// Contractilities...
	bool contractilitiesRefresh = false;
	if (contractilitiesNode != NULL)
	{
		refresh_needed = true;
		contractilitiesRefresh = true;
		node = contractilitiesNode->FirstChildElement("contractility");
		vector<patterns::Contractility> contractilities;
		if (node != NULL)
		{
			long baseTime = patterns::CRIConductor::to_integer(contractilitiesNode->Attribute("basetime"));
			long startDate = (long)f->get_start_date();
			long timeAdjustment = startDate - baseTime;
		
			while (node != NULL)
			{
				patterns::Contractility contractility;
				string contractilityData = node->Attribute("data");
				
				if(CRIConductor::ToContractility(contractility, contractilityData.substr(4), '|'))
				{	
					if (timeAdjustment != 0)
						contractility -= timeAdjustment;

					contractilities.push_back(contractility);
				}
				node = node->NextSiblingElement();	
			}
		}

		f->MergeContractilities(contractilities);
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
		if (refresh_contractions_rate || contractilitiesRefresh)
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
	//m_Tracing->show(patterns_gui::CRITracing::wshowdisclaimer, (m_Disclaimer > 0));
	m_Tracing30MinView->show(patterns_gui::CRITracing::wshowdisclaimer, (m_Disclaimer > 0));
	m_Tracing15MinView->show(patterns_gui::CRITracing::wshowdisclaimer, (m_Disclaimer > 0));


	// Suspend notifications so that GUI refreshes are done all at once
	f->suspend_notifications(false);

	if (incremental == 0)
	{
		//m_Tracing->zoom(false);
		//m_Tracing->move_to(f->get_number_of_fhr() + 1000000);
		//m_Tracing->scroll(true);
		m_Tracing15MinView->zoom(false);
		m_Tracing30MinView->zoom(false);
		m_Tracing15MinView->move_to(f->get_number_of_fhr() + 1000000);
		m_Tracing30MinView->move_to(f->get_number_of_fhr() + 1000000);
		m_Tracing15MinView->scroll(true);
		m_Tracing30MinView->scroll(true);
		m_Tracing->GetCompressedViewBoundsInSeconds(m_tracingLeftInSeconds, m_tracingRightInSeconds);
		if (mvuIsVisible != m_MVUButtonVisible)
		{
			ShowMVU(mvuIsVisible, false);
		}
		
	}

	UpdatePatternsAdapter();
	if (bDefaut30MinView != m_bIs30MinView)
		Switch15Min30MinViews();

}

// CRIPatternsChartCtrl message handlers
void CRIPatternsChartCtrl::OnConnectionDataChanged()
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
		//if (node->Attribute("logo") != 0)
		//{
		//	node->Attribute("logo", &m_logo);
		//}
		//m_Tracing30MinView->SetLogo(m_logo);
		//m_Tracing15MinView->SetLogo(m_logo);
		// Retrieve all values
		//CString patient_id = node->Attribute("patient_id");
		m_patientID = node->Attribute("patient_id");
		CString user_name_attribute = node->Attribute("user_name");
		CString user_name;
		int pos = user_name_attribute.Find("@%");
		//retrieve CRI client window handle
		m_criHWND = NULL;
		if(pos >= 0)
		{
			user_name = user_name_attribute.Left(pos);
			if(pos < user_name_attribute.GetLength() - 2)
			{
				CString criHandleStr = user_name_attribute.Mid(pos + 2);
				
				if(!criHandleStr.IsEmpty())
				{
					long nHaddle = atol(criHandleStr);
					m_criHWND = (HWND)nHaddle;	
				}
			}
		}
		else
		{
			user_name = user_name_attribute;
		}

		m_ServerURL = node->Attribute("server_url");
		
		m_UserID = node->Attribute("user_id");
		m_UserName = user_name; //node->Attribute("user_name");
		m_Permissions = node->Attribute("permissions");

		node->Attribute("refresh", &m_RefreshDelay);
		node->Attribute("cr_limit", &m_maxContractionRateValue);
		node->Attribute("cr_window", &m_contractionRateWindowSize);
		node->Attribute("cr_stage1", &m_contractionRateTrigger);
		node->Attribute("cr_stage2", &m_cr_stage2);
		node->Attribute("banner", &m_Banner);
				

		int timeout;
		node->Attribute("timeout_dlg", &timeout);
		CRIPatternsChartApp::set_timeout_dialog(timeout);

		// Apply contraction rate settings
		patterns_gui::CRITracing::SetMaxContractionRate(m_maxContractionRateValue);
		patterns::CRIFetus::SetContractionRateWindowSize(m_contractionRateWindowSize);
		patterns_gui::CRITracing::SetContractionRateTrigger(m_contractionRateTrigger);
		patterns_gui::CRITracing::set_cr_kcrstage(m_cr_stage2);

		// Apply user permission
		//m_Tracing->set_can_delete(m_Permissions.CompareNoCase("readonly") != 0);
		m_Tracing15MinView->set_can_delete(m_Permissions.CompareNoCase("readonly") != 0);
		m_Tracing30MinView->set_can_delete(m_Permissions.CompareNoCase("readonly") != 0);

		// Prepare the data request header for that patient
		PrepareInitialRequestHeader();
		//m_RequestHeader.Clear();
		//m_RequestHeader.Parse("<patients version=\"" FILEVERSTR "\"><request key=\"0\"/></patients>\r\n");
		//m_RequestHeader.FirstChildElement("patients")->SetAttribute("user", m_UserID);
		//m_RequestHeader.FirstChild("patients")->FirstChildElement("request")->SetAttribute("key", patient_id);

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
			//m_Tracing->zoom(false);
			//m_Tracing->move_to(get_fetus()->get_number_of_fhr() + 1000000);
			//m_Tracing->scroll(true);
			m_Tracing15MinView->zoom(false);
			m_Tracing30MinView->zoom(false);
			m_Tracing15MinView->move_to(get_fetus()->get_number_of_fhr() + 1000000);
			m_Tracing30MinView->move_to(get_fetus()->get_number_of_fhr() + 1000000);
			m_Tracing15MinView->scroll(true);
			m_Tracing30MinView->scroll(true);


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
/////////////////////////////////////////////////////////////////////
/// COMMAND BAR


void CRIPatternsChartCtrl::DoStrikeOutEvent()
{
	if (m_Tracing->get_selection_type() != patterns_gui::CRITracing::cevent)
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
// User ask to strike an event
void CRIPatternsChartCtrl::on_strike_out_event(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::CRITracing::cevent)
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

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, CRIPatternsChartApp::get_timeout_dialog() * 60000) == IDYES)
	{
		if(m_criHWND != NULL && IsWindow(m_criHWND))
		{
			if(m_waitTimer)
				KillTimer(m_waitTimer);
			m_waitTimer = SetTimer(10, 60200, NULL); 
			m_waitAuthenticationStatus = WaitForEventStrikeOut;
			EnableWindow(FALSE);
			::SendMessageA(m_criHWND, MSG_STRIKEOUT_EVENT, 0, (LPARAM)m_hWnd);
			return;
		}

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
void CRIPatternsChartCtrl::on_accept_event(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::CRITracing::cevent)
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

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, CRIPatternsChartApp::get_timeout_dialog() * 60000) == IDYES)
	{
		if(m_criHWND != NULL && IsWindow(m_criHWND))
		{
			if(m_waitTimer)
				KillTimer(m_waitTimer);
			m_waitTimer = SetTimer(10, 60200, NULL); 
			m_waitAuthenticationStatus = WaitForEventAccept;
			EnableWindow(FALSE);
			::SendMessageA(m_criHWND, MSG_ACCEPT_EVENT, 0, (LPARAM)m_hWnd);
			return;
		}

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


void CRIPatternsChartCtrl::DoAcceptEvent(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::CRITracing::cevent)
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

// User ask to strike a contraction
void CRIPatternsChartCtrl::on_strike_out_contraction(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::CRITracing::ccontraction)
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

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, CRIPatternsChartApp::get_timeout_dialog() * 60000) == IDYES)
	{
		if(m_criHWND != NULL && IsWindow(m_criHWND))
		{
			if(m_waitTimer)
				KillTimer(m_waitTimer);
			m_waitTimer = SetTimer(10, 60200, NULL); 
			m_waitAuthenticationStatus = WaitForContractionStrikeOut;
			EnableWindow(FALSE);
			::SendMessageA(m_criHWND, MSG_STRIKEOUT_CONTRACTION, 0, (LPARAM)m_hWnd);
			return;
		}

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


void CRIPatternsChartCtrl::DoStrikeOutContraction(void)
{
	if (m_Tracing->get_selection_type() != patterns_gui::CRITracing::ccontraction)
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

// Open the about box
void CRIPatternsChartCtrl::on_show_about(void)
{
	//if(m_criHWND != NULL && IsWindow(m_criHWND))
	//{	
	//	::SendMessage(m_criHWND, MSG_ABOUTBTN_CLICKED, 0, (LPARAM)m_hWnd);

	//	return;
	//}
	int timeout = CRIPatternsChartApp::get_timeout_dialog();
	CString checklist;
	if (m_ServerURL.GetLength() > 2 && m_ServerURL[m_ServerURL.GetLength() - 1] == '/')
		checklist = "CheckListPlugin";
	else
		checklist = "/CheckListPlugin";

	CString url = m_ServerURL + checklist;
	bool isChecklistApp = (m_criHWND != NULL && IsWindow(m_criHWND));
	ShowAboutDialog(url, true, isChecklistApp, timeout);
/*	switch (m_Banner)
	{
		case 1: //Power by PeriGen
		{
			aboutbox_powerby d(this);
			d.DoModal();
			
		}
		break;
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
	*/
}

// Open the about box
void CRIPatternsChartCtrl::display_data_status(void)
{
	if (m_DataStatusDetails.length() > 0)
	{
		if (m_CriticalError || (m_DataStatus == data_status_error))
		{
			CString t;
			t.LoadString(error_title);
			MessageBoxTimeoutA(m_hWnd, m_DataStatusDetails.c_str(), t, MB_ICONERROR | MB_OK | MB_DEFBUTTON1, LANG_NEUTRAL, CRIPatternsChartApp::get_timeout_dialog() * 60000);
		}
		else
		{
			CString t;
			t.LoadString(information_title);
			MessageBoxTimeoutA(m_hWnd, m_DataStatusDetails.c_str(), t, MB_ICONINFORMATION | MB_OK | MB_DEFBUTTON1, LANG_NEUTRAL, CRIPatternsChartApp::get_timeout_dialog() * 60000);
		}
	}
}



/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// DRAWING

// Timer refresh...
void CRIPatternsChartCtrl::OnTimer(UINT_PTR timer)
{
	//if ((m_Initialized) && (!m_DemoMode) && (timer == m_RefreshTimer))
	//{
	//	download_patient_data();
	//}
	if ((m_Initialized) && (timer == m_waitTimer))
	{
		m_waitAuthenticationStatus = WaitForNone;
		KillTimer(m_waitTimer);
		m_waitTimer = 0;
		EnableWindow(TRUE);
	}

	PatternsChartCtrl::OnTimer(timer);
}


void CRIPatternsChartCtrl::LoadCRIAlgorithmSettings(TiXmlElement* patientNode)
{
	TiXmlElement* settingsNode = patientNode->FirstChildElement("criAlgorithmSettings");
	if(settingsNode)
	{
		double minBaselineVariability = CRI_MINIMAL_BASELINE_VARIABILITY;
		settingsNode->Attribute("minBaselineVariability", &minBaselineVariability);
		CRITracing::SetMinBaselineVariability(minBaselineVariability);

		int minAccelAmount = CRI_MINIMAL_ACCEL_AMOUNT;
		settingsNode->Attribute("accelsRate", &minAccelAmount);
		CRITracing::SetMinAccelAmount(minAccelAmount);

		double lateDecelConfidence = CRI_MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL;
		settingsNode->Attribute("lateDecelConfidence", &lateDecelConfidence);
		CRITracing::SetMinLateDecelConfidence(lateDecelConfidence);

		int lateDecelsRate = CRI_MINIMAL_LATE_DECEL_AMOUNT;
		settingsNode->Attribute("lateDecelsRate", &lateDecelsRate);
		CRITracing::SetMinLateDecelAmount(lateDecelsRate);

		int prolongedDecelHeight = CRI_MINIMAL_PROLONGED_DECEL_HEIGHT;
		settingsNode->Attribute("prolongedDecelHeight", &prolongedDecelHeight);
		CRITracing::SetProlongedDecelHeight(prolongedDecelHeight);

		int lateAndProlongedDecelsRate = CRI_MINIMAL_LATE_AND_PROLONGED_DECEL_AMOUNT;
		settingsNode->Attribute("lateAndProlongedDecelsRate", &lateAndProlongedDecelsRate);
		CRITracing::SetLateAndProlongedDecelAmount(lateAndProlongedDecelsRate);

		int lateAndLargeAndLongDecelRate = CRI_MINIMAL_LATE_AND_LARGE_AND_LONG_DECEL_AMOUNT;
		settingsNode->Attribute("lateAndLargeAndLongDecelRate", &lateAndLargeAndLongDecelRate);
		CRITracing::SetLateAndLargeAndLongDecelAmount(lateAndLargeAndLongDecelRate);

		int largeAndLongDecelRate = CRI_MINIMAL_LARGE_AND_LONG_DECEL_AMOUNT;
		settingsNode->Attribute("largeAndLongDecelRate", &largeAndLongDecelRate);
		CRITracing::SetMinLargeAndLongDecelAmount(largeAndLongDecelRate);

		int contractionRate = CRI_MINIMAL_CONTRACTION_AMOUNT;
		settingsNode->Attribute("contractionRate", &contractionRate);
		CRITracing::SetContractionsAmount(contractionRate);

		int longContractionRate = CRI_MINIMAL_LONG_CONTRACTION_AMOUNT;
		settingsNode->Attribute("longContractionRate", &longContractionRate);
		CRITracing::SetMinLongContractionsAmount(longContractionRate);

		int qualificationWindowSize = CRI_STATE_QUALIFICATION_WINDOW_SIZE;
		settingsNode->Attribute("minDataWindowToQualify", &qualificationWindowSize);
		CRIFetus::SetQualificationWindowSize(qualificationWindowSize);

		int minimalAmountOfDataInWindow = CRI_MINIMAL_AMOUNT_OF_DATA_IN_WINDOW;
		settingsNode->Attribute("minAmountOfDataInWindow", &minimalAmountOfDataInWindow);
		CRIFetus::SetMinimalAmountOfDataInWindow(minimalAmountOfDataInWindow);
		
	}

}

long CRIPatternsChartCtrl::OnStrikeoutEventCallback(WPARAM wParam, LPARAM lParam)
{
	if(m_waitAuthenticationStatus != WaitForEventStrikeOut)
	{
		return 0;
	}
	m_waitAuthenticationStatus = WaitForNone;

	if(m_waitTimer != 0)
	{
		KillTimer(m_waitTimer);
	}

	EnableWindow(TRUE);

	if(wParam)
	{
		DoStrikeOutEvent();
	}
	return 0;
}

long CRIPatternsChartCtrl::OnAcceptEventCallback(WPARAM wParam, LPARAM lParam)
{	
	if(m_waitAuthenticationStatus != WaitForEventAccept)
	{
		return 0;
	}
	m_waitAuthenticationStatus = WaitForNone;

	if(m_waitTimer != 0)
	{
		KillTimer(m_waitTimer);
	}

	EnableWindow(TRUE);

	if(wParam)
	{
		DoAcceptEvent();
	}
	return 0;
}

long CRIPatternsChartCtrl::OnStrikeoutContractionCallback(WPARAM wParam, LPARAM lParam)
{
	if(m_waitAuthenticationStatus != WaitForContractionStrikeOut)
	{
		return 0;
	}

	m_waitAuthenticationStatus = WaitForNone;

	if(m_waitTimer != 0)
	{
		KillTimer(m_waitTimer);
	}

	EnableWindow(TRUE);

	if(wParam)
	{
		DoStrikeOutContraction();
	}
	return 0;
}
/////////////////////////////////////////////////
CString CRIPatternsChartCtrl::LoadIconTextZoom()
{
	CString s;
	s.LoadString(icon_text_zoom);
	return s;
}
CString CRIPatternsChartCtrl::LoadIconTextBaselines()
{
	CString s;
	s.LoadString(icon_text_baselines);
	return s;
}
CString CRIPatternsChartCtrl::LoadIconTextToco()
{
	CString s;
	s.LoadString(icon_text_toco);
	return s;

}
CString CRIPatternsChartCtrl::LoadIconTextEvents()
{
	CString s;
	s.LoadString(icon_text_events);
	return s;

}
CString CRIPatternsChartCtrl::LoadIconTextMontevideo()
{
	CString s;
	s.LoadString(icon_text_montevideo);
	return s;

}
CString CRIPatternsChartCtrl::LoadIconTextAbout()
{
	CString s;
	s.LoadString(icon_text_about);
	return s;

}
CString CRIPatternsChartCtrl::LoadTextAvailableSpaceTooSmall()
{
	CString s;
	s.LoadString(available_space_too_small);
	return s;

}
CString CRIPatternsChartCtrl::LoadTextAnErrorHappened()
{
	CString s;
	s.LoadString(an_error_happened);
	return s;

}
//////////////////////////////////////////////////

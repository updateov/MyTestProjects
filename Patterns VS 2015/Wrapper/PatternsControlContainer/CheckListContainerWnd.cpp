// CheckListContainerWnd.cpp : implementation file
//

#include "stdafx.h"

#include "CheckListContainerWnd.h"

#include "patterns gui, double buffer.h"
#include "patterns gui, services.h"

#include "PatternsVersionNumber.h"
#include "controlaboutbox.h"

#include <iostream>
#include <fstream>


using namespace patterns_gui;

// CheckListContainerWnd

IMPLEMENT_DYNAMIC(CheckListContainerWnd, ContainerBaseWnd)

CheckListContainerWnd::CheckListContainerWnd()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	m_maxContractionRateValue = 30;
	m_contractionRateWindowSize = 30;

	m_contractionRateTrigger = 15;	

	m_criHWND = NULL;
	ContainerBaseWnd::initialize_bitmaps();
	m_Tracing = new CRITracing();
	m_Tracing->set_can_delete(false); // Readonly by default!


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


}

CheckListContainerWnd::~CheckListContainerWnd()
{
}

void CheckListContainerWnd::SetInitialData(const CString& url, const CString& patientID)
{
	ContainerBaseWnd::SetInitialData(url, patientID);
	
	m_maxContractionRateValue = 10;
	m_contractionRateWindowSize = 10;
	m_contractionRateTrigger = 5;

	// Apply contraction rate settings
	patterns_gui::CRITracing::SetMaxContractionRate(m_maxContractionRateValue);
	patterns::CRIFetus::SetContractionRateWindowSize(m_contractionRateWindowSize);
	patterns_gui::CRITracing::SetContractionRateTrigger(m_contractionRateTrigger);
	patterns_gui::CRITracing::set_cr_kcrstage(m_cr_stage2);

	// Apply user permission
	m_Tracing->set_can_delete(m_Permissions.CompareNoCase("readonly") != 0);

}

BEGIN_MESSAGE_MAP(CheckListContainerWnd, ContainerBaseWnd)
	ON_WM_TIMER()
	ON_WM_CREATE()
	ON_WM_DESTROY()

	ON_BN_CLICKED(63, display_data_status)
	ON_BN_CLICKED(5, on_show_about)

	ON_CONTROL(patterns_gui::CRITracing::ndeleteevent, 200, on_strike_out_event)
	ON_CONTROL(patterns_gui::CRITracing::ndeletecontraction, 200, on_strike_out_contraction)
	ON_CONTROL(patterns_gui::CRITracing::nacceptevent, 200, on_accept_event)
	ON_MESSAGE(MSG_STRIKEOUT_EVENT, OnStrikeoutEventCallback)
	ON_MESSAGE(MSG_ACCEPT_EVENT, OnAcceptEventCallback)
	ON_MESSAGE(MSG_STRIKEOUT_CONTRACTION, OnStrikeoutContractionCallback)
END_MESSAGE_MAP()



// CheckListContainerWnd message handlers


int CheckListContainerWnd::OnCreate(LPCREATESTRUCT s)
{
	if(ContainerBaseWnd::OnCreate(s) == -1)
		return -1;
	return 0;

}

// Cleanup
void CheckListContainerWnd::OnDestroy()
{
	if(m_waitTimer != 0)
	{
		KillTimer(m_waitTimer);
		m_waitTimer = 0;
	}
	ContainerBaseWnd::OnDestroy();
}

//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
/////////////// DATA FUNCTION
patterns::CRIFetus* CheckListContainerWnd::GetFetus()
{
	return dynamic_cast<CRIFetus*>(get_fetus());
}
// Process the data and adjust the display
void CheckListContainerWnd::process_patient_data(TiXmlElement* rootNode)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
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
	if (actionsNode != NULL)
	{
		node = actionsNode->FirstChildElement("action");
		while (node != NULL)
		{

			refresh_needed = true;

			int artifact_id = patterns::CRIConductor::to_integer(node->Attribute("artifact"));
			int action_type = patterns::CRIConductor::to_integer(node->Attribute("type"));

			string performed = services::date_to_string(patterns::CRIConductor::to_integer(node->Attribute("performed")), patterns_gui::services::fnormal);
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
	m_Tracing->show(patterns_gui::CRITracing::wshowdisclaimer, (m_Disclaimer > 0));

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

/////////////////////////////////////////////////////////////////////
/// COMMAND BAR


void CheckListContainerWnd::DoStrikeOutEvent()
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
void CheckListContainerWnd::on_strike_out_event(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
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

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, get_timeout_dialog() * 60000) == IDYES)
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
void CheckListContainerWnd::on_accept_event(void)
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

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, get_timeout_dialog() * 60000) == IDYES)
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


void CheckListContainerWnd::DoAcceptEvent(void)
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
void CheckListContainerWnd::on_strike_out_contraction(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
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

	if (MessageBoxTimeoutA(m_hWnd, s, t, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2, LANG_NEUTRAL, get_timeout_dialog() * 60000) == IDYES)
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


void CheckListContainerWnd::DoStrikeOutContraction(void)
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
void CheckListContainerWnd::on_show_about(void)
{
	//if(m_criHWND != NULL && IsWindow(m_criHWND))
	//{	
	//	::SendMessage(m_criHWND, MSG_ABOUTBTN_CLICKED, 0, (LPARAM)m_hWnd);

	//	return;
	//}
	//
	switch (m_Banner)
	{
		case 1: //Power by PeriGen
		{
			aboutbox_powerby d(this);
			AFX_MANAGE_STATE(AfxGetStaticModuleState());
			d.DoModal();
			
		}
		break;
		//case 2: // GE
		//{
		//	aboutbox_ge d(this);
		//	d.DoModal();
		//	break;
		//}

		default: // PeriGen
		{
			aboutbox d(this);
			AFX_MANAGE_STATE(AfxGetStaticModuleState());

			d.DoModal();
			break;
		}
	}
}


void CheckListContainerWnd::display_data_status(void)
{
	if (m_DataStatusDetails.length() > 0)
	{
		if (m_CriticalError || (m_DataStatus == data_status_error))
		{
			CString t;
			t.LoadString(error_title);
			MessageBoxTimeoutA(m_hWnd, m_DataStatusDetails.c_str(), t, MB_ICONERROR | MB_OK | MB_DEFBUTTON1, LANG_NEUTRAL, get_timeout_dialog() * 60000);
		}
		else
		{
			CString t;
			t.LoadString(information_title);
			MessageBoxTimeoutA(m_hWnd, m_DataStatusDetails.c_str(), t, MB_ICONINFORMATION | MB_OK | MB_DEFBUTTON1, LANG_NEUTRAL, get_timeout_dialog() * 60000);
		}
	}
}



/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////
/// DRAWING

// Timer refresh...
void CheckListContainerWnd::OnTimer(UINT_PTR timer)
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

	ContainerBaseWnd::OnTimer(timer);
}


void CheckListContainerWnd::LoadCRIAlgorithmSettings(TiXmlElement* patientNode)
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

long CheckListContainerWnd::OnStrikeoutEventCallback(WPARAM wParam, LPARAM lParam)
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

long CheckListContainerWnd::OnAcceptEventCallback(WPARAM wParam, LPARAM lParam)
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

long CheckListContainerWnd::OnStrikeoutContractionCallback(WPARAM wParam, LPARAM lParam)
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
//////////////////////////////////////////////////

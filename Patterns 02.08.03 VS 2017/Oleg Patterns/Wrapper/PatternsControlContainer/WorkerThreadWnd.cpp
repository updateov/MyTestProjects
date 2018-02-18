#include "StdAfx.h"
#include "WorkerThreadWnd.h"
#include "patterns gui, cri tracing.h"
#include "DataStatus.h"

CString  WorkerThreadWnd::m_className;
IMPLEMENT_DYNAMIC(WorkerThreadWnd, CWnd)
IMPLEMENT_DYNAMIC(ChecklistWorkerThreadWnd, WorkerThreadWnd)
IMPLEMENT_DYNAMIC(PatternsWorkerThreadWnd, WorkerThreadWnd)

WorkerThreadWnd::WorkerThreadWnd()
{

	m_pData = 0;

}


WorkerThreadWnd::~WorkerThreadWnd()
{
	delete m_pData;
}

//TODO use struct for initial data
void WorkerThreadWnd::SetControlData(BaseControlData* pData)
{
	m_pData = pData;
}

BOOL WorkerThreadWnd::Create()
{
	
	if (m_className.IsEmpty())
	{
		m_className = AfxRegisterWndClass(CS_PARENTDC);
	}
	if (!CreateEx(WS_EX_TOOLWINDOW, m_className, "", WS_POPUP, 0, 0, 0, 0, NULL, 0))
	{
		ASSERT(0);
		return FALSE;
	}

	
	
	return TRUE;

}


LRESULT WorkerThreadWnd::WindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	try
	{
		return CWnd::WindowProc(message, wParam, lParam);
	}
	catch (CException* e)
	{
		TCHAR	szCause[255];
		CString strFormatted;
		e->GetErrorMessage(szCause, 255);
		CString errStr;
		errStr.Format("Exception Caught in CEITextGeneratorThread::WindowProc(): %s", szCause);
		trace(EVENTLOG_ERROR_TYPE, errStr);		
		e->Delete();
	}

	return 0;
}

void WorkerThreadWnd::trace(WORD type, LPCTSTR szFormat, ...)
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
			ReportEvent(m_hEventLog, type, 1, 0, NULL, 1, 0, &msg, NULL);
		}
	}
	catch (...)
	{
		// Just ignore
	}
}

BEGIN_MESSAGE_MAP(WorkerThreadWnd, CWnd)
	ON_WM_DESTROY()
	ON_MESSAGE(MSG_INITIALIZATION, OnInitMsg)
END_MESSAGE_MAP()


void WorkerThreadWnd::OnDestroy()
{
	CWnd::OnDestroy();
}
void WorkerThreadWnd::PostIntializationFailureMessage()
{
	InitializationResult* pResult = new InitializationResult();
	pResult->m_error = m_error;
	pResult->m_bSucceed = false;
	pResult->m_status = DataStatus::data_status_error;
	pResult->m_statusDetails = m_error;
	::SendMessage(m_pData->m_controlWnd, MSG_END_INITIAL_REQUEST, TRUE, (LPARAM)pResult);
	AfxGetThread()->PostThreadMessage(WM_QUIT, 0, 0);
}

long WorkerThreadWnd::OnInitMsg(WPARAM wParam, LPARAM lParam)
{
	InitializationResult* pResult = NULL;
	if (!m_DataLock.try_acquire())
	{
		m_error = "Initialization failed";		
		PostIntializationFailureMessage();		
		return 0;
	}
	try
	{
		//// Request data from the server
		//string response = utils::perform_server_postrequest(string((LPCSTR)m_pData->m_ServerURL) + string("data"), utils::to_string(*m_pData->m_pRequestHeader), "PATTERNS_CONTROL", 10000) + "\r\n";

		//// Failed?
		//if (response.length() == 2) // 2 because of the + "\r\n"
		//{
		//	m_error = "Unable to reach the server";
		//	PostIntializationFailureMessage();
		//	m_DataLock.release();
		//	return 0;
		//}

		//// Parse the answer
		//TiXmlDocument xmlDoc;
		//if (xmlDoc.Parse(response.c_str()) == 0)
		//{
		//	m_error = "Server response has invalid data format, unable to parse it as a proper xml";
		//	PostIntializationFailureMessage();
		//	m_DataLock.release();
		//	return 0;
		//}

		//// Fetch the data
		//TiXmlElement* rootNode = xmlDoc.RootElement();
		//if (rootNode == NULL)
		//{
		//	m_error = "Server response is empty";
		//	PostIntializationFailureMessage();
		//	m_DataLock.release();
		//	return 0;
		//	
		//}

		//// Process the response
		//pResult = LoadData(rootNode);
		pResult = DownloadData();

	}
	catch (exception& e)
	{
		
		m_error = "Error while requesting server data";
		log_exception("Error while requesting server data", e);
		PostIntializationFailureMessage();
		m_DataLock.release();
		return 0;
	}
	catch (...)
	{
		log_exception("Unknown error while requesting server data");
		PostIntializationFailureMessage();
		m_DataLock.release();
		return 0;
	}

	m_DataLock.release();
	
	::SendMessage(m_pData->m_controlWnd, MSG_END_INITIAL_REQUEST, TRUE, (LPARAM)pResult);

	AfxGetThread()->PostThreadMessage(WM_QUIT, 0, 0);
	return 0;
}
InitializationResult* WorkerThreadWnd::DownloadData()
{
	InitializationResult* pResult = NULL;
	// Request data from the server
	string response = utils::perform_server_postrequest(string((LPCSTR)m_pData->m_ServerURL) + string("data"), utils::to_string(*m_pData->m_pRequestHeader), "PATTERNS_CONTROL", 10000) + "\r\n";

	// Failed?
	if (response.length() == 2) // 2 because of the + "\r\n"
	{
		m_error = "Unable to reach the server";
		PostIntializationFailureMessage();
		m_DataLock.release();
		return 0;
	}

	// Parse the answer
	TiXmlDocument xmlDoc;
	if (xmlDoc.Parse(response.c_str()) == 0)
	{
		m_error = "Server response has invalid data format, unable to parse it as a proper xml";
		PostIntializationFailureMessage();
		m_DataLock.release();
		return 0;
	}

	// Fetch the data
	TiXmlElement* rootNode = xmlDoc.RootElement();
	if (rootNode == NULL)
	{
		m_error = "Server response is empty";
		PostIntializationFailureMessage();
		m_DataLock.release();
		return 0;

	}

	// Check for invalid version
	int invalid_version = 0;

	rootNode->Attribute("invalid_version", &invalid_version);
	if (invalid_version != 0)
	{
		if (RetryInitialRequest(pResult) && pResult != NULL)
		{
			return pResult;
		}
	}


	// Process the response
	pResult = LoadData(rootNode);

	return pResult;
}

// An exception happened...
void WorkerThreadWnd::log_exception(string msg, exception& e)
{
	log_exception(msg + "\r\n\r\n" + e.what());
}

// An exception happened...
void WorkerThreadWnd::log_exception(string msg)
{	
	string message = msg.c_str();
	log_error((string)message);
}

// An error happened...
void WorkerThreadWnd::log_error(string msg)
{
	trace(EVENTLOG_ERROR_TYPE, msg.c_str());
	m_error = msg.c_str();

}
void WorkerThreadWnd::ParseGA(string GA)
{
	CString gaStr(GA.c_str());

	if (m_pData->m_GA->Compare(gaStr) != 0)
	{
		*m_pData->m_GA = gaStr;
		long plus = gaStr.Find("+");
		CString weeks = (plus > 0) ? gaStr.Left(plus) : gaStr;
		CString days = (plus >= 0 && plus < gaStr.GetLength() - 1) ?
			gaStr.Mid(plus + 1) : "";

		weeks.TrimRight();
		long totalDays = 0;
		if (!weeks.IsEmpty())
		{
			int w = atoi(weeks);
			totalDays = 7 * w;
		}
		days.TrimLeft();
		if (!days.IsEmpty())
		{
			int d = atoi(days);
			totalDays += d;
		}
		*m_pData->m_gaTotalDays = totalDays;

	}
}


void WorkerThreadWnd::LoadExportableIntervals(TiXmlElement* intervalsNode)
{
	if (m_pData->m_bExportEnabled)
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

void WorkerThreadWnd::AddExportableInterval(long basetime, string data)
{
	if (!m_pData->m_bExportEnabled)
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
		if (m_pData->m_exportableIntervals->size() == 0)
			m_pData->m_exportableIntervals->push_back(chunk);
		else if (id > m_pData->m_exportableIntervals->back().m_id)
		{
			int oldIndex = FindIntervalByStartTime(intervalStartTime);
			if (oldIndex == -1)
			{
				m_pData->m_exportableIntervals->push_back(chunk);
			}
			else if(!m_pData->m_exportableIntervals->at(oldIndex).IsExported())
			{
				m_pData->m_exportableIntervals->at(oldIndex) = chunk;
			}
		}
		else
		{
			int oldIndex = FindIntervalByID(id);
			if (oldIndex != -1 && oldIndex < m_pData->m_exportableIntervals->size())
			{
				m_pData->m_exportableIntervals->at(oldIndex) = chunk;
			}
		}
	}

}

int WorkerThreadWnd::FindIntervalByID(long id)
{
	int index = -1;
	for (int i = m_pData->m_exportableIntervals->size() - 1; i >= 0; i--)
	{
		if ((*m_pData->m_exportableIntervals)[i].m_id == id)
		{
			index = i;
			break;
		}
	}
	return index;
}

int WorkerThreadWnd::FindIntervalByStartTime(time_t startTime)
{
	int index = -1;
	for (int i = m_pData->m_exportableIntervals->size() - 1; i >= 0; i--)
	{
		if ((*m_pData->m_exportableIntervals)[i].m_startTime == startTime)
		{
			index = i;
			break;
		}
	}
	return index;

}

void WorkerThreadWnd::UpdateExportTimeRange(TiXmlElement* requestNode)
{
	if (m_pData->m_bExportEnabled)
	{
		int intervalduration = 30;
		requestNode->Attribute("intervalDuration", &intervalduration);

		if (intervalduration != *m_pData->m_intervalDuration)
		{
			*m_pData->m_intervalDuration = intervalduration;
		}
	}
}

void WorkerThreadWnd::PrepareInitialRequestHeader()
{
	m_pData->m_pRequestHeader->Clear();
	CString str;
	str.Format("<patients version=\"%s\"><request key=\"0\"/></patients>\r\n", *m_pData->m_currentPatternsVersion);
	//m_RequestHeader.Parse("<patients version=\"" FILEVERSTR "\"><request key=\"0\"/></patients>\r\n");
	m_pData->m_pRequestHeader->Parse(str);
	m_pData->m_pRequestHeader->FirstChildElement("patients")->SetAttribute("user", m_pData->m_userID);
	m_pData->m_pRequestHeader->FirstChild("patients")->FirstChildElement("request")->SetAttribute("key", m_pData->m_patientID);
}

bool WorkerThreadWnd::RetryInitialRequest(InitializationResult*& pResult)
{
	if (*m_pData->m_pisInitialRequest)
	{
		(*m_pData->m_pCurrentVersionIndex)++;
		if (*m_pData->m_pCurrentVersionIndex < m_pData->m_pSupportedPatternsVersions->GetSize())
		{
			
			*m_pData->m_currentPatternsVersion = (*m_pData->m_pSupportedPatternsVersions)[*m_pData->m_pCurrentVersionIndex];

			PrepareInitialRequestHeader();
			pResult = DownloadData();
			return true;
		}
		else
		{
			*m_pData->m_pisInitialRequest = false;
		}		

	}
	return false;
}
void PatternsWorkerThreadWnd::SetControlData(PatternsInitialData* pData)
{
	WorkerThreadWnd::SetControlData(pData);
	PostMessage(MSG_INITIALIZATION, 0, 0);

}

InitializationResult* PatternsWorkerThreadWnd::LoadData(TiXmlElement* rootNode)
{
	
	InitializationResult* pResult = new InitializationResult();
	// Check for invalid version
	int invalid_version = 0;
	rootNode->Attribute("invalid_version", &invalid_version);
	if (invalid_version != 0)
	{
		m_error = "This version of the application is outdated.";
		*m_pData->m_pCriticalError = true;

		pResult->m_error = m_error;
		pResult->m_bSucceed = false;
		return pResult;
	}


	// Retrieve data
	TiXmlElement* patientNode = rootNode->FirstChildElement("patient");
	if (patientNode == NULL)
	{
		m_error = "Invalid response, no patient node found";
		pResult->m_error = m_error;
		pResult->m_bSucceed = false;
	}

	patterns::fetus* f = ((PatternsInitialData*)m_pData)->m_pFetus;
	input_adapter::patient* p = ((PatternsInitialData*)m_pData)->m_pPatient;

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
		m_pData->m_exportableIntervals->clear();

		*m_pData->m_pisFirstRequest = false;
		const char* default15MinView = rootNode->Attribute("Default15MinView");
		if (default15MinView != NULL)
		{
			CString defaultView(default15MinView);
			pResult->m_b30MinView = defaultView.CompareNoCase("true") != 0;
		}

		const char* roundBaselineFHRAttr = rootNode->Attribute("MeanBaselineFHRRoundingTo");
		int roundValue = 1;
		if (roundBaselineFHRAttr != NULL)
		{
			rootNode->Attribute("MeanBaselineFHRRoundingTo", &roundValue);
		}
		pResult->m_roundBaselineFHRValue = roundValue <= 0 ? 1 : roundValue;

		const char* roundMontevideoAttr = rootNode->Attribute("MVURoundingTo");
		roundValue = 1;
		if (roundMontevideoAttr != NULL)
		{
			rootNode->Attribute("MVURoundingTo", &roundValue);
		}
		pResult->m_roundByMontevideoUnits = roundValue <= 0 ? 1 : roundValue;

	}
	
	//////////////////////////////////////////////////////////////////////////////////
	// Patient data...
	p->set_key("KEY");
	p->set_id((string)patientNode->Attribute("id"));

	const char* gaAttr = patientNode->Attribute("ga");
	if (gaAttr != NULL)
	{
		string ga = cleanString((string)(gaAttr));
		ParseGA(ga);
	}

	bool update_banner = false;
	if (m_pData->m_displayPatientBanner)
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

	
	patientNode->Attribute("status", &m_status);
	m_statusDetails = cleanString((string)(patientNode->Attribute("statusdetails")));

	// Readonly?
	bool patient_readonly = (patientNode->Attribute("readonly") != NULL) && (((CString)(patientNode->Attribute("readonly"))).CompareNoCase("true") == 0);
	
	pResult->m_readOnly = patient_readonly;

	switch (m_status)
	{
	case ePatientLive:
		if (!p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(true);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_connected;
		pResult->m_statusDetails = m_statusDetails;

		break;

	case ePatientUnplugged:
		if (p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(false);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_connected;
		pResult->m_statusDetails = m_statusDetails;
		break;

	case ePatientLate:
		if (!p->get_is_collecting_tracing() || !p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(true);
			p->set_is_late_collecting_tracing(true);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_connected;
		pResult->m_statusDetails = m_statusDetails;
		break;

	case ePatientRecovery:
		if (p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(false);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_recovery;
		pResult->m_statusDetails = m_statusDetails;
		break;

	case ePatientInvalid:
		pResult->m_status = DataStatus::data_status_no_data;
		pResult->m_statusDetails = m_statusDetails;
		break;
	case ePatientError:
		pResult->m_status = DataStatus::data_status_error;
		pResult->m_statusDetails = m_statusDetails;

	default:

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
	if (intervalsNode != NULL)
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
	long lastMVUActionTime = 0;
	bool mvuIsVisible = false;

	if (actionsNode != NULL)
	{
		node = actionsNode->FirstChildElement("action");
		while (node != NULL)
		{

			refresh_needed = true;

			int artifact_id = patterns::conductor::to_integer(node->Attribute("artifact"));
			int action_type = patterns::conductor::to_integer(node->Attribute("type"));
			long performedatTime = patterns::conductor::to_integer(node->Attribute("performed"));
			//string performed = services::date_to_string(patterns::conductor::to_integer(node->Attribute("performed")), patterns_gui::services::fnormal);
			string performed = services::date_to_string(performedatTime, patterns_gui::services::fnormal);
			string username = cleanString((string)(node->Attribute("username")));
			string userid = cleanString((string)(node->Attribute("userid")));

			if (userid.length() > 0)
			{
				username = username + " (" + userid + ")";
			}

					
			string strikeby = (LPCSTR)m_pData->m_strikeout_by_label;

			
			string restoreby = (LPCSTR)m_pData->m_restore_by_label;

			
			string confirmby = (LPCSTR)m_pData->m_confirm_by_label;

			
			string performedat = (LPCSTR)m_pData->m_action_performed_at;

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
	pResult->m_showMVU = mvuIsVisible;
	//////////////////////////////////////////////////////////////////////////////////
	// Request...
	if (requestNode != NULL)
	{
		TiXmlElement* request = m_pData->m_pRequestHeader->FirstChild("patients")->FirstChildElement("request");
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


	// Check for demo mode
	m_pData->m_Disclaimer = 0;
	rootNode->Attribute("demo_mode", m_pData->m_Disclaimer);

	
	// Suspend notifications so that GUI refreshes are done all at once
	f->suspend_notifications(false);

	pResult->m_updateBanner = update_banner;
	pResult->m_bSucceed = true;
	return pResult;

}

InitializationResult*  ChecklistWorkerThreadWnd::LoadData(TiXmlElement* rootNode)
{
	
	InitializationResult* pResult = new InitializationResult();
	// Check for invalid version
	int invalid_version = 0;
	rootNode->Attribute("invalid_version", &invalid_version);
	if (invalid_version != 0)
	{		
		m_error ="This version of the application is outdated.";
		*m_pData->m_pCriticalError = true;

		pResult->m_error = m_error;
		pResult->m_bSucceed = false;
		return pResult;
	}


	// Retrieve data
	TiXmlElement* patientNode = rootNode->FirstChildElement("patient");
	if (patientNode == NULL)
	{
		m_error = "Invalid response, no patient node found";
		pResult->m_error = m_error;
		pResult->m_bSucceed = false;
		return pResult;
	}

	

	patterns::CRIFetus* f = ((ChecklistInitialData*)m_pData)->m_pFetus;
	CRIInputAdapter::patient* p = ((ChecklistInitialData*)m_pData)->m_pPatient;

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

	if (incremental == 0)
	{
		LoadCRIAlgorithmSettings(patientNode);
	}

	// Empty all data if requested to
	if (incremental == 0)
	{
		p->reset();
		f->clear();
		f->ResetContractilities();

		*m_pData->m_pisFirstRequest = false;

		const char* default15MinView = rootNode->Attribute("Default15MinView");
		if (default15MinView != NULL)
		{
			CString defaultView(default15MinView);
			pResult->m_b30MinView = defaultView.CompareNoCase("true") != 0;
		}

		const char* roundBaselineFHRAttr = rootNode->Attribute("MeanBaselineFHRRoundingTo");
		int roundValue = 1;
		if (roundBaselineFHRAttr != NULL)
		{
			rootNode->Attribute("MeanBaselineFHRRoundingTo", &roundValue);
		}
		pResult->m_roundBaselineFHRValue = roundValue <= 0 ? 1 : roundValue;

		const char* roundMontevideoAttr = rootNode->Attribute("MVURoundingTo");
		roundValue = 1;
		if (roundMontevideoAttr != NULL)
		{
			rootNode->Attribute("MVURoundingTo", &roundValue);
		}
		pResult->m_roundByMontevideoUnits = roundValue <= 0 ? 1 : roundValue;

	}


	//////////////////////////////////////////////////////////////////////////////////
	// Patient data...
	p->set_key("KEY");
	p->set_id((string)patientNode->Attribute("id"));
	const char* gaAttr = patientNode->Attribute("ga");
	if (gaAttr != NULL)
	{
		string ga = cleanString((string)(gaAttr));
		ParseGA(ga);
	}

	bool update_banner = false;
	if (m_pData->m_displayPatientBanner)
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


	patientNode->Attribute("status", &m_status);
	m_statusDetails = cleanString((string)(patientNode->Attribute("statusdetails")));

	// Readonly?
	bool patient_readonly = (patientNode->Attribute("readonly") != NULL) && (((CString)(patientNode->Attribute("readonly"))).CompareNoCase("true") == 0);
	pResult->m_readOnly = patient_readonly;

	switch (m_status)
	{
	case ePatientLive:
		if (!p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(true);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_connected;
		pResult->m_statusDetails = m_statusDetails;

		break;

	case ePatientUnplugged:
		if (p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(false);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_connected;
		pResult->m_statusDetails = m_statusDetails;
		break;

	case ePatientLate:
		if (!p->get_is_collecting_tracing() || !p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(true);
			p->set_is_late_collecting_tracing(true);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_connected;
		pResult->m_statusDetails = m_statusDetails;
		break;

	case ePatientRecovery:
		if (p->get_is_collecting_tracing() || p->get_is_late_collecting_tracing())
		{
			p->set_is_collecting_tracing(false);
			p->set_is_late_collecting_tracing(false);
			f->note(subscription::mfetus);
		}
		pResult->m_status = DataStatus::data_status_recovery;
		pResult->m_statusDetails = m_statusDetails;
		break;

	case ePatientInvalid:
		pResult->m_status = DataStatus::data_status_no_data;
		pResult->m_statusDetails = m_statusDetails;
		break;
	case ePatientError:
		pResult->m_status = DataStatus::data_status_error;
		pResult->m_statusDetails = m_statusDetails;

	default:

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

	if (intervalsNode != NULL)
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
	bool mvuIsVisible = false;

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


			string strikeby = (LPCSTR)m_pData->m_strikeout_by_label;

			string restoreby = (LPCSTR)m_pData->m_restore_by_label;

			string confirmby = (LPCSTR)m_pData->m_confirm_by_label;

			string performedat = (LPCSTR)m_pData->m_action_performed_at;

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
	pResult->m_showMVU = mvuIsVisible;


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

				if (CRIConductor::ToContractility(contractility, contractilityData.substr(4), '|'))
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
		TiXmlElement* request = m_pData->m_pRequestHeader->FirstChild("patients")->FirstChildElement("request");
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

	pResult->m_updateBanner = update_banner;

	rootNode->Attribute("demo_mode", m_pData->m_Disclaimer);
	// Suspend notifications so that GUI refreshes are done all at once
	f->suspend_notifications(false);
	pResult->m_bSucceed = true;
	return pResult;

}


void ChecklistWorkerThreadWnd::LoadCRIAlgorithmSettings(TiXmlElement* patientNode)
{
	TiXmlElement* settingsNode = patientNode->FirstChildElement("criAlgorithmSettings");
	if (settingsNode)
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

void ChecklistWorkerThreadWnd::SetControlData(ChecklistInitialData* pData)
{
	WorkerThreadWnd::SetControlData(pData);

	PostMessage(MSG_INITIALIZATION, 0, 0);
}

BEGIN_MESSAGE_MAP(ChecklistWorkerThreadWnd, WorkerThreadWnd)
	ON_MESSAGE(MSG_SET_INITIAL_DATA, OnSetDataMsg)
END_MESSAGE_MAP()

long ChecklistWorkerThreadWnd::OnSetDataMsg(WPARAM wParam, LPARAM lParam)
{

	ChecklistInitialData* pData = (ChecklistInitialData*)lParam;
	SetControlData(pData);
	return 1;

}

BEGIN_MESSAGE_MAP(PatternsWorkerThreadWnd, WorkerThreadWnd)
	ON_MESSAGE(MSG_SET_INITIAL_DATA, OnSetDataMsg)
END_MESSAGE_MAP()

long PatternsWorkerThreadWnd::OnSetDataMsg(WPARAM wParam, LPARAM lParam)
{
	PatternsInitialData* pData = (PatternsInitialData*)lParam;
	SetControlData(pData);
	return 1;
}
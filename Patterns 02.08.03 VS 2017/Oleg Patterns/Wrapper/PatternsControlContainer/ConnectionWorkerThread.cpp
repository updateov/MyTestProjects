#include "StdAfx.h"
#include "ConnectionWorkerThread.h"

#include "..\PatternsActiveXChart\PatternsChart\utils.h"

CString  ConnectionWorkerThreadWnd::m_className;

IMPLEMENT_DYNAMIC(ConnectionWorkerThreadWnd, CWnd)
IMPLEMENT_DYNCREATE(ConnectionWorkerThread, CWinThread);

ConnectionWorkerThread::ConnectionWorkerThread(void)
{
	
}


ConnectionWorkerThread::~ConnectionWorkerThread(void)
{
	
}

BOOL ConnectionWorkerThread::InitInstance()
{
	
	MSG msg;
	BOOL result = PeekMessage(&msg, NULL, WM_USER, WM_USER, PM_NOREMOVE);

	m_threadWnd.Create();

	m_startEvent.SetEvent();

	//CString logStr;
	//logStr.Format("InitInstance for worker hThread = 0x%x", m_hThread);
	//ConnectionWorkerThreadWnd::trace(EVENTLOG_INFORMATION_TYPE, logStr);
	return TRUE;
}

int ConnectionWorkerThread::ExitInstance()
{
	//CString logStr;
	//logStr.Format("ExitInstance for worker hThread = 0x%x", m_hThread);
	//ConnectionWorkerThreadWnd::trace(EVENTLOG_INFORMATION_TYPE, logStr);
	m_threadWnd.DestroyWindow();
	return CWinThread::ExitInstance();
}

BOOL ConnectionWorkerThread::PreTranslateMessage(MSG *pMsg)
{
	switch (pMsg->message)
	{
	case CONNECTION_THREAD_INIT_MSG:
		
		return TRUE;
	
	default:
		return FALSE;
	}
}

void ConnectionWorkerThread::StopRequest()
{
	if(IsWindow(m_threadWnd.m_hWnd))
	{
		m_threadWnd.SendMessage(MSG_STOP_REQUEST, 0, 0);
	}
}

void ConnectionWorkerThread::SendRequest(TiXmlDocument& requestHeader)
{
	m_threadWnd.SetRequestHeader(&requestHeader);
	if(IsWindow(m_threadWnd.m_hWnd))
	{
		m_threadWnd.PostMessage(MSG_SEND_REQUEST, 0, 0);
	}
}
void ConnectionWorkerThread::SetData(const CString& url, HWND hCtrlWnd)
{
	m_threadWnd.SetData(url, hCtrlWnd);
}


ConnectionWorkerThreadWnd::ConnectionWorkerThreadWnd()
{
	m_stopRequest = false;
	m_refreshEnabled = false;
	m_hCtrlWnd = NULL;
}

ConnectionWorkerThreadWnd::~ConnectionWorkerThreadWnd()
{
}

BOOL ConnectionWorkerThreadWnd::Create()
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

LRESULT ConnectionWorkerThreadWnd::WindowProc(UINT message, WPARAM wParam, LPARAM lParam)
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
		errStr.Format("Exception Caught in ConnectionWorkerThreadWnd::WindowProc(): %s", szCause);
		trace(EVENTLOG_ERROR_TYPE, errStr);		
		e->Delete();
	}

	return 0;
}
BEGIN_MESSAGE_MAP(ConnectionWorkerThreadWnd, CWnd)
	ON_WM_DESTROY()
	ON_MESSAGE(MSG_SEND_REQUEST, OnSendRequest)
	ON_MESSAGE(MSG_STOP_REQUEST, OnStopRequest)
END_MESSAGE_MAP()

void ConnectionWorkerThreadWnd::OnDestroy()
{
	CWnd::OnDestroy();
}
long ConnectionWorkerThreadWnd::OnSendRequest(WPARAM wParam, LPARAM lParam)
{
	SendRequest();
	return 0;
}
long ConnectionWorkerThreadWnd::OnStopRequest(WPARAM wParam, LPARAM lParam)
{
	m_stopRequest = true;
	m_hCtrlWnd = NULL;
	PostMessage(WM_CLOSE, 0, 0);
	AfxGetThread()->PostThreadMessage(WM_QUIT, 0, 0);
	return 1;
}
void ConnectionWorkerThreadWnd::SetRequestHeader(TiXmlDocument* requestHeader)
{
	CSingleLock lock(&m_requestHeaderLock, TRUE);

	m_RequestHeader = *requestHeader;
	m_refreshEnabled = true; 
}

void ConnectionWorkerThreadWnd::GetRequestHeader(TiXmlDocument& requestHeader)
{
	CSingleLock lock(&m_requestHeaderLock, TRUE);
	requestHeader = m_RequestHeader;
}

void ConnectionWorkerThreadWnd::SendRequest()
{
	if(m_refreshEnabled && !m_stopRequest)
	{
		m_error.Empty();
		m_refreshEnabled = false;
		TiXmlDocument requestHeader;
		GetRequestHeader(requestHeader);
		try
		{
			
			string response = utils::perform_server_postrequest(string((LPCSTR)m_ServerURL) + string("data"), utils::to_string(requestHeader), "PATTERNS_CONTROL") + "\r\n";

			if(IsWindow(m_hCtrlWnd) && !m_stopRequest)
			{
				string* pResponse = new string(response);
				::SendMessageA(m_hCtrlWnd, MSG_REQUEST_RESPONSE, 0, (LPARAM)pResponse);
			}
		}
		catch (exception& e)
		{
		
			m_error = "Error while requesting server data";
			log_exception("Error while requesting server data", e);
			PostRequestFailureMessage();
		}
		catch (...)
		{
			m_error = "Error while requesting server data";
			log_exception("Unknown error while requesting server data");
			PostRequestFailureMessage();
		}

	}
	
}


void ConnectionWorkerThreadWnd::PostRequestFailureMessage()
{
	if(IsWindow(m_hCtrlWnd))
	{
		CString* pStr = new CString(m_error);
		::PostMessageA(m_hCtrlWnd, MSG_REQUEST_FAILURE, 0, (LPARAM)pStr);
	}
}


void ConnectionWorkerThreadWnd::trace(WORD type, LPCTSTR szFormat, ...)
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

// An exception happened...
void ConnectionWorkerThreadWnd::log_exception(string msg, exception& e)
{
	log_exception(msg + "\r\n\r\n" + e.what());
}

// An exception happened...
void ConnectionWorkerThreadWnd::log_exception(string msg)
{	
	string message = msg.c_str();
	log_error((string)message);
}

// An error happened...
void ConnectionWorkerThreadWnd::log_error(string msg)
{
	trace(EVENTLOG_ERROR_TYPE, msg.c_str());
	m_error = msg.c_str();

}

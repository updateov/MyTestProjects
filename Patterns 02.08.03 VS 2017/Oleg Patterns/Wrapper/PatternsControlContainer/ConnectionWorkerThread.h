#pragma once
#include "afxwin.h"
#include "tinyxml.h"

#define CONNECTION_THREAD_INIT_MSG (WM_USER + 1)

#define MSG_SET_REQUESTHEADER (WM_USER + 301)
#define MSG_REQUEST_RESPONSE (WM_USER + 302)
#define MSG_SEND_REQUEST (WM_USER + 303)
#define MSG_REQUEST_FAILURE (WM_USER + 304)
#define MSG_STOP_REQUEST (WM_USER + 305)

using namespace patterns;
class ConnectionWorkerThreadWnd :
	public CWnd
{
	DECLARE_DYNAMIC(ConnectionWorkerThreadWnd);
public:
	ConnectionWorkerThreadWnd();
	virtual ~ConnectionWorkerThreadWnd();
public: 
	BOOL Create();
protected:
	DECLARE_MESSAGE_MAP()
	afx_msg void OnDestroy();
	//long OnInitMsg(WPARAM wParam, LPARAM lParam);	
	long OnSendRequest(WPARAM wParam, LPARAM lParam);
	long OnStopRequest(WPARAM wParam, LPARAM lParam);
protected:
	void SendRequest();
protected:
	virtual LRESULT WindowProc(UINT message, WPARAM wParam, LPARAM lParam);
public:
	void SetRequestHeader(TiXmlDocument* requestHeader);
	void GetRequestHeader(TiXmlDocument& requestHeader);
	void SetData(const CString& url, HWND hCtrlWnd)
	{
		m_ServerURL = url;
		m_hCtrlWnd = hCtrlWnd;
	}
public:
	static void trace(WORD type, LPCTSTR szFormat, ...);
	void log_exception(string msg, exception& e);
	void log_exception(string msg);
	void log_error(string msg);
protected:
	void PostRequestFailureMessage();
protected:
	static CString m_className;
	CString m_error;

	TiXmlDocument m_RequestHeader;	
	CString m_ServerURL;
	HWND m_hCtrlWnd;

	bool m_refreshEnabled;
	bool m_stopRequest;
	CCriticalSection m_requestHeaderLock;

};

class ConnectionWorkerThread :
	public CWinThread
{
	DECLARE_DYNCREATE(ConnectionWorkerThread);
	
public:
	ConnectionWorkerThread(void);
	virtual ~ConnectionWorkerThread(void);
protected:
	public:
	BOOL PreTranslateMessage(MSG *pMsg);
	BOOL InitInstance();
	int ExitInstance();

public:
	void SetData(const CString& url, HWND hCtrlWnd);
	void SendRequest(TiXmlDocument& requestHeader);
	void StopRequest();
protected:
	//afx_msg LONG DoInitThread(WPARAM wParam, LPARAM lParam);
	
protected:
	ConnectionWorkerThreadWnd m_threadWnd;
	
public:
	CEvent m_startEvent;

};


#include "StdAfx.h"
#include "rendevouz.h"
#include "process.h"

using namespace rendevouz;

IMPLEMENT_DYNAMIC(CBaseThread, CObject)
IMPLEMENT_DYNAMIC(CUserThread, CBaseThread)
IMPLEMENT_DYNAMIC(CSyncRendevouz, CObject)
IMPLEMENT_DYNAMIC(CAsyncRendevouz, CSyncRendevouz)

CBaseThread::CBaseThread(HANDLE hStopEvent, volatile bool *bStop, unsigned(__stdcall *thread)(void *), bool bWait, LPVOID data) : CObject()
{
	ASSERT(bStop);

	m_bStop = bStop;
	m_hStopEvent = hStopEvent;
	m_pvUserData = data;
	m_bWaiting = bWait;
	m_hThreadHandle = HANDLE(_beginthreadex(NULL, 0, thread, this, bWait ? CREATE_SUSPENDED : 0, &m_uiThreadID));
}

CBaseThread::~CBaseThread()
{
	CloseHandle(m_hThreadHandle);
}

bool CBaseThread::Wait(DWORD dwTimeout) const
{
	Run();
	return WaitForSingleObject(m_hThreadHandle, dwTimeout) != WAIT_TIMEOUT;
}

CUserThread::CUserThread(unsigned(__stdcall *thread)(void *), bool bWait, LPVOID data) : CBaseThread(NULL, &m_bStopVar, thread, bWait, data)
{
	m_bStopVar = false;
	m_hStopEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
}

CUserThread::~CUserThread()
{
	CloseHandle(m_hStopEvent);
}

void CUserThread::TerminateThread()
{
	m_bStopVar = true;
	SetEvent(m_hStopEvent);
}

CSyncRendevouz::CSyncRendevouz(void) : CObject()
{
	m_bStop = FALSE;
	m_hStopEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
}

CSyncRendevouz::~CSyncRendevouz(void)
{
	CBaseThread *pThread;

	//	Stop all threads under our control and make
	//	sure they've exited.
	Stop();
	Wait();

	//	Now release resources
	CloseHandle(m_hStopEvent);

	//	And delete each thread object
	while (m_threads.GetCount())
	{
		pThread = m_threads.RemoveHead();

		ASSERT(pThread);
		ASSERT_KINDOF(CBaseThread, pThread);

		delete pThread;
	}
}

bool CSyncRendevouz::AddThread(unsigned(__stdcall *thread)(void *), volatile bool bWait, LPVOID data)
{
	if (m_handleArray.GetCount() > MAXIMUM_WAIT_OBJECTS - 1)
		return false;

	ASSERT(thread);

	CBaseThread *pThread = new CBaseThread(m_hStopEvent, &m_bStop, thread, bWait, data);

	ASSERT(pThread);
	ASSERT_KINDOF(CBaseThread, pThread);

	m_threads.AddTail(pThread);
	m_handleArray.Add(pThread->ThreadHandle());
	return true;
}

bool CSyncRendevouz::AddHandle(HANDLE hHandle)
{
	if (m_handleArray.GetCount() > MAXIMUM_WAIT_OBJECTS - 1)
		return false;

	m_handleArray.Add(hHandle);
	return true;
}

bool CSyncRendevouz::Wait(DWORD dwTimeout)
{
	CBaseThread *pThread;
	POSITION			 pos = m_threads.GetHeadPosition();

	while (pos != POSITION(NULL))
	{
		pThread = m_threads.GetNext(pos);

		ASSERT(pThread);
		ASSERT_KINDOF(CBaseThread, pThread);

		if (pThread->IsWaiting())
			pThread->Run();
	}

	return WaitForMultipleObjects(m_handleArray.GetCount(), m_handleArray.GetData(), TRUE, dwTimeout) != WAIT_TIMEOUT;
}

CAsyncRendevouz::CAsyncRendevouz(HWND wndTarget, UINT uiMsg, LPVOID pvUserData) : CSyncRendevouz()
{
	ASSERT(wndTarget);
	ASSERT(IsWindow(wndTarget));

	m_wndTarget = wndTarget;
	m_uiMsg = uiMsg;
	m_pvUserData = pvUserData;
	m_pThread = (CBaseThread *) NULL;
}

CAsyncRendevouz::~CAsyncRendevouz()
{
	delete m_pThread;
}

bool CAsyncRendevouz::Wait(DWORD dwTimeout)
{
	m_dwTimeout = dwTimeout;
	m_pThread = new CBaseThread(m_hStopEvent, &m_bStop, WaitProc, 0, LPVOID(this));
	return TRUE;
}

unsigned __stdcall CAsyncRendevouz::WaitProc(LPVOID data)
{
	{
		DEREF(data);

		CAsyncRendevouz *pThis = (CAsyncRendevouz *) pThread->UserData();

		ASSERT(pThis);
		ASSERT_KINDOF(CAsyncRendevouz, pThis);

		bool bResult = pThis->CSyncRendevouz::Wait(pThis->m_dwTimeout);

		if (IsWindow(pThis->m_wndTarget))
			::PostMessage(pThis->m_wndTarget, pThis->m_uiMsg, WPARAM(bResult), LPARAM(pThis->m_pvUserData));
	}

	_endthreadex(0);

	//	Not reached
	return 0;
}

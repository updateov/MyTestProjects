#include "StdAfx.h"
#include "InitializationWorkerThread.h"

IMPLEMENT_DYNCREATE(InitializationWorkerThread, CWinThread)
IMPLEMENT_DYNCREATE(ChecklistInitWorkerThread, InitializationWorkerThread)
IMPLEMENT_DYNCREATE(PatternsInitWorkerThread, InitializationWorkerThread)

InitializationWorkerThread::InitializationWorkerThread()
{
	m_pThreadWnd = NULL;
}


InitializationWorkerThread::~InitializationWorkerThread()
{
}


void InitializationWorkerThread::CreateThreadWnd(HWND controlWnd)
{
	
	ASSERT(m_pThreadWnd->m_hWnd == 0);
	
	
	PostThreadMessage(WORKER_THREAD_INIT_MSG, 0, (LPARAM)controlWnd);
	
	

}


BOOL InitializationWorkerThread::InitInstance()
{
	m_pMainWnd = m_pThreadWnd;
	MSG msg;
	BOOL result = PeekMessage(&msg, NULL, WM_USER, WM_USER, PM_NOREMOVE);
	m_startEvent.SetEvent();
	return TRUE;
}

int InitializationWorkerThread::ExitInstance()
{
	m_pThreadWnd->DestroyWindow();
	return CWinThread::ExitInstance();
}

BOOL InitializationWorkerThread::PreTranslateMessage(MSG *pMsg)
{
	switch (pMsg->message)
	{
	case WORKER_THREAD_INIT_MSG:
		DoInitThread(pMsg->wParam, pMsg->lParam);
		return TRUE;
	
	default:
		return FALSE;
	}
}

LONG InitializationWorkerThread::DoInitThread(WPARAM wParam, LPARAM lParam)
{
	ASSERT(m_pThreadWnd->m_hWnd == 0);

	m_pThreadWnd->Create();
	//m_startEvent.SetEvent();
	HWND wnd = (HWND)lParam;
	if (IsWindow(wnd))
	{
		
		::PostMessage(wnd, MSG_END_CREATE_THREAD_WND, 0, (LPARAM)m_pThreadWnd->m_hWnd);
	}
	return 0;
}


void ChecklistInitWorkerThread::SetInitialData(ChecklistInitialData* pInitialData)
{
	m_threadWnd.SendMessage(MSG_SET_INITIAL_DATA, 0, (LPARAM)pInitialData);
}

void PatternsInitWorkerThread::SetInitialData(PatternsInitialData* pInitialData)
{
	m_threadWnd.SendMessage(MSG_SET_INITIAL_DATA, 0, (LPARAM)pInitialData);
}
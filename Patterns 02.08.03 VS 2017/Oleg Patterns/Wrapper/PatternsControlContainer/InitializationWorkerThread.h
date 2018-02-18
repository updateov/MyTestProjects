#pragma once
#include "afxwin.h"
#include "afxmt.h"
#include "WorkerThreadWnd.h"

#define WORKER_THREAD_INIT_MSG (WM_USER + 1)

class InitializationWorkerThread :
	public CWinThread
{
	DECLARE_DYNCREATE(InitializationWorkerThread)
public:
	InitializationWorkerThread();
	virtual ~InitializationWorkerThread();	

public:
	BOOL PreTranslateMessage(MSG *pMsg);
	BOOL InitInstance();
	int ExitInstance();
public:
	void CreateThreadWnd(HWND controlWnd);

protected:
	afx_msg LONG DoInitThread(WPARAM wParam, LPARAM lParam);
	
protected:
	WorkerThreadWnd* m_pThreadWnd;
public:
	CEvent m_startEvent;
};

class ChecklistInitWorkerThread :
	public InitializationWorkerThread
{
	DECLARE_DYNCREATE(ChecklistInitWorkerThread)
public:
	ChecklistInitWorkerThread() : InitializationWorkerThread()
	{
		m_pThreadWnd = &m_threadWnd;
	}
	virtual ~ChecklistInitWorkerThread() {}

public:

	void SetInitialData(ChecklistInitialData* pInitialData);
	
protected:
	ChecklistWorkerThreadWnd m_threadWnd;
	
	
};

class PatternsInitWorkerThread :
	public InitializationWorkerThread
{
	DECLARE_DYNCREATE(PatternsInitWorkerThread)
public:
	PatternsInitWorkerThread():	InitializationWorkerThread()
	{
		m_pThreadWnd = &m_threadWnd;
	}
	virtual ~PatternsInitWorkerThread() {}

public:
	void SetInitialData(PatternsInitialData* pInitialData);

protected:
	PatternsWorkerThreadWnd m_threadWnd;


};
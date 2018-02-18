#pragma once

namespace rendevouz
{
#define DEREF(data) \
	rendevouz::CBaseThread *pThread = (rendevouz::CBaseThread *) data; \
	ASSERT(pThread); \
	ASSERT_KINDOF(CBaseThread, pThread);

class CBaseThread : public CObject
{
	DECLARE_DYNAMIC(CBaseThread);
public:
				CBaseThread(HANDLE hStopEvent, volatile bool *bStop, unsigned(__stdcall *thread)(void *), bool bWait = false, LPVOID data = NULL);
				~CBaseThread();

	bool		IsWaiting()	const		{ return m_bWaiting; }
	volatile bool Stop() const			{ return *m_bStop; }
	HANDLE		StopEvent() const		{ return m_hStopEvent; }
	HANDLE		ThreadHandle() const	{ return m_hThreadHandle; }
	LPVOID		UserData() const		{ return m_pvUserData; }

	virtual bool Wait(DWORD dwTimeout = INFINITE) const;
	bool		Run() const				{ return ResumeThread(m_hThreadHandle) == 1; }

	UINT		ThreadID() const		{ return m_uiThreadID; }

protected:
	LPVOID		m_pvUserData;
	HANDLE		m_hStopEvent,
				m_hThreadHandle;
	volatile bool *m_bStop,
				m_bWaiting;
	UINT		m_uiThreadID;
};

class CUserThread : public CBaseThread
{
	DECLARE_DYNAMIC(CUserThread);
public:
				CUserThread(unsigned(__stdcall *thread)(void *), bool bWait = false, LPVOID data = NULL);
	virtual		~CUserThread();

	void		TerminateThread();

private:
	volatile bool m_bStopVar;
};

class CSyncRendevouz : public CObject
{
	DECLARE_DYNAMIC(CSyncRendevouz);
public:
				CSyncRendevouz(void);
				~CSyncRendevouz(void);

	void		Stop()					{ m_bStop = TRUE; SetEvent(m_hStopEvent); }
	virtual bool Wait(DWORD dwTimeout = INFINITE);

	bool		AddThread(unsigned(__stdcall *thread)(void *), bool bWait = false, LPVOID data = NULL);
	bool		AddHandle(HANDLE hHandle);

protected:
	CArray<HANDLE, HANDLE>m_handleArray;
	CList<CBaseThread*, CBaseThread *> m_threads;
	HANDLE		m_hStopEvent;
	volatile bool m_bStop;
};

class CAsyncRendevouz : public CSyncRendevouz
{
	DECLARE_DYNAMIC(CAsyncRendevouz);
public:
				CAsyncRendevouz(HWND wndTarget, UINT uiMsg, LPVOID pvUserData = NULL);
				~CAsyncRendevouz();

	virtual bool Wait(DWORD dwTimeout = INFINITE);

private:
	static unsigned __stdcall WaitProc(LPVOID data);

	HWND		m_wndTarget;
	UINT		m_uiMsg;
	DWORD		m_dwTimeout;
	LPVOID		m_pvUserData;
	CBaseThread *m_pThread;
};
};
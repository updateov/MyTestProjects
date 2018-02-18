// main frame.cpp : implementation of the main_frame class
#include "stdafx.h"

#include "main frame.h"
#include "Patterns Application.h"
#include "patient list view.h"
#include "patient view.h"
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// main_frame
IMPLEMENT_DYNAMIC(main_frame, CFrameWnd)
BEGIN_MESSAGE_MAP(main_frame, CFrameWnd)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_WM_CLOSE()
	ON_MESSAGE(WM_POWERBROADCAST, OnPowerBroadcast)
	ON_MESSAGE(WM_ENDSESSION, OnEndSession)
	ON_MESSAGE(WM_QUERYENDSESSION, OnQueryEndSession)
END_MESSAGE_MAP()
static UINT indicators[] = { ID_SEPARATOR, // status line indicator
	ID_INDICATOR_CAPS, ID_INDICATOR_NUM, ID_INDICATOR_SCRL, };

//
// =======================================================================================================================
//    main_frame construction/destruction
// =======================================================================================================================
//
main_frame::main_frame(void)
{
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
main_frame::~main_frame(void)
{
}


LRESULT main_frame::WindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	if (message == WM_SYSCOMMAND && wParam == SC_MINIMIZE)
	{
		if (!get_main_view().get_patient_view().get_tracing().is_scrolling())
		{
			get_main_view().get_patient_view().get_tracing().stop_animating();
		}
	}
	return CFrameWnd::WindowProc(message, wParam, lParam);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
main_view &main_frame::get_main_view(void)
{
	return m_wndView;
}

/*
 =======================================================================================================================
    creation of the main view. The main view contains and managed all controls.
 =======================================================================================================================
 */
BOOL main_frame::OnCreateClient(LPCREATESTRUCT, CCreateContext *)
{
	m_wndView.Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS, CRect(0, 0, 0, 0), this, 0, NULL);
	m_wndView.initialize();

#ifdef OEM_patterns
	ModifyStyle (WS_CAPTION, 0);
	SetWindowPos (&wndTopMost, 0, 0, 0, 0, SWP_NOMOVE & SWP_NOSIZE & SWP_NOZORDER & SWP_DRAWFRAME);
#endif

	return true;
}

/*
 =======================================================================================================================
    limit the application window to 800x600 pixels.
 =======================================================================================================================
 */
void main_frame::OnGetMinMaxInfo(MINMAXINFO FAR *lpMMI)
{
#ifdef patterns_retrospective
	lpMMI->ptMinTrackSize.x = 1024;
	lpMMI->ptMinTrackSize.y = 700;
#elif defined(OEM_patterns)
	lpMMI->ptMinTrackSize.x = 400;
	lpMMI->ptMinTrackSize.y = 400;
#else
	lpMMI->ptMinTrackSize.x = 800;
	lpMMI->ptMinTrackSize.y = 600;
#endif
	CFrameWnd::OnGetMinMaxInfo(lpMMI);
}

/*
 =======================================================================================================================
    we adjust the main view size here.
 =======================================================================================================================
 */
void main_frame::OnSize(UINT t, int cx, int cy)
{
	CFrameWnd::OnSize(t, cx, cy);
	m_wndView.SetWindowPos(0, 0, 0, cx, cy, SWP_SHOWWINDOW | SWP_NOZORDER);
}

/*
 =======================================================================================================================
    remove client edge.
 =======================================================================================================================
 */
BOOL main_frame::PreCreateWindow(CREATESTRUCT &cs)
{
#ifdef OEM_patterns
	cs.style ^= WS_BORDER;
	cs.style ^= WS_CAPTION;
	cs.style ^= WS_MAXIMIZEBOX;
	cs.style ^= WS_MINIMIZEBOX;
	cs.style ^= WS_THICKFRAME;
	cs.style ^= WS_SYSMENU;
#endif

	if (!CFrameWnd::PreCreateWindow(cs))
	{
		return FALSE;
	}

	// remove the client edge.
	cs.dwExStyle ^= WS_EX_CLIENTEDGE;

	return TRUE;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
LRESULT main_frame::OnPowerBroadcast(WPARAM wParam, LPARAM lParam)
{
	switch (wParam)
	  {
		case PBT_APMSUSPEND:
		case PBT_APMSTANDBY:
		case PBT_APMRESUMEAUTOMATIC:
		case PBT_APMRESUMECRITICAL:
		case PBT_APMRESUMESUSPEND:
		case PBT_APMRESUMESTANDBY:
			RequestShutdown();
			break;
	  }

	return FALSE;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
LRESULT main_frame::OnEndSession(WPARAM wParam, LPARAM lParam)
{
	RequestShutdown();
	return TRUE;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
LRESULT main_frame::OnQueryEndSession(WPARAM wParam, LPARAM lParam)
{
	RequestShutdown();
	return TRUE;
}

void main_frame::StartPumping()
{
	MSG msg;
	PeekMessage(&msg, NULL, 0, 0, PM_NOREMOVE);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void main_frame::RequestShutdown(void)
{
	::PostQuitMessage(0);
}

//
// =======================================================================================================================
//    Intercept the close, do not close if not permitted
// =======================================================================================================================
//
void main_frame::OnClose(void)
{
	if (!m_bCloseDisabled)
	{
		PostQuitMessage(0);
	}
}

//
// =======================================================================================================================
//    Disable / Enable the user to close the application
// =======================================================================================================================
//
void main_frame::allow_application_to_close(bool bEnabled)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	CMenu *pSM = GetSystemMenu(FALSE);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (pSM)
	{
		pSM->EnableMenuItem(SC_CLOSE, bEnabled ? MF_BYCOMMAND : MF_BYCOMMAND | MF_GRAYED | MF_DISABLED);
	}

	m_bCloseDisabled = !bEnabled;
}

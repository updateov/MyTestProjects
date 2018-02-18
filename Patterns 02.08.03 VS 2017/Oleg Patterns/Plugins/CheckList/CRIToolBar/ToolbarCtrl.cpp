

#include "stdafx.h"
#include "ToolbarCtrl.h"
#include "NOperatingSystem.h"
#include "wnd.hpp"


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

#define TIMER_EV 100
#define CRI_TOOLBAR "CRI_Toolbar"
#define CRI_PROGRESS32 _T("msctls_progress32")

//const TCHAR* CToolbarCtrl::m_pstcoReBarWindow = L"ReBarWindow32";
//const TCHAR* CToolbarCtrl::m_pstcoTrayNotifyWnd = L"TrayNotifyWnd";
//const TCHAR* CToolbarCtrl::m_pstcoParent = L"Shell_TrayWnd";
const char* CToolbarCtrl::m_pstcoReBarWindow = "ReBarWindow32";
const char* CToolbarCtrl::m_pstcoTrayNotifyWnd = "TrayNotifyWnd";
const char* CToolbarCtrl::m_pstcoParent = "Shell_TrayWnd";

int CToolbarCtrl::m_stnWindowWidth = 120;
int CToolbarCtrl::m_stnWindowHeight = 36;

/////////////////////////////////////////////////////////////////////////////
// CToolbarCtrl

CToolbarCtrl::CToolbarCtrl(CWnd* pcoParent)
{
	m_bOk = true;
	m_nMode = NOTDEFINED;
	m_nLeft = 0;
	m_bSpezialColored = false;
	m_bHided = false;
	m_pcoParentWindow = pcoParent;
	m_bWindowSet = false;
	m_bSet = false;
	m_bTaskbarSizeChanged = false;

	m_ReBarWindowCurrentRect.left = m_ReBarWindowCurrentRect.right = m_ReBarWindowCurrentRect.top = m_ReBarWindowCurrentRect.bottom;

	m_pcoParent = new Wnd((char*)m_pstcoParent, NULL);
	m_pcoReBarWindow = new Wnd((char*)m_pstcoReBarWindow, m_pcoParent->GetHWnd());
	m_pcoTrayNotifyWnd = new Wnd((char*)m_pstcoTrayNotifyWnd, m_pcoParent->GetHWnd());

	CNOperatingSystem *pcoOS = new CNOperatingSystem();
	if (pcoOS->IsWindowsWin7SP1OrGreater() == FALSE)
	{
		m_bOk = false;
	}
	delete pcoOS;


	if (!(m_pcoReBarWindow->GetHWnd()))
	{
		::MessageBox(NULL,
			m_pstcoReBarWindow,
			"Can't find...",
			MB_OK);
		m_bOk = false;
	}

	if (!(m_pcoReBarWindow->GetHWnd()))
	{
		::MessageBox(NULL,
			m_pstcoTrayNotifyWnd,
			"Can't find...",
			MB_OK);
		m_bOk = false;
	}

	//char* pcClassName = "msctls_progress32";
	//Wnd* m_pcoReBarWindow = new Wnd("msctls_progress32", m_pcoParent->GetHWnd());
	LPTSTR pcClassName = _T("msctls_progress32");
	Wnd* m_pcoReBarWindow = new Wnd(pcClassName, m_pcoParent->GetHWnd());

	if (m_pcoReBarWindow->GetHWnd())
	{
		::MessageBox(NULL,
			"I can place just one Progressbar in the Taskbar! Progressbar not created!",
			"Control already exists",
			MB_OK | MB_ICONWARNING);
		m_bOk = false;
	}
	delete m_pcoReBarWindow;

	if (IsControlSuccessfullyCreated())
	{
		this->StartUp();
		this->SetBkColour(RGB(255, 255, 255));
		this->SetForeColour(RGB(0, 255, 0));
		this->SetTextBkColour(RGB(0, 0, 0));
		this->SetTextForeColour(RGB(255, 255, 255));
		//this->SetRange32(0, 100);
	}
}

CToolbarCtrl::~CToolbarCtrl()
{
	try
	{
		if (IsControlSuccessfullyCreated())
		{
			this->ReModifyTaskbar();
		}
		delete m_pcoReBarWindow;
		delete m_pcoTrayNotifyWnd;
		delete m_pcoParent;
	}
	catch (...)
	{
		TRACE0("ERROR ~CToolbarCtrl\n");
	}
}


BEGIN_MESSAGE_MAP(CToolbarCtrl, CToolbarBaseCtrl)
	//{{AFX_MSG_MAP(CToolbarCtrl)
	ON_WM_TIMER()
	ON_WM_RBUTTONDOWN()
	ON_WM_LBUTTONDBLCLK()
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// Behandlungsroutinen für Nachrichten CToolbarCtrl 

void CToolbarCtrl::StartUp()
{
	this->ModifyTaskbar();
	this->PutWindowIntoTaskbar(m_pcoReBarWindow->GetProportion());
	this->SetTimer(TIMER_EV, 1000, 0);
}


bool CToolbarCtrl::IsControlSuccessfullyCreated()
{
	return m_bOk;
}

void CToolbarCtrl::PutWindowIntoTaskbar(int nDirection)
{
	RECT coRectReBar;
	CRect coRect;

	coRectReBar = m_pcoReBarWindow->GetRect();

	int nDiff = 0;
	int nRightSmaller = 0;
	int nCenter = 0;

	if (m_pcoReBarWindow->GetProportion() == HORIZONTAL)
	{
		m_nHeightTskControl = m_pcoReBarWindow->GetRect().bottom - m_pcoReBarWindow->GetRect().top;
		if (m_nHeightTskControl > m_stnWindowHeight)
		{
			nCenter = (m_nHeightTskControl - m_stnWindowHeight) / 2;
			m_nHeightTskControl = m_stnWindowHeight;
		}

		//nDiff = m_nHeightTskControl / 10;

		coRect.SetRect(coRectReBar.right,
			0 + nDiff + nCenter,
			coRectReBar.right + m_stnWindowWidth - nRightSmaller,
			m_nHeightTskControl - nDiff + nCenter);

		if (m_bCreated != true)
		{
			this->Create(PBS_SMOOTH | WS_CHILD | WS_VISIBLE, coRect, CWnd::FromHandle(m_pcoParent->GetHWnd()), NULL);
			m_bCreated = true;
		}
		else
		{
			this->MoveWindow(coRect.left,
				coRect.top,
				m_stnWindowWidth - nRightSmaller,
				coRect.bottom - coRect.top,
				TRUE);

			this->RedrawWindow(NULL, NULL, RDW_UPDATENOW | RDW_FRAME | RDW_INVALIDATE);
		}
	}
	else if (m_pcoReBarWindow->GetProportion() == VERTICAL)
	{
		coRect.SetRect(0 + nDiff,
			coRectReBar.bottom,
			coRectReBar.right - coRectReBar.left - nDiff,
			coRectReBar.bottom + m_stnWindowHeight - nRightSmaller);

		if (m_bCreated != true)
		{
			this->Create(PBS_SMOOTH | WS_CHILD | WS_VISIBLE, coRect, CWnd::FromHandle(m_pcoParent->GetHWnd()), NULL);
			m_bCreated = true;
		}
		else
		{
			this->MoveWindow(coRect.left,
				coRect.top,
				coRect.right - coRect.left,
				m_stnWindowHeight - nRightSmaller,
				TRUE);

			this->RedrawWindow(NULL, NULL, RDW_UPDATENOW | RDW_FRAME | RDW_INVALIDATE);
		}
	}
}

void CToolbarCtrl::ModifyTaskbar()
{
	RECT coReBarWindowRect = m_pcoReBarWindow->GetRect();
	RECT coTrayNotifyWndRect = m_pcoTrayNotifyWnd->GetRect();

	if (m_pcoReBarWindow->GetProportion() == HORIZONTAL)
	{
		m_ReBarWindowCurrentRect.left = coReBarWindowRect.left + m_pcoParent->GetRect().left;
		m_ReBarWindowCurrentRect.right = coTrayNotifyWndRect.left - coReBarWindowRect.left - m_stnWindowWidth + m_pcoParent->GetRect().left;
		m_ReBarWindowCurrentRect.bottom = coReBarWindowRect.bottom - coReBarWindowRect.top;
		m_ReBarWindowCurrentRect.top = 0;

		m_nMode = HORIZONTAL;
	}
	else if (m_pcoReBarWindow->GetProportion() == VERTICAL)
	{
		m_ReBarWindowCurrentRect.left = 0;
		m_ReBarWindowCurrentRect.right = coTrayNotifyWndRect.right - coTrayNotifyWndRect.left;
		m_ReBarWindowCurrentRect.top = coReBarWindowRect.top + m_pcoParent->GetRect().top;
		m_ReBarWindowCurrentRect.bottom = coTrayNotifyWndRect.top - m_stnWindowHeight - coReBarWindowRect.top + m_pcoParent->GetRect().top;

		m_nMode = VERTICAL;
	}

	::MoveWindow(m_pcoReBarWindow->GetHWnd(),
		m_ReBarWindowCurrentRect.left,
		m_ReBarWindowCurrentRect.top,
		m_ReBarWindowCurrentRect.right,
		m_ReBarWindowCurrentRect.bottom,
		TRUE);
}

void CToolbarCtrl::ReModifyTaskbar()
{
	RECT coReBarWindowRect = m_pcoReBarWindow->GetRect();
	RECT coTrayNotifyWndRect = m_pcoTrayNotifyWnd->GetRect();

	if (m_nMode == HORIZONTAL)
	{
		m_ReBarWindowCurrentRect.left = coReBarWindowRect.left + m_pcoParent->GetRect().left;
		m_ReBarWindowCurrentRect.right = m_pcoTrayNotifyWnd->GetRect().left - coReBarWindowRect.left;
		m_ReBarWindowCurrentRect.top = 0;
		m_ReBarWindowCurrentRect.bottom = coReBarWindowRect.bottom - coReBarWindowRect.top;
	}
	else if (m_nMode == VERTICAL)
	{
		m_ReBarWindowCurrentRect.left = 0;
		m_ReBarWindowCurrentRect.right = coTrayNotifyWndRect.right - coTrayNotifyWndRect.left;
		m_ReBarWindowCurrentRect.top = coReBarWindowRect.top + m_pcoParent->GetRect().top;
		m_ReBarWindowCurrentRect.bottom = coTrayNotifyWndRect.top - coReBarWindowRect.top;
	}

	::MoveWindow(m_pcoReBarWindow->GetHWnd(),
		m_ReBarWindowCurrentRect.left,
		m_ReBarWindowCurrentRect.top,
		m_ReBarWindowCurrentRect.right,
		m_ReBarWindowCurrentRect.bottom,
		TRUE);
}

void CToolbarCtrl::OnTimer(UINT nIDEvent)
{
	switch (nIDEvent)
	{
	case TIMER_EV:
		if (!m_bHided)
		{
			if (m_pcoParent->GetProportion() == HORIZONTAL)
			{
				if (((m_pcoTrayNotifyWnd->GetRect().left - m_pcoReBarWindow->GetRect().right) < m_stnWindowWidth) ||
					(m_pcoTrayNotifyWnd->IsRectChanged() ||
					m_pcoParent->IsRectChanged())
					)
				{
					this->Refresh();
					this->PutWindowIntoTaskbar(m_pcoReBarWindow->GetProportion());
				}
				else
				{
					this->RedrawWindow();
				}
			}
			else
			{
				if (((m_pcoTrayNotifyWnd->GetRect().top - m_pcoReBarWindow->GetRect().bottom) < m_stnWindowHeight) ||
					(m_pcoTrayNotifyWnd->IsRectChanged() ||
					m_pcoParent->IsRectChanged())
					)
				{
					this->Refresh();
					this->PutWindowIntoTaskbar(m_pcoReBarWindow->GetProportion());
				}
				else
				{
					this->RedrawWindow();
				}
			}
		}
		break;
	};
	//this->Invalidate();

	CToolbarBaseCtrl::OnTimer(nIDEvent);
}


void CToolbarCtrl::Refresh()
{
	this->ModifyTaskbar();
	this->RedrawWindow();
}

void CToolbarCtrl::SetPosEx(int nPos)
{
	int r, g;

	if (IsControlSuccessfullyCreated())
	{
		if (m_bSpezialColored)
		{
			if (nPos<0) nPos = 0;
			if (nPos>255) nPos = 255;
			r = (int)((100 - nPos)*5.1);
			g = (int)(nPos*5.1);
			if (g>255) g = 255;
			if (r>255) r = 255;
			if (g<0) g = 0;
			if (r<0) r = 0;
			this->SetForeColour(RGB(r, g, 0));
		}
		else
		{
			this->SetBkColour(RGB(0, 0, 0));
			this->SetForeColour(RGB(0, 255, 0));
			this->SetTextBkColour(RGB(0, 0, 0));
			this->SetTextForeColour(RGB(255, 255, 255));
		}

		this->SetPos(nPos);
		this->RedrawWindow();
	}
}


void CToolbarCtrl::Show(bool bShow)
{
	if (!bShow)
	{
		this->ShowWindow(false);
		KillTimer(TIMER_EV);
		ReModifyTaskbar();
		m_bHided = true;
	}
	else
	{
		this->ShowWindow(true);
		this->ModifyTaskbar();
		this->PutWindowIntoTaskbar(m_pcoReBarWindow->GetProportion());
		this->SetTimer(TIMER_EV, 1000, 0);
		m_bHided = false;
	}
}

bool CToolbarCtrl::IsHided()
{
	return m_bHided;
}

void CToolbarCtrl::SetColorMode(bool bColorMode)
{
	m_bSpezialColored = bColorMode;
	this->RedrawWindow();
}

void CToolbarCtrl::OnRButtonDown(UINT nFlags, CPoint point)
{
	//((CDialog*)m_pcoParentWindow)->OnCmdMsg(WM_RBUTTONDOWN, 0, NULL, NULL);

	//::SendMessage(m_pcoReBarWindow->GetHWnd(), WM_LBUTTONDBLCLK, NULL, NULL);
}

void CToolbarCtrl::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	//((CDialog*)m_pcoParentWindow)->OnCmdMsg(WM_LBUTTONDBLCLK, 0, NULL, NULL);
}

void CToolbarCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{
	((CDialog*)m_pcoParentWindow)->OnCmdMsg(WM_LBUTTONDOWN, 0, NULL, NULL);
}

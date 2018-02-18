
// CRI_ToolBarDlg.cpp : implementation file
//

#include "stdafx.h"
#include "CRI_ToolBar.h"
#include "CRI_ToolBarDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif



// CRI_ToolBarDlg dialog


CRI_ToolBarDlg::CRI_ToolBarDlg(CWnd* pParent /*=NULL*/)
: CDialog(CRI_ToolBarDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDI_APP_ICON);
	m_hCALM_Agent = NULL;
	m_pcoControl = new CToolbarCtrl(this);
}

CRI_ToolBarDlg::~CRI_ToolBarDlg()
{
	if (m_pcoControl != NULL)
	{
		delete m_pcoControl;
	}
}

void CRI_ToolBarDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CRI_ToolBarDlg, CDialog)
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_WM_APPCOMMAND()
	ON_WM_CLOSE()
	ON_MESSAGE(OWM_AGENT_MSG, OnAgentMessage)
END_MESSAGE_MAP()


// CRI_ToolBarDlg message handlers

BOOL CRI_ToolBarDlg::OnInitDialog()
{
	CDialog::OnInitDialog();
	CRect rect(0, 0, 0, 0);
	MoveWindow(rect);

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	//SetIcon(m_hIcon, TRUE);			// Set big icon
	//SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here

	LPTSTR CommandLineDetails = AfxGetApp()->m_lpCmdLine;

	/*if (_tcslen(CommandLineDetails) > 0)
	{
		m_hCALM_Agent = (HWND)_wtol(CommandLineDetails);
	}*/

	if (strlen(CommandLineDetails) > 0)
	{
		m_hCALM_Agent = (HWND)atol(CommandLineDetails);
	}

	m_pcoControl->SetPosEx(100);
	m_pcoControl->SetColorMode(true);

	m_pcoControl->SetCRIStatus(Off);
	m_pcoControl->DrawBitmap();

	return TRUE;  // return TRUE  unless you set the focus to a control
}


LRESULT CRI_ToolBarDlg::OnAgentMessage(WPARAM wParam, LPARAM lParam)
{
	CRIState nStatus = (CRIState)wParam;

	m_pcoControl->SetCRIStatus(nStatus);
	m_pcoControl->DrawBitmap();

	return 0;
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CRI_ToolBarDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

void CRI_ToolBarDlg::OnAppCommand(CWnd* pWnd, UINT nCmd, UINT nDevice, UINT nKey)
{
	CDialog::OnAppCommand(pWnd, nCmd, nDevice, nKey);
}


BOOL CRI_ToolBarDlg::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	if (m_hCALM_Agent != NULL)
	{
		if (nID == WM_LBUTTONDBLCLK ||
			nID == WM_LBUTTONDOWN ||
			nID == WM_RBUTTONDOWN)
		{
			::SendMessage(m_hCALM_Agent, nID, 0, 0);
		}
	}

	return CDialog::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}


void CRI_ToolBarDlg::OnClose()
{
	// TODO: Add your message handler code here and/or call default

	if (m_pcoControl != NULL)
		delete m_pcoControl;

	CDialog::OnClose();
}

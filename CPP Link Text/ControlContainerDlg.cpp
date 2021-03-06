// ControlContainerDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ControlContainer.h"
#include "ControlContainerDlg.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CControlContainerDlg dialog

CControlContainerDlg::CControlContainerDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CControlContainerDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CControlContainerDlg)
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CControlContainerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CControlContainerDlg)
	DDX_Control(pDX, IDC_STATIC3, m_Static3);
	DDX_Control(pDX, IDC_STATIC2, m_Static2);
	DDX_Control(pDX, IDC_LINK, m_Static1);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CControlContainerDlg, CDialog)
	//{{AFX_MSG_MAP(CControlContainerDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()

	//Message Trap - to trap message from the CMyHyperlink control
	//ON_MESSAGE(_HYPERLINK_EVENT,OnChildFire)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CControlContainerDlg message handlers

BOOL CControlContainerDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	


	////////////////////////SET THE LINKS HERE //////////////////////////


	//Set the target URL 
	m_Static1.SetLinkUrl("www.codeproject.com");
	//Enable showing the Tooltip
	//m_Static1.ActiveToolTip(1);
	////Set the Tooltiptext
	//m_Static1.SetTootTipText("Click Here to go Codeproject");
	////Set the tooltip Background olor
	//m_Static1.SetToolTipBgColor(RGB(0, 0, 0));
	////Set the Tooltip Text Color
	//m_Static1.SetToolTipTextColor(RGB(0, 255, 0));


	// Change the default Linktext,HoverText,Visited Colors 
	// if you dont specify the colors the control will use the 
	//defaults..

	m_Static1.SetLinkColor(RGB(255, 0, 0));
	m_Static1.SetHoverColor(RGB(0, 0, 255));
	m_Static1.SetVisitedColor(RGB(0, 13, 0));


	m_Static2.SetLinkUrl("mailto:renjith_sree@hotmail.com");
	//m_Static2.ActiveToolTip(1);
	//m_Static2.SetTootTipText("Click here to Email Me..");


	m_Static3.SetFireChild(1);
	//m_Static3.ActiveToolTip(1);
	//m_Static3.SetTootTipText("Click Here to Fire An event to parent");


	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CControlContainerDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CControlContainerDlg::OnPaint() 
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CControlContainerDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}


///////// Event From the child catch here//////////////////////
///////// WAPARAM conatians the ID of the control from which the event 
//generates

//void CControlContainerDlg::OnChildFire(WPARAM wparam, LPARAM lparam)
//{
//	CString csTmp;
//	csTmp.Format(" Child Fire Me ====>>with resource ID(#%d)",(int)wparam);
//
//	MessageBox(csTmp.GetBuffer(MAX_PATH));
//	csTmp.ReleaseBuffer();
//
//}

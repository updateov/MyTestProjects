
// Perigen.Patterns.NnetControls.TestDlg.cpp : implementation file
//

#include "stdafx.h"
#include "Perigen.Patterns.NnetControls.Test.h"
#include "NativeManager.h"
#include "Perigen.Patterns.NnetControls.TestDlg.h"
#include "afxdialogex.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CPerigenPatternsNnetControlsTestDlg dialog



CPerigenPatternsNnetControlsTestDlg::CPerigenPatternsNnetControlsTestDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CPerigenPatternsNnetControlsTestDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CPerigenPatternsNnetControlsTestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CPerigenPatternsNnetControlsTestDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_CREATE()
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_BUTTON1, &CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton1)
	ON_BN_CLICKED(IDC_BUTTON2, &CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton2)
	ON_BN_CLICKED(IDC_BUTTON3, &CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton3)
	ON_BN_CLICKED(IDC_BUTTON4, &CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton4)
	ON_BN_CLICKED(IDC_BUTTON5, &CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton5)
END_MESSAGE_MAP()


// CPerigenPatternsNnetControlsTestDlg message handlers

BOOL CPerigenPatternsNnetControlsTestDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	HRESULT hr = CoInitialize(NULL);

	/*RECT rect;
	this->GetWindowRect(&rect);
	HWND hParent = this->GetSafeHwnd();

	m_pPatternsAdapter = new NativeManager();
	m_pPatternsAdapter->InitNavigationPanel((long)hParent, 5, 145, rect.right - rect.left - 25, 32);

	m_pPatternsAdapter->InitExportButton((long)hParent, 155, 176, 100, 25);
	m_pPatternsAdapter->SetTimeRange(15);*/
	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CPerigenPatternsNnetControlsTestDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

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
		CDialogEx::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CPerigenPatternsNnetControlsTestDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}



int CPerigenPatternsNnetControlsTestDlg::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CDialogEx::OnCreate(lpCreateStruct) == -1)
		return -1;

	//RECT rect;
	//this->GetWindowRect(&rect);
	//HWND hParent = this->GetSafeHwnd();

	//m_pPatternsAdapter->InitNavigationPanel((long)hParent, 0, rect.bottom - 100, rect.right - rect.left - 20, 31, &retVal);
	//m_pPatternsAdapter = new NativeManager();
	//m_pPatternsAdapter->InitNavigationPanel((long)hParent, 5, 145, rect.right - rect.left - 25, 31);

	return 0;
}


void CPerigenPatternsNnetControlsTestDlg::OnSize(UINT nType, int cx, int cy)
{
	CDialogEx::OnSize(nType, cx, cy);

	RECT rect;
	this->GetWindowRect(&rect);

	m_pPatternsAdapter->ResizeNavigationPanel(5, 145, rect.right - rect.left - 25, 32);
}


void CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton1()
{
	time_t rawtime;
    struct tm * timeinfo;

    time ( &rawtime );
    timeinfo = localtime ( &rawtime );

	m_pPatternsAdapter->SetExportableChunks(NULL);
}


void CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton2()
{
	m_pPatternsAdapter->HideControls();
}


void CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton3()
{
	RECT rect;
	this->GetWindowRect(&rect);

	m_pPatternsAdapter->ResizeNavigationPanel( 5, 145, rect.right - rect.left - 25, 32);
	m_pPatternsAdapter->ResizeExportButton(155, 176, 100, 25);
}


void CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton4()
{
	// TODO: Add your control notification handler code here
	RECT rect;
	this->GetWindowRect(&rect);
	HWND hParent = this->GetSafeHwnd();

	m_pPatternsAdapter = new NativeManager();
	m_pPatternsAdapter->InitNavigationPanel((long)hParent, 5, 145, rect.right - rect.left - 25, 32);

	m_pPatternsAdapter->InitExportButton((long)hParent, 155, 176, 100, 25);
	m_pPatternsAdapter->SetTimeRange(15);
}


void CPerigenPatternsNnetControlsTestDlg::OnBnClickedButton5()
{
	if(m_pPatternsAdapter)
	{
		delete m_pPatternsAdapter;
	}
}

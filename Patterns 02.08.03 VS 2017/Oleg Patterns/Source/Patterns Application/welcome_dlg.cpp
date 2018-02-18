// welcome_dlg.cpp : implementation file
#include "stdafx.h"
#include "welcome_dlg.h"

// =====================================================================================================================
//    welcome_dlg dialog
// =====================================================================================================================
welcome_dlg::welcome_dlg(CWnd* pParent /* NULL */ ) :
	CDialog(welcome_dlg::IDD, pParent),
	sampleChecked(1)
{
	VERIFY(m_HollowBrush.CreateStockObject(HOLLOW_BRUSH));
	m_bmpBackground.LoadResource(IDB_WELCOME);
}

welcome_dlg::~welcome_dlg()
{
}

void welcome_dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Check(pDX, IDC_CHECK_SAMPLES, sampleChecked);
}

BOOL welcome_dlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	CString s_p = "Version ";
	s_p = s_p + FILEVERSTR;
	s_p = s_p + " Build ";
	s_p = s_p + FILEDESCRSTR;

	
	GetDlgItem(IDC_VERSION)->SetWindowText(s_p);

	sampleChecked = 1;

	HKEY key;
	if (::RegOpenKey(HKEY_CURRENT_USER, "Software\\PeriGen\\Retrospective PeriCALM Patterns", &key) == ERROR_SUCCESS)
	{
		DWORD value = 0;
		DWORD type = REG_DWORD;
		DWORD len = sizeof(value);

		if (::RegQueryValueEx(key, "Samples", 0, &type, (LPBYTE) & value, &len) == ERROR_SUCCESS)
		{
			sampleChecked = (value != 0) ? 1 : 0;
		}

		::RegCloseKey(key);
	}

	UpdateData(FALSE);

	CenterWindow();
	ShowWindow(SW_SHOW);

	return TRUE;	// return TRUE unless you set the focus to a control
}

BEGIN_MESSAGE_MAP(welcome_dlg, CDialog)
	ON_WM_ERASEBKGND()
	ON_WM_CTLCOLOR()
	ON_WM_QUERYNEWPALETTE()
	ON_WM_PALETTECHANGED()
	ON_NOTIFY(NM_CLICK, IDC_SL_OPENFILE, &welcome_dlg::OnNMClickSlOpenfile)
	ON_NOTIFY(NM_CLICK, IDC_SL_TUTORIAL, &welcome_dlg::OnNMClickSlTutorial)
	ON_NOTIFY(NM_CLICK, IDC_SL_HELP, &welcome_dlg::OnNMClickSlHelp)
	ON_NOTIFY(NM_CLICK, IDC_SL_WEBSITE, &welcome_dlg::OnNMClickSlWebsite)
	ON_BN_CLICKED(IDOK, &welcome_dlg::OnBnClickedOk)
	ON_BN_CLICKED(IDC_CHECK_SAMPLES, &welcome_dlg::OnBnClickedCheckSamples)
	ON_NOTIFY(NM_CLICK, IDC_SL_AUTOUPDATE, &welcome_dlg::OnNMClickSlAutoupdate)
END_MESSAGE_MAP()

void welcome_dlg::OnNMClickSlOpenfile(NMHDR* pNMHDR, LRESULT* pResult)
{
	EndDialog(IDC_SL_OPENFILE);
}

void welcome_dlg::OnNMClickSlTutorial(NMHDR* pNMHDR, LRESULT* pResult)
{
	SHELLEXECUTEINFO sei;
	::ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
	sei.cbSize = sizeof(SHELLEXECUTEINFO);				// Set Size
	sei.lpVerb = TEXT("open");							// Set Verb
	sei.lpFile = "RetrospectivePatternsQuickStart.pdf"; // Set Target To Open
	sei.nShow = SW_SHOWNORMAL;					// Show Normal
	ShellExecuteEx(&sei);
}

void welcome_dlg::OnNMClickSlHelp(NMHDR* pNMHDR, LRESULT* pResult)
{
	SendMessage(WM_COMMAND, ID_HELP);
}

void welcome_dlg::OnNMClickSlWebsite(NMHDR* pNMHDR, LRESULT* pResult)
{
	SHELLEXECUTEINFO sei;

	::ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));
	sei.cbSize = sizeof(SHELLEXECUTEINFO);		// Set Size
	sei.lpVerb = TEXT("open");					// Set Verb
	sei.lpFile = "http://www.perigen.com/";		// Set Target To Open
	sei.nShow = SW_SHOWNORMAL;					// Show Normal
	ShellExecuteEx(&sei);
}

void welcome_dlg::OnBnClickedOk()
{
	UpdateData(TRUE);
	OnOK();
}

void welcome_dlg::OnBnClickedCheckSamples()
{
	UpdateData(TRUE);

	HKEY key;
	DWORD disp;
	::RegCreateKeyEx(HKEY_CURRENT_USER, "Software\\PeriGen\\Retrospective PeriCALM Patterns", 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &key, &disp);

	DWORD value = sampleChecked != 0 ? 1 : 0;
	DWORD type = REG_DWORD;
	DWORD len = sizeof(value);
	::RegSetValueEx(key, "Samples", 0, type, (CONST BYTE *) &value, len);

	::RegCloseKey(key);
}

BOOL welcome_dlg::OnEraseBkgnd(CDC* pDC)
{
	CDialog::OnEraseBkgnd(pDC);

	CRect rc;
	GetClientRect(rc);

	int x = 0, y = 0;

	// stretch bitmap so it will best fit to the dialog
	CDialog::OnEraseBkgnd(pDC);

	CRect rc2 = rc;
	rc2.bottom = rc.bottom - 50;
	pDC->FillRect(&rc2, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	m_bmpBackground.DrawDIB(pDC, 0, 0, m_bmpBackground.GetWidth(), m_bmpBackground.GetHeight());

	return TRUE;
}

HBRUSH welcome_dlg::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	switch (nCtlColor)
	{
		case CTLCOLOR_STATIC:
			TCHAR lpszClassName[255];
			GetClassName(pWnd->m_hWnd, lpszClassName, 255);
			if (_tcscmp(lpszClassName, TRACKBAR_CLASS) == 0)
			{
				return CDialog::OnCtlColor(pDC, pWnd, nCtlColor);
			}

		case CTLCOLOR_BTN:
			if (pWnd->GetDlgCtrlID() != 1077)	// The checkbox should stay 'normal'
			{
				// let static controls shine through
				pDC->SetBkMode(TRANSPARENT);
				return HBRUSH(m_HollowBrush);
			}
			break;
	}

	// if we reach this line, we haven't set a brush so far
	return CDialog::OnCtlColor(pDC, pWnd, nCtlColor);
}

BOOL welcome_dlg::OnQueryNewPalette()
{
	CPalette*  pPal = m_bmpBackground.GetPalette();
	if (pPal != 0 && GetSafeHwnd() != 0)
	{
		CClientDC dc(this);
		CPalette*  pOldPalette = dc.SelectPalette(pPal, FALSE);
		UINT nChanged = dc.RealizePalette();
		dc.SelectPalette(pOldPalette, TRUE);

		if (nChanged == 0)
		{
			return FALSE;
		}

		Invalidate();
		return TRUE;
	}

	return CDialog::OnQueryNewPalette();
}

void welcome_dlg::OnPaletteChanged(CWnd* pFocusWnd)
{
	CPalette*  pPal = m_bmpBackground.GetPalette();
	if (pPal != 0 && GetSafeHwnd() != 0 && pFocusWnd != this && !IsChild(pFocusWnd))
	{
		CClientDC dc(this);
		CPalette*  pOldPalette = dc.SelectPalette(pPal, TRUE);
		UINT nChanged = dc.RealizePalette();
		dc.SelectPalette(pOldPalette, TRUE);

		if (nChanged)
		{
			Invalidate();
		}
	}
	else
	{
		CDialog::OnPaletteChanged(pFocusWnd);
	}
}

void welcome_dlg::OnNMClickSlAutoupdate(NMHDR* pNMHDR, LRESULT* pResult)
{
	EndDialog(IDC_SL_AUTOUPDATE);
}

// no_license_dlg.cpp : implementation file
//

#include "stdafx.h"
#include "no_license_dlg.h"


// no_license_dlg dialog

IMPLEMENT_DYNAMIC(no_license_dlg, CDialog)

no_license_dlg::no_license_dlg(license_validation* pLicense, CWnd* pParent /*=NULL*/)
	: CDialog(no_license_dlg::IDD, pParent)
	, m_pLicense(pLicense)
{
}

no_license_dlg::~no_license_dlg()
{
}

BOOL no_license_dlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	HICON hIcon;

	hIcon = ::LoadIcon(NULL, MAKEINTRESOURCE(IDI_WARNING));
	m_WarningIcon.SetIcon(hIcon);

	GetDlgItem(IDC_LICENSE_TEXT)->SetWindowText(m_pLicense->get_license_description());
	GetDlgItem(IDC_BUTTON_ENTER_CODE)->EnableWindow(m_pLicense->is_license_tampered()?0:1);
	UpdateData(FALSE);

	CenterWindow();
	ShowWindow(SW_SHOW);

	return TRUE;
}

void no_license_dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_ICONBOX, m_WarningIcon);
}

BEGIN_MESSAGE_MAP(no_license_dlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_REQUEST_CODE, &no_license_dlg::OnBnClickedButtonRequestCode)
	ON_BN_CLICKED(IDC_BUTTON_ENTER_CODE, &no_license_dlg::OnBnClickedButtonEnterCode)
	ON_BN_CLICKED(IDCANCEL, &no_license_dlg::OnBnClickedCancel)
END_MESSAGE_MAP()


// no_license_dlg message handlers

void no_license_dlg::OnBnClickedButtonRequestCode()
{
	EndDialog(IDC_BUTTON_REQUEST_CODE);
}

void no_license_dlg::OnBnClickedButtonEnterCode()
{
	EndDialog(IDC_BUTTON_ENTER_CODE);
}

void no_license_dlg::OnBnClickedCancel()
{
	OnCancel();
}

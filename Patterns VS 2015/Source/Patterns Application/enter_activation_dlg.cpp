// enter_activation_dlg.cpp : implementation file
//

#include "stdafx.h"
#include "enter_activation_dlg.h"

// enter_activation_dlg dialog

IMPLEMENT_DYNAMIC(enter_activation_dlg, CDialog)

enter_activation_dlg::enter_activation_dlg(license_validation* pLicense, CWnd* pParent /*=NULL*/)
: CDialog(enter_activation_dlg::IDD, pParent)
, m_pLicense(pLicense)
, m_editkey(_T(""))
{
}

enter_activation_dlg::~enter_activation_dlg()
{
}

BOOL enter_activation_dlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	UpdateData(FALSE);

	CenterWindow();
	ShowWindow(SW_SHOW);

	return TRUE;
}

void enter_activation_dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_KEY, m_editkey);
}


BEGIN_MESSAGE_MAP(enter_activation_dlg, CDialog)
	ON_BN_CLICKED(IDOK, &enter_activation_dlg::OnBnClickedOk)
END_MESSAGE_MAP()


// enter_activation_dlg message handlers

void enter_activation_dlg::OnBnClickedOk()
{
	UpdateData(TRUE);

	CString message;
	if ((m_pLicense->update_key(m_editkey.GetBuffer(m_editkey.GetLength()), message))
		&& (m_pLicense->is_license_valid()))
	{
		MessageBox("Thank you for registering PeriCALM® Patterns Retrospective™.\r\nLicense: " + m_pLicense->get_license_description(), "Registration Succeeded", MB_OK);
		OnOK();
	}
	else
	{
		MessageBox("Your activation code is not valid or already expired.\r\nPlease contact patterns@perigen.com", "Error", MB_OK|MB_ICONERROR);
	}
}
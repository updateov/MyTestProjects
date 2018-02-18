// disclaimer_dlg.cpp : implementation file
//

#include "stdafx.h"
#include "disclaimer_dlg.h"


// disclaimer_dlg dialog

IMPLEMENT_DYNAMIC(disclaimer_dlg, CDialog)

disclaimer_dlg::disclaimer_dlg(CWnd* pParent /*=NULL*/)
	: CDialog(disclaimer_dlg::IDD, pParent)
	, m_radio_agree(FALSE)
{

}

disclaimer_dlg::~disclaimer_dlg()
{
}

void disclaimer_dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Radio(pDX, IDC_RADIO_DISAGREE, m_radio_agree);
}

BOOL disclaimer_dlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	CString s;
	s.LoadString(NULL, retrospective_disclaimer);
	GetDlgItem(IDC_EDIT_DISCLAIMER)->SetWindowText(s);

	m_radio_agree = FALSE;
	GetDlgItem(IDOK)->EnableWindow(m_radio_agree);

	UpdateData(TRUE);

	CenterWindow();
	ShowWindow(SW_SHOW);

	return TRUE;
}

BEGIN_MESSAGE_MAP(disclaimer_dlg, CDialog)
	ON_BN_CLICKED(IDC_RADIO_DISAGREE, &disclaimer_dlg::OnBnClickedRadioDisagree)
	ON_BN_CLICKED(IDC_RADIO_AGREE, &disclaimer_dlg::OnBnClickedRadioAgree)
END_MESSAGE_MAP()

void disclaimer_dlg::OnBnClickedRadioDisagree()
{
	UpdateData(TRUE);
	GetDlgItem(IDOK)->EnableWindow(m_radio_agree);
}

void disclaimer_dlg::OnBnClickedRadioAgree()
{
	UpdateData(TRUE);
	GetDlgItem(IDOK)->EnableWindow(m_radio_agree);
}

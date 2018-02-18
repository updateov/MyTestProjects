// request_license_dialog.cpp : implementation file
#include "stdafx.h"
#include "request_license_dialog.h"
#include "sendmail.h"
#include "Patterns Application.h"

// request_license_dialog dialog
IMPLEMENT_DYNAMIC(request_license_dialog, CDialog)
request_license_dialog::request_license_dialog(license_validation* pLicense, CWnd* pParent /* NULL */ ) :
	CDialog(request_license_dialog::IDD, pParent),
	m_pLicense(pLicense),
	m_editusername(_T("")),
	m_editorganization(_T("")),
	m_editphone(_T("")),
	m_editmachineid(_T(""))
{
}

request_license_dialog::~request_license_dialog()
{
}

void request_license_dialog::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDITUSERNAME, m_editusername);
	DDX_Text(pDX, IDC_EDITMACHINEID, m_editmachineid);
	DDX_Text(pDX, IDC_EDITORGANIZATION, m_editorganization);
	DDX_Text(pDX, IDC_EDIT_PHONE, m_editphone);
}

BEGIN_MESSAGE_MAP(request_license_dialog, CDialog)
	ON_BN_CLICKED(IDSENDEMAIL, &request_license_dialog::OnBnClickedSendemail)
	ON_BN_CLICKED(IDCOPYCLIPBOARD, &request_license_dialog::OnBnClickedCopyclipboard)
	ON_BN_CLICKED(IDOK, &request_license_dialog::OnBnClickedOk)
END_MESSAGE_MAP()

BOOL request_license_dialog::OnInitDialog()
{
	CDialog::OnInitDialog();

	m_editusername = m_pLicense->get_user_name();
	m_editorganization = m_pLicense->get_organization();
	m_editphone = m_pLicense->get_phone();
	m_editmachineid = m_pLicense->get_hardware_id();

	UpdateData(FALSE);

	CenterWindow();
	ShowWindow(SW_SHOW);

	return TRUE;
}

void request_license_dialog::OnBnClickedSendemail()
{
	CString msg = prepare_request_text();

#ifdef patterns_research
	bool sent = CSendMailHelper::SendMail(AfxGetMainWnd()->GetSafeHwnd(), "patterns@perigen.com", "Activation code request for PeriCALM® Patterns Retrospective™ - Research ONLY", msg.GetBuffer(msg.GetLength()));
#else
	bool sent = CSendMailHelper::SendMail(AfxGetMainWnd()->GetSafeHwnd(), "patterns@perigen.com", "Activation code request for PeriCALM® Patterns Retrospective™", msg.GetBuffer(msg.GetLength()));
#endif
	msg.ReleaseBuffer();

	if (!sent)
	{
		OnBnClickedCopyclipboard();
		MessageBox("Error: unable to open your email client. Please paste the contents of the clipboard into the body of a new email and send to patterns@perigen.com.", patterns_application::get_application().get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
	}
}

void request_license_dialog::OnBnClickedCopyclipboard()
{
	CString strData = "Please, send an email to 'patterns@perigen.com' with the following text to receive your license.\r\n\r\n";
	strData += prepare_request_text();

	if (OpenClipboard())
	{
		EmptyClipboard();

		HGLOBAL hClipboardData;
		hClipboardData = GlobalAlloc(GMEM_DDESHARE, strData.GetLength() + 1);

		char*  pchData;
		pchData = (char*)GlobalLock(hClipboardData);

		strcpy(pchData, LPCSTR(strData));

		GlobalUnlock(hClipboardData);
		SetClipboardData(CF_TEXT, hClipboardData);
		CloseClipboard();
	}
}

CString request_license_dialog::prepare_request_text()
{
	UpdateData(TRUE);

	// Remember values
	m_pLicense->set_user_info(m_editusername, m_editorganization, m_editphone);

	CString fmt;
	fmt.LoadString(NULL, request_activation_text);

	CString version = FILEVERSTR;
	version += " Build ";
	version += FILEDESCRSTR;

	CString msg;
	msg.Format(
		fmt, 
		m_editusername.GetBuffer(m_editusername.GetLength()), 
		m_editorganization.GetBuffer(m_editorganization.GetLength()),
		m_editphone.GetBuffer(m_editphone.GetLength()),
		m_editmachineid.GetBuffer(m_editmachineid.GetLength()),
		version.GetBuffer(version.GetLength()));

	m_editusername.ReleaseBuffer();
	m_editorganization.ReleaseBuffer();
	m_editphone.ReleaseBuffer();
	m_editmachineid.ReleaseBuffer();
	version.ReleaseBuffer();

	return msg;
}

void request_license_dialog::OnBnClickedOk()
{
	prepare_request_text();

	OnOK();
}

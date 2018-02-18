// about_dlg.cpp : implementation file
#include "stdafx.h"
#include "about_dlg.h"

#include "Patterns Application.h"
#include <htmlhelp.h>

#if defined(patterns_retrospective) && !defined(OEM_patterns)
#include "request_license_dialog.h"
#include "enter_activation_dlg.h"
#endif

// about_dlg dialog
IMPLEMENT_DYNAMIC(about_dlg, CDialog)

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	about_dlg::about_dlg(CWnd *pParent /* NULL */ ) :
CDialog(about_dlg::IDD, pParent)
{
}

/*
=======================================================================================================================
=======================================================================================================================
*/
about_dlg::~about_dlg(void)
{
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void about_dlg::DoDataExchange(CDataExchange *pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COMPANY_ADDRESS, m_CompanyAddressCtrl);
	DDX_Control(pDX, IDC_COPYRIGHT, m_CopyrightCtrl);
	DDX_Control(pDX, IDC_EMAIL, m_EmailCtrl);
	DDX_Control(pDX, IDC_FAX, m_FaxCtrl);
	DDX_Control(pDX, IDC_INTENDED_USE, m_IntendedUse);
	DDX_Control(pDX, IDC_INFORMATION_GROUP, m_ProductInformationGroupCtrl);
	DDX_Control(pDX, IDC_INTERNET, m_InternetCtrl);
	//DDX_Control(pDX, IDC_PRODUCT_NAME, m_ProductNameCtrl);
	DDX_Control(pDX, IDC_VERSION, m_ProductVersionCtrl);
	DDX_Control(pDX, IDC_TECHNICAL, m_TechnicalCtrl);
	DDX_Control(pDX, IDC_TELEPHONE, m_TelephoneCtrl);
	DDX_Control(pDX, IDC_TECHNICAL_PHONE_NUMBER, m_TechnicalNumberCtrl);
	DDX_Control(pDX, IDC_MAIN_PHONE_NUMBERS, m_MainPhoneNumbers);
	DDX_Control(pDX, IDC_FAX_PHONE_NUMBER, m_FaxNumber);
	DDX_Control(pDX, IDC_CONTACT_GROUP, m_ContactGroupCtrl);
	DDX_Control(pDX, IDC_EMAIL_ADDRESS, m_EmailAddressCtrl);
	DDX_Control(pDX, IDC_INTERNET_ADDRESS, m_InternetAddressCtrl);
	DDX_Control(pDX, IDC_TECHNICAL_MAIL_ADDR, m_TechnicalMailAddressCtrl);
}

/*
=======================================================================================================================
init all controls. Set the controls text from the strings in ressources. Also set the hyperlinks using the
CHyperLink class.
=======================================================================================================================
*/
BOOL about_dlg::OnInitDialog(void)
{
	CDialog::OnInitDialog();
	UpdateData();

#if defined(OEM_patterns)
	SetWindowPos(NULL, 0, 0, 398, 410, SWP_NOMOVE);
	GetDlgItem(IDOK)->SetWindowPos(NULL, 310, 350, 0, 0, SWP_NOSIZE);
	GetDlgItem(IDC_PATTERNS_HELP)->SetWindowPos(NULL, 310, 350, 0, 0, SWP_HIDEWINDOW | SWP_NOSIZE);
#elif !defined(patterns_retrospective) 
	SetWindowPos(NULL, 0, 0, 398, 410, SWP_NOMOVE);
	GetDlgItem(IDOK)->SetWindowPos(NULL, 230, 350, 0, 0, SWP_NOSIZE);
	GetDlgItem(IDC_PATTERNS_HELP)->SetWindowPos(NULL, 310, 350, 0, 0, SWP_NOSIZE);
#endif

	CString s;

	s.LoadString(NULL, about_dlg_company_address);
	m_CompanyAddressCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_copyright);
	m_CopyrightCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_contact_group);
	m_ContactGroupCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_support_inquries);
	GetDlgItem(IDC_SUPPORT_AND_INQUIRIES_GROUP)->SetWindowText(s);

	s.LoadString(NULL, about_dlg_email);
	m_EmailCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_internet);
	m_InternetCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_fax);
	m_FaxCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_product_information_group);
	m_ProductInformationGroupCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_technical);
	m_TechnicalCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_telephone);
	m_TelephoneCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_technical_phone_number);
	m_TechnicalNumberCtrl.SetWindowText(s);

	s.LoadString(NULL, about_dlg_main_phone_numbers);
	m_MainPhoneNumbers.SetWindowText(s);

	s.LoadString(NULL, about_dlg_fax_number);
	m_FaxNumber.SetWindowText(s);

	s.LoadString(NULL, retrospective_disclaimer);
	m_IntendedUse.SetWindowText(s);

#if defined(patterns_retrospective) && !defined(OEM_patterns)
	GetDlgItem(IDC_EDIT_NAME)->SetWindowText(patterns_application::get_application().get_license()->get_user_name());
	GetDlgItem(IDC_EDIT_ORGANIZATION)->SetWindowText(patterns_application::get_application().get_license()->get_organization());
	GetDlgItem(IDC_EDIT_PHONE)->SetWindowText(patterns_application::get_application().get_license()->get_phone());
	GetDlgItem(IDC_EDIT_SERIAL)->SetWindowText(patterns_application::get_application().get_license()->get_license_description());
#else
	GetDlgItem(IDC_LICENSE_GRP)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_STATIC_NAME)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_STATIC_ORGANIZATION)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_STATIC_PHONE)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_STATIC_LICENSE)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_BUTTON_REQUEST)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_BUTTON_ACTIVATE)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_EDIT_NAME)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_EDIT_ORGANIZATION)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_EDIT_SERIAL)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
	GetDlgItem(IDC_EDIT_PHONE)->SetWindowPos(0, 0, 0, 0, 0, SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE);
#if defined(patterns_retrospective) && !defined(OEM_patterns)
	GetDlgItem(IDC_BUTTON_ACTIVATE)->EnableWindow(patterns_application::get_application().get_license()->is_license_tampered()?0:1);
#endif
#endif

	//m_ProductNameCtrl.SetWindowText("PeriCALM® Patterns™");

	// product version
	s.LoadString(NULL, about_dlg_version);
	CString s0 = s;
	s0 += " ";
	s0 += FILEVERSTR;
	s0 += " - Build ";
	s0 += FILEDESCRSTR;

	m_ProductVersionCtrl.SetWindowText(s0);

	// set hyperlinks
	s.LoadString(NULL, about_dlg_internet_address);
	m_InternetAddressCtrl.SetWindowText(s);

#if !defined(OEM_patterns)
	m_company_hyperlink.ConvertStaticToHyperlink(GetSafeHwnd(), IDC_INTERNET_ADDRESS, s);
#endif

	s.LoadString(NULL, about_dlg_email_address);
	m_EmailAddressCtrl.SetWindowText(s);


#if !defined(OEM_patterns)
	s0 = "mailto:";
	s0 += s;
	m_email_hyperlink.ConvertStaticToHyperlink(GetSafeHwnd(), IDC_EMAIL_ADDRESS, s0);

#endif

	s.LoadString(NULL, about_dlg_technical_mail);
	GetDlgItem(IDC_TECHNICAL_MAIL)->SetWindowText(s);

	s.LoadString(NULL, about_dlg_technical_mail_addr);
	m_TechnicalMailAddressCtrl.SetWindowText(s);

#if !defined(OEM_patterns)
	s0 = "mailto:";
	s0 += s;
	m_technicalEmail_hyperlink.ConvertStaticToHyperlink(GetSafeHwnd(), IDC_TECHNICAL_MAIL_ADDR, s0);

#endif

	// dialog title.
	s.LoadString(NULL, about_dlg_title);
	s += " ";
	s += patterns_application::get_application().get_product_name().c_str();
	SetWindowText(s);
//#if !defined(patterns_retrospective) || defined(OEM_patterns)
//	HBITMAP hBitmap = (HBITMAP)LoadBitmap(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDB_CE_LOGO));
//	((CStatic*)GetDlgItem(IDC_BMP))->SetBitmap(hBitmap);
//#endif
	CenterWindow();
	ShowWindow(SW_SHOW);

	return TRUE;
}

BEGIN_MESSAGE_MAP(about_dlg, CDialog)
	ON_BN_CLICKED(IDC_PATTERNS_HELP, OnBnClickedPatternsHelp)
	ON_BN_CLICKED(IDC_BUTTON_REQUEST, &about_dlg::OnBnClickedButtonRequest)
	ON_BN_CLICKED(IDC_BUTTON_ACTIVATE, &about_dlg::OnBnClickedButtonActivate)
	ON_BN_CLICKED(IDOK, &about_dlg::OnBnClickedOk)
END_MESSAGE_MAP()

//
// =======================================================================================================================
//    about_dlg message handlers
// =======================================================================================================================
//
void about_dlg::OnBnClickedPatternsHelp(void)
{
	SendMessage(WM_COMMAND, ID_HELP);
}

void about_dlg::OnBnClickedButtonRequest()
{
#if defined(patterns_retrospective) && !defined(OEM_patterns)
	request_license_dialog dlg(patterns_application::get_application().get_license(), this);
	if (dlg.DoModal() == IDOK)
	{
		GetDlgItem(IDC_EDIT_NAME)->SetWindowText(patterns_application::get_application().get_license()->get_user_name());
		GetDlgItem(IDC_EDIT_ORGANIZATION)->SetWindowText(patterns_application::get_application().get_license()->get_organization());
		GetDlgItem(IDC_EDIT_PHONE)->SetWindowText(patterns_application::get_application().get_license()->get_phone());
	}
#endif
}

void about_dlg::OnBnClickedButtonActivate()
{
#if defined(patterns_retrospective) && !defined(OEM_patterns)
	enter_activation_dlg dlg(patterns_application::get_application().get_license(), this);
	if (dlg.DoModal() == IDOK)
	{
		GetDlgItem(IDC_EDIT_SERIAL)->SetWindowText(patterns_application::get_application().get_license()->get_license_description());
	}
#endif
}

void about_dlg::OnBnClickedOk()
{
	// TODO: Add your control notification handler code here
	OnOK();
}

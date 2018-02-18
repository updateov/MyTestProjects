// aboutbox.cpp : implementation file
//

#include "stdafx.h"
#include "ContainerBaseWnd.h"
#include "ControlAboutbox.h"
#include "Resource.h"



///////////////////////////////////////////////////////////////////////
// aboutbox dialog
CWinApp theApp;
IMPLEMENT_DYNAMIC(aboutbox, CDialog)

aboutbox::aboutbox(CWnd* pParent /*=NULL*/)
: CDialog(aboutbox::IDD, pParent)
{
	
	m_font.CreateFont(14,							// Height
						0,							// Width
						0,							// Escapement
						0,							// Orientation
						FW_NORMAL,					// Weight
						FALSE,						// Italic
						FALSE,						// Underline
						0,							// StrikeOut
						ANSI_CHARSET,				// CharSet
						OUT_DEFAULT_PRECIS,			// OutPrecision
						CLIP_DEFAULT_PRECIS,		// ClipPrecision
						DEFAULT_QUALITY,			// Quality
						DEFAULT_PITCH | FF_SWISS,	// PitchAndFamily
						"Consolas");
}

aboutbox::aboutbox(UINT nIDTemplate, CWnd* pParentWnd)
: CDialog(nIDTemplate, pParentWnd)
{
	//AFX_MANAGE_STATE(AfxGetStaticModuleState());
	m_font.CreateFont(14,							// Height
						0,							// Width
						0,							// Escapement
						0,							// Orientation
						FW_NORMAL,					// Weight
						FALSE,						// Italic
						FALSE,						// Underline
						0,							// StrikeOut
						ANSI_CHARSET,				// CharSet
						OUT_DEFAULT_PRECIS,			// OutPrecision
						CLIP_DEFAULT_PRECIS,		// ClipPrecision
						DEFAULT_QUALITY,			// Quality
						DEFAULT_PITCH | FF_SWISS,	// PitchAndFamily
						"Consolas");
}

//void aboutbox::DoDataExchange(CDataExchange* pDX)
//{
//	CDialog::DoDataExchange(pDX);
//	//{{AFX_DATA_MAP(CControlContainerDlg)
//	DDX_Control(pDX, IDC_SUPPORT_MAIL_ADDR, m_supportEmail);
//	DDX_Control(pDX, IDC_MAIL_ADDR, m_email);
//	DDX_Control(pDX, IDC_WEBSITE_URL, m_website);
//	//}}AFX_DATA_MAP
//}

BEGIN_MESSAGE_MAP(aboutbox, CDialog)
	ON_WM_TIMER()
END_MESSAGE_MAP()

const UINT ID_TIMER_TIMEOUT = 0x1001;

void aboutbox::ConfigureDialog(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;

	s.LoadString(aboutbox_company_info);
	GetDlgItem(IDC_CONTACT_INFORMATION)->SetWindowText(s);

	s.LoadString(aboutbox_product_patent);
	GetDlgItem(IDC_PRODUCT_PATENT)->SetWindowText(s);

	s.LoadString(aboutbox_product_group);
	GetDlgItem(IDC_PRODUCT_GROUP)->SetWindowText(s);

	//s.LoadString(aboutbox_intended_use_group);
	//GetDlgItem(IDC_INTENDED_USE_GROUP)->SetWindowText(s);

	s.LoadString(aboutbox_title);
	SetWindowText(s);

	s.LoadString(aboutbox_product_info);
	GetDlgItem(IDC_PRODUCT_INFORMATION)->SetWindowText(s);

	s.LoadString(aboutbox_product_udi);
	GetDlgItem(IDC_PRODUCT_UDI_LABEL)->SetWindowText(s);


	s.LoadString(aboutbox_product_udi_num);
	GetDlgItem(IDC_PRODUCT_UDI_NUMBER)->SetFont(&m_font);
	GetDlgItem(IDC_PRODUCT_UDI_NUMBER)->SetWindowText(s);

	s.LoadString(aboutbox_product_copyright);
	GetDlgItem(IDC_ABOUT_COPYRIGHT)->SetWindowText(s);

	s.LoadString(aboutbox_manufactured_group);
	GetDlgItem(IDC_CONTACT_GROUP)->SetWindowText(s);

	s.LoadString(aboutbox_intended_use);
	GetDlgItem(IDC_INTENDED_USE_INFORMATION)->SetWindowText(s);

	s.LoadString(aboutbox_statement_instructions);
	GetDlgItem(IDC_ABOUT_STATEMENT_INSTRUCTIONS)->SetWindowText(s);

	s.LoadString(aboutbox_support_group);
	GetDlgItem(IDC_SUPPORT_GROUP)->SetWindowText(s);

	s.LoadString(aboutbox_support_phone);
	GetDlgItem(IDC_SUPPORT_PHONE)->SetWindowText(s);

	s.LoadString(aboutbox_support_number1);
	GetDlgItem(IDC_SUPPORT_PHONE_NUMBER1)->SetWindowText(s);

	s.LoadString(aboutbox_support_number2);
	GetDlgItem(IDC_SUPPORT_PHONE_NUMBER2)->SetWindowText(s);

	s.LoadString(aboutbox_support_mail);
	GetDlgItem(IDC_SUPPORT_MAIL)->SetWindowText(s);

	s.LoadString(aboutbox_support_mail_addr);
	GetDlgItem(IDC_SUPPORT_MAIL_ADDR)->SetWindowText(s);
	m_supportEmail.ConvertStaticToHyperlink(GetSafeHwnd(), IDC_SUPPORT_MAIL_ADDR, "mailto:support@perigen.com");


	s.LoadString(aboutbox_info);
	GetDlgItem(IDC_INFO_PHONE)->SetWindowText(s);

	s.LoadString(aboutbox_info_number);
	GetDlgItem(IDC_INFO_NUMBER)->SetWindowText(s);

	s.LoadString(aboutbox_fax);
	GetDlgItem(IDC_FAX)->SetWindowText(s);

	s.LoadString(aboutbox_fax_number);
	GetDlgItem(IDC_FAX_NUMBER)->SetWindowText(s);

	s.LoadString(aboutbox_email);
	GetDlgItem(IDC_MAIL)->SetWindowText(s);

	s.LoadString(aboutbox_email_address);
	GetDlgItem(IDC_MAIL_ADDR)->SetWindowText(s);
	m_email.ConvertStaticToHyperlink(GetSafeHwnd(), IDC_MAIL_ADDR, "mailto:perigen@perigen.com");
	//m_email.SetLinkText(s);
	//m_email.SetLinkUrl("mailto:perigen@perigen.com");

	s.LoadString(aboutbox_website);
	GetDlgItem(IDC_WEBSITE)->SetWindowText(s);

	s.LoadString(aboutbox_website_url);
	GetDlgItem(IDC_WEBSITE_URL)->SetWindowText(s);
	m_website.ConvertStaticToHyperlink(GetSafeHwnd(), IDC_WEBSITE_URL, "http://www.perigen.com");


	HBITMAP hBitmap = (HBITMAP)LoadBitmap(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDB_ABOUT_LOGO));
	((CStatic*)GetDlgItem(IDC_ABOUT_LOGO))->SetBitmap(hBitmap);
}

BOOL aboutbox::OnInitDialog(void)
{
	
	CDialog::OnInitDialog();
	UpdateData();
	ConfigureDialog();

	m_TimeoutTimer = SetTimer(ID_TIMER_TIMEOUT, ContainerBaseWnd::get_timeout_dialog() * 60000, 0);

	return true;
}

void aboutbox::OnTimer(UINT_PTR timer_id)
{
	if (timer_id == ID_TIMER_TIMEOUT)
	{
		// Timeout
		KillTimer(m_TimeoutTimer);
		m_TimeoutTimer = 0;

		// Close the dialog
		EndDialog(IDCANCEL);
	}
}


aboutbox_powerby::aboutbox_powerby(CWnd* pParent /*=NULL*/)
: aboutbox(aboutbox_powerby::IDD, pParent)
{
}

void aboutbox_powerby::ConfigureDialog(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CString s;

	s.LoadString(aboutbox_company_info);
	GetDlgItem(IDC_CONTACT_INFORMATION)->SetWindowText(s);

	s.LoadString(aboutbox_product_patent);
	GetDlgItem(IDC_PRODUCT_PATENT)->SetWindowText(s);

	s.LoadString(aboutbox_product_group);
	GetDlgItem(IDC_PRODUCT_GROUP)->SetWindowText(s);

	s.LoadString(aboutbox_intended_use_group);
	GetDlgItem(IDC_INTENDED_USE_GROUP)->SetWindowText(s);

	s.LoadString(aboutbox_title);
	SetWindowText(s);

	s.LoadString(aboutbox_product_info);
	GetDlgItem(IDC_PRODUCT_INFORMATION)->SetWindowText(s);

	s.LoadString(aboutbox_manufactured_group);
	GetDlgItem(IDC_CONTACT_GROUP)->SetWindowText(s);

	s.LoadString(aboutbox_intended_use);
	GetDlgItem(IDC_INTENDED_USE_INFORMATION)->SetWindowText(s);

	HBITMAP hBitmap = (HBITMAP)LoadBitmap(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDB_ABOUT_LOGO));
	((CStatic*)GetDlgItem(IDC_ABOUT_LOGO))->SetBitmap(hBitmap);

	((CStatic*)GetDlgItem(IDC_BMP_CE_LOGO))->ShowWindow(SW_HIDE);
}
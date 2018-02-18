// aboutbox.cpp : implementation file
//

#include "stdafx.h"
#include "CRIPatternsChart.h"
#include "aboutbox.h"

//HyperLink::HyperLink()
//{
//	m_bMouseOver = false;
//	m_bVisited =  false;
//
//	m_sLinkColor = RGB(0, 0 ,255);
//	m_sHoverColor = RGB(0, 0, 255);
//	m_sVisitedColor = RGB(110, 0, 161);
//}
//
//HyperLink::~HyperLink()
//{
//}
//
//BEGIN_MESSAGE_MAP(HyperLink, CStatic)
//	ON_CONTROL_REFLECT(BN_CLICKED, OnClicked)
//	ON_WM_CTLCOLOR_REFLECT()
//	ON_WM_MOUSEMOVE()
//	ON_WM_SETCURSOR()
//END_MESSAGE_MAP()
//
//void HyperLink::OnClicked() 
//{
//	GoToLinkUrl(m_csUrl);
//
//	//reddraw the control 
//	this->Invalidate(true);
//}
//
//BOOL HyperLink::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message) 
//{
//
//	::SetCursor(m_hHyperCursor);
//	return true;
//	//return CStatic::OnSetCursor(pWnd, nHitTest, message);
//}
//
////open the URL by Windows ShellExecute()
//bool HyperLink::GoToLinkUrl(CString csLink)
//{
//
//	HINSTANCE hInstance = (HINSTANCE)ShellExecute(NULL, _T("open"), csLink, NULL, NULL, 2);
//
//	if ((UINT)hInstance < HINSTANCE_ERROR)
//	{
//		return false;
//	}
//	else
//		return true;
//}
//
////The Mouse Move Message
//void HyperLink::OnMouseMove(UINT nFlags, CPoint point) 
//{
//	CStatic::OnMouseMove(nFlags, point);
//	if (m_bMouseOver)
//	{
//		CRect oRect;
//		GetClientRect(&oRect);
//
//		//check if the mouse is in the rect
//		if (oRect.PtInRect(point) == false)
//		{
//			m_bMouseOver = false;
//			//Release the Mouse capture previously take
//			ReleaseCapture();
//			RedrawWindow();
//			return;
//		}
//	}
//	else
//	{
//		m_bMouseOver = true;
//		RedrawWindow();
//		//capture the mouse
//		SetCapture();
//	}
//}
//
//HBRUSH HyperLink::CtlColor(CDC* pDC, UINT nCtlColor) 
//{
//	if (m_bMouseOver){
//		if (m_bVisited)
//			pDC->SetTextColor(m_sVisitedColor);
//		else
//			pDC->SetTextColor(m_sHoverColor);
//	}else {
//		if (m_bVisited)
//			pDC->SetTextColor(m_sVisitedColor);
//		else
//			pDC->SetTextColor(m_sLinkColor);
//	}
//	pDC->SetBkMode(TRANSPARENT);
//	return((HBRUSH)GetStockObject(NULL_BRUSH));
//}
//
////before Subclassing 
//void HyperLink::PreSubclassWindow() 
//{
//	//Enable the Static to send the Window Messages To its parent
//	DWORD dwStyle = GetStyle();
//	SetWindowLong(GetSafeHwnd() ,GWL_STYLE ,dwStyle | SS_NOTIFY);
//	
//	CStatic::PreSubclassWindow();
//}
//
//void HyperLink::SetLinkText(CString csLinkText)
//{
//	m_csLinkText = csLinkText;
//	this->SetWindowText(csLinkText);
//
//}
//
//CString HyperLink::GetLinkText() 
//{
//	if (m_csLinkText.IsEmpty())
//		return CString("");
//
//	return m_csLinkText;
//}
//
//void HyperLink::SetLinkUrl(CString csUrl) 
//{
//	m_csUrl = csUrl;
//}
//
//CString HyperLink::GetLinkUrl() 
//{
//	return m_csUrl;
//}


///////////////////////////////////////////////////////////////////////
// aboutbox dialog

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
	//m_supportEmail.SetLinkText(s);
	//m_supportEmail.SetLinkUrl("mailto:support@perigen.com");

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
	//m_website.SetLinkText(s);
	//m_website.SetLinkUrl("http://www.perigen.com");

	HBITMAP hBitmap = (HBITMAP)LoadBitmap(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDB_ABOUT_PERIGEN_BANNER));
	((CStatic*)GetDlgItem(IDC_ABOUT_LOGO))->SetBitmap(hBitmap);
}

BOOL aboutbox::OnInitDialog(void)
{
	CDialog::OnInitDialog();
	UpdateData();
	ConfigureDialog();

	m_TimeoutTimer = SetTimer(ID_TIMER_TIMEOUT, CRIPatternsChartApp::get_timeout_dialog() * 60000, 0);

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

aboutbox_ge::aboutbox_ge(CWnd* pParent /*=NULL*/)
: aboutbox(aboutbox_ge::IDD, pParent)
{
}

void aboutbox_ge::ConfigureDialog(void)
{
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

	s.LoadString(aboutbox_product_info_ge);
	GetDlgItem(IDC_PRODUCT_INFORMATION)->SetWindowText(s);

	s.LoadString(aboutbox_manufactured_group);
	GetDlgItem(IDC_CONTACT_GROUP)->SetWindowText(s);

	s.LoadString(aboutbox_intended_use_ge);
	GetDlgItem(IDC_INTENDED_USE_INFORMATION)->SetWindowText(s);

	HBITMAP hBitmap = (HBITMAP)LoadBitmap(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDB_ABOUT_GE_BANNER));
	((CStatic*)GetDlgItem(IDC_ABOUT_LOGO))->SetBitmap(hBitmap);
}

aboutbox_powerby::aboutbox_powerby(CWnd* pParent /*=NULL*/)
: aboutbox(aboutbox_powerby::IDD, pParent)
{
}

void aboutbox_powerby::ConfigureDialog(void)
{
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

	HBITMAP hBitmap = (HBITMAP)LoadBitmap(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDB_POWERBY_PERIGEN_BANNER));
	((CStatic*)GetDlgItem(IDC_ABOUT_LOGO))->SetBitmap(hBitmap);

	((CStatic*)GetDlgItem(IDC_BMP_CE_LOGO))->ShowWindow(SW_HIDE);
}
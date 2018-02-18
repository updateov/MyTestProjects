#pragma once
#include "afxwin.h"
#include "resource.h"
#include "hyperlink.h"

/*
 =======================================================================================================================
    about_dlg class. Display the lms about dialog. All strings are extracted from the ressources. The hyperlinks are
    managed by a free class named CHyperLink.
 =======================================================================================================================
 */
class about_dlg :
	public CDialog
{
		DECLARE_DYNAMIC(about_dlg)
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
#if !defined(OEM_patterns)
	private:
		CHyperLink m_company_hyperlink;
		CHyperLink m_email_hyperlink;
		CHyperLink m_technicalEmail_hyperlink;
#endif
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		about_dlg(CWnd *pParent = NULL);	// standard constructor
		virtual ~about_dlg(void);

		// Dialog Data
		enum { IDD = IDD_ABOUTBOX };

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		virtual void DoDataExchange(CDataExchange *pDX);	// DDX/DDV support
		virtual BOOL OnInitDialog(void);
		virtual afx_msg void OnBnClickedPatternsHelp(void);

		DECLARE_MESSAGE_MAP()
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		CStatic m_AddressGroupCtrl;
		CStatic m_CompanyAddressCtrl;
		CStatic m_CopyrightCtrl;
		CStatic m_EmailCtrl;
		CStatic m_FaxCtrl;
		CStatic m_ProductInformationGroupCtrl;
		CStatic m_InternetCtrl;
		//CStatic m_ProductNameCtrl;
		CStatic m_ProductVersionCtrl;
		CStatic m_TechnicalCtrl;
		CStatic m_TelephoneCtrl;
		CStatic m_TechnicalNumberCtrl;
		CStatic m_MainPhoneNumbers;
		CStatic m_FaxNumber;
		CStatic m_ContactGroupCtrl;
		CStatic m_EmailAddressCtrl;
		CStatic m_InternetAddressCtrl;
		CStatic m_IntendedUse;
		CStatic m_TechnicalMailAddressCtrl;

		afx_msg void OnBnClickedButtonRequest();
		afx_msg void OnBnClickedButtonActivate();
		afx_msg void OnBnClickedOk();
};

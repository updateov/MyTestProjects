#pragma once

#include "resource.h"
#include "license_validation.h"

// request_license_dialog dialog

class request_license_dialog : public CDialog
{
	DECLARE_DYNAMIC(request_license_dialog)

public:
	request_license_dialog(license_validation* pLicense, CWnd* pParent = NULL);   // standard constructor
	virtual ~request_license_dialog();

	// Dialog Data
	enum { IDD = IDD_REQUEST_LICENSE };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()

protected:
	license_validation* m_pLicense;
	CString m_editusername;
	CString m_editorganization;
	CString m_editphone;
	CString m_editmachineid;

	CString prepare_request_text();

public:
	afx_msg void OnBnClickedSendemail();
	afx_msg void OnBnClickedCopyclipboard();
	afx_msg void OnBnClickedOk();
};

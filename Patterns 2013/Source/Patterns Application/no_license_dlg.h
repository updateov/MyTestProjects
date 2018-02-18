#pragma once

#include "resource.h"
#include "license_validation.h"

// no_license_dlg dialog

class no_license_dlg : public CDialog
{
	DECLARE_DYNAMIC(no_license_dlg)

public:
	no_license_dlg(license_validation* pLicense, CWnd* pParent = NULL);   // standard constructor
	virtual ~no_license_dlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_NO_LICENSE };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog(void);

	DECLARE_MESSAGE_MAP()

	license_validation* m_pLicense;
	CStatic m_WarningIcon;
	
public:
	afx_msg void OnBnClickedButtonRequestCode();
	afx_msg void OnBnClickedButtonEnterCode();
	afx_msg void OnBnClickedCancel();
};

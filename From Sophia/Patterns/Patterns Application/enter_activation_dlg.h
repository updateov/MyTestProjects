#pragma once

#include "resource.h"
#include "license_validation.h"

// enter_activation_dlg dialog

class enter_activation_dlg : public CDialog
{
	DECLARE_DYNAMIC(enter_activation_dlg)

public:
	enter_activation_dlg(license_validation* pLicense, CWnd* pParent = NULL);   // standard constructor
	virtual ~enter_activation_dlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_ENTER_LICENSE };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedOk();

protected:
	license_validation* m_pLicense;
	CString m_editkey;
};

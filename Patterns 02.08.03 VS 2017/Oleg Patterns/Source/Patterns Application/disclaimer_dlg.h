#pragma once

#include "resource.h"

// disclaimer_dlg dialog

class disclaimer_dlg : public CDialog
{
	DECLARE_DYNAMIC(disclaimer_dlg)

public:
	disclaimer_dlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~disclaimer_dlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_DISCLAIMER };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog(void);

	DECLARE_MESSAGE_MAP()
public:
	BOOL m_radio_agree;
	afx_msg void OnBnClickedRadioDisagree();
	afx_msg void OnBnClickedRadioAgree();
};

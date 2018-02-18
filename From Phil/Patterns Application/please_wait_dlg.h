#pragma once

#include "resource.h"

// please_wait_dlg dialog

class please_wait_dlg : public CDialog
{
	DECLARE_DYNAMIC(please_wait_dlg)

public:
	please_wait_dlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~please_wait_dlg();

// Dialog Data
	enum { IDD = IDD_PLEASE_WAIT };

	void SetMessage(CString message);

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	virtual BOOL OnInitDialog(void);

	DECLARE_MESSAGE_MAP()
};

#pragma once

#include "resource.h"

// auto_update_dlg dialog

class auto_update_dlg : public CDialog
{
	DECLARE_DYNAMIC(auto_update_dlg)

public:
	auto_update_dlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~auto_update_dlg();

	static string CheckForUpdate();
	
	void DoUpgrade();

// Dialog Data
	enum { IDD = IDD_DIALOG_AUTOUPDATE };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	virtual BOOL OnInitDialog(void);

	afx_msg void OnCancelUpgrade();
	afx_msg void OnTimer(UINT_PTR);
	
	CRITICAL_SECTION critical_section;

	bool upgrade_cancelled;
	bool upgrade_failed;
	bool upgrade_running;

	int upgrade_progress;

	DECLARE_MESSAGE_MAP()
};

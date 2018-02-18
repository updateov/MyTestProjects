//Review: 17/02/15
// CRI_ToolBarDlg.h : header file
//

#pragma once

#include "ToolbarCtrl.h"

#define OWM_AGENT_MSG        (WM_APP+11)

//TODO: remove the "C" prefix from class name

// CRI_ToolBarDlg dialog
class CRI_ToolBarDlg : public CDialog
{
private:
	CToolbarCtrl *m_pcoControl;
	HWND m_hCALM_Agent;
	HICON m_hIcon;

	// Construction
public:
	CRI_ToolBarDlg(CWnd* pParent = NULL);	// standard constructor
	virtual ~CRI_ToolBarDlg();

	// Dialog Data
	enum { IDD = IDD_CRI_TOOLBAR_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

	// Implementation
protected:

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnAppCommand(CWnd* pWnd, UINT nCmd, UINT nDevice, UINT nKey);
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	afx_msg void OnClose();
	afx_msg LRESULT OnAgentMessage(WPARAM, LPARAM);
};

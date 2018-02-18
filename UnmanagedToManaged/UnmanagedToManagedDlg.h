
// UnmanagedToManagedDlg.h : header file
//

#pragma once

class UnmanagedWrapper;
//#include "Wrapper\UnmanagedWrapper.h"

// UnmanagedToManagedDlg dialog

class UnmanagedToManagedDlg : public CDialogEx
{
// Construction
public:
	UnmanagedToManagedDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_UNMANAGEDTOMANAGED_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonRunManaged();

	void GetHwnd(HWND parent, int x, int y, int width, int height);

private:
	UnmanagedWrapper* m_pWrapper;
};

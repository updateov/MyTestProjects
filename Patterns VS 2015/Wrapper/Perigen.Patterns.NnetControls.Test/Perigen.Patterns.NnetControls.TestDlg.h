
// Perigen.Patterns.NnetControls.TestDlg.h : header file
//

#pragma once

class NativeManager;

// CPerigenPatternsNnetControlsTestDlg dialog
class CPerigenPatternsNnetControlsTestDlg : public CDialogEx
{
// Construction
public:
	CPerigenPatternsNnetControlsTestDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_PERIGENPATTERNSNNETCONTROLSTEST_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBnClickedButton1();
	afx_msg void OnBnClickedButton2();
	afx_msg void OnBnClickedButton3();	
	
	NativeManager* m_pPatternsAdapter;
	afx_msg void OnBnClickedButton4();
	afx_msg void OnBnClickedButton5();
};

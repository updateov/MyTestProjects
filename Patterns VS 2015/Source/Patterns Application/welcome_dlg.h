#pragma once

#include "resource.h"
#include "dib256.h"

// welcome_dlg dialog

class welcome_dlg : public CDialog
{
public:
	welcome_dlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~welcome_dlg();

// Dialog Data
	enum { IDD = IDD_WELCOME_SCREEN };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	afx_msg BOOL OnQueryNewPalette();
	afx_msg void OnPaletteChanged(CWnd* pFocusWnd);

	CDIBitmap m_bmpBackground;
	CBrush m_HollowBrush;

	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnNMClickSlOpenfile(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMClickSlTutorial(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMClickSlHelp(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMClickSlWebsite(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnNMClickSlLoadsamples(NMHDR *pNMHDR, LRESULT *pResult);
	
	int sampleChecked;
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedCheckSamples();
	afx_msg void OnNMClickSlAutoupdate(NMHDR *pNMHDR, LRESULT *pResult);
};

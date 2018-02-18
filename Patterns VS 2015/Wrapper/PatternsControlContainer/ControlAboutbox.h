#pragma once
#include "afxwin.h"
#include "resource.h"
#include "hyperlink.h"

//class HyperLink : public CStatic
//{
//public:
//	// Construction
//	HyperLink();
//	virtual ~HyperLink();
//
//	// Implementation
//	CString GetLinkText();
//	CString GetLinkUrl();
//
//	virtual void PreSubclassWindow();
//
//	bool GoToLinkUrl(CString csLink);
//
//	void SetLinkUrl(CString csUrl);
//	void SetLinkText(CString csLinkText);
//
//protected:
//	HCURSOR m_hHyperCursor;	
//
//	bool m_bMouseOver;
//	bool m_bVisited;
//
//	COLORREF m_sHoverColor;
//	COLORREF m_sLinkColor;
//	COLORREF m_sVisitedColor;
//
//	CFont m_oTextFont;
//
//	CString m_csLinkText;
//	CString m_csUrl;
//
//	afx_msg void OnClicked();
//	afx_msg HBRUSH CtlColor(CDC* pDC, UINT nCtlColor);
//	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
//	afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
//
//	DECLARE_MESSAGE_MAP()
//};


// aboutbox dialog

class aboutbox : public CDialog
{
	DECLARE_DYNAMIC(aboutbox)

public:
	aboutbox(CWnd* pParent = NULL);   // standard constructor
	aboutbox(UINT nIDTemplate, CWnd* pParentWnd = NULL);
	virtual ~aboutbox() {};
	//virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

// Dialog Data
	enum { IDD = IDD_ABOUT_DLG };

private:
		CHyperLink m_website;
		CHyperLink m_email;
		CHyperLink m_supportEmail;

protected:
	virtual BOOL OnInitDialog(void);

	virtual void ConfigureDialog(void);

	CFont m_font;
	UINT m_TimeoutTimer;
	afx_msg void OnTimer(UINT_PTR);

	DECLARE_MESSAGE_MAP()
};


//class aboutbox_ge : public aboutbox
//{
//public:
//	aboutbox_ge(CWnd* pParent = NULL);   // standard constructor
//	virtual ~aboutbox_ge() {};
//
//// Dialog Data
//	enum { IDD = IDD_ABOUT_DLG_GE };
//
//protected:
//	virtual void ConfigureDialog(void);
//};

class aboutbox_powerby : public aboutbox
{
public:
	aboutbox_powerby(CWnd* pParent = NULL);   // standard constructor
	virtual ~aboutbox_powerby() {};

// Dialog Data
	enum { IDD = IDD_ABOUT_DLG };

protected:
	virtual void ConfigureDialog(void);
};
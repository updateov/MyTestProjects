#pragma once


// MyControl dialog
#ifdef IMPEXP
#undef IMPEXP
#endif

#ifdef OCCL
#define IMPEXP __declspec(dllexport)
#else
#define IMPEXP __declspec(dllimport)
#endif

class IMPEXP MyControl : public CDialog
{
	DECLARE_DYNAMIC(MyControl)

public:
	MyControl(CWnd* pParent = NULL);   // standard constructor
	virtual ~MyControl();

	// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMyButton)
public:
	virtual void OnDrawItem(UINT nIdCTL, LPDRAWITEMSTRUCT lpDrawItemStruct);
protected:
	virtual void PreSubclassWindow();
	//}}AFX_VIRTUAL
	
	// Dialog Data
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_DIALOG1 };
#endif

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	afx_msg void OnPaint();

	DECLARE_MESSAGE_MAP()
};

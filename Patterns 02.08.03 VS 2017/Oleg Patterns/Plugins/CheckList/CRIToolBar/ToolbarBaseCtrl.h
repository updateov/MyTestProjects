#if !defined(AFX_TEXTPROGRESSCTRL_H__4C78DBBE_EFB6_11D1_AB14_203E25000000__INCLUDED_)
#define AFX_TEXTPROGRESSCTRL_H__4C78DBBE_EFB6_11D1_AB14_203E25000000__INCLUDED_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

/////////////////////////////////////////////////////////////////////////////
// CToolbarBaseCtrl window

enum CRIState
{
	Off = 0,
	UnknownNotEnoughTime,
	UnknownGAOrSingletonNotMet,
	UnknownGAOrSingletonMissing,
	PositivePastNotYetReviewed,
	PositiveCurrent,
	PositiveReviewed,
	Negative
};

class CToolbarBaseCtrl : public CProgressCtrl
{
	// Construction
public:
	CToolbarBaseCtrl();

	// Attributes
public:

	// Operations
public:
	int			SetPos(int nPos);
	int			StepIt();
	void		SetRange(int nLower, int nUpper);
	int			OffsetPos(int nPos);
	int			SetStep(int nStep);
	void		SetForeColour(COLORREF col);
	void		SetBkColour(COLORREF col);
	void		SetTextForeColour(COLORREF col);
	void		SetTextBkColour(COLORREF col);
	COLORREF	GetForeColour();
	COLORREF	GetBkColour();
	COLORREF	GetTextForeColour();
	COLORREF	GetTextBkColour();

	void		SetShowText(BOOL bShow);
	void		DrawBitmap();
	CString		GetPNGPathByStatus(CRIState status);

	// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CToolbarBaseCtrl)
	//}}AFX_VIRTUAL

	// Implementation
public:
	virtual ~CToolbarBaseCtrl();
public:
	void ShowMyText(char* text, bool show);
	void SetBkControl(CWnd* parent);
	void SetCRIStatus(CRIState status);

	// Generated message map functions
private:
	int			m_nPos,
		m_nStepSize,
		m_nMax,
		m_nMin;
	CRIState	m_nCRIStatus;
	//CString		m_strText;
	BOOL		m_bShowText;
	int			m_nBarWidth;
	COLORREF	m_colFore,
		m_colBk,
		m_colTextFore,
		m_colTextBk;
	bool		m_showMyText;
	//CStatic*	m_statBackground;

	//char m_myText[20];

protected:
	//{{AFX_MSG(CToolbarBaseCtrl)
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnPaint();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	//}}AFX_MSG
	afx_msg void OnLeftClick();
	afx_msg void OnRightClick();

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Developer Studio will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TEXTPROGRESSCTRL_H__4C78DBBE_EFB6_11D1_AB14_203E25000000__INCLUDED_)

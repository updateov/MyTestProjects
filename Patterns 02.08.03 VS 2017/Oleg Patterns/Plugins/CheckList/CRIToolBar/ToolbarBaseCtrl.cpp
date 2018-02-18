#include "stdafx.h"
#include "ToolbarBaseCtrl.h"
#include "resource.h"
#include "atlimage.h"



#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


#ifndef _MEMDC_H_
//////////////////////////////////////////////////

////   PNG Resources /////
#define ID_STATIC_BOX 50000

class CMemDC : public CDC
{
public:

	// constructor sets up the memory DC
	CMemDC(CDC* pDC) : CDC()
	{
		ASSERT(pDC != NULL);

		m_pDC = pDC;
		m_pOldBitmap = NULL;
		m_bMemDC = !pDC->IsPrinting();

		if (m_bMemDC)    // Create a Memory DC
		{
			pDC->GetClipBox(&m_rect);
			CreateCompatibleDC(pDC);
			m_bitmap.CreateCompatibleBitmap(pDC, m_rect.Width(), m_rect.Height());
			m_pOldBitmap = SelectObject(&m_bitmap);
			SetWindowOrg(m_rect.left, m_rect.top);
		}
		else        // Make a copy of the relevent parts of the current DC for printing
		{
			m_bPrinting = pDC->m_bPrinting;
			m_hDC = pDC->m_hDC;
			m_hAttribDC = pDC->m_hAttribDC;
		}
	}

	// Destructor copies the contents of the mem DC to the original DC
	~CMemDC()
	{
		if (m_bMemDC)
		{
			try
			{
				// Copy the offscreen bitmap onto the screen.
				m_pDC->BitBlt(m_rect.left, m_rect.top, m_rect.Width(), m_rect.Height(),
					this, m_rect.left, m_rect.top, SRCCOPY);

				//Swap back the original bitmap.
				SelectObject(m_pOldBitmap);
			}
			catch (...)
			{
				m_hDC = m_hAttribDC = NULL;
				TRACE0("ERROR: ~CMemDC\n");
			}
		}
		else
		{
			// All we need to do is replace the DC with an illegal value,
			// this keeps us from accidently deleting the handles associated with
			// the CDC that was passed to the constructor.
			m_hDC = m_hAttribDC = NULL;
		}
	}

	// Allow usage as a pointer
	CMemDC* operator->() { return this; }

	// Allow usage as a pointer
	operator CMemDC*() { return this; }

private:
	CBitmap  m_bitmap;      // Offscreen bitmap
	CBitmap* m_pOldBitmap;  // bitmap originally found in CMemDC
	CDC*     m_pDC;         // Saves CDC passed in constructor
	CRect    m_rect;        // Rectangle of drawing area.
	BOOL     m_bMemDC;      // TRUE if CDC really is a Memory DC.
};

#endif



/////////////////////////////////////////////////////////////////////////////
// CToolbarBaseCtrl

CToolbarBaseCtrl::CToolbarBaseCtrl()
{
	m_nPos = 0;
	m_nStepSize = 1;
	m_nMax = 100;
	m_nMin = 0;
	m_bShowText = TRUE;
	//m_strText.Empty();
	m_colFore = ::GetSysColor(COLOR_HIGHLIGHT);
	m_colBk = ::GetSysColor(COLOR_WINDOW);
	m_colTextFore = ::GetSysColor(COLOR_HIGHLIGHT);
	m_colTextBk = ::GetSysColor(COLOR_WINDOW);
	m_showMyText = false;

	m_nBarWidth = -1;

	//m_statBackground = new CStatic;
	m_nCRIStatus = (CRIState)0;
	//m_myText = new CString();
}

CToolbarBaseCtrl::~CToolbarBaseCtrl()
{
	/*if (m_statBackground != NULL)
	{
		delete m_statBackground;
	}*/
}

BEGIN_MESSAGE_MAP(CToolbarBaseCtrl, CProgressCtrl)
	//{{AFX_MSG_MAP(CToolbarBaseCtrl)
	ON_WM_ERASEBKGND()
	ON_WM_PAINT()
	ON_WM_SIZE()
	//}}AFX_MSG_MAP    
	ON_STN_CLICKED(ID_STATIC_BOX, OnLeftClick)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CToolbarBaseCtrl message handlers

void CToolbarBaseCtrl::OnLeftClick()
{


}

void CToolbarBaseCtrl::OnRightClick()
{

}

BOOL CToolbarBaseCtrl::OnEraseBkgnd(CDC* /*pDC*/)
{
	return TRUE;
}

void CToolbarBaseCtrl::OnSize(UINT nType, int cx, int cy)
{
	//CWnd::OnSize(nType, cx, cy);

	m_nBarWidth = -1;   // Force update if SetPos called
}

void CToolbarBaseCtrl::SetBkControl(CWnd* parent)
{
	CRect rect;
	this->GetClientRect(rect);

}

void CToolbarBaseCtrl::SetCRIStatus(CRIState status)
{
	m_nCRIStatus = status;
}

CString	CToolbarBaseCtrl::GetPNGPathByStatus(CRIState status)
{
	switch (status)
	{
	case Off:
		return ".\\res\\off_-blue.png";

	case Negative:
		return ".\\res\\negative-blue.png";

	case PositiveCurrent:
		return ".\\res\\positive-blue.png";

	case PositivePastNotYetReviewed:
		return ".\\res\\positive-past_-blue.png";

	case PositiveReviewed: // ICON MISSING!!!
		return ".\\res\\positive-blue.png";

	case UnknownGAOrSingletonMissing:
		return ".\\res\\unknown-missing-ga_-blue.png";

	case UnknownNotEnoughTime:
		return ".\\res\\unknown_time-blue.png";

	case UnknownGAOrSingletonNotMet:
		return ".\\res\\unknown-criteria-blue.png";

	default:
		return ".\\res\\off_-blue.png";
	}
}

void CToolbarBaseCtrl::OnPaint()
{
	//m_nCRIStatus = (CRIState)(rand() % 8);
	DrawBitmap();

	//this->RedrawWindow();
}

void CToolbarBaseCtrl::DrawBitmap()
{
	try
	{
		if (m_nMin >= m_nMax)
			return;

		CRect LeftRect, RightRect, ClientRect;
		GetClientRect(ClientRect);

		double Fraction = (double)(m_nPos - m_nMin) / ((double)(m_nMax - m_nMin));

		CPaintDC PaintDC(this); // device context for painting
		CMemDC dc(&PaintDC);

		LeftRect = RightRect = ClientRect;

		LeftRect.right = LeftRect.left + (int)((LeftRect.right - LeftRect.left)*Fraction);

		CImage img;
		img.Load(GetPNGPathByStatus(m_nCRIStatus));

		CBitmap bmp;

		if (bmp.Attach(img.Detach()))
		{
			BITMAP bmpInfo;
			bmp.GetBitmap(&bmpInfo);

			CDC dcMemory;
			dcMemory.CreateCompatibleDC(&dc);

			CBitmap* pOldBitmap = dcMemory.SelectObject(&bmp);
			//CBitmap* pOldBitmap = dc.SelectObject(&bmp);

			GetClientRect(&RightRect);
			int nX = RightRect.left + (RightRect.Width() - bmpInfo.bmWidth) / 2;
			int nY = RightRect.top + (RightRect.Height() - bmpInfo.bmHeight) / 2;

			
			//dc.StretchBlt(0, 0, RightRect.Width(), RightRect.Height(), &dcMemory, 0, 0, bmpInfo.bmWidth, bmpInfo.bmHeight, SRCCOPY);
			dc.BitBlt(nX, nY, bmpInfo.bmWidth, bmpInfo.bmHeight, &dcMemory, 0, 0, SRCCOPY);

			dcMemory.SelectObject(pOldBitmap);
			//dc.SelectObject(pOldBitmap);

			RightRect.left = LeftRect.right;
			dc.FillSolidRect(RightRect, m_colBk);
		}
		else
		{
			TRACE0("ERROR: Where's IDB_BITMAP1?\n");
		}
	}
	catch (...)
	{
		TRACE0("ERROR DrawBitmap\n");
	}
}

void CToolbarBaseCtrl::SetForeColour(COLORREF col)
{
	m_colFore = col;
}

void CToolbarBaseCtrl::SetBkColour(COLORREF col)
{
	m_colBk = col;
}

void CToolbarBaseCtrl::SetTextForeColour(COLORREF col)
{
	m_colTextFore = col;
}

void CToolbarBaseCtrl::SetTextBkColour(COLORREF col)
{
	m_colTextBk = col;
}

COLORREF CToolbarBaseCtrl::GetForeColour()
{
	return m_colFore;
}

COLORREF CToolbarBaseCtrl::GetBkColour()
{
	return m_colBk;
}

COLORREF CToolbarBaseCtrl::GetTextForeColour()
{
	return m_colTextFore;
}

COLORREF CToolbarBaseCtrl::GetTextBkColour()
{
	return m_colTextBk;
}
/////////////////////////////////////////////////////////////////////////////
// CToolbarBaseCtrl message handlers

void CToolbarBaseCtrl::SetShowText(BOOL bShow)
{
	if (::IsWindow(m_hWnd) && m_bShowText != bShow)
		Invalidate();

	m_bShowText = bShow;
}


void CToolbarBaseCtrl::SetRange(int nLower, int nUpper)
{
	m_nMax = nUpper;
	m_nMin = nLower;
}

int CToolbarBaseCtrl::SetPos(int nPos)
{
	if (!::IsWindow(m_hWnd))
		return -1;

	int nOldPos = m_nPos;
	m_nPos = nPos;

	CRect rect;
	GetClientRect(rect);

	double Fraction = (double)(m_nPos - m_nMin) / ((double)(m_nMax - m_nMin));
	int nBarWidth = (int)(Fraction * rect.Width());

	if (nBarWidth != m_nBarWidth)
	{
		m_nBarWidth = nBarWidth;
		RedrawWindow();
	}

	return nOldPos;
}

int CToolbarBaseCtrl::StepIt()
{
	return SetPos(m_nPos + m_nStepSize);
}

int CToolbarBaseCtrl::OffsetPos(int nPos)
{
	return SetPos(m_nPos + nPos);
}

int CToolbarBaseCtrl::SetStep(int nStep)
{
	int nOldStep = m_nStepSize;
	m_nStepSize = nStep;
	return nOldStep;
}

void CToolbarBaseCtrl::ShowMyText(char* text, bool show)
{
	//	m_showMyText=show;
	//	if (text!=NULL)
	//		strcpy(m_myText, text);
}

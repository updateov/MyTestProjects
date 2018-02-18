// icon_button.cpp : implementation file
#include "stdafx.h"
#include "icon_button.h"
#include "patterns gui, services.h"

using namespace patterns_gui;

// HHOOK icon_button::m_mouse_hook = 0;
// icon_button
IMPLEMENT_DYNAMIC(icon_button, CWnd)

/*
=======================================================================================================================
=======================================================================================================================
*/
icon_button::icon_button(void)
{
	m_state = qnormal;
	m_icon = "";
}

/*
=======================================================================================================================
=======================================================================================================================
*/
icon_button::~icon_button(void)
{
}

/*
=======================================================================================================================
draw specified text.
=======================================================================================================================
*/
void icon_button::draw_text(CDC *c, long x, long offsety)
{
	CRect r;
	GetClientRect(&r);

	/* Set the device context up. */
	c->SaveDC();
	c->SetBkMode(TRANSPARENT);

	services::select_font(80, "Arial", c);

	/* Set text. */
	CSize t = c->GetTextExtent(get_text());
	c->TextOut(x, offsety + r.Height() / 2 - t.cy / 2, get_text());

	c->RestoreDC(-1);
}

/*
=======================================================================================================================
get icon name.
=======================================================================================================================
*/
string icon_button::get_icon(void)
{
	return m_icon;
}

/*
=======================================================================================================================
get mask name.
=======================================================================================================================
*/
string icon_button::get_mask(void)
{
	return m_mask;
}

/*
=======================================================================================================================
get current button state.
=======================================================================================================================
*/
icon_button::state icon_button::get_state(void)
{
	return m_state;
}

/*
=======================================================================================================================
get displayed text (if any).
=======================================================================================================================
*/
CString icon_button::get_text(void)
{
	/*~~~~~~*/
	CString s;
	/*~~~~~~*/

	GetWindowText(s);
	return s;
}

/*
=======================================================================================================================
Is there a specified icon?
=======================================================================================================================
*/
bool icon_button::has_icon(void)
{
	return m_icon.length() > 0 ? true : false;
}

/*
=======================================================================================================================
Is there a specified text?
=======================================================================================================================
*/
bool icon_button::has_text(void)
{
	return get_text().GetLength() > 0 ? true : false;
}

int icon_button::optimal_width(void)
{
	int width = 0;
	if (has_icon())
	{
		width = services::get_bitmap_rectangle(get_icon()).Width();
	}

	width += 3;

	CPaintDC dc(this);
	services::select_font(80, "Arial", &dc);
	width += dc.GetTextExtent(get_text()).cx;

	width += 2;
	return width;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
BOOL icon_button::OnEraseBkgnd(CDC *)
{
	return TRUE;
}

/*
=======================================================================================================================
change the state on the mouse button down and set mouse capture.
=======================================================================================================================
*/
void icon_button::OnLButtonDown(UINT, CPoint)
{
	SetCapture();
	set_state(qdownin);
}

/*
=======================================================================================================================
change the state on the mouse button up and release capture.
=======================================================================================================================
*/
void icon_button::OnLButtonUp(UINT, CPoint)
{
	ReleaseCapture();

	// send bn_clicked message to its parent if release mouse button on top of the
	// button control.
	if (get_state() != qdownout)
	{
		GetParent()->SendMessage(WM_COMMAND, MAKELONG(GetDlgCtrlID(), BN_CLICKED), (LPARAM) m_hWnd);
	}

	set_state(qnormal);
}

/*
=======================================================================================================================
change the state depending on if the mouse is inside or outside the button region.
=======================================================================================================================
*/
void icon_button::OnMouseMove(UINT, CPoint p0)
{
	/*~~~~*/
	CRect r;
	/*~~~~*/

	GetClientRect(&r);
	if (PtInRect(&r, p0) && get_state() == qdownout)
	{
		set_state(qdownin);
	}
	else if (!PtInRect(&r, p0) && get_state() == qdownin)
	{
		set_state(qdownout);
	}
}

/*
=======================================================================================================================
paint the button depending on its state.
=======================================================================================================================
*/
void icon_button::OnPaint(void)
{
	InvalidateRgn(0);

	PAINTSTRUCT ps;
	CDC* pDC = BeginPaint(&ps);

	CRect r;
	GetClientRect(&r);

	pDC->SetTextColor(RGB(0, 0, 0));

#ifdef OEM_patterns
	pDC->FillRect(r, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
#endif

	if (has_icon())
	{
		r.left += services::get_bitmap_rectangle(get_icon()).Width();
	}

	switch (get_state())
	{
	case qnormal:
	case qdownout:
		if (has_icon())
		{
			services::draw_bitmap(pDC, get_icon(), get_mask(), 0, 0);
		}

		if (has_text())
		{
			draw_text(pDC, r.left + 2, 0);
		}

		break;

	case qdownin:
		if (has_icon())
		{
#ifdef OEM_patterns
			services::draw_bitmap(pDC, get_icon(), get_mask(), 1, 1);
#else
			services::draw_bitmap(pDC, get_icon(), get_mask(), 0, 0);
#endif
		}

		if (has_text())
		{
#ifdef OEM_patterns
			draw_text(pDC, r.left + 3, 1);
#else
			pDC->SetTextColor(RGB(192,192,192));
			draw_text(pDC, r.left + 2, 0);
#endif
		}

		break;

	default:
		if (has_icon())
		{
			services::draw_bitmap(pDC, get_icon(), get_mask(), 0, 0);
		}

		break;
	}

	EndPaint(&ps);
}

/*
=======================================================================================================================
we modify the control style to create an owner draw push button.
=======================================================================================================================
*/
BOOL icon_button::PreCreateWindow(CREATESTRUCT &cs)
{
	if (!CWnd::PreCreateWindow(cs))
	{
		return FALSE;
	}

	cs.dwExStyle |= WS_EX_TRANSPARENT;
	return TRUE;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void icon_button::set_icon(string n)
{
	m_icon = n;
	if (::IsWindow(m_hWnd))
	{
		Invalidate(true);
	}
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void icon_button::set_mask(string n)
{
	m_mask = n;
}

/*
=======================================================================================================================
set view state of the button
=======================================================================================================================
*/
void icon_button::set_state(state s)
{
	if (get_state() != s)
	{
		m_state = s;
		Invalidate();
	}
}

BEGIN_MESSAGE_MAP(icon_button, CWnd)
	ON_WM_ERASEBKGND()
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	ON_WM_PAINT()
END_MESSAGE_MAP()
// icon_button message handlers

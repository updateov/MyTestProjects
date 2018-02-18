// patient list view.cpp : implementation file
#include "stdafx.h"
#include "popup message.h"
#include "patterns gui, services.h"

/*
 =======================================================================================================================
 =======================================================================================================================
 */

popup_message::popup_message(void)
{
	m_format = DT_WORDBREAK;
	m_color = RGB(127, 127, 127);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
popup_message::~popup_message(void)
{
}

BEGIN_MESSAGE_MAP(popup_message, CWnd)
	ON_WM_PAINT()
END_MESSAGE_MAP()

/*
 =======================================================================================================================
    text accessor method.
 =======================================================================================================================
 */
string popup_message::get_text(void)
{
	return m_text;
}

/*
 =======================================================================================================================
    redraw text.
 =======================================================================================================================
 */
void popup_message::OnPaint(void)
{
	CPaintDC dc(this);
	CDC coffscreen;
	CRect r0;

	CRect r;
	GetClientRect(&r);

	// create offscreen dc, draw everything in it and copy its content to the main dc.
	CBitmap boffscreen;

	boffscreen.CreateCompatibleBitmap(&dc, r.Width(), r.Height());

	coffscreen.CreateCompatibleDC(&dc);
	coffscreen.SaveDC();
	coffscreen.SelectObject(&boffscreen);

	// adjust font properties.
	patterns_gui::services::select_font(100, "Arial", &coffscreen);
	coffscreen.SetTextColor(m_color);
	coffscreen.SetBkMode(TRANSPARENT);
	coffscreen.FillRect(r, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));

	r0.left = r.left + 15;
	r0.top = r.top;
	r0.right = r.right - 15;
	r0.bottom = r.bottom;

	if ((m_format & DT_VCENTER) == DT_VCENTER)
	{
		CRect b = r0;
		coffscreen.DrawText(m_text.c_str(), -1, &b, m_format | DT_CALCRECT);
		r0.top += (r0.Height() - b.Height()) / 2;
	}
	if ((r0.top < r0.bottom) && (r0.left < r0.right))
	{
		coffscreen.DrawText(m_text.c_str(), -1, &r0, m_format);
	}

	// copy everthing back to the main dc.
	dc.BitBlt(0, 0, r.Width(), r.Height(), &coffscreen, 0, 0, SRCCOPY);
	boffscreen.DeleteObject();
	coffscreen.RestoreDC(-1);
}

/*
 =======================================================================================================================
    set the text to display.
 =======================================================================================================================
 */
void popup_message::set_text(string t)
{
	m_text = t;
}

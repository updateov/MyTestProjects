// patient list view.cpp : implementation file
#include "stdafx.h"
#include "Patterns Application.h"
#include "patient list view.h"
#include "Patterns, conductor.h"
#include "patterns gui, double buffer.h"

#define RIGHT_BORDER_WIDTH	1

/*
 =======================================================================================================================
 =======================================================================================================================
 */

patient_list_view::patient_list_view(void)
{
	m_patient_list_ctrl.set_conductor(&get_conductor());
	m_patient = "";
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
patient_list_view::~patient_list_view(void)
{
}

BEGIN_MESSAGE_MAP(patient_list_view, CWnd)
	ON_WM_CREATE()
	ON_WM_SIZE()
	ON_WM_PAINT()
	ON_WM_SETFOCUS()
	ON_NOTIFY(LVN_ITEMCHANGED, 100, on_item_changed)
	ON_EN_CHANGE(2, OnDataChange)
END_MESSAGE_MAP()

//
// =======================================================================================================================
//    CInputView message handlers
// =======================================================================================================================
//
void patient_list_view::OnDataChange(void)
{
	if (!UpdateData())
	{
		return;
	}
}

//
// =======================================================================================================================
//    patient_list_view2 drawing
// =======================================================================================================================
//
void patient_list_view::OnDraw(CDC *)
{
}

/* access method - conductor. */
patterns::conductor & patient_list_view::get_conductor (void)
{
	return patterns_application::get_application().get_conductor();
}

/* access method - input adapter. */
patterns::input_adapter & patient_list_view::get_input_adapter (void)
{
	return get_conductor().get_input_adapter();
}

/*
 =======================================================================================================================
    Initial update. Called once before the control is displayed. We create the column here.
 =======================================================================================================================
 */
void patient_list_view::initialize(void)
{
	m_patient_list_ctrl.initialize();
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
int patient_list_view::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CWnd::OnCreate(lpCreateStruct) == -1)
	{
		return -1;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// Create the style
	DWORD dwStyle = WS_CLIPCHILDREN /* | LVS_OWNERDRAWFIXED */ | WS_CHILD | WS_VISIBLE | WS_TABSTOP | LVS_REPORT | LVS_NOCOLUMNHEADER | LVS_SINGLESEL | LVS_EX_FULLROWSELECT;
	// Create the list control. Don't worry about specifying correct coordinates.
	// That will be handled in OnSize()
	BOOL bResult = m_patient_list_ctrl.Create(dwStyle, CRect(0, 0, 0, 0), this, 100);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	return bResult ? 0 : -1;
}

/*
 =======================================================================================================================
    give focus to its child control.
 =======================================================================================================================
 */
void patient_list_view::OnSetFocus(CWnd *)
{
	m_patient_list_ctrl.SetFocus();
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void patient_list_view::on_item_changed(NMHDR *pNMHDR, LRESULT *)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW> (pNMHDR);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (pNMLV && !m_patient_list_ctrl.is_locked())
	{
		if (pNMLV->uNewState == 2 || pNMLV->uNewState == 3)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string *s = (string *) m_patient_list_ctrl.GetItemData(pNMLV->iItem);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (s)
			{
				patterns_application::get_application().set_patient(*s);
				m_patient = *s;
			}
		}
		else if (pNMLV->uOldState == 2 && pNMLV->uChanged == 8)
		{
			patterns_application::get_application().set_patient("");
			m_patient = "";
		}
	}
}

/*
 =======================================================================================================================
    redraw the magnifying glass and maybe other items (futur) that could be part of the patient list view.
 =======================================================================================================================
 */
void patient_list_view::OnPaint(void)
{
	CPaintDC dc(this);
	
	CRect r;
	GetClientRect(&r);

	patterns_gui::double_buffer dbDC(&dc, &r);

	CBrush bwhite = RGB(255, 255, 255);

	dbDC.FillRect(CRect(0, 0, r.Width(), r.Height()), &bwhite);
	dbDC.MoveTo(r.Width() - 1, 0);
	dbDC.LineTo(r.Width() - 1, r.Height());

	DefWindowProc(WM_PAINT, (WPARAM)dbDC.m_hDC, (LPARAM)0);
}

/*
 =======================================================================================================================
    The formview size changed. The list view rect. and the column width is adjusted.
 =======================================================================================================================
 */
void patient_list_view::OnSize(UINT, int cx, int cy)
{
	// resize list view to the size of the formview.
	m_patient_list_ctrl.SetWindowPos(0, 0, 0, cx - RIGHT_BORDER_WIDTH, cy - RIGHT_BORDER_WIDTH, SWP_SHOWWINDOW);
}

/*
 =======================================================================================================================
    modify list style - remove the client edge.
 =======================================================================================================================
 */
BOOL patient_list_view::PreCreateWindow(CREATESTRUCT &cs)
{
	if (!CWnd::PreCreateWindow(cs))
	{
		return FALSE;
	}

	cs.dwExStyle &= ~WS_EX_CLIENTEDGE;
	return TRUE;
}

/*
 =======================================================================================================================
    Change current patient.
 =======================================================================================================================
 */
void patient_list_view::set_patient(const string &id)
{
	// change patient selection and lock any patient list control messages.
	if (id != m_patient)
	{
		m_patient_list_ctrl.select(id);
	}
}
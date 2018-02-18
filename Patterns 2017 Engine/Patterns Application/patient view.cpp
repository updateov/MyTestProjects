//REVIEW: 27/03/14
// patient view.cpp : implementation file
#include "stdafx.h"
#include "Patterns Application.h"
#include "patient view.h"
#include "Patterns, samples.h"
#include "Patterns, fetus.h"
#ifdef patterns_calm_support
#include "calm input adapter.h"
#endif
#include "patterns, fetus.h"

#define MARGIN	8

//
// =======================================================================================================================
//    patient_view
// =======================================================================================================================
//
patient_view::patient_view(void)
{
	m_patient_id = "";

#ifdef patterns_standalone
	m_baseline_variabiliy = true;
#else
	m_baseline_variabiliy = false;
#endif

#ifdef patterns_parer_classification
	m_classification_activated = true;
#endif

	get_conductor().subscribe(new subscription_to_patient_view(this));
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
patient_view::~patient_view(void)
{
	get_conductor().unsubscribe(this);
}

BEGIN_MESSAGE_MAP(patient_view, CWnd)
	ON_WM_CREATE()
	ON_WM_SIZE()
	ON_WM_SETFOCUS()
	ON_CONTROL(patterns_gui::tracing::ndeleteevent, 100, on_strike_out_event)
	ON_CONTROL(patterns_gui::tracing::ndeletecontraction, 100, on_strike_out_contraction)
	ON_CONTROL(patterns_gui::tracing::nacceptevent, 100, on_accept_event)
	
END_MESSAGE_MAP()
/* access method - conductor. */
patterns::conductor & patient_view::get_conductor (void)
{
	return patterns_application::get_application().get_conductor();
}

/* access method - input adapter. */
patterns::input_adapter & patient_view::get_input_adapter (void)
{
	return get_conductor().get_input_adapter();
}

/*
 =======================================================================================================================
    get dword registry value from its subkey name and value name.
 =======================================================================================================================
 */
long patient_view::get_dword_reg_value(string subkey, string valuename) const
{
	/*~~~~~~~~~~~~~~*/
	long v = LONG_MIN;
	HKEY h = NULL;
	/*~~~~~~~~~~~~~~*/

	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, subkey.c_str(), 0, KEY_READ, &h) == ERROR_SUCCESS)
	{
		/*~~~~*/
		DWORD t;
		DWORD s;
		DWORD p;
		/*~~~~*/

		t = REG_DWORD;
		s = sizeof(DWORD);
		if (RegQueryValueEx(h, TEXT(valuename.c_str()), NULL, &t, (PBYTE) & p, &s) == ERROR_SUCCESS)
			v = p;
		RegCloseKey(h);
	}

	return v;
}

/* return tracing reference. */
patterns_gui::tracing & patient_view::get_tracing (void)
{
	return m_Tracing;
}

/*
 =======================================================================================================================
    do we have access to the debug keys?
 =======================================================================================================================
 */
bool patient_view::has_debug_keys(void)
{
	return m_Tracing.has_debug_keys();
}

/*
 =======================================================================================================================
    Baseline variability feature. 
 =======================================================================================================================
 */
bool patient_view::is_baseline_variability_activated(void)
{
	return m_baseline_variabiliy;
}

/*
 =======================================================================================================================
    Baseline variability feature. 
 =======================================================================================================================
 */
void patient_view::is_baseline_variability_activated(bool value)
{
	m_baseline_variabiliy = value;
	m_Tracing.show(patterns_gui::tracing::wbaselinevariability, is_baseline_variability_activated());
}


#ifdef patterns_parer_classification
/*
 =======================================================================================================================
    Overall classification
 =======================================================================================================================
 */
bool patient_view::is_classification_activated(void)
{
	return m_classification_activated;
}

/*
 =======================================================================================================================
    Overall classification. 
 =======================================================================================================================
 */
void patient_view::is_classification_activated(bool value)
{
	m_classification_activated = value;
	m_Tracing.show(patterns_gui::tracing::wparerclassification, is_classification_activated());
}
#endif

/*
 =======================================================================================================================
    on the creation of the view, we create the tracing control and set its default values.
 =======================================================================================================================
 */
int patient_view::OnCreate(LPCREATESTRUCT)
{
	m_Tracing.DisableToggleZoom();
	// Create tracings windows.
	m_Tracing.Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 100);

	m_Tracing.set_type(patterns_gui::tracing::tnormal);
	m_Tracing.set_paper(patterns_gui::tracing::pusa);
	m_Tracing.set_scaling_mode(patterns_gui::tracing::spaper);
	m_Tracing.show_grid();
	m_Tracing.lock_scaling();
	m_Tracing.set_lengths(TRACING_APP_VIEW_SIZE_SEC, COMPRESS_VIEW_SIZE_SEC, SLIDER_VIEW_SIZE_SEC);
	m_Tracing.show(patterns_gui::tracing::winformation);
	m_Tracing.set_can_delete();
	m_Tracing.show(patterns_gui::tracing::wbaselinevariability, is_baseline_variability_activated());

#ifdef patterns_parer_classification
	m_Tracing.show(patterns_gui::tracing::wparerclassification, false);
#endif

	m_Tracing.show(patterns_gui::tracing::wdecellag, false);

	return true;
}

/*
 =======================================================================================================================
    strike out of an event. Message sends by the tracing control when the user click the strike out event button.
 =======================================================================================================================
 */
void patient_view::on_strike_out_event(void)
{
#if defined(patterns_retrospective)
	if (m_Tracing.get_selection_type() != patterns_gui::tracing::cevent)
	{
		return;
	}

	const patterns::event& evt = m_Tracing.get()->get_event(m_Tracing.get_selection());
	bool undo = evt.is_strike_out();

	CString s;
	s.LoadString(undo?patient_view_restore_event:patient_view_delete_event);

	if (MessageBox(s, patterns_application::get_application().get_product_name().c_str(), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDYES)
	{
		(const_cast<event&>(evt)).set_as_strike_out(!undo);
		m_Tracing.get()->note(subscription::mfetus);
	}
#else
	/*~~~~~~~~*/
	CString str;
	/*~~~~~~~~*/

	str.LoadString(NULL, patient_view_delete_event);
	if (MessageBox(str, patterns_application::get_application().get_product_name().c_str(), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDYES)
	{
		m_Tracing.get()->strike_event_out(m_Tracing.get_selection());
		m_Tracing.deselect();
	}

#endif
}

/*
 =======================================================================================================================
    accept a non interpretable event.
 =======================================================================================================================
 */
void patient_view::on_accept_event(void)
{
	/*~~~~~~~~*/
	CString str;
	/*~~~~~~~~*/

	str.LoadString(NULL, patient_view_accept_event);
	if (MessageBox(str, patterns_application::get_application().get_product_name().c_str(), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDYES)
	{
		m_Tracing.get()->accept_event(m_Tracing.get_selection());
	}
}


/*
 =======================================================================================================================
    strike out of an event. Message sends by the tracing control when the user click the strike out event button.
 =======================================================================================================================
 */
void patient_view::on_strike_out_contraction(void)
{
#if defined(patterns_retrospective)
	if (m_Tracing.get_selection_type() != patterns_gui::tracing::ccontraction)
	{
		return;
	}

	const patterns::contraction& ctr = m_Tracing.get()->get_contraction(m_Tracing.get_selection());
	bool undo = ctr.is_strike_out();

	CString s;
	s.LoadString(undo?patient_view_restore_contraction:patient_view_delete_contraction);

	if (MessageBox(s, patterns_application::get_application().get_product_name().c_str(), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDYES)
	{
		(const_cast<contraction&>(ctr)).set_as_strike_out(!undo);
		m_Tracing.get()->ResetContracionRate();
		m_Tracing.get()->note(subscription::mfetus);
	}
#else
	/*~~~~~~~~*/
	CString str;
	/*~~~~~~~~*/

	str.LoadString(NULL, patient_view_delete_contraction);
	if (MessageBox(str, patterns_application::get_application().get_product_name().c_str(), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDYES)
	{
		m_Tracing.get()->strike_contraction_out(m_Tracing.get_selection());
		m_Tracing.deselect();
	}
#endif
}


/*
 =======================================================================================================================
    give focus to its child control.
 =======================================================================================================================
 */
void patient_view::OnSetFocus(CWnd *)
{
	m_Tracing.SetFocus();
}

/*
 =======================================================================================================================
    repositionning of the content of the patient view - tracing
 =======================================================================================================================
 */
void patient_view::OnSize(UINT, int, int)
{
	/*~~~~~~~~~~~~~~*/
	// Create tracing windows.
	CRect rect_window;
	/*~~~~~~~~~~~~~~*/

	GetWindowRect(&rect_window);
	ScreenToClient(&rect_window);

	// remove properties portion...
	rect_window.bottom = rect_window.bottom;

	/*~~~~~~~~~~~~~~~~~~~~~*/
	// FHR tracing
	CRect rect = rect_window;
	/*~~~~~~~~~~~~~~~~~~~~~*/

	m_Tracing.SetWindowPos(0, rect_window.left, rect_window.top, rect_window.Width(), rect_window.Height(), SWP_SHOWWINDOW);
	m_Tracing.refresh_layout();
}

/*
 =======================================================================================================================
    remove client edge of the view.
 =======================================================================================================================
 */
BOOL patient_view::PreCreateWindow(CREATESTRUCT &cs)
{
	if (!CWnd::PreCreateWindow(cs))
	{
		return FALSE;
	}

	// remove client edge.
	cs.style &= ~WS_EX_CLIENTEDGE;
	cs.dwExStyle &= ~WS_EX_CLIENTEDGE;
	return TRUE;
}

/*
 =======================================================================================================================
    set given patient to the view.
 =======================================================================================================================
 */
void patient_view::set_patient(const string &id)
{
	if (m_patient_id != id)
	{
		if ((m_patient_id.length() > 0) && (get_conductor().is_known(m_patient_id)))
		{
			get_conductor().get(m_patient_id).unsubscribe(this);
		}

		m_patient_id = id;
		if ((m_patient_id.length() > 0) && (get_conductor().is_known(m_patient_id)))
		{
			fetus& f = get_conductor().get(m_patient_id);
	
			f.subscribe(new subscription_to_patient_view(this));

			m_Tracing.set(&f);

			// we do not preserve the position for each patient yet. this gone be implemented
			// in futur release. For now, when switching patient, we move the slider window at
			// the end.
			m_Tracing.move_to(f.get_number_of_fhr());
		}
		else
		{
			m_Tracing.set(0);
			m_Tracing.move_to(0);
		}
	}

	update_data();
}

/*
 =======================================================================================================================
    toggle on/off baseline display.
 =======================================================================================================================
 */
void patient_view::toggle_baseline_display(void)
{
	m_Tracing.show(patterns_gui::tracing::wbaselines, !m_Tracing.is_visible(patterns_gui::tracing::wbaselines));
}

/*
 =======================================================================================================================
    toggle tracing debug keys access.
 =======================================================================================================================
 */
void patient_view::toggle_debug_keys_access(void)
{
	m_Tracing.set_has_debug_keys(!has_debug_keys());
}

/*
 =======================================================================================================================
    update data in the patient view. The data is updated on demand.
 =======================================================================================================================
 */
void patient_view::update_data(void)
{
	// we set to the tracing the starting valid date and the message to display
	// before that data. The events are replaced by a message for points of the
	// tracing before 36 weeks of gestation (GA).
	date d = undetermined_date;

	patterns::input_adapter::patient* p = get_input_adapter().get_patient(m_patient_id);
	if (p != 0)
	{
		d = p->get_edc();
	}

	if (d != undetermined_date)
	{
		// Calculate the time edc - 4 weeks
		d = (d - (60 * 60 * 24 * 28)) + (2 * 60 * 60); // The 2 hours is there to fix the possible daylight saving issue where a day is 23 hours OR 25

		SYSTEMTIME st = fetus::convert_to_SYSTEMTIME(d);

		// Round it so that it's sure at midnight
		st.wHour = 0;
		st.wMinute = 0;
		st.wSecond = 0;
		st.wMilliseconds = 0;

		// Done
		d = fetus::convert_to_utc(fetus::convert_to_time_t(st));
	}

	m_Tracing.get()->set_cutoff_date(d);
}

/*
 =======================================================================================================================
    Receive messages through subscription.
 =======================================================================================================================
 */
void patient_view::subscription_to_patient_view::note(message m)
{
	switch (m)
	{
		case mpatientlist:
			// Update the display
			m_patient_view->update_data();
			break;
	}
}

// main view.cpp : implementation of the main_view class
#include "stdafx.h"
#include "main view.h"

#include "Patterns Application.h"

#include "patterns gui, double buffer.h"
#include "patterns gui, services.h"
#include "about_dlg.h"

#include <Psapi.h>

using namespace patterns_gui;

HWND main_view:: m_caller_window = 0;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
#define TOP_HEIGHT				0
#define BANNER_HEIGHT			33
#define BOTTOM_HEIGHT			30
#define PATIENT_LIST_WIDTH		250
#define INTENT_TO_USE_HEIGHT	40
#define INTENT_TO_USE_TIMER		2
#define COMMIT_EVENTS_TIMER		6
#define NAV_BUTTONS_WIDTH		18
#define CONTROL_HEIGHT			18
#define CONTROLS_SEPARATOR		3

// =====================================================================================================================
//    main_view
// =====================================================================================================================
main_view::main_view(void)
{
	m_bInitialized = false;
	m_intent_to_use_timer_id = 0;
	m_commit_events_id = 0;
	bplayback = bplayforward = false;
	m_t_zoomdown = 0;
	get_conductor().subscribe(new subscription_to_main_view(this));
	m_is_patient_list_pinned = false;
}

main_view::~main_view(void)
{
	get_conductor().unsubscribe(this);

	if (m_commit_events_id)
	{
		KillTimer(m_commit_events_id);
	}

	if (m_intent_to_use_timer_id)
	{
		KillTimer(m_intent_to_use_timer_id);
	}

	m_bInitialized = false;

	services::forget_all_bitmaps();
}

BEGIN_MESSAGE_MAP(main_view, CWnd)
	ON_WM_PAINT()
	ON_WM_SIZE()
	ON_WM_TIMER()
	ON_BN_CLICKED(5, about)
	ON_BN_CLICKED(15, go_beginning)
	ON_BN_CLICKED(20, go_end)
	ON_BN_CLICKED(25, go_next_page)
	ON_BN_CLICKED(45, go_previous_page)
	ON_BN_CLICKED(30, pause)
	ON_BN_CLICKED(35, play_back)
	ON_BN_CLICKED(40, play_forward)
	ON_BN_CLICKED(55, show_patient_list)
	ON_BN_CLICKED(100, toggle_events)
	ON_BN_CLICKED(105, toggle_montevideo)
	ON_BN_CLICKED(103, toggle_baseline)
	ON_BN_CLICKED(102, toggle_toco)
	ON_BN_CLICKED(61, toggle_zoom)
	ON_BN_CLICKED(62, open_file)
END_MESSAGE_MAP()

// =====================================================================================================================
//    open patient list
// =======================================================================================================================
void main_view::open_patient_list(void)
{
#ifndef patterns_viewer
	if (!m_is_patient_list_pinned && !get_patient_list_view().IsWindowVisible())
	{
		get_patient_list_view().ShowWindow(SW_SHOW);
	}

	get_patient_list_view().SetFocus();
#endif
}

// =====================================================================================================================
//    close patient list
// =======================================================================================================================
void main_view::close_patient_list(void)
{
#ifndef patterns_viewer
	if (!m_is_patient_list_pinned && get_patient_list_view().IsWindowVisible())
	{
		get_patient_list_view().ShowWindow(SW_HIDE);
	}
#endif
}

// =====================================================================================================================
//    close/open patient list
// =======================================================================================================================
void main_view::toggle_patient_list(void)
{
#ifndef patterns_viewer
	if (get_patient_list_view().IsWindowVisible())
	{
		close_patient_list();
	}
	else
	{
		open_patient_list();
	}
#endif
}

// =====================================================================================================================
//    draw control bars gradient background
// =======================================================================================================================
void main_view::draw_control_bar(CDC* dc, const char* name, long x, long y)
{
	CBitmap b;
	BITMAP bdetails;
	CDC dc2;
	CRect r;

	GetClientRect(&r);

	dc2.CreateCompatibleDC(dc);
	dc2.SaveDC();
	b.LoadBitmap(name);
	b.GetBitmap(&bdetails);
	dc2.SelectObject(&b);

	long n = (r.Width() - x) / bdetails.bmWidth + 1;

	for (long i = x; i < n; ++i)
	{
		dc->BitBlt(i * bdetails.bmWidth, y, bdetails.bmWidth, bdetails.bmHeight, &dc2, 0, 0, SRCCOPY);
	}

	dc2.RestoreDC(-1);
}

// =====================================================================================================================
//    fit given text to the given width of the dc. If the text doesn't fit, the text is cut and ... is put at the end. If
//    there is empty space, we fill using the given character.
// =======================================================================================================================
string main_view::fit_text_to_width(string s, CDC* dc, long w, char c)
{
	string s0 = s;
	CSize size = dc->GetTextExtent(s.c_str(), (long)s.size());
	long n = 4;

	while (size.cx > w)
	{
		if ((long)s.size() <= n)
		{
			break;
		}

		s0 = s;
		s0 = s0.substr(0, (long)s0.size() - n);
		s0 += "...";
		size = dc->GetTextExtent(s0.c_str(), (long)s0.size());
		n++;
	}

	CString cs;

	cs = c;

	CSize charspace = dc->GetTextExtent(cs);
	long tofill = w - size.cx;
	long toadd = (long)(tofill / charspace.cx);

	for (long i = 0; i < toadd; ++i)
	{
		s0 += " ";
	}

	return s0;
}

// =====================================================================================================================
//    draw patient banner. Adjust text to window width.
// =======================================================================================================================
void main_view::draw_patient_banner(CDC* dc, long, long)
{
	CRect r;

	GetClientRect(&r);

	// Set the device context up.
	dc->SaveDC();
	dc->SetBkMode(TRANSPARENT);
	dc->SetTextColor(RGB(101, 92, 62));
	services::select_font(80, "Arial Bold", dc);

	string s;
	long w = r.Width() - 10;

	// No patient
	if (m_banner.size() == 1)
	{
		s += m_banner[0];
	}

	else 
	{
		ASSERT(m_banner.size() >= 7);

		w -= 60 + 115 + 75;

		s += fit_text_to_width(m_banner[0], dc, (long)(w * 3.00 / 10));		// Name
		s += fit_text_to_width(m_banner[1], dc, (long)(w * 2.25 / 10));		// Patient ID
		s += fit_text_to_width(m_banner[2], dc, (long)(w * 2.25 / 10));		// Account No
		s += fit_text_to_width(m_banner[3], dc, 60);						// Age
		s += fit_text_to_width(m_banner[4], dc, 115);						// EDD
		s += fit_text_to_width(m_banner[5], dc, 75);						// GA
		s += fit_text_to_width(m_banner[6], dc, (long)(w * 2.50 / 10));		// Bed
	}

	CSize size = dc->GetTextExtent("h", 1);

	dc->TextOut(5, BANNER_HEIGHT / 2 - size.cy / 2, s.c_str());

	// Restore device context.
	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    conductor accessor method.
// =======================================================================================================================
conductor& main_view::get_conductor(void)
{
	return patterns_application::get_application().get_conductor();
}

patient_list_view& main_view::get_patient_list_view(void)
{
	return m_p_list_view;
}

patient_view& main_view::get_patient_view(void)
{
	return m_p_view;
}

// =====================================================================================================================
//    initialize view content. create splitter and his two views (patient_list_view and patient_view)
// =======================================================================================================================
void main_view::initialize(void)
{
	get_button("open patient list")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 55);
	get_button("zoom")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 61);

	get_button("open file")->Create(NULL, NULL, WS_CHILD | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 62);

	get_button("go to beginning")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 15);
	get_button("page left")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 45);
	get_button("play left")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 35);
	get_button("pause")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 30);
	get_button("play right")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 40);
	get_button("page right")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 25);
	get_button("go to end")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(20, 20, 20, 20), this, 20);

	get_button("baseline")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 103);
	get_button("toco")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 102);
	get_button("events")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 100);
	get_button("montevideo")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 105);

	get_button("about")->Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 5);

	m_p_list_view.CreateEx(WS_EX_TOPMOST, NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 60);
	m_p_list_view.initialize();
	m_p_view.Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 10, 10), this, 65);
	m_intent_to_use_view.Create(NULL, NULL, WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, CRect(0, 0, 1, 1), this, 85);

	CString s0;

#if defined(patterns_retrospective)
	s0 = "For training, evaluation and/or quality assurance purposes only. Not for use in clinical care.";
#else
	s0.LoadString(NULL, intent_to_use_text);
#endif

	m_intent_to_use_view.set_text(s0.GetBuffer(s0.GetLength()));

	// set ressource name used for the navigation buttons
	get_button("go to beginning")->set_icon("leftmost");
	get_button("page left")->set_icon("previouspage");
	get_button("play left")->set_icon("left");
	get_button("pause")->set_icon("pause");
	get_button("play right")->set_icon("right");
	get_button("page right")->set_icon("nextpage");
	get_button("go to end")->set_icon("rightmost");

	// fragment navigation bitmaps.
	CRect r0;

	// fragment navigation bitmaps.
	CRect r = services::get_bitmap_rectangle("NAVIGATIONBUTTONS");

	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "patient list", CRect(0, 0, r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "zoom", CRect(r.Height(), 0, 2 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "about", CRect(4 * r.Height(), 0, 5 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "mask button", CRect(5 * r.Height(), 0, 6 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "action on", CRect(10 * r.Height(), 0, 11 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "action off", CRect(11 * r.Height(), 0, 12 * r.Height(), r.Height()));
	services::create_bitmap_fragment("NAVIGATIONBUTTONS", "open file", CRect(12 * r.Height(), 0, 13 * r.Height(), r.Height()));

	get_button("about")->SetWindowText("About");
	get_button("about")->set_icon("about");
	get_button("about")->set_mask("mask button");

	get_button("open file")->SetWindowText("Open");
	get_button("open file")->set_icon("open file");
	get_button("open file")->set_mask("mask button");

	get_button("open patient list")->SetWindowText("Patient list");
	get_button("open patient list")->set_icon("patient list");
	get_button("open patient list")->set_mask("mask button");

	get_button("baseline")->SetWindowText("Baselines");
	get_button("baseline")->set_icon("action on");
	get_button("baseline")->set_mask("mask button");

	get_button("toco")->SetWindowText("TOCO Graph");
	get_button("toco")->set_icon("action on");
	get_button("toco")->set_mask("mask button");

	get_button("events")->SetWindowText("Acc/Decel");
	get_button("events")->set_icon("action on");
	get_button("events")->set_mask("mask button");

	get_button("montevideo")->SetWindowText("Montevideo");
	get_button("montevideo")->set_icon("action off");
	get_button("montevideo")->set_mask("mask button");

	get_button("zoom")->SetWindowText("Zoom");
	get_button("zoom")->set_icon("zoom");
	get_button("zoom")->set_mask("mask button");

	// keep the this pointer in the user data of the window. this is gone be uses in the patient list timer and also
	// in the DialogProc of the login dialog.
	SetWindowLong(m_hWnd, GWL_USERDATA, (LONG) this);

#ifdef patterns_has_signal_processing
	m_commit_events_id = SetTimer(COMMIT_EVENTS_TIMER, 2000, 0);
#endif
	update_config();

	m_bInitialized = true;
}

// =====================================================================================================================
//    about box.
// =======================================================================================================================
void main_view::about(void)
{
	about_dlg d(AfxGetApp()->m_pMainWnd);
	d.DoModal();
}

void main_view::toggle_events(void)
{
	bool event_on = fetus::get_event_detection_enabled();
	patterns_gui::tracing & t = get_patient_view().get_tracing();
	t.show(patterns_gui::tracing::whideevents, !event_on || !t.is_visible(patterns_gui::tracing::whideevents));

	reset_zoom();
	update_events_button();
}

void main_view::toggle_toco(void)
{
	patterns_gui::tracing & t = get_patient_view().get_tracing();
	t.show(patterns_gui::tracing::wcrtracing, !t.is_visible(patterns_gui::tracing::wcrtracing));

	reset_zoom();
	update_toco_button();
}

void main_view::toggle_baseline(void)
{
	patterns_gui::tracing & t = get_patient_view().get_tracing();
	t.show(patterns_gui::tracing::wbaselines, !t.is_visible(patterns_gui::tracing::wbaselines));

	reset_zoom();
	update_baseline_button();
}

void main_view::toggle_montevideo(void)
{
	bool montevideo_on = patterns_application::get_application().is_enable_Montevideo();

	patterns_gui::tracing & t = get_patient_view().get_tracing();
	t.show(patterns_gui::tracing::wmontevideo, montevideo_on && !t.is_visible(patterns_gui::tracing::wmontevideo));

	reset_zoom();
	update_montevideo_button();
}

// =====================================================================================================================
//    tracing navigation. Go to the beginning of the tracing.
// =======================================================================================================================
void main_view::go_beginning(void)
{
	get_patient_view().get_tracing().move_to(0);
}

// =====================================================================================================================
//    tracing navigation. Go to the end of the tracing.
// =======================================================================================================================
void main_view::go_end(void)
{
	get_patient_view().get_tracing().move_to(get_patient_view().get_tracing().get()->get_number_of_fhr() + 100);
	get_patient_view().get_tracing().scroll(true);
}

// =====================================================================================================================
//    tracing navigation. Go to the next page A page represent the width of the view.
// =======================================================================================================================
void main_view::go_next_page(void)
{
	if (get_patient_view().get_tracing().is_animating())
	{
		get_patient_view().get_tracing().stop_animating();
	}

	get_patient_view().get_tracing().move_page_animating(false);
}

// =====================================================================================================================
//    tracing navigation. Go to the previous page A page represent the width of the view.
// =======================================================================================================================
void main_view::go_previous_page(void)
{
	if (get_patient_view().get_tracing().is_animating())
	{
		get_patient_view().get_tracing().stop_animating();
	}

	get_patient_view().get_tracing().move_page_animating();
}

// =====================================================================================================================
//    move current view by the given offset.
// =======================================================================================================================
void main_view::move(long d)
{
	get_patient_view().get_tracing().set_offset(get_patient_view().get_tracing().get_offset() + d);
}

// =====================================================================================================================
//    pause animation if moving to the left or right.
// =======================================================================================================================
void main_view::pause(void)
{
	get_patient_view().get_tracing().stop_animating();
}

// =====================================================================================================================
//    tracing navigation. play back.
// =======================================================================================================================
void main_view::play_back(void)
{
	if (get_patient_view().get_tracing().is_animating() && bplayforward)
	{
		get_patient_view().get_tracing().stop_animating();
		bplayforward = false;
	}

	if (get_patient_view().get_tracing().is_animating())
	{
		get_patient_view().get_tracing().set_animation_rate(get_patient_view().get_tracing().get_animation_rate() * 2);
	}
	else
	{
		get_patient_view().get_tracing().set_animation_rate(1);
		get_patient_view().get_tracing().move_to_animating(0);
	}

	bplayback = true;
}

// =====================================================================================================================
//    tracing navigation. play forward.
// =======================================================================================================================
void main_view::play_forward(void)
{
	if (get_patient_view().get_tracing().is_animating() && bplayback)
	{
		get_patient_view().get_tracing().stop_animating();
		bplayback = false;
	}

	if (get_patient_view().get_tracing().is_animating())
	{
		get_patient_view().get_tracing().set_animation_rate(get_patient_view().get_tracing().get_animation_rate() * 2);
	}
	else
	{
		get_patient_view().get_tracing().set_animation_rate(1);
		get_patient_view().get_tracing().move_to_animating(get_patient_view().get_tracing().get()->get_number_of_fhr());
	}

	bplayforward = true;
}

// =====================================================================================================================
//    quit application.
// =======================================================================================================================
void main_view::quit(void)
{
	if (m_bInitialized)
	{
		AfxGetMainWnd()->PostMessage(WM_CLOSE, 0, 0);
	}
}

// =====================================================================================================================
//    resize view content.
// =======================================================================================================================
void main_view::OnSize(UINT t, int width, int height)
{
	CWnd::OnSize(t, width, height);

	if (m_bInitialized && GetSafeHwnd())
	{

		long top = height - (BOTTOM_HEIGHT + CONTROL_HEIGHT) / 2;

#if defined(patterns_viewer) || defined(OEM_patterns)
		bool show_patient_list_button = false;
#else
		bool show_patient_list_button = !m_is_patient_list_pinned;
#endif
		m_p_view.SetWindowPos(&m_intent_to_use_view, m_is_patient_list_pinned ? PATIENT_LIST_WIDTH : 0, TOP_HEIGHT + BANNER_HEIGHT, width - (m_is_patient_list_pinned ? PATIENT_LIST_WIDTH : 0), height - TOP_HEIGHT - BOTTOM_HEIGHT - BANNER_HEIGHT, 0);
		m_p_list_view.SetWindowPos(0, 0, BANNER_HEIGHT, PATIENT_LIST_WIDTH, height - BOTTOM_HEIGHT - BANNER_HEIGHT, m_is_patient_list_pinned ? SWP_SHOWWINDOW : SWP_HIDEWINDOW);

		// BUTTONS on the command bar
		get_button("open patient list")->SetWindowPos(0, 10, top, 90, CONTROL_HEIGHT, show_patient_list_button ? SWP_SHOWWINDOW : SWP_HIDEWINDOW);
		get_button("zoom")->SetWindowPos(0, show_patient_list_button ? 100 : 10, top, 50, CONTROL_HEIGHT, 0);

#if defined(patterns_retrospective) && !defined(OEM_patterns)
		get_button("open file")->SetWindowPos(0, 165, top, 90, CONTROL_HEIGHT, SWP_SHOWWINDOW);
#endif patterns_retrospective

		get_button("toco")->SetWindowPos(0, width - 490, top, 90, CONTROL_HEIGHT, 0);
		get_button("baseline")->SetWindowPos(0, width - 400, top, 70, CONTROL_HEIGHT, 0);
		get_button("events")->SetWindowPos(0, width - 320, top, 70, CONTROL_HEIGHT, 0);
		get_button("montevideo")->SetWindowPos(0, width - 240, top, 90, CONTROL_HEIGHT, 0);
		get_button("about")->SetWindowPos(0, width - 60, top, 60, CONTROL_HEIGHT, 0);

		m_intent_to_use_view.SetWindowPos(&m_p_list_view, m_is_patient_list_pinned ? PATIENT_LIST_WIDTH : 0, height - INTENT_TO_USE_HEIGHT - BOTTOM_HEIGHT, width - (m_is_patient_list_pinned ? PATIENT_LIST_WIDTH : 0), INTENT_TO_USE_HEIGHT, SWP_NOACTIVATE);

		long left = ((show_patient_list_button ? 100 : 30) + (width - 320 - (NAV_BUTTONS_WIDTH * 7 + CONTROLS_SEPARATOR * 6))) / 2;

#ifdef patterns_retrospective
		left -= 30;
#endif
		get_button("go to beginning")->SetWindowPos(0, left, top, NAV_BUTTONS_WIDTH, CONTROL_HEIGHT, 0);
		left += NAV_BUTTONS_WIDTH + CONTROLS_SEPARATOR;
		get_button("page left")->SetWindowPos(0, left, top, NAV_BUTTONS_WIDTH, CONTROL_HEIGHT, 0);
		left += NAV_BUTTONS_WIDTH + CONTROLS_SEPARATOR;
		get_button("play left")->SetWindowPos(0, left, top, NAV_BUTTONS_WIDTH, CONTROL_HEIGHT, 0);
		left += NAV_BUTTONS_WIDTH + CONTROLS_SEPARATOR;
		get_button("pause")->SetWindowPos(0, left, top, NAV_BUTTONS_WIDTH, CONTROL_HEIGHT, 0);
		left += NAV_BUTTONS_WIDTH + CONTROLS_SEPARATOR;
		get_button("play right")->SetWindowPos(0, left, top, NAV_BUTTONS_WIDTH, CONTROL_HEIGHT, 0);
		left += NAV_BUTTONS_WIDTH + CONTROLS_SEPARATOR;
		get_button("page right")->SetWindowPos(0, left, top, NAV_BUTTONS_WIDTH, CONTROL_HEIGHT, 0);
		left += NAV_BUTTONS_WIDTH + CONTROLS_SEPARATOR;
		get_button("go to end")->SetWindowPos(0, left, top, NAV_BUTTONS_WIDTH, CONTROL_HEIGHT, 0);
	}
}

// =====================================================================================================================
//    TEMP
// =======================================================================================================================
void main_view::show_patient_list(void)
{
	string msg;
	if (patterns_application::get_application().is_patient_list_shown(msg))
	{
		toggle_patient_list();
	}
	else if (msg.length() > 0)
	{
		MessageBox(msg.c_str(), patterns_application::get_application().get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
	}
}

// =====================================================================================================================
//    repaint window - control bars
// =======================================================================================================================
void main_view::OnPaint(void)
{
	CPaintDC dc(this);
	CRect r;

	GetClientRect(&r);
	double_buffer dbDC(&dc, &r);

	draw_control_bar(&dbDC, "banner", 0, r.Height() - BOTTOM_HEIGHT);
	draw_control_bar(&dbDC, "banner", 0, 0);
	draw_patient_banner(&dbDC, 0, 0);

	DefWindowProc(WM_PAINT, (WPARAM)dbDC.m_hDC, (LPARAM)0);
}

// =====================================================================================================================
//    ontimer: uses to animate the patient list window.
// =======================================================================================================================
void main_view::OnTimer(UINT_PTR k)
{
#ifdef patterns_has_signal_processing
	if (k == m_commit_events_id)
	{
		get_conductor().commit_pending_calculations();
	}
#endif
	if (k == m_intent_to_use_timer_id)
	{
		// Hide the intent to use message
		KillTimer(m_intent_to_use_timer_id);
		m_intent_to_use_timer_id = 0;

		display_intent_to_use_message(false);
	}
}

// =====================================================================================================================
//    the current patient has changed. Update patient banner.
// =======================================================================================================================
void main_view::set_patient(string id)
{
	// Reset montevideo unit if it's on and we change the patient
	if ((m_patient != id) && (get_patient_view().get_tracing().is_visible(patterns_gui::tracing::wmontevideo)))
	{
		get_patient_view().get_tracing().show(patterns_gui::tracing::wmontevideo, false);
		update_montevideo_button();
	}

	m_patient = id;
	update_data();
}

// =====================================================================================================================
//    Display/hide the intent to use... warning message
// =======================================================================================================================
void main_view::display_intent_to_use_message(bool bDisplay)
{
	m_intent_to_use_view.ShowWindow(bDisplay ? SW_SHOW : SW_HIDE);

	if (m_intent_to_use_timer_id != 0)
	{
		KillTimer(m_intent_to_use_timer_id);
		m_intent_to_use_timer_id = 0;
	}

	if (bDisplay)
	{
		m_intent_to_use_timer_id = SetTimer(INTENT_TO_USE_TIMER, 20000, 0);
	}
}

// =====================================================================================================================
//    update patient banner.
// =======================================================================================================================
void main_view::update_data(void)
{
	if (!patterns_application::get_application().get_conductor().has_input_adapter())
	{
		return;
	}

	vector<string> s;
	string p0;

	patterns::input_adapter::patient * p = get_conductor().get_input_adapter().get_patient(m_patient);

	if ((p != 0) && ((long)p->get_key().size() > 0))
	{
		p0 = "Name: " + p->get_surname();
		p0 += " ";
		p0 += p->get_name();
		s.push_back(p0);

		p0 = " | Pt ID: ";
		p0 += p->get_id();

		s.push_back(p0);

		p0 = " | Acc #: ";
		p0 += p->get_accountno();
		s.push_back(p0);

		p0 = " | Age: ";
		p0 += p->get_age();
		s.push_back(p0);

		date u = undetermined_date;

		p0 = " | EDD: ";
		if (p->get_edc_string().length() > 0)
		{
			p0 += p->get_edc_string();
		}
		else if (p->get_edc() == undetermined_date)
		{
			p0 += "";
		}
		else if (p->get_edc() == u)
		{
			p0 += "?";
		}
		else
		{
			p0 += services::date_to_string(p->get_edc(), services::fdate);
		}

		s.push_back(p0);

		p0 = " | GA: ";
		p0 += p->get_gestational_age();
		s.push_back(p0);

		p0 = " | ";
		p0 += p->get_bed();
		s.push_back(p0);
	}
	else
	{
		CString s0;

		s0.LoadString(NULL, patient_view_no_patient_selected);
		p0 = s0;
		s.push_back(p0);
	}

	if (m_banner != s)
	{
		m_banner = s;

		CRect r;

		GetClientRect(&r);
		r.bottom = BANNER_HEIGHT;
		InvalidateRect(&r);
	}
}

// =====================================================================================================================
//    zoom down/up. Zoom the 2-hour compressed view.
// =======================================================================================================================
void main_view::zoom_down(void)
{
	if (m_t_zoomdown == 0)
	{
		m_t_zoomdown = clock();
		get_patient_view().get_tracing().zoom(!get_patient_view().get_tracing().is_zoomed());
	}
}

void main_view::zoom_up(void)
{
	if (m_t_zoomdown != 0)
	{
		if ((clock() - m_t_zoomdown) / CLOCKS_PER_SEC > 1)
		{
			get_patient_view().get_tracing().zoom(false);
		}

		m_t_zoomdown = 0;
	}
}

void main_view::toggle_zoom(void)
{
	get_patient_view().get_tracing().zoom(!get_patient_view().get_tracing().is_zoomed());
	m_t_zoomdown = 0;
}

void main_view::reset_zoom(void)
{
	get_patient_view().get_tracing().zoom(false);
	m_t_zoomdown = 0;
}

void main_view::open_file(void)
{
	patterns_application::get_application().on_debug_open();
}

void main_view::refresh_command_bar()
{
	Invalidate(TRUE);
	for (map < string, icon_button * >::iterator i = m_buttons.begin(); i != m_buttons.end(); ++i)
	{
		(i->second)->Invalidate(TRUE);
	}
}

// =====================================================================================================================
//    Receive messages through subscription.
// =======================================================================================================================
void main_view::subscription_to_main_view::note(message m)
{
	switch (m)
	{
		case mpatientlist:
			m_main_view->update_data();
			break;

		case mconfig:
			m_main_view->update_config();
			break;
	}
}

// =====================================================================================================================
//    update the config
// =======================================================================================================================
void main_view::update_config(void)
{
	// Read all values
	bool display_events = get_conductor().get_config().read("display_events", true);

	bool display_wcr_tracing = get_conductor().get_config().read("display_wcr_tracing", true);

	long hs_upper_limit = get_conductor().get_config().read("contraction_rate_upper_limit", patterns_gui::tracing::get_cr_maximum());

	long hs_crwindow = get_conductor().get_config().read("contraction_rate_window", patterns::fetus::get_cr_window());

	long hs_threshold = get_conductor().get_config().read("contraction_rate_stage1_threshold", patterns_gui::tracing::get_cr_threshold());

	long hs_kcrstage = get_conductor().get_config().read("contraction_rate_stage2_delay", patterns_gui::tracing::get_cr_kcrstage());

	bool event_is_on = get_conductor().get_config().read("event_detection_purchased", patterns_gui::fetus::get_event_detection_enabled());

	bool display_baseline_variability = get_conductor().get_config().read("display_baseline_variability", get_patient_view().is_baseline_variability_activated());

#ifdef patterns_parer_classification
	bool display_Classification = get_conductor().get_config().read("display_classification", get_patient_view().is_classification_activated());
#endif
#ifdef patterns_standalone
	bool enable_Montevideo = get_conductor().get_config().read("enable_montevideo", true);
#else
	bool enable_Montevideo = get_conductor().get_config().read("enable_montevideo", false);
#endif
	bool display_mhr = get_conductor().get_config().read("display_mhr", patterns_gui::tracing::get_display_mhr());

	// Apply them
	patterns_gui::tracing::set_cr_maximum(hs_upper_limit);
	patterns_gui::tracing::set_cr_threshold(hs_threshold);
	patterns_gui::tracing::set_cr_kcrstage(hs_kcrstage);
	patterns::fetus::set_cr_window(hs_crwindow);
	patterns::fetus::set_event_detection_enabled(event_is_on);

	get_patient_view().get_tracing().show(patterns_gui::tracing::wcrtracing, display_wcr_tracing);
	get_patient_view().get_tracing().show(patterns_gui::tracing::whideevents, !event_is_on || !display_events);
	get_patient_view().is_baseline_variability_activated(display_baseline_variability);

#ifdef patterns_parer_classification
	get_patient_view().is_classification_activated(display_Classification);
	fetus::set_classification_enabled(get_patient_view().is_classification_activated());
#endif
	patterns_application::get_application().is_enable_Montevideo(enable_Montevideo);
	if (!enable_Montevideo)
	{
		// Make sure montevideo is OFF if it's disabled
		get_patient_view().get_tracing().show(patterns_gui::tracing::wmontevideo, false);
	}

	patterns_gui::tracing::set_display_mhr(display_mhr);

	update_events_button();
	update_montevideo_button();
	update_baseline_button();
	update_toco_button();

	update_data();

	get_patient_view().get_tracing().RedrawWindow(NULL, NULL, RDW_INVALIDATE);
}

// =====================================================================================================================
//    Update the look of the toggle montevideo button
// =====================================================================================================================
void main_view::update_montevideo_button()
{
	// If montevideo is not on, just hide the button
	bool bMontevideoOn = patterns_application::get_application().is_enable_Montevideo();
	get_button("montevideo")->ShowWindow(bMontevideoOn ? SW_SHOW : SW_HIDE);

	// Montevideo is ON
	if (patterns_application::get_application().is_visible(patterns_gui::tracing::wmontevideo))
	{
		get_button("montevideo")->set_icon("action on");
	}

	// Montevideo is OFF
	else
	{
		get_button("montevideo")->set_icon("action off");
	}

	refresh_command_bar();
}

// =====================================================================================================================
//    Update the look of the toggle events button
// =====================================================================================================================
void main_view::update_baseline_button()
{
	// Baseline is ON
	if (!patterns_application::get_application().is_visible(patterns_gui::tracing::wbaselines))
	{
		get_button("baseline")->set_icon("action on");
	}

	// Baseline is OFF
	else
	{
		get_button("baseline")->set_icon("action off");
	}

	refresh_command_bar();
}

// =====================================================================================================================
//    Update the look of the toggle events button
// =====================================================================================================================
void main_view::update_events_button()
{
	// Events is ON
	if (!patterns_application::get_application().is_visible(patterns_gui::tracing::whideevents))
	{
		get_button("events")->set_icon("action on");
	}

	// Events is OFF
	else
	{
		get_button("events")->set_icon("action off");
	}

	refresh_command_bar();
}

// =====================================================================================================================
//    Update the look of the toggle events button
// =====================================================================================================================
void main_view::update_toco_button()
{
	if (!patterns_application::get_application().is_visible(patterns_gui::tracing::wcrtracing))
	{
		get_button("toco")->set_icon("action off");
	}
	else
	{
		get_button("toco")->set_icon("action on");
	}

	refresh_command_bar();
}

void main_view::set_button(string name, icon_button* button)
{
	m_buttons[name] = button;
}

icon_button* main_view::get_button(string name)
{
	map<string, icon_button *>::iterator i = m_buttons.find(name);
	if (i == m_buttons.end())
	{
		icon_button*  button = new icon_button();
		set_button(name, button);
		return button;
	}

	return i->second;
}

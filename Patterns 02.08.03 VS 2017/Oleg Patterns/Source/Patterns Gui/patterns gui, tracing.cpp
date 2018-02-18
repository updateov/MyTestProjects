#include "stdafx.h"

#include "patterns gui, tracing.h"
#include "patterns, conductor.h"
#include "patterns, fetus.h"
#include "patterns, input adapter.h"
#include "patterns gui, services.h"
#include "patterns, samples.h"
#include "patterns, subscription.h"
#include "patterns gui, double buffer.h"
#include "afxdrawmanager.h"

#include <fstream>

#ifdef patterns_parer_classification
#include "..\Parer\ParerClassifier.h"
#endif
using namespace patterns_gui;

BEGIN_MESSAGE_MAP(tracing, CWnd)
	ON_WM_ERASEBKGND()
	ON_WM_GETDLGCODE()
	ON_WM_KEYDOWN()
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	ON_WM_PAINT()
	ON_WM_TIMER()
	ON_WM_SIZE()
	ON_WM_MOVE()
END_MESSAGE_MAP()

bool tracing:: is_display_mhr = false;
bool tracing:: idebugkeys = false;

long tracing::cr_maximum(10);
long tracing::cr_threshold(5); 
long tracing::cr_kcrstage(30);

// =====================================================================================================================
//    Construction and destruction. We need to unsubscribe when being destroyed to ensure that no instance is left with
//    dangling pointers.We go through set() and set_partial() to create and delete the current fetus instances in order
//    to centralize implementation.
// =======================================================================================================================
tracing::tracing(void)
{
	m_bIsTogleZoomDisabled = false;
	m_logo = false;

	m_exportEnabled = false;
	m_intervalExported = false;
	m_exportOpen = false;
	m_currentExportInterval = 0;
	m_highlightedExportInterval = 0;
	m_currentExportEndTime = undetermined_date;
	m_highlightedExportTime = undetermined_date;
	

	m_animationLeft1 = m_animationLeft2 = 0;
	m_animationRight1 = m_animationRight2 = 0;
	m_animationRate = 1;
	offset = 0;
	m_pFetus = fpartial = 0;
	g1 = g2 = 0;
	ianimating = false;
	icandelete = false;
	idataneedsupdate = true;
	idebug = false;
	ifollowparent = false;
	iinontimer = false;
	ineedsupdate = true;
	ipropagate = true;
	iscalinglocked = false;
	iscrolling = true;
	ishow = wgrid + winformation + wcpictograms + wceventbars + wguides + wstruckout + wcr;
	isubview = false;
	izoomed = false;
	lcompressed = lexpanded = m_sliderLength = -1;
	kpaper = pinternational;
	kparent = 0;
	kprecision = 2;
	m_scalingMode = sfree;
	q = qidle;
	rbdeletebutton = CRect(0, 0, 0, 0);
	rbacceptbutton = CRect(0, 0, 0, 0);
	m_scaling = 1;
	m_tracingType = tup;
	tselection = cnone;
	zleft = zright = -1;

	m_bLeftPartShown = false;
	m_toggleRect.SetRectEmpty();
	m_zoomBtnRect.SetRectEmpty();

	create_bitmaps();

#ifdef ACTIVE_CONTRACTION
	contraction_move_start = false;
	contraction_move_end = false;
#endif
}

tracing::~tracing(void)
{
	if (m_pFetus)
	{
		set(0);
	}

	if (fpartial)
	{
		set_partial(0);
	}

	unsubscribe();
	unsubscribe_children();
	destroy_subviews();
}

// =====================================================================================================================
//    Propagate settings to subviews. This should be called when settings are changed that need to be reflected to
//    subviews.
// =======================================================================================================================
void tracing::adjust_subviews(void) const
{
	long i;
	long n = (long)ksubviews.size();

	const_cast<tracing*>(this)->zoom(false, true);

	for (i = 0; i < n; i++)
	{
		ksubviews[i]->scroll(is_scrolling());
		ksubviews[i]->set_paper(get_paper());
		if (i < 2)
		{
			ksubviews[i]->set_scaling_mode(get_scaling_mode());
		}

		if (i < 2 && get_scaling_mode() == sfree)
		{
			ksubviews[i]->set_scaling(get_scaling());
		}

		ksubviews[i]->ishow = ishow;
		ksubviews[i]->lock_scaling(is_scaling_locked());
		ksubviews[i]->set_can_delete(can_delete());
		ksubviews[i]->set_precision(get_precision());
	}

	if (!ksubviews.empty() && (get_type() == tnormal || get_type() == tnormalnc))
	{
		ksubviews[0]->set_type(tfhr);
		ksubviews[1]->set_type(tup);
	}

	if (!ksubviews.empty() && get_type() == tnormal)
	{
		ksubviews[2]->set_type(tfhr);
		ksubviews[3]->set_type(tup);
		if (is_visible(wcr))
		{
			ksubviews[4]->set_type(tcr);
		}

		ksubviews[2]->set_scaling_mode(has_compressed_length() ? spaper : sfit);
		ksubviews[3]->set_scaling_mode(sfit);
		for (i = 4; i < n; i++)
		{
			ksubviews[i]->set_scaling_mode(sfit);
		}
	}
}

// =====================================================================================================================
//    Should the user be allowed to delete objects? The client should set this through set_can_delete().If the user
//    deletes an object(event or contraction), the client receives an Mfc notification ndeleteevent or
//    ndeletecontraction.The tracing takes no other actions by itself.
// =======================================================================================================================
bool tracing::can_delete(void) const
{
#ifdef patterns_viewer
	return false;
#else
	return icandelete;
#endif
}

// =====================================================================================================================
//    Can we successfully draw given bitmap rectangle? This is use to determine that we need to draw alternative
//    representation for the slider, events and contractions.
// =======================================================================================================================
bool tracing::can_draw_bitmap_rectangle(const string& n) const
{
	// Use a map to ease the pain
	map<string, bool>::iterator itr = can_draw_bitmap.find(n);
	if (itr != can_draw_bitmap.end())
	{
		return itr->second;
	}

	// Do the research since it's not in the map
	long nfound = 0;
	if (services::is_bitmap(n + "_top_left"))
	{
		nfound++;
	}

	if (services::is_bitmap(n + "_top"))
	{
		nfound++;
	}

	if (services::is_bitmap(n + "_top_right"))
	{
		nfound++;
	}

	if (services::is_bitmap(n + "_left"))
	{
		nfound++;
	}

	if (services::is_bitmap(n))
	{
		nfound++;
	}

	if (services::is_bitmap(n + "_right"))
	{
		nfound++;
	}

	if (services::is_bitmap(n + "_bottom_left"))
	{
		nfound++;
	}

	if (services::is_bitmap(n + "_bottom"))
	{
		nfound++;
	}

	if (services::is_bitmap(n + "_bottom_right"))
	{
		nfound++;
	}

	bool result = nfound >= 3;

	can_draw_bitmap.insert(pair<string, bool> (n, result));
	return result;
}

// =====================================================================================================================
//    Create our bitmaps from compound resources. See method create_rectangle_bitmaps() for comments.
// =======================================================================================================================
void tracing::create_bitmaps(void)
{
	if (!services::is_bitmap("m_disconnected"))
	{
		// First split up the big t_objects and t_objects_2 images.
		services::create_bitmap_fragment("t_objects", "contraction_h", CRect(0, 0, 34, 6));
		services::create_bitmap_fragment("t_objects", "contraction_hs", CRect(0, 16, 34, 50));
		services::create_bitmap_fragment("t_objects", "contraction", CRect(0, 60, 34, 70));
		services::create_bitmap_fragment("t_objects", "contraction_s", CRect(0, 76, 34, 110));
		services::create_bitmap_fragment("t_objects", "contraction_hn", CRect(1 * 40, 0, 1 * 40 + 34, 6));
		services::create_bitmap_fragment("t_objects", "contraction_hns", CRect(1 * 40, 16, 1 * 40 + 34, 50));
		services::create_bitmap_fragment("t_objects", "contraction_n", CRect(1 * 40, 60, 1 * 40 + 34, 70));
		services::create_bitmap_fragment("t_objects", "contraction_ns", CRect(1 * 40, 76, 1 * 40 + 34, 110));
		services::create_bitmap_fragment("t_objects", "acceleration_h", CRect(2 * 40, 0, 2 * 40 + 34, 6));
		services::create_bitmap_fragment("t_objects", "acceleration_hs", CRect(2 * 40, 16, 2 * 40 + 34, 50));
		services::create_bitmap_fragment("t_objects", "acceleration", CRect(2 * 40, 60, 2 * 40 + 34, 70));
		services::create_bitmap_fragment("t_objects", "acceleration_s", CRect(2 * 40, 76, 2 * 40 + 34, 110));
		services::create_bitmap_fragment("t_objects", "deceleration_h", CRect(3 * 40, 0, 3 * 40 + 34, 6));
		services::create_bitmap_fragment("t_objects", "deceleration_hs", CRect(3 * 40, 16, 3 * 40 + 34, 50));
		services::create_bitmap_fragment("t_objects", "deceleration", CRect(3 * 40, 60, 3 * 40 + 34, 70));
		services::create_bitmap_fragment("t_objects", "deceleration_s", CRect(3 * 40, 76, 3 * 40 + 34, 110));
		services::create_bitmap_fragment("t_objects", "so_event_h", CRect(4 * 40, 0, 4 * 40 + 34, 6));
		services::create_bitmap_fragment("t_objects", "so_event_hs", CRect(4 * 40, 16, 4 * 40 + 34, 50));
		services::create_bitmap_fragment("t_objects", "so_event", CRect(4 * 40, 60, 4 * 40 + 34, 70));
		services::create_bitmap_fragment("t_objects", "so_event_s", CRect(4 * 40, 76, 4 * 40 + 34, 110));
		services::create_bitmap_fragment("t_objects", "so_contraction_h", CRect(4 * 40, 120, 4 * 40 + 34, 126));
		services::create_bitmap_fragment("t_objects", "so_contraction_hs", CRect(4 * 40, 136, 4 * 40 + 34, 170));
		services::create_bitmap_fragment("t_objects", "so_contraction", CRect(4 * 40, 180, 4 * 40 + 34, 190));
		services::create_bitmap_fragment("t_objects", "so_contraction_s", CRect(4 * 40, 196, 4 * 40 + 34, 230));

		services::create_bitmap_fragment("t_objects", "t_events", CRect(1 * 24 + 1, 125, 1 * 24 + 17, 405));
		services::create_bitmap_fragment("t_objects", "t_events_h", CRect(2 * 24 + 13, 125, 2 * 24 + 21, 270));


		services::forget_bitmap("t_objects");

		// Do objects.
		create_rectangle_bitmaps_h("contraction", 10, true);
		create_rectangle_bitmaps_h("contraction_h", 5, true);
		create_rectangle_bitmaps_h("contraction_hn", 5, true);
		create_rectangle_bitmaps("contraction_hns", 5);
		create_rectangle_bitmaps("contraction_hs", 5);
		create_rectangle_bitmaps_h("contraction_n", 10, true);
		create_rectangle_bitmaps("contraction_ns", 10);
		create_rectangle_bitmaps("contraction_s", 10);
		create_rectangle_bitmaps_h("acceleration", 10);
		create_rectangle_bitmaps_h("acceleration_h", 5);
		create_rectangle_bitmaps("acceleration_hs", 5);
		create_rectangle_bitmaps("acceleration_s", 10);
		create_rectangle_bitmaps_h("deceleration", 10);
		create_rectangle_bitmaps_h("deceleration_h", 5);
		create_rectangle_bitmaps("deceleration_hs", 5);
		create_rectangle_bitmaps("deceleration_s", 10);
		create_rectangle_bitmaps_h("so_event", 10);
		create_rectangle_bitmaps_h("so_event_h", 5);
		create_rectangle_bitmaps("so_event_hs", 5);
		create_rectangle_bitmaps("so_event_s", 10);
		create_rectangle_bitmaps_h("so_contraction", 10, true);
		create_rectangle_bitmaps_h("so_contraction_h", 5, true);
		create_rectangle_bitmaps("so_contraction_hs", 5);
		create_rectangle_bitmaps("so_contraction_s", 10);
		create_rectangle_bitmaps("information", 10);
		create_rectangle_bitmaps("information_m", 10);
		create_rectangle_bitmaps("slider", 60);

		// Do other pictograms.
		services::create_bitmap_fragment("t_disconnected", "disconnected", CRect(0, 0, 40, 40));
		services::create_bitmap_fragment("t_disconnected", "m_disconnected", CRect(40, 0, 80, 40));
		services::forget_bitmap("t_disconnected");

		services::create_bitmap_fragment("t_hyperstimulation", "hyperstimulation", CRect(0, 0, 100, 40));
		services::create_bitmap_fragment("t_hyperstimulation", "hyperstimulation_2", CRect(100, 0, 200, 40));
		services::create_bitmap_fragment("t_hyperstimulation", "hyperstimulation_3", CRect(200, 0, 300, 40));
		services::forget_bitmap("t_hyperstimulation");


		services::create_bitmap_fragment("t_object_buttons", "object_delete", CRect(0, 0, 12, 12));
		services::create_bitmap_fragment("t_object_buttons", "object_delete_selected", CRect(12, 0, 24, 12));
		services::create_bitmap_fragment("t_object_buttons", "object_accept", CRect(24, 0, 36, 12));
		services::create_bitmap_fragment("t_object_buttons", "object_accept_selected", CRect(36, 0, 48, 12));
		services::create_bitmap_fragment("t_object_buttons", "object_mask", CRect(48, 0, 60, 12));
		services::create_bitmap_fragment("t_object_buttons", "object_undo", CRect(60, 0, 72, 12));
		services::create_bitmap_fragment("t_object_buttons", "object_undo_selected", CRect(72, 0, 84, 12));
		services::forget_bitmap("t_object_buttons");

		for (long i = 0; i < 2; i++)
		{
			string p;
			string t;

			switch (i)
			{
			case 0:
				p = "e_";
				t = "t_events";
				break;

			case 1:
				p = "eh_";
				t = "t_events_h";
				break;
			}

			for (long j = 0; j <= 13; j++)
			{
				string n = p;

				switch (j)
				{
				case 0:
					n += "na_acceleration";
					break;

				case 1:
					n += "ni_acceleration";
					break;

				case 2:
					n += "acceleration";
					break;

				case 3:
					n += "late";
					break;

				case 4:
					n += "atypical";
					break;

				case 5:
					n += "typical";
					break;

				case 6:
					n += "prolonged";
					break;

				case 7:
					n += "early";
					break;

				case 8:
					n += "ni_deceleration";
					break;

				case 9:
					n += "na_deceleration";
					break;

				case 10:
					n += "so_event";
					break;

				case 11:
					n += "prolonged_atypical";
					break;

				case 12:
					n += "prolonged_atypical_critic";
					break;

				case 13:
					n += "atypical_critic";
					break;
				}

				if (i == 0)
				{
					services::create_bitmap_fragment(t, n, CRect(0, j * 20, 16, j * 20 + 16));
				}
				else
				{
					services::create_bitmap_fragment(t, n, CRect(0, j * 10, 8, j * 10 + 8));
				}
			}
		}

		services::forget_bitmap("t_events");
		services::forget_bitmap("t_events_h");
	}
}

// =====================================================================================================================
//    Create and store bitmaps for drawing rectangles. This makes it easier to create resources for bitmap rectangles:
//    instead of having to create 3, 6, 9 or even 18 bitmaps, we need only create one or two and then use this method to
//    split them into bitmap fragments that methods can_draw_bitmap_rectangle() and draw_bitmap_rectangle() will
//    ultimately use. Basically, this method cuts a single bitmap into nine bitmaps: top -left, top, top-right, left,
//    center, right, bottom-left, bottom and bottom-right: d | | d + d | | | | --+ | | | | | | | | | | | | | | | | --+ d
//    | | | | + n: base name, see method draw_rectangle_bitmap(). d: distance from edge of original bitmap to where
//    fragments are cut.
// =======================================================================================================================
void tracing::create_rectangle_bitmaps(const string& n, long d)
{
	if (services::is_bitmap(n) && !services::is_bitmap(n + "_top"))
	{
		CRect rect = services::get_bitmap_rectangle(n);

		if (rect.Width() > 2 * d && rect.Height() > 2 * d)
		{
			services::create_bitmap_fragment(n, n + "_top_left", CRect(0, 0, d, d));
			services::create_bitmap_fragment(n, n + "_top", CRect(d, 0, rect.right - d, d));
			services::create_bitmap_fragment(n, n + "_top_right", CRect(rect.right - d, 0, rect.right, d));
			services::create_bitmap_fragment(n, n + "_left", CRect(0, d, d, rect.bottom - d));
			services::create_bitmap_fragment(n, n + "_right", CRect(rect.right - d, d, rect.right, rect.bottom - d));
			services::create_bitmap_fragment(n, n + "_bottom_left", CRect(0, rect.bottom - d, d, rect.bottom));
			services::create_bitmap_fragment(n, n + "_bottom", CRect(d, rect.bottom - d, rect.right - d, rect.bottom));
			services::create_bitmap_fragment(n, n + "_bottom_right", CRect(rect.right - d, rect.bottom - d, rect.right, rect.bottom));
			services::create_bitmap_fragment(n, n, CRect(d, d, rect.right - d, rect.bottom - d));
		}
	}
}

// =====================================================================================================================
//    Create bitmaps for horizontal rectangles. This creates the three top bitmaps, that is, top-left, top and top-
//    right.See method create_rectangle_bitmap().
// =======================================================================================================================
void tracing::create_rectangle_bitmaps_h(const string& n, long d, bool idown)
{
	if (services::is_bitmap(n) && !services::is_bitmap(n + (idown ? "_bottom" : "_top")))
	{
		CRect rect = services::get_bitmap_rectangle(n);

		if (rect.Width() > 2 * d)
		{
			services::create_bitmap_fragment(n, n + (idown ? "_bottom_left" : "_top_left"), CRect(0, 0, d, rect.bottom));
			services::create_bitmap_fragment(n, n + (idown ? "_bottom" : "_top"), CRect(d, 0, rect.right - d, rect.bottom));
			services::create_bitmap_fragment(n, n + (idown ? "_bottom_right" : "_top_right"), CRect(rect.right - d, 0, rect.right, rect.bottom));
			services::forget_bitmap(n);
		}
	}
}

// =====================================================================================================================
//    Create and manage subviews if need be. Since set_type() calls destroy_subviews(), we can assume that, if ksubviews
//    contains elements, then they are the right elements already.We also make sure that all subviews have the correct
//    size and position. Implementation notes. The create_subviews() methods are complex and sensitive.In short: don't
//    touch them unless you really have to and, if possible, talk to François Breton or to me, jean-françois gauthier
//    beforehand. As an overview: - subviews are created, - they are arranged to fill the height of the parent, - if in
//    paper scaling mode: - if lengths are set, heights are rearranged to fit both the aspect-ratio and length
//    constraints, including the Cr height, - otherwise, only the repsective heights of both Fhr-Up pairs are rearranged
//    without changing their total height. c_s_do_height(i, h): sets the height of ksubviews[i] to h. c_s_do_length(i,
//    d): adjust heights of ksubviews[i] and ksubviews [i + 1] according to given length d.Method ..._f() is simply a
//    utility function. c_s_do_paper(i): adjust relative heights of ksubviews[i] and ksubviews[i + 1] according to their
//    relative paper proportions. c_s_do_space(i, s): move ksubviews[i] though[n - 1] so that the space between
//    ksubviews[i - 1] and[i] is exactly s, regardless of what it is before the call. c_s_height(i): returns the height
//    of ksubviews[i]. c_s_space(i): returns the space betweend ksubviews[i - 1] and ksubviews[i]. The height of a
//    subview is set according to the following formula: hp = h*lp/(dt*rect);
//    where hp is the height in pixels, h the height in centimetres, lp the length in pixels(that is, the width), dt the
//    length in seconds (that is, the desired duration) and rect the paper ratio, in centimetres per second.As this is
//    genereally something like .05 cm/s(3 cm/min), we use the inverse instead, 20 s/cm.
// =======================================================================================================================
void tracing::create_subviews(void) const
{
	long i;
	static bool iin = false;
	long n;
	CRect rect = get_bounds(oview);

	if (iin)
	{
		return;
	}

	iin = true;

	// Create subviews per se.
	if (ksubviews.empty())
	{
		switch (get_type())
		{
		case tnormal:
		case tnormalnc:
			{
				tracing*  tc = const_cast<tracing*>(this);				
				tracing*  x = NewTracing();
				tracing*  y = NewTracing();

				x->Create(NULL, "", WS_CHILD, CRect(), tc, 0);
				y->Create(NULL, "", WS_CHILD, CRect(), tc, 0);
				x->subscribe_to(tc, true);
				y->subscribe_to(tc, true);
				x->set_offset(get_offset());
				y->set_offset(get_offset());
				ksubviews.push_back(x);
				ksubviews.push_back(y);
				if (get_type() == tnormal)
				{
					tracing*  z1 = NewTracing();
					tracing*  z2 = NewTracing();
					tracing*  z3 = NewTracing();

					z1->Create(NULL, "", WS_CHILD, CRect(), tc, 0);
					z2->Create(NULL, "", WS_CHILD, CRect(), tc, 0);
					z1->subscribe_to(tc);
					z2->subscribe_to(tc);
					ksubviews.insert(ksubviews.end(), z1);
					ksubviews.insert(ksubviews.end(), z2);
					if (is_visible(wcr))
					{
						z3->Create(NULL, "", WS_CHILD, CRect(), tc, 0);
						z3->subscribe_to(tc);
						ksubviews.insert(ksubviews.end(), z3);
					}
				}

				adjust_subviews();
			}
			break;
		}

		for (i = 0, n = (long)ksubviews.size(); i < n; i++)
		{
			ksubviews[i]->isubview = true;
		}
	}

	// Arrange the subviews if the size changed.
	if ((ksubviews.size() > 0) && (ksubviews[0]->get_bounds().Width() != rect.Width()))
	{
		switch (get_type())
		{
		case tnormal:
			{
				long h = rect.Height() - 9;
				long p[8];
				long pd[] = { 0, 1, 3, 4, 6};

				if (h < 200)
				{
					h = 200;
				}

				p[0] = 0;
				p[1] = 6 * h / 19;
				p[2] = 9 * h / 19;
				p[3] = 10 * h / 19;
				p[4] = 14 * h / 19;
				p[5] = 16 * h / 19;
				p[6] = 17 * h / 19;
				p[7] = h;
				for (i = 0; i < (long)ksubviews.size(); i++)
				{
					SetSubviewBounds(i, 0, p[pd[i]], rect.Width(), p[pd[i] + 1] - p[pd[i]]);
				}
			}
			break;

		case tnormalnc:
			{
				long h0 = 2 * rect.Height() / 3;
				SetSubviewBounds(0, 0, 0, rect.Width(), h0);
				SetSubviewBounds(1, 0, h0, rect.Width(), rect.Height() - h0);
			}
			break;
		}
	}
	{
		bool idodown = get_scaling_mode() == spaper && get_type() == tnormal;
		bool idoup = get_scaling_mode() == spaper && (get_type() == tnormal || get_type() == tnormalnc);

		// Enforce length limitations.
		if (idoup && has_expanded_length())
		{
			create_subviews_do_length(0, get_expanded_length());
		}

		if (idodown && has_compressed_length())
		{
			long d0;
			double s0;

			create_subviews_do_length(2, get_compressed_length());
			ksubviews[2]->update_now();
			d0 = ksubviews[2]->offset;
			s0 = ksubviews[2]->m_scaling;
			ksubviews[3]->set_scaling(s0);
			ksubviews[3]->set_offset(d0);
			if (is_visible(wcr))
			{
				ksubviews[4]->set_scaling(s0);
				ksubviews[4]->set_offset(d0);
				create_subviews_do_height(4, is_visible(wcrtracing) ? 3 * create_subviews_height(3) / 2 : 20);
				create_subviews_do_space(4, create_subviews_height(3));
			}

			create_subviews_do_space(2, create_subviews_height(2) / 2);
		}

		// Enforce paper scaling mode.
		if (idoup && !has_expanded_length())
		{
			create_subviews_do_paper(0);
			ksubviews[0]->update_now();
			priv_set_scaling(ksubviews[0]->m_scaling);
		}

		if (idodown && !has_compressed_length())
		{
			create_subviews_do_paper(2);
		}
	}

	iin = false;
}

void tracing::create_subviews_do_height(long i, long h) const
{
	CRect rect = ksubviews[i]->get_bounds();
	if (rect.Height() != (int)h)
	{
		// ScreenToClient(&rect);
		ksubviews[i]->MoveWindow(rect.left, rect.top, rect.Width(), h);
		create_subviews_do_space(i + 1, 0);
	}
}

void tracing::create_subviews_do_length(long i, long d) const
{
	long h0 = create_subviews_do_length_f(i, d);
	long h1 = create_subviews_do_length_f(i + 1, d);

	create_subviews_do_height(i, h0);
	create_subviews_do_height(i + 1, h1);
}

long tracing::create_subviews_do_length_f(long i, long d) const
{
	tracing*  curTracings = ksubviews[i];

	return curTracings->get_bounds(oview).Height() - curTracings->get_bounds(otracing).Height() + 20 * (long)curTracings->get_paper_height() * curTracings->get_bounds(otracing).Width() / d;
}

void tracing::create_subviews_do_paper(long i) const
{
	double d1;
	double d2;
	double h;
	double h1;
	double h2;
	double p1;
	double p2;
	CRect rect = get_bounds(oview);
	CRect rectClient;

	rectClient = ksubviews[i]->get_bounds();

	// ScreenToClient(&rclient);
	h1 = ksubviews[i]->get_bounds(oview).Height();
	h2 = ksubviews[i + 1]->get_bounds(oview).Height();
	h = h1 + h2;
	d1 = h1 - ksubviews[i]->get_bounds(otracing).Height();
	d2 = h2 - ksubviews[i + 1]->get_bounds(otracing).Height();
	p1 = ksubviews[i]->get_paper_height();
	p2 = ksubviews[i + 1]->get_paper_height();
	h1 = (p1 * (h - d2) + p2 * d1) / (p2 * ((double)1 + p1 / p2));
	h2 = h - h1;

	SetSubviewBounds(i, 0, rectClient.top, rect.Width(), (long)h1);
	SetSubviewBounds(i + 1, 0, rectClient.top + (long)h1, rect.Width(), (long)h2);
}

void tracing::create_subviews_do_space(long i, long h) const
{
	CRect rect;

	for (long j = i, n = (long)ksubviews.size(); j < n; j++)
	{
		if (j == i)
		{
			h -= create_subviews_space(i);
		}

		if (h != 0)
		{
			rect = ksubviews[j]->get_bounds();

			// ScreenToClient(&rect);
			ksubviews[j]->MoveWindow(rect.left, rect.top + h, rect.Width(), rect.Height());
		}
	}
}

long tracing::create_subviews_height(long i) const
{
	return ksubviews[i]->get_bounds().Height();
}

long tracing::create_subviews_space(long i) const
{
	return ksubviews[i]->get_bounds().top - ksubviews[i - 1]->get_bounds().bottom;
}

// =====================================================================================================================
//    Deselect any selected object. Index is irrelevant, see select().
// =======================================================================================================================
void tracing::deselect(void)
{
	select(cnone, -1);
}

// =====================================================================================================================
//    Destroy all subviews. This is meant to be called from the destructor or from set_type().
// =======================================================================================================================
void tracing::destroy_subviews(void) const
{
	if (ksubviews.size() > 0)
	{
		for (long i = 0, n = (long)ksubviews.size(); i < n; i++)
		{
			delete ksubviews[i];
		}

		ksubviews.clear();
	}
}

// Optimization for drawing
BOOL tracing::OnEraseBkgnd(CDC *)
{
	return TRUE;
}


// =====================================================================================================================
//    Draw everything into given context and rectangle. First off, we decide if we ask subviews to draw themselves or if
//    we draw ourself. Before we draw anything, we need to make sure that the internal state is up to date.This is why we
//    call update_now().See comments for methods update() and update_now(). The various draw_...() methods encapsulate
//    the different parts of the tracing so that they me be easily moved or reordered. We set the clipping region to make
//    sure we are well-behaved when called directly by clients, outside the normal OnPaint() sequence.
// =======================================================================================================================
void tracing::draw(CDC* dc) const
{
	dc->SaveDC();

	services::select_font(80, "Arial", dc);

	create_subviews();

	const fetus*  curFetus  = get();
	bool is_fake_fetus = !curFetus ->has_start_date();

	if (ksubviews.empty())
	{
		const fetus* partialFetus = get_partial();
		CRgn region;
		CRect rect = get_bounds(oview);

		region.CreateRectRgn(rect.left, rect.top, rect.right, rect.bottom);
		region.OffsetRgn(dc->GetViewportOrg());
		dc->SelectClipRgn(&region, RGN_AND);

		update_now();

		dc->FillRect(get_bounds(oview), CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));

		draw_grid(dc);
		draw_background(dc);

		if (!is_fake_fetus)
		{
			draw_tracing(dc);

			switch (get_type())
			{
			case tfhr:
				rbdeletebutton = CRect(0, 0, 0, 0);
				rbacceptbutton = CRect(0, 0, 0, 0);
				draw_events(dc, curFetus , true);
				draw_events(dc, partialFetus , false);
				break;

			case tup:
				rbdeletebutton = CRect(0, 0, 0, 0);
				rbacceptbutton = CRect(0, 0, 0, 0);
				draw_contractions(dc, curFetus , true);
				draw_contractions(dc, partialFetus , false);
				break;
			}

			draw_baselines(dc);

			if (!is_compressed())
			{
				if (!is_scrolling())
				{
					draw_watermark(dc, "in_review");
				}
				else if (is_late_realtime())
				{
					draw_watermark(dc, "tracing_delayed");
				}
			}

			if (get_type() == tfhr && !is_compressed() && has_selection() && get_selection_type() == cevent && is_visible(winformation) && !is_visible(whideevents))
			{
				draw_event_information(dc, get_selection());
			}
#if defined(OEM_patterns) || defined(patterns_retrospective)
			if (get_type() == tup && !is_compressed() && has_selection() && get_selection_type() == ccontraction && is_visible(winformation))
			{
				draw_contraction_information(dc, get_selection());
			}
#endif
		}
	}
	else
	{
		COLORREF cdate = RGB(127, 127, 127);
		CRect rect;
		string s;

		dc->SetTextColor(cdate);
		dc->SetBkMode(TRANSPARENT);

		GetClientRect(&rect);
		dc->FillRect(rect, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));

		long long t1 = ksubviews[0]->get_left_in_ms();

		CRect t2 = ksubviews[0]->get_bounds(otracing);
		double t3 = ksubviews[0]->get_scaling();
		long t4 = ksubviews[0]->get_offset();
		long t5 = get_sample_rate();

		static long long s1;
		static CRect s2;
		static double s3;
		static long s4;
		static long s5;

		s1 = t1;
		s2 = t2;
		s3 = t3;
		s4 = t4;
		s5 = t5;

		for (long i = 0, n = (long)ksubviews.size(); i < n; i++)
		{
			ksubviews[i]->GetWindowRect(&rect);
			ScreenToClient(&rect);
			dc->SetViewportOrg(rect.TopLeft());
			ksubviews[i]->draw(dc);
			dc->SetViewportOrg(0, 0);

			if (!is_fake_fetus)
			{


				//DrawExportedAreaMarker(dc, i);

				if (ksubviews[i]->is_compressed() && ksubviews[i]->get_type() == tcr)
				{	
					s = "Persistent contractility index";
					dc->TextOut(rect.left + 3, rect.top - 14, s.c_str());
				}
				else if (ksubviews[i]->is_compressed() && ksubviews[i]->get_type() == tfhr)
				{
					s = get_compressed_fhr_title();
					dc->TextOut(rect.left + 3, rect.top - 10, s.c_str());

					if (is_visible(patterns_gui::tracing::wshowdisclaimer))
					{
						COLORREF previousColor = dc->SetTextColor(RGB(238, 118, 0));
						UINT previousAlign = dc->SetTextAlign(TA_CENTER | TA_BOTTOM);
						dc->TextOut(rect.left + (rect.Width() / 2), rect.top - 10, "--- Demonstration version only ---");
						dc->SetTextColor(previousColor);
						dc->SetTextAlign(previousAlign);
					}
				}
			}
		}

				
		if (!is_fake_fetus)
		{
			draw_guides(dc);
			DrawEdge(dc);
		}
	}


	if (!is_fake_fetus)
	{
		dc->SetViewportOrg(0, 0);
		if (get_type() == tnormal)
		{	
			DrawExportedRangeLines(dc);

			draw_slider(dc);

			//draw_contractions_average(dc);
			DrawSummaryPanel(dc);

			//if (is_visible(patterns_gui::tracing::wbaselinevariability))
			//{
			//	draw_baseline_average(dc);
			//}

			//if (is_visible(patterns_gui::tracing::wmontevideo))
			//{
			//	draw_montevideo_units(dc);
			//}


#ifdef patterns_parer_classification
			if (is_visible(patterns_gui::tracing::wparerclassification))
			{
				draw_parer_classification(dc);
			}
#endif
		}
	}

	if (!is_fake_fetus || ((m_pFetus != 0) && (m_pFetus->get_key().length() > 0)))
	{
		if ((!is_visible(patterns_gui::tracing::whidedisconnected)) && (!is_connected()))
		{
			draw_disconnected(dc);
		}
	}

#ifdef patterns_research
	draw_debug(dc);
#endif

	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    Draw the background plane. We draw the grey areas that show the user what portion of the tracing is displayed by
//    the parent view. Depending on ifollowparent, we infer the sliding window from our parent or from our parent's
//    parent.
// =======================================================================================================================
void tracing::draw_background(CDC* dc) const
{
	if (!is_subview() && has_parent_for_view())
	{
		CBrush bgrey = RGB(230, 230, 230);
		CPen* p0;
		CPen pdarkgrey(PS_SOLID, 0, RGB(191, 191, 191));
		CRect rect;
		CRect r2;
		long xleft;
		long xright;

		p0 = dc->SelectObject(&pdarkgrey);
		rect = get_bounds(otracing);

		CRect sr = get_bounds(oslider);
		xleft = sr.left;
		xright = sr.right;
		if (xleft >= rect.left)
		{
			r2 = rect;
			r2.right = xleft;
			dc->FillRect(&r2, &bgrey);
			dc->MoveTo(r2.right - 1, r2.top);
			dc->LineTo(r2.right - 1, r2.bottom - 1);
		}

		if (xright <= rect.right)
		{
			r2 = rect;
			r2.left = xright;
			dc->FillRect(&r2, &bgrey);
			dc->MoveTo(r2.left, r2.top);
			dc->LineTo(r2.left, r2.bottom - 1);
		}

		dc->SelectObject(p0);
	}
}

// =====================================================================================================================
//    Draw baselines from list of events. We draw baselines associated to the primary fetus, including the height of the
//    baseline, centered above the line itself..
// =======================================================================================================================
void tracing::draw_baselines(CDC* dc) const
{
	if (get_type() == tfhr && is_visible(wbaselines))
	{
		const fetus*  curFetus = get();
		long ileft;
		long iright;
		CRect rect = get_bounds(otracing);
		char t[1000];

		dc->SaveDC();
		dc->SetBkMode(TRANSPARENT);
		dc->SetTextAlign(TA_CENTER | TA_BASELINE);
		dc->SetTextColor(RGB(0, 0, 0));

		iright = get_index_from_x(rect.right);
		ileft = iright - (GetCompleteLength() * get_sample_rate());
		CPen baselinesPen(PS_SOLID, is_compressed()? 0 : 1, RGB(0, 0, 0));
		CPen* pOldPen = dc->SelectObject(&baselinesPen);
		for (long i = 0, n = curFetus->GetEventsCount(); i < n; i++)
		{
			const event&  ei = curFetus->get_event(i);

			if (ei.get_type() == event::tbaseline && ei.get_end() >= ileft && ei.get_start() <= iright)
			{
				double yMean = floor((ei.get_y1() + ei.get_y2()) / 2.0);
				double x1 = get_x_from_index(ei.get_start());
				double x2 = get_x_from_index(ei.get_end());
				double y = get_y_from_units(rect, yMean);

				dc->MoveTo((long)x1, (long)y);
				dc->LineTo((long)x2, (long)y);

				if (!is_compressed())
				{
					sprintf(t, "%ld", (long)(yMean));
					dc->TextOut((long)((x1 + x2) / 2), (long)y + 15, t);
				}
			}
		}
		dc->SelectObject(pOldPen);
		dc->RestoreDC(-1);
	}
}

// =====================================================================================================================
//    Draw a rectangle with given bitmap set. dc: device context to draw into. n: base name of resources to draw rectangle
//    with.Bitmaps are expected to be named(and not numbered with identifiers in resource.h) "n_top", "n_top_left",
//    "n_top_right", "n_left", "n_right", "n_bottom", "n_bottom_left" and "n_bottom_right". d: distance from side of
//    rectangle edge of bitmaps, to leave room for fuzzy, antialiased border.For instance, bitmap "n top left" will be
//    placed at point(x - d, y - d) if top- left corner of rectangle is at(x, y). rect: rectangle to draw image into.
// =======================================================================================================================
void tracing::draw_bitmap_rectangle(CDC* dc, const string& n, long d, long x1, long y1, long x2, long y2) const
{
	CSize sb;
	CSize sbl;
	CSize sbr;
	CSize sl;
	CSize sr;
	CSize st;
	CSize stl;
	CSize str;

	if (x1 > x2)
	{
		long t = x1;

		x1 = x2;
		x2 = t;
	}

	if (y1 > y2)
	{
		long t = y1;

		y1 = y2;
		y2 = t;
	}

	// Compute sizes.
	sb = services::get_bitmap_rectangle(n + "_bottom").Size();
	sb.cy = min(d + y2 - y1 - (y2 - y1) / 2, sb.cy);
	sbl = services::get_bitmap_rectangle(n + "_bottom_left").Size();
	sbl.cx = min(d + (x2 - x1) / 2, sbl.cx);
	sbl.cy = min(d + y2 - y1 - (y2 - y1) / 2, sbl.cy);
	sbr = services::get_bitmap_rectangle(n + "_bottom_right").Size();
	sbr.cx = min(d + x2 - x1 - (x2 - x1) / 2, sbr.cx);
	sbr.cy = min(d + y2 - y1 - (y2 - y1) / 2, sbr.cy);
	sl = services::get_bitmap_rectangle(n + "_left").Size();
	sl.cx = min(d + (x2 - x1) / 2, sl.cx);
	sr = services::get_bitmap_rectangle(n + "_right").Size();
	sr.cx = min(d + x2 - x1 - (x2 - x1) / 2, sr.cx);
	st = services::get_bitmap_rectangle(n + "_top").Size();
	st.cy = min(d + (y2 - y1) / 2, st.cy);
	stl = services::get_bitmap_rectangle(n + "_top_left").Size();
	stl.cx = min(d + (x2 - x1) / 2, stl.cx);
	stl.cy = min(d + (y2 - y1) / 2, stl.cy);
	str = services::get_bitmap_rectangle(n + "_top_right").Size();
	str.cx = min(d + x2 - x1 - (x2 - x1) / 2, str.cx);
	str.cy = min(d + (y2 - y1) / 2, str.cy);

	// Draw corner bitmaps.
	services::draw_bitmap(dc, n + "_top_left", n + "_m_top_left", x1 - d, y1 - d, x1 - d + stl.cx, y1 - d + stl.cy, services::ctopleft);
	services::draw_bitmap(dc, n + "_bottom_left", n + "_m_bottom_left", x1 - d, y2 + d - sbl.cy, x1 - d + sbl.cx, y2 + d, services::cbottomleft);
	services::draw_bitmap(dc, n + "_top_right", n + "_m_top_right", x2 + d - str.cx, y1 - d, x2 + d, y1 - d + str.cy, services::ctopright);
	services::draw_bitmap(dc, n + "_bottom_right", n + "_m_bottom_right", x2 + d - sbr.cx, y2 + d - sbr.cy, x2 + d, y2 + d, services::cbottomright);

	// Draw border bitmaps.
	services::draw_bitmap(dc, n + "_top", n + "_m_top", x1 - d + stl.cx, y1 - d, x2 + d - str.cx, y1 - d + st.cy, services::ctopleft, true);
	services::draw_bitmap(dc, n + "_bottom", n + "_m_bottom", x1 - d + sbl.cx, y2 + d - sb.cy, x2 + d - sbr.cx, y2 + d, services::cbottomright, true);
	services::draw_bitmap(dc, n + "_left", n + "_m_left", x1 - d, y1 - d + stl.cy, x1 - d + sl.cx, y2 + d - sbl.cy, services::ctopleft, true);
	services::draw_bitmap(dc, n + "_right", n + "_m_right", x2 + d - sr.cx, y1 - d + str.cy, x2 + d, y2 + d - sbr.cy, services::cbottomright, true);

	// Draw filler bitmap.
	services::draw_bitmap(dc, n, n + "_m", x1 - d + sl.cx, y1 - d + st.cy, x2 + d - sr.cx, y2 + d - sb.cy, services::ctopleft, true);

	// Draw alternate picture if bitmaps are missing.
	if (!can_draw_bitmap_rectangle(n))
	{
		dc->MoveTo(x1, y1);
		dc->LineTo(x2, y1);
		dc->MoveTo(x1, y1);
		dc->LineTo(x1, y2);
		dc->MoveTo(x2, y1);
		dc->LineTo(x2, y2);
		dc->MoveTo(x1, y2);
		dc->LineTo(x2, y2);
		dc->MoveTo(x1, y1);
		dc->LineTo(x2, y2);
		dc->MoveTo(x2, y1);
		dc->LineTo(x1, y2);
	}
}

void tracing::draw_bitmap_rectangle(CDC* dc, const string& n, long d, CRect rect) const
{
	draw_bitmap_rectangle(dc, n, d, rect.left, rect.top, rect.right, rect.bottom);
}

// =====================================================================================================================
//    Draw contractions for one fetus. We draw the contractions for the given fetus and, if so specified, we draw
//    vertical boxes under the contractions. dc: device context to draw into. curFetus: fetus to take data from. iup: are we
//    drawing the top contraction line? If not, then we draw a contraction line immediately under the top one, meant for
//    the partial results fetus instance. icolumn: draw vertical boxes under the contractions? This is used when the
//    contraction line of the given fetus instance is selected.
// =======================================================================================================================
void tracing::draw_contractions(CDC* dc, const fetus* curFetus, bool iup) const
{
	CBrush bfull = RGB(171, 96, 99);
	CBrush bpale = RGB(220, 185, 183);
	bool icompressed = is_compressed();
	long ileft;
	long iright;

	CPen pborder(PS_SOLID, 0, RGB(171, 96, 99)), ppeak(PS_DOT, 0, RGB(171, 96, 99));

	CRect rect = get_bounds(oview);

	dc->SaveDC();
	dc->SetBkMode(TRANSPARENT);
	dc->SelectObject(&pborder);
	ileft = get_index_from_x(rect.left);
	iright = get_index_from_x(rect.right);

	LONG tracing_top = get_bounds(otracing).top;
	CRect tracing_bounds = get_bounds(iup ? oprimary : osecondary);

	bool show_peak = is_visible(wcontractionpeak);
	long hdelete = services::get_bitmap_rectangle("object_delete").Height();

	for (long i = 0, n = curFetus->GetContractionsCount(); i < n; i++)
	{
		const contraction& ci = curFetus->get_contraction(i);
		bool iselected = iup && get_selection_type() == ccontraction && get_selection() == i;

		if (ci.get_end() > ileft && ci.get_start() <= iright)
		{
			CRect ri = tracing_bounds;

			ri.left = get_x_from_index(ci.get_start());
			ri.right = get_x_from_index(ci.get_end());

			ri.top = tracing_top - 6;

			string bmp;
			if (icompressed)
			{
				bmp += "h";
			}

			if (!ci.is_final())
			{
				bmp += "n";
			}

			if (iselected)
			{
				bmp += "s";
			}

			if (bmp.length() > 0)
			{
				bmp = "contraction_" + bmp;
			}
			else
			{
				bmp = "contraction";
			}

			if (ci.is_strike_out())
			{
				bmp = "so_" + bmp;
			}

			draw_bitmap_rectangle(dc, bmp, 1, ri);

			if (show_peak)
			{
				long xpeak = get_x_from_index(ci.get_peak());

				dc->SelectObject(&ppeak);
				dc->MoveTo(xpeak, ri.top);
				dc->LineTo(xpeak, ri.bottom);
			}

			// Draw delete button is need be.
#if defined(OEM_patterns) || defined(patterns_retrospective)
			if (iselected && !icompressed && can_delete() && ci.is_final())
#else
			if (iselected && !icompressed && can_delete() && ci.is_final() && !ci.is_strike_out())
#endif
			{
				rbdeletebutton = ri;
				rbdeletebutton.top = ri.top + 2;
				rbdeletebutton.bottom = rbdeletebutton.top + hdelete;
				rbdeletebutton.left += (ri.Width() - hdelete) / 2;
				rbdeletebutton.right -= (ri.Width() - hdelete) / 2;
				services::draw_bitmap(
					dc,
#if defined(OEM_patterns) || defined(patterns_retrospective)
					ci.is_strike_out() ? (q == qdownindelete ? "object_undo_selected" : "object_undo") : (q == qdownindelete ? "object_delete_selected" : "object_delete"),
#else
					q == qdownindelete ? "object_delete_selected" : "object_delete",
#endif
					"object_mask", 
					rbdeletebutton.left, 
					rbdeletebutton.top, 
					rbdeletebutton.right, 
					rbdeletebutton.bottom, 
					services::ccenter);
			}
		}
	}

	dc->RestoreDC(-1);
}


double tracing::CalcContractionsAverage(long iLeft, long iRight) const
{
	int numOfContractionsInCurWindow = 0;
	// Count contractions.
	const fetus*  f = get();
	long count = f->GetContractionsCount();
	for (long i = 0; i < count; i++)
	{
		const contraction&	ci = f->get_contraction(i);
		if (!ci.is_strike_out())
		{
			long p = ci.get_peak();
			if ((p * get_sample_rate()) > (iLeft * f->get_up_sample_rate()) && (p * get_sample_rate()) <= (iRight * f->get_up_sample_rate()))
			{
				numOfContractionsInCurWindow++;
			}
		}
	}
	double meanContractions = -1.0;
	if(numOfContractionsInCurWindow > 0)
	{
		meanContractions = GetExpandedViewCompleteLength() / (double)(60 * numOfContractionsInCurWindow);
	}
	
	return meanContractions;
}
// =====================================================================================================================
//    Draw contractions average information. We draw the contractions average based on the expanded lenght.The
//    calculation is simple: number of minutes of the expanded view(shown by the slider) divided by the number of
//    contractions in that view. dc: device context to draw into.
// =======================================================================================================================
void tracing::draw_contractions_average(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	CRect r3;

	const long d = 5;	
	ptTopLeft = ksubviews[2]->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = ksubviews[subViewsSize - 1]->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	CRect bounds = get_bounds(ocompletetracing);
	long sampleRate = get_sample_rate();
	long iright = get_index_from_x(bounds.right);
	long ileft = iright - (GetExpandedViewCompleteLength() * sampleRate);

	// Set graphics up.
	dc->SaveDC();

	// Determine where we want to draw.
	r1 = rect;
	r1.top = rect.bottom + 10;
	r1.bottom = r1.top + 50;

	double meanContractions = CalcContractionsAverage(ileft, iright);
	if (meanContractions != -1)
	{
		//sprintf(summaryText, "Mean Contraction Interval: Q %.1lf min", numOfContractionsInCurWindow > 0 ? get_displayed_length() / (double)(60 * numOfContractionsInCurWindow) : 0);
		sprintf(summaryText, "Mean Contraction Interval: Q %.1lf min", meanContractions);
	}
	else
	{
		strcpy(summaryText, "Mean Contraction Interval: none");
	}

	
	CBrush bBrush = RGB(128, 0, 128);

	dc->FillRect(&r2, CBrush::FromHandle((HBRUSH)GetStockObject(GRAY_BRUSH)));

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);
	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();

	
	r3.left = d;
	r3.right = bounds.Width() - d;
	r3.top = rect.bottom + 10;
	r3.bottom = r3.top + 80;
	dc->FillRect(r3, &bBrush);
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// Clean graphics up.
	dc->RestoreDC(-1);
}

void tracing::DrawSummaryPanel(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	CRect r3;

	const long d = 5;
	ptTopLeft = ksubviews[2]->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = ksubviews[subViewsSize - 1]->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	CRect tracingbounds = get_bounds(otracing);

	CRect bounds = get_bounds(ocompletetracing);
	long sampleRate = get_sample_rate();
	long iright = get_index_from_x(bounds.right);
	long ileft = iright - (GetExpandedViewCompleteLength() * sampleRate);

	// Set graphics up.
	dc->SaveDC();

	CBrush bBrush = RGB(255, 255, 255);
	CPen pline(PS_SOLID, 1, RGB(128, 128, 128));

	r3.left = d;
	r3.right = tracingbounds.Width() - d;
	r3.top = rect.bottom + 10;
	r3.bottom = r3.top + 80;

	// create and select a solid blue brush
	CBrush* pOldBrush = dc->SelectObject(&bBrush);

	// create and select a thick, black pen
	CPen* pOldPen = dc->SelectObject(&pline);

	dc->Rectangle(r3);
	//dc->FillRect(r3, &bBrush);
	dc->SelectObject(pOldBrush);
	dc->SelectObject(pOldPen);

	//Print Summary header
	CRect rSummaryHeader;
	rSummaryHeader.left = r3.left + d;
	rSummaryHeader.right = (tracingbounds.Width() / 4) - d;
	rSummaryHeader.top = r3.top + d;
	rSummaryHeader.bottom = rSummaryHeader.top + 12;

	services::select_font(80, "Arial Bold", dc);
	strcpy(summaryText, "30 Minute Summary");
	dc->DrawText(summaryText, &rSummaryHeader, DT_WORDBREAK);

	services::select_font(80, "Arial", dc);

	// Print mean contraction interval
	CRect rMeanCtr;
	rMeanCtr.left = rSummaryHeader.left;
	rMeanCtr.right = rSummaryHeader.right;
	rMeanCtr.top = rSummaryHeader.bottom + d;
	rMeanCtr.bottom = rMeanCtr.top + 12;

	double meanContractions = CalcContractionsAverage(ileft, iright);
	if (meanContractions != -1)
	{
		sprintf(summaryText, "Mean Contraction Interval: Q %.1lf min", meanContractions);
	}
	else
	{
		strcpy(summaryText, "Mean Contraction Interval: none");
	}

	dc->DrawText(summaryText, &rMeanCtr, DT_WORDBREAK);

	//Print mean baseline and variability
	if (is_visible(patterns_gui::tracing::wbaselinevariability))
	{
		CRect rMeanBL;
		rMeanBL.left = (tracingbounds.Width() / 2) + d;
		rMeanBL.right = ((tracingbounds.Width() / 4) * 3) - d;
		rMeanBL.top = rSummaryHeader.bottom + d;
		rMeanBL.bottom = rMeanBL.top + 12;

		// Get Baseline rate.
		int mean_baseline = -1;


		// Get Baseline rate.
		int mean_var = -1;

		get()->get_mean_baseline(ileft, iright, mean_baseline, mean_var);

		if (mean_baseline > 0)
		{
			sprintf(summaryText, "Mean Baseline : %d bpm", mean_baseline);
		}
		else
		{
			strcpy(summaryText, "Mean Baseline : N/A");
		}

		dc->DrawText(summaryText, &rMeanBL, DT_WORDBREAK);

		CRect rMeanBLVar;
		rMeanBLVar.left = (tracingbounds.Width() / 2) + d;
		rMeanBLVar.right = ((tracingbounds.Width() / 4) * 3) - d;
		rMeanBLVar.top = rMeanBL.bottom + d;
		rMeanBLVar.bottom = rMeanBLVar.top + 12;

		if (mean_var > 0)
		{
			sprintf(summaryText, "Mean Baseline Variability: %d", mean_var);
		}
		else
		{
			strcpy(summaryText, "Mean Baseline Variability : N/A");
		}

		dc->DrawText(summaryText, &rMeanBLVar, DT_WORDBREAK);
	}

	//Print Montevideo
	if (is_visible(patterns_gui::tracing::wmontevideo))
	{
		CRect rMvu;
		rMvu.left = rSummaryHeader.left;
		rMvu.right = rSummaryHeader.right;
		rMvu.top = rMeanCtr.bottom + d;
		rMvu.bottom = rMvu.top + 12;

		double montevideo = CalculateMontevideo(ileft, iright);
		sprintf(summaryText, "Mean Montevideo Units: %.0f per 10 min", montevideo);

		dc->DrawText(summaryText, &rMvu, DT_WORDBREAK);
	}
	
	// Clean graphics up.
	dc->RestoreDC(-1);
}

double tracing::CalculateMontevideo(long iLeft, long iRight) const
{
	const fetus*  f = get();
	long totPeak = 0;
	long count = f->GetContractionsCount();
	for (long i = 0; i < count; i++)
	{
		const contraction&	ci = f->get_contraction(i);
		if (!ci.is_strike_out())
		{
			long p = ci.get_peak();
			if ((p * get_sample_rate()) > (iLeft * f->get_up_sample_rate()) && (p * get_sample_rate()) <= (iRight * f->get_up_sample_rate()))
			{
				totPeak += f->get_height(ci);
			}
		}
	}

	double montevideo = (600 * totPeak) / (double)GetExpandedViewCompleteLength();
	return montevideo;
	
}
// =====================================================================================================================
//    Draw montevideo units
// =======================================================================================================================
void tracing::draw_montevideo_units(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	const long d = 5;

	ptTopLeft = ksubviews[2]->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = ksubviews[subViewsSize - 1]->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	CRect bounds = get_bounds(ocompletetracing);
	long iright = get_index_from_x(bounds.right);
	long ileft = iright - (GetCompleteLength() * get_sample_rate());

	// Set graphics up.
	dc->SaveDC();

	//// Count contractions.
	//
	//const fetus*  f = get();
	//long totPeak = 0;
	//long count = f->GetContractionsCount();
	//for (long i = 0; i < count; i++)
	//{
	//	const contraction&	ci = f->get_contraction(i);
	//	if (!ci.is_strike_out())
	//	{
	//		long p = ci.get_peak();
	//		if ((p * get_sample_rate()) > (ileft * f->get_up_sample_rate()) && (p * get_sample_rate()) <= (iright * f->get_up_sample_rate()))
	//		{
	//			totPeak += f->get_height(ci);
	//		}
	//	}
	//}

	//sprintf(summaryText, "Mean Montevideo Units: %.0f per 10 min", (600 * totPeak) / (double)get_displayed_length());
	

	double montevideo = CalculateMontevideo(ileft, iright);
	sprintf(summaryText, "Mean Montevideo Units: %.0f per 10 min", montevideo);
	
	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);

	// Determine where we want to draw.
	r1 = rect;

	r1.top = rect.bottom + 10 + 2 * (1 + r2.Height());
	if (get_type() == tnormal && is_visible(patterns_gui::tracing::wbaselinevariability))
	{
		r1.top += 1 + r2.Height();
	}

#ifdef patterns_parer_classification
	if (get_type() == tnormal && is_visible(patterns_gui::tracing::wparerclassification))
	{
		r1.top += 1 + r2.Height();
	}
#endif

	r1.bottom = r1.top + 1 + r2.Height();

	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// Clean graphics up.
	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    draw_baseline_average: show average baseline level and variability in window
// =======================================================================================================================
void tracing::draw_baseline_average(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	long ileft;
	long iright;
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	const long d = 5;

	ptTopLeft = ksubviews[2]->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = ksubviews[subViewsSize - 1]->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	CRect bounds = get_bounds(ocompletetracing);
	iright = get_index_from_x(bounds.right);
	ileft = iright - (GetExpandedViewCompleteLength() * get_sample_rate());

	// Set graphics up.
	dc->SaveDC();

	// Get Baseline rate.
	int mean_baseline = -1;


	// Get Baseline rate.
	int mean_var = -1;

	get()->get_mean_baseline(ileft, iright, mean_baseline, mean_var);

	if (mean_baseline > 0)
	{
		sprintf(summaryText, "Mean Baseline : %d bpm", mean_baseline);
	}
	else
	{
		strcpy(summaryText, "Mean Baseline : N/A");
	}

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);

	// Determine where we want to draw.
	r1 = rect;
	r1.top = rect.bottom + 10 + 1 + r2.Height();
	r1.bottom = r1.top + 1 + r2.Height();

	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// baseline var
	r1.top = rect.bottom + 10 + 2 * (1 + r2.Height());
	r1.bottom = r1.top + 1 + r2.Height();
	if (mean_var > 0)
	{
		sprintf(summaryText, "Mean Baseline Variability: %d", mean_var);
	}
	else
	{
		strcpy(summaryText, "Mean Baseline Variability : N/A");
	}

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);
	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// Clean graphics up.
	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    Draw any disconnection status.
// =======================================================================================================================
void tracing::draw_disconnected(CDC* dc) const
{
	if (has_subviews())
	{
		switch (get_type())
		{
		case tnormal:
			draw_disconnected_f(dc, 2);

		case tnormalnc:
			draw_disconnected_f(dc, 0);
			break;
		}
	}
	else if (!is_subview())
	{
		CRect rect;
		CRect rt = get_bounds(otracing);
		CRect rb = services::get_bitmap_rectangle("disconnected");

		rect = rt;
		if (get_number() == 0)
		{
			rect.right = rt.right - rb.Width() / 2;
		}
		else
		{
			rect.right = get_x_from_index(get_number() - 1) - rb.Width() / 2;
		}

		rect.left = rect.right - rb.Width();
		if (rect.left < rt.right)
		{
			services::draw_bitmap(dc, "disconnected", "m_disconnected", rect.left, rect.top, rect.right, rect.bottom, services::ccenter);
		}
	}
}

void tracing::draw_disconnected_f(CDC* dc, long i) const
{
	CRect rect;
	CRect rb = services::get_bitmap_rectangle("disconnected");
	CRect ri = ksubviews[i]->get_bounds(otracing);
	CRect ri1 = ksubviews[i + 1]->get_bounds(otracing);

	ksubviews[i]->ClientToScreen(&ri);
	ScreenToClient(&ri);
	ksubviews[i + 1]->ClientToScreen(&ri1);
	ScreenToClient(&ri1);

	if (ksubviews[i]->get_number() == 0)
	{
		rect.right = ri.right - rb.Width() / 2;
	}
	else
	{
		rect.right = ksubviews[i]->get_x_from_index(ksubviews[i]->get_number() - 1) - rb.Width() / 2;
	}

	rect.left = rect.right - rb.Width();
	if (rect.left < ri.right)
	{
		rect.top = ri.top;
		rect.bottom = ri1.bottom;
		services::draw_bitmap(dc, "disconnected", "m_disconnected", rect.left, rect.top, rect.right, rect.bottom, services::ccenter);
	}
}

// =====================================================================================================================
//    Draw any event-detection-related message.
// =======================================================================================================================
void tracing::draw_event_message(CDC* dc) const
{
	const fetus*  curFetus = get();
	CRect rect = get_bounds(oprimary);
	string s;
	CSize ssize;

	if ((get_type() == tfhr) && (curFetus->has_reset_date()))
	{
		rect = get_bounds(opictogram);
		rect.bottom = get_bounds(oprimary).bottom;	
		rect.right = min(rect.right, get_x_from_index(curFetus->get_hr_sample_rate() * (long)(curFetus->get_reset_date() - curFetus->get_start_date())));
		dc->FillRect(&rect, &CBrush(RGB(244, 244, 244)));
	}

	dc->SaveDC();
	bool exportEnabled = has_parent() ? get_parent()->m_exportEnabled : m_exportEnabled;
	rect = get_bounds(oprimary);
	if (has_twins())
	{
		s = exportEnabled?
			 "Event detection and export are not enabled for multiple gestations" : "Event detection is not enabled for multiple gestations.";
	}
	else if (!is_singleton())
	{
		s = exportEnabled?
			"Event detection and export are not enabled for patients whose number of fetuses is 0 or undetermined" : "Event detection is not enabled for patients whose number of fetuses is 0 or undetermined";
	}
	else if (curFetus->has_cutoff_date())
	{
		s = exportEnabled?
			"Event detection and export are not enabled before 36 weeks of gestation" : "Event detection is not enabled before 36 weeks of gestation.";
		rect.right = min(rect.right, get_x_from_index(curFetus->get_hr_sample_rate() * (long)(curFetus->get_cutoff_date() - curFetus->get_start_date())));
	}
	else
	{
		s = exportEnabled?
			"Event detection and export are not enabled for patients whose gestational age is undetermined" : "Event detection is not enabled for patients whose gestational age is undetermined";
	}

	s += "   |   ";

	ssize = dc->GetTextExtent(s.c_str());
	rect.top -= ssize.cy / 2;
	rect.bottom += ssize.cy - ssize.cy / 2;
	if (rect.right > rect.left)
	{
		CRgn rclip;

		rclip.CreateRectRgn(rect.left, rect.top, rect.right, rect.bottom);
		rclip.OffsetRgn(dc->GetViewportOrg());
		dc->SelectClipRgn(&rclip, RGN_AND);
		for (long x = -(get_offset() % ssize.cx); x < rect.right; x += ssize.cx)
		{
			dc->TextOut(x, rect.top, s.c_str());
		}
	}

	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    Draw events for one fetus. See draw_contractions().We also keep the rectangle for the current delete button so that
//    methods find() and get_bounds() don't have to replicate the logic.
// =======================================================================================================================
void tracing::draw_events(CDC* dc, const fetus* curFetus, bool iup) const
{
	if (is_visible(whideevents))
	{
		return;
	}

	if (curFetus->get_number_of_fhr() == 0)
	{
		return;
	}

#if !defined(patterns_retrospective) || defined(OEM_patterns)
	if (!is_singleton() || !curFetus->has_cutoff_date())
	{
		if (iup)
		{
			draw_event_message(dc);
		}

		return;
	}
#endif

	CBrush bfull = RGB(19, 47, 119);
	CBrush bpale = RGB(105, 139, 231);

	bool icompressed = is_compressed();
	long hdelete = services::get_bitmap_rectangle("object_delete").Height();
	CPen pborder(PS_SOLID, 0, RGB(30, 72, 183)), ppeak(PS_DOT, 0, RGB(30, 72, 183));

	CRect rect = get_bounds(oview);

	long ileft = get_index_from_x(rect.left);
	long iright = get_index_from_x(rect.right);

	dc->SaveDC();
	dc->SetBkMode(TRANSPARENT);
	dc->SelectObject(&pborder);

#if !defined(patterns_retrospective) || defined(OEM_patterns)
	if (curFetus->get_number_of_fhr() > 0)
	{
		draw_event_message(dc);
	}
#endif

	for (long i = 0, n = curFetus->GetEventsCount(); i < n; i++)
	{
		const event&  ei = curFetus->get_event(i);

		if (ei.get_end() >= ileft && ei.get_start() <= iright && is_visible(ei))
		{
			bool iselected = iup && get_selection_type() == cevent && get_selection() == i;
			CRect ri = get_bounds(iup ? oprimary : osecondary);

			ri.left = get_x_from_index(ei.get_start());
			ri.right = get_x_from_index(ei.get_end());

			string nt;

			// Draw event proper.
			ri.bottom = get_bounds(otracing).bottom + 6;
			if (is_visible(wceventbars) || !icompressed)
			{
				string s;

				if (icompressed)
				{
					s += "h";
				}

				if (iselected)
				{
					s += "s";
				}

				if (ei.is_strike_out())
				{
					nt = "so_event";
				}
				else if (ei.is_acceleration())
				{
					if (ei.is_final())
					{
						nt = "acceleration";
					}
					else
					{
						nt = "acceleration_h";
					}
				}
				else
				{
					if (ei.is_final())
					{
						nt = "deceleration";
					}
					else
					{
						nt = "deceleration_h";
					}
				}

				if (!s.empty())
				{
					nt += "_";
					nt += s;
				}

				draw_bitmap_rectangle(dc, nt, 1, ri);

				// Draw delete && accept button if necessary.
#if defined(OEM_patterns) || defined(patterns_retrospective)
				if (iselected && !icompressed && can_delete())
#else
				if (iselected && !icompressed && can_delete() && !ei.is_strike_out())
#endif
				{
					// Draw delete button is need be
					rbdeletebutton = ri;
					rbdeletebutton.top = ri.bottom - 10 - hdelete;
					rbdeletebutton.left += (ri.Width() - hdelete) / 2;
					rbdeletebutton.right -= (ri.Width() - hdelete) / 2;
					services::draw_bitmap(
						dc,
#if defined(OEM_patterns) || defined(patterns_retrospective)
						ei.is_strike_out() ? (q == qdownindelete ? "object_undo_selected" : "object_undo") : (q == qdownindelete ? "object_delete_selected" : "object_delete"),
#else
						q == qdownindelete ? "object_delete_selected" : "object_delete",
#endif
						"object_mask", 
						rbdeletebutton.left, 
						rbdeletebutton.top, 
						rbdeletebutton.right, 
						rbdeletebutton.bottom, 
						services::ccenter);

					// Draw validate button is need be
#if defined(OEM_patterns) || defined(patterns_retrospective)
					if (!ei.is_strike_out() && ei.is_noninterp() && !ei.is_confirmed())
#else
					if (ei.is_noninterp() && !ei.is_confirmed())
#endif
					{
						rbacceptbutton = rbdeletebutton;
						rbacceptbutton.top -= 4 + hdelete;
						rbacceptbutton.bottom -= 4 + hdelete;
						services::draw_bitmap(dc, q == qdowninaccept ? "object_accept_selected" : "object_accept", "object_mask", rbacceptbutton.left, rbacceptbutton.top, rbacceptbutton.right, rbacceptbutton.bottom, services::ccenter);
					}
				}
			}

			// Draw pictogram.
			nt = "e";
			if (icompressed)
			{
				nt += "h";
			}

			nt += "_";
			if (ei.is_strike_out())
			{
				nt += "so_event";
			}
			else
			{
				if (ei.is_noninterp() && !ei.is_confirmed())
				{
					if (ei.is_acceleration())
					{
						nt += "ni_acceleration";
					}
					else if (ei.is_deceleration())
					{
						nt += "ni_deceleration";
					}
				}
				else
				{
					switch (ei.get_type())
					{
					case event::tacceleration:
						nt += "acceleration";
						break;

					case event::taccelerationni:
						nt += "ni_acceleration";
						break;

					case event::tearly:
						nt += "early";
						break;

					case event::ttypical:
						nt += "typical";
						break;

					case event::tatypical:
						nt += "atypical";
						if ((ei.get_atypical() > 0) && ((ei.is_loss_var() || ei.is_sixties() || ei.is_biphasic())))
						{
							nt += "_critic";
						}
						break;

					case event::tlate:
						nt += "late";
						break;

					case event::tnadeceleration:
						nt += "na_deceleration";
						break;

					case event::tnideceleration:
						nt += "ni_deceleration";
						break;

					case event::tbaseline:
						nt += "baseline";
						break;

					case event::trepaired:
						nt += "repaired";
						break;

					case event::terror:
						nt += "error";
						break;

					case event::tprolonged:
						nt += "prolonged";
						if (ei.get_atypical() > 0)
						{
							nt += "_atypical";
							if (ei.is_loss_var() || ei.is_sixties() || ei.is_biphasic())
							{
								nt += "_critic";
							}
						}
						break;
					}
				}
			}

			ri.top = get_bounds(opictogram).top;
			ri.bottom = get_bounds(opictogram).bottom;
			ri.left -= 50;
			ri.right += 50;
			services::draw_bitmap(dc, nt, "", ri.left, ri.top, ri.right, ri.bottom, services::ccenter);
		}
	}

	dc->RestoreDC(-1);
}

void tracing::draw_event_information(CDC* dc, long i) const
{
	const event&  curEvent = get()->get_event(i);

	CRect rect(0, 0, 400, 50);
	CRect re; 
	CRect rt = get_bounds(otracing);

	re = rt;
	re.left = get_x_from_index(curEvent.get_start());
	re.right = get_x_from_index(curEvent.get_end());
	if (re.left < rt.right && re.right > rt.left)
	{
		const long d = 5;
		string eventInfoText;

		// Set graphics up.
		dc->SaveDC();

		// Compose text.
		{
			char partialInfoText[1000];
			double a;

			// The type of event
			if (curEvent.is_noninterp() && !curEvent.is_confirmed())
			{
				if (curEvent.is_deceleration())
				{
					eventInfoText = "Deceleration";
				}
				else if (curEvent.is_acceleration())
				{
					eventInfoText = "Acceleration";
				}

				eventInfoText += " (non interpretable)";
			}
			else
			{
				eventInfoText = services::event_type_to_string(curEvent.get_type());
				if (curEvent.get_atypical() > 0)
				{
					int natyp = 0;
					if (curEvent.is_loss_var())
					{
						natyp++;
					}

					if (curEvent.is_sixties())
					{
						natyp++;
					}

					if (curEvent.is_biphasic())
					{
						natyp++;
					}

					if (curEvent.is_late())
					{
						natyp++;
					}

					if (curEvent.is_loss_rise())
					{
						natyp++;
					}

					if (curEvent.is_lower_bas())
					{
						natyp++;
					}

					if (curEvent.is_prol_sec_rise())
					{
						natyp++;
					}

					if (curEvent.is_slow_return())
					{
						natyp++;
					}

					if (natyp > 1)
					{
						eventInfoText += "s";
					}
				}
			}

			if (curEvent.is_strike_out())
			{
				eventInfoText += " (SO)";
			}

			// The non reassuring features details
			if ((curEvent.get_atypical() > 0) && (!curEvent.is_noninterp() || curEvent.is_confirmed()))	// this way include display of non-reassuring features for prolonged
			{
				int natyp = 0;

				string at;

				if (curEvent.is_loss_var())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "Loss of variability within deceleration";
				}

				if (curEvent.is_sixties())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "60s";
				}

				if (curEvent.is_biphasic())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "Biphasic shape";
				}

				if (curEvent.is_late())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "Late timing";
				}
				if (curEvent.is_loss_rise())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "Loss of rise";
				}
				if (curEvent.is_lower_bas())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "Lower baseline";
				}
				if (curEvent.is_prol_sec_rise())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "Prolonged secondary rise";
				}
				if (curEvent.is_slow_return())
				{
					if (natyp++ > 0)
					{
						at += ", ";
					}

					at += "Slow return";
				}

				if (natyp > 0)
				{
					eventInfoText += "\r\n(" + at + ")";
				}
			}

			// The length of the event
			sprintf(partialInfoText, "\r\n%ld sec", (long)(curEvent.get_length() / get_sample_rate()));
			eventInfoText += partialInfoText;

			// The height of the event
			if (curEvent.is_height_set())
			{
				sprintf(partialInfoText, "\r\n%ld bpm", (long)curEvent.get_height());
				eventInfoText += partialInfoText;
			}

			// The confidence level
			if (curEvent.is_confidence_set())
			{
				if (curEvent.is_noninterp() && !curEvent.is_confirmed())
				{
					eventInfoText += "\r\nConfidence: n/a";
				}
				else
				{
					a = curEvent.get_confidence() > 0 ? 100.0 * (curEvent.get_confidence() - 0.5) / 10 : 0;
					sprintf(partialInfoText, "\r\nConfidence: %.1lf/5", a);
					eventInfoText += partialInfoText;
				}
			}

			// End lag
			if ((is_visible(wdecellag)) && (curEvent.get_lag() > 0))
			{
				sprintf(partialInfoText, "\r\nEnd lag: %d sec.", (curEvent.get_lag() / get_sample_rate()));
				eventInfoText += partialInfoText;
			}

			// Repair percentage
			if (is_visible(wrepair) && curEvent.is_repair_set())
			{
				sprintf(partialInfoText, "\r\nRepair: %.0lf%%", curEvent.get_repair() * 100);
				eventInfoText += partialInfoText;
			}

			if (curEvent.is_strike_out())
			{
				if (curEvent.has_extension("strike_out"))
				{
					eventInfoText += "\r\n" + curEvent.get_extension("strike_out");
				}
			}
			else
			{
				if (curEvent.is_confirmed() && curEvent.has_extension("confirmed"))
				{
					eventInfoText += "\r\n" + curEvent.get_extension("confirmed");
				}
				if(curEvent.has_extension("restored"))
				{
					eventInfoText += "\r\n" + curEvent.get_extension("restored");
				}
			}
		}

		// Determine where we want to draw.
		{
			CRect r1 = rect;

			r1.right -= 2 * d;
			dc->DrawText(eventInfoText.c_str(), &r1, DT_CALCRECT | DT_WORDBREAK);
			rect.bottom = r1.bottom + d;
			rect.right = r1.left + r1.Width() + 2 * d;
		}

		if (re.right + 2 * d + rect.Width() > rt.right)
		{
			rect.left = re.left - d - rect.Width();
		}
		else
		{
			rect.left = re.right + d;
		}

		rect.right += rect.left;
		rect.top = rt.bottom - d - rect.Height();
		rect.bottom += rect.top;

		// Draw the rectangle and text.
		draw_bitmap_rectangle(dc, "information", 1, rect);
		rect.left += d;
		rect.right -= d;
		rect.top += d / 2;
		rect.bottom -= d / 2;
		dc->DrawText(eventInfoText.c_str(), rect, DT_WORDBREAK);

		// Clean graphics up.
		dc->RestoreDC(-1);
	}
}

void tracing::draw_contraction_information(CDC* dc, long i) const
{
	const contraction&	ctr = get()->get_contraction(i);

	CRect rect(0, 0, 400, 50), rc, rt = get_bounds(otracing);

	rc = rt;
	rc.left = get_x_from_index(ctr.get_start());
	rc.right = get_x_from_index(ctr.get_end());

	if (rc.left < rt.right && rc.right > rt.left)
	{
		const long d = 5;
		string contractionInfoText;

		// Set graphics up.
		dc->SaveDC();

		// Compose text.
		{
			char partialInfoText[1000];

			// The type of contraction
			contractionInfoText = "Contraction";

			if (ctr.is_strike_out())
			{
				contractionInfoText += " (SO)";
			}

			// The length of the contraction
			sprintf(partialInfoText, "\r\n%ld sec", (long)(ctr.get_length() / get_sample_rate()));
			contractionInfoText += partialInfoText;

#ifdef _DEBUG
			// The height of the contraction
			sprintf(partialInfoText, "\r\n%ld mmHg", (long)get()->get_height(ctr));
			contractionInfoText += partialInfoText;
#endif

			if (ctr.is_strike_out())
			{
				if (ctr.has_extension("strike_out"))
				{
					contractionInfoText += "\r\n" + ctr.get_extension("strike_out");
				}
			}
			else if(ctr.has_extension("restored"))
			{
				contractionInfoText += "\r\n" + ctr.get_extension("restored");
			}
		}

		// Determine where we want to draw.
		{
			CRect r1 = rect;

			r1.right -= 2 * d;
			dc->DrawText(contractionInfoText.c_str(), &r1, DT_CALCRECT | DT_WORDBREAK);
			rect.bottom = r1.bottom + d;
			rect.right = r1.left + r1.Width() + 2 * d;
		}

		if (rc.right + 2 * d + rect.Width() > rt.right)
		{
			rect.left = rc.left - d - rect.Width();
		}
		else
		{
			rect.left = rc.right + d;
		}

		rect.right += rect.left;
		rect.top = rt.bottom - d - rect.Height();
		rect.bottom += rect.top;

		// Draw the rectangle and text.
		draw_bitmap_rectangle(dc, "information", 1, rect);
		rect.left += d;
		rect.right -= d;
		rect.top += d / 2;
		rect.bottom -= d / 2;
		dc->DrawText(contractionInfoText.c_str(), rect, DT_WORDBREAK);

		// Clean graphics up.
		dc->RestoreDC(-1);
	}
}

// =====================================================================================================================
//    Draw the tracing's grid. We facsimile as much as possible the grids of the us and international paper, both for fhr
//    and up tracings. cbase: base colour depending on the paper type.Other colours are derived from this one. cmajor:
//    colour for minor divisions. cminor: colour for minor divisions. d: interval between minor divisons, in seconds.
//    dif: difference, with respect to i/f, in seconds, between the current fetus' starting date and the last round hour,
//    that is.This, added to i/f, lets us determine major divisions. dy: interval between horizontal(y axis) lines, in
//    units. f: sample rate(frequency). fnumbers: font for numbers on the tracing. idates: index of sample where a date
//    should be displayed.See xnumebrs. [ileft, iright]: interval, in sample indices, of the currently viewed part of the
//    tracing. pmajor, pminor: pens for the cmajor and cminor colours. rect: bounds of the tracing. xnumbers: horizontal
//    positions where vertical units should be displayed.This is computed when drawing vertical lines in order to
//    centralize computing the vertical grid.As drawing itself takes computationally much longer than growing a vector
//    does, so we do not worry about this.
// =======================================================================================================================
void tracing::draw_grid(CDC* dc) const
{
	if (!is_grid_visible())
	{
		return;
	}

	// !!!!!!!!!!!!!!!!!! WARNING !!!!!!!!!!!!!!!!!!!
	get_scaling();				// Do not remove, it ensure the data is properly initialize OR ELSE sometime the grid or the top fhr part is badly drawn...

	// !!!!!!!!!!!!!!!!!! WARNING !!!!!!!!!!!!!!!!!!! ;
	// Colors used to draw the grid
	COLORREF cbase = draw_grid_base_colour();
	COLORREF cdate = RGB(127, 127, 127);
	COLORREF cmajor = cbase;
	COLORREF cminor = services::make_colour(cbase, 40);

	// Pens used to draw the grid
	CPen pmajor(PS_SOLID, 0, cmajor);
	CPen pminorh(PS_SOLID, 0, services::make_colour(cminor, draw_grid_vertical_shade()));
	CPen pminorv(PS_SOLID, 0, cminor);
	long lSampleRate = get_sample_rate();
	CRect rect = get_bounds(otracing);

	dc->SaveDC();

	// Horizontal lines for vertical(fhr or up) axis ;
	// Interval between horizontal(y axis) lines, in units.
	long dy = get_type() == tcr ? get_maximum() : (get_type() == tup || get_paper() == pusa) ? 10 : 5;

	if (is_compressed())
	{
		long ybottom = get_y_from_units(rect, get_minimum());
		long ytop = get_y_from_units(rect, get_maximum());

		dc->FillRect(&CRect(rect.left, ytop, rect.right, ybottom), &CBrush(services::make_colour(cbase, 10)));
		dc->SelectObject(&pminorh);
		dc->MoveTo(rect.left, ytop);
		dc->LineTo(rect.right, ytop);
		dc->MoveTo(rect.left, ybottom);
		dc->LineTo(rect.right, ybottom);

		if (get_type() == tcr && is_visible(wcrtracing) && get_cr_threshold() > 0)
		{
			long ythreshold = get_y_from_units(rect, get_cr_threshold());

			dc->SelectObject(&pmajor);
			dc->MoveTo(rect.left, ythreshold);
			dc->LineTo(rect.right, ythreshold);
		}
	}
	else
	{
		int pos = 0;
		for (long i = get_minimum(); i <= get_maximum(); i += dy)
		{
			long yi = get_y_from_units(rect, i);

			bool major =
				(
				(i == get_minimum())
				||	(i == get_maximum())
				||	(get_type() == tup && ((i % 25) == 0))
				||	(get_type() == tfhr && get_paper() == pinternational && (i == 80 || i == 120 || i == 160))
				||	(get_type() == tfhr && get_paper() == pusa && ((i % 60) == 0))
				||	(get_type() == tcr && i == get_cr_threshold())
				);

			dc->SelectObject(major ? &pmajor : &pminorh);

			dc->MoveTo(rect.left, yi);
			dc->LineTo(rect.right, yi);
		}
	}

	long ileft = get_index_from_x(rect.left);
	long iright = get_index_from_x(rect.right);

	if (iright <= ileft)
	{
		iright = ileft + 1;
	}

	// Calculate the interval between vertical lines
	long dstep;

	// Calculate the interval between vertical lines
	long dfreqScale;

	// Calculate the interval between vertical lines
	long dfreqDates;
	double dPixelPerMinute = 60 * (double)(lSampleRate * (rect.right - rect.left)) / (double)(iright - ileft);

	if (!is_compressed())
	{
		if (dPixelPerMinute >= 20)
		{
			dstep = 10;			// Every 10 seconds
			dfreqScale = 18;	// Every 18 steps(3 minutes)
			dfreqDates = 18;	// Every 18 steps(3 minutes)
		}
		else
		{
			dstep = 60;			// Every 60 seconds
			dfreqScale = 10;	// Every 10 steps(10 minutes)
			dfreqDates = 10;	// Every 10 steps(10 minutes)
		}
	}
	else if (dPixelPerMinute >= 2)
	{
		dstep = 1800;			// Every 30 minutes
		dfreqScale = 2;			// Every 4 steps(1 hours)
		dfreqDates = 2;			// Every 4 steps(1 hours)
	}
	else if (dPixelPerMinute >= 1)
	{
		dstep = 3600;			// Every 60 minutes
		dfreqScale = 2;			// Every 2 steps(2 hours)
		dfreqDates = 2;			// Every 2 steps(2 hours)
	}
	else
	{
		dstep = 7200;			// Every 240 minutes
		dfreqScale = 2;			// Every 2 steps(8 hours)
		dfreqDates = 2;			// Every 2 steps(8 hours)
	}

	// Interval between horizontal(y axis) scale numbers, in units.
	long dynumbers = get_type() == tcr ? get_maximum() : get_type() == tfhr && get_paper() == pusa ? 30 : (get_type() == tcr ? 10 : 25);

	// Number of seconds to adjust the start time of the tracing to the previous hour
	long startTimeAdjustment = get()->has_start_date() ? services::date_to_time_of_day(get()->get_start_date()) % 86400 : ileft / lSampleRate;

	// We don't display most of the elements if there is no real tracing
	bool bValidTracing = get()->has_start_date();

	// We don't display the scale if the tracing height is too small to do it properly
	bool bDisplayScale = bValidTracing && (dynumbers * get_bounds(otracing).Height() / (get_maximum() - get_minimum()) > 10);

	if ((get_type() == tcr && !is_visible(wcrtracing)))
	{
		// We don't dislay the scale for tcr when the graph is turned off
		bDisplayScale = false;
	}

	bool bDisplayDates = bValidTracing && get_type() == tfhr;

	if (is_compressed())
	{
		dc->SetBkMode(TRANSPARENT);
	}
	else
	{
		dc->SetBkMode(OPAQUE);
	}
	
	dc->SetTextAlign(TA_CENTER | TA_BASELINE);
	

	long nRepetition = ((ileft / lSampleRate) + startTimeAdjustment) / dstep;
	while (true)
	{
		long i = ((nRepetition * dstep) - startTimeAdjustment) * lSampleRate;
		if (i > iright)
		{
			break;
		}

		if (i > ileft)
		{
			long x = get_x_from_index(i);

			// Vertical lines
			dc->SelectObject(!bValidTracing || ((nRepetition * dstep) % 60) ? &pminorv : &pmajor);
			dc->MoveTo(x, rect.top);
			dc->LineTo(x, rect.bottom - 1);

			// Vertical scales
			if (bDisplayScale && (nRepetition % dfreqScale == 0))
			{
				dc->SetTextColor(cmajor);

				for (long j = get_minimum(); j <= get_maximum(); j += dynumbers)
				{
					CString t;
					t.Format("%ld", (long)j);
					dc->TextOut(x - 20, get_y_from_units(rect, j) + 3, t);
				}
			}

			// Dates and times
			if (bDisplayDates)
			{
				dc->SetTextColor(cdate);

				if (nRepetition % dfreqDates == 0)
				{
					date d = get()->get_start_date() + (i / lSampleRate);
					dc->TextOut(x, rect.bottom + 14, services::date_to_string(d, is_compressed() ? services::fnormal : (d % 900 == 0 ? services::fnormal : services::ftime)).c_str());
				}

				if ((is_compressed()) && (nRepetition % dfreqDates != 0))
				{
					dc->TextOut(x, rect.bottom + 14, (string("+") + services::span_to_string((nRepetition % dfreqDates) * dstep, dstep < 3600 ? services::fminutes : services::fhours)).c_str());
				}
			}
		}

		// Next
		++nRepetition;
	}
	dc->RestoreDC(-1);
}

COLORREF tracing::draw_grid_base_colour(void) const
{
	//return get_type() == tcr || !is_visible(wbaselines) ? RGB(127, 127, 127) : (get_paper() == pusa ? RGB(148, 102, 127) : RGB(83, 140, 107));
	return get_type() == tcr || get_paper() == pusa ? RGB(148, 102, 127) : RGB(83, 140, 107);
}

long tracing::draw_grid_vertical_shade(void) const
{
	long r = 100;

	if (!is_compressed())
	{
		double d = (get_type() != tup && get_paper() != pusa ? 5.0 : 10.0) * (double)get_bounds(otracing).Height() / (double)(get_maximum() - get_minimum());
		double dhigh = 4;
		double dlow = 0.5;

		if (d < dlow)
		{
			r = 0;
		}
		else if (d <= dhigh)
		{
			r = (long)(100.0 * (d - dlow) / (dhigh - dlow));
		}
	}

	return r;
}

// =====================================================================================================================
//    Draw tracing-wide guides. If the cursor over the current selection, we display lines across the expanded view to
//    make it clearer to the user where exactly an object falls in al tracings.
// =======================================================================================================================
void tracing::draw_guides(CDC* dc) const
{
	if (is_visible(wguides) && (g1 != g2 || is_zoomed() || is_zooming()))
	{
		CPen pen(PS_SOLID, 0, RGB(127, 127, 127));
		CRect rect(get_x_from_index(g1), get_subview_bounds(0).top, get_x_from_index(g2), get_subview_bounds(1).bottom);

		dc->SaveDC();
		dc->SelectObject(&pen);
		if (g1 != g2)
		{
			dc->MoveTo(rect.TopLeft());
			dc->LineTo(rect.left, rect.bottom);
			dc->MoveTo(rect.right - 1, rect.top);
			dc->LineTo(rect.right - 1, rect.bottom);
		}

		if ((is_zoomed() || is_zooming()) && ksubviews.size() > 2)
		{
			const tracing*	s2 = ksubviews[2];
			CPoint p1;
			CPoint p2;

			p1 = CPoint(s2->get_x_from_index(zleft), 0);
			p2 = CPoint(s2->get_x_from_index(zright), 0);
			s2->ClientToScreen(&p1);
			ScreenToClient(&p1);
			s2->ClientToScreen(&p2);
			ScreenToClient(&p2);
			rect = CRect(p1.x, get_subview_bounds(2).top, p2.x, get_subview_bounds((long)ksubviews.size() - 1).bottom);
			dc->MoveTo(rect.TopLeft());
			dc->LineTo(rect.left, rect.bottom);
			dc->MoveTo(rect.right - 1, rect.top);
			dc->LineTo(rect.right - 1, rect.bottom);
		}

		dc->RestoreDC(-1);
	}
}

string classLevel_string(int v)
{
	switch (v)
	{
	case -1:
		return "Undefined";

	case 0:
		return "Green";

	case 1:
		return "Blue";

	case 2:
		return "Yellow";

	case 3:
		return "Orange";

	case 4:
		return "Red";

	default:
		return "Unknown";
	}
}

string varLevel_string(int v)
{
	switch (v)
	{
	case -1:
		return "Undefined";

	case 0:
		return "Normal";

	case 1:
		return "Minimal";

	case 2:
		return "Absent";

	case 3:
		return "Marked";

	default:
		return "Unknown";
	}
}

string basLevel_string(int v)
{
	switch (v)
	{
	case -1:
		return "Undefined";

	case 0:
		return "Tachy";

	case 1:
		return "Normal";

	case 2:
		return "Mild";

	case 3:
		return "Moderate";

	case 4:
		return "Severe";

	default:
		return "Unknown";
	}
}

string decelLevel_string(int v)
{
	switch (v)
	{
	case -1:
		return "Undefined";

	case 0:
		return "None";

	case 1:
		return "Mild";

	case 2:
		return "Moderate";

	case 3:
		return "Severe";

	default:
		return "Unknown";
	}
}

#ifdef patterns_parer_classification
COLORREF classification_color(int v)
{
	switch (v)
	{
	case 0:
		return RGB(34, 139, 34);

	case 1:
		return RGB(0, 0, 205);

	case 2:
		return RGB(255, 200, 0);

	case 3:
		return RGB(238, 118, 0);

	case 4:
		return RGB(205, 0, 0);

	default:
		return RGB(255, 255, 255);
	}
}

// =====================================================================================================================
//    Draw the classification as per Parer rules
// =======================================================================================================================
void tracing::draw_parer_classification(CDC* dc) const
{
	long n = (long)ksubviews.size();
	char summaryText[1000];
	CPoint p1;
	CPoint p2;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	const long d = 5;

	// Colors used to display classification
	COLORREF cgreen = RGB(34, 139, 34);
	COLORREF cblue = RGB(0, 0, 205);
	COLORREF cyellow = RGB(255, 200, 0);
	COLORREF corange = RGB(238, 118, 0);
	COLORREF cred = RGB(205, 0, 0);

	p1 = ksubviews[2]->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&p1);
	p2 = ksubviews[n - 1]->get_bounds(oslider).BottomRight();
	ksubviews[n - 1]->ClientToScreen(&p2);
	rect = CRect(p1, p2);
	ScreenToClient(&rect);

	CRect bounds = get_bounds(ocompletetracing);
	long iright = get_index_from_x(bounds.right);
	long ileft = iright - (GetCompleteLength() * get_sample_rate());

	// Set graphics up.
	dc->SaveDC();

	long basLevel;
	long basVar;
	long late;
	long variable;
	long prolonged;

	long oClass = (!is_singleton() || has_debug_keys()) ? patterns_classifier::CParerClassifier::t_undef : get()->GetParerClassification(iright, &basLevel, &basVar, &late, &variable, &prolonged);

	// long oClass = get().getParerClass(iright);
	strcpy(summaryText, "Classification : ");
	switch (oClass)
	{
	case patterns_classifier::CParerClassifier::t_green:
		dc->SetTextColor(cgreen);
		sprintf(summaryText, "%sGREEN", summaryText);
		break;

	case patterns_classifier::CParerClassifier::t_blue:
		dc->SetTextColor(cblue);
		sprintf(summaryText, "%sBLUE", summaryText);
		break;

	case patterns_classifier::CParerClassifier::t_yellow:
		dc->SetTextColor(cyellow);
		sprintf(summaryText, "%sYELLOW", summaryText);
		break;

	case patterns_classifier::CParerClassifier::t_orange:
		dc->SetTextColor(corange);
		sprintf(summaryText, "%sORANGE", summaryText);
		break;

	case patterns_classifier::CParerClassifier::t_red:
		dc->SetTextColor(cred);
		sprintf(summaryText, "%sRED", summaryText);
		break;

	default:
		sprintf(summaryText, "%sN/A", summaryText);
		break;
	}

	if (is_visible(patterns_gui::tracing::wparerinfo))
	{
		sprintf(summaryText, "%s (BV=%s - BL=%s || V=%s - L=%s - P=%s)", summaryText, varLevel_string(basVar).c_str(), basLevel_string(basLevel).c_str(), decelLevel_string(variable).c_str(), decelLevel_string(late).c_str(), decelLevel_string(prolonged).c_str());
	}

	dc->DrawText(summaryText, &r2, DT_CALCRECT);

	// Determine where we want to draw.
	r1 = rect;

	r1.top = rect.bottom + 10 + 2 * (1 + r2.Height());
	if (get_type() == tnormal && is_visible(patterns_gui::tracing::wbaselinevariability))
	{
		r1.top += 1 + r2.Height();
	}

	r1.bottom = r1.top + 1 + r2.Height();

	r0 = get_bounds(otracing);
	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// Clean graphics up.
	dc->RestoreDC(-1);
}
#endif

// =====================================================================================================================
//    Draw the slider over the subviews.
// =======================================================================================================================
void tracing::draw_slider(CDC* dc) const
{
	long n = (long)ksubviews.size();
	CPoint p1;
	CPoint p2;
	CRect rect;

	p1 = ksubviews[2]->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&p1);
	p2 = ksubviews[n - 1]->get_bounds(oslider).BottomRight();
	p2.y += 4;
	ksubviews[n - 1]->ClientToScreen(&p2);
	rect = CRect(p1, p2);
	ScreenToClient(&rect);
	draw_bitmap_rectangle(dc, "slider", 32, &rect);
	if (IsToggleButtonAvailable())
	{
		int rectLeft = m_bLeftPartShown ? rect.left : rect.left + rect.Width() / 2;
		int rectRight = m_bLeftPartShown ? rect.left + rect.Width() / 2 : rect.right;
		CRect halfRect(rectLeft, rect.top - 2, rectRight - 1, rect.bottom - 2);
		CDrawingManager dm(*dc);
		BOOL rightShadow = m_bLeftPartShown ? TRUE : FALSE;
		//dm.DrawShadow(halfRect, 6, 100, 50, 0, 0, -1, rightShadow);
		CRect shadowRect(halfRect);
		int w = shadowRect.Width() + 2;
		int count = 1;
		shadowRect.top += 1;
		if (m_bLeftPartShown)
			shadowRect.right += 1;
		else
			shadowRect.left -= 1;
		while (count < w)
		{
			count++;
			dm.DrawShadow(shadowRect, 1, 100, 92, 0, 0, -1, rightShadow);
			if (m_bLeftPartShown)
			{
				shadowRect.right++;
			}
			else
			{
				shadowRect.left--;
			}

		}
		dm.DrawShadow(halfRect, 6, 100, 50, 0, 0, -1, rightShadow);
		halfRect.top += 2;
		COLORREF color = get_type() == tcr || get_paper() == pusa ? RGB(148, 102, 127) : RGB(83, 140, 107);
		CBrush brush(color);
		dc->FrameRect(halfRect, &brush);
	}
	if (!m_bIsTogleZoomDisabled)
		DrawToogleButton(dc, rect);
}

void tracing::DrawToogleButton(CDC* dc, const CRect& sliderRect) const
{
	int toggleHeight = sliderRect.Height() / 8;
	CRect toggleRect(sliderRect.left + 2, sliderRect.top - toggleHeight - 4, sliderRect.right - 2, sliderRect.top - 4);
	m_toggleRect = toggleRect;
	COLORREF cbase = draw_grid_base_colour();
	COLORREF cmajor = cbase;
	COLORREF cminor = RGB(234, 234, 234);//services::make_colour(cbase, 40);

	COLORREF darkGrey = cbase;//RGB(127, 127, 127);
	COLORREF grey = RGB(255, 255, 255);

	CPen pen(PS_SOLID, 2, darkGrey);
	CBrush brush(grey);
	CPen* oldPen = dc->SelectObject(&pen);
	CBrush* oldBrush = dc->SelectObject(&brush);
	dc->Rectangle(toggleRect);


	services::select_font(80, "Arial Bold", dc);

	TEXTMETRIC tm;
	dc->GetTextMetrics(&tm);
	COLORREF cdate = RGB(20, 20, 20);
	COLORREF oldColor = dc->SetTextColor(cdate);
	UINT oldMode = dc->SetBkMode(TRANSPARENT);
	UINT oldAline = dc->SetTextAlign(TA_CENTER | TA_TOP);

	if (IsToggleButtonAvailable())
	{
		//services::select_font(80, "Verdana Bold", dc);
		

		int half = toggleRect.left + toggleRect.Width() / 2;
		int left = m_bLeftPartShown ? half : toggleRect.left;
		int right = m_bLeftPartShown ? toggleRect.right : half;
		CRect halfRect(left, toggleRect.top, right, toggleRect.bottom);	
		
		
		CBrush darkBrush(cminor);//(darkGrey);
		dc->SelectObject(&darkBrush);
		dc->Rectangle(halfRect);
		

		DrawToggleZoom(toggleRect, dc);

		if (!m_bLeftPartShown)
		{
			halfRect.MoveToX(halfRect.right);
			halfRect.left += m_zoomBtnRect.Width() / 2;
		}
		else
		{
			halfRect.MoveToX(toggleRect.left);
			halfRect.right -= m_zoomBtnRect.Width() / 2;
		}
		CRect textRect(halfRect);
		textRect.DeflateRect(2, 2, 2, 2);

		CString butonText = "<15 min>"; //m_bLeftPartShown ? "I" : "II";	

		dc->TextOut(textRect.left + textRect.Width()/2 + 1 , textRect.top + (textRect.Height() - tm.tmHeight)/2, butonText);



	}
	else
	{
		CString butonTextLeft = "< 30";
		CSize extLeft = dc->GetTextExtent(butonTextLeft);
		CString butonTextRight = "min >";
		CSize extRight = dc->GetTextExtent(butonTextRight);

		DrawToggleZoom(toggleRect, dc);
		int leftTextStartX = m_toggleRect.left + (m_toggleRect.Width() - m_zoomBtnRect.Width()) / 2 - 2 - extLeft.cx;
		int leftTextEndX = leftTextStartX + extLeft.cx + 2;
		CRect textRectLeft(leftTextStartX, m_toggleRect.top + 2, leftTextEndX, m_toggleRect.bottom -2);

		int rightTextStart = m_toggleRect.right - (m_toggleRect.Width() - m_zoomBtnRect.Width()) / 2 + 2;
		int rightTextEnd = rightTextStart + extRight.cx + 2;

		CRect textRectRight(rightTextStart, m_toggleRect.top + 1, rightTextEnd, m_toggleRect.bottom - 2);

		dc->TextOut(textRectLeft.left + textRectLeft.Width() / 2 + 1, textRectLeft.top + (textRectLeft.Height() - tm.tmHeight) / 2 , butonTextLeft);
		dc->TextOut(textRectRight.left + textRectRight.Width() / 2 + 1, textRectRight.top + (textRectRight.Height() - tm.tmHeight) / 2 , butonTextRight);
	}
	
	dc->SetTextColor(oldColor);
	dc->SetBkMode(oldMode);
	dc->SetTextAlign(oldAline);
	
	dc->SelectObject(oldPen);
	dc->SelectObject(oldBrush);

}

void tracing::DrawToggleZoom(const CRect& toggleRect, CDC* dc) const
{
	CString bmName;
	if (IsToggleButtonAvailable())
	{
		bmName = IsLeftPartShown() ? "TO_30_MIN_LEFT" : "TO_30_MIN_RIGHT";
	}
	else
	{
		bmName = "TO_15_MIN";
	}
	CBitmap zoomBitmap;
	zoomBitmap.LoadBitmap(bmName);
	CDC dcMemory;
	dcMemory.CreateCompatibleDC(dc);
	CBitmap* pOld = dcMemory.SelectObject(&zoomBitmap);
	BITMAP bm;
	zoomBitmap.GetBitmap(&bm);
	int bmWidth = bm.bmWidth;
	int bmHeight = bm.bmHeight;
	int toggleH = toggleRect.Height();
	double ratio = (double)toggleH / (double)bmHeight;

	int left = toggleRect.left + (toggleRect.Width() - bmWidth) / 2;
	int top = toggleRect.top - bmHeight / 4;

	m_zoomBtnRect = CRect(left, top, left + bmWidth, top + bmHeight);
	
	dc->BitBlt(left, top, bmWidth, bmHeight, &dcMemory, 0, 0, SRCCOPY);

	if (IsToggleButtonAvailable())
	{
		dc->MoveTo(left + bmWidth / 2, top + bmHeight);
		dc->LineTo(left + bmWidth / 2, toggleRect.bottom - 1);
	}
	dcMemory.SelectObject(pOld);
}


// =====================================================================================================================
//    Draw the actual tracing. is all handled by the various get_...() methods below.See method set_type(). Flag
//    tracingConnectValue is set if the last visited point was drawn.If not, then the currently visited point is moved
//    to, not lined to.
// =======================================================================================================================
void tracing::draw_tracing(CDC* dc) const
{
	bool draw_all_points = get_type() == tcr || get_displayed_length() < COMPRESS_VIEW_SIZE_SEC;//7200;

	const fetus*  curFetus = get();

	CRect rect = get_bounds(otracing);

	long minimum = get_minimum();
	long maximum = get_maximum();

	long ileft = max(0, get_index_from_x(rect.left));
	long iright = get_index_from_x(rect.right);

	double xCoef = get_scaling() / get_sample_rate();
	long xOffset = get_offset();
	double yCoef = (rect.Height() - 1) / (double)(maximum - minimum);
	long yOffset = get_minimum();

	// Save the context
	dc->SaveDC();

	// Clip the drawing to the current tracign region
	CRgn region;
	region.CreateRectRgn(rect.left, rect.top, rect.right, rect.bottom);
	region.OffsetRgn(dc->GetViewportOrg());
	dc->SelectClipRgn(&region, RGN_AND);

	// We loop since we may have to do multiple pass (for tfhr we draw mhr & fhr1)
	int drawing_pass_number = 0;
	while (true)
	{
		LONG lastX = 0;
		long tracingValue = 0;
		long lastValue = 0;
		bool tracingValueIsValid = (get_type() != tcr || is_visible(wcrtracing));
		bool tracingConnectValue = false;

		long stimulationStage2Limit = get_cr_kcrstage() * 60 * get_sample_rate();
		long stimulationThreshold = get_cr_threshold();
		bool stimulationOverThreshold = false;
		long istartOverThreshold;

		bool hyperstimulation_enabled = stimulationThreshold > 0;

		//COLORREF color = services::make_colour(drawing_pass_number == 1 ? RGB(0, 0, 255) : RGB(0, 0, 0), drawing_pass_number == 1 || !is_visible(wbaselines) ? 50 : 100);
		COLORREF color = services::make_colour(drawing_pass_number == 1 ? RGB(0, 0, 255) : RGB(0, 0, 0), drawing_pass_number == 1 || !is_visible(wbaselines) ? 100 : 50);
		CPen pline(PS_SOLID, 0, color);
		dc->SelectObject(&pline);

		// Special case for hyperstimulation, if the most left point is above the threshold, we need to retrieve the
		// start point so that we know for how long it's above the threshold
		if (get_type() == tcr)
		{			
			(const_cast<fetus&>(*curFetus)).ComputeContractionRateNow();			
			if (hyperstimulation_enabled && curFetus->GetContractionRate(ileft) > stimulationThreshold)
			{
				stimulationOverThreshold = true;
				istartOverThreshold = ileft;
				while (istartOverThreshold >= 0 && curFetus->GetContractionRate(istartOverThreshold) > stimulationThreshold)
				{
					--istartOverThreshold;
				}
			}
		}

		// Do the painting now...
		dc->SelectObject(&pline);

		bool bTryHarderToGetAllPoints = ((double)(iright - ileft) / (double)(rect.right - rect.left)) < 2 * get_sample_rate();

#ifdef patterns_parer_classification
		long classificationValue;
		long lastClassificationStart = ileft;
		long lastClassificationValue;

		bool bClassificationOn = is_visible(patterns_gui::tracing::wparerclassification) && (is_visible(patterns_gui::tracing::wparerinfo));
		if (bClassificationOn)
		{
			lastClassificationValue = (!is_singleton() || has_debug_keys()) ? patterns_classifier::CParerClassifier::t_undef : curFetus->GetParerClassification(ileft);
		}

#endif
		for (long i = ileft; i <= iright; ++i)
		{
			switch (get_type())
			{
			case tup:
				tracingValue = curFetus->get_up(i);
				tracingValueIsValid = tracingValue < 127; // For UP, no-data is any value >= 127
				break;

			case tfhr:
#ifdef patterns_parer_classification
				if (bClassificationOn)
				{
					classificationValue = (!is_singleton() || has_debug_keys()) ? patterns_classifier::CParerClassifier::t_undef : curFetus->GetParerClassification(i);
				}
#endif
				tracingValue = (drawing_pass_number == 0) ? curFetus->get_fhr(i) : curFetus->get_mhr(i);
				tracingValueIsValid = (tracingValue > 0 && tracingValue < 255); // 0 is not a valid value for HR
				break;

			case tcr:
				tracingValue = curFetus->GetContractionRate(i);
				break;

			default:
				assert(0);
				break;
			}

#ifdef patterns_parer_classification
			if (bClassificationOn && (get_type() == tfhr) && (i == iright || lastClassificationValue != classificationValue))
			{
				if (lastClassificationValue >= 0 && lastClassificationValue <= 4)
				{
					CBrush brush(classification_color(lastClassificationValue));
					CBrush* oldBrush = dc->SelectObject(&brush);

					CPen pen;
					pen.CreatePen(PS_SOLID, 1, classification_color(lastClassificationValue));

					CPen* oldPen = dc->SelectObject(&pen);

					CRect rectangle(get_x_from_index(lastClassificationStart), rect.top, get_x_from_index(i), min(rect.top + 2, rect.bottom));

					dc->Rectangle(rectangle);

					dc->SelectObject(oldBrush);
					dc->SelectObject(oldPen);
				}

				// Remember
				lastClassificationValue = classificationValue;
				lastClassificationStart = i;
			}
#endif
			if (tracingValueIsValid)
			{
				long x = (long)(i * xCoef - xOffset);
				long y;

				if (tracingValue == maximum)
				{
					y = rect.top;
				}
				else if (tracingValue > maximum)
				{
					y = rect.top - 5;
				}
				else if (tracingValue == minimum)
				{
					y = rect.bottom - 1;
				}
				else if (tracingValue < minimum)
				{
					y = rect.bottom + 5;
				}
				else
				{
					y = (long)((rect.bottom - 1) - ((tracingValue - yOffset) * yCoef));
				}

				// We can have more points than pixel (usual case)
				if (draw_all_points 
					|| (!tracingConnectValue)
					|| (i == ileft) 
					|| (i == iright) 
					|| (x >= lastX + 1) 
					|| (abs(lastValue - tracingValue) >= 10))
				{
					if (!tracingConnectValue)
					{
						dc->MoveTo(x, y);
						dc->SetPixel(x, y, color);
					}
					else
					{
						dc->LineTo(x, y);
					}

					// Remember we draw that point
					lastX = x;
					lastValue = tracingValue;
					tracingConnectValue = true;
				}
			}

			tracingConnectValue = tracingConnectValue && tracingValueIsValid;

			// Color for hyperstimultation
			if (hyperstimulation_enabled && get_type() == tcr)
			{				
				

				if (stimulationOverThreshold)
				{
					if ((i == iright) || (tracingValue <= stimulationThreshold))
					{
						// One stage (orange)
						if ((stimulationStage2Limit < 0) || (i - istartOverThreshold <= stimulationStage2Limit))
						{
							services::draw_bitmap(dc, "hyperstimulation", "", get_x_from_index(max(istartOverThreshold, ileft)), rect.top, get_x_from_index(i), rect.bottom, services::ctopleft, true);
						}

						// Two stages (orange then red)
						else
						{
							long iThreshold = max(istartOverThreshold + stimulationStage2Limit, ileft);

							if (iThreshold >= ileft)
							{
								services::draw_bitmap(dc, "hyperstimulation", "", get_x_from_index(max(istartOverThreshold, ileft)), rect.top, get_x_from_index(iThreshold), rect.bottom, services::ctopleft, true);
							}

							services::draw_bitmap(dc, "hyperstimulation_3", "", get_x_from_index(iThreshold), rect.top, get_x_from_index(i), rect.bottom, services::ctopleft, true);
						}

						stimulationOverThreshold = false;
					}
				}
				else if (tracingValue > stimulationThreshold)
				{
					stimulationOverThreshold = true;
					istartOverThreshold = i;
				}
			}
		}
		
		if (get_type() == tfhr && get_display_mhr() && drawing_pass_number < 1)
		{
			drawing_pass_number = 1;
			continue;
		}

		// We're done, leave now
		break;
	}
	
	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    Draw any needed watermark.
// =======================================================================================================================
void tracing::draw_watermark(CDC* dc, const string& name) const
{
	CRect rb = services::get_bitmap_rectangle(name);
	CRect rect = get_bounds(otracing);
	long h = rect.Height();
	long hb = rb.Height();

	rect.left -= get_offset() % rb.Width();

	if (hb < h)
	{
		rect.top += (h - hb) / 2;
		rect.bottom -= h - hb - (h - hb) / 2;
	}

	services::draw_bitmap(dc, name, "", rect.left, rect.top, rect.right, rect.bottom, services::ctopleft, true);
}

// =====================================================================================================================
//    Find the object lying under the given point. Order of the various if () statements is significant, see method
//    get_bounds() for details. Method find_index(pt) is designed to be called only by subviews or non-composite views.It
//    will not work when called in composite view types.
// =======================================================================================================================
tracing::object tracing::find(CPoint pt) const
{
	object obj = oview;
	CRect rprimary = get_bounds(oprimary);

	if (get_type() == tfhr)
	{
		rprimary.top = get_bounds(opictogram).top;
	}

	rprimary.top -= get(kclicktolerance);
	rprimary.bottom += get(kclicktolerance);
	if (get_bounds(obuttondelete).PtInRect(pt))
	{
		obj = obuttondelete;
	}
	else if (get_bounds(obuttonaccept).PtInRect(pt))
	{
		obj = obuttonaccept;
	}
	else if (rprimary.PtInRect(pt))
	{
		obj = oprimary;
	}
	else if (get_bounds(osecondary).PtInRect(pt))
	{
		obj = osecondary;
	}
	else if (get_bounds(ozoombtn).PtInRect(pt))
	{
		obj = ozoombtn;
	}
	else if (get_bounds(otoggleleft).PtInRect(pt))
	{
		obj = otoggleleft;
	}
	else if (get_bounds(otoggleright).PtInRect(pt))
	{
		obj = otoggleright;
	}
	else if (get_bounds(osliderleft).PtInRect(pt))
	{
		obj = osliderleft;
	}
	else if (get_bounds(osliderright).PtInRect(pt))
	{
		obj = osliderright;
	}
	else if (get_bounds(oslider).PtInRect(pt))
	{
		obj = oslider;
	}
	else if (get_bounds(otracing).PtInRect(pt))
	{
		obj = otracing;
	}

	return obj;
}

long tracing::find_index(CPoint pt) const
{
	const fetus* curFetus = get();
	long i;
	long n;
	long r = -1;

	switch (get_type())
	{
	case tfhr:
		for (i = 0, n = curFetus->GetEventsCount(); i < n && r < 0; i++)
		{
			const event&  ei = curFetus->get_event(i);

			if (pt.x >= get_x_from_index(ei.get_start()) && pt.x <= get_x_from_index(ei.get_end()))
			{
				r = i;
			}
		}

		if (r >= 0 && !is_visible(curFetus->get_event(r)))
		{
			r = -1;
		}
		break;

	case tup:
		for (i = 0, n = curFetus->GetContractionsCount(); i < n && r < 0; i++)
		{
			if (pt.x >= get_x_from_index(curFetus->get_contraction(i).get_start()) && pt.x <= get_x_from_index(curFetus->get_contraction(i).get_end()))
			{
				r = i;
			}
		}
		break;
	}

	return r;
}

// =====================================================================================================================
//    Find subview under given point. This returns -1 if the point is outside all subviews or if we do not have subviews
//    at all.
// =======================================================================================================================
long tracing::find_subview(CPoint pt) const
{
	long r = -1;

	for (long i = 0, n = (long)ksubviews.size(); i < n && r < 0; i++)
	{
		if (get_subview_bounds(i).PtInRect(pt))
		{
			r = i;
		}
	}

	return r;
}

// =====================================================================================================================
//    Follow one of our subviews. Subviews that should be kept synchronized need to call this method passing themselves
//    as parameter so that the parent view may propagate any movement or change this need to be reflected to other
//    subviews. This method calls itself for subviews that need to mirror changes from the given view.
// =======================================================================================================================
void tracing::follow(const tracing* s)
{
	if (is_subview())
	{
		if (*this != *s)
		{
			set_offset(s->get_offset());
			set_paper(s->get_paper());
			if (s->get_scaling_mode() != sfree)
			{
				set_scaling_mode(s->get_scaling_mode());
			}
			else
			{
				set_scaling(s->get_scaling());
			}
		}
	}
	else
	{
		long i;
		long j;
		long n;

		for (i = -1, j = 0, n = (long)ksubviews.size(); j < n; j++)
		{
			if (ksubviews[j] == s)
			{
				i = j;
				j = n;
			}
		}

		if (i >= 2)
		{
			for (j = 2; j < n; j++)
			{
				if (j != i)
				{
					ksubviews[j]->follow(s);
				}
			}
		}
	}
}

// =====================================================================================================================
//    Access the fetus instance for the underlying data. If we have subscribed to a parent view, we route the call to the
//    parent view's data.
// =======================================================================================================================
const fetus* tracing::get(void) const
{
	if (!has_parent() && !m_pFetus)
	{
		set(NewFetus());
	}

	return has_parent() ? get_parent()->get() : m_pFetus;
}

fetus* tracing::get(void)
{
	if (!has_parent() && !m_pFetus)
	{
		set(NewFetus());
	}

	return has_parent() ? get_parent()->get() : m_pFetus;
}

// =====================================================================================================================
//    Control-wide constants.
// =======================================================================================================================
long tracing::get(constant k) const
{
	long r = 0;

	switch (k)
	{
	case kanimationduration:
		r = 200;
		break;

	case kclicktolerance:
		r = 4;
		break;

	case kimagingtest:
		r = 100;
		break;
	}

	return r;
}

// =====================================================================================================================
//    Get the current animation rate. See methods set_animation_rate() and move_to_animating().
// =======================================================================================================================
double tracing::get_animation_rate(void) const
{
	return m_animationRate;
}

// =====================================================================================================================
//    Get given object's bounding rectangle. These rectangles are not mutually exclusive.For instance, the slider's left
//    and right handles will overlap the slider's rectangle.
// =======================================================================================================================
CRect tracing::get_bounds(object k) const
{
	CRect rect(0, 0, m_bounds.Width(), m_bounds.Height());
	if (k == oview)
	{
		return rect;
	}

	const bool icompressed = is_compressed();
	const long htop = 6;
	const long hpictogram = icompressed ? (is_visible(wcpictograms) ? 8 : 0) : 16;
	const long hbelowpictogram = icompressed && !is_visible(wcpictograms) ? 0 : 3;
	const long hevents = icompressed ? (is_visible(wceventbars) ? 4 : 0) : 8;
	const long hbelowevents = icompressed ? (is_visible(wceventbars) ? 3 : 0) : 6;
	const long hdate = 14;
	const long hbottom = 2;
	const long hcontractions = icompressed ? 4 : 8;
	const long hovercontractions = icompressed ? 3 : 6;

	switch (k)
	{
	case obuttondelete:
		rect = rbdeletebutton;
		break;

	case obuttonaccept:
		rect = rbacceptbutton;
		break;

	case opictogram:
		rect.top = htop;
		rect.bottom = rect.top + hpictogram;
		break;

	case oprimary:
		switch (get_type())
		{
		default:

		case tcr:
			rect.bottom = rect.top;
			break;

		case tfhr:
			rect.top += htop + hpictogram + hbelowpictogram;
			rect.bottom = rect.top + hevents;
			break;

		case tup:
			rect.bottom -= hbottom;
			rect.top = rect.bottom - hcontractions;
			break;
		}
		break;

	case osecondary:
		switch (get_type())
		{
		default:

		case tcr:
			rect.bottom = rect.top;
			break;

		case tfhr:
			rect.top += htop + hpictogram + hbelowpictogram + hevents;
			rect.bottom = rect.top + hevents;
			break;

		case tup:
			rect.bottom -= hbottom - hcontractions;
			rect.top = rect.bottom - hcontractions;
			break;
		}
		break;

	case oslider:
		if (has_parent_for_view())
		{
			const tracing*	curTracing = get_parent_for_view();

			curTracing->update_now();
			update_now();

			rect.left = get_x_from_index(get_index_from_ms(curTracing->GetSliderLeftInMs()));
			rect.right = get_x_from_index(get_index_from_ms(curTracing->GetSliderRightInMs()));
		}
		break;

	case osliderleft:
		if (has_parent_for_view())
		{
			rect = get_bounds(oslider);

			rect.left -= 4;
			rect.right = rect.left + 8;
		}
		break;

	case osliderright:
		if (has_parent_for_view())
		{
			rect = get_bounds(oslider);

			rect.right += 4;
			rect.left = rect.right - 8;
		}
		break;
	case ozoombtn:
	{
		if (!has_parent_for_view())
		{
			rect = m_zoomBtnRect;
		}
		else
		{
			rect.SetRectEmpty();
		}
	}
		break;
	case otoggle:
	{
		if (IsToggleButtonAvailable() && !has_parent_for_view())
		{
			rect = m_toggleRect;
		}
		else
		{
			rect.SetRectEmpty();
		}
	}
	break;
	case otoggleleft:
	{
		if (IsToggleButtonAvailable() && !has_parent_for_view())
		{
			rect = get_bounds(otoggle);
			rect.right = rect.left + rect.Width() / 2;
		}
		else
		{
			rect.SetRectEmpty();
		}

	}
	break;
	case otoggleright:
	{
		if (IsToggleButtonAvailable() && !has_parent_for_view())
		{
			rect = get_bounds(otoggle);
			rect.left = rect.left + rect.Width() / 2;
		}
		else
		{
			rect.SetRectEmpty();
		}
	}
	break;

	case otracing:
		switch (get_type())
		{
		default:
			rect.bottom = rect.top;
			break;

		case tcr:
			rect.top += htop;
			rect.bottom -= hbottom;
			break;

		case tfhr:
			rect.top += htop + hpictogram + hbelowpictogram + hevents + hbelowevents;
			rect.bottom -= hbottom + hdate;
			break;

		case tup:
			rect.top += htop;
			rect.bottom -= hbottom + hcontractions + hovercontractions;
			break;
		}
		break;	

		case ocompletetracing:
		{
			//rect = get_bounds(oslider);
			rect = get_bounds(otracing);
			if (IsToggleButtonAvailable())
			{
				if (m_bLeftPartShown)
				{
					rect.right += rect.Width();
				}
				else
				{
					rect.left -= rect.Width();
				}
			}
		}
		break;
	}

	return rect;
}

string tracing::get_compressed_fhr_title(void) const
{
	double dTotalNumberOfSeconds = ((double)get_number()) / get_sample_rate();
	double dDisplayedNumberOfSeconds;

	if (is_zooming() || is_zoomed() || !has_compressed_length())
	{
		dDisplayedNumberOfSeconds = (double)(ksubviews.size() > 2 ? ksubviews[2]->get_displayed_length() : 0);
	}
	else
	{
		dDisplayedNumberOfSeconds = get_compressed_length();
	}

	char t[1000];
	string msg;

	sprintf(t, "%.1lf hour view", dDisplayedNumberOfSeconds / 3600);
	msg = t;

	sprintf(t, " (out of %.1lf %s)", dTotalNumberOfSeconds / 3600, (dTotalNumberOfSeconds >= 7200) ? "hours" : "hour");
	msg += t;

	return msg.c_str();
}

// =====================================================================================================================
//    Get subview lengths. See set_lengths().
// =======================================================================================================================
long tracing::get_compressed_length(void) const
{
	return lcompressed;
}

long tracing::get_expanded_length(void) const
{
	return lexpanded;
}

long tracing::GetSliderLength(void) const
{
	return m_sliderLength;
}
// =====================================================================================================================
//    Get displayed length of tracing in seconds. If we are a composite tracing, we return the length of the expanded
//    subtracing.
// =======================================================================================================================
long tracing::get_displayed_length(void) const
{
	if (!is_zoomed() && (!has_parent() || !get_parent()->is_zoomed()))
	{
		if (is_compressed())
		{
			if (has_compressed_length())
			{
				return get_compressed_length();
			}

			if (has_parent() && get_parent()->has_compressed_length())
			{
				return get_parent()->get_compressed_length();
			}
		}
		else
		{
			if (has_expanded_length())
			{
				return get_expanded_length();
			}

			if (has_parent() && get_parent()->has_expanded_length())
			{
				return get_parent()->get_expanded_length();
			}
		}
	}

	long ileft;
	long iright;

	CRect rect = get_bounds(otracing);

	ileft = get_index_from_x(rect.left);
	iright = get_index_from_x(rect.right);
	return (iright - ileft) / get_sample_rate();
}

long tracing::GetCompleteLength(void) const
{
	if (!is_zoomed() && (!has_parent() || !get_parent()->is_zoomed()))
	{
		if (is_compressed())
		{
			if (has_compressed_length())
			{
				return get_compressed_length();
			}

			if (has_parent() && get_parent()->has_compressed_length())
			{
				return get_parent()->get_compressed_length();
			}
		}
		else
		{
			if (has_expanded_length())
			{
				return GetSliderLength();
			}

			if (has_parent() && get_parent()->has_expanded_length())
			{
				return get_parent()->GetSliderLength();
			}
		}
	}

	long ileft;
	long iright;

	CRect rect = get_bounds(ocompletetracing);

	ileft = get_index_from_x(rect.left);
	iright = get_index_from_x(rect.right);
	return (iright - ileft) / get_sample_rate();

}

long tracing::GetExpandedViewCompleteLength() const
{
	if (!is_zoomed() && (!has_parent() || !get_parent()->is_zoomed()))
	{

		if (has_expanded_length())
		{
			return GetSliderLength();
		}

		if (has_parent() && get_parent()->has_expanded_length())
		{
			return get_parent()->GetSliderLength();
		}
		
	}

	long ileft;
	long iright;

	CRect rect = get_bounds(ocompletetracing);

	ileft = get_index_from_x(rect.left);
	iright = get_index_from_x(rect.right);
	return (iright - ileft) / get_sample_rate();

}

// =====================================================================================================================
//    Convert between indices in the tracing and on-screen positions. This takes into account any offset or scaling
//    factor and should be used whenever an index needs to be found for a given x or y position or vice versa.The x and y
//    values are in pixels and relative to the upper-left corner.
// =======================================================================================================================
long tracing::get_index_from_ms(long long t) const
{
	return (long)((long long)get_sample_rate() * t / (long long)1000);
}

long tracing::get_index_from_x(long x) const
{
	return (long)(get_sample_rate() * (x + get_offset()) / get_scaling());
}

long long tracing::get_ms_from_index(long i) const
{
	return (long)((long long)1000 * (long long)i / (long long)get_sample_rate());
}

long tracing::get_x_from_index(long i) const
{
	return (long)(i * get_scaling() / get_sample_rate() - get_offset());
}

long tracing::get_x_from_ms(long long t) const
{
	long index = get_index_from_ms(t);
	return get_x_from_index(index);
}

long tracing::get_y_from_units(CRect& rect, double u) const
{
	if (u == get_maximum())
	{
		return rect.top;
	}
	if (u > get_maximum())
	{
		return rect.top - 1;
	}
	if (u == get_minimum())
	{
		return rect.bottom - 1;
	}
	if (u < get_minimum())
	{
		return rect.bottom;
	}
	return (long)((rect.bottom - 1) - ((u - get_minimum()) * (rect.Height() - 1) / (double)(get_maximum() - get_minimum())));
}

// =====================================================================================================================
//    Get position in milliseconds of leftmost visible point.
// =======================================================================================================================
long long tracing::get_left_in_ms(void) const
{
	return (long long)1000 * (long long)get_index_from_x(get_bounds(otracing).left) / (long long)get_sample_rate();
}

long long tracing::GetSliderLeftInMs(void) const
{
	CRect tracingBounds = get_bounds(ocompletetracing);
	long long	sliderLeft = (long long)1000 * (long long)get_index_from_x(tracingBounds.left) / (long long)get_sample_rate();
	
	return sliderLeft;
}
// =====================================================================================================================
//    Access vertical bounds for current type and paper.
// =======================================================================================================================
long tracing::get_maximum(void) const
{
	switch (get_type())
	{
	case tcr:
		return get_cr_maximum();

	case tup:
		return 100;

	case tfhr:
	default:
		return get_paper() == pusa ? 240 : 210;
	}
}

long tracing::get_minimum(void) const
{
	switch (get_type())
	{
	case tcr:
		return 0;

	case tup:
		return 0;

	case tfhr:
	default:
		return get_paper() == pusa ? 30 : 50;
	}
}

// =====================================================================================================================
//    Access number of tracing points. This encapsulates the graph type, see get(i) and set_type().
// =======================================================================================================================
long tracing::get_number(void) const
{
	const fetus*  curFetus = get();

	switch (get_type())
	{
	case tup:
	case tcr:
		return max(curFetus->get_number_of_up(), (long)(curFetus->get_number_of_fhr() * curFetus->get_up_sample_rate() / curFetus->get_hr_sample_rate()));

	case tfhr:
	default:
		return max(curFetus->get_number_of_fhr(), (long)(curFetus->get_number_of_up() * curFetus->get_hr_sample_rate() / curFetus->get_up_sample_rate()));
	}
}

// =====================================================================================================================
//    Access the current canvas offset. This is always valid, wether scaling to fit or not.See set_offset().If we follow
//    our parent's view, we return it's offset instead of ours.
// =======================================================================================================================
long tracing::get_offset(void) const
{
	return has_parent() && ifollowparent ? get_parent()->get_offset() : offset;
}

// =====================================================================================================================
//    Access current paper format. See set_paper().
// =======================================================================================================================
tracing::paper tracing::get_paper(void) const
{
	return kpaper;
}

// =====================================================================================================================
//    Get the tracing height for the current paper and type. This is the height, in centimetres, of the fhr or up tracing
//    on the usa or international paper.
// =======================================================================================================================
double tracing::get_paper_height(void) const
{
	if (get_type() == tup)
	{
		return 4;
	}
	if (get_paper() == pusa)
	{
		return 7;
	}

	return 8;
}

// =====================================================================================================================
//    Access parent tracings. This encapsulates access to kparent and kparent->kparent via ifollowparent.method
//    get_parent_for_view() means whatever view is the visual parent, wether it be the real parent of the parent's
//    parent.We always return a valid object to minimize crash risks.See predicates has_parent() and
//    has_parent_for_view().
// =======================================================================================================================
const tracing* tracing::get_parent(void) const
{
	return kparent ? kparent : this;
}

tracing* tracing::get_parent(void)
{
	return kparent ? kparent : this;
}

const tracing* tracing::get_parent_for_view(void) const
{
	tracing*  r = 0;

	if (kparent && !ifollowparent)
	{
		r = kparent;
	}
	else if (kparent && ifollowparent)
	{
		r = kparent->kparent;
	}

	return r ? r : this;
}

tracing* tracing::get_parent_for_view(void)
{
	tracing*  r = 0;

	if (kparent && !ifollowparent)
	{
		r = kparent;
	}
	else if (kparent && ifollowparent)
	{
		r = kparent->kparent;
	}

	return r ? r : this;
}

const tracing* tracing::get_parent_root(void) const
{
	const tracing*	r = this;

	while (r->kparent)
	{
		r = r->kparent;
	}

	return r;
}

tracing* tracing::get_parent_root(void)
{
	tracing*  r = this;

	while (r->kparent)
	{
		r = r->kparent;
	}

	return r;
}

// =====================================================================================================================
//    Access the fetus instance for the underlying data. If we have subscribed to a parent view, we route the call to the
//    parent view's data.
// =======================================================================================================================
const fetus* tracing::get_partial(void) const
{
	if (!has_parent() && !fpartial)
	{
		set_partial(NewFetus());
	}

	return has_parent() ? get_parent()->get_partial() : fpartial;
}

fetus* tracing::get_partial(void)
{
	if (!has_parent() && !fpartial)
	{
		set_partial(NewFetus());
	}

	return has_parent() ? get_parent()->get_partial() : fpartial;
}

// Get the current patient.
input_adapter::patient * tracing::get_patient (void) const
{
	if (has_parent())
	{
		return get_parent_root()->get_patient();
	}
	if ((m_pFetus != 0) && (m_pFetus->has_conductor()) && (m_pFetus->get_conductor().has_input_adapter()))
	{
		return m_pFetus->get_conductor().get_input_adapter().get_patient(m_pFetus->get_key());
	}
	return 0;
}

// =====================================================================================================================
//    Get tracing precision in samples per pixel. See set_precision().
// =======================================================================================================================
double tracing::get_precision(void) const
{
	return kprecision;
}

// =====================================================================================================================
//    Get position in milliseconds of rightmost visible point.
// =======================================================================================================================
long long tracing::get_right_in_ms(void) const
{
	return (long long)1000 * (long long)get_index_from_x(get_bounds(otracing).right) / (long long)get_sample_rate();
}

long long  tracing::GetSliderRightInMs(void) const
{
	CRect tracingBounds = get_bounds(otracing);
	long long sliderLeft = 0;
	if (!m_bLeftPartShown || m_sliderLength == lexpanded)
		sliderLeft = (long long)1000 * (long long)get_index_from_x(tracingBounds.right) / (long long)get_sample_rate();
	else
		sliderLeft = (long long)1000 * (long long)get_index_from_x(tracingBounds.right + tracingBounds.Width()) / (long long)get_sample_rate();
	return sliderLeft;
}
// =====================================================================================================================
//    Get the currently-displayed sample rate. This is similar to methods get(i), get_number(), etc.
// =======================================================================================================================
long tracing::get_sample_rate(void) const
{
	switch (get_type())
	{
	case tcr:
		return get()->get_up_sample_rate();

	case tup:
		return get()->get_up_sample_rate();

	case tfhr:
	default:
		return get()->get_hr_sample_rate();
	}
}

// =====================================================================================================================
//    Access current scaling factor. This is always valid, returning the calculated factor if scaling to fit.See
//    set_scaling().
// =======================================================================================================================
double tracing::get_scaling(void) const
{
	if (!has_parent() || !ifollowparent)
	{
		update_now();
		return m_scaling;
	}

	return get_parent()->get_scaling();
}

// =====================================================================================================================
//    Access current scaling factor. This is always valid, returning the calculated factor if scaling to fit.See
//    set_scaling().
// =======================================================================================================================
double tracing::priv_get_scaling(void) const
{
	if (!has_parent() || !ifollowparent)
	{
		return m_scaling;
	}

	return get_parent()->get_scaling();
}

// =====================================================================================================================
//    Get the current scaling mode. See set_scaling_mode().
// =======================================================================================================================
tracing::scaling_mode tracing::get_scaling_mode(void) const
{
	return has_parent() && ifollowparent ? get_parent()->get_scaling_mode() : m_scalingMode;
}

// =====================================================================================================================
//    Access the currently selected object's index. This is an index in a set of objects that depends on
//    get_selection_type().See also select().
// =======================================================================================================================
inline long tracing::get_selection(void) const
{
	return iselection;
}

// =====================================================================================================================
//    Access current type of selection. This determines what the selection index means.See methods get_selection() and
//    select().
// =======================================================================================================================
inline tracing::selectable tracing::get_selection_type(void) const
{
	return tselection;
}

// =====================================================================================================================
//    Access the currently displayed graph type. See method set_type().
// =======================================================================================================================
inline tracing::type tracing::get_type(void) const
{
	return m_tracingType;
}

// =====================================================================================================================
//    Get width in milliseconds.
// =======================================================================================================================
long long tracing::get_width_in_ms(void) const
{
	return get_right_in_ms() - get_left_in_ms();
}

// =====================================================================================================================
//    Are subviews bounded in length?
// =======================================================================================================================
bool tracing::has_compressed_length(void) const
{
	return lcompressed > 0;
}

// =====================================================================================================================
//    Are the debug keys accessible?
// =======================================================================================================================
bool tracing::has_debug_keys(void) const
{
	return idebugkeys;
}

bool tracing::has_expanded_length(void) const
{
	return lexpanded > 0;
}

// =====================================================================================================================
//    Do we have a parent view? See methods get_parent...().
// =======================================================================================================================
bool tracing::has_parent(void) const
{
	return kparent != NULL;
}

bool tracing::has_parent_for_view(void) const
{
	return kparent && (!ifollowparent || kparent->kparent);
}

// =====================================================================================================================
//    Do we have a patient?
// =======================================================================================================================
bool tracing::has_patient(void) const
{
	if (has_parent())
	{
		return get_parent_root()->has_patient();
	}

	return (m_pFetus != 0);
}

// =====================================================================================================================
//    Is there a currently selected object? See select().
// =======================================================================================================================
bool tracing::has_selection(void) const
{
	return tselection != cnone;
}

// =====================================================================================================================
//    Do we or should we have subviews ? This is meant to be true regardless of wether subviews are actually currently
//    created or not;
//    it returns true if the current mode implies having subviews.See methods create_subviews() and update_now().
// =======================================================================================================================
bool tracing::has_subviews(void) const
{
	return get_type() == tnormal || get_type() == tnormalnc;
}

// =====================================================================================================================
//    Does the current patient have twins?
// =======================================================================================================================
bool tracing::has_twins(void) const
{
#if defined(patterns_retrospective) && !defined(OEM_patterns)
	return false;
#else
	input_adapter::patient* curPatient = get_patient();
	return (curPatient == 0) ? false : (curPatient->get_number_of_fetuses() > 1);
#endif
}

// =====================================================================================================================
//    Does the current patient have twins?
// =======================================================================================================================
bool tracing::is_singleton(void) const
{
#if defined(patterns_retrospective) && !defined(OEM_patterns)
	return true;
#else
	input_adapter::patient* curPatient = get_patient();
	return (curPatient == 0) ? false : (curPatient->get_number_of_fetuses() == 1);
#endif
}

// =====================================================================================================================
//    As the tracing currently animated? This does not garantee that the offset will change again, as OnTimer() might
//    reset the ianimating flag during the next event loop.It simply means that the tracing is still in animation mode.
// =======================================================================================================================
bool tracing::is_animating(void) const
{
	return ianimating;
}

// =====================================================================================================================
//    Are we the compressed view of a composite tracing?
// =======================================================================================================================
bool tracing::is_compressed(void) const
{
	bool r = false;

	if (is_subview())
	{
		const tracing*	curTracing = get_parent_root();

		if (curTracing->get_type() == tnormal)
		{
			for (long i = 2, n = (long)curTracing->ksubviews.size(); !r && i < n; i++)
			{
				r = curTracing->ksubviews[i] == this;
			}
		}
	}

	return r;
}

// =====================================================================================================================
//    Is the current fetus connected?
// =======================================================================================================================
bool tracing::is_connected(void) const
{
	const fetus* curFetus = get();

	if ((curFetus->has_conductor()) && (curFetus->get_conductor().has_input_adapter()))
	{
		return curFetus->get_conductor().get_input_adapter().is_collecting_tracing(curFetus->get_key());
	}

	return false;
}

// =====================================================================================================================
//    Is the current fetus connected?
// =======================================================================================================================
bool tracing::is_late_realtime(void) const
{
	const fetus* curFetus = get();

	if ((curFetus->has_conductor()) && (curFetus->get_conductor().has_input_adapter()))
	{
		return curFetus->get_conductor().get_input_adapter().is_late_collecting_tracing(curFetus->get_key());
	}

	return false;
}

// =====================================================================================================================
//    Is the grid shown? This method is deprecated.Use is_visible(wgrid) instead.
// =======================================================================================================================
bool tracing::is_grid_visible(void) const
{
	return is_visible(wgrid);
}

// =====================================================================================================================
//    Is scaling locked at current value? See method lock_scaling().
// =======================================================================================================================
bool tracing::is_scaling_locked(void) const
{
	return iscalinglocked;
}

// =====================================================================================================================
//    Are we scaling to view to fit the current data? This method is deprecated.Use get_scaling_mode() instead.
// =======================================================================================================================
bool tracing::is_scaling_to_fit(void) const
{
	return get_scaling_mode() == sfit;
}

// =====================================================================================================================
//    Are we scrolling to show the last sample? See method scroll().
// =======================================================================================================================
bool tracing::is_scrolling(void) const
{
	return has_parent() && ifollowparent ? get_parent()->is_scrolling() : iscrolling;
}

// =====================================================================================================================
//    Are we a subview managed by a parent view? This is true if we were created through create_subviews() by another
//    view, that is, if we are an invisible view used to draw one of the parts of a complex tracing.See set_type().The
//    isubview property should only be set to true by create_subviews ().
// =======================================================================================================================
bool tracing::is_subview(void) const
{
	return isubview;
}

// =====================================================================================================================
//    Should we display given event? This centralizes decision for displaying guides, clicking, dragging, drawing, etc.
// =======================================================================================================================
bool tracing::is_visible(const event& curEvent) const
{
	const fetus*  curFetus = get();
	long icutoff = 0;

	if (curFetus->has_cutoff_date())
	{
		icutoff = curFetus->get_hr_sample_rate() * (long)(curFetus->get_cutoff_date() - curFetus->get_start_date());
	}

	return curFetus->has_cutoff_date() && is_singleton() && curEvent.get_start() >= icutoff && curEvent.get_type() != curEvent.tbaseline && (is_visible(wstruckout) || !curEvent.is_strike_out()) && (!is_visible(whideevents));
}

// =====================================================================================================================
//    Is given element visible? See method show().
// =======================================================================================================================
bool tracing::is_visible(showable k) const
{
	return ishow & k ? true : false;
}

// =====================================================================================================================
//    Is the compressed view currently zoomed? This is true if the compressed view is zoomed or being animated into. See
//    method zoom().
// =======================================================================================================================
bool tracing::is_zoomed(void) const
{
	return izoomed;
}

// =====================================================================================================================
//    Are we currently animating into or out of zoomed mode? This may be true even if is_zoomed() is not;
//    this means that we are animating out of zoomed mode.Otherwise, this is true if we are animating into zoomed mode.
// =======================================================================================================================
bool tracing::is_zooming(void) const
{
	return ksubviews.size() > 2 && ksubviews[2]->is_animating() && (zleft > 0 || zright > 0);
}

// =====================================================================================================================
//    Lock scaling to its current value. In fact, the scaling factor may still change if the scaling mode so implies.This
//    really just prevents the user from manually changing the scaling factor from some compressed view that may be
//    connected to this one.See methods is_scaling_locked() and set_scaling().
// =======================================================================================================================
void tracing::lock_scaling(bool i0)
{
	if ((iscalinglocked ? 1 : 0) != (i0 ? 1 : 0))
	{
		iscalinglocked = i0;
		adjust_subviews();
	}
}

// =====================================================================================================================
//    move to previous/next page. A page is represent by the view width.
// =======================================================================================================================
void tracing::move_page(bool i, bool toggleSwith)
{
	CRect rect = get_bounds(otracing);

	if (i)
	{
		move_to(get_index_from_x(rect.left - rect.Width()), -1, toggleSwith);
	}
	else
	{
		move_to(get_index_from_x(rect.left + rect.Width()), -1, toggleSwith);
	}
}

// =====================================================================================================================
//    move to previous/next page using the animation. A page is represent by the view width.
// =======================================================================================================================
void tracing::move_page_animating(bool i)
{
	CRect rect = get_bounds(otracing);

	move_to_animating(get_index_from_x(rect.left + (i ? -1 : 1) * rect.Width()), -1, get(kanimationduration));
}

// =====================================================================================================================
//    Scale and move so that given indices be visible. If only one index is given, then no scaling occurs and the view is
//    moved so that the index becomes left edge.
// =======================================================================================================================
void tracing::move_to(long i1, long i2, bool toggleSwitch)
{
	stop_animating();
	
	if (IsToggleButtonAvailable())
	{
		long total = (get()->get_number_of_fhr())/get()->get_hr_sample_rate();		
		long sliderLength = GetSliderLength();

		if (sliderLength >= total)
		{
			m_bLeftPartShown = false;
		}
		else if (!m_bLeftPartShown && i1 <= 0)
		{			
			m_bLeftPartShown = true;
			update();
		}
		else
		{	
			long long destinationMS = get_ms_from_index(i1);
		
			if (destinationMS <= 0)
			{
				m_bLeftPartShown = true;
				//update();
			}
			else if (!toggleSwitch && m_bLeftPartShown && destinationMS - get_width_in_ms() >= 0)
			{
				m_bLeftPartShown = false;
			}
		}
	}

	if (i2 < 0 || i1 == i2)
	{
		set_offset((long)(get_scaling() * i1 / get_sample_rate()));
	}
	else
	{
		if (i1 > i2)
		{
			long t = i2;

			i2 = i1;
			i1 = t;
		}

		set_scaling((double)get_sample_rate() * (double)get_bounds(otracing).Width() / (i2 - i1));
		set_offset((long)(get_scaling() * i1 / get_sample_rate()));
	}
	if(!is_subview() && GetParent() != NULL)
	{
		GetParent()->PostMessage(FORCE_UPDATE_ADAPTER_MSG, 0, 0);
	}
}

// =====================================================================================================================
//    Start animating towards given destination index or indices. We disregard any currently running animation.The
//    destination dl and dr correspond to parameters i1 and i2 of method move_to().If t is given, then we disregard the
//    current animation speed set through set_animation_rate(). dl: destination left index. dr: destination right index
//    or -1.If left as -1, then no scaling occurs.See method move_to(). t: animation time in milliseconds or -1.If left
//    unspecified as -1, then total animation time is deduced from the distance from m_animationLeft1 to m_animationLeft2 and get_animation_rate().
// =======================================================================================================================
void tracing::move_to_animating(long dl, long dr, long t, long step, bool toggleSwitch)
{

	if (is_animating())
	{
		KillTimer(tanimation);
	}

	SetTimer(tanimation, step > 0 ? step : 50, 0);

	if (IsToggleButtonAvailable())
	{
		long total = (get()->get_number_of_fhr()) / get()->get_hr_sample_rate();
		long sliderLength = GetSliderLength();
		if (total < sliderLength)
		{
			m_bLeftPartShown = false;
		}
		else if(!m_bLeftPartShown && dl <= 0)
		{
			m_bLeftPartShown = true;
			update();
		}
		else
		{
			long long destinationMS = get_ms_from_index(dl);
			
			if(destinationMS <= 0)
			{
				m_bLeftPartShown = true;
				//update();
			}
			else if (!toggleSwitch && m_bLeftPartShown && destinationMS - get_width_in_ms() >= 0)
			{				
				m_bLeftPartShown = false;
			}
		}
	}
	ianimating = true;
	m_animationLeft1 = get_index_from_ms(get_left_in_ms());
	m_animationLeft2 = dl;


	if (dr < 0)
	{
		m_animationRight1 = m_animationRight2 = -1;
	}
	else
	{
		m_animationRight1 = get_index_from_ms(get_right_in_ms());
		m_animationRight2 = dr;
	}

	m_animationTime1 = clock();
	if (t < 0)
	{
		m_animationTime2 = m_animationTime1 + (long)((double)abs(m_animationLeft2 - m_animationLeft1) / ((double)get_sample_rate() * get_animation_rate())) * CLOCKS_PER_SEC;
	}
	else
	{
		m_animationTime2 = m_animationTime1 + t * CLOCKS_PER_SEC / 1000;
	}
}

// =====================================================================================================================
//    Ask parent dialogue for keyboard input. This is needed if this control is to be embedded in a dialogue box, or
//    else, we will not get OnChar() messages.
// =======================================================================================================================
UINT tracing::OnGetDlgCode(void)
{
	return DLGC_WANTCHARS;
}

// =====================================================================================================================
//    Get the bounds of given subview in our coordinates.
// =======================================================================================================================
CRect tracing::get_subview_bounds(long i) const
{
	CRect rect(0, 0, 0, 0);

	if (i >= 0 && i < (long)ksubviews.size())
	{
		ksubviews[i]->GetWindowRect(&rect);
		ScreenToClient(&rect);
	}

	return rect;
}

// =====================================================================================================================
//    Respond to mouse-down event. We redirect messages to subviews as needed.Other mouse-message- handling methods need
//    not bother as SetCapture() ensures that messages go directly to the appropriate window.See OnMouseMove (). We do
//    route mouse clicks that fall in between compressed subviews, so that the compressed view appears to the user as an
//    integrated, unified component.
// =======================================================================================================================
void tracing::OnLButtonDown(UINT k, CPoint p0)
{
	stop_animating();

#ifdef ACTIVE_CONTRACTION
	if (k & MK_SHIFT)
	{
		if (resize_contraction(p0, resize_peak))
		{
			return;
		}
	}
	else if (k & MK_CONTROL)
	{
		if (resize_contraction(p0, resize_add))
		{
			return;
		}
	}
	else if (resize_contraction(p0, resize_start))
	{
		return;
	}

#endif

	if (has_subviews())
	{
		long i;

		create_subviews();
		i = find_subview(p0);
		if (i < 0)
		{
			object obj = find(p0);
			if (obj == ozoombtn)
			{
				q = obj;
				qp = p0;
				SetFocus();
				SetCapture();
				return;
			}
			if (IsToggleButtonAvailable())
			{				
				if (obj == otoggleleft || obj == otoggleright)
				{
					q = obj;
					qp = p0;
					SetFocus();
					SetCapture();
					return;
				}
			}
		}

		if (i < 0 && ksubviews.size() > 2)
		{
			CRect r2 = get_subview_bounds(2);

			if (p0.y >= r2.top && p0.y <= get_subview_bounds((long)ksubviews.size() - 1).bottom)
			{
				i = 2;
				p0.y = (r2.top + r2.bottom) / 2;
			}
		}

		if (i >= 0)
		{
			ClientToScreen(&p0);
			ksubviews[i]->ScreenToClient(&p0);
			ksubviews[i]->OnLButtonDown(k, p0);
		}
		
		return;
	}

	SetFocus();
	SetCapture();
	switch (find(p0))
	{
	case obuttondelete:
		q = qdownindelete;
		update();
		break;

	case obuttonaccept:
		q = qdowninaccept;
		update();
		break;
	case otoggleleft:
		q = otoggleleft;
		break;
	case otoggleright:
		q = otoggleright;
		break;
	case ozoombtn:
		q = ozoombtn;
		break;
	default:
		q = qdown;
		qp = p0;
		break;
	}
}

// =====================================================================================================================
//    Respond to mouse-up event. See OnMouseMove().We don't bother with view types other than Fhr or Up because execution
//    should never this method for the parent tracing in composite view types.
// =======================================================================================================================
void tracing::OnLButtonUp(UINT k, CPoint pt)
{
#ifdef ACTIVE_CONTRACTION
	if (resize_contraction(pt, resize_end))
	{
		if (q == qdown)
		{
			q = qidle;
			return;
		}
	}

#endif

	ReleaseCapture();
	get_parent_root()->SetFocus();
	switch (q)
	{
	case qdown:
		switch (find(pt))
		{
		case oprimary:
			switch (get_type())
			{
			case tfhr:
				select(cevent, find_index(pt));
				break;

			case tup:
				select(ccontraction, find_index(pt));
				break;
			}
			break;

		default:
			if (find(qp) != oslider && is_subview() && is_compressed())
			{
				tracing* useTracing = get_parent_for_view();
				bool is15MinView = useTracing->IsToggleButtonAvailable();	
				CRect sliderBounds = get_bounds(oslider);
				int halfWidth = is15MinView ? sliderBounds.Width()/4 : sliderBounds.Width()/2;
				//get_parent_for_view()->move_to(get_parent_for_view()->get_sample_rate() * get_index_from_x(pt.x - get_bounds(oslider).Width() / 2) / get_sample_rate());
				get_parent_for_view()->move_to(get_parent_for_view()->get_sample_rate() * get_index_from_x(pt.x - halfWidth) / get_sample_rate());
			}

			if (is_subview() && is_compressed() && get_parent_root()->is_zoomed())
			{
				get_parent_root()->zoom(false, true);
			}

			deselect();
			break;
		}
		break;

	case qdownindelete:
	case qdowninaccept:
	case qdownout:
		if (find(pt) == obuttondelete)
		{
			tracing*  r = get_parent_root();

			if (has_selection() && get_selection_type() == cevent)
			{
				r->GetParent()->SendMessage(WM_COMMAND, MAKELONG(r->GetDlgCtrlID(), ndeleteevent), (LPARAM) r->GetSafeHwnd());
				update();
			}
			else if (has_selection() && get_selection_type() == ccontraction)
			{
				r->GetParent()->SendMessage(WM_COMMAND, MAKELONG(r->GetDlgCtrlID(), ndeletecontraction), (LPARAM) r->GetSafeHwnd());
				update();
			}
		}
		else if (find(pt) == obuttonaccept)
		{
			tracing*  r = get_parent_root();

			if (has_selection() && get_selection_type() == cevent)
			{
				r->GetParent()->SendMessage(WM_COMMAND, MAKELONG(r->GetDlgCtrlID(), nacceptevent), (LPARAM) r->GetSafeHwnd());
				update();
			}
		}
		break;

	case qresizingparent:
	case qslidingparent:
		OnMouseMove(k, pt);
		if (is_subview() && is_compressed() && get_parent_root()->is_zoomed())
		{
			get_parent_root()->zoom(false, true);
		}
		break;

	case qsliding:
		OnMouseMove(k, pt);
		break;
	case otoggleleft:
		{
			if (IsToggleButtonAvailable())
			{
				ToggleSwitchLeft();
			}
		}
		break;
	case otoggleright:
		{
			if (IsToggleButtonAvailable())
			{
				ToggleSwitchRight();
			}
		}
		break;
	case ozoombtn:
		SwitchViews();
		break;
	}

	q = qidle;
}

#ifdef ACTIVE_CONTRACTION

bool tracing::resize_contraction(CPoint p0, resize_action action)
{
	if (!has_debug_keys())
		return false;

	if (has_subviews())
	{
		create_subviews();
		long i = find_subview(p0);

		if (i < 0 && ksubviews.size() > 2)
		{
			CRect r2 = get_subview_bounds(2);

			if (p0.y >= r2.top && p0.y <= get_subview_bounds((long)ksubviews.size() - 1).bottom)
			{
				i = 2;
				p0.y = (r2.top + r2.bottom) / 2;
			}
		}

		if (i >= 0)
		{
			CPoint p1 = p0;
			ClientToScreen(&p1);
			ksubviews[i]->ScreenToClient(&p1);
			return (ksubviews[i]->resize_contraction(p1, action));
		}

		return false;
	}

	if (m_tracingType != tup)
	{
		return false;
	}

	long index = get_index_from_x(p0.x);
	const fetus* curFetus = get();

	switch (action)
	{
		// Start resizing an existing contraction?
	case resize_start:
		if (has_selection() && (!contraction_move_start && !contraction_move_end))
		{
			const contraction& ci = curFetus->get_contraction(get_selection());

			if (((index >= ci.get_start() - 5) && (index <= ci.get_start() + 10)) || ((index >= ci.get_end() - 10) && (index <= ci.get_end() + 5)))
			{
				if (2 * index >= ci.get_end() + ci.get_start())
				{
					contraction_move_end = true;
				}
				else
				{
					contraction_move_start = true;
				}
				break;
			}
		}
		break;

		// Add a new contraction?
	case resize_add:
		if ((index >= 0) && (index < curFetus->get_number_of_up() - 10))
		{
			long position = 0;
			contraction* next = NULL;

			while (position < curFetus->GetContractionsCount())
			{
				const contraction& ci = curFetus->get_contraction(position);
				if ((ci.get_start() <= index) && (ci.get_end() >= index))
				{
					return false;
				}

				if (ci.get_start() > index)
				{
					next = const_cast<contraction*>(&ci);
					break;
				}
				++position;
			}

			long end = index + 10;
			if ((next != NULL) && (end >= next->get_start()))
			{
				break;
			}

			contraction new_contraction(index, (index + end) / 2, end);
			const_cast<fetus*>(curFetus)->append_contraction(new_contraction);

			select(ccontraction, find_index(p0)); 
			update();

			contraction_move_end = true;
			return true;
		}
		break;

		// Move the contraction peak time
	case resize_peak:
		if (has_selection() && get_selection_type() == ccontraction)
		{
			const contraction& ci = curFetus->get_contraction(get_selection());

			if (index > ci.get_start() && index < ci.get_end())
			{
				(const_cast<contraction*>(&ci))->set_peak(index);
				const_cast<fetus*>(curFetus)->note(subscription::mfetus);
				update();

				return true;
			}
		}
		break;

	case resize_move:
		if ((contraction_move_start || contraction_move_end) && has_selection() && get_selection_type() == ccontraction)
		{
			int selection = get_selection();
			const contraction&	ci = curFetus->get_contraction(selection);

			if (contraction_move_start)
			{
				index = min(index, ci.get_end() - 9);
				if (selection > 0)
				{
					const contraction&	previous = curFetus->get_contraction(selection - 1);
					index = max(index, previous.get_end());
				}
			}
			else
			{
				index = max(index, ci.get_start() + 9);
				if (selection < curFetus->GetContractionsCount() - 1)
				{
					const contraction&	next = curFetus->get_contraction(selection + 1);
					index = min(index, next.get_start());
				}
			}

			index = min(index, curFetus->get_number_of_up());
			index = max(index, 0);

			if (contraction_move_start && index != ci.get_start())
			{
				(const_cast<contraction*>(&ci))->set_start(index);
				(const_cast<contraction*>(&ci))->set_peak((index + ci.get_end()) / 2);
			}
			else if (contraction_move_end && index != ci.get_end())
			{
				(const_cast<contraction*>(&ci))->set_peak((ci.get_start() + index) / 2);
				(const_cast<contraction*>(&ci))->set_end(index);
			}		
			const_cast<fetus*>(curFetus)->note(subscription::mfetus);
			update();

			return true;
		}
		break;

	case resize_end:
		if (contraction_move_start || contraction_move_end)
		{
			contraction_move_start = false;
			contraction_move_end = false;

			return true;
		}
		break;
	}

	return false;
}
#endif

void tracing::ChangeToggleButtonStateWhileSliding(CPoint p0, bool expanded)
{
	tracing* useTracing = NULL;
	if (expanded && is_subview())
		useTracing = get_parent_root();
	else if (!expanded)
		useTracing = get_parent_for_view();
	if (useTracing != NULL && useTracing->IsToggleButtonAvailable())
	{
		long diff = expanded? qp.x - p0.x : p0.x - qp.x;
		long shift = (long)(useTracing->get_scaling() * (diff) / get_scaling());
		long long parentLeftInMs = useTracing->get_left_in_ms();
		long long width = useTracing->get_width_in_ms();
		long parentLeft = useTracing->get_index_from_ms(parentLeftInMs) + shift;
		long long newLeftInMs = useTracing->get_ms_from_index(parentLeft);
		long sliderW = useTracing->GetSliderLength();
		long totalLength = useTracing->get()->get_number_of_fhr() / useTracing->get()->get_hr_sample_rate();
		bool leftPartAvailable = newLeftInMs - width > 0 ;
		if (diff < 0 && !useTracing->IsLeftPartShown() && !leftPartAvailable && sliderW < totalLength)
		{
			useTracing->SetLeftPartShown(true);			
			
		}
		else if (diff > 0 && useTracing->IsLeftPartShown() && leftPartAvailable)
		{
			useTracing->SetLeftPartShown(false);			
		}
	}

}
// =====================================================================================================================
//    Respond to mouse-move event. The mouse-handling mechanism is a finite-state automaton with q as its state, taken
//    from set {qdown...}.Parameters qd and qp are set at mouse-down time.
// =======================================================================================================================
void tracing::OnMouseMove(UINT k, CPoint p0)
{
#ifdef ACTIVE_CONTRACTION
	if (resize_contraction(p0, resize_move))
	{
		return;
	}
#endif

	switch (q)
	{
	case qdown:
		if (abs(p0.x - qp.x) > 2 || abs(p0.y - qp.y) > 2)
		{
			object o = find(qp);

			if (k & MK_CONTROL || (!has_parent_for_view()))
			{
				qd = get_offset();
				q = qsliding;
			}
			else
			{
				switch (o)
				{
				case oslider:
					
					qd = get_parent_for_view()->get_offset();
					q = qslidingparent;
					break;

				case osliderleft:
					if (is_scaling_locked())
					{						
						qd = get_parent_for_view()->get_offset();
						q = qslidingparent;
					}
					else
					{
						qi = get_index_from_x(get_bounds(oslider).right);
						q = qresizingparent;
					}
					break;

				case osliderright:
					if (is_scaling_locked())
					{
						

						qd = get_parent_for_view()->get_offset();
						q = qslidingparent;
					}
					else
					{
						qi = get_index_from_x(get_bounds(oslider).left);
						q = qresizingparent;
					}
					break;

				case oprimary:
				case osecondary:
				{
					CRect rect = get_bounds(oslider);
					rect.bottom += 4;
					if (rect.PtInRect(qp))
					{
						
						qd = get_parent_for_view()->get_offset();
						q = qslidingparent;
						break;
					}
				}

				default:
					if (!is_scaling_locked())
					{
						qi = get_index_from_x(p0.x);
						q = qresizingparent;
					}
					else
					{
						
						qd = get_offset();
						q = qsliding;
					}
					break;
				}
			}

			OnMouseMove(k, p0);
		}
		break;

	case qdownindelete:
		if (find(p0) != obuttondelete)
		{
			q = qdownout;
			update();
		}
		break;

	case qdowninaccept:
		if (find(p0) != obuttonaccept)
		{
			q = qdownout;
			update();
		}
		break;

	case qdownout:
		if (find(p0) == obuttondelete)
		{
			q = qdownindelete;
			update();
		}
		else if (find(p0) == obuttonaccept)
		{
			q = qdowninaccept;
			update();
		}
		break;

	case qidle:
		if (!is_subview())
		{
			update_guides(p0);
		}
		break;

	case qresizingparent:
		get_parent_for_view()->move_to(qi, (long)(get_parent_for_view()->get_scaling() * get_index_from_x(p0.x) / get_scaling()));
		break;

	case qsliding:
		ChangeToggleButtonStateWhileSliding(p0, true);
		set_offset(qd + qp.x - p0.x);
		break;

	case qslidingparent:
		ChangeToggleButtonStateWhileSliding(p0, false);
		get_parent_for_view()->set_offset(qd + (long)(get_parent_for_view()->get_scaling() * (p0.x - qp.x) / get_scaling()));
		break;
	}
	if (is_subview())
	{
		get_parent_root()->follow(this);
	}
}

// =====================================================================================================================
//    Redraw contents when asked to by the framework. This simply slaps the off-screen bitmap in the current device
//    context.We only redraw if we know there is an update pending.See comment for method draw(). Method
//    OnPaint_propagate() forces redraw for all subscribed views.It visits all views using the same scheme as the
//    update... () methods.Flag ipropagate is set to false by OnPaint_propagate() so that OnPaint() does not reënter the
//    propagation mechanism.
// =======================================================================================================================
void tracing::OnPaint(void)
{
	CPaintDC dc(this);

	CRect rcBounds;
	GetClientRect(&rcBounds);

	double_buffer dbDC(&dc, &rcBounds);
	draw(&dbDC);
	DefWindowProc(WM_PAINT, (WPARAM)dbDC.m_hDC, (LPARAM)0);

	if (ipropagate)
	{
		OnPaint_propagate();
	}
}

void tracing::OnPaint_propagate(bool iup)
{
	if (iup && has_parent())
	{
		get_parent()->OnPaint_propagate();
	}
	else
	{
		ipropagate = false;
		RedrawWindow(NULL, NULL, RDW_UPDATENOW);
		ipropagate = true;
		for (long i = 0, n = (long)kchildren.size(); i < n; i++)
		{
			kchildren[i]->OnPaint_propagate(false);
		}
	}
}

// =====================================================================================================================
//    Respond to timer events. This is used by debug code to test real-time contraction and event detection. In case
//    tanimation, we really stop animating through setting iinontimer to false.
// =======================================================================================================================
void tracing::OnTimer(UINT_PTR k)
{
	switch (k)
	{
	case tanimation:
		{
			iinontimer = true;
			if (!is_animating())
			{
				KillTimer(tanimation);
				update();
			}
			else
			{
				tracing*  curTracing = get_parent_for_view();
				long dl;
				long dr;

				if (m_animationTime2 > m_animationTime1)
				{
					dl = m_animationLeft1 + (long)((long long)(clock() - m_animationTime1) * (long long)(m_animationLeft2 - m_animationLeft1) / (long long)(m_animationTime2 - m_animationTime1));
					dr = m_animationRight1 + (long)((long long)(clock() - m_animationTime1) * (long long)(m_animationRight2 - m_animationRight1) / (long long)(m_animationTime2 - m_animationTime1));
				}
				else
				{
					dl = m_animationLeft2;
					dr = m_animationRight2;
				}

				if ((m_animationLeft2 < m_animationLeft1 && dl <= m_animationLeft2) || (m_animationLeft2 > m_animationLeft1 && dl >= m_animationLeft2))
				{
					dl = m_animationLeft2;
					iinontimer = false;
					ianimating = false;
				}

				if ((m_animationRight2 < m_animationRight1 && dr <= m_animationRight2) || (m_animationRight2 > m_animationRight1 && dr >= m_animationRight2))
				{
					dr = m_animationRight2;
					iinontimer = false;
					ianimating = false;
				}

				if (!iinontimer && curTracing->is_zooming() && !curTracing->is_zoomed())
				{
					curTracing->zleft = curTracing->zright = -1;
				}

				move_to(dl, dr);
			}

			iinontimer = false;
		}
		break;
	}
}

// =====================================================================================================================
//    Reset guides.
// =======================================================================================================================
void tracing::reset_guides(void)
{
	if (g1 != g2)
	{
		g1 = g2 = 0;
		update();
	}
}

// =====================================================================================================================
//    Reset subview lengths. This sets both the compressed and expanded subviews to unbounded length.See set_lengths().
// =======================================================================================================================
void tracing::reset_lengths(void)
{
	set_lengths(-1, -1, -1);
}

// =====================================================================================================================
//    Scale tracing to fit available width. This method is deprecated.Use set_scaling_mode(sfit) or(sfree).
// =======================================================================================================================
void tracing::scale_to_fit(bool i)
{
	set_scaling_mode(i ? sfit : sfree);
}

// =====================================================================================================================
//    Scroll automatically from last sample. If this is true, whenever a point is added, we adjust the offset or, if
//    scaling to fit, the scaling factor.See predicate is_scrolling().This is used by update_now().
// =======================================================================================================================
void tracing::scroll(bool i0)
{
	stop_animating();
	if (has_parent() && ifollowparent)
	{
		get_parent()->scroll(i0);
	}

	if (i0 != iscrolling)
	{
		iscrolling = i0;
		if (iscrolling)
		{
			update();
		}
	}
}

// =====================================================================================================================
//    Select an object. If we are a composite view, that is, if we have subviews, we redirect the selection appropriately
//    to our subviews.If we are a subview, we let our parent know selection has changed so that it may let our siblings
//    know what's going on.If we are a non- composite view, we simply do our thing and update the view. The
//    select_low_level() method does the actual job for an instance, while the select() method redirects traffic.
//    Semantics of iselection and tselection is discussed in the class comment.See methods deselect(), get_selection(),
//    get_selection_type() and has_selection().
// =======================================================================================================================
void tracing::select(selectable t0, long i0)
{
	if (i0 < 0)
	{
		t0 = cnone;
	}

	if ((t0 != cnone) && (tselection == t0) && (iselection == i0))
	{
		deselect();
		return;
	}

	if (is_subview() && kparent != 0)
	{
		get_parent_root()->select(t0, i0);
	}
	else
	{
		reset_guides();
		if (has_subviews())
		{
			for (long i = 0, n = (long)ksubviews.size(); i < n; i++)
			{
				ksubviews[i]->select_low_level(t0, i0);
			}
		}

		select_low_level(t0, i0);
	}
}

void tracing::select_low_level(selectable t0, long i0)
{
#ifdef ACTIVE_CONTRACTION
	contraction_move_start = false;
	contraction_move_end = false;
#endif

	if (tselection != t0 || t0 != cnone && iselection != i0)
	{
		tselection = t0;
		iselection = i0;
		update();
	}
}

// =====================================================================================================================
//    Set the fetus to display. IMPORTANT: we only own the given instance if its predicate has_conductor() returns
//    false.If it returns true, then we know the the associated conductor instance owns it, therefore we do not own it
//    ourselves.The property is mutable and the protected method is const in order to do lazy instanciation.See method
//    set_partial ().
// =======================================================================================================================
void tracing::set(fetus* curFetus) const
{
	const_cast<tracing&>(*this).set(curFetus);
}

void tracing::set(fetus* curFetus)
{
	deselect();
	stop_animating();
	stop_zooming();

	if (m_pFetus)
	{
		m_pFetus->unsubscribe((void*)this);
		if (!m_pFetus->has_conductor())
		{
			delete m_pFetus;
		}
	}

	m_pFetus = curFetus;
	if (m_pFetus)
	{
		m_pFetus->subscribe(new subscription_to_fetus(this, m_pFetus));
		m_pFetus->ResetContracionRate();
	}

	update_data();
	update_now();
}

// =====================================================================================================================
//    Set the animation rate. This is the rate, in seconds of tracing par second, at which the tracing is moved towards
//    the destination.In other words, this is the accelerating factor at which the tracing is animated.A factor of 1(one)
//    means that the tracing is animated at the speed at which the monitor originally received the data.The rate must be
//    greater than 0(zero), otherwise, the behaviour is undefined, though the control is guaranteed to behave
//    gracefully.This may be set while the tracing is already animating.See methods get_animation_rate() and
//    move_to_animating().
// =======================================================================================================================
void tracing::set_animation_rate(double r0)
{
	m_animationRate = r0;
	if (is_animating())
	{
		stop_animating();
	}

	move_to_animating(m_animationLeft2, m_animationRight2, -1, 1000);
}

// =====================================================================================================================
//    Set the user's permission to delete objects.
// =======================================================================================================================
void tracing::set_can_delete(bool i0)
{
	if (icandelete != i0)
	{
		icandelete = i0;
		adjust_subviews();
		update();
	}
}

// =====================================================================================================================
//    Give or remove access to debug keys.
// =======================================================================================================================
void tracing::set_has_debug_keys(bool d)
{
	idebugkeys = d;
	get()->note(subscription::mfetus);
	update();
}

// =====================================================================================================================
//    Set compressed and expanded subview lengths. This sets the lengths, in seconds, of the compressed and expanded
//    subviews when in tnormal and tnormalnc view types.If a length is 0 (zero) or less then the length is unbounded and
//    implicitely determined through paper proportions or scaling factors.See methods get_compressed_length(),
//    get_expanded_length(), has_compressed_length(), has_expanded_length() and reset_lengths ().
// =======================================================================================================================
void tracing::set_lengths(long le0, long lc0, long sliderLength)
{
	if (lc0 != lcompressed || le0 != lexpanded)
	{
		lcompressed = lc0;
		lexpanded = le0;
		m_sliderLength = sliderLength;
		if (get_type() == tnormal || get_type() == tnormalnc && le0 != lexpanded)
		{
			adjust_subviews();
			update();
		}
	}
}

// =====================================================================================================================
//    Change the scaling
// =======================================================================================================================
void tracing::priv_set_scaling(double s0) const
{
	if (m_scaling != s0)
	{
		if (offset != 0 && !is_scrolling())
		{
			long long t1 = get_left_in_ms();
			m_scaling = s0;

			long o = get_index_from_ms(t1);
			const_cast<tracing*>(this)->move_to(o, -1, true);
		}
		else
		{
			m_scaling = s0;
		}
	}
}

// =====================================================================================================================
//    Change the offset
// =======================================================================================================================
void tracing::priv_set_offset(long d0) const
{
	if (d0 != offset)
	{
		offset = d0;
	}
}

// =====================================================================================================================
//    Set desired tracing offset. The offset is always a positive value, measured in pixels, after any scaling.For
//    instance, an offset of 40 means that the tracing is shifted 40 pixels to the left, whatever the current scaling
//    factor.Offsets are ignored when scaling to fit. When in tnormal view, that is, when we have compressed subviews, we
//    need to make sure the slider stays within the compressed views.
// =======================================================================================================================
void tracing::set_offset(long d0)
{
	stop_animating();
	if (has_parent() && ifollowparent)
	{
		get_parent()->set_offset(d0);
		return;
	}

	if (get_scaling_mode() == sfit)
	{
		return;
	}

	// If we don't have a real tracing, no need to really change the offset there
	if (!get()->has_start_date())
	{
		d0 = 0;
	}

	// If the new offset is below 0, we have to make sure there is a real good reason
	if (d0 < 0)
	{
		if (get_scaling_mode() == spaper)
		{
			d0 = 0;
		}
		else
		{
			long number_of_displayed_indexes = get_sample_rate() * (long)((get_bounds(otracing).right - get_bounds(otracing).left) / priv_get_scaling());
			long minimum_allowed_offset = (long)(priv_get_scaling() * (long)((min((get_number() - number_of_displayed_indexes), 0)) / get_sample_rate()));
			d0 = max(d0, minimum_allowed_offset);
		}
	}

	if (offset != d0)
	{
		priv_set_offset(d0);

		if (get_type() == tnormal && !ksubviews.empty())
		{
			long long target = get_left_in_ms();
			tracing*  s2 = ksubviews[2];

			long long current = s2->get_left_in_ms();
			if (target < current)
			{
				if (target > current - (s2->get_width_in_ms() / 2))
				{
					s2->move_to(s2->get_index_from_ms(current - (s2->get_width_in_ms() / 2)));
				}
				else
				{
					s2->move_to(s2->get_index_from_ms(target));
				}
			}
			else if (target + get_width_in_ms() > current + s2->get_width_in_ms())
			{
				if (target + get_width_in_ms() < current + (3 * s2->get_width_in_ms()) / 2)
				{
					s2->move_to(s2->get_index_from_ms(current + (s2->get_width_in_ms() / 2)));
				}
				else
				{
					s2->move_to(s2->get_index_from_ms(target - s2->get_width_in_ms()));
				}
			}

			follow(s2);
		}

		update();
		scroll(false);
	}
}

// =====================================================================================================================
//    Set the paper format. Taken from paper set.This really just impacts vertical bounds for Fhr tracings
// =======================================================================================================================
void tracing::set_paper(paper p0)
{
	if (kpaper != p0)
	{
		kpaper = p0;
		adjust_subviews();
		update();
	}
}

// =====================================================================================================================
//    Set the partial fetus to display. IMPORTANT: see method set().
// =======================================================================================================================
void tracing::set_partial(fetus* curFetus) const
{
	const_cast<tracing&>(*this).set_partial(curFetus);
}

void tracing::set_partial(fetus* curFetus)
{
	if (fpartial != curFetus)
	{
		if (fpartial)
		{
			fpartial->unsubscribe(this);
		}

		if (fpartial && !fpartial->has_conductor())
		{
			delete fpartial;
		}

		fpartial = curFetus;
		if (fpartial)
		{
			fpartial->subscribe(new subscription_to_fetus(this, fpartial));
		}

		update_data();
	}
}

// =====================================================================================================================
//    Set tracing precision in samples per pixel. This set the number of samples displayed for every pixel of the width
//    of the tracing.The more the samples per pixel, the longer it takes to draw.This should be between 0.5 and 2.
// =======================================================================================================================
void tracing::set_precision(double p0)
{
	if (kprecision != p0)
	{
		kprecision = p0;
		adjust_subviews();
		update();
	}
}

// =====================================================================================================================
//    Set desired scaling factor. The scaling factor is a horizontal scaling only.Vertical positions are calculated so
//    that uterine pressure(Up) values fill the available space. This overrides the scaling-to-fit attribute if
//    set.Updating is triggered as this affects the internal state. The current scaling factor is a real number greater
//    than 0 (zero), 1(one) being normal, 100% size. See methods get_scaling(), get_scaling_mode(), is_scaling_locked(),
//    lock_scaling(), set_scaling() and set_scaling_mode().
// =======================================================================================================================
void tracing::set_scaling(double s0)
{
	if (!has_parent() || !ifollowparent)
	{
		set_scaling_mode(sfree);
		if (m_scaling != s0)
		{
			double w = get_bounds(otracing).Width();

			priv_set_offset((long)(s0 * (w / 2 + offset) / m_scaling - w / 2));
			priv_set_scaling(s0);
			update();
		}
	}
	else
	{
		get_parent()->set_scaling(s0);
	}
}

// =====================================================================================================================
//    Set the scaling mode. the scaling mode, if different from sfree, means that the scaling factor is calculated
//    automatically.How it is calculated depends on the mode, see the class comment for a description of these modes.
// =======================================================================================================================
void tracing::set_scaling_mode(scaling_mode m0)
{
	if (m0 == sfit)
	{
		stop_animating();
	}

	if (has_parent() && ifollowparent)
	{
		get_parent()->set_scaling_mode(m0);
	}

	if (m_scalingMode != m0 && (m_scalingMode = m0) != sfree)
	{
		if (m_scalingMode == sfit)
		{
			priv_set_offset(0);
		}

		update();
	}
}

// =====================================================================================================================
//    Set the type of graph to display. See get_type().We destroy the subviews and method draw(), if and when it gets
//    called, will call create_subviews().See also these methods.
// =======================================================================================================================
void tracing::set_type(type newType)
{
	if (m_tracingType != newType)
	{
		destroy_subviews();
		m_tracingType = newType;
		update_data();
	}
}

// =====================================================================================================================
//    Show or hide given element. The ishow property is a bit field and the showable enumeration members are given
//    indices in it.
// =======================================================================================================================
void tracing::show(showable k, bool i0)
{
	if ((ishow & (long)k ? 1 : 0) != (i0 ? 1 : 0))
	{
		if (i0)
		{
			ishow |= (long)k;
		}
		else
		{
			ishow &= ~ (long)k;
		}

		if (k != wguides)
		{
			adjust_subviews();
		}

		if (has_selection() && get_selection_type() == cevent)
		{
			if (k == wstruckout && !i0 && get()->get_event(get_selection()).is_strike_out())
			{
				deselect();
			}
			else if (k == whideevents && i0)
			{
				deselect();
			}
		}

		update();
	}
}

// =====================================================================================================================
//    Should the grid be visible? This method is deprecated.Use show(wgrid) instead.
// =======================================================================================================================
void tracing::show_grid(bool i0)
{
	show(wgrid, i0);
}

// =====================================================================================================================
//    Stop animating the offset. We simply reset the ianimating flag, as OnTimer() is responsible for killing the
//    animation timer.We do not stop animation if the iinontimer flag is set, as this means that we are getting called
//    ultimately by a method called by OnTimer() for animating.See move_to_animating().Furthermore, we do not stop if
//    zooming is in progress.This is needed because we do not separate low-level and high-level
//    functionalities.Specifically, methods like set_offset() call stop_animating() are themselves called by low-level
//    implementation.
// =======================================================================================================================
void tracing::stop_animating(void)
{
	if (ianimating && !iinontimer && !get_parent_for_view()->is_zooming())
	{
		ianimating = false;
	}
}

// =====================================================================================================================
//    Text representation of time interval in samples. We assume a 4-Hz frequency, that is, 1 min = 240 samples.
// =======================================================================================================================
string tracing::string_from_number_of_samples(long p) const
{
	char t[100];

	if (p < 60)
	{
		sprintf(t, "%ld s", p / 4);
	}
	else
	{
		sprintf(t, "%ld min", p / 240);
	}

	return t;
}

// =====================================================================================================================
//    Add given tracing as one of our subscribed views.
// =======================================================================================================================
void tracing::subscribe_child(tracing* k0)
{
	bool ifound = false;

	for (long i = 0, n = (long)kchildren.size(); i < n; i++)
	{
		if (kchildren[i] == k0)
		{
			ifound = true;
			i = n;
		}
	}

	if (!ifound)
	{
		kchildren.insert(kchildren.end(), k0);
	}
}

// =====================================================================================================================
//    Subscribe as a child view of the given view. We become a slave view of the given view, which becomes our parent
//    view.A sliding window is displayed, showing what part of the tracing the parent view is displaying.Our instances of
//    fetus become useless and all calls to methods get() and get_partial() are routed to the parent view's instances.See
//    method update(). If i0 is true, then this instance follows the parent instance's view(offset, scaling, etc.) and
//    displays the parent's sliding windows instead of inferring one from the parent's view.See draw_background().
//    IMPORTANT: cascading slave views will not behave as expected, for lowly implementation reasons, though one may very
//    well make the implementation more robust without changing the Api.
// =======================================================================================================================
void tracing::subscribe_to(tracing* k0, bool i0)
{
	unsubscribe();
	kparent = k0;
	if (kparent)
	{
		kparent->subscribe_child(this);
		ifollowparent = i0;
		update();
	}
}

// =====================================================================================================================
//    Receive fetus messages through subscription.
// =======================================================================================================================
void tracing::subscription_to_fetus::note(message msg)
{
	switch (msg)
	{
	case mfetus:
		m_pTracing->update_data();
		m_pTracing->update_now();
		break;

	case mpatientstatus:
		m_pTracing->update();
		break;

	case mwilldeletefetus:
		if (m_pTracing->get() == m_pFetus)
		{
			m_pTracing->set(0);
		}
		else if (m_pTracing->get_partial() == m_pFetus)
		{
			m_pTracing->set_partial(0);
		}
		break;
	}
}

// =====================================================================================================================
//    Unsubscribe from our parent view. This is called automatically when an instance is deleted so that no dangling
//    pointers remain.See method subscribe().
// =======================================================================================================================
void tracing::unsubscribe(void)
{
	if (kparent)
	{
		tracing*  k0 = kparent;

		kparent = 0;
		k0->unsubscribe_child(this);
		update();
	}
}

// =====================================================================================================================
//    Unsubscribe a specific child view. This is meant to be called whenever an instance subscribed to this instance
//    unsubscribes.This notifies us it no longer is linked to this instance.
// =======================================================================================================================
void tracing::unsubscribe_child(tracing* k0)
{
	for (long i = 0, n = (long)kchildren.size(); i < n; i++)
	{
		if (kchildren[i] == k0)
		{
			kchildren[i]->unsubscribe();
			kchildren.erase(kchildren.begin() + i);
			i = n;
		}
	}
}

// =====================================================================================================================
//    Unsubscribe all child views. This is meant to be called when an instance is deleted.
// =======================================================================================================================
void tracing::unsubscribe_children(void)
{
	while (!kchildren.empty())
	{
		kchildren[0]->unsubscribe();
	}
}

// =====================================================================================================================
//    Asynchronously update when something changes. The internal update mechanism is built on a set of update...()
//    methods and the update_now() method.The update_now() method is meant to be called whenever one needs the update
//    operation to be performed before proceeding.For instance, methods that draw or that need the current scaling factor
//    should call update_now() before using metrics information.Methods that change data or metrics or both should call
//    the appropriate update...() method, as documented here. The iup parameter lets us propagate the message by first
//    climbing the tree-like structure up to the ultimate parent, then recursively descending the structure while
//    performing the actual work.We call the child views' update...() methods regardless because they may already have
//    been updated while we haven't been yet. update(): triggers asynchronous redrawing when the underlying data hasn't
//    changed.This is meant to be called, for instance, when the scaling factor or the offset change.This method scraps
//    the current list of mark rectangles as their position on screen likely has changes.The list will be rebuilt at draw
//    data.This is meant to be called when the underlying data does change.
// =======================================================================================================================
void tracing::update(bool iup)
{
	if (iup && has_parent())
	{
		get_parent()->update();
	}
	else
	{
		if (!ineedsupdate)
		{
			ineedsupdate = true;
			if (GetSafeHwnd())
			{
				RedrawWindow(NULL, NULL, RDW_INVALIDATE);
			}
		}

		for (long i = 0, n = (long)kchildren.size(); i < n; i++)
		{
			kchildren[i]->update(false);
		}
	}
}

void tracing::update_data(bool iup)
{
	// Flush the selection if the item does not exist anymore
	if ((m_pFetus != 0) && (has_selection()) && (((get_selection_type() == cevent) && (m_pFetus->GetEventsCount() <= get_selection())) || ((get_selection_type() == ccontraction) && (m_pFetus->GetContractionsCount() <= get_selection()))))
	{
		deselect();
	}

	if (iup && has_parent())
	{
		get_parent()->update_data();
	}
	else
	{
		if (!idataneedsupdate)
		{
			idataneedsupdate = true;
			if (!has_parent())
			{
				update();
			}
		}

		for (long i = 0, n = (long)kchildren.size(); i < n; i++)
		{
			kchildren[i]->update_data(false);
		}
	}
}

// =====================================================================================================================
//    Update guides for given cursor position. We set guides to the start and end of the object under the cursor.
// =======================================================================================================================
void tracing::update_guides(CPoint pt)
{
	long a1 = 0;
	long a2 = 0;
	long i;

	if (has_subviews())
	{
		switch (find_subview(pt))
		{
		case 0:
			ClientToScreen(&pt);
			ksubviews[0]->ScreenToClient(&pt);
			i = ksubviews[0]->find_index(pt);
			if (i >= 0 && ((get_selection_type() == cevent && i == get_selection()) || ksubviews[0]->find(pt) == oprimary))
			{
				a1 = get()->get_event(i).get_start();
				a2 = get()->get_event(i).get_end();
			}
			break;

		case 1:
			ClientToScreen(&pt);
			ksubviews[1]->ScreenToClient(&pt);
			i = ksubviews[1]->find_index(pt);
			if (i >= 0 && ((get_selection_type() == ccontraction && i == get_selection()) || ksubviews[1]->find(pt) == oprimary))
			{
				a1 = get()->get_hr_sample_rate() * get()->get_contraction(i).get_start() / get()->get_up_sample_rate();
				a2 = get()->get_hr_sample_rate() * get()->get_contraction(i).get_end() / get()->get_up_sample_rate();
			}
			break;
		}
	}
	else if (!is_compressed())
	{
		i = find_index(pt);
		if (i >= 0 && (has_selection() || find(pt) == oprimary))
		{
			switch (get_type())
			{
			case tfhr:
				a1 = get()->get_event(i).get_start();
				a2 = get()->get_event(i).get_end();
				break;

			case tup:
				a1 = get()->get_contraction(i).get_start();
				a2 = get()->get_contraction(i).get_end();
				break;
			}
		}
	}

	if (a1 != g1 || a2 != g2)
	{
		g1 = a1;
		g2 = a2;
		update();
	}
}

// =====================================================================================================================
// Refresh the layout (recalculation of height to respect aspect ratio)
// =====================================================================================================================
void tracing::refresh_layout(void)
{
	idataneedsupdate = true;
	ineedsupdate = true;
	update_now();

	if (has_subviews())
	{
		for (int i = 0; i < (int)ksubviews.size(); ++i)
		{
			ksubviews[i]->refresh_layout();
		}
		update(true);
	}
}

// =====================================================================================================================
//    Update internal state for current data and presentation. This computes whatever we need in order to perform draw
//    operations our whenever we need to know metrics information.See discussion for update...() methods. When updating,
//    we scroll as needed in order to see the last sample.
// =======================================================================================================================
void tracing::update_now(void) const
{
	if (idataneedsupdate)
	{
		idataneedsupdate = false;
	}

	if (ineedsupdate)
	{
		ineedsupdate = false;
		switch (get_scaling_mode())
		{
		case sfit:
			priv_set_scaling((double)get_sample_rate() * get_bounds(otracing).Width() / (double)get_number());
			iscrolling = true;
			break;

		case sfree:
		case spaper:
			if (get_scaling_mode() == spaper && has_subviews())
			{
				create_subviews();
				if (ksubviews.size() > 0)
				{
					priv_set_scaling(ksubviews[0]->m_scaling);
				}
			}
			else if (get_scaling_mode() == spaper)
			{
				priv_set_scaling((double)get_bounds(otracing).Height() / (double)(20 * get_paper_height()));
			}

			if (!iscrolling && get()->has_start_date() && (get_index_from_x(get_bounds(otracing).right) >= (get_number() - (4 * get_sample_rate()))))
			{
				const_cast<patterns_gui::tracing*>(this)->scroll(true);
			}

			if (is_scrolling())
			{
				priv_set_offset((long)(m_scaling * get_number() / get_sample_rate() - get_bounds(otracing).Width()));
			}
			break;
		}
	}
}

// =====================================================================================================================
//    Zoom the compressed view. This animates the compressed view in or out of a zoomed mode, that is, a mode where all
//    samples are visible and where the relative position of the original compressed view is indicated with guides. We
//    animate the compressed Fhr subview, that is, subview index 2, relying on method follow() to synchronize the other
//    compressed subviews.
// =======================================================================================================================
void tracing::zoom(bool i0, bool notifyParent)
{
	if ((i0 != is_zoomed()) && ksubviews.size() > 2)
	{
		tracing*  s2 = ksubviews[2];

		// Cannot zoom if less than current display length (when tracing contains only 10 minutes of data for
		// instance)
		if (i0)
		{
			long pl = s2->get_index_from_ms(s2->get_left_in_ms());
			long pr = s2->get_index_from_ms(s2->get_right_in_ms());

			if (s2->get_number() < pr - pl)
			{
				return;
			}
		}

#ifdef _animated_zoom
		if (is_zooming())
		{
			if (izoomed)
			{
				s2->move_to(0, s2->get_number());
			}
			else
			{
				s2->move_to(zleft, zright);
			}
		}
#endif

		izoomed = i0;
		if (izoomed)
		{
			// Remember current display boundaries for tracing 2 for when we'll unzoomed
			CRect rect = s2->get_bounds(otracing);
			zleft = s2->get_index_from_x(rect.left);
			zright = s2->get_index_from_x(rect.right);
#ifndef patterns_research
			const long lMaximumTimespan = 43200;	// We cap to a maximum of 12h(43200s) in the compressed view
#else
			const long lMaximumTimespan = 14400;	// We cap to a maximum of 4h(43200s) in the compressed view
#endif

			if (s2->get_number() > lMaximumTimespan * s2->get_sample_rate())
			{
				long long tMiddle = s2->get_ms_from_index((long)((zleft + zright) / 2));

				long lStart = get_index_from_ms(tMiddle - 1000 * (lMaximumTimespan / 2));
				long lEnd = get_index_from_ms(tMiddle + 1000 * (lMaximumTimespan / 2));

				// First we cap at the max right
				if (lEnd > s2->get_number())
				{
					lStart -= lEnd - s2->get_number();
					lEnd = s2->get_number();
				}

				// Then at the beginning
				if (lStart < 0)
				{
					lEnd -= lStart;
					lStart = 0;
				}

				// Should NEVER happened, that would mean we have less than the maximum length of tracing
				if (lEnd > s2->get_number())
				{
					assert(false);
					lEnd = s2->get_number();
				}

#ifdef _animated_zoom
				s2->move_to_animating(lStart, lEnd, get(kanimationduration));
#else
				s2->move_to(lStart, lEnd);
#endif
			}
			else
			{
#ifdef _animated_zoom
				s2->move_to_animating(0, s2->get_number(), get(kanimationduration));
#else
				s2->move_to(0, s2->get_number());
#endif
			}
		}
		else
		{
			// Check if primary chart is still in the old s2 selected time portion
			CRect rect = ksubviews[0]->get_bounds(otracing);
			long pl = ksubviews[0]->get_index_from_x(rect.left) * s2->get_sample_rate() / ksubviews[0]->get_sample_rate();
			long pr = ksubviews[0]->get_index_from_x(rect.right) * s2->get_sample_rate() / ksubviews[0]->get_sample_rate();

			// If the primary chart is fully outside of the unzoomed portion, recenter it
			if (pr <= zleft || pl >= zright)
			{
				long lmiddle = pl + ((pr - pl) / 2);
				long lspan = zright - zleft;
				zleft = lmiddle - (lspan / 2);
				zright = zleft + lspan;
			}

			if (zleft < 0)
			{
				zright = zright - zleft;
				zleft = 0;
			}

			long ldelta = zright - get_number();
			if (ldelta > 0)
			{
				zright -= ldelta;
				zleft -= ldelta;
			}

#ifdef _animated_zoom
			s2->move_to_animating(zleft, zright, get(kanimationduration));
#else
			s2->move_to(zleft, zright);
#endif
		}

		if (is_subview())
		{
			get_parent_root()->follow(s2);
		}
		else
		{
			follow(s2);
		}
	}
	if(notifyParent && !is_subview() && IsWindow(m_hWnd) && GetParent() != NULL  && ksubviews.size() > 2 )
	{
		GetParent()->PostMessage(SHOW_EXPORT_MSG, izoomed? 0 : 1, 0);
	}
}

// =====================================================================================================================
//    Immediately stops zooming and returns to normal view.
// =======================================================================================================================
void tracing::stop_zooming(void)
{
	zoom(false, true);
}

void tracing::OnSize(UINT nType, int cx, int cy)
{
	if ((nType != SIZE_MINIMIZED) && cx > 0 && cy > 0)
	{
		m_bounds.right = m_bounds.left + cx;
		m_bounds.bottom = m_bounds.top + cy;
	}
	else
	{
		stop_zooming();
	}

	if (has_parent())
	{
		update(false);
	}
	else
	{
		adjust_subviews();
	}
}

void tracing::OnMove(int _x, int _y)
{
	int w = m_bounds.Width();
	int h = m_bounds.Height();

	m_bounds.left = _x;
	m_bounds.right = m_bounds.left + w;

	m_bounds.top = _y;
	m_bounds.bottom = m_bounds.top + h;
}

CRect tracing::get_bounds() const
{
	return m_bounds;
}

void tracing::SetSubviewBounds(int i, int x, int y, int w, int h) const
{
	if (i >= 0 && i < (long)ksubviews.size())
	{
		CRect rect = ksubviews[i]->get_bounds();
		if (rect.left != x || rect.top != y || rect.Width() != w || rect.Height() != h)
		{
			ksubviews[i]->SetWindowPos(0, x, y, w, h, SWP_NOACTIVATE);
		}
	}
}


void tracing::DrawExportedAreaMarker(CDC* dc, long ksubviewIndex) const
{
	if(m_exportOpen && //m_currentExportInterval == 15 && 
		m_currentExportEndTime != undetermined_date && 	get_type() == tnormal &&
		!ksubviews[ksubviewIndex]->is_compressed() && ksubviews[ksubviewIndex]->get_type() == tup && (long)ksubviews.size() > ksubviewIndex+1)
	{
		date endTime = m_currentExportEndTime;
		date startTime = (m_currentExportInterval == 15)? endTime - 900 : endTime - 1800;
		long leftP = get_x_from_ms(startTime * 1000);
		long rightP = get_x_from_ms(endTime * 1000);

		COLORREF blueColor = RGB(231, 244,254);
		CRect upRect, compressedRect;
		ksubviews[ksubviewIndex]->GetWindowRect(upRect);
		ksubviews[ksubviewIndex + 1]->GetWindowRect(compressedRect);
		ScreenToClient(upRect);
		ScreenToClient(compressedRect);
		int availableHeight = compressedRect.top - upRect.bottom;
		int penWith = 3;
		int round = 10;
		int dY = 8;
		if(availableHeight < 22)
		{
			penWith = 1;
			round = 3;
			dY = 3;
		}

		CBrush brushBlue(blueColor);
		CBrush* pOldBrush = dc->SelectObject(&brushBlue);
		CPen penBlue;
		penBlue.CreatePen(PS_SOLID, penWith, RGB(176, 215,246));
		CPen* pOldPen = dc->SelectObject(&penBlue);
		CRect clientRect;
		GetClientRect(clientRect);
		if(clientRect.right < rightP)
		{
			rightP = clientRect.right;	
		}
		CRect colorRect(leftP, upRect.bottom + 1, rightP - 1, compressedRect.top - dY);			
		dc->RoundRect(colorRect, CPoint(round, round));
			
		dc->SelectObject(pOldBrush);
		dc->SelectObject(pOldPen);
		string s = "Export Range";
		CSize sz = dc->GetTextExtent(s.c_str());
		int textLeft = colorRect.left + (colorRect.Width() - sz.cx)/2;
		int textTop = colorRect.top + (colorRect.Height() - sz.cy)/2;
		if(textTop <= colorRect.top)
			textTop = colorRect.top + 1;

		dc->TextOut(textLeft, textTop, s.c_str());
		
	}


}

void tracing::DrawExportedRangeLines(CDC* dc) const
{
	if(IsWCRVisible() )
	{
		CRect clientRect;
		GetClientRect(clientRect);
		CRect tcrRect;
		ksubviews[4]->GetWindowRect(tcrRect);
		ScreenToClient(tcrRect);

		CPen penLight;
		penLight.CreatePen(PS_DOT, 1, RGB(0xc8, 0xc8,0xc8));
		CPen* pOld = dc->SelectObject(&penLight);
		std::set<long>::iterator it = m_visibleChunkEnds.begin();
		for(; it != m_visibleChunkEnds.end(); it++)
		{
			long x = *it;
			dc->MoveTo(x, tcrRect.bottom);
			dc->LineTo(x, clientRect.bottom);
		}

		dc->SelectObject(pOld);
		if(m_highlightedExportInterval != 0 && m_highlightedExportTime != undetermined_date
			|| m_exportOpen && m_currentExportEndTime != undetermined_date && m_currentExportInterval != 0)
		{
			bool bHighlited = (m_highlightedExportInterval != 0 && m_highlightedExportTime != undetermined_date);
			date endTime = bHighlited?
				m_highlightedExportTime : m_currentExportEndTime;
			date startTime = bHighlited?
				endTime - m_highlightedExportInterval * 60 : endTime - m_currentExportInterval * 60;
			long leftP = ksubviews[2]->get_x_from_ms(startTime * 1000);
			long rightP = ksubviews[2]->get_x_from_ms(endTime * 1000);

			CPen penH;
			penH.CreatePen(PS_DOT, 1, RGB(0x90, 0x90,0x90));
		
			CPen* pOldPen = dc->SelectObject(&penH);	
			dc->MoveTo(leftP, tcrRect.bottom);
			dc->LineTo(leftP, clientRect.bottom);
			dc->MoveTo(rightP, tcrRect.bottom);
			dc->LineTo(rightP, clientRect.bottom);
			dc->SelectObject(pOldPen);
		}
	}
		
}
bool tracing::IsWCRVisible() const
{
	return is_visible(wcr);
}

void tracing::MoveToTime(date endTime, bool toggleSwith, bool useAnimating)
{

	CRect rect = get_bounds(otracing);
	long pageLength = get_expanded_length();
	long sampleRate = get_sample_rate();
	long iRight = get_index_from_x(rect.right);	
	long iLeft = iRight - pageLength * sampleRate; 
	long iCurrentLeft = get_index_from_x(rect.left);	
	long iDiff = (iLeft > iCurrentLeft) ? (iLeft - iCurrentLeft)/sampleRate : 0; 

	long long msDestination = (long)(endTime - get()->get_start_date() - iDiff) * 1000;


	long long pageLengthInMs = pageLength * 1000;
	long long iRightInMs = msDestination;
	long long iLeftInMs = max(0, (msDestination - pageLengthInMs));	
	long iNewLeft = iLeftInMs <= 0 ? 0 : get_index_from_ms(iLeftInMs);
	
	long long difference = msDestination - get_ms_from_index(iRight);

	if(useAnimating && abs(difference) < pageLengthInMs * 1.5)
	{		
		move_to_animating(iNewLeft, -1, get(kanimationduration), -1, toggleSwith);
	}
	else
	{
		move_to(iNewLeft, -1, toggleSwith);
	}
	
}

void tracing::MoveCompressedViewToTime(date endTime)
{
	if (get_type() == tnormal && ksubviews.size() > 2)
	{	
		tracing* s2 = ksubviews[2];
		CRect rect = s2->get_bounds(otracing);		
		long iRight = s2->get_index_from_x(rect.right);
		long newRightInMs = (long) (1000 * (endTime - get()->get_start_date()));
		long newRightIndex = s2->get_index_from_ms(newRightInMs);
		if(newRightIndex != iRight)
			s2->move_to(newRightIndex);
	}
}

void tracing::GetCompressedViewBoundsInSeconds(long& left, long& right)
{
	left = 0;
	right = 0;
	
	if(get_type() == tnormal && ksubviews.size() > 2)//IsWCRVisible())
	{
		
		CRect rect = 	ksubviews[2]->get_bounds(otracing);
		long iLeft = ksubviews[2]->get_index_from_x(rect.left);
		long iRight = ksubviews[2]->get_index_from_x(rect.right);
		long long msLeft = ksubviews[2]->get_ms_from_index(iLeft);
		long long msRight = ksubviews[2]->get_ms_from_index(iRight);
		long tLeft = (long)(msLeft * 0.001);
		long tRight = (long)(msRight * 0.001);
		date absolutestart = get()->get_start_date();
		left = absolutestart + tLeft;
		right = absolutestart + tRight;

	}

}

void tracing::SetHighlightedExportTime(DATE highlightedStart, DATE highlightedEnd, bool dlgOpen, bool isExported)
{
	COleDateTime oleStartT(highlightedStart);
	SYSTEMTIME sStartT, sEndT;
	oleStartT.GetAsSystemTime(sStartT);
	COleDateTime oleEndT(highlightedEnd);
	oleEndT.GetAsSystemTime(sEndT);

	SYSTEMTIME utcStart = fetus::convert_to_utc(sStartT);
	SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);
	long roundSecond = 0;
	if (utcEnd.wSecond == 59)
	{
		roundSecond = 1;
	}

	date highlightedExportTime = fetus::convert_to_time_t(utcEnd);
	date highlightedstartTime = fetus::convert_to_time_t(utcStart);
	fetus* fetus = get();
	if(fetus)
	{
		m_highlightedExportTime = highlightedExportTime - fetus->get_start_date();
		m_highlightedExportInterval = (int)(((long)highlightedExportTime - (long)highlightedstartTime)/60);
		if(dlgOpen)
		{
			m_intervalExported = isExported;
			m_exportOpen = true;
			m_currentExportEndTime = m_highlightedExportTime;
			m_currentExportInterval = m_highlightedExportInterval;
			MoveToTime(highlightedExportTime);
		}
		Invalidate();
	}
}
void tracing::ResetHighlightedExportTime(DATE highlightedStart, DATE highlightedEnd, bool dlgClosed)
{
	COleDateTime oleStartT(highlightedStart);
	SYSTEMTIME sStartT, sEndT;
	oleStartT.GetAsSystemTime(sStartT);
	COleDateTime oleEndT(highlightedEnd);
	oleEndT.GetAsSystemTime(sEndT);

	SYSTEMTIME utcStart = fetus::convert_to_utc(sStartT);
	SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);
	long roundSecond = 0;
	if (utcEnd.wSecond == 59)
	{
		roundSecond = 1;
	}

	date highlightedExportTime = fetus::convert_to_time_t(utcEnd) + roundSecond;
	fetus* fetus = get();
	if(fetus && m_highlightedExportTime == highlightedExportTime - fetus->get_start_date() && (!m_exportOpen || dlgClosed))
	{
		m_highlightedExportTime = patterns::undetermined_date;
		m_highlightedExportInterval = 0;
	}
	if(m_exportOpen && dlgClosed)
	{
		m_intervalExported = false;
		m_exportOpen = false;
		m_currentExportEndTime = patterns::undetermined_date;
		m_currentExportInterval = 0;
	}
	

	Invalidate();

}

long tracing::GetChunkXFromSeconds(long s)
{
	if(get_type() == tnormal && ksubviews.size() > 2)
	{
		long long ms = s * 1000;
		return ksubviews[2]->get_x_from_ms(ms);
	}
	return 0;
}

void tracing::ResetVisibleChunks()
{
	m_visibleChunkEnds.clear();
}
void tracing::AddVisibleChunkEnd(long x)
{
	//m_visibleChunkEnds.push_back(x);
	m_visibleChunkEnds.insert(x);
}

void tracing::SetExportEnabled(bool enabled)
{	
	m_exportEnabled = enabled;	
}

CString tracing::GetExportNotEnabledMessage(DATE& endTime)
{	
	COleDateTime maxTime;
	maxTime.SetDateTime(9999,12,31,0,0,0);
	endTime = maxTime;

	CString msg = "";

	if(m_exportEnabled)
	{
		const fetus*  curFetus = get();
		if (has_twins())
		{
			msg = "Event detection and export are not enabled for multiple gestations";
		}
		else if (!is_singleton())
		{
			msg = "Event detection and export are not enabled for patients whose number of fetuses is 0 or undetermined";
		}
		else if (curFetus->has_cutoff_date())
		{			
			msg = "Event detection and export are not enabled before 36 weeks of gestation";
			SYSTEMTIME t = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(curFetus->get_cutoff_date()));
			endTime = COleDateTime(t);			
		}
		else
		{			
			msg = "Event detection and export are not enabled for patients whose gestational age is undetermined";
		}
	}
	return msg;

}

void tracing::GetExportDlgCalculatedEntriesEx(long iLeft, long iRight, double& meanContructions, int& meanBaseline, int& meanBaselineVariability, double& montevideo)
{
	meanContructions = -1.0;
	meanBaseline = -1.0;
	meanBaselineVariability = -1.0;
	montevideo = -1.0;

	meanContructions = CalcContractionsAverage(iLeft, iRight);
	int iMeanBaseLine, iVariability;
	get()->get_mean_baseline(iLeft, iRight, iMeanBaseLine, iVariability);
	meanBaseline = iMeanBaseLine;
	meanBaselineVariability = iVariability;
	montevideo = CalculateMontevideo(iLeft, iRight);
}


bool tracing::DoOnExportDlgCalculatedEntriesRequestEx(DATE intervalStart, DATE intervalEnd, double& meanContructions, int& meanBaseline, int& meanBaselineVariability, double& montevideo, bool& contractionThresholdExceeded)
{
	COleDateTime oleStartT(intervalStart);
	SYSTEMTIME sStartT, sEndT;
	oleStartT.GetAsSystemTime(sStartT);
	COleDateTime oleEndT(intervalEnd);
	oleEndT.GetAsSystemTime(sEndT);

	SYSTEMTIME utcStart = fetus::convert_to_utc(sStartT);
	SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);

	date highlightedExportTime = fetus::convert_to_time_t(utcEnd);
	date highlightedstartTime = fetus::convert_to_time_t(utcStart);
	fetus* fetus = get();
	if (fetus)
	{
		date exportEndTime = highlightedExportTime - fetus->get_start_date();
		CRect rect = get_bounds(otracing);


		long long msDestination = (long)(exportEndTime) * 1000;
		int exportInterval = (int)(((long)highlightedExportTime - (long)highlightedstartTime) / 60);
		if (exportInterval > 15)
		{
			long pageLength = GetSliderLength();
			long long pageLengthInMs = pageLength * 1000;
			long long iLeftInMs = max(0, (msDestination - pageLengthInMs));
			long iNewLeft = iLeftInMs <= 0 ? 0 : get_index_from_ms(iLeftInMs);

			long ileft = iNewLeft;
			long iright = get_index_from_ms(msDestination);

			GetExportDlgCalculatedEntriesEx(ileft, iright, meanContructions, meanBaseline, meanBaselineVariability, montevideo);
			contractionThresholdExceeded = IsContractionThresholdExceeded(ileft, iright);
			return true;

		}

	}
	return false;

}

bool tracing::GetContractionThresholdExceeded(DATE from, DATE to)
{
	
	SYSTEMTIME  sEndT;
	
	COleDateTime oleEndT(to);
	oleEndT.GetAsSystemTime(sEndT);

	
	SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);
	date highlightedExportTime = fetus::convert_to_time_t(utcEnd);
	
	fetus* fetus = get();
	if(fetus)
	{
		date exportEndTime = highlightedExportTime - fetus->get_start_date();
		CRect rect = get_bounds(otracing);
		long long msDestination = (long)(exportEndTime) * 1000;
		long pageLength = GetSliderLength(); 
		long long pageLengthInMs = pageLength * 1000;		
		long long iLeftInMs = max(0, (msDestination - pageLengthInMs));
		long iNewLeft = iLeftInMs <= 0 ? 0 : get_index_from_ms(iLeftInMs);

		long ileft = iNewLeft;		
		long iright = get_index_from_ms(msDestination);

		return IsContractionThresholdExceeded(ileft, iright);

	}
	return false;

}

void tracing::ResetExportData()
{
	m_visibleChunkEnds.clear();
}

bool tracing::IsToggleButtonAvailable() const
{
	return m_sliderLength > lexpanded;
}

void tracing::ToggleSwitchLeft(bool bNotify)
{
	if (has_parent())
	{
		get_parent()->ToggleSwitchLeft();
		return;
	}
	if (!m_bLeftPartShown)
	{
		long long sliderLeft = GetSliderLeftInMs();	
		if (get_index_from_ms(sliderLeft) >= 0)
		{
			move_page(true, true);
			m_bLeftPartShown = true;
		}
		else 
		{
			long total = (get()->get_number_of_fhr()) / get()->get_hr_sample_rate();
			long sliderLength = GetSliderLength();

			if (sliderLength <= total)
			{				
				move_to(0);
			}
		}
		if (bNotify)
		{
			GetParent()->PostMessage(TOGGLE_SWITCH_NOTIFICATION_MSG, 1, 0);
		}
	}
	
}
void tracing::ToggleSwitchRight(bool bNotify)
{
	if (has_parent())
	{
		get_parent()->ToggleSwitchLeft();
		return;
	}
	if (m_bLeftPartShown)
	{
		move_page(false, true);
		m_bLeftPartShown = false;
		if (bNotify)
		{
			GetParent()->PostMessage(TOGGLE_SWITCH_NOTIFICATION_MSG, 0, 0);
		}
		
	}
	
}


void tracing::DrawEdge(CDC* dc) const
{
	if (get_type() == tnormal)
	{
		CRect tracingRect = get_bounds(otracing);	

		for (long i = 0, n = (long)ksubviews.size(); i < n; i++)
		{
			if (!ksubviews[i]->is_compressed())
			{
				CRect rect;
				if (ksubviews[i]->get_type() == tup)
				{					
					CRect primaryBounds = ksubviews[i]->get_bounds(oprimary);
					ksubviews[i]->ClientToScreen(primaryBounds);
					ScreenToClient(primaryBounds);
					tracingRect.bottom = primaryBounds.bottom;
				}
				else if (ksubviews[i]->get_type() == tfhr)
				{

					tracingRect.left = rect.left;
					CRect secondaryBounds = ksubviews[i]->get_bounds(osecondary);
					ksubviews[i]->ClientToScreen(secondaryBounds);
					ScreenToClient(secondaryBounds);
					tracingRect.top = secondaryBounds.top;
				}
			}
		}

		bool drawExportIntervalEdge = (m_currentExportEndTime != undetermined_date);

		if (IsToggleButtonAvailable())
		{
			CRect leftShadowRect(tracingRect.left, tracingRect.top, tracingRect.left + 10, tracingRect.bottom);
			CRect rightShadowRect(tracingRect.right - 10, tracingRect.top, tracingRect.right, tracingRect.bottom);
			CDrawingManager dm(*dc);
			
			if (drawExportIntervalEdge && m_currentExportInterval <= 15)
			{
				DrawShadowRect(dc, leftShadowRect, true, true);
				DrawShadowRect(dc, rightShadowRect, true, false);
			}
			else
			{
				if (get_parent()->IsLeftPartShown())
				{
					
					DrawShadowRect(dc, leftShadowRect, drawExportIntervalEdge, true);
				}
				else
				{
					DrawShadowRect(dc, rightShadowRect, drawExportIntervalEdge, false);
				}
			}
		}
		else if(drawExportIntervalEdge)
		{			
			

			if (m_highlightedExportInterval > 15)
			{
				CRect leftShadowRect(tracingRect.left, tracingRect.top, tracingRect.left + 10, tracingRect.bottom);
				CRect rightShadowRect(tracingRect.right - 10, tracingRect.top, tracingRect.right, tracingRect.bottom);
				DrawShadowRect(dc, leftShadowRect, true, true);
				DrawShadowRect(dc, rightShadowRect, true, false);

			}
			else
			{
				date endTime = m_currentExportEndTime;
				date startTime =  endTime - 900;
				long leftP = get_x_from_ms(startTime * 1000);
				long rightP = get_x_from_ms(endTime * 1000);
				CRect leftShadowRect(leftP, tracingRect.top, leftP + 10, tracingRect.bottom);
				CRect rightShadowRect(rightP - 10, tracingRect.top, rightP, tracingRect.bottom);
				DrawShadowRect(dc, leftShadowRect, true, true);
				DrawShadowRect(dc, rightShadowRect, true, false);

			}
		}
		

	}
}

void tracing::DrawShadowRect(CDC* dc, const CRect& rect, bool blue, bool rightShadow) const
{
	//COLORREF color = blue ? RGB(32, 188, 255) : RGB(110, 110, 110);
	COLORREF color = blue ? RGB(0, 164, 234) : RGB(100, 100, 100);
	CDrawingManager dm(*dc);
	CRect shadowRect(rect);
	int w = shadowRect.Width();
	shadowRect.top -= w;
	if (rightShadow)
		shadowRect.right = shadowRect.left + 1;
	else
		shadowRect.left = shadowRect.right - 1;
	int count = 0;
	while (count < w)
	{
		count++;
		dm.DrawShadow(shadowRect, 1, 90, 90, 0, 0, color, rightShadow);
		if (rightShadow)
		{
			shadowRect.right++;
		}
		else
		{
			shadowRect.left--;
		}

	}
	
	


}

long tracing::GetSliderLeftIndex()
{
	long long leftInMS = GetSliderLeftInMs();
	long index = get_index_from_ms(leftInMS);
	return index;
}

void tracing::SwitchViews()
{
	
	if (!is_subview())
	{
		GetParent()->PostMessage(SWITCH_15MIN_30MIN_VIEWS_MSG, 0, 0);
	}
}

bool tracing::MoveToInterval(DATE intervalStart, int range)
{
	long left, right;
	GetCompressedViewBoundsInSeconds(left, right);
	COleDateTime oleStartT(intervalStart);
	SYSTEMTIME sStartT, sEndT;
	oleStartT.GetAsSystemTime(sStartT);
	COleDateTime oleEndT = oleStartT + COleDateTimeSpan(0, 0, range, 0);
	oleEndT.GetAsSystemTime(sEndT);

	SYSTEMTIME utcStart = fetus::convert_to_utc(sStartT);
	SYSTEMTIME utcEnd = fetus::convert_to_utc(sEndT);


	date startTime = fetus::convert_to_time_t(utcStart);
	date displayTime = fetus::convert_to_time_t(utcEnd);
	if (startTime < left || displayTime > right)
	{
		fetus* fetus = get();
		if (fetus)
		{
			MoveToTime(startTime, false, false);
			move_page(true);
			Invalidate();
			return true;
		}
	}
	return false;
}

CRect tracing::GetUpRect()
{
	CRect upRect(0,0,0,0);
	if (ksubviews.size() > 1)
	{
		long n = (long)ksubviews.size();
		for (int i = 0; i < n; i++)
		{
			tracing* p = ksubviews[i];
			if (p->get_type() == tup && !p->is_compressed() && IsWindow(p->m_hWnd))
			{
				p->GetWindowRect(upRect);
				break;
			}
		}
	}
	return upRect;
}

bool tracing::GetExpandedViewStartAndEnd(date& startTime, date& endTime)
{
	CRect rect = get_bounds(otracing);
	long iRight = get_index_from_x(rect.right);
	long long rightInMS = get_ms_from_index(iRight);
	long iCurrentLeft = get_index_from_x(rect.left);
	long long  leftInMS =  get_ms_from_index(iCurrentLeft);
	if (get()->has_start_date())
	{
		startTime = get()->get_start_date() + (long)(leftInMS * 0.001);
		endTime = get()->get_start_date() + (long)(rightInMS * 0.001);
	}
	return get()->has_start_date();
}

bool tracing::IsExportDlgOpenFor15MinInterval()
{
	if (m_exportOpen && m_highlightedExportInterval != 0 && m_highlightedExportTime != undetermined_date)
	{		
		return m_highlightedExportInterval <= 15;
	}
	return false;
}
void tracing::MoveToHighlited15MinExportInterval()
{
	if (m_exportOpen && m_highlightedExportInterval != 0 && m_highlightedExportTime != undetermined_date)
	{
		fetus* fetus = get();
		if (fetus)
		{
			date highlightedExportTime = m_highlightedExportTime + fetus->get_start_date();
			MoveToTime(highlightedExportTime);
			Invalidate();
		}
	}
}

void tracing::UpdateScaling()
{
	if (get_scaling_mode() == spaper)
	{
		priv_set_scaling((double)get_bounds(otracing).Height() / (double)(20 * get_paper_height()));
	}
}
///////////////////////////////////
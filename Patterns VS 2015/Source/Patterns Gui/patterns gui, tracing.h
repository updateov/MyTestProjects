#pragma once

#include "patterns, fetus.h"
#include "patterns, input adapter.h"
#include "patterns, subscription.h"
#include <vector>
#include <set>

using namespace std;
using namespace patterns;
const int TRACING_VIEW_SIZE_SEC = 1800;//1020; //1800;
const int COMPRESS_VIEW_SIZE_SEC = 14400;//7200; //14400;
namespace patterns_gui
{
#define SHOW_EXPORT_MSG (WM_USER + 20)
#define FORCE_UPDATE_ADAPTER_MSG (WM_USER + 21)
	/*
	=======================================================================================================================
	Control for displaying and exploring tracings. 
	=======================================================================================================================
	*/
	class tracing : public CWnd
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum mfc_notifications { ndeleteevent, ndeletecontraction, nacceptevent};
		enum paper { pinternational, pusa };
		enum scaling_mode { sfit, sfree, spaper };
		enum selectable { ccontraction, cevent, cnone };
		enum showable
		{
			wbaselines				= 0x01,
			wgrid					= 0x02,
			winformation			= 0x04,
			wcpictograms			= 0x08,
			wceventbars				= 0x10,
			wguides					= 0x40,
			wstruckout				= 0x200,
			wclock					= 0x400,
			wcr						= 0x800,
			wcrtracing				= 0x1000,
			wrepair					= 0x2000,
			wcontractionpeak		= 0x4000,
			wbaselinevariability	= 0x8000,
			wparerclassification	= 0x10000,
			wparerinfo				= 0x20000,
			wdecellag				= 0x40000,
			whideevents				= 0x80000,
			wmontevideo				= 0x100000,
			whidedisconnected		= 0x200000,
			wshowdisclaimer			= 0x400000,
		};
		enum type { tcr, tfhr, tnormal, tnormalnc, tup };

		static void create_bitmaps(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		DECLARE_MESSAGE_MAP()
		class subscription_to_fetus : public subscription
		{
		protected:
			fetus* m_pFetus;
			tracing* m_pTracing;
		public:
			subscription_to_fetus(tracing* curTracing, fetus* curFetus)
			{
				m_pFetus = curFetus;
				m_pTracing = curTracing;
			}

			/*
			===========================================================================================================
			===========================================================================================================
			*/
			virtual bool is(void* curTracing) const
			{
				return m_pTracing == curTracing;
			}

			virtual void note(subscription::message);
		};

		long m_animationLeft1;//al1;
		long m_animationLeft2;//al2;

		long m_animationRight1;//ar1;
		long m_animationRight2;//ar2;

		double m_animationRate;//arate;
		clock_t m_animationTime1;//at1;
		clock_t m_animationTime2;//at2
		enum constant { kanimationduration, kclicktolerance, kimagingtest };
		mutable long offset;
		mutable fetus* m_pFetus;
		mutable fetus* fpartial;
		long g1;
		long g2;
		mutable bool ianimating;
		bool icandelete;

#ifdef ACTIVE_CONTRACTION
		enum resize_action { resize_start, resize_move, resize_end, resize_peak, resize_add };
		bool resize_contraction(CPoint p0, resize_action action);

		bool contraction_move_start;
		bool contraction_move_end;
#endif

		mutable bool idataneedsupdate;
		mutable bool idebug;
		static bool idebugkeys;
		bool ifollowparent;
		bool iinontimer;
		mutable bool ineedsupdate;
		bool ipropagate;
		bool iscalinglocked;
		mutable bool iscrolling;
		long iselection;
		long ishow;
		bool isubview;
		bool izoomed;
		long lcompressed;
		long lexpanded;
		vector<tracing*> kchildren;
		paper kpaper;
		tracing *kparent;
		double kprecision;
		mutable vector<tracing *> ksubviews;
		scaling_mode m_scalingMode;
		enum object { obuttonaccept, obuttondelete, opictogram, oprimary, osecondary, oselection, oslider, osliderleft, osliderright, otracing, oview };
		enum { qdown, qdowninaccept, qdownindelete, qdownout, qidle, qresizingparent, qsliding, qslidingparent };
		long q;
		long qd;
		long qi;
		long qm;
		CPoint qp;
		mutable CRect rbdeletebutton;
		mutable CRect rbacceptbutton;
		mutable vector<CRect> rmarks;
		mutable double m_scaling;
		type m_tracingType;
		enum timer_selector { tanimation };
		selectable tselection;
		long zleft;
		long zright;
		
		
		mutable bool m_exportOpen;
		mutable int m_currentExportInterval;
		mutable date m_currentExportEndTime;
		mutable date m_highlightedExportTime;
		mutable int m_highlightedExportInterval;
		mutable bool m_intervalExported;
		mutable bool m_exportEnabled;
		


		virtual void adjust_subviews(void) const;
		virtual bool can_draw_bitmap_rectangle(const string &) const;
		static void create_rectangle_bitmaps(const string &, long);
		static void create_rectangle_bitmaps_h(const string &, long, bool = false);
		virtual void create_subviews(void) const;
		virtual void create_subviews_do_height(long, long) const;
		virtual void create_subviews_do_length(long, long) const;
		virtual long create_subviews_do_length_f(long, long) const;
		virtual void create_subviews_do_paper(long) const;
		virtual void create_subviews_do_space(long, long) const;
		virtual long create_subviews_height(long) const;
		virtual long create_subviews_space(long) const;
		virtual void destroy_subviews(void) const;
		virtual void draw(CDC *) const;
		virtual void draw_background(CDC *) const;
		virtual void draw_baselines(CDC *) const;
		virtual void draw_bitmap_rectangle(CDC *, const string &, long, CRect) const;
		virtual void draw_bitmap_rectangle(CDC *, const string &, long, long, long, long, long) const;
		virtual void draw_contractions(CDC *, const fetus *, bool) const;
		virtual void draw_contractions_average(CDC *) const;
		virtual void draw_montevideo_units(CDC *) const;
		virtual void draw_baseline_average(CDC *) const;


#ifdef patterns_research
		virtual void draw_debug(CDC *) const;
#endif

		virtual void draw_disconnected(CDC *) const;
		virtual void draw_disconnected_f(CDC *, long) const;
		virtual void draw_event_information(CDC *, long) const;
		virtual void draw_contraction_information(CDC *, long) const;
		virtual void draw_event_message(CDC *) const;
		virtual void draw_events(CDC *, const fetus *, bool) const;
		virtual void draw_grid(CDC *) const;
		virtual COLORREF draw_grid_base_colour(void) const;
		virtual long draw_grid_vertical_shade(void) const;
		virtual void draw_guides(CDC *) const;

#ifdef patterns_parer_classification
		virtual void draw_parer_classification(CDC *) const;
#endif
		virtual void DrawExportedAreaMarker(CDC* dc, long ksubviewIndex) const;
		virtual void DrawExportedRangeLines(CDC* dc) const;
		virtual bool IsWCRVisible() const;
		virtual void draw_slider(CDC *) const;
		virtual void draw_tracing(CDC *) const;
		virtual void draw_watermark(CDC *, const string &) const;
		virtual object find(CPoint) const;
		virtual long find_index(CPoint) const;
		virtual long find_subview(CPoint) const;
		virtual void follow(const tracing *);
		virtual long get(constant) const;
		virtual CRect get_bounds(object) const;
		virtual CRect get_bounds(void) const;
		virtual string get_compressed_fhr_title(void) const;
		virtual long get_displayed_length(void) const;
		virtual long get_index_from_ms(long long) const;
		virtual long get_index_from_x(long) const;
		virtual long long get_left_in_ms(void) const;
		virtual long get_maximum(void) const;
		virtual long get_minimum(void) const;
		virtual long long get_ms_from_index(long) const;
		virtual long get_number(void) const;
		virtual double get_paper_height(void) const;
		virtual const tracing *get_parent(void) const;
		virtual tracing *get_parent(void);
		virtual const tracing *get_parent_for_view(void) const;
		virtual tracing *get_parent_for_view(void);
		virtual const tracing *get_parent_root(void) const;
		virtual tracing *get_parent_root(void);
		virtual input_adapter::patient* get_patient(void) const;
		virtual long long get_right_in_ms(void) const;
		virtual long get_sample_rate(void) const;
		virtual CRect get_subview_bounds(long) const;
		virtual long long get_width_in_ms(void) const;
		virtual long get_x_from_index(long) const;
		virtual long get_x_from_ms(long long) const;
		virtual long get_y_from_units(CRect&, double) const;
		virtual bool has_parent(void) const;
		virtual bool has_parent_for_view(void) const;
		virtual bool has_patient(void) const;
		virtual bool has_subviews(void) const;
		virtual bool has_twins(void) const;
		virtual bool is_singleton(void) const;
		virtual bool is_compressed(void) const;
		virtual bool is_connected(void) const;
		virtual bool is_late_realtime(void) const;
		virtual bool is_subview(void) const;
		virtual bool is_visible(const event &) const;
		afx_msg UINT OnGetDlgCode(void);

#if !defined(patterns_viewer) && !defined(OEM_patterns)
		afx_msg void OnKeyDown(UINT, UINT, UINT);	// see comment above.
#endif

		afx_msg void OnLButtonDown(UINT, CPoint);
		afx_msg void OnLButtonUp(UINT, CPoint);
		afx_msg void OnMouseMove(UINT, CPoint);
		afx_msg void OnPaint(void);
		afx_msg void OnSize(UINT, int, int);
		afx_msg void OnMove(int, int);
		virtual afx_msg BOOL OnEraseBkgnd(CDC *);
		virtual void OnPaint_propagate(bool = true);
		afx_msg void OnTimer(UINT_PTR);
		virtual void reset_guides(void);
		virtual void select_low_level(selectable, long);
		virtual void set(fetus *) const;
		virtual void set_partial(fetus *) const;
		virtual void stop_zooming(void);
		virtual string string_from_number_of_samples(long) const;
		virtual void subscribe_child(tracing *);
		virtual void unsubscribe_child(tracing *);
		virtual void unsubscribe_children(void);
		virtual void update_data(bool = true);
		virtual void update_guides(CPoint);
		virtual void update_now(void) const;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/


	public:
		tracing(void);
		virtual ~tracing(void);

		virtual tracing* NewTracing() const
		{return new tracing;}
		virtual fetus* NewFetus() const
		{return new fetus;}

		virtual bool can_delete(void) const;
		virtual void deselect(void);
		virtual const fetus *get(void) const;
		virtual fetus *get(void);
		virtual double get_animation_rate(void) const;
		virtual long get_compressed_length(void) const;
		virtual long get_expanded_length(void) const;
		virtual long get_offset(void) const;
		virtual paper get_paper(void) const;
		virtual const fetus *get_partial(void) const;
		virtual fetus *get_partial(void);
		virtual double get_precision(void) const;

		virtual double get_scaling(void) const;
		virtual double priv_get_scaling(void) const;

		virtual scaling_mode get_scaling_mode(void) const;
		virtual long get_selection(void) const;
		virtual selectable get_selection_type(void) const;
		virtual type get_type(void) const;
		virtual bool has_compressed_length(void) const;
		virtual bool has_debug_keys(void) const;
		virtual bool has_expanded_length(void) const;
		virtual bool has_selection(void) const;
		virtual bool is_animating(void) const;
		virtual bool is_scaling_locked(void) const;
		virtual bool is_scrolling(void) const;
		virtual bool is_visible(showable) const;
		virtual bool is_zooming(void) const;
		virtual bool is_zoomed(void) const;
		virtual void lock_scaling(bool = true);
		virtual void move_page(bool = true);
		virtual void move_page_animating(bool = true);
		virtual void move_to(long, long = -1);
		virtual void move_to_animating(long, long = -1, long = -1, long = -1);
		virtual void MoveToTime(date endTime);
		virtual void GetCompressedViewBoundsInSeconds(long& left, long& right);
		virtual void reset_lengths(void);
		virtual void scroll(bool = true);
		virtual void select(selectable, long);
		virtual void set(fetus *);
		virtual void set_animation_rate(double);
		virtual void set_can_delete(bool = true);
		virtual void set_has_debug_keys(bool = true);
		virtual void set_lengths(long, long = -1);
		virtual void set_offset(long);
		virtual void set_paper(paper);
		virtual void set_partial(fetus *);
		virtual void set_precision(double);
		virtual void set_scaling(double);
		virtual void set_scaling_mode(scaling_mode);
		virtual void set_type(type);
		virtual void show(showable, bool = true);
		virtual void stop_animating(void);
		virtual void subscribe_to(tracing *, bool = false);

		virtual void update(bool = true);

		virtual void refresh_layout(void);
		/*
		===============================================================================================================
		===============================================================================================================
		*/
		virtual void toggle(showable s)
		{
			show(s, !is_visible(s));
		}

		virtual void unsubscribe(void);
		virtual void zoom(bool = true, bool notifyParent = false);

		/* Deprecated methods (see comment above). */
		virtual bool is_grid_visible(void) const;
		virtual bool is_scaling_to_fit(void) const;
		virtual void scale_to_fit(bool = true);
		virtual void show_grid(bool = true);

		static long get_cr_maximum() {return cr_maximum;}
		static void set_cr_maximum(int value) {cr_maximum = value;}

		static long get_cr_threshold() {return cr_threshold;}
		static void set_cr_threshold(int value) {cr_threshold = value;}

		static long get_cr_kcrstage() {return cr_kcrstage;}
		static void set_cr_kcrstage(int value) {cr_kcrstage = value;}

		static bool get_display_mhr() {return is_display_mhr;}
		static void set_display_mhr(bool value) {is_display_mhr = value;}


	protected:
		virtual void priv_set_offset(long) const;
		virtual void priv_set_scaling(double) const;
		void SetSubviewBounds(int i, int x, int y, int w, int h) const;

		mutable map<string, bool> can_draw_bitmap;

		static long cr_maximum;
		static long cr_threshold;
		static long cr_kcrstage;
		static bool is_display_mhr;

		void save_file();

		CRect m_bounds;

		std::set<long> m_visibleChunkEnds;
public:
	void SetHighlightedExportTime(DATE highlightedStart, DATE highlightedEnd, bool dlgOpen = false, bool isExported = false);
	void ResetHighlightedExportTime(DATE highlightedStart, DATE highlightedEnd, bool dlgClose = false);
	long GetChunkXFromSeconds(long s);
	void ResetVisibleChunks();	
	void AddVisibleChunkEnd(long x);
	void SetExportEnabled(bool bFlag);
	CString GetExportNotEnabledMessage(DATE& endTime);
	bool DoOnExportDlgCalculatedEntriesRequest(DATE intervalStart, DATE intervalEnd, CString& resultString);
	virtual void GetExportDlgCalculatedEntriesEx(long iLeft, long iRight, double& meanContructions, int& meanBaseline, int& meanBaselineVariability, double& montevideo);
	bool DoOnExportDlgCalculatedEntriesRequestEx(DATE intervalStart, DATE intervalEnd, double& meanContructions, int& meanBaseline, int& meanBaselineVariability, double& montevideo);
protected:
	virtual double CalcContractionsAverage(long iLeft, long iRight) const;
	double CalculateMontevideo(long iLeft, long iRight) const;
};
	
}
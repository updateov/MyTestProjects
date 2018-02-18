#pragma once
#include "Patterns gui, tracing.h"
#include "afxwin.h"
#include "patterns, input adapter.h"


//const int TRACING_VIEW_SIZE_SEC = 1800;//1020; //1800;
//const int COMPRESS_VIEW_SIZE_SEC = 14400;//7200; //14400;

/*
 =======================================================================================================================
    Patient view class. This class creates and communicate to the tracings container (patterns_gui::tracing).
 =======================================================================================================================
 */
class patient_view :
	public CWnd
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		class subscription_to_patient_view : public patterns::subscription
		{
		protected: patient_view *m_patient_view;
		public:
			subscription_to_patient_view(patient_view * x0)
			{
				m_patient_view = x0;
			}

			virtual void note(subscription::message);
		};

		bool m_baseline_variabiliy;

#ifdef patterns_parer_classification
		bool m_classification_activated;
#endif

		string m_patient_id;
		patterns_gui::tracing m_Tracing;

		virtual patterns::conductor & get_conductor (void);
		virtual long get_dword_reg_value(string, string) const;
		virtual patterns::input_adapter & get_input_adapter (void);
		virtual afx_msg int OnCreate(LPCREATESTRUCT);

		virtual afx_msg void on_strike_out_event(void);
		virtual afx_msg void on_strike_out_contraction(void);
		virtual afx_msg void on_accept_event(void);
		
		virtual afx_msg void OnSetFocus(CWnd *);
		virtual afx_msg void OnSize(UINT nType, int cx, int cy);
		virtual BOOL PreCreateWindow(CREATESTRUCT &);
		virtual void update_data(void);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		patient_view(void);
		virtual ~patient_view(void);

		virtual patterns_gui::tracing & get_tracing (void);
		virtual bool has_debug_keys(void);
		virtual void set_patient(const string &);
		virtual void toggle_baseline_display(void);
		virtual void toggle_debug_keys_access(void);

		virtual bool is_baseline_variability_activated(void);
		virtual void is_baseline_variability_activated(bool);

#ifdef patterns_parer_classification
		virtual bool is_classification_activated(void);
		virtual void is_classification_activated(bool);
#endif

		DECLARE_MESSAGE_MAP()
};

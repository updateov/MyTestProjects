// main view.h : interface of the main_view class
#pragma once
#include "patient view.h"
#include "patient list view.h"
#include "popup message.h"
#include "icon_button.h"

/*
 =======================================================================================================================
    main_view class. This view contains all controls on screen
 =======================================================================================================================
 */
class main_view :
	public CWnd
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		class subscription_to_main_view : public patterns::subscription
		{
		protected: main_view *m_main_view;
		public:
			subscription_to_main_view(main_view * x0)
			{
				m_main_view = x0;
			}

			virtual void note(subscription::message);
		};

		vector<string> m_banner;

		bool m_bInitialized;

		map<string, icon_button*> m_buttons;
		icon_button* get_button(string name);
		void set_button(string name, icon_button* button);

		string m_patient;
		
		long m_intent_to_use_timer_id;
		long m_autologoff_timer_id;
		long m_commit_events_id;

		popup_message m_intent_to_use_view;
		patient_list_view m_p_list_view;
		patient_view m_p_view;

		bool bplayback;
		bool bplayforward;

		clock_t m_t_zoomdown;

		virtual void draw_control_bar(CDC *, const char *, long, long);
		virtual void draw_patient_banner(CDC *, long, long);
		virtual string fit_text_to_width(string, CDC *, long, char = ' ');
		virtual conductor &get_conductor(void);
		virtual afx_msg void OnPaint(void);
		virtual afx_msg void OnSize(UINT, int, int);
		virtual afx_msg void OnTimer(UINT_PTR);
		virtual afx_msg void about(void);
		virtual afx_msg void open_file(void);
		virtual afx_msg void show_patient_list(void);
		virtual void update_data(void);
		virtual void update_config(void);

	//
	// -------------------------------------------------------------------------------------------------------------------
	//    Implementation
	// -------------------------------------------------------------------------------------------------------------------
	//
	public:
		main_view(void);
		virtual ~main_view(void);

		static HWND m_caller_window;

		// For patient list management
		virtual void open_patient_list(void);
		virtual void close_patient_list(void);
		virtual void toggle_patient_list(void);
		virtual patient_list_view &get_patient_list_view(void);

		virtual patient_view &get_patient_view(void);
		virtual void initialize(void);
		virtual afx_msg void go_beginning(void);
		virtual afx_msg void go_end(void);
		virtual afx_msg void go_next_page(void);
		virtual afx_msg void go_previous_page(void);
		virtual void move(long);
		virtual afx_msg void pause(void);
		virtual afx_msg void play_back(void);
		virtual afx_msg void play_forward(void);
		virtual void set_patient(string);
		virtual string get_patient() {return m_patient;}

		virtual void display_intent_to_use_message(bool bDisplay);

		virtual afx_msg void quit(void);
		virtual void toggle_zoom(void);
		virtual void reset_zoom(void);
		virtual void zoom_down(void);
		virtual void zoom_up(void);

		virtual afx_msg void toggle_events(void);
		virtual afx_msg void toggle_toco(void);
		virtual afx_msg void toggle_montevideo(void);
		virtual afx_msg void toggle_baseline(void);

		virtual void refresh_command_bar();
		virtual void update_montevideo_button();
		virtual void update_events_button();
		virtual void update_toco_button();
		virtual void update_baseline_button();

		bool m_is_patient_list_pinned;
	//
	// -------------------------------------------------------------------------------------------------------------------
	//    Generated message map functions
	// -------------------------------------------------------------------------------------------------------------------
	//
	protected:
		DECLARE_MESSAGE_MAP()
};

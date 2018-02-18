// Patterns Application.h : main header file for the Patterns Application
// application
#pragma once
#ifndef __AFXWIN_H__
#error include 'stdafx.h' before including this file for PCH
#endif
#include "resource.h"	// main symbols
#include "patterns, conductor.h"
#include "patient list view.h"
#include "patient view.h"

#include "SplashScreen.h"

#include "..\Pattern Detection\ThreadLock.h"

#include "please_wait_dlg.h"

#ifdef patterns_retrospective
#include "license_validation.h"
#endif


// patterns_application: See Patterns Application.cpp for the implementation of
// this class ;
// Main application class
class patterns_application :
	public CWinApp
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	private:
		bool m_bdebug_mode;

		patterns::ThreadLock m_thread_mutex;

		bool m_badapter_initialized;
		bool m_badapter_created;
		input_adapter *m_adapter;
		bool m_bpatientlist;
		string m_initialXMLFile;
		uintptr_t m_hthread;
		string m_product_name;
		bool m_enable_Montevideo;

		CSplashScreen *m_splash_dlg;
		please_wait_dlg* wait_dlg;

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		patterns::conductor m_conductor;

#ifdef patterns_retrospective
		license_validation* m_pLicense;
		bool check_license_info(void);

		bool check_disclaimer(void);
		void open_welcome_screen(void);
#endif

		virtual patient_list_view &get_patient_list_view(void);
		virtual patient_view &get_patient_view(void);
		virtual void OnHelp(void);
		virtual afx_msg void on_debug_mode(void);
		virtual afx_msg void on_debug_save(void);
		virtual afx_msg void on_go_beginning(void);
		virtual afx_msg void on_go_end(void);
		virtual afx_msg void on_go_left(void);
		virtual afx_msg void on_go_next_page(void);
		virtual afx_msg void on_go_previous_page(void);
		virtual afx_msg void on_go_right(void);
		virtual afx_msg void on_patient_list(void);
		virtual afx_msg void on_patient_list_return(void);
		virtual afx_msg void on_toggle_hyperstim_view(void);
		virtual afx_msg void on_toggle_display_events(void);
		virtual afx_msg void on_view_baseline(void);
		virtual afx_msg void on_view_montevideo(void);

		void wait_for_completion(CString message, int seconds);
		void show_wait_dialog(CString msg);
		void close_wait_dialog();

	public:
		virtual afx_msg void on_debug_open(void);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		patterns_application(void);
		virtual ~patterns_application(void);

#if defined(patterns_standalone)
		bool load_file(string );
#endif

#ifdef patterns_retrospective
		license_validation* get_license() { return m_pLicense; }
#endif

		/* Adapter creation methods (adapter created in a seperate thread. */
		virtual void create_adapter(void);
		virtual void set_initial_state(void);
		virtual void start_adapter_creation(void);

		virtual void close_patient_list(void);
		static patterns_application &get_application(void);

#ifdef patterns_retrospective
		static unsigned int __stdcall LoadFileThread(LPVOID data);
		static unsigned int __stdcall ApplyUpdateThread(LPVOID data);
#endif

		virtual patterns::conductor & get_conductor (void)
		{
			return m_conductor;
		}

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual string get_product_name(void)
		{
			return m_product_name;
		}

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual bool is_enable_Montevideo(void)
		{
			return m_enable_Montevideo;
		}

		virtual BOOL InitInstance(void);
		virtual int Run();

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual bool is_adapter_created(void)
		{
			return m_badapter_created;
		}

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual bool is_adapter_initialized(void)
		{
			return m_badapter_initialized;
		}

		virtual bool is_patient_list_shown(void);
		virtual bool is_patient_list_shown(string&);
		virtual BOOL OnIdle(LONG);
		virtual BOOL PreTranslateMessage(MSG *);

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual void set_adapter_as_created(bool b = true)
		{
			m_badapter_created = b;
		}

		virtual void set_adapter_as_initialized(bool b = true);

		// Check if a module is visible
		virtual bool is_visible(patterns_gui::tracing::showable k)
		{
			return get_patient_view().get_tracing().is_visible(k);
		}

		// Toggle on / off the display of a module
		virtual void toggle(patterns_gui::tracing::showable k) 
		{
			get_patient_view().get_tracing().toggle(k);
		}

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual void set_product_name(string n)
		{
			m_product_name = n;
		}

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual void is_enable_Montevideo(bool b)
		{
			m_enable_Montevideo = b;
		}

		virtual string get_patient();
		virtual void set_patient(const string &);
		virtual void show_patient_list(bool = true);
		virtual void update_title_bar(void);

		DECLARE_MESSAGE_MAP()
};

extern patterns_application theApp;

// main frame.h : interface of the main_frame class
#pragma once
#include "main view.h"

class patient_list_view;
class patient_view;

class main_frame :
	public CFrameWnd
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		main_frame(void);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		main_view m_wndView;

		DECLARE_DYNAMIC(main_frame)
	//
	// -------------------------------------------------------------------------------------------------------------------
	//    Overrides
	// -------------------------------------------------------------------------------------------------------------------
	//
	public:
		virtual BOOL OnCreateClient(LPCREATESTRUCT, CCreateContext *);
		virtual void OnGetMinMaxInfo(MINMAXINFO FAR *);
		virtual void OnSize(UINT, int, int);
		virtual BOOL PreCreateWindow(CREATESTRUCT &);
		virtual afx_msg void OnClose(void);

	//
	// -------------------------------------------------------------------------------------------------------------------
	//    Implementation
	// -------------------------------------------------------------------------------------------------------------------
	//
	public:
		virtual ~main_frame(void);
		virtual main_view &get_main_view(void);

		DECLARE_MESSAGE_MAP()
		LRESULT OnPowerBroadcast(WPARAM wParam, LPARAM lParam);
		LRESULT OnEndSession(WPARAM wParam, LPARAM lParam);
		LRESULT OnQueryEndSession(WPARAM wParam, LPARAM lParam);

	protected:
		virtual LRESULT WindowProc(UINT message, WPARAM wParam, LPARAM lParam);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		void RequestShutdown(void);
		void allow_application_to_close(bool b);
		void StartPumping();
		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		bool is_allow_application_to_close(void)
		{
			return !m_bCloseDisabled;
		}

		bool m_bCloseDisabled;
};

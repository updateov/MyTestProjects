#pragma once
#include "patterns, input adapter.h"
#include "patterns gui, patient list control.h"

//
// =======================================================================================================================
//    patient_list_view form view
// =======================================================================================================================
//
class patient_list_view :
	public CWnd
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		patterns_gui::patient_list_control m_patient_list_ctrl;
		string m_patient;

		virtual patterns::conductor & get_conductor (void);
		virtual patterns::input_adapter & get_input_adapter (void);
		virtual void on_item_changed(NMHDR *, LRESULT *);
		virtual void OnDataChange(void);
		virtual afx_msg int OnCreate(LPCREATESTRUCT);
		virtual afx_msg void OnPaint(void);
		virtual afx_msg void OnSize(UINT, int, int);
		virtual afx_msg void OnSetFocus(CWnd *);
		virtual BOOL PreCreateWindow(CREATESTRUCT &);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		patient_list_view(void);
		virtual ~patient_list_view(void);

		virtual void OnDraw(CDC *pDC);
		virtual afx_msg void initialize(void);
		virtual void set_patient(const string &);

		DECLARE_MESSAGE_MAP()
};

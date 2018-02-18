#pragma once

#include "patterns, subscription.h"
#include "patterns, input adapter.h"

#include <string>
#include <afxcmn.h>

using namespace std;
using namespace patterns;

namespace patterns_gui
{
	class patient_list_control : public CListCtrl
	{
	protected:
		class subscription_to_patient_list : public patterns::subscription
		{
		protected: patient_list_control *m_patient_list_ctrl;
		public:
			subscription_to_patient_list(patient_list_control * x0)
			{
				m_patient_list_ctrl = x0;
			}

			//
			// ===========================================================================================================
			//    bool subscription:::is (void *) const
			// ===========================================================================================================
			//
			virtual bool is(void *x0) const
			{
				return m_patient_list_ctrl == x0;
			}

			virtual void note(subscription::message);

			friend class patient_list_control;
		};

		conductor *c;
		bool m_block;

		virtual void append(void);
		virtual void build(void);
		virtual void draw_item(long itemID, CDC *dc);
		virtual void draw_items(CDC *dc);
		virtual RECT get_bounds(void);
		virtual RECT get_bounds(long);
		virtual RECT get_text_bounds(long);
		virtual void lock(bool = true);
		virtual afx_msg void MeasureItem(LPMEASUREITEMSTRUCT lpMeasureItemStruct);
		virtual afx_msg int OnCreate(LPCREATESTRUCT);
		virtual BOOL OnEraseBkgnd(CDC *);
		virtual void OnPaint(void);
		virtual void OnSize(UINT, int, int);
		virtual void redraw(void);
		virtual void remove(long);
		virtual void remove_all(void);
		virtual void set(patterns::input_adapter::patient*, long);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		patient_list_control(void);
		virtual ~patient_list_control(void);

		virtual void deselect(void);
		virtual const conductor &get_conductor(void) const;
		virtual conductor &get_conductor(void);
		virtual const string get_selection(void) const;
		virtual long get_selection_index(void) const;
		virtual bool has_selection(void) const;
		virtual void initialize(void);
		virtual bool is_locked(void);
		virtual void select(const string &, bool = false);
		virtual void set_conductor(conductor *);

		DECLARE_MESSAGE_MAP()
	};
}

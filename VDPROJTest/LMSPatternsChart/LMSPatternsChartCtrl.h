#pragma once

#include "Patterns gui, tracing.h"
#include "patterns gui, viewer input adapter.h"
#include "patterns, conductor.h"
#include "patterns, fetus.h"
#include "icon_button.h"
#include "popup message.h"
#include "tinyxml.h"
#include "..\..\..\Source\Pattern Detection\ThreadLock.h"
#include "PatternsChartCtrl.h"
#include <objsafe.h>

class CLMSPatternsChartCtrl : public PatternsChartCtrl
{
	DECLARE_DYNCREATE(CLMSPatternsChartCtrl)

	//interface IObjectSafety 
public:
	BEGIN_INTERFACE_PART(ObjectSafety, IObjectSafety) 
		STDMETHOD(GetInterfaceSafetyOptions)(REFIID riid, DWORD __RPC_FAR *pdwSupportedOptions, DWORD __RPC_FAR *pdwEnabledOptions); 
		STDMETHOD(SetInterfaceSafetyOptions)(REFIID riid, DWORD dwOptionSetMask, DWORD dwEnabledOptions); 
	END_INTERFACE_PART(ObjectSafety) 
		DECLARE_INTERFACE_MAP() 
	// Constructor
public:
	CLMSPatternsChartCtrl();



	// Overrides
public:
	virtual int OnCreate(LPCREATESTRUCT);
	virtual void OnDestroy();
	virtual void DoPropExchange(CPropExchange* pPX);
	afx_msg void OnTimer(UINT_PTR);

	// Implementation
protected:
	virtual ~CLMSPatternsChartCtrl();

	DECLARE_OLECREATE_EX(CLMSPatternsChartCtrl)    // Class factory and guid
	DECLARE_OLETYPELIB(CLMSPatternsChartCtrl)      // GetTypeInfo
	DECLARE_PROPPAGEIDS(CLMSPatternsChartCtrl)     // Property page IDs
	DECLARE_OLECTLTYPE(CLMSPatternsChartCtrl)		// Type name and misc status

	// Message maps
	DECLARE_MESSAGE_MAP()

	// Dispatch maps
	DECLARE_DISPATCH_MAP()

	// Event maps
	DECLARE_EVENT_MAP()

	// Dispatch and event IDs
public:



protected:


	virtual void OnConnectionDataChanged(void);
	int m_cr_limit;
	int m_cr_window;
	int m_cr_stage1;
	///////////////////////////////////////////////////////////////////////////////////////
	/// Data functions

	virtual void process_patient_data(TiXmlElement* rootNode);

	///////////////////////////////////////////////////////////////////////////////////////
	/// COMMAND BAR

	virtual afx_msg void display_data_status(void);
	virtual afx_msg void on_strike_out_event(void);
	virtual afx_msg void on_strike_out_contraction(void);
	virtual afx_msg void on_accept_event(void);

	void CLMSPatternsChartCtrl::on_show_about(void);

protected:
	//load resource strings
	virtual CString LoadIconTextZoom();
	virtual CString LoadIconTextBaselines();
	virtual CString LoadIconTextToco();
	virtual CString LoadIconTextEvents();
	virtual CString LoadIconTextMontevideo();
	virtual CString LoadIconTextAbout();
	virtual CString LoadTextAvailableSpaceTooSmall();
	virtual CString LoadTextAnErrorHappened();
};
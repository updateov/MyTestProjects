#pragma once

#include "Patterns gui, cri tracing.h"
#include "patterns gui, cri viewer input adapter.h"
#include "patterns, criconductor.h"
#include "patterns, crifetus.h"
#include "PatternsChartCtrl.h"
#include <objsafe.h>

class CRIPatternsChartCtrl : public PatternsChartCtrl
{
	DECLARE_DYNCREATE(CRIPatternsChartCtrl)

	//interface IObjectSafety 
public:
	BEGIN_INTERFACE_PART(ObjectSafety, IObjectSafety) 
		STDMETHOD(GetInterfaceSafetyOptions)(REFIID riid, DWORD __RPC_FAR *pdwSupportedOptions, DWORD __RPC_FAR *pdwEnabledOptions); 
		STDMETHOD(SetInterfaceSafetyOptions)(REFIID riid, DWORD dwOptionSetMask, DWORD dwEnabledOptions); 
	END_INTERFACE_PART(ObjectSafety) 
		DECLARE_INTERFACE_MAP() 
	// Constructor
public:
	CRIPatternsChartCtrl();



	// Overrides
public:
	virtual int OnCreate(LPCREATESTRUCT);
	virtual void OnDestroy();
	virtual void DoPropExchange(CPropExchange* pPX);
	afx_msg void OnTimer(UINT_PTR);

	// Implementation
protected:
	virtual ~CRIPatternsChartCtrl();

	DECLARE_OLECREATE_EX(CRIPatternsChartCtrl)    // Class factory and guid
	DECLARE_OLETYPELIB(CRIPatternsChartCtrl)      // GetTypeInfo
	DECLARE_PROPPAGEIDS(CRIPatternsChartCtrl)     // Property page IDs
	DECLARE_OLECTLTYPE(CRIPatternsChartCtrl)		// Type name and misc status

	// Message maps
	DECLARE_MESSAGE_MAP()

	// Dispatch maps
	DECLARE_DISPATCH_MAP()

	// Event maps
	DECLARE_EVENT_MAP()

	// Dispatch and event IDs


protected:


	virtual void OnConnectionDataChanged(void);

	///////////////////////////////////////////////////////////////////////////////////////
	/// Data functions
	patterns::CRIFetus* GetFetus();// {return &m_conductor->GetFetus("KEY");}
	virtual void process_patient_data(TiXmlElement* rootNode);


	///////////////////////////////////////////////////////////////////////////////////////
	/// COMMAND BAR

	virtual afx_msg void display_data_status(void);

	virtual afx_msg void on_strike_out_event(void);
	virtual afx_msg void on_strike_out_contraction(void);
	virtual afx_msg void on_accept_event(void);

	long  OnStrikeoutEventCallback(WPARAM wParam, LPARAM lParam);
	long OnAcceptEventCallback(WPARAM wParam, LPARAM lParam);
	long OnStrikeoutContractionCallback(WPARAM wParam, LPARAM lParam);
	
	void DoStrikeOutEvent();
	void DoStrikeOutContraction();
	void DoAcceptEvent();

	virtual void on_show_about(void);

protected:
	void LoadCRIAlgorithmSettings(TiXmlElement* patientNode);
protected:
	enum WaitForAuthenticationStatus
	{
		WaitForNone,
		WaitForEventStrikeOut,
		WaitForEventAccept,
		WaitForContractionStrikeOut
	} m_waitAuthenticationStatus;

	long m_waitTimer;
	int m_maxContractionRateValue;
	int m_contractionRateWindowSize;
	int m_contractionRateTrigger;
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
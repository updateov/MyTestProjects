#pragma once
#include "ContainerBaseWnd.h"
#include "Patterns gui, cri tracing.h"
#include "patterns gui, cri viewer input adapter.h"
#include "patterns, criconductor.h"
#include "patterns, crifetus.h"

#include <objsafe.h>

using namespace patterns;
// CheckListContainerWnd


class CheckListContainerWnd : public ContainerBaseWnd
{
	DECLARE_DYNAMIC(CheckListContainerWnd)

public:
	CheckListContainerWnd();
	virtual ~CheckListContainerWnd();

public:
	virtual void SetInitialData(const CString& url, const CString& patientID);
protected:
	DECLARE_MESSAGE_MAP()
	virtual int OnCreate(LPCREATESTRUCT);
	virtual void OnDestroy();	
	afx_msg void OnTimer(UINT_PTR);
protected:	

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


};



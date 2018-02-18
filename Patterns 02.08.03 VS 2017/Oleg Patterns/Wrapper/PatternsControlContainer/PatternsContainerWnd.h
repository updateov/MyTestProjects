#pragma once
#include "ContainerBaseWnd.h"
#include "patterns gui, viewer input adapter.h"

// PatternsContainerWnd
using namespace patterns;

class PatternsContainerWnd : public ContainerBaseWnd
{
	DECLARE_DYNAMIC(PatternsContainerWnd)

public:
	PatternsContainerWnd();
	virtual ~PatternsContainerWnd();
public:

	void SetInitialData(const CString& url, const CString& patientID,  const CString& userID, const CString& userName, const CString& permissions, bool exportSupport, const CString& version);
protected:
	DECLARE_MESSAGE_MAP()
	virtual int OnCreate(LPCREATESTRUCT);
	virtual void OnDestroy();	
	afx_msg void OnTimer(UINT_PTR);
protected:
	///////////////////////////////////////////////////////////////////////////////////////
	/// Data functions

	virtual void process_patient_data(TiXmlElement* rootNode);

	///////////////////////////////////////////////////////////////////////////////////////
	/// COMMAND BAR

	virtual afx_msg void display_data_status(void);
	virtual afx_msg void on_strike_out_event(void);
	virtual afx_msg void on_strike_out_contraction(void);
	virtual afx_msg void on_accept_event(void);

	

protected:
	virtual void StartInitializationThread();
	virtual void SetDataToWorkerThread();
protected:
	int m_cr_limit;
	int m_cr_window;
	int m_cr_stage1;

};



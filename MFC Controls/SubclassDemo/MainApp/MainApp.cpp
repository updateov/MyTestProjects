
// MainApp.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "MainApp.h"
#include "MainAppDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CMainAppApp

BEGIN_MESSAGE_MAP(CMainAppApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()


// CMainAppApp construction

CMainAppApp::CMainAppApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}


// The one and only CMainAppApp object

CMainAppApp theApp;


// CMainAppApp initialization

BOOL CMainAppApp::InitInstance()
{
	AfxEnableControlContainer();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	//  of your final executable, you should remove from the following
	//  the specific initialization routines you do not need.


	CMainAppDlg dlg;
	m_pMainWnd = &dlg;
	INT_PTR nResponse = dlg.DoModal();
	if (nResponse == IDOK)
	{
		// TODO: Place code here to handle when the dialog is
		//  dismissed with OK
	}
	else if (nResponse == IDCANCEL)
	{
		// TODO: Place code here to handle when the dialog is
		//  dismissed with Cancel
	}

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}


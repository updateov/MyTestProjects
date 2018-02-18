#include "stdafx.h"
#include "developpement.h"

#include "dialogue developpement.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
BEGIN_MESSAGE_MAP(application_developpement, CWinApp)
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/*
 =======================================================================================================================
 =======================================================================================================================
 */
application_developpement::application_developpement(void)
{
}

application_developpement theApp;

/*
 =======================================================================================================================
 =======================================================================================================================
 */

BOOL application_developpement::InitInstance(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~*/
	dialogue_developpement d;
	/*~~~~~~~~~~~~~~~~~~~~~*/

	InitCommonControls();
	CWinApp::InitInstance();
	SetRegistryKey(_T("lms, développement patterns"));
	m_pMainWnd = &d;
	d.DoModal();
	return FALSE;
}


// MainApp.h : main header file for the PROJECT_NAME application
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CMainAppApp:
// See MainApp.cpp for the implementation of this class
//

class CMainAppApp : public CWinApp
{
public:
	CMainAppApp();

// Overrides
public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};


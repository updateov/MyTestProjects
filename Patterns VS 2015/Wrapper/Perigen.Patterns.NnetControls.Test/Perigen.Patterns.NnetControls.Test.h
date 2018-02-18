
// Perigen.Patterns.NnetControls.Test.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CPerigenPatternsNnetControlsTestApp:
// See Perigen.Patterns.NnetControls.Test.cpp for the implementation of this class
//

class CPerigenPatternsNnetControlsTestApp : public CWinApp
{
public:
	CPerigenPatternsNnetControlsTestApp();

// Overrides
public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CPerigenPatternsNnetControlsTestApp theApp;
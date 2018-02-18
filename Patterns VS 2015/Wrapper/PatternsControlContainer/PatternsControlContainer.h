// PatternsControlContainer.h : main header file for the PatternsControlContainer DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CPatternsControlContainerApp
// See PatternsControlContainer.cpp for the implementation of this class
//

class CPatternsControlContainerApp : public CWinApp
{
public:
	CPatternsControlContainerApp();

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};

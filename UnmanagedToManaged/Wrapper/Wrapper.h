// Wrapper.h : main header file for the Wrapper DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CWrapperApp
// See Wrapper.cpp for the implementation of this class
//

class CWrapperApp : public CWinApp
{
public:
	CWrapperApp();

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};

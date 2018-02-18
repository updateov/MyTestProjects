#pragma once

// CRIPatternsChart.h : main header file for CRIPatternsChart.DLL

#if !defined( __AFXCTL_H__ )
#error "include 'afxctl.h' before including this file"
#endif

#include "resource.h"       // main symbols


// CRIPatternsChartApp : See CRIPatternsChart.cpp for implementation.

class CRIPatternsChartApp : public COleControlModule
{
public:
	BOOL InitInstance();
	int ExitInstance();

	static int get_timeout_dialog() 
	{
		if (s_TimeoutDialog > 0)
			return s_TimeoutDialog;
		return 24*60; // Disabled is one day timeout
	}
	static void set_timeout_dialog(int value) {s_TimeoutDialog = value;}

protected:
	static int s_TimeoutDialog;
};

extern const GUID CDECL _tlid;
extern const WORD _wVerMajor;
extern const WORD _wVerMinor;

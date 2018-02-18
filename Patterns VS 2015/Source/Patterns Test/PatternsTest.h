// Patterns.h : main header file for the PROJECT_NAME application
#pragma once
#ifndef __AFXWIN_H__
#error include 'stdafx.h' before including this file for PCH
#endif
#include "resource.h"	// main symbols

//
// =======================================================================================================================
//    CPatternsApp: See Patterns.cpp for the implementation of this class
// =======================================================================================================================
//
class CPatternsApp :
	public CWinApp
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		CPatternsApp(void);

	//
	// -------------------------------------------------------------------------------------------------------------------
	//    Overrides
	// -------------------------------------------------------------------------------------------------------------------
	//
	public:
		virtual BOOL InitInstance(void);

		// Implementation
		DECLARE_MESSAGE_MAP()
};

extern CPatternsApp theApp;

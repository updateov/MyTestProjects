
// CRI_ToolBar.h : main header file for the PROJECT_NAME application
//

#if !defined(AFX_TASKB3_H__039F0FBE_F360_4C26_8DA2_2409D3D41E34__INCLUDED_)
#define AFX_TASKB3_H__039F0FBE_F360_4C26_8DA2_2409D3D41E34__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols


// CCRI_ToolBarApp:
// See CRI_ToolBar.cpp for the implementation of this class
//

class CCRI_ToolBarApp : public CWinApp
{
public:
	CCRI_ToolBarApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CCRI_ToolBarApp theApp;

#endif // !defined(AFX_TASKB3_H__039F0FBE_F360_4C26_8DA2_2409D3D41E34__INCLUDED_)
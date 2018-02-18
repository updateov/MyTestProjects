// stdafx.h : include file for standard system include files, or project specific
// include files that are used frequently, but are changed infrequently
#pragma once
#include <iostream>
#include <tchar.h>
#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

#define _CRT_SECURE_NO_WARNINGS
#define _CRT_NON_CONFORMING_SWPRINTFS
#define _CRT_NONSTDC_NO_WARNINGS

#if (_MSC_VER > 1310) // VS2005
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN						// Exclude rarely-used stuff from Windows headers
#endif
#include <afx.h>
#include <afxwin.h>		// MFC core and standard components
#include <afxext.h>		// MFC extensions
#include <afxdtctl.h>	// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>		// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

// TODO: reference additional headers your program requires here

// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently

#pragma once

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN            // Exclude rarely-used stuff from Windows headers
#endif

//////////////////////////////////
#define _CRT_SECURE_NO_WARNINGS
#define _CRT_NON_CONFORMING_SWPRINTFS
#define _CRT_NONSTDC_NO_WARNINGS

#if (_MSC_VER > 1310) // VS2005
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif

#define _BIND_TO_CURRENT_CRT_VERSION 1
#define _BIND_TO_CURRENT_ATL_VERSION 1
#define _BIND_TO_CURRENT_MFC_VERSION 1
#define _BIND_TO_CURRENT_OPENMP_VERSION 1


#ifndef NTDDI_VERSION
#define NTDDI_VERSION 0x05020000				// Windows XP with SP2
#endif

#ifndef WINVER
#define WINVER 0x0502							// Windows Server 2003 with SP1, Windows XP with SP2
#endif

#ifndef _WIN32_WINNT
#define _WIN32_WINNT WINVER						// Windows Server 2003 with SP1, Windows XP with SP2
#endif

#ifndef _WIN32_WINDOWS							
#define _WIN32_WINDOWS 0x0410					// Change this to the appropriate value to target Windows Me or later.
#endif

#ifndef _WIN32_IE								
#define _WIN32_IE 0x0600						// Internet Explorer 6.0
#endif

#ifndef _CRT_SECURE_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS
#endif

#ifndef _ATL_CSTRING_EXPLICIT_CONSTRUCTORS
#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS
#endif

#ifndef _AFX_ALL_WARNINGS
#define _AFX_ALL_WARNINGS
#endif

#ifndef VC_EXTRALEAN 
#define VC_EXTRALEAN
#endif

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

//////////////////////////////////

#include "targetver.h"

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // some CString constructors will be explicit

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions

//#ifndef _AFX_NO_OLE_SUPPORT
//#include <afxole.h>         // MFC OLE classes
//#include <afxodlgs.h>       // MFC OLE dialog classes
//#include <afxdisp.h>        // MFC Automation classes
//#endif // _AFX_NO_OLE_SUPPORT

#ifndef _AFX_NO_DB_SUPPORT
#include <afxdb.h>                      // MFC ODBC database classes
#endif // _AFX_NO_DB_SUPPORT

#ifndef _AFX_NO_DAO_SUPPORT
#include <afxdao.h>                     // MFC DAO database classes
#endif // _AFX_NO_DAO_SUPPORT

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdtctl.h>           // MFC support for Internet Explorer 4 Common Controls
#endif
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>                     // MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT


#include <afxmt.h>

#include <assert.h>
#include <math.h>
#
#include <stdlib.h>
#include <windows.h>


#include <time.h>

#include "patterns, contraction.h"
#include "CRICalculation.h"
#include "Contractility.h"
#include "..\..\Source\Pattern Data\patterns, event.h"
#include "..\..\Source\Pattern Data\patterns, compression.h"

#include <string>
#include <iostream>
#include <fstream>
#include <sstream>
#include <vector>
#include <map> 
#include <set>
#include <iterator>
#include <algorithm> 

using namespace patterns;
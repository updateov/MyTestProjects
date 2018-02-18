// stdafx.h : include file for standard system include files, or project specific
// include files that are used frequently, but are changed infrequently
#pragma once

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN						// Exclude rarely-used stuff from Windows headers
#endif

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

#ifdef OEM_patterns
#include <afxctl.h>         // MFC support for ActiveX Controls
#endif

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxmt.h>

#include <assert.h>
#include <math.h>
#include <vector> 
#include <map> 
#include <set>
#include <iterator>
#include <algorithm> 
#include <stdlib.h>
#include <windows.h>
#include <assert.h>
#include <string>
#include <time.h>
#include <iostream>
#include <fstream>
#include <sstream>

#ifndef patterns_standalone
#include "LMSSiteAll.h"
#endif

#include "..\Pattern Data\patterns, contraction.h"
#include "CRICalculation.h"
#include "..\Pattern Data\patterns, event.h"
#include "..\Pattern Data\patterns, compression.h"

#ifdef patterns_has_signal_processing
#include "..\Pattern Detection\patterns, contraction detection.h"
#include "..\Pattern Detection\DigitalSignal.h"
#endif

#include "PatternsVersionNumber.h"
#include "base64.h"

// warning C6031: return value ignored: <function> could return unexpected value
#pragma warning(disable : 6031)

// warning C6211: Leaking memory <pointer> due to an exception. Consider using a local catch block to clean up memory
#pragma warning(disable : 6211)

// warning C6011: Dereferencing NULL pointer 
#pragma warning(disable : 6011)

// warning C6387: 'argument x' might be '0': this does not adhere to the specification for the function xxx
#pragma warning(disable : 6387)

//#ifdef _DEBUG
//	#import "..\..\..\Debug\Perigen.Patterns.NnetControls.tlb" raw_interfaces_only //, named_guids, no_namespace
//#else
//	#import "..\..\..\Release\Perigen.Patterns.NnetControls.tlb" raw_interfaces_only //, named_guids, no_namespace
//#endif
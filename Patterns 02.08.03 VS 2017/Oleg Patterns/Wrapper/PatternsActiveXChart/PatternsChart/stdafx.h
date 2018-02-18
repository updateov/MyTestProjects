// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // some CString constructors will be explicit

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN            // Exclude rarely-used stuff from Windows headers
#endif

#define _CRT_SECURE_NO_WARNINGS
#define _CRT_NON_CONFORMING_SWPRINTFS
#define _CRT_NONSTDC_NO_WARNINGS

#define _BIND_TO_CURRENT_CRT_VERSION 1
#define _BIND_TO_CURRENT_ATL_VERSION 1
#define _BIND_TO_CURRENT_MFC_VERSION 1
#define _BIND_TO_CURRENT_OPENMP_VERSION 1

//#include <afx.h>
//#include <afxwin.h>         // MFC core and standard components
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


// TODO: reference additional headers your program requires here

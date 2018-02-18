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

#ifndef WINVER				// Allow use of features specific to Windows XP or later.
#define WINVER 0x0501		// Change this to the appropriate value to target other versions of Windows.
#endif

#ifndef _WIN32_WINNT		// Allow use of features specific to Windows XP or later.                   
#define _WIN32_WINNT 0x0501	// Change this to the appropriate value to target other versions of Windows.
#endif						

#ifndef _WIN32_WINDOWS		// Allow use of features specific to Windows 98 or later.
#define _WIN32_WINDOWS 0x0410 // Change this to the appropriate value to target Windows Me or later.
#endif

#ifndef _WIN32_IE			// Allow use of features specific to IE 6.0 or later.
#define _WIN32_IE 0x0600	// Change this to the appropriate value to target other versions of IE.
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

#include <iostream>
#include <tchar.h>

// TODO: reference additional headers your program requires here
#ifdef MATLAB_OUTPUT
#include "..\Pattern Detection\matlabIO.h"
#endif

#include "..\Pattern Data\patterns, contraction.h"
#include "..\Pattern Data\patterns, event.h"
#include "..\Pattern Data\patterns, compression.h"


#include "..\Pattern Detection\patterns, contraction detection.h"
#include "..\Pattern Detection\DigitalSignal.h"
#include "..\Patterns\patterns, fetus.h"

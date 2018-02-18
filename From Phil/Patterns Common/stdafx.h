// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

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


#include "patterns, compression.h"
#include "patterns, contraction.h"
#include "patterns, event.h"
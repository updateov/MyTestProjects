
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
#define _WIN32_WINDOWS 0x0502					// Change this to the appropriate value to target Windows Me or later.
#endif

#ifndef _WIN32_IE								
#define _WIN32_IE 0x0600						// Internet Explorer 6.0
#endif


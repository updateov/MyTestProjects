// AboutBoxWrapper.h

#pragma once



#ifdef ABOUT_BOX_EXPORT
#define ABOUT_BOX_IMPORT_EXPORT __declspec(dllexport)
#else
#define ABOUT_BOX_IMPORT_EXPORT __declspec(dllimport)
#pragma comment (lib, "AboutBoxWrapper.lib") // if importing, link also
#endif

extern "C"
{
	ABOUT_BOX_IMPORT_EXPORT void  __cdecl OpenAboutBox(int logo, bool bCheckList, const char* url, const char* udi, bool checklistApp, int timeout);

}



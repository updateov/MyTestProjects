// This is the main DLL file.

#include "stdafx.h"

#include "AboutBoxWrapper.h"

using namespace System;
using namespace System::Runtime::InteropServices;

extern "C"
{
	ABOUT_BOX_IMPORT_EXPORT void __cdecl OpenAboutBox(int logo, bool bCheckList, const char* url, const char* udi, bool checklistApp, int timeout)
	{
		PeriGen::Patterns::WPFLibrary::LibraryHelper::Instance->InitLogger(System::Diagnostics::SourceLevels::Information,
			System::Diagnostics::SourceLevels::Information,
			System::Diagnostics::SourceLevels::Information,
			"LMSLogs");
	
		String^ urlStr = gcnew String(url);
		String^ udiStr = gcnew String(udi);
		PeriGen::Patterns::WPFLibrary::LibraryHelper::Instance->ShowAboutWindow(urlStr, bCheckList, logo, udiStr, checklistApp);
	}

}

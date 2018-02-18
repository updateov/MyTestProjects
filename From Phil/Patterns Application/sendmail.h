#pragma once

#include <mapi.h>

class CSendMailHelper
{
public:
	static bool SendMail(HWND hWndParent, char* szDest, char* szSubject, char* szText)
	{
		if (!hWndParent || !::IsWindow(hWndParent))
			return false;

		bool isMAPIInstalled = GetProfileInt("Mail", "MAPI", 0) != 0;
		if (!isMAPIInstalled)
			return false;

		char libName[_MAX_PATH];
		GetProfileString("Mail", "CMCDLLNAME32", "MAPI32.DLL", libName, _MAX_PATH);

		HINSTANCE hMAPI = ::LoadLibraryA(libName);
		if (!hMAPI)
			return false;

		LPMAPISENDMAIL lpfnMAPISendMail = (LPMAPISENDMAIL)GetProcAddress(hMAPI, "MAPISendMail");
		if (!lpfnMAPISendMail)
			return false;

		// Prepare the message
		MapiMessage message;
		::ZeroMemory(&message, sizeof(message));

		message.lpRecips = (MapiRecipDesc *) malloc(sizeof(MapiRecipDesc));
		memset(&message.lpRecips[0], 0, sizeof(MapiRecipDesc));

		message.lpRecips[0].lpszName = szDest;
		message.lpRecips[0].ulRecipClass = MAPI_TO;
		message.nRecipCount = 1;
		message.lpszSubject = szSubject;
		message.lpszNoteText = szText;

		// Send the message
		int errorSend = (*lpfnMAPISendMail)(lhSessionNull, (ULONG_PTR)hWndParent, &message, MAPI_LOGON_UI | MAPI_DIALOG, 0);
		free(message.lpRecips);

		return (errorSend == SUCCESS_SUCCESS) || (errorSend == MAPI_USER_ABORT);
	}
};
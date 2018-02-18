// SerialShield.cpp: implementation of the SerialShield class.
// 
// This class is copyright Nic Wilson (C) 2005
// nic@nicwilson.com
//
// It is provided as an example of using SerialShield only.
// Distribution as example source code is permitted for IONWORX.COM
// only.  No other distribution is permitted without written consent
// of the author.  
//
// No changes to this file or files is permitted 
//
// Distribution in compiled binary form is permitted at no cost whatsoever,
// however acknowlegment of use and copyright must be shown in the "about"
// window of the application using this code,  and in the documentation.
// such as:  "Some code portions Copyright Nic Wilson  nic@nicwilson.com"
//
// 
// Version History
// V1.00 16th July 2005 - Initial Release
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "SerialShield.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

SerialShield::SerialShield()
{

}

SerialShield::~SerialShield()
{
	if (m_hSS)
		FreeLibrary(m_hSS);
}

BOOL SerialShield::InitClass()
{
	m_hSS = LoadLibrary(CString("SerialShield.dll"));
	if (m_hSS == NULL)
		return 2;
	GetHardwardID		= (char* (__stdcall *)(void))				 GetProcAddress( m_hSS, "GetHardwardID");
	SSUser				= (int  (__stdcall *)(char*, char*, char*))GetProcAddress( m_hSS, "SSUser");
	SS_TrialExpired		= (bool (__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_TrialExpired");
	SS_RemoveKey		= (bool (__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_RemoveKey");
	SS_IsUnlocked		= (bool (__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_IsUnlocked");
	SS_LicenseInfo		= (char*(__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_LicenseInfo");
	SS_GetUserName		= (char*(__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_GetUserName");
	SS_GetUserKey		= (char*(__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_GetUserKey");
	SS_GetUserSerialID	= (char*(__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_GetUserSerialID");
	SS_R				= (int  (__stdcall *)(char*, char*))		 GetProcAddress( m_hSS, "SS_R");
	SS_TrialMode		= (int  (__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_TrialMode");
	TripleDESEncrypt	= (char*(__stdcall *)(char*, char*))		 GetProcAddress( m_hSS, "TripleDESEncrypt");
	TripleDESDecrypt	= (char*(__stdcall *)(char*, char*))		 GetProcAddress( m_hSS, "TripleDESDecrypt");
	SS_Initialize		= (void (__stdcall *)(void))				 GetProcAddress( m_hSS, "SS_Initialize");
	SetApplicationInfo	= (void (__stdcall *)(char*, char*))		 GetProcAddress( m_hSS, "SetApplicationInfo");
	Antidebugging		= (void (__stdcall *)(void))				 GetProcAddress( m_hSS, "Antidebugging");
	AntiMonitors		= (void (__stdcall *)(void))				 GetProcAddress( m_hSS, "AntiMonitors");
	SS_DefaultKey		= (void (__stdcall *)(char*, char*, char*))GetProcAddress( m_hSS, "SS_DefaultKey");

	bool fail = false;
	if (GetHardwardID == NULL)
		fail = true;
	if (SSUser == NULL)
		fail = true;
	if (SS_TrialExpired == NULL)
		fail = true;
	if (SS_RemoveKey == NULL)
		fail = true;
	if (SS_IsUnlocked == NULL)
		fail = true;
	if (SS_LicenseInfo == NULL)
		fail = true;
	if (SS_GetUserName == NULL)
		fail = true;
	if (SS_GetUserKey == NULL)
		fail = true;
	if (SS_GetUserSerialID == NULL)
		fail = true;
	if (SS_R == NULL)
		fail = true;
	if (SS_TrialMode == NULL)
		fail = true;
	if (TripleDESEncrypt == NULL)
		fail = true;
	if (TripleDESDecrypt == NULL)
		fail = true;
	if (SS_Initialize == NULL)
		fail = true;
	if (SetApplicationInfo == NULL)
		fail = true;
	if (Antidebugging == NULL)
		fail = true;
	if (AntiMonitors == NULL)
		fail = true;
	if (SS_DefaultKey == NULL)
		fail = true;
	if (fail)
	{
		FreeLibrary(m_hSS);
		m_hSS = NULL;
		return 1;
	}
	return 0;
}

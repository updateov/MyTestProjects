// SerialShield.h: interface for the SerialShield class.
// 
// This class is copyright Nic Wilson (C) 2005
//  nic@nicwilson.com
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
#if !defined(AFX_SERIALSHIELD_H__5C4DA086_C149_472E_AF8B_DAC673603DA2__INCLUDED_)
#define AFX_SERIALSHIELD_H__5C4DA086_C149_472E_AF8B_DAC673603DA2__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class SerialShield  
{
public:
	//All these are public as they need to be accessed from outside the class.
	SerialShield();	
	virtual ~SerialShield();
	char*	(__stdcall *GetHardwardID)	 (void);
	int		(__stdcall *SSUser) 			 (char* Fullname, char* Key, char* SerialID);	
	bool	(__stdcall *SS_TrialExpired)	 (void);												
	bool	(__stdcall *SS_RemoveKey) 	 (void);
	bool	(__stdcall *SS_IsUnlocked)  	 (void);
	char*	(__stdcall *SS_LicenseInfo)	 (void);												
	char*	(__stdcall *SS_GetUserName)	 (void);													
	char*	(__stdcall *SS_GetUserKey)	 (void);													
	char*	(__stdcall *SS_GetUserSerialID)(void);													
	int		(__stdcall *SS_R) 			 (char* Name, char* Key);							
	int		(__stdcall *SS_TrialMode)		 (void);													
	char*	(__stdcall *TripleDESEncrypt)	 (char* Text, char* Key);						
	char*	(__stdcall *TripleDESDecrypt)	 (char* Text, char* Key);							
	void	(__stdcall *SS_Initialize) 	 (void);													
	void	(__stdcall *SetApplicationInfo)(char* ApplicationName, char* SoftKey);				
	void	(__stdcall *Antidebugging) 	 (void);													
	void	(__stdcall *AntiMonitors)		 (void);													
	void	(__stdcall *SS_DefaultKey) 	 (char* Username, char* LicenseKey, char* SerialID); 
	BOOL	InitClass();
	
private:
	//This is for class member use only.
	HINSTANCE m_hSS;
};

#endif // !defined(AFX_SERIALSHIELD_H__5C4DA086_C149_472E_AF8B_DAC673603DA2__INCLUDED_)


#include "stdafx.h"
#include "license_validation.h"
#include "shlwapi.h"

license_validation::license_validation()
{
	// Read user name & organization from registry
	m_User = "";
	m_Organization = "";
	m_Phone = "";

	HKEY key;
	if (::RegOpenKey(HKEY_LOCAL_MACHINE, "Software\\PeriGen\\Retrospective PeriCALM Patterns", &key) == ERROR_SUCCESS)
	{
		TCHAR buffer[256];
		DWORD len;
		DWORD type;

		len = sizeof(buffer)/sizeof(TCHAR);
		if ((::RegQueryValueEx(key, "User", 0, &type, (LPBYTE)buffer, &len)  == ERROR_SUCCESS) && (type == REG_SZ))
		{
			m_User = buffer;
		}

		len = sizeof(buffer)/sizeof(TCHAR);
		if ((::RegQueryValueEx(key, "Organization", 0, &type, (LPBYTE)buffer, &len)  == ERROR_SUCCESS) && (type == REG_SZ))
		{
			m_Organization = buffer;
		}

		len = sizeof(buffer)/sizeof(TCHAR);
		if ((::RegQueryValueEx(key, "Phone", 0, &type, (LPBYTE)buffer, &len)  == ERROR_SUCCESS) && (type == REG_SZ))
		{
			m_Phone = buffer;
		}

		::RegCloseKey(key);
	}

	m_SSptr = NULL;
}

license_validation::~license_validation(void)
{
	if (m_SSptr != NULL)
	{
		delete m_SSptr;
		m_SSptr = NULL;
	}
}

bool license_validation::initialize(CString application_name, CString application_key, CString& error)
{
	m_SSptr = new SerialShield;
	if (m_SSptr == NULL)
	{
		error = "Failed to load the license validation engine, contact PeriGen Support (error reference 8801)";
		return false;
	}

	int ret = m_SSptr->InitClass();
	if (ret == 2)
	{
		error = "Failed to initialize the license validation engine, contact PeriGen Support (error reference 8802)";
		return false;
	}

	if (ret == 1)
	{
		error = "Incorrect version of the license validation engine, contact PeriGen Support (error reference 8803)";
		return false;
	}

	m_SSptr->SS_R("Bruno Bendavid", "lXcsBCcUEgf/vg+bEYzzb8r4cksDbB0wV5+isMmk9BvvayXF+j4fChYY2+L83URYqlTCIxoRL16PcHoMtHICEQ==");
	m_SSptr->SetApplicationInfo(application_name.GetBuffer(application_name.GetLength()), application_key.GetBuffer(application_key.GetLength()));

	application_name.ReleaseBuffer();
	application_key.ReleaseBuffer();

	m_SSptr->SS_Initialize();

	return true;
}

bool license_validation::is_license_tampered()
{
	if (m_SSptr->SS_IsUnlocked())
		return false;

	if (m_SSptr->SS_TrialExpired())
		return false;

	return m_SSptr->SS_TrialMode() == 99;
}

bool license_validation::is_license_valid()
{
	if (m_SSptr->SS_IsUnlocked())
		return true;

	if (m_SSptr->SS_TrialExpired())
		return false;

	switch (m_SSptr->SS_TrialMode())
	{
		// Date tempered
		case 99:
			return false;

		// Time base / Run based / Date based
		case 1:
		case 2:
		case 3:
			return true;	
	}

	// Trial mode or any other unexpected mode is OFF
	return false;
}

CString license_validation::get_license_description()
{
	if (m_SSptr->SS_IsUnlocked())
		return "Registered Version.";

	if (m_SSptr->SS_TrialExpired())
		return "Your trial period has expired.";

	switch (m_SSptr->SS_TrialMode())
	{
		case 99:
			return "Your date system has been moved back - License blocked.";

		case 1:
			return CString(m_SSptr->SS_LicenseInfo()) + " day(s) remain before expiration.";

		case 2:
			return CString(m_SSptr->SS_LicenseInfo()) + " day(s) remain before expiration.";

		case 3:
			return CString(m_SSptr->SS_LicenseInfo()) + " run(s) remain before expiration.";
	}

	return "Your copy of PeriCALM® Patterns Retrospective™ is not registered.";
}

CString license_validation::get_hardware_id()
{
	return m_SSptr->GetHardwardID();
}

void license_validation::set_user_info(CString user, CString organization, CString phone)
{
	// Save the user & organization in the regitry
	HKEY key;
	DWORD disp;
	::RegCreateKeyEx(HKEY_LOCAL_MACHINE, "Software\\PeriGen\\Retrospective PeriCALM Patterns", 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &key, &disp);

	::RegSetValueEx(key, "User", 0, REG_SZ, (LPBYTE)(LPCTSTR)user.GetBuffer(user.GetLength()), user.GetLength() + 1);
	::RegSetValueEx(key, "Organization", 0, REG_SZ, (LPBYTE)(LPCTSTR)organization.GetBuffer(organization.GetLength()), organization.GetLength() + 1);
	::RegSetValueEx(key, "Phone", 0, REG_SZ, (LPBYTE)(LPCTSTR)phone.GetBuffer(phone.GetLength()), phone.GetLength() + 1);

	::RegCloseKey(key);

	user.ReleaseBuffer();
	organization.ReleaseBuffer();
	phone.ReleaseBuffer();

	m_User = user;
	m_Organization = organization;
	m_Phone = phone;
}

bool license_validation::update_key(CString user_key, CString& error)
{
#ifndef _DEBUG
	//	m_SSptr->Antidebugging();
	//	m_SSptr->AntiMonitors();
#endif

	CString hardwareID = get_hardware_id();

	// Update new one
	int result = m_SSptr->SSUser(m_User.GetBuffer(m_User.GetLength()), user_key.GetBuffer(user_key.GetLength()), hardwareID.GetBuffer(hardwareID.GetLength()));

	m_User.ReleaseBuffer();
	user_key.ReleaseBuffer();
	hardwareID.ReleaseBuffer();

	switch (result)
	{
		case 1:
		case 4:
			return true;

		case 3:
		default:
			error = "Invalid Key";
			return false;
	}

}


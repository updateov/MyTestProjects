#pragma once

#include "afxwin.h"
#include "SerialShield.h"

class license_validation
{
public:
	license_validation();
	virtual ~license_validation(void);

	bool initialize(CString application_name, CString application_key, CString& error);
	bool update_key(CString user_key, CString& error);

	void set_user_info(CString user, CString organization, CString phone);

	bool is_license_valid();
	bool is_license_tampered();

	CString get_user_name() {return m_User;}
	CString get_organization() {return m_Organization;}
	CString get_phone() {return m_Phone;}
	CString get_hardware_id();
	CString get_license_description();

	
private:
	SerialShield *m_SSptr;

	CString m_User;
	CString m_Organization;
	CString m_Phone;
};

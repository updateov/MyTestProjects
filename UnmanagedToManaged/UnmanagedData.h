#pragma once
class UnmanagedData
{
public:
	UnmanagedData();
	virtual ~UnmanagedData();

	CString GetString();

private:
	CString m_str;
};


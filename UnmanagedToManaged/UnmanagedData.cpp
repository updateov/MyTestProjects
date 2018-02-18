#include "stdafx.h"
#include "UnmanagedData.h"


UnmanagedData::UnmanagedData()
{
	m_str = "blabla";
}


UnmanagedData::~UnmanagedData()
{
}

CString UnmanagedData::GetString()
{
	return m_str;
}

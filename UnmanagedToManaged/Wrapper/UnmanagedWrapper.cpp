#include "stdafx.h"
#include "UnmanagedWrapper.h"
#include "ManagedPart.h"

//UnmanagedWrapper::UnmanagedWrapper(UnmanagedData& data)
UnmanagedWrapper::UnmanagedWrapper(const CString& txt)
{
	//m_txt = data.GetString();
	m_txt = txt;
	ManagedPart^ mp = ManagedPart::Instance();
}


UnmanagedWrapper::~UnmanagedWrapper()
{
}

void UnmanagedWrapper::GetHwnd(HWND parent, int x, int y, int width, int height)
{
	ManagedPart^ mp = ManagedPart::Instance();
	mp->GetHwnd(parent, x, y, width, height);
}

UnmanagedWrapper* CreateWrapper(CString txt)
{
	UnmanagedWrapper* pWrapper = new UnmanagedWrapper(txt);
	return pWrapper;
}

void GetHwndE(UnmanagedWrapper* pWrapper, HWND parent, int x, int y, int width, int height)
{
	pWrapper->GetHwnd(parent, x, y, width, height);
}

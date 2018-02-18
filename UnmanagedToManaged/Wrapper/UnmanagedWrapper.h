#pragma once

#include "..\UnmanagedData.h"

class LIBRARY_API UnmanagedWrapper
{
public:
	UnmanagedWrapper(const CString& txt);
	//UnmanagedWrapper(UnmanagedData& data);
	~UnmanagedWrapper();

	void GetHwnd(HWND parent, int x, int y, int width, int height);

private:
	CString m_txt;
};

//LIBRARY_API UnmanagedWrapper* CreateWrapper(CString txt);
//LIBRARY_API void GetHwndE(UnmanagedWrapper* pWrapper, HWND parent, int x, int y, int width, int height);


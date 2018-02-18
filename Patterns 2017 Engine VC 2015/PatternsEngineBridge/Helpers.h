#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Text;

inline CString ToCString(String^ strParam)
{
	using Runtime::InteropServices::Marshal;
	IntPtr ptr = Marshal::StringToHGlobalAnsi(strParam);
	CString str = static_cast<LPCTSTR>(const_cast<void*>(static_cast<const void*>(ptr)));
	Marshal::FreeHGlobal(ptr);
	return str;
}

inline String^ ToSystemString(LPCTSTR strParam)
{
	return gcnew String(strParam);
}

inline StringBuilder^ ToSystemStringBuilder(LPCTSTR strParam)
{
	return gcnew StringBuilder(gcnew String(strParam));
}

inline char* ToCharPtr(cli::array<byte>^ byteArray)
{
	pin_ptr<byte> p = &byteArray[0];
	unsigned char* pby = p;
	char* pch = reinterpret_cast<char*>(pby);
	return pch;
}

// ChildClientBridge.h

#pragma once

#include "..\ChildClientEngine\EngineManager.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Text;

namespace ChildClientBridge 
{
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

	public ref class EngineBridge
	{
	public: 
		EngineBridge();
		void LoadArrayNumbers(int start, int length, long double val);
		List<long double>^ GetSubArray(int start, int length);

		String^ StringBuilderTest();
	};
}

// Bridge.h

#pragma once

#include "NativeCode.h"

using namespace System;

namespace Bridge 
{

	public ref class BridgeClass
	{
	private:
		NativeCode* m_pNativeCode;

	public:
		BridgeClass();
		int GetNumBridge(int num);
	};
}

// This is the main DLL file.

#include "stdafx.h"

#include "Bridge.h"

namespace Bridge
{
	BridgeClass::BridgeClass()
	{
		m_pNativeCode = new NativeCode();
	}

	int BridgeClass::GetNumBridge(int num)
	{
		return m_pNativeCode->GetNum(num);
	}
}
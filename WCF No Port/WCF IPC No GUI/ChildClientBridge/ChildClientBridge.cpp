// This is the main DLL file.

#include "stdafx.h"

#include "ChildClientBridge.h"
#include "..\ChildClientEngine\EngineManager.h"
#include "..\ChildClientEngine\ChildClientEngine.h"

using namespace System;

namespace ChildClientBridge
{
	EngineBridge::EngineBridge()
	{
		EngineManager::InitInstance();
	}

	void EngineBridge::LoadArrayNumbers(int start, int length, long double val)
	{
		EngineManager* engMngr = EngineManager::Instance();
		ChildClientEngine* engine = engMngr->GetEngine();
		engine->LoadArrayNumbers(start, length, val);
	}

	List<long double>^ EngineBridge::GetSubArray(int start, int length)
	{
		return nullptr;
	}


	String^ EngineBridge::StringBuilderTest()
	{
		EngineManager* engMngr = EngineManager::Instance();
		CString str("");
		engMngr->GetStringBuilder(str);
		String^ toRet = ToSystemString(str);
		return toRet;
	}
}
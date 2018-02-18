// This is the main DLL file.

#include "stdafx.h"

#include "PatternsEngineBridge.h"
#include "EngineManager.h"
#include "Helpers.h"

using namespace std;


namespace PatternsEngineBridge
{
	EngineBridge::EngineBridge()
	{
	}

	array<String^>^ EngineBridge::ActiveVisits::get()
	{
		CStringList* result = EngineManager::GetManager()->GetActiveVisits();
		
		array<String^>^ managedArray = gcnew array<String^>(result->GetCount());

		POSITION pos = result->GetHeadPosition();
		int i = 0;
		while (pos != NULL)
		{
			CString value = result->GetNext(pos);
			managedArray[i] = ToSystemString(value);
			i++;
		}

		return managedArray;
	}

	void EngineBridge::AddEpisode(String^ visitKey)
	{
		EngineManager::GetManager()->AddEpisode(ToCString(visitKey));
	}

	void EngineBridge::RemoveEpisode(String^ visitKey)
	{
		EngineManager::GetManager()->RemoveEpisode(ToCString(visitKey));
	}

	void EngineBridge::RemoveAllEpisodes()
	{
		EngineManager::GetManager()->RemoveAllEpisodes();
	}

	int EngineBridge::EngineProcessData(String^ visitKey, array<byte>^ ups, int upPosition, int upBlockSize , array<byte>^ hrs, int hrPosition, int hrBlockSize)
	{
		return EngineManager::GetManager()->EngineProcessData(ToCString(visitKey), ups, upPosition, upBlockSize, hrs, hrPosition, hrBlockSize);
	}

	bool EngineBridge::EngineReadResults(String^ visitKey, StringBuilder^% data, int buffer_size)
	{
		CString dataStr('0', buffer_size);
		bool moreData = EngineManager::GetManager()->EngineReadResults(ToCString(visitKey), dataStr, buffer_size);
		data = ToSystemStringBuilder(dataStr);
		return moreData;
	}
}

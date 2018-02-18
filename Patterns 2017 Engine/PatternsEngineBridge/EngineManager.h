#pragma once

#include "..\Patterns Driver\PatternsWorkerWrapper.h"
//#include "..\..\Source\Pattern Detection\patterns, contraction detection.h"
//#include "..\..\Source\Patterns\patterns, fetus.h"
//#include "..\..\Source\Pattern Detection\DigitalSignal.h"

using namespace PatternsDriver;
using namespace std;
using namespace patterns;

class EngineManager
{
// Singleton operations
private:
	EngineManager();
	virtual ~EngineManager();

	static EngineManager* s_instance;

public:
	static void InitManager();
	static void DestroyManager();
	static EngineManager* GetManager();
// End Singleton

public:
	PatternsWorkerWrapper* GetPatternsEngine(const CString& visitKey);
	CStringList* GetActiveVisits();
	void AddEpisode(const CString& visitKey);
	void RemoveEpisode(const CString& visitKey);
	void RemoveAllEpisodes();
	int EngineProcessData(const CString& visitKey, array<byte>^ ups, int upPosition, int upBlockSize, array<byte>^ hrs, int hrPosition, int hrBlockSize);
	bool EngineReadResults(CString& visitKey, CString& data, int buffer_size);

private:
	CMapStringToPtr m_visitKey2EnginePtr;
};


#include "StdAfx.h"
#include "EngineManager.h"
#include "Helpers.h"

using namespace PatternsDriver;

EngineManager* EngineManager::s_instance = NULL;

EngineManager::EngineManager()
{
}

EngineManager::~EngineManager()
{
}

void EngineManager::InitManager()
{
	if (!s_instance)
		s_instance = new EngineManager;
}

EngineManager* EngineManager::GetManager()
{
	InitManager();
	return s_instance;
}

PatternsWorkerWrapper* EngineManager::GetPatternsEngine(const CString& visitKey)
{
	void* ptr;
	m_visitKey2EnginePtr.Lookup(visitKey, ptr);
	if (!ptr)
		return NULL;

	PatternsWorkerWrapper* pWorker = (PatternsWorkerWrapper*)ptr;
	return pWorker;
}

CStringList* EngineManager::GetActiveVisits()
{
	CStringList* activeVisits = new CStringList();

	POSITION pos;
	CString curStr("");
	PatternsWorkerWrapper* pWorker = NULL;
	for (pos = m_visitKey2EnginePtr.GetStartPosition(); pos; )
	{
		m_visitKey2EnginePtr.GetNextAssoc(pos, curStr, (void*&)pWorker);
		activeVisits->AddTail(curStr);
	}

	return activeVisits;
}

void EngineManager::AddEpisode(const CString& visitKey)
{
	m_visitKey2EnginePtr.SetAt(visitKey, new PatternsWorkerWrapper());
}

void EngineManager::RemoveEpisode(const CString& visitKey)
{
	void* ptr;
	m_visitKey2EnginePtr.Lookup(visitKey, ptr);
	if (ptr)
	{
		PatternsWorkerWrapper* pWorker = (PatternsWorkerWrapper*)ptr;
		delete pWorker;
		pWorker = NULL;

		m_visitKey2EnginePtr.RemoveKey(visitKey);
	}
}

void EngineManager::RemoveAllEpisodes()
{
	POSITION pos;
	CString curStr("");
	PatternsWorkerWrapper* pWorker = NULL;
	for (pos = m_visitKey2EnginePtr.GetStartPosition(); pos; )
	{
		m_visitKey2EnginePtr.GetNextAssoc(pos, curStr, (void*&)pWorker);
		if (pWorker)
		{
			delete pWorker;
			pWorker = NULL;
		}

		m_visitKey2EnginePtr.SetAt(curStr, NULL);
	}
	m_visitKey2EnginePtr.RemoveAll();
}

// PAW 2017112
//int EngineManager::EngineProcessData(const CString& visitKey, array<byte>^ ups, int upPosition, int upBlockSize, array<byte>^ hrs, int hrPosition, int hrBlockSize)
int EngineManager::EngineProcessData(const CString& visitKey, cli::array<byte>^ ups, int upPosition, int upBlockSize, cli::array<byte>^ hrs, int hrPosition, int hrBlockSize)
{
	PatternsWorkerWrapper* pWorker = GetPatternsEngine(visitKey);
	int bufferSize = pWorker->ProcessUP(ToCharPtr(ups), upPosition, upBlockSize);
	bufferSize = pWorker->ProcessHR(ToCharPtr(hrs), hrPosition, hrBlockSize);
	return bufferSize;
}

bool EngineManager::EngineReadResults(CString& visitKey, CString& data, int buffer_size)
{
	PatternsWorkerWrapper* pWorker = GetPatternsEngine(visitKey);
	char* chData = (LPSTR)(LPCSTR)data;
	return pWorker->ReadResults(chData, buffer_size);
}

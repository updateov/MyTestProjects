#include "StdAfx.h"
#include "EngineManager.h"

EngineManager* EngineManager::s_instance = NULL;

EngineManager::EngineManager()
{
	if (m_engine)
		m_engine = NULL;

	m_engine = new ChildClientEngine();
}

EngineManager::~EngineManager()
{
	if (m_engine)
		delete m_engine;

	m_engine = NULL;
}

void EngineManager::InitInstance()
{
	if (s_instance == NULL)
		s_instance = new EngineManager();
}

EngineManager* EngineManager::Instance()
{
	return s_instance;
}

void EngineManager::Destroy()
{
	if (s_instance)
		delete s_instance;

	s_instance = NULL;
}

ChildClientEngine* EngineManager::GetEngine()
{
	return m_engine;
}

void EngineManager::GetStringBuilder(CString& toRet)
{
	toRet = m_engine->GetStringBuffer();
}
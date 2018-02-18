#pragma once

#include "ChildClientEngine.h"

#ifdef IMPEXP
	#undef IMPEXP
#endif

#ifdef CHILDCLIENTENGINE
	#define IMPEXP __declspec(dllexport)
#else
	#define IMPEXP __declspec(dllimport)
#endif

class IMPEXP EngineManager
{
private:
	EngineManager();
	~EngineManager();

	static EngineManager* s_instance;

public:
	static void InitInstance();
	static EngineManager* Instance();
	static void Destroy();

private:
	ChildClientEngine* m_engine;

public:
	ChildClientEngine* GetEngine();
	void GetStringBuilder(CString& output_buffer);
};


// ChildClientEngine.cpp : Defines the initialization routines for the DLL.
//

#include "stdafx.h"
#include "ChildClientEngine.h"
#include <string>

using namespace std;

ChildClientEngine::ChildClientEngine()
{
}

ChildClientEngine::~ChildClientEngine()
{
}

void ChildClientEngine::LoadArrayNumbers(int startIndex, int length, long double val)
{
	for (int i = startIndex; i < length; i++)
	{
		m_array1[i] = val;
	}
}

CString ChildClientEngine::GetStringBuffer()
{
	std::string m_pendingResults = "bdfsb|4|6|6|6|gfd|7657|fgdh";
	CString toRet(m_pendingResults.c_str());
	return toRet;
}
// ChildClientEngine.h : main header file for the ChildClientEngine DLL
//

#pragma once

#ifdef IMPEXP
	#undef IMPEXP
#endif

#ifdef CHILDCLIENTENGINE
	#define IMPEXP __declspec(dllexport)
#else
	#define IMPEXP __declspec(dllimport)
#endif

using namespace std;

class IMPEXP ChildClientEngine
{
public:
	ChildClientEngine();
	~ChildClientEngine();

private:
	long double m_array1[500000];

public:
	void LoadArrayNumbers(int startIndex, int length, long double val);
	CString GetStringBuffer();
};

// FiltersTestConsole.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "FiltersTestConsole.h"
#include "FiltersCalculator.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// The one and only application object

CWinApp theApp;

using namespace std;

int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	FiltersCalculator calc;
	calc.InitTracingData();
	calc.BP2Processing();
	calc.AddMoreData();
	calc.BP2Processing();
	return 0;
}

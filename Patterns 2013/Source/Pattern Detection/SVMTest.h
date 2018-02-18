#pragma once

#include "SVMInput.h"
#include "SVMModel.h"

#include <vector>
using namespace std;
using namespace Torch;

namespace patterns
{
class SVMTest : Object
{
public:
	SVMTest();
	~SVMTest(void);
public:
	bool Run(SVMInput& input, SVMModel* pModel);
	//bool Run(SVMInput& input, const char* modelName);
//private:	
//	bool InitCmdLine(const char* modelName);
public:
	vector<int> m_results;
private:	
	vector<bool> m_testResults;
	
	
	int m_numExamples;
};

}


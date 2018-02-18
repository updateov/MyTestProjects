#pragma once
#include "cmdline.h"
namespace Torch
{
class MemCmdLine :
	public CmdLine
{
public:
	MemCmdLine(void);
	~MemCmdLine(void);
public:
	bool LoadParameters(char** names, char** values, int numOfInitParams, int masterSwitch);

};
}


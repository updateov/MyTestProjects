#include "MemCmdLine.h"

namespace Torch
{

MemCmdLine::MemCmdLine(void)
{
}


MemCmdLine::~MemCmdLine(void)
{
}

bool MemCmdLine::LoadParameters(char** names, char** values, int numOfInitParams, int masterSwitch)
{
	CmdOption **cmd_options_ = cmd_options[masterSwitch];
	int n_cmd_options_ = n_cmd_options[masterSwitch];
	  // Initialize the options.
	for(int i = 0; i < n_cmd_options_; i++)
		cmd_options_[i]->initValue();
	for(int n = 0; n < numOfInitParams; n++)
	{
		char* name = names[n];
		int current_option = -1;
		for(int i = 0; i < n_cmd_options_; i++)
		{
			if(strcmp(name, cmd_options_[i]->name) == 0)		 
			{
				current_option = i;
				break;
			}
		}
		if(current_option >= 0)
		{	
			if(!cmd_options_[current_option]->LoadValue(values[n]))
				return false;
			cmd_options_[current_option]->is_setted = true;
		}
		else
		{
			// Check for arguments
			for(int i = 0; i < n_cmd_options_; i++)
			{
				if(cmd_options_[i]->isArgument() && (!cmd_options_[i]->is_setted))
				{
					current_option = i;
					break;
				}
			}
       
			if(current_option >= 0)
			{
				if(!cmd_options_[current_option]->LoadValue(values[n]))
					return false;
				cmd_options_[current_option]->is_setted = true;        
			}
			else
				return false;
		    
		}
		
	}
	return true;
 
}

}

// Copyright (C) 2003--2004 Ronan Collobert (collober@idiap.ch)
//                
// This file is part of Torch 3.1.
//
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. The name of the author may not be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#include "afxwin.h"
#include "general.h"
#include <exception>
using namespace std;
namespace Torch {

char xxpetit_message_pour_melanie[10000];
static HANDLE m_hEventLog = NULL;
void error(const char* msg, ...)
{
  va_list args;
  va_start(args,msg);
  vsprintf(xxpetit_message_pour_melanie, msg, args);
#ifdef _DEBUG
  printf("\n$ Error: %s\n\n", xxpetit_message_pour_melanie);
  fflush(stdout);
#endif
  va_end(args);
  ////// trace //////////////////
	::OutputDebugString(xxpetit_message_pour_melanie);
	if (xxpetit_message_pour_melanie[::_tcslen(xxpetit_message_pour_melanie) - 1] != _T('\n'))
	{
		::OutputDebugString(_T("\n"));
	}

	if (m_hEventLog == NULL)
	{
		m_hEventLog = RegisterEventSource(NULL, _T("PeriGen Patterns Torch"));
	}

	if (m_hEventLog != NULL)
	{
		LPCTSTR msg = (LPCTSTR)(&xxpetit_message_pour_melanie[0]);
		ReportEvent(m_hEventLog, EVENTLOG_ERROR_TYPE, 1, 0, NULL, 1,	0, &msg, NULL);
	}
	///////////////////////////////////////
 // exit(-1);
	throw exception("TORCH FATAL ERROR");
}

void warning(const char* msg, ...)
{
  va_list args;
  va_start(args,msg);
  vsprintf(xxpetit_message_pour_melanie, msg, args);
#ifdef _DEBUG
  printf("! Warning: %s\n", xxpetit_message_pour_melanie);
  fflush(stdout);
#endif
  va_end(args);
    ////// trace //////////////////
	::OutputDebugString(xxpetit_message_pour_melanie);
	if (xxpetit_message_pour_melanie[::_tcslen(xxpetit_message_pour_melanie) - 1] != _T('\n'))
	{
		::OutputDebugString(_T("\n"));
	}

	if (m_hEventLog == NULL)
	{
		m_hEventLog = RegisterEventSource(NULL, _T("PeriGen Patterns Torch"));
	}

	if (m_hEventLog != NULL)
	{
		LPCTSTR msg = (LPCTSTR)(&xxpetit_message_pour_melanie[0]);
		ReportEvent(m_hEventLog, EVENTLOG_WARNING_TYPE, 1, 0, NULL, 1,	0, &msg, NULL);
	}
	///////////////////////////////////////

}

void message(const char* msg, ...)
{

  va_list args;
  va_start(args,msg);
  vsprintf(xxpetit_message_pour_melanie, msg, args);
  #ifdef _DEBUG
  printf("# %s\n", xxpetit_message_pour_melanie);
  fflush(stdout);
  #endif
  va_end(args);

 //     ////// trace //////////////////
	//::OutputDebugString(xxpetit_message_pour_melanie);
	//if (xxpetit_message_pour_melanie[::_tcslen(xxpetit_message_pour_melanie) - 1] != _T('\n'))
	//{
	//	::OutputDebugString(_T("\n"));
	//}

	//if (m_hEventLog == NULL)
	//{
	//	m_hEventLog = RegisterEventSource(NULL, _T("PeriGen Patterns Torch"));
	//}

	//if (m_hEventLog != NULL)
	//{
	//	LPCTSTR msg = (LPCTSTR)(&xxpetit_message_pour_melanie[0]);
	//	ReportEvent(m_hEventLog, EVENTLOG_INFORMATION_TYPE, 1, 0, NULL, 1,	0, &msg, NULL);
	//}
	/////////////////////////////////////////


}

void print(const char* msg, ...)
{
#ifdef _DEBUG
  va_list args;
  va_start(args,msg);
  vsprintf(xxpetit_message_pour_melanie, msg, args);
  printf("%s", xxpetit_message_pour_melanie);
  fflush(stdout);
  va_end(args);
#endif
}

void controlBar(int level, int max_level)
{
  if(level == -1)
    print("[");
  else
  {
    if(max_level < 10)
      print(".");
    else
    {
      if( !(level % (max_level/10) ) )
        print(".");
    }
  
    if(level == max_level-1)
      print("]\n");
  }
}

}

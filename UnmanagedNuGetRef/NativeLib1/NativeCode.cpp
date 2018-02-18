#include "StdAfx.h"
#include "NativeCode.h"


NativeCode::NativeCode()
{
}


NativeCode::~NativeCode()
{
}

int NativeCode::GetNum(int num)
{
	int toRet = num * 2;
	return toRet;
}

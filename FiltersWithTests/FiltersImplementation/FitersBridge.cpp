#include "stdafx.h"
#include "FitersBridge.h"
#include "FilterWrapper.h"

using namespace std;

FitersBridge::FitersBridge()
{
}


FitersBridge::~FitersBridge()
{
}

void FitersBridge::FillTracingsData(List<int>^ tracingsData)
{
	FilterWrapper* wrapper = FilterWrapper::Instance();
	FiltersCalculator* calc = wrapper->GetCalculator();
	vector<unsigned char> inputData = calc->GetTracingData();
	for each (int i in tracingsData)
	{
		inputData.push_back(i);
	}
}
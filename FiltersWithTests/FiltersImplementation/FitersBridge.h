#pragma once

using namespace std;
using namespace System;
using namespace System::Collections::Generic;

ref class FitersBridge
{
public:
	FitersBridge();
	virtual ~FitersBridge();

	void FillTracingsData(List<int>^ tracingsData);
};


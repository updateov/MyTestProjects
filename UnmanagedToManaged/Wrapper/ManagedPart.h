#pragma once

using System::String;
using namespace System;
using namespace System::Windows;
using namespace System::Windows::Controls;
using namespace System::Windows::Media;
using namespace System::Runtime;
using namespace System::Runtime::InteropServices;
using namespace System::Windows::Interop;
using namespace WPFControls;
using namespace System::Threading;


ref class Globals
{
public:
	static System::Windows::Interop::HwndSource^ gHwndSource;
	static ControlData^ gwcControlDAta;
};


ref class ManagedPart
{
public:
	ManagedPart(CString txt);
	ManagedPart();
	virtual ~ManagedPart();

	static ManagedPart^ Instance();

	//void GetHwnd(HWND parent, int x, int y, int width, int height);
	HWND GetHwnd(HWND parent, int x, int y, int width, int height);

private:
	String^ m_txt;
	static ManagedPart^ s_instance;
};


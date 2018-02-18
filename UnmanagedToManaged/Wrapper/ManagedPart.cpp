#include "stdafx.h"
#include "ManagedPart.h"

using namespace ManagedGUI;
using namespace System;
using namespace System::Windows;
using namespace System::Windows::Interop;
using namespace System::Threading;

ManagedPart::ManagedPart(CString txt)
{
	m_txt = gcnew String(txt);
	//ManagedWindow^ gcWind = gcnew ManagedWindow();
	//gcWind->SetText = m_txt;
	//gcWind->ShowDialog();
}

ManagedPart::ManagedPart()
{
	m_txt = "blablabla";
}

ManagedPart::~ManagedPart()
{
}

ManagedPart^ ManagedPart::Instance()
{
	if (s_instance == nullptr)
	{
		s_instance = gcnew ManagedPart();
	}

	return s_instance;
}

HWND ManagedPart::GetHwnd(HWND parent, int x, int y, int width, int height)
//void ManagedPart::GetHwnd(HWND parent, int x, int y, int width, int height)
{
	HwndSourceParameters^ sourceParams = gcnew HwndSourceParameters("MFCWPFApp");
	sourceParams->PositionX = x;
	sourceParams->PositionY = y;
	sourceParams->Height = height;
	sourceParams->Width = width;
	sourceParams->ParentWindow = IntPtr(parent);
	sourceParams->WindowStyle = WS_VISIBLE | WS_CHILD;
	
	//Globals::gHwndSource = gcnew HwndSource(*sourceParams);

	//FrameworkElement^ myPage = Globals::gwcControlDAta;

	//
	//Globals::gHwndSource->RootVisual = myPage;
	Connector^ cn = gcnew Connector(*sourceParams);

	return (HWND)cn->InitCtrl()->Handle.ToPointer();
}


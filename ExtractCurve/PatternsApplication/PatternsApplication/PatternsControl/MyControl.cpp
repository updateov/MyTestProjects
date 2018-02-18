#include "stdafx.h"
#include "MyControl.h"

IMPLEMENT_DYNAMIC(MyControl, CWnd)


MyControl::MyControl()
{
}


MyControl::~MyControl()
{
}



BEGIN_MESSAGE_MAP(MyControl, CWnd)
END_MESSAGE_MAP()



// MyControl message handlers

/////////////////////////////////////////////////////////////////////////////
// MyControl message handlers

BOOL MyControl::RegisterWndClass()
{
	WNDCLASS windowclass;
	HINSTANCE hInst = AfxGetInstanceHandle();

	//Check weather the class is registerd already
	if (!(::GetClassInfo(hInst, MYWNDCLASS, &windowclass)))
	{
		//If not then we have to register the new class
		windowclass.style = CS_DBLCLKS;// | CS_HREDRAW | CS_VREDRAW;
		windowclass.lpfnWndProc = ::DefWindowProc;
		windowclass.cbClsExtra = windowclass.cbWndExtra = 0;
		windowclass.hInstance = hInst;
		windowclass.hIcon = NULL;
		windowclass.hCursor = AfxGetApp()->LoadStandardCursor(IDC_ARROW);
		windowclass.hbrBackground = ::GetSysColorBrush(COLOR_WINDOW);
		windowclass.lpszMenuName = NULL;
		windowclass.lpszClassName = MYWNDCLASS;


		if (!AfxRegisterClass(&windowclass))
		{
			AfxThrowResourceException();
			return FALSE;
		}
	}

	return TRUE;

}


int MyControl::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CWnd::OnCreate(lpCreateStruct) == -1)
		return -1;

	// TODO: Add your specialized creation code here


	return 0;
}



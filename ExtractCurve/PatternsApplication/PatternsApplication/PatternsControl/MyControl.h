#pragma once
#include "afxwin.h"

#define DllExport   __declspec( dllexport )


#define MYWNDCLASS "MyDrawPad"

class DllExport MyControl : public CWnd
{
	DECLARE_DYNAMIC(MyControl)

public:
	MyControl();
	virtual ~MyControl();


protected:
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);

	DECLARE_MESSAGE_MAP()
private:
	CDC cDC;
	BOOL RegisterWndClass();
	CPoint oldpt;
	BOOL flag;
};


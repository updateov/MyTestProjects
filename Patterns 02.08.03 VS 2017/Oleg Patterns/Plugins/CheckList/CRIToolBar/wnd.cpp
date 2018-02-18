


#include "stdafx.h"

#include "globals.hpp"
#include "wnd.hpp"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

Wnd::Wnd(char* pcClassName, HWND parentWnd)
{
	this->hWnd = NULL;
	this->GetHandle(pcClassName, parentWnd);
	this->GetRect();
}

Wnd::~Wnd()
{
}

HWND Wnd::GetHWnd()
{
	return this->hWnd;
}

void Wnd::GetHandle(char* pcClassName, HWND parentWnd)
{
	HWND hWnd = ::FindWindowExA(parentWnd, NULL, pcClassName, NULL);
	this->SaveHWnd(hWnd);
}

void Wnd::SaveHWnd(HWND hWnd)
{
	this->hWnd = hWnd;
	m_pCWnd = CWnd::FromHandle(hWnd); //CDialog::FromHandlePermanent(this->GetHWnd());
}

CWnd* Wnd::GetCWnd()
{
	return m_pCWnd;
}

RECT Wnd::GetRect()
{
	::GetWindowRect(this->hWnd, &r);
	return r;
}

bool Wnd::IsRectChanged()
{
	bool bRet = false;
	RECT coTmpRect;
	::GetWindowRect(this->hWnd, &coTmpRect);

	if (
		(coTmpRect.left != r.left) ||
		(coTmpRect.top != r.top) ||
		(coTmpRect.bottom != r.bottom) ||
		(coTmpRect.right != r.right)
	   )
	{
		bRet = true;
		r = coTmpRect;
	}

	return bRet;
}


int Wnd::GetProportion()
{
	int nRet = NOTDEFINED;
	int nHor, nVer;

	nHor = this->GetRect().right - this->GetRect().left;
	nVer = this->GetRect().bottom - this->GetRect().top;

 	if (nHor >= nVer)
	{
		nRet = HORIZONTAL;
	}
	else if (nHor < nVer)
	{
		nRet = VERTICAL;
	}

	return nRet;
}


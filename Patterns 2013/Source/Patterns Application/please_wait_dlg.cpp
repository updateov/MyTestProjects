// please_wait_dlg.cpp : implementation file
//

#include "stdafx.h"
#include "please_wait_dlg.h"


// please_wait_dlg dialog

IMPLEMENT_DYNAMIC(please_wait_dlg, CDialog)

please_wait_dlg::please_wait_dlg(CWnd* pParent /*=NULL*/)
	: CDialog(please_wait_dlg::IDD, pParent)
{

}

please_wait_dlg::~please_wait_dlg()
{
}

/*
 =======================================================================================================================
    center the window before displaying it.
 =======================================================================================================================
 */
BOOL please_wait_dlg::OnInitDialog(void)
{
	CDialog::OnInitDialog();

	CenterWindow();
	ShowWindow(SW_SHOW);

	return true;
}


void please_wait_dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BOOL please_wait_dlg::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message)
{
	SetCursor(::LoadCursor(NULL, IDC_WAIT));
	return TRUE;
}

void please_wait_dlg::SetMessage(CString message)
{
	GetDlgItem(IDC_WAIT_MESSAGE)->SetWindowText(message);
	UpdateWindow();
}

BEGIN_MESSAGE_MAP(please_wait_dlg, CDialog)
	ON_WM_SETCURSOR()
END_MESSAGE_MAP()


// please_wait_dlg message handlers

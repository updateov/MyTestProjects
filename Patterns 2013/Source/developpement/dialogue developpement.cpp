#include "stdafx.h"
#include "developpement.h"
#include "dialogue developpement.h"
#include "./dialogue developpement.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
BEGIN_MESSAGE_MAP(dialogue_developpement, CDialog)
	ON_WM_SIZE()
END_MESSAGE_MAP()

/*
 =======================================================================================================================
 =======================================================================================================================
 */
dialogue_developpement::dialogue_developpement(CWnd *p) :
	CDialog(dialogue_developpement::IDD, p)
{
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void dialogue_developpement::DoDataExchange(CDataExchange *x)
{
	CDialog::DoDataExchange(x);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
BOOL dialogue_developpement::OnInitDialog(void)
{
	/*~~~~~~~~~~~~~~~~~~~~*/
	CRect r(0, 0, 100, 100);
	/*~~~~~~~~~~~~~~~~~~~~*/

	CDialog::OnInitDialog();
	f.Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_VISIBLE, r, this, 0);
	f.set_has_debug_keys();
	place_controles();
	return TRUE;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void dialogue_developpement::place_controles(void)
{
	if (f.GetSafeHwnd())
	{
		/*~~~~*/
		CRect r;
		/*~~~~*/

		GetClientRect(&r);
		r.bottom -= 40;
		f.SetWindowPos(0, r.left, r.top, r.Width(), r.Height(), SWP_NOACTIVATE | SWP_NOCOPYBITS | SWP_NOOWNERZORDER | SWP_NOZORDER);
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
VOID dialogue_developpement::OnSize(UINT, int, int)
{
	place_controles();
}

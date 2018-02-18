// MyControl.cpp : implementation file
//

#include "stdafx.h"
#include "MyControl.h"
#include "afxdialogex.h"
#include "resource.h"


// MyControl dialog

IMPLEMENT_DYNAMIC(MyControl, CDialog)

MyControl::MyControl(CWnd* pParent /*=NULL*/) 
	: CDialog(IDD_DIALOG1, pParent)
{

}

MyControl::~MyControl()
{
}

void MyControl::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

void MyControl::OnDrawItem(UINT nIdCTL, LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	//CDC* pDC = CDC::FromHandle(lpDrawItemStruct->hDC);
	//CRect rect = lpDrawItemStruct->rcItem;
	//UINT state = lpDrawItemStruct->itemState;
	//CBitmap bmp;
	//if (bmp.LoadBitmapA("IDB_BITMAP1"))
	//{
	//	TRACE0("loaded IDB_BITMAP1\n");
	//}
	//CString strText("XXX");
	//SetWindowText(strText);

	//// draw the control edges (DrawFrameControl is handy!)
	//if (state & ODS_SELECTED)
	//	pDC->DrawFrameControl(rect, DFC_BUTTON, DFCS_BUTTONPUSH | DFCS_PUSHED);
	//else
	//	pDC->DrawFrameControl(rect, DFC_BUTTON, DFCS_BUTTONPUSH);

	//// Fill the interior color if necessary

	//rect.DeflateRect(CSize(GetSystemMetrics(SM_CXEDGE), GetSystemMetrics(SM_CYEDGE)));
	//if (m_bOverControl)
	//	pDC->FillSolidRect(rect, RGB(255, 255, 0)); // yellow

	//												// Draw the text
	//if (!strText.IsEmpty())
	//{
	//	CSize Extent = pDC->GetTextExtent(strText);
	//	CPoint pt(rect.CenterPoint().x - Extent.cx / 2, rect.CenterPoint().y - Extent.cy / 2);

	//	if (state & ODS_SELECTED)
	//		pt.Offset(1, 1);

	//	int nMode = pDC->SetBkMode(TRANSPARENT);

	//	if (state & ODS_DISABLED)
	//		pDC->DrawState(pt, Extent, strText, DSS_DISABLED, TRUE, 0, (HBRUSH)NULL);
	//	else
	//		pDC->TextOut(pt.x, pt.y, strText);

	//	pDC->SetBkMode(nMode);
	//}
	CDialog::OnDrawItem(IDD_DIALOG1, lpDrawItemStruct);
}

void MyControl::PreSubclassWindow()
{
	CDialog::PreSubclassWindow();

	ModifyStyle(0, BS_OWNERDRAW);	// make the button owner drawn
}

BEGIN_MESSAGE_MAP(MyControl, CDialog)
	ON_WM_PAINT()
END_MESSAGE_MAP()


// MyControl message handlers
void MyControl::OnPaint()
{
	CDialog::OnPaint();
	CDC* pDC = GetDC();
	if (!pDC)
		return;

	CBrush* br = new CBrush();
	br->CreateSolidBrush(RGB(0, 255, 128));
	pDC->FillRect(new CRect(5, 5, 30, 30), br);

}
#include "stdafx.h"
#include <algorithm>

#include "patterns gui, patient list control.h"
#include "patterns, conductor.h"
#include "patterns, fetus.h"
#include "patterns, input adapter.h"
#include "./resource.h"

#include <math.h>
#include <assert.h>

using namespace patterns_gui;

#define TEXT_HEIGHT		16
#define TRACING_HEIGHT	75

BEGIN_MESSAGE_MAP(patient_list_control, CListCtrl)
	ON_WM_CREATE()
	ON_WM_MEASUREITEM_REFLECT()
	ON_WM_SIZE()
	ON_WM_ERASEBKGND()
	ON_WM_PAINT()
END_MESSAGE_MAP()

/*
 =======================================================================================================================
    Constructor and Destructor.
 =======================================================================================================================
 */
patient_list_control::patient_list_control(void)
{
	c = 0;
	m_block = false;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
patient_list_control::~patient_list_control(void)
{
	get_conductor().unsubscribe(this);
	map<string, fetus*>& fetuses = get_conductor().get_fetuses();
	for (map<string, fetus*>::iterator itr = fetuses.begin(); itr != fetuses.end(); ++itr)
	{
		itr->second->unsubscribe(this);
	}
}

/*
 =======================================================================================================================
    Append patient info to the list.
 =======================================================================================================================
 */
void patient_list_control::append(void)
{
	/*~~~~~~~~~~~*/
	LV_ITEM lvitem;
	/*~~~~~~~~~~~*/

	lvitem.iItem = this->GetItemCount();
	lvitem.iSubItem = 0;
	lvitem.mask = LVIF_TEXT;
	lvitem.pszText = (LPSTR) string("").c_str();
	InsertItem(&lvitem);
}

/*
 =======================================================================================================================
    see the comment for the OnPaint method.
 =======================================================================================================================
 */
BOOL patient_list_control::OnEraseBkgnd(CDC *)
{
	return TRUE;
}

/*
 =======================================================================================================================
    Draw given list item to the given device context.
 =======================================================================================================================
 */
void patient_list_control::draw_item(long itemID, CDC *dc)
{
	if (!GetSafeHwnd() || itemID == -1)
	{
		return;
	}

	/*~~~~~~~~*/
	// Get item image and state info
	LV_ITEM lvi;
	/*~~~~~~~~*/

	lvi.mask = LVIF_IMAGE | LVIF_STATE;
	lvi.iItem = itemID;
	lvi.iSubItem = 0;
	lvi.stateMask = (UINT) - 1;
	GetItem(&lvi);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// text and tracing control positions. ;
	RECT r = get_bounds();
	RECT rectText = get_text_bounds(itemID);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	rectText.right = r.right;

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	RECT rectItem = get_bounds(itemID);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	rectItem.right = r.right;

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string *id = (string *) GetItemData(itemID);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (id)
	{
		patterns::input_adapter::patient* p = get_conductor().get_input_adapter().get_patient(*id);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string n = (p == 0)?"":p->get_displayed();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// use default font for the text.
		dc->SelectObject(GetStockObject(DEFAULT_GUI_FONT));

		// To draw the Text set the background mode to Transparent.
		dc->SetBkMode(TRANSPARENT);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// Sauvegarde des anciennes valeurs
		COLORREF crOldTextColor = dc->GetTextColor();
		COLORREF crOldBkColor = dc->GetBkColor();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Traitement couleurs quand l'item est sélectionné.
		if (lvi.state & LVIS_SELECTED)
		{
			dc->SetTextColor(RGB(255, 255, 255));
			dc->SetBkColor(RGB(56, 112, 168));
			dc->FillSolidRect(&rectItem, RGB(56, 112, 168));
		}
		else
		{
			dc->SetTextColor(RGB(0, 0, 0));
			dc->FillSolidRect(&rectItem, crOldBkColor);
		}

		/*~~~*/
		RECT r;
		/*~~~*/

		r.left = 3;
		r.top = rectText.top;
		r.bottom = rectText.bottom;
		r.right = rectText.right;
		dc->DrawText(n.c_str(), (long) strlen(n.c_str()), &r, DT_LEFT | DT_SINGLELINE | DT_END_ELLIPSIS);

		// Draw focus rectangle if item has focus
		if (lvi.state & LVIS_FOCUSED && (GetFocus() == this))
		{
			dc->DrawFocusRect(&rectItem);
		}

		// Restitution des valeurs d'origines
		dc->SetTextColor(crOldTextColor);
		dc->SetBkColor(crOldBkColor);
	}
}

/*
 =======================================================================================================================
    Draw all visible items.
 =======================================================================================================================
 */
void patient_list_control::draw_items(CDC *dc)
{
	/*~~~~~~~~~~~~~~~~~~~*/
	// redraw background.
	CRect r = get_bounds();
	/*~~~~~~~~~~~~~~~~~~~*/

	dc->FillSolidRect(&r, GetSysColor(COLOR_WINDOW));

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// get the bounds of a row in order to draw only visible items.
	CRect i_r = CRect(0, 0, 0, 0);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (GetItemCount() > 0)
	{
		GetItemRect(0, &i_r, LVIR_BOUNDS);
	}

	for (int i = GetTopIndex(); i - GetTopIndex() <= ceil((double) r.Height() / (double) i_r.Height()) && i < GetItemCount(); i++)
	{
		draw_item(i, dc);
	}
}

/*
 =======================================================================================================================
    Override CListCtrl's drawing routine. We do this for several reasons. First, it seems that, whatever we do, we
    can't prevent CListCtrl from erasing the background before calling an draw_items. Also, overriding OnPaint () lets
    us centralize the offscreen drawing mechanism. That, along with overriding OnEraseBkgnd () makes for flickerless
    drawing. We call InvalidateRgn () before creating the CPaintDC because it seems that CListCtrl does not always set
    the update region correctly. Doing this has no noticeable impact on performance and, since we draw offscreen, no
    visible impact.
 =======================================================================================================================
 */
void patient_list_control::OnPaint(void)
{
	InvalidateRgn(0);

	/*~~~~~~~~~~~~~~*/
	// see comment above
	CPaintDC dc(this);
	CDC dcm;
	CBitmap bmt;
	/*~~~~~~~~~~~~~~*/

	dcm.CreateCompatibleDC(&dc);

	/*~~~~~~~~~~~~~~~~~~~*/
	CRect r = get_bounds();
	/*~~~~~~~~~~~~~~~~~~~*/

	bmt.CreateCompatibleBitmap(&dc, r.Width(), r.Height());

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	CBitmap *pBitmapOld = dcm.SelectObject(&bmt);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	draw_items(&dcm);
	BitBlt(dc.m_hDC, 0, 0, r.Width(), r.Height(), dcm.m_hDC, 0, 0, SRCCOPY);
	dcm.SelectObject(pBitmapOld);
}

struct SortingStruct
{
	patterns::input_adapter* adapter;

	SortingStruct() 
	{
		adapter = 0;
	};

	SortingStruct(patterns::input_adapter* a) 
	{
		adapter = a;
	}

	bool operator()(patterns::input_adapter::patient* p1, patterns::input_adapter::patient* p2) const
    {
		return adapter->compare(p1, p2);
	}
};


/*
 =======================================================================================================================
    Rebuild the list through superclass methods. Also sets up the tracings. This is meant to be called by interface
    methods or through subscription every time the list changes.
 =======================================================================================================================
 */
void patient_list_control::build(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~~*/
	bool notify_parent = true;
	/*~~~~~~~~~~~~~~~~~~~~~~*/

	// return if control not initialized yet.
	if (!this->GetSafeHwnd())
	{
		return;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// keep the current selection before removing or adding row.
	string sel = get_selection();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~*/

	patterns::input_adapter & a = get_conductor().get_input_adapter();

	// The list has changed, we must unsubscribe and subscribe again all fetuses.
	map<string, fetus*>& fetuses = get_conductor().get_fetuses();
	for (map<string, fetus*>::iterator itr = fetuses.begin(); itr != fetuses.end(); ++itr)
	{
		itr->second->unsubscribe(this);
		itr->second->subscribe(new subscription_to_patient_list(this));
	}

	// We need to adjust the number first...
	vector<patterns::input_adapter::patient*> list;
	for (map<string, fetus*>::iterator itr = fetuses.begin(); itr != fetuses.end(); ++itr)
	{
		string key = itr->first;

		patterns::input_adapter::patient* p = a.get_patient(key);
		if (p)
		{
			list.insert(list.end(), p);
		}	
	}				
	while (GetItemCount() < (long)list.size())
	{
		append();
	}
	while (GetItemCount() > (long)list.size())
	{
		remove(GetItemCount() - 1);
	}

	// Sort the list
	SortingStruct s(&get_conductor().get_input_adapter());
	std::sort(list.begin(), list.end(), s);

	// Do the real update now
	long i = 0;
	for (vector<patterns::input_adapter::patient*>::iterator itr = list.begin(); itr != list.end(); ++itr)
	{
		string key = (*itr)->get_key();
		set(*itr, i);
		if (key == sel)
		{
			notify_parent = false;
		}
		++i;
	}

		
	select(sel, notify_parent);
}

/*
 =======================================================================================================================
    Deselect currently selected patient if any. See select ().
 =======================================================================================================================
 */
void patient_list_control::deselect(void)
{
	select("");
}

/*
 =======================================================================================================================
    get bounds of the list control.
 =======================================================================================================================
 */
RECT patient_list_control::get_bounds(void)
{
	/*~~~*/
	RECT r;
	/*~~~*/

	GetClientRect(&r);
	return r;
}

/*
 =======================================================================================================================
    get bounds of a given list item index.
 =======================================================================================================================
 */
RECT patient_list_control::get_bounds(long i)
{
	/*~~~*/
	RECT r;
	/*~~~*/

	r.left = r.top = r.bottom = r.right = 0;
	GetItemRect(i, &r, LVIR_BOUNDS);
	return r;
}

/*
 =======================================================================================================================
    Access the currently referenced conductor. See set_conductor ().
 =======================================================================================================================
 */
const conductor &patient_list_control::get_conductor(void) const
{
	return *c;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
conductor &patient_list_control::get_conductor(void)
{
	return *c;
}

/*
 =======================================================================================================================
    Access the unique identifier of the current patient. This returns an empty string if there is no selection. See
    methods deselect (), has_selection () and select (). Implementation note: we do not handle selection ourselves, we
    use the superclass's built-in selection-handling capabilities. Methods get_selection () and select () provide an
    abstraction layer for clients, linking unique identifiers to item-data strings. Methods deselect () and
    has_selection () are syntactic sugar.
 =======================================================================================================================
 */
const string patient_list_control::get_selection(void) const
{
	/*~~~~~~~~~~*/
	string s = "";
	/*~~~~~~~~~~*/

	if (get_selection_index() >= 0)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string *s0 = (string *) GetItemData(get_selection_index());
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (s0)
		{
			s = *s0;
		}
	}

	return s;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
long patient_list_control::get_selection_index(void) const
{
	/*~~~~~~~~*/
	long i = -1;
	/*~~~~~~~~*/

	try
	{
		if (GetItemCount() > 0)
		{
			i = const_cast<patient_list_control *>(this)->GetSelectionMark();
		}
	}

	catch(...)
	{
	}

	return i;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
RECT patient_list_control::get_text_bounds(long i)
{
	/*~~~~~~~~~~~~~~~~~~~*/
	RECT rtext;
	RECT r = get_bounds(i);
	/*~~~~~~~~~~~~~~~~~~~*/

	rtext.top = r.top + 1;
	rtext.left = r.left + 1;
	rtext.right = r.right - 1;
	rtext.bottom = r.top + TEXT_HEIGHT;
	return rtext;
}

/*
 =======================================================================================================================
    Does the list currently have a selected patient? See method get_selection ().
 =======================================================================================================================
 */
bool patient_list_control::has_selection(void) const
{
	return get_selection() != "";
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void patient_list_control::initialize(void)
{
	InsertColumn(0, "", LVCFMT_LEFT, 250);
}

/*
 =======================================================================================================================
    lock list control when changing selection internally. This is used to make sure we don't change the current
    selection and to avoid the refreshing of the parent control.
 =======================================================================================================================
 */
bool patient_list_control::is_locked(void)
{
	return m_block;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void patient_list_control::lock(bool lock)
{
	m_block = lock;
}

/*
 =======================================================================================================================
    Called to get the item height. -- TODO --.
 =======================================================================================================================
 */
void patient_list_control::MeasureItem(LPMEASUREITEMSTRUCT lpmis)
{
	lpmis->itemHeight = TEXT_HEIGHT;
}

/*
 =======================================================================================================================
    modification of the extended style in order to be able to select the row at any places (not just by clicking on the
    text).
 =======================================================================================================================
 */
int patient_list_control::OnCreate(LPCREATESTRUCT s)
{
	if (CListCtrl::OnCreate(s) == -1)
	{
		return -1;
	}

	SetExtendedStyle(GetExtendedStyle() | LVS_EX_FULLROWSELECT);
	return 0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void patient_list_control::OnSize(UINT nType, int cx, int cy)
{
	if (!this->GetSafeHwnd())
	{
		return;
	}

	/*~~~~~~~*/
	// resize column.
	HDITEM hdi;
	/*~~~~~~~*/

	hdi.mask = HDI_WIDTH;
	hdi.cxy = cx;
	GetHeaderCtrl()->SetItem(0, &hdi);
	CListCtrl::OnSize(nType, cx, cy);
}

/*
 =======================================================================================================================
    Redraw all item in the list.
 =======================================================================================================================
 */
void patient_list_control::redraw(void)
{
	RedrawWindow(NULL, NULL, RDW_INVALIDATE);
}

/*
 =======================================================================================================================
    remove given item from the list. Delete string* saved in the item.
 =======================================================================================================================
 */
void patient_list_control::remove(long i0)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string *s = (string *) GetItemData(i0);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (s)
	{
		delete s;
	}

	DeleteItem(i0);
}

/*
 =======================================================================================================================
    remove all item from the list.
 =======================================================================================================================
 */
void patient_list_control::remove_all(void)
{
	deselect();
	while (GetItemCount())
	{
		remove(0);
	}
}


/*
 =======================================================================================================================
    Select patient with the given unique identifier. This replace any previously selected patient. If the given
    identifier does not refer to a known patient, the selection is left empty. See implementation note for method
    get_selection ().
 =======================================================================================================================
 */
void patient_list_control::select(const string &s0, bool notify)
{
	// lock on demand all messages sent to the parent control.
	if (!notify)
	{
		lock();
	}

	/*~~~~~~~~~~*/
	long pos = -1;
	/*~~~~~~~~~~*/

	for (long i = 0; i < GetItemCount(); ++i)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string *s = (string *) GetItemData(i);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (s && *s == s0)
		{
			pos = i;
			break;
		}
	}

	// unselect everyone
	for (long i = 0; i < GetItemCount(); ++i)
	{
		SetItemState(i, 0, LVIS_SELECTED);
		SetItemState(i, 0, LVIS_FOCUSED);
	}

	SetSelectionMark(pos);
	if (s0 != "")
	{
		SetItemState(pos, LVIS_SELECTED, LVIS_SELECTED);
		SetItemState(pos, LVIS_FOCUSED, LVIS_FOCUSED);
	}

	redraw();
	if (!notify)
	{
		lock(false);
	}
}

/*
 =======================================================================================================================
    set patient info to the given list item.
 =======================================================================================================================
 */
void patient_list_control::set(patterns::input_adapter::patient* p, long i0)
{
	if (p == 0)
	{
		assert(0);
		return;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string s = p->get_displayed();
	char *c_s = new char[s.length() + 1];
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	strcpy(c_s, s.c_str());

	/*~~~~~~~~~~~*/
	LV_ITEM lvitem;
	/*~~~~~~~~~~~*/

	if (i0 < GetItemCount())
	{
		string *s = (string *) GetItemData(i0);
		if (s)
		{
			delete s;
		}

		lvitem.iItem = i0;
		lvitem.iSubItem = 0;
		lvitem.mask = LVIF_TEXT;
		lvitem.pszText = (LPSTR) c_s;
		SetItemData(i0, (DWORD) new string(p->get_key()));
		SetItem(&lvitem);
	}

	delete[] c_s;
}

/*
 =======================================================================================================================
    Set referenced conductor. This triggers repopulating the list. We do not own the given instance.
 =======================================================================================================================
 */
void patient_list_control::set_conductor(conductor *c0)
{
	if (c)
	{
		c->unsubscribe(this);
	}

	c = c0;
	get_conductor().subscribe(new subscription_to_patient_list(this));
	build();
}

/*
 =======================================================================================================================
    Receive patient list messages through subscription.
 =======================================================================================================================
 */
void patient_list_control::subscription_to_patient_list::note(message m)
{
	switch (m)
	  {
		case mpatientlist:
			m_patient_list_ctrl->build();
			break;
	  }
}

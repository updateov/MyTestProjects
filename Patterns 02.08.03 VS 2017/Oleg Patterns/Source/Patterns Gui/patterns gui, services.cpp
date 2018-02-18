#include "stdafx.h"

#include "patterns gui, services.h"
#include <winuser.h>

using namespace patterns_gui;


/* Cache for loaded bitmaps. See method get_bitmap(). */
map<string, CBitmap *> services::bloaded;

/*
 =======================================================================================================================
    Create and select the font matching the parameter
 =======================================================================================================================
 */
void services::select_font(int nPointSize, LPCTSTR lpszFaceName, CDC* pDC)
{
	static double coefficient = -1;

	// Calculate coefficient for LARGE font
	if (coefficient == -1)
	{
		coefficient = 1;

		select_font(160, "Arial", pDC);
		int height = pDC->GetTextExtent("0").cy;

		// Do not start to compensate as long as it's not 28 or more
		if (height >= 28)	// Normally, Arial with a weight of 160 will need 
							//		24 pixels to render the 'W' letter at 100% display settings
							//		32 pixels to render the 'W' letter at 125% display settings
							//		36 pixels to render the 'W' letter at 150% display settings
		{
			coefficient = min(1, 24. / height);
		}
	}

	CFont font;

	LOGFONT lf; 
	memset(&lf, 0, sizeof(LOGFONT)); 
	lf.lfHeight = (LONG)(nPointSize * coefficient);  
	lf.lfQuality = CLEARTYPE_QUALITY;
	strcpy_s(lf.lfFaceName, LF_FACESIZE, lpszFaceName); 
	VERIFY(font.CreatePointFontIndirect(&lf, pDC)); 

	pDC->SelectObject(&font);
}

/*
 =======================================================================================================================
    Create and keep new bitmap from existing bitmap. This lets clients create many bitmaps from a single "template"
    bitmap.This is useful from drawing bitmap rectangles.See tracing::create_rectangle_bitmaps(). ns: name of source
    bitmap. nd: name of destination bitmap.If nd == ns, then the source bitmap is simply replaced. r: rectangle to take
    from bitmap ns.
 =======================================================================================================================
 */
void services::create_bitmap_fragment(const string &ns, const string &nd, const CRect &r)
{
	// Retrieve the big bitmap that will be cut in pieces
	CBitmap *s = get_bitmap(ns);

	// Calculate the fragment caracteristics
	BITMAP b;
	s->GetBitmap(&b);
	b.bmWidth = r.Width();
	b.bmHeight = r.Height();
	switch (b.bmBitsPixel)
	{
		case 1:
			b.bmWidthBytes = 2 * ((b.bmWidth + 15) >> 4);
			break;

		case 4:
			b.bmWidthBytes = 2 * ((b.bmWidth + 3) >> 2);
			break;

		case 8:
			b.bmWidthBytes = b.bmWidth + (b.bmWidth & 1);
			break;

		case 15:
		case 16:
			b.bmWidthBytes = 2 * b.bmWidth;
			break;

		case 24:
			b.bmWidthBytes = 3 * b.bmWidth + (3 * b.bmWidth & 1);
			break;

		case 32:
			b.bmWidthBytes = 4 * b.bmWidth;
			break;
	}

	// Create an empty bitmap to hold the fragment
	CBitmap *d = new CBitmap();
	d->CreateBitmapIndirect(&b);

	CDC cd;
	cd.CreateCompatibleDC(0);
	CBitmap* d0 = cd.SelectObject(d);

	CDC cs;
	cs.CreateCompatibleDC(0);
	CBitmap* s0 = cs.SelectObject(s);

	cd.BitBlt(0, 0, r.Width(), r.Height(), &cs, r.left, r.top, SRCCOPY);

	// Restore selection
	cd.SelectObject(d0);
	cs.SelectObject(s0);

	// Store the resulting fragment for later uses
	keep_bitmap(nd, d);
}

/*
 =======================================================================================================================
    Make a string from given Utc time. See span_to_string().
 =======================================================================================================================
 */
string services::date_to_string(const date &d, time_format f)
{
	string r;
	TCHAR sysTime[64];

	// Convert to system time
	SYSTEMTIME t0 = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(d));

	if (f == fnormal || f == fdate)
	{
		GetDateFormat(LOCALE_USER_DEFAULT, 0, &t0, NULL, sysTime, sizeof(sysTime));
		r = sysTime;
		r += (f == fnormal) ? " " : "";
	}

	if (f == fnormal || f == ftime)
	{
		GetTimeFormat(LOCALE_USER_DEFAULT, TIME_FORCE24HOURFORMAT|TIME_NOSECONDS|TIME_NOTIMEMARKER, &t0, NULL, sysTime, sizeof(sysTime));
		r += sysTime;
	}

	return r;
}

/*
 =======================================================================================================================
    Return time of day in seconds for given date. Converts to local time first.
 =======================================================================================================================
 */
long services::date_to_time_of_day(const date &d)
{
	SYSTEMTIME t0 = fetus::convert_to_local(fetus::convert_to_SYSTEMTIME(d));

	return 3600 * t0.wHour + 60 * t0.wMinute + t0.wSecond;
}

/*
 =======================================================================================================================
    Draw masked bitmap from resources. Draw bitmap with given name, using mask with given name, in given device
    context, at given position.If mask is not found, we simply blit the bitmap. c: device context to draw in. n: name
    in resources of bitmap to draw. nmask: name in resources of bitmap to use as a mask for transparency;
    this image is used as an alpha channel, that is, the blacker a pixel is, the more the resulting blend will be
    transparent. x1, y1: position of the upper-left corner of the blended image in coordinates of c. x2, y2: position
    of the lower right corner. k: placement of image when the(x1, y1),(x2, y2) rectangle is too small to hold the
    bitmap and cropping is needed or when tiling bitmaps.Cropping is taken from the {cbottomleft, cbottomright,
    ctopleft, ctopright} set with optional ctile, combined with bitwise Or operator.
 =======================================================================================================================
 */
void services::draw_bitmap(CDC *c, const string &n, const string &nmask, long x1, long y1, long x2, long y2, crop_bitmap k, bool itile)
{
	if (is_bitmap(nmask))
	{
		draw_bitmap_tile(c, get_bitmap(nmask), x1, y1, x2, y2, k, itile, MERGEPAINT);
	}

	if (is_bitmap(n))
	{
		draw_bitmap_tile(c, get_bitmap(n), x1, y1, x2, y2, k, itile, SRCAND);
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void services::draw_bitmap_tile(CDC *c, CBitmap *b, long x1, long y1, long x2, long y2, crop_bitmap k, bool itile, DWORD kraster)
{
	/*~~~~~~~~~~~~*/
	BITMAP bdetails;
	CDC c2;
	long dx = 0;
	long dy = 0;
	long h;
	long hb;
	long w;
	long wb;
	/*~~~~~~~~~~~~*/

	/* Compute offsets and measurements. */
	b->GetBitmap(&bdetails);
	hb = bdetails.bmHeight;
	wb = bdetails.bmWidth;
	if (x2 == LONG_MIN)
	{
		x2 = x1 + wb;
	}

	if (y2 == LONG_MIN)
	{
		y2 = y1 + hb;
	}

	h = y2 - y1;
	w = x2 - x1;
	if (k == ctopright || k == cbottomright || k == ccenter)
	{
		dx = itile ? wb - w % wb : wb - w;
	}

	if (k == cbottomleft || k == cbottomright || k == ccenter)
	{
		dy = itile ? hb - h % hb : hb - h;
	}

	if (k == ccenter)
	{
		dx /= 2;
		dy /= 2;
	}

	/* Draw bitmap, possibly tiling it. */
	c2.CreateCompatibleDC(c);
	c2.SaveDC();
	c2.SelectObject(b);
	if (itile)
	{
		for (long i = 0, n = w / wb + 1; i < n; i++)
		{
			for (long j = 0, m = h / hb + 1; j < m; j++)
			{
				draw_bitmap_within(c, &c2, x1, y1, x2, y2, x1 + i * wb - dx, y1 + j * hb - dy, wb, hb, kraster);
			}
		}
	}
	else
	{
		draw_bitmap_within(c, &c2, x1, y1, x2, y2, x1 - dx, y1 - dy, wb, hb, kraster);
	}

	c2.RestoreDC(-1);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void services::draw_bitmap_within(CDC *cd, CDC *cs, long x1, long y1, long x2, long y2, long xd, long yd, long wb, long hb, DWORD kraster)
{
	if (xd + wb > x1 && yd + hb > y1 && xd < x2 && yd < y2)
	{
		/*~~~~~~~~~*/
		long hd = hb;
		long wd = wb;
		long xs = 0;
		long ys = 0;
		/*~~~~~~~~~*/

		if (xd + wd >= x2)
		{
			wd = x2 - xd;
		}

		if (yd + hd >= y2)
		{
			hd = y2 - yd;
		}

		if (xd < x1)
		{
			xs = x1 - xd;
			wd -= x1 - xd;
			xd = x1;
		}

		if (yd < y1)
		{
			ys = y1 - yd;
			hd -= y1 - yd;
			yd = y1;
		}

		cd->BitBlt(xd, yd, wd, hd, cs, xs, ys, kraster);
	}
}

/*
 =======================================================================================================================
    Get text representation of given event type.
 =======================================================================================================================
 */
string services::event_type_to_string(event::type t)
{
	/*~~~~~*/
	string r;
	/*~~~~~*/
	switch (t)
	  {
		case event::tacceleration:
			r = "Accel";
			break;

		case event::taccelerationni:
			r = "Accel (non interpretable)";
			break;

		case event::tearly:
			r = "Early decel";
			break;

		case event::ttypical:
			r = "Variable decel";
			break;

		case event::tatypical:
			r = "Variable decel";
			break;

		case event::tlate:
			r = "Late decel";
			break;

		case event::tnadeceleration:
			r = "Decel (non associated)";
			break;

		case event::tnideceleration:
			r = "Decel (non interpretable)";
			break;

		case event::tbaseline:
			r = "Baseline";
			break;

		case event::trepaired:
			r = "Repaired";
			break;

		case event::tprolonged:
			r = "Prolonged decel";
			break;

		case event::terror:
			r = "error!";
			break;
	  }

	return r;
}

/*
 =======================================================================================================================
    Forget given name as bitmap. This lets clients "mask" bitmaps from resources which they no longer should use.This
    does not prevent using keep_bitmap().
 =======================================================================================================================
 */
void services::forget_bitmap(const string &n)
{
	map<string, CBitmap *>::iterator itr = bloaded.find(n);
	if (itr != bloaded.end())
	{
		delete itr->second;
		bloaded.erase(itr);
	}
}

/*
 =======================================================================================================================
    Forget all bitmaps
 =======================================================================================================================
 */
void services::forget_all_bitmaps()
{
	for (map<string, CBitmap *>::iterator itr = bloaded.begin(); itr != bloaded.end(); ++itr)
	{
		delete itr->second;
	}
	bloaded.clear();
}


/*
 =======================================================================================================================
    Find and store bitmap with given name from resources. This is garanteed to return a valid bitmap.We return a dump
    object if the bitmap cannot be loaded.If a bitmap cannot be loaded, though, we need to leave the cache intact, as
    is_bitmap() will use it to determine if a bitmap exists. We store null pointers for bitmaps that we cannot find, so
    as to not go to the resources unnecessarily.
 =======================================================================================================================
 */
CBitmap *services::get_bitmap(const string &n)
{
	// Look for it in the loaded bitmap table
	map<string, CBitmap *>::iterator itr = bloaded.find(n);
	if (itr != bloaded.end())
	{
		return itr->second;
	}

	// Not yet loaded, load it from the resources and store it into the loaded table
	CBitmap *r = new CBitmap();
	if (!r->LoadBitmap(n.c_str()))
	{
		// Unable to load it, still remember it and return 0
		delete r;
		r = 0;
	}

    bloaded[n] = r;
	return r;
}

/*
 =======================================================================================================================
    Get given bitmap's bounds. This is not as trivial as it should be in the Mfc.
 =======================================================================================================================
 */
CRect services::get_bitmap_rectangle(const CBitmap &b)
{
	/*~~~~~~*/
	BITMAP b0;
	CRect r;
	/*~~~~~~*/

	if (b.GetSafeHandle() && const_cast<CBitmap &>(b).GetBitmap(&b0))
	{
		r = CRect(0, 0, b0.bmWidth, b0.bmHeight);
	}

	return r;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
CRect services::get_bitmap_rectangle(const string &n)
{
	return get_bitmap_rectangle(*get_bitmap(n));
}

/*
 =======================================================================================================================
    Is given string the name of a known bitmap? This is true if the bitmap can be found in resources, wether it was
    already loaded or not.This is how clients should determine if methods like draw_bitmap() and get_bitmap_rectangle()
    will perform successfully.This is also true if a bitmap with that name was previously added to the store through a
    call to method keep_bitmap().
 =======================================================================================================================
 */
bool services::is_bitmap(const string &n)
{
	return get_bitmap(n) != 0;
}

/*
 =======================================================================================================================
    Keep given image for subsequent use. This stores -- and makes us owner of -- given bitmap for subsequent use
    through is_bitmap() and draw_bitmap().See implementation of method get_bitmap().
 =======================================================================================================================
 */
void services::keep_bitmap(const string &n, CBitmap *x)
{
	forget_bitmap(n);
	bloaded[n] = x;
}

/*
 =======================================================================================================================
    Make shade of given colour. c: colour to make shade of. f: shade, from 100(purer) to 0%(whiter).
 =======================================================================================================================
 */
COLORREF services::make_colour(COLORREF c, long f)
{
	return COLORREF(RGB(BYTE(255 - (255 - GetRValue(c)) * f / 100), BYTE(255 - (255 - GetGValue(c)) * f / 100), BYTE(255 - (255 - GetBValue(c)) * f / 100)));
}

/*
 =======================================================================================================================
    Make string from given time span using given format. See date_to_string().
 =======================================================================================================================
 */
string services::span_to_string(long s, time_format f)
{
	/*~~~~~*/
	string r;
	/*~~~~~*/

	if (f == fminutes)
	{
		/*~~~~~~~*/
		char t[15];
		/*~~~~~~~*/

		sprintf(t, "%ld", s / 60);
		r = t;
	}
	else if (f == fhours)
	{
		/*~~~~~~~*/
		char t[15];
		/*~~~~~~~*/

		sprintf(t, "%ld", s / 3600);
		r = t;
	}
	else
	{
		r = string(CTimeSpan((__time64_t) s).Format("%H:%M"));
	}

	return r;
}

/// Flush all bitmaps
void services::reset()
{
	for (map<string, CBitmap *>::iterator itr = bloaded.begin(); itr != bloaded.end(); ++itr)
	{
		delete itr->second;
	}
	bloaded.clear();
}

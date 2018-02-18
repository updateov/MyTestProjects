#include "stdafx.h"

#include "patterns gui, tracing.h"

#include <math.h>
#include "patterns, conductor.h"
#include "patterns, fetus.h"
#include "patterns, input adapter.h"
#include "patterns gui, services.h"
#include "patterns, samples.h"
#include "patterns, subscription.h"

using namespace patterns_gui;

#ifdef patterns_research

// =====================================================================================================================
//    Draw debug information.
// =====================================================================================================================
void tracing::draw_debug(CDC* c) const
{
	if (idebug)
	{
		CRect r = get_bounds(otracing);

		long x = r.left + 5;
		long y = r.top + 80;

		c->SaveDC();
		c->SetBkMode(TRANSPARENT);

		INT t[] = { 40}; 

		// affichage des touches de débogage.
		c->TabbedTextOut(x, y, "d\tShow/hide this debug help overlay", 1, t, 0);
		y += 24;
		c->TabbedTextOut(x, y, "e\tRecalculate all", 1, t, 0);
		y += 24;
		c->TabbedTextOut(x, y, "ctrl o\tOpen...", 1, t, 0);
		y += 16;
		c->TabbedTextOut(x, y, "s\tSave as...", 1, t, 0);
		y += 24;
		c->TabbedTextOut(x, y, is_visible(wrepair) ? "h\tHide repair percentage in event's information" : "h\tShow repair percentage in event's information", 1, t, 0);
		y += 16;
		c->TabbedTextOut(x, y, is_visible(wceventbars) ? "b\tHide event small bar" : "b\tShow event small bar", 1, t, 0);
		y += 16;
		c->TabbedTextOut(x, y, is_visible(wcontractionpeak) ? "k\tHide contraction peak" : "k\tShow contraction peak", 1, t, 0);
		y += 16;
		c->TabbedTextOut(x, y, is_visible(wparerinfo) ? "j\tHide classification details" : "j\tShow classification details", 1, t, 0);
		y += 16;

		// Affichage des infos sur la sélection courante
		switch (get_selection_type())
		{
			case ccontraction:
				{
					const fetus*  f = get();
					const contraction&	ci = f->get_contraction(get_selection());

					long vs = f->get_up(ci.get_start());
					long vp = f->get_up(ci.get_peak());
					long ve = f->get_up(ci.get_end());

					CString s = "Selected contraction:\tindex=(%d) start=%d peak=%d end=%d height=%d";
					s.Format(CString(s), (long)get_selection(), ci.get_start(), ci.get_peak(), ci.get_end(), vp - min(vs, ve));
					
					c->TabbedTextOut(x, y, s, 1, t, 0);
				}
				break;

			case cevent:
				{
					const event&  ei = get()->get_event(get_selection());
					
					CString s = "Selected event: index=(%d) start=%d peak=%d end=%d contraction=%d";
					s.Format(CString(s), (long)get_selection(), ei.get_start(), ei.get_peak(), ei.get_end(), ei.get_contraction());

					c->TabbedTextOut(x, y, s, 1, t, 0);
				}
				break;
		}

#undef ligne
		c->RestoreDC(-1);
	}
}
#endif

// =====================================================================================================================
//    Respond to keyboard events. Keyboard events are exclusively debug commands.
// =====================================================================================================================
void tracing::OnKeyDown(UINT c, UINT n, UINT k)
{
#ifdef patterns_retrospective

	char t[1000];
	sprintf(t, "c =  %c, k = %c\n", (char) (unsigned char)c, (char) (unsigned char)k);
	TRACE0(t);
	switch (c)
	{
		case 'E':
			get()->compute_now();
			break;

		case 'H':
			show(wrepair, !is_visible(wrepair));
			break;

		case 'K':
			show(wcontractionpeak, !is_visible(wcontractionpeak));
			break;

#ifdef patterns_research

		case 'B':
			toggle(wceventbars);
			break;

		case 'D':
			idebug = !idebug;
			update();
			break;

		case 'J':
			toggle(wparerinfo);
			break;

		case 'S':
			save_file();
			break;
#endif
	}
#endif

	CWnd::OnKeyDown(c, n, k);
}

void tracing::save_file()
{
	CFileDialog fileDlg(FALSE, _T("*.*"), NULL, OFN_HIDEREADONLY | OFN_PATHMUSTEXIST, "XML Files (*.XML)|*.XML|IN Files (*.IN)|*.IN|CPP Files (*.CPP)|*.CPP|", NULL);
	if (fileDlg.DoModal() != IDOK)
	{
		return;
	}

	string filename = fileDlg.GetPathName();
	if (filename.length() == 0)
	{
		return;
	}

	CString e = fileDlg.GetFileExt();
	fetus::format format;
	if (e.CompareNoCase("cpp") == 0)
	{
		format = fetus::fcpp;
	}
	else if (e.CompareNoCase("in") == 0)
	{
		format = fetus::fin;
	}
	else
	{
		format = fetus::fxml;
	}

	CFile*	pFile = new CFile(filename.c_str(), CFile::modeCreate | CFile::modeWrite);
	if (!pFile)
	{
		return;
	}

	string c = get()->export_to_string(format);
	pFile->Write(c.c_str(), (UINT) c.length());
	pFile->Close();
	delete pFile;
}

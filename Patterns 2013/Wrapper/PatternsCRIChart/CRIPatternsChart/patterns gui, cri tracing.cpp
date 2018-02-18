//REVIEW: 27/03/14
#include "stdafx.h"

#include "patterns gui, cri tracing.h"
#include "patterns, criconductor.h"
#include "patterns, crifetus.h"
#include "patterns, cri input adapter.h"
#include "patterns gui, services.h"
#include "patterns, samples.h"
#include "patterns, subscription.h"
#include "patterns gui, double buffer.h"

#ifdef patterns_parer_classification
#include "..\Parer\ParerClassifier.h"
#endif
using namespace patterns_gui;

BEGIN_MESSAGE_MAP(CRITracing, tracing)
	ON_WM_KEYDOWN()
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
END_MESSAGE_MAP()


long CRITracing::s_maxContractionRate(CONTRACTION_RATE_MAX_PER_30_MIN);
long CRITracing::s_contractionRateTrigger(CONTRACTION_THRESHOLD_RATE_PER_30_MIN); // Disable for CALM by default, must be set in the pattern.config file

double CRITracing::s_minLateDecelConfidence(CRI_MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL);
int CRITracing::s_minLateDecelAmount(CRI_MINIMAL_LATE_DECEL_AMOUNT);
int CRITracing::s_minLargeAndLongDecelAmount(CRI_MINIMAL_LARGE_AND_LONG_DECEL_AMOUNT);
int CRITracing::s_minLateAndLargeAndLongDecelAmount(CRI_MINIMAL_LATE_AND_LARGE_AND_LONG_DECEL_AMOUNT);
int CRITracing::s_minLateAndProlongedDecelAmount(CRI_MINIMAL_LATE_AND_PROLONGED_DECEL_AMOUNT);
int CRITracing::s_minProlongedDecelHeight(CRI_MINIMAL_PROLONGED_DECEL_HEIGHT);
int CRITracing::s_minContractionsAmount(CRI_MINIMAL_CONTRACTION_AMOUNT);
int CRITracing::s_minLongContractionsAmount(CRI_MINIMAL_LONG_CONTRACTION_AMOUNT);
int CRITracing::s_minAccelAmount(CRI_MINIMAL_ACCEL_AMOUNT);
double CRITracing::s_minBaselineVariability(CRI_MINIMAL_BASELINE_VARIABILITY);



// =====================================================================================================================
//    Construction and destruction. We need to unsubscribe when being destroyed to ensure that no instance is left with
//    dangling pointers.We go through set() and set_partial() to create and delete the current fetus instances in order
//    to centralize implementation.
// =======================================================================================================================
CRITracing::CRITracing(void)
{
	m_bHover = false;
	m_numOfContractionsInCurWindow = 0;
	m_numOfLongContractionsInCurWindow = 0;
	m_totalContractionsPeakInCurWindow = 0;
	m_numOfAccelsInCurWindow = 0;
	m_numOfLargeAndLongDecelsInCurWindow = 0;
	m_numOfLateDecelsInCurWindow = 0;
	m_numOfDeepProlongedDecelsInCurWindow = 0;

	m_meanBaselineInCurWindow = -1.0;
	m_meanVariabilityInCurWindow = -1.0;

	create_cribitmaps();

}

CRITracing::~CRITracing(void)
{
}



// =====================================================================================================================
//    Create our bitmaps from compound resources. See method create_rectangle_bitmaps() for comments.
// =======================================================================================================================

void CRITracing::create_cribitmaps(void)
{
	if (!services::is_bitmap("ContractilityUnknown"))
	{
		services::create_bitmap_fragment("t_Contractility_Index", "ContractilityUnknown", CRect(0, 0, 100, 40));
		services::create_bitmap_fragment("t_Contractility_Index", "ContractilityNormal", CRect(100, 0, 200, 40));
		services::create_bitmap_fragment("t_Contractility_Index", "ContractilityAlert", CRect(200, 0, 300, 40));
		services::create_bitmap_fragment("t_Contractility_Index", "ContractilityDanger", CRect(300, 0, 400, 40));
		services::forget_bitmap("t_Contractility_Index");
	}

}

// =====================================================================================================================
//    Draw everything into given context and rectangle. First off, we decide if we ask subviews to draw themselves or if
//    we draw ourself. Before we draw anything, we need to make sure that the internal state is up to date.This is why we
//    call update_now().See comments for methods update() and update_now(). The various draw_...() methods encapsulate
//    the different parts of the tracing so that they me be easily moved or reordered. We set the clipping region to make
//    sure we are well-behaved when called directly by clients, outside the normal OnPaint() sequence.
// =======================================================================================================================
void CRITracing::draw(CDC* dc) const
{
	
	dc->SaveDC();

	services::select_font(80, "Arial", dc);

	create_subviews();

	const CRIFetus&  curFetus  = GetFetus();
	bool is_fake_fetus = !curFetus .has_start_date();

	if (ksubviews.empty())
	{
		const CRIFetus& partialFetus = GetPartialFetus();
		CRgn region;
		CRect rect = get_bounds(oview);

		region.CreateRectRgn(rect.left, rect.top, rect.right, rect.bottom);
		region.OffsetRgn(dc->GetViewportOrg());
		dc->SelectClipRgn(&region, RGN_AND);

		update_now();

		dc->FillRect(get_bounds(oview), CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));

		draw_grid(dc);
		draw_background(dc);

		if (!is_fake_fetus)
		{
			draw_tracing(dc);

			switch (get_type())
			{
			case tfhr:
				rbdeletebutton = CRect(0, 0, 0, 0);
				rbacceptbutton = CRect(0, 0, 0, 0);
				draw_events(dc, &curFetus , true);
				draw_events(dc, &partialFetus , false);
				break;

			case tup:
				rbdeletebutton = CRect(0, 0, 0, 0);
				rbacceptbutton = CRect(0, 0, 0, 0);
				draw_contractions(dc, &curFetus , true);
				draw_contractions(dc, &partialFetus , false);
				break;
			}

			draw_baselines(dc);

			if (!is_compressed())
			{
				if (!is_scrolling())
				{
					draw_watermark(dc, "in_review");
				}
				else if (is_late_realtime())
				{
					draw_watermark(dc, "tracing_delayed");
				}
			}

			if (get_type() == tfhr && !is_compressed() && has_selection() && get_selection_type() == cevent && is_visible(winformation) && !is_visible(whideevents))
			{
				draw_event_information(dc, get_selection());
			}
#if defined(OEM_patterns) || defined(patterns_retrospective)
			if (get_type() == tup && !is_compressed() && has_selection() && get_selection_type() == ccontraction && is_visible(winformation))
			{
				draw_contraction_information(dc, get_selection());
			}
#endif
		}
	}
	else
	{
		COLORREF cdate = RGB(127, 127, 127);
		CRect rect;
		string s;

		dc->SetTextColor(cdate);
		dc->SetBkMode(TRANSPARENT);

		GetClientRect(&rect);
		dc->FillRect(rect, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));

		long long t1 = dynamic_cast<CRITracing*>(ksubviews[0])->get_left_in_ms();

		CRect t2 = dynamic_cast<CRITracing*>(ksubviews[0])->get_bounds(otracing);
		double t3 = ksubviews[0]->get_scaling();
		long t4 = ksubviews[0]->get_offset();
		long t5 = get_sample_rate();

		static long long s1;
		static CRect s2;
		static double s3;
		static long s4;
		static long s5;

		s1 = t1;
		s2 = t2;
		s3 = t3;
		s4 = t4;
		s5 = t5;

		for (long i = 0, n = (long)ksubviews.size(); i < n; i++)
		{
			ksubviews[i]->GetWindowRect(&rect);
			ScreenToClient(&rect);
			dc->SetViewportOrg(rect.TopLeft());
			dynamic_cast<CRITracing*>(ksubviews[i])->draw(dc);
			dc->SetViewportOrg(0, 0);

			if (!is_fake_fetus)
			{
	

				DrawExportedAreaMarker(dc, i);

				if (dynamic_cast<CRITracing*>(ksubviews[i])->is_compressed() && ksubviews[i]->get_type() == tcr)
				{	
					s = "PeriCALM CheckList Trend";
					dc->TextOut(rect.left + 3, rect.top - 14, s.c_str());
				}
				else if (dynamic_cast<CRITracing*>(ksubviews[i])->is_compressed() && ksubviews[i]->get_type() == tfhr)
				{
					s = get_compressed_fhr_title();
					dc->TextOut(rect.left + 3, rect.top - 10, s.c_str());

					if (is_visible(patterns_gui::CRITracing::wshowdisclaimer))
					{
						COLORREF previousColor = dc->SetTextColor(RGB(238, 118, 0));
						UINT previousAlign = dc->SetTextAlign(TA_CENTER | TA_BOTTOM);
						dc->TextOut(rect.left + (rect.Width() / 2), rect.top - 10, "--- Demonstration version only ---");
						dc->SetTextColor(previousColor);
						dc->SetTextAlign(previousAlign);
					}
				}
			}
		}

		if (!is_fake_fetus)
		{
			draw_guides(dc);
		}
	}

	CountContractionsInCurWindow();
	CountEventsInCurWindow();

	if (!is_fake_fetus)
	{
		dc->SetViewportOrg(0, 0);
		if (get_type() == tnormal)
		{	
			DrawExportedRangeLines(dc);
			CalculateContractilityPositiveReason(m_contractilityPositiveReason);

			draw_slider(dc);

			draw_contractions_average(dc);

			if (is_visible(patterns_gui::CRITracing::wbaselinevariability))
			{
				draw_baseline_average(dc);
			}

			if (is_visible(patterns_gui::CRITracing::wmontevideo))
			{
				draw_montevideo_units(dc);
			}

			DrawContractilitySummary(dc);

#ifdef patterns_parer_classification
			if (is_visible(patterns_gui::CRITracing::wparerclassification))
			{
				draw_parer_classification(dc);
			}
#endif
		}
	}

	if (!is_fake_fetus || ((m_pFetus != 0) && (m_pFetus->get_key().length() > 0)))
	{
		if ((!is_visible(patterns_gui::CRITracing::whidedisconnected)) && (!is_connected()))
		{
			draw_disconnected(dc);
		}
	}

#ifdef patterns_research
	draw_debug(dc);
#endif

	dc->RestoreDC(-1);
}





// =====================================================================================================================
//    Draw contractions average information. We draw the contractions average based on the expanded lenght.The
//    calculation is simple: number of minutes of the expanded view(shown by the slider) divided by the number of
//    contractions in that view. dc: device context to draw into.
// =======================================================================================================================
void CRITracing::draw_contractions_average(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	const long d = 5;

	ptTopLeft = dynamic_cast<CRITracing*>(ksubviews[2])->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = dynamic_cast<CRITracing*>(ksubviews[subViewsSize - 1])->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	CRect bounds = get_bounds(otracing);
	long sampleRate = get_sample_rate();

	// Set graphics up.
	dc->SaveDC();

	// Determine where we want to draw.
	r1 = rect;
	r1.top = rect.bottom + 10;
	r1.bottom = r1.top + 50;

	if (m_numOfContractionsInCurWindow > 0)
	{
		sprintf(summaryText, "Mean Contraction Interval: Q %.1lf min", m_numOfContractionsInCurWindow > 0 ? get_displayed_length() / (double)(60 * m_numOfContractionsInCurWindow) : 0);
	}
	else
	{
		strcpy(summaryText, "Mean Contraction Interval: none");
	}

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);
	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// Clean graphics up.
	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    Draw montevideo units
// =======================================================================================================================
void CRITracing::draw_montevideo_units(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	const long d = 5;

	ptTopLeft = dynamic_cast<CRITracing*>(ksubviews[2])->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = dynamic_cast<CRITracing*>(ksubviews[subViewsSize - 1])->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	CRect bounds = get_bounds(otracing);

	// Set graphics up.
	dc->SaveDC();

	sprintf(summaryText, "Mean Montevideo Units: %.0f per 10 min", (600 * m_totalContractionsPeakInCurWindow) / (double)get_displayed_length());

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);

	// Determine where we want to draw.
	r1 = rect;

	r1.top = rect.bottom + 10 + 2 * (1 + r2.Height());
	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wbaselinevariability))
	{
		r1.top += 1 + r2.Height();
	}

#ifdef patterns_parer_classification
	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wparerclassification))
	{
		r1.top += 1 + r2.Height();
	}
#endif

	r1.bottom = r1.top + 1 + r2.Height();

	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// Clean graphics up.
	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    draw_baseline_average: show average baseline level and variability in window
// =======================================================================================================================
void CRITracing::draw_baseline_average(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	long ileft;
	long iright;
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	const long d = 5;

	ptTopLeft = dynamic_cast<CRITracing*>(ksubviews[2])->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = dynamic_cast<CRITracing*>(ksubviews[subViewsSize - 1])->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	CRect bounds = get_bounds(otracing);
	iright = get_index_from_x(bounds.right);
	ileft = iright - (get_displayed_length() * get_sample_rate());

	// Set graphics up.
	dc->SaveDC();

	// Get Baseline rate.
	//double mean_baseline = -1.0;
	m_meanBaselineInCurWindow = -1.0;


	// Get Baseline rate.
	//double mean_var = -1.0;

	m_meanVariabilityInCurWindow = -1.0;

	GetFetus().get_mean_baseline(ileft, iright, &m_meanBaselineInCurWindow, &m_meanVariabilityInCurWindow);

	if (m_meanBaselineInCurWindow > 0)
	{
		sprintf(summaryText, "Mean Baseline : %ld bpm", (long)(floor(m_meanBaselineInCurWindow)));
	}
	else
	{
		strcpy(summaryText, "Mean Baseline : N/A");
	}

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);

	// Determine where we want to draw.
	r1 = rect;
	r1.top = rect.bottom + 10 + 1 + r2.Height();
	r1.bottom = r1.top + 1 + r2.Height();

	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	//// baseline var



	r1.top = rect.bottom + 10 + 2 * (1 + r2.Height());
	r1.bottom = r1.top + 1 + r2.Height();
	if (m_meanVariabilityInCurWindow > 0)
	{
		string accelsStr = "Accels";
		if(m_contractilityPositiveReason.m_bAccelRate)
				std::transform(accelsStr.begin(), accelsStr.end(), accelsStr.begin(), toupper);
		string meanBaselineVariabilityStr =  "Mean Baseline Variability";
		if(m_contractilityPositiveReason.m_bMeanBaselineVariability)
			std::transform(meanBaselineVariabilityStr.begin(), meanBaselineVariabilityStr.end(), meanBaselineVariabilityStr.begin(), toupper);

		string roundedVariabilityStr = RoundVariability(m_meanVariabilityInCurWindow);

		sprintf(summaryText, "%s: %s | %s: %ld", meanBaselineVariabilityStr.c_str(), roundedVariabilityStr.c_str(), accelsStr.c_str(), m_numOfAccelsInCurWindow);

	}
	else
	{
		strcpy(summaryText, "Mean Baseline Variability : N/A | Accels: N/A");
	}

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);
	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// Clean graphics up.
	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    draw_baseline_average: show average baseline level and variability in window
// =======================================================================================================================
void CRITracing::DrawContractilitySummary(CDC* dc) const
{
	long subViewsSize = (long)ksubviews.size();
	char summaryText[1000];
	CPoint ptTopLeft;
	CPoint ptBottomRight;
	CRect rect;
	CRect r0;
	CRect r1;
	CRect r2;
	const long d = 5;

	ptTopLeft = dynamic_cast<CRITracing*>(ksubviews[2])->get_bounds(oslider).TopLeft();
	ksubviews[2]->ClientToScreen(&ptTopLeft);
	ptBottomRight = dynamic_cast<CRITracing*>(ksubviews[subViewsSize - 1])->get_bounds(oslider).BottomRight();
	ksubviews[subViewsSize - 1]->ClientToScreen(&ptBottomRight);
	rect = CRect(ptTopLeft, ptBottomRight);
	ScreenToClient(&rect);

	long sampleRate = get_sample_rate();
	CRect bounds = get_bounds(otracing);

	// Set graphics up.
	dc->SaveDC();

	const CRIFetus&  curFetus = GetFetus();

	// Get Baseline rate.
	//double mean_baseline = 10.0;

	// Get Baseline rate.
	//double mean_var = 5.0;

	//sprintf(summaryText, "Contractions: %ld | Accels: %ld | Large Decels: %ld | Late Decels: %ld | Large Contractions: %ld", (long)(15)), (long)(floor(mean_var)), (long)(floor(mean_baseline)), (long)(floor(mean_var)), (long)(floor(mean_baseline)));
	string contractionsStr = "Contractions";
	string longContractionsStr = "Contractions";
	

	if(m_contractilityPositiveReason.m_bContractionRate)
			std::transform(contractionsStr.begin(), contractionsStr.end(), contractionsStr.begin(), toupper);

	if(m_contractilityPositiveReason.m_bLongContractionsRate)
		std::transform(longContractionsStr.begin(), longContractionsStr.end(), longContractionsStr.begin(), toupper);


	sprintf(summaryText, "%s: %ld | %s (>120 sec): %d", contractionsStr.c_str(), m_numOfContractionsInCurWindow, longContractionsStr.c_str(), m_numOfLongContractionsInCurWindow);
	//sprintf(summaryText, "Contractions: %ld | Accels: %ld | Large Decels: %ld",  m_numOfContractionsInCurWindow,  m_numOfAccelsInCurWindow,  m_numOfLargeAndLongDecelsInCurWindow);

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);

	// Determine where we want to draw.
	r1 = rect;
	r1.top = rect.bottom + 10 + 2 * (1 + r2.Height());
	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wbaselinevariability))
	{
		r1.top += 1 + r2.Height();
	}

#ifdef patterns_parer_classification
	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wparerclassification))
	{
		r1.top += 1 + r2.Height();
	}
#endif

	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wmontevideo))
	{
		r1.top += 1 + r2.Height();
	}
		
	r1.bottom = r1.top + 1 + r2.Height();

	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// second line
	r1.top = rect.bottom + 10 + 3 * (1 + r2.Height());

	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wbaselinevariability))
	{
		r1.top += 1 + r2.Height();
	}

#ifdef patterns_parer_classification
	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wparerclassification))
	{
		r1.top += 1 + r2.Height();
	}
#endif

	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wmontevideo))
	{
		r1.top += 1 + r2.Height();
	}
		
	r1.bottom = r1.top + 1 + r2.Height();

	string lateDecelsStr = "Late Decels";
	string largeDecelsStr = "Decels";

	if(m_contractilityPositiveReason.m_bLateDecelRate)
			std::transform(lateDecelsStr.begin(), lateDecelsStr.end(), lateDecelsStr.begin(), toupper);
	if(m_contractilityPositiveReason.m_bLongAndLargeDecelRate)
			std::transform(largeDecelsStr.begin(), largeDecelsStr.end(), largeDecelsStr.begin(), toupper);
	double confidenceToShow = s_minLateDecelConfidence*10 - 5;
	sprintf(summaryText, "%s (Confidence>%.1f/5): %ld | %s (>= 60 x 60): %ld",  lateDecelsStr.c_str(), confidenceToShow, m_numOfLateDecelsInCurWindow, largeDecelsStr.c_str(), m_numOfLargeAndLongDecelsInCurWindow);

	//sprintf(summaryText, "Late Decels: %ld | Large Contractions: %ld",   m_numOfLateDecelsInCurWindow,   m_numOfLongContractionsInCurWindow);

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);
	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);

	// third line
	r1.top = rect.bottom + 10 + 4 * (1 + r2.Height());

	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wbaselinevariability))
	{
		r1.top += 1 + r2.Height();
	}

#ifdef patterns_parer_classification
	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wparerclassification))
	{
		r1.top += 1 + r2.Height();
	}
#endif

	if (get_type() == tnormal && is_visible(patterns_gui::CRITracing::wmontevideo))
	{
		r1.top += 1 + r2.Height();
	}
		
	r1.bottom = r1.top + 1 + r2.Height();

	string prolongedDcelsStr = "Prolonged decels";

	if(m_contractilityPositiveReason.m_bProlongedDecelRate)
			std::transform(prolongedDcelsStr.begin(), prolongedDcelsStr.end(), prolongedDcelsStr.begin(), toupper);

	sprintf(summaryText, "%s (> %dbpm below baseline): %ld", prolongedDcelsStr.c_str(), s_minProlongedDecelHeight, m_numOfDeepProlongedDecelsInCurWindow);
	//sprintf(summaryText, "Prolonged decels (> 20bpm below baseline): %ld", m_numOfDeepProlongedDecelsInCurWindow);

	dc->DrawText(summaryText, &r2, DT_CALCRECT);
	r0 = get_bounds(otracing);
	if (rect.left + r2.Width() + d > r0.right && rect.right <= r0.right)
	{
		r1.left = r0.right - r2.Width() - d;
	}

	if (rect.left < d && rect.left >= r0.left)
	{
		r1.left = d;
	}

	r1.right = r1.left + r2.Width();
	dc->FillRect(r1, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
	dc->DrawText(summaryText, &r1, DT_WORDBREAK);


	// Clean graphics up.
	dc->RestoreDC(-1);
}




// =====================================================================================================================
//    Draw the tracing's grid. We facsimile as much as possible the grids of the us and international paper, both for fhr
//    and up tracings. cbase: base colour depending on the paper type.Other colours are derived from this one. cmajor:
//    colour for minor divisions. cminor: colour for minor divisions. d: interval between minor divisons, in seconds.
//    dif: difference, with respect to i/f, in seconds, between the current fetus' starting date and the last round hour,
//    that is.This, added to i/f, lets us determine major divisions. dy: interval between horizontal(y axis) lines, in
//    units. f: sample rate(frequency). fnumbers: font for numbers on the tracing. idates: index of sample where a date
//    should be displayed.See xnumebrs. [ileft, iright]: interval, in sample indices, of the currently viewed part of the
//    tracing. pmajor, pminor: pens for the cmajor and cminor colours. rect: bounds of the tracing. xnumbers: horizontal
//    positions where vertical units should be displayed.This is computed when drawing vertical lines in order to
//    centralize computing the vertical grid.As drawing itself takes computationally much longer than growing a vector
//    does, so we do not worry about this.
// =======================================================================================================================
void CRITracing::draw_grid(CDC* dc) const
{
	if (!is_grid_visible())
	{
		return;
	}

	// !!!!!!!!!!!!!!!!!! WARNING !!!!!!!!!!!!!!!!!!!
	get_scaling();				// Do not remove, it ensure the data is properly initialize OR ELSE sometime the grid or the top fhr part is badly drawn...

	// !!!!!!!!!!!!!!!!!! WARNING !!!!!!!!!!!!!!!!!!! ;
	// Colors used to draw the grid
	COLORREF cbase = draw_grid_base_colour();
	COLORREF cdate = RGB(127, 127, 127);
	COLORREF cmajor = cbase;
	COLORREF cminor = services::make_colour(cbase, 40);

	// Pens used to draw the grid
	CPen pmajor(PS_SOLID, 0, cmajor);
	CPen pminorh(PS_SOLID, 0, services::make_colour(cminor, draw_grid_vertical_shade()));
	CPen pminorv(PS_SOLID, 0, cminor);
	long lSampleRate = get_sample_rate();
	CRect rect = get_bounds(otracing);

	dc->SaveDC();

	// Horizontal lines for vertical(fhr or up) axis ;
	// Interval between horizontal(y axis) lines, in units.
	long dy = get_type() == tcr ? get_maximum() : (get_type() == tup || get_paper() == pusa) ? 10 : 5;

	if (is_compressed())
	{
		long ybottom = get_y_from_units(rect, get_minimum());
		long ytop = get_y_from_units(rect, get_maximum());

		dc->FillRect(&CRect(rect.left, ytop, rect.right, ybottom), &CBrush(services::make_colour(cbase, 10)));
		dc->SelectObject(&pminorh);
		dc->MoveTo(rect.left, ytop);
		dc->LineTo(rect.right, ytop);
		dc->MoveTo(rect.left, ybottom);
		dc->LineTo(rect.right, ybottom);

		if (get_type() == tcr && is_visible(wcrtracing) && GetContractionRateTrigger() > 0)
		{
			long ythreshold = get_y_from_units(rect, GetContractionRateTrigger());

			dc->SelectObject(&pmajor);
			dc->MoveTo(rect.left, ythreshold);
			dc->LineTo(rect.right, ythreshold);
		}
	}
	else
	{
		int pos = 0;
		for (long i = get_minimum(); i <= get_maximum(); i += dy)
		{
			long yi = get_y_from_units(rect, i);

			bool major =
				(
				(i == get_minimum())
				||	(i == get_maximum())
				||	(get_type() == tup && ((i % 25) == 0))
				||	(get_type() == tfhr && get_paper() == pinternational && (i == 80 || i == 120 || i == 160))
				||	(get_type() == tfhr && get_paper() == pusa && ((i % 60) == 0))
				||	(get_type() == tcr && i == GetContractionRateTrigger())
				);

			dc->SelectObject(major ? &pmajor : &pminorh);

			dc->MoveTo(rect.left, yi);
			dc->LineTo(rect.right, yi);
		}
	}

	long ileft = get_index_from_x(rect.left);
	long iright = get_index_from_x(rect.right);

	if (iright <= ileft)
	{
		iright = ileft + 1;
	}

	// Calculate the interval between vertical lines
	long dstep;

	// Calculate the interval between vertical lines
	long dfreqScale;

	// Calculate the interval between vertical lines
	long dfreqDates;
	double dPixelPerMinute = 60 * (double)(lSampleRate * (rect.right - rect.left)) / (double)(iright - ileft);

	if (!is_compressed())
	{
		if (dPixelPerMinute >= 20)
		{
			dstep = 10;			// Every 10 seconds
			dfreqScale = 18;	// Every 18 steps(3 minutes)
			dfreqDates = 18;	// Every 18 steps(3 minutes)
		}
		else
		{
			dstep = 60;			// Every 60 seconds
			dfreqScale = 10;	// Every 10 steps(10 minutes)
			dfreqDates = 10;	// Every 10 steps(10 minutes)
		}
	}
	else if (dPixelPerMinute >= 2)
	{
		dstep = 1800;			// Every 30 minutes
		dfreqScale = 2;			// Every 4 steps(1 hours)
		dfreqDates = 2;			// Every 4 steps(1 hours)
	}
	else if (dPixelPerMinute >= 1)
	{
		dstep = 3600;			// Every 60 minutes
		dfreqScale = 2;			// Every 2 steps(2 hours)
		dfreqDates = 2;			// Every 2 steps(2 hours)
	}
	else
	{
		dstep = 7200;			// Every 240 minutes
		dfreqScale = 2;			// Every 2 steps(8 hours)
		dfreqDates = 2;			// Every 2 steps(8 hours)
	}

	// Interval between horizontal(y axis) scale numbers, in units.
	long dynumbers = get_type() == tcr ? get_maximum() : get_type() == tfhr && get_paper() == pusa ? 30 : (get_type() == tcr ? 10 : 25);

	// Number of seconds to adjust the start time of the tracing to the previous hour
	long startTimeAdjustment = GetFetus().has_start_date() ? services::date_to_time_of_day(GetFetus().get_start_date()) % 86400 : ileft / lSampleRate;

	// We don't display most of the elements if there is no real tracing
	bool bValidTracing = GetFetus().has_start_date();

	// We don't display the scale if the tracing height is too small to do it properly
	bool bDisplayScale = bValidTracing && (dynumbers * get_bounds(otracing).Height() / (get_maximum() - get_minimum()) > 10);

	if ((get_type() == tcr && !is_visible(wcrtracing)))
	{
		// We don't dislay the scale for tcr when the graph is turned off
		bDisplayScale = false;
	}

	bool bDisplayDates = bValidTracing && get_type() == tfhr;

	if (is_compressed())
	{
		dc->SetBkMode(TRANSPARENT);
	}
	else
	{
		dc->SetBkMode(OPAQUE);
	}
	
	dc->SetTextAlign(TA_CENTER | TA_BASELINE);

	long nRepetition = ((ileft / lSampleRate) + startTimeAdjustment) / dstep;
	while (true)
	{
		long i = ((nRepetition * dstep) - startTimeAdjustment) * lSampleRate;
		if (i > iright)
		{
			break;
		}

		if (i > ileft)
		{
			long x = get_x_from_index(i);

			// Vertical lines
			dc->SelectObject(!bValidTracing || ((nRepetition * dstep) % 60) ? &pminorv : &pmajor);
			dc->MoveTo(x, rect.top);
			dc->LineTo(x, rect.bottom - 1);

			// Vertical scales
			if (bDisplayScale && (nRepetition % dfreqScale == 0))
			{
				dc->SetTextColor(cmajor);

				for (long j = get_minimum(); j <= get_maximum(); j += dynumbers)
				{
					CString t;
					t.Format("%ld", (long)j);
					dc->TextOut(x - 20, get_y_from_units(rect, j) + 3, t);
				}
			}

			// Dates and times
			if (bDisplayDates)
			{
				dc->SetTextColor(cdate);

				if (nRepetition % dfreqDates == 0)
				{
					date d = GetFetus().get_start_date() + (i / lSampleRate);
					dc->TextOut(x, rect.bottom + 14, services::date_to_string(d, is_compressed() ? services::fnormal : (d % 900 == 0 ? services::fnormal : services::ftime)).c_str());
				}

				if ((is_compressed()) && (nRepetition % dfreqDates != 0))
				{
					dc->TextOut(x, rect.bottom + 14, (string("+") + services::span_to_string((nRepetition % dfreqDates) * dstep, dstep < 3600 ? services::fminutes : services::fhours)).c_str());
				}
			}
		}

		// Next
		++nRepetition;
	}

	dc->RestoreDC(-1);
}

// =====================================================================================================================
//    Draw the actual tracing. is all handled by the various get_...() methods below.See method set_type(). Flag
//    tracingConnectValue is set if the last visited point was drawn.If not, then the currently visited point is moved
//    to, not lined to.
// =======================================================================================================================
void CRITracing::draw_tracing(CDC* dc) const
{
	bool draw_all_points = get_type() == tcr || get_displayed_length() < COMPRESS_VIEW_SIZE_SEC;//7200;

	const CRIFetus&  curFetus = GetFetus();

	CRect rect = get_bounds(otracing);

	long minimum = get_minimum();
	long maximum = get_maximum();

	long ileft = max(0, get_index_from_x(rect.left));
	long iright = get_index_from_x(rect.right);

	double xCoef = get_scaling() / get_sample_rate();
	long xOffset = get_offset();
	double yCoef = (rect.Height() - 1) / (double)(maximum - minimum);
	long yOffset = get_minimum();

	// Save the context
	dc->SaveDC();

	// Clip the drawing to the current tracign region
	CRgn region;
	region.CreateRectRgn(rect.left, rect.top, rect.right, rect.bottom);
	region.OffsetRgn(dc->GetViewportOrg());
	dc->SelectClipRgn(&region, RGN_AND);

	// We loop since we may have to do multiple pass (for tfhr we draw mhr & fhr1)
	int drawing_pass_number = 0;
	while (true)
	{
		LONG lastX = 0;
		long lastDrawnContractility = 0;
		long tracingValue = 0;
		long lastValue = 0;
		bool tracingValueIsValid = (get_type() != tcr || is_visible(wcrtracing));
		bool tracingConnectValue = false;

		long stimulationStage2Limit = get_cr_kcrstage() * 60 * get_sample_rate();
		long stimulationThreshold = GetContractionRateTrigger();
		bool stimulationOverThreshold = false;
		long istartOverThreshold;

		bool hyperstimulation_enabled = stimulationThreshold > 0;

		COLORREF color = services::make_colour(drawing_pass_number == 1 ? RGB(0, 0, 255) : RGB(0, 0, 0), drawing_pass_number == 1 || !is_visible(wbaselines) ? 50 : 100);
		CPen pline(PS_SOLID, 0, color);
		dc->SelectObject(&pline);

		// Special case for hyperstimulation, if the most left point is above the threshold, we need to retrieve the
		// start point so that we know for how long it's above the threshold
		if (get_type() == tcr)
		{			
			(const_cast<CRIFetus&>(curFetus)).ComputeContractionRateNow();
			if (hyperstimulation_enabled && curFetus.GetContractionRate(ileft) > stimulationThreshold)
			{
				stimulationOverThreshold = true;
				istartOverThreshold = ileft;
				while (istartOverThreshold >= 0 && curFetus.GetContractionRate(istartOverThreshold) > stimulationThreshold)
				{
					--istartOverThreshold;
				}
			}
		}

		// Do the painting now...
		dc->SelectObject(&pline);

		bool bTryHarderToGetAllPoints = ((double)(iright - ileft) / (double)(rect.right - rect.left)) < 2 * get_sample_rate();

#ifdef patterns_parer_classification
		long classificationValue;
		long lastClassificationStart = ileft;
		long lastClassificationValue;

		bool bClassificationOn = is_visible(patterns_gui::CRITracing::wparerclassification) && (is_visible(patterns_gui::CRITracing::wparerinfo));
		if (bClassificationOn)
		{
			lastClassificationValue = (!is_singleton() || has_debug_keys()) ? patterns_classifier::CParerClassifier::t_undef : curFetus.GetParerClassification(ileft);
		}

#endif
		for (long i = ileft; i <= iright; ++i)
		{
			switch (get_type())
			{
			case tup:
				tracingValue = curFetus.get_up(i);
				tracingValueIsValid = tracingValue < 127; // For UP, no-data is any value >= 127
				break;

			case tfhr:
#ifdef patterns_parer_classification
				if (bClassificationOn)
				{
					classificationValue = (!is_singleton() || has_debug_keys()) ? patterns_classifier::CParerClassifier::t_undef : curFetus.GetParerClassification(i);
				}
#endif
				tracingValue = (drawing_pass_number == 0) ? curFetus.get_fhr(i) : curFetus.get_mhr(i);
				tracingValueIsValid = (tracingValue > 0 && tracingValue < 255); // 0 is not a valid value for HR
				break;

			case tcr:
				tracingValue = curFetus.GetContractionRate(i);
				break;

			default:
				assert(0);
				break;
			}

#ifdef patterns_parer_classification
			if (bClassificationOn && (get_type() == tfhr) && (i == iright || lastClassificationValue != classificationValue))
			{
				if (lastClassificationValue >= 0 && lastClassificationValue <= 4)
				{
					CBrush brush(classification_color(lastClassificationValue));
					CBrush* oldBrush = dc->SelectObject(&brush);

					CPen pen;
					pen.CreatePen(PS_SOLID, 1, classification_color(lastClassificationValue));

					CPen* oldPen = dc->SelectObject(&pen);

					CRect rectangle(get_x_from_index(lastClassificationStart), rect.top, get_x_from_index(i), min(rect.top + 2, rect.bottom));

					dc->Rectangle(rectangle);

					dc->SelectObject(oldBrush);
					dc->SelectObject(oldPen);
				}

				// Remember
				lastClassificationValue = classificationValue;
				lastClassificationStart = i;
			}
#endif
			if (tracingValueIsValid)
			{
				long x = (long)(i * xCoef - xOffset);
				long y;

				if (tracingValue == maximum)
				{
					y = rect.top;
				}
				else if (tracingValue > maximum)
				{
					y = rect.top - 5;
				}
				else if (tracingValue == minimum)
				{
					y = rect.bottom - 1;
				}
				else if (tracingValue < minimum)
				{
					y = rect.bottom + 5;
				}
				else
				{
					y = (long)((rect.bottom - 1) - ((tracingValue - yOffset) * yCoef));
				}

				// We can have more points than pixel (usual case)
				if (draw_all_points 
					|| (!tracingConnectValue)
					|| (i == ileft) 
					|| (i == iright) 
					|| (x >= lastX + 1) 
					|| (abs(lastValue - tracingValue) >= 10))
				{
					if (!tracingConnectValue)
					{
						dc->MoveTo(x, y);
						dc->SetPixel(x, y, color);
					}
					else
					{
						dc->LineTo(x, y);
					}

					// Remember we draw that point
					lastX = x;
					lastValue = tracingValue;
					tracingConnectValue = true;
				}
			}

			tracingConnectValue = tracingConnectValue && tracingValueIsValid;

			// Color for hyperstimultation
			if (hyperstimulation_enabled && get_type() == tcr)
			{				
				
				const Contractility& curContractility = curFetus.GetContractilityByIndex(i);			

				Contractility::ContractilityClassification classification = curContractility.GetClassification();
				string bitmapToDraw = "ContractilityUnknown";
				switch(classification)
				{
				case Contractility::Normal:
					bitmapToDraw = "ContractilityNormal";
					break;
				case Contractility::Alert:
					bitmapToDraw = "ContractilityDanger";
					//bitmapToDraw = "ContractilityAlert";
					break;
				case Contractility::Danger:
					bitmapToDraw = "ContractilityDanger";
					//bitmapToDraw = "ContractilityAlert";
					break;
				default:
					bitmapToDraw = "ContractilityUnknown";
					break;
				}

				long leftBound = curContractility.GetStart();
				long rightBound = curContractility.GetEnd();
				if (lastDrawnContractility < i)
				{
					if (rightBound < i)
					{
						leftBound = lastDrawnContractility;
						rightBound = i;
						bitmapToDraw = "ContractilityUnknown";
					}

					services::draw_bitmap(dc, bitmapToDraw, "", get_x_from_index(leftBound) - 1, rect.top, get_x_from_index(rightBound), rect.bottom, services::ctopleft, true);
					lastDrawnContractility = rightBound;
					CString str;
					str.Format("index = %d, leftBound = %d, rightBound = %d, lastDrawnContractility = %d\r\n", i, leftBound, rightBound, lastDrawnContractility);						

				}
			}
		}
		
		if (get_type() == tfhr && get_display_mhr() && drawing_pass_number < 1)
		{
			drawing_pass_number = 1;
			continue;
		}

		// We're done, leave now
		break;
	}

	dc->RestoreDC(-1);
}


void CRITracing::CountContractionsInCurWindow(void) const
{
	CRect bounds = get_bounds(otracing);
	long sampleRate = get_sample_rate();
	long iright = get_index_from_x(bounds.right);
	long ileft = iright - (get_displayed_length() * sampleRate);

	m_numOfContractionsInCurWindow = 0;
	m_totalContractionsPeakInCurWindow = 0;
	m_numOfLongContractionsInCurWindow = 0;

	const CRIFetus&  curFetus = GetFetus();
	long contractionCount = curFetus.GetContractionsCount();
	for (long i = 0; i < contractionCount; ++i)
	{
		const contraction&	curContraction = curFetus.get_contraction(i);
		if (!curContraction.is_strike_out())
		{
			long peakTime = curContraction.get_peak();
			if ((peakTime * get_sample_rate()) > (ileft * curFetus.get_up_sample_rate()) && (peakTime * get_sample_rate()) <= (iright * curFetus.get_up_sample_rate()))
			{
				++m_numOfContractionsInCurWindow;
				m_totalContractionsPeakInCurWindow += curFetus.get_height(curContraction);
				if (curContraction.get_end() - curContraction.get_start() > 120)
					++m_numOfLongContractionsInCurWindow;
			}
		}
	}
}

void CRITracing::CountEventsInCurWindow(void) const
{
	CRect bounds = get_bounds(otracing);
	long sampleRate = get_sample_rate();
	long iright = get_index_from_x(bounds.right);
	long ileft = iright - (get_displayed_length() * sampleRate);
	
	m_numOfLateDecelsInCurWindow = 0;
	m_numOfDeepProlongedDecelsInCurWindow = 0;
	m_numOfAccelsInCurWindow = 0;
	m_numOfLargeAndLongDecelsInCurWindow = 0;

	const CRIFetus&  curFetus = GetFetus();
	long eventsCount = curFetus.GetEventsCount();
	for (long i = 0; i < eventsCount; ++i)
	{
		const event& curEvent = curFetus.get_event(i);
		if (!curEvent.is_strike_out())
		{
			long meanTime = (curEvent.get_end() + curEvent.get_start()) / 2 ;
			if ((meanTime * get_sample_rate()) > (ileft * curFetus.get_hr_sample_rate()) && (meanTime * get_sample_rate()) <= (iright * curFetus.get_hr_sample_rate()))
			{
				if (curEvent.get_type() == event::tacceleration || curEvent.is_acceleration())
					++m_numOfAccelsInCurWindow;
				else if (curEvent.get_type() == event::tlate && curEvent.get_confidence() >= s_minLateDecelConfidence)
					++m_numOfLateDecelsInCurWindow;
				else if (curEvent.get_type() == event::tprolonged)
				{
					if (curEvent.is_late() && curEvent.get_confidence() >= s_minLateDecelConfidence)
						++m_numOfLateDecelsInCurWindow;
					else if (curEvent.get_height() > s_minProlongedDecelHeight)
						++m_numOfDeepProlongedDecelsInCurWindow;
				}
				else if (curEvent.is_large_and_long())
					++m_numOfLargeAndLongDecelsInCurWindow;
			}
		}
	}
}

CRITracing* CRITracing::GetCRIParent()
{
	return dynamic_cast<CRITracing*>(get_parent());
}
CRITracing* CRITracing::GetCRIParent() const
{
	tracing* parent = const_cast<CRITracing*>(this)->get_parent();
	return dynamic_cast<CRITracing*>(parent);
}
// =====================================================================================================================
//    Access the fetus instance for the underlying data. If we have subscribed to a parent view, we route the call to the
//    parent view's data.
// =======================================================================================================================
const CRIFetus& CRITracing::GetFetus(void) const
{
	if (!has_parent() && !m_pFetus)
	{
		set(new CRIFetus);
	}
	
	return has_parent() ? GetCRIParent()->GetFetus() : *dynamic_cast<CRIFetus*>(m_pFetus);
}


CRIFetus& CRITracing::GetFetus(void)
{
	if (!has_parent() && !m_pFetus)
	{
		set(new CRIFetus);
	}
	
	return has_parent() ? GetCRIParent()->GetFetus() : *dynamic_cast<CRIFetus*>(m_pFetus);
}

// =====================================================================================================================
//    Access vertical bounds for current type and paper.
// =======================================================================================================================
long CRITracing::get_maximum(void) const
{
	switch (get_type())
	{
	case tcr:
		return GetMaxContractionRate();

	case tup:
		return 100;

	case tfhr:
	default:
		return get_paper() == pusa ? 240 : 210;
	}
}


// =====================================================================================================================
//    Access the fetus instance for the underlying data. If we have subscribed to a parent view, we route the call to the
//    parent view's data.
// =======================================================================================================================
const CRIFetus& CRITracing::GetPartialFetus(void) const
{
	if (!has_parent() && !fpartial)
	{
		set_partial(new CRIFetus);
	}
	
	return has_parent() ? GetCRIParent()->GetPartialFetus() :  *dynamic_cast<CRIFetus*>(fpartial);;
}

CRIFetus& CRITracing::GetPartialFetus(void)
{
	if (!has_parent() && !fpartial)
	{
		set_partial(new CRIFetus);
	}

	
	return has_parent() ? GetCRIParent()->GetPartialFetus() : *dynamic_cast<CRIFetus*>(fpartial);
}

// =====================================================================================================================
//    Respond to mouse-down event. We redirect messages to subviews as needed.Other mouse-message- handling methods need
//    not bother as SetCapture() ensures that messages go directly to the appropriate window.See OnMouseMove (). We do
//    route mouse clicks that fall in between compressed subviews, so that the compressed view appears to the user as an
//    integrated, unified component.
// =======================================================================================================================
void CRITracing::OnLButtonDown(UINT k, CPoint p0)
{
	m_bHover = false;
	tracing::OnLButtonDown(k, p0);
}

// =====================================================================================================================
//    Respond to mouse-up event. See OnMouseMove().We don't bother with view types other than Fhr or Up because execution
//    should never this method for the parent tracing in composite view types.
// =======================================================================================================================
void CRITracing::OnLButtonUp(UINT k, CPoint pt)
{
	m_bHover = false;
	tracing::OnLButtonUp(k, pt);
}


// =====================================================================================================================
//    Select an object. If we are a composite view, that is, if we have subviews, we redirect the selection appropriately
//    to our subviews.If we are a subview, we let our parent know selection has changed so that it may let our siblings
//    know what's going on.If we are a non- composite view, we simply do our thing and update the view. The
//    select_low_level() method does the actual job for an instance, while the select() method redirects traffic.
//    Semantics of iselection and tselection is discussed in the class comment.See methods deselect(), get_selection(),
//    get_selection_type() and has_selection().
// =======================================================================================================================
void CRITracing::select(selectable t0, long i0, bool hover)
{
	m_bHover = hover;
	if (i0 < 0)
	{
		t0 = cnone;
	}

	if (!hover && (t0 != cnone) && (tselection == t0) && (iselection == i0))
	{
		deselect();
		return;
	}

	if (is_subview() && kparent != 0)
	{
		get_parent_root()->select(t0, i0);
	}
	else
	{
		if(!m_bHover)
			reset_guides();
		if (has_subviews())
		{
			for (long i = 0, n = (long)ksubviews.size(); i < n; i++)
			{
				dynamic_cast<CRITracing*>(ksubviews[i])->select_low_level(t0, i0);
			}
		}

		select_low_level(t0, i0);
	}
}


// =====================================================================================================================
//    Update guides for given cursor position. We set guides to the start and end of the object under the cursor.
// =======================================================================================================================
void CRITracing::update_guides(CPoint pt)
{
	long a1 = 0;
	long a2 = 0;
	long i = -1;
	selectable sel = cnone;
	if (has_subviews())
	{
		switch (find_subview(pt))
		{
		case 0:
			ClientToScreen(&pt);
			ksubviews[0]->ScreenToClient(&pt);
			i = dynamic_cast<CRITracing*>(ksubviews[0])->find_index(pt);
			if (i >= 0 && ((get_selection_type() == cevent && i == get_selection()) || dynamic_cast<CRITracing*>(ksubviews[0])->find(pt) == oprimary))
			{
				a1 = get()->get_event(i).get_start();
				a2 = get()->get_event(i).get_end();
				sel = cevent;
			}
			break;

		case 1:
			ClientToScreen(&pt);
			dynamic_cast<CRITracing*>(ksubviews[1])->ScreenToClient(&pt);
			i = dynamic_cast<CRITracing*>(ksubviews[1])->find_index(pt);
			if (i >= 0 && ((get_selection_type() == ccontraction && i == get_selection()) || dynamic_cast<CRITracing*>(ksubviews[1])->find(pt) == oprimary))
			{
				a1 = get()->get_hr_sample_rate() * get()->get_contraction(i).get_start() / get()->get_up_sample_rate();
				a2 = get()->get_hr_sample_rate() * get()->get_contraction(i).get_end() / get()->get_up_sample_rate();				
				sel = ccontraction;				
			}
			break;
		}
	}
	else if (!is_compressed())
	{
		i = find_index(pt);
		if (i >= 0 && (has_selection() || find(pt) == oprimary))
		{
			switch (get_type())
			{
			case tfhr:
				a1 = get()->get_event(i).get_start();
				a2 = get()->get_event(i).get_end();
				break;

			case tup:
				a1 = get()->get_contraction(i).get_start();
				a2 = get()->get_contraction(i).get_end();
				break;
			}
		}
	}
	
	//if(q = qidle && has_selection() && sel == cnone && find(pt) == oprimary) //find(qp) != oslider && is_subview() && is_compressed())
	if(sel == cnone && m_bHover)
	{
		deselect();
	}

	if (a1 != g1 || a2 != g2)
	{
		g1 = a1;
		g2 = a2;
		if(sel != cnone && i >=0)
		{
			select(sel, i, true);
		}
		update();

	}
}


// =====================================================================================================================
//    Update internal state for current data and presentation. This computes whatever we need in order to perform draw
//    operations our whenever we need to know metrics information.See discussion for update...() methods. When updating,
//    we scroll as needed in order to see the last sample.
// =======================================================================================================================
void CRITracing::update_now(void) const
{
	if (idataneedsupdate)
	{
		idataneedsupdate = false;
	}

	if (ineedsupdate)
	{
		ineedsupdate = false;
		switch (get_scaling_mode())
		{
		case sfit:
			priv_set_scaling((double)get_sample_rate() * get_bounds(otracing).Width() / (double)get_number());
			iscrolling = true;
			break;

		case sfree:
		case spaper:
			if (get_scaling_mode() == spaper && has_subviews())
			{
				create_subviews();
				if (ksubviews.size() > 0)
				{
					priv_set_scaling(dynamic_cast<CRITracing*>(ksubviews[0])->m_scaling);
				}
			}
			else if (get_scaling_mode() == spaper)
			{
				priv_set_scaling((double)get_bounds(otracing).Height() / (double)(20 * get_paper_height()));
			}

			if (!iscrolling && get()->has_start_date() && (get_index_from_x(get_bounds(otracing).right) >= (get_number() - (4 * get_sample_rate()))))
			{
				const_cast<patterns_gui::CRITracing*>(this)->scroll(true);
				
			}

			if (is_scrolling())
			{
				priv_set_offset((long)(m_scaling * get_number() / get_sample_rate() - get_bounds(otracing).Width()));
			}
			break;
		}
	}
}

void CRITracing::CalculateContractilityPositiveReason(ContractilityPositiveReason& reason) const
{	
	reason.Reset();
	if(m_numOfContractionsInCurWindow >= s_minContractionsAmount)
	{
			reason.m_bContractionRate = true;        
	}

	if(m_numOfLongContractionsInCurWindow >= s_minLongContractionsAmount)
	{
			reason.m_bLongContractionsRate = true;   
	}

	if(m_numOfAccelsInCurWindow <= s_minAccelAmount && m_meanVariabilityInCurWindow > 0 && m_meanVariabilityInCurWindow < s_minBaselineVariability)
	{
			reason.m_bAccelRate = true;
			reason.m_bMeanBaselineVariability = true;
	}

	if (m_numOfLateDecelsInCurWindow >= s_minLateDecelAmount)
			reason.m_bLateDecelRate = true;

	if(m_numOfLateDecelsInCurWindow > 0 && m_numOfDeepProlongedDecelsInCurWindow > 0 && 
			m_numOfLateDecelsInCurWindow + m_numOfDeepProlongedDecelsInCurWindow >= s_minLateAndProlongedDecelAmount)
	{
			reason.m_bLateDecelRate = true;
			reason.m_bProlongedDecelRate = true;
	}

	if(m_numOfLateDecelsInCurWindow > 0 && m_numOfLargeAndLongDecelsInCurWindow  > 0 &&
			m_numOfLateDecelsInCurWindow + m_numOfLargeAndLongDecelsInCurWindow >= s_minLateAndLargeAndLongDecelAmount)
	{
			reason.m_bLateDecelRate = true;
			reason.m_bLongAndLargeDecelRate = true;
	}

	if(m_numOfLargeAndLongDecelsInCurWindow >= s_minLargeAndLongDecelAmount)
	{
			reason.m_bLongAndLargeDecelRate = true;
	}
}

string CRITracing::RoundVariability(double variability) const
{
	string roundedVariabilityString;	
	double roundedVariability = variability;	
	char buf[1000];
	sprintf(buf, "%.6f", (double)variability);
	string variabilityStr = buf;		
	std::size_t dotPos = variabilityStr.find('.');
	if (dotPos !=std::string::npos && variabilityStr.length() >= dotPos + 2)
	{
		roundedVariabilityString = variabilityStr.substr(0, dotPos + 2); 
		roundedVariability = CRIConductor::to_real(roundedVariabilityString);
		m_meanVariabilityInCurWindow =  roundedVariability;
	}
	else
	{
		char text[1000];		
		printf(text, "%.1f", variability);
		roundedVariabilityString = text;
	}
	return roundedVariabilityString;	

}

double CRITracing::CalcContractionsAverage(long iLeft, long iRight) const
{
	long numOfContractionsInCurWindow = 0;


	const CRIFetus&  curFetus = GetFetus();
	long contractionCount = curFetus.GetContractionsCount();
	for (long i = 0; i < contractionCount; ++i)
	{
		const contraction&	curContraction = curFetus.get_contraction(i);
		if (!curContraction.is_strike_out())
		{
			long peakTime = curContraction.get_peak();
			if ((peakTime * get_sample_rate()) > (iLeft * curFetus.get_up_sample_rate()) && (peakTime * get_sample_rate()) <= (iRight * curFetus.get_up_sample_rate()))
			{
				++numOfContractionsInCurWindow;
	
			}
		}
	}
	double meanContractions = -1.0;
	if (numOfContractionsInCurWindow > 0)
	{
		meanContractions = get_displayed_length() / (double)(60 * numOfContractionsInCurWindow);;
	}
	return meanContractions;
}


void CRITracing::GetExportDlgCalculatedEntries(long iLeft, long iRight, CString& meanContructionsStr, CString& meanBaselineStr, CString& meanBaselineVariabilityStr, CString& montevideoStr)
{
	double meanContructions = -1.0;
	double meanBaseline = -1.0;
	double meanBaselineVariability = -1.0;
	double montevideo = -1.0;

	meanContructions = CalcContractionsAverage(iLeft, iRight);
	GetFetus().get_mean_baseline(iLeft, iRight, &meanBaseline, &meanBaselineVariability);
	montevideo = CalculateMontevideo(iLeft, iRight);

	meanContructionsStr.Format("%.1lf", meanContructions);
	meanBaselineStr.Format("%ld", meanBaseline);
	string roundedVariabilityStr = RoundVariability(meanBaselineVariability);
	meanBaselineVariabilityStr.Format("%s", roundedVariabilityStr.c_str());
	montevideoStr.Format("%.0f", montevideo);
}

void CRITracing::GetExportDlgCalculatedEntriesEx(long iLeft, long iRight, double& meanContructions, double& meanBaseline, double& meanBaselineVariability, double& montevideo)
{
	meanContructions = -1.0;
	meanBaseline = -1.0;
	meanBaselineVariability = -1.0;
	montevideo = -1.0;

	meanContructions = CalcContractionsAverage(iLeft, iRight);
	GetFetus().get_mean_baseline(iLeft, iRight, &meanBaseline, &meanBaselineVariability);
	montevideo = CalculateMontevideo(iLeft, iRight);
}
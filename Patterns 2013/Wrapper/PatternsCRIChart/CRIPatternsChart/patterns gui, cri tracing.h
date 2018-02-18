#pragma once

#include "patterns, crifetus.h"
#include "patterns, cri input adapter.h"
#include "patterns, subscription.h"
#include "patterns gui, tracing.h"
#include <vector>

using namespace std;
using namespace patterns;

namespace patterns_gui
{

	const int CONTRACTION_RATE_MAX_PER_30_MIN = 30;
	const int CONTRACTION_THRESHOLD_RATE_PER_30_MIN = 15;


	const double CRI_MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL = 0.7;
	const int CRI_MINIMAL_LATE_DECEL_AMOUNT = 2;
	const int CRI_MINIMAL_LARGE_AND_LONG_DECEL_AMOUNT = 3;
	const int CRI_MINIMAL_LATE_AND_LARGE_AND_LONG_DECEL_AMOUNT = 2;
	const int CRI_MINIMAL_LATE_AND_PROLONGED_DECEL_AMOUNT = 2;
	const int CRI_MINIMAL_PROLONGED_DECEL_HEIGHT = 20;
	const int CRI_MINIMAL_CONTRACTION_AMOUNT = 16;
	const int CRI_MINIMAL_LONG_CONTRACTION_AMOUNT = 2;
	const int CRI_MINIMAL_ACCEL_AMOUNT = 0;
	const double CRI_MINIMAL_BASELINE_VARIABILITY = 6.0;

	class ContractilityPositiveReason
	{
	public:

		ContractilityPositiveReason()
		{
			Reset();
		}

		void Reset()
		{
			m_bMeanBaselineVariability = false;
			m_bAccelRate = false;
			m_bLateDecelRate = false;
			m_bProlongedDecelRate = false;
			m_bLongAndLargeDecelRate = false;
			m_bContractionRate = false;
			m_bLongContractionsRate = false;
		}

		bool m_bMeanBaselineVariability;
		bool m_bAccelRate; 
		bool m_bLateDecelRate;
		bool m_bProlongedDecelRate;
		bool m_bLongAndLargeDecelRate;
		bool m_bContractionRate;
		bool m_bLongContractionsRate;
	};
	/*
	=======================================================================================================================
	Control for displaying and exploring tracings. 
	=======================================================================================================================
	*/
	class CRITracing : public tracing
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		static void create_cribitmaps(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		DECLARE_MESSAGE_MAP()

		mutable long m_numOfContractionsInCurWindow;
		mutable long m_totalContractionsPeakInCurWindow;
		mutable long m_numOfLongContractionsInCurWindow;

		mutable long m_numOfLateDecelsInCurWindow;
		mutable long m_numOfDeepProlongedDecelsInCurWindow;
		mutable long m_numOfAccelsInCurWindow;
		mutable long m_numOfLargeAndLongDecelsInCurWindow;
		
		mutable double m_meanBaselineInCurWindow;
		mutable double m_meanVariabilityInCurWindow;

		mutable ContractilityPositiveReason m_contractilityPositiveReason;

		virtual void draw(CDC *) const;
		virtual void draw_contractions_average(CDC *) const;
		virtual void draw_montevideo_units(CDC *) const;
		virtual void draw_baseline_average(CDC *) const;
		virtual void DrawContractilitySummary(CDC *) const;

		void CalculateContractilityPositiveReason(ContractilityPositiveReason& reason) const;

		virtual void draw_grid(CDC *) const;
		virtual void draw_tracing(CDC *) const;
		virtual long get_maximum(void) const;
		virtual CRITracing* GetCRIParent();
		virtual CRITracing* GetCRIParent() const;

		afx_msg void OnLButtonDown(UINT, CPoint);
		afx_msg void OnLButtonUp(UINT, CPoint);
		virtual void update_guides(CPoint);
		virtual void update_now(void) const;
		
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		virtual bool IsWCRVisible() const
		{return true;}
	private:
		void CountContractionsInCurWindow(void) const;
		void CountEventsInCurWindow(void) const;

	public:
		CRITracing(void);
		virtual ~CRITracing(void);

		virtual tracing* NewTracing() const
		{return new CRITracing();}
		virtual CRIFetus* NewFetus() const
		{return new CRIFetus;}

		
		virtual const CRIFetus &GetFetus(void) const;		
		virtual CRIFetus &GetFetus(void);
		virtual const CRIFetus &GetPartialFetus(void) const;
		virtual CRIFetus &GetPartialFetus(void);

		virtual void select(selectable t0, long i0)
		{
			select(t0, i0, false);
		}
		virtual void select(selectable t0, long i0, bool hover);

		//cri algorithm configuration settings
		static long GetMaxContractionRate() {return s_maxContractionRate;}
		static void SetMaxContractionRate(int value) {s_maxContractionRate= value;}

		static long GetContractionRateTrigger() {return s_contractionRateTrigger;}
		static void SetContractionRateTrigger(int value) {s_contractionRateTrigger = value;}

		static double GetMinLateDecelConfidence() {return s_minLateDecelConfidence;}
		static void SetMinLateDecelConfidence(double value) {s_minLateDecelConfidence= value;}

		static long GetMinLateDecelAmount() {return s_minLateDecelAmount;}
		static void SetMinLateDecelAmount(int value) {s_minLateDecelAmount = value;}

		static long GetMinLargeAndLongDecelAmount() {return s_minLargeAndLongDecelAmount;}
		static void SetMinLargeAndLongDecelAmount(int value) {s_minLargeAndLongDecelAmount = value;}

		static long GetLateAndLargeAndLongDecelAmount() {return s_minLateAndLargeAndLongDecelAmount;}
		static void SetLateAndLargeAndLongDecelAmount(int value) {s_minLateAndLargeAndLongDecelAmount = value;}

		static long GetLateAndProlongedDecelAmount() {return s_minLateAndProlongedDecelAmount;}
		static void SetLateAndProlongedDecelAmount(int value) {s_minLateAndProlongedDecelAmount = value;}

		static long GetProlongedDecelHeight() {return s_minProlongedDecelHeight;}
		static void SetProlongedDecelHeight(int value) {s_minProlongedDecelHeight = value;}

		static long GetContractionsAmount() {return s_minContractionsAmount;}
		static void SetContractionsAmount(int value) {s_minContractionsAmount = value;}

		static long GetMinLongContractionsAmount() {return s_minLongContractionsAmount;}
		static void SetMinLongContractionsAmount(int value) {s_minLongContractionsAmount = value;}

		static long GetMinAccelAmount() {return s_minAccelAmount;}
		static void SetMinAccelAmount(int value) {s_minAccelAmount = value;}

		static double GetMinBaselineVariability() {return s_minBaselineVariability;}
		static void SetMinBaselineVariability(double value) {s_minBaselineVariability = value;} 
	public:
		virtual void GetExportDlgCalculatedEntries(long iLeft, long iRight, CString& meanContructionsStr, CString& meanBaselineStr, CString& meanBaselineVariabilityStr, CString& montevideoStr);
		virtual void GetExportDlgCalculatedEntriesEx(long iLeft, long iRight, double& meanContructions, double& meanBaseline, double& meanBaselineVariability, double& montevideo);
	protected:
		virtual double CalcContractionsAverage(long iLeft, long iRight) const;
	private:
		string RoundVariability(double variability) const;
	protected:

		static long s_maxContractionRate;
		static long s_contractionRateTrigger;

		static double s_minLateDecelConfidence;
		static int s_minLateDecelAmount;
		static int s_minLargeAndLongDecelAmount;
		static int s_minLateAndLargeAndLongDecelAmount;
		static int s_minLateAndProlongedDecelAmount;
		static int s_minProlongedDecelHeight;
		static int s_minContractionsAmount;
		static int s_minLongContractionsAmount;
		static int s_minAccelAmount;
		static double s_minBaselineVariability;


		private:
			bool m_bHover;

	};
}
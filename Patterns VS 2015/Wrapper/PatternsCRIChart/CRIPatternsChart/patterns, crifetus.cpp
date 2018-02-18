//REVIEW: 27/03/14
#include "stdafx.h"

#include "patterns, crifetus.h"
#include "patterns, criconductor.h"

#include <fstream> 

namespace patterns
{
	long CRIFetus::s_contractionRateWindowSize(CRI_CONTRACTILITY_WINDOW_SIZE);
	int CRIFetus::s_qualificationWindowSize(CRI_STATE_QUALIFICATION_WINDOW_SIZE);
	int CRIFetus::s_minimalAmountOfDataInWindow(CRI_MINIMAL_AMOUNT_OF_DATA_IN_WINDOW);

	// =================================================================================================================
	//    Construction and destruction.
	// ===================================================================================================================
	CRIFetus::CRIFetus(void)
	{		
	}

	CRIFetus::CRIFetus(const CRIFetus& curFetus)
	{
		*this = curFetus;
	}

	CRIFetus::~CRIFetus(void)
	{
	}


	// =================================================================================================================
	//    Contractility index methods
	// ===================================================================================================================
	const vector<Contractility>& CRIFetus::GetContractilities()
	{
		return m_CRICalculation.GetContractilities();
	}

	const Contractility& CRIFetus::GetContractilityByIndex(long index) const
	{
		return m_CRICalculation.GetContractilityByIndex(index);
	}

	void CRIFetus::AppendContractilities(const vector<Contractility>& contractilities)
	{
		if (contractilities.size() == 0)
			return;

		long initNumOfContractilities = m_CRICalculation.GetNumberOfContractilities();
		if (initNumOfContractilities > 0)
			suspend_notifications(true);

		m_CRICalculation.AppendContractilities(contractilities);
		if (initNumOfContractilities > 0)
			suspend_notifications(false);
		else
			note(subscription::mfetus);
	}

	void CRIFetus::MergeContractilities(const vector<Contractility>& contractilities)
	{
		if (contractilities.size() == 0)
			return;

		long initNumOfContractilities = m_CRICalculation.GetNumberOfContractilities();
		if (initNumOfContractilities > 0)
			suspend_notifications(true);

		m_CRICalculation.MergeContractilities(contractilities);
		if (initNumOfContractilities > 0)
			suspend_notifications(false);
		else
			note(subscription::mfetus);
	}



	bool CRIFetus::ContractilitiesEqual(const CRIFetus& a, bool iincludeoverlap/* = false*/) const
	{
		return m_CRICalculation.ContractilitiesEqual(a.m_CRICalculation, iincludeoverlap);
	}


	// =================================================================================================================
	//    Detect contraction rate immediately.
	// ===================================================================================================================
	void CRIFetus::ComputeContractionRateNow(void)
	{
		if (m_bComputeContractionRate)
		{
			m_bComputeContractionRate = false;

			// reset m_contractionRate and m_contractionRateIndex list and set their first entry as zero.
			m_CRICalculation.ResetContractionsRates();

			for (long i = 0; i < GetContractionsCount(); ++i)
			{
				const contraction& curContraction = get_contraction(i);
				if (!curContraction.is_strike_out())
				{
					long windowSize = GetContractionRateWindowSize() * 60 * get_up_sample_rate();
					m_CRICalculation.AppendContractionsRates(windowSize, curContraction.get_peak());

					if (curContraction.get_end() - curContraction.get_start() > 120)
					{
						m_CRICalculation.AppendContractions120SecRates(windowSize, curContraction.get_peak());
					}
				}
			}
		}
	}

	// ===================================================================================================================
	// ===================================================================================================================
	Contractility::ContractilityClassification CRIFetus::GetContractilityClassification(long index) const
	{
		return m_CRICalculation.GetContractilityClassification(index);
	}

	// =================================================================================================================
	//    Access the currently associated conductor. See predicate has_conductor() and method set_conductor().
	// ===================================================================================================================
	const CRIConductor& CRIFetus::get_conductor(void) const
	{
		return *dynamic_cast<CRIConductor*>(kconductor);
	}

	CRIConductor& CRIFetus::get_conductor(void)
	{
		return *dynamic_cast<CRIConductor*>(kconductor);
	}


	vector<long> CRIFetus::GetContractionsRate() const
	{
		return m_CRICalculation.GetContractionsRate();
	}

	vector<long> CRIFetus::GetContractionsRateIndex() const
	{
		return m_CRICalculation.GetContractionsRateIndex();
	}
	
	// =================================================================================================================
	//    get contraction rate value from a up index.
	// ===================================================================================================================
	long CRIFetus::GetContractionRate(long upi) const
	{
		return m_CRICalculation.GetContractionsRateAt(upi);
	}

	// =================================================================================================================
	//    Calculate mean baseline level and variability between indexes lX1 and lX2 Results returned in mean_baseline and
	//    mean_var. values of -1.0 signifies that there was less than the minimum amount of baseline(2 minutes) in the window
	//    required to make a valid average.
	// ===================================================================================================================
	void CRIFetus::get_mean_baseline(long lX1, long lX2, double* mean_baseline, double* mean_var) const
	{
		double dTotal = 0.0;
		double dTotalVar = 0.0;
		double dMean = -1.0;
		double dMeanVar = -1.0;
		long n = GetEventsCount();
		long total_samples = 0, total_samples_var = 0;
		long min_samples = 2 * 60 * get_hr_sample_rate();
		long lMinBasLen = 0;
		long lTotBasTrim = 0;

		// NOTE 27/05/08 : until have mechanism to access signal processing config without needing the signal
		// * processing engine code, we hardcode these params so that they are equivalent whether running Patterns in
		// * client or server mode
		lMinBasLen = 60;						// (30 / 2) * 4 -> need half of 30 second segment at 4 Hz
		lTotBasTrim = 40;						// 2 * 5 * 4 -> 5 seconds at 4 Hz on each side of baseline are trimmed for var calculations

		// ifdef patterns_has_signal_processing if (m_devents) { lMinBasLen = m_devents->GetBasSegLenForVarCalc() / 2;
		// // length of segments over which var is calculated lTotBasTrim = m_devents->GetBasTrimForVarCalc() * 2;
		// // amount trimmed from baseline extremeties when calculating var } else { /* ;
		// endif
		

		if(!SufficientTracingDurationInQualificationWindow(lX2))
		{
			(*mean_baseline) = -1;
			(*mean_var) = -1;
			return;
		}

		for (long i = 0; i < n; i++)
		{
			const event&  ei = get_event(i);
			
			if (ei.is_baseline())
			{
				if ((ei.get_start() <= lX2) && (ei.get_end() >= lX1))
				{
					long ns = ei.get_length();	// length of baseline
					if (ns - lTotBasTrim >= lMinBasLen) // baselines that are too short will not have variability calculated
					{
						if (ei.get_start() < lX1)
						{	// starts before window - don't consider samples outside of window
							ns -= (lX1 - ei.get_start());
						}

						if (ei.get_end() > lX2)
						{	// end after window - don't consider samples outside window
							ns -= (ei.get_end() - lX2);
						}

						total_samples += ns;
						dTotal += (double)ns * ((ei.get_y1() + ei.get_y2()) / 2.0); // take mean of y1 and y2 as level for individual baseline
						if (ei.get_baseline_var() > 0)
						{
							total_samples_var += ns;
							dTotalVar += (double)ns * (ei.get_baseline_var());		// var as calculated in engine
						}
					}
				}
			}
		}

		if (total_samples > min_samples)	// enough baseline to make valid estimate
		{
			dMean = (double)dTotal / total_samples;
		}

		if (total_samples_var > min_samples)
		{
			dMeanVar = (double)dTotalVar / total_samples;
		}
		(*mean_baseline) = dMean;
		(*mean_var) = dMeanVar;
	}
	void CRIFetus::get_mean_baseline(long lX1, long lX2, int& meanBaseline, int& meanVariability) const
	{
		double mean_baseline,  mean_var;
		get_mean_baseline(lX1, lX2, &mean_baseline, &mean_var);
		meanBaseline = (int) floor(mean_baseline);
		meanVariability = (int)floor(mean_var);
	}

	// =================================================================================================================
	//    Access the current number of stored contractilities.
	// ===================================================================================================================
	long CRIFetus::GetContractilitiesCount(void) const
	{
		return m_CRICalculation.GetNumberOfContractilities();
	}


	// =================================================================================================================
	//    Assignment operator. Nothing to see here, folks, move along.
	// ===================================================================================================================
	CRIFetus &CRIFetus::operator=(const CRIFetus& curFetus)
	{
		if(this == &curFetus)
			return *this;

		suspend_notifications(true);
		fetus::operator=(curFetus);

		m_CRICalculation = curFetus.m_CRICalculation;

		suspend_notifications(false);

		return *this;
	}

	// =================================================================================================================
	//    Comparison operators.
	// ===================================================================================================================
	bool CRIFetus::operator==(const CRIFetus& x) const
	{
		return fetus::operator==(x) && 
			ContractilitiesEqual(x);
	}

	bool CRIFetus::operator!=(const CRIFetus& x) const
	{
		return !operator ==(x);
	}




	bool CRIFetus::SufficientTracingDurationInQualificationWindow(long iRight) const
	{
		long fhrDuration = ValidFHRAmountInQualificationWindow(iRight);
		bool fhrOK = CheckTraceRatio(fhrDuration, s_qualificationWindowSize * 4);
		bool upOK = true;

		// Oleg - fix for UP gap bug, reason - UP gap shouldn't affect BL variability value calculation
		//if(fhrOK)
		//{
		//	long upDuration = ValidUPAmountInQualificationWindow(iRight);
		//	upOK = CheckTraceRatio(upDuration, s_qualificationWindowSize);
		//}

		return fhrOK && upOK;
	}


	long CRIFetus::ValidFHRAmountInQualificationWindow(long iRight) const
	{
		int count = 0;
		long end = min(iRight, (long)m_fhr.size() - 1);
		long start = max(0, end - s_qualificationWindowSize * 4);
		for(int i = start; i <= end; i++)
		{
			if(IsFHRValid(get_fhr(i)))
				count++;
		}
		return count;
	}

	long CRIFetus::ValidUPAmountInQualificationWindow(long iRight) const
	{
		int count = 0;
		long end = min(iRight/4, (long)m_up.size() - 1);
		long start = max(0, iRight/4 -  s_qualificationWindowSize);
		for(int i = start; i <= end; i++)
		{
			if(IsUPValid(get_up(i)))
				count++;
		}
		return count;
	}

	bool CRIFetus::IsFHRValid(long fhr)
	{
		return fhr > 0 && fhr < 255;
	}

	bool CRIFetus::IsUPValid(long up)
	{
		 return up < 127;
	}

	bool CRIFetus::CheckTraceRatio(double traceDuration, long qualificationWndSize) const
	{
		double ratio = traceDuration/qualificationWndSize;
		return (int)(ratio*100) >= s_minimalAmountOfDataInWindow;
	}

}

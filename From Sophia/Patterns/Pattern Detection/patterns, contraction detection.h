#pragma once

#include "..\Pattern Data\patterns, contraction.h"

#include <vector>

using namespace std;

namespace patterns
{

	/*
	=======================================================================================================================
	Encapsulation of the contraction calculation. This class is used to detect contractions from an uterine pressure
	array. 
	=======================================================================================================================
	*/
	class contraction_detection
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		static const long HSWS_SEC_SMOOTH;
		static const long HSWS_SEC_INST_SLOPE;
		static const long WS_SEC_SMOOTH;
		static const double STD_DEV_SMOOTH;
		static const double STD_DEV_SMOOTH_SQUARE;
		static const double PI;
		static const double SQRT_2_PI;
		static const double S;
		static const double START_SLOPE_CUT_OFF;
		static const double PEAK_SLOPE_CUT_OFF;
		static const double END_SLOPE_CUT_OFF;
		static const char DETECT_START;
		static const char DETECT_PEAK;
		static const char DETECT_END;
		static const short START_RANGE_CUT_OFF;
		static const short PEAK_RANGE_CUT_OFF;
		static const short END_RANGE_CUT_OFF;
		static const double LATENT_VECTOR_CUT_OFF;
		static const float SLOPE_UNDEFINED;

		long m_nStartCounter;		
		long m_nStart_StartOfRange;
		long m_nStart_EndOfRange;
		bool m_bStartFlag;

		long m_nPeakCounter;
		long m_nPeak_StartOfRange;
		long m_nPeak_CenterOfRange;
		long m_nPeak_EndOfRange;
		bool m_bPeakFlag;

		long m_nEndCounter;
		long m_nEnd_StartOfRange;
		long m_nEnd_EndOfRange;
		bool m_bEndFlag;

		char m_chFeatureWanted;

		double m_LatentVectorCutOff;

		virtual void build_contraction(const vector<char> &, const vector<char> &, const vector<float> &, long, long, long, vector<contraction> *);
		virtual long calc_area(const vector<char> &, unsigned long, unsigned long, unsigned long, unsigned long);
		virtual double calc_slope_in(const vector<char> &, long, long);
		virtual double calc_slope_out(const vector<char> &, long, long);
		virtual const vector<float> calc_slopes(const vector<char> &);
		virtual void detect_end(const vector<char> &, const vector<char> &, const vector<float> &, long, vector<contraction> *);
		virtual void detect_peak(const vector<float> &, long);
		virtual void detect_start(const vector<float> &, long);
		virtual bool is_real_contraction(long, long, double, double, long);
		virtual const vector<char> smooth(const vector<char> &);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		contraction_detection(void);
		virtual ~contraction_detection(void);

		virtual void detect(vector<char>&, vector<contraction>&);

		virtual double get_latent_vector_cut_off(void);
		virtual void set_latent_vector_cut_off(double v);

	protected:
		virtual vector<contraction> OLD_detect(const vector<char> &);


	};
}
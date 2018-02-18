#include "stdafx.h"
#include "patterns, contraction detection.h"

#include <math.h>

namespace patterns
{
	// Static variable initialization
	const long contraction_detection:: HSWS_SEC_SMOOTH = 10;	// Half sliding window size in seconds (used for smoothing)
	const long contraction_detection:: HSWS_SEC_INST_SLOPE = 20;
	const long contraction_detection:: WS_SEC_SMOOTH = 2 * contraction_detection::HSWS_SEC_SMOOTH;
	const double contraction_detection:: STD_DEV_SMOOTH = 8.0;
	const double contraction_detection:: STD_DEV_SMOOTH_SQUARE = contraction_detection::STD_DEV_SMOOTH * contraction_detection::STD_DEV_SMOOTH;
	const double contraction_detection:: PI = 3.14159265358;
	const double contraction_detection:: SQRT_2_PI = sqrt(2 * contraction_detection::PI);
	const double contraction_detection:: S = 8.0;

	const double contraction_detection:: LATENT_VECTOR_CUT_OFF = 0.695;
	const double contraction_detection:: START_SLOPE_CUT_OFF = 0.25;
	const double contraction_detection:: PEAK_SLOPE_CUT_OFF = 0.25;
	const double contraction_detection:: END_SLOPE_CUT_OFF = -0.25;
	const short contraction_detection:: START_RANGE_CUT_OFF = 12;
	const short contraction_detection:: PEAK_RANGE_CUT_OFF = 3;
	const short contraction_detection:: END_RANGE_CUT_OFF = 12;

	const char contraction_detection:: DETECT_START = 'S';
	const char contraction_detection:: DETECT_PEAK = 'P';
	const char contraction_detection:: DETECT_END = 'E';
	const float contraction_detection:: SLOPE_UNDEFINED = 999.0;

// =====================================================================================================================
//    cast utility.
// =====================================================================================================================
#define S_TO_UC(x)	(char) (unsigned char)x
#define UC_TO_S(x)	(short) (unsigned char)x

	// =================================================================================================================
	//    constructor/destructor.
	// ===================================================================================================================
	contraction_detection::contraction_detection(void)
	{
		m_LatentVectorCutOff = LATENT_VECTOR_CUT_OFF;
	}

	contraction_detection::~contraction_detection(void)
	{
	}

	// =================================================================================================================
	//    A contraction has been detected. It's now time to validate and build it.
	// ===================================================================================================================
	void contraction_detection::build_contraction(const vector<char>& uterinePressure, const vector<char>& smoothed, const vector<float>& slopes, long startNdx, long peakNdx, long endNdx, vector<contraction>* contractions)
	{
		// slopes set is not the same size as the uterine and smoothed array. We must calculte the equivalent index.
		long nStartNdx = startNdx + HSWS_SEC_INST_SLOPE;
		long nPeakNdx = peakNdx + HSWS_SEC_INST_SLOPE;
		long nEndNdx = endNdx + HSWS_SEC_INST_SLOPE;
		long nHeight(UC_TO_S(smoothed[nPeakNdx - 1]) - (abs((UC_TO_S(smoothed[nStartNdx - 1]) + UC_TO_S(smoothed[nEndNdx - 1])) / 2)));
		long nArea(calc_area(smoothed, nStartNdx, nEndNdx, UC_TO_S(smoothed[nStartNdx - 1]), UC_TO_S(smoothed[nEndNdx - 1])));
		double dSlopeIn(calc_slope_in(smoothed, nStartNdx, nPeakNdx));
		double dSlopeOut(calc_slope_out(smoothed, nEndNdx, nPeakNdx));

		short nAbsStartHeight(0), nAbsPeakHeight(0), nAbsEndHeight(0);

		// if real reading exists, we use that value.
		nAbsStartHeight = UC_TO_S(uterinePressure[nStartNdx - 1]);
		nAbsPeakHeight = UC_TO_S(uterinePressure[nPeakNdx - 1]);
		nAbsEndHeight = UC_TO_S(uterinePressure[nEndNdx - 1]);

		if (is_real_contraction(nHeight, nArea, dSlopeIn, dSlopeOut, nEndNdx - nStartNdx))
		{
			contraction c(nStartNdx - 1, nPeakNdx - 1, nEndNdx - 1);

			// c.set_area(nArea) ;
			// c.set_height(nHeight) ;
			// c.set_slopein(dSlopeIn) ;
			// c.set_slopeout(dSlopeOut) ;
			contractions->insert(contractions->end(), c);
		}
	}

	// =================================================================================================================
	//    Calculation of the "Area" info.
	// ===================================================================================================================
	long contraction_detection::calc_area(const vector<char>& smoothed, unsigned long nXstart, unsigned long nXend, unsigned long nYstart, unsigned long nYend)
	{
		unsigned long nHeight = 0;
		unsigned long nBrSmHeight = 0;
		unsigned long nArea = 1;
		float dDeltaY(static_cast<float>(static_cast<long>(nYend) -static_cast<long>(nYstart)));
		float dSlope(dDeltaY / (nXend - nXstart));	// i.e. change in Y over change in X
		for (unsigned long nXi(nXstart); nXi <= nXend; nXi++)
		{
			nBrSmHeight = UC_TO_S(smoothed[nXi - 1]);
			if ((nHeight = (unsigned long)(nBrSmHeight - nYstart - (dSlope * (nXi - nXstart)))) < 0)
			{
				nHeight = 0;
			}

			nArea += nHeight;
		}

		return nArea;
	}

	// =================================================================================================================
	//    Calculation of the slopein info.
	// ===================================================================================================================
	double contraction_detection::calc_slope_in(const vector<char>& smoothed, long startNdx, long peakNdx)
	{
		double r = 0.0;

		if (peakNdx - startNdx > 8)
		{
			long deltaNdx = 7;
			const short value1 = UC_TO_S(smoothed[startNdx - 1]);
			short value2 = UC_TO_S(smoothed[startNdx + deltaNdx - 1]);

			// difference must be positive so the slope will be positive
			while (value2 - value1 <= 0)
			{
				deltaNdx++;
				if (startNdx + deltaNdx == peakNdx)
				{
					return 0.0; // 0.0 used to indicate failure
				}

				value2 = UC_TO_S(smoothed[startNdx + deltaNdx - 1]);
			}

			r = abs(long(100.0 * (value2 - value1) / deltaNdx));
		}

		return r;
	}

	// =================================================================================================================
	//    Calculation of the slopein info.
	// ===================================================================================================================
	double contraction_detection::calc_slope_out(const vector<char>& smoothed, long endNdx, long peakNdx)
	{
		double r = 0.0;

		if (endNdx - peakNdx > 8)
		{
			long deltaNdx = 7;
			short value1 = UC_TO_S(smoothed[endNdx - deltaNdx - 1]);
			const short value2 = UC_TO_S(smoothed[endNdx - 1]);

			while (value2 - value1 >= 0)	// the difference must negative so the slope will be negative
			{
				deltaNdx++;

				// 0.0 used to indicate failure
				if (endNdx - deltaNdx == peakNdx)
				{
					return 0.0;
				}

				value1 = UC_TO_S(smoothed[endNdx - deltaNdx - 1]);
			}

			r = abs(long(100.0 * (value2 - value1) / deltaNdx));
		}

		return r;
	}

	// =================================================================================================================
	//    Slopes calculation.
	// ===================================================================================================================
	const vector<float> contraction_detection::calc_slopes(const vector<char>& smoothed)
	{
		vector<float> slopes;

		slopes.resize(smoothed.size(), SLOPE_UNDEFINED);

		unsigned long nCard = (unsigned long)smoothed.size();

		if (nCard < (2 * HSWS_SEC_INST_SLOPE) + 1)
		{
			return slopes;
		}

		double dIUPAVG(0.0);
		double dTempSum(0.0);
		double dTempSum2(0.0);
		double dSlope(0.0);
		double dT(0.0);
		double dWeight(0.0);
		double dSumWeight(0.0);

		for (unsigned long i = HSWS_SEC_INST_SLOPE + 1; i < nCard - HSWS_SEC_INST_SLOPE + 1; i++)
		{
			dT = (((i + HSWS_SEC_INST_SLOPE) * (i + HSWS_SEC_INST_SLOPE + 1) / 2) - ((i - HSWS_SEC_INST_SLOPE) * (i - HSWS_SEC_INST_SLOPE - 1) / 2)) / (HSWS_SEC_INST_SLOPE * 2 + 1);

			// for sum calculation of IUP within the current Sliding window
			dWeight = exp(-(pow((double)UC_TO_S(smoothed[i - 1]), (double)2.0) / (S * S)) / (S * sqrt(2 * PI)));
			for (unsigned long k(i - HSWS_SEC_INST_SLOPE); k <= i + HSWS_SEC_INST_SLOPE; k++)
			{
				dTempSum = dTempSum + UC_TO_S(smoothed[k - 1]);
				dSumWeight += dWeight;
			}

			dIUPAVG = dTempSum / ((HSWS_SEC_INST_SLOPE * 2) + 1);
			dTempSum = 0;

			// calc. stats for current Sliding Window.
			dWeight = exp(-((double)pow((double)UC_TO_S(smoothed[i - 1]), (double)2.0) / (S * S)) / (S * sqrt(2 * PI)));
			for (unsigned long j(i - HSWS_SEC_INST_SLOPE); j <= i + HSWS_SEC_INST_SLOPE; j++)
			{
				dTempSum = dTempSum + (pow((double)dWeight / dSumWeight, (double)2.0) * (UC_TO_S(smoothed[j - 1]) - dIUPAVG) * (j - dT));	// instant. slope
				dTempSum2 = dTempSum2 + (pow((double)dWeight / dSumWeight, (double)2.0) * (pow((double)(j - dT), (double)2.0)));
			}

			// this is the instantanous slope at point i
			dSlope = dTempSum / dTempSum2;

			// this replaces the IUP value by an instantaneous slope
			slopes[i - 1] = (static_cast<float>(dSlope));
			dTempSum = dTempSum2 = dSlope = 0;
		}

		// remove undefined values.
		{
			for (long i = 0, n = (long)slopes.size(); i < n; i++)
			{
				if (slopes[i] == SLOPE_UNDEFINED)
				{
					slopes.erase(slopes.begin() + i--);
					n--;
				}
			}
		}

		return slopes;
	}

	// =================================================================================================================
	//    Detect contractions from given uterine pressure array. All uterine pressure values are considered as real. We
	//    create an array of bool with all values set to true.
	// ===================================================================================================================
	vector<contraction> contraction_detection::OLD_detect(const vector<char>& uterinePressure)
	{
		vector<contraction> contractions;
		vector<char> smoothed = smooth(uterinePressure);
		vector<float> slopes = calc_slopes(smoothed);

		if (slopes.size() > 0)
		{
			long curNdx = 1;

			m_bStartFlag = m_bPeakFlag = m_bEndFlag = false;
			m_nStartCounter = m_nPeakCounter = m_nEndCounter = 0;
			m_nStart_StartOfRange = m_nPeak_StartOfRange = m_nEnd_StartOfRange = 1;
			m_nPeak_CenterOfRange = m_nStart_EndOfRange = m_nPeak_EndOfRange = m_nEnd_EndOfRange = 1;
			m_chFeatureWanted = DETECT_START;

			while (curNdx <= (long)slopes.size())
			{
				switch (m_chFeatureWanted)
				{
					case DETECT_START:
						detect_start(slopes, curNdx);
						break;

					case DETECT_PEAK:
						detect_peak(slopes, curNdx);
						break;

					case DETECT_END:
						detect_end(uterinePressure, smoothed, slopes, curNdx, &contractions);
						break;
				}

				curNdx++;
			}
		}

		return contractions;
	}

	// =================================================================================================================
	//    Detection of the end point of a contraction.
	// ===================================================================================================================
	void contraction_detection::detect_end(const vector<char>& uterinePressure, const vector<char>& smoothed, const vector<float>& slopes, long nSlopeSetNdx, vector<contraction>* contractions)
	{
		double dSlopeValue = slopes[nSlopeSetNdx - 1];

		if (dSlopeValue < END_SLOPE_CUT_OFF)	// this point is within the end range
		{
			m_nEndCounter++;
			if (m_nEndCounter > END_RANGE_CUT_OFF && !m_bEndFlag)
			{
				m_bEndFlag = true;
				m_nEnd_StartOfRange = m_nEnd_EndOfRange = nSlopeSetNdx - m_nEndCounter;
				m_nPeak_EndOfRange = m_nEnd_StartOfRange;
				m_nPeak_CenterOfRange = static_cast<long>(floor (static_cast<double>(m_nPeak_EndOfRange -m_nPeak_StartOfRange) / 2.0)) + m_nPeak_StartOfRange;
				m_nPeakCounter = 0;
			}
		}
		else	// this point is out of the end range (either into peak or start range)
		{
			if (m_bStartFlag && m_bPeakFlag && m_bEndFlag)	// contraction defined
			{
				m_nEnd_EndOfRange = nSlopeSetNdx;

				// At this time we will make some adjustments to the start point and end point.
				float dMax(0.0);
				long nMaxNdx(0);

				// find highest rate of change point for the start range
				long i = 0;

				for (i = m_nStart_StartOfRange; i <= m_nStart_EndOfRange; i++)
				{
					float dDiff(slopes[i - 1] - slopes[i - 2]);

					if (dDiff > dMax)
					{
						dMax = dDiff;
						nMaxNdx = i;
					}
				}

				if (nMaxNdx - m_nStart_StartOfRange > 15)
				{
					m_nStart_StartOfRange = m_nStart_StartOfRange + (long)floor((double)(nMaxNdx - m_nStart_StartOfRange) / 2);
				}

				dMax = 0.0;
				nMaxNdx = 0;

				// find highest rate of change point for the end range
				for (i = m_nEnd_StartOfRange; i <= m_nEnd_EndOfRange; i++)
				{
					float dDiff(slopes[i - 2] - slopes[i - 1]);

					if (dDiff < dMax)
					{
						dMax = dDiff;
						nMaxNdx = i;
					}
				}

				if (m_nEnd_EndOfRange - nMaxNdx > 15)
				{
					m_nEnd_EndOfRange = nMaxNdx + static_cast<long>(floor((double)(m_nEnd_EndOfRange - nMaxNdx) / 2));
				}

				// Any necessary adjustments have taken place and we are now ready to build a contraction.
				build_contraction(uterinePressure, smoothed, slopes, m_nStart_StartOfRange, m_nPeak_CenterOfRange, m_nEnd_EndOfRange, contractions);
			}

			m_bStartFlag = false;
			m_bPeakFlag = false;
			m_bEndFlag = false;
			m_nStartCounter = 0;
			m_nPeakCounter = 0;
			m_nEndCounter = 0;
			m_chFeatureWanted = DETECT_START;
		}
	}

	// =================================================================================================================
	//    Detection of the peak of a contraction.
	// ===================================================================================================================
	void contraction_detection::detect_peak(const vector<float>& slopes, long nSlopeSetNdx)
	{
		static const double nPosCutoff(PEAK_SLOPE_CUT_OFF);
		static const double nNegCutoff(-1 * PEAK_SLOPE_CUT_OFF);
		double dSlopeValue = slopes[nSlopeSetNdx - 1];

		if (dSlopeValue < nPosCutoff && dSlopeValue > nNegCutoff)
		{
			m_nPeakCounter++;
			if (m_nPeakCounter > 60)				// can't find a peak after 1 minute -> reset
			{
				m_nStartCounter = 0;
				m_nPeakCounter = 0;
				m_bPeakFlag = false;
				m_bStartFlag = false;
				m_chFeatureWanted = DETECT_START;
			}
			else
			{
				m_bPeakFlag = true;
				m_nStartCounter = 0;
				m_nStart_EndOfRange = nSlopeSetNdx - m_nPeakCounter;
				m_nPeak_StartOfRange = m_nPeak_EndOfRange = m_nStart_EndOfRange;
			}
		}
		else
		{
			if (dSlopeValue > START_SLOPE_CUT_OFF)	// the slope increases again -> start range not finished yet.
			{
				m_nPeakCounter = 0;
				m_bPeakFlag = false;
				m_nStartCounter = nSlopeSetNdx - m_nStart_StartOfRange;
				m_chFeatureWanted = DETECT_START;
			}

			if (m_bPeakFlag)
			{
				m_chFeatureWanted = DETECT_END;
			}
			else
			{
				if (dSlopeValue < END_SLOPE_CUT_OFF)
				{
					m_bPeakFlag = true;
					m_nPeak_StartOfRange = m_nPeak_EndOfRange = nSlopeSetNdx;
					m_nStart_EndOfRange = nSlopeSetNdx;
					m_nStartCounter = 0;
					m_chFeatureWanted = DETECT_END;
				}
			}
		}
	}

	// =================================================================================================================
	//    Detection of the start point of a contraction.
	// ===================================================================================================================
	void contraction_detection::detect_start(const vector<float>& slopes, long nSlopeSetNdx)
	{
		double dSlopeValue = slopes[nSlopeSetNdx - 1];

		if (dSlopeValue > START_SLOPE_CUT_OFF)
		{
			m_nStartCounter++;

			if ((m_nStartCounter > START_RANGE_CUT_OFF) && (!m_bStartFlag))
			{
				m_bStartFlag = true;
				m_nStart_StartOfRange = m_nStart_EndOfRange = nSlopeSetNdx - m_nStartCounter;

				// if we tracing begins immediately with a start we will get 0 for m_nStart_StartOfRange, so we make
				// it 1
				m_nStart_StartOfRange = m_nStart_StartOfRange < 2 ? 2 : m_nStart_StartOfRange;
			}
		}
		else
		{
			if (m_bStartFlag)
			{
				m_chFeatureWanted = DETECT_PEAK;
			}
		}
	}

	// =================================================================================================================
	//    returns latent vector cut off value. (threshold to accept or not a contraction).
	// ===================================================================================================================
	double contraction_detection::get_latent_vector_cut_off(void)
	{
		return m_LatentVectorCutOff;
	}

	// =================================================================================================================
	//    Validation of a contraction.
	// ===================================================================================================================
	bool contraction_detection::is_real_contraction(long height, long area, double slopeIn, double slopeOut, long duration)
	{
		// An explanation of the following analysis can be found in "Latent Variable Logistic Regression of
		// Contraction Tracings" by Ebi Kimanani, May 16, 1997. This document may be obtained from Bruno Bendavid at
		// LMS Medical Systems Inc. - tel. 488-3461 x229.
		double contractionSize = (0.34563 * (duration - 83.22) / 17.57) + (0.36578 * (height - 59.28) / 22.56) + (0.43671 * (area - 1564.19) / 977.30) + (0.06815 * (fabs(slopeIn) - 33.41) / 33.00) + (0.10994 * (fabs(slopeOut) - 36.54) / 30.66);
		double contractionShape = (-0.37992 * (duration - 83.22) / 17.57) + (0.13338 * (height - 59.28) / 22.56) + (-0.05300 * (area - 1564.19) / 977.30) + (0.63868 * (fabs(slopeIn) - 33.41) / 33.00) + (0.56531 * (fabs(slopeOut) - 36.54) / 30.66);
		double N = 2.1199 + (0.9704 * contractionSize) - (0.2769 * contractionShape);
		double probOfBeingReal = exp(N) / (1 + exp(N));

		return probOfBeingReal >= get_latent_vector_cut_off();
	}

	// =================================================================================================================
	//    set the latent vector cut off (threshold to accept or not a contraction).
	// ===================================================================================================================
	void contraction_detection::set_latent_vector_cut_off(double v)
	{
		m_LatentVectorCutOff = v;
	}

	// =================================================================================================================
	//    Pass the uterine pressure array to the smoothing algorithm. Creates a new array of the same size. The smoothed
	//    array is used to calculate the slopes.
	// ===================================================================================================================
	const vector<char> contraction_detection::smooth(const vector<char>& up)
	{
		static bool initialized = false;
		static double distribution[HSWS_SEC_SMOOTH + 1];
		vector<char> smoothed;

		// First time initialisation of the distribution array
		if (!initialized)
		{
			initialized = true;

			// Calcul des facteurs de ponderation suivant une distribution Gaussienne. Une Gaussienne etant par
			// nature symetrique, les facteurs de ponderations sont calcules pour (Psmooth + 1) elements (et non 2P +
			// 1 elements).
			for (int i = 0; i <= HSWS_SEC_SMOOTH; ++i)
			{
				distribution[i] = exp(-((double)i * i) / (STD_DEV_SMOOTH * STD_DEV_SMOOTH)) / (STD_DEV_SMOOTH * SQRT_2_PI);
			}
		}

		// The IUP Mean Average filter of Ebi Kimanani.
		int nCard = (int)up.size();

		// We need a minimum number of samples to do something there...
		if (nCard < WS_SEC_SMOOTH + 1)
		{
			return smoothed;
		}

		// Collect all the data, if we encounter a <invalid data>, replace it with the previous one (gap the bridge
		// for missing data)
		unsigned char*	A = new BYTE[nCard];
		for (int i = 0; i < nCard; ++i)
		{
			A[i] = static_cast<unsigned char>(up[i]);
		}

		// Do the real job there
		for (int i = 0; i < nCard; ++i)
		{
			// Cas particulier pour le point i : La ponderation T[0] appliquee au point (i) de la courbe sur lequel
			// est calcule le filtrage
			double coefficient = distribution[0];
			double value = A[i] * coefficient;

			// Pour les autres points à au moins 1 de distance de i...
			for (int j = 1; (j <= i) && (j < nCard - i) && (j <= HSWS_SEC_SMOOTH); ++j)
			{
				// La distribution des ponderations appliquee symetriquement sur les 2*Psmooth points autour du
				// point (i) filtre
				value += (A[i - j] + A[i + j]) * distribution[j];
				coefficient += 2 * distribution[j];
			}

			// create smoothed array.
			smoothed.push_back (static_cast<unsigned char>(value / coefficient));
		}

		delete[] A;
		return smoothed;
	}

	// =================================================================================================================
	//    Detection of the contractions for the given uterine pressure data set
	// ===================================================================================================================
	void contraction_detection::detect(vector<char>& uterinePressure, vector<contraction>& contractions)
	{
		const long maximum_bridgeable_gap = 15;				// Bridge up to 15 seconds of missing data
		const long maximum_segment_size = 43200;			// Maximum 12 hours of detections at once
		const long delay_before_last_contraction = 30;		// If detection is done by chunk, restart 30 second prior to the end of the last contraction...
		const long delay_before_last_calculation = 600;		// ...but no more than 10 minutes overlap (in case there is no contraction close to the end of the segment)

		// Detection cannot be performed if there is less than 10 seconds of up...
		if (uterinePressure.size() <= WS_SEC_SMOOTH)		
			return;

		vector<char> segment_up;
		long segment_offset = 0;
		long segment_position = 0;
		long gap_length = 0;
		unsigned char last_valid_up = 255;
	
		vector<char>::const_iterator itr = uterinePressure.begin();
		while (itr != uterinePressure.end())
		{
			unsigned char up = (unsigned char) *itr;
			++itr;
			++segment_position;

			// Missing point?
			if (up >= 127)
			{
				// Very beginning? Just move to the next point
				if (segment_position == 1)
				{
					ASSERT(last_valid_up == 255);

					++segment_offset;
					segment_position = 0;
					continue;
				}

				++gap_length;
			}

			// Valid point?
			if (up < 127)
			{
				ASSERT((gap_length == 0) || (last_valid_up < 127));

				// Too long gap?
				if (gap_length > maximum_bridgeable_gap)
				{
					// Just flat out at ZERO
					segment_up.insert(segment_up.end(), gap_length, 0);
				}

				// Small gap?
				else if (gap_length > 0)
				{
					// Bridge drawing a line accross the gap
					double delta = ((double)up - (double)last_valid_up) / (gap_length + 1);
					for (long i = 0; i < gap_length; ++i)
					{
						segment_up.push_back((unsigned char)(last_valid_up + (i * delta)));
					}
				}

				segment_up.push_back(up);
				last_valid_up = up;
				gap_length = 0;
			}
			
			// Trigger a calculation?
			if ((segment_up.size() > WS_SEC_SMOOTH) && ((itr == uterinePressure.end()) || (segment_up.size() >= maximum_segment_size)))
			{
				// Do the detection
				vector<contraction> segment_contractions = OLD_detect(segment_up);

				// Accumulate the contractions and relocate them if necessary
				long last_contraction_end = 0;
				for (vector<contraction>::iterator c = segment_contractions.begin(); c != segment_contractions.end(); ++c)
				{
					if (segment_offset > 0)
					{
						*c += segment_offset;
					}

					contractions.push_back(*c);

					// Remember the last contraction end
					last_contraction_end = c->get_end();
				}

				// Calculate where to start next segment
				if (itr != uterinePressure.end())
				{
					ASSERT(segment_position > delay_before_last_calculation);

					// We restart at 10 minutes prior to the end of the actual segment or 30 seconds prior to the end of the last contraction
					long restart_next_segment = max(last_contraction_end - delay_before_last_contraction, segment_offset + segment_position - delay_before_last_calculation);

					ASSERT(restart_next_segment > segment_offset);
					ASSERT(restart_next_segment < segment_offset + segment_position);

					segment_up.clear();
					segment_offset = restart_next_segment;
					segment_position = 0;
					gap_length = 0;
					last_valid_up = 255;

					// Reposition iterator
					itr = uterinePressure.begin() + segment_offset;
				}
			}
		}
	}
}

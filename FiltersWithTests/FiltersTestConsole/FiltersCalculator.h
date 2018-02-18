#pragma once
using namespace std;

#include <vector>

enum HalfWavePart
{
	Pos20Min,
	Neg20Min,
	Pos8Min,
	Neg8Min
};

class FiltersCalculator
{
public:
	FiltersCalculator();
	virtual ~FiltersCalculator();

	void InitTracingData();
	void AddMoreData();

	bool BP2Processing(void);

private:
	void IniFiltersDataStructures();
	void CalculateMedian(unsigned char sample);
	void CalculateMedianResidual();
	void CalculateMedianResidualHalfWaves();

	void InsertSort(vector<unsigned char>& vecToIns, unsigned char val, int start, int end);
	int FindIndex(vector<unsigned char>& searchVector, int start, int end, unsigned char searchVal);

	void LinearInterpolation(HalfWavePart part);
	void LinearInterpolationInternal(vector<short>& halfWave, int start, int end);

	void AddToInterpolatedVector(HalfWavePart part);
	void AddToInterpolatedVectorInternal(vector<short>& halfWave, vector<short>& halfWaveInterp, int start);

	void CalculateFirFilter();
	void CalculateFirFilterInternal(vector<short>& halfWave, vector<double>& envelope);

	void CalculateSkewness();
	double CalculateSkewnessInternal(double devs[], int size);

private:
	int m_iFir_Flt_HP25_Size;
	vector<double> m_filterCoefsHP25;

	vector<unsigned char> m_tracingsData;		// sliding window of tracings as it arrives from episode.
	int m_lastInsertedIndex;

	vector<unsigned char> m_sorted20MinTracing;		// sliding window of sorted tracings for median calculation
	//vector<unsigned char> m_sorted8MinTracing;		// sliding window of sorted tracings for median calculation

	vector<unsigned char> m_median20MinResult;		// median calculation result ± 10 min
	//vector<unsigned char> m_median8MinResult;		// median calculation result ± 4 min

	vector<short> m_medianResidual20MinResult;		// median residual calculation result ± 10 min
	//vector<short> m_medianResidual8MinResult;		// median residual calculation result ± 4 min

	vector<short> m_medianResidual20MinHalfWavePos;		// median residual half wave pos ± 10 min
	vector<short> m_medianResidual20MinHalfWaveNeg;		// median residual half wave neg ± 10 min
	//vector<short> m_medianResidual8MinHalfWavePos;		// median residual half wave pos  ± 4 min
	//vector<short> m_medianResidual8MinHalfWaveNeg;		// median residual half wave neg  ± 4 min

	vector<short> m_medianResidual20MinHalfPosInterp;	// median residual half wave pos interpolated ± 10 min
	vector<short> m_medianResidual20MinHalfNegInterp;	// median residual half wave neg interpolated ± 10 min
	//vector<short> m_medianResidual8MinHalfPosInterp;	// median residual half wave pos interpolated ± 4 min
	//vector<short> m_medianResidual8MinHalfNegInterp;	// median residual half wave neg interpolated ± 4 min

	vector<double> m_upperEnvelope20Min;
	vector<double> m_lowerEnvelope20Min;
	//vector<double> m_upperEnvelope8Min;
	//vector<double> m_lowerEnvelope8Min;

	vector<short> m_20MinMedianResiduals;
	vector<short> m_8MinMedianResiduals;

	vector<double> m_skew20Min;
	vector<double> m_skew8Min;

	int m_lastPos20MinNOTNaN;
	int m_lastNeg20MinNOTNaN;
	//int m_lastPos8MinNOTNaN;
	//int m_lastNeg8MinNOTNaN;

	double m_sum20Min;
	double m_sum8Min;

	bool m_bPrint;
};


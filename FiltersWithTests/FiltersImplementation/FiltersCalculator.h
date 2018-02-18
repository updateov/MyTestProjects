#pragma once
using namespace std;

#include <vector>
#include "Stdafx.h"

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
	
	vector<unsigned char>& GetTracingData()
	{
		return m_tracingsData;
	}

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



private:
	vector<unsigned char> m_tracingsData;		// sliding window of tracings as it arrives from episode.
	int m_lastInsertedIndex;

	vector<unsigned char> m_sorted20MinTracing;		// sliding window of sorted tracings for median calculation
	vector<unsigned char> m_sorted8MinTracing;		// sliding window of sorted tracings for median calculation

	vector<unsigned char> m_median20MinResult;		// median calculation result ± 10 min
	vector<unsigned char> m_median8MinResult;		// median calculation result ± 4 min

	vector<short> m_medianResidual20MinResult;		// median residual calculation result ± 10 min
	vector<short> m_medianResidual8MinResult;		// median residual calculation result ± 4 min

	vector<short> m_medianResidual20MinHalfWavePos;		// median residual half wave pos ± 10 min
	vector<short> m_medianResidual20MinHalfWaveNeg;		// median residual half wave neg ± 10 min
	vector<short> m_medianResidual8MinHalfWavePos;		// median residual half wave pos  ± 4 min
	vector<short> m_medianResidual8MinHalfWaveNeg;		// median residual half wave neg  ± 4 min

	int m_lastPos20MinNOTNaN;
	int m_lastNeg20MinNOTNaN;
	int m_lastPos8MinNOTNaN;
	int m_lastNeg8MinNOTNaN;
};


#include "stdafx.h"
#include "FiltersCalculator.h"

#define FHR_20MIN_MEDIAN_WINDOW	49
#define FHR_20MIN_WINDOW		48
#define FHR_20MIN_HALF_WINDOW	24
#define FHR_8MIN_MEDIAN_WINDOW	29
#define FHR_8MIN_WINDOW			28
#define FHR_8MIN_HALF_WINDOW	14

FiltersCalculator::FiltersCalculator()
{
	m_tracingsData.clear();
	m_lastInsertedIndex = 0;

	m_sorted20MinTracing.clear();
	m_sorted8MinTracing.clear();

	m_median20MinResult.clear();
	m_median8MinResult.clear();

	m_medianResidual20MinResult.clear();
	m_medianResidual8MinResult.clear();
}


FiltersCalculator::~FiltersCalculator()
{
}

void FiltersCalculator::InitTracingData()
{
	unsigned char tmp[100] { 203, 116, 222, 111, 18, 212, 241, 3, 105, 207, 53, 72, 201, 93, 103, 221, 218, 216, 251, 37, 23, 110, 215, 16, 146, 195, 11, 240, 39, 72, 33, 11, 58, 117, 255, 177, 45, 22, 60, 47, 43, 226, 248, 126, 72, 241, 40, 246, 225, 233, 214, 249, 160, 121, 147, 92, 157, 205, 221, 78, 200, 1, 219, 33, 174, 41, 13, 83, 94, 181, 175, 196, 155, 204, 249, 53, 118, 88, 199, 161, 155, 20, 14, 62, 114, 39, 106, 193, 226, 162, 20, 95, 56, 237, 102, 42, 7, 215, 207, 30 };
	//unsigned char tmp[25] { 203, 116, 222, 111, 18, 212, 241, 3, 105, 207, 53, 72, 201, 93, 103, 221, 218, 216, 251, 37, 23, 110, 215, 16, 146};
	for (size_t i = 0; i < 100; i++)
	{
		m_tracingsData.push_back(tmp[i]);
	}
}

void FiltersCalculator::AddMoreData()
{
	unsigned char tmp[25] { 203, 116, 222, 111, 18, 212, 241, 3, 105, 207, 53, 72, 201, 93, 103, 221, 218, 216, 251, 37, 23, 110, 215, 16, 146};
	for (size_t i = 0; i < 25; i++)
	{
		m_tracingsData.push_back(tmp[i]);
	}
}

bool FiltersCalculator::BP2Processing(void)
{
	IniFiltersDataStructures();
	vector<unsigned char>::iterator itr = m_tracingsData.begin() + m_lastInsertedIndex;
	//while (itr != m_tracingsData.end())
	while (m_tracingsData.size() > FHR_20MIN_WINDOW)
	//while (m_tracingsData.size() > 6)
	{
		//unsigned char curSample = *itr;
		unsigned char curSample = (int)m_tracingsData.size() > m_lastInsertedIndex ? m_tracingsData.at(m_lastInsertedIndex) : m_tracingsData.back();
		CalculateMedian(curSample);
		CalculateMedianResidual();
		CalculateMedianResidualHalfWaves();
		
		if (m_lastInsertedIndex >= FHR_20MIN_WINDOW)
			m_tracingsData.erase(m_tracingsData.begin());
		else
		{
			m_lastInsertedIndex++;
			//itr++;
		}
	}

	CString str("Median 20 min result\n");
	CString cstr("");
	for (size_t i = 0; i < m_median20MinResult.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_median20MinResult.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	str = "Median 8 min result\n";
	cstr.Empty();
	for (size_t i = 0; i < m_median8MinResult.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_median8MinResult.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	str = "Median residual 20 min result\n";
	cstr.Empty();
	for (size_t i = 0; i < m_medianResidual20MinResult.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_medianResidual20MinResult.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	str = "Median residual 8 min result\n";
	cstr.Empty();
	for (size_t i = 0; i < m_medianResidual8MinResult.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_medianResidual8MinResult.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	str = "Median residual 20 min Half wave pos\n";
	cstr.Empty();
	for (size_t i = 0; i < m_medianResidual20MinHalfWavePos.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_medianResidual20MinHalfWavePos.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	str = "Median residual 20 min Half wave neg\n";
	cstr.Empty();
	for (size_t i = 0; i < m_medianResidual20MinHalfWaveNeg.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_medianResidual20MinHalfWaveNeg.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	str = "Median residual 8 min Half wave pos\n";
	cstr.Empty();
	for (size_t i = 0; i < m_medianResidual8MinHalfWavePos.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_medianResidual8MinHalfWavePos.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	str = "Median residual 8 min Half wave neg\n";
	cstr.Empty();
	for (size_t i = 0; i < m_medianResidual8MinHalfWaveNeg.size(); i++)
	{
		//cout << (short)m_median20MinResult.at(i)
		cstr.Format(_T("%d, "), m_medianResidual8MinHalfWaveNeg.at(i));
		str += cstr;
	}

	//cout << str.c_str();
	TRACE(str);
	TRACE(_T("\n"));

	return false;
}

void FiltersCalculator::IniFiltersDataStructures()
{
	unsigned char firstElem = m_tracingsData.front();
	if ((int)m_sorted20MinTracing.size() <= 0)
		m_sorted20MinTracing.insert(m_sorted20MinTracing.begin(), FHR_20MIN_HALF_WINDOW, firstElem);

	if ((int)m_sorted8MinTracing.size() <= 0)
		m_sorted8MinTracing.insert(m_sorted8MinTracing.begin(), FHR_8MIN_HALF_WINDOW, firstElem);
}

void FiltersCalculator::CalculateMedian(unsigned char sample)
{
	// Insertion to sorted array
	int size = (int)m_sorted20MinTracing.size();
	if (sample == m_sorted20MinTracing.front())
		m_sorted20MinTracing.insert(m_sorted20MinTracing.begin(), sample);
	else
		InsertSort(m_sorted20MinTracing, sample, 0, size - 1);

	size = (int)m_sorted8MinTracing.size();
	if (sample == m_sorted8MinTracing.front())
		m_sorted8MinTracing.insert(m_sorted8MinTracing.begin(), sample);
	else
		InsertSort(m_sorted8MinTracing, sample, 0, size - 1);

	// After insertion, check whether there's enough data, add result and remove head from sorted array
	size = (int)m_sorted20MinTracing.size();
	if (size >= FHR_20MIN_MEDIAN_WINDOW) // 4800 = 10 minutes in quarter seconds
	{
		m_median20MinResult.push_back(m_sorted20MinTracing.at(FHR_20MIN_HALF_WINDOW));
		unsigned char toRem;
		if (m_lastInsertedIndex >= FHR_20MIN_MEDIAN_WINDOW)
			toRem = m_tracingsData.at(m_lastInsertedIndex - FHR_20MIN_WINDOW);
		else
			toRem = m_tracingsData.front();

		int indexToRem = FindIndex(m_sorted20MinTracing, 0, size - 1, toRem);
		m_sorted20MinTracing.erase(m_sorted20MinTracing.begin() + indexToRem);
	}

	size = (int)m_sorted8MinTracing.size();
	if (size >= FHR_8MIN_MEDIAN_WINDOW) // 2880 = 4 minutes in quarter seconds 
	{
		m_median8MinResult.push_back(m_sorted8MinTracing.at(FHR_8MIN_HALF_WINDOW));
		unsigned char toRem;
		if (m_lastInsertedIndex >= FHR_8MIN_MEDIAN_WINDOW)
			toRem = m_tracingsData.at(m_lastInsertedIndex - FHR_8MIN_WINDOW);
		else
			toRem = m_tracingsData.front();

		int indexToRem = FindIndex(m_sorted8MinTracing, 0, size - 1, toRem);
		m_sorted8MinTracing.erase(m_sorted8MinTracing.begin() + indexToRem);
	}
}

void FiltersCalculator::CalculateMedianResidual()
{
	int tracingSize = (int)m_tracingsData.size();
	// 20 minutes median residual
	// the "if" filters the beginning of the strip (before we accumulate 10 mins of samples)
	if (tracingSize > FHR_20MIN_HALF_WINDOW && m_median20MinResult.size() > 0)
	{
		short calcSample = (short)m_tracingsData.at(m_lastInsertedIndex - FHR_20MIN_HALF_WINDOW);
		short medianVal = (short)m_median20MinResult.back();
		m_medianResidual20MinResult.push_back(calcSample - medianVal);
	}

	// 8 minutes median residual
	if (tracingSize > FHR_8MIN_HALF_WINDOW && m_median8MinResult.size() > 0)
	{
		short calcSample = (short)m_tracingsData.at(m_lastInsertedIndex - FHR_8MIN_HALF_WINDOW);
		short medianVal = (short)m_median8MinResult.back();
		m_medianResidual8MinResult.push_back(calcSample - medianVal);
	}
}

void FiltersCalculator::InsertSort(vector<unsigned char>& vecToIns, unsigned char val, int start, int end)
{
	int mid = (start + end) / 2;
	if (end == start + 1)
	{
		if (val >= vecToIns.at(end))
		{
			vecToIns.push_back(val);
			return;
		}
		else if (val >= vecToIns.at(start))
		{
			vecToIns.insert(vecToIns.begin() + end, val);
			return;
		}
	}

	if (start == end && val <= vecToIns.at(start))
	{
		vecToIns.insert(vecToIns.begin() + start, val);
		return;
	}

	if (val == vecToIns.at(mid))
		vecToIns.insert(vecToIns.begin() + mid, val);
	else if (val < vecToIns.at(mid))
		InsertSort(vecToIns, val, start, mid);
	else
		InsertSort(vecToIns, val, mid, end);
}

int FiltersCalculator::FindIndex(vector<unsigned char>& searchVector, int start, int end, unsigned char searchVal)
{
	int mid = (start + end) / 2;
	if (mid == start)
	{
		if (searchVector.at(mid) == searchVal)
			return mid;
		else if (searchVector.at(end) == searchVal)
			return end;
		else
			return -1;
	}

	if (searchVector.at(mid) == searchVal)
		return mid;
	else if (searchVector.at(mid) < searchVal)
		return FindIndex(searchVector, mid, end, searchVal);
	else
		return FindIndex(searchVector, start, mid, searchVal);
}

void FiltersCalculator::CalculateMedianResidualHalfWaves()
{
	if (m_medianResidual20MinResult.size() > 0)
	{
		short lastElem = m_medianResidual20MinResult.back();
		m_medianResidual20MinHalfWavePos.push_back(lastElem < 0 ? -255 : lastElem);
		if (lastElem >= 0)
		{
			if (m_lastPos20MinNOTNaN < m_medianResidual20MinHalfWavePos.size() - 2)
				LinearInterpolation(HalfWavePart::Pos20Min);

			m_lastPos20MinNOTNaN = m_medianResidual20MinHalfWavePos.size() - 1;
		}

		m_medianResidual20MinHalfWaveNeg.push_back(lastElem > 0 ? 255 : lastElem);
		if (lastElem <= 0)
		{
			if (m_lastNeg20MinNOTNaN < m_medianResidual20MinHalfWaveNeg.size() - 2)
				LinearInterpolation(HalfWavePart::Neg20Min);

			m_lastNeg20MinNOTNaN = m_medianResidual20MinHalfWaveNeg.size() - 1;
		}
	}

	if (m_medianResidual8MinResult.size() > 0)
	{
		short lastElem = m_medianResidual8MinResult.back();
		m_medianResidual8MinHalfWavePos.push_back(lastElem < 0 ? -255 : lastElem);
		if (lastElem >= 0)
		{
			if (m_lastPos8MinNOTNaN < m_medianResidual8MinHalfWavePos.size() - 2)
				LinearInterpolation(HalfWavePart::Pos8Min);

			m_lastPos8MinNOTNaN = m_medianResidual8MinHalfWavePos.size() - 1;
		}

		m_medianResidual8MinHalfWaveNeg.push_back(lastElem > 0 ? 255 : lastElem);
		if (lastElem <= 0)
		{
			if (m_lastNeg8MinNOTNaN < m_medianResidual8MinHalfWaveNeg.size() - 2)
				LinearInterpolation(HalfWavePart::Neg8Min);

			m_lastNeg8MinNOTNaN = m_medianResidual8MinHalfWaveNeg.size() - 1;
		}
	}
}

void FiltersCalculator::LinearInterpolation(HalfWavePart part)
{
	switch (part)
	{
	case HalfWavePart::Pos20Min:
		LinearInterpolationInternal(m_medianResidual20MinHalfWavePos, m_lastPos20MinNOTNaN, m_medianResidual20MinHalfWavePos.size() - 1);
		m_lastPos20MinNOTNaN = m_medianResidual20MinHalfWavePos.size() - 1;
		break;
	case HalfWavePart::Neg20Min:
		LinearInterpolationInternal(m_medianResidual20MinHalfWaveNeg, m_lastNeg20MinNOTNaN, m_medianResidual20MinHalfWaveNeg.size() - 1);
		m_lastNeg20MinNOTNaN = m_medianResidual20MinHalfWaveNeg.size() - 1;
		break;
	case HalfWavePart::Pos8Min:
		LinearInterpolationInternal(m_medianResidual8MinHalfWavePos, m_lastPos8MinNOTNaN, m_medianResidual8MinHalfWavePos.size() - 1);
		m_lastPos8MinNOTNaN = m_medianResidual8MinHalfWavePos.size() - 1;
		break;
	case HalfWavePart::Neg8Min:
		LinearInterpolationInternal(m_medianResidual8MinHalfWaveNeg, m_lastNeg8MinNOTNaN, m_medianResidual8MinHalfWaveNeg.size() - 1);
		m_lastNeg8MinNOTNaN = m_medianResidual8MinHalfWaveNeg.size() - 1;
		break;
	};
}

void FiltersCalculator::LinearInterpolationInternal(vector<short>& halfWave, int start, int end)
{
	double slope = (halfWave.at(end) - halfWave.at(start)) / (double)(end - start);
	short startVal = halfWave.at(start);
	for (int i = 1; i < end - start; i++)
	{
		halfWave.at(start + i) = startVal + (i * slope);
	}
}



#include "stdafx.h"
#include "SVMInput.h"

namespace patterns
{

SVMInput::SVMInput(vector<double>& medianBLDeviation, 
			vector<double>& BLVariability, 
			vector<double>& skew4Min, 
			vector<double>& skew20Min, 
			vector<double>& upperDeviation, 
			vector<double>& lowerDeviation)
{
	m_pMemoryXFile = NULL;
	m_toProcessedNum = GetMin(medianBLDeviation.size(), BLVariability.size(), skew4Min.size(), skew20Min.size(), upperDeviation.size(), lowerDeviation.size());
	//int bufferSize =  10 * m_toProcessedNum * sizeof(float) + 4 * sizeof(long);
	m_pMemoryXFile = new(allocator) MemoryXFile();
	if(m_toProcessedNum > 0)
	{
		int n_total_frames = m_toProcessedNum;
		int frame_size = 7;
		m_pMemoryXFile->write(&n_total_frames, sizeof(int), 1);		
		m_pMemoryXFile->write(&frame_size, sizeof(int), 1);
		
		for(int i = 0; i < m_toProcessedNum; i++)
		{
			double medianDeviation = medianBLDeviation[i];
			double variability = BLVariability[i];
			double skew4 = skew4Min[i];
			double skew20 = skew20Min[i]; 
			double upperDev = upperDeviation[i];
			double lowerDev = lowerDeviation[i];
			if(isnan(medianDeviation) || isnan(variability) || isnan(skew4) || isnan(skew20) || isnan(upperDev) || isnan(lowerDev))
			{
				m_toReject.insert(i);
			}
			else
			{
				AddToMemSet(medianDeviation, variability, skew4, skew20, upperDev, lowerDev);
			}
		}
		m_pMemoryXFile->rewind();
	}
}


SVMInput::~SVMInput(void)
{
	
}

void SVMInput::AddToMemSet(double medianDeviation, double BLVariability, double skew4Min, double skew20Min, double upperDeviation, double lowerDeviation)
{
	float fMedianDeviation = (float)medianDeviation;
	m_pMemoryXFile->write(&fMedianDeviation, sizeof(float), 1);
	float fBLVariability = (float)BLVariability;
	m_pMemoryXFile->write(&fBLVariability, sizeof(float), 1);
	float fSkew4Min = (float)skew4Min;
	m_pMemoryXFile->write(&fSkew4Min, sizeof(float), 1);
	float fSkew20Min = (float)skew20Min;
	m_pMemoryXFile->write(&fSkew20Min, sizeof(float), 1);
	float fUpperDeviation = (float)upperDeviation;
	m_pMemoryXFile->write(&fUpperDeviation, sizeof(float), 1);
	float fLowerDeviation = (float) lowerDeviation;
	m_pMemoryXFile->write(&fLowerDeviation, sizeof(float), 1);
	float target = 0;
	m_pMemoryXFile->write(&target, sizeof(float), 1);

}
bool SVMInput::IsBLCandidateRejected(int ind)
{
	return(m_toReject.find(ind) != m_toReject.end());

}

int SVMInput::NumOfRejectedExamples()
{
	return m_toReject.size();
}
}

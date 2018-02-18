#pragma once
#include "MemoryXFile.h"
#include <vector>
#include <set>
using namespace std;
using namespace Torch;

namespace patterns
{
	#define NaN numeric_limits<double>::quiet_NaN()

	class SVMInput : public Object
	{
	public:
		SVMInput(vector<double>& medianBLDeviation, 
			vector<double>& BLVariability, 
			vector<double>& skew4Min, 
			vector<double>& skew20Min, 
			vector<double>& upperDeviation, 
			vector<double>& lowerDeviation);
		~SVMInput(void);
	public: 
		MemoryXFile* GetXFile()
			{return m_pMemoryXFile;}
			int GetNumber()
			// PAW
			//{return m_toProcessedNum;}
			{return m_toProcessedNum - NumOfRejectedExamples();}
		bool IsBLCandidateRejected(int ind);
		int NumOfRejectedExamples();
	protected:
		void AddToMemSet(double medianDeviation, double BLVariability, double skew4Min, double skew20Min, double UpperDeviation, double lowerDeviation);
		bool InitModels();
	private:
		inline int GetMin(int val1, int val2, int val3, int val4, int val5, int val6)
		{ int min1 = min(min(val1, val2), val3);
		 int min2 = min(min(val4, val5), val6);
		return min(min1, min2);}
	private:
		vector<double> m_medianBLDeviation;
		vector<double> m_BLVariability;	
		vector<double> m_skew4Min;
		vector<double> m_skew20Min;
		vector<double> m_upperDeviation;
		vector<double> m_lowerDeviation;

		int m_toProcessedNum;

		MemoryXFile* m_pMemoryXFile;

		set<int> m_toReject;
	};
}


#pragma once
#include "resource.h"

using namespace patterns;

class CTestFetus : public fetus
{

	public:
		CFHRSignal*	 GetFHRSignal(void)		
		{ 
			if (!m_devents)
			{ 
				make_event_detector(); 
			}
			
			return m_devents; 
		}
};

class CPatternsConsoleApp
{

	public:
		CPatternsConsoleApp(void);
		virtual ~CPatternsConsoleApp(void);

		CTestFetus*	 m_pFetusLoad;
		CTestFetus*	 m_pFetus;
		patterns::CFHRSignal * m_pSig;
		string m_strInDir;
		string m_strOutDir;
		string*	 m_pStrFiles;
		string m_strRepIntsDir;
		int m_iNumFiles;
		bool m_bNNtrain;
		int m_iOPlevel;
		int m_iInterOPlevel;
		int m_iBS;
		bool m_bProcessing;
		int m_iMsgLevel;
		long m_lFhrIndex;
		long m_lUpIndex;
		int m_iFileIndex;
		bool m_bWritingOutput;
		bool m_bDisableRepRej;
		bool m_bUseInputRepIntervals;
		bool m_bWaitForThread;
		int m_iTimeAccel;
		bool m_bIsDir;
		long m_lNumAccelDecelIterations;

		// output signals to store
		double*	 m_dRepair;
		double*	 m_dLowPass;
		double*	 m_dFilterVar;
		double*	 m_dMinVar;
		fhrPartSet m_CandBump;
		fhrPartSet m_CandBumpExtend;
		fhrPartSet m_pRepairIntervals;
		long m_lRepairIntervals;
		fhrPartSet m_BaseLines;

		// If want to inject repair intervals for already repaired signal (used for NN training based on old repair)
		int m_iRep;
		fhrPartSet m_pRepairIntervalsIn;

#ifdef MATLAB_OUTPUT
		CMatFhrPartSet m_pMatPartsOut;
		CMatSignal m_pMatSigOut;
		CMatContractionSet m_pMatContractions;
#endif
		void SetBS(int iBS)			{ m_iBS = iBS; }
		void SetOPLevel(int i)		{ m_iOPlevel = i; }
		void SetIntOPLevel(int i)	{ m_iInterOPlevel = i; }
		void SetNNTrain(bool b)		{ m_bNNtrain = b; }
		bool GetInputFiles(string inLoc);
		bool SetupOutputDirs(string strOutDirBase);
		string getIDFromFileName(string fName);
		string getPathFromFileName(string fName);
		string GetFileContent(string filename);
		string FileStrFromMatFile(string filename);
		void ProcessFiles(void);
		void AppendWarmUpBlock(void);
		void AppendBlock(void);
		void Reset(void);

		bool SetRepairIntervalsFromInputFile(void);
		void SetNumAccelDecelIterations(long n);

		void InitOutputSignals(void);
		long GetTotalCount(void);
		void AddToTotalRepair(void);
		void AddToTotalRepairIntervals(void);
		void AddToTotalLowPass(void);
		void AddToTotalFilterVar(void);
		void AddToTotalMinVar(void);
		void AddToTotalBandPass(void);
		void AddToTotalZeroCrossings(void);
		void AddToCandBumps(void);
		void AddToCandBumpsExtend(void);
		void AddFhrPartsFromWindow(fhrPartSet* a, fhrPartSet* b, bool removePending = false);
		void AddToBaseLines(void);
		void AddToTotalContractions(void);

		void AddToTotalSignal(const double* pSignal, long lSignal, double* pTotSignal, long lBuffer);

		string GetOutFileStrAtTime(string strPrefix);

		bool OutputRepairToMat(void);
		bool OutputLowPassToMat(void);
		bool OutputFilterVarToMat(void);
		bool OutputMinVarToMat(void);
		bool OutputSignalToMat(double* pSignal, long lSignal, string strOutFile);
		bool OutputSignalToMat(const double* pSignal, long lSignal, string strOutFile);

		bool OutputRepairIntervalsToMat(void);
		bool OutputBaseLinesToMat(void);
		bool OutputMultiHToMat(long iteration);
		bool OutputBumpCandToMat(void);
		bool OutputExtendBumpCandToMat(long iteration);
		bool OutputFhrPartSetToMat(fhrPartSet* p, string strOutTitle);
		bool OutputBumpClassify(void);
		bool OutputAllBaseLinesToMat();

		void WriteFinalOutput(void);
		string GetOutputFileName(string dirTag);
		bool OutputTotalBaseLines(void);
		bool OutputTotalRepairIntervals(void);
		bool OutputTotalBumpCand(void);
		bool OutputTotalBumpCandExtend(void);
		bool OutputTotalOutput(void);
		bool OutputTotalRepair(void);
		bool OutputTotalLowPass(void);
		bool OutputTotalFilterVar(void);
		bool OutputTotalMinVar(void);
		bool OutputFetusOutput(void);
		bool OutputTotalContractions(void);

		string EscapeDirSlashes(string str);
		bool ThreadNotFinished(void);
		string GetDumpDir(void);
};

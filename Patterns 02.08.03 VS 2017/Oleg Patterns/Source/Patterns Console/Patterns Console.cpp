// Test.cpp : Defines the entry point for the console application.
#include "stdafx.h"

#include "Patterns Console.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
#define ONE_HOUR	14400

void CallBack(int i, void* );

// The one and only application object CWinApp theApp;
CPatternsConsoleApp a;

int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	if (argc < 4)
	{
		printf("PatternsConsole <inXML | inXMLDir> <outDir> <BS> [opLevel] [interOPlevel] [trainNN] [disableRep] [inRepIntDir]");

		// inRepInt is for NN training - if using repaired input to train need to specify repair intervals for that
		// repaired data These should be in text format and can be generated using MATLAB script
		// "convertRepIntsToTxtDirSet"
		exit(1);
	}
	else
	{
		if (!(a.GetInputFiles(argv[1])))
		{
			printf("Could not get input file(s)\n");
			exit(1);
		}

		if (!(a.SetupOutputDirs(argv[2])))
		{
			printf("Could not set up output directories\n");
			exit(1);
		}

		a.SetBS(atoi(argv[3]));
		if (argc > 4)
		{
			a.SetOPLevel(atoi(argv[4]));
		}

		if (argc > 5)
		{
			a.SetIntOPLevel(atoi(argv[5]));
		}

		if (argc > 6)
		{
			if (atoi(argv[6]) == 0)
			{
				a.SetNNTrain(false);
			}
			else
			{
				a.SetNNTrain(true);
				a.m_lNumAccelDecelIterations = atoi(argv[6]);
				if (a.m_iInterOPlevel < 1)
				{
					a.SetIntOPLevel(1); // needs to be at least 2 to get bump candidates
				}

				if (a.m_iOPlevel < 2)
				{
					a.SetOPLevel(2);
				}

				if (argc < 9)
				{
					printf("No repair intervals directory specified - needed for NN training\n");
					exit(1);
				}
			}
		}

		if (argc > 7)
		{
			if (atoi(argv[7]) == 1)
			{
				a.m_bDisableRepRej = true;
			}
		}

		if (argc > 8)
		{
			a.m_bUseInputRepIntervals = true;
			a.m_strRepIntsDir = argv[8];
		}
	}

	a.ProcessFiles();

	return 0;
}

CPatternsConsoleApp::CPatternsConsoleApp(void)
{
	m_pFetusLoad = NULL;
	m_pFetus = NULL;
	m_pSig = NULL;
	m_strInDir = "";
	m_strOutDir = "";
	m_strRepIntsDir = "";
	m_pStrFiles = NULL;
	m_iNumFiles = 0;
	m_bNNtrain = false;
	m_iOPlevel = 0;
	m_iInterOPlevel = 0;
	m_iBS = 60; // in seconds
	m_bProcessing = false;
	m_iMsgLevel = 2;
	m_lFhrIndex = 0;
	m_lUpIndex = 0;
	m_iFileIndex = 0;
	m_bWritingOutput = false;
	m_bDisableRepRej = false;
	m_bUseInputRepIntervals = false;
	m_bWaitForThread = true;
	m_iTimeAccel = 100;
	m_lNumAccelDecelIterations = 2;

	m_dRepair = NULL;
	m_dLowPass = NULL;
	m_dFilterVar = NULL;
	m_dMinVar = NULL;
	m_bIsDir = false;

	m_iRep = 0;
}

CPatternsConsoleApp::~CPatternsConsoleApp(void)
{
	Reset();

	if (m_pStrFiles)
	{
		if (m_bIsDir)
		{
			delete[] m_pStrFiles;
		}
		else
		{
			delete m_pStrFiles;
		}
	}
}

void CPatternsConsoleApp::Reset(void)
{
	if (m_pFetus)
	{
		delete m_pFetus;
	}

	m_pFetus = NULL;

	if (m_pFetusLoad)
	{
		delete m_pFetusLoad;
	}

	m_pFetusLoad = NULL;

	m_pSig = NULL;

	m_bProcessing = false;
	m_lFhrIndex = 0;
	m_lUpIndex = 0;

	// intermed signals
	if (m_dRepair)
	{
		delete[] m_dRepair;
	}

	m_dRepair = NULL;
	if (m_dLowPass)
	{
		delete[] m_dLowPass;
	}

	m_dLowPass = NULL;
	if (m_dMinVar)
	{
		delete[] m_dMinVar;
	}

	m_dMinVar = NULL;
	if (m_dFilterVar)
	{
		delete[] m_dFilterVar;
	}

	m_dFilterVar = NULL;

	m_CandBump.clear();
	m_CandBumpExtend.clear();
	m_pRepairIntervals.clear();

	m_BaseLines.clear();
	m_pRepairIntervalsIn.clear();
}

bool CPatternsConsoleApp::GetInputFiles(string inLoc)
{
	int n = 0;
	bool bRC = false;
	string strFile;

	// size_t argLen;
	if (m_pStrFiles)
	{
		if (m_bIsDir)
		{
			delete[] m_pStrFiles;
		}
		else
		{
			delete m_pStrFiles;
		}
	}

	FILE*  f = fopen(inLoc.c_str(), "r");

	if (f)
	{
		m_bIsDir = false;
		m_iNumFiles = 1;
		m_pStrFiles = new string;
		(*m_pStrFiles) = inLoc;
		m_strInDir = getPathFromFileName(inLoc);
		bRC = true;
	}
	else
	{
		m_bIsDir = true;
		m_strInDir = inLoc;

		CFileFind finder;
		string strWC(inLoc);

		strWC += "\\*.xml";

		BOOL bWorking = finder.FindFile(strWC.c_str());

		while (bWorking)
		{
			bRC = true;
			bWorking = finder.FindNextFile();
			n++;
		}

		m_pStrFiles = new string[n];
		m_iNumFiles = n;
		n = 0;
		bWorking = finder.FindFile(strWC.c_str());
		while (bWorking)
		{
			bWorking = finder.FindNextFile();

			CString s = finder.GetFilePath();

			m_pStrFiles[n++] = string(s);
		}

		finder.Close();
	}

	return bRC;
}

string CPatternsConsoleApp::getIDFromFileName(string fName)
{
	string strID = "";
	int s1;
	int s1b;
	int s2;

	s2 = (int)fName.rfind(".xml");
	if (s2 == -1)
	{
		s2 = (int)fName.rfind(".mat");
	}

	if (s2 == -1)
	{
		s2 = (int)fName.rfind(".in");
	}

	s1 = (int)fName.rfind("/");
	s1b = (int)fName.rfind("\\");
	if (s1 < s1b)
	{
		s1 = s1b;
	}

	int len = s2 - s1 - 1;

	if (s1 < s2)
	{
		strID = fName.substr(s1 + 1, len);
	}

	return strID;
}

string CPatternsConsoleApp::getPathFromFileName(string fName)
{
	string path = "";
	int s1;
	int s1b;

	s1 = (int)fName.rfind("/");
	s1b = (int)fName.rfind("\\");
	if (s1 < s1b)
	{
		s1 = s1b;
	}

	path = fName.substr(1, s1 - 1);

	return path;
}

string CPatternsConsoleApp::GetFileContent(string filename)
{
	if ((filename.rfind(".mat") != string::npos))
	{
		return FileStrFromMatFile(filename);
	}

	string str;
	CFile*	pFile = new CFile(filename.c_str(), CFile::modeRead | CFile::shareDenyNone);
	if (pFile)
	{
		long n = (long)pFile->GetLength();
		char* buf = new char[n+1];
		if (buf)
		{
			memset(buf, 0, n + 1);
			pFile->Read(buf, n);

			str = buf;
			delete buf;
		}

		pFile->Close();
		delete pFile;
	}

	return str;
}

string CPatternsConsoleApp::FileStrFromMatFile(string filename)
{
	string str("");

#ifdef MATLAB_OUTPUT
	long nfhr, nup;
	int*  fhr, *up;

	CMatSignal matSig;
	matSig.SetFilename(filename);
	matSig.ReadFromFile(&fhr, &nfhr, &up, &nup);
	str = matSig.ToInFileString(fhr, nfhr, up, nup);
	if (fhr)
	{
		delete[] fhr;
	}

	if (up)
	{
		delete[] up;
	}

#else
	printf("Cannot read .mat files - use Patterns Console MATLAB - will need matlab libraries");
	exit(1);
#endif
	return (str);
}

// =====================================================================================================================
//    NOTE: If MATLAB_OUTPUT is specified, the CallBack function in the CFHRSignal is set to a local CallBack. Appends
//    are blocked by m_bProcessing which is set to true before a block append, and set back to false in the local
//    CallBack function when processing of a block is finished AND any output is written to file. If MATLAB_OUTPUT is not
//    set, the CallBack function is left as the m_pFetus (patterns::fetus) note_event_detection. Appends are blocked by
//    checking that the thread is still running.
// =======================================================================================================================
void CPatternsConsoleApp::ProcessFiles(void)
{
	for (int i = 0; i < m_iNumFiles; i++)
	{
		Reset();
		m_iFileIndex = i;
		printf("\n----\nProcessing input file %s\n", m_pStrFiles[i].c_str());
		m_pFetusLoad = new CTestFetus;	// this just stores loaded up and fhr
		m_pFetus = new CTestFetus;

		// m_pFetus->make_event_detector();
		string str = GetFileContent(m_pStrFiles[i]);

		// CMat
		m_pFetusLoad->import(str);
		InitOutputSignals();
		m_pFetus->set_as_real_time(true);
		m_pFetus->set_up_sample_rate(m_pFetusLoad->get_up_sample_rate());
		m_pFetus->set_hr_sample_rate(m_pFetusLoad->get_hr_sample_rate());

		// need to wait until first append
		m_pFetus->set_as_real_time(true);
		m_pFetus->set_up_sample_rate(m_pFetusLoad->get_up_sample_rate());
		m_pFetus->set_hr_sample_rate(m_pFetusLoad->get_hr_sample_rate());

		printf("About %d hours to process\n", (m_pFetusLoad->get_number_of_fhr() / (m_pFetusLoad->get_hr_sample_rate() * 60 * 60)));

		AppendWarmUpBlock();			// enough for warmup - this will set m_pSig and callback
		while (m_lFhrIndex < m_pFetusLoad->get_number_of_fhr())
		{
			AppendBlock();
		}

		WriteFinalOutput();
	}
}

void CPatternsConsoleApp::AppendWarmUpBlock(void)
{
	m_bProcessing = true;				// this will be reset in CallBack and prevent more appends

	// m_pFetus->activate_baseline_variability(true);
	m_pSig = m_pFetus->GetFHRSignal();
	m_pSig->SetXMLFilePrefix(GetDumpDir() + "\\");
	m_pSig->SetPathToTestFiles(GetDumpDir() + "\\");

#ifdef MATLAB_OUTPUT
	m_pSig->SetCallBackFunction(CallBack, 0);
#endif
	m_pSig->SetMinSamplesAppend(1);
	m_pSig->setStoreXML(2);
	m_pSig->setStoreMAT(1);
	m_pSig->SetNumAccelDecelIterations(m_lNumAccelDecelIterations);
	if (m_bNNtrain)
	{
		m_pSig->SetNNtrainingParams();
	}

	if (m_bUseInputRepIntervals)
	{
		m_pSig->DisableRepair(true);
		if (!(SetRepairIntervalsFromInputFile()))
		{
			printf("Could not read input repair intervals from file\n");
			exit(1);
		}
	}

	if (m_bDisableRepRej)
	{
		m_pSig->SetRemoveRepairOutput(false);
	}
}

void CPatternsConsoleApp::AppendBlock(void)
{
	m_bProcessing = true;

	long hrCount = m_pFetusLoad->get_number_of_fhr();
	long upCount = m_pFetusLoad->get_number_of_up();

	int hrRate = m_pFetus->get_hr_sample_rate();
	int upRate = m_pFetus->get_up_sample_rate();

	vector<char> hrs;
	hrs.reserve(m_iBS * hrRate);

	vector<char> ups;
	ups.reserve(m_iBS * upRate);

	// Collect the hr & up to add
	for (int i = 0; i < m_iBS; ++i)
	{
		for (int j = 0; j < hrRate; ++j)
		{
			if (m_lFhrIndex < hrCount)
			{
				hrs.push_back((char)m_pFetusLoad->get_fhr(m_lFhrIndex));
				m_lFhrIndex++;
			}
		}

		for (int j = 0; j < upRate; ++j)
		{
			if (m_lUpIndex < upCount)
			{
				ups.push_back((char)m_pFetusLoad->get_up(m_lUpIndex));
				m_lUpIndex++;
			}
		}
	}

	// Add the data !
	m_pFetus->append_up(ups);
	m_pFetus->append_fhr(hrs);

	printf(".");
}

void CallBack(int i, void* p)
{
	// 1 - Appending 2 - MinVar finished 3 - Baseline Detection done 4 - MultiH done 5 - Accel/Decel done -100 -
	// * Repair exception -101 - LowPass exception -102 VarPass exception -103 - MinVar exception -104 - Baseline
	// * Detection exception -105 - multiH exception -106 - Accel/Decel exception -1000 - flushing -250 jumpMode (flat
	// * Repair?) -400 ? ;
	// * CPatternsConsoleApp *pApp = dynamic_cast<CPatternsConsoleApp*> (p);
	// * if
	switch (i)
	{
	case 2:
		{						// repair, lowpass, filterVar, minVar
			if (a.m_iOPlevel > 0)
			{
				a.AddToTotalRepair();
				if (a.m_iInterOPlevel > 1)
				{
					a.OutputRepairToMat();
				}
			}

			if (a.m_iOPlevel > 1)
			{
				a.AddToTotalRepairIntervals();
				a.AddToTotalLowPass();
				a.AddToTotalFilterVar();
				a.AddToTotalMinVar();
				if (a.m_iInterOPlevel > 2)
				{
					a.OutputRepairIntervalsToMat();
					a.OutputLowPassToMat();
					a.OutputFilterVarToMat();
					a.OutputMinVarToMat();
				}
			}
			break;
		}

	case 3:						// baseline detection
		{
			if (a.m_iOPlevel > 0)
			{
				a.AddToBaseLines();
				if (a.m_iInterOPlevel > 1)
				{
					a.OutputBaseLinesToMat();
					a.OutputAllBaseLinesToMat();
				}
			}
			break;
		}

	case 4:						// multiH
		{
			if (a.m_iOPlevel > 0)
			{
				if (a.m_iInterOPlevel > 1)
				{
					a.OutputMultiHToMat(0);
				}
			}
			break;
		}

	case 5:
		{
			if (a.m_iOPlevel > 0)
			{
				a.AddToCandBumps();
				a.AddToCandBumpsExtend();
				a.AddToTotalContractions();
				if (a.m_iInterOPlevel > 1)
				{
					a.OutputBumpCandToMat();
					a.OutputExtendBumpCandToMat(-1);
				}
			}

			if (a.m_iInterOPlevel > 0)
			{
				a.OutputBumpClassify();
			}

			a.m_bProcessing = false;
			break;
		}

	case 11:
		{
			long iteration = a.m_pSig->GetCurrAccelDecelIteration();
			if (a.m_iInterOPlevel > 1)
			{
				a.OutputExtendBumpCandToMat(iteration);
			}
			break;
		}

	case 12:
		{
			long iteration = a.m_pSig->GetCurrAccelDecelIteration();
			if (a.m_iInterOPlevel > 1)
			{					// for multiH, the original multiH will have already been output as multiH_0 (see
				///callback 4) before accel decel
				///;
				///so last iteration we want to write as multiH, and other iterations as
				///multiH_n+1 For bump candidates, the final version of the candidates is output
				///AFTER all accel decel iterations are done
				if (iteration + 2 == a.m_pSig->GetNumAccelDecelIterations())
				{
					a.OutputMultiHToMat(-1);
				}
				else
				{
					a.OutputMultiHToMat(iteration + 1);
				}
			}
			break;
		}

	default:
		break;
	}

	if ((i <= -100) && (i > -150))	// error
	{
		printf("Error:  Callback returned %d at time %d\n", i, a.m_pSig->GetTotalPtsCount());
		exit(1);
	}
}

// =====================================================================================================================
//    Buffers for storing intermediate output
// =====================================================================================================================
void CPatternsConsoleApp::InitOutputSignals(void)
{
	long lNumSamples = m_pFetusLoad->get_number_of_fhr();

	if (m_iOPlevel > 1)
	{
		m_dLowPass = new double[lNumSamples];
		m_dFilterVar = new double[lNumSamples];
		m_dMinVar = new double[lNumSamples];
	}

	if (m_iOPlevel > 0)
	{
		m_dRepair = new double[lNumSamples];
	}
}

void CPatternsConsoleApp::AddToTotalRepair(void)
{
	long lSignal = m_pSig->GetPtsCount();
	const double*  pSignal = m_pSig->GetSignal();

	// long lBuffer = 10 * 240;
	// // if have dropout of more than this then combined signals from RT will not match Batch
	long lBuffer = GetTotalCount() + (m_iBS * m_pFetus->get_hr_sample_rate()) - m_pSig->GetStableRepairIndex();

	AddToTotalSignal(pSignal, lSignal, m_dRepair, lBuffer);
}

void CPatternsConsoleApp::AddToTotalLowPass(void)
{
	long lSignal = m_pSig->GetLowPassCount();
	const double*  pSignal = m_pSig->GetLowPassSignal();
	long lBuffer = 10 * 240;	// this is just approx
	AddToTotalSignal(pSignal, lSignal, m_dLowPass, lBuffer);
}

void CPatternsConsoleApp::AddToTotalFilterVar(void)
{
	// long lSignal = m_pSig->GetTotalPtsCount();
	long lSignal = m_pSig->GetVariabilityCount();
	const double*  pSignal = m_pSig->GetVarPassSignal();
	long lBuffer = 10 * 240;

	AddToTotalSignal(pSignal, lSignal, m_dFilterVar, lBuffer);
}

// =====================================================================================================================
//    MinVar is signal indexes so need to shift them - this is a bit more complex
// =====================================================================================================================
void CPatternsConsoleApp::AddToTotalMinVar(void)
{
	long lSignal = m_pSig->GetPtsCount();
	const double*  pSignal = m_pSig->GetMinVarSignal();
	long lBuffer = 10 * 240;
	long lTotalCount = GetTotalCount();
	long lStartIndex = 0;
	long lOffset = lTotalCount - lSignal + m_pSig->GetNumExtrapolatedSignal();

	if (lOffset > 0)
	{	// first part is sketchy - if not on first run only need last append + a bit
		lStartIndex = max(0, lTotalCount - (m_iBS * m_pFetus->get_hr_sample_rate()) - lBuffer);
	}

	for (long i = lStartIndex; i < lTotalCount; i++)
	{
		m_dMinVar[i] = pSignal[i - lOffset] + lOffset;
	}
}

void CPatternsConsoleApp::AddToTotalSignal(const double* pSignal, long lSignal, double* pTotSignal, long lBuffer)
{
	long lTotalCount = GetTotalCount();
	long lStartIndex = 0;
	long lOffset = lTotalCount - m_pSig->GetPtsCount() + m_pSig->GetNumExtrapolatedSignal();
	long lDelay = m_pSig->GetPtsCount() - lSignal;

	if (lOffset > 0)
	{	// first part is sketchy - if not on first run only need last append + a bit
		lStartIndex = max(lOffset, lTotalCount - (m_iBS * m_pFetus->get_hr_sample_rate()) - lBuffer);
	}

	for (long i = lStartIndex; i < lTotalCount - lDelay; i++)
	{
		pTotSignal[i] = pSignal[i - lOffset];
	}

	for (long i = lTotalCount - lDelay; i < lTotalCount; i++)
	{
		pTotSignal[i] = 0.0;	// pad with zeroes if it is a filtered signal such that it has same # samples as raw
	}
}

void CPatternsConsoleApp::AddToCandBumps(void)
{
	fhrPartSet*	 pNewBumps = m_pSig->GetCandBumps();
	AddFhrPartsFromWindow(&m_CandBump, pNewBumps, true);
}

void CPatternsConsoleApp::AddToCandBumpsExtend(void)
{
	fhrPartSet*	 pNewBumps = m_pSig->GetCandBumpsExtend();
	AddFhrPartsFromWindow(&m_CandBumpExtend, pNewBumps, true);
}

void CPatternsConsoleApp::AddFhrPartsFromWindow(fhrPartSet* a, fhrPartSet* b, bool removePending)
{
	long lTotalCount = GetTotalCount();
	long lSignal = m_pSig->GetPtsCount();
	long lOffset = lTotalCount - lSignal + m_pSig->GetNumExtrapolatedSignal();

	fhrPartSet temp;
	temp.addcopy(b);
	if (removePending)
	{
		temp.removePending();
	}

	temp += lOffset;
	temp.setClearMemory(false);
	a->add(&temp);
}

void CPatternsConsoleApp::AddToBaseLines(void)
{
	fhrPartSet*	 BaseLines = m_pSig->GetNewBaseLines();
	AddFhrPartsFromWindow(&m_BaseLines, BaseLines);
}

void CPatternsConsoleApp::AddToTotalRepairIntervals(void)
{
	fhrPartSet*	 repInts = m_pSig->GetRepairIntervals();
	long lCurrRepInts = repInts->size();
	long lLastRepIntX2 = -1;
	long lStableIndex = m_pSig->GetStableRepairIndex();
	long lSignal = m_pSig->GetPtsCount();
	long lTotalCount = GetTotalCount();
	long lOffset = lTotalCount - lSignal + m_pSig->GetNumExtrapolatedSignal();
	long lBS = m_pSig->GetMinSamplesAppend();
	long n = m_pRepairIntervals.size();

	if (n > 0)
	{
		lLastRepIntX2 = m_pRepairIntervals.getAt(n - 1)->getX2();
	}

	// now get repair intervals between lastX2 and stable repair
	for (long i = 0; i < lCurrRepInts; i++)
	{
		fhrPart*  p = repInts->getAt(i);
		if ((p->getX1() < lStableIndex) && (p->getX1() > lLastRepIntX2))
		{
			m_pRepairIntervals.addcopy(p);
		}
		else if ((lStableIndex > lOffset) && (lStableIndex <= lOffset + lBS) && (p->getX1() >= lLastRepIntX2))
		{
			m_pRepairIntervals.addcopy(p);
		}
	}
}

void CPatternsConsoleApp::AddToTotalContractions(void)
{
#ifdef MATLAB_OUTPUT
	long lTotalCount = GetTotalCount();
	long lSignal = m_pSig->GetPtsCount();
	long lOffset = lTotalCount - lSignal + m_pSig->GetNumExtrapolatedSignal();
	long lNumC = m_pSig->m_lContrDet;
	long lCurrSize = m_pMatContractions.GetSize();
	long lLastXend = m_pMatContractions.GetLastXEnd();
	const ContractionDetection*	 pC = m_pSig->m_ContractDetect;

	for (long i = 0; i < lNumC; i++)
	{
		ContractionDetection c = pC[i];

		c.lEnd += lOffset;
		if (c.lEnd > lLastXend)
		{
			c.lPeak += lOffset;
			c.lStart += lOffset;
			m_pMatContractions.Add(c);
		}
	}
#endif
}

long CPatternsConsoleApp::GetTotalCount(void)
{
	return m_pSig->GetTotalPtsCount();
}

bool CPatternsConsoleApp::SetRepairIntervalsFromInputFile(void)
{
	bool bRC = true;

	// first get file name
	string strFileID = getIDFromFileName(m_pStrFiles[m_iFileIndex]);
	string repFile = m_strRepIntsDir;
	long x1, x2;

	repFile += "\\";
	repFile += strFileID;
	repFile += ".txt";

	CStdioFile*	 srRep = new CStdioFile(repFile.c_str(), CFile::modeRead | CFile::typeText | CFile::shareDenyWrite);

	if (srRep)
	{
		CString str;
		BOOL bRead = srRep->ReadString(str);

		if (bRead)
		{
			m_iRep = atol(str);
		}
		else
		{
			bRC = false;
		}

		m_pRepairIntervalsIn.clear();
		for (int i = 0; i < m_iRep; i++)
		{
			srRep->ReadString(str);
			x1 = atoi(str);
			srRep->ReadString(str);
			x2 = atoi(str);

			fhrPart*  p = new fhrPart(x1, x2);
			m_pRepairIntervalsIn.add(p);
		}

		m_pSig->SetRepairIntervals(&m_pRepairIntervalsIn);
	}

	if (srRep)
	{
		delete srRep;
	}

	return bRC;
}

// =====================================================================================================================
//    MATLAB OUTPUT
// =======================================================================================================================
bool CPatternsConsoleApp::OutputRepairToMat(void)	// seq number of the Frequency Band
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	// long lSignal = m_pSig->GetTotalPtsCount();
	long lSignal = m_pSig->GetPtsCount();
	const double*  pSignal = m_pSig->GetSignal();
	string strOutFile = GetOutFileStrAtTime("Repair");

	bRC = OutputSignalToMat(pSignal, lSignal, strOutFile);
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputLowPassToMat(void)	// seq number of the Frequency Band
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	long lSignal = m_pSig->GetLowPassCount();
	const double*  pSignal = m_pSig->GetLowPassSignal();
	string strOutFile = GetOutFileStrAtTime("FilterLP");

	bRC = OutputSignalToMat(pSignal, lSignal, strOutFile);
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputFilterVarToMat(void)	// seq number of the Frequency Band
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	long lSignal = m_pSig->GetVariabilityCount();
	const double*  pSignal = m_pSig->GetVarPassSignal();
	string strOutFile = GetOutFileStrAtTime("FilterVar");

	bRC = OutputSignalToMat(pSignal, lSignal, strOutFile);
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputMinVarToMat(void)		// seq number of the Frequency Band
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	long lSignal = m_pSig->GetTotalPtsCount();
	const double*  pSignal = m_pSig->GetMinVarSignal();
	string strOutFile = GetOutFileStrAtTime("MinVar");

	bRC = OutputSignalToMat(pSignal, lSignal, strOutFile);
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputSignalToMat(const double* pSignal, long lSignal, string strOutFile)
{
	bool bRC = false;

#ifdef MATLAB_OUTPUT
	if (pSignal == NULL)
	{
		bRC = false;
	}
	else
	{
		m_pMatSigOut.SetFilename(strOutFile);
		m_pMatSigOut.SetSaveUD(true);
		m_pMatSigOut.SetSignal(pSignal);
		m_pMatSigOut.SetSignalLength(lSignal);

		bRC = (m_pMatSigOut.OutputToFile());
	}
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputSignalToMat(double* pSignal, long lSignal, string strOutFile)
{
	bool bRC = false;

#ifdef MATLAB_OUTPUT
	if (pSignal == NULL)
	{
		bRC = false;
	}
	else
	{
		m_pMatSigOut.SetFilename(strOutFile);
		m_pMatSigOut.SetSaveUD(true);
		m_pMatSigOut.SetSignal(pSignal);
		m_pMatSigOut.SetSignalLength(lSignal);

		bRC = (m_pMatSigOut.OutputToFile());
	}
#endif
	return bRC;
}

string CPatternsConsoleApp::GetOutFileStrAtTime(string strPrefix)
{
	long lTotalCount = GetTotalCount();
	string ID = getIDFromFileName(m_pStrFiles[m_iFileIndex]);
	char buff[10];

	itoa(lTotalCount, buff, 10);

	string strOutFile = m_strOutDir + "\\output\\" + ID + "\\" + strPrefix + "_" + buff + ".mat";

	return strOutFile;
}

bool CPatternsConsoleApp::OutputRepairIntervalsToMat(void)
{
	string strOutFile = GetOutFileStrAtTime("RepairIntervals");
	long lRepInts = m_pSig->GetNumRepairIntervals();
	fhrPartSet*	 pRepInts = m_pSig->GetRepairIntervals();

	return OutputFhrPartSetToMat(pRepInts, strOutFile);
}

bool CPatternsConsoleApp::OutputBaseLinesToMat(void)
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	string strOutFile = GetOutFileStrAtTime("BaseLines");
	fhrPartSet*	 p = m_pSig->GetNewBaseLines();

	bRC = OutputFhrPartSetToMat(p, strOutFile);
#endif
	return bRC;
}

// =====================================================================================================================
//    Output full buffer of baselines - m_pTotalBaselines and Pending. Use OutputBaselinesToMat to get only new baselines
//    in window ;
//    Note that these will be in absolute indexing
// =======================================================================================================================
bool CPatternsConsoleApp::OutputAllBaseLinesToMat()
{
	bool bRC = false;

#ifdef MATLAB_OUTPUT
	string strOutFile = GetOutFileStrAtTime("BaseLinesAll");
	fhrPartSet allBas;
	allBas.addcopy(m_pSig->GetTotalBaseLines());
	allBas.addcopy(m_pSig->GetPendingBaseLines());

	bRC = OutputFhrPartSetToMat(&allBas, strOutFile);
#endif
	return (bRC);
}

bool CPatternsConsoleApp::OutputMultiHToMat(long iteration)
{
	bool bRC = false;
	char buff[10];
	ltoa(iteration, buff, 10);

	string outSuffix = "multiH";
#ifdef MATLAB_OUTPUT
	if (iteration >= 0)
	{
		outSuffix = outSuffix + "_" + buff;
	}

	string strOutFile = GetOutFileStrAtTime(outSuffix.c_str());
	fhrPartSet*	 p = m_pSig->GetMultiH();

	bRC = OutputFhrPartSetToMat(p, strOutFile);
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputBumpClassify(void)
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	fhrPartSet*	 Bumps = m_pSig->GetResultObjects();
	string strOutFile = GetOutFileStrAtTime("BumpClassify");

	bRC = OutputFhrPartSetToMat(Bumps, strOutFile);
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputFhrPartSetToMat(fhrPartSet* p, string strOutFile)
{
	bool bRC = false;

#ifdef MATLAB_OUTPUT
	m_pMatPartsOut.SetFilename(strOutFile);
	m_pMatPartsOut.SetSaveUD(true);

	// m_pMatPartsOut.SetSize(nOO);
	m_pMatPartsOut.SetFhrParts(p);

	bRC = (m_pMatPartsOut.OutputToFile());
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputBumpCandToMat(void)
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	string strOutFile = GetOutFileStrAtTime("BumpCandidates");
	fhrPartSet*	 Bumps = m_pSig->GetCandBumps();

	bRC = OutputFhrPartSetToMat(Bumps, strOutFile);
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputExtendBumpCandToMat(long iteration)
{
	bool bRC = false;
	char buff[10];
	ltoa(iteration, buff, 10);

	string outSuffix = "BumpCandExtend";
	if (iteration >= 0)
	{						// don't put tag if -1
		outSuffix = outSuffix + "_" + buff;
	}

	string strOutFile = GetOutFileStrAtTime(outSuffix);
	fhrPartSet*	 Bumps = m_pSig->GetCandBumpsExtend();

	return OutputFhrPartSetToMat(Bumps, strOutFile);
}

void CPatternsConsoleApp::WriteFinalOutput(void)
{
	// if (m_iBS == -1) // Batch - for now just take xml output OutputFetusOutput();
	// else {
	if (m_iOPlevel > 1)
	{
		OutputTotalLowPass();
		OutputTotalFilterVar();
		OutputTotalMinVar();
		OutputTotalRepairIntervals();

		// Note - if you want Band Pass / ZC then you need to dump these directly from the engine They are not
		// retrievable from the client
	}

	if (m_iOPlevel > 0)
	{
		OutputTotalBaseLines();
		OutputTotalRepair();
		OutputTotalBumpCand();
		OutputTotalBumpCandExtend();
		OutputTotalContractions();
	}

	OutputTotalOutput();	// this comes direct from CFHRSignal object (.mat)

#ifndef MATLAB_OUTPUT
	OutputFetusOutput();	// this comes from patterns::fetus object (.xml)
#endif

	// }
}

string CPatternsConsoleApp::GetOutputFileName(string dirTag)
{
	string ID = getIDFromFileName(m_pStrFiles[m_iFileIndex]);
	string strOutFile = m_strOutDir + "\\" + dirTag + "\\" + ID + ".mat";

	return strOutFile;
}

bool CPatternsConsoleApp::OutputTotalBaseLines(void)
{
	string strOutFile = GetOutputFileName("baselineDetection");

	return OutputFhrPartSetToMat(&m_BaseLines, strOutFile);
}

bool CPatternsConsoleApp::OutputTotalRepairIntervals(void)
{
	string strOutFile = GetOutputFileName("repairIntervals");

	return OutputFhrPartSetToMat(&m_pRepairIntervals, strOutFile);
}

bool CPatternsConsoleApp::OutputTotalContractions(void)
{
	bool bRC = false;
#ifdef MATLAB_OUTPUT
	string strOutFile = GetOutputFileName("contractionDetection");
	m_pMatContractions.SetFilename(strOutFile);
	bRC = m_pMatContractions.OutputToFile();
#endif
	return bRC;
}

bool CPatternsConsoleApp::OutputTotalBumpCand(void)
{
	string strOutFile = GetOutputFileName("bumpCandidates");

	return OutputFhrPartSetToMat(&m_CandBump, strOutFile);
}

bool CPatternsConsoleApp::OutputTotalBumpCandExtend(void)
{
	string strOutFile = GetOutputFileName("bumpCandExtend");
	if (m_bNNtrain)
	{
		m_CandBumpExtend.sortByEnd();
		m_CandBumpExtend.removeRepeat();
	}

	return OutputFhrPartSetToMat(&m_CandBumpExtend, strOutFile);
}

bool CPatternsConsoleApp::OutputTotalOutput(void)
{
	bool bRC = false;

	fhrPartSet*	 Bumps = m_pSig->GetTotalResults();
	if (Bumps->size() > 0)
	{
		string strOutFile = GetOutputFileName("BumpClassify");
		bRC = OutputFhrPartSetToMat(Bumps, strOutFile);
	}

	return bRC;
}

bool CPatternsConsoleApp::OutputTotalRepair(void)
{
	string strOutFile = GetOutputFileName("repaired");

	return OutputSignalToMat(m_dRepair, GetTotalCount(), strOutFile);
}

bool CPatternsConsoleApp::OutputTotalLowPass(void)
{
	string strOutFile = GetOutputFileName("filterLp");

	return OutputSignalToMat(m_dLowPass, GetTotalCount(), strOutFile);
}

bool CPatternsConsoleApp::OutputTotalFilterVar(void)
{
	string strOutFile = GetOutputFileName("filterVar");

	return OutputSignalToMat(m_dFilterVar, GetTotalCount(), strOutFile);
}

bool CPatternsConsoleApp::OutputTotalMinVar(void)
{
	string strOutFile = GetOutputFileName("minVar");

	return OutputSignalToMat(m_dMinVar, GetTotalCount(), strOutFile);
}

bool CPatternsConsoleApp::OutputFetusOutput(void)
{
	bool bRC = true;
	string ID = getIDFromFileName(m_pStrFiles[m_iFileIndex]);
	string strOutFile = m_strOutDir + "\\devOutput\\" + ID + ".xml";
	CFile*	pFile = new CFile(strOutFile.c_str(), CFile::modeCreate | CFile::modeWrite);

	if (pFile)
	{
		m_pFetus->fetch_events();
		m_pFetus->GetEventsCount();

		// this also fetches pending events
		string c = m_pFetus->export_to_string();

		pFile->Write(c.c_str(), (UINT) c.length());
		pFile->Close();
		delete pFile;
	}
	else
	{
		bRC = false;
	}

	return bRC;
}

bool CPatternsConsoleApp::SetupOutputDirs(string strOutDirBase)
{
	bool bRC = true;

	// Directory Dir;
	string currDir;
	string matDirs[13] =
	{
		"minVar", "filterVar", "filterLp", "repaired", "bumpCandExtend",
		"bumpCandidates", "repairIntervals", "baselineDetection",
		"BumpClassify", "BumpMeasures_Accel", "BumpMeasures_Decel", "output",
		"contractionDetection"
	};

	// m_strOutDir = EscapeDirSlashes(strOutDirBase);
	m_strOutDir = strOutDirBase;

	CreateDirectory(m_strOutDir.c_str(), NULL);

	currDir = m_strOutDir + "\\devOutput";

	CreateDirectory(currDir.c_str(), NULL);

	for (int i = 0; i < 13; i++)
	{
		currDir = strOutDirBase + "\\" + matDirs[i];
		CreateDirectory(currDir.c_str(), NULL);
	}

	for (int i = 0; i < m_iNumFiles; i++)
	{
		string ID = getIDFromFileName(m_pStrFiles[i]);

		currDir = strOutDirBase + "\\output\\" + ID;
		CreateDirectory(currDir.c_str(), NULL);
	}

	return bRC;
}

string CPatternsConsoleApp::EscapeDirSlashes(string str)
{
	string out = str;
	string slash2 = "\\\\";
	string slash1 = "\\";
	string::size_type loc = out.find(slash1, 0);

	while (loc != string::npos)
	{
		out.replace(loc, 1, slash2.c_str(), 2);
		loc = out.find(slash1, loc + 2);
	}

	return out;
}

bool CPatternsConsoleApp::ThreadNotFinished(void)
{
	return m_pSig->IsProcessRunning() || m_pSig->InTransfer();
}

string CPatternsConsoleApp::GetDumpDir(void)
{
	string ID = getIDFromFileName(m_pStrFiles[m_iFileIndex]);

	return m_strOutDir + "\\output\\" + ID;
}

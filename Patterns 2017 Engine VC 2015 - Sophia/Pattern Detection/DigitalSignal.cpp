/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection CLASSES: File contains
* class definition for: CDigitalSignal : Basic digitally sampled signal.
* CFHRSignal : Digitally sampled FHR signal. AUTHORS: Mark Doubson /FFT and
* Filters part /MultiHypothesis /BumpClassifications Vladimir Khavkine /Bumps
* Candidates Robert Morin /Repair part Kingsley Woodward /Neuron network
* calculation Jiang Wu /Baseline detection REVISION COMMNENTS: 06/Dec/2004 KW -
* Code review modification, added comments. 07/Dec/2004 RM - Code review modifs
* on CDigitalSignal, CFHRSignal. 29/Jul/2005 MD - Copyright LMS Medical Systems
* 2004 by Evonium Inc.
*/
#include "StdAFX.h"
#include "DigitalSignal.h"
#include "FHRSegment.h"
#include "Config.h"
#include "OOURA.h"
#include "Math.h"
#include "BumpExtension.h"
#include "AtypicalClassify.h"
#include "Repair.h"
#include "BaseLine.h"
#include "fhrPart.h"
#include <io.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <process.h>
#include "ThreadLock.h"
#include <math.h>
#include <fstream>

//#include "SVMTest.h"
//#include "SVMInput.h"
//#include "SVMModel.h"
namespace patterns
{

#define DEFINE_LOOKBACK			10
#define MH_WINDOW_SIZE			720
#define MH_THRESHOLD			0.8
#define MH_THRESHOLD_SEC		0.8

// PAW
//#define FHR_20MIN_MEDIAN_WINDOW	4801
#define FHR_20MIN_MEDIAN_WINDOW	4800
#define FHR_20MIN_WINDOW		4800
// PAW
//#define FHR_20MIN_HALF_WINDOW	3600
//#define FHR_20MIN_MIN_DATA_SIZE 1200
//#define FHR_20MIN_PAST_WINDOW	3600
//#define FHR_20MIN_FUTURE_WINDOW 1200
#define FHR_20MIN_PAST_WINDOW	4800
#define FHR_20MIN_FUTURE_WINDOW 0

#define FHR_20MIN_MEDIAN_INDEX	2400
// PAW
//#define FHR_4MIN_MEDIAN_WINDOW	1441
//#define FHR_4MIN_WINDOW			1440
//#define FHR_4MIN_HALF_WINDOW	720
//#define FHR_4MIN_MEDIAN_WINDOW	961
#define FHR_4MIN_MEDIAN_WINDOW	960
#define FHR_4MIN_WINDOW			960
//#define FHR_4MIN_HALF_WINDOW	480
//#define FHR_4MIN_PAST_WINDOW	480
#define FHR_4MIN_PAST_WINDOW	960
#define FIR_LPF_DELAY			123
#define FIR_LPF_SIZE			247
#define FHR_2MIN_WINDOW			480

	// bug in overlap add w/ short windows and long filters - use non-overlap add
	// filter call unless window is greater than OOURA FFT libraby max block siz
#define USE_OVERLAP_ADD 0
#define OOURA_NMAX		32768


	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//                                                                                                                         //
	//                                            DigitalSignal implementation                                                 //
	//                                                                                                                         //
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// change to 0 to disable OR to 1 to enable R&D printout
	static int BlNumFile = 1;
	//
	// =======================================================================================================================
	//    Definition of CDigitalSignal members. ;
	//    ! CDigitalSignal::<default constructor>
	// =======================================================================================================================
	//
	CDigitalSignal::CDigitalSignal(void)
	{
		m_ExtendedAtypicalClassification = false;
		CDigitalSignal::ResetSignal();
	}

	//
	// =======================================================================================================================
	//    ! CDigitalSignal::<destructor>
	// =======================================================================================================================
	//
	CDigitalSignal::~CDigitalSignal(void)
	{
	}

	/*
	=======================================================================================================================
	! CDigitalSignal::SetSignal: Sets the accessor function for Signal. The new signal is pointed to by pdSignal and
	must have nPtsCount sampled points in it. \return true iff the new signal could be properly set.
	=======================================================================================================================
	*/
	bool CDigitalSignal::SetSignal(long nPtsCount, double *pdSignal)
	{
		if (m_pdSignal != 0)
		{
			delete[] m_pdSignal;
		}
		m_nPtsCount = nPtsCount;
		m_nTotalCount = nPtsCount;
		m_nTotalAppend = nPtsCount;
		m_pdSignal = pdSignal;

		return true;
	}

	/*
	=======================================================================================================================
	! CDigitalSignal::ResetSignal: Resets the Signal property to NULL. All properties related to the signal are also
	reset appropriately.
	=======================================================================================================================
	*/
	void CDigitalSignal::ResetSignal(void)
	{
		m_nPtsCount = 0;
		m_pdSignal = NULL;
		m_dSmpFreq = 4.0;
	}

	//
	// =======================================================================================================================
	//    Definition of CFHRSignal members. ;
	//    ! CFHRSignal::<default constructor>
	// =======================================================================================================================
	//

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//                                                                                                                         //
	//                                            CyclicArray implementation                                                   //
	//                                                                                                                         //
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	using namespace std;

	CFHRSignal::CyclicArray::CyclicArray()
	{
		clear();
	}

	CFHRSignal::CyclicArray::~CyclicArray()
	{
	}

	double CFHRSignal::CyclicArray::at(int position)
	{
		return m_array[mod(m_currentHead + position)];
	}

	int CFHRSignal::CyclicArray::mod(int position)
	{
		int toRet = position % TRACING_DATA_SIZE;
		if (toRet < 0)
		{
			toRet += TRACING_DATA_SIZE;
			toRet %= TRACING_DATA_SIZE;
		}

		return toRet;
	}

	double CFHRSignal::CyclicArray::back()
	{
		return m_currentTail > -1 ? m_array[m_currentTail] : NaN;
	}

	double CFHRSignal::CyclicArray::front()
	{
		return m_array[m_currentHead];
	}

	void CFHRSignal::CyclicArray::initialize(int size, double val)
	{
		clear();
		int actSize = min(size, TRACING_DATA_MAX_ELEM_SIZE);
		for (int i = 0; i < actSize; ++i)
		{
			push_back(val);
		}
	}

	void CFHRSignal::CyclicArray::clear()
	{
		for (int i = 0; i < TRACING_DATA_SIZE; i++)
		{
			m_array[i] = NaN;
		}

		m_currentHead = 0;
		m_currentTail = 0;
		m_size = 0;
	}

	double CFHRSignal::CyclicArray::remove_head()
	{
		if (m_size == 0)
			return NaN;

		double toRet = m_array[m_currentHead];
		m_array[m_currentHead++] = NaN;
		m_currentHead = mod(m_currentHead);
		--m_size;
		return toRet;
	}

	void CFHRSignal::CyclicArray::push_back(const double value)
	{
		m_array[m_currentTail++] = value;
		++m_size;
		if (m_size > TRACING_DATA_MAX_ELEM_SIZE)
		{
			m_array[m_currentHead++] = NaN;
			m_size = TRACING_DATA_MAX_ELEM_SIZE;
		}

		m_currentTail = mod(m_currentTail);
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//                                                                                                                         //
	//                                         CyclicArray4Min implementation                                                  //
	//                                                                                                                         //
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	CFHRSignal::CyclicArray4Min::CyclicArray4Min()
	{
		clear();
	}

	CFHRSignal::CyclicArray4Min::~CyclicArray4Min()
	{
	}

	double CFHRSignal::CyclicArray4Min::at(int position)
	{
		return m_array[mod(m_currentHead + position)];
	}

	int CFHRSignal::CyclicArray4Min::mod(int position)
	{
		int toRet = position % TRACING_DATA_4MIN_SIZE;
		if (toRet < 0)
		{
			toRet += TRACING_DATA_4MIN_SIZE;
			toRet %= TRACING_DATA_4MIN_SIZE;
		}

		return toRet;
	}

	double CFHRSignal::CyclicArray4Min::back()
	{
		return m_currentTail > -1 ? m_array[m_currentTail] : NaN;
	}

	double CFHRSignal::CyclicArray4Min::front()
	{
		return m_array[m_currentHead];
	}

	void CFHRSignal::CyclicArray4Min::initialize(int size, double val)
	{
		clear();
		int actSize = min(size, TRACING_DATA_4MIN_MAX_ELEM_SIZE);
		for (int i = 0; i < actSize; ++i)
		{
			push_back(val);
		}
	}

	void CFHRSignal::CyclicArray4Min::clear()
	{
		for (int i = 0; i < TRACING_DATA_4MIN_SIZE; i++)
		{
			m_array[i] = NaN;
		}

		m_currentHead = 0;
		m_currentTail = 0;
		m_size = 0;
	}

	double CFHRSignal::CyclicArray4Min::remove_head()
	{
		if (m_size == 0)
			return NaN;

		double toRet = m_array[m_currentHead];
		m_array[m_currentHead++] = NaN;
		m_currentHead = mod(m_currentHead);
		--m_size;
		return toRet;
	}

	void CFHRSignal::CyclicArray4Min::push_back(const double value)
	{
		m_array[m_currentTail++] = value;
		++m_size;
		if (m_size > TRACING_DATA_4MIN_MAX_ELEM_SIZE)
		{
			m_array[m_currentHead++] = NaN;
			m_size = TRACING_DATA_4MIN_MAX_ELEM_SIZE;
		}

		m_currentTail = mod(m_currentTail);
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//                                                                                                                         //
	//                                            CFHRSignal implementation                                                    //
	//                                                                                                                         //
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	CFHRSignal::CFHRSignal(void) :
	CDigitalSignal()
	{
		InitializeCriticalSection(&m_csProcess);
		CFHRSignal::ResetSignal();
		SetSmpFreq(m_ConfigFHR.GetDefSmpFreq());
		m_nProcessedCount = 0;
		m_bSetMode = true;
		m_MessageLevel = 0;

		m_FirstRun = true;

		m_bMultihypothesisTopologyLoaded = false;
		m_bAccelDecelTopologyLoaded = false;

		m_bNonDisplayMode = true;
		m_InitDataPos = 0;
		m_InitDataSize = 0;
		m_LastAppend = 0;

		m_pdS2K = NULL;
		m_S2K = 0;

		m_pdC2K = NULL;
		m_C2K = 0;

		m_pucS2A = NULL;
		m_S2A = 0;

		// Oleg - init data structures for BP2
		//m_fhrWindowTracings.initialize(FHR_20MIN_PAST_WINDOW - 1, NaN);
		//m_lastInsertedIndexToTracingsData = FHR_20MIN_PAST_WINDOW - 1;

		//for (int i = 0; i < MEDIAN_HISTOGRAM_SIZE; ++i)
		//{
		//	m_medianHistogram[i] = 0;
		//}
		//
		//m_medianResult.clear();
		//m_medianResidualResult.clear();
		//m_medianBLDeviation.clear();

		//m_medianResidualHalfWavePos.clear();
		//m_medianResidualHalfWaveNeg.clear();

		//m_firFilterCoefs.clear();
		//m_firFilterCoefs = m_ConfigFHR.GetFlt_HP25Coeffs();

		//m_medianResidualHalfPosInterp.initial_insert(FIR_LPF_DELAY, 0); // Fill (first) half window with zeroz
		//m_medianResidualHalfNegInterp.initial_insert(FIR_LPF_DELAY, 0); // Fill (first) half window with zeroz

		//m_upperEnvelope.clear();
		//m_lowerEnvelope.clear();

		//m_upperDeviation.clear();
		//m_lowerDeviation.clear();

		//m_20MinMedianResiduals.clear();
		//m_20MinMedianResidualsSquares.clear();
		//m_20MinMedianResidualsCubes.clear();

		//m_4MinMedianResiduals.clear();
		//m_4MinMedianResidualsSquares.clear();
		//m_4MinMedianResidualsCubes.clear();

		//m_skew20Min.clear();
		//m_skew4Min.clear();

		//m_skew20MinBLDeviation.clear();
		//m_skew4MinBLDeviation.clear();
		//m_BLVariability.clear();
		//
		//m_samplesWindowSize = 0;
		//m_medianSamplesWindowSize = 0;

		//m_lastBaselineMedianDeviation = 0;
		//m_lastBaselineEnvelopeDeviation = 0;
		//m_lastBaselineSkewDeviation = 0;
		//m_lastBaselineVariability = 0;	

		//m_shiftFromAbsoluteStart = 0;

		//m_lastPosNOTNaN = -1;
		//m_lastNegNOTNaN = -1;

		//m_sum20Min = 0;
		//m_sumSquares20Min = 0;
		//m_sumCubes20Min = 0;

		//m_sum4Min = 0;
		//m_sumSquares4Min = 0;
		//m_sumCubes4Min = 0;

		//m_dataSkewness20MinWindowSize = 0;
		//m_actualSkewness20MinWindowSize = 0;
		//m_dataSkewness4MinWindowSize = 0;
		//m_actualSkewness4MinWindowSize = 0;
		// Oleg - END init data structures for BP2

		m_lConsecInvalidAppend = 0;

		m_pdC2A = NULL;
		m_C2A = 0;

		m_pdS4F = NULL;
		m_S4F = 0;
		m_pdUnrepairedSignal = NULL;
		m_pdLowPassSignal = NULL;
		m_nLowPassCount = 0;
		m_pdLowPass25Signal = NULL;
		m_nLowPass25Count = 0;
		m_pdVarPassSignal = NULL;
		m_nVariabilityCount = 0;
		m_pdMinVarSignal = NULL;
		m_nMinVarCount = 0;
		m_pdBufferSignal = NULL;
		m_ContractDetect = NULL;

		m_pucS2Aloc = NULL;
		m_S2Aloc = 0;
		m_pdC2Aloc = NULL;
		m_C2Aloc = 0;

		m_pdMovingAverage = NULL;
		m_lLastValidMovingAvgIndex = 0;
		m_lLastMovingAvgCompStart = 0;

		ThreadHandle = NULL;

		m_ThreadMutex = false;
		m_FlushMutex = false;

		m_bKeepAppend = false;

		m_bCatchException = false;
		m_OutputAbsStart = 0;
		m_NextOutputAbsStart = 0;

		m_lLastCommittedDecelX2 = -1;
		m_lLastCommittedAccelX2 = -1;
		m_lLastCommittedBasX2 = -1;

		m_NextBaselineDetectionStart = 0;	
		m_NextRepairStart = 0;
		m_dLastGoodBasRef = 0.0;
		m_lCurrIteration = 0;

		m_commitIndexDec = 0;
		m_commitIndexAcc = 0;

		m_basCutoff = 0;
		m_accCutoff = 0;
		m_decCutoff = 0;

		m_ProceedBuffer = true;

		m_ConfigFHR.m_AllowConfiguration = true;

		m_pCallBackFunction = NULL;

		m_FlatRepair = false;
		m_LastValidBaseLinePosition = 0;
		m_iOnlyBaseLines = 2;			// default - full (old) mode
		m_iRepStart = 0;
		m_iRepEnd = 0;

		m_bJumpMode = false;
		m_pData = NULL;

		m_bLastAppend = false;

		m_bKeepPending = false;

		m_bHaveNewEvents = false;

		m_rebootMode = false;
		m_synchroneCalculation = false;

		//m_tracingDataIndex = 0;
		//m_numOfErasedTracingDataSamples = 0;
		//m_lastRepairedDataIndex = 0;

		//// PAW
		//hwPosNSamples = 0;
		//hwPosInterpNSamples = 0;
		//hwNegNSamples = 0;
		//hwNegInterpNSamples = 0;
		//svmExampleCount = 0;	
		//m_bumpClassificationHeaderDone = false;
		//m_baselineMedianCount = 0;
	}

	//
	// =======================================================================================================================
	//    ! CFHRSignal::<destructor>
	// =======================================================================================================================
	//
	CFHRSignal::~CFHRSignal(void)
	{
		Flush(true);
		CleanMemory();

		if (m_pucS2Aloc)
		{
			delete[] m_pucS2Aloc;
			m_pucS2Aloc = NULL;
		}

		if (m_pdC2Aloc)
		{
			delete[] m_pdC2Aloc;
			m_pdC2Aloc = NULL;
		}

		if (m_pdS2K)
		{
			delete[] m_pdS2K;
			m_pdS2K = NULL;
		}
		
		if (m_pdC2K)
		{
			delete[] m_pdC2K;
			m_pdC2K = NULL;
		}

		m_pBaseLinesTotal.clear();
		m_pBaseLinesPending.clear();
		m_OutputFhrPartSet.clear();
		m_TotalOutputFhrPartSet.clear();
		m_pAllPreMergeDecels.clear();
		m_pPosCandidates.clear();
		m_pNegCandidates.clear();
		m_pCurrCandBumps.clear();
		m_pCurrCandBumpsExtend.clear();
		
		if (m_pucS2A)
		{
			delete[] m_pucS2A;
			m_pucS2A = NULL;
		}

		if (m_pdC2A)
		{
			delete[] m_pdC2A;
			m_pdC2A = NULL;
		}

		if (m_pdS4F)
		{
			delete[] m_pdS4F;
			m_pdS4F = NULL;
		}

		if (m_pdMovingAverage)
		{
			delete[] m_pdMovingAverage;
			m_pdMovingAverage = NULL;
		}

		m_pRepairIntervals.clear();

		if (m_bNonDisplayMode)
		{
			if (m_pdSignal)
			{
				delete[] m_pdSignal;
				m_pdSignal = NULL;
			}

			if (m_ContractDetect)
			{
				delete[] m_ContractDetect;
				m_ContractDetect = NULL;
			}
		}

		DeleteCriticalSection(&m_csProcess);

		// Oleg - BP2 release
		//for(int i = 0; i < m_svmModels.size(); i++)
		//{
		//	delete m_svmModels[i];
		//}

		//m_svmModels.clear();
	}

	//
	// =======================================================================================================================
	//    ! CFHRSignal::CleanMemory: Properly deletes all dynamically allocated objects.
	// =======================================================================================================================
	//
	void CFHRSignal::CleanMemory(void)
	{
		if (m_pdLowPassSignal)
		{
			delete[] m_pdLowPassSignal;
			m_pdLowPassSignal = NULL;
		}

		if (m_pdLowPass25Signal)
		{
			delete[] m_pdLowPass25Signal;
			m_pdLowPass25Signal = NULL;
		}

		if (m_pdVarPassSignal)
		{
			delete[] m_pdVarPassSignal;
			m_pdVarPassSignal = NULL;
		}

		if (m_pdMinVarSignal)
		{
			delete[] m_pdMinVarSignal;
			m_pdMinVarSignal = NULL;
		}

		m_pBaseLinesFinal.clear();
		m_pBaseLinesNewInWindow.clear();

		if (m_pdSignalToProcess)
		{
			if (m_pdSignalToProcess != m_pdSignal)
			{
				delete[] m_pdSignalToProcess;
			}
			m_pdSignalToProcess = NULL;
		}

		if (m_pdBufferSignal)
		{
			delete[] m_pdBufferSignal;
			m_pdBufferSignal = NULL;
		}

		if (m_ContractDetect)
		{
			delete[] m_ContractDetect;
			m_ContractDetect = NULL;
		}

		m_OutputFhrPartSet.clear();

		if (m_pdUnrepairedSignal)
		{
			delete[] m_pdUnrepairedSignal;
			m_pdUnrepairedSignal = NULL;
		}
		
		m_nLowPassCount = 0;
		m_nLowPass25Count = 0;
		m_nVariabilityCount = 0;
		m_nMinVarCount = 0;
		m_bKeepAppend = false;

		if (m_bLastAppend)
		{
			if (m_pucS2Aloc)
			{
				delete [] m_pucS2Aloc;
				m_pucS2Aloc = NULL;
			}
			m_S2Aloc = 0;

			if (m_pdC2Aloc)
			{
				delete [] m_pdC2Aloc;
				m_pdC2Aloc = NULL;
			}
			m_C2Aloc = 0;
		}
	}

	/*
	=======================================================================================================================
	! CFHRSignal::SetSignal: Set accessor function for Signal. The new signal is pointed to by pdSignal and must have
	nPtsCount sampled points in it. \param nPtsCount Number of points in the signal. \param pdSignal Pointer to an
	array containing the points constituting the signal. \return true iff the new signal could be properly set.
	=======================================================================================================================
	*/
	bool CFHRSignal::SetSignal(long nPtsCount, double *pdSignal)
	{
		m_ConfigFHR.m_AllowConfiguration = false;

		if (m_FlushMutex)
		{
			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (-1000, m_pData);
			}

			if (!m_bSetMode && m_MessageLevel > 0)
			{	// AppendMode!
				printf("Flushing - no more append allowed!\n");
			}

			return false;
		}

		if (!CDigitalSignal::SetSignal(nPtsCount, pdSignal))
		{
			return false;
		}

		m_bSetMode = true;

//		m_bNonDisplayMode = false;

		m_bSignalIsRepaired = false;

		m_ConfigFHR.m_PassedState = CConfigFHR::Raw_Data;

		if (m_pCallBackFunction)
		{
			(*m_pCallBackFunction) (-2, m_pData);
		}

		if (!m_bSetMode && m_MessageLevel > 0)
		{		// AppendMode!
			printf("SetSignal done!\n");
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::SetContraction: Sets the contractions as provided by the parameters. \param nCount Number of
	contractions contained in the following array. \param pContraction Pointer to an array containing, for each
	contraction, its start, its end and its peak. \return Trus iff the contractions were properly set.
	=======================================================================================================================
	*/
	bool CFHRSignal::SetContraction(long nCount, long *pContraction)
	{
		m_ConfigFHR.m_AllowConfiguration = false;

		if (m_FlushMutex)
		{
			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (-1000, m_pData);
			}

			if (!m_bSetMode && m_MessageLevel > 0)
			{	// AppendMode!
				printf("Flushing - no more append allowed!\n");
			}

			return false;
		}

		if (m_ContractDetect)
		{
			delete[] m_ContractDetect;
		}

		m_ContractDetect = new ContractionDetection[nCount];
		if (m_ContractDetect)
		{
			m_lContrDet = nCount;
			for (long i = 0; i < nCount; i++)
			{
				m_ContractDetect[i].lStart = pContraction[3 * i];
				m_ContractDetect[i].lEnd = pContraction[3 * i + 1];
				m_ContractDetect[i].lPeak = pContraction[3 * i + 2];
			}
		}

		if (m_pCallBackFunction)
		{
			(*m_pCallBackFunction) (-1, m_pData);
		}

		if (!m_bSetMode && m_MessageLevel > 0)
		{		// AppendMode!
			printf("SetContraction done!\n");
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::SetCurrContractions : populate m_ContractDetect for current iteration
	// contractions in m_pdC2Aloc are in absolute coordinates 
	// nAppend is the number of samples appended in current processing iteration (will only have multiple iterations if append very large data block > 5 hrs)
	// populate m_lContractDetect with contractions pertinent to current processing window and shift to relative coordinates
	=======================================================================================================================
	*/

	void CFHRSignal::SetCurrContractions()
	{
		if (m_ContractDetect)
		{
			delete[] m_ContractDetect;
			m_ContractDetect = NULL;
		}

		long i, sIndex = 0, eIndex, nCappend;
		while ((sIndex < m_C2Aloc) && (m_pdC2Aloc[sIndex].lEnd < m_OutputAbsStart))
		{
			sIndex++;
		}
		eIndex = sIndex;
		while ((eIndex < m_C2Aloc) && (m_pdC2Aloc[eIndex].lStart <= m_OutputAbsStart + m_nPtsCount))
		{
			eIndex++;
		}

		nCappend = eIndex - sIndex;
		m_lContrDet = nCappend + m_C2K;

		if (m_lContrDet > 0)
		{
			m_ContractDetect = new ContractionDetection[m_lContrDet];
			for (i = 0; i < m_C2K; i++)
			{
				m_ContractDetect[i].lStart = m_pdC2K[i].lStart;
				m_ContractDetect[i].lEnd = m_pdC2K[i].lEnd;
				m_ContractDetect[i].lPeak = m_pdC2K[i].lPeak;
			}

			for (i = 0; i < nCappend; i++)
			{
				m_ContractDetect[i + m_C2K].lStart = m_pdC2Aloc[i + sIndex].lStart;
				m_ContractDetect[i + m_C2K].lEnd = m_pdC2Aloc[i + sIndex].lEnd;
				m_ContractDetect[i + m_C2K].lPeak = m_pdC2Aloc[i + sIndex].lPeak;
			}

			if (m_pdC2K)
			{
				delete[] m_pdC2K;
				m_pdC2K = NULL;
			}
			m_C2K = 0;

		}

		ShiftContractions();  // to relative coordinates

		// purge append buffer of stale contractions if multi-iteration processing
		if ((!m_bLastAppend) && (sIndex > 0))
		{
			long nNew = m_C2Aloc - sIndex;
			if (nNew > 0)
			{
				ContractionDetection* cNew = new ContractionDetection[m_C2Aloc - sIndex];
				for (i = sIndex; i < m_C2Aloc; i++)
				{
					cNew[i - sIndex] = m_pdC2Aloc[i];
				}
				delete [] m_pdC2Aloc;
				m_pdC2Aloc = cNew;
				m_C2Aloc = nNew;
			}
		}
	}

	/*
	=======================================================================================================================
	! CFHRSignal::ShiftContractions: Shift all contractions to 0-base scale.
	=======================================================================================================================
	*/
	void CFHRSignal::ShiftContractions(void)
	{
		if (m_ContractDetect == NULL)
		{
			return;
		}

		// shift data to new 0-base for all calculations!
		for (int i = 0; i < m_lContrDet; i++)
		{
			m_ContractDetect[i].lStart -= m_OutputAbsStart;
			m_ContractDetect[i].lEnd -= m_OutputAbsStart;
			m_ContractDetect[i].lPeak -= m_OutputAbsStart;
		}
	}

	/*
	=======================================================================================================================
	! CFHRSignal::StoreTail: TBD
	=======================================================================================================================
	*/
	void CFHRSignal::StoreTail(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// ce - this done earlier m_OutputAbsStart = m_NextOutputAbsStart;
		// //from previous step ;
		// MAX_DELAY in minute
		int MaxDelay = (int) (m_ConfigFHR.GetMaxDelay() * m_dSmpFreq * 60);
		long lSignalIndex;
		long nPts = GetNumRealSignal(); // only store portion of signal that is not extrapolated
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// all contraction sets - in abs coordinates
		if (m_ContractDetect)
		{
			m_C2K = 0;
			for (int i = 0; i < m_lContrDet; i++)
			{
				if (m_ContractDetect[i].lEnd > 0)	// purge contractions if prior to signal in window
				{
					m_C2K++;
				}

				// assume ordered by end
			}

			if (m_C2K > 0)
			{
				m_pdC2K = new ContractionDetection[m_C2K];
				for (int j = 0; j < m_C2K; j++)
				{	// need to shift back to absolute coords for storage for next window
					m_pdC2K[j].lStart = m_ContractDetect[m_lContrDet - m_C2K + j].lStart + m_OutputAbsStart;
					m_pdC2K[j].lPeak = m_ContractDetect[m_lContrDet - m_C2K + j].lPeak + m_OutputAbsStart;
					m_pdC2K[j].lEnd = m_ContractDetect[m_lContrDet - m_C2K + j].lEnd + m_OutputAbsStart;
				}
			}
		}

		if (m_pdS2K)
		{
			delete[] m_pdS2K;
			m_pdS2K = NULL;
		}
		m_S2K = 0;

		/*
		* m_pdS4F - signal required for filter end effects at beginning of window m_pdS2K
		* - signal required to ensure that valid signal exists for max commit delay
		* m_pucS2A - signal appended in current window (Here shown for 3 successive
		* windows) [< MAX_DELAY >][< MAX_DELAY >][Appended] (time = t) [< MAX_DELAY >][<
		* MAX_DELAY >][Appended] (time = t + BS) [< MAX_DELAY >][< MAX_DELAY >][Appended]
		* (time = t + 2BS) m_pdS4F m_pdS4K m_S2A Save signal for next window - use
		* repaired signal up until m_NextRepairStart and then unrepaired signal for the
		* rest (don't want to save pending repairs that might change)
		*/
		if (m_pdS4F)
		{
			delete[] m_pdS4F;
			m_pdS4F = NULL;
		}
		m_S4F = 0;

		// m_S2K = max(MaxDelay, m_nTotalCount - AbsScaleShift);
		m_S2K = min(nPts, MaxDelay);			// change from Evon
		m_pdS2K = new double[m_S2K];

		for (long i = 0; i < m_S2K; i++)
		{
			lSignalIndex = i + nPts - m_S2K;
			if (lSignalIndex < (m_NextRepairStart - m_OutputAbsStart))
			{
				m_pdS2K[i] = m_pdSignal[lSignalIndex];
			}
			else
			{
				m_pdS2K[i] = m_pdUnrepairedSignal[lSignalIndex];
			}
		}

		if (m_FlatRepair)
		{
			m_FlatRepair = false;
		}

		//  HAVE TO LOOK FOR LONGEST BASELINE THERE SHIT SHIT SHIT LXP LXP LXP
		m_S4F = min(nPts - m_S2K, MaxDelay);	// change from Evon

		// m_S4F = min(m_nPtsCount, 2 * MaxDelay - m_S2K);
		if (m_S4F > 0)
		{
			m_pdS4F = new double[m_S4F];

			// m_pdSignal consist from 3 parts now m_pdS4F + m_pdS2K + extra appended points
			for (long i = 0; i < m_S4F; i++)
			{
				lSignalIndex = i + nPts - m_S4F - m_S2K;

				ASSERT((lSignalIndex >= 0) && (lSignalIndex < m_nPtsCount)); // LXP OUT OF BOUND

				if (lSignalIndex < (m_NextRepairStart - m_OutputAbsStart))
				{
					m_pdS4F[i] = m_pdSignal[lSignalIndex];
				}
				else
				{
					m_pdS4F[i] = m_pdUnrepairedSignal[lSignalIndex];
				}
			}
		}
		else
		{
			if (!m_bSetMode && m_MessageLevel > 0)
			{	// AppendMode!
				printf("Empty m_pdS4F\n");
			}
		}

		if (m_FirstRun)
		{
			m_FirstRun = false;
		}

		m_NextOutputAbsStart = m_nTotalCount - m_S2K - m_S4F;	// change from Evon

		// m_NextOutputAbsStart -= m_S4F;
		// Want to ensure that do not store all 0's during long dropout - hold last value
		// if all stored signal is dropout
		if ((nPts - m_S4F - m_S2K) >= (m_NextRepairStart - m_OutputAbsStart))	// no valid signal stored for next run
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// in this case set the first sample in the stored signal to the last valid FHR sample
			ASSERT((m_NextRepairStart - m_OutputAbsStart + 1 >= 0) && (m_NextRepairStart - m_OutputAbsStart + 1 < m_nPtsCount)); // LXP OUT OF BOUND
			double dLastValidFHR = m_pdSignal[m_NextRepairStart - m_OutputAbsStart + 1];
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (m_S4F > 0)
			{	// at beginning of tracing this may be empty
				m_pdS4F[0] = dLastValidFHR;
			}
			else
			{
				m_pdS2K[0] = dLastValidFHR;
			}
		}
	}

	/*
	=======================================================================================================================
	! CFHRSignal::Preprocessing: TBD ;
	CHE - should be able to remove preprocessing - can handle long repair segments w/ new baseline detection
	=======================================================================================================================
	*/
	bool CFHRSignal::Preprocessing(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// We have now signal chunk m_pucS2A with m_S2A points and contraction chunk
		// m_pdC2A with m_C2A elements ;
		// LASTPOINT CHECK AND LOOKBACK TEST
		long ValidPoints(-1);
		long LastValidPoint(-1);
		int FirstValidPoint(-1);
		bool bLastPointTest(false);
		int MaxDelay = (int) (m_ConfigFHR.GetMaxDelay() * m_dSmpFreq * 60);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (m_pdC2A)
		{
			if (m_pdC2A[m_C2A - 1].lEnd > m_nTotalAppend)
			{
				if (!m_bSetMode && m_MessageLevel > 0)
				{	// AppendMode!
					printf("Last Contractions beyond the signal! - wait for sync!\n");
				}

				return true;
			}
		}

		if (m_ProceedBuffer)
		{			// if this mutex don't set externally to false - apeend will keep going!
			return true;
		}

		if (m_iRepStart > 0)
		{			// m_iRepEnd = FirstValidPoint
			///MaxDelay;
			///
			m_iRepEnd = m_nTotalCount - (m_S2A - FirstValidPoint);	// in abs coordinates
		}

		return false;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::Append: Append extra chunk to the existing signal;
	=======================================================================================================================
	*/
	int CFHRSignal::Append(unsigned char* pSignal, long signal_count, long *pContraction, long contractions_count)
	{
		m_ConfigFHR.m_AllowConfiguration = false;

		if (m_FlushMutex)
		{
			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (-1000, m_pData);
			}

			if (!m_bSetMode && m_MessageLevel > 0)
			{
				// AppendMode!
				printf("Flushing - no more append allowed!\n");
			}

			return -2;
		}

		m_bSetMode = false;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// MAX_DELAY in minutes
		int MaxDelay = (int) (m_ConfigFHR.GetMaxDelay() * m_dSmpFreq * 60);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_nTotalAppend += signal_count;

		// here we adjust total passed timepoints count (the only place in the code for
		// chunk mode!)
		Append_Signal(pSignal, signal_count);
		Append_Contraction(contractions_count, pContraction);
		m_LastAppend += signal_count;

		if (m_FirstRun)
		{
			// if (m_S2A + m_S2K < 3 * MaxDelay) //very small chunk mode
			if (m_S2A < m_ConfigFHR.GetLongestBandPassDelay())
			{	// CHE - only wait min. amount until valid signal across all bands
				// if (m_S2A + m_S2K < MaxDelay) // CHE - no need to wait 3 * MaxDelay
				m_bKeepAppend = TRUE;
			}

			// else if (m_ConfigFHR.m_bNotAllowEmptyContractionAppend && m_C2A == 0) //no
			// contractions - too strong condition? m_bKeepAppend = TRUE;
			// // CHE - do not need contractions to continue
			else
			{
				m_bKeepAppend = Preprocessing();
			}
		}
		else	// we reached enough number of timepoints - current or historical
		{
			if (!m_ConfigFHR.IsShortAppendAllowed())	// Control min samples append before processing
			{
				// if (m_FlatRepair && m_S2A >= 3 * MaxDelay) // CHE - don't consider FlatRepair
				// for now m_bKeepAppend = Preprocessing();
				if (m_S2A >= m_ConfigFHR.GetMinSamplesAppend())
				{									// don't call extra process for small appends
					m_bKeepAppend = Preprocessing();
				}
				else
				{
					m_bKeepAppend = TRUE;
				}
			}
			else									// Allow arbitrarily small appends and process immediately
			{
				/*
				* Should still be able to do small appends even if in dropout - ce if
				* (m_FlatRepair) { if (m_S2A >= 3 * MaxDelay) m_bKeepAppend = Preprocessing();
				* else m_bKeepAppend = TRUE;
				* } else {
			 */
				m_bKeepAppend = Preprocessing();

				// }
			}
		}

		if (m_ThreadMutex)							// busy
		{
			// CHE - here want to check if thread is really running
			m_bKeepAppend = true;
			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (10, m_pData);
			}
		}

		if (!m_bKeepAppend)
		{
			m_TransferMutex.acquire();

			m_bSignalIsRepaired = false;

			bool ret = PrepareProceed();
			
			if (ret)
			{
				// LXP Temporary fix for the initial restart that could consume so much memory that the server would crash
				if (m_rebootMode || m_synchroneCalculation)
				{
					Proceed(); // NOT threaded
				}
				else
				{
					Process(); // Threaded
				}
			}

			m_ProceedBuffer = true;

			m_TransferMutex.release();

			return 2;								// Appended and Proceed
		}
		else
		{
			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (1, m_pData);
			}

			if (!m_bSetMode && m_MessageLevel > 2)	// AppendMode!
			{
				// if (m_FlatRepair) printf("%d", m_iRepStart);
				printf(".");
			}
		}

		return 1;	// Appended and Buffered
	}

	/*
	=======================================================================================================================
	! CFHRSignal::Append_Signal: Append extra chunk to the existing signal;
	update current Point Counter. \param nCount Number of points in the signal. \param pSignal Pointer to an
	array containing the points constituting the signal. \return TBD
	=======================================================================================================================
	*/
	int CFHRSignal::Append_Signal(unsigned char* signal, long nCount)
	{
		m_TransferMutex.acquire();

		if (nCount < 1)
		{
			return 0;
		}

		// Oleg - BP2 part
		//for (long i = 0; i < nCount; ++i)
		//{
		//	m_tracingsData.push_back(signal[i] != 0 ? (double)signal[i] : NaN);
		//}
		// Oleg - END BP2 part

		unsigned char *cNew = new unsigned char[m_S2A + nCount];

		if (m_pucS2A && m_S2A > 0)
		{
			memcpy(cNew, m_pucS2A, m_S2A * sizeof(unsigned char));
		}

		memcpy(&cNew[m_S2A], signal, nCount * sizeof(unsigned char));

		if (m_pucS2A)
		{
			delete[] m_pucS2A;
		}

		m_pucS2A = cNew;
		m_S2A += nCount;

		m_TransferMutex.release();

		return 1;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::Append_Contraction: Append extra contraction information. \param nCount Number of contractions
	contained in the following array. \param pContraction Pointer to an array containing, for each contraction, its
	start, its end and its peak. \return TBD
	=======================================================================================================================
	*/
	int CFHRSignal::Append_Contraction(long nCount, long *pContraction)
	{
		if (nCount <= 0)
		{
			return 0;
		}

		ContractionDetection *pdC2A = new ContractionDetection[m_C2A + nCount];

		if (m_pdC2A && m_C2A > 0)
		{
			memcpy(pdC2A, m_pdC2A, m_C2A * sizeof(ContractionDetection));
		}

		memcpy(&pdC2A[m_C2A], pContraction, nCount * sizeof(ContractionDetection));

		if (m_pdC2A)
		{
			delete m_pdC2A;
		}

		m_pdC2A = pdC2A;
		m_C2A += nCount;

		return 1;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::SetCallBackFunction: Set the callback function used to TBD
	=======================================================================================================================
	*/
	void CFHRSignal::SetCallBackFunction(void (*pCallBack) (int, void *), void *data)
	{
		m_ConfigFHR.m_AllowConfiguration = false;
		m_pData = data;
		m_pCallBackFunction = pCallBack;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::Flush: Instructs the worker thread to finish its current calculations, then to terminate while
	returning the last results.
	=======================================================================================================================
	*/
	int CFHRSignal::Flush(bool bStopNow)
	{
		m_FlushMutex = true;

		while (m_ThreadMutex)
		{
			Sleep(10);
		}

		if (!bStopNow)
		{
			// last chunk >>
			m_bSignalIsRepaired = false;

			Proceed();
		}

		if (m_pCallBackFunction)
		{
			(*m_pCallBackFunction) (-200, m_pData);
		}

		return (m_TotalOutputFhrPartSet.size());
	}

	/*
	=======================================================================================================================
	! CFHRSignal::ResetSignal: Resets the Signal property to NULL. All properties related to the signal are also reset
	appropriately.
	=======================================================================================================================
	*/
	void CFHRSignal::ResetSignal(void)
	{
		CDigitalSignal::ResetSignal();

		m_bSignalIsRepaired = false;
		m_bRepairInPlace = true;
		m_pdSignalToProcess = NULL;

		m_nTotalCount = 0;
		m_nTotalAppend = 0;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::RepairSignal: Repairs the FHR signal. For details about the repair process, refer to the relevant
	SRS. \return True iff the FHR signal could be repaired.
	=======================================================================================================================
	*/
	bool CFHRSignal::RepairSignal(void)
	{
		if (m_bSignalIsRepaired)
		{
			return true;
		}

		if (m_pdUnrepairedSignal != 0)
		{
			delete[] m_pdUnrepairedSignal;
		}

		m_pdUnrepairedSignal = new double[m_nPtsCount]; // extra space for filtering
		if (m_pdUnrepairedSignal == NULL)
		{
			return false;
		}

		memcpy(m_pdUnrepairedSignal, m_pdSignal, sizeof(double) * m_nPtsCount);

		// Above memcpy to m_pdUnrepaired signal is not strictly 'unrepaired' but any
		// pending repairs (at end of available signal in window) will not be committed
		// for next iteration See StoreTail()
		if (m_ConfigFHR.m_bDisableRepair)
		{	// in some cases want to disable repair for NN training
			m_NextRepairStart = m_nPtsCount - GetNumExtrapolatedSignal() - 2 + m_OutputAbsStart;
		}
		else
		{
			m_pRepairIntervals.filterStartingAfter(m_NextRepairStart);
			m_pRepairIntervals.filterEndingBefore(m_OutputAbsStart);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// remove intervals that occur before start of current window
			CRepairSignal repair;
			fhrPartSet *pNewRepInts;
			long lNumRep = 0;
			long lNextRepairRel = max(0, m_NextRepairStart - m_OutputAbsStart);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			repair.SetSignalToRepair(m_pdSignal, m_nPtsCount);
			repair.SetRepairBuffer(m_ConfigFHR.GetRepairBuffer()); 
			repair.SetRealSignalLength(m_nPtsCount - GetNumExtrapolatedSignal()); // any repair into extrapolated signal is pending
			repair.RepairSignal(lNextRepairRel);

			// record where need to start on next pass in absolute coords
			m_NextRepairStart = repair.GetLastGoodIndex() + m_OutputAbsStart;
			pNewRepInts = repair.GetRepairIntervals();

			pNewRepInts->filterByLength(m_ConfigFHR.m_lMaxRepairIgnore + 1);

			pNewRepInts->toAbsTime(m_OutputAbsStart);
			pNewRepInts->setClearMemory(false);
			m_pRepairIntervals.add(pNewRepInts);
			m_pRepairIntervals.mergeOverlapping();
		}

		m_bSignalIsRepaired = true;

		// Stay consistent with original way - copy repaired signal to
		// m_pdSignalToProcess This will be used by filtering algorithms
		if (m_pdSignalToProcess != NULL && m_pdSignalToProcess != m_pdSignal)
		{
			delete[] m_pdSignalToProcess;
		}

		m_pdSignalToProcess = new double[m_nPtsCount];	// extra space for fitering

		memcpy(m_pdSignalToProcess, m_pdSignal, sizeof(double) * m_nPtsCount);

		return true;	// might want return code from CRepairSignal::RepairSignal()
	}

	/*
	=======================================================================================================================
	! CFHRSignal::GetRepairEndBuffer: get number of samples at end of window that do not have stable repair
	=======================================================================================================================
	*/
	long CFHRSignal::GetRepairEndBuffer(void)
	{
		return m_nTotalCount - m_NextRepairStart;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::SetRepairIntervals: can set repair intervals from an external file. This is used when training NN
	based on old repair algorithm (on which expert markings are currently based). Used in combination w/
	m_bDisableRepair, we can use already repaired FHR (from the old repair algorithm) as input, as well as repair
	intervals which tell us where the repairs were made.
	=======================================================================================================================
	*/
	void CFHRSignal::SetRepairIntervals(fhrPartSet *pRepairIntervals)
	{
		m_pRepairIntervals.clear();
		m_pRepairIntervals.addcopy(pRepairIntervals);
		m_pRepairIntervals.setAsNonPending();
	}

	/*
	=======================================================================================================================
	! CFHRSignal::BandPass: Combination of High Pass + Rectification + Low Pass. \return True iff the filter passed. //
	NOTE - CHE: This is the variability (filterVar) filter
	=======================================================================================================================
	*/
	bool CFHRSignal::BandPass(void) // High Pass + Rectification + Low Pass
	{
		/*~~~~~~~~~~~~~~~~*/
		bool bReturn(false);
		/*~~~~~~~~~~~~~~~~*/

		bReturn = HighPass();		// High Pass filter (acutally band pass)
		if (bReturn)
		{
			Rectify();				// Rectification
			bReturn = VarPass();	// Low Pass
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// delay = 226 + 101 = 327
		long iDelay = (m_ConfigFHR.m_iFir_Flt8_Size / 2) + (m_ConfigFHR.m_iFir_Flt4_Size / 2);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (m_pdVarPassSignal)
		{
			delete[] m_pdVarPassSignal;
		}

		m_pdVarPassSignal = new double[m_nPtsCount];
		if (m_pdVarPassSignal == NULL)
		{
			return false;
		}

		memcpy(m_pdVarPassSignal, m_pdSignalToProcess, sizeof(double) * m_nPtsCount);

		m_nVariabilityCount = m_nPtsCount - iDelay;

		FixFilterVarRepair();		// interpolate repaired segments so that filterVar is not skewed by repair

		if (bReturn)
		{
			m_ConfigFHR.m_PassedState = CConfigFHR::VarPass;
		}

		return bReturn;
	}

	/*
	=======================================================================================================================
	FixFilterVarRepair: Interpolate filterVar signal in areas of repair so do not have artificially low values. To get
	Variability signal based solely on 'real' signal, would need to take 327 samples on each side of repaired segment.
	This was deemed excessive so take 327 / 2 samples on each side, unless the rep seg is 10sec or shorter, in which
	case do not take any buffer (but interpolate filterVar in actual repair interval still)
	=======================================================================================================================
	*/
	bool CFHRSignal::FixFilterVarRepair(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lX1 = 0;
		long lX2 = 0;
		long lFiltDel = (long) round(double(0.5 * (m_ConfigFHR.m_iFir_Flt4_Size + m_ConfigFHR.m_iFir_Flt8_Size) / 2.0));
		long lMinRepLength = (long) (m_ConfigFHR.MinRepLengthFilterVar() * GetSmpFreq());
		long k = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = 0; i < m_pRepairIntervals.size(); i++)
		{
			fhrPart *p = m_pRepairIntervals.getAt(i);
			if (p->length() > lMinRepLength)
			{
				lX1 = max(0, p->getX1() - m_OutputAbsStart - lFiltDel);
				lX2 = min(m_nVariabilityCount - 1, p->getX2() - m_OutputAbsStart + lFiltDel);
			}
			else
			{
				lX1 = max(0, p->getX1() - m_OutputAbsStart - 1);
				lX2 = min(m_nVariabilityCount - 1, p->getX2() - m_OutputAbsStart + 1);
			}

			// do linear interpolation between lX1 and lX2
			if (lX1 == 0)	// fill in beginning
			{
				for (k = 0; k < lX2; k++)
				{
					m_pdVarPassSignal[k] = m_pdVarPassSignal[lX2];
				}
			}
			else if (lX2 == m_nVariabilityCount - 1)	// at end of signal
			{
				for (k = lX1; k <= lX2; k++)
				{
					m_pdVarPassSignal[k] = m_pdVarPassSignal[lX1];
				}
			}
			else	// find linear interp
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				double dSlope = (m_pdVarPassSignal[lX2] - m_pdVarPassSignal[lX1]) / (lX2 - lX1);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				for (k = lX1 + 1; k < lX2; k++)
				{
					m_pdVarPassSignal[k] = m_pdVarPassSignal[k - 1] + dSlope;
				}
			}
		}

		return true;
	}

	/*
	=======================================================================================================================
	! CFHRSignal::MinVarPass: TBD \return True iff the filter passed.
	=======================================================================================================================
	*/
	bool CFHRSignal::MinVarPass(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// Let's keep local vars the same as MATLAB code
		bool min_found = false;
		long max_idx = 0;				// Begin of last max. var. index
		long last_known_var_idx = 0;	// Last known variability index
		double min_var = 0;				// last computed minimum variability
		double actual_var;
		double x_1 = 0.0;
		long i_1_beg = 0;
		long i_1_end = 0;				// First point
		double x_2 = 0.0;
		long i_2_beg = 0;
		long i_2_end = 0;				// Second point
		double x_3 = 0.0;
		long i_3_beg = 0;
		long i_3_end = 0;				// Third point
		long minVarIndex = 0;			// Index of the last minima
		long maxDelay = 0;
		long cmpr_num;
		long last_f_index;
		long current_middle_index;
		long iCurrent;
		// int MAX_ACC_DELAY = 120;
		// //critical MATLAB step constant - don't chang
		int MAX_ACC_DELAY = (int) (30.0 * GetSmpFreq());
		bool bCont = true;
		CLinePointArray m_MinVarArray;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (m_pdMinVarSignal)
		{
			delete[] m_pdMinVarSignal;
		}

		m_pdMinVarSignal = new double[m_nPtsCount];
		if (m_pdMinVarSignal == NULL)
		{
			return false;
		}

		for (iCurrent = 0; iCurrent < m_nPtsCount; iCurrent++)
		{
			m_pdMinVarSignal[iCurrent] = 0;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lStartIndex = m_ConfigFHR.m_iFir_Flt4_Size / 2;
		long lEndIndex = GetVariabilityCount();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (iCurrent = 0; iCurrent <= lStartIndex; iCurrent++)
		{
			m_pdMinVarSignal[iCurrent] = lStartIndex;						// first valid filterVar sample is at lStartIndex
		}

		minVarIndex = lStartIndex;
		last_known_var_idx = lStartIndex;
		min_var = m_pdMinVarSignal[lStartIndex];

		for (iCurrent = lStartIndex; iCurrent < m_nPtsCount; iCurrent++)
		{
			actual_var = m_pdVarPassSignal[iCurrent];
			if (fabs(x_3 - actual_var) <= .01)
			{
				bCont = true;
				if (i_3_end - i_3_beg > MAX_ACC_DELAY)
				{
					if (Approx_Equal(x_1, x_2))
					{
						minVarIndex = iCurrent + 1;
						min_var = actual_var;
					}

					if (last_known_var_idx < iCurrent)
					{
						for (long jCrt = last_known_var_idx + 1; jCrt <= iCurrent; jCrt++)
						{
							m_pdMinVarSignal[jCrt] = minVarIndex;
						}

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						CLinePoint *pLP = new CLinePoint(last_known_var_idx + 1, min_var);
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						m_MinVarArray.Add(pLP);

						last_known_var_idx = iCurrent;
					}

					bCont = false;
				}

				cmpr_num = i_3_end - i_3_beg + 1;
				x_3 = ((x_3 * cmpr_num) + actual_var) / (cmpr_num + 1);
				i_3_end = iCurrent;
				if (bCont)
				{
					continue;
				}
			}
			else
			{
				current_middle_index = (i_2_end + i_2_beg) / 2;

				if (Approx_Equal(x_1, x_2) && Approx_Equal(x_3, x_2))
				{
					min_found = true;										// We have found min
					min_var = x_2;											// Set new min variability
					minVarIndex = max(minVarIndex, current_middle_index + 1);

					if (last_known_var_idx < i_2_end)
					{
						for (long jCrt = last_known_var_idx + 1; jCrt <= i_2_end; jCrt++)
						{
							m_pdMinVarSignal[jCrt] = minVarIndex;
						}

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						CLinePoint *pLP = new CLinePoint(last_known_var_idx + 1, min_var);
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						m_MinVarArray.Add(pLP);

						last_known_var_idx = i_2_end;
					}
				}
				else if (Approx_Equal(x_2, x_1) && Approx_Equal(x_2, x_3))
				{
					if (min_found)
					{
						min_found = false;									// found max here
						max_idx = current_middle_index;

						if (last_known_var_idx < max_idx)
						{
							/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
							CLinePoint *pLP = new CLinePoint(i_2_beg, min_var);
							/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

							if (m_MinVarArray.GetCount() == 0)
							{
								for (long jCrt = i_2_beg; jCrt <= max_idx; jCrt++)
								{
									m_pdMinVarSignal[jCrt] = minVarIndex;
								}
							}
							else
							{
								/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
								CLinePoint *pLp = m_MinVarArray.GetAt(m_MinVarArray.GetCount() - 1);
								/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

								if (i_2_beg >= pLp->x)
								{
									for (long jCrt = i_2_beg; jCrt <= max_idx; jCrt++)
									{
										m_pdMinVarSignal[jCrt] = minVarIndex;
									}
								}
								else
								{											// this was a bug in matlab code!
									for (long jCrt = pLp->x; jCrt <= max_idx; jCrt++)
									{
										m_pdMinVarSignal[jCrt] = minVarIndex;
									}

									delete pLP;
									pLP = new CLinePoint(pLp->x, min_var);
								}
							}

							m_MinVarArray.Add(pLP);

							last_known_var_idx = max_idx;
						}
					}
				}
				else if (Approx_Equal(x_2, x_1) && Approx_Equal(x_3, x_2))
				{
					if (last_known_var_idx < i_2_end)
					{
						for (long jCrt = last_known_var_idx + 1; jCrt <= i_2_end; jCrt++)
						{
							m_pdMinVarSignal[jCrt] = minVarIndex;
						}

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						CLinePoint *pLP = new CLinePoint(last_known_var_idx + 1, min_var);
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						m_MinVarArray.Add(pLP);

						last_known_var_idx = i_2_end;
					}
				}
				else if (Approx_Equal(x_1, x_2) && Approx_Equal(x_2, x_3))
				{
					last_f_index = i_3_end - MAX_ACC_DELAY;

					if (Approx_Equal(min_var, actual_var))
					{
						if (actual_var < m_pdVarPassSignal[last_known_var_idx])
						{
							min_var = actual_var;
							minVarIndex = iCurrent + 1;
						}
					}

					if (last_known_var_idx < last_f_index)
					{
						for (long jCrt = last_known_var_idx + 1; jCrt <= last_f_index; jCrt++)
						{
							m_pdMinVarSignal[jCrt] = minVarIndex;
						}

						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
						CLinePoint *pLP = new CLinePoint(last_known_var_idx + 1, min_var);
						/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

						m_MinVarArray.Add(pLP);

						last_known_var_idx = last_f_index;
						max_idx = last_known_var_idx;
					}
				}
			}

			x_1 = x_2;
			i_1_beg = i_2_beg;
			i_1_end = i_2_end;

			x_2 = x_3;
			i_2_beg = i_3_beg;
			i_2_end = i_3_end;

			x_3 = actual_var;
			i_3_beg = iCurrent;
			i_3_end = iCurrent;
		}

		// hold last index until end of window
		for (iCurrent = last_known_var_idx; iCurrent < m_nPtsCount; iCurrent++)
		{
			m_pdMinVarSignal[iCurrent] = m_pdMinVarSignal[last_known_var_idx];
		}

		m_ConfigFHR.m_PassedState = CConfigFHR::MinVar;
		m_nMinVarCount = m_nPtsCount;

		if (m_ConfigFHR.m_iStoreXML > 2)
		{
			memcpy(m_pdSignalToProcess, m_pdMinVarSignal, sizeof(double) * m_nPtsCount);
		}

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::VarPass RETURNS: true iff the filter passed.
	// =======================================================================================================================
	//
	bool CFHRSignal::VarPass(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bReturn(false);
		double *coeff = m_ConfigFHR.m_dFir_Flt4;	// Low Pass Filter
		long M = m_ConfigFHR.m_iFir_Flt4_Size;		// length of Flt4 filter
		// 452 points array - delay = 226
		long iDelay = M / 2;
		long iBlocksize = OptimalBuffer(M);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while (m_nPtsCount < iBlocksize)
		{
			iBlocksize /= 2;
		}

		/*~~~~~~~~~~~~~~~~~~~~*/
		long N = iBlocksize - M;
		/*~~~~~~~~~~~~~~~~~~~~*/

		bReturn = ApplyFilter(M, N, coeff, iDelay);

		return bReturn;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::HighPass RETURNS: true iff the filter passed. NOTE - CHE: this is actually the band pass filter used in
	//    the variability (filterVar) filter
	// =======================================================================================================================
	//
	bool CFHRSignal::HighPass(void)
	{
		/*~~~~~~~~~~~~~~~~*/
		bool bReturn(false);
		/*~~~~~~~~~~~~~~~~*/

		// copy repaired signal m_pdSignal for high pass step
		memcpy(m_pdSignalToProcess, m_pdSignal, sizeof(double) * m_nPtsCount);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double *coeff = m_ConfigFHR.m_dFir_Flt8;	// High Pass Filter
		long M = m_ConfigFHR.m_iFir_Flt8_Size;		// length of filter
		// 203 points array - delay = 101
		long iDelay = M / 2;
		long iBlocksize = OptimalBuffer(M);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while (m_nPtsCount < iBlocksize)
		{
			iBlocksize /= 2;
		}

		/*~~~~~~~~~~~~~~~~~~~~*/
		long N = iBlocksize - M;
		/*~~~~~~~~~~~~~~~~~~~~*/

		bReturn = ApplyFilter(M, N, coeff, iDelay);

		if (bReturn)
		{
			m_ConfigFHR.m_PassedState = CConfigFHR::HighPass;
		}

		return bReturn;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::Rectify
	// =======================================================================================================================
	//
	void CFHRSignal::Rectify(void)
	{
		for (long i = 0; i < m_nPtsCount; i++)
		{
			m_pdSignalToProcess[i] = fabs(m_pdSignalToProcess[i]);
		}
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::LowPass RETURNS: true iff the filter passed.
	// =======================================================================================================================
	//
	bool CFHRSignal::LowPass(void)
	{
		/*~~~~~~~~~~~~~~~~*/
		bool bReturn(false);
		/*~~~~~~~~~~~~~~~~*/

		// copy repaired signal m_pdSignal for low pass step
		memcpy(m_pdSignalToProcess, m_pdSignal, sizeof(double) * m_nPtsCount);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// default filter
		double *coeff = m_ConfigFHR.m_dFir_Flt2;
		long M = m_ConfigFHR.m_iFir_Flt2_Size;	// length of filter
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		if (m_ConfigFHR.m_dFirFlt)				// for external Filter File
		{
			coeff = m_ConfigFHR.m_dFirFlt;
			M = m_ConfigFHR.m_iFirFlt_Size;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// 162 points array - delay = 81
		long iDelay = M / 2;
		long iBlocksize = OptimalBuffer(M);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while (m_nPtsCount < iBlocksize)
		{
			iBlocksize /= 2;
		}

		/*~~~~~~~~~~~~~~~~~~~~*/
		/*
		* if (m_ConfigFHR.m_iExtBlockSize > 1) { for (long i = 0;
		* i < m_ConfigFHR.m_iExtBlockSize;
		* i++) iBlocksize *= 2;
		* } else if (m_ConfigFHR.m_iExtBlockSize < 1) { for (long i = 0;
		* i > m_ConfigFHR.m_iExtBlockSize;
		* i--) iBlocksize /= 2;
		*
		*/
		long N = iBlocksize - M;
		/*~~~~~~~~~~~~~~~~~~~~*/

		bReturn = ApplyFilter(M, N, coeff, iDelay);

		if (bReturn)
		{
			m_ConfigFHR.m_PassedState = CConfigFHR::LowPass;
		}

		if (m_pdLowPassSignal)
		{
			delete[] m_pdLowPassSignal;
		}

		m_pdLowPassSignal = new double[m_nPtsCount];
		if (m_pdLowPassSignal == NULL)
		{
			return false;
		}

		memcpy(m_pdLowPassSignal, m_pdSignalToProcess, sizeof(double) * m_nPtsCount);

		// m_nLowPassCount = m_nPtsCount;
		m_nLowPassCount = m_nPtsCount - iDelay;


		return bReturn;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::LowPass25 - this uses low pass filter of band one (for accel slope features) RETURNS: true iff the
	//    filter passed.
	// =======================================================================================================================
	//
	bool CFHRSignal::LowPass25(void)
	{
		/*~~~~~~~~~~~~~~~~*/
		bool bReturn(false);
		/*~~~~~~~~~~~~~~~~*/

		// copy repaired signal m_pdSignal for low pass step
		memcpy(m_pdSignalToProcess, m_pdSignal, sizeof(double) * m_nPtsCount);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// default filter
		double *coeff = m_ConfigFHR.m_dFir_Flt_LP25;
		long M = m_ConfigFHR.m_iFir_Flt_LP25_Size;	// length of filter
		// CHE - no allowance for loading from external file
		long iDelay = M / 2;
		long iBlocksize = OptimalBuffer(M);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while (m_nPtsCount < iBlocksize)
		{
			iBlocksize /= 2;
		}

		/*~~~~~~~~~~~~~~~~~~~~*/
		long N = iBlocksize - M;
		/*~~~~~~~~~~~~~~~~~~~~*/

		bReturn = ApplyFilter(M, N, coeff, iDelay);

		if (m_pdLowPass25Signal)
		{
			delete[] m_pdLowPass25Signal;
		}

		m_pdLowPass25Signal = new double[m_nPtsCount];
		if (m_pdLowPass25Signal == NULL)
		{
			return false;
		}

		memcpy(m_pdLowPass25Signal, m_pdSignalToProcess, sizeof(double) * m_nPtsCount);

		// m_nLowPassCount = m_nPtsCount;
		m_nLowPass25Count = m_nPtsCount - iDelay;

		return bReturn;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::OptimalBuffer RETURNS: Optimal buffer size as long (power of 2)
	// =======================================================================================================================
	//
	long CFHRSignal::OptimalBuffer(long BufferSize)
	{
		/*~~~~~~~~~~~~~~~~~~*/
		long iBlocksize(4096);
		/*~~~~~~~~~~~~~~~~~~*/

		// optimal blocksize - see ref in documentation
		if (BufferSize < 10)
		{
			iBlocksize = 32;
		}
		else if (BufferSize < 20)
		{
			iBlocksize = 64;
		}
		else if (BufferSize < 30)
		{
			iBlocksize = 128;
		}
		else if (BufferSize < 50)
		{
			iBlocksize = 256;
		}
		else if (BufferSize < 100)
		{
			iBlocksize = 512;
		}
		else if (BufferSize < 200)
		{
			iBlocksize = 1024;
		}
		else if (BufferSize < 300)
		{
			iBlocksize = 2048;
		}
		else if (BufferSize < 600)
		{
			iBlocksize = 4096;
		}
		else
		{
			iBlocksize = 8192;
		}

		// else if (BufferSize < 1000) iBlocksize = 8192;
		// else if (BufferSize < 2000) iBlocksize = 16384;
		// else iBlocksize = 32768;
		// //let's make it as max case else if (BufferSize < 4000) iBlocksize = 32768;
		// else iBlocksize = 65536;
		return iBlocksize;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::ApplyFilterNoOverlapAdd ;
	//    This was given as a fix in 09/2006 to a bug found in ApplyFilter. The bug is in the overlap add portion, and occurs
	//    when processing smaller blocks (~ < 10000 samples) of signal and is more pronounced for longer filters (e.g. band
	//    pass 6). This function was given by Evonium as a fix, and essentially it does not use overlap-add, but instead
	//    computes an N-pt FFT, where N is the number of samples in the signal to be processed (m_nPtsCount). Because there
	//    was no window-to-window communication, there was no point in doing overlap-add anyway - it is only useful in a true
	//    realtime implementation. So until the time where we implement true realtime filtering, there is no point in using
	//    overlap-add, and this fix is satisfactory. But it should be noted that ApplyFilter can give erroneous output under
	//    some circumstances. ADDENDUM - N-pt FFT can only be used up until N = 32768 due to library (OOURA) being used. So
	//    use this when N < 32768, otherwise (in Batch mode) use ApplyFilter
	// =======================================================================================================================
	//
	bool CFHRSignal::ApplyFilterNoOverlapAdd(long M, double *coeff)
	{
		/*~~~~~~~~~~~~~~~~~*/
		long N = m_nPtsCount;
		long lDelay = M / 2;
		PatternsVector b;
		/*~~~~~~~~~~~~~~~~~*/

		b.setBounds(0, M - 1);
		b.setContent(0, M - 1, coeff);

		/*~~~~~~*/
		// filter coefficients
		PatternsVector dd;
		/*~~~~~~*/

		dd.setBounds(0, M - 1);

		/*~~~~~~~~~~~~~~~~~*/
		// temporary array to store between iterations
		PatternsVector dOverlapArray;
		/*~~~~~~~~~~~~~~~~~*/

		dOverlapArray.setBounds(0, M - 1);

		// temporary array to pass between iterations
		for (long i = 0; i < M; i++)
		{
			dOverlapArray(i) = 0;
		}

		for (long i = 0; i < M; i++)
		{
			dd(i) = 0;	// initial
		}

		//~~~~~
		PatternsVector a;
		/*~~~~~*/

		a.setBounds(0, N - 1);

		// signal's segment to pass
		for (long i = 0; i < N; i++)
		{
			a(i) = m_pdSignalToProcess[i];
		}

		FftConvolution(a, N, b, M, dOverlapArray, true);

		for (long i = lDelay; i < N; i++)
		{
			m_pdSignalToProcess[i - lDelay] = a(i);
		}

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::ApplyFilter Main function to process filter using FFT, FFT Convolution and inverse FFT using Overlap
	//    and save method RETURNS: true iff the filter passed. NOTE::Problems using this when M is close to m_nPtsCount -
	//    use ApplyFilterNoOverlapAdd
	// =======================================================================================================================
	//
	bool CFHRSignal::ApplyFilter(long M, long N, double *coeff, long iDelay)
	{
		if (!(USE_OVERLAP_ADD))
		{
			if (m_nPtsCount + M <= OOURA_NMAX)
			{	// can do at most 32768 pt FFT with OOURA lib
				return ApplyFilterNoOverlapAdd(M, coeff);
			}
		}

		/*~~~~~*/
		PatternsVector b;
		/*~~~~~*/

		b.setBounds(0, M - 1);
		b.setContent(0, M - 1, coeff);

		/*~~~~~~*/
		// filter coefficients

		PatternsVector dd;
		/*~~~~~~*/

		dd.setBounds(0, M - 1);

		/*~~~~~~~~~~~~~~~~~*/
		// temporary array to store between iterations

		PatternsVector dOverlapArray;
		/*~~~~~~~~~~~~~~~~~*/

		dOverlapArray.setBounds(0, M - 1);	// temporary array to pass between iterations

		for (long i = 0; i < M; i++)
		{
			dOverlapArray(i) = 0;
		}

		for (long i = 0; i < M; i++)
		{
			dd(i) = 0;						// initial
		}

		//~~~~~
		PatternsVector a;
		/*~~~~~*/

		a.setBounds(0, N - 1);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// signal's segment to pass
		// real N should be 2^p (other case MATLAB expand it inside!)
		long Range = m_nPtsCount / N;
		long RestPart = m_nPtsCount % N;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// RestPart + Range * N = m_nPtsCount
		for (long j = 0; j < Range; j++)
		{
			// fill current signal segment to pass into the filter
			for (long i = 0; i < N; i++)
			{		// read next segment
				a(i) = m_pdSignalToProcess[j * N + i];
			}

			FftConvolution(a, N, b, M, dOverlapArray, true);

			// we can optimize asking to make or not fft(coef)!
			for (long i = 0; i < iDelay; i++)
			{		// segment begin part
				m_pdSignalToProcess[j * N + i] = a(i + iDelay) + dd(i + iDelay);
			}

			for (long i = iDelay; i < N - iDelay; i++)
			{		// segment middle part
				m_pdSignalToProcess[j * N + i] = a(i + iDelay);
			}

			if (j > 0)
			{		// Initial segment
				for (long i = N - iDelay; i < N; i++)
				{	// prev segment end part
					m_pdSignalToProcess[(j - 1) * N + i] = a(i + iDelay - N) + dd(i + iDelay - N);
				}
			}

			for (long i = 0; i < M; i++)
			{
				dd(i) = dOverlapArray(i);
			}
		}

		// extra step for last signal segment (non-standard length - shorter)
		for (long i = 0; i < RestPart; i++)
		{			// read next segment
			if (Range * N + i < m_nPtsCount)
			{
				a(i) = m_pdSignalToProcess[Range * N + i];
			}
		}

		for (long i = RestPart; i < N; i++)
		{			// read next segment
			if (Range * N + i < m_nPtsCount)
			{
				a(i) = a(RestPart - 1);
			}
		}

		FftConvolution(a, N, b, M, dOverlapArray, true);

		for (long i = 0; i < iDelay; i++)
		{			// segment begin part
			if (Range * N + i < m_nPtsCount)
			{
				m_pdSignalToProcess[Range * N + i] = a(i + iDelay) + dd(i + iDelay);
			}
		}

		for (long i = iDelay; i < N - iDelay; i++)
		{			// segment middle part
			if (Range * N + i < m_nPtsCount)
			{
				m_pdSignalToProcess[Range * N + i] = a(i + iDelay);
			}
		}

		for (long i = N - iDelay; i < N; i++)	// prev segment end part
		{
			if ((Range - 1) * N + i >= 0)
			{
				m_pdSignalToProcess[(Range - 1) * N + i] = a(i + iDelay - N) + dd(i + iDelay - N);
			}
		}

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ApplyFilterDirect(double *coeff, long lNumTaps)
	{
		for (long i = 0; i < m_nPtsCount; i++)
		{
			double value = 0;
			for (long k = i; k > max(0, i - lNumTaps + 1); k--)
			{
				value += m_pdSignal[k] * coeff[i - k];
			}
			m_pdSignalToProcess[i] = value;
		}

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::ApplyBandFilter Main function to process band filter using FFT, FFT Convolution and inverse FFT using
	//    Overlap and save method RETURNS: true iff the filter passed.
	// =======================================================================================================================
	//
	bool CFHRSignal::ApplyBandFilter(long N, long M1, double *coeff1, long M2, double *coeff2)
	{
		/*~~~~~~~~~~~~~~~~~~*/
		long iDelay1 = M1 / 2;
		long iDelay2 = M2 / 2;
		PatternsVector b1;
		/*~~~~~~~~~~~~~~~~~~*/

		b1.setBounds(0, M1 - 1);
		b1.setContent(0, M1 - 1, coeff1);

		/*~~~~~~*/
		// filter1 coefficients
		PatternsVector b2;
		/*~~~~~~*/

		b2.setBounds(0, M2 - 1);
		b2.setContent(0, M2 - 1, coeff2);

		/*~~~~~~~*/
		// filter2 coefficients
		PatternsVector dd1;
		/*~~~~~~~*/

		dd1.setBounds(0, M1 - 1);

		/*~~~~~~~*/
		// temporary array to store between iterations
		PatternsVector dd2;
		/*~~~~~~~*/

		dd2.setBounds(0, M2 - 1);

		/*~~~~~~~~~~~~~~~~~~*/
		// temporary array to store between iterations
		PatternsVector dOverlapArray1;
		/*~~~~~~~~~~~~~~~~~~*/

		dOverlapArray1.setBounds(0, M1 - 1);	// temporary array to pass between iterations
		for (long i = 0; i < M1; i++)
		{
			dOverlapArray1(i) = 0;
		}

		for (long i = 0; i < M1; i++)
		{
			dd1(i) = 0; // initial
		}

		//~~~~~~~~~~~~~~~~~~
		PatternsVector dOverlapArray2;
		/*~~~~~~~~~~~~~~~~~~*/

		dOverlapArray2.setBounds(0, M2 - 1);	// temporary array to pass between iterations
		for (long i = 0; i < M2; i++)
		{
			dOverlapArray2(i) = 0;
		}

		for (long i = 0; i < M2; i++)
		{
			dd2(i) = 0; // initial
		}

		//~~~~~
		PatternsVector a;
		/*~~~~~*/

		a.setBounds(0, N - 1);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// signal's segment to pass
		// real N should be 2^p (other case MATLAB expand it inside!)
		long Range = m_nPtsCount / N;
		long RestPart = m_nPtsCount % N;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// RestPart + Range * N = m_nPtsCount
		for (long j = 0; j < Range; j++)
		{
			// fill current signal segment to pass into the filter
			for (long i = 0; i < N; i++)
			{			// read next segment
				a(i) = m_pdSignalToProcess[j * N + i];
			}

			FftConvolution(a, N, b1, M1, dOverlapArray1, true);

			// we can optimize asking to make or not fft(coef)!
			for (long i = 0; i < iDelay1; i++)
			{			// segment begin part
				m_pdSignalToProcess[j * N + i] = a(i + iDelay1) + dd1(i + iDelay1);
			}

			for (long i = iDelay1; i < N - iDelay1; i++)
			{			// segment middle part
				m_pdSignalToProcess[j * N + i] = a(i + iDelay1);
			}

			if (j > 0)
			{			// Initial segment
				for (long i = N - iDelay1; i < N; i++)
				{		// prev segment end part
					m_pdSignalToProcess[(j - 1) * N + i] = a(i + iDelay1 - N) + dd1(i + iDelay1 - N);
				}
			}

			for (long i = 0; i < M1; i++)
			{
				dd1(i) = dOverlapArray1(i);
			}
		}

		return true;
	}

	// include any header file for the third party library. however make sure they do
	// not use library linking. You want to make it plug and play (in other words, you
	// do not want to have to ship another library with this one, then make it
	// LoadLib() with a catch on the load. If the dll doesn't exist then default to
	// one that does or raise error.
#include "fftw3.h"

	//
	// =======================================================================================================================
	//    CFHRSignal::FftConvolution Main function to process FFT Convolution Allow to use internal FFT implementation
	//    (realFFT) or external (OOURA) implementation controlled by m_ConfigFHR.m_bUseOOURA
	// =======================================================================================================================
	//
	void CFHRSignal::FftConvolution(PatternsVector& signal, long signalLen, const PatternsVector& filter, int filterLen, PatternsVector& overlap, bool bConvertCoeffs)
	{
		/*~~~~~~*/
		PatternsVector a1;
		PatternsVector a2;
		long nl;
		long i;
		double t1;
		double t2;
		/*~~~~~~*/

		nl = signalLen + filterLen;
		i = 1;
		while (i < nl)
		{
			i = i * 2;
		}

		nl = i;
		a1.setBounds(0, nl - 1);
		a2.setBounds(0, nl - 1);

		for (i = 0; i < signalLen; i++)
		{
			a1(i) = signal(i);
		}

		for (i = signalLen; i <= nl - 1; i++)
		{
			a1(i) = 0;
		}

		for (i = 0; i < filterLen; i++)
		{
			a2(i) = filter(i);
		}

		for (i = filterLen; i < nl; i++)
		{
			a2(i) = 0;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int nLibrary = m_ConfigFHR.GetFFTLibrary();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (!m_ConfigFHR.m_bUseOOURA)
		{
			nLibrary = CConfigFHR::eINTERNAL;
		}

		// here we have a conditional for the different FFT libraries this dll can support
		switch (nLibrary)
		{
		case CConfigFHR::eINTERNAL:
			{
				realFFT(a1, nl, false);

				if (bConvertCoeffs)
				{
					realFFT(a2, nl, false); // FFT (coeffs)
				}

				a1(0) = a1(0) * a2(0);
				a1(1) = a1(1) * a2(1);
				for (i = 1; i < nl / 2; i++)
				{
					t1 = a1(2 * i);
					t2 = a1(2 * i + 1);
					a1(2 * i) = t1 * a2(2 * i) - t2 * a2(2 * i + 1);
					a1(2 * i + 1) = t2 * a2(2 * i) + t1 * a2(2 * i + 1);
				}

				realFFT(a1, nl, true);
				for (i = 0; i < signalLen; i++)
				{
					signal(i) = a1(i);
				}

				for (i = 0; i < filterLen; i++)
				{
					overlap(i) = a1(i + signalLen);
				}

				break;
			}

		case CConfigFHR::eFFTW30:
			{
				break;
			}

		default:	// if eOOURA or eNONE
			{
				/*~~~~~~~~~~~*/
				OOURA objOOURA;
				/*~~~~~~~~~~~*/

				// default to eOOURA
				objOOURA.fft(nl, a1.getContent());
				if (bConvertCoeffs)
				{
					objOOURA.fft(nl, a2.getContent());
				}

				a1(0) = a1(0) * a2(0);
				a1(1) = a1(1) * a2(1);
				for (i = 1; i < nl / 2; i++)
				{
					t1 = a1(2 * i);
					t2 = a1(2 * i + 1);
					a1(2 * i) = t1 * a2(2 * i) - t2 * a2(2 * i + 1);
					a1(2 * i + 1) = t2 * a2(2 * i) + t1 * a2(2 * i + 1);
				}

				objOOURA.ifft(nl, a1.getContent());
				for (i = 0; i < signalLen; i++)
				{
					signal(i) = a1(i);
				}

				for (i = 0; i < filterLen; i++)
				{
					overlap(i) = a1(i + signalLen);
				}

				break;
			}
		}
	}

	/*
	=======================================================================================================================
		DetectBaseline - detect candidate baselines, update appropriate baseline buffers and compute features for multiH classification
	=======================================================================================================================
	*/
	bool CFHRSignal::DetectBaseline(void)
	{
		// m_pBaseLinesNewInWindow : baseline candidates newly stable in current window
		// m_pBaseLinesPending : baseline candidates pending (i.e. from previous window
		// but unstable). Will be updated with pending baselines from current window
		// m_pBaseLinesTotal: buffer of all stable candidates over all windows (saved in
		// abs. coords) m_pBaseLinesFinal: buffer of baselines that is classified in
		// multiHypothesis and is used for accelDecel detection

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		fhrPartSet pBaseLinesPendingTemp;
		baseline *pBL;
		CBaseLineDetection basDet;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_pBaseLinesNewInWindow.clear();

		basDet.SetSampFreq(m_dSmpFreq);
		basDet.SetCommitBuffer(m_ConfigFHR.GetMinVarDelay()); // assumes minVar delay is longer that LP delay
		basDet.SetMinTruncateLenSec(3 * 60);
		basDet.m_nOffset = m_OutputAbsStart;
		basDet.m_iRepStart = m_iRepStart;
		basDet.m_iRepEnd = m_iRepEnd;
		basDet.m_lLastValidRepairIndex = m_NextRepairStart - m_OutputAbsStart;
		basDet.m_nNextWindowStart = m_NextBaselineDetectionStart;

		basDet.SetRepairIntervals(&m_pRepairIntervals);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// add repair signals that are of interest for baseline detection
		// Need to shift pending baselines into relative coords for input into BD alg
		//int nBaseLinesPending = m_pBaseLinesPending2->size();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		m_pBaseLinesPending.toRelativeTime(m_OutputAbsStart);
		for (long i = 0; i < m_pBaseLinesPending.size(); i++)
		{
			baseline *b = (baseline*) m_pBaseLinesPending.getAt(i);
			if (b->getX1() < 0)
			{
				b->setX1(0);
			}
			if (b->getX2() < 0)
			{
				b->setX2(0);
			}
		}

		// new stable baselines will be returned in m_pBaseLinesNewInWindow
		// m_pBaseLinesPending contains pending baselines (i.e. unstable) from previous
		// window (if exist) Pending baselines from current window will overwrite
		// m_pBaseLinesPending in baseline.DetectBaseLine
		basDet.DetectBaseLine(this, &m_pBaseLinesNewInWindow, &m_pBaseLinesPending);
		m_pBaseLinesNewInWindow.setAsAbsTime(false); // mark as in relative time
		m_pBaseLinesPending.setAsAbsTime(false);

		m_NextBaselineDetectionStart = basDet.m_nNextWindowStart;

		// Check to see if we have long dropout / repair at end of window - if so upgrade pending to final and force advance of baseline detection start for next window
		// This eliminates the long delays incurred on output when have long dropout
		if (LongRepairAtEndOfWindow())
		{
			bool bAddPending = ((m_pBaseLinesPending.size() > 0) && (m_pBaseLinesPending.getLast()->getX2() + m_OutputAbsStart == m_NextRepairStart));
			if (bAddPending)
			{
				for (int i = m_pBaseLinesPending.size() - 1; i >= 0; i--)
				{
					baseline *b = (baseline*) m_pBaseLinesPending.getAt(i);
					m_pBaseLinesNewInWindow.addcopy(b);
					m_NextBaselineDetectionStart = max(m_NextBaselineDetectionStart, b->getX2() + m_OutputAbsStart + 1);
					m_pBaseLinesPending.removeAt(i);
				}		
			}
			m_pBaseLinesNewInWindow.sortByEnd();
		}

		// Much of this code used to be in multiH but makes more sense to be here Update
		// the appropriate baseline buffers here
		SubdivideLargeBaseLines(&m_pBaseLinesNewInWindow);
		GetBaselinesPolyfit(&m_pBaseLinesNewInWindow);						// Calcs PolyFit and checks degeneracy
		GetBaselinesPolyfit(&m_pBaseLinesPending);
		ComputeLPMean(&m_pBaseLinesPending);
		ComputeLPMean(&m_pBaseLinesNewInWindow);
		ComputeMultiHFeatures(&m_pBaseLinesNewInWindow, &m_pBaseLinesPending);
		ComputeVarMin(&m_pBaseLinesNewInWindow);
		ComputeVarMin(&m_pBaseLinesPending);

		pBaseLinesPendingTemp.addcopy(&m_pBaseLinesPending); // local copy that can modify
		m_pBaseLinesPending.setAsAbsTime(false);
		m_pBaseLinesPending.toAbsTime(m_OutputAbsStart); // to absolute coords for storing until next window

		// Compute multiH features here so only have to do this once For the last bas in
		// pBaseLinesNewInWindow, features dependent on the next baseline will not be
		// stable (in that these features may change in ensuing window)

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int nBaseLinesTotal = m_pBaseLinesTotal.size();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Update the last baseline in the buffer of total baselines - recompute features
		// dependent on ensuing baseline (which is new in this window)
		if (nBaseLinesTotal > 0)
		{
			if (m_pBaseLinesNewInWindow.size() > 0)
			{
				pBL = (baseline *) m_pBaseLinesTotal.getAt(nBaseLinesTotal - 1);
				pBL->toRelativeTime(m_OutputAbsStart);
				if (ComputeMultiHFeaturesBtwnBas(pBL, (baseline *) m_pBaseLinesNewInWindow.getAt(0)))
				{
					pBL->setMhFeatStable(true);
				}
				pBL->toAbsTime(m_OutputAbsStart);
			}
			else if (pBaseLinesPendingTemp.size() > 0)
			{
				pBL = (baseline *) m_pBaseLinesTotal.getAt(nBaseLinesTotal - 1);
				pBL->toRelativeTime(m_OutputAbsStart);
				ComputeMultiHFeaturesBtwnBas(pBL, (baseline *) pBaseLinesPendingTemp.getAt(0));
				pBL->toAbsTime(m_OutputAbsStart);
				// do not set multiH feature stable to true - will want to recompute when ensuing
				// baseline becomes final
			}
		}

		// Add new baselines to total baselines buffer
		for (int i = 0; i < m_pBaseLinesNewInWindow.size(); i++)
		{
			pBL = (baseline *) m_pBaseLinesNewInWindow.getAt(i);
			if (pBL)
			{
				baseline *bas = new baseline;
				(*bas) = (*pBL);
				bas->toAbsTime(m_OutputAbsStart);
				m_pBaseLinesTotal.add(bas);
			}
		}

		// Now assign baselines that are in current window to m_pBaselinesFinal Purge
		// older baselines from m_pBaseLinesTotal
		m_pBaseLinesFinal.clear();
		
		// Oleg - in MH we used m_pBaseLinesTotal.filterEndingBefore, when in BP2 - m_pBaseLinesTotal.filterStartingBefore
		//m_pBaseLinesTotal.filterStartingBefore(m_OutputAbsStart - m_ConfigFHR.GetReqBaseLinesHistory());
		m_pBaseLinesTotal.filterEndingBefore(m_OutputAbsStart - m_ConfigFHR.GetReqBaseLinesHistory());

		for (long i = 0; i < m_pBaseLinesTotal.size(); i++) 
		{
			baseline *b = (baseline *) m_pBaseLinesTotal.getAt(i);
			m_pBaseLinesFinal.addcopy(b);
		}
		m_pBaseLinesFinal.toRelativeTime(m_OutputAbsStart);
		pBaseLinesPendingTemp.setClearMemory(false);
		m_pBaseLinesFinal.add(&pBaseLinesPendingTemp);
		

		m_iRepStart = 0;
		m_iRepEnd = 0;

		// we can delete this m_pdSignalToProcess now
		memset(m_pdSignalToProcess, 0, sizeof(double) * m_nPtsCount);

		m_ConfigFHR.m_PassedState = CConfigFHR::Baseline;

		return true;
	}

	/*
	=======================================================================================================================
	CFHRSignal::ComputeMultiHFeatures Compute features required for multiH classification for the set of baselines
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeMultiHFeatures(fhrPartSet *pBasStable, fhrPartSet *pBasPending)
	{
		ComputeMovingAverage();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// 3 minute moving average

		long lNumStable = pBasStable->size();
		long lNumPending = pBasPending->size();
		baseline *pBL;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = 0; i < lNumStable - 1; i++)
		{
			pBL = (baseline *) pBasStable->getAt(i);
			if (ComputeMultiHFeaturesInBas(pBL))
			{
				if (ComputeMultiHFeaturesBtwnBas(pBL, (baseline *) pBasStable->getAt(i + 1)))
				{
					pBL->setMhFeatStable(true); // features will not change
				}
			}
		}

		// last stable baseline needs to use first pending baseline to compute 'btwn'
		// features these features will not be stable between windows because pending bas
		// may change
		if (lNumStable > 0)
		{
			pBL = (baseline *) pBasStable->getAt(lNumStable - 1);
			ComputeMultiHFeaturesInBas(pBL);
			if (lNumPending > 0)
			{
				ComputeMultiHFeaturesBtwnBas(pBL, (baseline *) pBasPending->getAt(0));
			}

			pBL->setMhFeatStable(false);
		}

		// now do pending baselines
		for (long i = 0; i < lNumPending; i++)
		{
			pBL = (baseline *) pBasPending->getAt(i);
			ComputeMultiHFeaturesInBas(pBL);
			if (i < lNumPending - 1)
			{	// not last baseline
				ComputeMultiHFeaturesBtwnBas(pBL, (baseline *) pBasPending->getAt(i + 1));
			}

			pBL->setMhFeatStable(false);
		}

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeMultiHFeaturesInBas(baseline *pBL)
	{
		ASSERT((pBL->getX1() >= 0) && (pBL->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lMidPt = pBL->midpt();
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (lMidPt >= GetVariabilityCount())
		{
			pBL->setVarPassMid(m_pdVarPassSignal[GetVariabilityCount() - 1]);
		}
		else
		{
			pBL->setVarPassMid(m_pdVarPassSignal[lMidPt]);
		}

		pBL->setMoveAvgMid(m_pdMovingAverage[lMidPt]);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dMax = 0.0;
		double dMin = 999.0;
		double dTotal = 0.0;
		double dTotalSqr = 0.0;
		long lBasLen = pBL->length();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = pBL->getX1(); i <= pBL->getX2(); i++)
		{
			if (m_pdSignal[i] > dMax)
			{
				dMax = m_pdSignal[i];
			}

			if (m_pdSignal[i] < dMin)
			{
				dMin = m_pdSignal[i];
			}

			dTotal += m_pdSignal[i];
			dTotalSqr += sqr(m_pdSignal[i]);
		}

		pBL->setYmean(dTotal / lBasLen);
		pBL->setYmax(dMax);
		pBL->setYmin(dMin);

		// Note that MATLAB var function normalizes by N-1 (i.e. length - 1) - do same to
		// be consistent
		pBL->setMhVar((dTotalSqr / (lBasLen - 1)) - ((pBL->getYmean()) * (dTotal / (lBasLen - 1))));

		return true;
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeMultiHFeaturesBtwnBas(baseline *pBL1, baseline *pBL2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lMidPt = (long) round((pBL1->getX2() + pBL2->getX1()) / 2.0);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (lMidPt >= GetVariabilityCount())
		{
			return false;	// don't have filterVar signal at very end of window
		}

		if (lMidPt < 0)
		{
			lMidPt = 0;		// this should not happen but is technically possible
		}

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		double dMax = 0.0;
		double dMin = 999.0;
		double dTotal = 0.0;
		long lStartIndex = max(0, pBL1->getX2());	// technically possible that pBL1 has -ve index if huge gap between baselines
		long lSegLen = pBL2->getX1() - lStartIndex + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pBL1->setVarPassMidToNext(m_pdVarPassSignal[lMidPt]);

		ASSERT((lStartIndex >= 0) && (pBL2->getX1() < m_nPtsCount)); // LXP OUT OF BOUNDS

		for (long i = lStartIndex; i <= pBL2->getX1(); i++)
		{
			if (m_pdSignal[i] > dMax)
			{
				dMax = m_pdSignal[i];
			}

			if (m_pdSignal[i] < dMin)
			{
				dMin = m_pdSignal[i];
			}

			dTotal += m_pdSignal[i];
		}

		pBL1->setMeanBtwnNext(dTotal / lSegLen);
		pBL1->setMaxBtwnNext(dMax);
		pBL1->setMinBtwnNext(dMin);

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeLPMean(fhrPartSet *pBaseLines)
	{
		for (long i = 0; i < pBaseLines->size(); i++)
		{
			ComputeLPMean((baseline *) pBaseLines->getAt(i));
		}

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeVarMin(fhrPartSet *pBaseLines)
	{
		/*~~~~~~~~~~~*/
		baseline *pBL;
		/*~~~~~~~~~~~*/

		for (long i = 0; i < pBaseLines->size(); i++)
		{
			pBL = (baseline *) pBaseLines->getAt(i);
			pBL->setVarMin(BaselineVarMin(pBL));
		}

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeLPMean(baseline *pBL)
	{
		if ((pBL->getX2() < 0) || (pBL->getX1() > GetLowPassCount() - 1))
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long x1 = max(0, pBL->getX1());
		long x2 = min(pBL->getX2(), GetLowPassCount() - 1);
		double dSum = 0.0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pBL->setLpMean(GetSignalMean(m_pdLowPassSignal, x1, x2));

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeMean(baseline *pBL)
	{
		if ((pBL->getX1() < 0) || (pBL->getX2() > GetTotalPtsCount() - 1))
		{
			ASSERT (FALSE); // LXP OUT OF BOUNDS

			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long x1 = max(0, pBL->getX1());
		long x2 = min(pBL->getX2(), GetTotalPtsCount() - 1);
		double dSum = 0.0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		pBL->setYmean(GetSignalMean(m_pdSignal, x1, x2));


		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CFHRSignal::GetSignalMean(double *s, long x1, long x2)
	{
		double dSum = 0.0;

		for (long i = x1; i <= x2; i++)
		{
			dSum += s[i];
		}
		return(dSum / (double) (x2 - x1 + 1));
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::ComputeMovingAverage(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double d_MH_WINDOW_SIZE = (double) MH_WINDOW_SIZE;	// this should not be defined in samples
		long lIndexStart;	// start index in rel coord
		long lIndexEnd;
		long lOffset = m_OutputAbsStart - m_lLastMovingAvgCompStart;	// offset from last window
		long lDelay = (long) floor(d_MH_WINDOW_SIZE / 2);
		double dFhrSum = 0.0;
		double *pdMovingAverageLocal = new double[m_nPtsCount];			// size of window might change - need to realloc
		// to ensure have enough space
		long i;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// index up until can copy signal computed in last window
		lIndexStart = m_lLastValidMovingAvgIndex - m_OutputAbsStart - lOffset;

		/*
		* m_lLastValid Window N | | | Window N+1 |XXXXXXXXXXXXXXXXXXXXXX | <- Copy from
		* last win-><- Compute new > Note that portion after m_lLastValid is generally
		* much smaller than portion befor
		*/
		if (m_pdMovingAverage)
		{
			// take already computed signal from last window
			for (i = 0; i < lIndexStart; i++)
			{
				pdMovingAverageLocal[i] = m_pdMovingAverage[i + lOffset];
			}
		}

		// now compute for new signal 1) if need to start at very beginning of signal
		lIndexEnd = min(lDelay, m_nPtsCount - lDelay);
		if (lIndexStart < lIndexEnd)
		{
			lIndexStart = 0;						// easier at this point to start from 0 otherwise not easy to reconstruct current dFhrSum from m_pdMovingAverage
		}

		ASSERT((lIndexStart >= 0) && (lIndexEnd + lDelay < m_nPtsCount)); // LXP OUT OF BOUNDS

		for (i = lIndexStart; i < lIndexEnd; i++)
		{
			if (i == 0)
			{
				for (long k = 0; k <= lDelay; k++)
				{
					dFhrSum += m_pdSignal[k];
				}
			}
			else
			{
				dFhrSum += m_pdSignal[i + lDelay];
			}

			pdMovingAverageLocal[i] = dFhrSum / ((double) (lDelay + i + 1));
		}

		// 2) middle section - no problems
		lIndexStart = max(lIndexStart, lIndexEnd);	// advance to end of beginning section if not past already
		lIndexEnd = m_nPtsCount - lDelay;
		dFhrSum = pdMovingAverageLocal[lIndexStart - 1] * d_MH_WINDOW_SIZE;

		ASSERT((lIndexStart >= 0) && (lIndexEnd < m_nPtsCount)); // LXP OUT OF BOUNDS

		for (i = lIndexStart; i < lIndexEnd; i++)
		{
			dFhrSum += (m_pdSignal[i + lDelay] - m_pdSignal[i - lDelay]);
			pdMovingAverageLocal[i] = dFhrSum / d_MH_WINDOW_SIZE;
		}

		// 3) end section - take increasingly smaller window moving average
		for (i = lIndexEnd; i < m_nPtsCount; i++)
		{
			dFhrSum -= m_pdSignal[i - lDelay];
			pdMovingAverageLocal[i] = dFhrSum / ((double) (lDelay + m_nPtsCount - i - 1));
		}

		if (m_pdMovingAverage)
		{
			delete[] m_pdMovingAverage;
		}

		m_pdMovingAverage = pdMovingAverageLocal;

		// if the window ended in repair segment, this repair may be done differently in
		// next window only start from last stable repair index
		m_lLastValidMovingAvgIndex = min(m_NextRepairStart - lDelay, lIndexEnd) + m_OutputAbsStart;
		m_lLastMovingAvgCompStart = m_OutputAbsStart;

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::realFFT Internal FFT implementation
	// =======================================================================================================================
	//
	void CFHRSignal::realFFT(PatternsVector& a, long arrayLen, bool inverseFFT)
	{
		/*~~~~~~~~~~*/
		double twr;
		double twi;
		double twpr;
		double twpi;
		double twtemp;
		double ttheta;
		double wtemp;
		double wr;
		double wpr;
		double wpi;
		double wi;
		double theta;
		double tempr;
		double tempi;
		double c1;
		double c2;
		double h1r;
		double h1i;
		double h2r;
		double h2i;
		double wrs;
		double wis;
		long i;
		long i1;
		long i2;
		long i3;
		long i4;
		long nn;
		long ii;
		long jj;
		long n;
		long mmax;
		long m;
		long j;
		long istep;
		long isign;
		/*~~~~~~~~~~*/

		if (arrayLen == 1)
		{
			return;
		}

		if (!inverseFFT)
		{
			ttheta = double(2 * pi()) / double(arrayLen);
			c1 = 0.5;
			c2 = -0.5;
		}
		else
		{
			ttheta = double(2 * pi()) / double(arrayLen);
			c1 = 0.5;
			c2 = 0.5;
			ttheta = -ttheta;
			twpr = -2.0 * sqr(sin(0.5 * ttheta));
			twpi = sin(ttheta);
			twr = 1.0 + twpr;
			twi = twpi;
			for (i = 2; i < arrayLen / 4; i++)
			{
				i1 = i + i - 2;
				i2 = i1 + 1;
				i3 = arrayLen + 1 - i2;
				i4 = i3 + 1;
				wrs = twr;
				wis = twi;
				h1r = c1 * (a(i1) + a(i3));
				h1i = c1 * (a(i2) - a(i4));
				h2r = -c2 * (a(i2) + a(i4));
				h2i = c2 * (a(i1) - a(i3));
				a(i1) = h1r + wrs * h2r - wis * h2i;
				a(i2) = h1i + wrs * h2i + wis * h2r;
				a(i3) = h1r - wrs * h2r + wis * h2i;
				a(i4) = -h1i + wrs * h2i + wis * h2r;
				twtemp = twr;
				twr = twr * twpr - twi * twpi + twr;
				twi = twi * twpr + twtemp * twpi + twi;
			}

			h1r = a(0);
			a(0) = c1 * (h1r + a(1));
			a(1) = c1 * (h1r - a(1));
		}

		if (inverseFFT)
		{
			isign = -1;
		}
		else
		{
			isign = 1;
		}

		n = arrayLen;
		nn = arrayLen / 2;
		j = 1;
		for (ii = 1; ii <= nn; ii++)
		{
			i = 2 * ii - 1;
			if (j > i)
			{
				tempr = a(j - 1);
				tempi = a(j);
				a(j - 1) = a(i - 1);
				a(j) = a(i);
				a(i - 1) = tempr;
				a(i) = tempi;
			}

			m = n / 2;
			while (m >= 2 && j > m)
			{
				j = j - m;
				m = m / 2;
			}

			j = j + m;
		}

		mmax = 2;
		while (n > mmax)
		{
			istep = 2 * mmax;
			theta = double(2 * pi()) / double(isign * mmax);
			wpr = -2.0 * sqr(sin(0.5 * theta));
			wpi = sin(theta);
			wr = 1.0;
			wi = 0.0;
			for (ii = 1; ii <= mmax / 2; ii++)
			{
				m = 2 * ii - 1;
				for (jj = 0; jj <= (n - m) / istep; jj++)
				{
					i = m + jj * istep;
					j = i + mmax;
					tempr = wr * a(j - 1) - wi * a(j);
					tempi = wr * a(j) + wi * a(j - 1);
					a(j - 1) = a(i - 1) - tempr;
					a(j) = a(i) - tempi;
					a(i - 1) = a(i - 1) + tempr;
					a(i) = a(i) + tempi;
				}

				wtemp = wr;
				wr = wr * wpr - wi * wpi + wr;
				wi = wi * wpr + wtemp * wpi + wi;
			}

			mmax = istep;
		}

		if (inverseFFT) // normalizing
		{
			for (i = 1; i <= 2 * nn; i++)
			{
				a(i - 1) = double(a(i - 1)) / double(nn);
			}
		}

		if (!inverseFFT)
		{
			twpr = -2.0 * sqr(sin(0.5 * ttheta));
			twpi = sin(ttheta);
			twr = 1.0 + twpr;
			twi = twpi;
			for (i = 2; i < arrayLen / 4; i++)
			{
				i1 = i + i - 2;
				i2 = i1 + 1;
				i3 = arrayLen + 1 - i2;
				i4 = i3 + 1;
				wrs = twr;
				wis = twi;
				h1r = c1 * (a(i1) + a(i3));
				h1i = c1 * (a(i2) - a(i4));
				h2r = -c2 * (a(i2) + a(i4));
				h2i = c2 * (a(i1) - a(i3));
				a(i1) = h1r + wrs * h2r - wis * h2i;
				a(i2) = h1i + wrs * h2i + wis * h2r;
				a(i3) = h1r - wrs * h2r + wis * h2i;
				a(i4) = -h1i + wrs * h2i + wis * h2r;
				twtemp = twr;
				twr = twr * twpr - twi * twpi + twr;
				twi = twi * twpr + twtemp * twpi + twi;
			}

			h1r = a(0);
			a(0) = h1r + a(1);
			a(1) = h1r - a(1);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::Approx_Equal(double x1, double x2)
	{
		if ((x1 - x2) > 0.0001)
		{
			return true;
		}

		return false;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::GetBaselinesPolyfit : calc y1 and y2 for baselines using linear interpolation
	// =======================================================================================================================
	//
	void CFHRSignal::GetBaselinesPolyfit(fhrPartSet *bas)
	{
		for (long i = 0; i < bas->size() ; i++)
		{
			GetBaselinePolyfit((baseline *) bas->getAt(i));
		}
	}

	void CFHRSignal::GetBaselinePolyfit(baseline *b)
	{
		double dYBegin, dYEnd;

		ASSERT((b->getX1() >= 0) && (b->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		PatternsVector::CalPolyFit(m_pdSignal, b->getX1(), b->getX2(), dYBegin, dYEnd);

		b->setY2(dYEnd);
		b->setY1(dYBegin);
	}


	// =======================================================================================================================
	//		CFHRSignal::SubdivideLargeBaseLines - check the baselines and split them so that none is greater than 1.5 * maxLen
	//		This addresses issues in real time operation where you have a very long baseline ( > normal window size) that is not 
	//		commitable in current window and then in ensuing window will have indexed baseline that requires variability calculation
	// =======================================================================================================================
	bool CFHRSignal::SubdivideLargeBaseLines(fhrPartSet *bas)
	{
		int maxLen = (long) (60 * 5 * m_dSmpFreq); // 5 minutes

		for (int i = 0; i < bas->size(); ++i)
		{
			baseline *pBL1 = (baseline*)(bas->getAt(i));
			while (pBL1->length() >= 1.5 * maxLen)
			{
				long middle = pBL1->getX1() + (pBL1->length() / 2);

				// Clone the baseline
				baseline* pBL2 = new baseline();
				pBL2->operator =(*pBL1);

				// Split the length over the 2 baselines
				pBL1->setX2(middle);
				pBL2->setX1(middle + 1);

				// Insert the new baseline
				bas->insert(pBL2, i + 1);
			}
		}

		return true;
	}

	////////////////////////////////////////
	//          BP2 process               //
	////////////////////////////////////////

	//bool CFHRSignal::IsSampleRepaired(int tracingDataIndex)
	//{	
	//	int fromIndex = m_lastRepairedDataIndex;
	//	int foundIndex = 0;
	//	bool res = m_pRepairIntervals.Contains(tracingDataIndex, fromIndex, foundIndex);
	//	if(res && foundIndex != -1)
	//	{
	//		m_lastRepairedDataIndex = foundIndex;
	//	}
	//	return res;

	//}

	//bool CFHRSignal::BP2Processing(void)
	//{
	//	
	//	#if BP2_DEBUG_SHOW_OVERALL_CLOCK
	//	fstream myFile;
	//	myFile.open("c:\\temp\\bp2_dur.txt", fstream::app | fstream::in);
	//	clock_t start_ = clock();
	//	#endif

	//	if (m_samplesWindowSize <= 0)
	//		m_samplesWindowSize = FHR_20MIN_PAST_WINDOW;

	//	m_lastRepairedDataIndex = 0;
	//	int endOfLastRepairInterval = m_pRepairIntervals.EndOfLastRepairInterval();
	//	int validInputSizeAfterLastRepair = max(endOfLastRepairInterval, m_NextRepairStart) + FHR_2MIN_WINDOW;
	//	int cycledTracingsSamples = 0;
	//	vector<double>::iterator it = m_tracingsData.begin();

	//	while (it != m_tracingsData.end())
	//	{
	//		double curSample = NaN;
	//		if(!IsSampleRepaired(m_tracingDataIndex))			
	//			curSample = *it;

	//		m_fhrWindowTracings.push_back(curSample);
	//		++cycledTracingsSamples;

	//		try
	//		{							  
	//			CalculateMedian(curSample);				
	//			CalculateMedianResidualHalfWaves();	
	//			CalculateFirFilter(curSample);	
	//			CalculateSkewness(); // Skewness is calculated over Median Residual window of ±10 min
	//		}
	//		catch(...)
	//		{
	//		}

	//		if (m_nTotalCount >= 6000)
	//			bool gothere = true;

	//		// all sliding window logic must come before this "if"
	//		++it;
	//		if (m_lastInsertedIndexToTracingsData >= FHR_20MIN_WINDOW - 1)
	//			m_fhrWindowTracings.remove_head();
	//		else
	//			m_lastInsertedIndexToTracingsData++;
	//		
	//		// END - all sliding window logic must come before this "if"
	//		m_tracingDataIndex++;
	//	}

	//	try
	//	{
	//	CalculateBLDeviations();
	//	}
	//	catch(...)
	//	{
	//	}

	//	try
	//	{
	//	CalculateBLVariability();
	//	}
	//	catch(...)
	//	{
	//	}

	//	try
	//	{
	//		BP2SetStableFeatures();
	//	}
	//	catch(...)
	//	{
	//	}

	//	try
	//	{
	//		SVM();
	//	}
	//	catch(...)
	//	{
	//	}

	//	m_tracingsData.erase(m_tracingsData.begin(), m_tracingsData.begin() + cycledTracingsSamples);
	//	CleanVectors();			
	//
	//	#if BP2_DEBUG_SHOW_OVERALL_CLOCK
	//	clock_t end_ = clock();
	//	long dur = end_ - start_;
	//	myFile << dur << "\r\n";
	//	myFile.close();
	//	#endif

	//	return true;
	//}

	//void CFHRSignal::InsertSampleToSortedArray(double sample)
	//{
	//	// Insertion to sorted array
	//	if (!isnan(sample))
	//	{
	//		m_medianHistogram[(int)sample]++;
	//		++m_medianSamplesWindowSize;
	//	}

	//	++m_samplesWindowSize;
	//}

	//void CFHRSignal::CalculateMedian(double sample)
	//{
	//	InsertSampleToSortedArray(sample);
	//	#if BP2_DEBUG_SHOW_TIMESERIES_SAMPLE
	//	std::fstream fs;
	//	fs.open ("sample.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//	if (isnan(sample))
	//		fs <<  "nan";
	//	else
	//		fs << sample;
	//	fs << ",";
	//	fs.close();
	//	#endif

	//	//After insertion, check whether there's enough data, add result and remove m_tracingsData head from sorted array
	//	if (m_samplesWindowSize >= FHR_20MIN_MEDIAN_WINDOW) // 4800 = 20 minutes in quarter seconds
	//	{
	//		int currIndex = m_lastInsertedIndexToTracingsData;
	//		double curSample = m_fhrWindowTracings[currIndex];
	//		if (isnan(curSample))
	//		{
	//			m_medianResult.push_back(NaN);
	//		}
	//		else if (m_medianSamplesWindowSize == 1)
	//		{
	//			m_medianResult.push_back(curSample);
	//		}
	//		else
	//		{
	//			int medianPos = m_medianSamplesWindowSize / 2;
	//			int cnt = 0;
	//			for (int i = 0; i < MEDIAN_HISTOGRAM_SIZE; i++)
	//			{
	//				cnt += m_medianHistogram[i];
	//				if (cnt >= medianPos + 1)
	//				{
	//					m_medianResult.push_back((double)i);
	//					break;
	//				}

	//				if (cnt == medianPos)
	//				{
	//					double medianVal = 0.0;
	//					int nextNonZeroElemInHstogram = GetNextNonZeroElementInHistogram(i);
	//					if (m_medianSamplesWindowSize % 2 == 0)
	//						medianVal = (i + nextNonZeroElemInHstogram) / 2.;
	//					else
	//						medianVal = (double)nextNonZeroElemInHstogram;

	//					m_medianResult.push_back(medianVal);
	//					break;
	//				}
	//			}
	//		}

	//		#if BP2_DEBUG_SHOW_TIMESERIES_MEDIAN
	//		std::fstream fs;
	//		fs.open ("Median.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//		double val = m_medianResult.back();
	//		if (isnan(val))
	//			fs <<  "NaN";
	//		else
	//			fs << val;
	//		fs << ",";
	//		fs.close();
	//		#endif

	//		double curMedian = m_medianResult.back();
	//		m_medianResidualResult.push_back(isnan(curMedian) || isnan(curSample) ? NaN : curSample - curMedian);

	//		#if BP2_DEBUG_SHOW_TIMESERIES_MEDIANRES
	//		fs.open ("MedianResidual.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//		val = m_medianResidualResult.back();
	//		if (isnan(val))
	//			fs <<  "NaN";
	//		else
	//			fs << val;
	//		fs << ",";
	//		fs.close();
	//		#endif

	//		double toRem = NaN;
	//		if (m_lastInsertedIndexToTracingsData >= (FHR_20MIN_WINDOW - 1))
	//			toRem = m_fhrWindowTracings[0];

	//		if (!isnan(toRem))
	//		{
	//			int histIndToRem = (int)toRem;
	//			--m_medianHistogram[histIndToRem];
	//			if (m_medianHistogram[histIndToRem] < 0)
	//				m_medianHistogram[histIndToRem] = 0;

	//			--m_medianSamplesWindowSize;
	//		}

	//		 --m_samplesWindowSize; 
	//	}
	//}

	//int CFHRSignal::GetNextNonZeroElementInHistogram(int ind)
	//{
	//	for (int i = ind + 1; i < MEDIAN_HISTOGRAM_SIZE; i++)
	//	{
	//		if (m_medianHistogram[i] > 0)
	//			return i;
	//	}
	//}

	//void CFHRSignal::CalculateMedianResidualHalfWaves()
	//{
	//	if (m_medianResidualResult.size() > 0)
	//	{
	//		double lastElem = m_medianResidualResult.back();
	//		m_medianResidualHalfWavePos.push_back(lastElem < 0 || isnan(lastElem) ? 0 : lastElem);

	//		// if more than one sample has accumulated since the last non-NaN sample, interpolate
	//		int currAbsPosIndex = m_shiftFromAbsoluteStart + m_medianResidualHalfWavePos.size() - 1;
	//		if (m_medianResidualHalfWavePos.size() >= 2 && m_lastPosNOTNaN > -1 && currAbsPosIndex > m_lastPosNOTNaN+1)
	//			LinearInterpolation(HalfWavePart::Pos20Min);

	//		AddToInterpolatedVector(HalfWavePart::Pos20Min);
	//		m_lastPosNOTNaN = m_medianResidualHalfWavePos.size() + m_shiftFromAbsoluteStart - 1;

	//		#if BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_POS
	//		std::fstream fs;
	//		fs.open ("MedianResidualHalfWavePos.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//		double val = m_medianResidualHalfWavePos.back();
	//		if (isnan(val))
	//			fs <<  "NaN";
	//		else
	//			fs << val;
	//		fs << ",";
	//		fs.close();
	//		#endif

	//		m_medianResidualHalfWaveNeg.push_back(lastElem > 0 || isnan(lastElem) ? 0 : lastElem);

	//		// if more than one sample has accumulated since the last non-NaN sample, interpolate
	//		int currAbsNegIndex = m_shiftFromAbsoluteStart + m_medianResidualHalfWaveNeg.size() - 1;
	//		if (m_medianResidualHalfWaveNeg.size() >= 2 && m_lastNegNOTNaN > -1 && currAbsNegIndex > m_lastNegNOTNaN+1)
	//			LinearInterpolation(HalfWavePart::Neg20Min);


	//		AddToInterpolatedVector(HalfWavePart::Neg20Min);
	//		m_lastNegNOTNaN = m_medianResidualHalfWaveNeg.size() + m_shiftFromAbsoluteStart - 1;

	//		#if BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_NEG
	//		std::fstream fs2;
	//		fs2.open ("MedianResidualHalfWaveNeg.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//		val = m_medianResidualHalfWaveNeg.back();
	//		if (isnan(val))
	//			fs2 <<  "NaN";
	//		else
	//			fs2 << val;
	//		fs2 << ",";
	//		fs2.close();
	//		hwNegNSamples++;
	//		#endif

	//	}
	//}

	//void CFHRSignal::LinearInterpolation(HalfWavePart part)
	//{
	//	switch (part)
	//	{
	//	case HalfWavePart::Pos20Min:
	//		LinearInterpolationInternal(m_medianResidualHalfWavePos, m_lastPosNOTNaN - m_shiftFromAbsoluteStart, m_medianResidualHalfWavePos.size() - 1);
	//		break;
	//	case HalfWavePart::Neg20Min:
	//		LinearInterpolationInternal(m_medianResidualHalfWaveNeg, m_lastNegNOTNaN - m_shiftFromAbsoluteStart, m_medianResidualHalfWaveNeg.size() - 1);
	//		break;
	//	};
	//}

	//void CFHRSignal::LinearInterpolationInternal(vector<double>& halfWave, int start, int end)
	//{
	//	if (end <= start) 
	//		return;

	//	double slope = (halfWave.at(end) - halfWave.at(start)) / (end - start);
	//	short startVal = halfWave.at(start);
	//	for (int i = 1; i < end - start; i++)
	//	{
	//		halfWave.at(start + i) = startVal + (i * slope);
	//	}
	//}

	//void CFHRSignal::AddToInterpolatedVector(HalfWavePart part)
	//{
	//	switch (part)
	//	{
	//	case HalfWavePart::Pos20Min:
	//		AddToInterpolatedVectorInternal(m_medianResidualHalfWavePos, m_medianResidualHalfPosInterp, m_lastPosNOTNaN - m_shiftFromAbsoluteStart);
	//		break;
	//	case HalfWavePart::Neg20Min:
	//		AddToInterpolatedVectorInternal(m_medianResidualHalfWaveNeg, m_medianResidualHalfNegInterp, m_lastNegNOTNaN - m_shiftFromAbsoluteStart, false);
	//		break;
	//	};
	//}

	//void CFHRSignal::AddToInterpolatedVectorInternal(vector<double>& halfWave, InterpolatedHalfWaveList& halfWaveInterp, int start, bool isUpper)
	//{
	//	#if BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_INTERP
	//	CString fileName = isUpper? "MedianResidualHalfWavePosInterp.txt" : "MedianResidualHalfWaveNegInterp.txt";
	//	std::fstream fs;
	//	fs.open (fileName, std::fstream::in | std::fstream::out | std::fstream::app);		
	//	#endif

	//	for (long i = start + 1; i < halfWave.size(); i++)
	//	{
	//		halfWaveInterp.push_back(halfWave.at(i));

	//		#if BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_INTERP
	//		double val = halfWaveInterp.back();
	//		if (isnan(val))
	//			fs <<  "NaN";
	//		else
	//			fs << val;

	//		fs << ",";
	//		#endif
	//		// PAW
	//		isUpper? hwPosInterpNSamples++ : hwNegInterpNSamples++;
	//	}
	//	int diffSamples = hwPosInterpNSamples - hwPosNSamples;
	//	if (hwPosInterpNSamples != hwPosNSamples+1)
	//		bool gothere = true;
	//	if (hwNegInterpNSamples != hwNegNSamples+1)
	//		bool gothere = true;
	//	
	//	#if BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_INTERP
	//	fs.close();
	//	#endif

	//}

	//void CFHRSignal::CalculateFirFilter(double sample)
	//{
	//	CalculateFirFilterInternal(m_medianResidualHalfPosInterp, m_upperEnvelope, m_upperDeviation, sample);
	//	CalculateFirFilterInternal(m_medianResidualHalfNegInterp, m_lowerEnvelope, m_lowerDeviation, sample, false);
	//}
	//	
	//void CFHRSignal::CalculateFirFilterInternal(InterpolatedHalfWaveList& halfWave, vector<double>& envelope, vector<double>& fhrDeviation, double sample, bool isUpper)
	//{
	//	#if BP2_DEBUG_SHOW_TIMESERIES_ENV
	//	CString fileName = isUpper? "upperEnvelope.txt" : "lowerEnvelope.txt";
	//	std::fstream fs;
	//	fs.open (fileName, std::fstream::in | std::fstream::out | std::fstream::app);		
	//	#endif

	//	while (halfWave.size() >= m_firFilterCoefs.size())
	//	{
	//		double toadd = 0.0;
	//		double midVal = 0.;
	//		InterpolatedHalfWaveList::InterpolatedHalfWaveListElement* pCurElem = halfWave.GetFirstElement();
	//		for (int i = 0; i < m_firFilterCoefs.size() && pCurElem; i++)
	//		{
	//			double val = pCurElem->GetValue();
	//			toadd += m_firFilterCoefs.at(i) * val;
	//			pCurElem = pCurElem->GetNext();
	//			if (i == FIR_LPF_DELAY)
	//				midVal = val;
	//		}

	//		toadd = midVal - toadd;
	//		envelope.push_back(toadd);
	//		
	//		halfWave.erase_head();
	//		CalculateEnvelopeDeviation(envelope, fhrDeviation, sample, isUpper);
	//
	//		#if BP2_DEBUG_SHOW_TIMESERIES_ENV
	//		double val = envelope.back();
	//		if (isnan(val))
	//			fs <<  "NaN";
	//		else
	//			fs << val;
	//		fs << ",";
	//		#endif
	//	}
	//	#if BP2_DEBUG_SHOW_TIMESERIES_ENV
	//	fs.close();
	//	#endif
	//}

	//void CFHRSignal::CalculateEnvelopeDeviation(const vector<double>& envelope, vector<double>& fhrDeviation, double sample, bool isUpper)
	//{
	//	if (envelope.size() <= 0)
	//		return;

	//	fhrDeviation.push_back(isnan(sample) ? NaN : sample - envelope.back());
	//}

	//void CFHRSignal::CalculateSkewness()
	//{
	//	if (m_medianResidualResult.size() > 0)
	//	{
	//		if (m_dataSkewness20MinWindowSize <= 0)
	//		{
	//			m_20MinMedianResiduals.initialize(FHR_20MIN_PAST_WINDOW - 1, NaN);
	//			m_20MinMedianResidualsSquares.initialize(FHR_20MIN_PAST_WINDOW - 1, NaN);
	//			m_20MinMedianResidualsCubes.initialize(FHR_20MIN_PAST_WINDOW - 1, NaN);
	//			m_dataSkewness20MinWindowSize = FHR_20MIN_PAST_WINDOW - 1;
	//		}

	//		double sample = m_medianResidualResult.back();
	//		m_20MinMedianResiduals.push_back(sample);
	//		double sq20 = sample * sample;
	//		m_20MinMedianResidualsSquares.push_back(isnan(sample) ? NaN : sq20);
	//		double cube20 = sq20 * sample;
	//		m_20MinMedianResidualsCubes.push_back(isnan(sample) ? NaN : cube20);
	//		if (!isnan(sample))
	//		{
	//			++m_actualSkewness20MinWindowSize;
	//			m_sum20Min += sample;
	//			m_sumSquares20Min += sq20;
	//			m_sumCubes20Min += cube20;
	//		}

	//		++m_dataSkewness20MinWindowSize;
	//		if (m_dataSkewness20MinWindowSize == FHR_20MIN_MEDIAN_WINDOW)
	//		{
	//			double curRes = m_20MinMedianResiduals.at(FHR_20MIN_PAST_WINDOW - 1);
	//			double mean20Min = m_actualSkewness20MinWindowSize > 0 ? m_sum20Min / m_actualSkewness20MinWindowSize : NaN;
	//			double variance20Min = m_actualSkewness20MinWindowSize > 0 ? m_sumSquares20Min / m_actualSkewness20MinWindowSize - (mean20Min * mean20Min) : NaN;
	//			double sigma20Min = sqrt(variance20Min);
	//			double skew = NaN;
	//			if (!isnan(curRes) && !isnan(mean20Min) && m_actualSkewness20MinWindowSize > 0)
	//				skew = CalculateSkewnessInternal(m_sum20Min, m_sumSquares20Min, m_sumCubes20Min, mean20Min, variance20Min, sigma20Min, m_actualSkewness20MinWindowSize);

	//			m_skew20Min.push_back(skew);
	//			double firstSample = m_20MinMedianResiduals.front();
	//			if (!isnan(firstSample))
	//			{
	//				m_sum20Min -= firstSample;
	//				double sq = m_20MinMedianResidualsSquares.front();
	//				m_sumSquares20Min -= sq;
	//				double cube = m_20MinMedianResidualsCubes.front();
	//				m_sumCubes20Min -= cube;
	//				--m_actualSkewness20MinWindowSize;
	//			}

	//			m_20MinMedianResiduals.remove_head();
	//			m_20MinMedianResidualsSquares.remove_head();
	//			m_20MinMedianResidualsCubes.remove_head();
	//			--m_dataSkewness20MinWindowSize;

	//			if (m_actualSkewness20MinWindowSize == 0)
	//			{
	//				m_sum20Min = 0;
	//				m_sumSquares20Min = 0;
	//				m_sumCubes20Min = 0;
	//			}

	//			#if BP2_DEBUG_SHOW_TIMESERIES_SKEW20
	//			std::fstream fs;
	//			fs.open ("skew20Min.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//			double val = m_skew20Min.back();
	//			if (isnan(val))
	//				fs <<  "NaN";
	//			else
	//				fs << val;
	//			fs << ",";
	//			fs.close();
	//			#endif
	//		}

	//		if (m_dataSkewness4MinWindowSize <= 0)
	//		{
	//			m_4MinMedianResiduals.initialize(FHR_4MIN_PAST_WINDOW - 1, NaN);
	//			m_4MinMedianResidualsSquares.initialize(FHR_4MIN_PAST_WINDOW - 1, NaN);
	//			m_4MinMedianResidualsCubes.initialize(FHR_4MIN_PAST_WINDOW - 1, NaN);
	//			m_dataSkewness4MinWindowSize = FHR_4MIN_PAST_WINDOW - 1;
	//		}

	//		m_4MinMedianResiduals.push_back(sample);
	//		double sq4 = sample * sample;
	//		m_4MinMedianResidualsSquares.push_back(isnan(sample) ? NaN : sq4);
	//		double cube4 = sq4 * sample;
	//		m_4MinMedianResidualsCubes.push_back(isnan(sample) ? NaN : cube4);
	//		if (!isnan(sample))
	//		{
	//			++m_actualSkewness4MinWindowSize;
	//			m_sum4Min += sample;
	//			m_sumSquares4Min += sq4;
	//			m_sumCubes4Min += cube4;
	//		}

	//		++m_dataSkewness4MinWindowSize;
	//		if (m_dataSkewness4MinWindowSize == FHR_4MIN_MEDIAN_WINDOW)
	//		{
	//			double curRes = m_4MinMedianResiduals.at(FHR_4MIN_PAST_WINDOW - 1);
	//			double mean4Min = m_actualSkewness4MinWindowSize > 0 ? m_sum4Min / m_actualSkewness4MinWindowSize : NaN;
	//			double variance4Min = (m_sumSquares4Min / m_actualSkewness4MinWindowSize) - (mean4Min * mean4Min);
	//			double sigma4Min = sqrt(variance4Min);
	//			double skew = NaN;
	//			if (!isnan(curRes) && !isnan(mean4Min) && m_actualSkewness4MinWindowSize > 0)
	//				skew = CalculateSkewnessInternal(m_sum4Min, m_sumSquares4Min, m_sumCubes4Min, mean4Min, variance4Min, sigma4Min, m_actualSkewness4MinWindowSize);

	//			m_skew4Min.push_back(skew);
	//			double firstSample = m_4MinMedianResiduals.front();
	//			if (!isnan(firstSample))
	//			{
	//				m_sum4Min -= firstSample;
	//				double sq = m_4MinMedianResidualsSquares.front();
	//				m_sumSquares4Min -= sq;
	//				double cube = m_4MinMedianResidualsCubes.front();
	//				m_sumCubes4Min -= cube;
	//				--m_actualSkewness4MinWindowSize;
	//			}

	//			m_4MinMedianResiduals.remove_head();
	//			m_4MinMedianResidualsSquares.remove_head();
	//			m_4MinMedianResidualsCubes.remove_head();
	//			--m_dataSkewness4MinWindowSize;

	//			if (m_actualSkewness4MinWindowSize == 0)
	//			{
	//				m_sum4Min = 0;
	//				m_sumSquares4Min = 0;
	//				m_sumCubes4Min = 0;
	//			}

	//			#if BP2_DEBUG_SHOW_TIMESERIES_SKEW4
	//			std::fstream fs;
	//			fs.open ("skew4Min.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//			double val = m_skew4Min.back();
	//			if (isnan(val))
	//				fs <<  "NaN";
	//			else
	//				fs << val;
	//			fs << ",";
	//			fs.close();
	//			#endif

	//		}
	//	}
	//}

	//double CFHRSignal::CalculateSkewnessInternal(double sums, double sumSquares, double sumCubes, double mean, double variance, double sigma, int windowSize)
	//{
	//	double meanCubes = sumCubes / windowSize;
	//	double unBiasedFactor = windowSize > 2 ? sqrt(1.0 * windowSize * (windowSize - 1)) / (windowSize - 2) : NaN;
	//	double toRet = unBiasedFactor * (meanCubes - 3 * mean * variance - pow(mean, 3)) / (sigma * variance);
	//	return toRet;
	//}

	//void CFHRSignal::CalculateBLDeviations()
	//{
	//	if (m_pBaseLinesTotal.size() <= 0)
	//		return;

	//	CalculateBLMedianDeviation();
	//	CalculateBLEnvelopeDeviation();
	//	CalculateBLSkewnessDeviation();
	//}

	//baseline* CFHRSignal::GetBaselineByTime(long x)
	//{
	//	int ind = 0;
	//	baseline* pToRet = NULL;
	//	if (x <= m_pBaseLinesTotal.getAt(ind)->getX1())
	//		pToRet = (baseline*)m_pBaseLinesTotal.getAt(ind);
	//	else
	//	{
	//		for (ind = 1; ind < m_pBaseLinesTotal.size(); ind++)
	//		{
	//			baseline* pTmpBL = (baseline*)m_pBaseLinesTotal.getAt(ind);
	//			if (x <= pTmpBL->getX1())
	//			{
	//				pToRet = (baseline*)m_pBaseLinesTotal.getAt(ind);
	//				break;
	//			}
	//		}
	//	}

	//	return pToRet;
	//}

	//int CFHRSignal::GetFinalBaselineIndexByTime(long x)
	//{
	//	int toRet = 0;
	//	int ind = 0;
	//	
	//	if (x - m_OutputAbsStart <= m_pBaseLinesFinal.getAt(ind)->getX1())
	//		toRet = 0;
	//	else
	//	{
	//		for (ind = 1; ind < m_pBaseLinesFinal.size(); ind++)
	//		{
	//			baseline* pTmpBL = (baseline*)m_pBaseLinesFinal.getAt(ind);
	//			if (x - m_OutputAbsStart <= pTmpBL->getX1())
	//			{					
	//				break;
	//			}
	//		}			
	//		toRet = ind;
	//	}

	//	return toRet;
	//}


	//void CFHRSignal::CalculateBLMedianDeviation()
	//{
	//	if (m_pBaseLinesFinal.empty())
	//		return;

	//	long ind = 0;
	//	baseline* pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);

	//	int envelopeResultMinSize = min(m_upperEnvelope.size(), m_lowerEnvelope.size());
	//	while (pCurBL->getX2() + m_OutputAbsStart - m_shiftFromAbsoluteStart < envelopeResultMinSize)
	//	{
	//		if(!pCurBL->isBP2FeatStable())
	//		{
	//			CalculateBLMedianDeviationInternal(pCurBL);
	//			m_lastBaselineMedianDeviation = pCurBL->getX2() + m_OutputAbsStart + 1;

	//			#if BP2_DEBUG_SHOW_BAS_FEATURE_MEDIANDEV
	//			std::fstream fs;
	//			fs.open ("baselineMedianDeviation.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//			double val = m_medianBLDeviation.back();
	//			fs << m_lastBaselineMedianDeviation << ",";
	//			if (isnan(val))
	//				fs <<  "NaN";
	//			else
	//				fs << val;
	//			fs << "\r\n";
	//			fs.close();
	//			#endif
	//			
	//			m_baselineMedianCount++;
	//		}

	//		ind++;

	//		if (ind >= m_pBaseLinesFinal.size())
	//			break;

	//		pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);
	//	}
	//	#if BP2_DEBUG_SHOW_BAS_PROCESSING_COUNTS
	//	std::fstream fs;
	//	fs.open ("calculateBLMedianDeviation.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//	fs << m_OutputAbsStart<< "," << m_shiftFromAbsoluteStart << "," << m_baselineMedianCount << "," << ind << "\r\n";
	//	fs.close();
	//	#endif

	//	if (m_baselineMedianCount == 1209)
	//		bool gothere = true;
	//}

	//void CFHRSignal::CalculateBLMedianDeviationInternal(baseline* pBL)
	//{
	//	double sum = 0;
	//	long numOfSamples = 0;
	//	long blX2 = pBL->getX2();
	//	for (long i = pBL->getX1(); i <= blX2; i++)
	//	{
	//		long curIndex = i + m_OutputAbsStart - m_shiftFromAbsoluteStart;
	//		ASSERT(curIndex >= 0 && curIndex < m_medianResult.size());
	//		if (!isnan(m_medianResult.at(curIndex)))
	//		{
	//			sum += m_medianResult.at(curIndex);
	//			++numOfSamples;
	//		}
	//	}

	//	double meanMedian = numOfSamples > 0 ? sum / numOfSamples : NaN;
	//	double meanBL = (pBL->getY1() + pBL->getY2()) / 2;
	//	double deviation = !isnan(meanMedian) ? meanBL - meanMedian : NaN;

	//	m_medianBLDeviation.push_back(deviation);
	//}

	//void CFHRSignal::CalculateBLEnvelopeDeviation()
	//{
	//	if (m_pBaseLinesFinal.empty())
	//		return;

	//	long ind = 0;
	//	baseline* pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);

	//	int envelopeResultMinSize = min(m_upperEnvelope.size(), m_lowerEnvelope.size());
	//	while (pCurBL->getX2() + m_OutputAbsStart - m_shiftFromAbsoluteStart < envelopeResultMinSize)
	//	{
	//		if (!pCurBL->isBP2FeatStable())
	//		{
	//			CalculateBLEnvelopeDeviationInternal(pCurBL);
	//			m_lastBaselineEnvelopeDeviation = pCurBL->getX2() + m_OutputAbsStart + 1;
	//		}

	//		ind++;
	//		if (ind >= m_pBaseLinesFinal.size())
	//			break;

	//		pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);
	//	}
	//}

	//void CFHRSignal::CalculateBLEnvelopeDeviationInternal(baseline* pBL)
	//{
	//	double sumUpper = 0;
	//	double sumLower = 0;
	//	long numUpperSamples = 0, numLowerSamples = 0;

	//	for (long i = pBL->getX1(); i <= pBL->getX2(); i++)
	//	{
	//		long curIndex = i + m_OutputAbsStart - m_shiftFromAbsoluteStart;
	//		ASSERT(curIndex >= 0 && curIndex < m_upperEnvelope.size());
	//		if (!isnan(m_upperEnvelope.at(curIndex)))
	//		{
	//			sumUpper += m_upperEnvelope.at(curIndex);
	//			++numUpperSamples;
	//		}
	//		
	//		ASSERT(curIndex < m_lowerEnvelope.size());
	//		if (!isnan(m_lowerEnvelope.at(i + m_OutputAbsStart - m_shiftFromAbsoluteStart)))
	//		{
	//			sumLower += m_lowerEnvelope.at(i + m_OutputAbsStart - m_shiftFromAbsoluteStart);
	//			++numLowerSamples;
	//		}
	//	}

	//	m_upperBLDeviation.push_back(numUpperSamples > 0 ? sumUpper / numUpperSamples : NaN);
	//	m_lowerBLDeviation.push_back(numLowerSamples > 0 ? sumLower / numLowerSamples : NaN);

	//	#if BP2_DEBUG_SHOW_BAS_FEATURE_ENV_DEV
	//	std::fstream fsU;
	//	fsU.open ("upperBLDeviation.txt", std::fstream::in | std::fstream::app);
	//	if(numUpperSamples > 0)
	//		fsU << m_upperBLDeviation.back()<< ",";
	//	else
	//		fsU << "NaN,";
	//	fsU.close();
	//	std::fstream fsL;
	//	fsL.open ("lowerBLDeviation.txt", std::fstream::in | std::fstream::app);
	//	if(numLowerSamples > 0)
	//		fsL << m_lowerBLDeviation.back()<< ",";
	//	else
	//		fsL << "NaN,";
	//	fsL.close();
	//	#endif
	//}

	//void CFHRSignal::CalculateBLSkewnessDeviation()
	//{
	//	if (m_pBaseLinesFinal.empty())
	//		return;

	//	long ind = 0;
	//	baseline* pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);

	//	int envelopeResultMinSize = min(m_upperEnvelope.size(), m_lowerEnvelope.size());
	//	while (pCurBL->getX2() + m_OutputAbsStart - m_shiftFromAbsoluteStart < envelopeResultMinSize)
	//	{
	//		if (!pCurBL->isBP2FeatStable())
	//		{
	//			CalculateBLSkewnessDeviationInternal(pCurBL);
	//			m_lastBaselineSkewDeviation = pCurBL->getX2() + m_OutputAbsStart + 1;

	//			#if BP2_DEBUG_SHOW_BAS_FEATURE_SKEW_DEV
	//			std::fstream fs;
	//			fs.open ("baselineSkew20.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//			fs << m_lastBaselineSkewDeviation << ",";
	//			double val = m_skew20MinBLDeviation.back();
	//			if (isnan(val))
	//				fs <<  "NaN";
	//			else
	//				fs << val;
	//			fs << "\r\n";
	//			fs.close();
	//			fs.open ("baselineSkew4.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//			fs << m_lastBaselineSkewDeviation << ",";
	//			val = m_skew4MinBLDeviation.back();
	//			if (isnan(val))
	//				fs <<  "NaN";
	//			else
	//				fs << val;
	//			fs << "\r\n";
	//			fs.close();
	//			#endif
	//		}

	//		ind++;
	//		if (ind >= m_pBaseLinesFinal.size())
	//			break;

	//		pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);
	//	}
	//}

	//void CFHRSignal::CalculateBLSkewnessDeviationInternal(baseline* pBL)
	//{
	//	double sum20Min = 0;
	//	double sum4Min = 0;
	//	long numOfSamples20Min = 0, numOfSamples4Min = 0;
	//	for (long i = pBL->getX1(); i <= pBL->getX2(); i++)
	//	{
	//		long curIndex = i + m_OutputAbsStart - m_shiftFromAbsoluteStart;
	//		ASSERT(curIndex >= 0 && curIndex < m_skew20Min.size());
	//		if (!isnan(m_skew20Min.at(curIndex)))
	//		{
	//			sum20Min += m_skew20Min.at(curIndex);
	//			++numOfSamples20Min;
	//		}

	//		ASSERT(curIndex < m_skew4Min.size());
	//		if (!isnan(m_skew4Min.at(curIndex)))
	//		{
	//			sum4Min += m_skew4Min.at(curIndex);
	//			++numOfSamples4Min;
	//		}
	//	}

	//	m_skew20MinBLDeviation.push_back(numOfSamples20Min > 0 ? sum20Min / numOfSamples20Min : NaN);
	//	m_skew4MinBLDeviation.push_back(numOfSamples4Min > 0 ? sum4Min / numOfSamples4Min : NaN);
	//}

	//void CFHRSignal::CalculateBLVariability()
	//{
	//	if (m_pBaseLinesFinal.empty())
	//		return;

	//	long ind = 0;
	//	baseline* pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);

	//	int envelopeResultMinSize = min(m_upperEnvelope.size(), m_lowerEnvelope.size());
	//	while (pCurBL->getX2() + m_OutputAbsStart - m_shiftFromAbsoluteStart < envelopeResultMinSize)
	//	{
	//		if (!pCurBL->isBP2FeatStable())
	//		{
	//			CalculateBLVariabilityInternal(pCurBL);
	//			m_lastBaselineVariability = pCurBL->getX2() + m_OutputAbsStart + 1;

	//			#if BP2_DEBUG_SHOW_BAS_FEATURE_VAR
	//			std::fstream fs;
	//			fs.open ("baselineVariability.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//			fs << m_lastBaselineVariability << ",";
	//			double val = m_BLVariability.back();
	//			if (isnan(val))
	//				fs <<  "NaN";
	//			else
	//				fs << val;
	//			fs << "\r\n";
	//			fs.close();
	//			#endif
	//		}

	//		ind++;
	//		if (ind >= m_pBaseLinesFinal.size())
	//			break;

	//		pCurBL = (baseline*)m_pBaseLinesFinal.getAt(ind);
	//	}
	//}

	//void CFHRSignal::CalculateBLVariabilityInternal(baseline* pBL)
	//{
	//	double sum = 0, sumSquares = 0;
	//	long numOfItems = 0;
	//	for (long i  = pBL->getX1(); i <= pBL->getX2(); i++)
	//	{
	//		long curIndex = i + m_OutputAbsStart - m_shiftFromAbsoluteStart;
	//		ASSERT(curIndex >= 0);
	//		double curVal = m_medianResidualResult.at(curIndex);
	//		if (!isnan(curVal))
	//		{
	//			sum += curVal;
	//			sumSquares += (curVal * curVal);
	//			++numOfItems;
	//		}
	//	}

	//	double mean = numOfItems > 0 ? sum / numOfItems : NaN;
	//	double meanSquares = numOfItems > 0 ? sumSquares / numOfItems : NaN;

	//	double sigmaSquare = numOfItems > 1 ? (meanSquares - (mean*mean)) * numOfItems / (numOfItems-1) : NaN;
	//	double variability = sqrt(sigmaSquare);
	//	m_BLVariability.push_back(variability);
	//}


	//void CFHRSignal::BP2SetStableFeatures()
	//{
	//	for (int i = m_pBaseLinesFinal.size() - 1; i >= 0; i--)
	//	{
	//		baseline *b = (baseline *) m_pBaseLinesFinal.getAt(i);
	//		if (b->isBP2FeatStable())
	//			break;
	//		if (b->getX2() + m_OutputAbsStart < m_NextRepairStart)
	//		{
	//			b->setBP2FeatStable();
	//		}
	//	}

	//	///*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	//	//long lMidPt = (long) round((pBL1->getX2() + pBL2->getX1()) / 2.0);
	//	///*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	//	//if (lMidPt >= GetVariabilityCount())
	//	//{
	//	//	return false;	// don't have filterVar signal at very end of window
	//	//}
	//}


	//void CFHRSignal::CleanVectors()
	//{
	//	// this is overly-conservative retention of past history, ~corresponding to limit of baseline retention 
	//	// could be later to same some memory (but not time) 
	//	int staleBaselineLimit = m_OutputAbsStart - m_ConfigFHR.GetReqBaseLinesHistory();
	//	long end = staleBaselineLimit - m_shiftFromAbsoluteStart;
	//	if (end <= 0)
	//		return;
	//	m_shiftFromAbsoluteStart = staleBaselineLimit;

	//	m_medianResult.erase(m_medianResult.begin(), m_medianResult.begin() + end);
	//	m_medianResidualResult.erase(m_medianResidualResult.begin(), m_medianResidualResult.begin() + end);
	//	m_medianResidualHalfWavePos.erase(m_medianResidualHalfWavePos.begin(), m_medianResidualHalfWavePos.begin() + end);
	//	m_medianResidualHalfWaveNeg.erase(m_medianResidualHalfWaveNeg.begin(), m_medianResidualHalfWaveNeg.begin() + end);
	//	m_upperEnvelope.erase(m_upperEnvelope.begin(), m_upperEnvelope.begin() + end);
	//	m_lowerEnvelope.erase(m_lowerEnvelope.begin(), m_lowerEnvelope.begin() + end);
	//	m_upperDeviation.erase(m_upperDeviation.begin(), m_upperDeviation.begin() + end);
	//	m_lowerDeviation.erase(m_lowerDeviation.begin(), m_lowerDeviation.begin() + end);
	//	m_skew20Min.erase(m_skew20Min.begin(), m_skew20Min.begin() + end);
	//	m_skew4Min.erase(m_skew4Min.begin(), m_skew4Min.begin() + end);
	//}

	//int CFHRSignal::GetBp2StableBaselineLimit(bool disabled)
	//{
	//	return disabled ? 1000000:
	//				min(min(m_lastBaselineMedianDeviation, m_lastBaselineVariability), min(m_lastBaselineEnvelopeDeviation, m_lastBaselineSkewDeviation));
	//}

	//CString CFHRSignal::GetSVMModelsPath()
	//{
	//	if(m_svmModelsPath.IsEmpty())
	//	{
	//		TCHAR szEXEPath[2048];	
	//		GetModuleFileName ( NULL, szEXEPath, 2048 );
	//		CString str(szEXEPath);
	//		int slash = str.ReverseFind('\\');
	//		if(slash > 0)
	//		{
	//			m_svmModelsPath = str.Left(slash + 1);
	//		}
	//	}
	//	return m_svmModelsPath;
	//}
	//void CFHRSignal::SVM()
	//{
	//	SVMInput svmInput(m_medianBLDeviation, 
	//		m_BLVariability, 
	//		m_skew20MinBLDeviation, 
	//		m_skew4MinBLDeviation, 
	//		m_upperBLDeviation, 
	//		m_lowerBLDeviation);

	//	int numOfExamples = svmInput.GetNumber();
	//	if(numOfExamples > 0)
	//	{
	//		CString modelPath = GetSVMModelsPath();
	//		for(int i = 1; i <= 10; i++)
	//		{
	//			if(i > m_svmModels.size())
	//			{
	//				char num[5];
	//				itoa(i, num, 10);
	//				CString modelName = modelPath + "model" + num;
	//				SVMModel* model = new SVMModel(modelName, 6, 1, true, false);
	//				m_svmModels.push_back(model);
	//			}
	//		}

	//		vector<int> positiveResults;

	//		if(numOfExamples -  svmInput.NumOfRejectedExamples() > 0)
	//		{
	//			#if BP2_DEBUG_SHOW_CLASSIFICATION_BY_FOLD
	//			std::fstream fs;
	//			fs.open ("baselineSVMClassificationByFold.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//			#endif

	//			for(int iModel = 0; iModel < m_svmModels.size(); iModel++)
	//			{
	//				SVMModel* model = m_svmModels[iModel];
	//				if(model && model->Init())
	//				{
	//					SVMTest svmTest;
	//					if(svmTest.Run(svmInput, model))
	//					{
	//						int size = svmTest.m_results.size();
	//						if(size != numOfExamples)
	//						{								
	//							throw exception("Unable to verify the SVM result");
	//						}

	//						for(int iExample = 0; iExample < size; iExample++)
	//						{						
	//							if(positiveResults.size() <= iExample)
	//							{
	//								positiveResults.push_back(0);
	//							}
	//
	//							if(svmTest.m_results[iExample] == 1)	
	//							{
	//								positiveResults[iExample]++;
	//							}
	//							#if BP2_DEBUG_SHOW_CLASSIFICATION_BY_FOLD
	//							fs << svmTest.m_results[iExample];
	//							if (iExample < size-1 )
	//								fs << ',';
	//							else
	//								fs << "\r\n";
	//							#endif
	//						}
	//					}
	//				}
	//				else
	//				{
	//					throw exception("Load SVM Model Error");						
	//				}
	//			}
	//			#if BP2_DEBUG_SHOW_CLASSIFICATION_BY_FOLD
	//			fs.close();
	//			#endif
	//		}
	//		
	//		if(positiveResults.size() < numOfExamples)
	//		{				
	//			throw exception("Unable to verify the SVM result");
	//		}

	//		int numOfRejected = svmInput.NumOfRejectedExamples();		
	//		int index = m_pBaseLinesFinal.size() - 1;
	//		for (int i = 0; i < m_pBaseLinesFinal.size(); i++) 
	//		{
	//			baseline *b = (baseline *) m_pBaseLinesFinal.getAt(i);
	//			if (!b->isBP2Stable())
	//			{
	//				index = i;
	//				break;
	//			}
	//		}

	//		int resIndex = 0;
	//
	//		#if BP2_DEBUG_SHOW_CLASSIFICATION
	//		std::fstream fs;
	//		fs.open ("baselineSVMClassification.txt", std::fstream::in | std::fstream::out | std::fstream::app);
	//		#endif

	//		int voteThreshold = m_svmModels.size()/2;

	//		for(int i = 0; i < numOfExamples + numOfRejected;  i++)
	//		{				
	//			if(index >=0 && index < m_pBaseLinesFinal.size())
	//			{
	//				
	//				baseline* pCurBLFinal  = (baseline*)m_pBaseLinesFinal.getAt(index);
	//				
	//				ASSERT(!pCurBLFinal->isBP2Stable());

	//				index++;
	//				bool positive = false;
	//				if(!svmInput.IsBLCandidateRejected(i))
	//				{
	//					positive = (positiveResults[resIndex] >= voteThreshold);
	//					resIndex++;
	//				}
	//			
	//			
	//				if(pCurBLFinal)
	//				{
	//					pCurBLFinal->setMhClass(positive);	
	//					if(positive)
	//					{
	//						pCurBLFinal->setMhType(baseline::MH_NONE);
	//						pCurBLFinal->setMhStable(true);
	//					}

	//					if (pCurBLFinal->isBP2FeatStable())
	//					{
	//						pCurBLFinal->setBP2Stable(true);
	//						// write BP2 result to total buffer so used in subsequent windows
	//						UpdateTotalBaseLineBuffer(pCurBLFinal);			
	//					}

	//				}
	//				#if BP2_DEBUG_SHOW_CLASSIFICATION
	//				fs << svmExampleCount << "," << i << "," << m_OutputAbsStart+pCurBLFinal->getX1() << "," << m_OutputAbsStart+pCurBLFinal->getX2() << ",";
	//				int val = svmInput.IsBLCandidateRejected(i) ? -1 : positive;
	//				fs << val;
	//				fs << "\r\n";
	//				#endif
	//			}
	//		}
	//		#if BP2_DEBUG_SHOW_CLASSIFICATION
	//		svmExampleCount += numOfExamples;
	//		fs.close();
	//		#endif
	//	}

	//	CleanSVMInput(numOfExamples);
	//}

	//void CFHRSignal::CleanSVMInput(int nExamples)
	//{
	//	m_medianBLDeviation.erase(m_medianBLDeviation.begin(), m_medianBLDeviation.begin() + nExamples);
	//	m_BLVariability.erase(m_BLVariability.begin(), m_BLVariability.begin() + nExamples);
	//	m_skew4MinBLDeviation.erase(m_skew4MinBLDeviation.begin(), m_skew4MinBLDeviation.begin() + nExamples);
	//	m_skew20MinBLDeviation.erase(m_skew20MinBLDeviation.begin(), m_skew20MinBLDeviation.begin() + nExamples);
	//	m_upperBLDeviation.erase(m_upperBLDeviation.begin(), m_upperBLDeviation.begin() + nExamples);
	//	m_lowerBLDeviation.erase(m_lowerBLDeviation.begin(), m_lowerBLDeviation.begin() + nExamples);
	//}

	//
	// =======================================================================================================================
	//    CFHRSignal::MultiHypothesis Function loads the the nnet and given the baseline that have been detected classifies
	//    them use a neural net RETURNS: true iff the function is successful
	// =======================================================================================================================
	//
	bool CFHRSignal::MultiHypothesis(void)
	{
		if (!m_bMultihypothesisTopologyLoaded)
		{
			if (!m_ConfigFHR.LoadMultihypothesisTopology())
			{
				throw exception("Load / Validate Topology Error");
				return false;
			}

			m_bMultihypothesisTopologyLoaded = true;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int nBaseLines = m_pBaseLinesFinal.size();

		CStdioFile fileFeature_Statistic;
		CString sFeature_Statistic;
		CString sFeature_Stat;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		try
		{
			if (m_ConfigFHR.m_bStoreXML)
			{
				if (m_OutputAbsStart == 0)
				{
					fileFeature_Statistic.Open(CString(m_ConfigFHR.m_strXMLFilePrefix.c_str()) + CString("_MH_Feature_Statistic_Values.txt"), CFile::modeCreate | CFile::modeWrite | CFile::typeText);
				}
				else
				{
					fileFeature_Statistic.Open
						(
						CString(m_ConfigFHR.m_strXMLFilePrefix.c_str()) + CString("_MH_Feature_Statistic_Values.txt"),
						CFile::modeNoTruncate | CFile::modeCreate | CFile::modeWrite | CFile::typeText
						);
					fileFeature_Statistic.SeekToEnd();
				}
			}

			// sFeature_Statistic.Format("Feature PatternsVector in %d baselines\n", nBaseLines);
			// if (m_ConfigFHR.m_bStoreXML) fileFeature_Statistic.WriteString(sFeature_Statistic);
		}

		catch(...)
		{
		}

		/*~~~~~~~~~~~~~~~*/
		double dRes(0);
		baseline *pBL[5];
		baseline *pCurrBL;
		/*~~~~~~~~~~~~~~~*/
		for (int i = 0; i < min(4, nBaseLines); i++) 
		{  // first two baselines can't be classified - accept them
			pCurrBL = (baseline *) m_pBaseLinesFinal.getAt(i);
			if ((!(pCurrBL->isMhStable())))
			{
				pCurrBL->setMhClass(true);
				pCurrBL->setMhType(baseline::MH_NONE);
				pCurrBL->setMhStable(true);
			}
		}

		// For N baselines (0 -> N-1) can do nothing for first 2, primary for 2->N-3, and
		// secondary for N-2,N-1
		for (int i = 2; i < nBaseLines - 2; i++)
		{
			pCurrBL = (baseline *) m_pBaseLinesFinal.getAt(i);
			if ((!(pCurrBL->isMhStable())) || (pCurrBL->getMhType() != baseline::MH_PRIM))
			{		// not already classified under stable conditions
				for (int j = i - 2; j <= i + 2; j++)
				{	// take 2 prior baselines and 2 ensuing baselines
					pBL[j - i + 2] = (baseline *) m_pBaseLinesFinal.getAt(j);
				}

				if (m_ConfigFHR.m_bStoreXML)
				{
					dRes = Calculate_FeaturesPrimary(pBL, (void *) &fileFeature_Statistic);
				}
				else
				{
					dRes = Calculate_FeaturesPrimary(pBL);
				}

				pBL[2]->setMhType(baseline::MH_PRIM);
				if (dRes <= MH_THRESHOLD)
				{
					pBL[2]->setMhClass(false);
				}
				else
				{
					pBL[2]->setMhClass(true);
					m_dLastGoodBasRef = pBL[2]->getYmean();	// keep as ref in case long dropout
				}

				// if last baseline (pBL[3]) has stable features, then classification for pBL[2]
				// will never change and we don't need to redo pBL[3] having stable features means
				// that pBL[4] is not a pending baseline
				pBL[2]->setMhStable(pBL[3]->isMhFeatStable());
				UpdateTotalBaseLineBuffer(pBL[2]);			// write multiH result to total buffer so used in subsequent windows
			}
		}

		// Now do secondary classification (using 4 prior baselines) for baselines N-2,
		// N-1 Need at least 4 prior baseline in buffer
		for (int i = max(4, nBaseLines - 2); i < nBaseLines; i++)
		{
			pCurrBL = (baseline *) m_pBaseLinesFinal.getAt(i);
			if (!(pCurrBL->isMhStable()))
			{
				for (int j = i - 4; j <= i; j++)
				{
					pBL[j - i + 4] = (baseline *) m_pBaseLinesFinal.getAt(j);
				}

				if (m_ConfigFHR.m_bStoreXML)
				{
					dRes = Calculate_FeaturesSecondary(pBL, (void *) &fileFeature_Statistic);
				}
				else
				{
					dRes = Calculate_FeaturesSecondary(pBL);
				}

				pBL[4]->setMhType(baseline::MH_SEC);
				if (dRes <= MH_THRESHOLD_SEC)
				{
					pBL[4]->setMhClass(false);
				}
				else
				{
					pBL[4]->setMhClass(true);
				}

				// If pBL[3] has stable features, then pBL[4] is not pending and secondary
				// classification will not change
				pBL[4]->setMhStable(pBL[3]->isMhFeatStable());
				UpdateTotalBaseLineBuffer(pBL[4]);			// write multiH result to total buffer so used in subsequent windows
			}
		}

		try
		{
			if (m_ConfigFHR.m_bStoreXML)
			{
				fileFeature_Statistic.Close();
			}
		}

		catch(...)
		{
		}

		m_ConfigFHR.m_PassedState = CConfigFHR::MutliHypothesis;

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::UpdateTotalBaseLineBuffer This updates the multiH information of the baseline in the m_pTotalBaseLines
	//    buffer that corresponds to the baseline pBL. This allows multiH classification to be skipped in future windows if
	//    the classification is stable INPUT: pBL - baseline corresponding to one to be updated in totalBaselines buffer
	//    (indexes in relative coords) RETURNS: true if succesfully updated corresponding baseline, false otherwise
	// =======================================================================================================================
	//
	bool CFHRSignal::UpdateTotalBaseLineBuffer(baseline *pBL)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long x1 = pBL->getX1() + m_OutputAbsStart;
		long x2 = pBL->getX2() + m_OutputAbsStart;
		int nBaseLines = m_pBaseLinesTotal.size();
		baseline *pCurrBL;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// look backwards from end of buffer
		for (int i = nBaseLines - 1; i >= 0; i--)
		{
			pCurrBL = (baseline *) m_pBaseLinesTotal.getAt(i);
			if ((pCurrBL->getX1() == x1) && (pCurrBL->getX2() == x2))
			{					// found baseline in buffer
				pCurrBL->setMhType(pBL->getMhType());
				pCurrBL->setMhClass(pBL->getMhClass());
				pCurrBL->setMhStable(pBL->isMhStable());
				
				// Oleg - BP2 related
				//pCurrBL->setBP2FeatStable(pBL->isBP2FeatStable());
				//pCurrBL->setBP2Stable(pBL->isBP2Stable());
				return true;
			}

			if (pCurrBL->getX2() < x2)
			{
				return false;	// could not find baseline
			}
		}

		return false;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::SimulatePrimaryNN Function SimulatePrimaryNN() simulates a given BL with the primary set of arbiters
	//    INPUT: vFHR_BL differs from the MATLAB code, this should be the vector of length 55 going into this function it
	//    should be build in the calling function Multihypothesis() RETURNS: double value of the vector after running NNet.
	// =======================================================================================================================
	//
	double CFHRSignal::SimulatePrimaryNN(PatternsVector& vFhr_Bl)
	{
		/*~~~~~~~~~~~~~~*/
		double dRet = 0.0;
		/*~~~~~~~~~~~~~~*/

		if (m_ConfigFHR.GetNumberOfExperts() <= 0)
		{
			return DBL_MAX; // error
		}

		if (m_ConfigFHR.GetNnetExperts() == NULL)
		{
			return DBL_MAX; // error
		}

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		PatternsVector* pVector = new PatternsVector[m_ConfigFHR.GetNumberOfExperts()];
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// for each expert get results
		for (int i = 0; i < m_ConfigFHR.GetNumberOfExperts(); i++)
		{
			pVector[i] = m_ConfigFHR.GetNnetExperts()[i].Simulate(vFhr_Bl);
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// take these and input them into Arbiter (the second NNet)
		PatternsVector vAbiterInput(m_ConfigFHR.GetNumberOfExperts());
		PatternsVector vAbiterResult;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_ConfigFHR.GetNumberOfExperts(); i++)
		{
			if (pVector[i].getSize() != 1)	// error wrong NNet
			{
				delete[] pVector;
				pVector = NULL;
				return DBL_MAX;
			}
		}

		for (int i = 0; i < m_ConfigFHR.GetNumberOfExperts(); i++)
		{
			vAbiterInput(i) = pVector[i](0);
		}

		// TO DO LOAD THE NNET -
		vAbiterResult = m_ConfigFHR.GetNnetArbiter()->Simulate(vAbiterInput);

		if (vAbiterResult.getSize() != 1)	// error wrong NNet
		{
			delete[] pVector;
			pVector = NULL;
			return DBL_MAX;
		}

		dRet = vAbiterResult(0);

		// get Results of Arbiter
		delete[] pVector;
		pVector = NULL;

		return dRet;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::SimulateSecondaryNN Function SimulateSecondaryNN() simulates a given BL with the secondary set of
	//    arbiters INPUT: vFhr_Bl differs from the MATLAB code, this should be the vector of length 55 going into this
	//    function it should be build in the calling function Multihypothesis() RETURNS: double value of the vector after
	//    running NNet.
	// =======================================================================================================================
	//
	double CFHRSignal::SimulateSecondaryNN(PatternsVector& vFhr_Bl)
	{
		/*~~~~~~~~~~~~~~*/
		double dRet = 0.0;
		/*~~~~~~~~~~~~~~*/

		if (m_ConfigFHR.GetNumberOfSecondaryExperts() <= 0)
		{
			return DBL_MAX;					// error
		}

		if (m_ConfigFHR.GetNnetSecondaryExperts() == NULL)
		{
			return DBL_MAX;					// error
		}

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		PatternsVector* pVector = new PatternsVector[m_ConfigFHR.GetNumberOfSecondaryExperts()];
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// for each expert get results
		for (int i = 0; i < m_ConfigFHR.GetNumberOfSecondaryExperts(); i++)
		{
			pVector[i] = m_ConfigFHR.GetNnetSecondaryExperts()[i].Simulate(vFhr_Bl);
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// take these and input them into Arbiter (the second NNet)
		PatternsVector vAbiterInput(m_ConfigFHR.GetNumberOfSecondaryExperts());
		PatternsVector vAbiterResult;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_ConfigFHR.GetNumberOfSecondaryExperts(); i++)
		{
			if (pVector[i].getSize() != 1)	// error wrong NNet
			{
				delete[] pVector;
				pVector = NULL;
				return DBL_MAX;
			}
		}

		for (int i = 0; i < m_ConfigFHR.GetNumberOfSecondaryExperts(); i++)
		{
			vAbiterInput(i) = pVector[i](0);
		}

		// TO DO LOAD THE NNET -
		vAbiterResult = m_ConfigFHR.GetNnetSecondaryArbiter()->Simulate(vAbiterInput);

		if (vAbiterResult.getSize() != 1)	// error wrong NNet
		{
			delete[] pVector;
			pVector = NULL;
			return DBL_MAX;
		}

		dRet = vAbiterResult(0);

		// get Results of Arbiter
		delete[] pVector;
		pVector = NULL;

		return dRet;
	}

	//
	// =======================================================================================================================
	//    Accel Decel protected methods. CFHRSignal::ACBandPass DESCRIPTION: This function does Fhr signal filtering for one
	//    Frequency Band It takes the input from m_pdSignal and puts the result in m_pdSignalToProcess PARAMETER: seq number
	//    of the Frequency Band RETURNS: true if success
	// =======================================================================================================================
	//
	bool CFHRSignal::ACBandPass(int ind)
	{
		/*~~~~~~~~~~~~*/
		bool bRes(true);
		long lDelay = 0;
		/*~~~~~~~~~~~~*/

		// this is repaired signal (m_pdSignal)
		memcpy(m_pdSignalToProcess, m_pdSignal, sizeof(double) * m_nPtsCount);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// for (int iS = 0;
		// iS < m_nPtsCount;
		// iS++) m_pdSignalToProcess[iS] = m_pdSignal[iS];
		CAccelDecelBandPassConfig *pConf = m_ConfigFHR.GetBandPassConfig(ind);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Low Pass Filtering
		if (pConf->GetLowPassFilter() == NULL)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long M = pConf->GetLowPassFilterLength();	// M - filter length
		long iBlocksize = 32768;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while (m_nPtsCount < iBlocksize)
		{
			iBlocksize /= 2;
		}

		if (iBlocksize < 3 * M / 2)
		{
			iBlocksize *= 2;
		}

		/*~~~~~~~~~~~~~~~~~~~~*/
		long N = iBlocksize - M;
		/*~~~~~~~~~~~~~~~~~~~~*/

		// bRes = ApplyFilterDirect(pConf->GetLowPassFilter(), M);
		bRes = ApplyFilter(M, N, // N - array length
			pConf->GetLowPassFilter(), // Filter
			M / 2); // Delay
		lDelay = M / 2;

		// High Pass Filtering
		if (pConf->GetHighPassFilter() == NULL)
		{
			m_lFiltered = m_nPtsCount - lDelay;
			return false;
		}
		else
		{
			M = pConf->GetHighPassFilterLength();	// M - filter length
			if (1 == M) // identity filter case
			{
				m_lFiltered = m_nPtsCount - lDelay;
				return true;
			}
		}

		iBlocksize = OptimalBuffer(M);
		while (m_nPtsCount < iBlocksize)
		{
			iBlocksize /= 2;
		}

		N = iBlocksize - M;

		// bRes = ApplyFilterDirect(pConf->GetLowPassFilter(), M);
		bRes = ApplyFilter(M, N, // N - array length
			pConf->GetHighPassFilter(), // Filter
			M / 2);	   // Delay

		lDelay += M / 2;

		m_lFiltered = m_nPtsCount - lDelay;

		return bRes;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::ACZeroCrossing DESCRIPTION: This function founds zero crossings for Fhr signal filtering for one
	//    Frequency Band. It takes the input from m_pdSignalToProcess and puts the result in lZeroCrossings (supplied
	//    pointer) PARAMETERS: seq number of the Frequency Band RETURNS: true if success
	// =======================================================================================================================
	//
	bool CFHRSignal::ACZeroCrossing
		(
		long **lZeroCrossings,	// pointer to store the result
		long *lPoints,			// number of crossings
		int ind
		)	// seq number of Frequncy Band
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRes(true);
		CAccelDecelBandPassConfig *pConf = m_ConfigFHR.GetBandPassConfig(ind);
		long lMaxDecelCandLen = (long) (pConf->GetDecelMaxBumpCandLength() * m_dSmpFreq);
		long lMaxAccelCandLen = (long) (pConf->GetAccelMaxBumpCandLength() * m_dSmpFreq);
		long lStartIndex = (long) (min(m_commitIndexDec - m_OutputAbsStart - lMaxDecelCandLen, m_accCutoff - m_OutputAbsStart - lMaxAccelCandLen));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		lStartIndex = max(lStartIndex, 0);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// can't start before beginning of window
		long lDelay = (pConf->GetLowPassFilterLength() + pConf->GetHighPassFilterLength()) / 2;
		long lEndBuffer = GetRepairEndBuffer();					// end of repaired signal may not be stable - do not want to take ZC from unstable portion
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		if ((lEndBuffer >= lDelay) || (LongRepairAtEndOfWindow()))
		{
			lEndBuffer = 0;										// if have repair as long as filter delay then no point in waiting for clean signal - proceed w/ what we have
		}

		//~~~~~~~~~~~
		long lZero = 0;
		/*~~~~~~~~~~~*/

		if (m_lFiltered > 1)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long *lTemp = new long[m_lFiltered - lStartIndex];	// Do not know its length yet
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

			for (long l = lStartIndex; l < (m_lFiltered - lEndBuffer); l++)
			{
				if (sign(m_pdSignalToProcess[l]) != sign(m_pdSignalToProcess[l + 1]))
				{
					// lTemp [lZero] = l + 1;
					lTemp[lZero] = l;	// To be consistent w/ same signal sample as in MATLAB - ce (NOTE that sample indexes will be offset

					// by one sample as compared to MATLAB because MATLAB array index start at 1)
					lZero++;
				}
			}

			if (LongRepairAtEndOfWindow()) // special case - do not let any ZC finish more than LongRep into long repair
			{ // in these cases BP signal will hang out just below or above 0 w/out crossing and just delay output
				long longRep = m_ConfigFHR.GetMaxRepairForCommitIndexForce();
				long maxZC = m_NextRepairStart + longRep;
				long z = lZero - 1;
				while ((z >= 0) && (lTemp[z] > maxZC))
				{
					lTemp[z] = maxZC;
					lZero = z+1;
					z--;
				}
			}


			for (long k = 0; k < lZero; k++)
			{
				(*lZeroCrossings)[k] = lTemp[k];
			}

			delete[] lTemp;
			lTemp = NULL;
		}

		*lPoints = lZero;

		return bRes;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::AccelDecel DESCRIPTION: This function performs all the steps related to Accelerations and Decelerations
	//    processing PARAMETERS: RETURNS: true if success CHE - split this up into bump competition, bump classification, and
	//    merging (if not more) - method is 400 lines long
	// =======================================================================================================================
	//
	bool CFHRSignal::AccelDecel(void)
	{
		// if (m_bSetMode)
		RemoveNOB();

		if (!m_bAccelDecelTopologyLoaded)
		{
			if (!m_ConfigFHR.LoadBumpClassifyTopology())
			{
				throw exception("LoadBumpClassifyTopology failed!");
				return false;
			}

			m_bAccelDecelTopologyLoaded = true;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long *lZC = new long[m_nPtsCount];	// Zero Crossings
		long lZCPoints;
		int iBands = m_ConfigFHR.GetNumberOfFreqBands();
		long *dLastZCPos = new long[iBands];
		long *dLastZCNeg = new long[iBands];
		fhrPartSet PositiveBumps;
		fhrPartSet NegativeBumps;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Populate PositiveBumps and NegativeBumps with buffer of current competition
		// winners This way we only have to consider new bump candidates
//		CopyBumps(m_pPosCandidates, PositiveBumps, m_lNumPosCand);
		PositiveBumps.addcopy(&m_pPosCandidates);
		PositiveBumps.toRelativeTime(m_OutputAbsStart);
		NegativeBumps.addcopy(&m_pNegCandidates);
		NegativeBumps.toRelativeTime(m_OutputAbsStart);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bR = FALSE;
		long iEarliestNextWindowAccStart = 0;						// for purposes of baseline commit
		long iEarliestNextWindowDecStart = 0;						// for baseline and accel commit
		long MaxDelay = (long) (m_ConfigFHR.GetMaxDelay() * m_dSmpFreq * 60);
		long nRealPts = GetNumRealSignal();
		long commitIndexPos = nRealPts;
		long commitIndexNeg = nRealPts;
		long commitIndexDec = nRealPts;
		long commitIndexAcc = nRealPts;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// START OF BUMP COMPETITION LOOP
		for (int i = 0; i < iBands; i++)
		{
			bR = ACBandPass(i);

			if (m_ConfigFHR.m_iStoreXML > 2)
			{
				bR = OutputBandPass(i);
			}

			bR = ACZeroCrossing(&lZC, &lZCPoints, i);

			if (m_ConfigFHR.m_iStoreMAT > 1)
			{
				OutputMatlab(lZC, lZCPoints, i);
			}

			if (m_ConfigFHR.m_iStoreXML > 2)
			{
				bR = OutputZeroCrossings(lZC, lZCPoints, i);
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			fhrPartSet PosBumpsLocal;
			fhrPartSet NegBumpsLocal;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			getPosNegBumps(lZC, lZCPoints, i, dLastZCPos, dLastZCNeg, &PosBumpsLocal, &NegBumpsLocal);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long lLastValidBPSample = max(0, m_lFiltered - GetNumExtrapolatedSignal() - GetRepairEndBuffer() - 1);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (m_pdSignalToProcess[lLastValidBPSample] < 0)		// ends midway through decel
			{
				commitIndexNeg = max(dLastZCNeg[i], dLastZCPos[i]); // commmit index for current band
				commitIndexPos = lLastValidBPSample;				// commit index for current band
			}
			else	// ends midway through accel
			{
				commitIndexNeg = lLastValidBPSample;					// commit index for current band
				commitIndexPos = max(dLastZCPos[i], dLastZCNeg[i]);		// commit index for current band
			}

			NegBumpsLocal.sortByEnd();

			/*~~~~~~~~*/
			long lc = 0;
			/*~~~~~~~~*/

			for (lc = 0; lc < NegBumpsLocal.size(); lc++)
			{
				FreqBandDependentBumpMeasures(m_pdSignalToProcess, (decel*) NegBumpsLocal.getAt(lc));
			}

			PosBumpsLocal.sortByEnd();

			if (m_ConfigFHR.NumBandsAccel() > i)
			{
				BumpCompetition(&PositiveBumps, &PosBumpsLocal, true);
				commitIndexAcc = min(commitIndexPos, commitIndexAcc);	// update commit index over all bands
			}

			if (m_ConfigFHR.NumBandsDecel() > i)
			{
				BumpCompetition(&NegativeBumps, &NegBumpsLocal, false);
				commitIndexDec = min(commitIndexNeg, commitIndexDec);	// update commit index over all bands
			}

			PosBumpsLocal.clear();
			NegBumpsLocal.clear();
		}

		PositiveBumps.setAsAbsTime(false);
		NegativeBumps.setAsAbsTime(false);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// END BUMP COMPETITION LOOP commit index from previous window in relative
		// coordinates
		long lLastCommitIndexDecRel = m_commitIndexDec - m_OutputAbsStart;
		long lLastCommitIndexAccRel = m_commitIndexAcc - m_OutputAbsStart;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// update commit indexes
		m_commitIndexDec = max(m_commitIndexDec - m_OutputAbsStart, commitIndexDec);				// all decel candidates ending before this are stable
		m_commitIndexAcc = max(m_commitIndexAcc - m_OutputAbsStart, commitIndexAcc);				// all accel candidates ending before this are stable

		// Can put lower bounds on commit Indexes based on longest filter delay and
		// longest event length Shift Commit Indexes to absolute time as they are used
		// from window to window
		m_commitIndexDec = max(m_commitIndexDec, (GetNumRealSignal() - MaxDelay));							// "MaxDelay" should be function of filter delay and max decel length
		m_commitIndexDec = m_commitIndexDec + m_OutputAbsStart;
		m_commitIndexAcc = max(m_commitIndexAcc, (GetNumRealSignal() - MaxDelay));							// "MaxDelay" should be function of filter delay and max accel length
		m_commitIndexAcc = m_commitIndexAcc + m_OutputAbsStart;

		// PAW
		//int bp2StableBaselineLimit = GetBp2StableBaselineLimit();
		//m_commitIndexDec = min(m_commitIndexDec, bp2StableBaselineLimit);							
		//m_commitIndexAcc = min(m_commitIndexAcc, bp2StableBaselineLimit);							
		
		if (LongRepairAtEndOfWindow())
		{   // For cases where we have long dropout can force advance of commit index so don't consider long flat bump candidates and can output pending events
			long longRep = m_ConfigFHR.GetMaxRepairForCommitIndexForce();
			m_commitIndexDec = max(m_commitIndexDec, m_nTotalCount - longRep - m_ConfigFHR.GetLongestBandPassDelay());
			m_commitIndexAcc = max(m_commitIndexAcc, m_nTotalCount - longRep - m_ConfigFHR.GetLongestBandPassDelay());
		}

		// Sort bumps by end time
		PositiveBumps.sortByEnd();
		NegativeBumps.sortByEnd();

		// Post bump-competition want to remove bumps that ended before the
		// commitIndexDec / Acc of the previous window These are bumps that were already
		// classified in the last window but were needed to be kept for competition
		// purposes because they may have overlapped a bump that had not been considered
		// until this window
		PositiveBumps.filterEndingBefore(m_accCutoff - m_OutputAbsStart - m_ConfigFHR.GetAccExtBuffer() + 1);  // buffer so don't cut non-extended accels that were extended past cutoff in last window	
		NegativeBumps.filterEndingBefore(lLastCommitIndexDecRel + 1);
		
		// Here we can determine earliest time accel and decel that could be commited in
		// a future window can start, based on commitIndexes and existing candidates.
		// These will be used for commiting baseLines and accels to final o/p by telling
		// us when the merge is stable
		iEarliestNextWindowAccStart = GetEarliestX1InNextWindow(&PositiveBumps, (m_commitIndexAcc - m_OutputAbsStart));
		iEarliestNextWindowDecStart = GetEarliestX1InNextWindow(&NegativeBumps, (m_commitIndexDec - m_OutputAbsStart));



		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// Based on earliest starting points of accel and decel candidates in the ensuing
		// window, we can calculate the cutoffs for accels and baselines
		long lNewBasCutoff = min(iEarliestNextWindowAccStart, iEarliestNextWindowDecStart) + m_OutputAbsStart - 1;
		// -2 is used instead of -1 for accels so that you can wait to see if decel immediately follows accel and use this info for nextDecelX1 fetaure in accel classification
		// if you had some method of lookahead you could put this to -1, thus reducing o/p delay on accels.  Currently if you make this -1 then you will get more FP accels
		// that are really primary rises for decels.
		long lNewAccCutoff = min(m_commitIndexAcc, iEarliestNextWindowDecStart + m_OutputAbsStart) - 2;
		// need to adjust basCutoff for accels that are past the commit index but before
		// the cutof
		long lCurrIndex;
		lCurrIndex = PositiveBumps.size() - 1;
		
		while (lCurrIndex >= 0)
		{
			if (PositiveBumps.getAt(lCurrIndex)->getX2() > lNewAccCutoff - m_OutputAbsStart)
			{
				lNewBasCutoff = min(lNewBasCutoff, PositiveBumps.getAt(lCurrIndex)->getX1() + m_OutputAbsStart);
			}

			lCurrIndex--;
		}
		

		// can eliminate any bump candidates ending after corresponding commitIndex here
		// - no need to send these to be classified
		//PositiveBumps.filterEndingAfter(lNewAccCutoff - m_OutputAbsStart); // use cutoff for accels so don't classify anything that can't be output
		//NegativeBumps.filterEndingAfter(m_commitIndexDec - m_OutputAbsStart);
		PositiveBumps.setAsNonPending();
		PositiveBumps.setAsPendingEndingAfter(lNewAccCutoff - m_OutputAbsStart);

		NegativeBumps.setAsNonPending();
		NegativeBumps.setAsPendingEndingAfter(m_commitIndexDec - m_OutputAbsStart);
		if (!(UsingPendingRT()))
		{
			PositiveBumps.removePending();
			NegativeBumps.removePending();
		}

		// Shift to Absolute time before adding to buffer of all (across multiple
		// windows) bump candidate
		PositiveBumps.toAbsTime(m_OutputAbsStart);
		NegativeBumps.toAbsTime(m_OutputAbsStart);

		PositiveBumps.copyOrigX(); // set origX1, origX2 so that can find in buffer of all candidate after extension
		NegativeBumps.copyOrigX();
		// Add to PositiveBumps and NegativeBumps bumps that finished before current commit index but were extended past the previous commit index in the previous window
		// This is designated by committed flag.  Allows to reclassify with more information
		PositiveBumps.addcopyUncommitted(&m_pPosCandidates);
		PositiveBumps.sortByEnd();
		NegativeBumps.addcopyUncommitted(&m_pNegCandidates);
		NegativeBumps.sortByEnd();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// Add bumps to buffer of non-extended candidates - these will be used in
		// subsequent windows for bump competition
		long lBumpCutoff = m_OutputAbsStart - m_ConfigFHR.GetReqAccelDecelHistory();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		m_pPosCandidates.filterStartingBefore(lBumpCutoff);
		m_pNegCandidates.filterStartingBefore(lBumpCutoff);

		m_pPosCandidates.addcopynew(&PositiveBumps);
		m_pPosCandidates.removePending(); // do not store candidates finishing after commit Index
		m_pNegCandidates.addcopynew(&NegativeBumps);
		m_pNegCandidates.removePending();

		// Shift new bumps from this window back to relative time
		PositiveBumps.toRelativeTime(m_OutputAbsStart);
		NegativeBumps.toRelativeTime(m_OutputAbsStart);

		if (m_ConfigFHR.RemoveRepairCandidates())
		{	// for now only want to do this for NN trainings, otherwise remove repair events after merging
			MarkRepairBumps(&PositiveBumps);
			PositiveBumps.removeNonInterp();
		//	MarkRepairBumps(&NegativeBumps);
		//	NegativeBumps.removeNonInterp();
		}

		if (m_ConfigFHR.m_iStoreMAT > 0)
		{	// construct array of non-extended bump candidates accesible from client (for debug output)
			m_pCurrCandBumps.clear();
			m_pCurrCandBumps.addcopy(&PositiveBumps);
			m_pCurrCandBumps.addcopy(&NegativeBumps);
			//m_pCurrCandBumps.removePending();
			m_pCurrCandBumps.sortByEnd();
			m_pCurrCandBumps.setTimestamp(m_nTotalCount);
		}

		delete[] lZC;
		lZC = NULL;
		delete[] dLastZCPos;
		dLastZCPos = NULL;
		delete[] dLastZCNeg;
		dLastZCNeg = NULL;

		LowPass25();	// generate low pass 25 sec to use for accel slope features
		fhrPartSet NegativeBumpsCopy, PositiveBumpsCopy;
		NegativeBumpsCopy.addcopy(&NegativeBumps);
		PositiveBumpsCopy.addcopy(&PositiveBumps);
		// START OF MULTI-ITERATION CLASSIFICATION
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// Extend new bumps Cannot extend accel into last committed accel or baseline Do
		// not consider lastCommittedDecelX2 becuase any overlap will be taken care of in
		// merge, and it is possible for an accel that occurs before a decel to be
		// committed in a later window than when the decel was committed
		//
		// Change by Philip Warrick
		//
		// Old line: long lExtendBackLim = max(m_lLastCommittedAccelX2, m_lLastCommittedBasX2) - m_OutputAbsStart;

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		for (long iteration = 0; iteration < GetNumAccelDecelIterations(); iteration++)
		{
			// PAW: Allows detection of accels preceding committed decels
            long lExtendBackLim = max(m_lLastCommittedAccelX2, m_lLastCommittedBasX2) - m_OutputAbsStart;

			m_lCurrIteration = iteration;  // just so client can know if want to get intermed output
			if (iteration > 0)
			{
				// reset candidates to non-extended version so can redo extension
				PositiveBumps.clear();
				PositiveBumps.addcopy(&PositiveBumpsCopy);
				NegativeBumps.clear();
				NegativeBumps.addcopy(&NegativeBumpsCopy);
			}
			

			if (m_ConfigFHR.m_bExtendAccelCandidates)
			{
				ExtendBumps(&PositiveBumps, lExtendBackLim);		
				if (iteration == GetNumAccelDecelIterations() - 1)
				{ // so can reclassify candidates extended past current commit index in the following window we update the buffer of all candidates 
					PositiveBumps.setAsPendingEndingAfter(lNewAccCutoff - m_OutputAbsStart);
					UpdateNonExtendCandBuffer(&PositiveBumps, &m_pPosCandidates);
				}
		//		PositiveBumps.filterEndingBefore(m_accCutoff - m_OutputAbsStart + 1); // after extension can remove any ending before cutoff - would have been output in last window
			}
			// Cannot extend decel into any already committed event
			lExtendBackLim = max(lExtendBackLim, m_lLastCommittedDecelX2 - m_OutputAbsStart);
			ExtendBumps(&NegativeBumps, lExtendBackLim);
			
			if (iteration == GetNumAccelDecelIterations() - 1)
			{   // so can reclassify candidates extended past current commit index in the following window we update the buffer of all candidates 
				NegativeBumps.setAsPendingEndingAfter(m_commitIndexDec - m_OutputAbsStart);
				UpdateNonExtendCandBuffer(&NegativeBumps, &m_pNegCandidates);
			}

			// First classify decels
			PreprocessDecels(&NegativeBumps);
			ClassifyDecels(&NegativeBumps);
			PostProcessDecels(&NegativeBumps); // remove abrupt less than 15 bpm

			// Modify accel candidate bounds based on both newly classified decels and any
			// prior decels form previous windows
			ModifyAccelCandidateBounds(&PositiveBumps, &m_pAllPreMergeDecels, m_OutputAbsStart);
			ModifyAccelCandidateBounds(&PositiveBumps, &NegativeBumps, 0);
			PreprocessAccels(&PositiveBumps);

			ClassifyAccels(&PositiveBumps, &NegativeBumps);

			if (m_ConfigFHR.m_iStoreMAT > 0)
			{	// construct array of extended bump candidates accesible from client (for debug output)
				m_pCurrCandBumpsExtend.clear();
				m_pCurrCandBumpsExtend.addcopy(&PositiveBumps);
				m_pCurrCandBumpsExtend.addcopy(&NegativeBumps);
				if (m_ConfigFHR.m_bNNtrain)
					m_pCurrCandBumpsExtend.removePending();
				m_pCurrCandBumpsExtend.sortByEnd();
				m_pCurrCandBumpsExtend.setTimestamp(m_nTotalCount);
			}

			if ((m_ConfigFHR.m_iStoreMAT > 0) && (iteration < GetNumAccelDecelIterations()))
			{ // CallBack here so can dump data for all iterations
				// CallBack if not last iteration - last iteration will get captured as actual bumpCandExtend and multiH
				if (m_pCallBackFunction)
					(*m_pCallBackFunction)(11, m_pData); // can get per-iteration classifications and baselines
			}

			// Remove bump candidates classified as non-accels and non-decels
			RemoveNonEvents(&PositiveBumps);
			RemoveNonEvents(&NegativeBumps);

			RegenBaseLinesFinalFromMerge(NegativeBumps, PositiveBumps);
			if ((m_ConfigFHR.m_iStoreMAT > 0) && (iteration < GetNumAccelDecelIterations() - 1))
			{ // special callback so can dump non-final iteration classifications for testing / debugging
				if (m_pCallBackFunction)
					(*m_pCallBackFunction)(12, m_pData); 
			}
		}			

		PositiveBumpsCopy.clear();
		NegativeBumpsCopy.clear();

		// Shift to Absolute time before adding to overall array of accepted bumps
		PositiveBumps.toAbsTime(m_OutputAbsStart);
		NegativeBumps.toAbsTime(m_OutputAbsStart);

		// Add pre-merge events to total buffers Remove any old bumps from total buffer
		// that start before the required history
		m_pAllPreMergeAccels.filterStartingBefore(lBumpCutoff);
		m_pAllPreMergeDecels.filterStartingBefore(lBumpCutoff);
		m_pAllPreMergeAccels.removePending(); // remove pending used in prior window
		m_pAllPreMergeDecels.removePending();
		PositiveBumps.setAsNonPending();
		PositiveBumps.setAsPendingEndingAfter(lNewAccCutoff); // reset pending in case accel truncated during classification over boundary of cutoff
		PositiveBumps.setClearMemory(false);
		NegativeBumps.setClearMemory(false);
		m_pAllPreMergeAccels.add(&PositiveBumps);
		m_pAllPreMergeDecels.add(&NegativeBumps);
		// In most cases will already be in order, but due to bump extension is possible
		// that bumps are unordered if sorted based on end time. Important that they are
		// sorted when merging overlapping decels (overlap due to extension)
		m_pAllPreMergeAccels.sortByEnd();
		m_pAllPreMergeDecels.sortByEnd();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// This also removes baselines ending prior to the cutoff from the last window
		long lCutBefore = m_basCutoff - m_OutputAbsStart;	// cut any baselines that are already commited
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		/*~~~~~~~~~~~~~~~~~~~~~~~*/
		// Keep things in relative coordinates until the end First merge accels with
		// baselines ;
		// Need to use all accels / decels because a bump that has already been commited
		// may affect a baseline that has not already been commited, during the merge
		// process.
		/*~~~~~~~~~~~~~~~~~~~~~~~*/
		
		m_OutputFhrPartSet.addcopy(&m_pBaseLinesTotal);
		m_OutputFhrPartSet.addcopy(&m_pBaseLinesPending);	// these will only make it to final o/p if get truncated during merge and truncated version finishes before basCutoff
		m_OutputFhrPartSet.toRelativeTime(m_OutputAbsStart);
		m_OutputFhrPartSet.filterEndingBefore(lCutBefore + 1);

		// first we merge w/ already output parts
		long mergeMinX2 = m_nTotalCount;
		if (m_OutputFhrPartSet.size() > 0)
		{
			mergeMinX2 = m_OutputFhrPartSet.getAt(0)->getX1() + m_OutputAbsStart; // start of first uncommitted decel
		}
		long earliestUncommitted = m_pAllPreMergeAccels.getEarliestUncommitted();
		if (earliestUncommitted >= 0)
			mergeMinX2 = min(mergeMinX2, m_pAllPreMergeAccels.getAt(earliestUncommitted)->getX1());
		earliestUncommitted = m_pAllPreMergeDecels.getEarliestUncommitted();
		if (earliestUncommitted >= 0)
			mergeMinX2 = min(mergeMinX2, m_pAllPreMergeDecels.getAt(earliestUncommitted)->getX1());

		long lb = m_TotalOutputFhrPartSet.size() - 1;
		while ((lb >= 0) && (m_TotalOutputFhrPartSet.getAt(lb)->getX2() >= mergeMinX2))
		{
			if ((!(m_TotalOutputFhrPartSet.getAt(lb)->isPending())) && (!(m_TotalOutputFhrPartSet.getAt(lb)->isBaseline())))
			{
				fhrPart *p = new fhrPart;
				(*p) = *(m_TotalOutputFhrPartSet.getAt(lb));
				p->toRelativeTime(m_OutputAbsStart);
				fhrPartSetMerge(&m_OutputFhrPartSet, p);
			}
			lb--;
		}

		// now merge non-committed accels to output
		accel *a;
		for (lb = 0; lb < m_pAllPreMergeAccels.size(); lb++)
		{
			a = (accel*) m_pAllPreMergeAccels.getAt(lb);
			if ((!(a->isCommitted())) && (a->getX2() > m_lLastCommittedAccelX2))
			{ 
				accel *acopy = new accel;
				(*acopy) = (*a);
				acopy->toRelativeTime(m_OutputAbsStart);		
				fhrPartSetMerge(&m_OutputFhrPartSet, acopy); 
			}
		}
		
		// non-committed decels
		decel *d;
		for (lb = 0; lb < m_pAllPreMergeDecels.size(); lb++)
		{
			d = (decel*) m_pAllPreMergeDecels.getAt(lb);
			if ((!(d->isCommitted())) && (d->getX2() > m_lLastCommittedDecelX2))
			{ 
				decel *dcopy = new decel;
				*(dcopy) = *(d);
				dcopy->toRelativeTime(m_OutputAbsStart);
				fhrPartSetMerge(&m_OutputFhrPartSet, dcopy);
			}
		}

		// ELIMINATE FIRST Output Object Remove any events that have already been
		// committed 
		EliminateFirstOutputObjects();

		// After merging - now we need to eliminate non-commitable accels and baselines
		// Baseline cutoff - cannot commit past earliest possible start point of future
		// accel or decel (from next window)
		m_basCutoff = max(m_basCutoff, lNewBasCutoff);	// explicitly do not allow cutoff to go backwards in time - can happen in rare circumstances

		// ensure that no new baseline (from ensuing window) could end before basCutoff
		// this also ensures that no pending baselines will be commited
		m_basCutoff = min(m_basCutoff, m_NextBaselineDetectionStart);

		// Accel cutoff - cannot commit past earliest possible start point of future
		// decel (from next window)
		m_accCutoff = max(m_accCutoff, lNewAccCutoff);	// explicitly do not allow cutoff to go backwards in time - can happen in rare circumstances

		// Decel cutoff is same as commit index because cannot be overwritten in merge
		m_decCutoff = m_commitIndexDec;

		if (m_ConfigFHR.m_iStoreMAT > 1)
		{
		//	OutputXML(6);			// output commit indexes
		}

		SetOutputAfterCutoffAsPending();
		if (!(m_bKeepPending))
		{
			m_OutputFhrPartSet.removePending();
		}
		else
		{
			m_OutputFhrPartSet.filterEndingAfter(GetNumRealSignal()); // even if output pending don't output based on extrapolated signal
		}
		
		MarkRepairBumps(&m_OutputFhrPartSet);

		if (IsBasVarCalcActivated())
		{
			CalcOutputBaselineVar(&m_OutputFhrPartSet);
		}

		// Atypical Classification
		AtypicalDecelClassify(&m_OutputFhrPartSet);

		m_OutputFhrPartSet.nonBasToBas(); // mark output baselines mhClass as true so test client considers them as baselines

		m_OutputFhrPartSet.setAsAbsTime(false);
		m_OutputFhrPartSet.toAbsTime(m_OutputAbsStart);
		LockTotalResultObjects();
		if (m_bKeepPending)
		{
			m_TotalOutputFhrPartSet.removePending(); // remove pending from previous window
		}
		m_TotalOutputFhrPartSet.addcopy(&m_OutputFhrPartSet);

        // PAW: previously it would create a situation where baselines times could overlap Accels/Decels times
        m_TotalOutputFhrPartSet.sortByEnd();

		m_bHaveNewEvents = true;
		LockTotalResultObjects(false);

		m_ConfigFHR.m_PassedState = CConfigFHR::AccelDecel;

		return bR;
	}

	/*
	=======================================================================================================================
		fhrPartSetMerge: merge part *p to existing set *f.  If no overlap then simply insert in correct place, otherwise
			need to truncate *p or parts in *f based on precedence (decel over accel over baseline).  Reclassify any part
			that was truncated in merge process to ensure still valid.
	=======================================================================================================================
	*/	
	void CFHRSignal::fhrPartSetMerge(fhrPartSet* f, fhrPart* p)
	{
		long i = 0;
		long n = f->size();
		
		while ((i < n) && (f->getAt(i)->getX2() < p->getX1()))
		{
			i++;
		}
		if (n == 0)
		{
			f->insert(p, 0);
		}
		else if (i == n)
		{
			f->add(p);
		}
		else
		{
			fhrPart *x = f->getAt(i);	
			bool insertp = true;
			bool inLoop = p->intersects(*x);
			long pindex = i;
			while (inLoop) // loop in case intersect multiple parts
			{	
				inLoop = false;
				bool prec = p->precedence(*x);
				// check for special case where x already committed and p not
				if (x->isCommitted() && !(p->isCommitted()))
				{
					prec = false;
				}
				else if (p->isCommitted() && !(x->isCommitted()))
				{
					prec = true;
				}
				if (p->inside(*x))
				{
					if (prec) // p cuts x in two
					{
						f->insertcopy(x, pindex+1);
						fhrPart *x2 = f->getAt(pindex+1);
						x2->setX1(p->getX2()+1);
						if (IsDegenerateFhrPart(x2))
						{
							f->removeAt(pindex+1);
						}
						x->setX2(p->getX1()-1);
						if (IsDegenerateFhrPart(x))
						{
							f->removeAt(pindex);		
						}
						else
						{
							pindex++;
						}
					}
					else
					{
						insertp = false;
					}
				}
				else if (x->inside(*p))
				{
					if (prec)
					{
						f->removeAt(pindex);
						inLoop = true; // have to continue and check next
					}
					else
					{  // p gets cut in two
						//fhrPart *p2 = new fhrPart; // do we need to do a cast here? 
						//(*p2) = (*p);
						void *p2; 
						p2 = p->copy();
						
						((fhrPart*) p2)->setX2(x->getX1() - 1);
						if (!(IsDegenerateFhrPart((fhrPart*) p2)))
						{
							f->insert((fhrPart*) p2, pindex);
							pindex++;
						}
						else
						{
							delete p2;
						}
						p->setX1(x->getX2() + 1);
						if (!(IsDegenerateFhrPart(p)))
						{
							inLoop = true;
							pindex++;
						}
						else
						{
							insertp = false;
						}
					}
				}
				else if (p->startsBefore(*x))
				{
					if (prec)
					{
						x->setX1(p->getX2() + 1);
						if (IsDegenerateFhrPart(x))
						{
							f->removeAt(pindex);
						}
					}
					else
					{
						p->setX2(x->getX1() - 1);
						if (IsDegenerateFhrPart(p))
						{
							insertp = false;
						}
					}
				}
				else // p starts after x and neither is wholly inside the other
				{
					inLoop = true;
					if (prec)
					{
						x->setX2(p->getX1() - 1);
						if (IsDegenerateFhrPart(x))
						{
							f->removeAt(pindex);
						}
						else
						{
							pindex++;
						}
					}
					else
					{
						p->setX1(x->getX2() + 1);
						if (!(IsDegenerateFhrPart(p)))
						{
							pindex++;
						}
						else
						{
							insertp = false;
							inLoop = false;
						}
					}
				}
				
				if (inLoop)
				{
					if (pindex < f->size())
					{
						x = f->getAt(pindex);
						inLoop = p->intersects(*x);
					}
					else
					{
						inLoop = false;
					}
				}
			}
			if (insertp)
			{
				f->insert(p, pindex);
			}
			else
			{
				delete p;
			}
		}
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::ExtendBumps(fhrPartSet *Bumps, long prevBumpX2)
	{
		if (Bumps->size() > 0)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			CBumpExtendSet *pBumpExtendSet = new CBumpExtendSet(GetSmpFreq());
			fhrPartSet repInts;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			
			if (m_lCurrIteration > 0)
			{  // reduce min ref baseline length as more confident in baselines after first iteration
				pBumpExtendSet->SetMinRefBasLenSec(45);
			}
			pBumpExtendSet->SetRefBas(&m_pBaseLinesFinal);	// set ref bas and filter out ones that are too short
			pBumpExtendSet->SetRepairIntervals(&m_pRepairIntervals, m_OutputAbsStart);
			pBumpExtendSet->SetRefSignal(m_pdLowPassSignal, m_nLowPassCount);
			
			// pBumpExtendSet->SetSmpFreq(GetSmpFreq());
			pBumpExtendSet->SetFirstPrevX2(prevBumpX2);
			pBumpExtendSet->SetBumps(Bumps);
			pBumpExtendSet->Extend();

			delete pBumpExtendSet;
			repInts.clear();
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::ClassifyDecels(fhrPartSet *d)
	{
		if (d->size() == 0)
		{
			return;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		CStdioFile fileFeature_Statistic;	
		bool toFile = ((m_ConfigFHR.m_iStoreXML > 1) && (d->getNumPending() < d->size()) && (m_lCurrIteration == GetNumAccelDecelIterations() - 1));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (toFile)
		{
			try
			{
				/*~~~~~~~~~~*/
				char buff[10];
				/*~~~~~~~~~~*/

				_ltoa(m_nTotalCount, buff, 10);
				wchar_t wBuff[1000];
				mbstowcs(wBuff, buff, strlen(buff) + 1);//Plus null
				LPWSTR ptrBuff = wBuff;

				fileFeature_Statistic.Open(CString(m_ConfigFHR.m_strXMLFilePrefix.c_str()) + CString("BumpMeasures_Decel_") + ptrBuff + CString(".txt"), CFile::modeCreate | CFile::modeWrite | CFile::typeText);
			}
			catch(...)
			{
			}
		}

		for (long lb = 0; lb < d->size(); lb++)
		{
			if (toFile)
			{
				ClassifyDecel((decel*) d->getAt(lb), (void *) &fileFeature_Statistic);
			}
			else
			{
				ClassifyDecel((decel*) d->getAt(lb), NULL);
			}
		}
		

		if (toFile)
		{
			try
			{
				fileFeature_Statistic.Close();
			}
			catch(...)
			{
			}

		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::ClassifyDecel(decel *d, void *pFile)
	{
		/*~~~~~~~~~~~~*/
		PatternsVector inVector;
		/*~~~~~~~~~~~~*/

		inVector.setBounds(0, 19);
		MeasureOneDecel(d);
		DecelToVector(d, &inVector);

		if ((pFile) && (!(d->isPending())))
		{
			CString sFeature_Statistic;
			CString sFeature_Stat;
			sFeature_Statistic = "";
			for (int iField = 0; iField < 20; iField++)
			{
				if (inVector(iField) == DBL_MAX)
				{
					sFeature_Stat.Format(_T("NaN\t"));
				}
				else
				{
					sFeature_Stat.Format(_T("%f\t", inVector(iField)));
				}

				sFeature_Statistic += sFeature_Stat;
			}

			try
			{
				((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
			}
			catch(...)
			{
			}
		}

		NormalizeOneDecel(&inVector);
		SimulateOneDecel(&inVector, d, (void *) pFile);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::DecelToVector(decel *d, PatternsVector* v)
	{
		bumpMeasuresDecel *bm = d->getBM();
		(*v) (0) = bm->getLength();
		(*v) (1) = bm->getOnset();
		(*v) (2) = bm->getRecovery();
		(*v) (3) = bm->getFhrBegin();
		(*v) (4) = bm->getFhrEnd();
		(*v) (5) = bm->getFhrStd();
		(*v) (6) = -1.0 * (bm->getFhrInSlopeVal()); // just so consistent with old way for now
		(*v) (7) = bm->getFhrInSlopeTime();
		(*v) (8) = bm->getFhrOutSlopeVal();
		(*v) (9) = bm->getFhrOutSlopeTime();
		(*v) (10) = bm->getFhrPrev();
		(*v) (11) = bm->getFhrNext();
		(*v) (12) = bm->getMaxHeight();
		(*v) (13) = bm->getMeanHeight();
		(*v) (14) = bm->getArea();
		(*v) (15) = bm->getVarMax();
		(*v) (16) = bm->getVarPrev();
		(*v) (17) = bm->getVarNext();
		(*v) (18) = bm->getContrBegin();
		(*v) (19) = bm->getContrEnd();
	}



	/*
	=======================================================================================================================
	ClassifyAccels: seperate classification for accels
	=======================================================================================================================
	*/
	void CFHRSignal::ClassifyAccels(fhrPartSet *accels, fhrPartSet *decels)
	{
		/*~~~~~~~~~~~~*/
		long lTrueB = 0;
		bool toFile = (m_ConfigFHR.m_iStoreXML > 1) && (accels->getNumPending() < accels->size() && (m_lCurrIteration == GetNumAccelDecelIterations() - 1));
		/*~~~~~~~~~~~~*/

		if (accels->size() == 0)
		{
			return;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		CStdioFile fileFeature_Statistic;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		try
		{
			if (toFile)
			{
				/*~~~~~~~~~~*/
				char buff[10];
				/*~~~~~~~~~~*/

				_ltoa(m_nTotalCount, buff, 10);
				wchar_t wBuff[1000];
				mbstowcs(wBuff, buff, strlen(buff) + 1);//Plus null
				LPWSTR ptrBuff = wBuff;

				fileFeature_Statistic.Open(CString(m_ConfigFHR.m_strXMLFilePrefix.c_str()) + CString("BumpMeasures_Accel_") + ptrBuff + CString(".txt"), CFile::modeCreate | CFile::modeWrite | CFile::typeText);
			}
		}

		catch(...)
		{
			toFile = false;
		}

		for (long lb = 0; lb < accels->size(); lb++)
		{
			if (toFile)
			{
				ClassifyAccel((accel*) accels->getAt(lb), decels, (void *) &fileFeature_Statistic);
			}
			else
			{
				ClassifyAccel((accel*) accels->getAt(lb), decels, NULL);
			}
		}

		try
		{
			if (toFile)
			{
				fileFeature_Statistic.Close();
			}
		}

		catch(...)
		{
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::ClassifyAccel(accel *a, fhrPartSet *decels, void *pFile)
	{

			/*~~~~~~~~~~~~*/
			PatternsVector inVector;
			CString sFeature_Statistic;
			CString sFeature_Stat;
			/*~~~~~~~~~~~~*/

			inVector.setBounds(0, m_ConfigFHR.GetNumberOfAccelStats() - 1);
			MeasureOneAccel(a, decels);
			AccelToVector(a, &inVector);

			if ((pFile) && (!(a->isPending())))
			{
				sFeature_Statistic = "";
				for (int iField = 0; iField < m_ConfigFHR.GetNumberOfAccelStats(); iField++)
				{
					if (inVector(iField) == DBL_MAX)
					{
						sFeature_Stat.Format(_T("NaN\t"));
					}
					else
					{
						sFeature_Stat.Format(_T("%f\t", inVector(iField)));
					}

					sFeature_Statistic += sFeature_Stat;
				}

				try
				{
					if (m_ConfigFHR.m_iStoreXML > 1)
					{
						((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
					}
				}
				catch(...)
				{
				}
			}

			NormalizeOneAccel(&inVector);
			SimulateOneAccel(&inVector, a, (void *) pFile);

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::AccelToVector(accel *a, PatternsVector* v)
	{
		bumpMeasuresAccel *bm = a->getBM();
		(*v) (0) = bm->getLength();
		(*v) (1) = bm->getFhrBegin();
		(*v) (2) = bm->getFhrEnd();
		(*v) (3) = bm->getFhrStd();
		(*v) (4) = bm->getFhrPrev();
		(*v) (5) = bm->getFhrNext();
		(*v) (6) = bm->getMaxHeight();
		(*v) (7) = bm->getMeanHeight();
		(*v) (8) = bm->getArea();
		(*v) (9) = bm->getVarMax() * 10.0; // legacy scaling
		(*v) (10) = bm->getVarPrev();
		(*v) (11) = bm->getVarNext();
		(*v) (12) = bm->getSlopeTimeIn25();
		(*v) (13) = bm->getSlopeTimeOut25();
		(*v) (14) = bm->getOnset25();
		(*v) (15) = bm->getRecovery25();
		(*v) (16) = bm->getOnsetSlope25();
		(*v) (17) = bm->getRecoverySlope25();
		(*v) (18) = bm->getLastDecelX2();
		(*v) (19) = bm->getNextDecelX1();
		(*v) (20) = bm->getDecelRate();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::RejectRepairBump(bump *b)
	{
		ASSERT((b->getX1() >= 0) && (b->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double percRepair = 0.0;
		double percInSlopeRepSamps = 0.0;
		double percOutSlopeRepSamps = 0.0;
		long lOnset = 0;
		long lOnsetIndex = 0;
		long lRecov = 0;
		long lRecovIndex = 0;
		double dSlopeShift = 0.0;
		double dShiftRep = 0.0;
		double dPercShiftRep = 0.0;
		bool bInSlope = false;
		bool bOutSlope = false;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		percRepair = b->getPercRepair();

		// Check if perc repair is big enough to not even bother checking in and out
		// segments
		if (percRepair > m_ConfigFHR.MaxBumpRep2Slopes())
		{
			return true;
		}
		else if (percRepair == 0.0)
		{
			return false;	// if no repair don't need to bother checking rest
		}

		// Now check the onset and recovery portions Need at least 50% of samples
		// non-repaired AND 50% of shift coming from good signal to consider the onset or
		// recovery as clean. Also need at least 70% of one of the perc of samples or
		// percentage of shift
		GetOnsetAndRecovFromLP(b, &lOnset, &lRecov);

		// onset
		lOnsetIndex = b->getX1() + lOnset;
		percInSlopeRepSamps = CalcPercRepair(b->getX1(), lOnsetIndex);
		if (percInSlopeRepSamps == 0.0)
		{
			bInSlope = true;
		}
		else if (percInSlopeRepSamps <= m_ConfigFHR.MaxPercSlopeRep())
		{
			dSlopeShift = abs(m_pdSignal[lOnsetIndex] - m_pdSignal[b->getX1()]);	// shift in FHR level
			dShiftRep = abs(GetRepairLevelShift(m_pdSignal, b->getX1(), lOnsetIndex));
			dPercShiftRep = dShiftRep / dSlopeShift;
			if (dPercShiftRep <= m_ConfigFHR.MaxPercSlopeRep())
			{
				bInSlope = ((dPercShiftRep < m_ConfigFHR.MaxPercSlopeRep2()) || (percInSlopeRepSamps < m_ConfigFHR.MaxPercSlopeRep2()));	// at least one over 70% clean
			}
		}

		// recovery
		lRecovIndex = b->getX2() - lRecov;
		percOutSlopeRepSamps = CalcPercRepair(lRecovIndex, b->getX2());
		if (percOutSlopeRepSamps == 0.0)
		{
			bOutSlope = true;
		}
		else if (percOutSlopeRepSamps <= m_ConfigFHR.MaxPercSlopeRep())
		{
			dSlopeShift = abs(m_pdSignal[lRecovIndex] - m_pdSignal[b->getX2()]); // shift in FHR level
			dShiftRep = abs(GetRepairLevelShift(m_pdSignal, lRecovIndex, b->getX2()));
			dPercShiftRep = dShiftRep / dSlopeShift;
			if (dPercShiftRep <= m_ConfigFHR.MaxPercSlopeRep())
			{
				bOutSlope = ((dPercShiftRep < m_ConfigFHR.MaxPercSlopeRep2()) || (percOutSlopeRepSamps < m_ConfigFHR.MaxPercSlopeRep2()));	// at least one over 70% clean
			}
		}

		if (bOutSlope && bInSlope)
		{
			return false;	// enough clean signal - do not reject
		}
		else if ((!(bOutSlope)) && (!(bInSlope)))
		{
			return true;	// neither onset or recovery is clean - reject
		}
		else
		{				// have one of recovery or onset
			return percRepair > m_ConfigFHR.MaxBumpRepOneSlope();
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::SetRemoveRepairOutput(bool b)
	{
		m_ConfigFHR.SetRemoveRepairOutput(b);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CFHRSignal::MarkRepairBumps(fhrPartSet *fset)
	{
		/*~~~~~~~~~~~~~~~*/
		long numRemove = 0;
		/*~~~~~~~~~~~~~~~*/

		for (long i = fset->size() - 1; i >= 0; i--)
		{
			fhrPart *f = fset->getAt(i);
			f->setNonInterp(false);
			if (f->isBump())
			{			// is accel or decel
				f->setPercRepair(CalcPercRepair(f->getX1(), f->getX2()));
				if (m_ConfigFHR.RemoveRepairOutput())
				{
					if (f->getPercRepair() > m_ConfigFHR.GetMaxPercRepairForOutput())
					{
						fset->removeAt(i);
						numRemove++;
					}
					else if (RejectRepairBump((bump*) f))
					{
						f->setNonInterp(true);
					}
				}
			}
			else
			{
				f->setPercRepair(0.0);
			}
		}

		return numRemove;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CFHRSignal::CalcPercRepair(long lX1, long lX2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lRep = CalcRepairSamples(lX1, lX2);
		double percRep = (double) (lRep / (lX2 - lX1 + 1.0));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		
		return percRep;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CFHRSignal::CalcRepairSamples(long lX1, long lX2)
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		long lRep = 0;
		fhrPart currRep;
		long lIndex = 0;
		/*~~~~~~~~~~~~~~~~~~~~*/

		if (m_pRepairIntervals.size() == 0)
		{
			return lRep;
		}

		currRep = *(m_pRepairIntervals.getAt(lIndex));
		currRep.toRelativeTime(m_OutputAbsStart);	// repair Intervals are store in absolute coords
		while ((lIndex < m_pRepairIntervals.size()) && (currRep.getX1() <= lX2))
		{
			if (currRep.getX2() >= lX1)		// have overlap
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~*/
				long lC1 = currRep.getX1();
				long lC2 = currRep.getX2();
				/*~~~~~~~~~~~~~~~~~~~~~~~*/

				if ((lC1 <= lX1) && (lC2 >= lX2))
				{	// entire interval is in repair segment
					return lX2 - lX1 + 1;
				}
				else if ((lC1 > lX1) && (lC2 < lX2))
				{	// repair segment wholly inside interval
					lRep += currRep.length();
				}
				else if (lC1 <= lX1)
				{	// repair start before and ends in middle of interval
					lRep += lC2 - lX1 + 1;
				}
				else if (lC2 >= lX2)
				{	// repair starts in middle and ends after interval
					lRep += lX2 - lC1 + 1;
				}
			}

			lIndex++;
			if (lIndex < m_pRepairIntervals.size())
			{
				currRep = *(m_pRepairIntervals.getAt(lIndex));
				currRep.toRelativeTime(m_OutputAbsStart);
			}
		}

		return lRep;
	}

	//
	// =======================================================================================================================
	//    assumes lX2 is less than the number of samples in pSamples
	// =======================================================================================================================
	//
	double CFHRSignal::GetRepairLevelShift(double *pSamples, long lX1, long lX2)
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		double dRep = 0.0;
		fhrPart currRep;
		long lIndex = 0;
		/*~~~~~~~~~~~~~~~~~~~~*/

		if (m_pRepairIntervals.size() == 0)
		{
			return dRep;
		}

		currRep = *(m_pRepairIntervals.getAt(lIndex));
		currRep.toRelativeTime(m_OutputAbsStart);	// repair Intervals are stored in absolute coords
		while ((lIndex < m_pRepairIntervals.size()) && (currRep.getX1() <= lX2))
		{
			if (currRep.getX2() >= lX1)		// have overlap
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				long lC1 = max(0, currRep.getX1() - 1);
				long lC2 = min(m_nPtsCount, currRep.getX2() + 1);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if ((lC1 <= lX1) && (lC2 >= lX2))
				{	// entire interval is in repair segment
					return pSamples[lX2] - pSamples[lX1];
				}
				else if ((lC1 > lX1) && (lC2 < lX2))
				{	// repair segment wholly inside interval
					dRep += pSamples[lC2] - pSamples[lC1];
				}
				else if (lC1 <= lX1)
				{	// repair start before and ends in middle of interval
					dRep += pSamples[lC2] - pSamples[lX1];
				}
				else if (lC2 >= lX2)
				{	// repair starts in middle and ends after interval
					dRep += pSamples[lX2] - pSamples[lC1];
				}
			}

			lIndex++;
			if (lIndex < m_pRepairIntervals.size())
			{
				currRep = *(m_pRepairIntervals.getAt(lIndex));
				currRep.toRelativeTime(m_OutputAbsStart);	// repair Intervals are stored in absolute coords
			}
		}

		return dRep;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::EliminateFirstOutputObjects(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long iHistory;		// point after which event must end (or else it would have already been committed in a prior window)
		long lCommitCutoff; // this is an extra condition required due to bump extension
		long lMinStart = m_ConfigFHR.GetEarliestEventBegin(); // earliest absolute time an event can start (reject events at very beginning of tracing)
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~

		// if a bump is extended past the CommitIndex in a prior window, it could end
		// before the relevant cutoff in an ensuing window - check explicitly based on
		// last commited event (this is not an issue with baselines because they are not
		// extended)
		for (int i = m_OutputFhrPartSet.size() - 1; i >= 0; i--)
		{
			fhrPart *p = m_OutputFhrPartSet.getAt(i);
			if (p->isBump())
			{
				if (p->isAccel())
				{
					lCommitCutoff = m_lLastCommittedAccelX2;
				}
				else
				{
					lCommitCutoff = m_lLastCommittedDecelX2;
				}
				if (p->getX1() + m_OutputAbsStart <= lMinStart)
					m_OutputFhrPartSet.removeAt(i);
				else if (p->isCommitted())
					m_OutputFhrPartSet.removeAt(i);
				else if (p->getX2() + m_OutputAbsStart <= lCommitCutoff) // this condition required in certain cases where extension leads to two overlapping bumps of same type
					m_OutputFhrPartSet.removeAt(i);
			}
			else 
			{
				iHistory = m_basCutoff;
				lCommitCutoff = m_lLastCommittedBasX2;

				if (p->getX2() + m_OutputAbsStart <= iHistory)	// cutoffs in absolute time
				{
					if (p->getX1() + m_OutputAbsStart <= lCommitCutoff)
					{	// extra check required due to bump extension
						m_OutputFhrPartSet.removeAt(i);
					}
				}
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::SetOutputAfterCutoffAsPending(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long iCutoff, n;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = m_OutputFhrPartSet.size() - 1; i >= 0; i--)
		{
			fhrPart *p = m_OutputFhrPartSet.getAt(i);
			if (p->isBaseline())
			{
				iCutoff = m_basCutoff;
			}
			else if (p->isAccel())
			{
				iCutoff = m_accCutoff;
			}
			else
			{
				iCutoff = m_decCutoff;
			}

			// if (m_pOutputObjects[i].lStart < iHistory || m_pOutputObjects[i].lEnd <
			// iHistory)
			if (p->getX2() + m_OutputAbsStart > iCutoff)
			{
				p->setPending(true);
			}
			else	// ensure m_lLastCommitedX2 does not go backward because a bas can have x2 < dec commited in prev window
			{
				p->setPending(false);
				p->setCommitted(true);
				p->setTimestamp(m_nTotalCount);
				if (p->isBaseline())
				{
					m_lLastCommittedBasX2 = max(m_lLastCommittedBasX2, p->getX2() + m_OutputAbsStart);
				}
				else if (p->isAccel())
				{
					m_lLastCommittedAccelX2 = max(m_lLastCommittedAccelX2, p->getX2() + m_OutputAbsStart);
					n = m_pAllPreMergeAccels.getIndexFromX1X2(p->getX1() + m_OutputAbsStart, p->getX2() + m_OutputAbsStart);
					if (n >= 0)
					{
						m_pAllPreMergeAccels.getAt(n)->setCommitted(true);
					}
				}
				else
				{
					m_lLastCommittedDecelX2 = max(m_lLastCommittedDecelX2, p->getX2() + m_OutputAbsStart);
					n = m_pAllPreMergeDecels.getIndexFromX1X2(p->getX1() + m_OutputAbsStart, p->getX2() + m_OutputAbsStart);
					if (n >= 0)
					{
						m_pAllPreMergeDecels.getAt(n)->setCommitted(true);
					}
				}
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::MeasureOneDecel(decel *d)
	{
		ASSERT((d->getX1() >= 0) && (d->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		// DBL_MAX cases convert params to zero in NormalizeOneBump
		bumpMeasuresDecel *bm = d->getBM();
		// Set ContractionBegin and ContractionEnd to outlying non-associated values (here
		// set so contraction appears to start 1 min after bump). This is so do not use a
		// mean value for non-associated bumps - use an outlying value
		bm->setContrBegin((long) (60 * GetSmpFreq()));
		bm->setContrEnd((long) (60 * GetSmpFreq()));

		bm->setLength(d->length());
		bm->setFhrBegin(m_pdSignal[d->getX1()]);
		bm->setFhrEnd(m_pdSignal[d->getX2()]);

		GetDecelHeightBM(d);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dBase = (m_pdSignal[d->getX1()] + m_pdSignal[d->getX2()]) / 2.0;
		double varMax = fabs(m_pdVarPassSignal[d->getX1()]);
		double stdev = 0.0;
		double meanVal;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = d->getX1(); i <= d->getX2(); i++)
		{
			if (fabs(m_pdVarPassSignal[i]) > varMax)
			{
				varMax = fabs(m_pdVarPassSignal[i]);
			}
		}

		bm->setVarMax(varMax * 10.0); // legacy scaling (serves no real purpose)
	
		// std
		meanVal = dBase - bm->getMeanHeight();
		for (long i = d->getX1(); i <= d->getX2(); i++)
		{
			stdev += sqr(m_pdSignal[i] - meanVal);
		}
		stdev = sqrt(stdev / (d->length() - 1));
		bm->setFhrStd(stdev);

		GetBaselineBumpMeasures(d);

		if (m_ContractDetect == NULL)
		{
			return false;
		}

		d->setContrIndex(GetContractionIndex(d->getX1(), d->getX2()));

		if (d->getContrIndex() > -1)
		{
			bm->setContrBegin(m_ContractDetect[d->getContrIndex()].lStart - d->getX1());
			bm->setContrEnd(m_ContractDetect[d->getContrIndex()].lEnd - d->getX2());
		}

		bm->setMeasured(true);

		return true;
	}
	// =======================================================================================================================
	// =======================================================================================================================
	void CFHRSignal::GetDecelHeightBM(decel *d)
	{
		ASSERT((d->getX1() >= 0) && (d->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		bumpMeasuresDecel *bm = d->getBM();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dBase = (m_pdSignal[d->getX1()] + m_pdSignal[d->getX2()]) / 2.0;
		long peakIndex = d->getX1();
		double peakVal = m_pdSignal[d->getX1()];
		double area = 0.0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = d->getX1(); i <= d->getX2(); i++)
		{
			area += dBase - m_pdSignal[i];
			if (m_pdSignal[i] < peakVal)
			{
				peakVal = m_pdSignal[i];
				peakIndex = i;
			}
		}

		bm->setArea(area);
		bm->setMeanHeight(area / d->length());
		bm->setMaxHeight(dBase - peakVal);
		d->setPeak(peakIndex);
		d->setPeakVal(peakVal);
		d->setHeight(bm->getMaxHeight());
	}

	// =======================================================================================================================
	// =======================================================================================================================
	void CFHRSignal::GetBaselineBumpMeasures(bump *b)
	{
		ASSERT((b->getX1() >= 0) && (b->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		baseline blPrev;
		baseline blNext;

		GetPrevNextBaseLines(b, &blPrev, &blNext);
		bool havePrev = (blPrev.getY1() != -1.0);
		bool haveNext = (blNext.getY1() != -1.0);
		bumpMeasures *bm = b->getBM();
		bm->setFhrPrev(DBL_MAX);
		bm->setFhrNext(DBL_MAX);
		bm->setVarPrev(DBL_MAX);
		bm->setVarNext(DBL_MAX);

		#if BP2_DEBUG_SHOW_BUMP_PREV_NEXT_BAS
		// PAW debug
		std::fstream fs;
		fs.open ("bumpClassification.txt", std::fstream::in | std::fstream::out | std::fstream::app);
		if (!m_bumpClassificationHeaderDone)
		{
			m_bumpClassificationHeaderDone = true;
			fs << "m_nTotalCount\t"
			       "m_NextRepairStart\t"
				   "m_tracingDataIndex\t"
				   "Bp2StableBaselineLimit\t"
				   "m_basCutoff\t"
				   "m_accCutoff\t"
				   "m_decCutoff\t"
				   "m_commitIndexDec\t"
				   "m_commitIndexAcc\t"
				   "ACC/DEC\t"
				   "PrevBAS\t"
				   "Bump\t"
				   "NextBAS\t"
				   "m_pBaseLinesTotal.getLast\t"
				   "m_pBaseLinesFinal.getLast\t"
				   "DecisionDelay\t"
				   "MHType\t"
				   "\r\n";
		}

		fs << m_nTotalCount << "\t";
		fs << m_NextRepairStart << "\t";
		fs << m_tracingDataIndex << "\t";
		fs << GetBp2StableBaselineLimit() << "\t";

		fs << m_basCutoff << "\t";
		fs << m_accCutoff << "\t";
		fs << m_decCutoff << "\t";

		fs << m_commitIndexDec << "\t";
		fs << m_commitIndexAcc << "\t";

		if (b->isDecel())
			fs << "DEC" << "\t";
		else
			fs << "ACC" << "\t";

		if (havePrev)
			fs << blPrev.getX1()+m_OutputAbsStart << "-" << blPrev.getX2()+m_OutputAbsStart << "\t";
		else
			fs << "\t";
		fs <<  b->getX1()+m_OutputAbsStart << "-" << b->getX2()+m_OutputAbsStart << "\t";
		if (haveNext)
			fs << blNext.getX1()+m_OutputAbsStart << "-" << blNext.getX2()+m_OutputAbsStart << "\t";
		else
			fs << "\t";
		if (!m_pBaseLinesTotal.empty())
			fs << m_pBaseLinesTotal.getLast()->getX1() << "-" << m_pBaseLinesTotal.getLast()->getX2() << "\t";
		else
			fs << "\t";

		if (!m_pBaseLinesFinal.empty())
			fs << m_pBaseLinesFinal.getLast()->getX1() +m_OutputAbsStart << "-" << m_pBaseLinesFinal.getLast()->getX2() +m_OutputAbsStart << "\t";
		else
			fs << "\t";

		fs << m_nTotalCount - (m_OutputAbsStart+b->getX2()) << "\t";
		if (b->getX2() < GetBp2StableBaselineLimit())
		{
			fs << "*";
		}
		if (haveNext)		
			fs << blNext.getMhType() << "\t";
		else
			fs << "\t";
		fs << "\r\n";
		fs.close();
		#endif

		if (m_nTotalCount==62160)
			bool gotHere = true;

		if (havePrev)
		{
			// CHE - do not recalc here - should already be valid - may have -ve indexed
			// baselines 
			if (m_ConfigFHR.m_bUseAvgBasLevelForYvals)
			{
				bm->setFhrPrev(blPrev.getYmean());
			}
			else
			{
				bm->setFhrPrev(blPrev.getY2());
			}
			bm->setVarPrev(blPrev.getVarMin() * 10.0); // scaling as during training
		}

		if (haveNext)
		{
			if (m_ConfigFHR.m_bUseAvgBasLevelForYvals)
			{
				bm->setFhrNext(blNext.getYmean());
			}
			else
			{
				bm->setFhrNext(blNext.getY1());
			}

			bm->setVarNext(blNext.getVarMin() * 10.0);	// scaling as during training
			if (!(havePrev))	// no previous baseline - use next baseline as ref
			{
				bm->setVarPrev(bm->getVarNext());
				bm->setFhrPrev(bm->getFhrNext());
			}
		}
		else if (havePrev)						// no reference baseline after candidate - use values of prev instead of NaN
		{										// NaN gets normalized to the average value across all training events from all tracings
			bm->setVarNext(bm->getVarPrev());
			bm->setFhrNext(bm->getFhrPrev());
		}
		else									// no reference baseline at all - either at beginning of tracing or long time w/out baseline (dropout)
		{
			if (m_dLastGoodBasRef > 0.0)
			{									// not at beginning of tracing - use old ref baseline as better than no ref baseline
				bm->setFhrPrev(m_dLastGoodBasRef);
				bm->setFhrNext(m_dLastGoodBasRef);
			}
			else
			{									// have absolutely no reference baseline - use fhr value at beginning and end of event
				bm->setFhrPrev(m_pdSignal[b->getX1()]);
				bm->setFhrNext(m_pdSignal[b->getX2()]);
			}
		}

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::MeasureOneAccel(accel *a, fhrPartSet *decels)
	{
		ASSERT((a->getX1() >= 0) && (a->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		/*~~~~~~~~~~~~*/
		// DBL_MAX cases convert params to zero in NormalizeOneBump
		bool bRC = true;
		bumpMeasuresAccel *bm = a->getBM();
		/*~~~~~~~~~~~~*/
		// height measures (max Height, mean height, area etc.. should have been already calculated during preprocessing 
		if (bm->getMaxHeight() < 0) // invalid
		{
			GetAccelHeightBM(a);
		}	
		
		bm->setLength(a->length());
		bm->setFhrBegin(m_pdSignal[a->getX1()]);
		bm->setFhrEnd(m_pdSignal[a->getX2()]);
		GetBaselineBumpMeasures(a);

		double stdev = 0.0;
		double dBase = (m_pdSignal[a->getX1()] + m_pdSignal[a->getX2()]) / 2.0;
		double meanVal = bm->getMeanHeight() + dBase;
		double varMax = 0.0;

		for (long i = a->getX1(); i <= a->getX2(); i++)
		{
			if (fabs(m_pdVarPassSignal[i]) > varMax)
			{
				varMax = fabs(m_pdVarPassSignal[i]);
			}
			stdev += sqr(m_pdSignal[i] - meanVal);
		}
		stdev = sqrt(stdev / (a->length() - 1));
		bm->setFhrStd(stdev);
		bm->setVarMax(varMax);

		/*~~~~~~~~~~~~~~~~~*/
		// LP25 features
		long lSlopeTimeIn25;
		long lSlopeTimeOut25;
		/*~~~~~~~~~~~~~~~~~*/

		GetMaxSlopeInOutAccel25(a, &lSlopeTimeIn25, &lSlopeTimeOut25);
		bm->setSlopeTimeIn25(lSlopeTimeIn25);
		bm->setSlopeTimeOut25(lSlopeTimeOut25);

		/*~~~~~~~~~~~~~~~~~~~~*/
		long lOnset25;
		long lRecovery25;
		double dOnsetSlope25;
		double dRecoverySlope25;
		/*~~~~~~~~~~~~~~~~~~~~*/

		GetOnsetAndRecovFromSignal(a, m_pdLowPass25Signal, &lOnset25, &lRecovery25, &dOnsetSlope25, &dRecoverySlope25);
		bm->setOnset25(lOnset25);
		bm->setRecovery25(lRecovery25);
		bm->setRecoverySlope25(dRecoverySlope25);
		bm->setOnsetSlope25(dOnsetSlope25);
		bm->setLastDecelX2(a->getX1() - GetLastDecelX2(a->getX1(), decels));
		bm->setNextDecelX1(GetNextDecelX1(a->getX2(), decels) - a->getX2());
		bm->setDecelRate(GetDecelRate(a->getX1(), m_ConfigFHR.GetDecelRateWindow(), decels));

		return bRC;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::GetAccelHeightBM(accel *a)
	{
		ASSERT((a->getX1() >= 0) && (a->getX2() < m_nPtsCount)); // LXP OUT OF BOUNDS

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dBase = (m_pdSignal[a->getX1()] + m_pdSignal[a->getX2()]) / 2.0;
		long peakIndex = a->getX1();
		double peakVal = m_pdSignal[a->getX1()];
		double area = 0.0;
		bumpMeasuresAccel *bm = a->getBM();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (long i = a->getX1(); i <= a->getX2(); i++)
		{
			area += m_pdSignal[i] - dBase;
			if (m_pdSignal[i] > peakVal)
			{
				peakVal = m_pdSignal[i];
				peakIndex = i;
			}
		}

		bm->setArea(area);
		bm->setMeanHeight(area / a->length());
		bm->setMaxHeight(peakVal - dBase);
		a->setPeak(peakIndex);
		a->setPeakVal(peakVal);
		a->setHeight(bm->getMaxHeight());
	}

	/*
	=======================================================================================================================
	GetPrevNextBaseLines: get baselines immediately prior to and after Bump.
	=======================================================================================================================
	*/
	void CFHRSignal::GetPrevNextBaseLines(bump *b, baseline *blPrev, baseline *blNext)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lSize = m_pBaseLinesFinal.size();
		long lStart = b->getX1();
		long lEnd = b->getX2();
		long lc1, lc2;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (lc1 = 0; lc1 < lSize; lc1++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			baseline *bl = (baseline *) m_pBaseLinesFinal.getAt(lc1);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (bl->getX1() < (lStart - m_ConfigFHR.GetMinBaseLineLength()))
			{
				*(blPrev) = *(bl);
				if (blPrev->getX2() >= lStart)
				{
					if (blPrev->getX1() < 0)
					{	// ensure have signal available in window to recalc polyfit
						blPrev->setX1(0);		// this could only happen if baseline is extremely long - truncate at beginning of signal window
					}

					blPrev->setX2(lStart - 1);	// truncate bas at start of bump
					if (!(m_ConfigFHR.m_bUseAvgBasLevelForYvals))
					{
						GetBaselinePolyfit(blPrev);
					}

					blPrev->getMeanVal(m_pdSignal, m_nPtsCount);
					blPrev->setVarMin(BaselineVarMin(blPrev)); // recalc min filter Var value
				}
			}
			else
			{
				break;
			}
		}

		for (lc2 = max(0, lc1 - 1); lc2 < lSize; lc2++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			baseline *bl = (baseline *) m_pBaseLinesFinal.getAt(lc2);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (bl->getX2() > (lEnd + m_ConfigFHR.GetMinBaseLineLength()))
			{
				*(blNext) = *(bl);
				if (blNext->getX1() <= lEnd)
				{
					blNext->setX1(lEnd + 1);				// truncate bas at end of bump
					if (!(m_ConfigFHR.m_bUseAvgBasLevelForYvals))
					{
						GetBaselinePolyfit(blNext);
					}

					blNext->getMeanVal(m_pdSignal, m_nPtsCount);
					blNext->setVarMin(BaselineVarMin(blNext));
				}

				break;
			}
		}

		return;
	}

	/*
	=======================================================================================================================
	CFHRSignal:GetOnsetAndRecovFromSignal: Get the onset index (time from start of bump to get to 90% of peak/nadir)
	and recovery (time from 90% of peak/nadir to end of bump) based on the specified signal. Boundaries of pBump must
	have valid indexes in pSignal
	=======================================================================================================================
	*/
	bool CFHRSignal::GetOnsetAndRecovFromSignal(fhrPart *f, double *pSignal, long *lOnset, long *lRecov, double *dOnsetSlope, double *dRecoverySlope)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long lMaxIndex = f->getX1();
		long lMinIndex = f->getX1();
		double dMaxSignal = pSignal[f->getX1()];
		double dMinSignal = pSignal[f->getX1()];
		long lPeakIndex = 0;
		double dPeak = 0.0;
		double dPeak90 = 0.0;
		double dSign = 1.0;
		double dBase;
		long i = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (i = f->getX1() + 1; i <= f->getX2(); i++)
		{
			if (pSignal[i] < dMinSignal)
			{
				lMinIndex = i;
				dMinSignal = pSignal[i];
			}

			if (pSignal[i] > dMaxSignal)
			{
				lMaxIndex = i;
				dMaxSignal = pSignal[i];
			}
		}

		if (f->isAccel())
		{
			dPeak = dMaxSignal;
			lPeakIndex = lMaxIndex;
			dSign = 1.0;
		}
		else
		{
			dPeak = dMinSignal;
			lPeakIndex = lMinIndex;
			dSign = -1.0;
		}

		dBase = (pSignal[f->getX1()] + pSignal[f->getX2()]) / 2.0;
		dPeak90 = dBase + (0.9 * (dPeak - dBase));

		// onset - need to look from start of bump because can not assume monotocity
		// (otherwise would be more efficient to look from peak
		i = f->getX1();
		while ((dSign * pSignal[i]) < (dSign * dPeak90))
			i++;
		(*lOnset) = i - f->getX1();
		if ((*lOnset) == 0)
		{
			(*dOnsetSlope) = 0;
		}
		else
		{
			(*dOnsetSlope) = (double) (pSignal[i] - pSignal[f->getX1()]) / (*lOnset);
		}

		i = f->getX2(); // need to look from end because can not assume montocity
		while ((dSign * pSignal[i]) < (dSign * dPeak90))
			i--;
		(*lRecov) = f->getX2() - i;
		if ((*lRecov) == 0)
		{
			(*dRecoverySlope) = 0;
		}
		else
		{
			(*dRecoverySlope) = (double) (pSignal[f->getX2()] - pSignal[i]) / (*lRecov);
		}

		return true;
	}

	/*
	=======================================================================================================================
	CFHRSignal:GetOnsetAndRecovFromLP: Get the onset index (time from start of bump to get to 90% of peak/nadir) and
	recovery (time from 90% of peak/nadir to end of bump) based on the lowpass signal. Normally Bump->onset and
	Bump->recovery come from band pass domain. The LP onset/recov can be used on extended candidates for determining
	whether should be rejected due to repair (and potentially classification in future)
	=======================================================================================================================
	*/
	bool CFHRSignal::GetOnsetAndRecovFromLP(bump *b, long *lOnset, long *lRecov)
	{
		/*~~~~~~~~~~~*/
		double dDummy1;
		double dDummy2;
		/*~~~~~~~~~~~*/

		return GetOnsetAndRecovFromSignal(b, m_pdLowPassSignal, lOnset, lRecov, &dDummy1, &dDummy2);
	}

	/*
	=======================================================================================================================
	GetMaxSlopeInOutAccel25: Using LP25 signal, get the time of the maximum slope in and time from end of maximum slope
	out Because LP25 domain do not need to search for peak - can just take max and min slope over entire bump
	=======================================================================================================================
	*/
	bool CFHRSignal::GetMaxSlopeInOutAccel25(accel *a, long *lMaxTimeIn, long *lMaxTimeOut)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dMaxSlopeIn = -999.0;
		double dMaxSlopeOut = 999.0;
		double dCurrSlope;
		long i = 0;
		long lInSlopeIndex;
		long lOutSlopeIndex;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		lInSlopeIndex = a->getX1();
		lOutSlopeIndex = a->getX2();

		for (i = a->getX1() + 1; i <= a->getX2(); i++)
		{
			dCurrSlope = m_pdLowPass25Signal[i] - m_pdLowPass25Signal[i - 1];
			if (dCurrSlope > dMaxSlopeIn)
			{
				dMaxSlopeIn = dCurrSlope;
				lInSlopeIndex = i;
			}

			if (dCurrSlope < dMaxSlopeOut)
			{
				dMaxSlopeOut = dCurrSlope;
				lOutSlopeIndex = i;
			}
		}

		(*lMaxTimeIn) = lInSlopeIndex - a->getX1() - 1;
		(*lMaxTimeOut) = a->getX2() - lOutSlopeIndex;

		return true;
	}

	/*
	=======================================================================================================================
	GetDecelRate: Get % time of decels in window of size lWindow back from lX1
	=======================================================================================================================
	*/
	double CFHRSignal::GetDecelRate(long lX1, long lWindow, fhrPartSet *decels)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// we want to get the percentage of time in the window lX1 - lWindow -> lX1 that
		// are occupied by decels DecelLocal can be used to pass newly classified decels
		// that have not yet been added to the overall buffer It is assumed that decels in
		// DecelLocal are expressed in relative coordinates It is assumed that lX1 is
		// expressed in relative coordinates Most decels used to calculate decel rate will
		// come from m_pTotalOutputObjects (already output decels), which are in absolute
		// coordinates
		long lDecelSamples = 0;
		long lWindowX1 = lX1 - lWindow;
		long lWindowX1Abs = lWindowX1 + m_OutputAbsStart;
		long lX1Abs = lX1 + m_OutputAbsStart;
		long lastX2;
		long lastX1;
		long currX1;
		long currX2;
		long lCurrSamples;
		long i = m_TotalOutputFhrPartSet.size() - 1;
		double dDecelRate = 0.0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		lastX2 = lX1Abs - 1;

		// First deal with decels that have already been output
		while ((i >= 0) && (lastX2 > lWindowX1Abs)) // assume decels are ordered
		{
			if (m_TotalOutputFhrPartSet.getAt(i)->isDecel())
			{
				currX2 = m_TotalOutputFhrPartSet.getAt(i)->getX2();
				currX1 = m_TotalOutputFhrPartSet.getAt(i)->getX1();
				if ((currX2 > lWindowX1Abs) && (currX1 < lX1Abs))
				{
					lDecelSamples += min(lX1Abs, currX2) - max(currX1, lWindowX1Abs) + 1;
				}

				lastX2 = currX2;
			}

			i--;
		}

		// Now consider local decels - can assume that these are ordered and are in
		// relative coords There is a possibility of overlap between decels in this case
		// due to bump extension
		lastX2 = lX1 - 1;
		lastX1 = lX1 - 1;
		i = decels->size() - 1;
		while ((i >= 0) && (lastX2 > lWindowX1))
		{
			currX2 = decels->getAt(i)->getX2();
			currX1 = decels->getAt(i)->getX1();
			if ((currX2 > lWindowX1) && (currX1 < lX1))
			{
				if (decels->getAt(i)->isDecel())
				{
					lCurrSamples = min(min(currX2, lastX1), lX1) - max(currX1, lWindowX1) + 1;
					if (lCurrSamples > 1)
					{
						lDecelSamples += lCurrSamples;
					}

					lastX1 = currX1;				// lastX1 used only to ensure that samples from overlapping decels not counted twice
				}
			}

			lastX2 = currX2;
			i--;
		}

		if (lWindowX1Abs < 0)
		{	// less than window length from start of tracing
			dDecelRate = (double) lDecelSamples / (lX1Abs);
		}
		else
		{
			dDecelRate = (double) lDecelSamples / lWindow;
		}

		return dDecelRate;
	}

	/*
	=======================================================================================================================
	GetDecelRate: if don't want to specify local decels (just look in already commited decels)
	=======================================================================================================================
	*/
	double CFHRSignal::GetDecelRate(long lX1, long lWindow)
	{
		fhrPartSet dummy;
		return GetDecelRate(lX1, lWindow, &dummy);
	}

	/*
	=======================================================================================================================
	GetNextDecelX1: get next start index of earliest ensuing decel after lX2 pDecelLoc specify decel candidates that
	have been classified but no commited to output
	=======================================================================================================================
	*/
	long CFHRSignal::GetNextDecelX1(long lX2, fhrPartSet *decels)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// first check local bumps - this array will have nondecels and decels
		long lMaxDist = (long) m_ConfigFHR.GetMaxSurrDecelDist();
		long i = decels->size() - 1;
		bool bInLoop = true;
		long lMaxNextX1 = lX2 + lMaxDist;
		long lNextX1 = lMaxNextX1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while ((i >= 0) && (bInLoop))
		{
			decel *d = (decel*) decels->getAt(i);
			if (d->isDecel())
			{
				if ((d->getX1() > lX2) && (d->getX1() < lMaxNextX1))
				{
					lNextX1 = d->getX1();
				}
				else
				{
					bInLoop = false;
				}
			}

			i--;
		}

		// if no local decels need to look in array of output objects
		i = m_TotalOutputFhrPartSet.size() - 1;
		while ((i >= 0) && (bInLoop))
		{
			if (m_TotalOutputFhrPartSet.getAt(i)->isDecel())
			{
				if ((m_TotalOutputFhrPartSet.getAt(i)->getX1() - m_OutputAbsStart > lX2) && (m_TotalOutputFhrPartSet.getAt(i)->getX1() - m_OutputAbsStart < lMaxNextX1))
				{
					lNextX1 = m_TotalOutputFhrPartSet.getAt(i)->getX1() - m_OutputAbsStart;
				}
				else
				{
					bInLoop = false;
				}
			}

			i--;
		}

		return lNextX1;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CFHRSignal::GetLastDecelX2(long lX1, fhrPartSet *decels)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// first check local bumps - this array will have nondecels and decels
		long lMaxDist = (long) m_ConfigFHR.GetMaxSurrDecelDist();
		long i = decels->size() - 1;
		bool bInLoop = true;
		long lMinLastX2 = lX1 - lMaxDist;
		long lLastX2 = lMinLastX2;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		while ((i >= 0) && (bInLoop))
		{
			decel *d = (decel*) decels->getAt(i);
			if (d->isDecel())
			{
				if ((d->getX2() < lX1) && (d->getX2() > lMinLastX2))
				{
					lLastX2 = d->getX2();
					bInLoop = false;
				}
			}

			i--;
		}

		// if no local decels need to look in array of output objects note that pre-merge
		// array will only keep last ~45 minutes of decels so use overall output instea
		i = m_TotalOutputFhrPartSet.size() - 1;
		while ((i >= 0) && (bInLoop))
		{
			if (m_TotalOutputFhrPartSet.getAt(i)->isDecel())
			{
				if ((m_TotalOutputFhrPartSet.getAt(i)->getX2() - m_OutputAbsStart < lX1) && (m_TotalOutputFhrPartSet.getAt(i)->getX2() - m_OutputAbsStart > lMinLastX2))
				{
					lLastX2 = m_TotalOutputFhrPartSet.getAt(i)->getX2() - m_OutputAbsStart;
					bInLoop = false;
				}
			}

			i--;
		}

		return lLastX2;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CFHRSignal::GetContractionIndex(long lStart, long lEnd)
	{
		/*~~~~~~~~~~~~~~~~~~*/
		long lContrIndex = -1;
		/*~~~~~~~~~~~~~~~~~~*/

		if (m_ContractDetect)
		{

			// Assumes contractions are ordered 1) Contraction starts before bump and ends
			// after start of bump if 1) does not exist then 2) Contraction ends less than 20
			// seconds before start of bump if 1) and 2) do not exist then 3) Contraction
			// starts no more than 20 seconds after start of bump and ends after start of bump
			for (long lc = 0; lc < m_lContrDet; lc++)
			{
				if ((m_ContractDetect[lc].lEnd > lStart) && (m_ContractDetect[lc].lStart <= lStart))
				{
					lContrIndex = lc;	// contraction starts before decel and ends after start of decel
					break;
				}
				else if ((lStart >= m_ContractDetect[lc].lEnd) && (lStart - m_ContractDetect[lc].lEnd <= m_ConfigFHR.GetMAX_TIME_AFTER_CONTRACT()))
				{
					lContrIndex = lc;
				}
				else if ((lContrIndex == -1) && (m_ContractDetect[lc].lEnd > lStart) && (m_ContractDetect[lc].lStart - lStart) <= m_ConfigFHR.GetMAX_TIME_BEFORE_CONTRACT())
				{
					lContrIndex = lc;
				}
				else if (m_ContractDetect[lc].lStart - lStart > m_ConfigFHR.GetMAX_TIME_BEFORE_CONTRACT())
				{
					break;
				}
			}
		}

		return lContrIndex;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::NormalizeOneBump DESCRIPTION: This function normalizes the measurements on their means and Standard
	//    Deviation from Stats collected during Neural Network learning PARAMETERS: RETURNS:
	// =======================================================================================================================
	//
	bool CFHRSignal::NormalizeOneDecel(PatternsVector* pV)
	{
		for (int i = 0; i < 20; i++)
		{
			if ((*pV) (i) == DBL_MAX)
			{
				(*pV) (i) = 0.0;
			}
			else
			{
				(*pV) (i) = ((*pV) (i) - m_ConfigFHR.GetDecelStats()[i].dMean) / m_ConfigFHR.GetDecelStats()[i].dStandardDev;
			}
		}

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::NormalizeOneAccel DESCRIPTION: This function normalizes the measurements on their means and Standard
	//    Deviation from Stats collected during Neural Network learning PARAMETERS: RETURNS:
	// =======================================================================================================================
	//
	bool CFHRSignal::NormalizeOneAccel(PatternsVector* pV)
	{
		for (int i = 0; i < m_ConfigFHR.GetNumberOfAccelStats(); i++)
		{
			if ((*pV) (i) == DBL_MAX)
			{
				(*pV) (i) = 0.0;
			}
			else
			{
				(*pV) (i) = ((*pV) (i) - m_ConfigFHR.GetAccelStats()[i].dMean) / m_ConfigFHR.GetAccelStats()[i].dStandardDev;
			}
		}

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::SimulateOneDecel DESCRIPTION: This function makes Bump Classification using Neural Network
	//    PARAMETERS: RETURNS:
	// =======================================================================================================================
	//
	bool CFHRSignal::SimulateOneDecel(const PatternsVector* inVector, decel *d, void *pFile)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int nDecExperts = m_ConfigFHR.GetNumberOfDecelExperts();
		double dMappedConfidence = 0.0;
		PatternsVector InVector = *inVector;
		PatternsVector outVector;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		d->setLate(false);
		d->setVariable(false);
		outVector.setBounds(0, 2);

		outVector(0) = 0.0;
		outVector(1) = 0.0;
		outVector(2) = 0.0;
		for (int i = 0; i < nDecExperts; i++)
		{
			outVector = outVector + m_ConfigFHR.GetDecelExperts(m_lCurrIteration)[i].Simulate(*inVector);
		}

		outVector(0) = outVector(0) / nDecExperts;
		outVector(1) = outVector(1) / nDecExperts;
		outVector(2) = outVector(2) / nDecExperts;

		if (m_ConfigFHR.m_bUseMaxConfidenceDecel)
		{										// original way of doing it
			if (outVector(0) > max(outVector(1), outVector(2)))
			{
				d->setSubtype(fhrPart::DecType_VAR);
				d->setConfidence(outVector(0));
			}
			else if (outVector(1) > max(outVector(0), outVector(2)))
			{
				d->setSubtype(fhrPart::DecType_GRAD);
				d->setConfidence(outVector(1));
			}
			else if (outVector(2) >= max(outVector(0), outVector(1)))
			{
				d->setType(fhrPart::FhrPartType_NONDECEL);
				d->setConfidence(outVector(2));
			}
		}
		else
		{										// different threshold for different types
			if (outVector(0) > outVector(1))	// abrupt > gradual
			{	// abrupt
				
				if (outVector(2) < GetMaxNonDecConf(fhrPart::DecType_VAR, m_lCurrIteration))
				{					
					d->setVariable(true);
					SubtypeDecel(d); // will set late flag if late timing
					d->setSubtype(fhrPart::DecType_VAR); // set type explicitly to VAR
					SubtypeProlonged(d);
				}
				else
				{
					d->setType(fhrPart::FhrPartType_NONDECEL);
				}
				d->setConfidence(MapDecelConfidence(outVector(2), GetMaxNonDecConf(fhrPart::DecType_VAR, m_lCurrIteration)));
			}
			else
			{
				SubtypeDecel(d);  // see what type of decel would be if decel (based on timing)
				double dMaxNonDecConf = GetMaxNonDecConf(d->getSubtype(), m_lCurrIteration);
				if (outVector(2) < dMaxNonDecConf)
				{ // decel			
					SubtypeProlonged(d);
				}
				else
				{ // non decel
					d->setType(fhrPart::FhrPartType_NONDECEL);
				}
				d->setConfidence(MapDecelConfidence(outVector(2), dMaxNonDecConf));
			}
		}

		if (m_ConfigFHR.m_iStoreXML > 1)
		{
			try
			{
				if ((pFile) && (!(d->isPending())))
				{
					/*~~~~~~~~~~~~~~~~~~~~~~~*/
					CString sFeature_Statistic;
					CString sFeature_Stat;
					/*~~~~~~~~~~~~~~~~~~~~~~~*/

					sFeature_Statistic = "";
					
					for (int iField = 0; iField < 3; iField++)
					{
						if (_isnan(outVector(iField)))
						{
							sFeature_Stat.Format(_T("NaN\t"));
						}
						else
						{
							sFeature_Stat.Format(_T("%f\t", outVector(iField)));
						}
						sFeature_Statistic += sFeature_Stat;
					}

					sFeature_Stat.Format(_T("%d\t%d\n", d->getX1(), d->getX2()));
					sFeature_Statistic += sFeature_Stat;
					((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
				}
			}

			catch(...)
			{
			}
		}

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::SimulateOneAccel DESCRIPTION: This function makes Bump Classification using Neural Network PARAMETERS:
	//    RETURNS:
	// =======================================================================================================================
	//
	bool CFHRSignal::SimulateOneAccel(const PatternsVector* inVector, accel *a, void *pFile)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRet = true;
		int nAccExperts = m_ConfigFHR.GetNumberOfAccelExperts();
		double dMinAccelConf = 0.5;
		double dMappedConfidence = 0.0;
		PatternsVector InVector = *inVector;
		PatternsVector outVector;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		outVector.setBounds(0, 1);

		outVector(0) = 0.0;
		outVector(1) = 0.0;
		for (int i = 0; i < nAccExperts; i++)
		{
			outVector = outVector + m_ConfigFHR.GetAccelExperts(m_lCurrIteration)[i].Simulate(*inVector);
		}

		outVector(0) = outVector(0) / nAccExperts;
		outVector(1) = outVector(1) / nAccExperts;

		if (!(m_ConfigFHR.m_bUseMaxConfidenceAccel))
		{
			dMinAccelConf = GetMinAccelConf(m_lCurrIteration);
		}

		if (outVector(0) < dMinAccelConf)
		{
			a->setType(fhrPart::FhrPartType_NONACCEL);
			a->setConfidence(outVector(1));
		}
		else
		{
			a->setType(fhrPart::FhrPartType_ACCEL);
			a->setConfidence(outVector(0));
			
		}
		if (m_ConfigFHR.m_bMapOutputConfidence)
		{
			dMappedConfidence = MapAccelConfidence(outVector(0), dMinAccelConf);
			a->setConfidence(dMappedConfidence);
		}

		try
		{
			if ((pFile) && (!(a->isPending())))
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~*/
				CString sFeature_Statistic;
				CString sFeature_Stat;
				/*~~~~~~~~~~~~~~~~~~~~~~~*/

				sFeature_Statistic = "";
				for (int iField = 0; iField < 2; iField++)
				{
					sFeature_Stat.Format(_T("%f\t", outVector(iField)));
					sFeature_Statistic += sFeature_Stat;
				}

				sFeature_Statistic += "ACC\t";

				sFeature_Stat.Format(_T("%d\t%d\n", a->getX1(), a->getX2()));
				sFeature_Statistic += sFeature_Stat;

				if (m_ConfigFHR.m_iStoreXML > 1)
				{
					((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
				}
			}
		}

		catch(...)
		{
		}

		return bRet;
	}

	/*
	=======================================================================================================================
	MapAccelConfidence: Want to map the confidence output of the NN to a value between 0.5 -> 1, 0.5 will correspond to
	the minimum required NN confidence to classify as an accel, 1 will correspond to NN confidence of 1
	=======================================================================================================================
	*/
	double CFHRSignal::MapAccelConfidence(double dAccelConf, double dMinAccelConf)
	{
		double dMapConf;

		if (dAccelConf >= dMinAccelConf)
		{
			dMapConf = 0.5 + (0.5 * (dAccelConf - dMinAccelConf) / (1 - dMinAccelConf));
		}
		else
		{
			dMapConf = 0.5 + (0.5 * (dMinAccelConf - dAccelConf) / dMinAccelConf);
		}


		dMapConf = min(1.0, dMapConf);

		return dMapConf;
	}

	/*
	=======================================================================================================================
	MapDecelConfidence: Want to map the confidence output of the NN to a value between 0.5 -> 1, For decels it is
	trickier as there are 3 outputs of the NN (abrupt, gradual, non) Here we map the nonDecelConfidence to 0 -> 0.5 and
	take the confidence as 1 minus that value. a nonDecelConfidence of maxNonDecelConfidence maps to 0.5 while a
	nonDecelConfidence of 0 maps to 0.
	=======================================================================================================================
	*/
	double CFHRSignal::MapDecelConfidence(double dNonDecelConf, double dMaxNonDecelConf)
	{
		double dMapConf;

		if (dNonDecelConf < dMaxNonDecelConf)
		{
			dMapConf = 0.5 + (0.5 * (dMaxNonDecelConf - dNonDecelConf) / dMaxNonDecelConf);
		}
		else
		{ // confidence that is non-decel
			dMapConf = 0.5 + (0.5 * (dNonDecelConf - dMaxNonDecelConf) / (1 - dMaxNonDecelConf));
		}
			
		dMapConf = min(1.0, dMapConf);

		return dMapConf;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CFHRSignal::GetMaxNonDecConf(fhrPart::FhrPartType BType, long iteration)
	{
		double* allConf;
		double conf;

		if ((GetNumAccelDecelIterations() == 1) && (m_ConfigFHR.m_bNNtrain == false))
		{
			if (BType == fhrPart::DecType_VAR)
				conf = m_ConfigFHR.m_dMaxNonDecConfForAbrupt_oneIter;
			else if (BType == fhrPart::DecType_LATE)
				conf = m_ConfigFHR.m_dMaxNonDecConfForLate_oneIter;
			else if (BType == fhrPart::DecType_EARLY)
				conf = m_ConfigFHR.m_dMaxNonDecConfForEarly_oneIter;
			else
				conf = m_ConfigFHR.m_dMaxNonDecConfForNonAssoc_oneIter;
		}
		else
		{

			if (BType == fhrPart::DecType_VAR)
				allConf = m_ConfigFHR.m_dMaxNonDecConfForAbrupt;
			else if (BType == fhrPart::DecType_LATE)
				allConf = m_ConfigFHR.m_dMaxNonDecConfForLate;
			else if (BType == fhrPart::DecType_EARLY)
				allConf = m_ConfigFHR.m_dMaxNonDecConfForEarly;
			else
				allConf = m_ConfigFHR.m_dMaxNonDecConfForNonAssoc;

			conf = allConf[iteration];
		}

		return(conf);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CFHRSignal::GetMinAccelConf(long iteration)
	{
		double conf;
		
		if ((GetNumAccelDecelIterations() == 1) && (m_ConfigFHR.m_bNNtrain == false))
			conf = m_ConfigFHR.m_dMinAccConf_oneIter;
		else
			conf = m_ConfigFHR.m_dMinAccConf[iteration];

		return(conf);
	}

	/*
	=======================================================================================================================
	ModifyAccelCandidateBounds: Truncate a based on decels in d
	=======================================================================================================================
	*/
	int CFHRSignal::ModifyAccelCandidateBounds(fhrPartSet *a, fhrPartSet *d, long lOffset)
	{
		/*~~~~~~~~~~~~~~~~~~*/
		long nOverlap = 0;
		long nBumpsRemove = 0;
		long overIndex1 = 0;
		long overIndex2 = 0;
		bool bInLoop = true;
		long lX1a;
		long lX2a;
		long lX1b;
		long lX2b;
		long lNumDegen = 0;
		/*~~~~~~~~~~~~~~~~~~*/

		for (long i = 0; i < a->size(); i++)
		{
			lX1a = a->getAt(i)->getX1();
			lX2a = a->getAt(i)->getX2();
			bInLoop = true;
			while ((overIndex1 < d->size()) && (bInLoop))	// advance through overlap bumps prior to current bump
			{
				lX2b = d->getAt(overIndex1)->getX2() - lOffset;
				if (lX2b < lX1a)
				{
					overIndex1++;
				}
				else
				{
					bInLoop = false;
				}
			}

			bInLoop = true;
			overIndex2 = overIndex1;
			while ((overIndex2 < d->size()) && (bInLoop))
			{
				decel *dcurr = (decel*) d->getAt(overIndex2);
				lX1b = dcurr->getX1() - lOffset;
				lX2b = dcurr->getX2() - lOffset;
				if (lX1b > lX2a)
				{	// past current bump
					bInLoop = false;
				}
				else
				{	// what if decel splits accel in two - do we need to deal with this case?
					if (dcurr->isDecel())
					{	// here we have overlap and need to truncate accel candidate
						if (lX1b < lX1a)
						{
							a->getAt(i)->setX1(lX2b + 1);
						}

						if (lX2b > lX2a)
						{
							a->getAt(i)->setX2(lX1b - 1);
						}
					}
					overIndex2++;
				}
			}

			if (a->getAt(i)->length() < m_ConfigFHR.GetLENGTH_THRESHOLD())
			{
				lNumDegen++;
			}
		}

		// if have degenerate need to shift bumps
		if (lNumDegen > 0)
		{
			/*~~~~~~~*/
			long i = 0;
			/*~~~~~~~*/
			for (i = a->size() - 1; i >= 0; i--)
			{
				if (a->getAt(i)->length() < m_ConfigFHR.GetLENGTH_THRESHOLD())
				{
					a->removeAt(i);
				}
			}
		}

		return lNumDegen;
	}

	/*
	=======================================================================================================================
	PostProcessDecels: Remove any variable decels (abrupt) that are less than 15 bpm in height
	=======================================================================================================================
	*/
	void CFHRSignal::PostProcessDecels(fhrPartSet *decels)
	{
		/*~~~*/
		long i;
		/*~~~*/

		for (i = 0; i < decels->size(); i++)
		{
			decel *d = (decel*) decels->getAt(i);
			PostProcessDecel(d);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::PostProcessDecel(decel *d)
	{
		if (d->getSubtype() == fhrPart::DecType_VAR)
		{
			if (d->getBM()->getMaxHeight() < m_ConfigFHR.m_dMinAbruptDecelHeight)
			{
				d->setType(fhrPart::FhrPartType_NONDECEL);
			}
		}

		return (d->isDecel());
	}

	/*
	=======================================================================================================================
	PreprocessAccels: Remove accel candidates that do not meet height requirements
	=======================================================================================================================
	*/
	bool CFHRSignal::PreprocessAccels(fhrPartSet *accels)
	{
		for (long i = accels->size() - 1; i >= 0; i--)
		{
			accel *a = (accel*) accels->getAt(i);
			if (PreprocessAccel(a))
			{
				accels->removeAt(i);
			}
		}

		return(true);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::PreprocessAccel(accel *a)
	{
		GetAccelHeightBM(a);
		return((a->getBM()->getMaxHeight() < m_ConfigFHR.m_dMinAccelCandHeight) 
			|| (a->getBM()->getMeanHeight() < m_ConfigFHR.m_dMinAccelCandMeanHeight) 
			|| (a->length() < m_ConfigFHR.GetLENGTH_THRESHOLD()));
	}

	bool CFHRSignal::PreprocessDecels(fhrPartSet *decels)
	{
		for (long i = decels->size() - 1; i >= 0; i--)
		{
			decel *d = (decel*) decels->getAt(i);
			if (PreprocessDecel(d))
			{
				decels->removeAt(i);
			}
		}

		return(true);
	}

	bool CFHRSignal::PreprocessDecel(decel *d)
	{
		return (d->length() < m_ConfigFHR.GetLENGTH_THRESHOLD());
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::getPosNegBumps DESCRIPTION: This function makes Bump Candidates from Zero Crossings PARAMETERS:
	//    RETURNS:
	// =======================================================================================================================
	//
	void CFHRSignal::getPosNegBumps
		(
		const long *lZC,
		long lZCPoints,
		int ind,
		long *dLastZCPos,
		long *dLastZCNeg,
		fhrPartSet *PositiveBumps,
		fhrPartSet *NegativeBumps
		)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		CAccelDecelBandPassConfig *pConf = m_ConfigFHR.GetBandPassConfig(ind);
		long lSmallAreaBumpsRejected = 0;
		long lLastRealValidIndex = m_lFiltered - GetNumExtrapolatedSignal();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		dLastZCPos[ind] = 0;
		dLastZCNeg[ind] = 0;

		for (long k = 0; k < lZCPoints - 1; k++)
		{
			if (m_pdSignalToProcess[lZC[k] - 1] > 0)	// going into decel
			{
				if (lZC[k] <= lLastRealValidIndex) // only record zc for purposes of commit index if it is in non-extrapolated signal
					dLastZCPos[ind] = lZC[k];				// from pos to neg

				// check if bumps would have been considered in previous window
				if (lZC[k + 1] <= (m_commitIndexDec - m_OutputAbsStart))
				{
					continue;
				}
			}
			else	// going into accel
			{
				if (lZC[k] <= lLastRealValidIndex) // only record zc for purposes of commit index if it is in non-extrapolated signal
					dLastZCNeg[ind] = lZC[k];

				// check if bumps ends before point at which it can not matter for output from
				// this window if (lZC[k+1] <= (m_commitIndexDec - m_OutputAbsStart -
				// (pConf->GetDecelMaxBumpCandLength() * m_dSmpFreq)))
				if (lZC[k + 1] <= (m_accCutoff - m_OutputAbsStart))
				{
					continue;
				}
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// if (lZC[k+1] <= (m_lastCommitIndex - m_OutputAbsStart)) // these bumps would
			// be final in last window - don't need to consider anew continue;
			double dBumpLength = lZC[k + 1] - lZC[k];
			if (dBumpLength < m_ConfigFHR.GetMinBumpCandLength() * m_dSmpFreq)
			{
				continue;
			}

			double dBumpArea = CalcBumpArea(lZC[k] - 1, lZC[k + 1] - 1);
			bool isAccel = dBumpArea > 0;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if ((isAccel && dBumpArea < pConf->GetAccelMinBumpArea()) || ((!isAccel) && fabs(dBumpArea) < pConf->GetDecelMinBumpArea()))
			{
				lSmallAreaBumpsRejected++;
				continue;
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// CHE - here check that not too long - should have accel and decel max length
			// Want to think about what to do with lastZCPos in event of a too long candidate
			// Can essentially advance a too long lastZCPos to inf
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			if ((isAccel && dBumpLength > (pConf->GetAccelMaxBumpCandLength() * m_dSmpFreq)))
			{
				if (lZC[k+1] <= lLastRealValidIndex)
				{
					dLastZCNeg[ind] = lZC[k + 1];	// can skip forward
				}
				continue;
			}

			if (((!isAccel) && dBumpLength > (pConf->GetDecelMaxBumpCandLength() * m_dSmpFreq)))
			{
				if (lZC[k+1] <= lLastRealValidIndex)
				{
					dLastZCPos[ind] = lZC[k + 1];	// can skip forward
				}
				continue;
			}
			
			long lEventBegin = lZC[k] + 1;
			long lEventEnd = lZC[k + 1];
			long lPeak;
			double dBumpHeight = BumpHeight(lEventBegin, lEventEnd, &lPeak);
			// There is no Bump Competition at this stage yet We are working in one BandPass
			// Filter and there is no overlapping bumps
			if (isAccel)
			{
				accel* a = new accel(lEventBegin, lEventEnd);
				a->setFreqBand(ind+1);
				a->setBpHeight(dBumpHeight);
				a->setBpPeak(lPeak);
				PositiveBumps->add(a);
			}
			else
			{
				decel* d = new decel(lEventBegin, lEventEnd);
				d->setFreqBand(ind+1);
				d->setBpHeight(dBumpHeight);
				d->setBpPeak(lPeak);
				NegativeBumps->add(d);
			}
		}

		// Now consider last ZC for purposes of updating dLastZCPos or dLastZCNeg - the
		// last ZC will correspond to the start of an as yet unfinished bum
		if (lZCPoints > 0)
		{
			if (lZC[lZCPoints - 1] <= lLastRealValidIndex)
			{
				if (m_pdSignalToProcess[lZC[lZCPoints - 1] - 1] > 0)
				{	// from pos to neg
					dLastZCPos[ind] = lZC[lZCPoints - 1];
				}
				else
				{	// from neg to pos
					dLastZCNeg[ind] = lZC[lZCPoints - 1];
				}
			}
		}

		// Here check if have hanging accel / decel for band 5/6 where can advance one of
		// lastZCPos or lastZCNeg because have uncompleted decel > 7.5 minutes or
		// uncompleted accel > 4.5 minutes that will just get tossed in ensuing window If
		// dLastZCPos > dLastZCNeg and is more than 7.5 minutes < m_lFiltered then can set
		// lastZCNeg to m_lFiltered - 1 If dLastZCNeg > dLastZCPos and is more than 4.5
		// minutes < m_lFiltered then can set lastZCPos to m_lFiltered - 1 m_lFiltered is
		// number of signal points in current band
		if (dLastZCPos[ind] > dLastZCNeg[ind])
		{
			if (dLastZCPos[ind] < (m_lFiltered - (pConf->GetDecelMaxBumpCandLength() * m_dSmpFreq)))
			{
				dLastZCNeg[ind] = m_lFiltered - 1;
			}
		}
		else
		{
			if (dLastZCNeg[ind] < (m_lFiltered - (pConf->GetAccelMaxBumpCandLength() * m_dSmpFreq)))
			{
				dLastZCPos[ind] = m_lFiltered - 1;
			}
		}
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::CalcBumpArea DESCRIPTION: This function calculates Bump Area PARAMETERS: Bump start and end RETURNS:
	// =======================================================================================================================
	//
	double CFHRSignal::CalcBumpArea(long l1, long l2)
	{
		/*~~~~~~~~~~~~~*/
		double dArea = 0;
		/*~~~~~~~~~~~~~*/

		for (long lc = l1; lc <= l2; lc++)
		{
			dArea += m_pdSignalToProcess[lc];
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dShiftArea = (l2 - l1 + 1) * (m_pdSignalToProcess[l1] + m_pdSignalToProcess[l2]) / 2.0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		dArea = dArea - dShiftArea;
		return dArea;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::BumpHeight DESCRIPTION: This function calculates Bump Max Height and its coordinate PARAMETERS: Bump
	//    start and end RETURNS:
	// =======================================================================================================================
	//
	/*double CFHRSignal::BumpHeight(long l1, long l2, long *lPeak)
	{
		/*~~~~~~~~~~~~*/
	/*	double dMax = 0;
		/*~~~~~~~~~~~~*/
/*
		for (long lc = l1; lc <= l2; lc++)
		{
			if (fabs(dMax) < fabs(m_pdSignalToProcess[lc]))
			{
				dMax = m_pdSignalToProcess[lc];
				*lPeak = lc;
			}
		}

		return dMax;
	}
*/
	/* Band pass domain height */
	double CFHRSignal::BumpHeight(long l1, long l2, long *lPeak)
	{
		/*~~~~~~~~~~~~*/
		double dMax = 0;
		/*~~~~~~~~~~~~*/

		for (long lc = l1; lc <= l2; lc++)
		{
			if (dMax < fabs(m_pdSignalToProcess[lc]))
			{
				dMax = fabs(m_pdSignalToProcess[lc]);
				*lPeak = lc;
			}
		}

		return dMax;
	}


	//
	// =======================================================================================================================
	//    CFHRSignal::addBumpIfHigher DESCRIPTION: This function adds to the Winners only the Candidate having the greatest
	//    height from all overlapped Bumps PARAMETERS: Bump start and end RETURNS:
	// =======================================================================================================================
	//
	void CFHRSignal::addBumpIfHigher(fhrPartSet *Bumps, bump *b)
	{
		bool bIsHighest = true;
		bump* x;
		long i, index1 = 0, index2 = -1;
		bool overlap = false;

		for (i = 0; i < Bumps->size(); i++)
		{
			x = (bump*) Bumps->getAt(i);
			if (x->intersects(*b))
			{
				if (overlap == false)
				{
					index1 = i; // index of first bump to remove if adding higher bump
				}
				overlap = true;
				index2 = i; // index of second bump to remove if adding higher bump
				if (b->getBpHeight() < x->getBpHeight())
				{
					bIsHighest = false;
					break;
				}
			}
			else if (x->getX1() < b->getX1())
			{
				index1 = i + 1;
			}
		}

		if (bIsHighest)
		{
			if (overlap)
			{
				Bumps->removeAt(index1, index2);
			}
			// need to explicitly cast
			if (b->isAccel())
			{
				Bumps->insertcopy((accel*) b, index1);
			}
			else
			{
				Bumps->insertcopy((decel*) b, index1);
			}
		}
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::addBumpAndReplace DESCRIPTION: This function adds the Bump and replaces all the overlapped Bumps
	//    PARAMETERS: RETURNS:
	// =======================================================================================================================
	//
	void CFHRSignal::addBumpAndReplace(fhrPartSet *Bumps, bump* b)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long i;
		long k = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (i = Bumps->size(); i >= 0; i--)
		{
			if (Bumps->getAt(i)->intersects(*b))
			{
				Bumps->removeAt(i);
			}
		}
		Bumps->add(b);
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::FreqBandDependentBumpMeasures DESCRIPTION: This function perfoms the measures using Band Pass filtered
	//    data PARAMETERS: RETURNS: ;
	//    Length: the time duration of the event Onset: the time (from the beginning to first 90% of the peak value)
	//    Recovery: the time (from the beginning to last 90% of the peak value) + 1 FhrBegin: the FHR values at the beginning
	//    of the event FhrEnd: the FHR values at the end of the event FhrStd: the standard deviation of the FHR over the
	//    event FhrInSlopeVal: the steepest slope during event onset FhrInSlopeTime: the time from the beginning to
	//    FhrInSlopeVal FhrOutSlopeVal: the steepest slope during event recovery FhrOutSlopeTime: the time from the beginning
	//    to FhrOutSlopeVal MaxHeight: the difference between the average of FhrBegin, FhrEnd and the peak FHR MeanHeight:
	//    the difference between the average of FhrBegin, FhrEnd and the mean FHR Area: the sum of differences between the
	//    mean FHR and the FHR at each event sample ContractionBegin: the time elapsed since the onset of the most recent
	//    contraction ContractionEnd: the time elapsed since the end of the most recent contraction the segment begins at the
	//    beginning of the previous baseline and ends at the end of the following baseline. The required signals are: shift
	//    all index offsets to be relative to the beginning of the previous baseline
	// =======================================================================================================================
	//
	void CFHRSignal::FreqBandDependentBumpMeasures(const double *dFiltered, decel *d)
	{
		long lStart = d->getX1();
		long lEnd = d->getX2();
		long lPeak = d->getBpPeak();

		double dHeight = d->getBpHeight(); // always +ve even if decel
		double dRec = .9 * dHeight;	// time from xBegin to 90% of the Band pass filtered peak value
		long lc;
		// time from xBegin to 90% of the Band pass filtered peak value
		long lOnset = 0;
		long lRecovery = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (lc = lStart - 1; lc <= lPeak - 1; lc++)
		{
			if (fabs(dFiltered[lc]) >= dRec)
			{
				lOnset = lc;
				break;
			}
		}

		for (lc = lPeak - 1; lc < lEnd - 1; lc++)
		{
			if (fabs(dFiltered[lc]) >= dRec)
			{
				lRecovery = lc + 1;
			}
		}

		d->getBM()->setOnset(lOnset - lStart + 2);
		d->getBM()->setRecovery(lRecovery - lStart + 1);

		/*~~~~~~~~~~~~~~~~*/
		double dInSlope = 0, dOutSlope = 0;
		double diff;
		long fhrInSlopeTime = 0, fhrOutSlopeTime = 0;
		/*~~~~~~~~~~~~~~~~*/

		for (lc = lStart - 1; lc < lPeak - 1; lc++)
		{
			diff = fabs(dFiltered[lc + 1] - dFiltered[lc]);
			if (dInSlope < diff)
			{
				dInSlope = diff;
				fhrInSlopeTime = lc - lStart + 3;
			}
		}

		d->getBM()->setFhrInSlopeTime(fhrInSlopeTime);
		d->getBM()->setFhrInSlopeVal(dInSlope);

		for (lc = lPeak - 1; lc < lEnd - 1; lc++)
		{
			diff = fabs(dFiltered[lc + 1] - dFiltered[lc]);
			if (dOutSlope < diff)
			{
				dOutSlope = diff;
				fhrOutSlopeTime = lEnd - lc - 2;
			}
		}

		d->getBM()->setFhrOutSlopeTime(fhrOutSlopeTime);
		d->getBM()->setFhrOutSlopeVal(dOutSlope);
	}


	//
	// =======================================================================================================================
	//    CFHRSignal::BumpCompetition DESCRIPTION: This function adds Bumps from a Freq band to the growing array of winners
	//    from the previous Freq bands PARAMETERS: RETURNS:
	// =======================================================================================================================
	//
	void CFHRSignal::BumpCompetition(fhrPartSet *BumpsTarget, fhrPartSet *BumpsSource, bool isAccel)
	{
		for (long i = 0; i < BumpsSource->size(); i++)
		{
			if (isAccel)
			{
				addBumpIfHigher(BumpsTarget, (accel*) BumpsSource->getAt(i));
			}
			else
			{
				addBumpIfHigher(BumpsTarget, (decel*) BumpsSource->getAt(i));
			}
		}
	}
	
	//
	// =======================================================================================================================
	//    CFHRSignal::RemoveNonEvents:  remove bumps classified as non decel or non accel
	// =======================================================================================================================
	//
	void CFHRSignal::RemoveNonEvents(fhrPartSet *f)
	{

		for (long i = f->size()-1 ; i >= 0; i--)
		{
			if (!(f->getAt(i)->isEvent()))
			{
				f->removeAt(i);
			}
		}
	}

	//
	// =======================================================================================================================
	//    CFHRSignal: GetEarliestX1InNextWindow Given a cutoff time lMaxEnd, determine the earliest start time of a bump that
	//    ends after lMaxEnd This assumes that BumpsIn are sorted based on endTime
	// =======================================================================================================================
	//
	long CFHRSignal::GetEarliestX1InNextWindow(fhrPartSet *BumpsIn, long lMaxEnd)
	{
		/*~~~~~~~~~~~~~~~~~*/
		long minX1 = lMaxEnd;
		long i = BumpsIn->size() - 1;
		/*~~~~~~~~~~~~~~~~~*/

		while ((i >= 0) && (BumpsIn->getAt(i)->getX2() > lMaxEnd))
		{
			minX1 = min(minX1, BumpsIn->getAt(i)->getX1());
			i--;
		}

		return minX1;
	}
	
	
	



	//
	// =======================================================================================================================
	//    CFHRSignal::IsDegenerateFhrPart - check any truncated part p to see if should be kept
	// =======================================================================================================================
	//
	bool CFHRSignal::IsDegenerateFhrPart(fhrPart *p)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long LENGTH_THRESHOLD = m_ConfigFHR.GetLENGTH_THRESHOLD();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (p->isBaseline())
		{
			baseline *b = (baseline *) p;
			if (b->getX1() < 0)
				b->setX1(0);
			if (b->length() < m_ConfigFHR.GetMinBaseLineLength())
				return true;
			else
			{
				GetBaselinePolyfit(b); // recalc y1, y2 values
				ComputeMean(b);
			}
		}

		if (p->length() < LENGTH_THRESHOLD)
		{
			return true;
		}
		
		if ((p->isAccel()) && (!(IsAlreadyCommitted(p))))
		{ 
			
			if (p->getX1() >= 0)
			{  // reclassify
				accel *a = (accel *) p;
				a->getBM()->reset();
				if (PreprocessAccel(a))
				{
					fhrPartSet dummy;
					ClassifyAccel(a, &dummy, NULL);
					return(!(a->isAccel()));
				}
			}
			else
			{
				return(true); // must be very long accel - reject
			}
		}
		else if ((p->isDecel() && (!(IsAlreadyCommitted(p)))))
		{	
			if (p->getX1() >= 0)
			{
				decel *d = (decel *) p;
		//	d->getBM()->reset(); // can't reset because freq. dep. bump measures cannot be remeasured
				ClassifyDecel(d, NULL);
				SubtypeProlonged(d);
				return(!(d->isDecel()));
			}
			else
			{
				return false; // should not happen but may have cases where have extremely long decel - want to keep
			}
		}

		return false;
	}

	//
	// ======================================================================================================================
	//	IsAlreadyCommitted - check to see if already committed based on type and lastCommitX2.  This is needed in special 
	//                     cases during the merge w/ very long events
	// =======================================================================================================================
	bool CFHRSignal::IsAlreadyCommitted(fhrPart *p)
	{
		long lastCommitX2;
		bool b;

		if (p->isBaseline())
			lastCommitX2 = m_lLastCommittedBasX2;
		else if (p->isAccel())
			lastCommitX2 = m_lLastCommittedAccelX2;
		else
			lastCommitX2 = m_lLastCommittedDecelX2;

		if (p->isAbsCoords())
		{
			b = p->getX2() <= lastCommitX2;
		}
		else
		{ 
			b = p->getX2() + m_OutputAbsStart <= lastCommitX2;
		}

		return(b);
	}
	//
	// =======================================================================================================================
	//    CFHRSignal::SubtypeDecel - subtype gradual decels based on timing relationship with contractions
	// =======================================================================================================================
	//
	bool CFHRSignal::SubtypeDecel(decel *d)
	{
		// NOTE: subtype prolonged decel seperately
		long lContrIndex;
		if (!(d->getBM()->isMeasured()))
		{
			lContrIndex = GetContractionIndex(d->getX1(), d->getX2());
		}
		else
		{
			lContrIndex = d->getContrIndex();
		}

		/*~~~~~~~~~~~~~~~~~~~~~~*/
		long contractionBegin = 0;
		long contractionEnd = 0;
		long contractionPeak = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		if (lContrIndex > -1)
		{
			contractionBegin = m_ContractDetect[lContrIndex].lStart - d->getX1();
			contractionEnd = m_ContractDetect[lContrIndex].lEnd - d->getX2();
			contractionPeak = m_ContractDetect[lContrIndex].lPeak - d->getPeak();
		}

		if (d->getType() != fhrPart::DecType_VAR)
		{
			if (IsUnassociatedDecel(d))
			{
				d->setSubtype(fhrPart::DecType_NONASSOC);
			}
			else if (contractionBegin <= m_ConfigFHR.GetBEFORE_CONTRACT_THRESHOLD())
			{
				if (contractionEnd >= 0 || (contractionPeak >= -m_ConfigFHR.GetBEFORE_CONTRACT_THRESHOLD() && contractionEnd >= -m_ConfigFHR.GetLATE_THRESHOLD()))
				{
					d->setSubtype(fhrPart::DecType_EARLY);
				}
				else
				{
					d->setSubtype(fhrPart::DecType_LATE);
					d->setLag(d->getX2() - m_ContractDetect[lContrIndex].lEnd);
					d->setLate(true);
				}
			}
			else
			{
				d->setSubtype(fhrPart::DecType_NONASSOC); // this covers former type INDETERMINATE - now bunch in w/ non-assoc
			}
		}

		return true;
	}

	//
	// =======================================================================================================================
	// =======================================================================================================================
	//
	bool CFHRSignal::SubtypeProlonged(decel *d)
	{
		bool bRC = false;
		if (d->length() >= m_ConfigFHR.MinProlongedLength())
		{
			d->setSubtype(fhrPart::DecType_PROL);
			bRC = true;
		}

		return(bRC);
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::IsUnassociatedDecel DESCRIPTION: This function checks if the Decel is associated with a Contraction
	//    PARAMETERS: RETURNS:
	// =======================================================================================================================
	//
	bool CFHRSignal::IsUnassociatedDecel(decel *d)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long x1 = d->getX1();
		long x2 = d->getX2();
		long x3 = d->getPeak();
		long length = d->length();
		long cIndex = d->getContrIndex();
		bool bRC = (cIndex == -1);
		long c1; 
		long c2;
		long cp;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (!bRC)
		{
			c1 = m_ContractDetect[cIndex].lStart;
			c2 = m_ContractDetect[cIndex].lEnd;
			cp = m_ContractDetect[cIndex].lPeak;
			
			if (cp > x1 && cp <= x2 + m_ConfigFHR.GetLATE_THRESHOLD())
			{
				bRC = false;  // associated
			}
			else if (c1 > x2 || c2 < x1) 
			{
				bRC = true; // no overlap w/ contraction
			}
			else
			{
				bRC = false;
			}
		}

		return bRC;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CFHRSignal::Calculate_FeaturesPrimary(baseline *pBL[5], void *pFile)
	{
		PatternsVector vFHR_BL(55);

		for (int iField = 0; iField < 55; iField++)
		{
			vFHR_BL(iField) = 0.0;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~*/
		long C0 = pBL[0]->midpt();
		long C1 = pBL[1]->midpt();
		long C2 = pBL[2]->midpt();
		long C3 = pBL[3]->midpt();
		long C4 = pBL[4]->midpt();
		/*~~~~~~~~~~~~~~~~~~~~~~~*/

		vFHR_BL(0) = pBL[2]->getMoveAvgMid();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long C34 = (long) round((double) (pBL[3]->getX2() + pBL[4]->getX1()) / 2.0);
		long C23 = (long) round((double) (pBL[2]->getX2() + pBL[3]->getX1()) / 2.0);
		long C12 = (long) round((double) (pBL[1]->getX2() + pBL[2]->getX1()) / 2.0);
		long C01 = (long) round((double) (pBL[0]->getX2() + pBL[1]->getX1()) / 2.0);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		vFHR_BL(22) = pBL[1]->getY1();
		vFHR_BL(6) = pBL[2]->getY1();
		vFHR_BL(14) = pBL[3]->getY1();
		vFHR_BL(28) = pBL[4]->getY1();

		vFHR_BL(33) = pBL[0]->getY2();
		vFHR_BL(23) = pBL[1]->getY2();
		vFHR_BL(7) = pBL[2]->getY2();
		vFHR_BL(15) = pBL[3]->getY2();

		vFHR_BL(34) = pBL[0]->length() - 1;				// to be consistent w/ MATLAB
		vFHR_BL(24) = pBL[1]->length() - 1;
		vFHR_BL(8) = pBL[2]->length() - 1;
		vFHR_BL(16) = pBL[3]->length() - 1;
		vFHR_BL(29) = pBL[4]->length() - 1;

		vFHR_BL(52) = pBL[1]->getX1() - pBL[0]->getX2();
		vFHR_BL(44) = pBL[2]->getX1() - pBL[1]->getX2();
		vFHR_BL(38) = pBL[3]->getX1() - pBL[2]->getX2();
		vFHR_BL(48) = pBL[4]->getX1() - pBL[3]->getX2();

		vFHR_BL(53) = pBL[1]->getY1() - pBL[0]->getY2();
		vFHR_BL(45) = pBL[2]->getY1() - pBL[1]->getY2();
		vFHR_BL(39) = pBL[3]->getY1() - pBL[2]->getY2();
		vFHR_BL(49) = pBL[4]->getY1() - pBL[3]->getY2();

		vFHR_BL(30) = pBL[0]->getVarPassMid();			
		vFHR_BL(17) = pBL[1]->getVarPassMid();			
		vFHR_BL(1) = pBL[2]->getVarPassMid();			
		vFHR_BL(9) = pBL[3]->getVarPassMid();			
		vFHR_BL(25) = pBL[4]->getVarPassMid();			

		vFHR_BL(40) = pBL[2]->getVarPassMidToNext(); 
		vFHR_BL(46) = pBL[1]->getVarPassMidToNext(); 
		vFHR_BL(50) = pBL[3]->getVarPassMidToNext(); 
		vFHR_BL(54) = pBL[0]->getVarPassMidToNext(); 

		vFHR_BL(31) = pBL[0]->getYmean();					
		vFHR_BL(18) = pBL[1]->getYmean();					
		vFHR_BL(2) = pBL[2]->getYmean();					
		vFHR_BL(10) = pBL[3]->getYmean();					
		vFHR_BL(26) = pBL[4]->getYmean();					

		vFHR_BL(51) = pBL[0]->getMeanBtwnNext();		
		vFHR_BL(41) = pBL[1]->getMeanBtwnNext();		
		vFHR_BL(35) = pBL[2]->getMeanBtwnNext();		
		vFHR_BL(47) = pBL[3]->getMeanBtwnNext();		

		vFHR_BL(19) = pBL[1]->getYmax();					
		vFHR_BL(3) = pBL[2]->getYmax();					
		vFHR_BL(11) = pBL[3]->getYmax();					

		vFHR_BL(42) = pBL[1]->getMaxBtwnNext();		
		vFHR_BL(36) = pBL[2]->getMaxBtwnNext();		

		vFHR_BL(20) = pBL[1]->getYmin();					
		vFHR_BL(4) = pBL[2]->getYmin();					
		vFHR_BL(12) = pBL[3]->getYmin();					

		vFHR_BL(43) = pBL[1]->getMinBtwnNext();		
		vFHR_BL(37) = pBL[2]->getMinBtwnNext();		

		vFHR_BL(32) = pBL[0]->getMhVar();					
		vFHR_BL(21) = pBL[1]->getMhVar();					
		vFHR_BL(5) = pBL[2]->getMhVar();					
		vFHR_BL(13) = pBL[3]->getMhVar();					
		vFHR_BL(27) = pBL[4]->getMhVar();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dRes = SimulatePrimaryNN(vFHR_BL);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		try
		{
			if (pFile)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~*/
				CString sFeature_Statistic;
				CString sFeature_Stat;
				/*~~~~~~~~~~~~~~~~~~~~~~~*/

				sFeature_Statistic.Format(_T("%d\t%d\t1\t%d\t%f\t", pBL[2]->getX1() + m_OutputAbsStart, pBL[2]->getX2() + m_OutputAbsStart, m_nTotalCount, dRes));
				for (int iField = 0; iField < 55; iField++)
				{
					sFeature_Stat.Format(_T("%f\t", vFHR_BL(iField)));
					sFeature_Statistic += sFeature_Stat;
				}

				if (m_ConfigFHR.m_bStoreXML)
				{
					((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
				}

				sFeature_Statistic = "";
				for (int i = 0; i < 5; i++)
				{
					sFeature_Stat.Format(_T("%d\t", pBL[i]->midpt()));
					sFeature_Statistic += sFeature_Stat;
				}

				sFeature_Statistic += "\n";
				if (m_ConfigFHR.m_bStoreXML)
				{
					((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
				}
			}
		}

		catch(...)
		{
		}
		return dRes;
	}

	double CFHRSignal::Calculate_FeaturesSecondary(baseline *pBL[5], void *pFile)
	{
		/*~~~~~~~~~~~~~~~*/
		PatternsVector vFHR_BL(55);
		/*~~~~~~~~~~~~~~~*/

		for (int iField = 0; iField < 55; iField++)
		{
			vFHR_BL(iField) = 0.0;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~*/
		long C0 = pBL[0]->midpt();
		long C1 = pBL[1]->midpt();
		long C2 = pBL[2]->midpt();
		long C3 = pBL[3]->midpt();
		long C4 = pBL[4]->midpt();
		/*~~~~~~~~~~~~~~~~~~~~~~~*/

		vFHR_BL(0) = pBL[4]->getMoveAvgMid();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long C34 = (long) round((double) (pBL[3]->getX2() + pBL[4]->getX1()) / 2.0);
		long C23 = (long) round((double) (pBL[2]->getX2() + pBL[3]->getX1()) / 2.0);
		long C12 = (long) round((double) (pBL[1]->getX2() + pBL[2]->getX1()) / 2.0);
		long C01 = (long) round((double) (pBL[0]->getX2() + pBL[1]->getX1()) / 2.0);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		vFHR_BL(6) = pBL[4]->getY1();
		vFHR_BL(14) = pBL[3]->getY1();
		vFHR_BL(22) = pBL[2]->getY1();
		vFHR_BL(28) = pBL[1]->getY1();

		vFHR_BL(7) = pBL[4]->getY2();
		vFHR_BL(15) = pBL[3]->getY2();
		vFHR_BL(23) = pBL[2]->getY2();
		vFHR_BL(33) = pBL[0]->getY2();

		vFHR_BL(8) = pBL[4]->length() - 1;				// to be consistent w/ MATLAB training
		vFHR_BL(16) = pBL[3]->length() - 1;
		vFHR_BL(24) = pBL[2]->length() - 1;
		vFHR_BL(29) = pBL[1]->length() - 1;
		vFHR_BL(34) = pBL[0]->length() - 1;

		vFHR_BL(38) = pBL[4]->getX1() - pBL[3]->getX2();
		vFHR_BL(44) = pBL[3]->getX1() - pBL[2]->getX2();
		vFHR_BL(48) = pBL[2]->getX1() - pBL[1]->getX2();
		vFHR_BL(52) = pBL[1]->getX1() - pBL[0]->getX2();

		vFHR_BL(39) = pBL[4]->getY1() - pBL[3]->getY2();
		vFHR_BL(45) = pBL[3]->getY1() - pBL[2]->getY2();
		vFHR_BL(49) = pBL[2]->getY1() - pBL[1]->getY2();
		vFHR_BL(53) = pBL[1]->getY1() - pBL[0]->getY2();

		vFHR_BL(1) = pBL[4]->getVarPassMid();			
		vFHR_BL(9) = pBL[3]->getVarPassMid();			
		vFHR_BL(17) = pBL[2]->getVarPassMid();			
		vFHR_BL(25) = pBL[1]->getVarPassMid();			
		vFHR_BL(30) = pBL[0]->getVarPassMid();			

		vFHR_BL(40) = pBL[3]->getVarPassMidToNext(); 
		vFHR_BL(46) = pBL[2]->getVarPassMidToNext(); 
		vFHR_BL(50) = pBL[1]->getVarPassMidToNext(); 
		vFHR_BL(54) = pBL[0]->getVarPassMidToNext(); 

		vFHR_BL(2) = pBL[4]->getYmean();					
		vFHR_BL(10) = pBL[3]->getYmean();					
		vFHR_BL(18) = pBL[2]->getYmean();					
		vFHR_BL(26) = pBL[1]->getYmean();					
		vFHR_BL(31) = pBL[0]->getYmean();					

		vFHR_BL(35) = pBL[3]->getMeanBtwnNext();		
		vFHR_BL(41) = pBL[2]->getMeanBtwnNext();		
		vFHR_BL(47) = pBL[1]->getMeanBtwnNext();		
		vFHR_BL(51) = pBL[0]->getMeanBtwnNext();		

		vFHR_BL(3) = pBL[4]->getYmax();					
		vFHR_BL(11) = pBL[3]->getYmax();					
		vFHR_BL(19) = pBL[2]->getYmax();					

		vFHR_BL(36) = pBL[3]->getMaxBtwnNext();		
		vFHR_BL(42) = pBL[2]->getMaxBtwnNext();		

		vFHR_BL(4) = pBL[4]->getYmin();					
		vFHR_BL(12) = pBL[3]->getYmin();					
		vFHR_BL(20) = pBL[2]->getYmin();					

		vFHR_BL(37) = pBL[3]->getMinBtwnNext();		
		vFHR_BL(43) = pBL[2]->getMinBtwnNext();		

		vFHR_BL(5) = pBL[4]->getMhVar();					
		vFHR_BL(13) = pBL[3]->getMhVar();					
		vFHR_BL(21) = pBL[2]->getMhVar();					
		vFHR_BL(27) = pBL[1]->getMhVar();					
		vFHR_BL(32) = pBL[0]->getMhVar();

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dRes = SimulateSecondaryNN(vFHR_BL);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		try
		{
			if (pFile)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~*/
				CString sFeature_Statistic;
				CString sFeature_Stat;
				/*~~~~~~~~~~~~~~~~~~~~~~~*/

				sFeature_Statistic.Format(_T("%d\t%d\t2\t%d\t%f\t", pBL[4]->getX1() + m_OutputAbsStart, pBL[4]->getX2() + m_OutputAbsStart, m_nTotalCount, dRes));
				for (int iField = 0; iField < 55; iField++)
				{
					sFeature_Stat.Format(_T("%f\t", vFHR_BL(iField)));
					sFeature_Statistic += sFeature_Stat;
				}

				if (m_ConfigFHR.m_bStoreXML)
				{
					((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
				}

				sFeature_Statistic = "";
				for (int i = 0; i < 5; i++)
				{
					sFeature_Stat.Format(_T("%d\t", pBL[i]->midpt()));
					sFeature_Statistic += sFeature_Stat;
				}

				sFeature_Statistic += "\n";
				if (m_ConfigFHR.m_bStoreXML)
				{
					((CStdioFile *) pFile)->WriteString(sFeature_Statistic);
				}
			}
		}

		catch(...)
		{
		}
		return dRes;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	bool CFHRSignal::PrepareProceed(void)
	{
		if (m_FlushMutex)
		{	// special analysis of most last chunk >>
			if (m_S2A > 0)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				long j(0);
				unsigned char d = m_pucS2A[m_S2A - 1 - j];
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				while (d < m_ConfigFHR.GetMinAmplitude() || d > m_ConfigFHR.GetMaxAmplitude())
				{
					j++;
					d = m_pucS2A[m_S2A - 1 - j];
					if (m_S2A - 1 < j)	// ALL NON VALID!
					{
						if (!m_bSetMode && m_MessageLevel > 1)
						{				// AppendMode!
							printf("No valid data in the last chunk\n");
						}

						if (m_pCallBackFunction)
						{
							(*m_pCallBackFunction) (-400, m_pData);
						}

						return false;
					}
				}

				m_S2A -= j;
			}
			else
			{
				return false;			// no data to proceed!
			}
		}					// special analysis of most last chunk <<

		if (!m_bSetMode)	// AppendMode!
		{
			//m_ThreadMutex = true;		
			if (m_pucS2Aloc)
			{
				delete [] m_pucS2Aloc;
			}
			m_S2Aloc = m_S2A;
			m_pucS2Aloc = m_pucS2A;
			m_pucS2A = NULL;
			m_S2A = 0;

			if (m_pdC2Aloc)
			{
				delete [] m_pdC2Aloc;
			}

			m_C2Aloc = m_C2A;
			m_pdC2Aloc = m_pdC2A;
			m_pdC2A = NULL;
			m_C2A = 0;

			m_LastAppend = 0;
		}
		m_ThreadMutex = true;
		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::Proceed(void)
	{

		// main loop.  take data from m_pucS2Aloc in chunks in cases where a lot of data appended at once to limit memory usage
		long sIndex = 0;
		bool bRet = true;
		while (sIndex < m_S2Aloc)
		{
			m_OutputAbsStart = m_NextOutputAbsStart;
			long nAppend;
			
			if (m_rebootMode || m_synchroneCalculation)
			{
				m_rebootMode = false;
				nAppend = m_S2Aloc - sIndex;
			}
			else
			{
				nAppend = min(GetMaxSamplesAppend(), m_S2Aloc - sIndex);
			}
			
			if (m_pdSignal)
			{
				delete[] m_pdSignal;
			}

			m_nPtsCount = nAppend + m_S2K + m_S4F + GetNumExtrapolatedSignal();
			m_nTotalCount += nAppend;

			m_bLastAppend = (m_nTotalCount == m_nTotalAppend);
			SetCurrContractions(); // populates m_ContractDetect

			m_pdSignal = new double[m_nPtsCount];
			memset(m_pdSignal, 0, m_nPtsCount * sizeof(double));

			if (m_S4F > 0)
			{
				memcpy(m_pdSignal, m_pdS4F, m_S4F * sizeof(double));
			}

			if (m_S2K > 0)
			{
				memcpy(&m_pdSignal[m_S4F], m_pdS2K, m_S2K * sizeof(double));
			}

			if (nAppend > 0)
			{
				// Cannot use a memcpy since m_pdSignal is a double* and m_pucS2Aloc is a unsigned char*
				for (long i = 0; i < nAppend; i++)
				{
					m_pdSignal[i + m_S4F + m_S2K] = (double)(m_pucS2Aloc[sIndex + i]);
				}
			}
			m_bSignalIsRepaired = false;

			/*~~~~~~~~~~~~~~~~~~~~~~*/

			// R&D flags - not used if false. Allow to accelerate process for long repaired
			// segments
			bool bCleanRepair = false;
			bool bCleanFirst = false;
			bool bClean = false;
			/*~~~~~~~~~~~~~~~~~~~~~~*/

			try
			{
				// bRet = true;
				bRet = RepairSignal();
				AddExtrapolatedSignal();
			}

			catch(...)
			{
				bRet = false;
			}

			if (!bRet || !m_ThreadMutex)
			{
				if (m_pCallBackFunction)
				{
					(*m_pCallBackFunction) (-100, m_pData);
				}

				if (!m_bSetMode)
				{
					if (m_MessageLevel > 0)
					{
						printf("RepairSignal exception\n");
					}

					if (bCleanRepair && m_bCatchException)
					{
						m_FirstRun = true;
						if (bClean)
						{
							CleanMemory();
						}
					}

					m_bCatchException = true;
				}
				m_ThreadMutex = false;
				return false;
			}

			if (!m_bSetMode)
			{
				StoreTail();	// ce - moved to after repair so store already repaired signal for next run
			}

			try
			{
				// bRet = true;
				bRet = LowPass();
			}

			catch(...)
			{
				bRet = false;
			}

			if (!bRet || !m_ThreadMutex)
			{
				if (m_pCallBackFunction)
				{
					(*m_pCallBackFunction) (-101, m_pData);
				}

				if (!m_bSetMode)
				{
					if (m_MessageLevel > 0)
					{
						printf("LowPass exception\n");
					}

					if (bCleanFirst)
					{
						m_FirstRun = true;
						if (bClean)
						{
							CleanMemory();
						}
					}

					m_bCatchException = true;
				}
				m_ThreadMutex = false;
				return false;
			}

			try
			{
				// bRet = true;
				bRet = BandPass();
			}

			catch(...)
			{
				bRet = false;
			}

			if (!bRet || !m_ThreadMutex)
			{
				if (m_pCallBackFunction)
				{
					(*m_pCallBackFunction) (-102, m_pData);
				}

				if (!m_bSetMode)
				{
					if (m_MessageLevel > 0)
					{
						printf("BandPass exception\n");
					}

					if (bCleanFirst)
					{
						m_FirstRun = true;
						if (bClean)
						{
							CleanMemory();
						}
					}

					m_bCatchException = true;
				}
				m_ThreadMutex = false;
				return false;
			}

			try
			{
				// bRet = true;
				bRet = MinVarPass();
			}

			catch(...)
			{
				bRet = false;
			}

			if (!bRet || !m_ThreadMutex)
			{
				if (m_pCallBackFunction)
				{
					(*m_pCallBackFunction) (-103, m_pData);
				}

				if (!m_bSetMode)
				{
					if (m_MessageLevel > 0)
					{
						printf("MinVar exception\n");
					}

					if (bCleanFirst)
					{
						m_FirstRun = true;
						if (bClean)
						{
							CleanMemory();
						}
					}

					m_bCatchException = true;
				}
				m_ThreadMutex = false;
				return false;
			}

			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (2, m_pData);
			}

			if (!m_bSetMode && m_MessageLevel > 1)
			{
				printf("MinVar done\n");
			}

			try
			{
				// bRet = true;
				bRet = DetectBaseline();
			}

			catch(...)
			{
				bRet = false;
			}

			if (!bRet || !m_ThreadMutex)
			{
				if (m_pCallBackFunction)
				{
					(*m_pCallBackFunction) (-104, m_pData);
				}

				if (!m_bSetMode)
				{
					if (m_MessageLevel > 0)
					{
						printf("DetectBaseline exception\n");
					}

					if (bCleanFirst)
					{
						m_FirstRun = true;
						if (bClean)
						{
							CleanMemory();
						}
					}

					m_bCatchException = true;
				}
				m_ThreadMutex = false;
				return false;
			}

			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (3, m_pData);
			}

			try
			{
				//bRet = true;
				bRet = MultiHypothesis();
				//bRet = BP2Processing();
			}

			catch(...)
			{
				bRet = false;
			}

			if (!bRet || !m_ThreadMutex)
			{
				if (m_pCallBackFunction)
				{
					(*m_pCallBackFunction) (-105, m_pData);
				}

				if (!m_bSetMode)
				{
					if (m_MessageLevel > 0)
					{
						printf("MultiHypothesis exception\n");
					}

					if (bCleanFirst)
					{
						m_FirstRun = true;
						if (bClean)
						{
							CleanMemory();
						}
					}

					m_bCatchException = true;
				}
				m_ThreadMutex = false;
				return false;
			}

			if (m_pCallBackFunction)
			{
				(*m_pCallBackFunction) (4, m_pData);
			}

			if (m_iOnlyBaseLines > 0 && !m_bSetMode && m_MessageLevel > 1)
			{					// AppendMode!
				printf("MultiHypothesis done\n");
			}

			// m_iOnlyBaseLines = 1 - use already calculated and passed baselines and perform
			// all next steps: MH and AD m_iOnlyBaseLines = 2 - old mode - calculate baselines
			// and perform all next steps: MH and AD
			if (m_iOnlyBaseLines > 0)
			{
				try
				{
					//bRet = true;
					bRet = AccelDecel();
				}

				catch(...)
				{
					bRet = false;
				}
			}
			else
			{
			}

			if (!bRet || !m_ThreadMutex)
			{
				if (m_pCallBackFunction)
				{
					(*m_pCallBackFunction) (-106, m_pData);
				}

				if (!m_bSetMode)
				{
					if (m_MessageLevel > 0)
					{
						printf("AccelDecel exception\n");
					}

					if (bCleanFirst)
					{
						m_FirstRun = true;
						if (bClean)
						{
							CleanMemory();
						}
					}

					m_bCatchException = true;
				}

				m_ThreadMutex = false;
				return false;
			}

			if (m_pCallBackFunction) 
			{
				(*m_pCallBackFunction) (5, m_pData);
			}

			m_bCatchException = false;

			if (m_bNonDisplayMode)
			{
				CleanMemory();
			}

			sIndex += nAppend;
		}

		m_ThreadMutex = false;

		return bRet;
	}

	//
	// =======================================================================================================================
	//    APPEND SIGNAL = 0, APPEND CONTRACTION = 1, MinVar = 2, BaseLine = 3, MultiHypothesis = 4, AccelDdecel = 5
	// =======================================================================================================================
	//
	void RunProcess(void *p)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		bool bRet = ((CFHRSignal *) p)->Proceed();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		EnterCriticalSection(&((CFHRSignal *) p)->m_csProcess);
		((CFHRSignal *) p)->ThreadHandle = NULL;
		LeaveCriticalSection(&((CFHRSignal *) p)->m_csProcess);
		_endthread();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool CFHRSignal::Process(void)
	{
		/*~~~~~~~~~~*/
		bool r = true;
		/*~~~~~~~~~~*/

		EnterCriticalSection(&m_csProcess);
		if (ThreadHandle == 0)
		{
			ThreadHandle = _beginthread(RunProcess, 0, (void *) this);
			if (ThreadHandle == 0)
			{
				r = false;
			}
		}

		LeaveCriticalSection(&m_csProcess);
		return r;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	ContractionDetection *CFHRSignal::CreateContractionDetection(void)
	{
		if (m_ContractDetect)
		{
			delete[] m_ContractDetect;
		}

		m_ContractDetect = new ContractionDetection[1];
		return m_ContractDetect;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double CFHRSignal::BaselineVarMin(baseline *pBaseLine)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dRes = DBL_MAX;
		// double dRes = m_pdVarPassSignal[m_nVariabilityCount - 1];
		long lEndIndex = min(m_nVariabilityCount - 1, pBaseLine->getX2());
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (lEndIndex < pBaseLine->getX1())
		{
			dRes = m_pdVarPassSignal[m_nVariabilityCount - 1];
		}

		for (long k = pBaseLine->getX1(); k <= lEndIndex; k++)
		{
			dRes = min(dRes, fabs(m_pdVarPassSignal[k]));
		}

		return dRes;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::RemoveNOB(void)
	{
		
		int nBaseLines = m_pBaseLinesFinal.size();

		for (int i = nBaseLines - 1; i >= 0; i--)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			baseline *pBaseLine = (baseline *) m_pBaseLinesFinal.getAt(i);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (!(pBaseLine->getMhClass())) // not multiH baseline
			{
				m_pBaseLinesFinal.removeAt(i);
			}
		}
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::OutputBandPass DESCRIPTION: This function outputs an xml file (for validation) with the filtered data
	//    PARAMETERS: seq number of the Frequency Band RETURNS: true if success
	// =======================================================================================================================
	//
	bool CFHRSignal::OutputBandPass(const int ind)	// seq number of the Frequency Band
	{
		if (!m_ConfigFHR.m_bStoreXML)
		{
			return true;
		}

		try
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			CAccelDecelBandPassConfig *pConf = m_ConfigFHR.GetBandPassConfig(ind);
			char buff[10];
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			_ltoa(m_nTotalCount, buff, 10);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string strOutFile = m_ConfigFHR.GetPathToTestFiles() + pConf->GetPathToSaveBandPass() + "_" + buff + ".xml";
			// string strOutFile = m_ConfigFHR.GetPathToTestFiles() + pConf->GetPathToSaveBandPass();
			string str = strOutFile;
			size_t ipos;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			while ((ipos = str.find_first_of("\\")) != string::npos)
			{
				str.replace(ipos, 1, "/");
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			int fh = _open(strOutFile.c_str(), _O_RDWR | _O_TEXT | _O_CREAT | _O_TRUNC, S_IREAD | _S_IWRITE);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (fh == -1)
			{
				return false;
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string sOut = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
			int bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			sOut = "<Evonium>\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			sOut = "<!--File: " + str + "-->\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			sOut = "<!--Band Pass-->\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());

			/*~~~~~~~~~*/
			char buf[20];
			/*~~~~~~~~~*/

			for (long index = 0; index < m_lFiltered; index++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				string s(_gcvt(m_pdSignalToProcess[index], 6, buf));
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				sOut = "   <sample>" + s + "</sample>\n";
				bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			}

			sOut = "</Evonium>\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			_close(fh);
		}

		catch(...)
		{
		}

		return true;
	}

	//
	// =======================================================================================================================
	//    CFHRSignal::OutputZeroCrossings DESCRIPTION: This function outputs an xml file (for validation) with zero crossing
	//    data PARAMETERS: RETURNS: true if success
	// =======================================================================================================================
	//
	bool CFHRSignal::OutputZeroCrossings
		(
		const long *lZC,	// pointer to data to output
		long lZCPoints,		// number of points
		int ind
		)						// seq number of the Frequency Band
	{
		if (!m_ConfigFHR.m_bStoreXML)
		{
			return true;
		}

		try
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			CAccelDecelBandPassConfig *pConf = m_ConfigFHR.GetBandPassConfig(ind);
			char buff[10];
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			_ltoa(m_nTotalCount, buff, 10);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string strOutFile = m_ConfigFHR.GetPathToTestFiles() + pConf->GetPathToSaveZeroCrossings() + "_" + buff + ".xml";
			// string strOutFile = m_ConfigFHR.GetPathToTestFiles() + pConf->GetPathToSaveZeroCrossings();
			string str = strOutFile;
			size_t ipos;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			while ((ipos = str.find_first_of("\\")) != string::npos)
			{
				str.replace(ipos, 1, "/");
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			int fh = _open(strOutFile.c_str(), _O_RDWR | _O_TEXT | _O_CREAT | _O_TRUNC, S_IREAD | _S_IWRITE);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (fh == -1)
			{
				return false;
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			string sOut = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
			int bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			sOut = "<Evonium>\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			sOut = "<!--File: " + str + "-->\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			sOut = "<!--Zero Crossings-->\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());

			/*~~~~~~~~~*/
			char buf[20];
			/*~~~~~~~~~*/

			for (long index = 0; index < lZCPoints; index++)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				string s(_ltoa(lZC[index], buf, 10));
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				sOut = "   <sample>" + s + "</sample>\n";
				bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			}

			sOut = "</Evonium>\n";
			bytes = _write(fh, sOut.c_str(), (int) sOut.length());
			_close(fh);
		}

		catch(...)
		{
		}

		return true;
	}	

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPartSet *CFHRSignal::GetResultObjects(void)
	{
		return (&m_OutputFhrPartSet);
	}

	/*
	=======================================================================================================================
	lock/unlock access to the total result array.
	=======================================================================================================================
	*/
	void CFHRSignal::LockTotalResultObjects(bool lock)
	{
		if (lock)
		{
			m_total_results_mutex.acquire();
		}
		else
		{
			m_total_results_mutex.release();
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	int CFHRSignal::GetTotalResultSize(void)
	{
		return m_TotalOutputFhrPartSet.size();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPartSet *CFHRSignal::GetTotalResults(void)
	{
		return (&m_TotalOutputFhrPartSet);
	}

	//
	// =======================================================================================================================
	//    Allows client to access baselines
	// =======================================================================================================================
	//
	fhrPartSet *CFHRSignal::GetNewBaseLines(void)
	{
		return (&m_pBaseLinesNewInWindow);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPartSet *CFHRSignal::GetMultiH(void)
	{
		return(&m_pBaseLinesFinal);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::SetMinSamplesAppend(long lMinSamplesAppend)
	{
		m_ConfigFHR.SetMinSamplesAppend(lMinSamplesAppend);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::SetMaxSamplesAppend(long lMaxSamplesAppend)
	{
		m_ConfigFHR.SetMaxSamplesAppend(lMaxSamplesAppend);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CFHRSignal::GetMinSamplesAppend(void)
	{
		return m_ConfigFHR.GetMinSamplesAppend();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long CFHRSignal::GetMaxSamplesAppend(void)
	{
		return m_ConfigFHR.GetMaxSamplesAppend();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::SetMessageLevel(int MsgLevel)
	{
		m_MessageLevel = MsgLevel;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::ProceedBuffer(void)
	{
		m_ProceedBuffer = false;
	}

	/*
	=======================================================================================================================
	ComputFhrStd::this is used for computing baseline variability computes std dev of pSignal from sample index lX1 to
	lX2
	=======================================================================================================================
	*/
	double CFHRSignal::ComputeFhrStd(double *pSignal, long lX1, long lX2)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		double dSampSum = 0.0;
		double dSampMean = 0.0;
		long lNumSamples = lX2 - lX1 + 1;
		long i;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (i = lX1; i <= lX2; i++)
		{
			dSampSum += pSignal[i];
		}

		dSampMean = (double) dSampSum / lNumSamples;

		dSampSum = 0.0;
		for (i = lX1; i <= lX2; i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			double dDiff = (double) pSignal[i] - dSampMean;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			dSampSum += (dDiff * dDiff);
		}

		return sqrt(double(dSampSum / lNumSamples));
	}

	/*
	=======================================================================================================================
	CalcBaselineVar Calculates the variability of one baseline, presented as an Output Object struct Offset is that
	between the Output Object indexing and that of the FHR signal. The variability is measured as follows: - remove
	lBasTrim (nominally 5 seconds) from each end of baseline to ensure not measuring baseline over end of beginning of
	decel/accel - rotate baseline along 'slope' defined by polyfit of baseline. This basically removes the slope from
	the variability calculation - segment resultant baseline into lSegLen (nominally 30 seconds) segments. If have a
	leftover segment of less than 1/2 SegLen then append to last segment, otherwise take as seperate segment - the
	variability of a given segment is 4xstd - the variability of the baseline is the weighted average of the segment
	variabilities (weighted average used because some segements can be slightly > or < than segLen
	=======================================================================================================================
	*/
	void CFHRSignal::CalcBaselineVar(fhrPart *f, long lOffset)
	{
		if (f->isBaseline())
		{
			baseline *b = (baseline*) f;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			bool bConsiderSlope = m_ConfigFHR.m_bConsiderSlopeForBasVar;
			long lSegLen = (long) GetBasSegLenForVarCalc();
			long lBasTrim = (long) GetBasTrimForVarCalc();
			double dTotVar = 0.0;
			double dSlope = 0.0;
			long lTotSamp = 0;
			long lCurrSamp = 0;
			long i = 0;
			// long lBasLen = pOO->lEnd - pOO->lStart + 1;
			long lBasLen = b->getX2() - b->getX1() + 1 - (2 * lBasTrim);
			long lX1 = b->getX1() - lOffset;

			// Security, baseline starting BEFORE the start of the current signal window
			if (lX1 + lBasTrim < 0)
				return;

			long lNumSeg = (long) round((double) lBasLen / lSegLen);
			long currX1;
			long currX2;
			double *pBasSignal = new double[lBasLen];
			double dSigOffset = 0;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			// remove DC component and slope from baseline signal
			if (bConsiderSlope)
			{					// dSlope = (double) (pOO->dY2 - pOO->dY1) / (lBasLen
				///1);
				///
				dSlope = (double) (b->getY2() - b->getY1()) / (b->getX2() - b->getX1());
			}

			dSigOffset = b->getY1() + (lBasTrim * dSlope);

			if (lNumSeg == 0)
			{
				lNumSeg = 1;	// for cases where the baseline when trimmed is less than half of lSegLen just measure variability of available samples
			}

			ASSERT((i + lX1 + lBasTrim >= 0) && (i + lX1 + lBasTrim < m_nPtsCount)); // LXP OUT OF BOUNDS

			for (i = 0; i < lBasLen; i++)
			{
				pBasSignal[i] = m_pdSignal[i + lX1 + lBasTrim] - dSigOffset;
				dSigOffset += dSlope;
			}

			for (i = 0; i < lNumSeg; i++)
			{
				currX1 = i * lSegLen;
				if (i == lNumSeg - 1)
				{				// last segment - take what is left
					currX2 = lBasLen - 1;
				}
				else
				{
					currX2 = currX1 + lSegLen - 1;
				}

				lCurrSamp = currX2 - currX1 + 1;
				dTotVar += (double) lCurrSamp * 4.0 * ComputeFhrStd(pBasSignal, currX1, currX2);
				lTotSamp += lCurrSamp;
			}

			b->setVar((double) dTotVar / lTotSamp);

			if (pBasSignal)
			{
				delete[] pBasSignal;
				pBasSignal = NULL;
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::CalcOutputBaselineVar(fhrPartSet *f, long lOffset)
	{
		for (long i = 0; i < f->size(); i++)
		{
			CalcBaselineVar(f->getAt(i), lOffset);
		}
	}

	/*
	==================================================================================
	Repopulate m_pBaselinesFinal based on already committed output and merge of all 
	baseline candidates with newly classified accels and decels.  Meant to generate
	reference baseline for second iteration of multi-iteration accel/decel classification
	==================================================================================
	*/
	void CFHRSignal::RegenBaseLinesFinalFromMerge(fhrPartSet &NegativeBumps, fhrPartSet &PositiveBumps)
	{
		fhrPartSet temp;
		long minX2 = m_OutputAbsStart - m_ConfigFHR.GetReqBaseLinesHistory();
		long lastx2 = -LONG_MAX;
		long i, startIndex;

		for (i = 0; i < m_TotalOutputFhrPartSet.size(); i++)
		{
			if (m_TotalOutputFhrPartSet.getAt(i)->getX2() > minX2)
			{
				temp.addcopy(m_TotalOutputFhrPartSet.getAt(i));
			}
		}
		
		if (temp.size() > 0)
		{
			long n = temp.size() - 1;
			while ((n >=0) && (!(temp.getAt(n)->isBaseline())))
			{
				n--;
			}
			if (n >= 0)
			{
				lastx2 = temp.getAt(n)->getX2();
			}
		}
		temp.toRelativeTime(m_OutputAbsStart);

		// now look at baselines that have not been committed yet and merge with newly classified bumps
		
		startIndex = m_pBaseLinesTotal.size();
		while ((startIndex > 0) && (m_pBaseLinesTotal.getAt(startIndex-1)->getX2() > lastx2))
		{
			startIndex--;
		}

		if (m_pBaseLinesTotal.size() > 0)
		{
			for (i = startIndex; i < m_pBaseLinesTotal.size(); i++)
			{
				baseline *b = new baseline;
				*b = *((baseline *) m_pBaseLinesTotal.getAt(i));
				b->toRelativeTime(m_OutputAbsStart);
				fhrPartSetMerge(&temp, b);
			}
		}
		
		// now add pending baselines
		for (i = 0; i < m_pBaseLinesPending.size(); i++)
		{
			baseline *b = new baseline;
			*b = *((baseline *) m_pBaseLinesPending.getAt(i));
			b->toRelativeTime(m_OutputAbsStart);
			fhrPartSetMerge(&temp, b);
		}

		temp.filterByType(fhrPart::FhrPartType_BAS);

		// now merge w/ Negative and PositiveBumps from previous and current windows that have not yet been committed
		for (i = 0; i < m_pAllPreMergeAccels.size(); i++)
		{
			if (m_pAllPreMergeAccels.getAt(i)->getX2() > m_lLastCommittedAccelX2)
			{
				accel *a = new accel;
				(*a) = *((accel *) (m_pAllPreMergeAccels.getAt(i)));
				a->toRelativeTime(m_OutputAbsStart);
				fhrPartSetMerge(&temp, a);
			}
		}

		for (i = 0; i < PositiveBumps.size(); i++)
		{
			accel *a = new accel;
			(*a) = *((accel *) (PositiveBumps.getAt(i)));
			fhrPartSetMerge(&temp, a);
		}
		temp.filterByType(fhrPart::FhrPartType_BAS); 

		for (i = 0; i < m_pAllPreMergeDecels.size(); i++)
		{
			if (m_pAllPreMergeDecels.getAt(i)->getX2() > m_lLastCommittedDecelX2)
			{
				decel *d = new decel;
				(*d) = *((decel *) (m_pAllPreMergeDecels.getAt(i)));
				d->toRelativeTime(m_OutputAbsStart);
				fhrPartSetMerge(&temp, d);
			}
		}

		for (i = 0; i < NegativeBumps.size(); i++)
		{
			decel *d = new decel;
			(*d) = *((decel *) (NegativeBumps.getAt(i)));
			fhrPartSetMerge(&temp, d);
		}
		temp.filterByType(fhrPart::FhrPartType_BAS);

		temp.nonBasToBas();

		m_pBaseLinesFinal.clear();
		temp.setClearMemory(false);
		m_pBaseLinesFinal.add(&temp);
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void CFHRSignal::AtypicalDecelClassify(fhrPartSet *p)
	{
		fhrPartSet decels;
		decel *d;
		CAtypicalClassify a;
		fhrPartSet repInts;

		decels.setClearMemory(false);

		for (long i = 0; i < p->size(); i++)
		{
			if (p->getAt(i)->isDecel())
			{
				d = (decel *) p->getAt(i);
				if (d->isVariable())
				{
					decels.add(d);
				}
			}
		}
	
		a.SetSampFreq(m_dSmpFreq);
		a.SetBumps(&decels);
		a.SetFHR(m_pdSignal, m_nPtsCount);
		a.SetLpFHR(m_pdLowPassSignal, m_nLowPassCount);
		a.SetRefBaseLines(&m_pBaseLinesFinal); 
		repInts = m_pRepairIntervals;
		repInts.toRelativeTime(m_OutputAbsStart);
		repInts.filterEndingBefore(0);
		a.SetRepairIntervals(&repInts); 
		a.AtypicalClassify(m_ExtendedAtypicalClassification);

		repInts.clear();
	}
	
	/*
	=======================================================================================================================
	AddExtrapolatedSignal - add 'fake' signal at end of window so that can get approximation of decels and accels
	near end of window.
	=======================================================================================================================
	*/
	void CFHRSignal::AddExtrapolatedSignal()
	{
		long nExtrap = GetNumExtrapolatedSignal();
		long nReal = m_nPtsCount - nExtrap;
		double s;
		long i, x1, x2;
		for (i = 0; i < nExtrap; i++)
		{
			if (i > nReal) // more extrapolated signal than actual signal - only will happen at beginning of tracing
				s = m_pdSignal[0];
			else
				s = m_pdSignal[nReal - i - 1];
			m_pdSignal[nReal + i] = s;
		}

		// now we want to extrapolate repair intervals
		if (m_ConfigFHR.m_bDisableRepair)
		{  // will have loaded repair intervals from file - need to manage fake extrapolated intervals using pending flag
			m_pRepairIntervals.removePending();
		}

		long maxRepX2 = nReal - nExtrap + m_OutputAbsStart;
		i = m_pRepairIntervals.size() - 1;
		while ((i >= 0) && (m_pRepairIntervals.getAt(i)->getX2() > maxRepX2))
		{  // 3rd condition is for when inject repair intervals manually (i.e. when use expert markings for training)
			if (!(m_ConfigFHR.m_bDisableRepair) || (m_pRepairIntervals.getAt(i)->getX2() <= m_nTotalCount))
			{
				fhrPart *p = new fhrPart;
				(*p) = *(m_pRepairIntervals.getAt(i));
				p->toRelativeTime(m_OutputAbsStart);
				x2 = 2*nReal - p->getX1() - 1;
				x1 = 2*nReal - p->getX2() - 1;
				if (x2 >= m_nPtsCount)
				{
					x2 = m_nPtsCount - 1;
				}	
				p->setX1(x1);
				p->setX2(x2);
				p->toAbsTime(m_OutputAbsStart);
				p->setPending(true);
				m_pRepairIntervals.add(p);	
			}
			i--;
		}

	}

	/*
	=======================================================================================================================
	Reset the total output fhrPartSet.  This allows Patterns fetus to inject events after server reboot.
	=======================================================================================================================
	*/
	void CFHRSignal::SetTotalOutputFhrPartSet(fhrPartSet *s)
	{
		m_TotalOutputFhrPartSet.clear();
		m_TotalOutputFhrPartSet.addcopy(s);
		for (long i = 0; i < m_TotalOutputFhrPartSet.size(); i++)
		{
			m_TotalOutputFhrPartSet.getAt(i)->setPending(false);
			m_TotalOutputFhrPartSet.getAt(i)->setCommitted(true);
		}
	}

	/*
	=======================================================================================================================
	On server reboot, want to inject m_TotalOutputFhrPartSet (using SetTotalOutputFhrPartSet) and then reset the appropriate
	commitIndexes and start Indexes for submodules.  Will need to start recalc from before point at which to recalc so can
	repopulate baseline candidate buffers.  Use GetReqWarmupAfterReboot() to get the required buffer.
	=======================================================================================================================
	*/
	void CFHRSignal::SetUpRecalcAfterReboot(long xStart)
	{
		long i;
		bool foundDec = false, foundAcc = false, foundBas = false;
		fhrPart *p;

		m_rebootMode = true;
		
		m_commitIndexDec = xStart;
		m_commitIndexAcc = xStart;

		m_basCutoff = xStart;
		m_accCutoff = xStart;
		m_decCutoff = xStart;

		m_lLastCommittedDecelX2 = xStart;
		m_lLastCommittedAccelX2 = xStart;
		m_lLastCommittedBasX2 =  xStart;

		//m_nTotalCount = xStart;
		//m_nTotalAppend = xStart;

		i = m_TotalOutputFhrPartSet.size() - 1;
		while ((i >= 0) && ((!foundDec) || (!foundAcc) || (!foundBas)))
		{
			p = m_TotalOutputFhrPartSet.getAt(i);
			if (p->isAccel())
			{
				if (!foundAcc)
				{
					foundAcc = true;
					m_lLastCommittedAccelX2 = p->getX2();
					m_accCutoff = p->getX2();
				}
			}
			else if (p->isDecel())
			{
				if (!foundDec)
				{
					foundDec = true;
					m_lLastCommittedDecelX2 = p->getX2();
					m_decCutoff = p->getX2();
				}
			}
			else if (p->isBaseline())
			{
				if (!foundBas)
				{
					foundBas = true;
					m_lLastCommittedBasX2 = p->getX2();
					m_basCutoff = p->getX2();
				}
			}
			i--;
		}

//		m_NextOutputAbsStart = xStart;
		m_NextRepairStart=xStart;
		m_NextBaselineDetectionStart=xStart;


	}

	/*
	=======================================================================================================================
	After server reboot need to repopulate baseline candidate buffer so need a warmup period.
	=======================================================================================================================
	*/
	long CFHRSignal::GetReqWarmupAfterReboot()
	{
		return m_ConfigFHR.GetReqBaseLinesHistory() + m_ConfigFHR.GetLongestBandPassDelay() + (long) (5.0 * 60.0 * m_dSmpFreq);
	}

	/*
	=======================================================================================================================
	Update buffer of all candidates - set as committed in candBuffer if not pending, otherwise leave as uncommitted and
	will reclassify in next window.
	=======================================================================================================================
	*/
	void CFHRSignal::UpdateNonExtendCandBuffer(fhrPartSet *ExtendedBumps, fhrPartSet *candBuffer)
	{
		long index;
		fhrPart *p;

		for (long i = 0; i < ExtendedBumps->size(); i++)
		{
			p = ExtendedBumps->getAt(i);
			if (!(p->isPending()))
			{
				index = candBuffer->getIndexFromX1X2(p->getX1Orig(), p->getX2Orig());
				if (index >= 0)
				{
					candBuffer->getAt(index)->setCommitted(true);
				}
			}
		}
	}
}

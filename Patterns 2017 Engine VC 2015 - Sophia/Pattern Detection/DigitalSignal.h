/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection CLASSES: File contains
* class declaration for: CDigitalSignal : Basic digitally sampled signal.
* CFHRSignal : Digitally sampled FHR signal. AUTHORS: Mark Doubson /FFT and
* Filters part /MultiHypothesis /BumpClassifications Vladimir Khavkine /Bumps
* Candidates Robert Morin /Repair part Kingsley Woodward /Neuron network
* calculation Jiang Wu /Baseline detection REVISION COMMNENTS: 06/Dec/2004 KW -
* Code review modification, added comments. 07/Dec/2004 RM - Code review modifs
* on CDigitalSignal, CFHRSignal. 29/Jul/2005 MD - Copyright LMS Medical Systems
* 2004 by Evonium Inc.
*/
#pragma once

#include <math.h>
#include <afxmt.h>			// mutex
#include "Config.h"
#include "VectorMatrix.h"
#include "Repair.h"
#include "BaseLine.h"
#include "fhrPart.h"
#include "fhrPartSet.h"
#include <vector>
#include "NN.h"
#include "ThreadLock.h"


namespace patterns
{
	class SVMModel;

	struct ContractionDetection
	{
		long lStart;
		long lEnd;
		long lPeak;
	};

	enum HalfWavePart
	{
		Pos20Min,
		Neg20Min,
		Pos8Min,
		Neg8Min
	};

	#define NaN numeric_limits<double>::quiet_NaN()

	// Oleg - CyclicArray size definition is only for m_tracingData member, it's size 4801 when 1 is for empty sliding spot between end and begin
	#define TRACING_DATA_SIZE 4801
	#define TRACING_DATA_MAX_ELEM_SIZE 4800

	// Oleg - CyclicArray size definition is only for m_tracingData member, it's size 961 when 1 is for empty sliding spot between end and begin
	#define TRACING_DATA_4MIN_SIZE 961
	#define TRACING_DATA_4MIN_MAX_ELEM_SIZE 960

	// Oleg - BP2 related debug prints

	//#define MEDIAN_HISTOGRAM_SIZE 256
	//#define BP2_DEBUG_SHOW_OVERALL_CLOCK 0

	//#define BP2_DEBUG_SHOW_TIMESERIES_SAMPLE 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_MEDIAN 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_MEDIANRES 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_POS 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_NEG 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_HALFWAVE_INTERP 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_ENV 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_SKEW20 0
	//#define BP2_DEBUG_SHOW_TIMESERIES_SKEW4 0

	//#define BP2_DEBUG_SHOW_BAS_FEATURE_MEDIANDEV 0
	//#define BP2_DEBUG_SHOW_BAS_FEATURE_ENV_DEV 0
	//#define BP2_DEBUG_SHOW_BAS_FEATURE_SKEW_DEV 0
	//#define BP2_DEBUG_SHOW_BAS_FEATURE_VAR 0

	//#define BP2_DEBUG_SHOW_BAS_PROCESSING_COUNTS 0

	//#define BP2_DEBUG_SHOW_CLASSIFICATION 0
	//#define BP2_DEBUG_SHOW_CLASSIFICATION_BY_FOLD 0
	//
	//#define BP2_DEBUG_SHOW_BUMP_PREV_NEXT_BAS 0


	/*
	=======================================================================================================================
	! CLASS: CDigitalSignal DESCRIPTION: Basic digitally sampled signal. This should be the base class for every
	digitally sampled signals.
	=======================================================================================================================
	*/
	class CDigitalSignal
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CDigitalSignal(void);
		virtual ~CDigitalSignal(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		long m_nPtsCount;		// ! Used in processing
		long m_nTotalCount;		// ! Total number of passed timepoints
		long m_nTotalAppend;    // total number that have been appended - differs from m_nTotalCount in that includes samples that have been appended (are in m_pucS2Aloc) but are not being processed in current processing loop
		long m_nProcessedCount; // ! Start of Next segment

		bool m_ExtendedAtypicalClassification; // Full atypical classification (8 types) or normal one (4 types)
		
		double *m_pdSignal;
		double m_dSmpFreq;

	public:
	
		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetExtendedAtypicalClassification() 
		{
			m_ExtendedAtypicalClassification = true;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetPtsCount(void) const
		{
			return m_nPtsCount;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetTotalPtsCount(void) const
		{
			return m_nTotalCount;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetTotalAppendCount(void) const
		{
			return m_nTotalAppend;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetProcessedPtsCount(void) const
		{
			return m_nProcessedCount;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		const double *const GetSignal(void)
		{
			return m_pdSignal;
		}

		virtual bool SetSignal(long nPtsCount, double *pdSignal);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		double GetSmpFreq(void) const
		{
			return m_dSmpFreq;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetSmpFreq(double dSmpFreq)
		{
			m_dSmpFreq = dSmpFreq;
		}

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		virtual void ResetSignal(void);
	};

	/*
	* ! CLASS: CFHRSignal DESCRIPTION: Digitally sampled FHR signal. This class is
	* made to handle the FHR signal processing proprietary to LMS.
	*/
	class CFHRSegment;

	class CFHRSignal : public CDigitalSignal
	{
	public:
		class CyclicArray
		{
		public:
			CyclicArray();
			virtual ~CyclicArray();

		private:
			double m_array[TRACING_DATA_SIZE];

			int m_currentHead;
			int m_currentTail;
			int m_size;

		public:
			double at(int position);
			int size() { return m_size; }
			int mod(int position);

			double back();
			double front();

			void initialize(int size, double val);
			void clear();

			double remove_head();
			void push_back(const double value);

			double operator[](int position) { return m_array[mod(m_currentHead + position)]; }
		};

		class CyclicArray4Min
		{
		public:
			CyclicArray4Min();
			virtual ~CyclicArray4Min();

		private:
			double m_array[TRACING_DATA_4MIN_SIZE];

			int m_currentHead;
			int m_currentTail;
			int m_size;

		public:
			double at(int position);
			int size() { return m_size; }
			int mod(int position);

			double back();
			double front();

			void initialize(int size, double val);
			void clear();

			double remove_head();
			void push_back(const double value);

			double operator[](int position) { return m_array[mod(m_currentHead + position)]; }
		};



		class InterpolatedHalfWaveList
		{
		public:
			class InterpolatedHalfWaveListElement
			{
			public:
				InterpolatedHalfWaveListElement(double val)
				{
					m_value = val;
					m_pNext = NULL;
				}

				virtual ~InterpolatedHalfWaveListElement() { }
			
			private:
				InterpolatedHalfWaveListElement* m_pNext;
				double m_value;

			public:
				double GetValue() { return m_value; }
				void SetNext(InterpolatedHalfWaveListElement* pNext) { m_pNext = pNext; }
				InterpolatedHalfWaveListElement* GetNext() { return m_pNext; }
			};

		public:
			InterpolatedHalfWaveList()
			{
				m_pHead = m_pTail = NULL;
				m_size = 0;
			}

			virtual ~InterpolatedHalfWaveList()
			{
				clear();
			}

		private:
			int m_size;
			InterpolatedHalfWaveListElement* m_pHead;
			InterpolatedHalfWaveListElement* m_pTail;

		public:
			void push_back(double val)
			{
				InterpolatedHalfWaveListElement* pTmp = new InterpolatedHalfWaveListElement(val);
				if (m_size > 0)
				{
					m_pTail->SetNext(pTmp);
					m_pTail = pTmp;
					++m_size;
				}
				else
				{
					m_pHead = m_pTail = pTmp;
					m_size = 1;
				}
			}

			InterpolatedHalfWaveListElement* GetFirstElement() { return m_pHead; }
			double front() { return m_pHead->GetValue(); }
			double back() { return m_pTail->GetValue(); }

			void erase_head()
			{
				if (m_size > 0)
				{
					InterpolatedHalfWaveListElement* pTmp = m_pHead->GetNext();
					delete m_pHead;
					m_pHead = pTmp;
					--m_size;
				}
			}

			void clear()
			{
				while (m_pHead != NULL)
				{
					InterpolatedHalfWaveListElement* pNext = m_pHead->GetNext();
					delete m_pHead;
					m_pHead = pNext;
				}

				m_pTail = NULL;
				m_size = 0;
			}

			void initial_insert(int numOfElems, double val)
			{
				clear();
				for (int i = 0; i < numOfElems; ++i)
				{
					push_back(val);
				}
			}

			int size() { return m_size; }

		};
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CFHRSignal(void);
		virtual ~CFHRSignal(void);

		friend class CBaseLineDetection;
		int m_iOnlyBaseLines;

		CString m_CurrentBaselineFile;
		CString m_CurrentOutputFile;
		
		//
		// ===============================================================================================================
		//    mode selection flag: 0 - calculate baselines only and return baseline set 1 - use already calculated and passed
		//    baselines and perform all next steps: MH and AD 2 - old mode - calculate baselines and perform all next steps: MH
		//    and AD ;
		//    functions
		// ===============================================================================================================
		//
		CConfigFHR *const GetConfig(void)
		{
			return &m_ConfigFHR;
		}

		virtual bool SetSignal(long nPtsCount, double *pdSignal);
		bool SetContraction(long nCount, long *pContraction);

		int Append(unsigned char* pSignal, long signal_count, long *pContraction, long contractions_count);

		int Append_Signal(unsigned char* signal, long nCount);
		int Append_Contraction(long nCount, long *pContraction);

		void ResetSignal(void);
		void SetMessageLevel(int MsgLevel);
		void ProceedBuffer(void);
		bool Process(void);
		int Flush(bool bStopNow);
		int GetResultSize(void);
		fhrPartSet *GetResultObjects(void);

		int GetTotalResultSize(void);
		fhrPartSet *GetTotalResults(void);
		void LockTotalResultObjects(bool lock = true);

		void SetCallBackFunction(void (*pCallBack) (int i, void *), void *);

		void SetTotalOutputFhrPartSet(fhrPartSet *s);
		void SetUpRecalcAfterReboot(long xStart);
		long GetReqWarmupAfterReboot();

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		const double *const GetProcessedSignal(void)
		{
			return m_pdSignalToProcess;
		}

		bool Proceed(void);
		bool PrepareProceed(void);
		void OutputXML(int iStep);
		uintptr_t ThreadHandle;

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool IsProcessRunning(void)
		{
			return m_ThreadMutex;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool InTransfer(void)
		{
			return false; //TODO
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void PrepareFlush(void)
		{
			m_ThreadMutex = false;
		};

		ContractionDetection *m_ContractDetect;
		long m_lContrDet;	// Number of Contractions
		ContractionDetection *CreateContractionDetection(void);

		fhrPartSet m_OutputFhrPartSet;
		fhrPartSet m_TotalOutputFhrPartSet;
		bool m_bJumpMode;

		bool m_bLastAppend;

		bool m_bHaveNewEvents;

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long GetLowPassCount(void)
		{
			return m_nLowPassCount;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetVariabilityCount(void)
		{
			return m_nVariabilityCount;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		const double *const GetLowPassSignal(void)
		{
			return m_pdLowPassSignal;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		const double *const GetVarPassSignal(void)
		{
			return m_pdVarPassSignal;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		const double *const GetMinVarSignal(void)
		{
			return m_pdMinVarSignal;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		fhrPartSet* GetCandBumps(void)
		{
			return (&m_pCurrCandBumps);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		fhrPartSet* GetCandBumpsExtend(void)
		{
			return (&m_pCurrCandBumpsExtend);
		}
		/*
		===============================================================================================================
		===============================================================================================================
		*/

		fhrPartSet *GetNewBaseLines(void);
		fhrPartSet *GetMultiH(void);
		fhrPartSet *GetTotalBaseLines(void) {return &m_pBaseLinesTotal;}
		fhrPartSet *GetPendingBaseLines(void) {return &m_pBaseLinesPending;}

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long GetNumRepairIntervals(void)
		{
			return m_pRepairIntervals.size();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		fhrPartSet *GetRepairIntervals(void)
		{
			return (&m_pRepairIntervals);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetStableRepairIndex(void)
		{
			return m_NextRepairStart;
		}

		void SetRepairIntervals(fhrPartSet *pRepairIntervals);	// used only for NN training w/ old repair

		void SetMinSamplesAppend(long lMinSamplesAppend);	// set min total appended samples required before processing
		void SetMaxSamplesAppend(long lMaxSamplesAppend);	// set max samples to process in one iteration
		void SetRemoveRepairOutput(bool b);
		long GetMinSamplesAppend(void);
		long GetMaxSamplesAppend(void);

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetLongestBandPassDelay(void)
		{
			return m_ConfigFHR.GetLongestBandPassDelay();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetNNtrainingParams(void)
		{
			m_ConfigFHR.SetNNtrainingParams();
		}	// setup config params for training

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void DisableRepair(bool b)
		{
			m_ConfigFHR.DisableRepair(b);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetXMLFilePrefix(string str)
		{
			m_ConfigFHR.SetXMLFilePrefix(str);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetPathToTestFiles(string str)
		{
			m_ConfigFHR.SetPathToTestFiles(str);
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long GetBasTrimForVarCalc(void)
		{
			return m_ConfigFHR.GetBasTrimForVarCalc();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetBasSegLenForVarCalc(void)
		{
			return m_ConfigFHR.GetBasSegLenForVarCalc();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool IsBasVarCalcActivated(void)
		{
			return m_ConfigFHR.IsBasVarCalcActivated();
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void ActivateBasVarCalc(bool b = true)
		{
			m_ConfigFHR.ActivateBasVarCalc(b);
		}

		void SetSynchroneCalculation(bool b = true) { m_synchroneCalculation = b;}

		long GetNumExtrapolatedSignal() { return(m_ConfigFHR.GetNumExtrapolatedSignal()); }
		void AddExtrapolatedSignal();
		long GetNumRealSignal() { return(m_nPtsCount - GetNumExtrapolatedSignal()); }
		bool UsingPendingRT() {return m_ConfigFHR.UsingPendingRT();};
		long GetNumAccelDecelIterations() { return(m_ConfigFHR.GetNumAccelDecelIterations()); }
		void SetNumAccelDecelIterations(long n) { m_ConfigFHR.SetNumAccelDecelIterations(n); }
		long GetCurrAccelDecelIteration() { return(m_lCurrIteration);}

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
		bool m_bKeepPending;
		void setStoreXML(int i) {m_ConfigFHR.m_iStoreXML = i;}
		void setStoreMAT(int i) {m_ConfigFHR.m_iStoreMAT = i;}
		void setKeepPending(bool b = true) {m_bKeepPending = b;}

		bool hasNewEvents(void) {return m_bHaveNewEvents;}
		void resetNewEvents(void) {m_bHaveNewEvents = false;}

	private:
		CConfigFHR m_ConfigFHR;

		CRITICAL_SECTION m_csProcess;		// see methods Process () and RunProcess ()
		friend void RunProcess(void *);

		bool m_ThreadMutex;
		patterns::ThreadLock m_TransferMutex;
		bool m_FlushMutex;

		bool m_bKeepAppend;
		bool m_bSignalIsRepaired;
		bool m_bRepairInPlace;

		bool m_bCatchException;

		double *m_pdSignalToProcess;

		bool m_FlatRepair;
		long m_LastAppend;

		long m_S2A;
		unsigned char *m_pucS2A;					// signal to append
		long m_S2Aloc;
		unsigned char *m_pucS2Aloc;					// local buffer so can copy all new data and then loop only processing certain size chunk at a time

		// BP2 Calculation region

		//vector<double> m_tracingsData;					// sliding window of tracings as it arrives from episode.
		//CyclicArray m_fhrWindowTracings;				// Cyclic array of fhr window length (4801).
		//int m_lastInsertedIndexToTracingsData;
		//int m_samplesWindowSize;
		//int m_medianSamplesWindowSize;

		//int m_medianHistogram[MEDIAN_HISTOGRAM_SIZE];

		//vector<double> m_medianResult;					// median calculation result ± 20/0 min

		//vector<double> m_medianResidualResult;			// median residual calculation result ± 5/15 min

		//vector<double> m_medianResidualHalfWavePos;		// median residual half wave pos ± 20/0 min
		//vector<double> m_medianResidualHalfWaveNeg;		// median residual half wave neg ± 20/0 min

		//vector<double> m_firFilterCoefs;
		//
		//InterpolatedHalfWaveList m_medianResidualHalfPosInterp;		// fast median residual half wave pos interpolated ± 20/0 min								
		//InterpolatedHalfWaveList m_medianResidualHalfNegInterp;		// fast median residual half wave neg interpolated ± 20/0 min								

		//// PAW
		//int hwPosNSamples;
		//int hwPosInterpNSamples;
		//int hwNegNSamples;
		//int hwNegInterpNSamples;
		//int svmExampleCount;

		//vector<double> m_upperEnvelope;					// median residual upper envelope filter
		//vector<double> m_lowerEnvelope;					// median residual lower envelope filter

		//CyclicArray m_20MinMedianResiduals;
		//CyclicArray m_20MinMedianResidualsSquares;
		//CyclicArray m_20MinMedianResidualsCubes;

		//CyclicArray4Min m_4MinMedianResiduals;
		//CyclicArray4Min m_4MinMedianResidualsSquares;
		//CyclicArray4Min m_4MinMedianResidualsCubes;

		//vector<double> m_skew20Min;					// FHR skewness ± 5/15 min
		//vector<double> m_skew4Min;					// FHR skewness ± 2 min

		//vector<double> m_upperDeviation;			// FHR upper deviation - fhr sample - upper envelope sample 20 min
		//vector<double> m_lowerDeviation;			// FHR lower deviation - fhr sample - lower envelope sample 20 min

		//vector<double> m_medianBLDeviation;			// Baseline median deviation - for each BL: mean(FHRMedian indices) - mean(BL.Y1, BL.Y2) - Input for TORCH

		//vector<double> m_upperBLDeviation;			// BL upper deviation - for each BL: mean(FHR upper deviation) 20 min - Input for TORCH
		//vector<double> m_lowerBLDeviation;			// BL lower deviation - for each BL: mean(FHR lower deviation) 20 min - Input for TORCH

		//vector<double> m_skew20MinBLDeviation;		// BL skew deviation - for each BL: mean(fhr window skew) 20 min - Input for TORCH
		//vector<double> m_skew4MinBLDeviation;		// BL skew deviation - for each BL: mean(fhr window skew) 4 min  - Input for TORCH

		//vector<double> m_BLVariability;				// BL variability - for each BL: stdev(median residual indices of BL) - Input for TORCH

		//long m_lastPosNOTNaN;
		//long m_lastNegNOTNaN;

		//double m_sum20Min;
		//double m_sumSquares20Min;
		//double m_sumCubes20Min;

		//double m_sum4Min;
		//double m_sumSquares4Min;
		//double m_sumCubes4Min;

		//int m_dataSkewness20MinWindowSize;
		//int m_actualSkewness20MinWindowSize;
		//int m_dataSkewness4MinWindowSize;
		//int m_actualSkewness4MinWindowSize;

		//long m_lastBaselineMedianDeviation;
		//long m_lastBaselineEnvelopeDeviation;
		//long m_lastBaselineSkewDeviation;
		//long m_lastBaselineVariability;

		//int m_shiftFromAbsoluteStart;

		//int m_tracingDataIndex;
		//int m_numOfErasedTracingDataSamples;
		//int m_lastRepairedDataIndex;


		//CString m_svmModelsPath;

		// End BP2 Calculation region

		long m_lConsecInvalidAppend;		// keeps track of consecutive invalid signal appended - use for jumpMode

		long m_S2K;
		double *m_pdS2K;					// signal to keep

		long m_S4F;
		double *m_pdS4F;					// signal to fill (cover small appended chunk with history data!)

		ContractionDetection *m_pdC2A; // contractions to append
		long m_C2A;
		ContractionDetection *m_pdC2Aloc; // local buffer 
		long m_C2Aloc;

		ContractionDetection *m_pdC2K;
		int m_C2K;

		long m_OutputAbsStart;
		long m_NextOutputAbsStart;

		long m_lLastCommittedDecelX2;
		long m_lLastCommittedAccelX2;
		long m_lLastCommittedBasX2;

		long m_NextBaselineDetectionStart;	
		long m_NextRepairStart;
		double m_dLastGoodBasRef; // use for long dropouts so have some reference baseline level

		long m_lCurrIteration; // what accel/decel iteration (used for dumping intermediate stats from test client)

		int m_MessageLevel;					// 0 - silence, 1 - summary, 2 - details (R&D - oriented), 3 - annoying (show all append event - even waiting)
		bool m_bNonDisplayMode;
		bool m_bSetMode;
		void CleanMemory(void);
		bool m_ProceedBuffer;
		bool Preprocessing(void);

		void StoreTail(void);
		void ShiftContractions(void);
		void SetCurrContractions();

		// BP2 calculations

		//void InsertSampleToSortedArray(double sample);
		//void InitFiltersDataStructures();
		//int GetNextNonZeroElementInHistogram(int ind);
		//void CalculateMedian(double sample);

		//void CalculateMedianResidualHalfWaves();
		//void CalculateFirFilter(double sample);
		//void CalculateFirFilterInternal(InterpolatedHalfWaveList& halfWave, vector<double>& envelope, vector<double>& fhrDeviation, double sample, bool isUpper = true);
		//void CalculateEnvelopeDeviation(const vector<double>& envelope, vector<double>& fhrDeviation, double sample, bool isUpper = true);
		//void CalculateSkewness();
		//double CalculateSkewnessInternal(double sums, double sumSquares, double sumCubes, double mean, double variance, double sigma, int windowSize);

		//void LinearInterpolation(HalfWavePart part);
		//void LinearInterpolationInternal(vector<double>& halfWave, int start, int end);

		//void AddToInterpolatedVector(HalfWavePart part);
		//void AddToInterpolatedVectorInternal(vector<double>& halfWave, InterpolatedHalfWaveList& halfWaveInterp, int start, bool isUpper = true);

		//baseline* GetBaselineByTime(long x);
		//int GetFinalBaselineIndexByTime(long x);

		//void CalculateBLDeviations();
		//void CalculateBLMedianDeviation();
		//void CalculateBLMedianDeviationInternal();
		//void CalculateBLMedianDeviationInternal(baseline* pBL);
		//void CalculateBLEnvelopeDeviation();
		//void CalculateBLEnvelopeDeviationInternal(baseline* pBL);
		//void CalculateBLSkewnessDeviation();
		//void CalculateBLSkewnessDeviationInternal(baseline* pBL);
		//void CalculateBLVariability();
		//void CalculateBLVariabilityInternal(baseline* pBL);

		//void CleanVectors();
		//void CleanSVMInput(int nExamples);

		//void SVM();
		//CString GetSVMModelsPath();

		//bool IsSampleRepaired(int tracinDataIndex);

		// END BP2 calculations

		void *m_pData;
		ThreadLock m_total_results_mutex;

		// ! pCallBackFunction: File-global pointer to callback function set using
		// CFHRSignal::SetCallBackFunction().
		void (*m_pCallBackFunction) (int i, void *);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/

#ifdef _APPLICATION // This switch changes the public functions to protected so the dll exposes only minimal set
	public:
#else

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
#endif
		double *m_pdUnrepairedSignal;
		double *m_pdBufferSignal;
		double *m_pdLowPassSignal;
		double *m_pdLowPass25Signal;
		double *m_pdVarPassSignal;
		double *m_pdMinVarSignal;

		// ! Array used for Feature PatternsVector generation
		double *m_pdMovingAverage;	// with 720 points window size
		long m_lLastValidMovingAvgIndex;	// so can use signal from window to window without recalc all
		long m_lLastMovingAvgCompStart;		// so know offset from window to window (can not assume is constant)

		bool m_FirstRun;
		bool m_bMultihypothesisTopologyLoaded;
		bool m_bAccelDecelTopologyLoaded;

		fhrPartSet m_pBaseLinesNewInWindow;
		fhrPartSet m_pBaseLinesPending;
		fhrPartSet m_pBaseLinesFinal;
		fhrPartSet m_pBaseLinesTotal;

		fhrPartSet m_pAllPreMergeAccels;
		fhrPartSet m_pAllPreMergeDecels;
		fhrPartSet m_pPosCandidates;
		fhrPartSet m_pNegCandidates;

		fhrPartSet m_pCurrCandBumps;
		fhrPartSet m_pCurrCandBumpsExtend;

		long m_commitIndexDec;
		long m_commitIndexAcc;

		long m_basCutoff;
		long m_accCutoff;
		long m_decCutoff;

		bool m_rebootMode;
		bool m_synchroneCalculation;

		fhrPartSet m_pRepairIntervals;

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		void SetLowPassCount(int nCount)
		{
			m_nLowPassCount = nCount;
		};

		//
		// ===============================================================================================================
		//    int GetVariabilityCount(){return m_nVariabilityCount;
		//    };
		// ===============================================================================================================
		//
		void SetVariabilityCount(int nCount)
		{
			m_nVariabilityCount = nCount;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long GetMinVarCount(void)
		{
			return m_nMinVarCount;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetMinVarCount(long nCount)
		{
			m_nMinVarCount = nCount;
		};

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/

	public:
		bool RepairSignal(void);
		bool DetectBaseline(void);
		bool MultiHypothesis(void);
		//bool BP2Processing(void);
		bool AccelDecel(void);
		bool LowPass(void);
		bool MinVarPass(void);
		bool BandPass(void);


#ifdef _APPLICATION // ! This switch changes the public functions to protected so the dll exposes only minimal set.
	public:
#else

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
#endif

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		void SetRepairInPlace(bool bRepairInPlace)
		{
			m_bRepairInPlace = bRepairInPlace;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool GetSignalIsRepaired(void)
		{
			return m_bSignalIsRepaired;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		bool GetRepairInPlace(void)
		{
			return m_bRepairInPlace;
		}

		long GetRepairEndBuffer(void);
		bool LongRepairAtEndOfWindow(void) {return ((m_nTotalCount - m_NextRepairStart) > (m_ConfigFHR.GetMaxRepairForCommitIndexForce()));}

		// Filter-related public methods. bool Proceed();
		bool LowPass25(void);
		bool FixFilterVarRepair(void);
		bool VarPass(void);
		bool HighPass(void);
		bool ApplyFilter(long M, long N, double *coeff, long iDelay);
		bool ApplyFilterNoOverlapAdd(long M, double *coeff);
		bool ApplyFilterDirect(double *coeff, long lNumTaps);
		bool ApplyBandFilter(long N, long M1, double *coeff1, long M2, double *coeff2);
		void GetBaselinesPolyfit(fhrPartSet *bas);
		void GetBaselinePolyfit(baseline *b);
		bool SubdivideLargeBaseLines(fhrPartSet* bas);
		bool ComputeMovingAverage(void);

		bool ComputeLPMean(baseline *pBL);
		bool ComputeMean(baseline *pBL);
		double GetSignalMean(double *s, long x1, long x2);
		bool ComputeLPMean(fhrPartSet *pBaseLines);
		bool ComputeVarMin(fhrPartSet *pBaseLines);
		bool ComputeMultiHFeatures(fhrPartSet *pBasStable, fhrPartSet *pBasPending);
		bool ComputeMultiHFeaturesInBas(baseline *pBL);
		bool ComputeMultiHFeaturesBtwnBas(baseline *pBL1, baseline *pBL2);
		bool UpdateTotalBaseLineBuffer(baseline *pBL);

		double Calculate_FeaturesPrimary(baseline *pBL[5], void *pFile = NULL);
		double Calculate_FeaturesSecondary(baseline *pBL[5], void *pFile = NULL);


		long m_lFiltered;		// ! Number of Points after Band Pass Filtering
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		int SignOf(double dValue)
		{
			return (dValue > 0) ? 1 : (dValue < 0) ? -1 : 0;
		}

		// Filter-related protected methods.
		void Rectify(void);
		long OptimalBuffer(long BufferSize);
		void realFFT(PatternsVector& a, long arrayLen, bool inverseFFT);
		void FftConvolution(PatternsVector& signal, long signalLen, const PatternsVector& filter, int filterLen, PatternsVector& overlap, bool bConvertCoeffs);

		bool Approx_Equal(double x1, double x2);
		long m_nLowPassCount;
		long m_nLowPass25Count;
		long m_nVariabilityCount;
		long m_nMinVarCount;

		// MultiHypothesis protected methods.
		double SimulatePrimaryNN(PatternsVector& vFHR_baseline);
		double SimulateSecondaryNN(PatternsVector& vFHR_baseline);

		bool ACBandPass(int ind);
		bool ACZeroCrossing
			(
			long **lZeroCrossings,	// pointer to store the result
			long *lPoints,			// number of crossings
			int ind
			);	// seq number of Frequncy Band

		bool OutputBandPass(int ind);	// seq number of the Frequency Band
		bool OutputZeroCrossings
			(
			const long *lZC,		// pointer to data to output
			long lZCPoints,			// number of points
			int ind
			);					// seq number of the Frequency Band

		virtual void OutputMatlab(long*, long, int ) {};

		void ClassifyDecels(fhrPartSet *d);
		void ClassifyDecel(decel *d, void *pFile = NULL);
		void ClassifyAccels(fhrPartSet *accels, fhrPartSet *decels);
		void ClassifyAccel(accel *a, fhrPartSet *decels, void *pFile = NULL);
		long GetEarliestX1InNextWindow(fhrPartSet *BumpsIn, long lMaxEnd);
		void RemoveNonEvents(fhrPartSet *f);
		void getPosNegBumps
			(
			const long *lZC,
			long lZCPoints,
			int ind,
			long *dLastZCPos,
			long *dLastZCNeg,
			fhrPartSet *PositiveBumps,
			fhrPartSet *NegativeBumps
			);
		double CalcBumpArea(long l1, long l2);
		double BumpHeight(long l1, long l2, long *lPeak);
		void addBumpIfHigher(fhrPartSet *Bumps, bump *b);
		void addBumpAndReplace(fhrPartSet *Bumps, bump* b);
		int ModifyAccelCandidateBounds(fhrPartSet *a, fhrPartSet *d, long lOffset);
		void PostProcessDecels(fhrPartSet *decels);
		bool PostProcessDecel(decel *d);
		bool PreprocessAccels(fhrPartSet *accels);
		bool PreprocessAccel(accel *a);
		bool PreprocessDecels(fhrPartSet *decels);
		bool PreprocessDecel(decel *a);
		void FreqBandDependentBumpMeasures(const double *dFiltered, decel *d);
		void BumpCompetition(fhrPartSet *BumpsTarget, fhrPartSet *BumpsSource, bool isAccel);
		bool MeasureOneDecel(decel *d);
		void DecelToVector(decel *d, PatternsVector* v);
		void GetDecelHeightBM(decel *d);
		bool MeasureOneAccel(accel *a, fhrPartSet *decels);
		void GetAccelHeightBM(accel *a);
		void AccelToVector(accel *a, PatternsVector* v);
		double GetDecelRate(long lX1, long lWindow, fhrPartSet *decels);
		double GetDecelRate(long lX1, long lWindow);
		long GetNextDecelX1(long lX2, fhrPartSet *decels);
		long GetLastDecelX2(long lX1, fhrPartSet *decels);
		void GetBaselineBumpMeasures(bump *b);
		void GetPrevNextBaseLines(bump *b, baseline *blPrevOut, baseline *blNextOut);
		bool GetOnsetAndRecovFromSignal(fhrPart *f, double *pSignal, long *lOnset, long *lRecov, double *dOnsetSlope, double *dRecoverySlope);
		bool GetOnsetAndRecovFromLP(bump *b, long *lOnset, long *lRecov);
		bool GetMaxSlopeInOutAccel25(accel *a, long *lMaxTimeIn, long *lMaxTimeOut);
		bool NormalizeOneBump(PatternsVector* pV, bool bAccel);
		bool NormalizeOneDecel(PatternsVector* pV);
		bool NormalizeOneAccel(PatternsVector* pV);
		bool SimulateOneDecel(const PatternsVector* inVector, decel *d, void *pFile = NULL);
		bool SimulateOneAccel(const PatternsVector* inVector, accel *a, void *pFile = NULL);
		double MapDecelConfidence(double dNonDecelConf, double dMaxNonDecelConf);
		double MapAccelConfidence(double dAccelConf, double dMinAccelConf);
		long GetContractionIndex(long lStart, long lEnd);
		bool IsDegenerateFhrPart(fhrPart *p);
		bool IsAlreadyCommitted(fhrPart *p);
		bool SubtypeDecel(decel *d);
		bool SubtypeProlonged(decel *d);
		bool IsUnassociatedDecel(decel *d);
		void EliminateFirstOutputObjects(void);
		void SetOutputAfterCutoffAsPending(void);
		void ExtendBumps(fhrPartSet *Bumps, long prevBumpX2);

		double GetMaxNonDecConf(fhrPart::FhrPartType, long iteration);
		double GetMinAccelConf(long iteration);

		bool RejectRepairBump(bump *b);
		long MarkRepairBumps(fhrPartSet *fset);
		double CalcPercRepair(long lX1, long lX2);
		long CalcRepairSamples(long lX1, long lX2);
		double GetRepairLevelShift(double *pSamples, long lX1, long lX2);

		void RemoveNOB(void);
	
		double BaselineVarMin(baseline *pBaseLine);

		double ComputeFhrStd(double *pSignal, long lX1, long lX2);
		void CalcBaselineVar(fhrPart *f, long lOffset = 0);
		void CalcOutputBaselineVar(fhrPartSet *f, long lOffset = 0);

		void RegenBaseLinesFinalFromMerge(fhrPartSet &NegativeBumps, fhrPartSet &PositiveBumps);
		void AtypicalDecelClassify(fhrPartSet *p);
		void UpdateNonExtendCandBuffer(fhrPartSet *ExtendedBumps, fhrPartSet *candBuffer);


		void fhrPartSetMerge(fhrPartSet* f, fhrPart* p);

		long m_InitDataPos;
		long m_InitDataSize;

		// RD step
		long m_LastValidBaseLinePosition;
		long m_iRepStart;
		long m_iRepEnd;


		// Oleg - BP2 methods
		//SVM model
		//vector<SVMModel*> m_svmModels;

		//int GetBp2StableBaselineLimit(bool disabled = false);
		//bool m_bumpClassificationHeaderDone;

		//void CFHRSignal::BP2SetStableFeatures();

		int m_baselineMedianCount;

	};
}

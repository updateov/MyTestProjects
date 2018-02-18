#pragma once

#include "patternsworker.h"

#ifdef IMPEXP
	#undef IMPEXP
#endif

#ifdef PATTERNS_DRIVER
	#define IMPEXP __declspec(dllexport)
#else
	#define IMPEXP __declspec(dllimport)
#endif

namespace PatternsDriver
{
	class IMPEXP PatternsWorkerWrapper
	{
	private:
		PatternsWorker* m_pPatternsWorker;

	public:
		PatternsWorkerWrapper();
		virtual ~PatternsWorkerWrapper();


		// Create an engine instance
		PatternsWorker* GetEngine();

		// Detection of events
		int ProcessHR(char* signal, long start, long count);

		// Detection of contractions
		int ProcessUP(char* signal, long start, long count);

		// Retrieve results
		bool ReadResults(char* output_buffer, long output_size);

		// Cleanup an engine instance
		//void EngineUninitialize();

	};
}

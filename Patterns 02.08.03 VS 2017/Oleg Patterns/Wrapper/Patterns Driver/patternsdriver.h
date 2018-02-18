#pragma once

#ifdef PATTERNSDRIVER_EXPORTS
	#define PATTERNSDRIVER_API
#else
	#define PATTERNSDRIVER_API __declspec(dllimport)
#endif

namespace PatternsDriver
{
	// Create an engine instance
	HANDLE PATTERNSDRIVER_API __stdcall EngineInitialize();

	// Detection of events
	int PATTERNSDRIVER_API __stdcall EngineProcessHR(HANDLE param, char* signal, long start, long count);

	// Detection of contractions
	int PATTERNSDRIVER_API __stdcall EngineProcessUP(HANDLE param, char* signal, long start, long count);

	// Retrieve results
	bool PATTERNSDRIVER_API __stdcall EngineReadResults(HANDLE param, char* output_buffer, long output_size);

	// Cleanup an engine instance
	void PATTERNSDRIVER_API __stdcall EngineUninitialize(HANDLE param);

}

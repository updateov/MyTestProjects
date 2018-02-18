#pragma once

#include "patterns, contraction detection.h"
#include "patterns, contraction.h"
#include "patterns, event.h"

#include "DigitalSignal.h"
#include "patterns, fetus.h"

using namespace std;

namespace PatternsDriver
{
	class PatternsWorker
	{

	public:
		PatternsWorker();
		virtual ~PatternsWorker();

		int ProcessHR(char* signal, long count);
		int ProcessUP(char* signal, long count);

		bool ReadResults(char* outputBuffer, long outputSize);

	protected:
		void append_up(char* signal, long count);
		void append_fhr(char* signal, long count);

		// Contraction detection
		vector<char> m_upBuffer;
		long m_upStart;

		long m_lastContractionEnd;
		long m_lastContractilityEnd;
		patterns::contraction_detection m_contractionEngine;

		patterns::fetus m_fetus;

		// Events detection
		long m_eventLastIndex;
		patterns::CFHRSignal m_patternsEngine;

		// The data not yet returned to the worker user
		string m_pendingResults;

		string EventToString(const patterns::event& curEvent);
		string ContractionToString(const patterns::contraction& curContraction);

		std::vector<long> m_pastContractions;
		long FindContractionStart(int contractionIndex);
	};
};


	

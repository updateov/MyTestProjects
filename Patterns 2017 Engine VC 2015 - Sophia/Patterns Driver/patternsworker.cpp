#include "stdafx.h"
#include "patternsworker.h"

#define PIPE_SEPARATOR '|'

using namespace patterns;

namespace PatternsDriver
{

	///
	/// Simple constructor
	PatternsWorker::PatternsWorker()
	{
		// Synchronic, not thread there !
		m_patternsEngine.SetSynchroneCalculation(true);

		// Wants all atypical types
		// patterns_engine.SetExtendedAtypicalClassification();

		m_patternsEngine.GetConfig()->SetResourceHandleModuleName("PatternsDriver.dll");
		m_patternsEngine.GetConfig()->AllowShortAppend(true);

		// Threshold for contraction detection
		m_contractionEngine.set_latent_vector_cut_off(0.4);
		m_lastContractionEnd = -1;
		m_lastContractilityEnd = -1;

		m_upStart = 0;
		m_eventLastIndex = 0;
		m_pendingResults = "";

		m_fetus.set_hr_sample_rate(4);
		m_fetus.set_up_sample_rate(1);
	}

	///
	/// Simple destructor
	PatternsWorker::~PatternsWorker()
	{
	}

	///
	/// Add signal and process it
	/// Return the size of the buffer necessary to retrieve ALL results at once
	int PatternsWorker::ProcessHR(char* signal, long count)
	{
		// Process the FHR and calculate the events
		if (count > 0)
		{
			append_fhr(signal, count);
		}

		// Return the size of the results
		return m_pendingResults.length() == 0 ? 0: m_pendingResults.length() + 1;
	}

	///
	/// Add signal and process it
	/// Return the size of the buffer necessary to retrieve ALL results at once
	int PatternsWorker::ProcessUP(char* signal, long count)
	{
		// Process the UP and calculate contractions
		if (count > 0)
		{
			append_up(signal, count);
		}

		// Return the size of the results
		return m_pendingResults.length() == 0 ? 0 : m_pendingResults.length() + 1;
	}

	///
	/// Output the pending results into the specified buffer
	/// Return true if there are still some pending results that don't fit in the given buffer size
	bool PatternsWorker::ReadResults(char* output_buffer, long output_size)
	{
		// Clear the output buffer
		memset(output_buffer, 0, output_size);

		// The output buffer may be too small to retrieve all pending results at once...
		int pending_count = m_pendingResults.length();
		int returned_count = min(output_size - 1, pending_count); // - 1 is to ensure there is one byte for the \0 at the end of the string

		memcpy(output_buffer, m_pendingResults.c_str(), returned_count);

		if (returned_count == pending_count)
		{
			m_pendingResults = "";
			return false; // No pending results left
		}

		m_pendingResults = m_pendingResults.substr(returned_count, pending_count - returned_count);
		return true; // Some pending results left
	}

	///
	/// Append the up signal and calculate the contractions
	void PatternsWorker::append_up(char* signal, long count)
	{
		// Store the incoming signal
		m_upBuffer.reserve(m_upBuffer.size() + count);
		for (long i = 0; i < count; ++i)
		{
			m_upBuffer.push_back(signal[i]);
		}

		// Do the calculation NOW
		vector<patterns::contraction> detected_contractions;
		m_contractionEngine.detect(m_upBuffer, detected_contractions);

		long new_count = 0;
		long* new_contractions = new long[3 * detected_contractions.size()];

		for (vector<patterns::contraction>::iterator itr = detected_contractions.begin(); itr != detected_contractions.end(); ++itr)
		{
			patterns::contraction& curContraction = *itr;

			// Shift the contraction to reflect the offset of the current buffer of data
			curContraction += m_upStart;

			// Overlapping contractions are rejected
			if (curContraction.get_start() < m_lastContractionEnd)
			{
				continue;
			}

			// Contraction too close to the end are non final!
			if (curContraction.get_end() + 60 > (long)(m_upStart + m_upBuffer.size()))
			{
				continue;
			}

			// Remember last valid contraction end;
			m_lastContractionEnd = max(m_lastContractionEnd, curContraction.get_end());

			// Add the contraction to the buffer that the engine will consume
			new_contractions[(new_count * 3)] = 4 * curContraction.get_start();
			new_contractions[(new_count * 3) + 1] = 4 * curContraction.get_end();
			new_contractions[(new_count * 3) + 2] = 4 * curContraction.get_peak();
			++new_count;

			// Remember start index of detected contraction in order to later link decelerations and contractions
			m_pastContractions.insert(m_pastContractions.end(), 4 * curContraction.get_start());

			// Add the contraction to the result storage
			if (m_pendingResults.size() > 0)
			{
				m_pendingResults += "\r\n";
			}

			m_fetus.append_contraction(curContraction);
			m_pendingResults += ContractionToString(curContraction);
		}

		// If new final contractions were added to the list, send that to the patterns engine
		if (new_count > 0)
		{
			m_patternsEngine.Append_Contraction(new_count, new_contractions);
		}

		delete[] new_contractions;

		// If there is no contraction for a long time, we don't go back in time more that x indexes since the last calculation starting index
		if (m_upBuffer.size() > 300)
		{
			long removed = m_upBuffer.size() - 300;
			m_upBuffer.erase(m_upBuffer.begin(), m_upBuffer.begin() + removed);

			m_upStart += removed;
		}

		// Drop up signal up to the last final contraction
		if (m_lastContractionEnd > m_upStart)
		{
			long removed = m_lastContractionEnd - m_upStart;
			m_upBuffer.erase(m_upBuffer.begin(), m_upBuffer.begin() + removed);

			m_upStart = m_lastContractionEnd;
		}
	}

	///
	/// Append the fhr signal and calculate the events
	void PatternsWorker::append_fhr(char* signal, long count)
	{
		// The engine doesn't support 255 for NoData so replace all 255 with 0
		for (long i = 0; i < count; ++i)
		{
			if ((unsigned char)signal[i] == 255)
			{
				signal[i] = 0;
			}
		}

		m_patternsEngine.ProceedBuffer();
		m_patternsEngine.Append((unsigned char*)signal, count, NULL, 0);

		// Retrieve results
		if (m_patternsEngine.hasNewEvents())
		{
			patterns::fhrPartSet* fset = m_patternsEngine.GetTotalResults();
			long ntotal = fset->size();
			for (long i = m_eventLastIndex; i < ntotal; ++i)
			{
				patterns::event curEvent;
				fset->getAt(i)->toEvent(&curEvent, 0, 0);

				if (curEvent.is_final())
				{
					// Add the events to the result storage
					if (m_pendingResults.size() > 0)
					{
						m_pendingResults += "\r\n";
					}

					m_fetus.append_event(curEvent);
					m_pendingResults += EventToString(curEvent);
					++m_eventLastIndex;
				}
			}
		}
	}

	///
	// Helper function to convert a double to a string
	string convert2string(double x)
	{
		char t[255];
		sprintf_s(t, 255, "%.6f", x);
		return t;
	}

	///
	// Helper function to convert a long to a string
	string convert2string(long x)
	{
		char t[255];
		sprintf_s(t, 255, "%ld", x);
		return t;
	}

	///
	/// Find the start index of the contraction with the given index
	long PatternsWorker::FindContractionStart(int contractionIndex)
	{
		if (contractionIndex >= 0)
		{
			if (contractionIndex < (long)m_pastContractions.size())
			{
				return m_pastContractions[contractionIndex];
			}
			ASSERT(false);
		}
		return -1;
	}

	///
	// Convert an event to a string
	string PatternsWorker::EventToString(const patterns::event& curEvent)
	{
		string toRet;

		/* 00 */ 
		toRet.append("EVT");

		/* 01 */ 
		toRet.push_back(PIPE_SEPARATOR); 
		toRet.append(convert2string((long) curEvent.get_type()));

		/* 02 */ 
		toRet.push_back(PIPE_SEPARATOR); 
		toRet.append(convert2string(curEvent.get_start()));

		/* 03 */ 
		toRet.push_back(PIPE_SEPARATOR);
		toRet.append(convert2string(curEvent.get_end()));

		if (curEvent.is_baseline())
		{
			/* 04 */ 
			toRet.push_back(PIPE_SEPARATOR);
			toRet.append(convert2string(curEvent.get_y1()));

			/* 05 */ 
			toRet.push_back(PIPE_SEPARATOR);
			toRet.append(convert2string(curEvent.get_y2()));

			/* 06 */ 
			toRet.push_back(PIPE_SEPARATOR);
			toRet.append(convert2string(curEvent.get_baseline_var()));
		}
		else
		{
			/* 04 */ 
			toRet.push_back(PIPE_SEPARATOR);
			toRet.append(convert2string(curEvent.get_peak()));

			/* 05 */ 
			toRet.push_back(PIPE_SEPARATOR);
			toRet.append(convert2string(curEvent.get_peak_val()));

			/* 06 */ 
			toRet.push_back(PIPE_SEPARATOR);
			if (curEvent.is_confidence_set()) toRet.append(convert2string(curEvent.get_confidence()));

			/* 07 */ 
			toRet.push_back(PIPE_SEPARATOR);
			if (curEvent.is_repair_set()) toRet.append(convert2string(curEvent.get_repair()));

			/* 08 */ 
			toRet.push_back(PIPE_SEPARATOR);
			if (curEvent.is_height_set()) toRet.append(convert2string(curEvent.get_height()));

			/* 09 */ 
			toRet.push_back(PIPE_SEPARATOR);
			if (curEvent.is_noninterp()) toRet.push_back('y');

			if (curEvent.is_deceleration())
			{
				/* 10 */ 
				toRet.push_back(PIPE_SEPARATOR);
				long cs = FindContractionStart(curEvent.get_contraction());
				if (cs >= 0) 
					toRet.append(convert2string(cs));

				/* 11 */ 
				toRet.push_back(PIPE_SEPARATOR);
				toRet.append(convert2string(curEvent.get_atypical()));
			}
		}

		return toRet;
	}

	///
	// Convert a contraction to a string
	string PatternsWorker::ContractionToString(const patterns::contraction& curContraction)
	{
		string toRet;

		/* 00 */ 
		toRet.append("CTR");

		/* 01 */ 
		toRet.push_back(PIPE_SEPARATOR); 
		toRet.append(convert2string(curContraction.get_start()));

		/* 02 */ 
		toRet.push_back(PIPE_SEPARATOR);
		toRet.append(convert2string(curContraction.get_peak()));

		/* 03 */ 
		toRet.push_back(PIPE_SEPARATOR);
		toRet.append(convert2string(curContraction.get_end()));

		return toRet;
	}

}



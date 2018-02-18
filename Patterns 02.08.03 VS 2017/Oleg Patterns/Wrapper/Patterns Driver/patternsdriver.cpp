#include "stdafx.h"
#include "patternsdriver.h"
#include "patternsworker.h"
#include "SerialShield.h"
#include "..\..\Source\Patterns Application\PatternsVersionNumber.h"
#include "Threadlock.h"

namespace PatternsDriver
{
	void Trace(WORD type, LPCTSTR szFormat, ...)
	{
		static HANDLE m_hEventLog = NULL;
		try
		{
			TCHAR szBuf[0x2000];

			va_list pArg;
			va_start(pArg, szFormat);
			::_vsntprintf_s(szBuf, sizeof(szBuf) / sizeof(TCHAR), szFormat, pArg);
			va_end(pArg);

			::OutputDebugString(szBuf);
			if (szBuf[::_tcslen(szBuf) - 1] != _T('\n'))
			{
				::OutputDebugString(_T("\n"));
			}

			if (m_hEventLog == NULL)
			{
				m_hEventLog = RegisterEventSource(NULL, _T("PeriGen Patterns Engine"));
			}

			if (m_hEventLog != NULL)
			{
				LPCTSTR msg = (LPCTSTR)(&szBuf[0]);
				ReportEvent(m_hEventLog, type, 1, 0, NULL, 1,	0, &msg, NULL);
			}
		}
		catch(...)
		{
			// Just ignore
		}
	}

	// Make sure there is a proper license for the patterns engine on that computer
	bool EngineCheckLicense()
	{
		static SerialShield* m_SSptr = NULL;

		// For thread security, only ONE thread at a time can access the serial shield validation code
		static patterns::ThreadLock threadLock;
		patterns::ScopeThreadLock scopeThreadLock(&threadLock);

		try
		{
			// Initialization of the license validation engine
			if (m_SSptr == NULL)
			{
				m_SSptr = new SerialShield;
				if (m_SSptr == NULL)
				{
					Trace(EVENTLOG_ERROR_TYPE, "Failed to load the license validation engine, contact PeriGen Support (error reference 8801)");
					return false;
				}

				int ret = m_SSptr->InitClass();
				if (ret == 2)
				{
					delete m_SSptr;
					Trace(EVENTLOG_ERROR_TYPE, "Failed to initialize the license validation engine, contact PeriGen Support (error reference 8802)");
					return false;
				}

				if (ret == 1)
				{
					delete m_SSptr;
					Trace(EVENTLOG_ERROR_TYPE, "Incorrect version of the license validation engine, contact PeriGen Support (error reference 8803)");
					return false;
				}

				m_SSptr->SS_R("Bruno Bendavid", "lXcsBCcUEgf/vg+bEYzzb8r4cksDbB0wV5+isMmk9BvvayXF+j4fChYY2+L83URYqlTCIxoRL16PcHoMtHICEQ==");
				m_SSptr->SetApplicationInfo("PeriCALM Patterns Engine", "84591A5B-F651-430e-A17E-D2EDF096010B");

				m_SSptr->SS_Initialize();
			}

			// Validation of the license
			if (m_SSptr->SS_IsUnlocked())
			{
				return true;
			}

			if (m_SSptr->SS_TrialExpired())
			{
				Trace(EVENTLOG_ERROR_TYPE, "The license has expired.");
				return false;
			}

			switch (m_SSptr->SS_TrialMode())
			{
				// Date tempered
				case 99:
				{
					Trace(EVENTLOG_ERROR_TYPE, "Your date system has been moved back - License blocked.");
					return false;
				}

				// Time base / Run based / Date based
				case 1:
				case 2:
				case 3:
				case 4:
					return true;
			}

			Trace(EVENTLOG_ERROR_TYPE, "No license found on that computer.");
			return false;
		}
		catch (...) 
		{ 
			Trace(EVENTLOG_ERROR_TYPE, "Error while validating the license.");
			return false;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Create an engine instance
	HANDLE PATTERNSDRIVER_API __stdcall EngineInitialize()
	{
		// Check for a license
		if (!EngineCheckLicense())
			return NULL;

		return new PatternsWorker();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Cleanup an engine instance
	void PATTERNSDRIVER_API __stdcall EngineUninitialize(HANDLE param)
	{
		if (param == 0)
			return;

		PatternsWorker* pWorker = (PatternsWorker*)param;

		delete pWorker;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Detection of events
	int PATTERNSDRIVER_API __stdcall EngineProcessHR(HANDLE param, char* signal, long start, long count)
	{
		if (param == 0)
			throw exception("Invalid operation, no valid engine provided");

		if (start < 0)
			throw exception ("Invalid parameter, start is below 0");

		if (count < 0)
			throw exception ("Invalid parameter, count is below 0");

		PatternsWorker* pWorker = (PatternsWorker*)param;

		return pWorker->ProcessHR(&signal[start], count);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Detection of contractions
	int PATTERNSDRIVER_API __stdcall EngineProcessUP(HANDLE param, char* signal, long start, long count)
	{
		if (param == 0)
			throw exception("Invalid operation, no valid engine provided");

		if (start < 0)
			throw exception ("Invalid parameter, start is below 0");

		if (count < 0)
			throw exception ("Invalid parameter, count is below 0");

		PatternsWorker* pWorker = (PatternsWorker*)param;

		return pWorker->ProcessUP(&signal[start], count);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Read results of ProcessUP and ProcessHR
	bool PATTERNSDRIVER_API __stdcall EngineReadResults(HANDLE param, char* output_buffer, long output_size)
	{
		if (param == 0)
			throw exception("Invalid operation, no valid engine provided");

		PatternsWorker* pWorker = (PatternsWorker*)param;

		return pWorker->ReadResults(output_buffer, output_size);
	}

}
#include "stdafx.h"
#include "PatternsWorkerWrapper.h"
#include "SerialShield.h"
#include "..\Patterns Application\PatternsVersionNumber.h"
#include "Threadlock.h"
#include "patternsdriver.h"

namespace PatternsDriver
{
	void Trace1(WORD type, LPCTSTR szFormat, ...)
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
				ReportEvent(m_hEventLog, type, 1, 0, NULL, 1, 0, &msg, NULL);
			}
		}
		catch (...)
		{
			// Just ignore
		}
	}

	// Make sure there is a proper license for the patterns engine on that computer
	bool CheckLicense()
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
					Trace1(EVENTLOG_ERROR_TYPE, "Failed to load the license validation engine, contact PeriGen Support (error reference 8801)");
					return false;
				}

				int ret = m_SSptr->InitClass();
				if (ret == 2)
				{
					delete m_SSptr;
					Trace1(EVENTLOG_ERROR_TYPE, "Failed to initialize the license validation engine, contact PeriGen Support (error reference 8802)");
					return false;
				}

				if (ret == 1)
				{
					delete m_SSptr;
					Trace1(EVENTLOG_ERROR_TYPE, "Incorrect version of the license validation engine, contact PeriGen Support (error reference 8803)");
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
				Trace1(EVENTLOG_ERROR_TYPE, "The license has expired.");
				return false;
			}

			switch (m_SSptr->SS_TrialMode())
			{
				// Date tempered
			case 99:
			{
				Trace1(EVENTLOG_ERROR_TYPE, "Your date system has been moved back - License blocked.");
				return false;
			}

			// Time base / Run based / Date based
			case 1:
			case 2:
			case 3:
			case 4:
				return true;
			}

			Trace1(EVENTLOG_ERROR_TYPE, "No license found on that computer.");
			return false;
		}
		catch (...)
		{
			Trace1(EVENTLOG_ERROR_TYPE, "Error while validating the license.");
			return false;
		}
	}

	PatternsWorkerWrapper::PatternsWorkerWrapper()
	{
		if (!CheckLicense())
			m_pPatternsWorker = NULL;
		else
			m_pPatternsWorker = new PatternsWorker;
	}

	PatternsWorkerWrapper::~PatternsWorkerWrapper()
	{
		if (m_pPatternsWorker)
			delete m_pPatternsWorker;

		m_pPatternsWorker = NULL;
	}

	PatternsWorker* PatternsWorkerWrapper::GetEngine()
	{
		return m_pPatternsWorker;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Detection of events
	int PatternsWorkerWrapper::ProcessHR(char* signal, long start, long count)
	{
		if (m_pPatternsWorker == 0)
			throw exception("Invalid operation, no valid engine provided");

		if (start < 0)
			throw exception("Invalid parameter, start is below 0");

		if (count < 0)
			throw exception("Invalid parameter, count is below 0");

		return m_pPatternsWorker->ProcessHR(&signal[start], count);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Detection of contractions
	int PatternsWorkerWrapper::ProcessUP(char* signal, long start, long count)
	{
		if (m_pPatternsWorker == 0)
			throw exception("Invalid operation, no valid engine provided");

		if (start < 0)
			throw exception("Invalid parameter, start is below 0");

		if (count < 0)
			throw exception("Invalid parameter, count is below 0");

		return m_pPatternsWorker->ProcessUP(&signal[start], count);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////
	// Read results of ProcessUP and ProcessHR
	bool PatternsWorkerWrapper::ReadResults(char* output_buffer, long output_size)
	{
		if (m_pPatternsWorker == 0)
			throw exception("Invalid operation, no valid engine provided");

		return m_pPatternsWorker->ReadResults(output_buffer, output_size);
	}
}
#include "StdAfx.h"

#include "NativeManager.h"

bool is_file_exist(const char *fileName)
{
    std::ifstream infile(fileName);
    return infile.good();
}

NativeManager::NativeManager(void)
{
	/*bool ans = is_file_exist("Perigen.Patterns.NnetControls.dll");

	if(ans == true)
	{
		MessageBox(NULL, "Yes! ;-)", "", MB_OK);
	}
	else
{
		MessageBox(NULL, "No :-(", "", MB_OK);
	}
*/
	m_bInitialized = false;
	m_pPatternsAdapter = NULL;
	m_screenWidthInMinutes = 255;
	m_startTime = (DATE)0;
	m_patternsCtrl = NULL;
}

NativeManager::~NativeManager(void)
{
	if(m_bInitialized)
	{
		m_pPatternsAdapter->DisposeControls();
		m_pPatternsAdapter->Release();
		m_pPatternsAdapter = NULL;
	}
}

void NativeManager::Init(DATE startTime, double screenWidthInMinutes)
{
	m_screenWidthInMinutes = screenWidthInMinutes;
	m_startTime = startTime;
	if(!m_bInitialized)
	{
		try
		{
			m_pPatternsAdapter = new IPatternsWPFAdapterPtr(__uuidof(PatternsWPFAdapter));
			m_pPatternsAdapter->AddListener(this);
			m_pPatternsAdapter->SetStartTime(startTime);
			m_pPatternsAdapter->SetScreenWidth(screenWidthInMinutes);
		
			m_bInitialized = true;
		}
		catch(...)
		{
			m_pPatternsAdapter = NULL;
		}
	}
}

HRESULT NativeManager::QueryInterface(const IID & iid,void ** pp)
{
    if (iid == __uuidof(IPatternEvent) || iid == __uuidof(IUnknown))
    {
        *pp = this;
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

void NativeManager::InitNavigationPanel(long parentHandle, int x, int y, int width, int height)
{
	if(m_pPatternsAdapter)
	{
		VARIANT_BOOL retVal;
		m_pPatternsAdapter->InitNavigationPanel(parentHandle, x,  y, width, height, &retVal);
	}
}

void NativeManager::ResizeNavigationPanel(int x, int y, int width, int height)
{
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->ResizeNavigationPanel(x,  y, width, height);
}

void NativeManager::InitExportButton(long parentHandle, int x, int y, int width, int height)
{
	VARIANT_BOOL retVal;
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->InitExportButton(parentHandle, x,  y, width, height, &retVal);
}

void NativeManager:: ResizeExportButton(int x, int y, int width, int height)
{
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->ResizeExportButton(x,  y, width, height);
}

HRESULT NativeManager::RaiseMouseOverEvent(DATE from, DATE to)
{
	if(m_patternsCtrl != NULL && IsWindow(m_patternsCtrl))
	{
		EportBarActiveChunck activeChunk(from, to, -1);
		::SendMessageA(m_patternsCtrl, MOUSE_OVER_CALLBACK_MSG, 0, (LPARAM)&activeChunk);
	}
	return 0;
}

HRESULT NativeManager::RaiseMouseLeaveEvent(DATE from, DATE to)
{
	if(m_patternsCtrl != NULL && IsWindow(m_patternsCtrl))
	{		
		EportBarActiveChunck activeChunk(from, to, -1);
		::SendMessageA(m_patternsCtrl, MOUSE_LEAVE_CALLBACK_MSG, 0, (LPARAM)&activeChunk);
	}
	return 0;
}

HRESULT NativeManager::RaiseBtnPressedEvent(DATE from, DATE to, long id)
{
	if(m_patternsCtrl != NULL && IsWindow(m_patternsCtrl))
	{		
		EportBarActiveChunck activeChunk(from, to, id);
		::SendMessageA(m_patternsCtrl, MOUSE_OVER_CALLBACK_MSG, 1, (LPARAM)&activeChunk);
		
		if(m_pPatternsAdapter != NULL)
		{
			ExportEntriesStruct entries;
			GetExportCalculatedEntriesEx(from, to, id, entries);

			m_pPatternsAdapter->SetExportDialogParamsEx(entries.m_meanContructions,
														entries.m_meanBaseline,
														entries.m_meanBaselineVariability,
														entries.m_montevideo);
		}
	}
	return 0;
}


HRESULT NativeManager::RaiseExportDialogClosedEvent(DATE from, DATE to)
{
	if(m_patternsCtrl != NULL && IsWindow(m_patternsCtrl))
	{		
		EportBarActiveChunck activeChunk(from, to, -1);
		::SendMessageA(m_patternsCtrl, MOUSE_LEAVE_CALLBACK_MSG, 1, (LPARAM)&activeChunk);
	}
	return 0;
}

HRESULT NativeManager::RaiseTimeRangeChangedEvent(long timeRange)
{	
	if(m_patternsCtrl != NULL && IsWindow(m_patternsCtrl))
	{		
		::SendMessageA(m_patternsCtrl, TIME_RANGE_CHANGE_CALLBAK_MSG, 0, (LPARAM)timeRange);
	}
	return 0;
}

HRESULT NativeManager::SetExportableChunks(DATE startTime)
{
//	DATE t;
	time_t rawtime;
    struct tm * timeinfo;

	if(m_pPatternsAdapter)
	{
		time ( &rawtime );
		timeinfo = localtime ( &rawtime );

		m_pPatternsAdapter->BeginUpdateChunks();

		m_pPatternsAdapter->SetStartTime(m_startTime);

		m_pPatternsAdapter->EndUpdateChunks();
	}

	return S_OK;
}

void NativeManager::HideControls()
{
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->HideControls();
}

void NativeManager::SetStartTime(DATE startTime)
{
	m_startTime = startTime;

	if(m_pPatternsAdapter)
	{	
		m_pPatternsAdapter->SetStartTime(startTime);		
	}
}

void NativeManager::BeginUpdateChunks()
{
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->BeginUpdateChunks();
}

void NativeManager::AddChunk(int exportID, int intervalID, DATE startTime, int timeRange, bool isExported, long x1, long x2)
{
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->AddChunkEx(exportID, intervalID, startTime, timeRange, isExported, x1, x2);
}

void NativeManager::EndUpdateChunks()
{
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->EndUpdateChunks();
}

void NativeManager::SetTimeRange(int timeRange)
{
	if(m_pPatternsAdapter)
		m_pPatternsAdapter->SetTimeRange((long)timeRange);
}

void NativeManager::SetPluginURL(const CString& serverURL)
{
	if(m_pPatternsAdapter != NULL)
	{
		m_pPatternsAdapter->SetPluginURL(serverURL.AllocSysString());
	}
}

void NativeManager::SetInitParams(const CString& userID, int episodeID, bool canModify, HWND m_criHWND)
{
	if(m_pPatternsAdapter != NULL)
	{
		m_pPatternsAdapter->SetInitParams(userID.AllocSysString(), episodeID, canModify, (long)m_criHWND);
	}
}

void NativeManager::SetMontevideoVisible(bool isPressed)
{
	if(m_pPatternsAdapter != NULL)
	{
		m_pPatternsAdapter->SetMontevideoVisible(isPressed);
	}
}

void NativeManager::SetPanelTooltip(const CString  message, DATE end36Weeks)
{
	if(m_pPatternsAdapter != NULL)
	{
		m_pPatternsAdapter->SetPanelTooltip(message.AllocSysString(), end36Weeks);
	}
}

BSTR NativeManager::GetExportCalculatedEntries(DATE from, DATE to, long id)
{
	CString resultStr;
	if(m_patternsCtrl != NULL && IsWindow(m_patternsCtrl))
	{		
		EportBarActiveChunck activeChunk(from, to, id);
		
		::SendMessageA(m_patternsCtrl, GET_EXPORT_ENTRIES_MSG, (WPARAM)&resultStr, (LPARAM)&activeChunk);
	}
	return resultStr.AllocSysString();
}

void NativeManager::GetExportCalculatedEntriesEx(DATE from, DATE to, long id, ExportEntriesStruct& entries)
{
	if(m_patternsCtrl != NULL && IsWindow(m_patternsCtrl))
	{		
		EportBarActiveChunck activeChunk(from, to, id);
		
		::SendMessageA(m_patternsCtrl, GET_EXPORTENTRIES_MSG, (WPARAM)&entries, (LPARAM)&activeChunk);
	}
}
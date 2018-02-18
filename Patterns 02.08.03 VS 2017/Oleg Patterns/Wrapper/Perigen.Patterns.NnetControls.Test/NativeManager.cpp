#include "StdAfx.h"
#include "Perigen.Patterns.NnetControls.Test.h"
#include "Perigen.Patterns.NnetControls.TestDlg.h"
#include "NativeManager.h"
#include "PatternsExportableChunk.h"


NativeManager::NativeManager(void)
{
	m_pPatternsAdapter = new IPatternsWPFAdapterPtr(__uuidof(PatternsWPFAdapter));
	m_pPatternsAdapter->AddListener(this);
	m_pPatternsAdapter->SetScreenWidth(255);

	m_pPatternsAdapter->GetTestStartTime(&m_startTime);
}

NativeManager::~NativeManager(void)
{
	m_pPatternsAdapter->DisposeControls();
	m_pPatternsAdapter->Release();
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
	VARIANT_BOOL retVal;
	m_pPatternsAdapter->InitNavigationPanel(parentHandle, x,  y, width, height, &retVal);
}

void NativeManager::ResizeNavigationPanel(int x, int y, int width, int height)
{
	m_pPatternsAdapter->ResizeNavigationPanel(x,  y, width, height);
}

void NativeManager::InitExportButton(long parentHandle, int x, int y, int width, int height)
{
	VARIANT_BOOL retVal;
	m_pPatternsAdapter->InitExportButton(parentHandle, x,  y, width, height, &retVal);
}

void NativeManager:: ResizeExportButton(int x, int y, int width, int height)
{
		m_pPatternsAdapter->ResizeExportButton(x,  y, width, height);
}

void NativeManager::SetTimeRange(int timeRange)
{
	m_pPatternsAdapter->SetTimeRange((long)timeRange);
}

HRESULT NativeManager::RaiseMouseOverEvent(DATE from, DATE to)
{
	//AfxMessageBox(_T("RaiseMouseOverEvent!!!"));

	return 0;
}

HRESULT NativeManager::RaiseMouseLeaveEvent(DATE from, DATE to)
{
	//AfxMessageBox(_T("RaiseMouseOverEvent!!!"));

	return 0;
}

HRESULT NativeManager::RaiseBtnPressedEvent(DATE from, DATE to, long id)
{
	
	//AfxMessageBox(_T("RaiseBtnPressedEvent!!!"));

	return 0;
}

HRESULT NativeManager::RaiseExportDialogClosedEvent(DATE from, DATE to)
{
	
	return 0;
}

HRESULT NativeManager::RaiseTimeRangeChangedEvent(long timeRange)
{
	return 0;
}

HRESULT NativeManager::SetExportableChunks(DATE startTime)
{
	time_t rawtime;
    struct tm * timeinfo;

    time ( &rawtime );
    timeinfo = localtime ( &rawtime );

	m_pPatternsAdapter->BeginUpdateChunks();

	m_pPatternsAdapter->SetStartTime(m_startTime);

	//PatternsExportableChunkCPP chunk1;// = new PatternsExportableChunk();
	//chunk1.
	//m_pPatternsAdapter
	
	m_pPatternsAdapter->EndUpdateChunks();

	return S_OK;
}

void NativeManager::HideControls()
{
	m_pPatternsAdapter->HideControls();
}

DATE NativeManager::GetStartTime()
{
	DATE startTime;

	time_t rawtime;
    struct tm * timeinfo;

    time ( &rawtime );
    timeinfo = localtime ( &rawtime );

	m_pPatternsAdapter->BeginUpdateChunks();

	COleDateTime* x = new COleDateTime(timeinfo->tm_year, timeinfo->tm_mon, timeinfo->tm_mday, timeinfo->tm_hour, timeinfo->tm_min, timeinfo->tm_sec);

	return 0;
}

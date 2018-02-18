#include "StdAfx.h"
#include "PatternsExportableChunk.h"


PatternsExportableChunkCPP::PatternsExportableChunkCPP(void)
{	
}


PatternsExportableChunkCPP::~PatternsExportableChunkCPP(void)
{
}


HRESULT PatternsExportableChunkCPP::QueryInterface(const IID & iid,void ** pp)
{
    if (iid == __uuidof(IPatternsExportableChunk) || iid == __uuidof(IUnknown))
    {
        *pp = this;
        AddRef();
        return S_OK;
    }
    return E_NOINTERFACE;
}

HRESULT PatternsExportableChunkCPP::putStartTime(DATE startTime) 
{ 
    m_startTime = startTime;
	return S_OK;
}

HRESULT PatternsExportableChunkCPP::getStartTime(DATE* startTime) 
{
	startTime = &m_startTime;
	return S_OK;
}

HRESULT PatternsExportableChunkCPP::putTimeRange(long timeRange) 
{ 
    m_timeRange = timeRange;
	return S_OK;
}

HRESULT PatternsExportableChunkCPP::getTimeRange(long* timeRange) 
{
	timeRange = &m_timeRange;
    return S_OK;
}

HRESULT PatternsExportableChunkCPP::putIsExported(VARIANT_BOOL isExported) 
{ 
    m_isExported = isExported;
	return S_OK;
}

HRESULT PatternsExportableChunkCPP::getIsExported(VARIANT_BOOL* isExported) 
{
	isExported = &m_isExported;
	return S_OK;
}
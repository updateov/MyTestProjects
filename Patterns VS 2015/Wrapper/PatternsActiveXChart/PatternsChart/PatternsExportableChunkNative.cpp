#include "stdafx.h"
#include "PatternsExportableChunkNative.h"
#include "patterns, fetus.h"



PatternsExportableChunkNative::PatternsExportableChunkNative(void)
{
	m_startTime = 0;
	m_timeRange = 30;
	m_isExported = false;
	m_id = -1;
	m_exportID = -1;
}

PatternsExportableChunkNative::PatternsExportableChunkNative(time_t startTime, int timeRange, bool exported)
{
	m_startTime = startTime;
	m_timeRange = timeRange;
	m_isExported = exported;
	m_id = -1;
	m_exportID = -1;
}

PatternsExportableChunkNative::~PatternsExportableChunkNative(void)
{
}

DATE PatternsExportableChunkNative::ConvertTimeToDATE(time_t t)
{
	SYSTEMTIME sysTime = patterns::fetus::convert_to_local(patterns::fetus::convert_to_SYSTEMTIME(t));
	COleDateTime oleTime(sysTime);
	return (DATE)oleTime;
}


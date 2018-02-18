#pragma once

using namespace patterns;


class PatternsExportableChunkNative
{
public:
	PatternsExportableChunkNative(void);
	PatternsExportableChunkNative(time_t startTime, int timeRange, bool exported);
	virtual ~PatternsExportableChunkNative(void);
public:
	time_t GetStartTime() const
	{return m_startTime;}
	time_t GetEndTime() const
	{return m_startTime + m_timeRange * 60;}
	DATE GetStartDATE() const
	{return ConvertTimeToDATE(m_startTime);}
	int GetTimeRange() const
	{return m_timeRange;}
	bool IsExported() const
		{return m_isExported;}
	static DATE ConvertTimeToDATE(time_t t);
public:
	time_t m_startTime;
	int m_timeRange;
	bool m_isExported;
	long m_id;
	long m_exportID;
};



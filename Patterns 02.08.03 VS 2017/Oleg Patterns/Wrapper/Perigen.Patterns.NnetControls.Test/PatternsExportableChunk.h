#pragma once

class PatternsExportableChunkCPP : public IPatternsExportableChunk
{
public:

	PatternsExportableChunkCPP(void);
	~PatternsExportableChunkCPP(void);

	HRESULT __stdcall QueryInterface(const IID &, void **);
    ULONG __stdcall AddRef(void) { return 1; }
    ULONG __stdcall Release(void) { return 1; }

    HRESULT __stdcall putStartTime(DATE startTime);
    HRESULT __stdcall getStartTime(DATE* startTime);

    HRESULT __stdcall putTimeRange (long timeRange);
    HRESULT __stdcall getTimeRange (long * pRetVal);

    HRESULT __stdcall putIsExported (VARIANT_BOOL isExported);
    HRESULT __stdcall getIsExported (VARIANT_BOOL * pRetVal);

private:

	DATE m_startTime;
    long m_timeRange;
    VARIANT_BOOL m_isExported;
};


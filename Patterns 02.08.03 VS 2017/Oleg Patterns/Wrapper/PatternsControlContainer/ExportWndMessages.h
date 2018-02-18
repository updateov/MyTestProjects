#pragma once

struct ExportChunkStruct
{
	ExportChunkStruct(int exportID, int intervalID, DATE startTime, int timeRange, bool isExported, long x1, long x2)
	{
	m_exportID = exportID;
	m_intervalID = intervalID;
	m_startTime = startTime;
	m_timeRange = timeRange;
	m_isExported = isExported;
	m_x1 = x1;
	m_x2 = x2;
	}
	int m_exportID;
	int m_intervalID;
	double m_startTime;
	int m_timeRange;
	bool m_isExported;
	long m_x1;
	long m_x2;
};

struct PanelToolTip
{
	PanelToolTip(const CString message, DATE end36Weeks)
	{
		m_end36Weeks = end36Weeks;
		m_message = message;
	}
	DATE m_end36Weeks;
	CString m_message;
};

struct WPFAdapterInitParams
{
	WPFAdapterInitParams(const CString& userID, int episodeID, bool canModify, HWND criHWND)
	{
		 m_userID = userID;
		 m_episodeID = episodeID;
		 m_canModify = canModify;
		 m_criWnd = criHWND;

	}

	CString m_userID;
	int m_episodeID;
	bool m_canModify;
	HWND m_criWnd;
};

struct EportBarActiveChunck
{
	EportBarActiveChunck(DATE from, DATE to, long id)
	{ m_from = from; m_to = to; m_intervalID = id;}
	DATE m_from;
	DATE m_to;
	long m_intervalID;
};
struct ExportEntriesCalcStruct
{
	ExportEntriesCalcStruct()
	{
		m_meanContructions = -1.0;
		m_meanBaseline = -1;
		m_meanBaselineVariability = -1;
		m_montevideo = -1.0;
		m_contractionThresholdExceeded = 0;
	}
	double m_meanContructions;
	int m_meanBaseline;
	int m_meanBaselineVariability;
	double m_montevideo;
	bool m_contractionThresholdExceeded;
};
struct RoundExportValueStruct
{
	RoundExportValueStruct()
	{
		m_roundBaselineFHRValue = 1;
		m_roundByMontevideoUnits = 1;
	}
	int m_roundBaselineFHRValue;
	int m_roundByMontevideoUnits;
};

#define MSG_EXPORT_SET_START_TIME		(WM_USER + 1000)
#define MSG_EXPORT_SET_WND_SIZE			(WM_USER + 1001)
#define MSG_EXPORT_ADD_CHUNK			(WM_USER + 1002)
#define MSG_EXPORT_HIDE_CONTROLS		(WM_USER + 1003)
#define MSG_EXPORT_BEGIN_UPDATE			(WM_USER + 1004)
#define MSG_EXPORT_END_UPDATE			(WM_USER + 1005)
#define MSG_EXPORT_SET_RANGE			(WM_USER + 1006)
#define MSG_EXPORT_SET_TOOLTIP			(WM_USER + 1007)
#define MSG_EXPORT_SET_MONTEVIDEO		(WM_USER + 1008)
#define MSG_EXPORT_SET_INITPARAMS		(WM_USER + 1009)
#define MSG_EXPORT_SET_ROUND_VAL		(WM_USER + 1010)
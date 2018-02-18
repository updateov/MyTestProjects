#pragma once

#include "afxwin.h"

#define MIN_WEEKS_IN_DAYS 252


#define BOTTOM_HEIGHT 26
#define EXPORT_BAR_HEIGHT 32
#define BANNER_HEIGHT 36


#define EXPORT_PLUGIN_NAME "ExportPlugin"
#define CHECKLIST_PLUGIN_NAME "CheckListPlugin"

#define DEFAUULT_TRACINGS_VIEW_SIZE_MINUTES 15
#define DEFAULT_COMPRESSED_VIEW_SIZE_MINUTES 240

#define PATTERNS_DEV_VERSION "01.00.00.00"
#define PATTERNS_02_06_00 "02.06.00"
#define PATTERNS_02_08_00 "02.08.00"
#define PATTERNS_02_08_01 "02.08.01"
#define PATTERNS_02_08_02 "02.08.02"
#define PATTERNS_02_04_00 "02.04.00"

const int patterns_minimum_width = 700;
double patterns_ratio_coefficient(double tracingsViewSizeInMinutes,	double compressedViewSizeInMinutes);

//{
//	return	((210 / 90.) / tracingsViewSizeInMinutes)	// HR 15 minutes
//		+ ((100 / 75.) / tracingsViewSizeInMinutes)		// UP 15 minutes
//		+ ((210 / 90.) / compressedViewSizeInMinutes)	// HR 4 hours
//		+ ((100 / 75.) / compressedViewSizeInMinutes)	// UP 4 hours
//		+ ((200 / 100.) / compressedViewSizeInMinutes)	// CR 4 hours
//		+ ((250 / 100.) / compressedViewSizeInMinutes);	// Proportional blank inter space 
//}

const int patterns_fix_required_height = 197;


/// <summary>
/// The different type of action
/// </summary>
enum ActionTypes
{
	eActionStrikeoutContraction = 1,
	eActionStrikeoutEvent = 2,
	eActionConfirmEvent = 3,
	eActionUndoStrikeoutEvent = 4,
	eActionUndoStrikeoutContraction = 5,
	eMVUButtonOn = 6,
	eMVUButtonOff = 7,
};
const int MVUButtonDummyArtifactID = -99;
/// <summary>
/// The status of that patient
/// </summary>
enum PatientStatus
{
	ePatientInvalid = 0,
	ePatientLive = 1,
	ePatientUnplugged = 2,
	ePatientRecovery = 3,
	ePatientError = 4,
	ePatientLate = 5,
};

class CEmptyBarWnd : public CWnd
{
public:
	CEmptyBarWnd(){}
	virtual ~CEmptyBarWnd(){}
protected:
	DECLARE_MESSAGE_MAP()
	afx_msg void OnPaint();


};

std::wstring utf8_to_wstr(const std::string &utf8);
std::string wstr_to_str( const std::wstring& wstr );
string& cleanString(string& str);
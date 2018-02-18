#pragma once

#include "afxwin.h"

#define MIN_WEEKS_IN_DAYS 252


#define BOTTOM_HEIGHT 26
#define EXPORT_BAR_HEIGHT 32
#define BANNER_HEIGHT 36


#define EXPORT_PLUGIN_NAME "ExportPlugin"

const int patterns_minimum_width = 700;
const double patterns_ratio_coefficient =	((210 / 90.) / 17.)		// HR 17 minutes
										+	((100 / 75.) / 17.)		// UP 17 minutes
										+	((210 / 90.) / 120.)	// HR 2 hours
										+	((100 / 75.) / 120.)	// UP 2 hours
										+	((200 / 100.) / 120.)	// CR 2 hours
										+	((250 / 100.) / 120.);	// Proportional blank inter space 

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
};

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
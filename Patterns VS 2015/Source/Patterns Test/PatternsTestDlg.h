// PatternsDlg.h : header file
#pragma once
#include "afxwin.h"
#include <vector>

using namespace patterns;
using namespace std;

//
// =======================================================================================================================
//    CPatternsDlg dialog
// =======================================================================================================================
//
class CPatternsTestDlg :
	public CDialog
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	private:
		long m_ContractionCount;
		CListBox m_Contractions_Old;
		CListBox m_Contractions_New;
		UINT_PTR m_Timer;
#ifdef USE_CALM_DLL
		LMSValue::Time m_LastAbsTime;
#endif
		bool Compare(vector<contraction> &, vector<contraction> &);
		void ComputeDir(string);
		void ComputeFile(string, bool saveToFile = false);
		vector<patterns::contraction> ComputeNew(vector<char> &);
#ifdef USE_CALM_DLL
		LMSValue::ContractionSet ComputeOld(LMSValue::ReadingSet &);
		LMSValue::ReadingSet ConvertUPArrayToRS(vector<char> &);
		vector<contraction> ConvertRSToPCArray(LMSValue::ContractionSet &, LMSValue::ReadingSet &);
#endif
		vector<char> ImportUP(string);
		void ResetContent(void);
		void SaveToFile(string, string, long, long, long, bool);
		void SetStatus(string);

#ifdef USE_CALM_DLL
		void ShowContractions(LMSValue::ContractionSet &);
#endif
		void ShowContractions(vector<contraction> &, vector<char> &, CListBox *);
		string StandardOpenDir(void);
		string StandardOpenFile(void);

	//
	// -------------------------------------------------------------------------------------------------------------------
	//    Implementation
	// -------------------------------------------------------------------------------------------------------------------
	//
	protected:
		HICON m_hIcon;
		CStatic m_Status;
		CStatic m_Time_Old;
		CStatic m_Time_New;
		CStatic m_Time_Diff;
		CStatic m_TimePointsRatio;
		CStatic m_Nb_Points;
		CButton m_Compute_All;
		CEdit m_Test_Diagnostic;
		CStatic m_Nb_Contractions_Old;
		CStatic m_Nb_Contractions_New;
		CEdit m_Nb_Iteration;

		// Generated message map functions
		virtual void DoDataExchange(CDataExchange *pDX);	// DDX/DDV support
		virtual afx_msg void OnBnClickedCompute(void);
		virtual afx_msg void OnBnClickedComputeAll(void);
		virtual afx_msg void OnBnClickedReset(void);
		virtual afx_msg void OnBnClickedTest(void);
		virtual afx_msg void OnBnClickedTestLimits(void);
		virtual BOOL OnInitDialog(void);
		virtual afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
		virtual afx_msg void OnPaint(void);
		virtual afx_msg HCURSOR OnQueryDragIcon(void);
		virtual afx_msg void OnTimer(UINT nIDEvent);
		DECLARE_MESSAGE_MAP()
		virtual string TestLimits(long);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		// Construction
		CPatternsTestDlg(CWnd *pParent = NULL); // standard constructor
		~ CPatternsTestDlg(void);

		// Dialog Data
		enum { IDD = IDD_PATTERNS_DEBUG };
};

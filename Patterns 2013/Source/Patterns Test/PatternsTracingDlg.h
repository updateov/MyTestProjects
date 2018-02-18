#pragma once

#include "afxwin.h"

#include "Patterns gui, tracing.h"
#include "Patterns, fetus.h"


//
// =======================================================================================================================
//    PatternsTracingDlg dialog
// =======================================================================================================================
//
class CTestFetus : public fetus
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		CFHRSignal *GetFHRSignal(void)
		{
			return m_devents;
		}
};

class CPatternsTracingDlg :
	public CDialog
{
		DECLARE_DYNAMIC(CPatternsTracingDlg)
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		HICON m_hIcon;
		enum { kFetus1, kFetus2 };

		CComboBox m_Input_2;
		CComboBox m_Input_1;
		CButton m_ProcessCtrl;
		CButton m_ProcessCtrl1;
		CButton m_LoopCtrl;
		CButton m_StepCtrl;
		CButton m_RandCtrl;
		CButton m_WaitCtrl;
		CButton m_RepRej1Ctrl;
		CButton m_RepRej2Ctrl;
		CButton m_DisRep1Ctrl;
		CButton m_DisRep2Ctrl;
		CEdit m_BS1edit;
		CEdit m_BS2edit;
		CEdit m_TimeAccel;
		patterns_gui::tracing m_Tracing;
		patterns_gui::tracing m_Tracing_Compressed;
		patterns::fetus m_fetus_1;
		patterns::fetus m_fetus_2;

		virtual void DoDataExchange(CDataExchange *pDX);	// DDX/DDV support
		virtual string GetFileContent(string);
		virtual BOOL OnInitDialog(void);
		virtual void Rearrange(long w = 0, long h = 0);
		virtual void Recompute(patterns::fetus &);
		virtual void ResetFetus(long);
		virtual string StandardOpenFile(string);

		virtual afx_msg void OnPaint(void);
		virtual afx_msg HCURSOR OnQueryDragIcon(void);

		// CTestFetus* m_fetusCopy;
		// patterns::fetus m_fetusCopy;
		int m_iBS1sec;		// block size in seconds
		int m_iBS2sec;
		int m_iTimeAccel;	// time acceleration for RT sims
		long m_lFHRindex1;
		long m_lUPindex1;
		long m_lFHRindex2;
		long m_lUPindex2;

		bool m_bProcess1;
		bool m_bProcess2;
		bool m_bFromFile1;
		bool m_bFromFile2;

		bool m_bStep;
		bool m_bWaitFinish; // do not append more data until returns results from last chunk - ensures consistent BS to patterns engine
		bool m_bRandomBS;	// use different random block size every append
		bool m_bRepRej1;
		bool m_bRepRej2;
		bool m_bDisRep1;
		bool m_bDisRep2;
		bool bLoop; // loop (only for tracing 1 - for load testing)
		long numLoops;

		double m_dWinSizeMinutes;

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		CPatternsTracingDlg(CWnd *pParent = NULL);	// standard constructor
		virtual ~CPatternsTracingDlg(void);

		// Dialog Data
		enum { IDD = IDD_PATTERNS_TRACING };

		DECLARE_MESSAGE_MAP()
		afx_msg void OnBnClickedDebug(void);
		afx_msg void OnCbnSelchangeInput1(void);
		afx_msg void OnBnClickedReset1(void);
		afx_msg void OnBnClickedReset2(void);
		afx_msg void OnCbnSelchangeInput2(void);
		afx_msg void OnBnClickedRecompute1(void);
		afx_msg void OnBnClickedRecompute2(void);
		afx_msg void OnSize(UINT nType, int cx, int cy);
		afx_msg void OnBnClickedBrowse1(void);
		afx_msg void OnBnClickedBrowse2(void);
		afx_msg void OnBnClickedRandomInterval(void);
		afx_msg void OnBnClickedCompare21(void);
		afx_msg void OnBnClickedCompare12(void);
		afx_msg void OnBnClickedProcess2(void);
		afx_msg void OnBnClickedProcess1(void);
		afx_msg void OnBnClickedLoop(void);
		afx_msg void OnBnClickedStep(void);
		afx_msg void OnBnClickedWait(void);
		afx_msg void OnBnClickedRandom(void);
		afx_msg void OnChangeBS1(void);
		afx_msg void OnChangeBS2(void);
		afx_msg void OnChangeTimeAccel(void);
		afx_msg void OnBnClickedRepRej1(void);
		afx_msg void OnBnClickedRepRej2(void);
		afx_msg void OnBnClickedDisRep1(void);
		afx_msg void OnBnClickedDisRep2(void);
		afx_msg void OnBnClickedReboot(void);
		void OnTimer(UINT_PTR);
		void Append1(void);
		void Append2(void);
		void SetRandomBS(void);
		void CompareSets(bool TwoToOne);
};

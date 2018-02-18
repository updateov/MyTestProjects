#ifndef classe_dialogue_developpement
#define classe_dialogue_developpement

#include "patterns gui, tracing.h"

class dialogue_developpement :
	public CDialog
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		DECLARE_MESSAGE_MAP()
		patterns_gui::tracing f;
		HICON m_hIcon;
		enum { IDD = IDD_DEVELOPPEMENT_DIALOG };

		virtual void DoDataExchange(CDataExchange *);
		virtual BOOL OnInitDialog(void);
		afx_msg void OnSize(UINT, int, int);
		virtual void place_controles(void);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		dialogue_developpement(CWnd * = NULL);
};
#endif

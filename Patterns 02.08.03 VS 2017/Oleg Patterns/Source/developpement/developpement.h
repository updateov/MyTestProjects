#ifndef classe_developpement
#define classe_developpement

#include "resource.h"

class application_developpement :
	public CWinApp
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		application_developpement(void);

		virtual BOOL InitInstance(void);
		DECLARE_MESSAGE_MAP()
};

extern application_developpement theApp;
#endif

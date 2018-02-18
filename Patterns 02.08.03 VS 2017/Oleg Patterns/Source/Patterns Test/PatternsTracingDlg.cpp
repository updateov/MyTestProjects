// PatternsTestTracing.cpp : implementation file
#include "stdafx.h"
#include "PatternsTest.h"
#include "./patternstracingdlg.h"
#include "PatternsTestDlg.h"

#include "patterns, samples.h"

#include "fhrPart.h"
#include "fhrPartSet.h"
#include "FhrPartsCompare.h"

#define SAMPLE1STR		"Sample 1"
#define SAMPLE2STR		"Sample 2"
#define SAMPLE3STR		"Sample 3"
#define SAMPLE4STR		"Sample 4"
#define SAMPLE5STR		"Sample 5"
#define SAMPLE6STR		"Sample 6"
#define OTHERSTR		"Other"
#define SAMPLECOUNT		6
#define BTNWIDTH		80
#define BTNHEIGHT		24
#define MARGIN			6
#define COLUMN1LEFT		MARGIN
#define COLUMN2LEFT		(4 * MARGIN) + (int) (2.5 * BTNWIDTH)
#define COLUMN3LEFT		(8 * MARGIN) + (int) (7.5 * BTNWIDTH)
#define MAXDIALOGWIDTH	(4 * MARGIN) + (5 * BTNWIDTH)

// define MIN_SCREEN_MINUTES 16.5 #define MIN_SCREEN_MINUTES 17.0 #define
// MAX_SCREEN_MINUTES 8.0
#define MIN_RANDOM_BS	1
#define MAX_RANDOM_BS	120
#define WARM_UP_SAMPLES 921 // warm-up period for engine - need to use if 'wait' option enabled

// PatternsTestTracing dialog
class CTestFetus;

IMPLEMENT_DYNAMIC(CPatternsTracingDlg, CDialog)

/*
 =======================================================================================================================
 =======================================================================================================================
 */
CPatternsTracingDlg::CPatternsTracingDlg(CWnd *pParent /* NULL */ ) :
	CDialog(CPatternsTracingDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_iBS1sec = 60;		// block size in seconds
	m_iBS2sec = 60;
	m_iTimeAccel = 60;	// time acceleration for RT sims
	m_lFHRindex1 = 0;
	m_lUPindex1 = 0;
	m_lFHRindex2 = 0;
	m_lUPindex2 = 0;
	m_bProcess2 = false;
	m_bProcess1 = false;
	m_bStep = false;
	m_bFromFile1 = false;
	m_bFromFile2 = false;
	m_dWinSizeMinutes = 17.0;
	m_bWaitFinish = true;
	m_bRandomBS = false;
	m_bRepRej1 = true;
	m_bRepRej2 = true;
	m_bDisRep1 = false;
	m_bDisRep2 = false;
	bLoop = false;
	numLoops = 0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
CPatternsTracingDlg::~CPatternsTracingDlg(void)
{
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::DoDataExchange(CDataExchange *pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_INPUT_2, m_Input_2);
	DDX_Control(pDX, IDC_INPUT_1, m_Input_1);
	DDX_Control(pDX, IDC_PROCESS, m_ProcessCtrl);
	DDX_Control(pDX, IDC_PROCESS1, m_ProcessCtrl1);
	DDX_Control(pDX, IDC_STEP, m_StepCtrl);
	DDX_Control(pDX, IDC_WAITBS, m_WaitCtrl);
	DDX_Control(pDX, IDC_RANDBS, m_RandCtrl);
	DDX_Control(pDX, IDC_REPREJ1, m_RepRej1Ctrl);
	DDX_Control(pDX, IDC_REPREJ2, m_RepRej2Ctrl);
	DDX_Control(pDX, IDC_DISREP1, m_DisRep1Ctrl);
	DDX_Control(pDX, IDC_DISREP2, m_DisRep2Ctrl);
	DDX_Control(pDX, IDC_BS1, m_BS1edit);
	DDX_Control(pDX, IDC_BS2, m_BS2edit);
	DDX_Control(pDX, IDC_TIMEACCEL, m_TimeAccel);
}

BEGIN_MESSAGE_MAP(CPatternsTracingDlg, CDialog)
	ON_WM_SIZE()
	ON_WM_TIMER()
	ON_BN_CLICKED(IDC_DEBUG, OnBnClickedDebug)
	ON_CBN_SELCHANGE(IDC_INPUT_1, OnCbnSelchangeInput1)
	ON_BN_CLICKED(IDC_RESET_1, OnBnClickedReset1)
	ON_BN_CLICKED(IDC_RESET_2, OnBnClickedReset2)
	ON_CBN_SELCHANGE(IDC_INPUT_2, OnCbnSelchangeInput2)
	ON_BN_CLICKED(IDC_RECOMPUTE_1, OnBnClickedRecompute1)
	ON_BN_CLICKED(IDC_RECOMPUTE_2, OnBnClickedRecompute2)
	ON_BN_CLICKED(IDC_BROWSE_1, OnBnClickedBrowse1)
	ON_BN_CLICKED(IDC_BROWSE_2, OnBnClickedBrowse2)
// ON_BN_CLICKED(IDC_RANDOM_INTERVAL, OnBnClickedRandomInterval)
	ON_BN_CLICKED(IDC_COMPARE_2_1, OnBnClickedCompare21)
	ON_BN_CLICKED(IDC_COMPARE_1_2, OnBnClickedCompare12)
	ON_BN_CLICKED(IDC_PROCESS, OnBnClickedProcess2)
	ON_BN_CLICKED(IDC_PROCESS1, OnBnClickedProcess1)
	ON_BN_CLICKED(IDC_STEP, OnBnClickedStep)
	ON_BN_CLICKED(IDC_WAITBS, OnBnClickedWait)
	ON_BN_CLICKED(IDC_RANDBS, OnBnClickedRandom)
	ON_BN_CLICKED(IDC_REPREJ1, OnBnClickedRepRej1)
	ON_BN_CLICKED(IDC_REPREJ2, OnBnClickedRepRej2)
	ON_BN_CLICKED(IDC_DISREP1, OnBnClickedDisRep1)
	ON_BN_CLICKED(IDC_DISREP2, OnBnClickedDisRep2)
	ON_EN_CHANGE(IDC_BS1, OnChangeBS1)
	ON_EN_CHANGE(IDC_BS2, OnChangeBS2)
	ON_EN_CHANGE(IDC_TIMEACCEL, OnChangeTimeAccel)
	ON_BN_CLICKED(IDC_REBOOT, OnBnClickedReboot)
END_MESSAGE_MAP()

/*
 =======================================================================================================================
    Returns content of the given file.
 =======================================================================================================================
 */
string CPatternsTracingDlg::GetFileContent(string filename)
{
	/*~~~~~~~~~~~~~~~~*/
	string str;
	CFile *pFile = NULL;
	/*~~~~~~~~~~~~~~~~*/

	pFile = new CFile(filename.c_str(), CFile::modeRead | CFile::shareDenyNone);
	if (pFile)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		char *buf = 0;
		long n = (long) pFile->GetLength();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		buf = new char[n];
		if (buf)
		{
			pFile->Read(buf, n);
			str = buf;
			delete buf;
		}

		pFile->Close();
		delete pFile;
	}

	return str;
}

/*
 =======================================================================================================================
    Dialog init.
 =======================================================================================================================
 */
BOOL CPatternsTracingDlg::OnInitDialog(void)
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog. The framework does this automatically when the
	// application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);		// Set big icon
	SetIcon(m_hIcon, FALSE);	// Set small icon

	/*~~~~*/
	CRect r;
	/*~~~~*/

	GetDlgItem(IDC_TRACING_1)->GetWindowRect(&r);
	ScreenToClient(&r);
	m_Tracing.DisableToggleZoom();
	m_Tracing.Create(NULL, "", WS_CHILD | WS_TABSTOP | WS_VISIBLE, r, this, 0);
	m_Tracing.set_type(patterns_gui::tracing::tnormal);
	m_Tracing.set_paper(patterns_gui::tracing::pusa);
	m_Tracing.set_scaling_mode(patterns_gui::tracing::spaper);
	m_Tracing.show_grid();
	m_Tracing.lock_scaling();
	m_Tracing.get()->set_as_real_time();
	m_Tracing.set_lengths((long) (m_dWinSizeMinutes * 60), -1, SLIDER_VIEW_SIZE_SEC);
	m_Tracing.set_has_debug_keys(true);
	m_Tracing.show(patterns_gui::tracing::wbaselinevariability, true);
	m_Tracing.show(patterns_gui::tracing::wparerclassification, true);
	m_Tracing.show(patterns_gui::tracing::wdecellag, true);
	m_Tracing.show(patterns_gui::tracing::wbaselines, true);

	GetDlgItem(IDC_TRACING_2)->GetWindowRect(&r);
	ScreenToClient(&r);

	m_Input_1.InsertString(0, SAMPLE1STR);
	m_Input_1.InsertString(1, SAMPLE2STR);
	m_Input_1.InsertString(2, SAMPLE3STR);
	m_Input_1.InsertString(3, SAMPLE4STR);
	m_Input_1.InsertString(4, SAMPLE5STR);
	m_Input_1.InsertString(5, SAMPLE6STR);
	m_Input_1.InsertString(6, OTHERSTR);

	m_Input_2.InsertString(0, SAMPLE1STR);
	m_Input_2.InsertString(1, SAMPLE2STR);
	m_Input_2.InsertString(2, SAMPLE3STR);
	m_Input_2.InsertString(3, SAMPLE4STR);
	m_Input_2.InsertString(4, SAMPLE5STR);
	m_Input_2.InsertString(5, SAMPLE6STR);
	m_Input_2.InsertString(6, OTHERSTR);

	m_Input_1.SetCurSel(m_Input_1.GetCount() - 1);
	m_Input_2.SetCurSel(m_Input_1.GetCount() - 1);

	Rearrange();
	SetTimer(0, 1000, 0);
	return TRUE;
}


//
// =======================================================================================================================
//    The system calls this function to obtain the cursor to display while the user drags the minimized window.
// =======================================================================================================================
//
HCURSOR CPatternsTracingDlg::OnQueryDragIcon(void)
{
	return static_cast<HCURSOR>(m_hIcon);
}

//
// =======================================================================================================================
//    If you add a minimize button to your dialog, you will need the code below to draw the icon. For MFC applications
//    using the document/view model, this is automatically done for you by the framework.
// =======================================================================================================================
//
void CPatternsTracingDlg::OnPaint(void)
{
	if (IsIconic())
	{
		/*~~~~~~~~~~~~~~*/
		CPaintDC dc(this);	// device context for painting
		//~~~~~~~~~~~~~~

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM> (dc.GetSafeHdc()), 0);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		GetClientRect(&rect);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

/*
 =======================================================================================================================
    Browse button clicked - fetus 1.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedBrowse1(void)
{
	m_bProcess1 = false;

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// halt processing while browsing for file
	string filename = StandardOpenFile("IN Files (*.IN)|*.IN;|XML Files (*.XML)|*.XML|AllFiles(*.*)|*.*|");
	fetus* fNew = m_Tracing.NewFetus();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (filename.length() != 0)
	{
		// select "Other" in the combo box.
		m_Input_1.SetCurSel(m_Input_1.GetCount() - 1);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string str = GetFileContent(filename);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_Tracing.get()->reset();
		*m_Tracing.get() = *fNew;
		m_Tracing.get()->import(str);
		m_Tracing.get()->set_cutoff_date(0);
		m_Tracing.get()->set_start_date(1173686400); // so GA is in range
		m_fetus_1.reset();
		m_fetus_1 = *fNew;
		m_fetus_1 = *m_Tracing.get();
		m_fetus_1.set_cutoff_date(0);
		m_fetus_1.set_start_date(1173686400);		// so GA is in range
		m_bFromFile1 = true;
		m_lFHRindex1 = 0;
		m_lUPindex1 = 0;
		SetWindowText(filename.c_str());

		// m_bProcess1 = false;
	}
	delete fNew;
}

/*
 =======================================================================================================================
    Browse button clicked - fetus 2.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedBrowse2(void)
{
	m_bProcess2 = false;

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string filename = StandardOpenFile("IN Files (*.IN)|*.IN;|XML Files (*.XML)|*.XML|AllFiles(*.*)|*.*|");
	fetus* fNew = m_Tracing.NewFetus();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (filename.length() != 0)
	{
		// select "Other" in the combo box.
		m_Input_2.SetCurSel(m_Input_2.GetCount() - 1);

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		string str = GetFileContent(filename);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_Tracing.get_partial()->reset();
		*m_Tracing.get_partial() = *fNew;
		m_Tracing.get_partial()->import(str);
		m_Tracing.get_partial()->set_cutoff_date(0);
		m_Tracing.get_partial()->set_start_date(1173686400); // so GA is in range
		m_fetus_2.reset();
		m_fetus_2 = *fNew;
		m_fetus_2 = *m_Tracing.get_partial();
		m_fetus_2.set_cutoff_date(0);
		m_fetus_2.set_start_date(1173686400);				// so GA is in range
		m_bFromFile2 = true;
		m_lFHRindex2 = 0;
		m_lUPindex2 = 0;

		// m_bProcess2 = false;
	}
	delete fNew;
}

/*
 =======================================================================================================================
    Open debug dialog. This dialog allows to calculate the contractions with the old (calm) and new code.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedDebug(void)
{
	/*~~~~~~~~~~~~~~~~~*/
	CPatternsTestDlg dlg;
	/*~~~~~~~~~~~~~~~~~*/

	dlg.DoModal();
}

/*
 =======================================================================================================================
    Create interval from fetus 1 and put results in the partial fetus. Then compare the contractions of both fetus.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedRandomInterval(void)
{
	/*~~~~~*/
	long i;
	fetus* f0;
	/*~~~~~*/

	f0 = m_Tracing.get();

	// f0.adjust_up();
	// calculate contractions from a subset of the array of up.
	srand(clock());

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	double n00 = (double) rand() * (double) f0->get_number_of_up() / (double) RAND_MAX;
	double n01 = (double) rand() * (double) f0->get_number_of_up() / (double) RAND_MAX;
	long n0;
	long n1;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	n0 = (long) n00;
	n1 = (long) n01;
	if (n0 > n1)
	{
		/*~~~~~~~~~*/
		long n2 = n1;
		/*~~~~~~~~~*/

		n1 = n0;
		n0 = n2;
	}

	/*~~~~~~~~~~~~~~~~~~~*/
	// build the subset array.
	vector<char> up_subset;
	/*~~~~~~~~~~~~~~~~~~~*/

	for (i = n0; i <= n1; ++i)
	{
		up_subset.insert(up_subset.end(), (char) (unsigned char) f0->get_up(i));
	}

	/*~~~~~~~~~~~~~~~~~~~~~*/
	// detect the contractions from the subset array of up.
	contraction_detection cd;
	/*~~~~~~~~~~~~~~~~~~~~~*/

	cd.set_latent_vector_cut_off(0.4);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	vector<contraction> c_subset;
	cd.detect(up_subset, c_subset);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	f0->ResetContractions();
	for (i = 0; i < (long) c_subset.size(); ++i)
	{
		f0->append_contraction(contraction(4 * (c_subset[i].get_start() + n0), 4 * (c_subset[i].get_peak() + n0), 4 * (c_subset[i].get_end() + n0)));
	}

	*m_Tracing.get_partial() = *f0;
}

/*
 =======================================================================================================================
    Recompute contraction of fetus 1.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedRecompute1(void)
{
	Recompute(*m_Tracing.get());
	m_lFHRindex1 = 0;
	m_lUPindex1 = 0;
}

/*
 =======================================================================================================================
    Recompute contraction of fetus 2.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedRecompute2(void)
{
	Recompute(*m_Tracing.get_partial());
	m_lFHRindex2 = 0;
	m_lUPindex2 = 0;
}

/*
 =======================================================================================================================
    Reset "fetus 1" button is pressed
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedReset1(void)
{
	ResetFetus(kFetus1);
}

/*
 =======================================================================================================================
    Reset "fetus 2" button is pressed
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedReset2(void)
{
	ResetFetus(kFetus2);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedCompare12(void)
{
	CompareSets(false);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedCompare21(void)
{
	CompareSets(true);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::CompareSets(bool TwoToOne)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~*/
	CFhrPartsCompare partsComp;
	fhrPartSet set1;
	fhrPartSet set2;
	fhrPartSet currSet1;
	fhrPartSet currSet2;
	CString sR;
	long n = m_Tracing.get()->get_number_of_fhr();
	/*~~~~~~~~~~~~~~~~~~~~~~~*/

	if (TwoToOne)
	{
		partsComp.SetExpert((m_Tracing.get()));
		partsComp.SetTest((m_Tracing.get_partial()));
		set1 = *(partsComp.GetExpert());
		set2 = *(partsComp.GetTest());
	}
	else
	{
		partsComp.SetTest((m_Tracing.get()));
		partsComp.SetExpert((m_Tracing.get_partial()));
		set1 = *(partsComp.GetExpert());
		set2 = *(partsComp.GetTest());
	}

	set1.removeNonInterp();
	set2.removeNonInterp();

	// decel
	currSet1 = set1;
	currSet1.filterByType(fhrPart::FhrPartType_DECEL);
	currSet2 = set2;
	currSet2.filterByType(fhrPart::FhrPartType_DECEL);
	partsComp.SetExpert(&currSet1, n);
	partsComp.SetTest(&currSet2);
	partsComp.Compare();

	sR.Format("%d", partsComp.Detect());
	GetDlgItem(IDC_DEC_DETECT)->SetWindowText(sR);
	sR.Format("%d", partsComp.Miss());
	GetDlgItem(IDC_DEC_MISS)->SetWindowText(sR);
	sR.Format("%d", partsComp.FP());
	GetDlgItem(IDC_DEC_FP)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.SampSens() * 100.0));
	GetDlgItem(IDC_DEC_SAMPSENS)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.SampPPV() * 100.0));
	GetDlgItem(IDC_DEC_SAMPPPV)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.Sens() * 100.0));
	GetDlgItem(IDC_DEC_SENS)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.PPV() * 100.0));
	GetDlgItem(IDC_DEC_PPV)->SetWindowText(sR);

	// accel
	partsComp.Clear();
	currSet1 = set1;
	currSet1.filterByType(fhrPart::FhrPartType_ACCEL);
	currSet2 = set2;
	currSet2.filterByType(fhrPart::FhrPartType_ACCEL);
	partsComp.SetExpert(&currSet1, n);
	partsComp.SetTest(&currSet2);
	partsComp.Compare();

	sR.Format("%d", partsComp.Detect());
	GetDlgItem(IDC_ACC_DETECT)->SetWindowText(sR);
	sR.Format("%d", partsComp.Miss());
	GetDlgItem(IDC_ACC_MISS)->SetWindowText(sR);
	sR.Format("%d", partsComp.FP());
	GetDlgItem(IDC_ACC_FP)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.SampSens() * 100.0));
	GetDlgItem(IDC_ACC_SAMPSENS)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.SampPPV() * 100.0));
	GetDlgItem(IDC_ACC_SAMPPPV)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.Sens() * 100.0));
	GetDlgItem(IDC_ACC_SENS)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.PPV() * 100.0));
	GetDlgItem(IDC_ACC_PPV)->SetWindowText(sR);

	// baseline
	partsComp.Clear();
	currSet1 = set1;
	currSet1.filterByType(fhrPart::FhrPartType_BAS);
	currSet2 = set2;
	currSet2.filterByType(fhrPart::FhrPartType_BAS);
	partsComp.SetExpert(&currSet1, n);
	partsComp.SetTest(&currSet2);
	partsComp.Compare();

	sR.Format("%.2f", (partsComp.SampSens() * 100.0));
	GetDlgItem(IDC_BAS_SAMPSENS)->SetWindowText(sR);
	sR.Format("%.2f", (partsComp.SampPPV() * 100.0));
	GetDlgItem(IDC_BAS_SAMPPPV)->SetWindowText(sR);
}

/*
 =======================================================================================================================
    Input of fetus 1 has changed.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnCbnSelchangeInput1(void)
{
	m_bProcess1 = false;
	ResetFetus(kFetus1);
	ResetFetus(kFetus2);

	if (m_Input_1.GetCurSel() < SAMPLECOUNT)
	{
		patterns::samples::create(m_Tracing.get(), m_Input_1.GetCurSel());
		m_fetus_1 = *m_Tracing.get();
		m_bFromFile1 = true;
	}
}

/*
 =======================================================================================================================
    Input of fetus 2 has changed.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnCbnSelchangeInput2(void)
{
	m_bProcess2 = false;
	if (m_Input_2.GetCurSel() < SAMPLECOUNT)
	{
		patterns::samples::create(m_Tracing.get_partial(), m_Input_2.GetCurSel());
		m_fetus_2.reset();
		m_fetus_2 = *m_Tracing.get_partial();	// make local copy
		m_bFromFile2 = true;
	}
	else
	{
		ResetFetus(kFetus2);
	}
}

/*
 =======================================================================================================================
    resize window.
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnSize(UINT nType, int cx, int cy)
{
	if (m_Tracing)
	{
		Rearrange(cx, cy);
	}
}

/*
 =======================================================================================================================
    Rearrange controls position.
 =======================================================================================================================
 */
void CPatternsTracingDlg::Rearrange(long w, long h)
{
	/*~~~~~~~~~~~~*/
	long width = w;
	long height = h;
	/*~~~~~~~~~~~~*/

	// use current dialog size if null value passed.
	if (width == 0 || height == 0)
	{
		/*~~~~~~*/
		RECT rect;
		/*~~~~~~*/

		GetClientRect(&rect);
		width = rect.right - rect.left;
		height = rect.bottom - rect.top;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// Section 1 = tracing, section 2 = controls.
	long section2_h = (5 * MARGIN) + (4 * BTNHEIGHT);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	// do not reposition control under a certain limit.
	if (width < MAXDIALOGWIDTH)
	{
		width = MAXDIALOGWIDTH;
	}

	if ((height - section2_h) < 70)
	{
		height = section2_h + 70;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long section1_h = height - section2_h;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	// Section 3 controls - column 1
	GetDlgItem(IDC_RESET_1)->SetWindowPos(0, COLUMN1LEFT, height - MARGIN - BTNHEIGHT, BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_RECOMPUTE_1)->SetWindowPos(0, COLUMN1LEFT, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_INPUT_1)->SetWindowPos(0, COLUMN1LEFT, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT * 5, SWP_SHOWWINDOW);
	GetDlgItem(IDC_BROWSE_1)->SetWindowPos(0, COLUMN1LEFT + BTNWIDTH + MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNHEIGHT, BTNHEIGHT, SWP_SHOWWINDOW);

	// GetDlgItem(IDC_RANDOM_INTERVAL)->SetWindowPos(0, COLUMN1LEFT + BTNWIDTH +
	// MARGIN, height - (2*MARGIN) - (2*BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_COL1)->SetWindowPos(0, COLUMN1LEFT, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_PROCESS1)->SetWindowPos(0, COLUMN1LEFT + BTNWIDTH + MARGIN, height - MARGIN - BTNHEIGHT, BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_BS1)->SetWindowPos(0, COLUMN1LEFT + BTNWIDTH + MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_BS1TEXT)->SetWindowPos(0, COLUMN1LEFT + BTNWIDTH / 2, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_COMPARE_1_2)->SetWindowPos(0, COLUMN1LEFT + BTNWIDTH + MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_REPREJ1)->SetWindowPos(0, COLUMN1LEFT + 2 * BTNWIDTH + 2 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 4, BTNHEIGHT / 2, SWP_SHOWWINDOW);
	GetDlgItem(IDC_REPREJ1TEXT)->SetWindowPos(0, COLUMN1LEFT + (int) (1.5 * BTNWIDTH + 1.5 * MARGIN), height - (3 * MARGIN) - (3 * BTNHEIGHT), 3 * BTNWIDTH / 4, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DISREP1)->SetWindowPos(0, COLUMN1LEFT + 2 * BTNWIDTH + 2 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 4, BTNHEIGHT / 2, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DISREP1TEXT)->SetWindowPos(0, COLUMN1LEFT + (int) (1.5 * BTNWIDTH + 1.5 * MARGIN), height - (4 * MARGIN) - (4 * BTNHEIGHT), 3 * BTNWIDTH / 4, BTNHEIGHT, SWP_SHOWWINDOW);

	// controls - column 2
	GetDlgItem(IDC_RESET_2)->SetWindowPos(0, COLUMN2LEFT, height - MARGIN - BTNHEIGHT, BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_RECOMPUTE_2)->SetWindowPos(0, COLUMN2LEFT, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_INPUT_2)->SetWindowPos(0, COLUMN2LEFT, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT * 5, SWP_SHOWWINDOW);
	GetDlgItem(IDC_BROWSE_2)->SetWindowPos(0, COLUMN2LEFT + BTNWIDTH + MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNHEIGHT, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_COL2)->SetWindowPos(0, COLUMN2LEFT, (int) (height - (4 * MARGIN) - (4 * BTNHEIGHT)), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_PROCESS)->SetWindowPos(0, COLUMN2LEFT + BTNWIDTH + MARGIN, height - MARGIN - BTNHEIGHT, BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_BS2)->SetWindowPos(0, COLUMN2LEFT + BTNWIDTH + MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_BS2TEXT)->SetWindowPos(0, COLUMN2LEFT + BTNWIDTH / 2, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_COMPARE_2_1)->SetWindowPos(0, COLUMN2LEFT + BTNWIDTH + MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_REPREJ2)->SetWindowPos(0, COLUMN2LEFT + 2 * BTNWIDTH + 2 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 4, BTNHEIGHT / 2, SWP_SHOWWINDOW);
	GetDlgItem(IDC_REPREJ2TEXT)->SetWindowPos(0, COLUMN2LEFT + (int) (1.5 * BTNWIDTH + 1.5 * MARGIN), height - (3 * MARGIN) - (3 * BTNHEIGHT), 3 * BTNWIDTH / 4, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DISREP2)->SetWindowPos(0, COLUMN2LEFT + 2 * BTNWIDTH + 2 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 4, BTNHEIGHT / 2, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DISREP2TEXT)->SetWindowPos(0, COLUMN2LEFT + (int) (1.5 * BTNWIDTH + 1.5 * MARGIN), height - (4 * MARGIN) - (4 * BTNHEIGHT), 3 * BTNWIDTH / 4, BTNHEIGHT, SWP_SHOWWINDOW);

	GetDlgItem(IDC_STEP)->SetWindowPos(0, COLUMN2LEFT + (int) 3.5 * BTNWIDTH + (int) 3.5 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 4, BTNHEIGHT / 2, SWP_SHOWWINDOW);
	GetDlgItem(IDC_STEPTEXT)->SetWindowPos(0, COLUMN2LEFT + (int) 2.5 * BTNWIDTH + 3 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_WAITBS)->SetWindowPos(0, COLUMN2LEFT + (int) 3.5 * BTNWIDTH + (int) 3.5 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 4, BTNHEIGHT / 2, SWP_SHOWWINDOW);
	GetDlgItem(IDC_WAITTEXT)->SetWindowPos(0, COLUMN2LEFT + (int) 2.5 * BTNWIDTH + 3 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_RANDBS)->SetWindowPos(0, COLUMN2LEFT + (int) 3.5 * BTNWIDTH + (int) 3.5 * MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 4, BTNHEIGHT / 2, SWP_SHOWWINDOW);
	GetDlgItem(IDC_RANDBSTEXT)->SetWindowPos(0, COLUMN2LEFT + (int) 2.5 * BTNWIDTH + 3 * MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_TIMEACCEL)->SetWindowPos(0, COLUMN2LEFT + (int) 3.5 * BTNWIDTH + (int) 3.5 * MARGIN, height - (MARGIN) - (BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_ACCELTEXT)->SetWindowPos(0, COLUMN2LEFT + (int) 2.5 * BTNWIDTH + 3 * MARGIN, height - (MARGIN) - (BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);

	GetDlgItem(IDC_REBOOT)->SetWindowPos(0, COLUMN2LEFT + (int) 4 * BTNWIDTH + 3 *MARGIN, height - (MARGIN) - (BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);

	// Comparison Labels
	GetDlgItem(IDC_LBL_DECEL)->SetWindowPos(0, COLUMN3LEFT - BTNWIDTH / 2 - MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_ACCEL)->SetWindowPos(0, COLUMN3LEFT - BTNWIDTH / 2 - MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_BAS)->SetWindowPos(0, COLUMN3LEFT - BTNWIDTH / 2 - MARGIN, height - (1 * MARGIN) - (1 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);

	GetDlgItem(IDC_LBL_DETECT)->SetWindowPos(0, COLUMN3LEFT, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_MISS)->SetWindowPos(0, COLUMN3LEFT + (BTNWIDTH / 2) + MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_FP)->SetWindowPos(0, COLUMN3LEFT + (BTNWIDTH) + 2 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_SAMPSENS)->SetWindowPos(0, COLUMN3LEFT + (3 * BTNWIDTH / 2) + 3 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_SAMPPPV)->SetWindowPos(0, COLUMN3LEFT + (2 * BTNWIDTH) + 4 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_SENS)->SetWindowPos(0, COLUMN3LEFT + (5 * BTNWIDTH / 2) + 5 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_LBL_PPV)->SetWindowPos(0, COLUMN3LEFT + (3 * BTNWIDTH) + 6 * MARGIN, height - (4 * MARGIN) - (4 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);

	// Comparison Results
	GetDlgItem(IDC_DEC_DETECT)->SetWindowPos(0, COLUMN3LEFT, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DEC_MISS)->SetWindowPos(0, COLUMN3LEFT + (BTNWIDTH / 2) + MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DEC_FP)->SetWindowPos(0, COLUMN3LEFT + (BTNWIDTH) + 2 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DEC_SAMPSENS)->SetWindowPos(0, COLUMN3LEFT + (3 * BTNWIDTH / 2) + 3 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DEC_SAMPPPV)->SetWindowPos(0, COLUMN3LEFT + (2 * BTNWIDTH) + 4 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DEC_SENS)->SetWindowPos(0, COLUMN3LEFT + (5 * BTNWIDTH / 2) + 5 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DEC_PPV)->SetWindowPos(0, COLUMN3LEFT + (3 * BTNWIDTH) + 6 * MARGIN, height - (3 * MARGIN) - (3 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);

	GetDlgItem(IDC_ACC_DETECT)->SetWindowPos(0, COLUMN3LEFT, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_ACC_MISS)->SetWindowPos(0, COLUMN3LEFT + (BTNWIDTH / 2) + MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_ACC_FP)->SetWindowPos(0, COLUMN3LEFT + (BTNWIDTH) + 2 * MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_ACC_SAMPSENS)->SetWindowPos(0, COLUMN3LEFT + (3 * BTNWIDTH / 2) + 3 * MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_ACC_SAMPPPV)->SetWindowPos(0, COLUMN3LEFT + (2 * BTNWIDTH) + 4 * MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_ACC_SENS)->SetWindowPos(0, COLUMN3LEFT + (5 * BTNWIDTH / 2) + 5 * MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_ACC_PPV)->SetWindowPos(0, COLUMN3LEFT + (3 * BTNWIDTH) + 6 * MARGIN, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);

	GetDlgItem(IDC_BAS_SAMPSENS)->SetWindowPos(0, COLUMN3LEFT + (3 * BTNWIDTH / 2) + 3 * MARGIN, height - (1 * MARGIN) - (1 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_BAS_SAMPPPV)->SetWindowPos(0, COLUMN3LEFT + (2 * BTNWIDTH) + 4 * MARGIN, height - (1 * MARGIN) - (1 * BTNHEIGHT), BTNWIDTH / 2, BTNHEIGHT, SWP_SHOWWINDOW);

	// close and debug buttons.
	GetDlgItem(IDOK)->SetWindowPos(0, width - MARGIN - BTNWIDTH, height - MARGIN - BTNHEIGHT, BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);
	GetDlgItem(IDC_DEBUG)->SetWindowPos(0, width - MARGIN - BTNWIDTH, height - (2 * MARGIN) - (2 * BTNHEIGHT), BTNWIDTH, BTNHEIGHT, SWP_SHOWWINDOW);

	// Tracings.
	m_Tracing.SetWindowPos(0, MARGIN, MARGIN, width - (2 * MARGIN), section1_h - MARGIN, SWP_SHOWWINDOW);

	/*~~~~~~~~*/
	CString str;
	/*~~~~~~~~*/

	str.Format("%d", m_iBS1sec);
	m_BS1edit.SetWindowText(str);
	str.Format("%d", m_iBS2sec);
	m_BS2edit.SetWindowText(str);

	str.Format("%d", m_iTimeAccel);
	m_TimeAccel.SetWindowText(str);
	if (m_bRepRej1)
	{
		m_RepRej1Ctrl.SetCheck(BST_CHECKED);
	}

	if (m_bRepRej2)
	{
		m_RepRej2Ctrl.SetCheck(BST_CHECKED);
	}

	if (m_bWaitFinish)
	{
		m_WaitCtrl.SetCheck(BST_CHECKED);
	}
}

/*
 =======================================================================================================================
    Recompute contractions of the given fetus.
 =======================================================================================================================
 */
void CPatternsTracingDlg::Recompute(patterns::fetus &f)
{
	/*~~~~~*/
	fetus f0;
	/*~~~~~*/

	f0 = f;

	// f0.adjust_up();
	// // adjust to 1Hz.
	f.compute_now();
}

/*
 =======================================================================================================================
    Reset the content of the given fetus.
 =======================================================================================================================
 */
void CPatternsTracingDlg::ResetFetus(long f)
{
	/*~~~~~~~*/
	fetus* fNew = m_Tracing.NewFetus();
	/*~~~~~~~*/

	if (f == kFetus1)
	{	// m_Tracing.get().reset();
		///
		*m_Tracing.get() = *fNew;
	}
	else if (f == kFetus2)
	{
		*m_Tracing.get_partial() = *fNew;
	}

	// m_Tracing.get_partial().reset();
	delete fNew;
}

/*
 =======================================================================================================================
    Get existing file name using standard open file dialog.
 =======================================================================================================================
 */
string CPatternsTracingDlg::StandardOpenFile(string filter)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string filename;
	// static TCHAR szFilter[] = _T(filter.c_str());
	// //_T("IN Files (*.IN)|*.IN;
	// |XML Files (*.xml)|*.xml|AllFiles(*.*)|*.*|") ;
	CFileDialog fileDlg(TRUE, _T("*.*"), NULL, OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST, filter.c_str(), NULL);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (IDOK == fileDlg.DoModal())
	{
		filename = fileDlg.GetPathName();
	}

	return filename;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedProcess2(void)
{
	/*~~~~~~~*/
	fetus* fNew = m_Tracing.NewFetus();
	/*~~~~~~~*/

	if (!m_bProcess1)	// only process one at a time
	{
		if (m_lFHRindex2 == 0)
		{
			m_Tracing.get_partial()->reset();

			// m_Tracing.get_partial() = fNew;
			m_bFromFile2 = false;
		}

		m_Tracing.get_partial()->set_as_real_time(true);

		if (m_bStep)	// here do just one append
		{
			m_bProcess2 = true;
			Append2();
			m_Tracing.move_to((m_lFHRindex2 / m_Tracing.get_partial()->get_hr_sample_rate()) - ((long) (m_dWinSizeMinutes * 60)));

			// m_Tracing.move_to(m_lFHRindex2 / 4 - (m_dWinSizeMinutes * 60));
		}
		else
		{				// toggle simulated real time processing on timer
			m_bProcess2 = !m_bProcess2;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iMS = (1000 * m_iBS2sec) / m_iTimeAccel;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		SetTimer(0, iMS, 0);
	}
	delete fNew;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedProcess1(void)
{
	/*~~~~~~~*/
	fetus* fNew = m_Tracing.NewFetus();
	/*~~~~~~~*/

	if (!m_bProcess2)	// can only process one at a time
	{
		if (m_lFHRindex1 == 0)
		{
			m_Tracing.get()->reset();

			// m_Tracing.get() = fNew;
			m_bFromFile1 = false;
		}

		m_Tracing.get()->set_as_real_time(true);

		if (m_bStep)	// here do just one append
		{
			m_bProcess1 = true;
			Append1();
			m_Tracing.move_to((m_lFHRindex1 / m_Tracing.get()->get_hr_sample_rate()) - ((long) (m_dWinSizeMinutes * 60)));

			// m_Tracing.move_to(m_lFHRindex1 / 4 - (m_dWinSizeMinutes * 60));
		}
		else
		{				// toggle simulated real time processing on timer
			m_bProcess1 = !m_bProcess1;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iMS = (1000 * m_iBS2sec) / m_iTimeAccel;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		SetTimer(0, iMS, 0);
	}
	delete fNew;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedStep(void)
{
	m_bStep = m_StepCtrl.GetCheck() == BST_CHECKED;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedWait(void)
{
	m_bWaitFinish = m_WaitCtrl.GetCheck() == BST_CHECKED;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedRandom(void)
{
	m_bRandomBS = m_RandCtrl.GetCheck() == BST_CHECKED;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedRepRej1(void)
{
	m_bRepRej1 = m_RepRej1Ctrl.GetCheck() == BST_CHECKED;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedRepRej2(void)
{
	m_bRepRej2 = m_RepRej2Ctrl.GetCheck() == BST_CHECKED;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedDisRep1(void)
{
	m_bDisRep1 = m_DisRep1Ctrl.GetCheck() == BST_CHECKED;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedDisRep2(void)
{
	m_bDisRep2 = m_DisRep2Ctrl.GetCheck() == BST_CHECKED;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnChangeBS1(void)
{
	/*~~~~~~~~*/
	CString str;
	/*~~~~~~~~*/

	m_BS1edit.GetWindowText(str);
	m_iBS1sec = atoi(str);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnChangeBS2(void)
{
	/*~~~~~~~~*/
	CString str;
	/*~~~~~~~~*/

	m_BS2edit.GetWindowText(str);
	m_iBS2sec = atoi(str);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnChangeTimeAccel(void)
{
	/*~~~~~~~~*/
	CString str;
	/*~~~~~~~~*/

	m_TimeAccel.GetWindowText(str);
	m_iTimeAccel = atoi(str);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnTimer(UINT_PTR iID)
{
	if (m_bRandomBS && (m_bProcess1 || m_bProcess2))
	{						// gen random BS
		SetRandomBS();
	}

	if (m_bProcess2 && !m_bStep)
	{
		if (m_lFHRindex2 < m_fetus_2.get_number_of_fhr())
		{
			Append2();

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long lWindowLeftIndex = m_lFHRindex2 - ((long) (m_dWinSizeMinutes * 60) * m_Tracing.get()->get_hr_sample_rate());
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			// long lWindowLeftIndex = ((m_lFHRindex2 /
			// m_Tracing.get_partial().get_hr_sample_rate()) - (m_dWinSizeMinutes * 60));
			m_Tracing.move_to(lWindowLeftIndex);
		}
		else				// reached end of data
		{
			m_bProcess2 = false;
			m_lFHRindex2 = 0;
			m_lUPindex2 = 0;
		}

		if (m_bRandomBS)	// only reset timer based on BS on fly if generating random BS (not user input)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			int iMS = (1000 * m_iBS2sec) / m_iTimeAccel;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			SetTimer(0, iMS, 0);
		}
	}

	if (m_bProcess1 && !m_bStep)
	{
		if (bLoop)
		{
			if (m_lFHRindex1 >= m_fetus_1.get_number_of_fhr())
			{
				m_lFHRindex1 = 0;
				m_lUPindex1 = 0;
				numLoops++;
			}
		}

		if (m_lFHRindex1 < m_fetus_1.get_number_of_fhr())
		{
			Append1();

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long lWindowLeftIndex = (numLoops * m_fetus_1.get_number_of_fhr()) + m_lFHRindex1 - ((long) (m_dWinSizeMinutes * 60) * m_Tracing.get()->get_hr_sample_rate());
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			m_Tracing.move_to(lWindowLeftIndex);
		}
		else				// reached end of data
		{
			m_bProcess1 = false;
			m_lFHRindex1 = 0;
			m_lUPindex1 = 0;
		}

		if (m_bRandomBS)	// only reset timer based on BS on fly if generating random BS (not user input)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			int iMS = (1000 * m_iBS1sec) / m_iTimeAccel;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			SetTimer(0, iMS, 0);
		}
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::Append2(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	double sRateUP = m_Tracing.get_partial()->get_up_sample_rate();
	double sRateFHR = m_Tracing.get_partial()->get_hr_sample_rate();
	long lNumFhrAppend = (long) (m_iBS2sec * sRateFHR);
	bool bWaitFinish = false;
	CTestFetus *fetusCopy = (CTestFetus *) (m_Tracing.get_partial());
	CFHRSignal *pSig = fetusCopy->GetFHRSignal();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (pSig)
	{
		pSig->SetMinSamplesAppend(m_iBS2sec * 4);
		pSig->SetRemoveRepairOutput(m_bRepRej2);
		pSig->DisableRepair(m_bDisRep2);
		pSig->ActivateBasVarCalc();
		bWaitFinish = (m_bWaitFinish && (pSig->IsProcessRunning() || pSig->InTransfer()));
	}
	else				// first run - to make consistent if m_bWaitFinish is set do initial append of 921 samples (because of required warm up period)
	{
		if (m_bWaitFinish)
		{
			lNumFhrAppend = (long) ceil(WARM_UP_SAMPLES / (4.0 / sRateFHR));
		}
	}

	fetus* f = m_Tracing.get_partial();
	f->suspend_notifications(true);

	if (!(bWaitFinish)) // if waiting flag is set do not append if still processing last block
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iUPSampFactor = (int) (sRateFHR / sRateUP); // assume integer
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

		for (long i = 0; i < lNumFhrAppend; i++)
		{
			if (i % iUPSampFactor == 0)
			{
				f->append_up(m_fetus_2.get_up(m_lUPindex2++));
			}

			f->append_fhr(m_fetus_2.get_fhr(m_lFHRindex2++));
		}
	}

	f->fetch_events();
	f->suspend_notifications(true);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::Append1(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	double sRateUP = m_Tracing.get()->get_up_sample_rate();
	double sRateFHR = m_Tracing.get()->get_hr_sample_rate();
	long lNumFhrAppend = (long) (m_iBS1sec * sRateFHR);
	bool bWaitFinish = false;
	CTestFetus *fetusCopy = (CTestFetus *) (m_Tracing.get());
	CFHRSignal *pSig = fetusCopy->GetFHRSignal();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (pSig)
	{
		pSig->SetMinSamplesAppend(m_iBS1sec * 4);
		pSig->SetRemoveRepairOutput(m_bRepRej1);
		pSig->DisableRepair(m_bDisRep1);
		pSig->ActivateBasVarCalc();
		bWaitFinish = (m_bWaitFinish && (pSig->IsProcessRunning() || pSig->InTransfer()));
	}
	else	// first run - to make consistent if m_bWaitFinish is set do initial append of 921 samples (because of required warm up period)
	{
		if (m_bWaitFinish)
		{
			lNumFhrAppend = (long) ceil(WARM_UP_SAMPLES / (4.0 / sRateFHR));	// warmpup specified at 4 Hz
		}
	}

	fetus* f = m_Tracing.get();
	f->suspend_notifications(true);

	if (!(bWaitFinish)) // if waiting flag is set do not append if still processing last block
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		int iUPSampFactor = (int) (sRateFHR / sRateUP); // assume integer
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


		for (long i = 0; i < lNumFhrAppend; i++)
		{
			if (i % iUPSampFactor == 0)
			{
				f->append_up(m_fetus_1.get_up(m_lUPindex1++));
			}

			f->append_fhr(m_fetus_1.get_fhr(m_lFHRindex1++));
		}
	}

	f->fetch_events();
	f->suspend_notifications(false);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::SetRandomBS(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	double rd = (rand() / (double) RAND_MAX);	// number between 0 and 1
	double dBS = (double) (rd * (MAX_RANDOM_BS - MIN_RANDOM_BS)) + MIN_RANDOM_BS;
	int rBS = (int) floor(dBS);
	CString str;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	str.Format("%d", rBS);

	if (m_bProcess1)
	{
		m_BS1edit.SetWindowText(str);
		OnChangeBS1();
	}

	if (m_bProcess2)
	{
		m_BS2edit.SetWindowText(str);
		OnChangeBS2();
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTracingDlg::OnBnClickedReboot(void)
{

	CTestFetus *fetusCopy = (CTestFetus *) (m_Tracing.get());
	CFHRSignal *pSig = fetusCopy->GetFHRSignal();
	fetus* fNew = m_Tracing.NewFetus();

	KillTimer(0);

	while (pSig->IsProcessRunning())
		Sleep(10);

	//  Save all the events and fhr, up that have been appended already - in reality this info would be continuously dumped to a temp file
	vector<event> events;
	vector<contraction> contractions;
	vector<long> fhr;
	vector<long> up;

	fetus* curFetus = m_Tracing.get();
	curFetus->suspend_notifications(true);

	for (long i = 0; i < curFetus->get_number_of_fhr(); i++)
	{
		fhr.push_back(curFetus->get_fhr(i));
	}
	for (long i = 0; i < curFetus->get_number_of_up(); i++)
	{
		up.push_back(curFetus->get_up(i));
	}
	for (long i = 0; i < curFetus->GetEventsCount(); i++)
	{
		events.push_back(curFetus->get_event(i));
	}
	for (long i = 0; i < curFetus->GetContractionsCount(); i++)
	{
		contractions.push_back(curFetus->get_contraction(i));
	}

	// End save events, fhr, up

	// Now simulate starting over from scratch
	curFetus->reset();  // reset doesn't do enough
	*curFetus = *fNew;

	curFetus->set_cutoff_date(0);
	curFetus->set_start_date(1173686400); // so GA is in range
	curFetus->set_as_real_time(false);
	int upSampRate = (int) (4.0 * (double) up.size() / fhr.size());
	curFetus->set_up_sample_rate(upSampRate);

	// In reality this would be read back from a temp file
	for (long i = 0; i < (long) events.size(); i++)
	{
		curFetus->append_event(events.at(i));
	}
	for (long i = 0; i < (long) contractions.size(); i++)
	{
		curFetus->append_contraction(contractions.at(i));
	}
	for (long i = 0; i < (long) fhr.size(); i++)
	{
		curFetus->append_fhr(fhr.at(i));
	}
	for (long i = 0; i < (long) up.size(); i++)
	{
		curFetus->append_up(up.at(i));
	}

	// End read from temp file
	curFetus->restart_engine_after_reboot(m_lFHRindex1);
	curFetus->set_as_real_time(true);
	curFetus->suspend_notifications(false);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	int iMS = (1000 * m_iBS1sec) / m_iTimeAccel;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	SetTimer(0, iMS, 0);

	delete fNew;
}

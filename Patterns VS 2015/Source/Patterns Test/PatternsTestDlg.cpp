// PatternsDlg.cpp : implementation file
#include "stdafx.h"
#include "PatternsTest.h"
#include "PatternsTestDlg.h"

#include "patterns, fetus.h"
#include "patterns, samples.h"

#include "shlobj.h"

#define S_TO_UC(x)	(char) (unsigned char) x	// cast long to (char)(unsigned char)
#define UC_TO_S(x)	(short) (unsigned char) x

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//
// =======================================================================================================================
//    CPatternsTestDlg dialog
// =======================================================================================================================
//
CPatternsTestDlg::CPatternsTestDlg(CWnd *pParent /* NULL */ ) :
	CDialog(CPatternsTestDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	m_Timer = 0;
	m_ContractionCount = 0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
CPatternsTestDlg::~CPatternsTestDlg(void)
{
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTestDlg::DoDataExchange(CDataExchange *pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_CONTRACTIONS_OLD, m_Contractions_Old);
	DDX_Control(pDX, IDC_CONTRACTIONS_NEW, m_Contractions_New);
	DDX_Control(pDX, IDC_STATUS, m_Status);
	DDX_Control(pDX, IDC_EXE_TIME_OLD, m_Time_Old);
	DDX_Control(pDX, IDC_EXE_TIME_NEW, m_Time_New);
	DDX_Control(pDX, IDC_DIFF, m_Time_Diff);
	DDX_Control(pDX, IDC_TIME_POINTS, m_TimePointsRatio);
	DDX_Control(pDX, IDC_NB_POINTS, m_Nb_Points);
	DDX_Control(pDX, IDC_COMPUTE_ALL, m_Compute_All);
	DDX_Control(pDX, IDC_TEST_DIAGNOSTIC, m_Test_Diagnostic);
	DDX_Control(pDX, IDC_NB_CONTRACTIONS_OLD, m_Nb_Contractions_Old);
	DDX_Control(pDX, IDC_NB_CONTRACTIONS_NEW, m_Nb_Contractions_New);
	DDX_Control(pDX, IDC_NB_ITERATION, m_Nb_Iteration);
}

BEGIN_MESSAGE_MAP(CPatternsTestDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_TIMER()
//}}AFX_MSG_MAP	
	ON_BN_CLICKED(IDC_COMPUTE, OnBnClickedCompute)
	ON_BN_CLICKED(IDC_COMPUTE_ALL, OnBnClickedComputeAll)
	ON_BN_CLICKED(IDC_RESET, OnBnClickedReset)
	ON_BN_CLICKED(IDC_TEST, OnBnClickedTest)
	ON_BN_CLICKED(IDC_TEST_LIMITS, OnBnClickedTestLimits)
END_MESSAGE_MAP()

/*
 =======================================================================================================================
    Compare the index of two contractions array.
 =======================================================================================================================
 */
bool CPatternsTestDlg::Compare(vector<patterns::contraction> &cOld, vector<patterns::contraction> &cNew)
{
	/*~~~~~~~~~~*/
	bool r = true;
	/*~~~~~~~~~~*/

	if (cOld.size() != cNew.size())
	{
		r = false;
	}
	else
	{
		for (long i = 0; i < (long) cOld.size(); i++)
		{
			if (cOld[i] != cNew[i])
			{
				r = false;
				break;
			}
		}
	}

	return r;
}

/*
 =======================================================================================================================
    Compute all files in the given directory.
 =======================================================================================================================
 */
void CPatternsTestDlg::ComputeDir(string dirName)
{
	/*~~~~~~~~~~~~~*/
	CFileFind finder;
	/*~~~~~~~~~~~~~*/

	if (dirName.length() == 0)
	{
		return;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~*/
	// build a string with wildcards
	string strWildcard(dirName);
	/*~~~~~~~~~~~~~~~~~~~~~~~~*/

	strWildcard += "\\*.*";

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	BOOL bWorking = finder.FindFile(strWildcard.c_str());
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	while (bWorking)
	{
		bWorking = finder.FindNextFile();

		// skip . and .. files;
		// otherwise, we'd recur infinitely
		if (finder.IsDots())
		{
			continue;
		}

		if (!finder.IsDirectory())
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			CString s = finder.GetFilePath();
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			ComputeFile(string(s), true);
		}
	}

	finder.Close();
}

/*
 =======================================================================================================================
    Compute given in file.
 =======================================================================================================================
 */
void CPatternsTestDlg::ComputeFile(string filename, bool saveToFile)
{
	ResetContent();

	/*~~~~~~~~~~~~~~~~~~~~*/
	string s("Patterns - ");
	/*~~~~~~~~~~~~~~~~~~~~*/

	s += filename;
	SetWindowText(s.c_str());

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// return if no data found.
	vector<char> upList = ImportUP(filename);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (upList.size() == 0)
	{
		return;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// compute using new code. LMSValue::Time t_new0;
	vector<patterns::contraction> c_new = ComputeNew(upList);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	// LMSValue::Time t_new1;
	// long d1 = (t_new1-t_new0).LowPart;

	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#ifdef USE_CALM_DLL
	// compute using old code.
	LMSValue::ReadingSet rs = CPatternsTestDlg::ConvertUPArrayToRS(upList);
	LMSValue::Time t_old0;
	LMSValue::ContractionSet c_old = ComputeOld(rs);
	LMSValue::Time t_old1;
	long d0 = (t_old1 - t_old0).LowPart;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	memset(buf, 0, buf_size);
	sprintf(buf, "%ld", d0);
	m_Time_Old.SetWindowText(buf);

	memset(buf, 0, buf_size);
	sprintf(buf, "%.3f", (double) d0 / d1);
	m_Time_Diff.SetWindowText(buf);

	memset(buf, 0, buf_size);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long n = (long) upList.size();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	sprintf(buf, "%g", (double) (((double) d0 / d1) / n));
	m_TimePointsRatio.SetWindowText(buf);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// compare both method's results.
	vector<contraction> c_old_converted = ConvertRSToPCArray(c_old, rs);
	bool identical = Compare(c_old_converted, c_new);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (identical)
	{
		SetStatus("Identical");
	}
	else
	{
		SetStatus("Not identical");
	}

	// affichage du nombre de contractions.
	memset(buf, 0, buf_size);
	sprintf(buf, "%ld", c_old_converted.size());
	m_Nb_Contractions_Old.SetWindowText(buf);

	// display contractions found (by both methods).
	ShowContractions(c_old_converted, upList, &m_Contractions_Old);
#endif

	/*~~~~~~~~~~~~~~~~*/
	// display time results.
	char buf[100];
	long buf_size = 100;
	/*~~~~~~~~~~~~~~~~*/

	// memset(buf, 0, buf_size);
	// sprintf(buf, "%ld", d1);
	// m_Time_New.SetWindowText(buf);
	memset(buf, 0, buf_size);
	sprintf(buf, "%ld", upList.size());
	m_Nb_Points.SetWindowText(buf);

	memset(buf, 0, buf_size);
	sprintf(buf, "%ld", c_new.size());
	m_Nb_Contractions_New.SetWindowText(buf);

	ShowContractions(c_new, upList, &m_Contractions_New);

	// if (saveToFile) SaveToFile("results.txt", filename, d0, d1,
	// (long)upList.size(), identical);
}

/*
 =======================================================================================================================
    Calculate contractions using new code.
 =======================================================================================================================
 */
vector<patterns::contraction> CPatternsTestDlg::ComputeNew(vector<char> &up)
{
	/*~~~~~~~~~~~~~~~~~~~~~*/
	contraction_detection cd;
	/*~~~~~~~~~~~~~~~~~~~~~*/

	cd.set_latent_vector_cut_off(0.4);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	vector<patterns::contraction> c;
	cd.detect(up, c);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	return c;
}

#ifdef USE_CALM_DLL

/*
 =======================================================================================================================
    Calculate contractions using old code (CALM).
 =======================================================================================================================
 */
LMSValue::ContractionSet CPatternsTestDlg::ComputeOld(LMSValue::ReadingSet &rs)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~*/
	LMSValue::ContractionSet c;
	LMSCD::Detector cd;
	/*~~~~~~~~~~~~~~~~~~~~~~~*/

	c.Empty();
	cd.Detect(rs, c);

	return c;
}

/*
 =======================================================================================================================
    Convert a reading set to a new patterns::contraction array.
 =======================================================================================================================
 */
vector<contraction> CPatternsTestDlg::ConvertRSToPCArray(LMSValue::ContractionSet &cs, LMSValue::ReadingSet &rs)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	vector<contraction> contractions;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (cs.Cardinality() != 0 && rs.Cardinality() != 0)
	{
		LMSValue::Time firstPtTime(rs[1]->GetReadingTime());

		// conversion du temps vers un indice.
		for (long i = 1; i <= (long) cs.Cardinality(); i++)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long start = (cs[i]->GetStartTime() - firstPtTime).LowPart / 1000;
			long peak = (cs[i]->GetPeakTime() - firstPtTime).LowPart / 1000;
			long end = (cs[i]->GetEndTime() - firstPtTime).LowPart / 1000;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			contractions.insert(contractions.end(), contraction(start, peak, end));
		}
	}

	return contractions;
}

/*
 =======================================================================================================================
    Convert uterine pressure array (chars) to a recordset.
 =======================================================================================================================
 */
LMSValue::ReadingSet CPatternsTestDlg::ConvertUPArrayToRS(vector<char> &up)
{
	/*~~~~~~~~~~~~~~~*/
	LMSValue::Time now;
	/*~~~~~~~~~~~~~~~*/

	LMSValue::ReadingSet rs(LMSConcepts::_UPS);

	for (long i = 0; i < (long) up.size(); ++i)
	{
		rs << new LMSValue::Reading(UC_TO_S(up[i]), LMSValue::DBReading::GREEN, now + (i * 1000));
	}

	return rs;
}

#endif // USE_CALM_DLL

/*
 =======================================================================================================================
    Reset content of all controls.
 =======================================================================================================================
 */
void CPatternsTestDlg::ResetContent(void)
{
	m_Contractions_Old.ResetContent();
	m_Contractions_New.ResetContent();
	SetStatus("No data");
	m_Time_Old.SetWindowText("");
	m_Time_New.SetWindowText("");
	m_Time_Diff.SetWindowText("");
	m_Nb_Contractions_Old.SetWindowText("");
	m_Nb_Contractions_New.SetWindowText("");
	m_Nb_Points.SetWindowText("");
	m_TimePointsRatio.SetWindowText("");
	SetWindowText("Patterns");
}

/*
 =======================================================================================================================
    Create list of contraction from a IN file.
 =======================================================================================================================
 */
vector<char> CPatternsTestDlg::ImportUP(string fileName)
{
	/*~~~~~~~~~~~~~~~~*/
	vector<char> UPList;
	/*~~~~~~~~~~~~~~~~*/

	if (fileName.length() == 0)
	{
		return UPList;
	}

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	FILE *file = fopen(fileName.c_str(), "r");
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (file)
	{
		/*~~~~~~~~~~~~*/
		char stream[80];
		int r[15];				// 15 readings (3 seconds) on each line in file
		//~~~~~~~~~~~~

		while (fgets(stream, 80, file))
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// at this point in the code, stream is guarenteed to contain valid FHR and IUP
			// data
			int nb_chars = sscanf
				(
					stream,
					"%3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d",
					&r[0],
					&r[1],
					&r[2],
					&r[3],
					&r[4],
					&r[5],
					&r[6],
					&r[7],
					&r[8],
					&r[9],
					&r[10],
					&r[11],
					&r[12],
					&r[13],
					&r[14]
				);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (nb_chars == 15) // do nothing with data unless we have exactly 15 readings
			{
				/*~~~~~~~~~~~~*/
				char c0 = r[12];
				char c1 = r[13];
				char c2 = r[14];
				/*~~~~~~~~~~~~*/

				UPList.insert(UPList.end(), (char) (unsigned char) r[12]);
				UPList.insert(UPList.end(), (char) (unsigned char) r[13]);
				UPList.insert(UPList.end(), (char) (unsigned char) r[14]);
			}
		}

		fclose(file);
	}

	return UPList;
}

/*
 =======================================================================================================================
    Compute current uterine pressure (creates an array of contractions).
 =======================================================================================================================
 */
void CPatternsTestDlg::OnBnClickedCompute(void)
{
	// display the hourglass cursor
	AfxGetApp()->DoWaitCursor(1);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string filename = StandardOpenFile();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (filename.length() != 0)
	{
		ComputeFile(filename);
	}

	// reset cursor
	AfxGetApp()->DoWaitCursor(-1);
}

/*
 =======================================================================================================================
    "Compute All" button click.
 =======================================================================================================================
 */
void CPatternsTestDlg::OnBnClickedComputeAll(void)
{
	// display the hourglass cursor
	AfxGetApp()->DoWaitCursor(1);

	ComputeDir(StandardOpenDir());

	// reset cursor
	AfxGetApp()->DoWaitCursor(-1);
}

/*
 =======================================================================================================================
    Reset button click.
 =======================================================================================================================
 */
void CPatternsTestDlg::OnBnClickedReset(void)
{
	ResetContent();
}

/*
 =======================================================================================================================
    Test button click.
 =======================================================================================================================
 */
void CPatternsTestDlg::OnBnClickedTest(void)
{
}

/*
 =======================================================================================================================
    Test limits button click.
 =======================================================================================================================
 */
void CPatternsTestDlg::OnBnClickedTestLimits(void)
{
	// display the hourglass cursor
	AfxGetApp()->DoWaitCursor(1);

	// reset diagnostic results.
	m_Test_Diagnostic.SetWindowText("");
	m_Test_Diagnostic.RedrawWindow();

	/*~~~~~~~~~~~~~~~~~~*/
	CString s;
	long nb_iteration = 0;
	/*~~~~~~~~~~~~~~~~~~*/

	m_Nb_Iteration.GetWindowText(s);

	/*~~~~~~~~~~~~~~~~~~~~~*/
	LPTSTR p = s.GetBuffer();
	/*~~~~~~~~~~~~~~~~~~~~~*/

	sscanf(p, "%ld", &nb_iteration);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string r = TestLimits(nb_iteration);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	m_Test_Diagnostic.SetWindowText(r.c_str());

	// reset cursor
	AfxGetApp()->DoWaitCursor(-1);
}

/*
 =======================================================================================================================
    Init dialog
 =======================================================================================================================
 */
BOOL CPatternsTestDlg::OnInitDialog(void)
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog. The framework does this automatically when the
	// application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);		// Set big icon
	SetIcon(m_hIcon, FALSE);	// Set small icon

	// init number of iteration editbox.
	m_Nb_Iteration.SetWindowText("40");

	return TRUE;				// return TRUE unless you set the focus to a control
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTestDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	CDialog::OnSysCommand(nID, lParam);
}

//
// =======================================================================================================================
//    If you add a minimize button to your dialog, you will need the code below to draw the icon. For MFC applications
//    using the document/view model, this is automatically done for you by the framework.
// =======================================================================================================================
//
void CPatternsTestDlg::OnPaint(void)
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

//
// =======================================================================================================================
//    The system calls this function to obtain the cursor to display while the user drags the minimized window.
// =======================================================================================================================
//
HCURSOR CPatternsTestDlg::OnQueryDragIcon(void)
{
	return static_cast<HCURSOR>(m_hIcon);
}

/*
 =======================================================================================================================
    Called on demand to refresh controls.
 =======================================================================================================================
 */
void CPatternsTestDlg::OnTimer(UINT nIDEvent)
{
	// ShowTracingData();
}

/*
 =======================================================================================================================
    Save infos to file.
 =======================================================================================================================
 */
void CPatternsTestDlg::SaveToFile(string saveTo, string source, long t1, long t2, long nbPoints, bool identical)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	FILE *f = fopen(saveTo.c_str(), "a+");
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (!f)
	{
		return;
	}

	/*~~~~~*/
	// file name
	string s;
	/*~~~~~*/

	s = source;
	s += "\t";

	/*~~~~~~~~~~~~~~~~*/
	// time of the old calculation method.
	char buf[100];
	long buf_size = 100;
	/*~~~~~~~~~~~~~~~~*/

	memset(buf, 0, buf_size);
	sprintf(buf, "%ld", t1);
	s += buf;
	s += "\t";

	// time of the new calculation method.
	memset(buf, 0, buf_size);
	sprintf(buf, "%ld", t2);
	s += buf;
	s += "\t";

	// number of points
	memset(buf, 0, buf_size);
	sprintf(buf, "%ld", nbPoints);
	s += buf;
	s += "\t";

	// identical
	if (identical)
	{
		s += "Identical";
	}
	else
	{
		s += "Not identical";
	}

	s += "\t";
	s += "\n";

	fwrite(s.c_str(), s.length(), 1, f);
	fclose(f);
}

/*
 =======================================================================================================================
    set status text of current action.
 =======================================================================================================================
 */
void CPatternsTestDlg::SetStatus(string text)
{
	m_Status.SetWindowText(text.c_str());
}

#ifdef USE_CALM_DLL

/*
 =======================================================================================================================
    Display CALM current patient contractions.
 =======================================================================================================================
 */
void CPatternsTestDlg::ShowContractions(LMSValue::ContractionSet &cs)
{
	m_Contractions_Old.ResetContent();

	/*~~~~~~~~~~~~~~~~~~~*/
	LMSValue::Time dtStart;
	LMSValue::Time dtPeak;
	LMSValue::Time dtEnd;
	/*~~~~~~~~~~~~~~~~~~~*/

	if (m_ContractionCount < cs.Cardinality())
	{
		m_ContractionCount = cs.Cardinality();
		cs.Reset();
		while (LMSValue::Contraction * pContraction = cs.GetNext())
		{
			/*~~~~~~~*/
			CString s0;
			/*~~~~~~~*/

			pContraction->GetStartTime().GetLocalTime(dtStart);
			pContraction->GetPeakTime().GetLocalTime(dtPeak);
			pContraction->GetEndTime().GetLocalTime(dtEnd);

			s0.Format("Start: %ld, Peak: %ld, End: %ld", pContraction->GetAbsStartHeight(), pContraction->GetAbsPeakHeight(), pContraction->GetAbsEndHeight());
			m_Contractions_Old.AddString(s0.GetBuffer());
		}
	}
}

#endif // USE_CALM_DLL

/*
 =======================================================================================================================
    Display current patient contractions (contraction's array + uterine pressure's array (chars)).
 =======================================================================================================================
 */
void CPatternsTestDlg::ShowContractions(vector<patterns::contraction> &contractions, vector<char> &uterinePressure, CListBox *listBoxControl)
{
	m_Contractions_New.ResetContent();

	for (long i = 0; i < (long) contractions.size(); ++i)
	{
		/*~~~~~~~*/
		CString s0;
		/*~~~~~~~*/

		s0.Format
		(
			"Start: %ld (%ld), Peak: %ld (%ld), End: %ld (%ld)",
			uterinePressure[contractions[i].get_start()],
			contractions[i].get_start(),
			uterinePressure[contractions[i].get_peak()],
			contractions[i].get_peak(),
			uterinePressure[contractions[i].get_end()],
			contractions[i].get_end()
		);
		listBoxControl->AddString(s0.GetBuffer());
	}
}

/*
 =======================================================================================================================
    Get existing folder name using standard open dir dialog.
 =======================================================================================================================
 */
string CPatternsTestDlg::StandardOpenDir(void)
{
	/*~~~~~~~~~~~~*/
	string filename;
	/*~~~~~~~~~~~~*/

	CoInitialize(0);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	BROWSEINFO bi = { 0 };
	// bi.lpszTitle = _T("Select a directory");
	LPITEMIDLIST pidl = SHBrowseForFolder(&bi);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (pidl != 0)
	{
		/*~~~~~~~~~~~~~~~~~*/
		// get the name of the folder
		TCHAR path[MAX_PATH];
		/*~~~~~~~~~~~~~~~~~*/

		if (SHGetPathFromIDList(pidl, path))
		{
			filename = path;
		}

		/*~~~~~~~~~~~~~~~~~*/
		// free memory used
		IMalloc *imalloc = 0;
		/*~~~~~~~~~~~~~~~~~*/

		if (SUCCEEDED(SHGetMalloc(&imalloc)))
		{
			imalloc->Free(pidl);
			imalloc->Release();
		}
	}

	return filename;
}

/*
 =======================================================================================================================
    Get existing file name using standard open file dialog.
 =======================================================================================================================
 */
string CPatternsTestDlg::StandardOpenFile(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	string filename;
	static TCHAR szFilter[] = _T("TypeLib Files (*.IN)|*.IN;|AllFiles(*.*)|*.*|");
	CFileDialog fileDlg(TRUE, _T("*.IN"), NULL, OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST, szFilter, NULL);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (IDOK == fileDlg.DoModal())
	{
		filename = fileDlg.GetPathName();
	}

	return filename;
}

/*
 =======================================================================================================================
    Test limits. This test allow us to know the min and max limits needed to calculate real contractions. To calculate
    those limits, we calculate the contractions for the whole up set, then, we calculate the contractions on a subset.
    The comparison of the two results tell us if we should consider the contraction before time t1 and after time t2.
 =======================================================================================================================
 */
string CPatternsTestDlg::TestLimits(long n)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	const long size = 1000;
	char buf[size];
	string diagnosis;
	long max_limit_from_start = 0;
	long max_limit_from_end = 0;
	long min_limit_from_start = 0;
	long min_limit_from_end = 0;
	long max_limit_from_start_pos0 = 0;
	long max_limit_from_start_pos1 = 0;
	long max_limit_from_end_pos0 = 0;
	long max_limit_from_end_pos1 = 0;
	long min_limit_from_start_pos0 = 0;
	long min_limit_from_start_pos1 = 0;
	long min_limit_from_end_pos0 = 0;
	long min_limit_from_end_pos1 = 0;
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (samples::get_number() == 0)
	{
		return diagnosis;
	}

	time_t t0 = time(0);

	fetus *f = new fetus();
	samples::create(f, 0);

	/*~~~~~~~~~~~~*/
	vector<char> up;
	// create an array of up.
	long j;
	/*~~~~~~~~~~~~*/

	for (j = 0; j < f->get_number_of_up(); ++j)
	{
		up.insert(up.end(), S_TO_UC(f->get_up(j)));
	}

	/*~~~~~~~~~~~~~~~~~~~~~*/
	contraction_detection cd;
	/*~~~~~~~~~~~~~~~~~~~~~*/

	cd.set_latent_vector_cut_off(0.4);

	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// detect the contractions of the whole array of up.
	vector<contraction> c_set;
	cd.detect(up, c_set);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	srand(clock());
	for (long i = 0; i < n; ++i)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// calculate contractions from a subset of the array of up.
		long n0 = rand() * (long) up.size() / RAND_MAX;
		long n1 = rand() * (long) up.size() / RAND_MAX;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (n0 > n1)
		{
			/*~~~~~~~~~*/
			long n2 = n1;
			/*~~~~~~~~~*/

			n1 = n0;
			n0 = n2;
		}

		// size of up subset / size of the whole up array.
		memset(buf, 0, size);
		sprintf(buf, "%.2f%s", ((double) (n1 - n0) / ((long) up.size())) * 100, "%");
		diagnosis += buf;
		diagnosis += "\r\n";

		/*~~~~~~~~~~~~~~~~~~~*/
		// build the subset array.
		vector<char> up_subset;
		/*~~~~~~~~~~~~~~~~~~~*/

		for (j = n0; j <= n1; ++j)
		{
			up_subset.insert(up_subset.end(), up[j]);
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

		if (c_set.size() == 0 || c_subset.size() == 0)
		{
			continue;
		}

		// recherche de la contraction du tableau complet qui se trouve juste avant la
		// premiere contraction du sous-ensemble.
		for (j = 0; j < (long) c_set.size(); ++j)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			contraction c_temp(c_subset[0].get_start() + n0, c_subset[0].get_peak() + n0, c_subset[0].get_end() + n0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (c_set[j].get_start() >= n0)
			{
				if (!c_set[j].intersects(c_temp) && (c_set[j].get_end() < c_temp.get_start()))
				{
					/*~~~~~~~~~~~~~~~~~~~~~*/
					contraction c = c_set[j];
					/*~~~~~~~~~~~~~~~~~~~~~*/

					if (abs(c_set[j].get_end() - n0) > max_limit_from_end)
					{
						max_limit_from_end = abs(c_set[j].get_end() - n0);
						max_limit_from_end_pos0 = n0;
						max_limit_from_end_pos1 = n1;
						if (max_limit_from_end > 180)
						{
							max_limit_from_end = max_limit_from_end;
						}
					}

					if (abs(c_set[j].get_start() - n0) > max_limit_from_start)
					{
						max_limit_from_start = abs(c_set[j].get_start() - n0);
						max_limit_from_start_pos0 = n0;
						max_limit_from_start_pos1 = n1;
					}
				}

				break;
			}
		}

		// recherche de la contraction du tableau complet qui se trouve juste apres la
		// derniere contraction du sous-ensemble.
		for (j = (long) c_set.size() - 1; j >= 0; --j)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			contraction c_temp(c_subset[c_subset.size() - 1].get_start() + n0, c_subset[c_subset.size() - 1].get_peak() + n0, c_subset[c_subset.size() - 1].get_end() + n0);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (c_set[j].get_end() <= n1)
			{
				if (!c_set[j].intersects(c_temp) && c_set[j].get_start() > c_temp.get_end())
				{
					/*~~~~~~~~~~~~~~~~~~~~~*/
					contraction c = c_set[j];
					/*~~~~~~~~~~~~~~~~~~~~~*/

					if (abs(c_set[j].get_start() - n1) > min_limit_from_start)
					{
						min_limit_from_start = abs(c_set[j].get_start() - n1);
						min_limit_from_start_pos0 = n0;
						min_limit_from_start_pos1 = n1;
						if (min_limit_from_start > 180)
						{
							min_limit_from_start = min_limit_from_start;
						}
					}

					if (abs(c_set[j].get_end() - n1) > min_limit_from_end)
					{
						min_limit_from_end = abs(c_set[j].get_end() - n1);
						min_limit_from_end_pos0 = n0;
						min_limit_from_end_pos1 = n1;
					}
				}

				break;
			}
		}
	}

	delete f;

	/*~~~~~~~~~~~~~~~~*/
	time_t t1 = time(0);
	/*~~~~~~~~~~~~~~~~*/

	memset(buf, 0, size);
	sprintf(buf, "%ld", t1 - t0);
	diagnosis += "\r\n";
	diagnosis += "Time: ";
	diagnosis += buf;
	diagnosis += " seconds";
	diagnosis += "\r\n";

	diagnosis += "Max limit (start-n0): ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", max_limit_from_start);
	diagnosis += buf;

	memset(buf, 0, size);
	sprintf(buf, "%ld", max_limit_from_start_pos0);
	diagnosis += " (";
	diagnosis += buf;
	diagnosis += " - ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", max_limit_from_start_pos1);
	diagnosis += buf;
	diagnosis += ")";

	diagnosis += " / ";

	diagnosis += "Max limit (end-n0): ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", max_limit_from_end);
	diagnosis += buf;

	memset(buf, 0, size);
	sprintf(buf, "%ld", max_limit_from_end_pos0);
	diagnosis += " (";
	diagnosis += buf;
	diagnosis += " - ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", max_limit_from_end_pos1);
	diagnosis += buf;
	diagnosis += ")";

	diagnosis += "\r\n";

	diagnosis += "Min limit (start-n1): ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", min_limit_from_start);
	diagnosis += buf;

	memset(buf, 0, size);
	sprintf(buf, "%ld", min_limit_from_start_pos0);
	diagnosis += " (";
	diagnosis += buf;
	diagnosis += " - ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", min_limit_from_start_pos1);
	diagnosis += buf;
	diagnosis += ")";
	diagnosis += ".";

	diagnosis += " / ";

	diagnosis += "Min limit (end-n1): ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", min_limit_from_end);
	diagnosis += buf;

	memset(buf, 0, size);
	sprintf(buf, "%ld", min_limit_from_end_pos0);
	diagnosis += " (";
	diagnosis += buf;
	diagnosis += " - ";
	memset(buf, 0, size);
	sprintf(buf, "%ld", min_limit_from_end_pos1);
	diagnosis += buf;
	diagnosis += ")";
	diagnosis += ".";

	return diagnosis;
}

#if 0

/*
 =======================================================================================================================
 =======================================================================================================================
 */
void CPatternsTestDlg::FillPatientsList(void)
{
	if (!m_pACInstance)
	{
		return;
	}

	LMSModel::PatientSet & patientSet = m_pACInstance->GetPatients();

	for (long i = 0; i < patientSet.Cardinality(); ++i)
	{
		LMSModel::Patient * patient = patientSet[i + 1];

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		LMSCString name = patient->GetDisplayName();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_PatientList.AddString(name.GetBuffer(name.GetLength()));
		m_PatientList.SetCurSel(0);
	}
}

//
// =======================================================================================================================
//    Patient list selection changed.
// =======================================================================================================================
//
void CPatternsTestDlg::OnCbnSelchangePatientList(void)
{
	m_TracingsList.ResetContent();
	m_ContractionsList.ResetContent();
	m_ContractionCount = 0;
}

/*
 =======================================================================================================================
    Display patient's tracing data.
 =======================================================================================================================
 */
void CPatternsTestDlg::ShowTracingData(void)
{
	if (!m_pACInstance)
	{
		return;
	}

	LMSStorage::UITracingSet * pTracingSet = 0;

	LMSModel::PatientSet & patientSet = m_pACInstance->GetPatients();
	if (!patientSet.Cardinality())
	{
		return;
	}

	LMSModel::Patient * patient = patientSet[m_PatientList.GetCurSel() + 1];
	const LMSModel::Visit & v0 = patient->GetLastVisit();
	pTracingSet = const_cast<LMSStorage::UITracingSet *>(v0.GetUITracingSet());

	if (pTracingSet->CumulativeMillisecondTracingDuration() > 0)
	{
		if (pTracingSet->AbsoluteEndTime() > m_LastAbsTime)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// lorsqu'on obtient plus de points, on doit les récupérer en spécifiant un temps
			// de départ et un temps de fin. Le temps de départ est le dernier temps de fin +
			// 1 ms. Le temps de fin est pris de AbsoluteEndTime. Ce code est utilisé pour
			// comprendre le fonctionnement des tracés. En réalité, AbsoluteEndTime retourne
			// le plus long temps des deux fétus. Il ne représente pas nécessairement le temps
			// de fin du premier fetus. Pour obtenir le vrai temps de fin d'un fetus donné, il
			// faut procéder autrement.
			LMSValue::Time startTime = m_LastAbsTime;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			startTime.AddTime(1, LMSValue::Time::eDiffType::MILLISECOND);

			// fhr readings.
			LMSStorage::UITracing::Enumerator e = pTracingSet->GetFHRReadings(startTime, pTracingSet->AbsoluteEndTime(), 1);

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			int increment = pTracingSet->HRIncrement();
			LMSValue::Time time = m_LastAbsTime;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			while (e.MoveNext())
			{
				time.AddTime(pTracingSet->HRIncrement(), LMSValue::Time::eDiffType::MILLISECOND);

				/*~~~~~~~~~~~~~~~~~~~~~*/
				LMSValue::Time localTime;
				/*~~~~~~~~~~~~~~~~~~~~~*/

				time.GetLocalTime(localTime);

				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				unsigned char value = e.Current();
				long v = value;
				CString s0;
				CString s1;
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				s1.Format("%s%ld : %s", s0, v, localTime.GetTime(true, true, true, true));
				m_TracingsList.AddString(s1.GetBuffer());
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// display contractions.
			LMSValue::ContractionSet contractionSet;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			LMSValue::ContractionSet * temp = v0.GetContractions();
			v0.GetContractions()->GetSubSet(pTracingSet->AbsoluteStartTime(), pTracingSet->AbsoluteEndTime(), contractionSet);

			// ShowContractions(&contractionSet);
			m_LastAbsTime = pTracingSet->AbsoluteEndTime();

			// Toujours garder le dernier item sélectionné (scroll automatiquement).
			m_TracingsList.SetCurSel(m_TracingsList.GetCount() - 1);
		}
	}
}

#endif
#if 0
m_pACInstance = new PatternsClient();
PatternsClient *p = dynamic_cast<PatternsClient *>(m_pACInstance);
if (p)
{
	p->Initialize(false);

	// start timer that gets the tracings
	m_Timer = SetTimer(0, 0, 0);

	// FillPatientsList();
}
else
{
	delete m_pACInstance;
	m_pACInstance = 0;
}

#endif
#if 0
PatternsClient *p = dynamic_cast<PatternsClient *>(m_pACInstance);
if (p)
{
	p->Uninitialize();
}

#endif

// Patterns Application.cpp : Defines the class behaviors for the application.
#include "stdafx.h"
#include "Patterns Application.h"
#include "main frame.h"

#include "patterns gui, viewer input adapter.h"

#ifdef patterns_standalone
#include "file_loading.h"
#endif

#ifdef patterns_retrospective
#include "request_license_dialog.h"
#include "no_license_dlg.h"
#include "enter_activation_dlg.h"
#include "disclaimer_dlg.h"
#include "welcome_dlg.h"
#include "auto_update_dlg.h"
#endif

#include "main view.h"
#include "patterns application.h"

#include <htmlhelp.h>
#include <process.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// patterns_application
BEGIN_MESSAGE_MAP(patterns_application, CWinApp)
	ON_COMMAND(ID_VIEW_BASELINE, on_view_baseline)
	ON_COMMAND(ID_VIEW_MONTEVIDEO, on_view_montevideo)
	ON_COMMAND(ID_HYPERSTIMULATION_VIEW_SWITCH, on_toggle_hyperstim_view)
	ON_COMMAND(ID_TOGGLE_DISPLAY_EVENTS, on_toggle_display_events)
	ON_COMMAND(ID_DEBUG_MODE, on_debug_mode)
	ON_COMMAND(ID_DEBUG_SAVE, on_debug_save)
	ON_COMMAND(ID_DEBUG_OPEN, on_debug_open)
	ON_COMMAND(ID_PATIENT_LIST, on_patient_list)
	ON_COMMAND(ID_PATIENT_LIST_RETURN, on_patient_list_return)
	ON_COMMAND(ID_BEGINNING, on_go_beginning)
	ON_COMMAND(ID_END, on_go_end)
	ON_COMMAND(ID_MOVE_LEFT, on_go_left)
	ON_COMMAND(ID_NEXT_PAGE, on_go_next_page)
	ON_COMMAND(ID_PREVIOUS_PAGE, on_go_previous_page)
	ON_COMMAND(ID_MOVE_RIGHT, on_go_right)
	ON_COMMAND(ID_HELP, OnHelp)
END_MESSAGE_MAP()

// =====================================================================================================================
//    patterns_application construction
// =====================================================================================================================
patterns_application::patterns_application(void)
{
	m_bdebug_mode = false;
	m_adapter = 0;
	m_initialXMLFile = "";

	wait_dlg = NULL;
	m_splash_dlg = NULL;

	show_patient_list();
}

patterns_application::~patterns_application(void)
{
#ifdef patterns_retrospective

	if (m_pLicense != NULL)
	{
		delete m_pLicense;
		m_pLicense = NULL;
	}

#endif

}

int patterns_application::Run()
{
	return CWinApp::Run();
}

// =====================================================================================================================
//    Adapter creation. The adapter is created is an seperate thread to accelerate the creation and to give back the
//    control to the application right away.
// =======================================================================================================================
void patterns_application::start_adapter_creation(void)
{
	set_adapter_as_created(false);
	set_adapter_as_initialized(false);

	create_adapter();
	set_adapter_as_created(true);
	set_initial_state();
}

void patterns_application::create_adapter(void)
{
	m_adapter = new viewer_input_adapter();
}

// =====================================================================================================================
//    The adapter creation thread is completed, we finilize here the process.
// =======================================================================================================================
void patterns_application::set_initial_state(void)
{
	bool r = true;

	m_conductor.set_input_adapter(m_adapter);

	::CoInitialize(NULL);

#if defined(patterns_retrospective)
	// Ensure the splash screen is display at least 3 seconds and remove it
	wait_for_completion("", 3);
#endif

	// remove splash window.
	if (m_splash_dlg)
	{
		m_splash_dlg->Hide();
		m_splash_dlg = NULL;
	}

	((main_frame*)m_pMainWnd)->ShowWindow(SW_SHOWMAXIMIZED);
	((main_frame*)m_pMainWnd)->BringWindowToTop();
	((main_frame*)m_pMainWnd)->UpdateWindow();
	set_adapter_as_initialized(true);

	// force the application to quit if not allowed to run it.
	if (!r)
	{
		((main_frame*)m_pMainWnd)->get_main_view().quit();
		return;
	}

#if defined(patterns_viewer)
	r = false;

	string filename = m_initialXMLFile;
	if (filename.length() == 0)
	{
		CFileDialog fileDlg(TRUE, _T("*.XML"), NULL, OFN_DONTADDTORECENT | OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST, "All (*.XML;*.IN;*.v01)|*.IN;*.XML;*.v01|XML Files (*.XML)|*.XML|IN Files (*.IN)|*.IN|v01 Files (*.v01)|*.v01||", AfxGetMainWnd());
		if (fileDlg.DoModal() == IDOK)
		{
			filename = fileDlg.GetPathName();
		}
	}

	if (filename.length() != 0)
	{
		if (load_file(filename) != NULL)
		{
			r = true;
		}
		else
		{
			m_pMainWnd->MessageBox("Unable to load this Patterns archive file.", get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
		}
	}

	if (!r)
	{
		((main_frame*)m_pMainWnd)->get_main_view().quit();
		return;
	}

#elif defined(patterns_retrospective)
	if ((check_license_info()) && (check_disclaimer()))
	{
		open_welcome_screen();
	}
	else
	{
		((main_frame*)m_pMainWnd)->get_main_view().quit();
	}
#endif
}

#ifdef patterns_retrospective

// =====================================================================================================================
//    can run application? The application will run for the next 4 months after its compilation.
// =======================================================================================================================
bool patterns_application::check_license_info(void)
{
//#ifdef _DEBUG
//	return true;
//#endif
	try
	{
		m_pLicense = new license_validation();

		CString error = "";
#ifdef patterns_research
		if (!m_pLicense->initialize("Patterns Research", "EB30508E-CC0C-4450-AC8B-EF5DC2A34A37", error))
#else
		if (!m_pLicense->initialize("Retrospective CALM Patterns", "DCF72302-A0F0-4ff6-9A2F-5E915E2F285D", error))
#endif
		{
			m_pMainWnd->MessageBox(error, get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
			delete m_pLicense;
			m_pLicense = NULL;
			return false;
		}

		while (!m_pLicense->is_license_valid())
		{
			no_license_dlg dlg(m_pLicense, m_pMainWnd);
			int ret = dlg.DoModal();

			if (ret == IDC_BUTTON_REQUEST_CODE)
			{
				request_license_dialog dlg(m_pLicense);
				dlg.DoModal();
			}
			else if (ret == IDC_BUTTON_ENTER_CODE)
			{
				enter_activation_dlg dlg(m_pLicense, m_pMainWnd);
				dlg.DoModal();
			}
			else
			{
				break;
			}
		}

		return m_pLicense->is_license_valid();
	}
	catch(...)
	{
		m_pMainWnd->MessageBox("Unable to check the product license.", "ERROR LICENSE", MB_OK);
		return false;
	}
}

// =====================================================================================================================
//    Ensure the user agrees with the disclaimer
// =======================================================================================================================
bool patterns_application::check_disclaimer()
{
	// Check if the current user already agree to the disclaimer
	bool already_agreed = false;
	try
	{
		HKEY key;

#ifdef patterns_research
		if (::RegOpenKey(HKEY_CURRENT_USER, "Software\\PeriGen\\Retrospective PeriCALM Patterns - Research", &key) == ERROR_SUCCESS)
#else
		if (::RegOpenKey(HKEY_CURRENT_USER, "Software\\PeriGen\\Retrospective PeriCALM Patterns", &key) == ERROR_SUCCESS)
#endif
		{
			TCHAR buffer[256];
			DWORD len;
			DWORD type;

			len = sizeof(buffer) / sizeof(TCHAR);
			if ((::RegQueryValueEx(key, "Disclaimer", 0, &type, (LPBYTE) buffer, &len) == ERROR_SUCCESS) && (type == REG_SZ))
			{
				CString agreed_version = buffer;

				CString current_version = FILEVERSTR;
				already_agreed = (current_version.Compare(agreed_version) == 0);
			}

			::RegCloseKey(key);
		}
	}
	catch(...)
	{
		already_agreed = false;
	}

	if (already_agreed)
	{
		return true;
	}

	disclaimer_dlg dlg(m_pMainWnd);
	if (dlg.DoModal() == IDOK)
	{
		HKEY key;
		DWORD disp;

		// Remember for the current user
#ifdef patterns_research
		::RegCreateKeyEx(HKEY_CURRENT_USER, "Software\\PeriGen\\Retrospective PeriCALM Patterns - Research", 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &key, &disp);
#else
		::RegCreateKeyEx(HKEY_CURRENT_USER, "Software\\PeriGen\\Retrospective PeriCALM Patterns", 0, NULL, REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &key, &disp);
#endif

		string current_version = FILEVERSTR;

		::RegSetValueEx(key, "Disclaimer", 0, REG_SZ, (LPBYTE) (LPCTSTR) current_version.c_str(), current_version.length() + 1);
		::RegCloseKey(key);

		return true;
	}

	return false;
}

// =====================================================================================================================
//    Open a welcome screen and propose option
// =======================================================================================================================
void patterns_application::open_welcome_screen()
{
	welcome_dlg dlg(m_pMainWnd);
	int res = dlg.DoModal();

	// Autoupdate
	if (res == IDC_SL_AUTOUPDATE)
	{
		try
		{
			string update_version = auto_update_dlg::CheckForUpdate();
			string current_version = (string)FILEVERSTR + " Build " + (string)FILEDESCRSTR;

			if (update_version.compare(current_version) != 0)
			{
				string message = "Version " + current_version + " is installed.\r\nVersion " + update_version + " is available.\r\nDo you want to install it?";
				if (m_pMainWnd->MessageBox(message.c_str(), get_product_name().c_str(), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDYES)
				{
					// Show the progress dialog
					auto_update_dlg dlg(m_pMainWnd);
					switch (dlg.DoModal())
					{
						case IDABORT:
							m_pMainWnd->MessageBox("Error. Please try again later or contact PeriGen.", get_product_name().c_str(), MB_OK | MB_ICONERROR);
							break;

						case IDCANCEL:
							m_pMainWnd->MessageBox("Upgrade cancelled.", get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
							break;
					}
				}
			}
			else
			{
				m_pMainWnd->MessageBox("You are running the latest version", get_product_name().c_str(), MB_OK);
			}
		}
		catch(...)
		{
			m_pMainWnd->MessageBox("Unable to retrieve auto-update information.\r\nPlease try again later or contact PeriGen.", get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
		}
	}

	bool loadSamples = dlg.sampleChecked != 0;

	// Load the samples
	if (loadSamples)
	{
		CWaitCursor wc;
		show_wait_dialog("Analyzing the sample tracings...");
		try
		{
			string patient_id = ((viewer_input_adapter*)m_adapter)->load_samples();
			set_patient(patient_id);
		}
		catch(...)
		{
			close_wait_dialog();
			throw;
		}

		close_wait_dialog();
	}

	switch (res)
	{
		case IDC_SL_OPENFILE:
			on_debug_open();
			break;
	}
}
#endif

// =====================================================================================================================
//    get access to the application.
// =======================================================================================================================
patterns_application& patterns_application::get_application(void)
{
	return dynamic_cast<patterns_application&>(*AfxGetApp());
}

// The one and only patterns_application object
patterns_application theApp;

// =====================================================================================================================
//    close patient list - access method
// =======================================================================================================================
void patterns_application::close_patient_list(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().close_patient_list();
}

patient_list_view& patterns_application::get_patient_list_view(void)
{
	return ((main_frame*)m_pMainWnd)->get_main_view().get_patient_list_view();
}

patient_view& patterns_application::get_patient_view(void)
{
	return ((main_frame*)m_pMainWnd)->get_main_view().get_patient_view();
}

// =====================================================================================================================
//    patterns_application initialization
// =====================================================================================================================
BOOL patterns_application::InitInstance(void)
{
	CWaitCursor wait;

	// InitCommonControls() is required on Windows XP if an application manifest specifies use of ComCtl32.dll
	// version 6 or later to enable visual styles. Otherwise, any window creation will fail.
	InitCommonControls();
	CWinApp::InitInstance();

	bool patient_list_pinned = false;

	for (long i = 0; i < __argc; ++i)
	{
		CString arg = __argv[i];

		if (arg.CompareNoCase("/patient_list_always_on") == 0)
		{
			patient_list_pinned = true;
		}
		else if (arg.CompareNoCase("/file") == 0)
		{
			if (i + 1 < __argc)
			{
				m_initialXMLFile = __argv[i + 1];
			}
		}
	}

	main_frame*	 pFrame = new main_frame;

	if (!pFrame)
	{
		return FALSE;
	}

	m_pMainWnd = pFrame;

	pFrame->Create(NULL, "PeriCALM® Patterns™", WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS, CRect(10, 10, 810, 610), NULL, NULL, NULL, NULL);
	pFrame->LoadAccelTable("patterns_accelerators");

	// Set the application icon.
	HICON icon = LoadIcon("patterns_icon");

	pFrame->SetIcon(icon, true);

	// Initialize the frame... but don't show it yet
	((main_frame*)m_pMainWnd)->ShowWindow(SW_HIDE);
	//((main_frame*)m_pMainWnd)->get_main_view().m_is_patient_list_pinned = patient_list_pinned;

	// Do not remove this line !!!! It's there to create the message pump of this thread. If this is not there, the
	// push message in the second thread will fail !!
	pFrame->StartPumping();

	// Set the proper application name
	CString title;

#ifdef patterns_viewer
	title = "PeriCALM® Patterns™ Viewer";
#elif defined(patterns_research)
	title = "PeriCALM® Patterns Retrospective™ - Research ONLY";
#elif defined(patterns_standalone)
	title = "PeriCALM® Patterns Retrospective™";
#else
	title.LoadString(NULL, application_lms_title);
#endif
	set_product_name((LPCTSTR) title);
	update_title_bar();

	// Display the splash window.
	m_splash_dlg = new CSplashScreen();
	m_splash_dlg->Create(pFrame, NULL, 0, CSS_CENTERSCREEN);

#ifdef patterns_retrospective
	m_splash_dlg->SetBitmap(IDB_SPLASH_RETROSPECTIVE, -1, -1, -1);
	m_splash_dlg->SetTextRect(CRect(259, 184, 600, 220));
#else
	m_splash_dlg->SetBitmap(IDB_SPLASH, -1, -1, -1);
	m_splash_dlg->SetTextRect(CRect(259, 184, 600, 220));
#endif

	CString version = "Version ";
	version += FILEVERSTR;
	version += " Build ";
	version += FILEDESCRSTR;

	m_splash_dlg->SetTextFont("Arial", 80, CSS_TEXT_NORMAL);
	m_splash_dlg->SetTextColor(RGB(118, 118, 118));
	m_splash_dlg->SetTextFormat(DT_LEFT | DT_TOP | DT_SINGLELINE);
	m_splash_dlg->SetText(version.GetBuffer(version.GetLength()));

	version.ReleaseBuffer();

	m_splash_dlg->Show();
	m_splash_dlg->UpdateWindow();

	start_adapter_creation();

	return TRUE;
}

// =====================================================================================================================
//    is patient list shown to the user?
// =======================================================================================================================
bool patterns_application::is_patient_list_shown()
{
	string msg;
	return is_patient_list_shown(msg);
}

// =====================================================================================================================
//    is patient list shown to the user?
// =======================================================================================================================
bool patterns_application::is_patient_list_shown(string& message)
{
	message = "";
	if (!m_bpatientlist)
	{
		return false;
	}

	return true;
}

// =====================================================================================================================
//    display application help file.
// =======================================================================================================================
void patterns_application::OnHelp(void)
{
	CString helpFilename;

#ifdef patterns_retrospective
	helpFilename.LoadString(NULL, application_retrospective_help);
#else
	helpFilename.LoadString(NULL, application_lms_help);
#endif

	// the path is hardcoded for now... todo.
	::HtmlHelp(GetDesktopWindow(), TEXT(helpFilename), HH_DISPLAY_INDEX, 0);
}

// =====================================================================================================================
//    on idle, we must pass an idle to the conductor to let him
// =======================================================================================================================
BOOL patterns_application::OnIdle(LONG n)
{
	if (CWinApp::OnIdle(n))
	{
		return TRUE;
	}

	return FALSE;
}

// =====================================================================================================================
//    toggle between normal and debug mode.
// =======================================================================================================================
void patterns_application::on_debug_mode(void)
{
	get_patient_view().toggle_debug_keys_access();
}

void patterns_application::wait_for_completion(CString message, int seconds)
{
	// Wait a little bit more to let the engine process the events
	if ((wait_dlg) && (message.GetLength() > 0))
	{
		wait_dlg->SetMessage(message);
	}

	for (int i = 0; i < seconds * 10; ++i)
	{
		m_pMainWnd->UpdateWindow();
		if (wait_dlg)
		{
			wait_dlg->UpdateWindow();
		}

		Sleep(100);
	}
}

void patterns_application::show_wait_dialog(CString message)
{
	m_pMainWnd->UpdateWindow();

	if (wait_dlg != NULL)
	{
		wait_dlg->SetMessage(message);
		return;
	}

	wait_dlg = new please_wait_dlg();
	wait_dlg->Create(please_wait_dlg::IDD, m_pMainWnd);
	wait_dlg->SetMessage(message);
	wait_dlg->ShowWindow(SW_SHOW);

	wait_dlg->UpdateWindow();
	m_pMainWnd->UpdateWindow();
}

void patterns_application::close_wait_dialog()
{
	if (wait_dlg != NULL)
	{
		wait_dlg->ShowWindow(SW_HIDE);
		wait_dlg->DestroyWindow();
		delete wait_dlg;
		wait_dlg = NULL;

		m_pMainWnd->UpdateWindow();
	}
}

#ifdef patterns_retrospective

#include "Rendevouz.h"
#include <deque>

struct LoadingFileStructure
{
	deque<string> files;

	deque<input_adapter::patient *> patients;
	deque<fetus *> fetuses;

	deque<string> errors;
};

unsigned int patterns_application::LoadFileThread(LPVOID data)
{
	LoadingFileStructure*  pFiles = (LoadingFileStructure *) (((rendevouz::CBaseThread *) data)->UserData());
	patterns_application&  app = get_application();

	while (true)
	{
		// Retrieve the filename to load
		app.m_thread_mutex.acquire();

		string filename;
		if (pFiles->files.size() == 0)
		{
			app.m_thread_mutex.release();
			break;
		}

		filename = pFiles->files.front();
		pFiles->files.pop_front();

		app.m_thread_mutex.release();

		// Load the file
		input_adapter::patient* pPatient = new input_adapter::patient();

		fetus*	pCurFetus = new fetus();

		ConfigData configData;
		bool bLoaded = file_loading::load_file(filename, pPatient, *pCurFetus, configData);

#if defined(patterns_retrospective)
		if ((pCurFetus->GetEventsCount() == 0) && (pCurFetus->GetContractionsCount() == 0))
		{
			pCurFetus->compute_now();
		}
#endif

		// Store the result
		app.m_thread_mutex.acquire();
		if (bLoaded)
		{
			pFiles->patients.push_back(pPatient);
			pFiles->fetuses.push_back(pCurFetus);
		}
		else
		{
			delete pPatient;
			delete pCurFetus;

			pFiles->errors.push_back(filename);
		}

		app.m_thread_mutex.release();
	}

	// Done
	return 0;
}
#endif

// =====================================================================================================================
//    uses to export data of a given fetus to an external file
// =======================================================================================================================
void patterns_application::on_debug_open(void)
{
#ifdef patterns_retrospective

	CFileDialog fileDlg(TRUE, _T("*.*"), NULL, OFN_DONTADDTORECENT | OFN_HIDEREADONLY | OFN_ALLOWMULTISELECT | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST, "All (*.XML;*.IN;*.v01)|*.IN;*.XML;*.v01|XML Files (*.XML)|*.XML|IN Files (*.IN)|*.IN|v01 Files (*.v01)|*.v01||", AfxGetMainWnd());
	char fileBuffer[50000] = {0};
	fileDlg.m_ofn.lpstrFile = fileBuffer;
	fileDlg.m_ofn.nMaxFile = sizeof(fileBuffer);

	if (fileDlg.DoModal() != IDOK)
	{
		return;
	}

	// How many cores?
	SYSTEM_INFO siSysInfo;
	::GetSystemInfo(&siSysInfo);

	int nbCore = siSysInfo.dwNumberOfProcessors - 1;	// We don't want to kill the computer so core - 1 !
	if (nbCore < 1)
	{
		nbCore = 1;
	}

	CWaitCursor wait;
	show_wait_dialog("Analyzing the tracing... ");

	clock_t start;
	start = clock();

	LoadingFileStructure data;

	POSITION pos = fileDlg.GetStartPosition();
	while (pos)
	{
		CString filename = fileDlg.GetNextPathName(pos);

		data.files.push_back(filename.GetBuffer(filename.GetLength()));
		filename.ReleaseBuffer();
	}

	if (data.files.size() > 0)
	{
		set_patient("");
	}

	rendevouz::CSyncRendevouz rendevouz;
	for (int core = 0; core < nbCore; ++core)
	{
		rendevouz.AddThread(LoadFileThread, false, (LPVOID) & data);
	}

	int i = 0;
	while (!rendevouz.Wait(10))
	{
		// Keep windows happy and avoid the message "Not responding"
		MSG msg;
		PeekMessage(&msg, NULL, 0, 0, PM_NOREMOVE);

		Sleep(90);

		if (i % 10 == 0)
		{
			switch ((i / 10) % 10)
			{
				case 0:
					show_wait_dialog("Analyzing the tracing");
					break;

				case 1:
					show_wait_dialog("Analyzing the tracing.");
					break;

				case 2:
					show_wait_dialog("Analyzing the tracing..");
					break;

				default:
					show_wait_dialog("Analyzing the tracing...");
					break;
			}
		}
		else
		{
			wait_dlg->UpdateWindow();
			m_pMainWnd->UpdateWindow();
		}

		++i;
	}

	long seconds_processed = 0;
	long files_processed = 0;

	string key = "";
	while (data.patients.size() > 0)
	{
		input_adapter::patient * p = data.patients.front();

		fetus*	f = data.fetuses.front();

		data.fetuses.pop_front();
		data.patients.pop_front();

		seconds_processed += f->get_number_of_fhr() / f->get_hr_sample_rate();
		files_processed++;
		((viewer_input_adapter*)m_adapter)->add_patient(p, f);
		key = p->get_key();

		delete f;
	}

	set_patient(key);

	close_wait_dialog();

	long loading_time = (clock() - start) / CLOCKS_PER_SEC;

	if (files_processed > 1)
	{
		m_pMainWnd->MessageBox((conductor::to_string(files_processed) + " files opened (" + conductor::to_string(seconds_processed / 3600) + " hours loaded in " + conductor::to_string(loading_time) + " seconds).").c_str(), get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
	}

	while (data.errors.size() > 0)
	{
		string filename = data.errors.front();
		data.errors.pop_front();

		string message = "Unable to load this file\r\n";
		message += filename;
		m_pMainWnd->MessageBox(message.c_str(), get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
	}
#endif
}

// =====================================================================================================================
//    uses to export data of a given fetus to an external file. The data is taken from CALM cache directly.
// =======================================================================================================================
void patterns_application::on_debug_save(void)
{
#if !defined(patterns_viewer)
	// TODO
#endif
}

// =====================================================================================================================
//    Methods to navigate through the current displayed tracing.
// =======================================================================================================================
void patterns_application::on_go_beginning(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().go_beginning();
}

void patterns_application::on_go_end(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().go_end();
}

void patterns_application::on_go_left(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().move(-20);
}

void patterns_application::on_go_next_page(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().go_next_page();
}

void patterns_application::on_go_previous_page(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().go_previous_page();
}

void patterns_application::on_go_right(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().move(20);
}

// =====================================================================================================================
//    show/hide patient list.
// =======================================================================================================================
void patterns_application::on_patient_list(void)
{
	string msg;
	if (is_patient_list_shown(msg))
	{
		((main_frame*)m_pMainWnd)->get_main_view().toggle_patient_list();
	}
	else if (msg.length() > 0)
	{
		((main_frame*)m_pMainWnd)->MessageBox(msg.c_str(), get_product_name().c_str(), MB_OK | MB_ICONEXCLAMATION);
	}
}

// =====================================================================================================================
//    return key pressed. Close patient list and give the focus to the patient view (tracing).
// =======================================================================================================================
void patterns_application::on_patient_list_return(void)
{
	if (is_patient_list_shown())
	{
		((main_frame*)m_pMainWnd)->get_main_view().close_patient_list();
	}
}

// =====================================================================================================================
//    Toggle hypercontractility view. The one with the tracing and the one with only the colors trends.
// =======================================================================================================================
void patterns_application::on_toggle_hyperstim_view(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().toggle_toco();
}

// =====================================================================================================================
//    Toggle events display view
// =======================================================================================================================
void patterns_application::on_toggle_display_events(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().toggle_events();
}

// =====================================================================================================================
//    toggle baselise view.
// =======================================================================================================================
void patterns_application::on_view_baseline(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().toggle_baseline();
}

// =====================================================================================================================
//    toggle baselise view.
// =======================================================================================================================
void patterns_application::on_view_montevideo(void)
{
	((main_frame*)m_pMainWnd)->get_main_view().toggle_montevideo();
}

// =====================================================================================================================
//    Intercept keyboard for zoom
// =======================================================================================================================
BOOL patterns_application::PreTranslateMessage(MSG* pMsg)
{
	// support of the zoom feature using space bar.
	switch (pMsg->message)
	{
		case WM_KEYDOWN:
			if (pMsg->wParam == VK_SPACE)
			{
				((main_frame*)m_pMainWnd)->get_main_view().zoom_down();
			}
			break;

		case WM_KEYUP:
			if (pMsg->wParam == VK_SPACE)
			{
				((main_frame*)m_pMainWnd)->get_main_view().zoom_up();
			}
			break;
	}

	return CWinApp::PreTranslateMessage(pMsg);
}

// =====================================================================================================================
//    This allows to change the current patient from anywhere in the application
// =======================================================================================================================
void patterns_application::set_patient(const string& id)
{
	((main_frame*)m_pMainWnd)->get_main_view().set_patient(id); // update banner info.
	((main_frame*)m_pMainWnd)->get_main_view().get_patient_list_view().set_patient(id);
	((main_frame*)m_pMainWnd)->get_main_view().get_patient_view().set_patient(id);
}

string patterns_application::get_patient()
{
	return ((main_frame*)m_pMainWnd)->get_main_view().get_patient();
}

// =====================================================================================================================
//    show/hide patient list.
// =======================================================================================================================
void patterns_application::show_patient_list(bool s)
{
	m_bpatientlist = s;
}

// =====================================================================================================================
//    Set application title bar text.
// =======================================================================================================================
void patterns_application::update_title_bar(void)
{
	string text = get_product_name();
	m_pMainWnd->SetWindowText(text.c_str());
}

void patterns_application::set_adapter_as_initialized(bool b)
{
	m_badapter_initialized = b;
	((main_frame*)m_pMainWnd)->allow_application_to_close(m_badapter_initialized);
}

#if defined(patterns_standalone)

bool patterns_application::load_file(string filename)
{
	input_adapter::patient* pPatient = new input_adapter::patient();
	fetus curFetus;

	ConfigData configData;
	if (!file_loading::load_file(filename, pPatient, curFetus, configData))
	{
		delete pPatient;
		return false;
	}

#ifdef patterns_viewer
	m_conductor.apply_config(configData);
#endif

#if defined(patterns_retrospective)
	if ((curFetus.GetEventsCount() == 0) && (curFetus.GetContractionsCount() == 0))
	{
		curFetus.compute_now();
	}
#endif

	((viewer_input_adapter*)m_adapter)->add_patient(pPatient, &curFetus);
	set_patient(pPatient->get_key());

	return true;
}

#endif

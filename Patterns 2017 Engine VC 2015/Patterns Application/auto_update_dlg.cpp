// auto_update_dlg.cpp : implementation file
#include "stdafx.h"
#include "auto_update_dlg.h"

#include <afxcmn.h>
#include "afxinet.h"

#ifdef patterns_research
#define SERVER_UPDATE_URL	"http://www.lmsmedical.com/patterns-support/auto-update/researchpatterns"
#else
#define SERVER_UPDATE_URL	"http://www.lmsmedical.com/patterns-support/auto-update/retrospectivepatterns/"
#endif
#define SERVER_UPDATE_CONFIG	_T("update.txt")
#define SERVER_UPDATE_SETUP		_T("update.exe")

// auto_update_dlg dialog
IMPLEMENT_DYNAMIC(auto_update_dlg, CDialog)

auto_update_dlg::auto_update_dlg(CWnd* pParent /* NULL */ ) :
	CDialog(auto_update_dlg::IDD, pParent)
{
	InitializeCriticalSection(&critical_section);
}

auto_update_dlg::~auto_update_dlg()
{
	DeleteCriticalSection(&critical_section);
}

// =====================================================================================================================
// The method called by the thread
// =====================================================================================================================
UINT UpgradeApplicationWorker(LPVOID pParam)
{
	((auto_update_dlg*)pParam)->DoUpgrade();
	return 1;
}

const UINT ID_TIMER_REFRESH = 0x1001;

// =====================================================================================================================
// center the window before displaying it.
// =======================================================================================================================
BOOL auto_update_dlg::OnInitDialog(void)
{
	CDialog::OnInitDialog();

	CenterWindow();
	ShowWindow(SW_SHOW);
	((CProgressCtrl*)GetDlgItem(IDC_PROGRESS_UPGRADE))->SetRange(0, 100);
	((CProgressCtrl*)GetDlgItem(IDC_PROGRESS_UPGRADE))->SetPos(0);

	upgrade_cancelled = false;
	upgrade_progress = 0;
	upgrade_failed = false;
	upgrade_running = true;

	AfxBeginThread(UpgradeApplicationWorker, this, THREAD_PRIORITY_NORMAL);

	SetTimer(ID_TIMER_REFRESH, 1000, 0);

	return true;
}

void auto_update_dlg::OnTimer(UINT_PTR timer_id)
{
	if (timer_id == ID_TIMER_REFRESH)
	{
		// Get all data in a secure thread safe way
		EnterCriticalSection(&critical_section);

		int progress = upgrade_progress;
		bool cancelled = upgrade_cancelled;
		bool running = upgrade_running;
		bool failed = upgrade_failed;

		LeaveCriticalSection(&critical_section);

		// Update the progress bar
		((CProgressCtrl*)GetDlgItem(IDC_PROGRESS_UPGRADE))->SetPos(upgrade_progress);

		// Check for cancel
		if (cancelled)
		{
			if (!running)
			{
				KillTimer(ID_TIMER_REFRESH);
				EndDialog(IDCANCEL);
			}
		}

		// Check for failure
		else if (failed)
		{
			KillTimer(ID_TIMER_REFRESH);
			EndDialog(IDABORT);
		}
	}
}

void auto_update_dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BOOL auto_update_dlg::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message)
{
	SetCursor(::LoadCursor(NULL, IDC_WAIT));
	return TRUE;
}

BEGIN_MESSAGE_MAP(auto_update_dlg, CDialog)
	ON_WM_SETCURSOR()
	ON_BN_CLICKED(IDC_CANCEL_UPGRADE, OnCancelUpgrade)
	ON_WM_TIMER()
END_MESSAGE_MAP()

void auto_update_dlg::OnCancelUpgrade()
{
	EnterCriticalSection(&critical_section);
	upgrade_cancelled = true;
	LeaveCriticalSection(&critical_section);
}

// =====================================================================================================================
// Check if an update is required
// =====================================================================================================================
string auto_update_dlg::CheckForUpdate()
{
	// Download the content of the config file
	string url = string(SERVER_UPDATE_URL) + string(SERVER_UPDATE_CONFIG);
	CInternetSession isession;
	CStdioFile*	 pTargFile = isession.OpenURL(url.c_str(), 1, INTERNET_FLAG_TRANSFER_BINARY | INTERNET_FLAG_RELOAD | SECURITY_IGNORE_ERROR_MASK);

	char buffer[4096];
	::ZeroMemory(buffer, sizeof(buffer));

	DWORD dwBlockSize = pTargFile->Read(buffer, sizeof(buffer) - 1);
	if (dwBlockSize <= 0)
	{
		throw exception("Unable to download the config file (code - 1)");
	}
	if (dwBlockSize > 100)
	{
		throw exception("Unable to download the config file (code - 2)");
	}

	return (LPCTSTR) buffer;
}

// =====================================================================================================================
// Call from a worker thread
// =====================================================================================================================
void auto_update_dlg::DoUpgrade()
{
	HANDLE hFile = INVALID_HANDLE_VALUE;
	try
	{
		// Open a connection to the update URL
		string url = string(SERVER_UPDATE_URL) + string(SERVER_UPDATE_SETUP);

		// Create the file name to store the downloaded file
		char strExePath[MAX_PATH];
		GetModuleFileName(NULL, strExePath, MAX_PATH);

		char drive[_MAX_DRIVE];
		char path[_MAX_DIR];
		_splitpath(strExePath, drive, path, NULL, NULL);

		string filename = string(drive) + string(path) + string(SERVER_UPDATE_SETUP);

		// Download the file
		CInternetSession isession;
		CStdioFile*	 pTargFile = isession.OpenURL(url.c_str(), 1, INTERNET_FLAG_TRANSFER_BINARY | INTERNET_FLAG_RELOAD | SECURITY_IGNORE_ERROR_MASK);
		int size = (int)pTargFile->SeekToEnd();
		pTargFile->SeekToBegin();

		hFile = CreateFile(filename.c_str(), GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hFile == INVALID_HANDLE_VALUE)
		{
			throw exception("Unable to create the destination file on the disk");
		}

		DWORD dwBlockSize;
		int done = 0;
		char buffer[10240];
		while (dwBlockSize = pTargFile->Read(buffer, sizeof(buffer)))	// Read from file
		{
			DWORD dwNumWritten;
			WriteFile(hFile, buffer, dwBlockSize, &dwNumWritten, NULL);

			// File write error
			if (dwBlockSize != dwNumWritten)
			{
				pTargFile->Close();
				throw exception("Unable to write to the destination file on the disk");
			}

			done += dwNumWritten;

			EnterCriticalSection(&critical_section);

			upgrade_progress = (int)((100 * done) / (float)size);

			bool cancelled = upgrade_cancelled;

			LeaveCriticalSection(&critical_section);

			if (cancelled)
			{
				pTargFile->Close();
				CloseHandle(hFile);

				EnterCriticalSection(&critical_section);
				upgrade_running = false;
				LeaveCriticalSection(&critical_section);

				return;
			}
		}

		CloseHandle(hFile);
		hFile = INVALID_HANDLE_VALUE;

		// Execute the downloaded file
		SHELLEXECUTEINFO sei;
		::ZeroMemory(&sei, sizeof(SHELLEXECUTEINFO));

		sei.cbSize = sizeof(SHELLEXECUTEINFO);
		sei.lpVerb = TEXT("open");
		sei.lpFile = filename.c_str();
		sei.nShow = SW_SHOWNORMAL;

		if (ShellExecuteEx(&sei) != TRUE)
		{
			throw exception("Unable to execute the downloaded installer");
		}
	}
	catch(...)
	{
		if (hFile != INVALID_HANDLE_VALUE)
		{
			CloseHandle(hFile);
		}

		EnterCriticalSection(&critical_section);
		upgrade_running = false;
		upgrade_failed = true;
		LeaveCriticalSection(&critical_section);
	}
}

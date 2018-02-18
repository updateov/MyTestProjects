// This is the main DLL file.

#include "stdafx.h"
#include "PatternsContainerWnd.h"
#include "ChecklistContainerWnd.h"
#include "ExportNativeManager.h"
#include "CLRPatternsUserControls.h"
using namespace patterns;
namespace CLRPatternsUserControls {

	BasePatternsUserControl::BasePatternsUserControl(bool checklistEnabled)
	{
		AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0);
		InitializeComponent();
		if(checklistEnabled)
			m_pCtrl = new CheckListContainerWnd();
		else
			m_pCtrl = new PatternsContainerWnd();

	}

	void BasePatternsUserControl::OnHandleCreated(EventArgs^ e)
	{
		if(m_pCtrl != NULL)
		{
			HWND hWnd = (HWND)Handle.ToPointer();
			RECT rect;
			::GetClientRect(hWnd, &rect);
			if (!m_pCtrl->Create(&rect, hWnd))
			{
	
			}
		}
		
		UserControl::OnHandleCreated(e);
	}

	void BasePatternsUserControl::OnResize(EventArgs^ e)
	{
		UserControl::OnResize(e);
		if(m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{

			HWND hWnd = (HWND)Handle.ToPointer();
			RECT rect;
			::GetClientRect(hWnd, &rect);
			m_pCtrl->MoveWindow(&rect);
			
		}

	}

	void BasePatternsUserControl::SetInitialData(String^ url, String^ patientID)
	{
		

		CString urlStr = String::IsNullOrEmpty(url)? "" : url;
		CString patientIDStr = String::IsNullOrEmpty(patientID)? "" : patientID;
	
			
		if(m_pCtrl != NULL)
		{
			m_pCtrl->SetInitialData(urlStr, patientIDStr);
		}
	}


	CLRPatternsControl::CLRPatternsControl()
		: BasePatternsUserControl(false)
	{
		
	}

	ChecklistControl::ChecklistControl()
		: BasePatternsUserControl(true)
	{
		
	}
}
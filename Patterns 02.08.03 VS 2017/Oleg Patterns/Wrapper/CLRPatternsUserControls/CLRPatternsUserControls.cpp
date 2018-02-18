// This is the main DLL file.

#include "stdafx.h"
#include "PatternsContainerWnd.h"
#include "ChecklistContainerWnd.h"
#include "ExportNativeManager.h"
#include "CLRPatternsUserControls.h"
using namespace patterns;
using namespace PeriGen::Patterns::WPFLibrary::Screens;

namespace CLRPatternsUserControls {
#define PATTERNS_UDI "*+B087PATTERNS02081/$$7020802D*"
#define CUES_UDI "*+B087PATT02081/$$70208027*"

	BasePatternsUserControl::BasePatternsUserControl(bool checklistEnabled)
	{
		m_roundByBaselineFHRValue = 1;
		m_roundByMontevideoUnits = 1;

		m_IsInitialized = false;
		AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0);
		InitializeComponent();
		m_checklistEnabled = checklistEnabled;
		if(checklistEnabled)
			m_pCtrl = new CheckListContainerWnd();
		else
			m_pCtrl = new PatternsContainerWnd();

		//m_exportCtrl = gcnew PatternsWPFAdapter();
		//m_exportCtrl->SetScreenWidth(255);
		//m_exportCtrl->SetTimeRange(true);

		//m_exportCtrl->IntervalPressed += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnIntervalPressed);
		//m_exportCtrl->IntervalMouseOver += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnIntervalMouseOver);
		//m_exportCtrl->IntervalMouseLeave += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnIntervalMouseLeave);			
		//m_exportCtrl->ExportDialogClosed += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnExportDialogClosed);
		//m_exportCtrl->TimeRangeChanged += gcnew EventHandler<TimeRangeArgs^>(this, &BasePatternsUserControl::OnTimeRangeChanged);
	}

	void BasePatternsUserControl::OnIntervalPressed(Object^ sender, IntervalEventArgs^ args)
	{

		if(m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{		
			DATE from = args->StartTime.ToOADate();
			DATE to = args->EndTime.ToOADate(); 
			int id = args->IntervalID ;
			EportBarActiveChunck activeChunk(from, to, id);
			::SendMessageA(m_pCtrl->m_hWnd, MOUSE_OVER_CALLBACK_MSG, 1, (LPARAM)&activeChunk);
		}

		if(IsExternalUsing == true)
		{
			RaiseIntervalPressedEvent(args);
		}
	}

	void BasePatternsUserControl::OnIntervalMouseOver(Object^ sender, IntervalEventArgs^ args)
	{
		if(m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			DATE from = args->StartTime.ToOADate();
			DATE to = args->EndTime.ToOADate(); 
			EportBarActiveChunck activeChunk(from, to, -1);
			::SendMessageA(m_pCtrl->m_hWnd, MOUSE_OVER_CALLBACK_MSG, 0, (LPARAM)&activeChunk);
		}
	}

	void BasePatternsUserControl::OnIntervalMouseLeave(Object^ sender, IntervalEventArgs^ args)
	{
		if(m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			DATE from = args->StartTime.ToOADate();
			DATE to = args->EndTime.ToOADate(); 
			EportBarActiveChunck activeChunk(from, to, -1);
			::SendMessageA(m_pCtrl->m_hWnd, MOUSE_LEAVE_CALLBACK_MSG, 0, (LPARAM)&activeChunk);
		}
	}

	void BasePatternsUserControl::OnExportDialogClosed(Object^ sender, IntervalEventArgs^ args)
	{
		if(m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{	
			DATE from = args->StartTime.ToOADate();
			DATE to = args->EndTime.ToOADate(); 
			EportBarActiveChunck activeChunk(from, to, -1);
			::SendMessageA(m_pCtrl->m_hWnd, MOUSE_LEAVE_CALLBACK_MSG, 1, (LPARAM)&activeChunk);
		}
	}

	void BasePatternsUserControl::SetIntervalExported(DateTime fromTime, DateTime toTime)
	{
		if (m_exportCtrl != nullptr)
			m_exportCtrl->SaveIntervalExportedState(fromTime, toTime, m_visitKey);
	}

	void BasePatternsUserControl::PerformExport(DateTime fromTime, DateTime toTime)
	{
		SetIntervalExported(fromTime, toTime);
		DoOnEndExport(fromTime, toTime, true);
	}

	void BasePatternsUserControl::CancelExport(DateTime fromTime, DateTime toTime)
	{
		DoOnEndExport(fromTime, toTime, false);
	}

	void BasePatternsUserControl::DoOnEndExport(DateTime fromTime, DateTime toTime, bool isExported)
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			m_exportCtrl->ResetIntervalSelection(fromTime, isExported);
			DATE from = fromTime.ToOADate();
			DATE to = toTime.ToOADate();
			EportBarActiveChunck activeChunk(from, to, -1);
			::SendMessageA(m_pCtrl->m_hWnd, MOUSE_LEAVE_CALLBACK_MSG, 1, (LPARAM)&activeChunk);
		}
	}
	void BasePatternsUserControl::OnTimeRangeChanged(Object^ sender, TimeRangeArgs^ args)
	{
		if(m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{		
			::SendMessageA(m_pCtrl->m_hWnd, TIME_RANGE_CHANGE_CALLBAK_MSG, 0, (LPARAM)args->TimeRange);

			RaiseIntervalTimeRangeChangedEvent(args->TimeRange);
		}
	}

	void BasePatternsUserControl::RaiseIntervalTimeRangeChangedEvent(int intervalDuration)
	{
		IntervalTimeRangeArgs^ args = gcnew IntervalTimeRangeArgs();
		args->IntervalDuration = intervalDuration;

		OnIntervalTimeRangeChangedEvent(args);
	}

	void BasePatternsUserControl::SetSelectedInterval(DateTime startTime)
	{
		if (m_exportCtrl != nullptr)
		{
			if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
			{
				DATE from = startTime.ToOADate();
				m_pCtrl->SendMessage(MSG_MOVE_TO_INTERVAL_MSG, 0, (LPARAM)&from);
			}
			m_exportCtrl->SetSelectedInterval(startTime);
		}
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
			if (m_exportCtrl != nullptr)
			{
				CRect barRect, buttonRect;
				::GetWindowRect(m_pCtrl->GetExportBarWndHandle(), &barRect);
				::GetWindowRect(m_pCtrl->GetExportButtonWndHandle(), &buttonRect);
				m_exportCtrl->InitNavigationPanel(IntPtr((long)m_pCtrl->GetExportBarWndHandle()), 0, 0, barRect.Width(), barRect.Height());
				m_exportCtrl->InitExportButton(IntPtr((long)m_pCtrl->GetExportButtonWndHandle()), buttonRect.left, buttonRect.top, buttonRect.Width(), buttonRect.Height());
				System::DateTime startTime(2016, 12, 4, 14, 30, 0, 0);
				m_exportCtrl->SetStartTime(startTime);
				m_exportCtrl->SetScreenWidth(255);
				ResizeExportBar(true);
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
			ResizeExportBar(true);
			
		}

	}

	void BasePatternsUserControl::ResizeExportBar(bool valid)
	{
		if (m_exportCtrl != nullptr)
		{
			CRect barRect, buttonRect;
			::GetWindowRect(m_pCtrl->GetExportBarWndHandle(), &barRect);
			::GetWindowRect(m_pCtrl->GetExportButtonWndHandle(), &buttonRect);
			m_exportCtrl->ResizeNavigationPanel(0, 0, barRect.Width(), barRect.Height());
			m_exportCtrl->ResizeExportButton(0, 0, buttonRect.Width(), buttonRect.Height());
		}

	}
	void BasePatternsUserControl::SetInitialData(String^ url, String^ patientID, String^ userID, String^ username, String^ permissions, bool exportSupport, String^ patternsVersion)
	{
		m_visitKey = patientID;
		m_userID = userID;
		m_pluginURL = url;
		if (exportSupport)
		{
			m_exportCtrl = gcnew PatternsWPFAdapter();
			m_exportCtrl->SetScreenWidth(255);
			m_exportCtrl->SetTimeRange(true);

			m_exportCtrl->IntervalPressed += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnIntervalPressed);
			m_exportCtrl->IntervalMouseOver += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnIntervalMouseOver);
			m_exportCtrl->IntervalMouseLeave += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnIntervalMouseLeave);			
			m_exportCtrl->ExportDialogClosed += gcnew EventHandler<IntervalEventArgs^>(this, &BasePatternsUserControl::OnExportDialogClosed);
			m_exportCtrl->TimeRangeChanged += gcnew EventHandler<TimeRangeArgs^>(this, &BasePatternsUserControl::OnTimeRangeChanged);

			m_exportCtrl->SetPluginURL(url);
		}

		CString urlStr = String::IsNullOrEmpty(url)? "" : url;
		CString patientIDStr = String::IsNullOrEmpty(patientID)? "" : patientID;
		CString userIDStr = String::IsNullOrEmpty(userID)? "" : userID;
		CString usernameStr = String::IsNullOrEmpty(username)? "" : username;
		CString permissionsStr = String::IsNullOrEmpty(permissions)? "" : permissions;
		CString versionStr = String::IsNullOrEmpty(patternsVersion) ? "" : patternsVersion;
		if(m_pCtrl != NULL)
		{
			m_pCtrl->SetInitialData(urlStr, patientIDStr, userIDStr, usernameStr, permissionsStr, exportSupport, versionStr);
		}
	}

	int BasePatternsUserControl::RoundIntValueToTheNearestGivenNumber(int value, int roundByNumber)
	{
		int roundedValue = value;
		double roundBy = roundByNumber;
		if (roundByNumber != 1 && value >= 0)
		{			
			roundedValue = (int)(Math::Round(value / roundBy, MidpointRounding::AwayFromZero) * roundBy);
		}
		return roundedValue;
	}

	void BasePatternsUserControl::RaiseIntervalPressedEvent(IntervalEventArgs^ args)
	{
		ExportEntriesCalcStruct entries;
		DATE from = args->StartTime.ToOADate();
		DATE to = args->EndTime.ToOADate();
		IntervalDataArgs^ data = gcnew IntervalDataArgs();

		if (m_pCtrl->GetExportCalculatedEntries(from, to, args->IntervalID, entries))
		{
			data->ContractionCount30minThresholdExceeded = entries.m_contractionThresholdExceeded;
			data->MeanContractionInterval = Math::Round(entries.m_meanContructions, 1);
			data->BaselineFHR = entries.m_meanBaseline;
			data->BaselineVariability = entries.m_meanBaselineVariability.ToString();

			if (args->MontevideoUnits.HasValue == true)
			{
				data->MontevideoUnits = (int)Math::Round(entries.m_montevideo);
			}
		}
		else
		{
			data->ContractionCount30minThresholdExceeded = m_pCtrl->GetContractionThresholdExceeded(from, to);
			data->MeanContractionInterval = args->MeanContractionInterval;
			data->BaselineFHR = args->BaselineFHR;
			data->BaselineVariability = args->BaselineVariability;

			if (args->MontevideoUnits.HasValue == true)
				data->MontevideoUnits = args->MontevideoUnits;
		}
		
		data->OriginalBaselineFHR = data->BaselineFHR;		
		data->BaselineFHR = RoundIntValueToTheNearestGivenNumber(data->OriginalBaselineFHR, m_roundByBaselineFHRValue);

		data->OriginalMontevideoUnits = data->MontevideoUnits;
		if (data->MontevideoUnits.HasValue == true)
		{
			int montevideoUnits = data->MontevideoUnits.Value;
			data->MontevideoUnits = RoundIntValueToTheNearestGivenNumber(montevideoUnits, m_roundByMontevideoUnits);
		}

		data->VisitKey = GetVisitKey();
		data->StartTime = args->StartTime;
		data->EndTime = args->EndTime;
		data->IntervalID = args->IntervalID;
		data->IntervalDuration = args->IntervalDuration;
		data->ContractionIntervalRange = args->ContractionIntervalRange;
		data->ContractionCount10min = args->ContractionCount10min;
		data->LongContractionCount = args->LongContractionCount;

		data->ContractionDurationRange = args->ContractionDurationRange;
		data->OriginalContractionDurationRange = args->OriginalContractionDurationRange;
		if (args->MontevideoUnits.HasValue == true)
		{
			data->ContractionIntensityRange = args->ContractionIntensityRange;
			data->OriginalContractionIntensityRange = args->OriginalContractionIntensityRange;
		}
		else
		{
			data->ContractionIntensityRange = String::Empty;
			data->OriginalContractionIntensityRange = String::Empty;
		}

		
		data->Accels = args->Accels;

		data->Decels->Early = args->Decels->Early;
		data->Decels->Variable = args->Decels->Variable;
		data->Decels->Late = args->Decels->Late;
		data->Decels->None = args->Decels->None;
		data->Decels->Prolonged = args->Decels->Prolonged;
		data->Decels->Other = args->Decels->Other;

		OnIntervalPressedEvent(data);
	}



	IntervalDataArgs^ BasePatternsUserControl::GetIntervalData(DateTime startTime, DateTime endTime)
	{
		if (m_exportCtrl != nullptr)
		{
			IntervalEventArgs^ args = m_exportCtrl->GetIntervalData(startTime, endTime);
			IntervalDataArgs^ data = gcnew IntervalDataArgs();

			data->VisitKey = GetVisitKey();
			data->StartTime = args->StartTime;
			data->EndTime = args->EndTime;
			data->IntervalID = args->IntervalID;
			data->IntervalDuration = args->IntervalDuration;
			data->ContractionIntervalRange = args->ContractionIntervalRange;
			data->ContractionCount10min = args->ContractionCount10min;
			ExportEntriesCalcStruct entries;
			DATE from = args->StartTime.ToOADate();
			DATE to = args->EndTime.ToOADate();
			if (m_pCtrl->GetExportCalculatedEntries(from, to, args->IntervalID, entries))
			{
				data->ContractionCount30minThresholdExceeded = entries.m_contractionThresholdExceeded;
				data->MeanContractionInterval = Math::Round(entries.m_meanContructions, 1);
				data->BaselineFHR = entries.m_meanBaseline;
				data->BaselineVariability = entries.m_meanBaselineVariability.ToString();

				if (args->MontevideoUnits.HasValue == true)
				{
					data->MontevideoUnits = (int)Math::Round(entries.m_montevideo);
				}
			}
			else
			{
				data->ContractionCount30minThresholdExceeded = m_pCtrl->GetContractionThresholdExceeded(from, to);
				data->MeanContractionInterval = args->MeanContractionInterval;
				if (args->MontevideoUnits.HasValue == true)
					data->MontevideoUnits = args->MontevideoUnits;
				data->BaselineFHR = args->BaselineFHR;
				data->BaselineVariability = args->BaselineVariability;
			}
			
			data->LongContractionCount = args->LongContractionCount;

			data->OriginalBaselineFHR = data->BaselineFHR;
			data->BaselineFHR = RoundIntValueToTheNearestGivenNumber(data->OriginalBaselineFHR, m_roundByBaselineFHRValue);

			data->OriginalMontevideoUnits = data->MontevideoUnits;
			if (data->MontevideoUnits.HasValue == true)
			{
				int montevideoUnits = data->MontevideoUnits.Value;
				data->MontevideoUnits = RoundIntValueToTheNearestGivenNumber(montevideoUnits, m_roundByMontevideoUnits);
			}
			data->Accels = args->Accels;

			data->Decels->Early = args->Decels->Early;
			data->Decels->Variable = args->Decels->Variable;
			data->Decels->Late = args->Decels->Late;
			data->Decels->None = args->Decels->None;
			data->Decels->Prolonged = args->Decels->Prolonged;
			data->Decels->Other = args->Decels->Other;

			data->ContractionDurationRange = args->ContractionDurationRange;
			data->OriginalContractionDurationRange = args->OriginalContractionDurationRange;
			if (data->MontevideoUnits.HasValue == true)
			{
				data->ContractionIntensityRange = args->ContractionIntensityRange;
				data->OriginalContractionIntensityRange = args->OriginalContractionIntensityRange;
			}
			else
			{
				data->ContractionIntensityRange = String::Empty;
				data->OriginalContractionIntensityRange = String::Empty;

			}


			return data;
		}
		return nullptr;
	}

	void BasePatternsUserControl::SetTimeRange(int timeRange)
	{
		if (m_exportCtrl != nullptr)
		{
			m_exportCtrl->SetTimeRange(timeRange);

			if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
			{
				::SendMessageA(m_pCtrl->m_hWnd, TIME_RANGE_CHANGE_CALLBAK_MSG, 0, (LPARAM)timeRange);
			}
		}
	}

	void BasePatternsUserControl::WndProc(Message% m)
	{
		switch(m.Msg)
		{
		case MSG_MVU_STATE_CHANGED:
			{
				bool mvuIsOn = (0 != m.WParam.ToInt32());
				MVUStateChangedArgs^ args = gcnew MVUStateChangedArgs();
				args->MVUIsOn = mvuIsOn;
				RaiseMVUStateChangedEvent(args);
			}
			break;
		case MSG_VIEW_ZOOMED:
			{
				bool zoomOut =  (0 != m.WParam.ToInt32());
				ZoomViewArgs^ args = gcnew ZoomViewArgs();
				args->ZoomOut = zoomOut;
				RaiseViewZoomedEvent(args);
			}
			break;
		case MSG_SWITCH_TOGGLED_MSG:
			{
				bool bLeftPartShown = (0 != m.WParam.ToInt32());
				ToggleSwitchArgs^ args = gcnew ToggleSwitchArgs();
				args->m_newPosition = bLeftPartShown ? TogglePosition::Left : TogglePosition::Right;
				RaiseSwitchToggledEvent(args);
			}
		break;
		case MSG_END_INITIALIZATION:
			{
				bool bSucceed = (0 != m.WParam.ToInt32());
				EndInitializationArgs^ args = gcnew EndInitializationArgs();
				args->Succeded = bSucceed;
				RaiseControlInitializationEndEvent(args);
				m_IsInitialized = bSucceed;
			}
			break;
		case MSG_EXPORT_SET_START_TIME:
			{
				if (m_exportCtrl != nullptr)
				{
					double* sTime = (double*)(m.LParam.ToPointer());

					m_exportCtrl->SetStartTime(System::DateTime::FromOADate(*sTime));
				}
			}
			break;
		case MSG_EXPORT_BEGIN_UPDATE:
			if (m_exportCtrl != nullptr)
				m_exportCtrl->BeginUpdateChunks();
			break;
		case MSG_EXPORT_END_UPDATE:
			if (m_exportCtrl != nullptr)
				m_exportCtrl->EndUpdateChunks();
			break;
		case MSG_EXPORT_ADD_CHUNK:
			{
				if (m_exportCtrl != nullptr)
				{
					ExportChunkStruct* chunk = (ExportChunkStruct*)(m.LParam.ToPointer());
					m_exportCtrl->AddChunkEx(chunk->m_exportID, chunk->m_intervalID, System::DateTime::FromOADate(chunk->m_startTime), chunk->m_timeRange, chunk->m_isExported, chunk->m_x1, chunk->m_x2);
				}
			}
			break;
		case MSG_EXPORT_SET_RANGE:
			{
				if (m_exportCtrl != nullptr)
				{
					int* timeRange = (int*)(m.LParam.ToPointer());
					m_exportCtrl->SetTimeRange(*timeRange);
				}
			}
			break;
		case MSG_EXPORT_SET_TOOLTIP:
			{
				if (m_exportCtrl != nullptr)
				{
			
					PanelToolTip* tooltip = (PanelToolTip*)(m.LParam.ToPointer());
					String^ messageStr = gcnew String(tooltip->m_message);
					m_exportCtrl->SetPanelTooltip(messageStr, System::DateTime::FromOADate(tooltip->m_end36Weeks));
				}
			}
			break;
		case MSG_EXPORT_SET_MONTEVIDEO:
			{
				bool visible = ((int)m.WParam != 0);
				if (m_exportCtrl != nullptr)
				{					
					m_exportCtrl->SetMontevideoVisible(visible);					
				}
				bool raiseEvent = ((int)m.LParam != 0);
				if (raiseEvent)
				{
					MVUStateChangedArgs^ args = gcnew MVUStateChangedArgs();
					args->MVUIsOn = visible;
					RaiseMVUStateChangedEvent(args);
				}
			}
			break;
		case MSG_EXPORT_HIDE_CONTROLS:
			{
				if (m_exportCtrl != nullptr)
				{
					bool hide = ((int)m.WParam.ToInt32() != 0);
					if (hide)
					{
						m_exportCtrl->HideControls();
					}
					else
					{
						bool valid = ((int)m.LParam.ToInt32() != 0);
						ResizeExportBar(valid);
					}
				}
			};
			break;
		case MSG_EXPORT_SET_INITPARAMS:
		{
			if (m_exportCtrl != nullptr)
			{
				WPFAdapterInitParams* initParams = (WPFAdapterInitParams*)(m.LParam.ToPointer());
				String^ userID = gcnew String(initParams->m_userID);
				IntPtr hWnd(initParams->m_criWnd);
				m_exportCtrl->SetInitParams(userID, initParams->m_episodeID, initParams->m_canModify, hWnd);
			}
		}
			break;
		case MSG_EXPORT_SET_ROUND_VAL:
		{
			if (m_exportCtrl != nullptr)
			{
				RoundExportValueStruct* roundval = (RoundExportValueStruct*)(m.LParam.ToPointer());				
				m_exportCtrl->SetRoundExportValueParams(roundval->m_roundBaselineFHRValue);
				m_roundByBaselineFHRValue = roundval->m_roundBaselineFHRValue;
				m_roundByMontevideoUnits = roundval->m_roundByMontevideoUnits;
			}

		}
		break;
		case MSG_ABOUTBTN_CLICKED:
		{				
				
			OpenAboutBox();
		}
		break;
		}
		System::Windows::Forms::UserControl::WndProc(m);
	}

	void BasePatternsUserControl::RaiseControlInitializationEndEvent(EndInitializationArgs^ args)
	{
		EndInitializationArgs^ data = gcnew EndInitializationArgs();
		data->Succeded = args->Succeded;
		OnControlInitializationEndEvent(data);
	}


	void BasePatternsUserControl::GetExportCalculatedEntries(DateTime fromTime, DateTime toTime, int intervalID)
	{
		if(m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd) && m_exportCtrl != nullptr)
		{
			ExportEntriesCalcStruct entries;
			DATE from = fromTime.ToOADate();
			DATE to = toTime.ToOADate();
			if(m_pCtrl->GetExportCalculatedEntries(from, to, intervalID, entries))
			{
				m_exportCtrl->SetExportDialogParamsEx(entries.m_meanContructions,
															entries.m_meanBaseline,
															entries.m_meanBaselineVariability,
															entries.m_montevideo);
			}
		}
	}


	void BasePatternsUserControl::ToggleSwitch(bool right)
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			m_pCtrl->ToggleSwitch(right, true);
		}
	}

	void BasePatternsUserControl::OnSwitchToggledEvent(ToggleSwitchArgs^ args)
	{
		ToggleSwitch(args->m_newPosition == TogglePosition::Right);
	}

	TogglePosition BasePatternsUserControl::GetCurrentTogglePosition()
	{
		
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			return m_pCtrl->IsLeftPartShown() ? TogglePosition::Left : TogglePosition::Right;
		}
		return TogglePosition::Right;
	}

	void BasePatternsUserControl::RaiseSwitchToggledEvent(ToggleSwitchArgs^ args)
	{	
		SwitchToggled(args);
	}

	void BasePatternsUserControl::ZoomView(bool zoomOut)
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			m_pCtrl->SwitchViews(!zoomOut, true);
		}
	}

	void BasePatternsUserControl::RaiseViewZoomedEvent(ZoomViewArgs^ args)
	{
		ViewZoomed(args);
	}

	void BasePatternsUserControl::OnViewZoomedEvent(ZoomViewArgs^ args)
	{
		ZoomView(args->ZoomOut);
	}
	bool BasePatternsUserControl::IsViewZoomed()
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			return !m_pCtrl->Is30MinView();
		}
		return true;
	}

	void BasePatternsUserControl::RaiseMVUStateChangedEvent(MVUStateChangedArgs^ args)
	{
		MVUStateChanged(args);
	}

	void BasePatternsUserControl::OnMVUStateChangedEvent(MVUStateChangedArgs^ args)
	{
		SetMVUState(args->MVUIsOn);
	}

	void BasePatternsUserControl::SetMVUState(bool on)
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			m_pCtrl->ShowMVU(on, true, false);
		}
	}

	void BasePatternsUserControl::OpenAboutBox()
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			PeriGen::Patterns::WPFLibrary::LibraryHelper::Instance->InitLogger(System::Diagnostics::SourceLevels::Information,
				System::Diagnostics::SourceLevels::Information,
				System::Diagnostics::SourceLevels::Information,
				"LMSLogs");
			ProductInformationStruct info;
			m_pCtrl->GetProductInformation(info);
			String^ checklistURL = String::Format("{0}/CheckListPlugin", m_pluginURL);
			String^ url = m_checklistEnabled ?
				checklistURL : gcnew String("");
			String^ udi = (info.m_logo == 0) ? gcnew String(PATTERNS_UDI) : gcnew String(CUES_UDI);
			PeriGen::Patterns::WPFLibrary::LibraryHelper::Instance->ShowAboutWindow(url, m_checklistEnabled, info.m_logo, udi, false);
		}

	}

	bool BasePatternsUserControl::GetTracingsViewContext(TracingsViewContext^ context)
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			CtrlContextStruct currentContextNative;
			
			if (m_pCtrl->SendMessage(MSG_GET_STRIP_CURRENT_STATUS, 0, (LPARAM)&currentContextNative))
			{
				context->Events = currentContextNative.m_events;
				context->TOCO = currentContextNative.m_toco;
				context->Baselines = currentContextNative.m_baselines;
				context->MVU = currentContextNative.m_MVU;
				context->Is15MinView = currentContextNative.m_is15MinView;
				context->LeftHalfShown = currentContextNative.m_leftHalfShown;
				context->Zoom = currentContextNative.m_zoom;
				context->CompressedStartTime = currentContextNative.m_bScrollToEnd ? 
					DateTime::MaxValue : DateTime::FromOADate(currentContextNative.m_compressedStartTime);
				context->ExpandedEndTime = DateTime::FromOADate(currentContextNative.m_expandedEndTime);
				return true;
			}
		}
		return false;
	}

	void BasePatternsUserControl::SetTracingsViewContext(TracingsViewContext^ context)
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			
			CtrlContextStruct contextNative;
			contextNative.m_baselines = context->Baselines;
			contextNative.m_events = context->Events;
			contextNative.m_toco = context->TOCO;
			contextNative.m_MVU = context->MVU;
			contextNative.m_zoom = context->Zoom;
			contextNative.m_compressedStartTime = context->CompressedStartTime->ToOADate();
			contextNative.m_expandedEndTime = context->ExpandedEndTime->ToOADate();
			contextNative.m_bScrollToEnd = context->CompressedStartTime->Equals(DateTime::MaxValue) ? true : false;
			contextNative.m_is15MinView = context->Is15MinView;
			contextNative.m_leftHalfShown = context->LeftHalfShown;	
			m_pCtrl->SendMessage(MSG_SET_STRIP_STATUS, 0, (LPARAM)&contextNative);

			if (!contextNative.m_is15MinView && !contextNative.m_bScrollToEnd)
			{
				DATE* newTime = new DATE(contextNative.m_expandedEndTime);
				m_pCtrl->PostMessage(MSG_GOTO_TIME, (WPARAM)newTime, 0);
			}
		}
	}

	void BasePatternsUserControl::SwitchToLeft15Min()
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			m_pCtrl->SwitchToLeft15Min();
		}
	}
	void BasePatternsUserControl::SwitchToRight15Min()
	{
		if (m_pCtrl != NULL && IsWindow(m_pCtrl->m_hWnd))
		{
			m_pCtrl->SwitchToRight15Min();
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
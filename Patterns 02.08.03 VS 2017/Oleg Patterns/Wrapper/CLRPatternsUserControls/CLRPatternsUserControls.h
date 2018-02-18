// CLRPatternsUserControls.h

#pragma once

using namespace System;
using namespace System::Drawing;
using namespace System::Windows::Forms;
using namespace System::Runtime::Remoting::Messaging;

//#using "PeriGen.Patterns.NnetControls.dll"
using namespace Perigen::Patterns::NnetControls;


class ContainerBaseWnd;


namespace CLRPatternsUserControls {

	public ref class TracingsViewContext
	{
	public:
		TracingsViewContext()
		{
			MVU = false;
			Baselines = false;
			Events = false;
			TOCO = false;
			Zoom = false;
			Is15MinView = false;
			LeftHalfShown = false;
			CompressedStartTime = DateTime::MinValue;
			ExpandedEndTime = DateTime::MinValue;;
		}
		bool MVU;
		bool Baselines;
		bool Events;
		bool TOCO;
		bool Zoom;
		bool Is15MinView;
		bool LeftHalfShown;
		DateTime^ CompressedStartTime;
		DateTime^ ExpandedEndTime;
	};
	
	public ref struct _Decels
	{
		bool Early;
		bool Variable;
		bool Late;
		bool None;
		bool Prolonged;
		bool Other;
	};

	public ref class IntervalTimeRangeArgs
	{
	public:
		int IntervalDuration;

		IntervalTimeRangeArgs()
		{
			IntervalDuration = 0;
		}
	};

	public ref class IntervalDataArgs
	{
	public:

		//•	ExportHeader
		String^ VisitKey;
		int IntervalID;
		DateTime StartTime;
		DateTime EndTime;
		int IntervalDuration;

		//•	MaternalData
		String^ ContractionIntervalRange;
		double MeanContractionInterval;
		int ContractionCount10min;
		int LongContractionCount;
		System::Nullable<int> MontevideoUnits;
		System::Nullable<int> OriginalMontevideoUnits;
		String^ ContractionDurationRange;
		String^ OriginalContractionDurationRange;
		String^ ContractionIntensityRange;
		String^ OriginalContractionIntensityRange;
		bool ContractionCount30minThresholdExceeded;

		//•	FetalData
		int BaselineFHR;
		int OriginalBaselineFHR;
		String^ BaselineVariability;
		bool Accels;
		_Decels^ Decels;

		IntervalDataArgs()
		{
			Decels = gcnew _Decels();
			ContractionIntervalRange = String::Empty; 
			BaselineVariability = String::Empty;
			ContractionIntensityRange = String::Empty;
			OriginalContractionIntensityRange = String::Empty;
			OriginalContractionDurationRange = String::Empty;
			ContractionDurationRange = String::Empty;
		}
	};

	public ref class EndInitializationArgs
	{
	public:
		bool Succeded;

		EndInitializationArgs()
		{

		}
	};

	public enum class TogglePosition { Left, Right };
	public ref class ToggleSwitchArgs
	{
	public:
		TogglePosition m_newPosition;
		ToggleSwitchArgs()
		{
			m_newPosition = TogglePosition::Right;
		}
	};

	public ref class ZoomViewArgs
	{
	public:
		bool ZoomOut;
		ZoomViewArgs()
		{
			ZoomOut = false;
		}
	};

	public ref class MVUStateChangedArgs
	{
	public:
		bool MVUIsOn;
		MVUStateChangedArgs()
		{
			MVUIsOn = false;
		}
	};

	public ref class BasePatternsUserControl : public  System::Windows::Forms::UserControl
	{
	public:
		BasePatternsUserControl(bool checklistEnabled);
		!BasePatternsUserControl()
		{
			if (m_pCtrl != NULL)
				delete m_pCtrl;
			m_pCtrl = NULL;
		}
		~BasePatternsUserControl()
		{

			if (components)
			{		
				delete components;			
			}

			this->!BasePatternsUserControl();
		}

		virtual void OnHandleCreated(EventArgs^ e) override;
		virtual void OnResize(EventArgs^ e) override;

		void SetInitialData(String^ url, String^ patientID, String^ userID, String^ username, String^ permission, bool exportSupport, String^ patternsVersion);

		property bool IsExternalUsing
		{
			bool get() 
			{  
				return m_exportCtrl->IsExternalUsing;  
			} 
			void set(bool value)
			{
				m_exportCtrl->IsExternalUsing = value;
			}
		}

		delegate void IntervalPressedEventHandler(IntervalDataArgs^ args); 
		event IntervalPressedEventHandler^ OnIntervalPressedEvent;
		void RaiseIntervalPressedEvent(IntervalEventArgs^ args);
		void SetIntervalExported(DateTime fromTime, DateTime toTime);
		void SetTimeRange(int timeRange);
		void SetSelectedInterval(DateTime startTime);


		void OnIntervalPressed(Object^ sender, IntervalEventArgs^ args);
		void OnIntervalMouseOver(Object^ sender, IntervalEventArgs^ args);
		void OnIntervalMouseLeave(Object^ sender, IntervalEventArgs^ args);
		void OnExportDialogClosed(Object^ sender, IntervalEventArgs^ args);
		void OnTimeRangeChanged(Object^ sender, TimeRangeArgs^ args);

		void PerformExport(DateTime fromTime, DateTime toTime);
		void CancelExport(DateTime fromTime, DateTime toTime);
		
	protected:
		void DoOnEndExport(DateTime fromTime, DateTime toTime, bool isExported);
	public:
		void GetExportCalculatedEntries(DateTime fromTime, DateTime toTime, int intervalID);
		IntervalDataArgs^ GetIntervalData(DateTime startTime, DateTime endTime);
	protected:
		void ResizeExportBar(bool valid);
		virtual void WndProc(Message% m) override; 		
	public:
		delegate void ControlInitializationEndHandler(EndInitializationArgs^ args);
		event ControlInitializationEndHandler^ OnControlInitializationEndEvent;
		void RaiseControlInitializationEndEvent(EndInitializationArgs^ args);

		delegate void IntervalTimeRangeHandler(IntervalTimeRangeArgs^ args);
		event IntervalTimeRangeHandler^ OnIntervalTimeRangeChangedEvent;
		void RaiseIntervalTimeRangeChangedEvent(int intervalDuration);

	protected:
		void OpenAboutBox();
	public:
		void ToggleSwitch(bool right);

		delegate void SwitchToggledHandler(ToggleSwitchArgs^ args);
		event SwitchToggledHandler^ SwitchToggled;
		void RaiseSwitchToggledEvent(ToggleSwitchArgs^ args);

		void OnSwitchToggledEvent(ToggleSwitchArgs^ args);
		TogglePosition GetCurrentTogglePosition();
		String^ GetPluginURL() {return m_pluginURL;	}
		String^ GetVisitKey() { return m_visitKey; }
		String^ GetUserID() { return m_userID; }

		void ZoomView(bool in);
		delegate void ZoomViewHandler(ZoomViewArgs^ args);
		event ZoomViewHandler^ ViewZoomed;
		void RaiseViewZoomedEvent(ZoomViewArgs^ args);

		void OnViewZoomedEvent(ZoomViewArgs^ args);
		bool IsViewZoomed();

		void SetMVUState(bool on);
		delegate void MVUStateChangedHandler(MVUStateChangedArgs^ args);
		event MVUStateChangedHandler^ MVUStateChanged;
		void RaiseMVUStateChangedEvent(MVUStateChangedArgs^ args);

		void OnMVUStateChangedEvent(MVUStateChangedArgs^ args);

		void SwitchToLeft15Min();
		void SwitchToRight15Min();

	protected:
		int RoundIntValueToTheNearestGivenNumber(int value, int roundByNumber);
	public:
		bool GetTracingsViewContext(TracingsViewContext^ context);
		void SetTracingsViewContext(TracingsViewContext^ context);
		
		bool IsControlInitialized()
		{
			return m_IsInitialized;
		}
		bool m_IsInitialized;
	protected:
		System::ComponentModel::Container ^components;
		ContainerBaseWnd* m_pCtrl;
		PatternsWPFAdapter^ m_exportCtrl;
		String^ m_pluginURL;
		String^ m_visitKey;
		String^ m_userID;

		bool m_checklistEnabled;

		int m_roundByBaselineFHRValue;
		int m_roundByMontevideoUnits;
	private: 
		System::Void InitializeComponent() {
			components = gcnew System::ComponentModel::Container();
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
	
	}

	};

	public ref class CLRPatternsControl : public BasePatternsUserControl
	{
	public:
		CLRPatternsControl();
	
	};

	public ref class ChecklistControl : public BasePatternsUserControl
	{
	public:
		ChecklistControl();

	};
}

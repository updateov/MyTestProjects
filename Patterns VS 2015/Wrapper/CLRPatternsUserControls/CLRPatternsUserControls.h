// CLRPatternsUserControls.h

#pragma once

using namespace System;
using namespace System::Drawing;
using namespace System::Windows::Forms;
using namespace System::Runtime::Remoting::Messaging;


class ContainerBaseWnd;


namespace CLRPatternsUserControls {

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

		void SetInitialData(String^ url, String^ patientID);



	protected:
		System::ComponentModel::Container ^components;
		ContainerBaseWnd* m_pCtrl;
		
	private: 
		System::Void InitializeComponent() {
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
		// TODO: Add your methods for this class here.
	};
}

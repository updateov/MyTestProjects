#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

//#include "..\CustomControls\MyControl.h"

namespace CustomControlWrapper
{

	/// <summary>
	/// Summary for MyControlBridge
	/// </summary>
	public ref class MyControlBridge : public Control
	{
	public:
		MyControlBridge(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			//m_pCtrl = new MyControl();
		}

	protected:
		//!MyControlBridge()
		//{
		//	if (m_pCtrl != nullptr)
		//	{
		//		//delete m_pCtrl;
		//		m_pCtrl = nullptr;
		//	}
		//}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~MyControlBridge()
		{
			if (components)
			{
				delete components;
			}

			//this->!MyControlBridge();
		}

		//virtual void OnHandleCreated(EventArgs^ e) override
		//{
		//		System::Diagnostics::Debug::Assert(m_pCtrl->GetSafeHwnd() == nullptr);
		//		m_pCtrl->SubclassWindow((HWND)Handle.ToPointer());
		//		Control::OnHandleCreated(e);
		//}

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;
		//MyControl* m_pCtrl;


#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
		}
#pragma endregion
	};
}

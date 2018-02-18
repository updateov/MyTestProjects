#pragma once

// popup_message form view
#include <string>

using namespace std;

/*
 =======================================================================================================================
    popup message class. This small class display a given text into the surface of the window... also fills the
    background in white.
 =======================================================================================================================
 */
class popup_message :
	public CWnd
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		string m_text;
		COLORREF m_color;

		UINT m_format;
		virtual afx_msg void OnPaint(void);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		popup_message(void);
		virtual ~popup_message(void);

		virtual string get_text(void);
		virtual void set_text(string);

		UINT get_format(void) {return m_format;}
		void set_format(UINT format) {m_format = format;}

		COLORREF get_color(COLORREF color) {return m_color;}
		void set_color(COLORREF color) {m_color = color;}

		DECLARE_MESSAGE_MAP()
};

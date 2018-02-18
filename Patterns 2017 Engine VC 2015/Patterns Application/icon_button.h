#ifndef _ICONBUTTON_
#define _ICONBUTTON_

/*
 =======================================================================================================================
    icon_button class. This recreate the behavior of the button and display an image instead with different states (as
    a regular button.
 =======================================================================================================================
 */
class icon_button :
	public CWnd
{
		DECLARE_DYNAMIC(icon_button)
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	private:
		enum state { qnormal, qdownin, qdownout, qover };

		string m_icon;
		string m_mask;
		state m_state;

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		virtual void draw_text(CDC *, long, long);
		virtual string get_icon(void);
		virtual string get_mask(void);
		virtual state get_state(void);
		virtual CString get_text(void);
		virtual afx_msg BOOL OnEraseBkgnd(CDC *);
		virtual afx_msg void OnLButtonDown(UINT, CPoint);
		virtual afx_msg void OnLButtonUp(UINT, CPoint);
		virtual afx_msg void OnMouseMove(UINT, CPoint);
		virtual void OnPaint(void);
		virtual BOOL PreCreateWindow(CREATESTRUCT &);
		virtual void set_state(state);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		icon_button(void);
		virtual ~icon_button(void);

		virtual bool has_icon(void);
		virtual bool has_text(void);
		virtual void set_icon(string);
		virtual void set_mask(string);
		virtual int optimal_width(void);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		DECLARE_MESSAGE_MAP()
};
#endif

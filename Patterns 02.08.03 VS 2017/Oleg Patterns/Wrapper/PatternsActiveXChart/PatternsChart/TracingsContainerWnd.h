#pragma once


// TracingsContainerWnd

class TracingsContainerWnd : public CWnd
{
	DECLARE_DYNAMIC(TracingsContainerWnd)

public:
	TracingsContainerWnd();
	virtual ~TracingsContainerWnd();

protected:
	DECLARE_MESSAGE_MAP()
	//afx_msg void OnPaint();
	BOOL OnWndMsg(UINT message, WPARAM wParam, LPARAM lParam, LRESULT* pResult);
};


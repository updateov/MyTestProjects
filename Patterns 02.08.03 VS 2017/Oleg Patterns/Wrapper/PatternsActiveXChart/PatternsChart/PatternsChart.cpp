#include "stdafx.h"
#include "PatternsChart.h"
//////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CEmptyBarWnd, CWnd)
	ON_WM_PAINT()
END_MESSAGE_MAP()

void CEmptyBarWnd::OnPaint()
{
	CPaintDC dc(this);
	CRect clientRect;
	GetClientRect(clientRect);
	dc.FillRect(clientRect, CBrush::FromHandle((HBRUSH)GetStockObject(WHITE_BRUSH)));
}


////////////////////////////////////////////////////////////////////
// Convert a UTF-8 encoded string to a wstring
std::wstring utf8_to_wstr(const std::string &utf8)
{
	if (utf8 == "")
	{
		return L"";
	}

	std::vector<wchar_t> wbuff(utf8.length() + 1);
	if (!MultiByteToWideChar(CP_UTF8, 0,  utf8.c_str(), utf8.length(), &wbuff[0], utf8.length() + 1))
	{
		DWORD e = ::GetLastError();
		switch(e)
		{
			case ERROR_INSUFFICIENT_BUFFER:
			case ERROR_INVALID_FLAGS:
			case ERROR_INVALID_PARAMETER:
			case ERROR_NO_UNICODE_TRANSLATION:
				return L"Error";
			default:
				break;
		}
	}

	return &wbuff[0];
}
// Convert a wstring to a UTF-8 encoded string
std::string wstr_to_str( const std::wstring& wstr )
{
	if(wstr == L"")
		return "";

	std::vector<char> buff(wstr.length() + 1);

	size_t size = wstr.length();
	WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), size, &buff[0], size+1, NULL, NULL );
	return &buff[0];
}

// Due to the way data is retrieved from an XML send by a C# application, the accent characters (and all 'special' characters and not
// properly encoded in ANSI, doing a UTF8 to wstring then back to ansi fixes that
string& cleanString(string& str)
{
	wstring ws = utf8_to_wstr(str);
	str = wstr_to_str(ws);
	return str;
}
/////////////////////////////////////////////////////////////////
double patterns_ratio_coefficient(double tracingsViewSizeInMinutes, double compressedViewSizeInMinutes)
{
	return	((210 / 90.) / tracingsViewSizeInMinutes)	// HR 15 minutes
		+ ((100 / 75.) / tracingsViewSizeInMinutes)		// UP 15 minutes
		+ ((210 / 90.) / compressedViewSizeInMinutes)	// HR 4 hours
		+ ((100 / 75.) / compressedViewSizeInMinutes)	// UP 4 hours
		+ ((200 / 100.) / compressedViewSizeInMinutes)	// CR 4 hours
		+ ((250 / 100.) / compressedViewSizeInMinutes);	// Proportional blank inter space 
}
////////////////////////
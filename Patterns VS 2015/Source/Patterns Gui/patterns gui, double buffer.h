#pragma once

namespace patterns_gui
{
	class double_buffer : public CDC 
	{
	public:
		// Create the double buffered CDC
		double_buffer(CDC* pDC, const CRect* pRect) : CDC()
		{
			ASSERT(pDC != NULL); 
			ASSERT(!pDC->IsPrinting());

			m_pDC = pDC;
			m_pOldBitmap = NULL;
			m_rcBounds = *pRect;

			CreateCompatibleDC(pDC);
			pDC->LPtoDP(&m_rcBounds);
			m_MemoryBitmap.CreateCompatibleBitmap(pDC, m_rcBounds.Width(), m_rcBounds.Height());
			m_pOldBitmap = SelectObject(&m_MemoryBitmap);

			SetMapMode(pDC->GetMapMode());
			SetWindowExt(pDC->GetWindowExt());
			SetViewportExt(pDC->GetViewportExt());
			pDC->DPtoLP(&m_rcBounds);
			SetWindowOrg(m_rcBounds.left, m_rcBounds.top);
		}

		// Commit the changes
		~double_buffer()	
		{		
			m_pDC->BitBlt(m_rcBounds.left, m_rcBounds.top, m_rcBounds.Width(), m_rcBounds.Height(), this, m_rcBounds.left, m_rcBounds.top, SRCCOPY);			
			SelectObject(m_pOldBitmap);		
		}

		double_buffer* operator->() {return this;}	
		operator double_buffer*() {return this;}

	protected:	
		CBitmap m_MemoryBitmap;
		CBitmap* m_pOldBitmap;
		CDC* m_pDC;
		CRect m_rcBounds;
	};
}
#pragma once

#include "patterns, fetus.h"

namespace patterns_gui
{
	using namespace std;
	using namespace patterns;

	/*
	=======================================================================================================================
	General-purpose services class. This includes methods used by other classes in the patterns_gui component and-or
	meant to be commonly used by clients of the patterns and patterns_gui components. For instance, methods for drawing
	transparent bitmaps are used be the tracing and patient_list classes, while methods for managing dates are meant to
	be used by clients of the fetus class. Bitmap methods use a cache for storing bitmaps loaded from resources. This
	is more efficient than loading bitmaps multiple times and relieves client classes from implementing caching on
	their end. See protected method get_bitmap () for implementation details. See also method keep_bitmap () for
	storing bitmaps. This is especially useful for components that need to compose bitmaps and then use them as is they
	were taken from resources. See method tracing::create_rectangle_bitmaps (). bloaded -> get_bitmap ().
	=======================================================================================================================
	*/
	class services
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum crop_bitmap { cbottomleft, cbottomright, ccenter, ctopleft, ctopright };
		enum time_format { fnormal, ftime, fminutes, fdate, fhours };

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		static map<string, CBitmap *> bloaded;

		static void draw_bitmap_tile(CDC *, CBitmap *, long, long, long, long, crop_bitmap, bool, DWORD);
		static void draw_bitmap_within(CDC *, CDC *, long, long, long, long, long, long, long, long, DWORD);
		static CBitmap *get_bitmap(const string &);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		static void create_bitmap_fragment(const string &, const string &, const CRect &r);
		static string date_to_string(const date &, time_format = fnormal);
		static long date_to_time_of_day(const date &);
		static void draw_bitmap(CDC *, const string &, const string &, long, long, long = LONG_MIN, long = LONG_MIN, crop_bitmap = ctopleft, bool = false);
		static string event_type_to_string(event::type);
		static void forget_bitmap(const string &);
		static void forget_all_bitmaps();
		static CRect get_bitmap_rectangle(const CBitmap &);
		static CRect get_bitmap_rectangle(const string &);
		static bool is_bitmap(const string &);
		static void keep_bitmap(const string &, CBitmap *);
		static COLORREF make_colour(COLORREF, long);
		static string span_to_string(long, time_format = fnormal);

		static void reset();

		static void select_font(int nPointSize, LPCTSTR lpszFaceName, CDC* pDC);
	};


	class ThreadLock
	{
	public:
		ThreadLock( ) :
		  m_pCriticalSection( new CRITICAL_SECTION )
		  {
			  InitializeCriticalSection( m_pCriticalSection );	
		  }

		  ~ThreadLock( )
		  { 
			  DeleteCriticalSection( m_pCriticalSection );
			  delete m_pCriticalSection;
		  }

		  void acquire( ) 
		  { 
			  EnterCriticalSection( m_pCriticalSection );
		  }

		  void release( )
		  { 
			  LeaveCriticalSection( m_pCriticalSection );
		  }

	protected:

		CRITICAL_SECTION* m_pCriticalSection;

	};

	template< class T >
	class ScopeLock
	{
	public:
		ScopeLock( T* lock ) :
		  m_Lock( lock )
		  {
			  m_Lock->acquire( );
		  }
		  ~ScopeLock( )
		  {
			  m_Lock->release( );
		  }
	private:

		T* m_Lock;
	};

	typedef ScopeLock<ThreadLock> ScopeThreadLock;

}
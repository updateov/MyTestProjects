#pragma once

namespace patterns
{
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

		  BOOL try_acquire()
		  {
			  return TryEnterCriticalSection(m_pCriticalSection);
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
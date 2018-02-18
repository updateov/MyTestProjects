#pragma once

#include <map>
#include <string>

using namespace std;

namespace patterns
{
	/*
	=======================================================================================================================
	Encapsulation of all contraction properties. This is used as a data-holding class. This stores indices in the Fhr
	signal of the originating fetus instance. Most methods are self-documenting. A contraction may be final or not,
	depending on wether detection has determined it to be certain or not with regards to both its existence at all and
	its location. See methods is_final () and set_as_final (). s, p, e: start, peak and end sample indices in the
	originating fetus instance, see method fetus::get_fhr (). ifinal -> set_as_final ().
	=======================================================================================================================
	*/
	class contraction
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		long _artifactkey;

		long s;
		long p;
		long e;
		bool ifinal;
		bool istrikeout;

		mutable map<string, string> m_extensions;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		contraction(void);
		contraction(long, long, long);
		contraction(const contraction &x0);
		virtual ~contraction(void);

		virtual long get_artifactkey(void) const {return _artifactkey;}
		virtual void set_artifactkey(long value) {_artifactkey = value;}

		virtual long get_end(void) const;
		virtual long get_peak(void) const;
		virtual long get_start(void) const;
		virtual bool is_final(void) const;

		virtual void set_end(long);
		virtual void set_peak(long);
		virtual void set_start(long);
		virtual void set_as_final(bool = true);

		virtual bool is_strike_out(void) const {return istrikeout;}
		virtual void set_as_strike_out(bool value = true) {istrikeout = value;}

		virtual long get_length(void) const {return e - s + 1;}

		virtual contraction &operator =(const contraction &x0);
		virtual bool operator ==(const contraction &x0) const;
		virtual bool operator !=(const contraction &x0) const;
		virtual contraction operator +(long) const;
		virtual contraction &operator +=(long);
		virtual contraction operator -(long) const;
		virtual contraction &operator -=(long);
		virtual contraction operator *(long) const;
		virtual contraction &operator *=(long);
		virtual contraction operator /(long) const;
		virtual contraction &operator /=(long);

		virtual bool intersects(const contraction &x0) const;

		bool has_extension(string name) const;
		string get_extension(string name) const;
		void set_extension(string name, string value);
	};
}
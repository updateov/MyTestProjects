#pragma once

#include <string>

namespace patterns
{
	using namespace std;

	/*
	=======================================================================================================================
	Encapsulation of a marking on the tracing. This holds the properties of annotations, vital sign samples or whatever
	has to be attached to a specific point on the Fhr or Up tracings. As do the contraction and event classes, this
	hold an index in the Fhr signal of the originating fetus instance. Methods are self-documenting. d ->
	set_description (). i -> set_where (). t -> set_type (). x -> set ().
	=======================================================================================================================
	*/
	class mark
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum type { tdiastolic, tgeneral, toximetry, tpulse, trespirations, tsystolic, ttemperature };

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		string d;
		long i;
		type t;
		long x;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		mark(void);
		mark (long, type, const string &, long = -1);
		mark(long, type, long);
		mark(const mark &);
		virtual ~mark(void);

		virtual long get(void) const;
		virtual const string &get_description(void) const;
		virtual type get_type(void) const;
		virtual long get_where(void) const;
		virtual mark &operator =(const mark &);
		virtual bool operator ==(const mark &x0) const;
		virtual bool operator !=(const mark &x0) const;
		virtual void set(long);
		virtual void set_description(const string &);
		virtual void set_type(type);
		virtual void set_where(long);
	};
}
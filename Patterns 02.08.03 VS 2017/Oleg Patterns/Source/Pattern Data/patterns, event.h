//REVIEW 27/03/14
#pragma once

#include "fhrPart.h"

#include <map>
#include <string>


namespace patterns
{
	const int LONG_DECEL_DURATION = 240;
	const int LARGE_DECEL_HEIGHT = 60;

	/*
	=======================================================================================================================
	Encapsulation of all event properties. This is used as a data-holding class. Like the contraction class, this
	stores indices in the Fhr signal of the originating fetus instance. Most methods are self-documenting. Event type
	values will not change: types that become obsolete will be kept for backward compatibility and converted to new
	types by the set_type () method. Similarly, compatibility with the SignalProcessing component is encapsulated in
	the set_type (BumpType) method. a: assurance (confidence) of the event. h: height. r: repair. Represent the level
	of repair done on the fhr data under the event. c: index of corresponding contraction in the originating fetus
	instance, see method fetus::get_contraction (). ifinal -> set_as_final (). istrikeout -> strike_out(). s, p, e:
	start, peak and end sample indices in the originating fetus instance, see method fetus::get_fhr (). t: event
	type.
	=======================================================================================================================
	*/
	class event
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum type
		{
			tacceleration	= 1,
			taccelerationni = 2,
			tearly			= 3,
			ttypical		= 4,
			tatypical		= 5,
			tlate			= 6,
			tnadeceleration = 7,
			tnideceleration = 8,
			tbaseline		= 9,
			tdebug1			= 10,
			tdebug2			= 11,
			trepaired		= 12,
			terror			= 13,
			tprolonged		= 14
		};

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		long _artifactkey;

		mutable map<string, string> m_extensions;

		long c;
		long s;
		long p;
		long e;
		bool ifinal;
		bool istrikeout;
		double y1;
		double y2;
		type t;
		double m_confidence;
		double r;
		double h;
		double v;
		double pv;
		bool islate;
		bool isvariable;
		long lag;
		bool isnoninterp;
		bool isconfirmed;
		long atyp;

		virtual bool are_within_epsilon(double, double) const;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		event(void);
		event (long, long, long, long, double, double, type, double = -1, double = -1, double = -1, double = 0.0, double = 0.0, bool = false, bool = false, long = 0, bool = false, bool = false, long = 0);
		event(const event &);
		virtual ~event(void);

		virtual long get_artifactkey(void) const {return _artifactkey;}
		virtual void set_artifactkey(long value) {_artifactkey = value;}

		virtual long get_atypical(void) const {return atyp;}
		virtual void set_atypical (long atyp0) {atyp = atyp0 ;}
		virtual bool is_biphasic(void) const {return ((atyp & decel::tbiphasic) != 0) ;}
		virtual bool is_loss_rise(void) const {return ((atyp & decel::tlossrise) != 0) ;}
		virtual bool is_loss_var(void) const {return ((atyp & decel::tlossvar) != 0) ;}
		virtual bool is_lower_bas(void) const {return ((atyp & decel::tlowerbas) != 0) ;}
		virtual bool is_prol_sec_rise(void) const {return ((atyp & decel::tprolsecrise) != 0) ;}
		virtual bool is_sixties(void) const {return ((atyp & decel::tsixties) != 0) ;}
		virtual bool is_slow_return(void) const {return ((atyp & decel::tslowreturn) != 0) ;}
		virtual bool is_large_and_long(void) const {return (e - s > LONG_DECEL_DURATION && h > LARGE_DECEL_HEIGHT);}

		virtual long get_contraction(void) const {return c;}
		virtual void set_contraction(long value) {c = value;}

		virtual long get_start(void) const {return s;}
		virtual void set_start(long value) {s = value;}

		virtual long get_peak(void) const {return p;}
		virtual void set_peak(long value) {p = value;}

		virtual long get_end(void) const {return e;}
		virtual void set_end(long value) {e = value;}

		virtual bool is_final(void) const {return ifinal;}
		virtual void set_as_final(bool value = true) {ifinal = value;}

		virtual bool is_strike_out(void) const {return istrikeout;}
		virtual void set_as_strike_out(bool value = true) {istrikeout = value;}
		
		virtual double get_y1(void) const {return y1;}
		virtual void set_y1(double value) {y1 = value;}
		
		virtual double get_y2(void) const {return y2;}
		virtual void set_y2(double value) {y2 = value;}

		virtual type get_type(void) const {return t;}
		virtual void set_type(type value) {t = value;}

		virtual bool is_confidence_set(void) const {return m_confidence != -1;}
		virtual double get_confidence(void) const {return m_confidence;}
		virtual void set_confidence(double value) {m_confidence = value;}

		virtual bool is_repair_set(void) const {return r != -1;}
		virtual double get_repair(void) const {return r;}
		virtual void set_repair(double value) {r = value;}
		
		virtual bool is_height_set(void) const {return h != -1;}
		virtual double get_height(void) const {return h;}
		virtual void set_height(double value) {h = value;}

		virtual double get_baseline_var(void) const {return v;}
		virtual void set_baseline_var(double value) {v = value;}

		virtual double get_peak_val(void) const {return pv;}
		virtual void set_peak_val(double value) {pv = value;}

		virtual bool is_late(void) const {return islate;}
		virtual void set_as_late(bool value = true) {islate = value;}

		virtual bool is_variable(void) const {return isvariable;}
		virtual void set_as_variable(bool value = true) {isvariable = value;}

		virtual long get_lag(void) const {return lag;}
		virtual void set_lag(long value) {lag = value;}

		virtual bool is_noninterp(void) const {return isnoninterp;}
		virtual void set_as_noninterp(bool value = true) {isnoninterp = value;}

		virtual bool is_confirmed(void) const {return isconfirmed;}
		virtual void set_as_confirmed(bool value = true) {isconfirmed = value;}

		virtual long get_length(void) const {return e - s + 1;}
			
		
		virtual bool intersects(const event &x0) const;

		virtual bool is_baseline(void) const {return event::is_baseline(get_type());}
		virtual bool is_associated(void) const {return event::is_associated(get_type());}
		virtual bool is_acceleration(void) const {return event::is_acceleration(get_type());}	
		virtual bool is_deceleration(void) const {return event::is_deceleration(get_type());}

		static bool is_baseline(type);
		static bool is_associated(type);
		static bool is_acceleration(type);
		static bool is_deceleration(type);
		
		virtual event &operator=(const event &);
		virtual bool operator==(const event &x0) const;
		virtual bool operator!=(const event &x0) const;
		virtual event operator+(long) const;
		virtual event &operator+=(long);
		virtual event operator-(long) const;
		virtual event &operator-=(long);
		virtual event operator*(long) const;
		virtual event &operator*=(long);
		virtual event operator/(long) const;
		virtual event &operator/=(long);

		bool has_extension(string name) const;
		string get_extension(string name) const;
		void set_extension(string name, string value);

	};
}

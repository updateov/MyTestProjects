#include "stdafx.h"
#include "patterns, event.h"

using namespace patterns;

#define abs(a)	((a) > 0 ? (a) : -(a))

/*
 =======================================================================================================================
    Construction and destruction.
 =======================================================================================================================
 */
event::event(void)
{
	_artifactkey = -1;

	c = s = p = e = lag = atyp = 0;
	m_confidence = r = v = -1;
	h = pv = 0.0;
	ifinal = true;
	istrikeout = false;
	islate = isvariable = isnoninterp = isconfirmed = false;
	t = terror;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event::event(long c0, long s0, long p0, long e0, double y10, double y20, type t0, double a0, double r0, double h0, double v0, double pv0, bool islate0, bool isvariable0, long lag0, bool isnoninterp0, bool isconfirmed0, long atyp0)
{
	_artifactkey = -1;

	c = c0;
	s = s0;
	p = p0;
	e = e0;
	ifinal = true;
	istrikeout = false;
	y1 = y10;
	y2 = y20;
	t = t0;
	m_confidence = a0;
	r = r0;
	h = h0;
	v = v0;
	pv = pv0;
	islate = islate0;
	isvariable = isvariable0;
	lag = lag0;
	isnoninterp = isnoninterp0;
	isconfirmed = isconfirmed0;
	atyp = atyp0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event::event(const event &x0)
{
	*this = x0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event::~event(void)
{
}

/*
 =======================================================================================================================
    Are the two given real numbers equal within epsilon? As real are exported as strings or may be otherwise slightly
    modified for whatever reason, we need to compare them within an epsilon difference.
 =======================================================================================================================
 */
bool event::are_within_epsilon(double x, double y) const
{
	return abs(x - y) / (abs(x) + abs(y)) < (double) 0.00001;
}


/*
 =======================================================================================================================
    Do this and given event intersect? See patterns:::contraction:::intersects ().
 =======================================================================================================================
 */
bool event::intersects(const event &x0) const
{
	// TFS bug 3322 fix: allow 3/4 sec overlap for events
	
		return (get_end() > x0.get_start() + 3 && get_start() < x0.get_end() - 3);	
	
		//return get_end() >= x0.get_start() && get_start() <= x0.get_end();	
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool event::is_acceleration(type x)
{
	return x == tacceleration || x == taccelerationni;
}

/*
 =======================================================================================================================
    Is this event associated?
 =======================================================================================================================
 */
bool event::is_associated(type x)
{
	return x == tacceleration || x == taccelerationni || x == tearly || x == ttypical || x == tatypical || x == tlate || x == tnideceleration;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool event::is_baseline(type x)
{
	return x == tbaseline;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool event::is_deceleration(type x)
{
	return x == tearly || x == ttypical || x == tatypical || x == tlate || x == tnadeceleration || x == tnideceleration || x == tprolonged;
}


/*
 =======================================================================================================================
    Operators. All operators have the implied meaning and act on all properties.
 =======================================================================================================================
 */
event &event::operator=(const event &x0)
{
	_artifactkey = x0._artifactkey;
	m_extensions = x0.m_extensions;

	c = x0.c;
	s = x0.s;
	p = x0.p;
	e = x0.e;
	ifinal = x0.ifinal;
	istrikeout = x0.istrikeout;
	t = x0.t;
	y1 = x0.y1;
	y2 = x0.y2;
	m_confidence = x0.m_confidence;
	r = x0.r;
	h = x0.h;
	v = x0.v;
	pv = x0.pv;
	islate = x0.islate;
	isvariable = x0.isvariable;
	lag = x0.lag;
	isnoninterp = x0.isnoninterp;
	isconfirmed = x0.isconfirmed;
	atyp = x0.atyp;
	return *this;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool event::operator==(const event &x0) const
{
	return 
		c == x0.c
		&&	s == x0.s
		&&	p == x0.p
		&&	e == x0.e
		&&	ifinal == x0.ifinal
		&&	t == x0.t
		&&	m_confidence == x0.m_confidence
		&&	r == x0.r
		&&	h == x0.h
		&&	are_within_epsilon(y1, x0.y1)
		&&	are_within_epsilon(y2, x0.y2)
		&&	v == x0.v
		&&	pv == x0.pv
		&&	islate == x0.islate
		&&	isvariable == x0.isvariable
		&&	lag == x0.lag
		&&  isnoninterp == x0.isnoninterp
		&&  isconfirmed == x0.isconfirmed
		&&  atyp == x0.atyp
		&&  _artifactkey == x0._artifactkey;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool event::operator!=(const event &x0) const
{
	return c != x0.c || s != x0.s || p != x0.p || e != x0.e || ifinal != x0.ifinal || t != x0.t || m_confidence != x0.m_confidence || r != x0.r || h != x0.h || !are_within_epsilon(y1, x0.y1) || !are_within_epsilon(y2, x0.y2) || v != x0.v || pv != x0.pv || islate != x0.islate || isvariable != x0.isvariable || lag != x0.lag || isnoninterp != x0.isnoninterp || isconfirmed != x0.isconfirmed || atyp != x0.atyp || _artifactkey != x0._artifactkey;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event event::operator+(long d) const
{
	event r = *this;
	return r += d;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event &event::operator+=(long d)
{
	s = get_start() + d;
	p = get_peak() + d;
	e = get_end() + d;
	return *this;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event event::operator-(long d) const
{
	event r = *this;
	return r -= d;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event &event::operator-=(long d)
{
	s = get_start() - d;
	p = get_peak() - d;
	e = get_end() - d;
	return *this;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event event::operator*(long f) const
{
	event r = *this;
	return r *= f;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event &event::operator*=(long f)
{
	s = f * get_start();
	p = f * get_peak();
	e = f * get_end();
	return *this;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event event::operator/(long d) const
{
	event r = *this;
	return r /= d;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
event &event::operator/=(long d)
{
	s = get_start() / d;
	p = get_peak() / d;
	e = get_end() / d;
	return *this;
}


/////////////
/// Extension methods to allow class using contraction to store custom data on a contraction

// Check if a specific extension exists
bool event::has_extension(string name) const
{
	return m_extensions.find(name) != m_extensions.end();
}

// Returns the value associated to an extension
string event::get_extension(string name) const
{
	return m_extensions[name];
}

// Set the value associated to an extension
void event::set_extension(string name, string value)
{
	m_extensions[name] = value;
}
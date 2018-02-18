#include "stdafx.h"
#include "patterns, mark.h"

using namespace patterns;

/*
 =======================================================================================================================
    Construction and destruction.
 =======================================================================================================================
 */
mark::mark(void)
{
	i = 0;
	t = tgeneral;
	x = -1;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
mark::mark(long i0, type t0, const string &d0, long x0)
{
	d = d0;
	i = i0;
	t = t0;
	x = x0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
mark::mark(long i0, type t0, long x0)
{
	i = i0;
	t = t0;
	x = x0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
mark::mark(const mark &x0)
{
	*this = x0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
mark::~mark(void)
{
}

/*
 =======================================================================================================================
    Access the mark's value. See set () for a discussion of the values' domain.
 =======================================================================================================================
 */
long mark::get(void) const
{
	return x;
}

/*
 =======================================================================================================================
    Access the mark's description string.
 =======================================================================================================================
 */
const string &mark::get_description(void) const
{
	return d;
}

/*
 =======================================================================================================================
    Access the mark's type.
 =======================================================================================================================
 */
mark::type mark::get_type(void) const
{
	return t;
}

/*
 =======================================================================================================================
    Access the mark's position as an index in the Fhr set. See set_where ().
 =======================================================================================================================
 */
long mark::get_where(void) const
{
	return i;
}

/*
 =======================================================================================================================
    Operators. All operators have the implied meaning and act on all properties.
 =======================================================================================================================
 */
mark &mark::operator=(const mark &x0)
{
	d = x0.d;
	i = x0.i;
	t = x0.t;
	x = x0.x;
	return *this;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool mark::operator==(const mark &x0) const
{
	return d == x0.d && i == x0.i && t == x0.t && x == x0.x;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
bool mark::operator!=(const mark &x0) const
{
	return d != x0.d || i != x0.i || t != x0.t || x != x0.x;
}

/*
 =======================================================================================================================
    Set the mark's value. Certain mark types have an inherent associated value. Theses values' domains depend on the
    type of value but will usually be in the [0... 255] subset of N. Client components may use the tgeneral type at
    their convenience.
 =======================================================================================================================
 */
void mark::set(long x0)
{
	x = x0;
}

/*
 =======================================================================================================================
    Set the mark's description string.
 =======================================================================================================================
 */
void mark::set_description(const string &d0)
{
	d = d0;
}

/*
 =======================================================================================================================
    Set the mark's type.
 =======================================================================================================================
 */
void mark::set_type(type t0)
{
	t = t0;
}

/*
 =======================================================================================================================
    Set the mark's position in the Fhr sample set. As discussed in the class comments, a mark is always associated to a
    specific location, that is, to a specific index in the set of Fhr samples.
 =======================================================================================================================
 */
void mark::set_where(long i0)
{
	i = i0;
}
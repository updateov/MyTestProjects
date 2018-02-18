#include "stdafx.h"
#include "patterns, conductor.h"

#include "patterns, config.h"
#include "patterns, fetus.h"
#include "patterns, input adapter.h"
#include <time.h>
#include <assert.h>

using namespace patterns;

// =====================================================================================================================
//    Constructor and destructor.
// =======================================================================================================================
conductor::conductor(void)
{
	m_inputAdapter = 0;
	fdump = 0;
	idebug = false;

	config.read_file("patterns.config");
}

conductor::conductor(input_adapter* i0)
{
	m_inputAdapter = 0;
	fdump = 0;
	idebug = false;
	set_input_adapter(i0);

	config.read_file("patterns.config");
}

conductor::~conductor(void)
{
	if (m_inputAdapter)
	{
		delete m_inputAdapter;
		m_inputAdapter = NULL;
	}

	if (fdump)
	{
		delete fdump;
		fdump = NULL;
	}

	for (long i = 0; i < (long)m_subscribers.size(); i++)
	{
		delete m_subscribers[i];
	}

	m_subscribers.clear();

	for (map<string, fetus*>::iterator i = m_fetuses.begin(); i != m_fetuses.end(); i++)
	{
		delete i->second;
	}
	m_fetuses.clear();
}

// =====================================================================================================================
//    Access a fetus from given unique identifier. See predicate is_known ().
// =======================================================================================================================
const fetus& conductor::get(const string& n) const
{
	const fetus*  r = 0;

	if (is_known(n))
	{
		r = m_fetuses.find(n)->second;
	}

	if (!r && !fdump)
	{
		fdump = NewFetus();
		fdump->set_conductor (const_cast<conductor*>(this));
	}

	return r ? *r : *fdump;
}

fetus& conductor::get(const string& n)
{
	fetus*	r = 0;

	if (is_known(n))
	{
		r = m_fetuses.find(n)->second;
	}

	if (!r && !fdump)
	{
		fdump = NewFetus();
		fdump->set_conductor(this);
	}

	return r ? *r : *fdump;
}
const fetus* conductor::get_fetus(const string &n) const
{
	const fetus*  r = 0;

	if (is_known(n))
	{
		r = m_fetuses.find(n)->second;
	}

	if (!r && !fdump)
	{
		fdump = NewFetus();
		fdump->set_conductor (const_cast<conductor*>(this));
	}

	return r ? r : fdump;

}

fetus* conductor::get_fetus(const string &n)
{
	fetus*	r = 0;

	if (is_known(n))
	{
		r = m_fetuses.find(n)->second;
	}

	//if (!r && !fdump)
	{
		fdump = NewFetus();
		fdump->set_conductor(this);
	}

	return r ? r : fdump;

}
// =====================================================================================================================
//    Access the adapter instances. See the corresponding has_..._adapter () predicates.
// =======================================================================================================================
const input_adapter& conductor::get_input_adapter(void) const
{
	return *m_inputAdapter;
}

input_adapter& conductor::get_input_adapter(void)
{
	return *m_inputAdapter;
}

// =====================================================================================================================
//    Get the number of fetuses.
// =======================================================================================================================
long conductor::get_number(void) const
{
	return (long)m_fetuses.size();
}

// =====================================================================================================================
//    Get the set of all patients. This is just syntactic sugar.
// =======================================================================================================================
set<string> conductor::get_patients(void) const
{
	set<string> r;

	for (map<string, fetus*>::const_iterator i = m_fetuses.begin(); i != m_fetuses.end(); i++)
	{
		r.insert(i->first);
	}

	return r;
}

// =====================================================================================================================
//    Does the conductor have adapter instances set? See the corresponding get_..._adapter () methods.
// =======================================================================================================================
bool conductor::has_input_adapter(void) const
{
	return m_inputAdapter ? true : false;
}

// =====================================================================================================================
//    Does given unique identifier refer to a known fetus? See method get () and patient access methods in the
//    input_adapter class.
// =======================================================================================================================
bool conductor::is_known(const string& n) const
{
	return m_fetuses.count(n) > 0;
}

// =====================================================================================================================
//    Propagate given notification message to subscribers.
// =======================================================================================================================
void conductor::note(subscription::message m)
{
	for (long i = 0; i < (long)m_subscribers.size(); i++)
	{
		m_subscribers[i]->note(m);
	}
}

void conductor::note(subscription::message m, string id)
{
	for (long i = 0; i < (long)m_subscribers.size(); i++)
	{
		m_subscribers[i]->note(m, id);
	}

	switch (m)
	{
		case subscription::mpatientstatus:
			if (is_known(id))
			{
				get(id).note(m);
			}
			break;
	}
}

void conductor::note(subscription::message m, long id)
{
	for (long i = 0; i < (long)m_subscribers.size(); i++)
	{
		m_subscribers[i]->note(m, id);
	}
}

// =====================================================================================================================
//    Apply the new config
// =======================================================================================================================
void conductor::apply_config(ConfigData& data)
{
	config = data;
	note(subscription::mconfig);
}

// =====================================================================================================================
//    Every so often, make sure all newly detected data is flush and committed
// =======================================================================================================================
void conductor::commit_pending_calculations()
{
	for (map<string, fetus*>::iterator i = m_fetuses.begin(); i != m_fetuses.end(); i++)
	{
		i->second->fetch_events();
	}
}

// =====================================================================================================================
//    Format an event as a nice string that can be sent over the network
// =======================================================================================================================
string conductor::to_string(const event& ei, char separator)
{
	vector<string> v;

	v.push_back(to_string((long)ei.get_type()));
	v.push_back(to_string(ei.get_start()));
	v.push_back(to_string(ei.get_peak()));
	v.push_back(to_string(ei.get_end()));
	v.push_back(to_string(ei.get_y1()));
	v.push_back(to_string(ei.get_y2()));
	v.push_back(to_string(ei.get_contraction()));
	v.push_back(ei.is_final() ? "y" : "");
	v.push_back(ei.is_strike_out() ? "y" : "");
	v.push_back(to_string(ei.get_confidence()));
	v.push_back(to_string(ei.get_repair()));
	v.push_back(to_string(ei.get_height()));
	v.push_back(to_string(ei.get_baseline_var()));
	v.push_back(to_string(ei.get_peak_val()));
	v.push_back(ei.is_late() ? "y" : "");
	v.push_back(ei.is_variable() ? "y" : "");
	v.push_back(to_string(ei.get_lag()));
	v.push_back(to_string(ei.get_atypical()));
	v.push_back(ei.is_noninterp() ? "y" : "");
	v.push_back(ei.is_confirmed() ? "y" : "");
	v.push_back(to_string(ei.get_artifactkey()));

	return to_string(v, separator);
}

bool conductor::to_contraction(contraction& c, const string& s, char separator)
{
	vector<string> mri;
	to_vector(s, mri, separator);

	if (mri.size() < 5)
	{
		return false;
	}

	c.set_start(to_integer(mri[0]));
	c.set_peak(to_integer(mri[1]));
	c.set_end(to_integer(mri[2]));
	c.set_as_final(mri[3] == "y");
	c.set_as_strike_out(mri[4] == "y");

	if (mri.size() > 5)
	{
		c.set_artifactkey(to_integer(mri[5]));
	}

	return true;
}

bool conductor::to_event(event& e, const string& s, char separator)
{
	vector<string> mri;
	to_vector(s, mri, separator);

	if (mri.size() < 20)
	{
		return false;
	}

	e.set_type((event::type) to_integer(mri[0]));
	e.set_start(to_integer(mri[1]));
	e.set_peak(to_integer(mri[2]));
	e.set_end(to_integer(mri[3]));
	e.set_y1(to_real(mri[4]));
	e.set_y2(to_real(mri[5]));
	e.set_contraction(to_integer(mri[6]));
	e.set_as_final(mri[7] == "y");
	e.set_as_strike_out(mri[8] == "y");
	e.set_confidence(to_real(mri[9]));
	e.set_repair(to_real(mri[10]));
	e.set_height(to_real(mri[11]));
	e.set_baseline_var(to_real(mri[12]));
	e.set_peak_val(to_real(mri[13]));
	e.set_as_late(mri[14] == "y");
	e.set_as_variable(mri[15] == "y");
	e.set_lag(to_integer(mri[16]));
	e.set_atypical(to_integer(mri[17]));
	e.set_as_noninterp(mri[18] == "y");
	e.set_as_confirmed(mri[19] == "y");

	if (mri.size() > 20)
	{
		e.set_artifactkey(to_integer(mri[20]));
	}

	return true;
}

// =====================================================================================================================
//    Format a contraction as a nice string that can be sent over the network
// =======================================================================================================================
string conductor::to_string(const contraction& ci, char separator)
{
	vector<string> v;

	v.push_back(to_string(ci.get_start()));
	v.push_back(to_string(ci.get_peak()));
	v.push_back(to_string(ci.get_end()));
	v.push_back(ci.is_final() ? "y" : "");
	v.push_back(ci.is_strike_out() ? "y" : "");
	v.push_back(to_string(ci.get_artifactkey()));

	return to_string(v, separator);
}

// =====================================================================================================================
//    Format configuration as a nice string that can be sent over the network
// =======================================================================================================================
string conductor::to_string(const ConfigData& c, char separator)
{
	vector<string> message;

	message.push_back("config");
	message.push_back("1");
	message.push_back(c.to_string());

	return to_string(message, separator);
}

// =====================================================================================================================
//    Format contractions as a nice string that can be sent over the network
// =======================================================================================================================
string conductor::to_string(const string& fetusname, const vector<contraction>& cis, char separator)
{
	vector<string> header;

	header.push_back("results");
	header.push_back("1");
	header.push_back(fetusname);
	header.push_back("contractions");

	vector<string> data;

	for (vector<contraction>::const_iterator itr = cis.begin(); itr != cis.end(); ++itr)
	{
		data.push_back(to_string(*itr, separator + 2));
	}

	header.push_back(to_string(data, separator + 1));
	data.empty();

	return to_string(header, separator);
}

// =====================================================================================================================
//    Format events as a nice string that can be sent over the network
// =======================================================================================================================
string conductor::to_string(const string& fetusname, const vector<event>& eis, char separator)
{
	vector<string> header;

	header.push_back("results");
	header.push_back("1");
	header.push_back(fetusname);
	header.push_back("events");

	vector<string> data;
	for (vector<event>::const_iterator itr = eis.begin(); itr != eis.end(); ++itr)
	{
		data.push_back(to_string(*itr, separator + 2));
	}

	header.push_back(to_string(data, separator + 1));
	data.empty();

	return to_string(header, separator);
}

// =====================================================================================================================
//    instance.
// =======================================================================================================================
void conductor::set_input_adapter(input_adapter* a0)
{
	if (m_inputAdapter)
	{
		delete m_inputAdapter;
	}

	m_inputAdapter = a0;
	if (m_inputAdapter)
	{
		m_inputAdapter->set_conductor(this);
		update_fetuses();
	}
}

// =====================================================================================================================
//    Accept given subscription instance. We own the given object;
//    it will from now on receive notifications from the conductor. This does not imply that it will receive
//    notifications from fetuses, client instances must subscribe separately to fetuses which they want to receive
//    notifications from.
// =======================================================================================================================
void conductor::subscribe(subscription* s0)
{
	m_subscribers.insert(m_subscribers.end(), s0);
}

// =====================================================================================================================
//    Conversion methods. These are not in the Stl. We may decide to move them to some services class at some point in
//    the future. As this is used to build and decode messages that are inherently dependent on some network connection,
//    we do not care about making this code particularly efficient.
// =======================================================================================================================
long conductor::to_integer(const string& s)
{
	return atol(s.c_str());
}

double conductor::to_real(const string& s)
{
	return atof(s.c_str());
}

string conductor::to_string(double x)
{
	char t[1000];

	sprintf(t, "%.6f", (double)x);
	return t;
}

string conductor::to_string(long x)
{
	char t[1000];

	sprintf(t, "%ld", (long)x);
	return t;
}

string conductor::to_string(const vector<string>& x, char s)
{
	string r;

	for (long i = 0, n = (long)x.size(); i < n; i++)
	{
		if (i > 0)
		{
			r += s;
		}

		r += x[i];
	}

	return r;
}

// =====================================================================================================================
//    Helper function to split a string in sub strings based on a token
// =======================================================================================================================
void conductor::to_vector(const string& str, vector<string>& tokens, char delimiter)
{
	// Flush previous values
	tokens.clear();

	if (str.length() == 0)
	{
		return;
	}

	// Skip delimiter at beginning.
	string::size_type startToken = 0;

	// Scan the token one by one
	while (true)
	{
		// Find first "non-delimiter".
		string::size_type endToken = str.find_first_of(delimiter, startToken);

		// Found a token, add it to the vector.
		tokens.push_back(str.substr(startToken, endToken - startToken));

		// Are we done?
		if (endToken == string::npos)
		{
			return;
		}

		startToken = endToken + 1;
	}
}

// =====================================================================================================================
//    Unsubscribe given object. Objects may be unsubscribe by pointer or by opaque pointer. When unsubscribing through an
//    opaque pointer, we do not take for granted that there will be no more than one subscription related to the given
//    pointer. See method subscription:::is ().
// =======================================================================================================================
void conductor::unsubscribe(subscription* s0)
{
	for (long i = 0, n = (long)m_subscribers.size(); i < n; i++)
	{
		if (m_subscribers[i] == s0)
		{
			delete m_subscribers[i];
			m_subscribers.erase(m_subscribers.begin() + i);
			i = n;
		}
	}
}

void conductor::unsubscribe(void* s0)
{
	for (long i = 0, n = (long)m_subscribers.size(); i < n; i++)
	{
		if (m_subscribers[i]->is(s0))
		{
			delete m_subscribers[i];
			m_subscribers.erase(m_subscribers.begin() + i--);
			n--;
		}
	}
}

// =====================================================================================================================
//    Update list of fetuses from input adapter. This may be called when the input adapter is set or when it sends us a
//    notification of patient-list modification, for instance. We don't explicitely unsubscribe deleted fetuses because
//    the fetus class's destructor takes care of it.
// =======================================================================================================================
void conductor::update_fetuses(void)
{
	if (has_input_adapter())
	{
		input_adapter&	a = get_input_adapter();
		map<string, bool> ifound;

		for (map<string, fetus*>::const_iterator itr = m_fetuses.begin(); itr != m_fetuses.end(); ++itr)
		{
			ifound.insert(pair<string, bool> (itr->first, false));
		}

		const vector<input_adapter::patient *> patients = a.get_patients();
		for (vector < input_adapter::patient * >::const_iterator itr = patients.begin(); itr != patients.end(); ++itr)
		{
			string key = (*itr)->get_key();

			map<string, bool>::iterator found = ifound.find(key);
			if (found != ifound.end())
			{
				found->second = true;
			}
			else
			{
				pair<map<string, fetus*>::iterator, bool> added = m_fetuses.insert(pair<string, fetus*> (key, NewFetus()));
				added.first->second->set_conductor(this);
				added.first->second->set_key(key);
			}
		}

		for (map<string, bool>::iterator itr = ifound.begin(); itr != ifound.end(); ++itr)
		{
			if (!(itr->second))
			{
				string key = itr->first;

				map<string, fetus*>::iterator remove = m_fetuses.find(key);
				if (remove != m_fetuses.end())
				{
					if(remove->second != NULL)
					{
						remove->second->note(subscription::mwilldeletefetus);
						delete remove->second;
					}
					m_fetuses.erase(remove);
				}
			}
		}

		note(subscription::mpatientlist);
	}
}

fetus* conductor::NewFetus() const
{
	return new fetus;
}
#include "stdafx.h"
#include "patterns, input adapter.h"

#include "patterns, conductor.h"

using namespace patterns;

/*
=======================================================================================================================
Constructor and destructor.
=======================================================================================================================
*/
input_adapter::input_adapter(void)
{
	m_pConductor = 0;
	d = 0;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
input_adapter::~input_adapter(void)
{
	if (d)
	{
		delete d;
		d = NULL;
	}
}


/*
=======================================================================================================================
Access the associated conductor. If there is no such conductor, we still return a valid object to reduce crash
risks. We do not care about destroying the dump object as this is an exception situation and as the number of
instances is limited to one.
=======================================================================================================================
*/
const conductor &input_adapter::get_conductor(void) const
{
	if (!m_pConductor && !d)
	{
		d = NewConductor();
	}

	return m_pConductor ? *m_pConductor : *d;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
conductor &input_adapter::get_conductor(void)
{
	if (!m_pConductor && !d)
	{
		d = NewConductor();
	}

	return m_pConductor ? *m_pConductor : *d;
}

/*
=======================================================================================================================
do we have an associated conductor instance?
=======================================================================================================================
*/
bool input_adapter::has_conductor(void) const
{
	return m_pConductor ? true : false;
}

/*
=======================================================================================================================
Note given event and riderect to conductor. This is meant to be used by subclasses when they need to pass
information back to their conductor.
=======================================================================================================================
*/
void input_adapter::note(subscription::message x)
{
	switch (x)
	{
		case subscription::mpatientlist:
			if (m_pConductor)
			{
				m_pConductor->update_fetuses();
			}
			break;
		}
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void input_adapter::note(subscription::message x, string id)
{
	note(x);
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void input_adapter::note(subscription::message x, long id)
{
	switch (x)
	{
		case subscription::mpatientstatus:
			if (m_pConductor)
			{
				m_pConductor->note(x, id);
			}
			break;

		default:
			note(x);
			break;
	}
}
/*
=======================================================================================================================
Implementation of the patient class.
=======================================================================================================================
*/
input_adapter::patient::patient(void)
{
	key = "";
	reset();
}

void input_adapter::patient::reset(void)
{
	age = bed = dilatation = effacement = "" ;
	gestational_age = id = name = "";
	station = surname = accountno = "";
	edc = exam_date_time = undetermined_date;
	memset(&dob, 0, sizeof(FILETIME));
	edc_s = exam_date_time_s = "";
	fetuses = 1;
	is_collecting_tracing = false;
	is_late_collecting_tracing = false;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
input_adapter::patient::patient(const patient &x0)
{
	*this = x0;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
input_adapter::patient::~patient(void)
{
}

/*
=======================================================================================================================
Compare two patients
=======================================================================================================================
*/
bool input_adapter::compare(patient* p1, patient* p2) const
{
	if (p1 == 0)
	{
		return false;
	}
	if (p2 == 0)
	{
		return true;
	}

	string k1 = p1->get_key();
	string k2 = p2->get_key();

	if (k1 < k2)
	{
		return true;
	}
	if (k1 > k2)
	{
		return false;
	}
	return (DWORD)p1 < (DWORD)p2;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_age(void) const
{
	return age;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_bed(void) const
{
	return bed;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_dilatation(void) const
{
	return dilatation;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
date input_adapter::patient::get_edc(void) const
{
	return edc;
}
/*
=======================================================================================================================
=======================================================================================================================
*/
FILETIME input_adapter::patient::get_dob(void) const
{
	return dob;
}
/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_edc_string(void) const
{
	return edc_s;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_effacement(void) const
{
	return effacement;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_displayed(void) const
{
	/*~~~~~*/
	string s;
	/*~~~~~*/

	s = get_bed() + ": ";
	s += get_surname();
	s += " ";
	s += get_name();
	return s;
}

bool input_adapter::patient::get_is_collecting_tracing(void)
{
	return is_collecting_tracing;
}

void input_adapter::patient::set_is_collecting_tracing(bool value)
{
	is_collecting_tracing = value;
}

bool input_adapter::patient::get_is_late_collecting_tracing(void)
{
	return is_late_collecting_tracing;
}

void input_adapter::patient::set_is_late_collecting_tracing(bool value)
{
	is_late_collecting_tracing = value;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_gestational_age(void) const
{
	return gestational_age;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_id(void) const
{
	return id;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_key(void) const
{
	return key;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
date input_adapter::patient::get_last_exam_creation_time(void) const
{
	return exam_date_time;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_last_exam_creation_time_string(void) const
{
	return exam_date_time_s;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_name(void) const
{
	return name;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long input_adapter::patient::get_number_of_fetuses(void) const
{
	return fetuses;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_station(void) const
{
	return station;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
string input_adapter::patient::get_surname(void) const
{
	return surname;
}

string input_adapter::patient::get_accountno(void) const
{
	return accountno;
}

input_adapter::patient & input_adapter::patient::operator =(const patient &x0)
{
	age = x0.age;
	bed = x0.bed;
	dilatation = x0.dilatation;
	edc = x0.edc;
	dob = x0.dob;
	effacement = x0.effacement;
	is_collecting_tracing = x0.is_collecting_tracing;
	is_late_collecting_tracing = x0.is_late_collecting_tracing;
	gestational_age = x0.gestational_age;
	exam_date_time = x0.exam_date_time;
	id = x0.id;
	key = x0.key;
	name = x0.name;
	station = x0.station;
	surname = x0.surname;
	accountno = x0.accountno;
	fetuses = x0.fetuses;
	edc_s = x0.edc_s;
	exam_date_time_s = x0.exam_date_time_s;
	return *this;
}

bool input_adapter::patient::operator ==(const patient &x0) const
{
	return age == x0.age
		&&	bed == x0.bed
		&&	dilatation == x0.dilatation
		&&	edc == x0.edc
		&&	dob.dwLowDateTime == x0.dob.dwLowDateTime
		&&	dob.dwHighDateTime == x0.dob.dwHighDateTime
		&&	effacement == x0.effacement
		&&	is_collecting_tracing == x0.is_collecting_tracing
		&&	is_late_collecting_tracing == x0.is_late_collecting_tracing
		&&	exam_date_time == x0.exam_date_time
		&&	gestational_age == x0.gestational_age
		&&	id == x0.id
		&&	key == x0.key
		&&	name == x0.name
		&&	station == x0.station
		&&	surname == x0.surname
		&&	accountno == x0.accountno
		&&	fetuses == x0.fetuses
		&&	edc_s == x0.edc_s
		&&	exam_date_time_s == x0.exam_date_time_s;
}

bool input_adapter::patient::operator !=(const patient &x0) const
{
	return !(*this == x0);
}

void input_adapter::patient::set_age(const string &a0)
{
	age = a0;
}

void input_adapter::patient::set_bed(const string &b0)
{
	bed = b0;
}

void input_adapter::patient::set_dilatation(const string &d0)
{
	dilatation = d0;
}

void input_adapter::patient::set_edc(const date &curDate)
{
	edc = curDate;
}

void input_adapter::patient::set_dob(const FILETIME &fileTime)
{
	dob = fileTime;
}

void input_adapter::patient::set_edc_string(const string &edcStr)
{
	edc_s = edcStr;
}

void input_adapter::patient::set_effacement(const string &e0)
{
	effacement = e0;
}

void input_adapter::patient::set_gestational_age(const string &g0)
{
	gestational_age = g0;
}

void input_adapter::patient::set_id(const string &i0)
{
	id = i0;
}

void input_adapter::patient::set_key(const string &k0)
{
	key = k0;
}

void input_adapter::patient::set_last_exam_creation_time(date d0)
{
	exam_date_time = d0;
}

void input_adapter::patient::set_last_exam_creation_time_string(const string &d0)
{
	exam_date_time_s = d0;
}

void input_adapter::patient::set_name(const string &n0)
{
	name = n0;
}

void input_adapter::patient::set_number_of_fetuses (long n0)
{
	fetuses = n0;
}

void input_adapter::patient::set_station(const string &s0)
{
	station = s0;
}

void input_adapter::patient::set_surname(const string &s0)
{
	surname = s0;
}

void input_adapter::patient::set_accountno(const string &s0)
{
	accountno = s0;
}

/*
* Set the associated conductor instance. We do not destroy the dump object right
* away as clients may that have mistakenly accessed it may have kept references
* to it. See method get_conductor ().
*/
void input_adapter::set_conductor(conductor* curConductor)
{
	m_pConductor = curConductor;
}

/*
 =======================================================================================================================
    load event`s observations and append them to the given fetus.
 =======================================================================================================================
 */
bool input_adapter::load_saved_data(patterns::fetus &curFetus, string filename)
{
	vector<string> lines;

	// look for the file and read the data in it if necessary
	FILE* file = fopen(filename.c_str(), "rb"); 
	if (file == NULL)
		return false;

	// Read the file line by line
	char line[1025]; // Assume all lines are under 1024 bytes
	while (fgets(line, 1024, file) != NULL)
	{
		lines.push_back((string)line);
	}
	fclose(file);

	return load_saved_data(curFetus, lines);
}

/*
 =======================================================================================================================
    load event`s observations and append them to the given fetus.
 =======================================================================================================================
 */
bool input_adapter::load_saved_data(patterns::fetus &curFetus, vector<string>& lines, long timeadjustment)
{
	contraction curContraction;
	event curEvent;

	vector<contraction> contractions;
	vector<event> events;

	vector<long> strctr;
	vector<long> strevt;
	vector<long> acpevt;

	for (vector<string>::iterator itr = lines.begin(); itr != lines.end(); ++itr)
	{
		string line = *itr;

		if (line.compare(0, 4, "CTR|") == 0)
		{
			// Contractions
			if (conductor::to_contraction(curContraction, line.substr(4), '|'))
			{
				if (timeadjustment != 0)
				{
					curContraction -= timeadjustment;
				}
				contractions.push_back(curContraction);
			}
			else
			{
				assert(FALSE);
			}
		}
		else if (line.compare(0, 4, "EVT|") == 0)
		{
			// Events
			if (conductor::to_event(curEvent, line.substr(4), '|'))
			{
				if (timeadjustment != 0)
				{
					curEvent -= 4 * timeadjustment;
				}
				events.push_back(curEvent);
			}
			else
			{
				assert(FALSE);
			}
		}
		else if (line.compare(0, 7, "STRCTR|") == 0)
		{
			// Strikeout contractions
			vector<string> message;
			conductor::to_vector(line, message, '|');
			if (message.size() > 1)
			{
				try 
				{ 
					strctr.push_back(atol(message[1].c_str())); 
				} 
				catch (...) 
				{ 
					assert(FALSE);
				}
			}
		}
		else if (line.compare(0, 7, "STREVT|") == 0)
		{
			// Strikeout event
			vector<string> message;
			conductor::to_vector(line, message, '|');
			if (message.size() > 1)
			{
				try 
				{ 
					strevt.push_back(atol(message[1].c_str())); 
				} 
				catch (...) 
				{ 
					assert(FALSE);
				}
			}
		}
		else if (line.compare(0, 7, "ACPEVT|") == 0)
		{
			// Strikeout event
			vector<string> message;
			conductor::to_vector(line, message, '|');
			if (message.size() > 1)
			{
				try 
				{ 
					acpevt.push_back(atol(message[1].c_str()));
				}
				catch (...) 
				{ 
					assert(FALSE);
				}
			}
		}
	}

	// Apply the data!
	curFetus.suspend_notifications(true);
	curFetus.append_contraction(contractions);
	curFetus.append_event(events);

	for (vector<long>::iterator i = strctr.begin(); i != strctr.end(); ++i)
	{
		contraction* pContraction = curFetus.get_contraction_starting(*i);
		if (pContraction != 0)
		{
			pContraction->set_as_strike_out();
		}
		else
		{
			assert(0);
		}
	}
	for (vector<long>::iterator i = strevt.begin(); i != strevt.end(); ++i)
	{
		event* pEvent = curFetus.get_event_starting(*i);
		if (pEvent != 0)
		{
			pEvent->set_as_strike_out();
		}
		else
		{
			assert(0);
		}
	}
	for (vector<long>::iterator i = acpevt.begin(); i != acpevt.end(); ++i)
	{
		event* pEvent = curFetus.get_event_starting(*i);
		if (pEvent != 0)
		{
			pEvent->set_as_confirmed(true);
		}
		else
		{
			assert(0);
		}
	}
	curFetus.suspend_notifications(false);

	return (curFetus.GetEventsCount() > 0) || (curFetus.GetContractionsCount() > 0);
}

conductor* input_adapter::NewConductor() const
{
	return new conductor;
}
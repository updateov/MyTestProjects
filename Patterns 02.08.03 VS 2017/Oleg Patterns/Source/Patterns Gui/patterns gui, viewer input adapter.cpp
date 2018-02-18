#include "stdafx.h"

#include "patterns gui, viewer input adapter.h"
#include "patterns, conductor.h"

using namespace patterns;

viewer_input_adapter::viewer_input_adapter(void)
{
}

string viewer_input_adapter::load_samples(void)
{
#if !defined(patterns_viewer) && !defined(OEM_patterns)
	for (int i = 0; i < samples::get_number(); ++i)
	{
		string id = samples::get_name(i);	
		if (get_patient(id) != 0)
		{
			// Already a patient with that same ID
			continue;
		}

		patient *p = new patient();

		p->set_key(id);
		p->set_name("Jane Doe " + id);
		p->set_number_of_fetuses(1);
		p->set_edc(fetus::convert_to_utc(CTime::GetCurrentTime().GetTime()));
		p->set_gestational_age("40+0");
		p->set_id(id);
		p->set_bed("Sample");

		m_patients.insert(m_patients.end(), p);
		
		// Make sure the fetus is created
		get_conductor().update_fetuses();
		
		// Fetch data in the fetus
		if (!get_conductor().is_known(id))
			continue;

		fetus &f = get_conductor().get(id);
		samples::create(&f, id);
		f.set_key(id);

		f.set_as_real_time(true);
		f.compute_now();
	}

	if (samples::get_number() > 0)
		return samples::get_name(0);
#endif
	return "";
}

viewer_input_adapter::~viewer_input_adapter(void)
{
	for (vector<patient*>::iterator itr = m_patients.begin(); itr != m_patients.end(); ++itr)
	{
		delete (*itr);
	}
	m_patients.clear();
}

long viewer_input_adapter::get_number_of_patients(void) const
{
	return m_patients.size();
}

patterns::input_adapter::patient* viewer_input_adapter::get_patient(const string &key) const
{
	for (vector<patient*>::const_iterator itr = m_patients.begin(); itr != m_patients.end(); ++itr)
	{
		if ((*itr)->get_key() == key)
		{
			return *itr;
		}
	}
	return 0;
}

/*
=======================================================================================================================
Return all known patients
=======================================================================================================================
*/
const vector<input_adapter::patient*> viewer_input_adapter::get_patients() const
{
	return m_patients;
}

// Inject the given patient in the list

void viewer_input_adapter::add_patient(patient* p, patterns::fetus* f)
{
	// Make sure the key is unique
	string id = p->get_key();
	int retry = 1;
	while (get_patient(id) != 0)
	{
		char t[15];
		sprintf(t, "%ld", retry);
		++retry;

		id = p->get_key() + " (" + (string)t + ")";
	}
	p->set_key(id);
	f->set_key(id);
	
	// Add the patient
	m_patients.insert(m_patients.end(), p);

	// Make sure the fetus is created
	get_conductor().update_fetuses();
		
	// Fetch data in the fetus
	fetus &fc = get_conductor().get(id);
	fc = *f;
}
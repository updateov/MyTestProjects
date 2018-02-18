#include "stdafx.h"

#include "patterns gui, cri viewer input adapter.h"
#include "patterns, criconductor.h"

using namespace patterns;

CRIViewerInputAdapter::CRIViewerInputAdapter(void)
{
}

string CRIViewerInputAdapter::load_samples(void)
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
		p->set_edc(CRIFetus::convert_to_utc(CTime::GetCurrentTime().GetTime()));
		p->set_gestational_age("40+0");
		p->set_id(id);
		p->set_bed("Sample");

		m_patients.insert(m_patients.end(), p);
		
		// Make sure the fetus is created
		GetConductor().update_fetuses();
		
		// Fetch data in the fetus
		if (!GetConductor().is_known(id))
			continue;

		CRIFetus &f = GetConductor().GetFetus(id);
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

CRIViewerInputAdapter::~CRIViewerInputAdapter(void)
{
	for (vector<patient*>::iterator itr = m_patients.begin(); itr != m_patients.end(); ++itr)
	{
		delete (*itr);
	}
	m_patients.clear();
}

long CRIViewerInputAdapter::get_number_of_patients(void) const
{
	return m_patients.size();
}

patterns::CRIInputAdapter::patient* CRIViewerInputAdapter::get_patient(const string &key) const
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
const vector<CRIInputAdapter::patient*> CRIViewerInputAdapter::get_patients() const
{
	return m_patients;
}

// Inject the given patient in the list

void CRIViewerInputAdapter::add_patient(patient* p, patterns::CRIFetus* f)
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
	GetConductor().update_fetuses();
		
	// Fetch data in the fetus
	CRIFetus &fc = GetConductor().GetFetus(id);
	fc = *f;
}
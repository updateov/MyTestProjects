#pragma once

#include "patterns, input adapter.h"

#if !defined(patterns_viewer) && !defined(OEM_patterns)
#include "patterns, samples.h"
#endif

class viewer_input_adapter : public patterns::input_adapter
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		viewer_input_adapter(void);
		virtual ~viewer_input_adapter(void);

		virtual long get_number_of_patients(void) const;
		virtual const vector<patient*> get_patients() const;
		virtual patient* get_patient(const string &) const;

		string load_samples();
		void add_patient(patient* p, patterns::fetus* f);

	private:
		vector<patient*> m_patients;
};

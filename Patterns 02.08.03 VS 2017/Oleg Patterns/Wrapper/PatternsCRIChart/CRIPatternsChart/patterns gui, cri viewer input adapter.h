#pragma once

#include "patterns, cri input adapter.h"

#if !defined(patterns_viewer) && !defined(OEM_patterns)
#include "patterns, samples.h"
#endif

class CRIViewerInputAdapter : public patterns::CRIInputAdapter
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		CRIViewerInputAdapter(void);
		virtual ~CRIViewerInputAdapter(void);

		virtual long get_number_of_patients(void) const;
		virtual const vector<patient*> get_patients() const;
		virtual patient* get_patient(const string &) const;

		string load_samples();
		void add_patient(patient* p, patterns::CRIFetus* f);

	private:
		vector<patient*> m_patients;
};

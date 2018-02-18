#include "stdafx.h"
#include "patterns, cri input adapter.h"

#include "patterns, criconductor.h"

using namespace patterns;

/*
=======================================================================================================================
Constructor and destructor.
=======================================================================================================================
*/
CRIInputAdapter::CRIInputAdapter(void)
{
}

/*
=======================================================================================================================
=======================================================================================================================
*/
CRIInputAdapter::~CRIInputAdapter(void)
{
}


/*
=======================================================================================================================
Access the associated conductor. If there is no such conductor, we still return a valid object to reduce crash
risks. We do not care about destroying the dump object as this is an exception situation and as the number of
instances is limited to one.
=======================================================================================================================
*/
const CRIConductor &CRIInputAdapter::GetConductor(void) const
{
	if (!m_pConductor && !d)
	{
		d = new CRIConductor;
	}

	return m_pConductor ? *dynamic_cast<CRIConductor*>(m_pConductor) : *dynamic_cast<CRIConductor*>(d);
}

/*
=======================================================================================================================
=======================================================================================================================
*/
CRIConductor &CRIInputAdapter::GetConductor(void)
{
	if (!m_pConductor && !d)
	{
		d = new CRIConductor;
	}

	return m_pConductor ? *dynamic_cast<CRIConductor*>(m_pConductor) : *dynamic_cast<CRIConductor*>(d);
}




/*
 =======================================================================================================================
    load event`s observations and append them to the given fetus.
 =======================================================================================================================
 */
bool CRIInputAdapter::load_saved_data(patterns::CRIFetus &curFetus, string filename)
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
bool CRIInputAdapter::load_saved_data(patterns::CRIFetus &curFetus, vector<string>& lines, long timeadjustment)
{
	contraction curContraction;
	event curEvent;
	Contractility curContractility;

	vector<contraction> contractions;
	vector<event> events;
	vector<Contractility> contractilities;

	vector<long> strctr;
	vector<long> strevt;
	vector<long> acpevt;

	for (vector<string>::iterator itr = lines.begin(); itr != lines.end(); ++itr)
	{
		string line = *itr;

		if (line.compare(0, 4, "CTR|") == 0)
		{
			// Contractions
			if (CRIConductor::to_contraction(curContraction, line.substr(4), '|'))
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
			if (CRIConductor::to_event(curEvent, line.substr(4), '|'))
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
		else if (line.compare(0, 4, "CRI|") == 0)
		{
			// Events
			if (CRIConductor::ToContractility(curContractility, line.substr(4), '|'))
			{
				if (timeadjustment != 0)
				{
					curContractility -= timeadjustment;
				}
				contractilities.push_back(curContractility);
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
			CRIConductor::to_vector(line, message, '|');
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
			CRIConductor::to_vector(line, message, '|');
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
			CRIConductor::to_vector(line, message, '|');
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
	curFetus.AppendContractilities(contractilities);

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

	return (curFetus.GetEventsCount() > 0) || (curFetus.GetContractionsCount() > 0) || (curFetus.GetContractilitiesCount() > 0);
}

conductor* CRIInputAdapter::NewConductor() const
{
	return new CRIConductor;
}
#include "stdafx.h"

#include "ParerClassifier.h"

#include "patterns, fetus.h"

using namespace patterns;
using namespace patterns_classifier;

bool is_valid(const event& e)
{
	return !e.is_strike_out() && e.is_final() && (!e.is_noninterp() || e.is_confirmed());
}

bool is_valid(const contraction& c)
{
	return !c.is_strike_out() && c.is_final();
}

bool isInWindow(const event& e, long t1, long t2)
{
	return (e.get_end() > t1) && (e.get_end() <= t2);
}

bool isInWindow(const contraction& c, long t1, long t2)
{
	return (c.get_end() > t1) && (c.get_end() <= t2);
}


/*
=======================================================================================================================
=======================================================================================================================
*/

CParerClassifier::CParerClassifier(void)
{
	setDefParams();
	m_pFetus = NULL;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
CParerClassifier::CParerClassifier(fetus *pf)
{
	setDefParams();
	m_pFetus = pf;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
CParerClassifier::~CParerClassifier(void)
{
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::setDefParams(void)
{
	m_dUpdatePeriodMinutes = 1.0;
	m_dSampFreq = 4.0;
	m_bBatch = false;
	m_dDecelWindowMinutes = 20.0;
	m_dBasWindowMinutes = 10.0;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::setBatch(bool b)
{
	m_bBatch = b;
	if (b)
	{
		setRT(false);
	}
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::setRT(bool b)
{
	m_bRT = b;
	if (b)
	{
		setBatch(false);
	}
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::getUpdatePeriod(void)
{
	return (long) (60.0 * m_dUpdatePeriodMinutes * m_dSampFreq);
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::setSampFreq(double f)
{
	m_dSampFreq = f;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::setUpdatePeriodMinutes(double p)
{
	m_dUpdatePeriodMinutes = p;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::setFetus(fetus *f)
{
	reset();
	m_pFetus = f;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::reset(void)
{
	m_overallClass.reset();
	m_basClass.reset();
	m_varClass.reset();
	m_lateClass.reset();
	m_varDecelClass.reset();
	m_prolDecelClass.reset();
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::classify(void)
{
	if (m_pFetus)
	{
		if (m_bBatch)
		{
			batchClassify();
		}
		else if (m_bRT)
		{
			rtClassify();
		}
	}
}

//
// =======================================================================================================================
//    reset and redo classification from start ;
//    void CParerClassifier:::batchClassify() { long tStart = 0, tEnd = m_pFetus->get_number_of_fhr();
//    long tUpdate = getUpdatePeriod();
//    reset();
//    for (long t = tStart;
//    t < tEnd;
//    t+=tUpdate) getOverallClassAtTime(t);
//    }
// =======================================================================================================================
//
void CParerClassifier::batchClassify(void)
{
	// Flush all
	reset();

	// Refresh the classification every x minutes (getUpdatePeriod) and at the end of every event
	long tUpdate = getUpdatePeriod();
	long current = tUpdate;
	long end = m_pFetus->get_number_of_fhr();

	long n = m_pFetus->GetEventsCount();
	for (long i = 0; i < n; i++)
	{
		const event &e = m_pFetus->get_event(i);
		if (!is_valid(e))
			continue;

		long t = e.get_end() + 1;

		while (current < t)
		{
			getOverallClassAtTime(current, 1);
			current += tUpdate;
		}

		getOverallClassAtTime(t, 1);
	}

	while (current < m_pFetus->get_number_of_fhr())
	{
		getOverallClassAtTime(current, 1);
		current += tUpdate;
	}
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::rtClassify(void)
{
	/*~~~~~~~~~~~~*/
	long tStart = 0;
	long tEnd = 0;
	/*~~~~~~~~~~~~*/

	tEnd = getLastEventEnd() + 1;

	if (tEnd > 0)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long tUpdate = getUpdatePeriod();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		tStart = m_overallClass.getLastTime();
		if (tStart == -1)
		{
			tStart = 0;
		}

		tStart += tUpdate;

		for (long t = tStart; t <= tEnd; t += tUpdate)
		{
			getOverallClassAtTime(t, 1);
		}

		getOverallClassAtTime(tEnd, 1); // also get most current update
	}
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::getLastEventEnd(void)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long tEnd = -1;
	long maxT = -1;
	long n = m_pFetus->GetEventsCount();
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (n > 0)
	{
		for (long i = n - 1; i >= max(0, n - 3); i--)	// look back 3 events because can be out of order
		{
			const event &e = m_pFetus->get_event(i);
			if (!is_valid(e))
				continue;

			tEnd = e.get_end();
			if (tEnd > maxT)
			{
				maxT = tEnd;
			}
		}
	}

	return maxT;
}

/*
=======================================================================================================================
Baseline
=======================================================================================================================
*/
bool CParerClassifier::calcBasClassAtTime(long t, long *basClass, long *varClass)
{
	/*~~~~~~~~~~~~~*/
	long b = t_undef;
	long v = t_undef;
	bool bRC = false;
	double meanBas;
	double meanVar;
	long x1;
	/*~~~~~~~~~~~~~*/

	if (m_pFetus)
	{
		x1 = t - (long) (60.0 * m_dBasWindowMinutes * m_dSampFreq) + 1;
		m_pFetus->get_mean_baseline(x1, t, &meanBas, &meanVar);
		if (meanBas > 0)	// valid
		{
			if (meanBas <= 70)
			{
				b = t_severe_brady;
			}
			else if (meanBas < 80)
			{
				b = t_moderate_brady;
			}
			else if (meanBas < 110)
			{
				b = t_mild_brady;
			}
			else if (meanBas < 160)
			{
				b = t_normal_bas;
			}
			else
			{
				b = t_tachy;
			}
		}

		if (meanVar > 0)	// valid
		{
			if (meanVar < 3)
			{
				v = t_absent;
			}
			else if (meanVar < 5)
			{
				v = t_minimal;
			}
			else if (meanVar < 25)
			{
				v = t_normal_var;
			}
			else
			{
				v = t_marked;
			}

			bRC = true;
		}
	}

	(*basClass) = b;
	(*varClass) = v;

	return bRC;
}

/*
=======================================================================================================================
calcBasLevelClassAtTime::if going to calculate basVar level as well, it is more efficient to call
calcBasClassAtTime
=======================================================================================================================
*/
long CParerClassifier::calcBasLevelClassAtTime(long t)
{
	/*~~~*/
	long b;
	long v;
	/*~~~*/

	calcBasClassAtTime(t, &b, &v);

	return b;
}

/*
=======================================================================================================================
calcBasVarClassAtTime::if going to calculate basLevel level as well, it is more efficient to call
calcBasClassAtTime
=======================================================================================================================
*/
long CParerClassifier::calcBasVarClassAtTime(long t)
{
	/*~~~*/
	long b;
	long v;
	/*~~~*/

	calcBasClassAtTime(t, &b, &v);

	return v;
}

/*
=======================================================================================================================
Late decels
=======================================================================================================================
*/
long CParerClassifier::calcLateDecelClassAtTime(long t)
{
	long t1 = t - ((long) (60.0 * m_dSampFreq * m_dDecelWindowMinutes)) + 1;

	vector<event> e = getLateDecelInWindow(m_pFetus, t1, t);
	vector<contraction> c = contractionsInWindow(m_pFetus, t1, t);

	long classOut = t_undef;

	if (((2 * e.size()) < c.size()) || (e.size() <= 1))
	{
		return t_none;
	}

	double totH = 0.0;
	for (long i = 0; i < (long) e.size(); i++)
	{
		totH += e[i].get_height();
	}

	double meanH = (double) (totH / e.size());
	if (meanH > 45)
	{
		return t_severe;
	}
	if (meanH > 15)
	{
		return t_moderate;
	}
	return t_mild;
}

/*
=======================================================================================================================
Variable Decels
=======================================================================================================================
*/
long CParerClassifier::calcVariableDecelClassAtTime(long t)
{
	long t1 = t - ((long) (60.0 * m_dSampFreq * m_dDecelWindowMinutes)) + 1;

	vector<event> e = getVariableDecelInWindow(m_pFetus, t1, t);
	vector<contraction> c = contractionsInWindow(m_pFetus, t1, t);

	if (((2 * e.size()) < c.size()) || (e.size() <= 1))
		return t_none;

	bool isModerate = false;

	for (long i = 0; i < (long) e.size(); i++)
	{
		long len = e[i].get_length();
		double peak = e[i].get_peak_val();

		if ((len >= (120 * m_dSampFreq)) && (peak < 80))
		{
			return t_severe;
		}
		if ((len >= (60 * m_dSampFreq)) && (peak < 70))
		{
			return t_severe;
		}

		if (isModerate)
		{
			continue;
		}

		if ((len >= (60 * m_dSampFreq)) && (peak < 80))
		{
			isModerate = true;
		}
		else if ((len >= (30 * m_dSampFreq)) && (peak < 70))
		{
			isModerate = true;
		}
	}

	if (isModerate)
	{
		return t_moderate;
	}
	return t_mild;	
}

/*
=======================================================================================================================
Prolonged Decels
=======================================================================================================================
*/
long CParerClassifier::calcProlongedDecelClassAtTime(long t)
{
	long t1 = t - ((long) (60.0 * m_dSampFreq * m_dDecelWindowMinutes)) + 1;
	vector<event> e = getProlongedDecelInWindow(m_pFetus, t1, t);

	if (e.size() <= 0)
	{
		return t_none;
	}
	
	bool isModerate = false;

	for (long i = 0; i < (long) e.size(); i++)
	{
		double peak = e[i].get_peak_val();
		if (peak < 70)
		{
			return t_severe;
		}

		if (peak < 80)
		{
			isModerate = true;
		}
	}

	if (isModerate)
	{
		return t_moderate;
	}
	return t_mild;
}


/*
=======================================================================================================================
=======================================================================================================================
*/
vector<event> CParerClassifier::getLateDecelInWindow(fetus *pFetus, long t1, long t2)
{
	vector<event> eOut;

	long n = pFetus->GetEventsCount();
	for (long i = 0; i < n; i++)
	{
		const event &curEvent = pFetus->get_event(i);
		if (!is_valid(curEvent) || !curEvent.is_late() || !curEvent.is_deceleration() || !isInWindow(curEvent, t1, t2))
			continue;

		eOut.push_back(curEvent);
	}

	return eOut;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
vector<event> CParerClassifier::getVariableDecelInWindow(fetus *pFetus, long t1, long t2)
{
	vector<event> eOut;

	long n = pFetus->GetEventsCount();
	for (long i = 0; i < n; i++)
	{
		const event &curEvent = pFetus->get_event(i);
		if (!is_valid(curEvent) || !curEvent.is_variable() || !curEvent.is_deceleration() || !isInWindow(curEvent, t1, t2))
			continue;

		eOut.push_back(curEvent);
	}

	return eOut;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
vector<event> CParerClassifier::getProlongedDecelInWindow(fetus *pFetus, long t1, long t2)
{
	vector<event> eOut;

	long n = pFetus->GetEventsCount();
	for (long i = 0; i < n; i++)
	{
		const event &curEvent = pFetus->get_event(i);
		if (!is_valid(curEvent) || (curEvent.get_type() != event::tprolonged) || !isInWindow(curEvent, t1, t2))
			continue;

		eOut.push_back(curEvent);
	}

	return eOut;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
vector<contraction> CParerClassifier::contractionsInWindow(fetus *pFetus, long t1, long t2)
{
	vector<contraction> cOut;

	if (m_pFetus->get_hr_sample_rate() != m_pFetus->get_up_sample_rate())	// up at different sampling frequency
	{
		t1 = (long) ((t1 * m_pFetus->get_up_sample_rate()) / m_pFetus->get_hr_sample_rate());
		t2 = (long) ((t2 * m_pFetus->get_up_sample_rate()) / m_pFetus->get_hr_sample_rate());
	}

	for (long i = pFetus->GetContractionsCount(); i >= 0; i--)
	{
		const contraction &curContraction = pFetus->get_contraction(i);
		if (!isInWindow(curContraction, t1, t2))
			continue;

		if (!is_valid(curContraction))
			continue;

		cOut.push_back(curContraction);
	}

	return cOut;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::calcOverallClassAtTime(long t)
{
	return calcOverallClassAtTime(t, getUpdatePeriod());
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::calcOverallClassAtTime(long t, long p)
{
#define G t_green
#define B t_blue
#define Y t_yellow
#define O t_orange
#define R t_red

static int classification_color_table[4][5][3][4] = 
{
		/*									Decels variable			Decels late				Decels prolonged	*/	
		/*								None Mild Mod Severe	None Mild Mod Severe	None Mild Mod Severe	*/
	{	/* variability normal		*/			
		/* baseline tachy			*/	{	{B, B, Y, O},			{B, Y, Y, O},			{B, Y, Y, O}		},
		/* baseline normal			*/	{	{G, G, B, Y},			{G, B, Y, Y},			{G, Y, Y, O}		},
		/* baseline mild brady		*/	{	{Y, Y, Y, O},			{Y, Y, Y, O},			{Y, Y, Y, O}		},
		/* baseline moderate brady	*/	{	{Y, Y, O, O},			{Y, O, O, O},			{O, O, O, O}		},
		/* baseline severe brady	*/	{	{O, O, O, O},			{O, O, O, O},			{O, O, O, O}		},
	},
	{	/* variability minimal		*/
		/* baseline tachy			*/	{	{B, Y, O, O},			{B, O, O, R},			{B, 0, 0, R}		},
		/* baseline normal			*/	{	{B, Y, O, O},			{B, O, O, R},			{B, 0, 0, R}		},
		/* baseline mild brady		*/	{	{O, R, R, R},			{O, R, R, R},			{O, R, R, R}		},
		/* baseline moderate brady	*/	{	{O, R, R, R},			{O, R, R, R},			{O, R, R, R}		},
		/* baseline severe brady	*/	{	{R, R, R, R},			{R, R, R, R},			{R, R, R, R}		},
	},
	{	/* variability absent		*/
		/* baseline tachy			*/	{	{R, R, R, R},			{R, R, R, R},			{R, R, R, R}		},
		/* baseline normal			*/	{	{O, R, R, R},			{O, R, R, R},			{O, R, R, R}		},
		/* baseline mild brady		*/	{	{R, R, R, R},			{R, R, R, R},			{R, R, R, R}		},
		/* baseline moderate brady	*/	{	{R, R, R, R},			{R, R, R, R},			{R, R, R, R}		},
		/* baseline severe brady	*/	{	{R, R, R, R},			{R, R, R, R},			{R, R, R, R}		},
	},
	{	/* variability marked		*/
		/* baseline tachy			*/	{	{Y, Y, Y, Y},			{Y, Y, Y, Y},			{Y, Y, Y, Y},		},
		/* baseline normal			*/	{	{Y, Y, Y, Y},			{Y, Y, Y, Y},			{Y, Y, Y, Y},		},
		/* baseline mild brady		*/	{	{Y, Y, Y, Y},			{Y, Y, Y, Y},			{Y, Y, Y, Y},		},
		/* baseline moderate brady	*/	{	{Y, Y, Y, Y},			{Y, Y, Y, Y},			{Y, Y, Y, Y},		},
		/* baseline severe brady	*/	{	{Y, Y, Y, Y},			{Y, Y, Y, Y},			{Y, Y, Y, Y},		},
	},
};

#undef G
#undef B
#undef Y
#undef O
#undef R

	long basLevelClass;
	long basVarClass;

	getBasClassAtTime(t, &basLevelClass, &basVarClass, p);
	if (basLevelClass == t_undef)
	{
		// don't have two minutes of baseline in last 10 minutes
		getLastValidBasClass(t - getUpdatePeriod(), &basLevelClass, &basVarClass);
	}
	if (basVarClass == t_undef)
	{
		// in cases where you have valid baseline but not valid var
		getLastValidBasVarClass(t - getUpdatePeriod(), &basVarClass);
	}

	long late = getLateClassAtTime(t, p);
	long variable = getVariableClassAtTime(t, p);
	long prolonged = getProlongedClassAtTime(t, p);

	// Map undefined values to the most conservative value
	if (basVarClass < 0)
	{
		basVarClass = t_normal_var;
	}
	if (basLevelClass < 0)
	{
		basLevelClass = t_normal_bas;
	}
	if (late < 0)
	{
		late = t_none;
	}
	if (prolonged < 0)
	{
		late = t_none;
	}
	if (variable < 0)
	{
		late = t_none;
	}

	int level_variable = classification_color_table[(int)basVarClass][(int)basLevelClass][0][(int)variable];
	int level_late = classification_color_table[(int)basVarClass][(int)basLevelClass][1][(int)late];
	int level_prolonged = classification_color_table[(int)basVarClass][(int)basLevelClass][2][(int)prolonged];
	
	return max(level_late, max(level_prolonged, level_variable));
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::getOverallClassAtTime(long t)
{
	return getOverallClassAtTime(t, getUpdatePeriod());
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::getOverallClassAtTime(long t, long p)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long overallClass = m_overallClass.classAtTime(t, p);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (overallClass == CClassification::t_notcalc)
	{
		overallClass = calcOverallClassAtTime(t, p);
		m_overallClass.setClassAtTime(t, overallClass);
	}

	return overallClass;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::getLateClassAtTime(long t, long p)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long lateClass = m_lateClass.classAtTime(t, p);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (lateClass == CClassification::t_notcalc)
	{
		lateClass = calcLateDecelClassAtTime(t);
		m_lateClass.setClassAtTime(t, lateClass);
	}

	return lateClass;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::getVariableClassAtTime(long t, long p)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long variableClass = m_varDecelClass.classAtTime(t, p);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (variableClass == CClassification::t_notcalc)
	{
		variableClass = calcVariableDecelClassAtTime(t);
		m_varDecelClass.setClassAtTime(t, variableClass);
	}

	return variableClass;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
long CParerClassifier::getProlongedClassAtTime(long t, long p)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	long prolClass = m_prolDecelClass.classAtTime(t, p);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if (prolClass == CClassification::t_notcalc)
	{
		prolClass = calcProlongedDecelClassAtTime(t);
		m_prolDecelClass.setClassAtTime(t, prolClass);
	}

	return prolClass;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::getBasClassAtTime(long t, long *basLevelClass, long *basVarClass, long p)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	// long b = m_basClass.classAtTime(t, getUpdatePeriod());
	// long v = m_varClass.classAtTime(t, getUpdatePeriod());
	long b = m_basClass.classAtTime(t, p);
	long v = m_varClass.classAtTime(t, p);
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

	if ((b == CClassification::t_notcalc) || (v == CClassification::t_notcalc))
	{
		calcBasClassAtTime(t, basLevelClass, basVarClass);
		m_basClass.setClassAtTime(t, *basLevelClass);
		m_varClass.setClassAtTime(t, *basVarClass);
	}
	else
	{
		(*basLevelClass) = b;
		(*basVarClass) = v;
	}
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::getLastValidBasClass(long t, long *basLevelClass, long *basVarClass)
{
	/*~~~~~~~~~~~~~*/
	long b = t_undef;
	long v = t_undef;
	/*~~~~~~~~~~~~~*/

	while ((t > 0) && (b == t_undef))
	{
		getBasClassAtTime(t, &b, &v, getUpdatePeriod());
		t -= getUpdatePeriod();
	}

	(*basLevelClass) = b;
	(*basVarClass) = v;
}

/*
=======================================================================================================================
=======================================================================================================================
*/
void CParerClassifier::getLastValidBasVarClass(long t, long *basVarClass)
{
	/*~~~~~~~~~~~~~*/
	long v = t_undef;
	long b;
	/*~~~~~~~~~~~~~*/

	while ((t > 0) && (v == t_undef))
	{
		getBasClassAtTime(t, &b, &v, getUpdatePeriod());
		t -= getUpdatePeriod();
	}

	(*basVarClass) = v;
}

/*
=======================================================================================================================
for client - do not recalc - just get last classification given time
=======================================================================================================================
*/
long CParerClassifier::getLastOverallAtTime(long t)
{
	/*~~~~~~~~~~~~~*/
	long c = t_undef;
	/*~~~~~~~~~~~~~*/

	if (m_pFetus)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long tMax = m_pFetus->get_number_of_fhr();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		c = m_overallClass.classAtTime(t, tMax);
	}

	return c;
}

/*
=======================================================================================================================
for client - do not recalc - just get last classification given time
=======================================================================================================================
*/
long CParerClassifier::getLastClassAtTime(long t, long ctype)
{
	/*~~~~~~~~~~~~~*/
	long c = t_undef;
	/*~~~~~~~~~~~~~*/

	if (m_pFetus)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long tMax = m_pFetus->get_number_of_fhr();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		switch (ctype)
		{
		case t_late:
			c = m_lateClass.classAtTime(t, tMax);
			break;

		case t_variable:
			c = m_varDecelClass.classAtTime(t, tMax);
			break;

		case t_prolonged:
			c = m_prolDecelClass.classAtTime(t, tMax);
			break;

		case t_basLevel:
			c = m_basClass.classAtTime(t, tMax);
			break;

		case t_basVar:
			c = m_varClass.classAtTime(t, tMax);
			break;
		}
	}

	return c;
}

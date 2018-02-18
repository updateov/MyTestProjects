#pragma once

#include "Classification.h"
#include "patterns, event.h"
#include "patterns, contraction.h"

#include <vector>

namespace patterns
{
class fetus;
}

using namespace std;
using namespace patterns;

namespace patterns_classifier
{
	class CParerClassifier
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum classLevel { t_undef = -1, t_green = 0, t_blue = 1, t_yellow = 2, t_orange = 3, t_red = 4 };
		enum varLevel { t_normal_var = 0, t_minimal = 1, t_absent = 2, t_marked = 3 };
		enum basLevel { t_tachy = 0, t_normal_bas = 1, t_mild_brady = 2, t_moderate_brady = 3, t_severe_brady = 4};
		enum decelLevel { t_none = 0, t_mild = 1, t_moderate = 2, t_severe = 3 };
		enum classType { t_late = 0, t_variable = 1, t_prolonged = 2, t_basLevel = 3, t_basVar = 4 };

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		fetus *m_pFetus;

		// vector <contraction> contractions;
		// vector <event> events;
		CClassification m_overallClass;
		CClassification m_basClass;
		CClassification m_varClass;
		CClassification m_lateClass;
		CClassification m_varDecelClass;
		CClassification m_prolDecelClass;
		vector<event> lateDecels;			// only for batch mode
		vector<event> varDecels;			// only for batch mode
		vector<event> prolDecels;			// only for batch mode
		vector<contraction> contractions;	// only for batch mode

		double m_dUpdatePeriodMinutes;
		double m_dSampFreq;
		bool m_bBatch;
		bool m_bRT;
		double m_dDecelWindowMinutes;
		double m_dBasWindowMinutes;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CParerClassifier(void);

		// CParerClassifier (vector <event>) ;
		CParerClassifier(fetus *);
		virtual ~CParerClassifier(void);

		// note that RT mode sets Batch to false but you can run in non-batch, non-RT
		// (e.g. if are browsing tracing)
		void setBatch(bool = true); // this will classify entire tracing at once
		void setRT(bool = true);	// this assumes sequential classification
		long getUpdatePeriod(void);
		void setSampFreq(double);
		void setUpdatePeriodMinutes(double);
		void setFetus(fetus *);
		void classify(void);		// this to do batch or RT up to current time
		void reset(void);			// reset classification

		long getLastOverallAtTime(long);
		long getLastClassAtTime(long, long);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		void setDefParams(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	private:
		void batchClassify(void);
		void rtClassify(void);
		long getLastEventEnd(void);

		long getLateClassAtTime(long, long);
		long getVariableClassAtTime(long, long);
		long getProlongedClassAtTime(long, long);

		// long getBasLevelClassAtTime(long);
		// long getBasVarClassAtTime(long);
		void getBasClassAtTime(long, long *, long *, long);
		void getLastValidBasClass(long, long *, long *);
		void getLastValidBasVarClass(long, long *);
		long getOverallClassAtTime(long);				// use default update period
		long getOverallClassAtTime(long, long);			// specified update period (in case want classification at exact time)

		long calcLateDecelClassAtTime(long);
		long calcVariableDecelClassAtTime(long);
		long calcProlongedDecelClassAtTime(long);
		bool calcBasClassAtTime(long, long *, long *);	// more efficient to do bas var and level at same time
		long calcBasLevelClassAtTime(long);
		long calcBasVarClassAtTime(long);
		long calcOverallClassAtTime(long);
		long calcOverallClassAtTime(long, long);

		vector<event> getLateDecelInWindow(fetus *, long, long);
		vector<event> getVariableDecelInWindow(fetus *, long, long);
		vector<event> getProlongedDecelInWindow(fetus *, long, long);

		vector<contraction> contractionsInWindow(fetus *, long, long);

	};
}
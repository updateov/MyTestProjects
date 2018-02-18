//REVIEW: 27/03/14
#pragma once

#include "patterns, fetus.h"
#include "patterns, subscription.h"
#include <string>
#include <vector>
#include <map>

#ifdef patterns_parer_classification
#include "../Parer/ParerClassifier.h"
#endif

using namespace std;

namespace patterns
{
	/*
	* Date type. This is strictly a reference point for a fetus instance, populated
	* from input-adapter implementations. It is meant to be replaced as needed in the
	* future by a date class with full time-zone and operator implementation. This
	* first implementation however, is simply the signed number of seconds since the
	* Unix Epoch, 1 January 1970. Conversions to and from native Windows types are
	* found in the patterns_gui::services class.
	*/

	const long CRI_CONTRACTILITY_WINDOW_SIZE = 30; 

	const int CRI_STATE_QUALIFICATION_WINDOW_SIZE = 1800;
	const int CRI_MINIMAL_AMOUNT_OF_DATA_IN_WINDOW = 75;

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	class CRIFetus : public fetus
	{
		friend class CRIConductor;

	protected:

		CRICalculation m_CRICalculation;


		//void set_conductor(CRIConductor *);

		static long s_contractionRateWindowSize;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		CRIFetus(void);
		CRIFetus(const CRIFetus&);
		virtual ~CRIFetus(void);


		long GetContractilitiesCount(void) const;

		const vector<Contractility>& GetContractilities();
		const Contractility& GetContractilityByIndex(long index) const;
		void AppendContractilities(const vector<Contractility>&);
		void MergeContractilities(const vector<Contractility>& contractilities);
		bool ContractilitiesEqual(const CRIFetus&, bool = false) const;
		Contractility::ContractilityClassification GetContractilityClassification(long index) const;

		const CRIConductor &get_conductor(void) const;
		CRIConductor &get_conductor(void);

		static long GetContractionRateWindowSize() {return s_contractionRateWindowSize;}
		static void SetContractionRateWindowSize(int value) {s_contractionRateWindowSize = value;}

		static bool get_event_detection_enabled() {return event_detection_enabled;}
		static void set_event_detection_enabled(bool value) {event_detection_enabled = value;}

		virtual vector<long> GetContractionsRate() const;
		virtual vector<long> GetContractionsRateIndex() const;

		virtual long GetContractionRate(long) const;
		virtual void ComputeContractionRateNow(void);

		virtual void get_mean_baseline(long, long, double *, double *) const;



		CRIFetus &operator =(const CRIFetus &);
		bool operator ==(const CRIFetus &) const;
		bool operator !=(const CRIFetus &) const;

		friend class conductor;


		static int s_qualificationWindowSize;
		static int s_minimalAmountOfDataInWindow;

		static int GetQualificationWindowSize(){return s_qualificationWindowSize;}
		static void SetQualificationWindowSize(int value){s_qualificationWindowSize = value;}
		static int GetMinimalAmountOfDataInWindow(){return s_minimalAmountOfDataInWindow;}
		static void SetMinimalAmountOfDataInWindow(int value){ s_minimalAmountOfDataInWindow = value;}
private:
		bool SufficientTracingDurationInQualificationWindow(long iRight) const;
		long ValidFHRAmountInQualificationWindow(long iRight) const;
		long ValidUPAmountInQualificationWindow(long iRight) const;
		bool CheckTraceRatio(double traceDuration, long qualificationWndSize) const;
		static bool IsFHRValid(long fhr);
		static bool IsUPValid(long up);
	};
}
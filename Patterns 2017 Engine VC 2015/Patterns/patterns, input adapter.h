#ifndef patterns_input_adapter_class
#define patterns_input_adapter_class

namespace patterns
{
class conductor;
}

#include "patterns, subscription.h"
#include "patterns, fetus.h"
#include <string>


namespace patterns
{
using namespace std;
class input_adapter;
}

/*
 =======================================================================================================================
    Incoming data adapter for the Patterns component. The input adapter is meant to be derived from by client
    components. Instances of subclasses should be provided to the conductor class. The Patterns component will access
    data through these instances. Implementations are expected to push data to the fetus instances by accessing them
    through calls to get_conductor (). get (i), where i is a unique identifier. Identifiers are opaque and subclasses
    may use and define them as they see fit, the only requirement being that there be a one-to-one relationship between
    fetuses and identifiers. The patient class is a somewhat inaptly named
    one, as it will most probably end up encapsulating data about a visit or pregnancy. It is self-documenting. See
    methods get_number_of_patients (), get_patient () and is_known_patient (). Implementation classes must guarantee
    that patients in the list of patients will not change between notifications of patient-list change. See conductor
    class. c: pointer to the conductor we are attached to. We do not own the conductor. As the set_conductor () method
    is protected, we can expect that, if we do have an associated conductor, then it owns and will destroy us before
    being destroyed. Therefore, c being valid if non-null may be taken as an invariant. d: dump object to return when
    no object is available, see methods get_conductor () and set_conductor ().
 =======================================================================================================================
 */
class patterns::input_adapter
{
		friend class conductor;

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:
		conductor* m_pConductor;
		mutable conductor *d;

		virtual const conductor &get_conductor(void) const;
		virtual conductor &get_conductor(void);
		virtual bool has_conductor(void) const;

		virtual void note(subscription::message);
		virtual void note(subscription::message, string);
		virtual void note(subscription::message, long);

		virtual void set_conductor(conductor *);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		class patient
		{
		protected:
			string age;
			string bed;
			string dilatation;
			string edc_s;
			string effacement;
			string exam_date_time_s;
			string gestational_age;
			string id;
			string key;
			string name;
			string station;
			string surname;
			string accountno;
			bool is_collecting_tracing;
			bool is_late_collecting_tracing;
			date edc;
			FILETIME dob;
			date exam_date_time;
			long fetuses;

		public:
			patient (void);
			patient (const patient &);
			virtual ~patient(void);

			void reset(void);

			virtual string get_age(void) const;
			virtual string get_accountno(void) const;
			virtual string get_bed(void) const;
			virtual string get_dilatation(void) const;
			virtual date get_edc(void) const;
			virtual FILETIME get_dob(void) const;
			virtual string get_edc_string(void) const;
			virtual string get_effacement(void) const;
			virtual string get_displayed(void) const;
			virtual string get_gestational_age(void) const;
			virtual string get_id(void) const;
			virtual string get_key(void) const;
			virtual date get_last_exam_creation_time(void) const;
			virtual string get_last_exam_creation_time_string(void) const;
			virtual string get_name(void) const;
			virtual long get_number_of_fetuses(void) const;
			virtual string get_station(void) const;
			virtual string get_surname(void) const;
			virtual patient &operator =(const patient &);
			virtual bool operator ==(const patient &) const;
			virtual bool operator !=(const patient &) const;
			virtual void set_accountno(const string &);
			virtual void set_age(const string &);
			virtual void set_bed(const string &);
			virtual void set_dilatation(const string &);
			virtual void set_edc(const date &);
			virtual void set_dob(const FILETIME &);
			virtual void set_edc_string(const string &);
			virtual void set_effacement(const string &);
			virtual void set_gestational_age(const string &);
			virtual void set_id(const string &);
			virtual void set_key(const string &);
			virtual void set_last_exam_creation_time(date);
			virtual void set_last_exam_creation_time_string(const string &);
			virtual void set_name(const string &);
			virtual void set_number_of_fetuses(long);
			virtual void set_station(const string &);
			virtual void set_surname(const string &);

			virtual bool get_is_collecting_tracing(void);
			virtual void set_is_collecting_tracing(bool);

			virtual bool get_is_late_collecting_tracing(void);
			virtual void set_is_late_collecting_tracing(bool);
		};

		enum permission { pview, pprint, pmodify, padmitpatient, pdischargepatient, ptransferpatient, pmodifypatient};

		input_adapter(void);
		virtual~input_adapter(void);

		static bool load_saved_data(fetus &, string);
		static bool load_saved_data(fetus&, vector<string>&, long timeadjustment=0);

		virtual bool compare(patient* p1, patient* p2) const;

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */

		virtual void broadcast(const string &)
		{
		}

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual string get_name(void) const
		{
			return "";
		}

		virtual long get_number_of_patients(void) const = 0;
		virtual const vector<patient*> get_patients() const = 0;
		virtual patient* get_patient(const string &) const = 0;

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual bool is_collecting_tracing(const string &s) const
		{
			patient* p = get_patient(s);
			return (p == 0) ? false : p->get_is_collecting_tracing();
		}

		/*
		 ===============================================================================================================
		 ===============================================================================================================
		 */
		virtual bool is_late_collecting_tracing(const string &s) const
		{
			patient* p = get_patient(s);
			return (p == 0) ? false : p->get_is_late_collecting_tracing();
		}

		virtual conductor* NewConductor() const;
};
#endif

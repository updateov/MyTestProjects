#pragma once

#include "patterns, contraction.h"
#include "patterns, event.h"
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
	typedef time_t date;
	const date undetermined_date = 0;


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	class fetus
	{
		friend class conductor;

	protected:
		string m_key;
		vector<contraction> m_contractions;
		enum constant { kcdrate = 0, kcdleft, kcdright, kcdwait, kcdwindow, kedleft };
		long cdelta;
		long contractions_last_calculated_index;
		mutable vector<long> m_contractionsRate;;
		mutable vector<long> m_contractionsRateIndex;
		mutable long m_contractionsRateLast;

#ifdef patterns_has_signal_processing
		CFHRSignal* m_devents;
#endif

#ifdef patterns_parer_classification
		mutable patterns_classifier::CParerClassifier parerclass;
#endif

		date dcutoff;
		date dreset;
		date dstart;
		static const string dtagname;
		vector<event> m_events;
		long edelta;
		mutable long etotalfinal;
		mutable long ereinjected;

		mutable long notification_suspension_count;
		mutable bool notification_suspended;

#ifdef patterns_parer_classification
		mutable bool iclassification;
#endif

		mutable bool m_bComputeContractionRate;
		long inextfhr;
		long inextcontraction;
		bool irealtime;
		conductor *kconductor;
#ifdef patterns_has_signal_processing
		contraction_detection kcontractiondetector;
#endif
		vector<char> m_up;
		long prate;
		string rtest;
		vector<subscription*> m_subscribers;

		vector<char> m_fhr;
		vector<char> m_mhr;

		long m_fhrSampleRate;

		bool iin_compute_contractions_now;

		vector<char> adjust_sample_rate(const vector<char> &, long, long, long p0 = 0);
		void append_to_event_detector(long);
		vector<char> as_vector(const string &) const;
		string code_cdata(const vector<char> &) const;
		void compute_contractions(void);

		void compute_contractions_now(void);
		void compute_events_now(void);

#ifdef patterns_has_signal_processing
		void SetDEvents(CFHRSignal* devents);
#endif

#ifdef patterns_parer_classification
		void compute_classification_now(void);
#endif

		string decode_cdata(const string &) const;
		void delete_event_detector(void);
		string export_cpp(void) const;
		string export_in(void) const;
		string export_xml() const;
		string export_archive_xml() const;

		long find_cr_index(long) const;
		long get(constant) const;
#ifdef patterns_has_signal_processing
		contraction_detection &get_contraction_detection(void);
#endif

#if defined(patterns_standalone)
		void import_in(const string &);
		map<string, string> import_tag(string) const;
		void import_tag_insert(map<string, string> *, string, const string &, bool) const;
		void import_xml(const string &);
#endif

		void make_event_detector(void);
		void remove_trailing_non_final_contractions(void);
		void remove_trailing_non_final_events(void);
		void set_conductor(conductor *);

		static long cr_window;
		static bool event_detection_enabled;

#ifdef patterns_parer_classification
		static bool classification_enabled;
#endif

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum format { fcpp, fin, fxml };

		fetus(void);
		fetus(const fetus&);
		virtual ~fetus(void);

		string get_key(void) const { return m_key; }
		void set_key(const string& key) { m_key = key; }

		bool is_notifications_suspended() const;
		void suspend_notifications(bool value) const;

		long GetContractionsCount(void) const;
		const contraction& get_contraction(long) const;
		contraction* get_contraction_starting(long) const;
		const vector<contraction>& get_contractions() {return m_contractions;}
		void append_contraction(const contraction &);
		void append_contraction(const vector<contraction> &);



		long GetEventsCount(void) const;
		const event& get_event(long) const;
		event* get_event_starting(long) const;
		
		event* get_event_for_artifactkey(long key) const
		{
			long n = GetEventsCount();
			for (long i = 0; i < n; ++i)
			{
				if (m_events[i].get_artifactkey() == key)
				{
					return const_cast<event*>(&m_events[i]);
				}
			}
			return NULL;
		}
		
		contraction* get_contraction_for_artifactkey(long key) const
		{
			long n = GetContractionsCount();
			for (long i = 0; i < n; ++i)
			{
				if (m_contractions[i].get_artifactkey() == key)
				{
					return const_cast<contraction*>(&m_contractions[i]);
				}
			}
			return NULL;
		}

		/// Calculate an estimated height for the given contraction
		long get_height(const contraction& ci) const
		{
			long vs = get_up(ci.get_start());
			long vp = get_up(ci.get_peak());
			long ve = get_up(ci.get_end());

			return vp - min(vs, ve); // simple method of estimating 'baseline' up is by taking minimum value of contraction boundaries - may want to improve this
		}

		const vector<event>& get_events() {return m_events;}
		void append_event(const event &);
		void append_event(const vector<event> &);
		void fetch_events(void) const;

		long get_number_of_fhr(void) const;
		long get_fhr(long) const;
		const vector<char> &get_fhrs(void) const {return m_fhr;}
		void append_fhr(long);
		void append_fhr(const vector<char> &);
		void append_fhr(const vector<char>::const_iterator&, long);

		long get_number_of_mhr(void) const;
		long get_mhr(long) const;
		const vector<char> &get_mhrs(void) const {return m_mhr;}
		void append_mhr(long);
		void append_mhr(const vector<char> &);

		long get_number_of_up(void) const;
		long get_up(long) const;
		const vector<char> &get_ups(void) const {return m_up;}
		void append_up(long);
		void append_up(const vector<char> &);
		void append_up(const vector<char>::const_iterator&, long);

		bool are_contractions_equal(const fetus &, bool = false) const;
		bool are_events_equal(const fetus &, bool = false) const;
		bool are_fhr_equal(const fetus &) const;
		bool are_mhr_equal(const fetus &) const;
		bool are_up_equal(const fetus &) const;
		void compute_all(void); // batch processing using same interface as RT (i.e. append of huge block)
		void compute_now(void);

		void append_empty_fhr(long);
		void append_empty_mhr(long);
		void append_empty_up(long);
		string export_to_string(format = fxml) const;
		long find_contraction(long) const;
		long find_event(long) const;

		const conductor &get_conductor(void) const;
		conductor &get_conductor(void);

		static long get_cr_window() {return cr_window;}
		static void set_cr_window(int value) {cr_window = value;}

		static bool get_event_detection_enabled() {return event_detection_enabled;}
		static void set_event_detection_enabled(bool value) {event_detection_enabled = value;}

#ifdef patterns_parer_classification
		static bool get_classification_enabled() {return classification_enabled;}
		static void set_classification_enabled(bool value) {classification_enabled = value;}
#endif
		virtual vector<long> GetContractionsRate() const;
		virtual vector<long> GetContractionsRateIndex() const;

		virtual long GetContractionRate(long) const;
		date get_cutoff_date(void) const;
		date get_reset_date(void) const;
		virtual void ComputeContractionRateNow(void);
		void ResetContracionRate(void) { m_bComputeContractionRate = true; }

		long get_hr_sample_rate(void) const;

		virtual void get_mean_baseline(long, long, double *, double *) const;
		virtual void get_mean_baseline(long, long, int& ,int&) const;

		const string& get_last_test_result(void) const;
		date get_start_date(void) const;

		long get_up_sample_rate(void) const;
		bool has_conductor(void) const;
		bool has_cutoff_date(void) const;
		bool has_reset_date(void) const;
		bool has_start_date(void) const;

#if defined(patterns_standalone)
		void import(const string &);
		bool import_v01(const unsigned char *, long n);
#endif

		bool is_real_time(void) const;

		void note(subscription::message);
		void note(subscription::message, string);
		void note(subscription::message, long);

		fetus &operator =(const fetus &);
		bool operator ==(const fetus &) const;
		bool operator !=(const fetus &) const;
		void clear(void);
		void reset(void);
		void ResetContractions(void);
		void reset_events(void);
		void reset_fhr(void);
		void reset_mhr(void);

#ifdef patterns_parer_classification
		void reset_classification(void);
#endif

		void reset_up(void);
		void restart_engine_after_reboot(long x);
		void set_as_real_time(bool = true);
		void set_cutoff_date(const date &);
		void set_reset_date(const date &);
		void set_hr_sample_rate(long);
		void set_start_date(const date &);
		void set_up_sample_rate(long);
		void strike_event_out(long);
		void strike_event_out(event*);
		void strike_contraction_out(long);
		void strike_contraction_out(contraction*);
		void accept_event(long);
		void accept_event(event*);
		void subscribe(subscription *);
		void unsubscribe(subscription *);
		void unsubscribe(void *);

#ifdef patterns_parer_classification
		long GetParerClassification(long t) const;
		long GetParerClassification(long t, long *bl, long *bv, long *late, long *variable, long *prolonged) const;
#endif

		friend class conductor;

		static time_t convert_to_time_t(SYSTEMTIME st);
		static SYSTEMTIME convert_to_SYSTEMTIME(time_t t);

		static time_t convert_to_utc(time_t t);
		static SYSTEMTIME convert_to_utc(SYSTEMTIME st);
				
		static time_t convert_to_local(time_t t);
		static SYSTEMTIME convert_to_local(SYSTEMTIME t);
	};
}
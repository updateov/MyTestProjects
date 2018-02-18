#pragma once

#include "patterns, config.h"
#include "patterns, fetus.h"
#include "patterns, subscription.h"
#include "patterns, input adapter.h"

#include <map>
#include <set>
#include <string>
#include <vector>

using namespace std;

namespace patterns
{

	/*
	=======================================================================================================================
	The conductor class, directing the Patterns component. The conductor class, through the input adapter
	classes, interacts with client components to maintain a list of fetuses and direct messages to clients who
	subscribe to it. Fetuses may be accessed through unique identifiers. See adapter classes. Clients may subscribe to
	notifications using method subscribe () and supplying instances of subclasses of the subscription class. They may
	subsequently unsubscribe using either a pointer to the subscription object or an opaque pointer. See methods fetus
	::subscribe (), unsubscribe () and subscription::is (). The conductor class will notify client classes when
	instances of the fetus class are to be deleted, so that they may safely keep reference to fetus instances created
	by the conductor class. m_inputAdapter. c: list of known instances, built through reset_delay (). See comment
	for structure instance. f: dictionary of currently known fetuses, associated with their unique identifiers as
	returned by the input adapter instance. This is kept up to date through method update_fetuses (). fdump: dump
	object to return when no valid object is available. See method get (). fowned: set of owned patients, that is, of
	patients that this conductor has adopted and instructs to compute events and contractions. See method balance_load
	(). idebug -> debug (). idontsenddelete: true if we should refrain from broadcasting delete messages. Method
	receive_delete () , which in turn calls broadcast_delete_event (). We need to tell
	the latter to not send in that case. instance: structure that holds all known information about a know instance of
	conductor on the network, as gathered from messages received from the input adapter. Created exclusively through
	method reset_delay (). instance::d: delay, that is, time of last received message from the instance. instance::
	f: set of patients (fetuses) owned by the instance. These are of course patient identifiers as provided by the
	input adapter. instance::m: maximum number of patients that the instance will accept. instance::p: priority
	level of the instance. This will be set to LONG_MIN if the priority is still unknown. s: list of subscriptions to
	messaging. See methods note (), subscribe () and unsubscribe (). talive: timer for determining that the instance
	has been alive long enough to consider managing load balancing. tloadbalancing: timer for limiting load-balancing
	occurrences. See method balance_load (). tsendobjects: timer for making sure the remote instance has time to
	respond before we send another "send objects" message again. See method send_send_objects ().
	=======================================================================================================================
	*/
	class conductor
	{
		friend class input_adapter;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		ConfigData config;

		input_adapter *m_inputAdapter;
		map<string, fetus*> m_fetuses;
		mutable fetus *fdump;

		bool idebug;
		vector<subscription *> m_subscribers;
		
		virtual long get_number(void) const;	
		virtual set<string> get_patients(void) const;

	public:
		virtual void update_fetuses(void);

		virtual void apply_config(ConfigData&);

		static string to_string(const contraction&, char);
		static string to_string(const event&, char);
		static string to_string(const ConfigData& config, char separator);
		static string to_string(const string&, const vector<contraction>&, char);
		static string to_string(const string&, const vector<event>&, char);
		static string to_string(double);
		static string to_string(long);
		static string to_string(const vector<string> &, char = 1);

		static long to_integer(const string &);
		static double to_real(const string &);

		static void to_vector(const string& str, vector<string>& tokens, char delimiter);
		static bool to_event(event&, const string&, char);
		static bool to_contraction(contraction&, const string&, char);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		conductor(void);
		conductor(input_adapter *);
		virtual ~conductor(void);

		virtual const fetus &get(const string &) const;
		virtual fetus &get(const string &);
		virtual const fetus* get_fetus(const string &) const;
		virtual fetus* get_fetus(const string &);
		virtual bool is_known(const string &) const;
		virtual map<string, fetus*>& get_fetuses(void) { return m_fetuses;}

		virtual const input_adapter &get_input_adapter(void) const;
		virtual input_adapter &get_input_adapter(void);
		virtual bool has_input_adapter(void) const;
		virtual void set_input_adapter(input_adapter *);

		virtual void note(subscription::message);
		virtual void note(subscription::message, long);
		virtual void note(subscription::message, string);

		virtual void subscribe(subscription *);
		virtual void unsubscribe(subscription *);
		virtual void unsubscribe(void *);

		virtual void commit_pending_calculations(void);

		virtual ConfigData get_config(void) {return config;}

		virtual fetus* NewFetus() const;

		const input_adapter* get_input_adapter_ptr() const
		{return m_inputAdapter;}
	};
}
#pragma once

#include "patterns, config.h"
#include "patterns, crifetus.h"
#include "patterns, subscription.h"
#include "patterns, conductor.h"
#include "patterns, cri input adapter.h"

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
	class CRIConductor : public conductor
	{
		friend class CRIInputAdapter;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:

	public:
		static bool ToContractility(Contractility&, const string&, char);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CRIConductor(void);
		CRIConductor(CRIInputAdapter *);
		virtual ~CRIConductor(void);

		virtual const CRIFetus &GetFetus(const string &) const;
		virtual CRIFetus &GetFetus(const string &);

		virtual const CRIInputAdapter &get_input_adapter(void) const;
		virtual CRIInputAdapter &get_input_adapter(void);
		virtual fetus* NewFetus() const;
	};
}
#pragma once

#include <string>

namespace patterns
{
class subscription;
}

/*
 =======================================================================================================================
    Subscription superclass. This is meant to be derived from by clients of the patterns component. Clients must
    override method note () in order to receive messages from the conductor and fetus classes, as they may need. Method
    is () may be overridden for convenience, see method comment. mdebug: the debug mode of the conductor has been
    switched on or off. See conductor::debug (). mfetus: fetus data has changed, sent by fetus. minstancelist: the
    conductor's list of instance has changed, either through adding or deleting, or a particular instance has changed.
    mpatientlist: patient list has changed, sent by conductor. mwilldeletefetus: the conductor will delete the fetus,
    sent by fetus.
 =======================================================================================================================
 */
class patterns::subscription
{
	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:
		enum message { mdebug, mfetus, minstancelist, mpatientlist, mpatientstatus, mstrikeeventout, mwilldeletefetus, mconfig, mstrikecontractionout, macceptevent };

		subscription(void);
		virtual~subscription(void);

		virtual bool is(void *) const;

		virtual void note(message);
		virtual void note(message, long);
		virtual void note(message, std::string);
};

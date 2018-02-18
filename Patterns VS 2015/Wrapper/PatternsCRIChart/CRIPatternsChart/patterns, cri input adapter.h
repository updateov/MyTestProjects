#pragma once

namespace patterns
{
class CRIConductor;
}

#include "patterns, subscription.h"
#include "patterns, input adapter.h"
#include "patterns, crifetus.h"
#include <string>


namespace patterns
{
using namespace std;
class CRIInputAdapter;
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
class patterns::CRIInputAdapter : public patterns::input_adapter
{
		friend class CRIConductor;

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	protected:

		virtual const CRIConductor &GetConductor(void) const;
		virtual CRIConductor &GetConductor(void);

	/*
	 -------------------------------------------------------------------------------------------------------------------
	 -------------------------------------------------------------------------------------------------------------------
	 */
	public:

		CRIInputAdapter(void);
		virtual~CRIInputAdapter(void);

		static bool load_saved_data(CRIFetus &, string);
		static bool load_saved_data(CRIFetus&, vector<string>&, long timeadjustment=0);

		virtual conductor* NewConductor() const;
		
};


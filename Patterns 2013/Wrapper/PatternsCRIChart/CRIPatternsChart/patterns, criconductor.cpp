#include "stdafx.h"
#include "patterns, criconductor.h"

#include "patterns, config.h"
#include "patterns, crifetus.h"
#include "patterns, cri input adapter.h"
#include <time.h>
#include <assert.h>

using namespace patterns;

// =====================================================================================================================
//    Constructor and destructor.
// =======================================================================================================================
CRIConductor::CRIConductor(void)
{
}

CRIConductor::CRIConductor(CRIInputAdapter* i0)
	: conductor(i0)
{
}

CRIConductor::~CRIConductor(void)
{
}

// =====================================================================================================================
//    Access a fetus from given unique identifier. See predicate is_known ().
// =======================================================================================================================
const CRIFetus& CRIConductor::GetFetus(const string& n) const
{
	const CRIFetus*  r = 0;

	if (is_known(n))
	{
		r = dynamic_cast<CRIFetus*>(m_fetuses.find(n)->second);
	}

	if (!r && !dynamic_cast<CRIFetus*>(fdump))
	{
		fdump = new CRIFetus;
		dynamic_cast<CRIFetus*>(fdump)->set_conductor (const_cast<CRIConductor*>(this));
	}

	return r ? *r : *dynamic_cast<CRIFetus*>(fdump);
}


CRIFetus& CRIConductor::GetFetus(const string& n)
{
	CRIFetus*	r = 0;

	if (is_known(n))
	{
		r = dynamic_cast<CRIFetus*>(m_fetuses.find(n)->second);
	}

	if (!r && !dynamic_cast<CRIFetus*>(fdump))
	{
		fdump = new CRIFetus;
		dynamic_cast<CRIFetus*>(fdump)->set_conductor(this);
	}

	return r ? *r : *dynamic_cast<CRIFetus*>(fdump);
}


// =====================================================================================================================
//    Access the adapter instances. See the corresponding has_..._adapter () predicates.
// =======================================================================================================================
const CRIInputAdapter& CRIConductor::get_input_adapter(void) const
{
	return *dynamic_cast<CRIInputAdapter*>(m_inputAdapter);
}

CRIInputAdapter& CRIConductor::get_input_adapter(void)
{
	return *dynamic_cast<CRIInputAdapter*>(m_inputAdapter);
}



bool CRIConductor::ToContractility(Contractility& curContractility, const string& s, char separator)
{
	vector<string> mri;
	to_vector(s, mri, separator);

	if (mri.size() < 3)
	{
		return false;
	}

	curContractility.SetClassification((Contractility::ContractilityClassification) to_integer(mri[0]));
	curContractility.SetStart(to_integer(mri[1]));
	curContractility.SetEnd(to_integer(mri[2]));
	if (mri.size() > 3)
	{
		curContractility.SetContractilityKey(to_integer(mri[3]));
	}

	return true;
}



fetus* CRIConductor::NewFetus() const
{
	return new CRIFetus;
}
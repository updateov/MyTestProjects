#include "stdafx.h"
#include "fhrPart.h"
#include "patterns, event.h"
#include "patterns, contraction.h"
#include <assert.h>
#include <algorithm>

/*
 =======================================================================================================================
    Construction and destruction.
 =======================================================================================================================
 */
namespace patterns
{

	fhrPart::fhrPart()
	{
		reset();
	}

	fhrPart::fhrPart(long x10, long x20)
	{
		reset();
		setX1(x10);
		setX2(x20);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPart::~fhrPart(void)
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::reset()
	{
		setX1(-1);
		setX2(-1);
		setTimestamp(-1);
		setPercRepair(-1.0);
		setAbsCoords(true);
		setPending(true);
		setNonInterp(false);
		setType(FhrPartType_NONE);
		setUserMod(false);
		setCommitted(false);
		copyOrigX();
	}
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::debugDisplay()
	{
		printf("%10d : %10d - %10d\n", (int) getType(), getX1(), getX2());
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool fhrPart::intersects(fhrPart &p)
	{
		bool b = ((getX2() >= p.getX1()) && (getX1() <= p.getX2()));
		return(b);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool fhrPart::intersects(long x1, long x2)
	{
		bool b = ((getX2() >= x1) && (getX1() <= x2));
		return(b);
	}
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool fhrPart::inside(fhrPart &p)
	{
		bool b = ((getX1() >= p.getX1()) && (getX2() <= p.getX2()));
		return(b);
	}
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	double fhrPart::percOverlap(fhrPart &p)
	{
		double po = 0.0;
		if (intersects(p))
		{
			if (inside(p))
			{
				po = (double) length() / (double) p.length();
			}
			else if (p.inside(*this))
			{
				po = (double) p.length() / (double) length();
			}
			else if (startsBefore(p))
			{
				po = (double) (getX2() - p.getX1()) / (double) std::max(length(), p.length());
			}
			else
			{
				po = (double) (p.getX2() - getX1()) / (double) std::max(length(), p.length());
			}
		}
		return(po);
	}
	/*
	=======================================================================================================================
	always use longer of two intervals as denominator
	=======================================================================================================================
	*/
	double fhrPart::percOverlap(long x1, long x2)
	{
		fhrPart dummy = fhrPart(x1, x2);
		return(percOverlap(dummy));
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::toAbsTime(long offset)
	{
		if (!(isAbsCoords()))
		{
			operator+=(offset);
			setAbsCoords(true);
		}
	}
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::toRelativeTime(long offset)
	{
		if (isAbsCoords())
		{
			operator-=(offset);
			setAbsCoords(false);
		}
	}
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::copyOrigX(void)
	{
		x1orig = getX1();
		x2orig = getX2();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool fhrPart::merge(fhrPart &p)
	{
		bool b = intersects(p);
		if (b)
		{
			if (startsAfter(p))
			{
				setX1(p.getX1());
			}
			if (endsBefore(p))
			{
				setX2(p.getX2());
			}
		}

		return(b);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool fhrPart::precedence(fhrPart &p)
	{
		bool b = false;
		if (!(p.isUserMod()) && isUserMod())
		{
			b = true;
		}
		return(b);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::toEvent(event *e, long edelta, long cdelta)
	{
		// common to all fhrParts
		e->set_start(getX1() + edelta);
		e->set_end(getX2() + edelta);
		e->set_as_final(!(isPending()));
		e->set_as_noninterp(isNonInterp());
		e->set_repair(getPercRepair());

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::fromEvent(event *e)
	{
		setX1(e->get_start());
		setX2(e->get_end());
		setPending(!(e->is_final()));
		setPercRepair(e->get_repair());
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPart::fromContraction(contraction *c)
	{
		setX1(c->get_start());
		setX2(c->get_end());
	}

	void* fhrPart::copy()
	{
		void *pcopy;
		if (isDecel() || isNonDecel())
		{
			pcopy = ((decel*) this)->copy();
		}
		else if (isAccel() || isNonAccel())
		{
			pcopy = ((accel*) this)->copy();
		}
		else if (isBaseline())
		{
			pcopy = ((baseline *) this)->copy();
		}
		else
		{
			pcopy = NULL;
		}
		return(pcopy);
	}

	/*void fhrPart::fromRepInterval(CRepairInterval *r)
	{
		setX1(r->GetX1());
		setX2(r->GetX2());
	}
*/

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPart &fhrPart::operator+=(long d) 
	{
		x1 = getX1() + d;
		x2 = getX2() + d;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPart &fhrPart::operator-=(long d) 
	{
		x1 = getX1() - d;
		x2 = getX2() - d;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPart &fhrPart::operator=(const fhrPart &p)
	{
		x1 = p.x1;
		x2 = p.x2;
		timestamp = p.timestamp;
		prepair = p.prepair;
		absCoords = p.absCoords;
		pending = p.pending;
		nonInterp = p.nonInterp;
		type = p.type;
		x1orig = p.x1orig;
		x2orig = p.x2orig;
		userMod = p.userMod;
		committed = p.committed;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	baseline::baseline(void)
	{
		reset();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	baseline::baseline(long x1, long x2)
	{
		reset();
		setX1(x1);
		setX2(x2);
	}

	baseline::~baseline()
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void baseline::reset()
	{
		setType(FhrPartType_BAS);
		y1 = -1.0;
		y2 = -1.0;
		ymean = -1.0;
		ymax = -1.0;
		ymin = -1.0;
		var = -1.0;

		varPassMid = -1.0;
		moveAvgMid = -1.0;
		lpMean = -1.0;
		varMin = -1.0;
		varPassMidToNext = -1.0;
		meanBtwnNext = -1.0;
		maxBtwnNext = -1.0;
		minBtwnNext = -1.0;
		mhVar = -1.0;

		mhClass = false; //  multiH classification
		mhType = MH_NONE; // multiH classification type
		mhStable = false; // is multiH classification stable?
		mhFeatStable = false;  // features stable

		bp2Stable = false; // is BP2 classification stable?
		bp2FeatStable = false;  // BP2 features stable
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void* baseline::copy()
	{
		baseline* pcopy = new baseline;
		(*pcopy) = (*this);
		return(pcopy);
	}
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void baseline::toEvent(event *e, long edelta, long cdelta)
	{
		fhrPart::toEvent(e, edelta, cdelta);
		e->set_type(event::tbaseline);
		e->set_y1(getY1());
		e->set_y2(getY2());
		e->set_baseline_var(getVar());
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void baseline::fromEvent(event *e)
	{
		fhrPart::fromEvent(e);
		setY1(e->get_y1());
		setY2(e->get_y2());
		setVar(e->get_baseline_var());
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void baseline::getMeanVal(double* pSignal, long size)
	{
		if ((getX1() >= 0) && (getX2() < size))
		{
			double dTotal = 0.0;
			for (long i = getX1(); i <= getX2(); i++)
			{
				dTotal += pSignal[i];
			}

			setYmean(dTotal / length());
		}
		else
		{
			assert(false); // LXP OUT OF BOUNDS
		}

	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	baseline &baseline::operator=(const baseline &p)
	{
		this->fhrPart::operator=(p);
		y1 = p.y1;
		y2 = p.y2;
		ymean = p.ymean;
		ymax = p.ymax;
		ymin = p.ymin;
		var = p.var;

		varPassMid = p.varPassMid;
		moveAvgMid = p.moveAvgMid;
		lpMean = p.lpMean;
		varMin = p.varMin;
		varPassMidToNext = p.varPassMidToNext;
		meanBtwnNext = p.meanBtwnNext;
		maxBtwnNext = p.maxBtwnNext;
		minBtwnNext = p.minBtwnNext;
		mhVar = p.mhVar;

		mhClass = p.mhClass;
		mhType = p.mhType;
		mhStable = p.mhStable;
		mhFeatStable = p.mhFeatStable;

		// PAW
		bp2Stable = p.bp2Stable;
		bp2FeatStable = p.bp2FeatStable;
		return *this;
	}



	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bump::bump(void)
	{
		reset();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bump::~bump(void)
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void bump::reset(void)
	{
		xpeak = -1;
		peakVal = -1.0;
		freqBand = -1;
		confidence = -1.0;
		bpHeight = -1.0;
		bpPeak = -1;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void bump::toEvent(event *e, long edelta, long cdelta)
	{
		fhrPart::toEvent(e, edelta, cdelta);
		e->set_peak(getPeak() + edelta);
		e->set_confidence(getConfidence());
		e->set_height(getHeight());
		e->set_peak_val(getPeakVal());
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void bump::fromEvent(event *e)
	{
		fhrPart::fromEvent(e);
		setPeak(e->get_peak());
		setPeakVal(e->get_peak_val());
		setConfidence(e->get_confidence());
		setHeight(e->get_height());
		setNonInterp(e->is_noninterp());
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bump &bump::operator=(const bump &p)
	{
		this->fhrPart::operator=(p);
		xpeak = p.xpeak;
		peakVal = p.peakVal;
		freqBand = p.freqBand;
		confidence = p.confidence;
		bpHeight = p.bpHeight;
		bpPeak = p.bpPeak;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bump &bump::operator-=(long d) 
	{
		this->fhrPart::operator-=(d);
		xpeak = getPeak() - d;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bump &bump::operator+=(long d) 
	{
		this->fhrPart::operator+=(d);
		xpeak = getPeak() + d;
		return *this;
	}

	/*
	=======================================================================================================================
	// dummy function so can call getBM for accel or decel in same method
	=======================================================================================================================
	*/
	bumpMeasures* bump::getBM()
	{
		bumpMeasures* bm = new bumpMeasures;
		return(bm);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	decel::decel()
	{
		reset();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	decel::decel(long x1, long x2)
	{
		reset();
		setX1(x1);
		setX2(x2);
		copyOrigX();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	decel::~decel(void)
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void decel::reset()
	{
		//this->fhrPart::reset();
		this->fhrPart::setType(FhrPartType_DECEL);
		setSubtype(FhrPartType_DECEL); // no subtype for now
		setLate(false);
		setVariable(false);
		setContrIndex(-1);
		setLag(-1);
		clearAtypical();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool decel::checkAtypicalType(atypicalCode a)
	{
		return((atyp & a) != 0);
	}
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void decel::addAtypical(atypicalCode a)
	{
		if (!(checkAtypicalType(a)))
		{
			atyp += a;
		}
	}

	/*
	=======================================================================================================================
	Does this take precedence over p in a merge situation?
	=======================================================================================================================
	*/
	bool decel::precedence(fhrPart &p)
	{
		bool b = true;
		if (!(p.isUserMod()) && isUserMod())
		{
			b = true;
		}
		else if (p.isUserMod() && (!(isUserMod())))
		{
			b = false;
		} 
		else if (p.isDecel())
		{
			b = getConfidence() > p.getConfidence();
		}
		return(b);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void decel::toEvent(event *e, long edelta, long cdelta)
	{
		bump::toEvent(e, edelta, cdelta);
		FhrPartType t = getSubtype();
		event::type t2;

		switch(t)
		{
		case(DecType_VAR):
			t2 = event::ttypical;
			break;
		case(DecType_LATE):
			t2 = event::tlate;
			break;
		case(DecType_EARLY):
			t2 = event::tearly;
			break;
		case(DecType_NONASSOC):
			t2 = event::tnadeceleration;
			break;
		case(DecType_PROL):
			t2 = event::tprolonged;
			break;
		case(DecType_ATYP):
			t2 = event::tatypical;
			break;
		default:
			t2 = event::terror;
		}
		e->set_type(t2);

		e->set_as_late(isLate());
		e->set_as_variable(isVariable());
		e->set_contraction(getContrIndex() + cdelta);
		e->set_lag(getLag());
		e->set_atypical(getAtypicalCode());

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void decel::fromEvent(event *e)
	{
		fhrPart::FhrPartType t;

		bump::fromEvent(e);
		setContrIndex(e->get_contraction());
		setLate(e->is_late());
		setVariable(e->is_variable());
		setLag(e->get_lag());
		setAtypicalCode(e->get_atypical());
		switch(e->get_type())
		{
		case event::tlate:
			t = fhrPart::DecType_LATE;
			break;
		case event::tatypical:
			t = fhrPart::DecType_VAR;
			break;
		case event::tearly:
			t = fhrPart::DecType_EARLY;
			break;
		case event::tnadeceleration:
			t = fhrPart::DecType_NONASSOC;
			break;
		case event::tprolonged:
			t = fhrPart::DecType_PROL;
			break;
		case event::ttypical:
			t = fhrPart::DecType_VAR;
			break;
		default:
			t = fhrPart::FhrPartType_DECEL;
			break;
		}
		setSubtype(t);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void* decel::copy()
	{
		decel* pcopy = new decel;
		(*pcopy) = (*this);
		return(pcopy);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	decel &decel::operator=(const decel &p)
	{
		this->bump::operator=(p);
		subtype = p.subtype;
		late = p.late; 
		variable = p.variable; 
		contrIndex = p.contrIndex; 
		lag = p.lag;  
		atyp = p.atyp; 
		bm = p.bm;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	accel::accel()
	{
		reset();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	accel::accel(long x1, long x2)
	{
		reset();
		setX1(x1);
		setX2(x2);
		copyOrigX();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	accel::~accel(void)
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void accel::reset()
	{
		setType(FhrPartType_ACCEL);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool accel::precedence(fhrPart &p)
	{
		bool b = true;
		if (!(p.isUserMod()) && isUserMod())
		{
			b = true;
		}
		else if (p.isUserMod() && (!(isUserMod())))
		{
			b = false;
		} 
		else if (p.isDecel())
		{
			b = false;
		}
		else if (p.isAccel())
		{
			b = getConfidence() > p.getConfidence();
		}
		return(b);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void accel::toEvent(event *e, long edelta, long cdelta)
	{
		bump::toEvent(e, edelta, cdelta);
		e->set_type(event::tacceleration);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void* accel::copy()
	{
		accel* pcopy = new accel;
		(*pcopy) = (*this);
		return(pcopy);
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	accel &accel::operator=(const accel &p)
	{
		this->bump::operator=(p);
		bm = p.bm;
		return *this;
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasures::bumpMeasures()
	{
		reset();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasures::~bumpMeasures()
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void bumpMeasures::reset()
	{
		measured = false;
		length = -1;
		fhrBegin = -1.0;
		fhrEnd = -1.0;
		fhrStd = -1.0;
		fhrPrev = -1.0;
		fhrNext = -1.0;
		maxHeight = -1.0;
		meanHeight = -1.0;
		area = -1.0;
		varMax = -1.0;
		varPrev = -1.0;
		varNext = -1.0;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasures &bumpMeasures::operator =(const bumpMeasures &bm)
	{
		measured = bm.measured;
		length = bm.length;
		fhrBegin = bm.fhrBegin;
		fhrEnd = bm.fhrEnd;
		fhrStd = bm.fhrStd;
		fhrPrev = bm.fhrPrev;
		fhrNext = bm.fhrNext;
		maxHeight = bm.maxHeight;
		meanHeight = bm.meanHeight;
		area = bm.area;
		varMax = bm.varMax;
		varPrev = bm.varPrev;
		varNext = bm.varNext;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasuresDecel::bumpMeasuresDecel()
	{
		reset();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasuresDecel::~bumpMeasuresDecel()
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void bumpMeasuresDecel::reset()
	{
		onset = -1;
		recovery = -1;
		fhrInSlopeVal = -1.0;
		fhrOutSlopeVal = -1.0;
		fhrInSlopeTime = -1;
		fhrOutSlopeTime = -1;
		contrBegin = -1;
		contrEnd = -1;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasuresDecel &bumpMeasuresDecel::operator =(const bumpMeasuresDecel &bm)
	{
		this->bumpMeasures::operator=(bm);
		onset = bm.onset;
		recovery = bm.recovery;
		fhrInSlopeVal = bm.fhrInSlopeVal;
		fhrOutSlopeVal = bm.fhrOutSlopeVal;
		fhrInSlopeTime = bm.fhrInSlopeTime;
		fhrOutSlopeTime = bm.fhrOutSlopeTime;
		contrBegin = bm.contrBegin;
		contrEnd = bm.contrEnd;
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasuresAccel::bumpMeasuresAccel()
	{
		reset();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasuresAccel::~bumpMeasuresAccel()
	{
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void bumpMeasuresAccel::reset()
	{
		slopeTimeIn25 = -1;
		slopeTimeOut25 = -1;
		onset25 = -1;
		recovery25 = -1;
		onsetSlope25 = -1.0;
		recoverySlope25 = -1.0;
		lastDecelX2 = -1;
		nextDecelX1 = -1;
		decelRate = -1.0;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bumpMeasuresAccel &bumpMeasuresAccel::operator =(const bumpMeasuresAccel &bm)
	{
		this->bumpMeasures::operator=(bm);
		slopeTimeIn25 = bm.slopeTimeIn25;
		slopeTimeOut25 = bm.slopeTimeOut25;
		onset25 = bm.onset25;
		recovery25 = bm.recovery25;
		onsetSlope25 = bm.onsetSlope25;
		recoverySlope25 = bm.recoverySlope25;
		lastDecelX2 = bm.lastDecelX2;
		nextDecelX1 = bm.nextDecelX1;
		decelRate = bm.decelRate;
		return *this;
	}
}












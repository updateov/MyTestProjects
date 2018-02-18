#pragma once

#include <vector>

using namespace std;

namespace patterns
{
	/* 
	fhrPart : base class for fhr events

		fhrPart
		/      \
	baseline  bump
			  /  \
		  accel  decel

	*/
	class event;
	class contraction;

	class fhrPart
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum FhrPartType { FhrPartType_NONE, FhrPartType_DECEL, FhrPartType_NONDECEL, FhrPartType_ACCEL, FhrPartType_NONACCEL, FhrPartType_BAS, DecType_VAR, DecType_GRAD, DecType_LATE, DecType_EARLY, DecType_NONASSOC, DecType_PROL, DecType_ATYP};

	protected:

		long x1;  // start index
		long x2;  // end index
		long timestamp; // output timestamp
		double prepair; // perc repair signal
		bool absCoords; // in absolute coordinates
		bool pending; 
		bool nonInterp; // is non-interpretable (i.e. too much artifact)
		bool userMod; // was modified by user
		long x1orig; // original patterns x1, x2 in cases where was modified by user
		long x2orig; 
		bool committed; // already commmitted to output
		FhrPartType type; // fhr part type

	public:
		fhrPart();
		fhrPart(long, long);
		virtual ~fhrPart(void);
		
		virtual void reset();
		
		virtual long getX1(void) {return x1;}
		virtual void setX1(long x) {x1 = x;} 
		virtual long getX2(void) {return x2;}
		virtual void setX2(long x) {x2 = x;}
		virtual long length(void) {return (x2 - x1 + 1);}
		virtual long midpt(void) {return ((x2 + x1 + 1) / 2);}

		virtual long getTimestamp(void) {return timestamp;}
		virtual void setTimestamp(long x) {timestamp = x;}

		virtual double getPercRepair(void) {return prepair;}
		virtual void setPercRepair(double p) {prepair = p;}

		virtual bool isAbsCoords(void) {return absCoords;}
		virtual void setAbsCoords(bool b) {absCoords = b;}

		virtual bool isPending(void) {return pending;}
		virtual void setPending(bool b) {pending = b;}

		virtual bool isNonInterp(void) {return nonInterp;}
		virtual void setNonInterp(bool b) {nonInterp = b;}

		virtual FhrPartType getType(void) {return type;}
		virtual void setType(FhrPartType t) {type = t;}
		
		virtual long getX1Orig(void) {return x1orig;}
		virtual long getX2Orig(void) {return x2orig;}
		virtual void copyOrigX(void);

		virtual bool isUserMod(void) {return userMod;}
		virtual void setUserMod(bool b = true) {userMod = b;}

		virtual bool isCommitted(void) {return committed;}
		virtual void setCommitted(bool b = true) {committed = b;}

		virtual bool intersects(fhrPart &p);
		virtual bool intersects(long x1, long x2);
		virtual bool inside(fhrPart &p);
		virtual bool startsBefore(fhrPart &p) {return (getX1() < p.getX1());}
		virtual bool startsAfter(fhrPart &p) {return (getX1() > p.getX1());}
		virtual bool endsBefore(fhrPart &p) {return (getX2() < p.getX2());}
		virtual bool endsAfter(fhrPart &p) {return (getX2() > p.getX2());}
		virtual double percOverlap(long x1, long x2);
		virtual double percOverlap(fhrPart &p);

		virtual bool merge(fhrPart &p); // merge p to current part
		virtual bool precedence(fhrPart &p); // does fhrPart take precedence over p if merging?
		
		virtual bool isDecel() {return type == FhrPartType_DECEL;}
		virtual bool isNonDecel() {return type == FhrPartType_NONDECEL;}
		virtual bool isAccel() {return type == FhrPartType_ACCEL;}
		virtual bool isNonAccel() {return type == FhrPartType_NONACCEL;}
		virtual bool isBaseline() {return type == FhrPartType_BAS;}
		virtual bool isEvent() {return (isDecel() || isAccel() || isBaseline());}
		virtual bool isBump() {return (isDecel() || isAccel() || isNonDecel() || isNonAccel());}

		virtual void toAbsTime(long offset);
		virtual void toRelativeTime(long offset);

		virtual double getConfidence(void) {return -1.0;} // dummy function so can compile calling w/ arbitrary fhrPart

		virtual void toEvent(event *e, long edelta = 0, long cdelta = 0); // output to event object
		virtual void fromEvent(event *e);
		virtual void fromContraction(contraction *c);
	//	virtual void fromRepInterval(CRepairInterval *r);
		virtual void debugDisplay(void);

		virtual void *copy();

		virtual fhrPart &operator -=(long);
		virtual fhrPart &operator +=(long);
		virtual fhrPart &operator =(const fhrPart &);

	};

	class baseline :
		public fhrPart
	{

	public:
		enum mhClassType {MH_NONE, MH_SEC, MH_PRIM};
	
	protected:
		double y1;
		double y2;
		double ymean;
		double ymax;
		double ymin;
		double var; // this is the baseline variability as shown in client

		double varPassMid;
		double moveAvgMid;
		double lpMean;
		double varMin;
		double varPassMidToNext;
		double meanBtwnNext;
		double maxBtwnNext;
		double minBtwnNext;
		double mhVar; // this is a different variability measure used in MH classification

		bool mhClass; // multiH classification
		mhClassType mhType; // multiH classification type (primary, secondary, not done yet)
		bool mhStable; // is multiH classification stable?
		bool mhFeatStable;  // features stable

		// BP2
		//bool bp2Class; // multiH classification
		bool bp2Stable; // is multiH classification stable?
		bool bp2FeatStable;  // features stable
	public:

		baseline();
		baseline(long, long);
		virtual ~baseline(void);

		virtual void reset(void);

		virtual double getY1(void) {return y1;}
		virtual void setY1(double y) {y1 = y;}
		virtual double getY2(void) {return y2;}
		virtual void setY2(double y) {y2 = y;}
		virtual double getYmean(void) {return ymean;}
		virtual void setYmean(double y) {ymean = y;}
		virtual double getYmax(void) {return ymax;}
		virtual void setYmax(double y) {ymax = y;}
		virtual double getYmin(void) {return ymin;}
		virtual void setYmin(double y) {ymin = y;}

		virtual double getVar(void) {return var;}
		virtual void setVar(double v) {var = v;}

		virtual double getVarPassMid(void) {return varPassMid;}
		virtual void setVarPassMid(double v) {varPassMid = v;}
		virtual double getMoveAvgMid(void) {return moveAvgMid;}
		virtual void setMoveAvgMid(double v) {moveAvgMid = v;}
		virtual double getLpMean(void) {return lpMean;}
		virtual void setLpMean(double y) {lpMean = y;}
		virtual double getVarMin(void) {return varMin;}
		virtual void setVarMin(double v) {varMin = v;}
		virtual double getVarPassMidToNext(void) {return varPassMidToNext;}
		virtual void setVarPassMidToNext(double v) {varPassMidToNext = v;}
		virtual double getMeanBtwnNext(void) {return meanBtwnNext;}
		virtual void setMeanBtwnNext(double y) {meanBtwnNext = y;}
		virtual double getMaxBtwnNext(void) {return maxBtwnNext;}
		virtual void setMaxBtwnNext(double y) {maxBtwnNext = y;}
		virtual double getMinBtwnNext(void) {return minBtwnNext;}
		virtual void setMinBtwnNext(double y) {minBtwnNext = y;}
		virtual double getMhVar(void) {return mhVar;}
		virtual void setMhVar(double v) {mhVar = v;}

		virtual bool getMhClass(void) {return mhClass;}
		virtual void setMhClass(bool b = true) {mhClass = b;}
		virtual mhClassType getMhType(void) {return mhType;}
		virtual void setMhType(mhClassType t) {mhType = t;} 

		virtual bool isMhStable(void) {return mhStable;}
		virtual void setMhStable(bool b = true) {mhStable = b;}
		virtual bool isMhFeatStable(void) {return mhFeatStable;}
		virtual void setMhFeatStable(bool b = true) {mhFeatStable = b;}	

		// PAW BP2
		virtual bool isBP2Stable(void) {return bp2Stable;}
		virtual void setBP2Stable(bool b = true) {bp2Stable = b;}
		virtual bool isBP2FeatStable(void) {return bp2FeatStable;}
		virtual void setBP2FeatStable(bool b = true) {bp2FeatStable = b;}	

		virtual void getMeanVal(double* pSignal, long size);

		virtual void *copy();
		virtual void toEvent(event *e, long edelta = 0, long cdelta = 0);
		virtual void fromEvent(event *e);

		virtual baseline &operator =(const baseline &);
	};

	class bumpMeasures // measures in common with accels and decels
	{
		
	protected:
		bool measured;
		long length;
		double fhrBegin;
		double fhrEnd;
		double fhrStd;
		double fhrPrev;
		double fhrNext;
		double maxHeight;
		double meanHeight;
		double area;
		double varMax;
		double varPrev;
		double varNext;

	public:

		bumpMeasures(void);
		virtual ~bumpMeasures(void);

		virtual void reset();
		virtual bool isMeasured(void) {return measured;}
		virtual void setMeasured(bool b = true) {measured = b;}

		virtual long getLength(void) {return length;}
		virtual void setLength(long n) {length = n;}
		virtual double getFhrBegin(void) {return fhrBegin;}
		virtual void setFhrBegin(double d) {fhrBegin = d;}
		virtual double getFhrEnd(void) {return fhrEnd;}
		virtual void setFhrEnd(double d) {fhrEnd = d;}
		virtual double getFhrStd(void) {return fhrStd;}
		virtual void setFhrStd(double d) {fhrStd = d;}
		virtual double getFhrPrev(void) {return fhrPrev;}
		virtual void setFhrPrev(double d) {fhrPrev = d;}
		virtual double getFhrNext(void) {return fhrNext;}
		virtual void setFhrNext(double d) {fhrNext = d;}
		virtual double getMaxHeight(void) {return maxHeight;}
		virtual void setMaxHeight(double d) {maxHeight = d;}
		virtual double getMeanHeight(void) {return meanHeight;}
		virtual void setMeanHeight(double d) {meanHeight = d;}
		virtual double getArea(void) {return area;}
		virtual void setArea(double d) {area = d;}
		virtual double getVarMax(void) {return varMax;}
		virtual void setVarMax(double d) {varMax = d;}
		virtual double getVarPrev(void) {return varPrev;}
		virtual void setVarPrev(double d) {varPrev = d;}
		virtual double getVarNext(void) {return varNext;}
		virtual void setVarNext(double d) {varNext = d;}

		virtual bumpMeasures &operator =(const bumpMeasures &);

	};

	class bump :
		public fhrPart
	{

	protected:
		long xpeak;
		double peakVal;
		long freqBand;
		double confidence;
		double bpHeight;
		long bpPeak;
		double height;

	public:
		bump(void);
		virtual ~bump(void);

		virtual void reset(void);

		virtual long getPeak(void) {return xpeak;}
		virtual void setPeak(long x) {xpeak = x;}

		virtual double getPeakVal(void) {return peakVal;}
		virtual void setPeakVal(double v) {peakVal = v;}

		virtual long getFreqBand(void) {return freqBand;}
		virtual void setFreqBand(long b) {freqBand = b;}

		virtual double getConfidence(void) {return confidence;}
		virtual void setConfidence(double c) {confidence = c;}
		
		virtual void setHeight(double h) {bpHeight = h;}
		virtual double getHeight(void) {return bpHeight;}

		virtual void setBpHeight(double h) {bpHeight = h;}
		virtual double getBpHeight(void) {return bpHeight;}

		virtual void setBpPeak(long p) {bpPeak = p;}
		virtual long getBpPeak(void) {return bpPeak;}

		virtual bumpMeasures* getBM(void); 

		virtual void toEvent(event *e, long edelta = 0, long cdelta = 0);
		virtual void fromEvent(event *e);

		virtual bump &operator -=(long);
		virtual bump &operator +=(long);
		virtual bump &operator =(const bump &);

	};

	class bumpMeasuresDecel:
		public bumpMeasures
	{

	protected:
		long onset;
		long recovery;
		double fhrInSlopeVal;
		double fhrOutSlopeVal;
		long fhrInSlopeTime;
		long fhrOutSlopeTime;
		long contrBegin;
		long contrEnd;

	public:
		bumpMeasuresDecel(void);
		virtual ~bumpMeasuresDecel(void);

		virtual void reset();

		virtual long getOnset(void) {return onset;}
		virtual void setOnset(long n) {onset = n;}
		virtual long getRecovery(void) {return recovery;}
		virtual void setRecovery(long n) {recovery = n;}
		virtual double getFhrInSlopeVal(void) {return fhrInSlopeVal;}
		virtual void setFhrInSlopeVal(double d) {fhrInSlopeVal = d;}
		virtual long getFhrInSlopeTime(void) {return fhrInSlopeTime;}
		virtual void setFhrInSlopeTime(long n) {fhrInSlopeTime = n;}
		virtual double getFhrOutSlopeVal(void) {return fhrOutSlopeVal;}
		virtual void setFhrOutSlopeVal(double d) {fhrOutSlopeVal = d;}
		virtual long getFhrOutSlopeTime(void) {return fhrOutSlopeTime;}
		virtual void setFhrOutSlopeTime(long n) {fhrOutSlopeTime = n;}
		virtual long getContrBegin(void) {return contrBegin;}
		virtual void setContrBegin(long n) {contrBegin = n;}
		virtual long getContrEnd(void) {return contrEnd;}
		virtual void setContrEnd(long n) {contrEnd = n;}

		virtual bumpMeasuresDecel &operator =(const bumpMeasuresDecel &);

	};

	class bumpMeasuresAccel:
		public bumpMeasures
	{

	protected:
		long slopeTimeIn25;
		long slopeTimeOut25;
		long onset25;
		long recovery25;
		double onsetSlope25;
		double recoverySlope25;
		long lastDecelX2;
		long nextDecelX1;
		double decelRate;
	
	public:
		bumpMeasuresAccel(void);
		virtual ~bumpMeasuresAccel(void);

		virtual void reset();

		virtual long getSlopeTimeIn25(void) {return slopeTimeIn25;}
		virtual void setSlopeTimeIn25(long n) {slopeTimeIn25 = n;}
		virtual long getSlopeTimeOut25(void) {return slopeTimeOut25;}
		virtual void setSlopeTimeOut25(long n) {slopeTimeOut25 = n;}
		virtual long getOnset25(void) {return onset25;}
		virtual void setOnset25(long n) {onset25 = n;}
		virtual long getRecovery25(void) {return recovery25;}
		virtual void setRecovery25(long n) {recovery25 = n;}
		virtual double getOnsetSlope25(void) {return onsetSlope25;}
		virtual void setOnsetSlope25(double d) {onsetSlope25 = d;}
		virtual double getRecoverySlope25(void) {return recoverySlope25;}
		virtual void setRecoverySlope25(double d) {recoverySlope25 = d;}
		virtual long getLastDecelX2(void) {return lastDecelX2;}
		virtual void setLastDecelX2(long x) {lastDecelX2 = x;}
		virtual long getNextDecelX1(void) {return nextDecelX1;}
		virtual void setNextDecelX1(long x) {nextDecelX1 = x;}
		virtual double getDecelRate(void) {return decelRate;}
		virtual void setDecelRate(double d) {decelRate = d;}

		virtual bumpMeasuresAccel &operator =(const bumpMeasuresAccel &);
	};


	class decel :
		public bump
	{

	public:
		enum atypicalCode {tbiphasic = 1, tlossrise = 2, tlossvar = 4, tlowerbas = 8, tprolsecrise = 16, tsixties = 32, tslowreturn = 64, tvarlate = 128} ;

	protected:

		FhrPartType subtype;
		bool late; // is late?
		bool variable; // is variable?
		long contrIndex; // contraction index
		long lag;  // decel lag for late
		long atyp; // atypical code
		bumpMeasuresDecel bm; // bump measures for classification

	public:

		decel();
		decel(long, long);
		virtual ~decel(void);

		virtual void reset();

		virtual FhrPartType getSubtype(void) {return subtype;}
		virtual void setSubtype(FhrPartType t) {subtype = t;}

		virtual bool isLate(void) {return late;}
		virtual void setLate(bool b = true) {late = b;}
		virtual bool isVariable(void) {return variable;}
		virtual bool isGradual(void) {return (!(isVariable()));} 
		virtual void setVariable(bool b = true) {variable = b;}

		virtual long getContrIndex(void) {return contrIndex;}
		virtual void setContrIndex(long n) {contrIndex = n;}

		virtual long getLag(void) {return lag;}
		virtual void setLag(long n) {lag = n;}

		virtual bool isAtypical(void) {return (atyp > 0);}
		virtual long getAtypicalCode(void) {return atyp;}
		virtual void setAtypicalCode(long a) {atyp = a;} 
		virtual void clearAtypical(void) {atyp = 0;}
		virtual bool checkAtypicalType(atypicalCode a);
		virtual void addAtypical(atypicalCode a);
		virtual bool isBiphasic(void) {return checkAtypicalType(decel::tbiphasic);}
		virtual void setBiphasic(void) {addAtypical(decel::tbiphasic);}
		virtual bool isLossRise(void) {return checkAtypicalType(decel::tlossrise);}
		virtual void setLossRise(void) {addAtypical(decel::tlossrise);}
		virtual bool isLossVar(void) {return checkAtypicalType(decel::tlossvar);}
		virtual void setLossVar(void) {addAtypical(decel::tlossvar);}
		virtual bool isLowerBas(void) {return checkAtypicalType(decel::tlowerbas);}
		virtual void setLowerBas(void) {addAtypical(decel::tlowerbas);}
		virtual bool isProlSecRise(void) {return checkAtypicalType(decel::tprolsecrise);}
		virtual void setProlSecRise(void) {addAtypical(decel::tprolsecrise);}
		virtual bool isSixties(void) {return checkAtypicalType(decel::tsixties);}
		virtual void setSixties(void) {addAtypical(decel::tsixties);}
		virtual bool isSlowReturn(void) {return checkAtypicalType(decel::tslowreturn);}
		virtual void setSlowReturn(void) {addAtypical(decel::tslowreturn);}
		virtual void setVarLate(void) {addAtypical(decel::tvarlate);} // note that this repeats isLate flag (repeated so can consider as atypical/non-reassuring feature in same manner as others)

		virtual bumpMeasuresDecel* getBM(void) {return(&bm);}
		virtual void clearBM(void) {bm.reset();}
		virtual void toEvent(event *e, long edelta = 0, long cdelta = 0);
		virtual void fromEvent(event *e);
		virtual void *copy();
			
		virtual bool precedence(fhrPart &p);

		virtual decel &operator =(const decel &);
		
	};

	class accel:
		public bump
	{

	protected:
		bumpMeasuresAccel bm; // bump measures for classification

	public:
		accel();
		accel(long, long);
		virtual ~accel(void);

		virtual void reset();
		virtual void clearBM(void) {bm.reset();}
		virtual bool isMeasured(void) {return bm.isMeasured();}
		virtual void setMeasured(bool b = true) {bm.setMeasured(b);}

		virtual bool precedence(fhrPart &p);
		virtual bumpMeasuresAccel* getBM(void) {return(&bm);}
		virtual void toEvent(event *e, long edelta = 0, long cdelta = 0); // output to event object
		virtual void *copy();

		virtual accel &operator =(const accel &);

	};

	


		
}



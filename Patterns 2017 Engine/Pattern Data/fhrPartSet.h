#pragma once

#include <vector>
#include "fhrPart.h"
#include "patterns, event.h"
#include "patterns, contraction.h"

using namespace std;

namespace patterns
{
	class fhrPartSet
	{
	protected:
		vector<fhrPart*> p;
		bool sorted;
		bool doClearMemory;

	public:
		fhrPartSet();
		virtual ~fhrPartSet(void);

		virtual void add(fhrPart* f); // adds pointer
		virtual void add(fhrPartSet* fset);
		virtual void addcopy(fhrPart* f); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void addcopy(baseline* f); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void addcopy(bump* f); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void addcopy(decel* f); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void addcopy(accel* f); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void addcopy(fhrPartSet* fset); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void addcopy(fhrPartSet* fset, long minX1); 
		virtual void addcopyUncommitted(fhrPartSet* fset); 
		virtual void addcopy(event* e);
		virtual void addcopy(contraction* c); // used if want to convert contractions to do comparison with expert
		virtual void addcopynew(fhrPartSet* fset); // assuming both sets ordered only adds parts ending after last part in set
		virtual bool checkAddSorted(fhrPart* f);
		virtual void clear(void);
		virtual void clearMemory(void);
		virtual void copyOrigX(void);  // set x1orig, x2orif to current x1, x2 for all parts
		virtual void debugDisplay(void);
		virtual void deleteAt(long i);
		virtual bool empty(void) {return p.empty();}
		virtual void filterByType(fhrPart::FhrPartType t);
		virtual void filterByDecelSubtype(fhrPart::FhrPartType t);
		virtual void filterByLength(long minLen);
		virtual void filterEndingAfter(long x);
		virtual void filterEndingBefore(long x);
		virtual void filterStartingAfter(long x);
		virtual void filterStartingBefore(long x);
		virtual fhrPart* getAt(long i) {return p[i];} // returns pointer to element i
		virtual long getEarliestUncommitted();
		virtual long getIndexFromX1X2(long x1, long x2);
		virtual fhrPart* getLast() {return p[size()-1];}
		//virtual fhrPart* getCopyAt(long i); // returns pointer to copy of element i - caller will need to deal with delete
		virtual long getNumPending(void);
		virtual void insert(fhrPart* f, long i);
		virtual void insertcopy(fhrPart* f, long i); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void insertcopy(baseline* f, long i); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void insertcopy(bump* f, long i); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void insertcopy(decel* f, long i); // makes actual copy of fhrPart before adding pointer tp copy
		virtual void insertcopy(accel* f, long i); // makes actual copy of fhrPart before adding pointer tp copy
		virtual bool isSorted(void) {return sorted;}
		virtual void mergeOverlapping(void);
		virtual void nonBasToBas(void);
		virtual void removeAt(long i);	
		virtual void removeAt(long i1, long i2);
		virtual void removeNonInterp(void);
		virtual void removePending(void);
		virtual void removeCommitted(void);
		virtual void removeRepeat(void);
		virtual void setAsPendingEndingAfter(long x);
		virtual void setAsNonPending();
		virtual void setClearMemory(bool b) {doClearMemory = b;}
		virtual bool getClearMemory(void) {return doClearMemory;}
		virtual long size(void) {return (long) p.size();}
		virtual void sortByEnd(void);
		virtual void sortOn(long n, long *ind, long *x);
		virtual void toAbsTime(long offset); // shift to absolute
		virtual void toRelativeTime(long offset); // shift to relative
		virtual void setAsAbsTime(bool b); // mark parts as absolute / relative
		virtual void setTimestamp(long t);
	//	virtual void fromRepIntervals(CRepairInterval *r, long n);
		//virtual void fromFetus(fetus *f);

		virtual fhrPartSet &operator =(fhrPartSet &f);
		virtual fhrPartSet &operator +=(long x);
		virtual fhrPartSet &operator -=(long x);
		// additional functions for BP2Processing
		bool Contains(long x, int startIndex, int& outIndex);
		int FindIndexByX(long x, int start, int end);
		long EndOfLastRepairInterval();
		
	};
}

		

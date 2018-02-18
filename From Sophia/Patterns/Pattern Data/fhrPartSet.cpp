#include "stdafx.h"
#include "fhrPartSet.h"




namespace patterns
{
	/*
	=======================================================================================================================
		Construction and destruction.
	=======================================================================================================================
	*/
	fhrPartSet::fhrPartSet()
	{
		sorted = true;
		doClearMemory = true;
	}

	fhrPartSet::~fhrPartSet(void)
	{
		clear();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::clearMemory()
	{
		for (long i = 0; i < size(); i++)
		{
			deleteAt(i);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::clear()
	{
		if (getClearMemory())
		{
			clearMemory();
		}
		p.clear();
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::deleteAt(long i)
	{
		delete (getAt(i));
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	void fhrPartSet::add(fhrPart* f)
	{
		sorted = checkAddSorted(f);
		p.push_back(f);
	}




	/*
	=======================================================================================================================
	add:  fhrPartSet.  Add contents of fset to this.  Note that will have common pointers between fset and this - if want to 
						keep data in this make sure to call destructor for fset with (false) so that don't delete data
	=======================================================================================================================
	*/
	void fhrPartSet::add(fhrPartSet* fset)
	{
		for (long i = 0; i < fset->size();i++)
		{
			fhrPart* f = fset->getAt(i);
			add(f);
		}	
		fset->setClearMemory(false);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(fhrPart* f)
	{
		if (f->isBaseline())
		{
			addcopy((baseline *) f);
		}
		else if ((f->isDecel()) || (f->isNonDecel()))
		{
			addcopy((decel *) f);
		}
		else if ((f->isAccel()) || (f->isNonAccel()))
		{
			addcopy((accel *) f);
		}
		else
		{
			fhrPart* fcopy = new fhrPart;
			(*fcopy) = (*f);
			add(fcopy);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(event* e)
	{
		if (e->is_acceleration())
		{
			accel *a = new accel;
			a->fromEvent(e);
			add(a);
		}
		else if (e->is_deceleration())
		{
			decel *d = new decel;
			d->fromEvent(e);
			add(d);
		}
		else if (e->is_baseline())
		{
			baseline *b = new baseline;
			b->fromEvent(e);
			add(b);
		}
		else
		{
			fhrPart *p = new fhrPart;
			p->fromEvent(e);
			add(p);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(contraction* c)
	{
		fhrPart *p = new fhrPart;
		p->fromContraction(c);
		add(p);
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(baseline* f)
	{
		baseline* fcopy = new baseline;
		(*fcopy) = (*f);
		add(fcopy);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(bump* f)
	{
		bump* fcopy = new bump;
		(*fcopy) = (*f);
		add(fcopy);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(decel* f)
	{
		decel* fcopy = new decel;
		(*fcopy) = (*f);
		add(fcopy);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(accel* f)
	{
		accel* fcopy = new accel;
		(*fcopy) = (*f);
		add(fcopy);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(fhrPartSet* fset)
	{
		addcopy(fset, -LONG_MAX);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopy(fhrPartSet* fset, long minX1)
	{
		for (long i = 0; i < fset->size(); i++)
		{
			if (fset->getAt(i)->getX1() >= minX1)
			{
				fhrPart *p = fset->getAt(i);
				if (p->isDecel() || p->isNonDecel())
				{
					decel* d;
					d = (decel*) fset->getAt(i);
					addcopy(d);
				}
				else if (p->isAccel() || p->isNonAccel())
				{
					accel* a;
					a = (accel*) fset->getAt(i);
					addcopy(a);
				}
				else if (p->isBaseline())
				{
					baseline* b;
					b = (baseline*) fset->getAt(i);
					addcopy(b);
				}
				else
				{
					addcopy(p);	
				}
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::addcopyUncommitted(fhrPartSet* fset)
	{
		long n = fset->getEarliestUncommitted();
		if (n >= 0)
		{
			for (long i = n; i < fset->size(); i++)
			{
				addcopy(fset->getAt(i));
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	void fhrPartSet::addcopynew(fhrPartSet* fset)
	{
		long minX1 = 0;
		if (!(fset->isSorted()))
		{
			fset->sortByEnd();
		}
		if (!(isSorted()))
		{
			sortByEnd();
		}

		if (size() > 0)
		{
			minX1 = getAt(size()-1)->getX2() + 1;
		}
		addcopy(fset, minX1);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool fhrPartSet::checkAddSorted(fhrPart* f)
	{
		bool b = isSorted();
		if (b)
		{
			if (size() > 0)
			{ // if sorted verify still sorted after adding new part
				b = (getAt(size() - 1)->endsBefore(*f));
			}
		}
		return(b);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::copyOrigX()
	{
		for (long i = 0; i < size(); i++)
		{
			getAt(i)->copyOrigX();
		}
		
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::debugDisplay(void)
	{
		for (long i = 0; i < size(); i++)
		{
			getAt(i)->debugDisplay();
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::filterByType(fhrPart::FhrPartType t)
	{
		long i, rem = 0, n = size();
		long *remInd = new long[n];

		for (i = 0; i < n; i++)
		{
			fhrPart *f = getAt(i);
			if (f->fhrPart::getType() != t)
			{
				remInd[rem++] = i;
			}
		}

		for (i = rem-1; i >= 0; i--)
		{
			removeAt(remInd[i]);
		}

		if (remInd)
		{
			delete [] remInd;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::filterByDecelSubtype(fhrPart::FhrPartType t)
	{
		long i, rem = 0, n = size();
		long *remInd = new long[n];

		for (i = 0; i < n; i++)
		{
			if (getAt(i)->isDecel())
			{
				decel *d = (decel*) getAt(i);
				if (d->getSubtype() != t)
				{
					remInd[rem++] = i;
				}
			}
			else
			{
				remInd[rem++] = i;
			}
		}

		for (i = rem-1; i >= 0; i--)
		{
			removeAt(remInd[i]);
		}

		if (remInd)
		{
			delete [] remInd;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::filterByLength(long minLen)
	{
		long i, rem = 0, n = size();
		long *remInd = new long[n];

		for (i = 0; i < n; i++)
		{
			fhrPart *f = getAt(i);
			if (f->length() < minLen)
			{
				remInd[rem++] = i;
			}
		}

		for (i = rem-1; i >= 0; i--)
		{
			removeAt(remInd[i]);
		}

		if (remInd)
		{
			delete [] remInd;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::filterEndingAfter(long x)
	{
		long n = size() - 1;

		if (!(isSorted()))
		{
			sortByEnd();
		}

		while((n >= 0) && (getAt(n)->getX2() > x))
		{
			n--;
		}

		if (n < size() - 1)
		{
			removeAt(n+1, size()-1);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::filterEndingBefore(long x)
	{
		long n = 0;

		if (!(isSorted()))
		{
			sortByEnd();
		}

		while((n < size()) && (getAt(n)->getX2() < x))
		{
			n++;
		}

		if (n > 0)
		{
			removeAt(0, n-1);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::filterStartingAfter(long x)
	{
		long n = size() - 1;

		if (!(isSorted()))
		{
			sortByEnd();
		}

		while((n >= 0) && (getAt(n)->getX1() > x))
		{
			n--;
		}

		if (n < size() - 1)
		{
			removeAt(n+1, size() - 1);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::filterStartingBefore(long x)
	{
		long n = 0;

		if (!(isSorted()))
		{
			sortByEnd();
		}

		while((n < size()) && (getAt(n)->getX1() < x))
		{
			n++;
		}

		if (n > 0)
		{
			removeAt(0, n-1);
		}
	}

	/*
	=======================================================================================================================
	return index of earliest uncommitted part at end of fhrPartSet.  
	=======================================================================================================================
	*/
	long fhrPartSet::getEarliestUncommitted()
	{
		long n = -1;
		long i = size() - 1;
		while ((i >= 0) && (!(getAt(i)->isCommitted())))
		{
			n = i;
			i--;
		}

		return(n);
	}

	/*
	=======================================================================================================================
	  find index in fhrPartSet of part that has most exact overlap with x1, x2
	  return -1 if not found
	  Originally conceived for updating committed flag on preMerge bump buffers based on final merged output
	=======================================================================================================================
	*/
	long fhrPartSet::getIndexFromX1X2(long x1, long x2)
	{
		long n = size() - 1;
		double maxOverlap = 0, currOverlap;
		long index = -1;
		bool giveUp = false;

		sortByEnd();
		while ((n >= 0) && (!giveUp))
		{
			fhrPart *p = getAt(n);
			if (p->intersects(x1, x2))
			{
				currOverlap = p->percOverlap(x1, x2);
				if (currOverlap > maxOverlap)
				{
					maxOverlap = currOverlap;
					index = n;
				}
			}
			else if (p->getX2() < x1)
			{
				giveUp = true;
			}
			n--;
		}

		return(index);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	long fhrPartSet::getNumPending()
	{
		long n = 0;
		for (long i = 0; i < size(); i++)
		{
			if (getAt(i)->isPending())
			{
				n++;
			}
		}
		return(n);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::insert(fhrPart* f, long i)
	{
		p.insert(p.begin() + i, f);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::insertcopy(fhrPart* f, long i)
	{
		if (f->isDecel())
		{
			decel* d;
			d = (decel*) f;
			insertcopy(d, i);
		}
		else if (f->isAccel())
		{
			accel* a;
			a = (accel*) f;
			insertcopy(a, i);
		}
		else if (f->isBaseline())
		{
			baseline* b;
			b = (baseline*) f;
			insertcopy(b, i);
		}
		else
		{
			fhrPart* fcopy = new fhrPart;
			(*fcopy) = (*f);
			insert(fcopy, i);
		}
		
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::insertcopy(baseline* f, long i)
	{
		baseline* fcopy = new baseline;
		(*fcopy) = (*f);
		insert(fcopy, i);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::insertcopy(bump* f, long i)
	{
		bump* fcopy = new bump;
		(*fcopy) = (*f);
		insert(fcopy, i);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::insertcopy(decel* f, long i)
	{
		decel* fcopy = new decel;
		(*fcopy) = (*f);
		insert(fcopy, i);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::insertcopy(accel* f, long i)
	{
		accel* fcopy = new accel;
		(*fcopy) = (*f);
		insert(fcopy, i);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::mergeOverlapping(void)
	{
		for (long i = size() - 1; i > 0; i--)
		{
			if (getAt(i)->merge(*(getAt(i-1))))
			{
				removeAt(i);
			}
		}
	}
	/*
	=======================================================================================================================
	Force mhClass of any baselines to true.  For o/p baselines so that test client considers them as real baselines
	=======================================================================================================================
	*/
	void fhrPartSet::nonBasToBas(void)
	{
		baseline *b;

		for (long i = 0; i < size(); i++)
		{
			if (getAt(i)->isBaseline())
			{
				b = (baseline *) getAt(i);
				b->setMhClass(true);
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::removeAt(long i)
	{
		if (getClearMemory())
		{
			deleteAt(i);
		}
		p.erase(p.begin()+i);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::removeAt(long i1, long i2)
	{
		if (i1 == i2)
		{
			removeAt(i1);
		}
		else
		{
			if (getClearMemory())
			{
				for (long i = i1; i <= i2; i++)
				{
					deleteAt(i);
				}
			}
			p.erase(p.begin()+i1, p.begin()+ i2 + 1);
		}
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::removePending()
	{
		long n = size();
		long i;

		for (i = n-1; i >= 0; i--)
		{
			if (getAt(i)->isPending())
			{
				removeAt(i);
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::removeNonInterp()
	{
		long n = size();
		long i;

		for (i = n-1; i >= 0; i--)
		{
			if (getAt(i)->isNonInterp())
			{
				removeAt(i);
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::removeCommitted()
	{
		long n = size();
		long i;

		for (i = n-1; i >= 0; i--)
		{
			if (getAt(i)->isCommitted())
			{
				removeAt(i);
			}
		}
	}
	
	/*
	=======================================================================================================================
	remove parts with same x1, x2
	=======================================================================================================================
	*/
	void fhrPartSet::removeRepeat()
	{
		long n = size();
		long i;
		fhrPart *a, *b;
		for (i = n-1; i > 0; i--)
		{
			a = getAt(i);
			b = getAt(i-1);
			if ((a->getX1() == b->getX1()) && (a->getX2() == b->getX2()))
			{
				removeAt(i);
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::setAsPendingEndingAfter(long x)
	{
		long n = size() - 1;

		if (!(isSorted()))
		{
			sortByEnd();
		}

		while((n >= 0) && (getAt(n)->getX2() > x))
		{
			getAt(n)->setPending(true);
			n--;
		}

	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::setAsNonPending()
	{
		for (long i = 0; i < size(); i++)
		{
			getAt(i)->setPending(false);
		}
	}


	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::sortByEnd()
	{
		if (!isSorted())
		{
			long n = size();
			long *ind = new long[n];
			long *x = new long[n];
			long i;
			/*~~~~~~~~~~~~~~~~~~*/

			for (i = 0; i < n; i++)
			{
				ind[i] = i;
				x[i] = (getAt(i)->getX2());
			}

			sortOn(n, ind, x); // sorts indexes ind based on values x
			setClearMemory(false);
			fhrPartSet temp = *this;
			clear();

			for (i = 0; i < n; i++)
			{
				add(temp.getAt(ind[i]));
			}
			sorted = true;
			setClearMemory(true);

			if (ind)
			{
				delete [] ind;
			}
			if (x)
			{
				delete [] x;
			}
		}
	}
		
	/*
	=======================================================================================================================
	Can use bubble sort as sets should be nearly sorted and not that long
	Sort based on values in x, sorted indexes go in ind.
	=======================================================================================================================
	*/
	void fhrPartSet::sortOn(long n, long *ind, long *x)
	{

		long lTmp;
		long iTmp;
		long i, j;
			
		for (i = n - 1; i >= 0; i--)
		{
			for (j = 1; j <= i; j++)
			{
				if (x[j - 1] > x[j])
				{
					
					lTmp = x[j - 1];
					iTmp = ind[j - 1];
					x[j - 1] = x[j];
					x[j] = lTmp;
					ind[j - 1] = ind[j];
					ind[j] = iTmp;
				}
			}
		}
		
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::toRelativeTime(long offset)
	{
		for (long i = 0; i < size(); i++)
		{	
			getAt(i)->toRelativeTime(offset);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::toAbsTime(long offset)
	{
		for (long i = 0; i < size(); i++)
		{	
			getAt(i)->toAbsTime(offset);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::setAsAbsTime(bool b)
	{
		for (long i = 0; i < size(); i++)
		{
			getAt(i)->setAbsCoords(b);
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void fhrPartSet::setTimestamp(long t)
	{
		for (long i = 0; i < size(); i++)
		{
			getAt(i)->setTimestamp(t);
		}
	}
	/*
	void fhrPartSet::fromRepIntervals(CRepairInterval *r, long n)
	{
		clear();
		for (long i = 0; i < n; i++)
		{
			fhrPart *p = new fhrPart;
			p->fromRepInterval(&r[i]);
			add(p);
		}
	}
	*/
	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	/*void fhrPartSet::fromFetus(fetus *f)
	{
		clear();
		long n = f->get_number_of_events();
		for (long i = 0; i < n; i++)
		{
			addcopy(f->get_event(i));
		}
	}*/

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	fhrPartSet &fhrPartSet::operator=(fhrPartSet &f)
	{
		clear();
		sorted = f.sorted;
		doClearMemory = true;
		addcopy(&f);
		return *this;
	}

	fhrPartSet &fhrPartSet::operator+=(long x) 
	{
		for (long i = 0; i < size(); i++)
		{
			getAt(i)->operator+=(x);
		}
		return *this;
	}

	fhrPartSet &fhrPartSet::operator-=(long x) 
	{
		for (long i = 0; i < size(); i++)
		{
			getAt(i)->operator-=(x);
		}
		return *this;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	additional functions for BP2Processing
	*/
	int fhrPartSet::FindIndexByX(long x, int start, int end)
	{
		int nLast = size() - 1;
		if(end >= size())
			return -1;

		if(start == end)
		{
			fhrPart *p = getAt(end);
			if (p->intersects(x, x))
				return end;
			return -1;
		}

		if(start == end - 1)
		{
			fhrPart *pFirst = getAt(start);
			fhrPart *pLast = getAt(end);

			if(pFirst->intersects(x,x))
				return start;
			if(pLast->intersects(x,x))
				return end;
			return -1;
		}
	
		int mid = start + (end - start)/2;
	
		fhrPart *pMid = getAt(mid);
		if(pMid->intersects(x, x))
			return mid;

		if(pMid->getX1() > x && mid > start)
		{
			return FindIndexByX(x, start, mid);
		}

		if(pMid->getX2() < x && mid < end)
		{
			return FindIndexByX(x, mid, end);
		}

		return -1;
		
	}

	bool fhrPartSet::Contains(long x, int startIndex, int& outIndex)
	{	
		outIndex = -1;
		bool res = false;
		long n = size() - 1;
		if(n < 0)
			return false;
		sortByEnd();

		fhrPart *pFirst = getAt(startIndex);
		fhrPart *pLast = getAt(n);

		if(pFirst->intersects(x,x))
		{
			outIndex = startIndex;
			return true;
		}
		else if(pFirst->getX1() > x)
				return false;
		if(pLast->intersects(x,x))
		{
			outIndex = n;
			return true;
		}
		else if(pLast->getX2() < x)
			return false;
		
		int index = FindIndexByX(x, startIndex, n);
		outIndex = index;

		return (index != -1);
		
	}

	long fhrPartSet::EndOfLastRepairInterval()
	{
		long n = size() - 1;
		if(n < 0)
			return 0;
		sortByEnd();
		fhrPart *pLast = getAt(n);
		return pLast->getX2();
	}

}
		









	
#include "stdafx.h"
#include "Signal.h"

#pragma push_macro("min")
#pragma push_macro("max")

#undef min
#undef max

namespace filter
{
	const Signal::sizeT Signal::defaultAbsOffset=0; 
	const Signal::sizeT Signal::defaultAbsInc=1; 
	const double Signal::defaultTs=1.0; 

	// =================================================================================================================
	//    Construction and destruction.
	// ===================================================================================================================
	Signal::Signal(const Signal& this2)
	{
		this->absOffset = this2.absOffset;
		this->absInc = this2.absInc;
		this->Ts = this2.Ts;
		this->samples = vector<double>(this2.samples);
	}

	Signal::Signal(long absOffset, sizeT absInc, double Ts)
		:absOffset(absOffset),absInc(absInc),Ts(Ts),samples()
	{
	}

	//Signal::Signal(double initVal, const long length, long absOffset, sizeT absInc, double Ts)
	//	:absOffset(absOffset),absInc(absInc),Ts(Ts),samples(length, initVal)
	//{
	//}

	Signal::Signal(const vector<double>& samples, long absOffset, sizeT absInc, double Ts)
		:absOffset(absOffset), absInc(absInc),Ts(Ts), samples(samples)
	{
	}

	Signal::Signal(vector<double>::const_iterator &first, vector<double>::const_iterator &last, long absOffset, sizeT absInc, double Ts)
		:samples(first, last), absOffset(absOffset), absInc(absInc),Ts(Ts)
	{
	}

	Signal::Signal(const vector<char>& charSamples, long absOffset, sizeT absInc, double Ts)
		:absOffset(absOffset),absInc(absInc),Ts(Ts)
	{
		this->samples = vector<double>(charSamples.size());
		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			(*this)[i] = (double) (unsigned char) charSamples[i];
		}
	}

	Signal::Signal(const vector<sizeT>& samples, long absOffset, sizeT absInc, double Ts)
		:absOffset(absOffset),absInc(absInc),Ts(Ts)
	{
		this->samples = vector<double>(samples.size());
		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			(*this)[i] = (double) samples[i];
		}
	}

	Signal::Signal(const double sampleArr[], sizeT size_t, long absOffset, sizeT absInc, double Ts)
		:samples(sampleArr, sampleArr + size_t),absOffset(absOffset),absInc(absInc),Ts(Ts)
	{
	}

	Signal::Signal(const pair<double, double> aPair, long absOffset, sizeT absInc, double Ts)
		:absOffset(absOffset),absInc(absInc),samples(2),Ts(Ts)
	{
		this->samples[0] = aPair.first;
		this->samples[1] = aPair.second;
	}



	Signal &Signal::operator=(const Signal& this2)
	{
		this->absOffset = this2.absOffset;
		this->absInc = this2.absInc;
		this->Ts = this2.Ts;
		this->samples = vector<double>(this2.samples);

		return *this;
	}

	void Signal::toFloor(vector<int> &floorSamples)
		const
	{
		floorSamples.clear();
		floorSamples.resize(this->samples.size());

		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			floorSamples[i] =	(int) floor(this->samples[i]);
		}
	}


	void Signal::toChar(vector<char> &charSamples) const
	{
		charSamples.clear();
		charSamples.resize(this->samples.size());
		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			charSamples[i] = (unsigned char) floor((*this)[i]);
		}
	}

	void Signal::toUnsigned(vector<sizeT> &unSignedSamples) const
	{
		unSignedSamples.clear();
		unSignedSamples.resize(this->samples.size());
		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			unSignedSamples[i] = (sizeT) floor((*this)[i]);
		}
	}

	Signal*							/* returns a portion of the signal within the given indices */
	Signal::extract(vector<double>::const_iterator& i1, vector<double>::const_iterator& i2)
		const
	{
		if (i2 <= this->samples.end())
		//if (long(i2-this->samples.begin())<=this->samples.size())
			//return new Signal(i1, i2, sizeT(this->absOffset+(i1-this->samples.begin())*this->absInc), 
			//                          this->absInc, this->Ts);
			return new Signal(i1, i2, this->absOffset+long(i1-this->samples.begin())*this->absInc, 
			                          this->absInc, this->Ts);
		else
			return 0;
	}

	Signal*								/* returns a  portion of the signal within the absolute indices */
	Signal::extract(long a1, long a2)
		const
	{
		long i1 = a1>this->absOffset ? (a1-this->absOffset)/this->absInc : 0;
		long i2 = a2>this->absOffset ? (a2-this->absOffset)/this->absInc : 0;
		return extract(this->samples.begin() + i1,
					   this->samples.begin() + i2);
	}

	Signal*								/* returns a  portion of the signal within the absolute indices */
	Signal::extract(double d1, double d2)
		const
	{
		assert(false);
		return new Signal(long(0), 1, 1.0);
	}

	vector<double>::iterator Signal::erase(vector<double>::iterator first, 
			                                        vector<double>::iterator last)
	{
		if (first == this->samples.begin())
		{
			Signal::sizeT offset = Signal::sizeT(last - this->samples.begin());
			this->absOffset += offset;
		}
		return this->samples.erase(first, last);
	}

	Signal&								/* returns the superposition of this Signal and another */
	Signal::add(
		const Signal &this2) 
	{
		assert(this2.samples.size() == this->samples.size());

		//for (int i = 0; i < len1; i++) 
		//{
		//	this->samples[i] += this2.samples[i];
		//}
		transform (this->samples.begin(), this->samples.end(), 
			       this2.samples.begin(), this->samples.begin(), plus<double>());

		return *this;
	}

	Signal&								/* returns the difference of this Signal and another */
	Signal::diff(
		const Signal &this2) 
	{
		assert(this2.samples.size() == this->samples.size());

		//for (int i = 0; i < len1; i++) 
		//{
		//	this->samples[i] += this2.samples[i];
		//}
		transform (this->samples.begin(), this->samples.end(), 
			       this2.samples.begin(), this->samples.begin(), minus<double>());

		return *this;
	}

	Signal& /* returns the product of the signal and another signal */
	Signal::mult(const Signal &this2)
	{
		transform (this->samples.begin(), this->samples.end(), 
			       this2.samples.begin(), this->samples.begin(), multiplies<double>());

		return *this;
	}

	Signal& /* returns the quotient of the signal and another signal */
	Signal::divide(const Signal &this2)
	{
		transform (this->samples.begin(), this->samples.end(), 
			       this2.samples.begin(), this->samples.begin(), divides<double>());

		return *this;
	}

	Signal::sizeT /* returns the number of non-NaN samples */
	Signal::getNValidSamples(void)					 
		const
	{
		sizeT nValid = 0;

		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			if (!_isnan((*this)[i])) 
			{
				nValid += 1;
			}
		}

		return nValid;
	}

	double /* return the signal sum */
	Signal::sum(
		sizeT &nValid)					 /* return: number of valid samples */
		const
	{
		double theSum = 0;
		nValid = 0;

		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			if (!_isnan((*this)[i])) 
			{
				theSum += (*this)[i];
				nValid += 1;
			}
		}
		return theSum;
	}
	
	Signal& /* return the signal cumulative sum */
	Signal::cumSum(
		sizeT &nValid)					 /* return: number of valid samples */
	{
		double theSum = 0;
		nValid = 0;

		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			if (!_isnan((*this)[i])) 
			{
				theSum += (*this)[i];
				(*this)[i] = theSum;
				nValid += 1;
			}
		}
		return *this;
	}
	
	double /* return the signal integration */
	Signal::integrate(
		sizeT &nValid)					 /* return: number of valid samples */
		const
	{
		double theSum = 0;
		nValid = 0;

		for (sizeT i = 0; i < this->samples.size(); i++) 
		{
			if (!_isnan((*this)[i])) 
			{
				theSum += (*this)[i]*this->absInc;
				nValid += 1;
			}
		}
		return theSum;
	}
	
	double /* return the signal mean */
	Signal::mean(void)
		const
	{
		assert(!this->samples.empty());

		sizeT nValid = 0;
		double theSum = sum(nValid);
		return theSum/nValid;
	}
	
	double /* return the signal variance */
	Signal::variance(void)     
		const
	{
		assert(!this->samples.empty());

		Signal uSquared = Signal(*this);
		uSquared.squared();
		double theMean = mean();
		return uSquared.mean() - theMean*theMean;
	}

	vector<double>::const_iterator /* return the signal minimum value */
	Signal::min_element(void)
		const
	{
		assert(!this->samples.empty());

		return std::min_element(this->samples.begin(), this->samples.end());
	}
	
	vector<double>::const_iterator /* return the signal minimum value */
	Signal::max_element(void)
		const
	{
		assert(!this->samples.empty());

		return std::max_element(this->samples.begin(), this->samples.end());
	}
	
	Signal& /* returns the signal squared */
	Signal::squared(void)
	{
		transform (this->samples.begin(), this->samples.end(), 
			       this->samples.begin(), this->samples.begin(), multiplies<double>());

		return *this;
	}

	Signal& /* returns the signal square root */
	Signal::squareRoot(void)
	{
		transform (this->samples.begin(), this->samples.end(), 
			       this->samples.begin(), std::ptr_fun<double, double>(sqrt));
		
		return *this;
	}


	Signal& /* returns a scaled signal */
	Signal::scale(double scale)           /* scale factor */
	{
		transform (this->samples.begin(), this->samples.end(), 
			       this->samples.begin(), std::bind1st(std::multiplies<double>(), scale));

		return *this;
	}

	Signal& /* returns a signal raised to the given exponent*/
	Signal::power(double thePower)           /* the exponent */
	{
		transform (this->samples.begin(), this->samples.end(), 
			this->samples.begin(), std::bind2nd(std::ptr_fun((double (*)(double, int))std::pow), thePower));

		return *this;
	}

	Signal& /* applies an offset to the signal */
	Signal::offset(
		double offset)           /* the offset */
	{
		transform (this->samples.begin(), this->samples.end(), 
			       this->samples.begin(), std::bind1st(std::plus<double>(), offset));

		return *this;
	}

	Signal& /* returns the signal natural logarithm */
	Signal::log(void)
	{
		transform (this->samples.begin(), this->samples.end(), 
			this->samples.begin(), std::ptr_fun<double, double>(::log));
		
		return *this;
	}


	static double LOG2 = ::log(2.0);
	double Log2( double n )  
	{  
		return n==0.0? 0: ::log( n ) / LOG2;  
	}

	Signal& /* returns the signal base-10 logarithm */
	Signal::log2(void)
	{
		transform (this->samples.begin(), this->samples.end(), 
			this->samples.begin(), std::ptr_fun<double, double>(Log2));
		
		return *this;
	}


	Signal& /* returns the signal base-10 logarithm */
	Signal::log10(void)
	{
		transform (this->samples.begin(), this->samples.end(), 
			this->samples.begin(), std::ptr_fun<double, double>(::log10));
		
		return *this;
	}

	Signal& /* returns the signal with the slope between the first and last sample removed */
	Signal::removeTrend(void)
	{
		double slope = (this->samples[118]-this->samples[10])/(118-10+1);
		Signal slopeSignal = Signal(this->absOffset, this->absInc, this->Ts);
		slopeSignal.resize(this->samples.size());
		Signal::generateRamp(slopeSignal);
		slopeSignal.scale(-slope).offset(this->samples.size()*slope);
		this->add(slopeSignal);

		return *this;
	}


	Signal& /* return filtered signal with delay removed */
	Signal::firFilter(
		const Signal &h)     /*  FIR filter */
	{
		int lenH = h.samples.size();
		int filterDelay = (int) floor(lenH/2.0);

		int lenU = this->samples.size();

		Signal y(this->absOffset, this->absInc, this->Ts);
		y.resize(lenU);

		for (long i = 0; i < lenU; i++)
		{
			double value = 0;
			for (long k = i; k > std::max(long(0), long(i - lenH + 1)); k--)
			{
				value += (*this)[k] * h[i - k];
			}
			y[i] = value;
		}
		
		*this = y;
		
		return *this;
	}

	Signal& /* return filtered signal with delay removed */
	Signal::firFilterDelayRemoved(
		const Signal &h)     /*  FIR filter */
	{
		// augment signal to cancel delay
		int lenH = h.samples.size();
		int filterDelay = (int) floor(lenH/2.0);
		vector<double> pad = vector<double>(filterDelay, 0);
		this->samples.insert(this->samples.end(), pad.begin(), pad.end()); 
		int lenU = this->samples.size();
		this->absOffset += lenH;

		*this = firFilter(h);
		
		// remove delay
		this->erase(this->begin(), this->begin()+filterDelay);
		
		return *this;
	}

	Signal&              /* returns signal with values below thresh set to NaNs */
	Signal::belowThreshToNan(
	double thresh)		
	{
		sizeT count = 0;
		for (sizeT i = 0; i < this->samples.size(); i++)
		{
			if ((*this)[i] <= thresh)
			{
				(*this)[i] = NaN;
				count += 1;
			}	
		}
		return *this;
	}

	Signal&              /* returns signal with values below thresh set to NaNs */
	Signal::aboveThreshToNan(
	double thresh)		
	{
		for (sizeT i = 0; i < this->samples.size(); i++)
		{
			if ((*this)[i] >= thresh)
				(*this)[i] = NaN;
		}
		return *this;
	}

	Signal&              /* returns signal with fhrPartSet used as mask for NaNs */
	Signal::fhrPartToNan(
	patterns::fhrPartSet& anFhrPartSet)		/* input fhrPartSet */
	{
		assert(anFhrPartSet.size()>0 ? (sizeT)anFhrPartSet.getLast()->getX2() < this->size() : true);

		for (int i = 0; i < anFhrPartSet.size(); i++)
		{
			patterns::fhrPart* fp = anFhrPartSet.getAt(i);
			for (int j = fp->getX1(); j <= fp->getX2(); j ++)
			{
				(*this)[j] = NaN;
			}
		}
		return *this;
	}


	Signal&              /* returns signal zero-order upsampled by the given factor */
	Signal::upSample(
		sizeT factor)		/* up-sampling factor */
	{
		sizeT lenU = this->samples.size();
		Signal y(this->absOffset, this->absInc/factor, this->Ts);
		for (sizeT i = 0; i < lenU; i++)
		{
			vector<double> curr(factor, (*this)[i]);
			y.insert(y.end(), curr.begin(), curr.end());
		}
		*this = y;
		
		return *this;
	}

	void 
	Signal::autoCorr(
		const pair<sizeT, sizeT> lagRange,    /* input: range of lags */
		const pair<sizeT, sizeT> offsetRange, /* input: range of offsets */
		Signal &ac,                           /* output: autocorrelation signal without normalization */
		vector<sizeT> &nValid                 /* output: number of valid samples at each lag calculation*/
		)     
		const
	{
		ac.resize(lagRange.second+1);
		ac.assign(lagRange.second+1, 0);
		nValid.resize(lagRange.second+1);
		nValid.assign(lagRange.second+1, 0);

		for (sizeT j = offsetRange.first; j <= offsetRange.second; j++) 
		{
			for (sizeT i = lagRange.first; i <= std::min(lagRange.second, 
				                                         this->size()-1-j); i++) 
			{
				if (!_isnan((*this)[j]) && !_isnan((*this)[j+i]))
				{
					ac[i] += (*this)[j] * (*this)[j+i];
					nValid[i] += 1;
				}
			}
		}
	}

	void 
	Signal::autoCorr(
		const sizeT &maxLag,	  /* input: maximum number of lags */
		Signal &acSignal,      /* output: autocorrelation signal without normalization*/
		vector<sizeT> &nValid  /* output: number of valid samples at each lag calculation*/
		)     
		const
	{
		pair<sizeT, sizeT> lagRange(0, maxLag);
		pair<sizeT, sizeT> offsetRange(0, this->samples.size()-1);
		autoCorr(lagRange, offsetRange, acSignal, nValid);
	}

	Signal&					// returns signal postpadded with the reversed size_t samples
	Signal::padEnd(sizeT size_t)
	{
		vector<double> yPad(this->samples.end()-size_t, this->samples.end());
		std::reverse(yPad.begin(), yPad.end());

		this->insert(this->end(), yPad.begin(), yPad.end());
		return *this;
	}

	Signal&					// returns signal prepadded with the reversed size_t samples
	Signal::padBegin(sizeT size_t)
	{
		vector<double> yPad(this->samples.begin(), this->samples.begin()+size_t);
		std::reverse(yPad.begin(), yPad.end());

		this->insert(this->begin(), yPad.begin(), yPad.end());

		this->absOffset -= size_t;

		return *this; 
	}

	Signal&					// returns signal postpadded with size_t NaN samples
	Signal::padEndNaN(sizeT size_t)
	{
		vector<double> yPad(size_t, NaN);

		this->insert(this->end(), yPad.begin(), yPad.end());
		return *this;
	}

	Signal&					// returns signal prepadded with size_t NaN samples
	Signal::padBeginNaN(sizeT size_t)
	{
		vector<double> yPad(size_t, NaN);

		this->insert(this->begin(), yPad.begin(), yPad.end());
		return *this;
	}

	Signal&					// returns signal postpadded with size_t last-value samples
	Signal::padEndExtend(sizeT size_t)
	{
		vector<double> yPad(size_t, this->back());

		this->insert(this->end(), yPad.begin(), yPad.end());
		return *this;
	}

	Signal&					// returns signal prepadded with size_t first-value samples
	Signal::padBeginExtend(sizeT size_t)
	{
		vector<double> yPad(size_t, this->front());

		this->insert(this->begin(), yPad.begin(), yPad.end());
		return *this;
	}

	Signal&			// returns a ramp signal
	Signal::generateRamp(Signal &s)
	{
		sizeT nSamples = s.samples.size();
		for (sizeT i = 0; i<nSamples; i++)
		{
			s.samples[i] = i;
		}
		return s;
	}

	Signal&			// linearly transform a signal from one range to another
	Signal::transformRange(pair<double, double> inRange, pair<double, double> outRange)
	{
		this->offset(-inRange.first).scale((outRange.second-outRange.first)/
			                               (inRange.second-inRange.first)).
			            					offset(outRange.first);
		return *this; 
	}



}

#pragma pop_macro("max")
#pragma pop_macro("min")

#pragma once

#include <assert.h>
#include <math.h>
#include <vector>
#include <list>
#include <limits>
#include <cmath>
#include <algorithm>

#include "fhrPartSet.h"

using namespace std;

namespace filter
{		
	/*
	===========================================================================
	* A class to perform signal processing on a 1-dimensional vector of 
	* uniformly sampled signal amplitudes. NaN values are properly handled.
	===========================================================================
	*/

	#define NaN numeric_limits<double>::quiet_NaN()

	class Signal
	{
	public:
		typedef vector<double>::size_type sizeT;

		Signal(const Signal&);
		Signal(long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);
		//Signal(double=0.0, const long length=1, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);
		//Signal(const long length, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs, double=0);

		Signal(const vector<double>&, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);
		Signal(const vector<sizeT>&, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);
		Signal(const vector<char>&, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);
		Signal(const double sampleArr[], sizeT, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);
		Signal(vector<double>::const_iterator&, vector<double>::const_iterator&, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);
		Signal(const pair<double, double>, long=defaultAbsOffset, sizeT=defaultAbsInc, double=defaultTs);

		virtual ~Signal() {};

		virtual Signal* getCopy() const { return new Signal(*this); }

		//virtual Signal &operator=(const Signal& this2);
		Signal &operator=(const Signal& this2);

		virtual void toFloor(vector<int> &) const;
		virtual void toChar(vector<char> &) const;
		virtual void toUnsigned(vector<sizeT> &) const;
		virtual double* toArray() { return &samples[0]; }

		inline virtual sizeT domain_lower_bound(double d) const
		{
			return sizeT(d/this->Ts/this->absInc)-this->absOffset;
		}

		virtual Signal *extract(vector<double>::const_iterator&, vector<double>::const_iterator&) const;
		virtual Signal *extract(long, long) const;
		virtual Signal *extract(double, double) const;

		virtual sizeT getNValidSamples(void) const;

		virtual double sum(sizeT &) const;
		virtual double integrate(sizeT &) const;
		virtual double mean(void) const;
		virtual double variance(void) const;
		virtual vector<double>::const_iterator min_element(void) const;
		virtual vector<double>::const_iterator max_element(void) const;
		virtual void autoCorr(const pair<sizeT, sizeT>, 
			                  const pair<sizeT, sizeT>, 
							  Signal&, vector<sizeT>&) const;
		virtual void autoCorr(const sizeT&, Signal&, vector<sizeT>&) const;

		virtual Signal &add(const Signal&);
		virtual Signal &diff(const Signal&);
		virtual Signal &mult(const Signal&);
		virtual Signal &divide(const Signal&);
		virtual Signal &offset(double);
		virtual Signal &scale(double);
		virtual Signal &squared(void);
		virtual Signal &squareRoot(void);
		virtual Signal &log(void);
		virtual Signal &log2(void);
		virtual Signal &log10(void);
		virtual Signal &power(double);
		virtual Signal &firFilter(const Signal&);
		virtual Signal &firFilterDelayRemoved(const Signal&);
		virtual Signal &removeTrend(void);

		virtual Signal &belowThreshToNan(double thresh);
		virtual Signal &aboveThreshToNan(double thresh);

		virtual Signal &fhrPartToNan(patterns::fhrPartSet&);
		virtual Signal &upSample(sizeT);
		virtual Signal &transformRange(pair<double, double>, pair<double, double>);
		virtual Signal &reverse() { std::reverse(this->samples.begin(), this->samples.end()); return *this; }
		virtual Signal &cumSum(sizeT &);

		inline virtual const vector<double> &getSamples() const {return this->samples;}
		inline virtual const long &getAbsOffset() const {return this->absOffset;}
		inline virtual const sizeT &getAbsInc() const {return this->absInc;}
		inline virtual void setAbsOffset(sizeT in) {this->absOffset = in;}
		inline virtual void setAbsInc(sizeT in) {this->absInc = in;}
		inline virtual void setTs(double in) {this->Ts = in;}
		inline virtual const double &getTs() const {return this->Ts;}
		
		inline virtual const double &at(long index) const {return this->samples.at(index);}
		inline virtual double &at(long index) {return this->samples.at(index);}
		inline virtual void assign ( sizeT n, const double& u ) { this->samples.assign(n, u); }
		inline virtual const double &operator[] ( sizeT index ) const {return this->samples[index];}
		inline virtual double &operator[] ( sizeT index ) {return this->samples[index];}


		inline virtual sizeT size() const { return this->samples.size(); }
		inline virtual void clear() { this->samples.clear(); }
		inline virtual void resize(sizeT size_t) { this->samples.resize(size_t); }
		inline virtual long getEndIndex() const { return this->absOffset + (this->samples.size())*this->absInc; }
		inline virtual sizeT extent() const { return this->samples.size()*this->absInc; }

		inline virtual vector<double>::iterator begin() {return this->samples.begin();}
		inline virtual vector<double>::iterator end() {return this->samples.end();}
		inline virtual vector<double>::const_iterator begin() const {return this->samples.begin();}
		inline virtual vector<double>::const_iterator end() const {return this->samples.end();}

		inline virtual double& back() {return this->samples.back();}
		inline virtual const double& back() const {return this->samples.back();}
		inline virtual double& front() {return this->samples.front();}
		inline virtual const double& front() const {return this->samples.front();}

		inline virtual void insert( vector<double>::iterator &position, 
			                        const double &x)
		{
			this->samples.insert(position, x);
		}
		inline virtual void insert( vector<double>::iterator &position, 
									Signal::sizeT n, const double &x)
		{
			this->samples.insert(position, n, x);
		}
		inline virtual void insert( vector<double>::iterator &position, 
			                        vector<double>::iterator &first, 
									vector<double>::iterator &last )
		{
			this->samples.insert(position, first, last);
		}
		inline virtual void append(const Signal& in)
		{
			this->samples.insert(this->end(), in.begin(), in.end());
		}

		virtual vector<double>::iterator erase(vector<double>::iterator first, 
		                                       vector<double>::iterator last);

		inline void push_back ( const double& x ) { this->samples.push_back(x); }

		virtual Signal&	padBegin(sizeT size_t);
		virtual Signal&	padEnd(sizeT size_t);
		virtual Signal& padBeginNaN(sizeT size_t);
		virtual Signal& padEndNaN(sizeT size_t);
		virtual Signal& padBeginExtend(sizeT size_t);
		virtual Signal& padEndExtend(sizeT size_t);

		static Signal& generateRamp(Signal &s);


	protected:
		long	absOffset;			// base-rate index of first sample
		sizeT	absInc;				// base-rate index increment of each sample 
		double	Ts;					// base-rate sampling period
									// (sampling period of this Signal is absInc*Ts)

		vector<double> samples;		// sampled signal amplitude

	protected:
		static const sizeT defaultAbsOffset;
		static const sizeT defaultAbsInc;
		static const double defaultTs;
	};

}
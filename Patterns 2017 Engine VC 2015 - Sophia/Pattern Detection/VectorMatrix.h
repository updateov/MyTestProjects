// PROJECT: Fetal Heart Rate (FHR) Feature Detection CLASSES: File contains
// class declaration for: PatternsVector: PatternsMatrix : AUTHOR: Mark Doubson MODIFIED: KW
// (29/Jan/2005) - changed names of classes to be specific PatternsVector and PatternsMatrix
// classes. KW (31/Jan/2005) - Added operators and functions to PatternsVector and PatternsMatrix
// classes for full math.
#pragma once
#include <FLOAT.H>

#ifndef NULL
#define NULL	0
#endif

namespace patterns
{

	// general mathematical constants
	static const double machineEpsilon = 5E-16;
	static const double maxRealNumber = 1E300;
	static const double minRealNumber = 1E-300;

	//
	// =======================================================================================================================
	//    general purpose static functions:
	// =======================================================================================================================
	//
	static double sign(double x)
	{
		if (x > 0)
		{
			return 1.0;
		}

		if (x < 0)
		{
			return -1.0;
		}

		return 0;
	}

	//
	// =======================================================================================================================
	//    math standard round static double round(double x) { return floor(x + 0.5);
	//    } ;
	//    MATLAB-like round:
	// =======================================================================================================================
	//
	static double round(double x)
	{
		return sign(x) * floor(fabs(x) + .5);
	}

	//
	// =======================================================================================================================
	//    Round(a) function specific: The whole number nearest a. If a is halfway between two whole numbers, one of which by
	//    definition is even and the other odd, then the even number is returned. The behavior of this method follows IEEE
	//    Standard 754, section 4. This kind of rounding is sometimes called rounding to nearest, or banker's rounding. It
	//    allow to eliminate regular bias in C# standard: Math::Round(4.5);
	//    //Returns 4.0. !!! Math::Round(5.5);
	//    //Returns 6.0. !!! C++ standard: round(4.5) == 5 round(-4.5) == -4 MATLAB: don't use IEEE Standard and NOT USE C++
	//    standard >> disp(round(4.5)) 5 >> disp(round(-4.5)) -5 >> disp(round(5.5)) 6 IEEE Standard version: ;
	//    static double round(double x) { double a = floor(x);
	//    if (abs(x - a - .5) < machineEpsilon) { if ((int) a % 2 == 1) return 1.0 + a;
	//    } return floor(x + 0.5);
	//    }
	// =======================================================================================================================
	//
	static double trunc(double x)
	{
		return x > 0 ? floor(x) : ceil(x);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	static double pi(void)
	{
		return 3.14159265358979323846;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	static double sqr(double x)
	{
		return x * x;
	}

	//
	// =======================================================================================================================
	//    CLASS: PatternsVector - dynamic vector
	// =======================================================================================================================
	//
	class PatternsVector
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		// PatternsVector default constructor
		PatternsVector(void);

		// PatternsVector destructor
		virtual ~PatternsVector(void);

		// PatternsVector copy constructor
		PatternsVector(const PatternsVector &rhs);
		PatternsVector(long size);

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    base methods
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:
		// set Low and Upper Bounds for dynamic array
		void setBounds(long iLow, long iHigh);

		// set content of array of sub-array!
		void setContent(long iLow, long iHigh, const double *pContent);

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		double *getContent(void)
		{
			return m_Vec;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		const double *getContent(void) const
		{
			return m_Vec;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long getLowBound(long = 0) const
		{
			return m_iLow;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long getHighBound(long = 0) const
		{
			return m_iHigh;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long getSize(void) const
		{
			return m_iHigh - m_iLow + 1;
		};

		//
		// ===============================================================================================================
		//    this function will load a vector from a resource
		// ===============================================================================================================
		//
		bool loadFromResource(void)
		{
			return true;
		};

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    operators
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:
		// operator= standard equals operator
		PatternsVector &operator =(const PatternsVector &rhs);

		// const operator() function to access a specific element in an array acts like
		// operator[], done to increase performance.
		const double &operator () (long i) const
		{
			// need to validate this input before the accessing the array need the ability to
			// expand array. if (i - m_iLow > m_iVecSize) resize(i);
			return m_Vec[i - m_iLow];
		};

		// operator() function to access a specific element in an array acts like
		// operator[], done to increase performance. and looks like normal Mathematical
		// notation
		double &operator () (long i)
		{
			if (i - m_iLow >= m_iVecSize)
			{
				resize(i + 1);
			}

			return m_Vec[i - m_iLow];
		};

		// operator + function to add 2 vector need to check sizes of both vectors
		void zero(void);
		void empty(void);
		bool isempty(void);
		void error(void);
		bool iserror(void);
		PatternsVector operator +(const PatternsVector& v);
		PatternsVector operator -(const PatternsVector& v);
		PatternsVector operator *(const double c);
		void debug(void) const; // print entries on a single line
		const void resize(long n);
		int operator ==(const PatternsVector& v) const;

		// friend operators
		friend PatternsVector operator *(double c, PatternsVector& v);
		friend PatternsVector operator *(PatternsVector& v, double c);
		friend double operator *(const PatternsVector& A, const PatternsVector& B); // the dot product of two vectors - since there is no multiple

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	private:
		double *m_Vec;
		long m_iVecSize;
		long m_iLow;
		long m_iHigh;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		static void PatternsVector::CalPolyFit(double *pSignal, int nStart, int nEnd, double &dYStart, double &dYEnd, double &dInterception, double &dSlope)
		{
			// double dInterception(0.0), dSlope(0.0);
			Regression(pSignal, nStart, nEnd, dSlope, dInterception);
			dYStart = dSlope * nStart + dInterception;
			dYEnd = dSlope * nEnd + dInterception;

			return;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		static void PatternsVector::CalPolyFit(double *pSignal, int nStart, int nEnd, double &dYStart, double &dYEnd)
		{
			double dInterception(0.0), dSlope(0.0);
			Regression(pSignal, nStart, nEnd, dSlope, dInterception);
			dYStart = dSlope * nStart + dInterception;
			dYEnd = dSlope * nEnd + dInterception;

			return;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		static bool PatternsVector::Regression(double *pSignal, int nStart, int nEnd, double &dSlope, double &dIntercepte)
		{
			double dM(0.0), dB(0.0);
			double dX(0.0), dY(0.0);

			/*~~~~~~~~~~~~~~~~~~~*/
			double dValue(0.0);
			double dSumWeight(0.0);
			/*~~~~~~~~~~~~~~~~~~~*/

			double dSumX(0.0), dSumY(0.0), dSumXSQ(0.0), dSumYSQ(0.0), dSumXY(0.0);

			/*~~~~~~~~~~~~~~~*/
			double *pdY = NULL;
			double *pdX = NULL;
			/*~~~~~~~~~~~~~~~*/

			if (pSignal != NULL && (nEnd - nStart) > 3)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				// Step 0: Define data structure
				int nNumPoint(nEnd - nStart + 1);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				pdX = new double[nNumPoint];
				pdY = new double[nNumPoint];

				/*~~~~~~*/
				int i = 0;
				/*~~~~~~*/

				for (i = 0; i < nNumPoint; i++)
				{
					pdX[i] = nStart + i;
					pdY[i] = pSignal[nStart + i];
				}

				// Step 1: Prepare linear regression data
				dSumX = 0.0;
				dSumY = 0.0;
				dSumXSQ = 0.0;
				dSumYSQ = 0.0;
				dSumXY = 0.0;
				dSumWeight = 0.0;

				for (i = 0; i < nNumPoint; i++)
				{
					if (pdX[i] > 0 && pdY[i] > 0)
					{
						dY = pdY[i];
						dX = pdX[i];
						dSumX += dX;
						dSumY += dY;
						dSumXSQ += dX * dX;
						dSumYSQ += dY * dY;
						dSumXY += dX * dY;
						dSumWeight += 1.0;
					}
				}

				// Step 2: Perform linear regression
				if (!DoublesEqual(dSumWeight, 0.0, 0))
				{
					dValue = dSumXSQ - dSumX * dSumX / dSumWeight;
				}
				else
				{
					dValue = 0.0;
				}

				if (!DoublesEqual(dValue, 0.0, 0))
				{
					dM = (dSumXY - dSumX * dSumY / dSumWeight) / dValue;
					dB = (dSumY - dM * dSumX) / dSumWeight;
				}

				if (pdX)
				{
					delete[] pdX;
				}

				if (pdY)
				{
					delete[] pdY;
				}
			}

			dSlope = dM;
			dIntercepte = dB;
			return true;
		}

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		static bool PatternsVector::DoublesEqual(double d1, double d2, int nPrecision /* 0 */ )
		{
			if (d1 > 1E20 || d1 < -1E20)
			{
				return false;
			}

			if (d2 > 1E20 || d2 < -1E20)
			{
				return false;
			}

			if (!nPrecision)
			{
				if (d1 - d2 > 1E20 || d1 - d2 < -1E20)
				{
					return false;
				}

				return fabs(d1 - d2) < 1E-10;				// default precision
			}

			if (nPrecision < 0)
			{
				if (d1 - d2 > 1E20 || d1 - d2 < -1E20)
				{
					return false;
				}

				return fabs(d1 - d2) < pow(2.0, -1023.0);	// machine precision
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// RL
			double dLowlimit = -1.0 * (double) nPrecision - 1.0;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			dLowlimit = pow(10.0, dLowlimit);

			// RL
			return fabs(d1 - d2) < dLowlimit;				// given Precision
		}
	};

	//
	// =======================================================================================================================
	//    CLASS: PatternsMatrix - dynamic matrix DESCRIPTION: TO DO
	// =======================================================================================================================
	//
	class PatternsMatrix
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		// PatternsMatrix default constructor
		PatternsMatrix(void);

		PatternsMatrix(long nSize, long mSize);

		// PatternsMatrix destructor
		virtual ~PatternsMatrix(void);

		// PatternsMatrix copy-constructor
		PatternsMatrix(const PatternsMatrix& rhs);

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    base methods
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:
		// set Low and Upper Bounds for dynamic matrix
		void setBounds(long iLow1, long iHigh1, long iLow2, long iHigh2);

		// set content of matrix of sub-matrix!
		void setContent(long iLow1, long iHigh1, long iLow2, long iHigh2, const double *pContent);

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		double *getContent(void)
		{
			return m_Vec;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		const double *getContent(void) const
		{
			return m_Vec;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long getLowBound(long iBoundNum) const
		{
			return iBoundNum == 1 ? m_iLow1 : m_iLow2;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long getHighBound(long iBoundNum) const
		{
			return iBoundNum == 1 ? m_iHigh1 : m_iHigh2;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/

		long getRows(void) const
		{
			return m_iHigh1 - m_iLow1 + 1;
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		long getColumns(void) const
		{
			return m_iHigh2 - m_iLow2 + 1;
		};
		void zero(void);
		void error(void);
		bool iserror(void);
		const void resize(long n, long m);
		void debug(void) const; // print entries on a single line

		//
		// ===============================================================================================================
		//    this function will load a vector from a resource
		// ===============================================================================================================
		//
		bool LoadFromResource(void)
		{
			return true;
		};

		//
		// -------------------------------------------------------------------------------------------------------------------
		//    operators
		// -------------------------------------------------------------------------------------------------------------------
		//
	public:
		// operator= standard equals operator
		const PatternsMatrix& operator=(const PatternsMatrix& rhs);

		// operator() function to access a specific element in matrix acts like
		// operator[], done to increase performance and looks like normal Mathematical
		// notation specially for matrix: A(2,4) instead of A[2][4]!
		const double &operator () (long i1, long i2) const
		{
			return m_Vec[m_iConstOffset + i2 + i1 * m_iLinearMember];
		};

		double &operator () (long i1, long i2)
		{
			long rows = m_iHigh1 - m_iLow1 + 1;
			long cols = m_iHigh2 - m_iLow2 + 1;
			if (i1 + 1 > rows && i2 + 1 > cols)
			{
				// resize the array
				resize(i1 + 1, i2 + 1);
			}
			else if (i1 + 1 > rows)
			{
				// resize the array
				resize(i1 + 1, i2);
			}
			else if (i2 + 1 > cols)
			{
				// resize the array
				resize(i1, i2 + 1);
			}

			return m_Vec[m_iConstOffset + i2 + i1 * m_iLinearMember];
		};

		PatternsVector operator () (long i) const;

		// operator dot product function to create the dot product of a matrix
		PatternsVector operator *(const PatternsVector&);
		PatternsMatrix operator *(const PatternsMatrix& a);
		PatternsMatrix operator +(const PatternsMatrix& a);
		PatternsMatrix operator -(const PatternsMatrix& a);
		int operator ==(const PatternsMatrix& a);

		friend PatternsMatrix operator *(const double&, const PatternsMatrix&);
		friend PatternsMatrix operator *(const PatternsMatrix&, const double&);

		PatternsMatrix transpose(void);
		PatternsVector dot_prod(void);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	private:
		double *m_Vec;
		long m_iVecSize;
		long m_iLow1;
		long m_iLow2;
		long m_iHigh1;
		long m_iHigh2;
		long m_iConstOffset;
		long m_iLinearMember;
	};
}
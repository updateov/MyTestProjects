// PROJECT: Fetal Heart Rate (FHR) Feature Detection CLASSES: File contains
// implementation for: PatternsVector : PatternsMatrix : AUTHOR: Mark Doubson
#include "StdAFX.h"
#include "VectorMatrix.h"


namespace patterns
{
	//
	// =======================================================================================================================
	//    PatternsVector default constructor
	// =======================================================================================================================
	//
	PatternsVector::PatternsVector(void)
	{
		m_Vec = 0;
		m_iVecSize = 0;
		m_iLow = 0;
		m_iHigh = 0;
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	PatternsVector::PatternsVector(long size)
	{
		m_Vec = 0;
		m_iVecSize = size;
		m_iLow = 0;
		m_iHigh = m_iVecSize + m_iLow - 1;

		m_Vec = new double[m_iVecSize];
		zero();
	}

	//
	// =======================================================================================================================
	//    PatternsVector destructor
	// =======================================================================================================================
	//
	PatternsVector::~PatternsVector(void)
	{
		if (m_Vec)
		{
			delete[] m_Vec;
		}

		m_Vec = 0;
		m_iVecSize = 0;
		m_iLow = 0;
		m_iHigh = 0;
	};

	//
	// =======================================================================================================================
	//    PatternsVector copy constructor
	// =======================================================================================================================
	//
	PatternsVector::PatternsVector(const PatternsVector &rhs)
	{
		m_iVecSize = rhs.m_iVecSize;
		m_iLow = rhs.m_iLow;
		m_iHigh = rhs.m_iHigh;
		if (rhs.m_Vec)	// if does not exist we will create memory for it
		{
			m_Vec = new double[m_iVecSize];
			memcpy(m_Vec, rhs.m_Vec, m_iVecSize * sizeof(double));
		}
		else
		{
			m_Vec = 0;
		}
	};

	//
	// =======================================================================================================================
	//    TO DO FUNCTION & DESCRIPTION
	// =======================================================================================================================
	//
	void PatternsVector::setBounds(long iLow, long iHigh)
	{
		if (m_Vec)
		{
			delete[] m_Vec;
		}

		m_iLow = iLow;
		m_iHigh = iHigh;
		m_iVecSize = iHigh - iLow + 1;
		m_Vec = new double[m_iVecSize];
	};

	//
	// =======================================================================================================================
	//    TO DO FUNCTION & DESCRIPTION
	// =======================================================================================================================
	//
	void PatternsVector::setContent(long iLow, long iHigh, const double *pContent)
	{
		setBounds(iLow, iHigh);
		for (long i = iLow; i <= iHigh; i++)
		{
			(*this) (i) = pContent[i - iLow];
		}
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	const void PatternsVector::resize(long n)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~*/
		int nDiff = n - m_iVecSize;
		/*~~~~~~~~~~~~~~~~~~~~~~~*/

		if (nDiff > 0)
		{
			// shrink
			m_iVecSize += nDiff;

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			double *Vec = new double[m_iVecSize];
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (m_Vec != NULL)
			{
				memcpy(Vec, m_Vec, (m_iVecSize - nDiff) * sizeof(double));
			}

			if (m_Vec)
			{
				delete[] m_Vec;
			}

			m_iHigh = m_iLow - 1 + m_iVecSize;
			m_Vec = new double[m_iVecSize];

			if (m_Vec)
			{
				memcpy(m_Vec, Vec, (m_iVecSize - nDiff) * sizeof(double));
			}

			delete[] Vec;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void PatternsVector::zero(void)
	{
		for (int i = 0; i < m_iVecSize; i++)
		{
			m_Vec[i] = (double) 0.0;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void PatternsVector::empty(void)
	{
		for (int i = 0; i < m_iVecSize; i++)
		{
			m_Vec[i] = (double) DBL_MIN;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool PatternsVector::isempty(void)
	{
		/*~~~~~~~~~~~~~*/
		bool bRet = true;
		/*~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iVecSize; i++)
		{
			if (m_Vec[i] != DBL_MIN)
			{
				bRet = false;
				break;
			}
		}

		return bRet;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void PatternsVector::error(void)
	{
		for (int i = 0; i < m_iVecSize; i++)
		{
			m_Vec[i] = (double) DBL_MAX;
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool PatternsVector::iserror(void)
	{
		/*~~~~~~~~~~~~~*/
		bool bRet = true;
		/*~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iVecSize; i++)
		{
			if (m_Vec[i] != DBL_MAX)
			{
				bRet = false;
				break;
			}
		}

		return bRet;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void PatternsVector::debug(void) const
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		// print entries on a single line
		CString strOutFile = "";
		// print entries on a single line
		CString strValue;
		/*~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iVecSize; i++)
		{
			strValue.Format("%f", m_Vec[i]);
			strOutFile += "\t" + strValue;
		}

		strOutFile += "\n\n";
		TRACE(strOutFile);
	}

	//
	// =======================================================================================================================
	//    OPERATORS operator= standard equals operator
	// =======================================================================================================================
	//
	PatternsVector& PatternsVector::operator=(const PatternsVector& rhs)
	{
		if (this == &rhs)
		{
			return *this;
		}

		m_iLow = rhs.m_iLow;
		m_iHigh = rhs.m_iHigh;
		m_iVecSize = rhs.m_iVecSize;
		if (m_Vec)
		{
			delete[] m_Vec;
		}

		if (rhs.m_Vec)
		{
			m_Vec = new double[m_iVecSize];
			memcpy(m_Vec, rhs.m_Vec, m_iVecSize * sizeof(double));
		}
		else
		{
			m_Vec = 0;
		}

		return *this;
	};

	//
	// =======================================================================================================================
	//    operator+ standard plus operator
	// =======================================================================================================================
	//
	PatternsVector PatternsVector::operator+(const PatternsVector& v)
	{
		assert(m_iVecSize == v.m_iVecSize);

		/*~~~~~~~~~~~~~~~~~~~*/
		PatternsVector sum(m_iVecSize);
		/*~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iVecSize; i++)
		{
			sum(i) = m_Vec[i] + v.m_Vec[i];
		}

		return sum;
	}

	//
	// =======================================================================================================================
	//    operator- standard minus operator
	// =======================================================================================================================
	//
	PatternsVector PatternsVector::operator-(const PatternsVector& v)
	{
		assert(m_iVecSize == v.m_iVecSize);

		/*~~~~~~~~~~~~~~~~~~~*/
		PatternsVector sum(m_iVecSize);
		/*~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iVecSize; i++)
		{
			sum(i) = m_Vec[i] - v.m_Vec[i];
		}

		return sum;
	}

	//
	// =======================================================================================================================
	//    operator* standard product operator
	// =======================================================================================================================
	//
	PatternsVector PatternsVector::operator*(const double c)
	{
		/*~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector mult(m_iVecSize);
		/*~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < m_iVecSize; i++)
		{
			mult(i) = m_Vec[i] * c;
		}

		return mult;
	}

	//
	// =======================================================================================================================
	//    operator== standard equals operator
	// =======================================================================================================================
	//
	int PatternsVector::operator==(const PatternsVector& v) const
	{
		if (m_iVecSize != v.m_iVecSize)
		{
			return 0;
		}

		for (int i = 0; i < m_iVecSize; i++)
		{
			if (m_Vec[i] != v.m_Vec[i])
			{
				return 0;
			}
		}

		return 1;
	}

	//
	// =======================================================================================================================
	//    operator* standard equals operator
	// =======================================================================================================================
	//
	PatternsVector operator*(double c, PatternsVector& v)
	{
		/*~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector ans(v.m_iVecSize);
		/*~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < v.m_iVecSize; i++)
		{
			ans(i) = c * v.m_Vec[i];
		}

		return ans;
	}

	//
	// =======================================================================================================================
	//    operator* standard equals operator
	// =======================================================================================================================
	//
	PatternsVector operator*(PatternsVector& v, double c)
	{
		/*~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector ans(v.m_iVecSize);
		/*~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < v.m_iVecSize; i++)
		{
			ans(i) = c * v.m_Vec[i];
		}

		return ans;
	}

	//
	// =======================================================================================================================
	//    dot product of two vector
	// =======================================================================================================================
	//
	double operator*(const PatternsVector& A, const PatternsVector& B)
	{
		/*~~~~~~~~~~~*/
		double sum = 0;
		/*~~~~~~~~~~~*/

		assert(A.m_iVecSize == B.m_iVecSize);

		for (int i = 0; i < A.m_iVecSize; i++)
		{
			sum += A(i) * B(i);
		}

		return sum;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	PatternsMatrix::PatternsMatrix(void)
	{
		m_Vec = 0;
		m_iVecSize = 0;
		m_iLow1 = 0;
		m_iLow2 = 0;
		m_iHigh1 = -1;
		m_iHigh2 = -1;
		m_iConstOffset = 0;
		m_iLinearMember = 0;
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	PatternsMatrix::~PatternsMatrix(void)
	{
		if (m_Vec)
		{
			delete[] m_Vec;
		}
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	PatternsMatrix::PatternsMatrix(const PatternsMatrix& rhs)
	{
		m_iVecSize = rhs.m_iVecSize;
		m_iLow1 = rhs.m_iLow1;
		m_iLow2 = rhs.m_iLow2;
		m_iHigh1 = rhs.m_iHigh1;
		m_iHigh2 = rhs.m_iHigh2;
		m_iConstOffset = rhs.m_iConstOffset;
		m_iLinearMember = rhs.m_iLinearMember;
		if (rhs.m_Vec)
		{
			m_Vec = new double[m_iVecSize];
			memcpy(m_Vec, rhs.m_Vec, m_iVecSize * sizeof(double));
		}
		else
		{
			m_Vec = 0;
		}
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	PatternsMatrix::PatternsMatrix(long nSize, long mSize)
	{
		m_iLow1 = 0;
		m_iHigh1 = nSize - 1;
		m_iLow2 = 0;
		m_iHigh2 = mSize - 1;

		m_iVecSize = (m_iHigh1 - m_iLow1 + 1) * (m_iHigh2 - m_iLow2 + 1);
		m_Vec = new double[m_iVecSize];

		m_iConstOffset = -m_iLow2 - m_iLow1 * (m_iHigh2 - m_iLow2 + 1);
		m_iLinearMember = (m_iHigh2 - m_iLow2 + 1);
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	const PatternsMatrix& PatternsMatrix::operator=(const PatternsMatrix& rhs)
	{
		if (this == &rhs)
		{
			return *this;
		}

		m_iLow1 = rhs.m_iLow1;
		m_iLow2 = rhs.m_iLow2;
		m_iHigh1 = rhs.m_iHigh1;
		m_iHigh2 = rhs.m_iHigh2;
		m_iConstOffset = rhs.m_iConstOffset;
		m_iLinearMember = rhs.m_iLinearMember;
		m_iVecSize = rhs.m_iVecSize;
		if (m_Vec)
		{
			delete[] m_Vec;
		}

		if (rhs.m_Vec)
		{
			m_Vec = new double[m_iVecSize];
			memcpy(m_Vec, rhs.m_Vec, m_iVecSize * sizeof(double));
		}
		else
		{
			m_Vec = 0;
		}

		return *this;
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	void PatternsMatrix::setBounds(long iLow1, long iHigh1, long iLow2, long iHigh2)
	{
		if (m_Vec)
		{
			delete[] m_Vec;
		}

		m_iVecSize = (iHigh1 - iLow1 + 1) * (iHigh2 - iLow2 + 1);
		m_Vec = new double[m_iVecSize];
		m_iLow1 = iLow1;
		m_iHigh1 = iHigh1;
		m_iLow2 = iLow2;
		m_iHigh2 = iHigh2;
		m_iConstOffset = -m_iLow2 - m_iLow1 * (m_iHigh2 - m_iLow2 + 1);
		m_iLinearMember = m_iHigh2 - m_iLow2 + 1;
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	void PatternsMatrix::setContent(long iLow1, long iHigh1, long iLow2, long iHigh2, const double *pContent)
	{
		setBounds(iLow1, iHigh1, iLow2, iHigh2);
		for (long i = 0; i < m_iVecSize; i++)
		{
			m_Vec[i] = pContent[i];
		}
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	void PatternsMatrix::zero(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				m_Vec[m_iConstOffset + j + i * m_iLinearMember] = (double) 0.0;
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void PatternsMatrix::error(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				m_Vec[m_iConstOffset + j + i * m_iLinearMember] = (double) DBL_MIN;
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	bool PatternsMatrix::iserror(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				if (m_Vec[m_iConstOffset + j + i * m_iLinearMember] != (double) DBL_MIN)
				{
					return false;
				}
			}
		}

		return true;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	const void PatternsMatrix::resize(long n, long m)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (n - m_iLow1 > rows || m - m_iLow2 > cols)
		{
			/*~~~~~~~~~~~~~~~~~~*/
			double *OldVec = NULL;
			/*~~~~~~~~~~~~~~~~~~*/

			if (m_Vec)
			{
				OldVec = new double[m_iVecSize];
				memcpy(OldVec, m_Vec, m_iVecSize * sizeof(double));
			}

			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long iOldConstOffset = m_iConstOffset;
			long iOldLinearMember = m_iLinearMember;
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (m_Vec)
			{
				delete[] m_Vec;
			}

			if (m_iHigh1 < n)
			{
				m_iHigh1 = n - 1 + m_iLow1;
			}

			if (m_iHigh2 < m)
			{
				m_iHigh2 = m - 1 + m_iLow2;
			}

			m_iVecSize = (m_iHigh1 - m_iLow1 + 1) * (m_iHigh2 - m_iLow2 + 1);
			m_Vec = new double[m_iVecSize];

			m_iConstOffset = -m_iLow2 - m_iLow1 * (m_iHigh2 - m_iLow2 + 1);
			m_iLinearMember = (m_iHigh2 - m_iLow2 + 1);

			if (OldVec)
			{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < cols; j++)
					{
						m_Vec[m_iConstOffset + j + i * m_iLinearMember] = OldVec[iOldConstOffset + j + i * iOldLinearMember];
					}
				}

				delete[] OldVec;
			}
		}
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	void PatternsMatrix::debug(void) const
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// print entries on a single line
		CString strOutFile = "";
		// print entries on a single line
		CString strValue;
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				strValue.Format("%f", m_Vec[m_iConstOffset + j + i * m_iLinearMember]);
				strOutFile += "\t" + strValue;
			}

			strOutFile += "\n";
			TRACE(strOutFile);
			strOutFile = "";
		}

		strOutFile = "\n\n";
		TRACE(strOutFile);
	}

	// const PatternsVector& PatternsMatrix::operator() (long i) const
	PatternsVector PatternsMatrix::operator () (long i) const
	{
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;

		assert(i <= rows && i > 0);
		PatternsVector row(cols);
		for (int j = 0; j < cols; j++)
		{
			row(i) = m_Vec[m_iConstOffset + j + i * m_iLinearMember];
		}

		return row;
	};

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	PatternsVector PatternsMatrix::operator*(const PatternsVector& v)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		assert(cols == v.getSize());

		/*~~~~~~~~~~~~~*/
		PatternsVector ans(rows);
		/*~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			ans(i) = 0.0;
			for (int j = 0; j < cols; j++)
			{
				ans(i) += m_Vec[m_iConstOffset + j + i * m_iLinearMember] * v(j);
			}
		}

		return ans;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	PatternsMatrix operator*(const double &x, const PatternsMatrix& s)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = s.m_iHigh1 - s.m_iLow1 + 1;
		long cols = s.m_iHigh2 - s.m_iLow2 + 1;
		PatternsMatrix ans(rows, cols);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				ans(i, j) = x * s(i, j);
			}
		}

		return ans;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	PatternsMatrix PatternsMatrix::transpose(void)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = m_iHigh1 - m_iLow1 + 1;
		long cols = m_iHigh2 - m_iLow2 + 1;
		PatternsMatrix ans(cols, rows);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				ans(j, i) = m_Vec[m_iConstOffset + j + i * m_iLinearMember];
			}
		}

		return ans;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	PatternsMatrix operator*(const PatternsMatrix& s, const double &x)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = s.m_iHigh1 - s.m_iLow1 + 1;
		long cols = s.m_iHigh2 - s.m_iLow2 + 1;
		PatternsMatrix ans(rows, cols);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				ans(i, j) = x * s(i, j);
			}
		}

		return ans;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	PatternsMatrix PatternsMatrix::operator*(const PatternsMatrix& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = a.m_iHigh1 - a.m_iLow1 + 1;
		long cols = a.m_iHigh2 - a.m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		assert(cols == rows);

		/*~~~~~~~~~~~~~~~~~~~*/
		PatternsMatrix ans(rows, cols);
		/*~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				ans(i, j) = 0.0;
				for (int k = 0; k < cols; k++)
				{
					ans(i, j) += m_Vec[m_iConstOffset + k + i * m_iLinearMember] * a(k, j);
				}
			}
		}

		return ans;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	PatternsMatrix PatternsMatrix::operator+(const PatternsMatrix& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = a.m_iHigh1 - a.m_iLow1 + 1;
		long cols = a.m_iHigh2 - a.m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		assert(m_iHigh1 - m_iLow1 + 1 == rows);
		assert(m_iHigh2 - m_iLow2 + 1 == cols);

		/*~~~~~~~~~~~~~~~~~~~*/
		PatternsMatrix ans(rows, cols);
		/*~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				ans(i, j) = m_Vec[m_iConstOffset + j + i * m_iLinearMember] + a(i, j);	// faster than assigning vectors?
			}
		}

		return ans;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	PatternsMatrix PatternsMatrix::operator-(const PatternsMatrix& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = a.m_iHigh1 - a.m_iLow1 + 1;
		long cols = a.m_iHigh2 - a.m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		assert(m_iHigh1 - m_iLow1 + 1 == rows);
		assert(m_iHigh2 - m_iLow2 + 1 == cols);

		/*~~~~~~~~~~~~~~~~~~~*/
		PatternsMatrix ans(rows, cols);
		/*~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				ans(i, j) = m_Vec[m_iConstOffset + j + i * m_iLinearMember] - a(i, j);	// faster than assigning vectors?
			}
		}

		return ans;
	}

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/
	int PatternsMatrix::operator==(const PatternsMatrix& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		long rows = a.m_iHigh1 - a.m_iLow1 + 1;
		long cols = a.m_iHigh2 - a.m_iLow2 + 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (m_iHigh1 - m_iLow1 + 1 != rows)
		{
			return 0;
		}

		if (m_iHigh2 - m_iLow2 + 1 != cols)
		{
			return 0;
		}

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				if (m_Vec[m_iConstOffset + j + i * m_iLinearMember] != a(i, j))
				{
					return 0;
				}
			}
		}

		return 1;
	}
}
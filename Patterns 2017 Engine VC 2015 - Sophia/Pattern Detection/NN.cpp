/*
* ! PROJECT: Foetal Heart Rate (FHR) Feature Detection FILE: Neural Net Copyright
* LMS Medical Systems 2004 by Evonium Inc.
*/
#include "stdafx.h"
#include "NN.h"
#include <io.h>

namespace patterns
{

	/*
	=======================================================================================================================
	! NeuralNet::Clear: This function clears the current neural net. All data structures are cleaned and layer are set
	to zero. Once cleared SetInputs() and GetResults() will fail. The net must be reinitilized. \return true iff is
	delete all structures
	=======================================================================================================================
	*/
	bool NeuralNet::Clear(void)
	{
		if (m_W != NULL)
		{
			delete[] m_W;
			m_W = NULL;
		}

		if (m_bias != NULL)
		{
			delete[] m_bias;
			m_bias = NULL;
		}

		if (m_transfer != NULL)
		{
			delete[] m_transfer;
			m_transfer = NULL;
		}

		m_nLayers = 0;

		return true;
	}

	/*
	=======================================================================================================================
	! NeuralNet::getExpectedInputSize: Gets the expected input size. \return The expected input size.
	=======================================================================================================================
	*/
	long NeuralNet::getExpectedInputSize(void)
	{
		if (m_W != NULL && m_nLayers > 0)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long nSize = m_W[0].getRows();
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			return nSize;
		}
		else
		{
			return -1l; // return error
		}
	}

	/*
	=======================================================================================================================
	! NeuralNet::getExpectedOutputSize: Gets the expected output size. \return The expected output size.
	=======================================================================================================================
	*/
	long NeuralNet::getExpectedOutputSize(void)
	{
		if (m_bias != NULL && m_nLayers > 0)
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			long nSize = m_bias[m_nLayers - 1].getSize();
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			return nSize;
		}
		else
		{
			return -1l; // return error
		}
	}

	/*
	=======================================================================================================================
	! NeuralNet::LoadFromResource: This function, given a resource handle, a name and type of resources, will attempt
	to load the information in the rc file. It then will call parse to convert this text file into the nnet data
	structures. \param qpszName Resource name. \param qpszType Type of resource. \return true iff the resource loads.
	=======================================================================================================================
	*/
	bool NeuralNet::LoadFromResource(const char* qpszModule, const char *qpszName, const char *qpszType)
	{
		/*~~~~~~~~~~~~~*/
		HGLOBAL zhRes;
		HINSTANCE zhInst;
		/*~~~~~~~~~~~~~*/

		wchar_t wtext[1000];
		mbstowcs(wtext, qpszModule, strlen(qpszModule) + 1);//Plus null
		LPWSTR ptr = wtext;
		zhInst = GetModuleHandle(ptr);

		wchar_t wtextName[1000];
		mbstowcs(wtextName, qpszName, strlen(qpszName) + 1);//Plus null
		LPWSTR ptrName = wtextName;

		wchar_t wtextType[1000];
		mbstowcs(wtextType, qpszType, strlen(qpszType) + 1);//Plus null
		LPWSTR ptrType = wtextType;


		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		LPVOID pdata = NULL;
		HRSRC zRsc = FindResource(zhInst, ptrName, ptrType);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (zRsc)
		{
			zhRes = LoadResource(zhInst, zRsc);
			if (zhRes)
			{
				pdata = LockResource(zhRes);
			}
			else
			{
				return false;
			}
		}
		else
		{
			/*~~~~~~~~~~~~~~~~~~~*/
			// for debugging purpose (if we are in the MFC test app, I would like to see the
			// TRACE of error)
			LPVOID lpMessageBuffer;
			/*~~~~~~~~~~~~~~~~~~~*/

			FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPTSTR) & lpMessageBuffer, 0, NULL);

			/*~~~~~~~~~~*/
			CString z_msg;
			/*~~~~~~~~~~*/

			z_msg += (char *) lpMessageBuffer;
			TRACE(z_msg);
			LocalFree(lpMessageBuffer);
		}

		if (!pdata)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// now read out of the resource file
		unsigned char *sData = (unsigned char *) pdata;
		//LPTSTR sExpert = (LPTSTR) sData;
		std::string strExpert(reinterpret_cast< char const* >(sData));
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		Parse(strExpert);

		FreeResource(zhRes);

		return true;
	};

	/*
	* ! NeuralNet::LoadFromFile: This function, given a filename, will attempt to
	* load the information in the file. It then will call parse to convert this text
	* file into the nnet data structures. \param FileName The name of the file we
	* should load. \return true iff the information loads.
	*/
#include <io.h>
#include <fcntl.h>

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	bool NeuralNet::LoadFromFile(string FileName)
	{
		/*~~~~~~~~~~~~~*/
		bool bRet = true;
		int fh;
		/*~~~~~~~~~~~~~*/

		fh = ::_open(FileName.c_str(), _O_RDONLY | _O_TEXT);
		if (fh == -1)
		{
			return false;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~*/
		char* buffer = new char[60000];
		unsigned int nbytes = 60000;
		unsigned int bytesread;
		/*~~~~~~~~~~~~~~~~~~~~~~~~*/

		/* Read in input: */
		if ((bytesread = _read(fh, buffer, nbytes)) <= 0)
		{
			bRet = false;
		}
		else
		{
			/*~~~~~~~~~~~~~*/
			// put the buffer in a string object
			string strExpert;
			/*~~~~~~~~~~~~~*/

			strExpert.replace(0, bytesread, buffer);
			strExpert.resize(bytesread, ' ');
			bRet = Parse(strExpert);
		}

		_close(fh);
		delete[] buffer;
		return bRet;
	}

	/*
	* ! NeuralNet::Parse: This function, given a text string, will convert it into
	* the various matrices, vector, and function that make up a neural net. \param
	* strExpert TBD \return true iff the parsing of text is correct.
	*/
#define PARSE_WEIGHT	0
#define PARSE_BIAS		1
#define PARSE_TRANSF	2

	/*
	=======================================================================================================================
	=======================================================================================================================
	*/

	bool NeuralNet::Parse(string strExpert)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		size_t iLen = strExpert.length();
		size_t iPos = strExpert.find('\n', 0);
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		if (iPos != string::npos)
		{
			Clear();	// if there is some text then we should clear the data structure to be ready
		}

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		// first string that it reads is the number of layer
		string strLayer = strExpert.substr(0, iPos);
		int nParsedLayer = 1;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		m_nLayers = atoi(strLayer.c_str());

		if (m_nLayers == 0)
		{
			return false;
		}

		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);						// move past the layer number
		iPos = strExpert.find('\n', 0);
		strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);						// move past the blank line under layer number
		iPos = strExpert.find('\n', 0);

		m_W = new PatternsMatrix[m_nLayers];
		m_bias = new PatternsVector[m_nLayers];

		// m_Input = new PatternsVector[m_nLayers];
		m_transfer = new TransferFunctions[m_nLayers];

		/*~~~~~~~~~~~~~~~~~~~~~~*/
		string strLine;
		string strTemp;
		int nState = PARSE_WEIGHT;
		long nMatrixRow = 0;
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		while (iLen > 0)
		{
			strLine = strExpert.substr(0, iPos);

			if ((strLine == "" || strExpert.find('\r', 0) == 0) && nState == PARSE_WEIGHT)
			{
				nState = PARSE_BIAS;
				if (strExpert.find('\r', 0) == 0)
				{
					strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);			// move past the \r line under layer number
				}
				else
				{
					strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);			// move past the blank line under layer number
				}

				iPos = strExpert.find('\n', 0);
				iLen = strExpert.length();
				continue;
			}
			else if ((strLine == "" || strExpert.find('\r', 0) == 0) && nState == PARSE_BIAS)
			{
				nState = PARSE_TRANSF;

				if (strExpert.find('\r', 0) == 0)
				{
					strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);			// move past the \r line under layer number
				}
				else
				{
					strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);		// move past the blank line under layer number
				}

				iPos = strExpert.find('\n', 0);
				iLen = strExpert.length();
				continue;
			}
			else if ((strLine == "" || strExpert.find('\r', 0) == 0) && nState == PARSE_TRANSF)
			{
				nState = PARSE_WEIGHT;

				if (strExpert.find('\r', 0) == 0)
				{
					strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);		// move past the \r line under layer number (resource adds a /r)
				}
				else
				{
					strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);	// move past the blank line under layer number
				}

				iPos = strExpert.find('\n', 0);
				iLen = strExpert.length();
				continue;
			}

			if (nState == PARSE_WEIGHT)
			{
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				double dValue = 0.0;
				size_t iVLen = strLine.length();
				size_t iVPos = strLine.find('\t', 0);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (iVPos == string::npos)
				{	// this is the case that the matrix is a single vector
					iVPos = iVLen;
				}

				/*~~~~~~~~~~~~*/
				long nIndex = 0;
				/*~~~~~~~~~~~~*/

				while (iVLen > 0)
				{
					strTemp = strLine.substr(0, iVPos);
					dValue = atof(strTemp.c_str());

					// now put the values in the vector m_bias[nParsedLayer - 1].resize(nIndex+1);
					m_W[nParsedLayer - 1](nMatrixRow, nIndex) = dValue;

					if (iVLen == iVPos)
					{
						strLine = "";
					}
					else
					{
						strLine = strLine.substr(iVPos + 1, iVLen - iVPos + 1);
					}

					iVPos = strLine.find('\t', 0);	// cannot find a tab then it will be the last element
					if (iVPos == string::npos)
					{
						iVPos = strLine.length();
					}

					iVLen = strLine.length();
					nIndex += 1;
				}

				nMatrixRow += 1;
			}

			if (nState == PARSE_BIAS)
			{
				nMatrixRow = 0;

				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				double dValue = 0.0;
				size_t iVLen = strLine.length();
				size_t iVPos = strLine.find('\t', 0);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (iVPos == string::npos)
				{	// this is the case that the matrix is a single vector
					iVPos = iVLen;
				}

				/*~~~~~~~~~~~~*/
				long nIndex = 0;
				/*~~~~~~~~~~~~*/

				while (iVLen > 0)
				{
					strTemp = strLine.substr(0, iVPos);
					dValue = atof(strTemp.c_str());

					// now put the values in the vector m_bias[nParsedLayer - 1].resize(nIndex+1);
					m_bias[nParsedLayer - 1](nIndex) = dValue;

					if (iVLen == iVPos)
					{
						strLine = "";
					}
					else
					{
						strLine = strLine.substr(iVPos + 1, iVLen - iVPos + 1);
					}

					iVPos = strLine.find('\t', 0);	// cannot find a tab then it will be the last element
					if (iVPos == string::npos)
					{
						iVPos = strLine.length();
					}

					iVLen = strLine.length();
					nIndex += 1;
				}
			}
			else if (nState == PARSE_TRANSF)
			{
				nMatrixRow = 0;

				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
				// trim all \r off the end
				size_t itPos = strLine.find('\r', 0);
				/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

				if (itPos != string::npos)
				{
					strLine = strLine.substr(0, itPos);
				}

				if (strLine == "hardlims" || strLine.substr(0, strLine.length() - 1) == "hardlims")
				{
					m_transfer[nParsedLayer - 1] = eHardlims;
					nParsedLayer++;
				}
				else if (strLine == "hardlim" || strLine.substr(0, strLine.length() - 1) == "hardlim")
				{
					m_transfer[nParsedLayer - 1] = eHardlim;
					nParsedLayer++;
				}
				else if (strLine == "logsig" || strLine.substr(0, strLine.length() - 1) == "logsig")
				{
					m_transfer[nParsedLayer - 1] = eLogsig;
					nParsedLayer++;
				}
				else if (strLine == "poslin" || strLine.substr(0, strLine.length() - 1) == "poslin")
				{
					m_transfer[nParsedLayer - 1] = ePoslin;
					nParsedLayer++;
				}
				else if (strLine == "purelin" || strLine.substr(0, strLine.length() - 1) == "purelin")
				{
					m_transfer[nParsedLayer - 1] = ePurelin;
					nParsedLayer++;
				}
				else if (strLine == "radbas" || strLine.substr(0, strLine.length() - 1) == "radbas")
				{
					m_transfer[nParsedLayer - 1] = eRadbas;
					nParsedLayer++;
				}
				else if (strLine == "satlins" || strLine.substr(0, strLine.length() - 1) == "satlins")
				{
					m_transfer[nParsedLayer - 1] = eSatlins;
					nParsedLayer++;
				}
				else if (strLine == "satlin" || strLine.substr(0, strLine.length() - 1) == "satlin")
				{
					m_transfer[nParsedLayer - 1] = eSatlin;
					nParsedLayer++;
				}
				else if (strLine == "tansig" || strLine.substr(0, strLine.length() - 1) == "tansig")
				{
					m_transfer[nParsedLayer - 1] = eTansig;
					nParsedLayer++;
				}
			}

			strExpert = strExpert.substr(iPos + 1, iLen - iPos + 1);

			// get a line of up to "\r\n"
			iPos = strExpert.find('\n', 0);
			if (iPos == string::npos)
			{
				iPos = strExpert.find('\r', 0);
			}

			iLen = strExpert.length();

			if (nParsedLayer > m_nLayers)
			{
				break;
			}
		}

		// if the number of layers defined at the top of the file is not equal the the
		// number of transferfunction read then this is a file which is not in our format.
		if (nParsedLayer - 1 != m_nLayers)
		{
			Clear();
			return false;
		}

		return true;
	}

	/*
	=======================================================================================================================
	! NeuralNet::debug: TBD
	=======================================================================================================================
	*/
	void NeuralNet::debug(void) const	// print entries on a single line
	{
		for (int i = 0; i < m_nLayers; i++)
		{
			m_W[i].debug();

			m_bias[i].debug();

			switch (m_transfer[i])
			{
			case eHardlim:
				TRACE("hardlim\n\n");
				break;

			case eHardlims:
				TRACE("hardlims\n\n");
				break;

			case eLogsig:
				TRACE("logsig\n\n");
				break;

			case ePoslin:
				TRACE("poslin\n\n");
				break;

			case ePurelin:
				TRACE("purelin\n\n");
				break;
			case eRadbas:
				TRACE("radbas\n\n");
				break;

			case eSatlin:
				TRACE("satlin\n\n");
				break;

			case eSatlins:
				TRACE("satlins\n\n");
				break;

			case eTansig:
				TRACE("tansig\n\n");
				break;

			default:
				TRACE("None\n\n");
				break;
			}
		}
	}

	/*
	=======================================================================================================================
	! NeuralNet::hardlim: This function converts a vector by the hardlim transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::hardlim(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = hardlim(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::hardlims: This function converts a vector by the hardlims transformation. \param a PatternsVector to
	transform. \return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::hardlims(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = hardlims(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::logsig: This function converts a vector by the logsig transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::logsig(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = logsig(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::poslin: This function converts a vector by the poslin transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::poslin(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = poslin(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::purelin: This function converts a vector by the purelin transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::purelin(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = purelin(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::radbas: This function converts a vector by the radbas transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::radbas(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = radbas(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::satlin: This function converts a vector by the satlin transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::satlin(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = satlin(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::satlins: This function converts a vector by the satlins transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::satlins(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = satlins(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::tansig: This function converts a vector by the tansig transformation. \param a PatternsVector to transform.
	\return A vector where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::tansig(PatternsVector& a)
	{
		/*~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector trans(a.getSize());
		/*~~~~~~~~~~~~~~~~~~~~~~*/

		for (int i = 0; i < a.getSize(); i++)
		{
			trans(i) = tansig(a(i));
		}

		return trans;
	}

	/*
	=======================================================================================================================
	! NeuralNet::Simulate: This function get the result given an input vector. It is a recursive function for each
	layer. To the outside caller it should be call without the layer parameter like: NeuralNet nnet;
	nnet.LoadFromFile("Expert1.txt");
	PatternsVector vInput;
	PatternsVector myVec = nnet.Simulate(vInput);
	any number of input vector can be passed and get results for. \param input TBD \param layer TBD \return A vector
	where each element is transformed.
	=======================================================================================================================
	*/
	PatternsVector NeuralNet::Simulate(const PatternsVector& input, int nLayer)
	{
		if (nLayer >= m_nLayers)
		{					// final recursion if we have gone past the total layer return the input
			return input;
		}

		/*~~~~~~~~~~*/
		PatternsVector result;
		/*~~~~~~~~~~*/

		if (((PatternsVector*) &input)->iserror())
		{
			result.error();
			return result;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		// check the size of the input vector and assure that it is the same length as
		// the m dimension of the layer weight matrix
		long nSize = input.getSize();
		long nCols = m_W[nLayer].getColumns();
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		nCols = m_W[nLayer].getRows();
		if (nSize != nCols) // this is a invalid nnet for this input vector
		{
			result.error();
			return result;
		}

		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
		PatternsVector ans = m_W[nLayer].transpose() * input;
		/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

		switch (m_transfer[nLayer])
		{
		case eHardlim:
			result = hardlim(ans + m_bias[nLayer]);
			break;

		case eHardlims:
			result = hardlims(ans + m_bias[nLayer]);
			break;

		case eLogsig:
			result = logsig(ans + m_bias[nLayer]);
			break;

		case ePoslin:
			result = poslin(ans + m_bias[nLayer]);
			break;

		case ePurelin:
			result = purelin(ans + m_bias[nLayer]);
			break;

		case eRadbas:
			result = radbas(ans + m_bias[nLayer]);
			break;

		case eSatlin:
			result = satlin(ans + m_bias[nLayer]);
			break;

		case eSatlins:
			result = satlin(ans + m_bias[nLayer]);
			break;

		case eTansig:
			result = tansig(ans + m_bias[nLayer]);
			break;

		default:
			result = ans + m_bias[nLayer];
			break;
		}

		result = Simulate(result, nLayer + 1);

		return result;
	}
}
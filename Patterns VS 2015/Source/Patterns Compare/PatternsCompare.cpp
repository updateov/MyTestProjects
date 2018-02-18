// PatternsCompare.cpp : Defines the entry point for the console application.
#include "stdafx.h"
#include "FhrPartsCompare.h"

#include <vector>

// PAW
#include <algorithm>
#include <stdio.h>

using namespace std;
using namespace patterns;

string FileRead(const string &filename);
bool GetFileList(int argc, char **argv, vector<string> *f1, vector<string> *f2);
bool AddFilesFromDir(string inDir, vector <string> *f);
void WriteListToFile(FILE *fid, vector<string> *strList);
//void wait(void);
string getIDFromFileName(string fName);

//
// =======================================================================================================================
//    using namespace std;
//    int _tmain(int argc, _TCHAR* argv[])
// =======================================================================================================================
//
int main(int argc, char **argv)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	fetus *pFetus1 = NULL;
	fetus *pFetus2 = NULL;
	fhrPartSet expertSet;
	fhrPartSet testSet;
	fhrPartSet currExp;
	fhrPartSet currTest;
	fhrPart p;
	vector<string> expFiles;
	vector<string> testFiles;
	string opDir;
	string opTag;	
	string strID;
	

	FILE **opFID;
	string *typeTag;
	fhrPart::FhrPartType *iPartFilter;
	CFhrPartsCompare *pCompTotal;
	CFhrPartsCompare *pComp;
	long lEdgeBuffer;
	long nPartTypes;
	bool bSubtypes;
	bool bContractions = false;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	// Could list set of directories in format <E1> <T1> <E2> <T2>
	if (argc < 5)
	{
		printf("PatternsCompare <expert_xml | expert_dir> <test_xml | test_dir> .... <output_dir> <output_tag> <do_subtypes>\n\n");
		printf("Note that can specify multiple files/dirs in format <E1> <T1> <E2> <T2> ... <EN> <TN>\n\n");
//		wait();
		exit(1);
	}
	else
	{
		// First check wheter args are xml files or directory names Create File List to
		// iterate ove
		if (!(GetFileList(argc, argv, &expFiles, &testFiles)))
		{
			printf("Could not build list of test files from specified inputs...");
		//	wait();
			exit(1);
		}
	}

	opDir = argv[argc - 3];
	opTag = argv[argc - 2];
	bSubtypes = (!(strcmp(argv[argc - 1], "1")));
	bContractions = (!(strcmp(argv[argc - 1], "2")));

	if (bSubtypes)
	{
		nPartTypes = 5;
	}
	else if (bContractions)
	{
		nPartTypes = 1;
	}
	else
	{
		nPartTypes = 3;
	}

	opFID = new FILE*[nPartTypes];

	for (int co = 0; co < 3; ++ co)
	{
		typeTag = new string[nPartTypes];
		iPartFilter = new fhrPart::FhrPartType[nPartTypes];
		pCompTotal = new CFhrPartsCompare[nPartTypes];
		pComp = new CFhrPartsCompare[nPartTypes];

		if (bSubtypes)
		{
			typeTag[0] = "Variable"; typeTag[1] = "Late" ; typeTag[2] = "Early"; typeTag[3] = "NonAssoc" ; typeTag[4] = "Prolonged";
			iPartFilter[0] = fhrPart::DecType_VAR; iPartFilter[1] = fhrPart::DecType_LATE; iPartFilter[2] = fhrPart::DecType_EARLY; iPartFilter[3] = fhrPart::DecType_NONASSOC; iPartFilter[4] = fhrPart::DecType_PROL;
		}
		else if (bContractions)
		{
			typeTag[0] = "Contraction";
			iPartFilter[0] = fhrPart::FhrPartType_NONE;
		}
		else
		{
			typeTag[0] = "Decel"; typeTag[1] = "Accel"; typeTag[2] = "BaseLine";
			iPartFilter[0] = fhrPart::FhrPartType_DECEL; iPartFilter[1] = fhrPart::FhrPartType_ACCEL; iPartFilter[2] = fhrPart::FhrPartType_BAS;
		}

		if (bContractions)
		{
			lEdgeBuffer = 240;
		}
		else
		{
			lEdgeBuffer = pComp[0].EdgeBuffer();	// should be default edge buffer of 2600
		}

		for (int i = 0; i < nPartTypes; i++)
		{
			if (co == 0)
			{
				string strFile;

				strFile = opDir;
				strFile.append("/");
				strFile.append(opTag);
				strFile.append("_");
				strFile.append(typeTag[i]);
				strFile.append(".txt");
				opFID[i] = fopen(strFile.c_str(), "w");

				if (!(opFID[i]))
				{
					printf("Could not open op file %s for writing\n\n", strFile.c_str());
					exit(1);
				}

				fprintf(opFID[i], "Expert Files:\n ");
				WriteListToFile(opFID[i], &expFiles);
				fprintf(opFID[i], "\nTest Files:\n ");
				WriteListToFile(opFID[i], &testFiles);
			}
		}

		if (bSubtypes)
		{
			for (int i = 0; i < nPartTypes; i++)
			{
				fprintf(opFID[i],"\n****************************************\n");
				fprintf(opFID[i], co==0?"subtype (expert) against subtype (detection)":co == 1?"subtype (expert) against decel (detection)":"decel (expert) against subtype (detection)");
				fprintf(opFID[i],"\n");
			}
		}

		for (int i = 0; i < (int) expFiles.size(); i++)
		{
			pFetus1 = new fetus;
			pFetus2 = new fetus;
			printf("Comparing %s and %s...\n\n", expFiles[i].c_str(), testFiles[i].c_str());
			pFetus1->import(FileRead(expFiles[i]));
			pFetus2->import(FileRead(testFiles[i]));

			if (bContractions)
			{
				CFhrPartsCompare::FhrPartsFromFetusContractions(&expertSet, pFetus1);
				CFhrPartsCompare::FhrPartsFromFetusContractions(&testSet, pFetus2);
			}
			else
			{
				CFhrPartsCompare::FhrPartsFromFetus(&expertSet, pFetus1);
				CFhrPartsCompare::FhrPartsFromFetus(&testSet, pFetus2);
			}
			strID = getIDFromFileName(expFiles[i]);

			long n = pFetus1->get_number_of_fhr();
			while ((n > 0) && ((pFetus1->get_fhr(n-1) == 0) || (pFetus1->get_fhr(n-1) == 255)))
				n--;
			long startIndex = 0;
			while ((startIndex < n) && ((pFetus1->get_fhr(startIndex) == 0) || (pFetus1->get_fhr(startIndex) == 255)))
				startIndex++;
			// Here we remove events at the beginning and end of tracing so that we do not
			// consider end effects In reality only need to cut about 4 minutes but to be
			// consistent w/ prior metrics (for FDA and MATLAB) trim 2600 samples at each end
			if (n > 0)
			{ // trim end parts
				expertSet.filterEndingAfter(n - lEdgeBuffer);
				testSet.filterEndingAfter(n - lEdgeBuffer);
				expertSet.filterStartingBefore(lEdgeBuffer + startIndex);
				testSet.filterStartingBefore(lEdgeBuffer + startIndex);
			}

			if (pFetus1)
			{
				delete pFetus1;
			}

			pFetus1 = NULL;

			if (pFetus2)
			{
				delete pFetus2;
			}

			pFetus2 = NULL;

			for (int k = 0; k < nPartTypes; k++)
			{
				currExp = expertSet;
				currTest = testSet;

				if (bSubtypes)
				{
					if (co == 2)
					{
						currExp.filterByType(fhrPart::FhrPartType_DECEL);
					}
					else
					{
						if (iPartFilter[k] == fhrPart::DecType_PROL)
						{
							currExp.filterByType(fhrPart::FhrPartType_DECEL);
							currExp.filterByLength(480); // because expert markings do not have prolonged subtype
						}
						else
						{
							currExp.filterByDecelSubtype(iPartFilter[k]);
						}
					}

					if (co == 1)
					{
						currTest.filterByType(fhrPart::FhrPartType_DECEL);
					}
					else
					{
						currTest.filterByDecelSubtype(iPartFilter[k]);
					}
				}
				else
				{
					currExp.filterByType(iPartFilter[k]);
					currTest.filterByType(iPartFilter[k]);

					if (!bSubtypes && iPartFilter[k] == fhrPart::FhrPartType_DECEL)
					{
						pComp[k].SetCrossTypeDecel();
						pCompTotal[k].SetCrossTypeDecel();
					}
				}
				pComp[k].SetExpert(&currExp, n);
				pComp[k].SetTest(&currTest);
				pComp[k].SetID(strID);	// from filename and event type
				pComp[k].Compare();

				pCompTotal[k].Merge(&(pComp[k]));

				// Here want to write to file
				if (i == 0)
				{
					fprintf(opFID[k], "\n\n");
					pComp[k].PrintHeaderString(opFID[k]);
				}

				pComp[k].PrintOPString(opFID[k]);
				pComp[k].Clear();
			}

			expertSet.clear();
			testSet.clear();
		}

		for (int i = 0; i < nPartTypes; i++)
		{
			pCompTotal[i].SetID("Total");
			fprintf(opFID[i], "\n\n");
			pCompTotal[i].PrintOPString(opFID[i]);
			pCompTotal[i].PrintCrossTypeDecel(opFID[i]);
		}

		if (!bSubtypes)
			break;
	}

	for (int i = 0; i < nPartTypes; i++)
	{
		fclose(opFID[i]);
	}

	delete [] opFID;
	delete [] typeTag;
	delete [] iPartFilter;
	delete [] pCompTotal;
	delete [] pComp;
	//wait();
	return 0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
string FileRead(const string &filename)
{
	string str;
	CFile*	pFile = new CFile(filename.c_str(), CFile::modeRead | CFile::shareDenyNone);
	if (pFile)
	{
		long n = (long)pFile->GetLength();
		char* buf = new char[n+1];
		if (buf)
		{
			memset(buf, 0, n + 1);
			pFile->Read(buf, n);

			str = buf;
			delete buf;
		}

		pFile->Close();
		delete pFile;
	}

	return str;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */


bool GetFileList(int argc, char **argv, vector<string> *f1, vector<string> *f2)
{
	/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
	int n = 0;
	bool bRC = true;
	CFileFind ff;
	CString wstr, fstr;
	string currDir;
	// currently cannot properly parse directory for list of files - somehow is not easy to do - giving up

	bRC = (ff.FindFile(argv[n + 1]) == TRUE);
	if (bRC)
	{
		ff.FindNextFile();
		if (ff.IsDirectory())
		{ // assume list of directories
			int nDir = (argc - 4) / 2;
			long argInd = n+1;
			
			for (long i = 0; i < nDir; i++)
			{
				currDir = argv[argInd++];
				AddFilesFromDir(currDir, f1);
				currDir = argv[argInd++];
				AddFilesFromDir(currDir, f2);
			}
		}
		else 
		{
			FILE *f = fopen(argv[n + 1], "r");
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			n = 1;
			while ((n < argc -1) && (f))
			{
				if ((n % 2) == 0)
				{
					f2->push_back(argv[n+1]);
				}
				else
				{
					f1->push_back(argv[n+1]);
				}
				n++;
				f = fopen(argv[n + 1], "r");
			}
		}
	}

	std::sort(f1->begin(), f1->end());
	std::sort(f2->begin(), f2->end());
	ff.Close();

	return bRC;
}

bool AddFilesFromDir(string inDir, vector <string> *f)
{
	CFileFind ff;
	bool b; 
	long n = 0;
	string wstr = inDir + "\\*.xml";
	b = (ff.FindFile(wstr.c_str()) == TRUE);
	while (b)
	{
		n++;
		b = (ff.FindNextFile() == TRUE);
		CString cstr = ff.GetFilePath();
		f->push_back((LPCTSTR)cstr);
	}

	ff.Close();
	return (n > 0);
}



/*
string convertWCharArrayToString(const WCHAR * const wcharArray) 
{
	stringstream ss;

    int i = 0;
    char c = (char) wcharArray[i];
    while(c != '\0') 
	{
        ss <<c;
        i++;
        c = (char) wcharArray[i];
    }
    string convert = ss.str();

    return convert;
}
*/


/*
 =======================================================================================================================
 =======================================================================================================================
 */
void WriteListToFile(FILE *fid, vector<string> *strList)
{
	for (int i = 0; i < (int) strList->size(); i++)
	{
		fprintf(fid, "%s\n", (strList->at(i)).c_str());
	}
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
/*void wait(void)
{
	cout << "Press any key to continue ...";

	/*~~~~~*/
//	string z;
	/*~~~~~*/
/*
	getline(cin, z);
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
string getIDFromFileName(string fName)
{
	/*~~~~~~~~~~~~~~*/
	string strID = "";
	int s1;
	int s1b;
	int s2;
	/*~~~~~~~~~~~~~~*/

	s2 = (int) fName.rfind(".xml");
	s1 = (int) fName.rfind("/");
	s1b = (int) fName.rfind("\\");
	if (s1 < s1b)
	{
		s1 = s1b;
	}

	/*~~~~~~~~~~~~~~~~~~*/
	int len = s2 - s1 - 1;
	/*~~~~~~~~~~~~~~~~~~*/

	if (s1 < s2)
	{
		strID = fName.substr(s1 + 1, len);
	}

	return strID;
}

#include "stdafx.h"
#include "file_loading.h"

#include "patterns, conductor.h"
#include "tinyxml.h"
#include "base64.h"

using namespace patterns;

SYSTEMTIME __parse_date(CString input, bool dateonly)
{
	SYSTEMTIME time = {0};
	try
	{
		if (dateonly)
		{
			sscanf(input, "%4d-%2d-%2d", &time.wYear, &time.wMonth, &time.wDay);
			return time;
		}
		else
		{
			sscanf(input, "%4d-%2d-%2d %2d:%2d:%2d", &time.wYear, &time.wMonth, &time.wDay, &time.wHour, &time.wMinute, &time.wSecond);
			return time;
		}
	}
	catch(...)
	{
		return time;
	}
}

bool is_file_archive_xml(string filename)
{
	CFile*	pFile = new CFile(filename.c_str(), CFile::modeRead | CFile::shareDenyNone);
	if (pFile == NULL)
	{
		return false;
	}

	// We need to read only 2Ko of the file to check the type
	long n = (long)pFile->GetLength();
	if (n > 2048)
	{
		n = 2048;
	}

	char*  buf = new char[n];

	pFile->Read(buf, n);
	pFile->Close();
	delete pFile;

	string header = buf;
	delete buf;

	std::transform(header.begin(), header.end(), header.begin(), tolower);
	if (header.find("patternsarchive") != string::npos)
	{
		return true;
	}

	return false;
}

// =====================================================================================================================
//    Load an archived xml
// =====================================================================================================================
bool file_loading::load_archive_xml(string filename, input_adapter::patient* p, fetus& f, ConfigData& data)
{
	f.set_as_real_time(false);

	TiXmlDocument xmlDoc(filename.c_str());
	if (!xmlDoc.LoadFile())
	{
		ASSERT(FALSE);
		return false;
	}

	TiXmlElement*  rootNode = xmlDoc.RootElement();
	if (rootNode == NULL)
	{
		ASSERT(FALSE);
		return false;
	}

	TiXmlElement*  visitNode = rootNode->FirstChildElement("visit");
	if (visitNode == NULL)
	{
		ASSERT(FALSE);
		return false;
	}

	TiXmlElement*  patientNode = visitNode->FirstChildElement("patient");
	if (patientNode == NULL)
	{
		ASSERT(FALSE);
		return false;
	}

	TiXmlElement*  dataNode = visitNode->FirstChildElement("data");
	if (dataNode == NULL)
	{
		ASSERT(FALSE);
		return false;
	}

	TiXmlElement*  tracingsNode = dataNode->FirstChildElement("tracings");
	if (tracingsNode == NULL)
	{
		ASSERT(FALSE);
		return false;
	}

	TiXmlElement*  patternsNode = dataNode->FirstChildElement("patterns");
	if (patternsNode == NULL)
	{
		ASSERT(FALSE);
		return false;
	}

#ifdef patterns_viewer
	// Patterns configuration
	string configuration = base64::base64_decode((string) (rootNode->Attribute("configuration")));
	data.from_string(configuration);
#endif

	// Patient data
	p->set_key(filename);
	p->set_id((string) patientNode->Attribute("patientid"));
	p->set_accountno((string) patientNode->Attribute("accountno"));
	p->set_name((string) patientNode->Attribute("lastname"));
	p->set_surname((string) patientNode->Attribute("firstname"));
	p->set_bed("Archive file");

	SYSTEMTIME time;

	time = __parse_date(visitNode->Attribute("dob"), true);
	if (time.wYear > 0)
	{
		FILETIME t;
		SystemTimeToFileTime(&time, &t);
		p->set_dob(t);
	}

	p->set_age((string) visitNode->Attribute("age"));

	CString sd = visitNode->Attribute("edd");
	time = __parse_date(sd, true);
	if (time.wYear > 0)
	{
		p->set_edc(fetus::convert_to_utc(fetus::convert_to_time_t(time)));
		p->set_edc_string((string)sd);
	}

	p->set_gestational_age((string) visitNode->Attribute("ga"));

	string fc = visitNode->Attribute("fetuscount");
	p->set_number_of_fetuses(fc.length() > 0 ? atol(fc.c_str()) : 0);

	// Tracing data
	time = __parse_date(tracingsNode->Attribute("starttime"), false);
	if (time.wYear > 1970)
	{
		f.set_start_date(fetus::convert_to_time_t(time));

#ifdef patterns_research
		p->set_edc(f.get_start_date());
#endif

		f.set_up_sample_rate(1);
		f.set_hr_sample_rate(4);

		TiXmlElement*  tracingNode = tracingsNode->FirstChildElement();
		while (tracingNode != NULL)
		{
			string typeOfTracing = tracingNode->Attribute("type");
			vector<char> data;

			TiXmlElement*  segmentNode = tracingNode->FirstChildElement();
			while (segmentNode != NULL)
			{
				long start = atol(segmentNode->Attribute("start"));
				if (start > (long)data.size())
				{
					data.insert(data.end(), start - (long)data.size(), 0);
				}

				CString raw = segmentNode->Attribute("data");
				string bytes = base64::base64_decode((string) raw);

				data.reserve(data.size() + bytes.length());
				std::copy(bytes.begin(), bytes.end(), std::back_inserter(data));

				segmentNode = segmentNode->NextSiblingElement();
			}

			if (typeOfTracing == "up")
			{
				f.append_up(data);
			}
			else if (typeOfTracing == "fhr1")
			{
				f.append_fhr(data);
			}
			else if (typeOfTracing == "mhr")
			{
				f.append_mhr(data);
			}

			tracingNode = tracingNode->NextSiblingElement();
		}
	}

	string patterns = base64::base64_decode((string) (patternsNode->Attribute("data")));
	vector<string> lines;
	patterns::conductor::to_vector(patterns, lines, '\n');

	input_adapter::load_saved_data(f, lines);

	f.set_as_real_time(true);

	return true;
}


bool file_loading::load_file(string filename, input_adapter::patient* p, fetus& f, ConfigData& data)
{
	bool bLoaded = false;
	try
	{
		char fext[_MAX_EXT];
		_splitpath(filename.c_str(), NULL, NULL, NULL, fext);

		string fileext = fext;
		std::transform(fileext.begin(), fileext.end(), fileext.begin(), toupper);

		if (fileext.compare(".V01") == 0)
		{
			bLoaded = load_old_format_file(filename, ev01file, p, f);
		}
		else if (fileext.compare(".IN") == 0)
		{
			// in file
			bLoaded = load_old_format_file(filename, einfile, p, f);
		}
		else if (fileext.compare(".XML") == 0)
		{
			// That's were we need to check which xml format we have there...
			if (is_file_archive_xml(filename))
			{
				bLoaded = load_archive_xml(filename, p, f, data);
			}
			else
			{
				bLoaded = load_old_format_file(filename, eoldxmlfile, p, f);
			}
		}
	}
	catch(...)
	{
	}

	return bLoaded;
}

bool file_loading::load_old_format_file(string filename, format_file eformat, input_adapter::patient* p, fetus& f)
{
	CFile*	pFile = new CFile(filename.c_str(), (eformat == ev01file) ? (CFile::modeRead | CFile::typeBinary | CFile::shareDenyNone) : (CFile::modeRead | CFile::shareDenyNone));
	if (pFile == NULL)
	{
		return false;
	}

	long n = (long)pFile->GetLength();
	char* buf = new char[n + 1];
	memset(buf, 0, n + 1);

	pFile->Read(buf, n);
	pFile->Close();

	delete pFile;
	p->set_bed((eformat == eoldxmlfile) ? "XML file" : (eformat == einfile) ? "IN file" : (eformat == ev01file) ? "V01 file" : "file");

	try
	{
		p->set_key(filename);

		char fname[_MAX_FNAME];
		_splitpath(filename.c_str(), NULL, NULL, fname, NULL);

		p->set_name(fname);
		p->set_number_of_fetuses(1);
		p->set_edc(fetus::convert_to_utc(CTime::GetCurrentTime().GetTime()));
		p->set_gestational_age("40+0");
		f.set_as_real_time(false);

		switch (eformat)
		{
			case eoldxmlfile:
				f.import(buf);
				p->set_bed("XML file");
				break;

			case ev01file:
				if (f.import_v01((unsigned char*)buf, n))
				{
					p->set_bed("V01 file (truncated)");
				}
				else
				{
					p->set_bed("V01 file");
					if ((f.get_number_of_fhr() == 0) && (f.get_number_of_up() == 0))
					{
						delete buf;
						return false;
					}
				}
				break;

			case einfile:
				f.import(buf);
				f.set_start_date(fetus::convert_to_utc(CTime::GetCurrentTime().GetTime()));
				p->set_bed("IN file");
				break;

			default:
				ASSERT(false);
				delete buf;

				return false;
		}
	}
	catch(...)
	{
		delete buf;
		return false;
	}

	delete buf;

	if (f.get_start_date() == undetermined_date)
	{
		f.set_start_date(fetus::convert_to_utc(CTime::GetCurrentTime().GetTime()));
	}

	f.set_hr_sample_rate(4);
	f.set_up_sample_rate(1);
	p->set_edc(f.get_start_date());

	return true;
}
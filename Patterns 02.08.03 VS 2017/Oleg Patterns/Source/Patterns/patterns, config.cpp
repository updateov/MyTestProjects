#include "stdafx.h"
#include <assert.h>
#include "patterns, config.h"

using std:: string;

ConfigData::ConfigData()
{
}

bool ConfigData::read_file(string filename)
{
	// Read keys and values from given file
	try
	{
		std::ifstream in(filename.c_str());
		if (in.fail())
		{
			return false;
		}

		std::string str((std::istreambuf_iterator<char> (in)), std::istreambuf_iterator<char> ());
		in.close();

		from_string(str);
	}
	catch(...)
	{
		assert(0);
		return false;
	};
	return true;
}

bool ConfigData::save_file(string filename)
{
	// Save keys and values into given file
	FILE*  file = fopen(filename.c_str(), "wbc");
	if (file == NULL)
	{
		return false;
	}

	try
	{
		string r = to_string();

		if (fwrite(r.c_str(), (int)r.length(), 1, file) <= 0)
		{
			fclose(file);
			return false;
		}
	}
	catch(...)
	{
		fclose(file);
		throw;
	}

	fclose(file);
	return true;
}

string ConfigData::to_string() const
{
	string r;

	// Save a ConfigData to a string
	for (std::map<string, string>::const_iterator p = data.begin(); p != data.end(); ++p)
	{
		r += p->first;
		r += " = ";
		r += p->second;
		r += "\r\n";
	}

	return r;
}

void ConfigData::from_string(const string& value)
{
	std::istringstream istr(value);

	string line;
	while (std::getline(istr, line))
	{
		// Parse the line if it contains a delimiter
		string::size_type delimPos = line.find("=");
		if (delimPos < string::npos)
		{
			// Extract the key
			string newKey = line.substr(0, delimPos);
			string newValue = line.substr(delimPos + 1);

			// Store key and value
			ConfigData::trim(newKey);
			ConfigData::trim(newValue);

			data[newKey] = newValue;
		}
	}
}

void ConfigData::trim(string& s)
{
	// Remove leading and trailing whitespace
	static const char whitespace[] = " \n\t\v\r\f";
	s.erase(0, s.find_first_not_of(whitespace));
	s.erase(s.find_last_not_of(whitespace) + 1U);
}

bool ConfigData::operator!=(const ConfigData& d) const
{
	return data != d.data;
}

bool ConfigData::read(const string& key, const bool& value) const
{
	// Return the value corresponding to key or given default value if key is not found
	std::map<string, string>::const_iterator p = data.find(key);
	if (p == data.end())
	{
		return value;
	}

	string sup = p->second;
	for (string::iterator itr = sup.begin(); itr != sup.end(); ++itr)
	{
		*itr = toupper(*itr);
	}

	return sup == string("TRUE") || sup == string("T") || sup == string("YES") || sup == string("Y") || sup == string("1") || sup == string("ENABLE") || sup == string("ENABLED") || sup == string("ON") || sup == string("O");
}

long ConfigData::read(const string& key, const long& value) const
{
	// Return the value corresponding to key or given default value if key is not found
	std::map<string, string>::const_iterator p = data.find(key);
	if (p == data.end())
	{
		return value;
	}

	try
	{
		return atol(p->second.c_str());
	}
	catch (...)
	{
		return value;
	}
}

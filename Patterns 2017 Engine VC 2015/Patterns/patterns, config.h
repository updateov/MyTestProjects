#pragma once
#include <string>
#include <map>
#include <iostream>
#include <fstream>
#include <sstream>

using std:: string;

class ConfigData
{
	// -----------------------------------------------------------------------------------------------------------------
	//    Data
	// -----------------------------------------------------------------------------------------------------------------

	protected:
		std::map<string, string> data;	// extracted keys and values

	// -----------------------------------------------------------------------------------------------------------------
	//    Methods
	// -----------------------------------------------------------------------------------------------------------------

	public:
		ConfigData();

		// Search for key and read value or optional default value
		bool read(const string& key, const bool& value) const;
		long read(const string& key, const long& value) const;

		virtual bool operator != (const ConfigData& d)const;

		string to_string() const;
		void from_string(const string& value);

		bool save_file(string filename);
		bool read_file(string filename);

	protected:
		static void trim(string& s);
};

#pragma once

#include "file_loading.h"
#include "patterns, input adapter.h"
#include "patterns, config.h"

using namespace std;

namespace patterns
{
	class file_loading
	{
	public:
		// File loading
		static bool load_file(string filename, input_adapter::patient* p, fetus& f, ConfigData& data);

	protected:
		enum format_file { eoldxmlfile, ev01file, einfile };

		static bool load_archive_xml(string, input_adapter::patient*, fetus&, ConfigData&);
		static bool	load_old_format_file(string , format_file, input_adapter::patient*, fetus&);
	};
}
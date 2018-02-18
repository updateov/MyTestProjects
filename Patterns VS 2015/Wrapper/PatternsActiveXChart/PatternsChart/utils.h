#pragma once

#include <string>
#include "tinyxml.h"
#include <time.h>
#include "afxinet.h"

using namespace std;

namespace patterns
{
	class utils
	{
	public:
		// xml functions
		static string to_string(const TiXmlDocument& doc);

		// http functions
		static string perform_server_request(string url, string appname = "");
		static string encode_url_parameter(string parameter);

		static string perform_server_postrequest(string url, string payload, string appname = "");
	};
}
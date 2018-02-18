#pragma once

using namespace std;

namespace patterns
{
	class base64
	{
	public:
		static string base64_encode(string const& string_to_encode);
		static string base64_decode(string const& encoded_string);
	};
}
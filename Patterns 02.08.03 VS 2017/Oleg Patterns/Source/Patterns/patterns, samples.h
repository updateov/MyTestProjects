#pragma once

#include <string>

namespace patterns
{
	class fetus;
	class samples;
}

class patterns::samples
{
	/*
	-------------------------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------------------------
	*/
protected:
	static fetus *create_0(fetus* f);
	static fetus *create_1(fetus* f);
	static fetus *create_2(fetus* f);
	static fetus *create_3(fetus* f);
	static fetus *create_4(fetus* f);
	static fetus *create_5(fetus* f);

	/*
	-------------------------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------------------------
	*/
public:
	static fetus *create(fetus* f, std::string);
	static fetus *create(fetus* f, long);
	static std::string get_name(long);
	static long get_number(void) {return 6;}
};


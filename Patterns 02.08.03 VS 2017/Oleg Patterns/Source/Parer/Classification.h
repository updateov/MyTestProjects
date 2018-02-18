#pragma once

#include <vector>

using namespace std;

namespace patterns_classifier
{
	class CClassification
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		enum classdef { t_notcalc = -2 };

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		vector<long> m_t;
		vector<long> m_c;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		CClassification(void);
		virtual ~CClassification(void);

		void reset(void);

		long classAtTime(long, long);
		long indexFromTime(long);
		void setClassAtTime(long, long);
		void setClassAtTime(long, long, long);
		long getLastTime(void);
		void debugCheck(void);
	};
}
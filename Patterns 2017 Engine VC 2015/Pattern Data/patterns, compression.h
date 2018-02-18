#pragma once

#include <vector>

namespace patterns
{
	using namespace std;

	/*
	=======================================================================================================================
	Codec for signal compression. This compresses Fhr or Up signals, for instance, assuming a fairly smooth signal. The
	compression scheme is basically a finite state machine. The automaton at all times has a current point and Mode q0
	is the normal mode, where points are no more than one unit apart. Mode qspecial is the escape mode to the four
	special cases, which are qabsolute, qdown, quniform and qup. Mode qabsolute sets the current point to an absolute
	value. Modes qdown and qup, respectivly, move relatively short distances, up ow down, to the next point. Mode
	quniform set the current point and adds a sequence of given length with this point. In all modes, moving right adds
	the current character to the signal, moving up or down set the current point one unit higher or lower without
	adding it to the signal and pausing changes mode. Here is a table demonstrating the relationship betweem states and
	operators: (q0) (qspecial) | | 00: up + right | 01: down + right | 10: pause 00: up + up 0: up | (qup) 1: right |
	01: down + down 0: down | (qdown) 1: right | 10: pause c | (qabsolute) 11: right c l n | (quniform) 11: right |
	where c is the absolute point (character) to move up or down to, l is the length (5 or 16 bits) and n the number of
	times, coded over l bits, that character c should be added to the signal. Operators are coded over two bits except
	in the qup and qdown modes, where they are coded over one bit. Values c is coded over eight bits, while l is coded
	over one bit. Methods from_ascii () and to_ascii () convert character vectors to and from quotable Ascii strings.
	These methods simply code every binary character over two characters taken from the set {0... 9, a... f}. Class
	bit_vector encapsulates all bitwise manipulation for concatenating sub-byte operators and values. rtest ->
	get_last_test_result ().
	=======================================================================================================================
	*/
	class compression
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	protected:
		class bit_vector
		{
		protected:
			long i;
			long n;
			static char k1[9];
			vector<char> x;
		public:
			bit_vector (void);
			bit_vector (const vector<char> &);
			virtual ~bit_vector(void);

			virtual void begin(void);
			virtual bool is_at_end(void) const;
			virtual char operator () (long);
			virtual bit_vector &operator () (char, long);
			virtual operator vector<char> (void) const;
		};

		string rtest;
		enum { zupright = 0, zdownright, zpause, zright, zup2 = 0, zdown2, zup1 = 0, zdown1 = 0, zright1 = 1, z5 = 0, z16 };

		static long find_uniform_length(const vector<char> &, long);
		virtual string passes_test_vector_to_string(const vector<char> &) const;

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:
		static vector<char> compress(const vector<char> &);
		static vector<char> decompress(const vector<char> &);
		static vector<char> from_ascii(const string &);
		virtual const string &get_last_test_result(void) const;
		virtual bool passes_test(void);
		static string to_ascii(const vector<char> &);
	};

}
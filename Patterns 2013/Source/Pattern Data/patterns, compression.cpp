#include "stdafx.h"
#include "patterns, compression.h"

using namespace patterns;

/*
 =======================================================================================================================
    Construction and destruction of bit vectors.
 =======================================================================================================================
 */
compression::bit_vector::bit_vector(void)
{
	i = n = 0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
compression::bit_vector::bit_vector(const vector<char> &x0)
{
	i = n = 0;
	x = x0;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
compression::bit_vector::~bit_vector(void)
{
}

/*
 =======================================================================================================================
    Move to iterator to the vector first bit. This is meant to be used with predicate is_at_end () and operator () (n).
 =======================================================================================================================
 */
void compression::bit_vector::begin(void)
{
	i = 0;
}

/*
 =======================================================================================================================
    Is the iterator past the vector's last bit? See begin () and operator () (n).
 =======================================================================================================================
 */
bool compression::bit_vector::is_at_end(void) const
{
	return i / 8 >= (long) x.size();
}

/*
 * Bit masks for bit-shift operators. Value k1 [i] has the first, most-significant
 * i bits set to true and all others to false.We use this while packing and
 * unpacking bits because C doesn't guarantee that bit-shift operators leave false
 * bits behind.See operators () ().
 */
char compression::bit_vector::k1[9] = { (char) 0x00, (char) 0x80, (char) 0xc0, (char) 0xe0, (char) 0xf0, (char) 0xf8, (char) 0xfc, (char) 0xfe, (char) 0xff };

/*
 * Read the next nc bits and move iterator. This reads the next nc bits, starting
 * at the current iterator position and moves it past the last bit read.See method
 * begin () and predicate is_at_end ().
 */
char compression::bit_vector::operator () (long nc)
{
	long ni = i % 8;
	char r = 0;

	if ((i + nc) / 8 >= (long) x.size());
	else if (ni + nc <= 8)
	{
		r = (x[i / 8] >> (8 - ni - nc)) &~k1[8 - nc];
	}
	else
	{
		r = (x[i / 8] << (nc + ni - 8)) & k1[16 - nc - ni] &~k1[8 - nc] | (x[i / 8 + 1] >> (16 - nc - ni)) &~k1[16 - nc - ni];
	}

	i += nc;
	return r;
}

/*
 * Add given bits to the end of the bit vector. Property n, always in [0, 8[,
 * holds the number of bits that the underlying vector's last byte holds.
 */
compression::bit_vector & compression::bit_vector::operator() (char c, long nc)
{
	if (nc < 0)
		nc = 0;
	if (nc > 8)
		nc = 8;
	if (n == 0)
	{
		x.push_back((c << (8 - nc)) & k1[nc]);
	}
	else if (n + nc <= 8)
	{
		x[x.
		size()
		- 1] |= (c << (8 - nc - n)) & k1[n + nc];
	}
	else
	{
		x[x.
		size()
		- 1] |= (c >> (n + nc - 8)) &~k1[n + nc - 8];
		x.push_back((c << (16 - n - nc)) & k1[n + nc - 8]);
	}

	n = (n + nc) % 8;

	return *this;
}

/*
 =======================================================================================================================
    Access the underlying vector.
 =======================================================================================================================
 */
compression::bit_vector::operator vector<char> (void) const
{
	return x;
}

/*
 =======================================================================================================================
    Compress given signal. This codes the given signal according to the scheme documented in the class comment.We rely
    on the zup2 and zup1 operators being 0 (zero) so that, if the bit vector does not end on a byte boundary, extra
    zeroes get decompressed into these operators, thus yielding nothing to the signal.This is why we add the extra
    zpause operator;
    we need to make sure extra bits will be interpreted as zup2 and zup1 operators.
 =======================================================================================================================
 */
vector<char> compression::compress(const vector<char> &x)
{
	/*~~~~~~~~~*/
	char c(0);
	bit_vector r;
	/*~~~~~~~~~*/

	for (long i = 0, count = (long) x.size(); i < count; i++)
	{
		// Special case for the first point => full point
		if (i == 0)
		{
			r(c = x[i], 8);
		}
		else
		{
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
			// Long unchanged serie of values => repetition
			long n = find_uniform_length(x, i);
			/*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

			if (n >= 10)
			{
				r(zpause, 2);
				r(zright, 2);
				r(c = x[i], 8);
				if (n < 32)
				{
					r(z5, 1);
					r((char) (unsigned char) n, 5);
				}
				else
				{
					r(z16, 1);
					r((char) (unsigned char) (n / 256), 8);
					r((char) (unsigned char) (n % 256), 8);
				}

				i += n - 1;
			}

			// Delta at least 10 => full point
			else if (abs((long) (unsigned char) c - (long) (unsigned char) x[i]) > 9)
			{
				r(zpause, 2);
				r(zpause, 2);
				r(c = x[i], 8);
			}

			// Delta at least 2 => +2/-2 and whatever +1/-1 necessary
			else if (abs((long) (unsigned char) c - (long) (unsigned char) x[i]) > 1)
			{
				r(zpause, 2);
				if ((unsigned char) c < (unsigned char) x[i])
				{
					c += 2;
					r(zup2, 2);
				}

				while ((unsigned char) c < (unsigned char) x[i])
				{
					c++;
					r(zup1, 1);
				}

				if ((unsigned char) c > (unsigned char) x[i])
				{
					c -= 2;
					r(zdown2, 2);
				}

				while ((unsigned char) c > (unsigned char) x[i])
				{
					c--;
					r(zdown1, 1);
				}

				r(zright1, 1);
			}

			// Delta +1
			else if ((unsigned char) c < (unsigned char) x[i])
			{
				c++;
				r(zupright, 2);
			}

			// Delta -1
			else if ((unsigned char) c > (unsigned char) x[i])
			{
				c--;
				r(zdownright, 2);
			}

			// Same value
			else
			{
				r(zright, 2);
			}
		}
	}

	if (!x.empty())
	{
		r(zpause, 2);	// see comment above.
	}

	return (vector<char> ) r;
}

/*
 =======================================================================================================================
    Decompress given stream. See class comment and comment for method compress ().This method mimics the actual
    compression scheme's finite state automaton.
 =======================================================================================================================
 */
vector<char> compression::decompress(const vector<char> &x0)
{
	/*~~~~~~~~~~~~~~~*/
	char c(0);
	enum { q0, qabsolute, qdown, qspecial, quniform, qup };
	long q = qabsolute;
	vector<char> r;
	/*~~~~~~~~~~~~~~~*/

	r.reserve((unsigned int) (x0.size() * 2));

	/*~~~~~~~~~~~~~~*/
	bit_vector x = x0;
	/*~~~~~~~~~~~~~~*/

	while (!x.is_at_end())
	{
		switch (q)
		  {
			case q0:
				switch (x(2))
				  {
					case zupright:
						r.push_back(++c);
						break;

					case zdownright:
						r.push_back(--c);
						break;

					case zpause:
						q = qspecial;
						break;

					case zright:
						r.push_back(c);
						break;
				  }

				break;

			case qabsolute:
				r.push_back(c = x(8));
				q = q0;
				break;

			case qdown:
			case qup:
				if (x(1) != zright1)
				{
					if (q == qdown)
						c--;
					else
						c++;
				}
				else
				{
					r.push_back(c);
					q = q0;
				}

				break;

			case qspecial:
				switch (x(2))
				  {
					case zup2:
						c += 2;
						q = qup;
						break;

					case zdown2:
						c -= 2;
						q = qdown;
						break;

					case zpause:
						q = qabsolute;
						break;

					case zright:
						q = quniform;
						break;
				  }

				break;

			case quniform:
				{
					/*~~~*/
					long n;
					/*~~~*/

					c = x(8);
					if (x(1) == z5)
					{
						n = (long) x(5);
					}
					else
					{
						n = 256L * (long) (unsigned char) x(8) + (long) (unsigned char) x(8);
					}

					for (long i = 0; i < n; i++)
					{
						r.push_back(c);
					}

					q = q0;
				}

				break;
		  }
	}

	return r;
}

/*
 =======================================================================================================================
    Find length of uniform sequence. We simply return the length of a uniform sequence in x starting at given position
    i.This will in [1, |x| - i].
 =======================================================================================================================
 */
long compression::find_uniform_length(const vector<char> &x, long i)
{
	/*~~~~~~*/
	long j, n;
	/*~~~~~~*/

	for (j = i + 1, n = (long) x.size(); j < n && x[i] == x[j]; j++);
	return j - i;
}

/*
 =======================================================================================================================
    Recreate vector from string representation. See method to_ascii ().
 =======================================================================================================================
 */
vector<char> compression::from_ascii(const string &x)
{
	/*~~~~~~~~~~~*/
	vector<char> r;
	long xi;
	/*~~~~~~~~~~~*/

	for (long i = 0, n = (long) x.size(); i < n; i += 2)
	{
		if (sscanf(x.substr(i, 2).c_str(), "%lx", &xi) != 0)
		{
			r.push_back((char) (unsigned char) xi);
		}
	}

	return r;
}

/*
 =======================================================================================================================
    Access last automated test result. See method passes_test ().
 =======================================================================================================================
 */
const string &compression::get_last_test_result(void) const
{
	return rtest;
}

/*
 =======================================================================================================================
    Does the class pass the automated test suite? This returns true if the class passes the test.The test is performed
    synchronously and may take a while.To access a more detailed diagnosis, whether this returns true or false, see
    method get_last_test_result ().
 =======================================================================================================================
 */
bool compression::passes_test(void)
{
	/*~~~~~~~~~~~~~~~~~*/
	long i, j;
	const long n = 20000;
	bool r = true;
	vector<char> x;
	/*~~~~~~~~~~~~~~~~~*/

	rtest = "";

	/* Test compression. */
	for (i = 0; i < n; i++)
	{
		x.clear();
		for (j = 0; rand() % 1000; j++);
		x.reserve(j);
		while (j--)
		{
			x.insert(x.end(), (char) (unsigned char) (rand() % 256));
		}

		if (x != decompress(compress(x)))
		{
			// noter résultat ici !
			rtest = "methods compress () and decompress () fail for string x = \"";
			rtest += passes_test_vector_to_string(x);
			rtest += "\"\ncompress (x) = \"";
			rtest += passes_test_vector_to_string(compress(x));
			rtest += "\"\ndecompress (compress (x)) = \"";
			rtest += passes_test_vector_to_string(decompress(compress(x)));
			rtest += "\"\n";
			r = false;
		}

		if (x != from_ascii(to_ascii(x)))
		{
			// noter résultat ici !
			rtest = "methods from_ascii () and to_ascii () fail for string \"";
			rtest += passes_test_vector_to_string(x);
			rtest += "\"\nto_ascii (x) = \"";
			rtest += to_ascii(x);
			rtest += "\"\from_ascii (to_ascii (x)) = \"";
			rtest += passes_test_vector_to_string(from_ascii(to_ascii(x)));
			rtest += "\"\n";
			r = false;
		}
	}

	/* Test Ascii encoding. */
	return r;
}

/*
 =======================================================================================================================
 =======================================================================================================================
 */
string compression::passes_test_vector_to_string(const vector<char> &x) const
{
	/*~~~~~~~~*/
	string r;
	char t[100];
	/*~~~~~~~~*/

	for (long i = 0, n = (long) x.size(); i < n; i++)
	{
		sprintf(t, "%s%ld", i ? "-" : "", (long) (unsigned char) x[i]);
		r += t;
	}

	return r;
}

/*
 =======================================================================================================================
    Create quotable Ascii string from given binary vector. We code every character over two lower-case hexadecimal
    digits.
 =======================================================================================================================
 */
string compression::to_ascii(const vector<char> &x)
{
	/*~~~~~~~~~*/
	string r;
	char t[1000];
	/*~~~~~~~~~*/

	for (long i = 0, n = (long) x.size(); i < n; i++)
	{
		sprintf(t, "%02lx", (long) (unsigned char) x[i]);
		r += t;
	}

	return r;
}

#pragma once

namespace patterns
{

	/*
	=======================================================================================================================
	! PROJECT: Foetal Heart Rate (FHR) Feature Detection CLASS: OOURA - FFT implementation Copyright(C) 1996-2001
	Takuya OOURA email: ooura@mmm.t.u-tokyo.ac.jp download: http://momonga.t.u-tokyo.ac.jp/~ooura/fft.html You may use,
	copy, modify this code for any purpose and without fee. You may distribute this ORIGINAL package.
	=======================================================================================================================
	*/
	class OOURA
	{
		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	public:

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		OOURA(void)
		{
		};

		/*
		===============================================================================================================
		===============================================================================================================
		*/
		virtual~OOURA(void)
		{
		};

		void fft(long, double *);
		void ifft(long, double *);
		void convolution(double *a, const double *b, const long n);

		/*
		-------------------------------------------------------------------------------------------------------------------
		-------------------------------------------------------------------------------------------------------------------
		*/
	private:
		void rdft(long n, long isgn, double *a, long NMAX = 32768, long NMAXSQRT = 128);
		void cdft(long, long, double *, long *, double *);
		void ddct(long, long, double *, long *, double *);
		void ddst(long, long, double *, long *, double *);
		void dfct(long, double *, double *, long *, double *);
		void dfst(long, double *, double *, long *, double *);

		void makewt(long nw, long *ip, double *w);
		void makect(long nc, long *ip, double *c);
		void bitrv2(long n, long *ip, double *a);
		void bitrv2conj(long n, long *ip, double *a);
		void cftfsub(long n, double *a, double *w);
		void cftbsub(long n, double *a, double *w);
		void cft1st(long n, double *a, double *w);
		void cftmdl(long n, long l, double *a, double *w);
		void rftfsub(long n, double *a, long nc, double *c);
		void rftbsub(long n, double *a, long nc, double *c);
		void dctsub(long n, double *a, long nc, double *c);
		void dstsub(long n, double *a, long nc, double *c);
	};
}
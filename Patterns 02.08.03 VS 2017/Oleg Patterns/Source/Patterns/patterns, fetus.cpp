#include "stdafx.h"

#include "patterns, fetus.h"
#include "patterns, conductor.h"

namespace patterns
{
	long fetus::cr_window(10);

#ifdef patterns_parer_classification
	bool fetus::classification_enabled(false);
#endif

	bool fetus::event_detection_enabled(true);

	// =================================================================================================================
	//    Construction and destruction.
	// ===================================================================================================================
	fetus::fetus(void)
	{
		iin_compute_contractions_now = false;

		m_key = "";

		contractions_last_calculated_index = 0;
		m_contractionsRateLast = 0;
		dcutoff = undetermined_date;
		dreset = undetermined_date;

#ifdef patterns_has_signal_processing
		m_devents = NULL;
#endif
		dstart = undetermined_date;
		edelta = 0;
		etotalfinal = 0;
		ereinjected = 0;

		notification_suspension_count = 0;
		notification_suspended = false;

		m_bComputeContractionRate = true;

#ifdef patterns_parer_classification
		reset_classification();
#endif
		inextcontraction = 0;
		inextfhr = 0;
		irealtime = true;
		kconductor = 0;
		prate = 4;
		m_fhrSampleRate = 4;
	}

	void fetus::clear()
	{
		reset();

		dcutoff = undetermined_date;
		dreset = undetermined_date;
		dstart = undetermined_date;
	}

	fetus::fetus(const fetus& curFetus)
	{
		iin_compute_contractions_now = false;

		m_key = "";

		contractions_last_calculated_index = 0;
		m_contractionsRateLast = 0;
#ifdef patterns_has_signal_processing
		m_devents = NULL;
#endif
		edelta = 0;
		etotalfinal = 0;
		ereinjected = 0;

		notification_suspension_count = 0;
		notification_suspended = false;

		m_bComputeContractionRate = true;

#ifdef patterns_parer_classification
		reset_classification();
#endif
		inextcontraction = 0;
		inextfhr = 0;
		irealtime = true;
		kconductor = 0;
		prate = 4;
		m_fhrSampleRate = 4;
		*this = curFetus;
	}

	fetus::~fetus(void)
	{
		for (long i = 0; i < (long)m_subscribers.size(); i++)
		{
			delete m_subscribers[i];
		}
		m_subscribers.clear();

#ifdef patterns_has_signal_processing
		if (m_devents)
		{
			delete m_devents;
			m_devents = NULL;
		}
#endif
	}

	// =================================================================================================================
	//    activation/deactivation of the modification notification. See the note(...) methods
	// ===================================================================================================================
	void fetus::suspend_notifications(bool value) const
	{
		if (!value)
		{
			--notification_suspension_count;
			if (notification_suspension_count <= 0)
			{
				notification_suspension_count = 0;
				if (notification_suspended)
				{
					const_cast<fetus*>(this)->note(subscription::mfetus);
					notification_suspended = false;
				}
			}
		}
		else
		{
			++notification_suspension_count;
		}
	}

	// =================================================================================================================
	//    return true is the notifications are suspended
	// ===================================================================================================================
	bool fetus::is_notifications_suspended() const
	{
		return notification_suspension_count > 0;
	}

	// =================================================================================================================
	//    Adjust the given array to the sample rate. We do not support all possible adjustment.For now, we can pass from 1 Hz
	//    to 4Hz and from 4 Hz to 1 Hz. If we pass -1 to the last parameter, this means we have to have extra points at the
	//    beginning of the vector in order to extrapolate correctly.Before doing the work, we copy the first point and insert
	//    it at the beginning.
	// ===================================================================================================================
	vector<char> fetus::adjust_sample_rate(const vector<char>& x, long f1, long f2, long p0)
	{
		vector<char> k;

		if ((f1 != 1 && f1 != 4) || (f2 != 1 && f2 != 4))
		{
			assert(0);
		}
		else
		{
			if (f1 == 4 && f2 == 1)
			{
				for (long i = 0, n = (long)x.size(); i < n; i += 4)
				{
					k.insert(k.end(), x[i]);
				}
			}
			else if (f1 == 1 && f2 == 4)
			{
				k.reserve(x.size() * 4 + 4);
				if (p0 == -1)
				{
					for (long i = 0; i < 3; ++i)
					{
						k.insert(k.end(), x[0]);
					}
				}

				for (long i = 0, n = (long)x.size(); i < n; i++)
				{
					k.insert(k.end(), x[i]);
					if (i != n - 1)
					{
						for (long j = 1; j < 4; j++)
						{
							k.push_back((char) (unsigned char)((j * (long) (unsigned char)x[i + 1] + (4 - j) * (long) (unsigned char)x[i]) / 4));
						}
					}
				}
			}
			else if (f1 == f2)
			{
				k.reserve(x.size() + 1);
				if (p0 == -1)
				{
					k.insert(k.end(), x[0]);
				}

				for (long i = 0, n = (long)x.size(); i < n; i++)
				{
					k.insert(k.end(), x[i]);
				}
			}
		}

		return k;
	}

	// =================================================================================================================
	//    Append a sample or an object to the corresponding set. See classes contraction, event and mark and method
	//    compute().
	// ===================================================================================================================
	void fetus::append_contraction(const contraction& c0)
	{
		long ic0 = find_contraction(c0.get_start());

		if (!get_contraction(ic0).intersects(c0))
		{
			m_contractions.insert(m_contractions.begin() + ic0, c0);
			m_bComputeContractionRate = true;

#ifdef patterns_parer_classification
			reset_classification();
#endif
			note(subscription::mfetus);
		}
	}

	// =================================================================================================================
	//    Append a group of contractions
	// ===================================================================================================================
	void fetus::append_contraction(const vector<contraction>& v)
	{
		if (v.size() == 0)
		{
			return;
		}

		// Mention that the contraction rate must be refreshed
		m_bComputeContractionRate = true;

#ifdef patterns_parer_classification
		reset_classification();
#endif
		if (m_contractions.size() == 0)
		{
			// Special case for first injection
			m_contractions.insert(m_contractions.end(), v.begin(), v.end());
			note(subscription::mfetus);
		}
		else
		{
			suspend_notifications(true);
			for (vector<contraction>::const_iterator itr = v.begin(); itr != v.end(); ++itr)
			{
				append_contraction(*itr);
			}

			suspend_notifications(false);
		}
	}


	// =================================================================================================================
	//    Append events
	// ===================================================================================================================
	void fetus::append_event(const event& e0)
	{
		long ie0 = find_event(e0.get_start());

		if (!get_event(ie0).intersects(e0))
		{
			m_events.insert(m_events.begin() + ie0, e0);

#ifdef patterns_parer_classification
			reset_classification();
#endif
			note(subscription::mfetus);
		}
	}

	// =================================================================================================================
	//    Append a group of events
	// ===================================================================================================================
	void fetus::append_event(const vector<event>& v)
	{
		if (v.size() == 0)
		{
			return;
		}


#ifdef patterns_parer_classification
		reset_classification();
#endif
		if (m_events.size() == 0)
		{
			// Special case for first injection
			m_events.insert(m_events.end(), v.begin(), v.end());
			note(subscription::mfetus);
		}
		else
		{
			suspend_notifications(true);
			for (vector<event>::const_iterator itr = v.begin(); itr != v.end(); ++itr)
			{
				append_event(*itr);
			}

			suspend_notifications(false);
		}
	}

	void fetus::append_empty_fhr(long n)
	{
		if (n > 0)
		{
			m_fhr.resize(m_fhr.size() + n - 1, 0);
			append_fhr(0);
		}
	}

	void fetus::append_empty_mhr(long n)
	{
		if (n > 0)
		{
			m_mhr.resize(m_mhr.size() + n - 1, 0);
			append_mhr(0);
		}
	}

	void fetus::append_empty_up(long n)
	{
		if (n > 0)
		{
			m_up.resize(m_up.size() + n - 1, 0);
			append_up(0);
		}
	}

	void fetus::append_fhr(long x0)
	{
		m_fhr.insert(m_fhr.end(), (char) (unsigned char)x0);

		if (is_real_time())
		{
			make_event_detector();
			append_to_event_detector(get_number_of_fhr() - inextfhr);
		}

		note(subscription::mfetus);
	}

	void fetus::append_mhr(long x0)
	{
		m_mhr.insert(m_mhr.end(), (char) (unsigned char)x0);
		note(subscription::mfetus);
	}

	void fetus::append_fhr(const vector<char>& x0)
	{
		append_fhr(x0.begin(), x0.size());
	}

	void fetus::append_fhr(const vector<char>::const_iterator& begin, long count)
	{
		if (count > 0)
		{
			m_fhr.reserve(m_fhr.size() + count);
			if (count > 1)
			{
				m_fhr.insert(m_fhr.end(), begin, begin + count - 1);
			}

			append_fhr((long) (unsigned char) *(begin + count - 1));
		}
	}

	void fetus::append_mhr(const vector<char>& x0)
	{
		if (x0.size() > 0)
		{
			m_mhr.reserve(m_mhr.size() + x0.size());
			m_mhr.insert(m_mhr.end(), x0.begin(), x0.end() - 1);

			append_mhr((long) (unsigned char)x0[x0.size() - 1]);
		}
	}

	// =================================================================================================================
	//    append the last n points to events detector algorithm(evonium). Before passing the points to evonium, we make sure
	//    to pass the data at 4Hz.If it's not the case, we adjust the sample rate. The data is extrapolate
	// ===================================================================================================================
	void fetus::append_to_event_detector(long n0)
	{
#ifdef patterns_has_signal_processing
		if (n0 <= 0)
		{
			return;
		}

		// Prepare the fhr data
		if (get_hr_sample_rate() == 1)
		{
			throw exception("FHR must be in 4 Hz");
		}

		unsigned char*	x0 = new unsigned char[n0];
		for (long i = 0; i < n0; ++i)
		{
			long p = get_fhr(get_number_of_fhr() - n0 + i);

			if (p == 255)
			{
				p = 0;
			}

			x0[i] = (unsigned char)p;
		}

		// Prepare the contraction data
		long nnewcontractions = GetContractionsCount() - inextcontraction;
		long*  c = new long[nnewcontractions == 0 ? 1 : nnewcontractions * 3];

		long nnewfinalcontractions = 0;
		for (long i = 0; i < nnewcontractions; i++)
		{
			const contraction&	ctr = get_contraction(inextcontraction + i);
			if (ctr.is_final())
			{
				c[3 * i] = (4 * ctr.get_start() / get_up_sample_rate()) - (4 * edelta / get_hr_sample_rate());
				c[3 * i + 1] = (4 * ctr.get_end() / get_up_sample_rate()) - (4 * edelta / get_hr_sample_rate());
				c[3 * i + 2] = (4 * ctr.get_peak() / get_up_sample_rate()) - (4 * edelta / get_hr_sample_rate());
				nnewfinalcontractions++;
			}
		}

		// Process
		m_devents->ProceedBuffer();
		m_devents->Append(x0, n0, c, nnewfinalcontractions);

		delete[] c;
		delete[] x0;

		inextfhr = get_number_of_fhr();
		inextcontraction += nnewfinalcontractions;
#endif
	}

	void fetus::append_up(long p0)
	{
		m_up.push_back((char) (unsigned char)p0);
		compute_contractions();
		note(subscription::mfetus);
	}

	void fetus::append_up(const vector<char>& p0)
	{
		append_up(p0.begin(), p0.size());
	}

	void fetus::append_up(const vector<char>::const_iterator& begin, long count)
	{
		if (count > 0)
		{
			m_up.reserve(m_up.size() + count);
			if (count > 1)
			{
				m_up.insert(m_up.end(), begin, begin + count - 1);
			}

			append_up((long) (unsigned char) *(begin + count - 1));
		}
	}

	// =================================================================================================================
	//    Compare sample and result sets separately. These predicates return true if all elements of the corresponding sets
	//    are equal, with one exception.In the case of Fhr and Up sample sets, if one of the sets has trailing zeroes while
	//    sets are otherwise equal, we return true.See operators ==() and !=() and methods contains...(). If iincludeoverlap
	//    is true, then two contractions that simply overlap are considered equal.If not, then two contractions have to have
	//    equal start, peak and end to be considered equal.
	// ===================================================================================================================
	bool fetus::are_contractions_equal(const fetus& a, bool iincludeoverlap) const
	{
		long i;
		long n = GetContractionsCount();
		bool r = n == a.GetContractionsCount();

		for (i = 0; i < n && r; i++)
		{
			r = iincludeoverlap ? get_contraction(i).intersects(a.get_contraction(i)) : get_contraction(i) == a.get_contraction(i);
		}

		return r;
	}


	bool fetus::are_events_equal(const fetus& a, bool iincludeoverlap) const
	{
		long i;
		long n = GetEventsCount();
		bool r = n == a.GetEventsCount();

		for (i = 0; i < n && r; i++)
		{
			r = iincludeoverlap ? get_event(i).intersects(a.get_event(i)) : get_event(i) == a.get_event(i);
		}

		return r;
	}

	bool fetus::are_fhr_equal(const fetus& a) const
	{
		long i;
		long n;
		long na = a.get_number_of_fhr();
		long nthis = get_number_of_fhr();
		bool r = true;

		n = max(na, nthis);
		for (i = min(na, nthis); i < n && r; i++)
		{
			r = (nthis < na ? a : *this).get_fhr(i) > 0;
		}

		for (i = 0, n = min(na, nthis); i < n && r; i++)
		{
			r = get_fhr(i) == a.get_fhr(i);
		}

		return r;
	}

	bool fetus::are_mhr_equal(const fetus& a) const
	{
		long i;
		long n;
		long na = a.get_number_of_mhr();
		long nthis = get_number_of_mhr();
		bool r = true;

		n = max(na, nthis);
		for (i = min(na, nthis); i < n && r; i++)
		{
			r = (nthis < na ? a : *this).get_mhr(i) > 0;
		}

		for (i = 0, n = min(na, nthis); i < n && r; i++)
		{
			r = get_mhr(i) == a.get_mhr(i);
		}

		return r;
	}

	bool fetus::are_up_equal(const fetus& a) const
	{
		long i;
		long n;
		long na = a.get_number_of_up();
		long nthis = get_number_of_up();
		bool r = true;

		n = max(na, nthis);
		for (i = min(na, nthis); i < n && r; i++)
		{
			r = (nthis < na ? a : *this).get_up(i) > 0;
		}

		for (i = 0, n = min(na, nthis); i < n && r; i++)
		{
			r = get_up(i) == a.get_up(i);
		}

		return r;
	}

	// =================================================================================================================
	//    Make character vector out of given string.
	// ===================================================================================================================
	vector<char> fetus::as_vector(const string& x) const
	{
		vector<char> r;

		for (long i = 0, n = (long)x.size(); i < n; i++)
		{
			r.insert(r.end(), x[i]);
		}

		return r;
	}

	// =================================================================================================================
	//    Code for Xml cdata section. We code out the cdata-ending sequence, "]]>" and we encode everything over the Ascii
	//    set.We use the following escape sequences: 0x7f switch to or from high-Ascii, 0x7e01 the "]]>" sequence, 0x7e02 the
	//    0x7f character, 0x7e03 the 0x7e character, 0x7e04 the 0x00 character, 0x7e05 the 0xff character. 0x7e06 the 0xfe
	//    character, See method decode_cdata.
	// ===================================================================================================================
	string fetus::code_cdata(const vector<char>& x) const
	{
		bool ihigh = false;
		string r;

		for (long i = 0, n = (long)x.size(); i < n; i++)
		{
			if (x[i] == '\x7f')
			{
				r += "\x7e\x02";
			}
			else if (x[i] == '\x7e')
			{
				r += "\x7e\x03";
			}
			else if (x[i] == '\x00')
			{
				r += "\x7e\x04";
			}
			else if (x[i] == '\xff')
			{
				r += "\x7e\x05";
			}
			else if (x[i] == '\xfe')
			{
				r += "\x7e\x06";
			}
			else if (x.size() - i >= 3 && x[i] == ']' && x[i + 1] == ']' && x[i + 2] == '>')
			{
				r += "\x7e\x01";
				i += 2;
			}
			else if ((x[i] & '\x80' ? 1 : 0) != (ihigh ? 1 : 0))
			{
				r += '\x7f';
				i--;
				ihigh = !ihigh;
			}
			else if (ihigh)
			{
				r += x[i] & '\x7f';
			}
			else
			{
				r += x[i];
			}
		}

		return r;
	}

	// =================================================================================================================
	//    Decide if we need to compute now. Contractions and features are considered separately, though contractions need to
	//    be considered first, as feature detection depends on contractions. We make assume that contractions and events are
	//    sorted as this reduces complexity in many cases.See methods append_contraction() and append_event().
	// ===================================================================================================================
	void fetus::compute_contractions(void)
	{
#ifdef patterns_has_signal_processing
		if (is_real_time())
		{
			compute_contractions_now();
		}
#endif
	}

	// =================================================================================================================
	//    Detect contractions immediately. As contraction detection must have a 1Hz sample set, we try and detect and
	//    downsample 4Hz sets. We recalculate nonfinal contraction in each iteration instead, this is why we delete nonfinal
	//    contractions, as detection will produce them back if need be.Of course, this implies that kcdwindow is larger than
	//    kcdright by at least about 600 + kcdleft, where 600 is the longest possible contraction(I pity that woman).
	// ===================================================================================================================
	void fetus::compute_contractions_now(void)
	{
#ifdef patterns_has_signal_processing
		// To avoid reentrance
		if (iin_compute_contractions_now)
		{
			return;
		}

		iin_compute_contractions_now = true;

		// No need to calculate too often
		if ((contractions_last_calculated_index > 0) && (contractions_last_calculated_index + get(kcdwait) > get_number_of_up()))
		{
			iin_compute_contractions_now = false;
			return;
		}

		long number_of_up = get_number_of_up();
		long frequency_of_up = get_up_sample_rate();

		// We don't reprocess ALL the up values, we go from the last final contraction end.
		long calculation_starting_point = 0;
		for (vector<contraction>::reverse_iterator i = m_contractions.rbegin(); i != m_contractions.rend(); ++i)
		{
			if (i->is_final())
			{
				calculation_starting_point = max(calculation_starting_point, i->get_end() - (30 * frequency_of_up));
				break;
			}
		}

		// If there is no contraction for a long time (20 minutes), we don't go back in time more that x indexes
		// since the last calculation starting index
		if (contractions_last_calculated_index - calculation_starting_point > 1200 * frequency_of_up)
		{
			calculation_starting_point += 600 * frequency_of_up;	// Add 60 seconds
		}

		// Need at least 15 seconds of up before doing something
		if (number_of_up - calculation_starting_point >= 15)
		{
			// Build interval to detect from
			vector<char> x;
			x.reserve(number_of_up - calculation_starting_point);
			x.insert(x.end(), m_up.begin() + calculation_starting_point, m_up.end());

			// Do the detection
			vector<contraction> detected_contractions;
			if (frequency_of_up == 1)
			{
				get_contraction_detection().detect(x, detected_contractions);
			}
			else
			{
				get_contraction_detection().detect(adjust_sample_rate(x, frequency_of_up, 1), detected_contractions);
			}

			// Remember the calculation starting index
			contractions_last_calculated_index = number_of_up;

			// Insert newly detected contractions in the fetus contraction pool
			suspend_notifications(true);

			// Remove previous nonfinal contractions
			remove_trailing_non_final_contractions();

			vector<contraction> added;

			for (long i = 0; i < (long)detected_contractions.size(); i++)
			{
				contraction ci = detected_contractions[i];

				// Readjust indexes
				if (frequency_of_up != 1)
				{
					ci *= frequency_of_up;
				}

				ci += calculation_starting_point;

				// Do we want that contraction ?
				if ((m_contractions.size() == 0) || (ci.get_start() >= m_contractions.back().get_end()))
				{
					// Contraction too close from the end of the tracing are marked as non final
					ci.set_as_final(ci.get_end() + get(kcdright) < number_of_up);
					added.push_back(ci);
				}
			}

			append_contraction(added);
			suspend_notifications(false);
		}

		// Done with this
		iin_compute_contractions_now = false;
#endif
	}

	// =================================================================================================================
	//    Detect contraction rate immediately.
	// ===================================================================================================================
	void fetus::ComputeContractionRateNow(void)
	{
		if (m_bComputeContractionRate)
		{
			m_bComputeContractionRate = false;

			// reset m_contractionRate and m_contractionRateIndex list and set their first entry as zero.
			m_contractionsRate.clear();
			m_contractionsRateIndex.clear();
			m_contractionsRate.insert(m_contractionsRate.end(), 0);
			m_contractionsRateIndex.insert(m_contractionsRateIndex.end(), 0);
			for (long i = 0; i < GetContractionsCount(); ++i)
			{
				const contraction& curContraction = get_contraction(i);
				if (!curContraction.is_strike_out())
				{
					long w = get_cr_window() * 60 * get_up_sample_rate();
					long j1 = find_cr_index(curContraction.get_peak());
					long j2 = find_cr_index(curContraction.get_peak() + w);
					long pos1 = j1 + 1;
					long pos2 = j2 + 2;

					m_contractionsRate.insert(m_contractionsRate.begin() + pos1, m_contractionsRate[j1] + 1);
					m_contractionsRateIndex.insert(m_contractionsRateIndex.begin() + pos1, curContraction.get_peak());

					m_contractionsRate.insert(m_contractionsRate.begin() + pos2, m_contractionsRate[j2 + 1] == 0 ? 0 : m_contractionsRate[j2 + 1] - 1);
					m_contractionsRateIndex.insert(m_contractionsRateIndex.begin() + pos2, curContraction.get_peak() + w);
					for (long j = pos1 + 1; j < pos2; ++j)
					{
						m_contractionsRate[j]++;
					}
				}
			}
		}
	}

	// Detect features(events) immediately.
#include <time.h>

	// =================================================================================================================
	//    // compute_all::meant to replace compute_now by using interface used in actual operation as // oppossed to set
	//    signal. To be used in standalone.
	// ===================================================================================================================
	void fetus::compute_all(void)
	{
#ifdef patterns_has_signal_processing
		suspend_notifications(true);

		// Copy the FHR & UP data
		vector<char> fhr = get_fhrs();
		vector<char> up = get_ups();

		// Dump all data from the fetus
		this->reset();

		// Re-inject the FHR & DATA in blocks of 60 seconds to simulate the real time mode
		set_as_real_time(true);

		append_up(up);
		append_fhr(fhr);

		suspend_notifications(false);
#endif
	}

#ifdef patterns_parer_classification
	void fetus::compute_classification_now(void)
	{
		if (iclassification && fetus::get_classification_enabled())
		{
			iclassification = false;
			parerclass.reset();
			parerclass.setFetus(this);
			parerclass.setRT(false);
			parerclass.setBatch(true);

			if (fetus::get_classification_enabled())
			{
				parerclass.classify();
			}
		}
	}
#endif
	void fetus::compute_events_now(void)
	{
#ifdef patterns_has_signal_processing
		if (get_number_of_fhr() > (60 * get_hr_sample_rate()))	// At least one minute require...
		{
			CFHRSignal a;

#ifdef patterns_research
			a.SetExtendedAtypicalClassification();
#endif

			a.ActivateBasVarCalc(true);

			// get value from m_devents
			long i;

			// get value from m_devents
			long n;
			bool ichange = false;
			double*	 s = new double[get_number_of_fhr()];
			long*  c = new long[GetContractionsCount() * 3];

			// Move data to the CFHRSignal instance.
			for (i = 0, n = get_number_of_fhr(); i < n; i++)
			{
				long p = get_fhr(i);

				if (p == 255)
				{
					p = 0;
				}

				s[i] = p;
			}

			a.SetSignal(n, s);
			for (i = 0, n = GetContractionsCount(); i < n; i++)
			{
				c[3 * i] = 4 * get_contraction(i).get_start() / get_up_sample_rate();
				c[3 * i + 1] = 4 * get_contraction(i).get_end() / get_up_sample_rate();
				c[3 * i + 2] = 4 * get_contraction(i).get_peak() / get_up_sample_rate();
			}

			a.SetContraction(GetContractionsCount(), c);
			delete[] c;
			a.m_lContrDet = n;

			// Compute synchronously. ;
			// * clock_t _t = clock();
			a.RepairSignal();
			a.BandPass();
			a.LowPass();
			a.MinVarPass();
			a.DetectBaseline();
			a.MultiHypothesis();
			//a.BP2Processing();
			a.AccelDecel();

			// printf(" CFHRSignal::...(): %ld s.\n",(long)((clock() - _t)/CLOCKS_PER_SEC));
			// ;
			// Move results out of the CFHRSignal instance. ichange = m_events.size() > 0 || a.m_nOutputObjects > 0;
			ichange = m_events.size() > 0 || a.m_OutputFhrPartSet.size() > 0;
			m_events.clear();
			for (i = 0; i < a.m_TotalOutputFhrPartSet.size(); i++)
			{
				event e0;
				a.m_TotalOutputFhrPartSet.getAt(i)->toEvent(&e0);
				m_events.insert(m_events.end(), e0);
			}

			if (ichange)
			{
				note(subscription::mfetus);
			}
		}
#endif
	}

	// =================================================================================================================
	//    Perform immediate and synchronous computation. We use the current data set for computation, returning only when we
	//    are done.The resulting contractions and events are stored.See methods compute(), get_contraction(), get_event(),
	//    get_number_of_contractions(), get_number_of_events(), is_real_time() and set_as_real_time().
	// ===================================================================================================================
	void fetus::compute_now(void)
	{
		suspend_notifications(true);

#ifdef patterns_standalone
		compute_all();
		fetch_events();
#else
		compute_contractions_now();
		ComputeContractionRateNow();
		compute_events_now();
#endif
		suspend_notifications(false);
	}
	

	// =================================================================================================================
	//    Decode cdata-section content. See method code_cdata.
	// ===================================================================================================================
	string fetus::decode_cdata(const string& x) const
	{
		bool ihigh = false;
		enum { q0, qescape };
		long q = q0;
		string r;

		for (long i = 0, n = (long)x.size(); i < n; i++)
		{
			switch (q)
			{
				case q0:
					if (x[i] == 0x7f)
					{
						ihigh = !ihigh;
					}
					else if (x[i] == 0x7e)
					{
						q = qescape;
					}
					else if (ihigh)
					{
						r += x[i] | 0x80;
					}
					else
					{
						r += x[i];
					}
					break;

				case qescape:
					if (x[i] == 0x01)
					{
						r += "]]>";
					}
					else if (x[i] == 0x02)
					{
						r += 0x7f;
					}
					else if (x[i] == 0x03)
					{
						r += 0x7e;
					}
					else if (x[i] == 0x04)
					{
						r += (char)0x00;
					}
					else if (x[i] == 0x05)
					{
						r += (char) (unsigned char)0xff;
					}
					else if (x[i] == 0x06)
					{
						r += (char) (unsigned char)0xfe;
					}
					else
					{
						i--;
					}

					q = q0;
					break;
			}
		}

		return r;
	}

	void fetus::delete_event_detector(void)
	{
#ifdef patterns_has_signal_processing
		if (m_devents)
		{
			delete m_devents;
		}

		m_devents = 0;
		etotalfinal = 0;
		ereinjected = 0;
#endif
	}

	// Name of dictionary entry for tag name. See methods import() and import_tag().
	const string fetus::dtagname = "com-lms-patterns-tag-name";

	// =================================================================================================================
	//    Export the current data set to Xml or In formats. This exports the data set, not the class's configuration, such as
	//    the current real-time status.We do export whatever results were computed. By default, the data is exported as
	//    Xml.This is the only format that can be imported back and carries contractions and event.Format fin contains the
	//    Fhr and Up samples, the latter downsampled from 4 Hz to 1 Hz if need be.Three seconds of tracing are stored on each
	//    line, 12 Fhr samples, 3 Up samples. Exporting as C++ yields variables recreating the four data sets.This is meant
	//    to be used by unit-testing methods. 
	// ===================================================================================================================
	string fetus::export_to_string(format f) const
	{
		string r;

		switch (f)
		{
			case fcpp:
				r = export_cpp();
				break;

			case fxml:
				r = export_archive_xml();
				break;

			case fin:
				r = export_in();
				break;
		}

		return r;
	}

	string fetus::export_cpp(void) const
	{
		long i;
		long n;
		string r = "patterns::fetus *create_fetus(bool bRawTracing)\n{\n\tusing namespace patterns;\n\t\n""\tlong i;\n";
		char t[1000];
		const long w = 50;
		const long wblock = 65000;

		// Export contractions.
		if (GetContractionsCount() > 0)
		{
			r += "\tconst contraction kcontractions[] = {";
			for (i = 0, n = GetContractionsCount(); i < n; i++)
			{
				const contraction&	ci = get_contraction(i);

				sprintf(t, "%s\n\t\tcontraction(%ld, %ld, %ld)", i > 0 ? "," : "", (long)ci.get_start(), (long)ci.get_peak(), (long)ci.get_end());
				r += t;
			}

			r += "};\n";
		}

		// Export events.
		if (GetEventsCount() > 0)
		{
			r += "\tconst event kevents[] = {";
			for (i = 0, n = GetEventsCount(); i < n; i++)
			{
				const event&  ei = get_event(i);

				sprintf
					(
					t,
					"%s\n\t\tevent(%ld, %ld, %ld, %ld, %g, %g,(event::type) %ld, %g, %g, %g, %g, %g, %d, %d, %d, %d, %d, %ld)",
					i > 0 ? "," : "",
					(long) ei.get_contraction(),
					(long) ei.get_start(),
					(long) ei.get_peak(),
					(long) ei.get_end(),
					(double) ei.get_y1(),
					(double) ei.get_y2(),
					(long) ei.get_type(),
					(double) ei.get_confidence(),
					(double) ei.get_repair(),
					(double) ei.get_height(),
					(double) ei.get_baseline_var(),
					(double) ei.get_peak_val(),
					(bool) ei.is_late(),
					(bool) ei.is_variable(),
					(long) ei.get_lag(),
					(bool) ei.is_noninterp(),
					(bool) ei.is_confirmed(),
					(long) ei.get_atypical()
					);
				r += t;
			}

			r += "};\n";
		}

		// Export fetal heart rate samples.
		if (get_number_of_fhr() > 0)
		{
			string r0 = compression::to_ascii(compression::compress(m_fhr));

			r += "\tconst vector <char> kfhr = compression::decompress(compression::from_ascii(";
			if (r0.size() > wblock)
			{
				r += "string(";
			}

			for (i = 0, n = (long)r0.size(); i < n; i += w)
			{
				r += "\n\t\t\"" + r0.substr(i, w) + "\"";
				if (i % wblock > (i + w) % wblock)
				{
					r += "\n\t\t) + string(";
				}
			}

			if (r0.size() > wblock)
			{
				r += ")";
			}

			r += "));\n";
		}

		// Export maternal heart rate samples.
		if (get_number_of_mhr() > 0)
		{
			string r0 = compression::to_ascii(compression::compress(m_mhr));

			r += "\tconst vector <char> kmhr = compression::decompress(compression::from_ascii(";
			if (r0.size() > wblock)
			{
				r += "string(";
			}

			for (i = 0, n = (long)r0.size(); i < n; i += w)
			{
				r += "\n\t\t\"" + r0.substr(i, w) + "\"";
				if (i % wblock > (i + w) % wblock)
				{
					r += "\n\t\t) + string(";
				}
			}

			if (r0.size() > wblock)
			{
				r += ")";
			}

			r += "));\n";
		}

		// Export uterine pressure samples.
		if (get_number_of_up() > 0)
		{
			string r0 = compression::to_ascii(compression::compress(m_up));

			r += "\tconst vector <char> kup = compression::decompress(compression::from_ascii(";
			if (r0.size() > wblock)
			{
				r += "string(";
			}

			for (i = 0, n = (long)r0.size(); i < n; i += w)
			{
				r += "\n\t\t\"" + r0.substr(i, w) + "\"";
				if (i % wblock > (i + w) % wblock)
				{
					r += "\n\t\t) + string(";
				}
			}

			if (r0.size() > wblock)
			{
				r += ")";
			}

			r += "));\n";
		}

		// Create fetus instance.
		r += "\tfetus *r = new fetus;\n\n";
		sprintf(t, "\tr->set_hr_sample_rate(%ld);\n", get_hr_sample_rate());
		r += t;
		sprintf(t, "\tr->set_up_sample_rate(%ld);\n", get_up_sample_rate());
		r += t;

		sprintf(t, "\tif (!bRawTracing)\n\t{\n");
		r += t;
		if (GetContractionsCount() > 0)
		{
			sprintf(t, "\t\tfor (i = 0; i < %ld; r->append_contraction(kcontractions[i++]));\n", (long)GetContractionsCount());
			r += t;
		}

		if (GetEventsCount() > 0)
		{
			sprintf(t, "\t\tfor (i = 0; i < %ld; r->append_event(kevents[i++]));\n", (long)GetEventsCount());
			r += t;
		}

		sprintf(t, "\t}\n");
		r += t;

		if (get_number_of_fhr() > 0)
		{
			r += "\tr->append_fhr(kfhr);\n";
		}

		if (get_number_of_mhr() > 0)
		{
			r += "\tr->append_mhr(kmhr);\n";
		}

		if (get_number_of_up() > 0)
		{
			r += "\tr->append_up();\n";
		}

		if (get_start_date() != undetermined_date)
		{
			sprintf(t, "\tr->set_start_date(%lld);\n", (long long)get_start_date());
			r += t;
		}

		// Function epilogue.
		r += "\treturn r;\n}\n\n";
		return r;
	}

	string fetus::export_in(void) const
	{
		bool i4Hzup = get_number_of_up() > get_number_of_fhr() / 2;
		string r;
		char t[1000];

		for (long i = 0, n = get_number_of_fhr(); i < n; i += 12)
		{
			long j;
			long m = get_number_of_up();

			// Fhr: 3 s at 4 Hz.
			for (j = 0; j < 12; j++)
			{
				sprintf(t, "%03ld ", (long)(i + j < n ? get_fhr(i + j) : 0));
				r += t;
			}

			// Up: 3 s at 1 Hz.
			for (j = 0; j < 3; j++)
			{
				long k = 0;

				if (i4Hzup && i + 4 * j < m)
				{
					k = get_up(i + 4 * j);
				}
				else if (!i4Hzup && i / 4 + j < m)
				{
					k = get_up(i / 4 + j);
				}

				sprintf(t, j == 2 ? "%03ld \n" : "%03ld ", k);
				r += t;
			}
		}

		return r;
	}

	string fetus::export_archive_xml() const
	{
		// LXP : TODO // NOT YET IMPLEMENTED
		return export_xml();
	}

	string fetus::export_xml() const
	{
		long i;
		long n;
		string r;
		char t[1000];

		// Create root tag. Export start date or sampling frequencies as needed.
		r = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<lms-patterns-fetus";
		if (get_start_date() != undetermined_date)
		{
			sprintf(t, " start-date=\"%lld\"", (long long)get_start_date());
			r += t;
		}

		if (get_hr_sample_rate() != 4)
		{
			sprintf(t, " fhr-sample-rate=\"%ld\"", (long)get_hr_sample_rate());
			r += t;
		}

		if (get_up_sample_rate() != 4)
		{
			sprintf(t, " up-sample-rate=\"%ld\"", (long)get_up_sample_rate());
			r += t;
		}

		r += ">\n";

		// Export contractions rate delta.
		vector<long> contractionsRate = GetContractionsRate();
		vector<long> contractionsRateIndex = GetContractionsRateIndex();

		for (i = 0, n = (long)contractionsRate.size(); i < n; i++)
		{
			sprintf(t, "\t<contraction-rate-delta where=\"%ld\" value=\"%ld\"/>\n", contractionsRateIndex[i], contractionsRate[i]);
			r += t;
		}

		// Export contractions.
		for (i = 0, n = (long)m_contractions.size(); i < n; i++)
		{
			sprintf(t, "\t<contraction start=\"%ld\" peak=\"%ld\" end=\"%ld\"/>\n", (long)m_contractions[i].get_start(), (long)m_contractions[i].get_peak(), (long)m_contractions[i].get_end());
			r += t;
		}

		// Export events.
		for (i = 0, n = (long)m_events.size(); i < n; i++)
		{
			sprintf
				(
				t,
				"\t<event start=\"%ld\" peak=\"%ld\" end=\"%ld\" contraction=\"%ld\" type=\"%ld\" y1=\"%g\" y2=\"%g\" confidence=\"%g\" repair=\"%g\" height=\"%g\" baseline_var=\"%g\" peak_val=\"%g\" is_late=\"%d\" is_variable=\"%d\" lag=\"%d\" is_noninterp=\"%d\" is_confirmed=\"%d\" atypical=\"%d\"/>\n",
				(long) m_events[i].get_start(),
				(long) m_events[i].get_peak(),
				(long) m_events[i].get_end(),
				(long) m_events[i].get_contraction(),
				(long) m_events[i].get_type(),
				(double) m_events[i].get_y1(),
				(double) m_events[i].get_y2(),
				(double) m_events[i].get_confidence(),
				(double) m_events[i].get_repair(),
				(double) m_events[i].get_height(),
				(double) m_events[i].get_baseline_var(),
				(double) m_events[i].get_peak_val(),
				(bool) m_events[i].is_late(),
				(bool) m_events[i].is_variable(),
				(long) m_events[i].get_lag(),
				(bool) m_events[i].is_noninterp(),
				(long) m_events[i].is_confirmed(),
				(long) m_events[i].get_atypical()
				);
			r += t;
		}

		// Export fetal heart rate samples.
		for (i = 0, n = (long)m_fhr.size(); i < n; i++)
		{
			sprintf(t, "\t<fhr-sample value=\"%ld\"/>\n", (long) (unsigned char)m_fhr[i]);
			r += t;
		}

		// Export maternal heart rate samples.
		for (i = 0, n = (long)m_mhr.size(); i < n; i++)
		{
			sprintf(t, "\t<mhr-sample value=\"%ld\"/>\n", (long) (unsigned char)m_mhr[i]);
			r += t;
		}

		// Export uterine pressure samples.
		for (i = 0, n = (long)m_up.size(); i < n; i++)
		{
			sprintf(t, "\t<up-sample value=\"%ld\"/>\n", (long) (unsigned char)m_up[i]);
			r += t;
		}

		r += "</lms-patterns-fetus>\n";
		return r;
	}

	// =================================================================================================================
	//    Fetch new generated events. By the time this method is process, new events can be generated, we loop until we get
	//    the newest events right away.Since we must access evonium's data, we have to lock the <total results array> before
	//    accessing it(and unlock it when done with it).Note that the array of fhr points passed to evonium could be a subset
	//    of the whole fhr array displayed. That's why we need to keep a delta that represent the first fhr point passed to
	//    evonium.We have the same concept for the contractions.
	// ===================================================================================================================
	void fetus::fetch_events(void) const
	{
#ifdef patterns_has_signal_processing
		if (m_devents)
		{
			m_devents->LockTotalResultObjects();	// so engine thread can not change buffer of total events
			if (m_devents->hasNewEvents())
			{
				vector<event> added;
				vector<event> saved;

				fhrPartSet*	 fset = m_devents->GetTotalResults();
				long ntotal = fset->size();
				for (long i = etotalfinal; i < ntotal; i++)
				{
					event curEvent;
					fset->getAt(i)->toEvent(&curEvent, edelta, cdelta);

					if (curEvent.get_start() >= ereinjected)
					{
						added.push_back(curEvent);
					}

					if (!fset->getAt(i)->isPending())
					{
						if (curEvent.get_start() >= ereinjected)
						{
							saved.push_back(curEvent);
						}

						++etotalfinal;
					}
				}

				suspend_notifications(true);
				const_cast<fetus*>(this)->remove_trailing_non_final_events();
				const_cast<fetus*>(this)->append_event(added);
				suspend_notifications(false);
				m_devents->resetNewEvents();
			}

			m_devents->LockTotalResultObjects(false);
		}
#endif
	}

	// =================================================================================================================
	//    Find contraction containing the given Up index. This returns the index of the last contractions with end at or
	//    after given position.
	// ===================================================================================================================
	long fetus::find_contraction(long i) const
	{
		long r = GetContractionsCount();
		while ((r > 0) && (m_contractions[r - 1].get_end() > i))
		{
			r--;
		}

		return r;
	}

	// =================================================================================================================
	//    Look for index in crate for given Up index. We first look at the last returned index and, if the given Up index is
	//    not close, that is if it does not fall between crateindex [cratelast - 1] and crateindex[cratelast + 1], we go for
	//    the binary search.
	// ===================================================================================================================
	long fetus::find_cr_index(long i) const
	{
		if (m_contractionsRateIndex.size() == 0)
		{
			return -1;
		}

		long a = m_contractionsRateLast;
		long b = m_contractionsRateLast;
		long n = (long)m_contractionsRateIndex.size();

		if (i < 0)
		{
			i = 0;
		}

		if (i >= m_contractionsRateIndex.back())
		{
			m_contractionsRateLast = n - 1;
		}
		else if (m_contractionsRateLast > 0 && i < m_contractionsRateIndex[m_contractionsRateLast - 1])
		{
			a = 0;
		}
		else if (i < m_contractionsRateIndex[m_contractionsRateLast])
		{
			m_contractionsRateLast--;
		}
		else if (m_contractionsRateLast + 2 < n && i > m_contractionsRateIndex[m_contractionsRateLast + 2])
		{
			b = n;
		}
		else if (m_contractionsRateLast + 2 < n && i == m_contractionsRateIndex[m_contractionsRateLast + 2])
		{
			m_contractionsRateLast += 2;
		}
		else if (m_contractionsRateLast + 1 < n && i >= m_contractionsRateIndex[m_contractionsRateLast + 1])
		{
			m_contractionsRateLast++;
		}

		if (a != b)
		{
			while (a < b - 1)
			{
				if (i == m_contractionsRateIndex[(a + b) / 2])
				{
					a = b = (a + b) / 2;
				}
				else if (i < m_contractionsRateIndex[(a + b) / 2])
				{
					b = (a + b) / 2;
				}
				else
				{
					a = (a + b) / 2;
				}
			}

			m_contractionsRateLast = a;
		}

		return m_contractionsRateLast;
	}

	// =================================================================================================================
	//    Find event containing the given Fhr index. See method find_contraction().
	// ===================================================================================================================
	long fetus::find_event(long i) const
	{
		long r = GetEventsCount();

		while (r > 0 && get_event(r - 1).get_end() > i)
		{
			r--;
		}

		return r;
	}

	// =================================================================================================================
	//    Access detection-related constants. This method is meant to eventually take constants from the current conductor,
	//    the conductor acting as en environment for contraction and feature detection. The kcd...constants are related to
	//    contraction detection.These constants are all meant for a 1-Hz sample rate.See compute_contractions_now() for
	//    constraints on these constants. Implementation note: when we get to adding non-kcd...constants, we need to move the
	//    get_up_sample_rate()*...operation inside the switch statement.See make_event_detector for kedleft.
	// ===================================================================================================================
	long fetus::get(constant k) const
	{
		long r = 0;

		switch (k)
		{
			case kcdrate:
				r = 10;
				break;

			case kcdleft:
				r = 30;
				break;

			case kcdright:
				r = 45;
				break;

			case kcdwait:
				r = 15;
				break;

			case kcdwindow:
				r = 300;
				break;

			case kedleft:
	#ifdef patterns_has_signal_processing
				if (m_devents)
				{
					r = (long)m_devents->GetConfig()->GetMaxDelay() * 60;
				}
	#endif
				break;
		}

		return get_up_sample_rate() * r;
	}

	// =================================================================================================================
	//    Access the currently associated conductor. See predicate has_conductor() and method set_conductor().
	// ===================================================================================================================
	const conductor& fetus::get_conductor(void) const
	{
		return *kconductor;
	}

	conductor& fetus::get_conductor(void)
	{
		return *kconductor;
	}

	// =================================================================================================================
	//    Get the contraction with given index. See append_contraction(), get_number_of_contractions(), reset_contractions()
	//    and set_contraction().
	// ===================================================================================================================
	const contraction& fetus::get_contraction(long i) const
	{
		static contraction cdump;

		if (i < 0)
		{
			i = 0;
		}

		if (i >= GetContractionsCount())
		{
			i = GetContractionsCount() - 1;
		}

		return i < 0 ? cdump : m_contractions[i];
	}

	contraction* fetus::get_contraction_starting(long start) const
	{
		long n = GetContractionsCount();
		for (long i = 0; i < n; ++i)
		{
			if (m_contractions[i].get_start() == start)
			{
				return const_cast<contraction*>(&m_contractions[i]);
			}
		}

		return 0;
	}

#ifdef patterns_has_signal_processing

	// =================================================================================================================
	//    Access common instance of contraction detection object. The instance is meant to be shared by all instances of the
	//    fetus class, so we make it static.
	// ===================================================================================================================
	contraction_detection& fetus::get_contraction_detection(void)
	{
		if (kcontractiondetector.get_latent_vector_cut_off() != 0.4)
		{
			kcontractiondetector.set_latent_vector_cut_off(0.4);
		}

		return kcontractiondetector;
	}
#endif

	vector<long> fetus::GetContractionsRate() const
	{
		return m_contractionsRate;
	}

	vector<long> fetus::GetContractionsRateIndex() const
	{
		return m_contractionsRateIndex;
	}
	// =================================================================================================================
	//    get contraction rate value from a up index.
	// ===================================================================================================================
	long fetus::GetContractionRate(long upi) const
	{
		return m_contractionsRate[find_cr_index(upi)];
	}

	// =================================================================================================================
	//    Get the cutoff date for event detection.
	// ===================================================================================================================
	date fetus::get_cutoff_date(void) const
	{
		return dcutoff;
	}


	// =================================================================================================================
	//    Get the reset date for event detection.
	// ===================================================================================================================
	date fetus::get_reset_date(void) const
	{
		return dreset;
	}

	// =================================================================================================================
	//    Get detected event with given index. See also methods get_last_considered_fhr(), reset_events() and
	//    is_up_to_date().
	// ===================================================================================================================
	const event& fetus::get_event(long i) const
	{
		static event edump;

		if (i < 0)
		{
			i = 0;
		}

		if (i >= GetEventsCount())
		{
			i = GetEventsCount() - 1;
		}

		return i < 0 ? edump : m_events[i];
	}

	event* fetus::get_event_starting(long start) const
	{
		long n = GetEventsCount();
		for (long i = 0; i < n; ++i)
		{
			if (m_events[i].get_start() == start)
			{
				return const_cast<event*>(&m_events[i]);
			}
		}

		return 0;
	}

	// =================================================================================================================
	//    Get fhr sample rate. See set_hr_sample_rate.
	// ===================================================================================================================
	long fetus::get_hr_sample_rate(void) const
	{
		return m_fhrSampleRate;
	}

	// =================================================================================================================
	//    Get the fetal heart rate sample with given index.
	// ===================================================================================================================
	long fetus::get_fhr(long i) const
	{
		if ((i < 0) || (i >= get_number_of_fhr()))
		{
			return 0;
		}

		return (long) (unsigned char)m_fhr[i];
	}

	// =================================================================================================================
	//    Get the maternal heart rate sample with given index.
	// ===================================================================================================================
	long fetus::get_mhr(long i) const
	{
		if ((i < 0) || (i >= get_number_of_mhr()))
		{
			return 0;
		}

		return (long) (unsigned char)m_mhr[i];
	}

	// =================================================================================================================
	//    Access last automated test result. See method passes_test().
	// ===================================================================================================================
	const string& fetus::get_last_test_result(void) const
	{
		return rtest;
	}

	// =================================================================================================================
	//    Calculate mean baseline level and variability between indexes lX1 and lX2 Results returned in mean_baseline and
	//    mean_var. values of -1.0 signifies that there was less than the minimum amount of baseline(2 minutes) in the window
	//    required to make a valid average.
	// ===================================================================================================================
	void fetus::get_mean_baseline(long lX1, long lX2, double* mean_baseline, double* mean_var) const
	{
		double dTotal = 0.0;
		double dTotalVar = 0.0;
		double dMean = -1.0;
		double dMeanVar = -1.0;
		long n = GetEventsCount();
		long total_samples = 0, total_samples_var = 0;
		long min_samples = 2 * 60 * get_hr_sample_rate();
		long lMinBasLen = 0;
		long lTotBasTrim = 0;

		// NOTE 27/05/08 : until have mechanism to access signal processing config without needing the signal
		// * processing engine code, we hardcode these params so that they are equivalent whether running Patterns in
		// * client or server mode
		lMinBasLen = 60;						// (30 / 2) * 4 -> need half of 30 second segment at 4 Hz
		lTotBasTrim = 40;						// 2 * 5 * 4 -> 5 seconds at 4 Hz on each side of baseline are trimmed for var calculations

		// ifdef patterns_has_signal_processing if (m_devents) { lMinBasLen = m_devents->GetBasSegLenForVarCalc() / 2;
		// // length of segments over which var is calculated lTotBasTrim = m_devents->GetBasTrimForVarCalc() * 2;
		// // amount trimmed from baseline extremeties when calculating var } else { /* ;
		// endif
		for (long i = 0; i < n; i++)
		{
			const event&  ei = get_event(i);

			if (ei.is_baseline())
			{
				if ((ei.get_start() <= lX2) && (ei.get_end() >= lX1))
				{
					long ns = ei.get_length();	// length of baseline
					if (ns - lTotBasTrim >= lMinBasLen) // baselines that are too short will not have variability calculated
					{
						if (ei.get_start() < lX1)
						{	// starts before window - don't consider samples outside of window
							ns -= (lX1 - ei.get_start());
						}

						if (ei.get_end() > lX2)
						{	// end after window - don't consider samples outside window
							ns -= (ei.get_end() - lX2);
						}

						total_samples += ns;
						dTotal += (double)ns * ((ei.get_y1() + ei.get_y2()) / 2.0); // take mean of y1 and y2 as level for individual baseline
						if (ei.get_baseline_var() > 0)
						{
							total_samples_var += ns;
							dTotalVar += (double)ns * (ei.get_baseline_var());		// var as calculated in engine
						}
					}
				}
			}
		}

		if (total_samples > min_samples)	// enough baseline to make valid estimate
		{
			dMean = (double)dTotal / total_samples;
		}

		if (total_samples_var > min_samples)
		{
			dMeanVar = (double)dTotalVar / total_samples;
		}
		(*mean_baseline) = dMean;
		(*mean_var) = dMeanVar;
	}

	void fetus::get_mean_baseline(long lX1, long lX2, int& meanBaseline, int& meanVariability) const
	{
		double mean_baseline,  mean_var;
		get_mean_baseline(lX1, lX2, &mean_baseline, &mean_var);
		meanBaseline = (int) floor(mean_baseline);
		meanVariability = (int)floor(mean_var);
	}
	// =================================================================================================================
	//    Access the current number of stored contractions. See method get_contraction().
	// ===================================================================================================================
	long fetus::GetContractionsCount(void) const
	{
		return (long)m_contractions.size();
	}

	// =================================================================================================================
	//    Access the current number of detected events. See method get_event().
	// ===================================================================================================================
	long fetus::GetEventsCount(void) const
	{
		return (long)m_events.size();
	}

	// =================================================================================================================
	//    Access the current number of fetal heart rate samples. See method get_fhr().
	// ===================================================================================================================
	//inline long fetus::get_number_of_fhr(void) const
	//{
	//	return (long)m_fhr.size();
	//}

	// =================================================================================================================
	//    Access the current number of fetal heart rate samples. See method get_fhr().
	// ===================================================================================================================
	inline long fetus::get_number_of_mhr(void) const
	{
		return (long)m_mhr.size();
	}

	// =================================================================================================================
	//    Access the current number of uterine pressure samples. See method get_up().
	// ===================================================================================================================
	//inline long fetus::get_number_of_up(void) const
	//{
	//	return (long)m_up.size();
	//}

	// =================================================================================================================
	//    Get the starting date for all data sets.
	// ===================================================================================================================
	date fetus::get_start_date(void) const
	{
		return dstart;
	}

	// =================================================================================================================
	//    Get the uterine pressure sample with given index. See append_up(), get_number_of_up(), reset_up()
	// ===================================================================================================================
	long fetus::get_up(long i) const
	{
		if ((i < 0) || (i >= get_number_of_up()))
		{
			return 0;
		}

		return (long) (unsigned char)m_up[i];
	}

	// =================================================================================================================
	//    Get up sample rate. See set_up_sample_rate.
	// ===================================================================================================================
	long fetus::get_up_sample_rate(void) const
	{
		return prate;
	}

	// =================================================================================================================
	//    Do we have an associated conductor?
	// ===================================================================================================================
	bool fetus::has_conductor(void) const
	{
		return kconductor ? true : false;
	}

	// =================================================================================================================
	//    Does this instance have an event-detection cutoff date?
	// ===================================================================================================================
	bool fetus::has_cutoff_date(void) const
	{
#if defined(patterns_retrospective) && !defined(OEM_patterns)
		return true;
#else
		return dcutoff != undetermined_date;
#endif
	}

	// =================================================================================================================
	//    Does this instance have an event-detection reset date?
	// ===================================================================================================================
	bool fetus::has_reset_date(void) const
	{
		return dreset != undetermined_date;
	}

	// =================================================================================================================
	//    Does this instance have a start date?
	// ===================================================================================================================
	bool fetus::has_start_date(void) const
	{
		return dstart != undetermined_date;
	}

	// =================================================================================================================
	//    Convert a systemtime to a time_t
	// =================================================================================================================
	time_t fetus::convert_to_time_t(SYSTEMTIME st)
	{
		FILETIME ft;
		if (SystemTimeToFileTime(&st, &ft) == 0)
		{
			throw exception("Unable to convert the timestamp to a proper FILETIME");
		}

		ULARGE_INTEGER i64;
		i64.LowPart = ft.dwLowDateTime;
		i64.HighPart = ft.dwHighDateTime;

		return (time_t) ((i64.QuadPart - 116444736000000000) / 10000000);
	}

	// =================================================================================================================
	//    Convert a time_t to a system time
	// =================================================================================================================
	SYSTEMTIME fetus::convert_to_SYSTEMTIME(time_t t)
	{
		FILETIME ft;
		LONGLONG ll;
		ll = Int32x32To64(t, 10000000) + 116444736000000000;
		ft.dwLowDateTime = (DWORD) ll;
		ft.dwHighDateTime = (DWORD) (ll >> 32);

		SYSTEMTIME st;
		if (FileTimeToSystemTime(&ft, &st) == 0)
		{
			throw exception("Unable to convert the timestamp to a proper SYSTEMTIME");
		}

		return st;
	}

	// =================================================================================================================
	//    Convert a local time_t to a utc systemtime
	// =================================================================================================================
	SYSTEMTIME fetus::convert_to_utc(SYSTEMTIME st)
	{
		TIME_ZONE_INFORMATION TimeZoneInfo;
		if (GetTimeZoneInformation(&TimeZoneInfo) == TIME_ZONE_ID_INVALID)
		{
			throw exception("Unable to retrieve the time zone info");
		}

		SYSTEMTIME utc;
		if (TzSpecificLocalTimeToSystemTime(&TimeZoneInfo, &st, &utc) == 0)
		{
			throw exception("Unable to translate the time to a UTC time");
		}

		return utc;
	}

	// =================================================================================================================
	//    Convert a local time_t to a utc time_t
	// =================================================================================================================
	time_t fetus::convert_to_utc(time_t t)
	{
		return convert_to_time_t(convert_to_utc(convert_to_SYSTEMTIME(t)));
	}

	SYSTEMTIME fetus::convert_to_local(SYSTEMTIME utc)
	{
		TIME_ZONE_INFORMATION TimeZoneInfo;
		if (GetTimeZoneInformation(&TimeZoneInfo) == TIME_ZONE_ID_INVALID)
		{
			throw exception("Unable to retrieve the time zone info");
		}

		SYSTEMTIME st;
		if (SystemTimeToTzSpecificLocalTime(&TimeZoneInfo, &utc, &st) == 0)
		{
			throw exception("Unable to translate the time to a UTC time");
		}

		return st;
	}

	// =================================================================================================================
	//    Convert a local time_t to a utc time_t
	// =================================================================================================================
	time_t fetus::convert_to_local(time_t t)
	{
		return convert_to_time_t(convert_to_local(convert_to_SYSTEMTIME(t)));
	}

#if defined(patterns_standalone)

	// =================================================================================================================
	//    Import the data set stored in the given Xml or In file. This replaces the current data set but otherwise leaves the
	//    current state(real-time status, for instance) unchanged. We also interpret the format used by Lms R&D for test
	//    files, for backward compatibility and because it is fairly simple to code here. We inhibit multiple and possibly
	//    redundant detections during import.Client component implementors should note that, when in real-time mode,
	//    importing a file with no contractions will trigger detection.
	// ===================================================================================================================
	void fetus::import(const string& t)
	{
		if (t.find('<') == string::npos && t.find("xml") == string::npos)
		{
			import_in(t);
		}
		else
		{
			import_xml(t);
		}
	}

	void fetus::import_in(const string& t)
	{
		reset();

		set_up_sample_rate(1);
		set_hr_sample_rate(4);

		// Scan the lines one by one
		string::size_type startToken = 0;
		int values[15];
		while (true)
		{
			// Find first "non-delimiter".
			string::size_type endToken = t.find_first_of("\r\n", startToken);

			// Found a token, add it to the vector.
			string line = t.substr(startToken, endToken - startToken);

			int nFieldsConverted = sscanf(line.c_str() , "%3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d %3d", &values[0],&values[1],&values[2],&values[3],&values[4],&values[5],&values[6],&values[7],&values[8],&values[9],&values[10],&values[11],&values[12],&values[13],&values[14]);
			if (nFieldsConverted == 15) // do nothing with data unless we have exactly 15 readings
			{
				for (int i = 0; i < 12; ++i)
				{
					m_fhr.push_back(values[i]);
				}

				for (int i = 12; i < 15; ++i)
				{
					m_up.push_back(values[i]);
				}
			}

			if (endToken == string::npos)
			{
				break;
			}

			startToken = endToken + 1;
		}

		note(subscription::mfetus);
	}

	// =================================================================================================================
	//    Import binary v01(GE) format
	// ===================================================================================================================
	bool fetus::import_v01(const unsigned char* t, long n)
	{
		long long first_tracing = 0;
		long long last_fhr = 0;
		long long last_up = 0;
		vector<char> tracing_fhr;
		vector<char> tracing_up;

		long max_duration = 48 * 60 * 60;	// We don't want more than 48 hours of tracings

		tracing_fhr.reserve(max_duration * 4);
		tracing_up.reserve(max_duration * 4);

		long i = 0;
		while (i + 24 < n)					// A tracing header is at least 25 bytes
		{
			// Find a block start
			if ((t[i] == 0x46) && (t[i + 1] == 0x57) && (t[i + 2] == 0x61) && (t[i + 3] == 0x76) && (t[i + 4] == 0x65))
			{
				i += 5;						// Skip the block header
				i += 4;						// Skip useless data

				// Retrieve the size of the block
				int block_size = *(unsigned short*) &(t[i]);
				i += 2;

				i += 7;						// Skip useless data

				// Retrieve the time associated to the block, unfortunatly, the time is in Local time !!
				time_t block_stamp = (time_t) (*(long*) &(t[i]));
				i += 4;

				// Calculate the index for that time (considering the data is in 4Hz, it's an easy one)
				long long block_index = block_stamp * 4;

				i += 2;						// Skip useless data

				// Calculate the useful payload of the block
				block_size -= 14;			// 14 is the overhead, constant for all block
				while (block_size > 3)		// Need at least 3 bytes for a segment header
				{
					// Read the channel
					byte segment_channel = t[i];

					// Read the channel segment size
					unsigned short segment_size = *(unsigned short*) &t[i + 1];
					ASSERT(segment_size > 0);

					// We are only interested in channel type 01 (FHR1) and 03 (UP)
					if ((segment_channel == 0x01) || (segment_channel == 0x03))
					{
						// Exception case, the first block ever
						if (first_tracing == 0)
						{
							ASSERT(block_index > 0);

							first_tracing = block_index;
							last_up = block_index;
							last_fhr = block_index;
						}

						// Deal with FHR
						if (segment_channel == 0x01)
						{
							// Exception gap, there is a gap between that block and the previous one, add 'NODATA'
							// to fill the gap
							long gap = (long)(block_index - last_fhr);
							if (gap > 0)
							{
								tracing_fhr.insert(tracing_fhr.end(), gap, 0);
								last_fhr += gap;
								gap = 0;
							}

							for (long j = -gap; j < segment_size; ++j)
							{
								tracing_fhr.push_back(t[i + 3 + j]);
								++last_fhr;
							}
						}

						// Deal with UP
						else if (segment_channel == 0x03)
						{
							// Exception gap, there is a gap between that block and the previous one, add 'NODATA'
							// to fill the gap
							long gap = (long)(block_index - last_up);
							if (gap > 0)
							{
								tracing_up.insert(tracing_up.end(), gap, 0);
								last_up += gap;
								gap = 0;
							}

							for (long j = -gap; j < segment_size; ++j)
							{
								tracing_up.push_back(t[i + 3 + j]);
								++last_up;
							}
						}
					}

					// Jump the segment
					i += 3 + segment_size;
					block_size -= (3 + segment_size);

					ASSERT(block_size >= 0);
					ASSERT(i <= n);
				}
			}
			else
			{
				++i;
			}
		}

		// Align the 2 buffers by adding 'NODATA' at the end of the shortest one
		if (tracing_fhr.size() < tracing_up.size())
		{
			tracing_fhr.insert(tracing_fhr.end(), tracing_up.size() - tracing_fhr.size(), 0);
		}
		else if (tracing_up.size() < tracing_fhr.size())
		{
			tracing_up.insert(tracing_up.end(), tracing_fhr.size() - tracing_up.size(), 0);
		}

		ASSERT(tracing_fhr.size() == tracing_up.size());

		// Trim the end of the no tracing
		long blank = 0;
		long size = tracing_fhr.size();
		while ((blank < size) 
			&& ((tracing_fhr[size - (blank + 1)] == 0) || (tracing_fhr[size - (blank + 1)] == 255)) 
			&& ((tracing_up[size - (blank + 1)] < 5) || (tracing_up[size - (blank + 1)] >= 127)))
		{
			++blank;
		}
		if (blank > 0)
		{
			tracing_up.erase(tracing_up.begin() + size - (blank + 1), tracing_up.end());
			tracing_fhr.erase(tracing_fhr.begin() + size - (blank + 1), tracing_fhr.end());
		}

		// Cap the tracing
		long delta = (long)tracing_up.size() - (max_duration * 4);
		if (delta > 0)
		{
			tracing_up.erase(tracing_up.begin(), tracing_up.begin() + delta);
			tracing_fhr.erase(tracing_fhr.begin(), tracing_fhr.begin() + delta);
			first_tracing += delta;
		}

		// Trim the beginning of the no tracing
		blank = 0;
		size = tracing_fhr.size();
		while ((blank < size) 
			&& ((tracing_fhr[blank] == 0) || (tracing_fhr[blank] == 255)) 
			&& ((tracing_up[blank] < 5) || (tracing_up[blank] >= 127)))
		{
			++blank;
		}

		if (blank > 0)
		{
			tracing_up.erase(tracing_up.begin(), tracing_up.begin() + (blank - 1));
			tracing_fhr.erase(tracing_fhr.begin(), tracing_fhr.begin() + (blank - 1));
			first_tracing += blank;
		}

		// The UP buffer is in 4 Hz in the V01 file, we need it in 1Hz
		vector<char> tracing_up1Hz;
		if (tracing_up.size() > 0)
		{
			tracing_up1Hz.reserve(tracing_up.size() / 4);
			for (long j = 0; j < (long)tracing_up.size(); ++j)
			{
				if ((j % 4) == 0)
				{
					tracing_up1Hz.push_back(tracing_up[j]);
				}
			}
		}

		tracing_up.clear();

		// Here we have everything, the start of the tracing, the up & the fhr, store them in the fetus
		if (first_tracing > 0)
		{
			// Calculate the base offset for the GE V01 date (a timestamp in there file is the number of seconds
			// elapsed since a date => convert that to the proper time_t based on 1970/01/01 00:00:00
			SYSTEMTIME st_v01 = {1975, 12, 0, 31, 0, 0, 0, 0};
			time_t t_v01 = convert_to_time_t(st_v01);

			// Adjust the start of	 the tracing time
			time_t tracing_start = t_v01 + (time_t) (first_tracing / 4);

			// Put all the data in the fetus now
			reset();

			set_start_date(convert_to_utc(tracing_start));
			set_up_sample_rate(1);
			set_hr_sample_rate(4);

			append_up(tracing_up1Hz);
			append_fhr(tracing_fhr);
		}

		note(subscription::mfetus);

		return delta > 0;
	}

	void fetus::import_xml(const string& t)
	{
		long long klonglong;
		string str;
		string pcdata;
		string ptag;
		enum { q0, qcdata, qtag };
		long q = q0;

		reset();

		m_fhr.reserve(12 * 60 * 60 * 4);
		m_up.reserve(12 * 60 * 60);

		for (long i = 0, n = (long)t.length(); i < n; i++)
		{
			switch (q)
			{
				case q0:
					if (t[i] == '<')
					{
						q = qtag;
					}
					else if (t[i] == '\n' || t[i] == '\r' || t[i] == '\t');
					else
					{
						str += t[i];
					}
					break;

				case qcdata:
					if (t.size() - i >= 3 && t.substr(i, 3) == "]]>")
					{
						str += decode_cdata(pcdata);
						q = q0;
						i += 2;
					}
					else
					{
						pcdata += t[i];
					}
					break;

				case qtag:
					if (t.size() - i >= 8 && t.substr(i, 8) == "![CDATA[")
					{
						q = qcdata;
						pcdata = "";
						i += 7;
					}
					else if (t[i] == '>')
					{
	#define do_integer(x, n, s)		{ if (d.count(s) > 0) { (x).set_##n(get_integer(s)); } }

	#define do_long_long(x, n, s)	{ if (d.count(s) > 0) { (x).set_##n(get_long_long(s)); } }

	#define do_real(x, n, s)		{ if (d.count(s) > 0) { (x).set_##n(get_real(s)); } }

	#define get_bool(n)				((atoi(d.find(n)->second.c_str())) != 0)
	#define get_integer(n)			(atol(d.find(n)->second.c_str()))
	#define get_long_long(n)		(sscanf(d.find(n)->second.c_str(), "%I64d", &klonglong), klonglong)
	#define get_real(n)				(atof(d.find(n)->second.c_str()))
						map<string, string> d = import_tag(ptag);
						string dname = d.find(dtagname)->second;

						if (dname == "fhr-sample/")
						{
							m_fhr.insert(m_fhr.end(), (char) (unsigned char)(d.count("value") > 0 ? get_integer("value") : 0));
						}
						else if (dname == "up-sample/")
						{
							m_up.insert(m_up.end(), (char) (unsigned char)(d.count("value") > 0 ? get_integer("value") : 0));
						}
						else if (dname == "event/" || dname == "artifact/")
						{
							event ei;

							do_integer(ei, contraction, "contraction");
							do_integer(ei, end, "end");
							do_integer(ei, peak, "peak");
							do_integer(ei, start, "start");
							if (d.count("type") > 0)
							{
								ei.set_type((event::type) get_integer("type"));
							}

							do_real(ei, y1, "y1");
							do_real(ei, y2, "y2");
							do_real(ei, confidence, "confidence");
							do_real(ei, repair, "repair");
							do_real(ei, height, "height");
							do_real(ei, baseline_var, "baseline_var");
							do_real(ei, peak_val, "peak_val");
							if (d.count("is_late") > 0)
							{
								ei.set_as_late(get_bool("is_late"));
							}

							if (d.count("is_variable") > 0)
							{
								ei.set_as_variable(get_bool("is_variable"));
							}

							do_integer(ei, lag, "lag");
							if (d.count("is_noninterp") > 0)
							{
								ei.set_as_noninterp(get_bool("is_noninterp"));
							}

							if (d.count("is_confirmed") > 0)
							{
								ei.set_as_confirmed(get_bool("is_confirmed"));
							}

							do_integer(ei, atypical, "atypical");

							m_events.insert(m_events.end(), ei);
						}
						else if (dname == "contraction/")
						{
							contraction ci;

							do_integer(ci, end, "end");
							do_integer(ci, peak, "peak");
							do_integer(ci, start, "start");
							m_contractions.insert(m_contractions.end(), ci);
						}
						else if (dname == "/compressed-samples" || dname == "/compressed-fhr-samples")
						{
							if (!m_fhr.empty())
							{
								vector<char> x0 = compression::decompress(as_vector(str));

								for (long j = 0, m = (long)x0.size(); j < m; j++)
								{
									m_fhr.insert(m_fhr.end(), x0[j]);
								}
							}
							else
							{
								m_fhr = compression::decompress(as_vector(str));
							}
						}
						else if (dname == "/compressed-mhr-samples")
						{
							if (!m_mhr.empty())
							{
								vector<char> x0 = compression::decompress(as_vector(str));

								for (long j = 0, m = (long)x0.size(); j < m; j++)
								{
									m_mhr.insert(m_mhr.end(), x0[j]);
								}
							}
							else
							{
								m_mhr = compression::decompress(as_vector(str));
							}
						}
						else if (dname == "/compressed-up-samples")
						{
							if (!m_up.empty())
							{
								vector<char> p0 = compression::decompress(as_vector(str));

								for (long j = 0, m = (long)p0.size(); j < m; j++)
								{
									m_up.insert(m_up.end(), p0[j]);
								}
							}
							else
							{
								m_up = compression::decompress(as_vector(str));
							}
						}
						else if (dname == "lms-patterns-fetus")
						{
							do_long_long(*this, start_date, "start-date");
							do_integer(*this, hr_sample_rate, "fhr-sample-rate");
							do_integer(*this, up_sample_rate, "up-sample-rate");
						}
						else if (dname == "/sample")
						{
							m_fhr.insert(m_fhr.end(), (char) (unsigned char)atol(str.c_str()));
						}
						else if (dname == "sample/")
						{
							m_fhr.insert(m_fhr.end(), (char) (unsigned char)(d.count("value") > 0 ? get_integer("value") : 0));
						}
						else if (dname == "mhr-sample/")
						{
							m_mhr.insert(m_mhr.end(), (char) (unsigned char)(d.count("value") > 0 ? get_integer("value") : 0));
						}
						

						str = ptag = "";
						q = q0;
	#undef do_integer
	#undef do_long_long
	#undef do_real
	#undef get_integer
	#undef get_long_long
	#undef get_real
					}
					else
					{
						ptag += t[i];
					}
					break;
			}
		}

		set_hr_sample_rate(4);
		set_up_sample_rate(1);

		note(subscription::mfetus);
	}

	// =================================================================================================================
	//    Make dictionary out of given tag string. This creates a dictionary where each entry is a property of the given
	//    tag.We expect the tag without its enclosing brackets.The actual tag name is included as an entry and also a the
	//    value of a special entry with dtagname as the name.We add our own entry for the tag name itself because map does
	//    not order its entries. For instance, an Html tag <a href="http://bla.com"> would be expected as: a
	//    href="http://bla.com" and would be decoded into: {("a", ""), ("href", "http://bla.com"), (dtagname, "a")}.
	// ===================================================================================================================
	map<string, string> fetus::import_tag(string x) const
	{
		bool islash = false;
		string p;
		string pvalue;
		enum { q0, qblank, qquote, qpreblank, qvalue, qvalueblank };
		long q = q0;
		map<string, string> r;

		if (x.length() > 0 && x[x.length() - 1] == '/')
		{
			islash = true;
			x = x.substr(0, x.length() - 1);
		}

		for (long i = 0, n = (long)x.length(); i < n; i++)
		{
			switch (q)
			{
				case q0:
					if (x[i] == ' ')
					{
						q = qblank;
					}
					else if (x[i] == '=')
					{
						q = qvalueblank;
					}
					else
					{
						p += x[i];
					}
					break;

				case qblank:
					if (x[i] == ' ');
					else if (x[i] == '=')
					{
						q = qvalueblank;
					}
					else
					{
						import_tag_insert(&r, p, pvalue, islash);
						p = pvalue = "";
						q = q0;
						i--;
					}
					break;

				case qquote:
					if (x[i] == '\"')
					{
						q = qvalue;
					}
					else
					{
						pvalue += x[i];
					}
					break;

				case qpreblank:
					if (x[i] == ' ');
					else
					{
						p += x[i];
						q = q0;
					}
					break;

				case qvalue:
					if (x[i] == ' ')
					{
						q = qpreblank;
						import_tag_insert(&r, p, pvalue, islash);
						p = pvalue = "";
					}
					else
					{
						pvalue += x[i];
					}
					break;

				case qvalueblank:
					if (x[i] == ' ');
					else if (x[i] == '\"')
					{
						q = qquote;
					}
					else
					{
						q = qvalue;
						pvalue += x[i];
					}
					break;
			}
		}

		if (q != qpreblank)
		{
			import_tag_insert(&r, p, pvalue, islash);
		}

		return r;
	}

	void fetus::import_tag_insert(map<string, string>* r, string p, const string& pvalue, bool islash) const
	{
		if (!p.empty())
		{
			if (r->empty())
			{
				if (islash)
				{
					p += '/';
				}

				r->insert(pair<string, string> (dtagname, p));
			}

			r->insert(pair<string, string> (p, pvalue));
		}
	}
#endif

	// =================================================================================================================
	//    Are we in real-time mode? See discussion for method set_as_real_time().
	// ===================================================================================================================
	bool fetus::is_real_time(void) const
	{
#if defined(patterns_standalone)
		return irealtime;
#else
		return true;
#endif
	}

	// =================================================================================================================
	//    CFHRSignal member creation.We pass to the cfhrsigmal all fhr and contractions data from the last event generated
	//    less MAXDELAY(sets in evoniuim's component).If there is not event, we send all points. kedleft: constant to get the
	//    maxdelay constant from evonium.
	// ===================================================================================================================
	void fetus::make_event_detector(void)
	{
#ifdef patterns_has_signal_processing
		if (!m_devents)
		{
			long n = get_number_of_fhr();

			inextfhr = inextcontraction = 0;
			edelta = cdelta = 0;

			m_devents = new CFHRSignal();

#ifdef patterns_research
			m_devents->SetExtendedAtypicalClassification();
#endif

#if defined(patterns_retrospective) || defined(synchrone_processing)
			m_devents->SetSynchroneCalculation(true);
#endif
			if (GetEventsCount() > 0)
			{
				edelta = get_event(GetEventsCount() - 1).get_end() - get(kedleft);
				if (edelta < 0)
				{
					edelta = 0;
				}

				n -= edelta;
				inextfhr = edelta;
				for (long i = 0; i < GetContractionsCount(); i++)
				{
					if (get_contraction(i).get_start() > edelta)
					{
						inextcontraction = i;
						break;
					}
				}

				cdelta = inextcontraction;
			}

			append_to_event_detector(n);
		}
#endif
	}

	// =================================================================================================================
	//    Propagate given notification message to subscribers.
	// ===================================================================================================================
	void fetus::note(subscription::message m)
	{
		if ((m == subscription::mfetus) && (is_notifications_suspended()))
		{
			notification_suspended = true;
			return;
		}

		for (long i = 0; i < (long)m_subscribers.size(); i++)
		{
			m_subscribers[i]->note(m);
		}
	}

	void fetus::note(subscription::message m, long id)
	{
		for (long i = 0; i < (long)m_subscribers.size(); i++)
		{
			m_subscribers[i]->note(m, id);
		}
	}

	void fetus::note(subscription::message m, string id)
	{
		for (long i = 0; i < (long)m_subscribers.size(); i++)
		{
			m_subscribers[i]->note(m, id);
		}
	}

	// =================================================================================================================
	//    Assignment operator. Nothing to see here, folks, move along.
	// ===================================================================================================================
	fetus &fetus::operator=(const fetus& curFetus)
	{
		suspend_notifications(true);

		m_key = curFetus.m_key;
		curFetus.fetch_events();
		m_contractions = curFetus.m_contractions;
		contractions_last_calculated_index = curFetus.contractions_last_calculated_index;


		m_bComputeContractionRate = true;

#ifdef patterns_parer_classification
		reset_classification();
#endif
		dcutoff = curFetus.dcutoff;
		dreset = curFetus.dreset;
		dstart = curFetus.dstart;
		m_events = curFetus.m_events;
		etotalfinal = curFetus.etotalfinal;
		ereinjected = curFetus.ereinjected;
		delete_event_detector();
		m_up = curFetus.m_up;
		m_fhr = curFetus.m_fhr;
		m_mhr = curFetus.m_mhr;
		prate = curFetus.prate;
		m_fhrSampleRate = curFetus.m_fhrSampleRate;

		note(subscription::mfetus);
		suspend_notifications(false);

		return *this;
	}

	// =================================================================================================================
	//    Comparison operators.
	// ===================================================================================================================
	bool fetus::operator==(const fetus& x) const
	{
		return m_key == x.get_key() && 
				get_hr_sample_rate() == x.get_hr_sample_rate() && 
				get_up_sample_rate() == x.get_up_sample_rate() && 
				are_contractions_equal(x) && 
				are_events_equal(x) && 
				are_fhr_equal(x) && 
				are_mhr_equal(x) && 
				are_up_equal(x);
	}

	bool fetus::operator!=(const fetus& x) const
	{
		return !operator ==(x);
	}

	// =================================================================================================================
	//    Remove non-final objects. When calculating, we want to make sure that we replace non-final objects and replace them
	//    with any new objects that may have been calculated.This is used by event detection, compute_contractions_now() and
	//    the conductor when receiving objects computed by other instances, see method conductor::receive_results()
	// ===================================================================================================================
	void fetus::remove_trailing_non_final_contractions(void)
	{
		while (!m_contractions.empty() && !m_contractions.back().is_final())
		{
			m_contractions.pop_back();
		}
	}

	void fetus::remove_trailing_non_final_events(void)
	{
		while (!m_events.empty() && !m_events.back().is_final())
		{
			m_events.pop_back();
		}
	}

	// =================================================================================================================
	//    Reset sample or result sets. See related get_...() methods. Resetting contractions or events when in real-time mode
	//    might not yield the desired results, as detection will be triggered as soon as methods like append_up() are
	//    called.Methods that trigger detection are not documented and client components should not expect a given method to
	//    trigger or not detection.
	// ===================================================================================================================
	void fetus::reset(void)
	{
#ifdef patterns_parer_classification
		reset_classification();
#endif
		reset_mhr();
		reset_fhr();
		reset_up();
	}

#ifdef patterns_parer_classification
	void fetus::reset_classification(void)
	{
		iclassification = true;
		parerclass.reset();
	}
#endif
	void fetus::ResetContractions(void)
	{
		contractions_last_calculated_index = 0;
		m_contractions.clear();
		ResetContracionRate();

#ifdef patterns_parer_classification
		reset_classification();
#endif
		note(subscription::mfetus);
	}

	void fetus::reset_events(void)
	{
		m_events.clear();
		delete_event_detector();

#ifdef patterns_parer_classification
		reset_classification();
#endif
		note(subscription::mfetus);
	}

	void fetus::reset_fhr(void)
	{
		m_fhr.clear();
		reset_events();
		note(subscription::mfetus);
	}

	void fetus::reset_mhr(void)
	{
		m_mhr.clear();
		note(subscription::mfetus);
	}

	void fetus::reset_up(void)
	{
		m_up.clear();
		ResetContractions();
		note(subscription::mfetus);
	}


	// =================================================================================================================
	//    Restart the engine at sample index x, x being the sample index of the next sample append The next append will then
	//    calculate without recalculating events already output It is assumed that fetus has reloaded a cache of the output
	//    events prior to this call
	// ===================================================================================================================
	void fetus::restart_engine_after_reboot(long x)
	{
#ifdef patterns_has_signal_processing
		if (x == -1)
		{
			if (GetEventsCount() > 0)
			{
				x = get_event(GetEventsCount() - 1).get_end();
			}
			else
			{
				x = 0;
			}

			if (x < 0)
			{
				x = 0;
			}
		}

		ereinjected = x;
#endif
	}

	// =================================================================================================================
	//    Set real-time status of this instance. When real-time computation is active, the instance triggers contraction and
	//    feature detections as it sees fit, generally using the number of Fhr or Up samples as a criterion.When inactive,
	//    client components may trigger detection using method compute_now().
	// ===================================================================================================================
	void fetus::set_as_real_time(bool i0)
	{
		irealtime = i0;
	}

	// =================================================================================================================
	//    Set associated conductor instance. This is meant to be called only by the conductor class as it creates fetus
	//    instances.See method get_conductor() and predicate has_conductor()
	// ===================================================================================================================
	void fetus::set_conductor(conductor* k0)
	{
		kconductor = k0;
	}

	// =================================================================================================================
	//    Set the cutoff date for computing events. This is currently unused by the fetus class itself, though it is
	//    compared, exported and imported.It is the date before which all computed events are deemed irrelevant.This should
	//    be reflected to the user at the user-interface level.We may eventually inhibit all event detection before the
	//    cutoff date and a buffer time. IMPORTANT! See set_start_date().
	// ===================================================================================================================
	void fetus::set_cutoff_date(const date& d0)
	{
		if (dcutoff != d0)
		{
			dcutoff = d0;
		}

		note(subscription::mfetus);
	}

	// =================================================================================================================
	//    Set the reset date for events. This is currently unused by the fetus class itself, though it is
	//    compared, exported and imported.It is the date before which all computed events were reset and calculated batch mode
	// ===================================================================================================================
	void fetus::set_reset_date(const date& d0)
	{
		if (dreset != d0)
		{
			dreset = d0;
		}

		note(subscription::mfetus);
	}

	// =================================================================================================================
	//    Set fhr sample rate. This method set the sample rate of the fhr data passed to the fetus. We adjust the fhr and the
	//    event arrays to the given sample rate. Note that the events detection algorithm can only manage a 4hz sample
	//    rate.If the sample rate is different, an adjustment is necessary before passing the data to the algorithm.
	// ===================================================================================================================
	void fetus::set_hr_sample_rate(long r)
	{
		if (r != get_hr_sample_rate())
		{
			if (get_number_of_mhr() > 0 || get_number_of_fhr() > 0 || GetEventsCount() > 0)
			{
				m_fhr = adjust_sample_rate(m_fhr, get_hr_sample_rate(), r);
				m_mhr = adjust_sample_rate(m_mhr, get_hr_sample_rate(), r);

				for (long i = 0; i < GetEventsCount(); ++i)
				{
					event&	ei = const_cast<event&>(get_event(i));
					ei *= r;
					ei /= get_hr_sample_rate();
				}

				note(subscription::mfetus);
			}

			m_fhrSampleRate = r;
		}
	}

	// =================================================================================================================
	//    Set the starting date for all data sets. This is currently unused by the fetus class itself, though it is compared,
	//    exported and imported. IMPORTANT! Setting the start date also sets the cutoff date.This is done so that clients,
	//    especially test software, may simply disregard the cutoff issue while retaining basic functionality. Clients that
	//    do take this into account should set the cutoff date after* setting the start date at all times.See
	//    set_cutoff_date(). See also method tracing::draw_events().
	// ===================================================================================================================
	void fetus::set_start_date(const date& d0)
	{
		dstart = d0;

#if !defined(OEM_patterns)
		if (dcutoff == undetermined_date)
		{
			set_cutoff_date(d0);
		}
#endif
		note(subscription::mfetus);
	}

	// =================================================================================================================
	//    Set up sample rate. This method set the sample rate of the up data passed to the fetus. We adjust the up and the
	//    contraction arrays to the given sample rate. Note that the contraction detection algorithm can only manage a 1hz
	//    sample rate.If the sample rate is different, an adjustment is necessary before passing the data to the algorithm.
	// ===================================================================================================================
	void fetus::set_up_sample_rate(long r)
	{
		if (r != get_up_sample_rate())
		{
			if (get_number_of_up() > 0 || GetContractionsCount() > 0)
			{
				m_up = adjust_sample_rate(m_up, get_up_sample_rate(), r);
				for (long i = 0; i < GetContractionsCount(); ++i)
				{
					contraction&  ci = const_cast<contraction&>(get_contraction(i));
					ci *= r;
					ci /= get_up_sample_rate();
				}

				note(subscription::mfetus);
			}

			prate = r;
		}
	}

	// =================================================================================================================
	//    Strike given event out. As of August 2007, the current implementation of the conductor needs all properties of an
	//    event to instruct the input adapter to strike it out.
	// ===================================================================================================================
	void fetus::strike_event_out(long i)
	{
		if (i >= 0 && i < GetEventsCount())
		{
			const_cast<event&>(get_event(i)).set_as_strike_out();
			m_bComputeContractionRate = true;

#ifdef patterns_parer_classification
			reset_classification();
#endif
			note(subscription::mstrikeeventout, i);
			note(subscription::mfetus);
		}
	}

	void fetus::strike_event_out(event* curEvent)
	{
		long n = GetEventsCount();
		for (long i = 0; i < n; ++i)
		{
			if (get_event(i).get_start() == curEvent->get_start())
			{
				strike_event_out(i);
			}
		}
	}

	// =================================================================================================================
	//    Strike given event out. As of August 2007, the current implementation of the conductor needs all properties of an
	//    event to instruct the input adapter to strike it out.
	// ===================================================================================================================
	void fetus::accept_event(long i)
	{
		if (i >= 0 && i < GetEventsCount())
		{
			const_cast<event&>(get_event(i)).set_as_confirmed(true);

#ifdef patterns_parer_classification
			reset_classification();
#endif
			note(subscription::macceptevent, i);
			note(subscription::mfetus);
		}
	}

	void fetus::accept_event(event* curEvent)
	{
		long n = GetEventsCount();
		for (int i = 0; i < n; ++i)
		{
			if (get_event(i).get_start() == curEvent->get_start())
			{
				accept_event(i);
			}
		}
	}

	// =================================================================================================================
	//    Strike given contraction out. As of August 2007, the current implementation of the conductor needs all properties
	//    of an event to instruct the input adapter to strike it out.
	// ===================================================================================================================
	void fetus::strike_contraction_out(long i)
	{
		if (i >= 0 && i < GetContractionsCount())
		{
#ifdef ACTIVE_CONTRACTION
			m_contractions.erase(m_contractions.begin() + i);
#else
			const_cast<contraction&>(get_contraction(i)).set_as_strike_out();
#endif
			m_bComputeContractionRate = true;

#ifdef patterns_parer_classification
			reset_classification();
#endif

#ifndef ACTIVE_CONTRACTION
			note(subscription::mstrikecontractionout, i);
#endif
			note(subscription::mfetus);
		}
	}

	void fetus::strike_contraction_out(contraction* curContraction)
	{
		long n = GetContractionsCount();
		for (int i = 0; i < n; ++i)
		{
			if (get_contraction(i).get_start() == curContraction->get_start())
			{
				strike_contraction_out(i);
			}
		}
	}

	// =================================================================================================================
	//    Accept given subscription instance. See conductor::subscribe().
	// ===================================================================================================================
	void fetus::subscribe(subscription* s0)
	{
		m_subscribers.push_back(s0);
	}

	// =================================================================================================================
	//    Unsubscribe given object. See conductor::unsubscribe().
	// ===================================================================================================================
	void fetus::unsubscribe(subscription* s0)
	{
		for (vector < subscription * >::iterator itr = m_subscribers.begin(); itr != m_subscribers.end(); ++itr)
		{
			if (*itr == s0)
			{
				delete *itr;
				m_subscribers.erase(itr);

				return;
			}
		}
	}

	void fetus::unsubscribe(void* s0)
	{
		for (long i = 0, n = (long)m_subscribers.size(); i < n; i++)
		{
			if (m_subscribers[i]->is(s0))
			{
				delete m_subscribers[i];
				m_subscribers.erase(m_subscribers.begin() + i--);
				n--;
			}
		}
	}

#ifdef patterns_parer_classification
	long fetus::GetParerClassification(long t) const
	{
		if (has_cutoff_date())
		{
			long iLimit = get_hr_sample_rate() * (long)(get_cutoff_date() - get_start_date());
			if (t >= iLimit)
			{
				const_cast<fetus*>(this)->compute_classification_now();
				return parerclass.getLastOverallAtTime(t);
			}
		}

		return -1;
	}

	long fetus::GetParerClassification(long t, long* bl, long* bv, long* late, long* variable, long* prolonged) const
	{
		if (has_cutoff_date())
		{
			long iLimit = get_hr_sample_rate() * (long)(get_cutoff_date() - get_start_date());
			if (t >= iLimit)
			{
				const_cast<fetus*>(this)->compute_classification_now();
				(*bl) = parerclass.getLastClassAtTime(t, patterns_classifier::CParerClassifier::t_basLevel);
				(*bv) = parerclass.getLastClassAtTime(t, patterns_classifier::CParerClassifier::t_basVar);
				(*late) = parerclass.getLastClassAtTime(t, patterns_classifier::CParerClassifier::t_late);
				(*variable) = parerclass.getLastClassAtTime(t, patterns_classifier::CParerClassifier::t_variable);
				(*prolonged) = parerclass.getLastClassAtTime(t, patterns_classifier::CParerClassifier::t_prolonged);

				return parerclass.getLastOverallAtTime(t);
			}
		}

		// No GA or before the GA 36 week date limit
		(*bl) = -1;
		(*bv) = -1;
		(*late) = -1;
		(*variable) = -1;
		(*prolonged) = -1;
		return -1;
	}
#endif
}

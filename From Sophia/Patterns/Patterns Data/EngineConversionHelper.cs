using System;
using System.Globalization;
using System.Diagnostics;

namespace PeriGen.Patterns.Engine.Data
{
	public static class EngineConversionHelper
	{
		static double ToSafeDouble(this string value)
		{
			decimal result;
			if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				return (double)result;
			}
			if (value.Contains("NAN"))
			{
				return double.NaN;
			}
			if (value.Contains("INF0"))
			{
				if (value.Contains("-"))
				{
					return double.NegativeInfinity;
				}
				return double.PositiveInfinity;
			}
			throw new ArgumentOutOfRangeException("value", value, "This is not a parsable double");
		}

		static Contraction ContractionFromEngine(DateTime baseTime, string[] data)
		{
			Debug.Assert(data.GetLength(0) == 4);
			Debug.Assert(string.CompareOrdinal(data[0], "CTR") == 0);

			return
				new Contraction
				{
					StartTime = baseTime.AddSeconds(Convert.ToUInt32(data[1], CultureInfo.InvariantCulture)),
					PeakTime = baseTime.AddSeconds(Convert.ToUInt32(data[2], CultureInfo.InvariantCulture)),
					EndTime = baseTime.AddSeconds(Convert.ToUInt32(data[3], CultureInfo.InvariantCulture))
				};
		}

		static Baseline BaselineFromEngine(DateTime baseTime, string[] data)
		{
			Debug.Assert(data.GetLength(0) == 7);
			Debug.Assert(string.CompareOrdinal(data[0], "EVT") == 0);
			Debug.Assert(Convert.ToInt32(data[1], CultureInfo.InvariantCulture) == 9); // event::tbaseline

			return
				new Baseline
				{
					StartTime = baseTime.AddSeconds(Convert.ToUInt32(data[2], CultureInfo.InvariantCulture) / 4f),
					EndTime = baseTime.AddSeconds(Convert.ToUInt32(data[3], CultureInfo.InvariantCulture) / 4f),
					Y1 = data[4].ToSafeDouble(),
					Y2 = data[5].ToSafeDouble(),
					BaselineVariability = data[6].ToSafeDouble(),
				};
		}

		static Acceleration AccelerationFromEngine(DateTime baseTime, string[] data)
		{
			Debug.Assert(data.GetLength(0) == 10);
			Debug.Assert(string.CompareOrdinal(data[0], "EVT") == 0);
			Debug.Assert(Convert.ToInt32(data[1], CultureInfo.InvariantCulture) == 1); // event::tacceleration

			return
				new Acceleration
				{
					StartTime = baseTime.AddSeconds(Convert.ToUInt32(data[2], CultureInfo.InvariantCulture) / 4f),
					EndTime = baseTime.AddSeconds(Convert.ToUInt32(data[3], CultureInfo.InvariantCulture) / 4f),

					PeakTime = baseTime.AddSeconds(Convert.ToUInt32(data[4], CultureInfo.InvariantCulture) / 4f),
					PeakValue = data[5].ToSafeDouble(),

					Confidence = data[6].ToSafeDouble(),
					Repair = data[7].ToSafeDouble(),
					Height = data[8].ToSafeDouble(),

					IsNonInterpretable = (string.CompareOrdinal(data[9], "y") == 0)
				};
		}

		static Deceleration DecelerationFromEngine(DateTime baseTime, string[] data)
		{
			Debug.Assert(data.GetLength(0) == 12);
			Debug.Assert(string.CompareOrdinal(data[0], "EVT") == 0);

			int type = Convert.ToInt32(data[1], CultureInfo.InvariantCulture);

			Deceleration decel =
				new Deceleration
				{
					StartTime = baseTime.AddSeconds(Convert.ToUInt32(data[2], CultureInfo.InvariantCulture) / 4f),
					EndTime = baseTime.AddSeconds(Convert.ToUInt32(data[3], CultureInfo.InvariantCulture) / 4f),

					PeakTime = baseTime.AddSeconds(Convert.ToUInt32(data[4], CultureInfo.InvariantCulture) / 4f),
					PeakValue = data[5].ToSafeDouble(),

					Confidence = data[6].ToSafeDouble(),
					Repair = data[7].ToSafeDouble(),
					Height = data[8].ToSafeDouble(),

					IsNonInterpretable = (string.CompareOrdinal(data[9], "y") == 0)
				};

			switch (type)
			{
				// event::tearly ***
				case 3:
					decel.IsEarlyDeceleration = true;
					break;

				// event::ttypical ***
				case 4:
					decel.IsVariableDeceleration = true;
					break;

				// event::tatypical ***
				case 5:
					decel.IsVariableDeceleration = true;
					break;

				// event::tlate ***
				case 6:
					decel.IsLateDeceleration = true;
					break;

				// event::tnadeceleration ***
				case 7:
					decel.IsNonAssociatedDeceleration = true;
					break;

				// event::tprolonged***
				case 14:
					decel.IsVariableDeceleration = true;
					decel.HasProlongedNonReassuringFeature = true;
					break;

				// others...
				default:
					throw new ArgumentOutOfRangeException("data", "Unrecognized EVT detection artifact subtype");
			}

			if (data[10].Length > 0)
			{
				int offset = Convert.ToInt32(data[10], CultureInfo.InvariantCulture);
				if (offset >= 0)
				{
					decel.ContractionStart = baseTime.AddSeconds(offset / 4d);
				}
			}

			int atypical = Convert.ToInt32(data[11], CultureInfo.InvariantCulture);
			if ((atypical & 1) == 1)
			{
				decel.HasBiphasicNonReassuringFeature = true;
			}
			if ((atypical & 2) == 2)
			{
				decel.HasLossRiseNonReassuringFeature = true;
			}
			if ((atypical & 4) == 4)
			{
				decel.HasLossVariabilityNonReassuringFeature = true;
			}
			if ((atypical & 8) == 8)
			{
				decel.HasLowerBaselineNonReassuringFeature = true;
			}
			if ((atypical & 16) == 16)
			{
				decel.HasProlongedSecondRiseNonReassuringFeature = true;
			}
			if ((atypical & 32) == 32)
			{
				decel.HasSixtiesNonReassuringFeature = true;
			}
			if ((atypical & 64) == 64)
			{
				decel.HasSlowReturnNonReassuringFeature = true;
			}

			return decel;
		}

		public static DetectionArtifact ToDetectionArtifact(this string line, DateTime baseTime)
		{
			var data = line.Split("|".ToCharArray(), StringSplitOptions.None);
			if (data.GetLength(0) < 0)
				return null;

			if (string.CompareOrdinal(data[0], "EVT") == 0)
			{
				int type = Convert.ToInt32(data[1], CultureInfo.InvariantCulture);
				switch (type)
				{
					// event::tbaseline
					case 9:
						return EngineConversionHelper.BaselineFromEngine(baseTime, data);

					//event::tacceleration
					case 1:
						return EngineConversionHelper.AccelerationFromEngine(baseTime, data);

					// event::tearly *** (all types of decels)
					default:
						return EngineConversionHelper.DecelerationFromEngine(baseTime, data);
				}
			}
			else if (string.CompareOrdinal(data[0], "CTR") == 0)
			{
				return EngineConversionHelper.ContractionFromEngine(baseTime, data);
			}

			throw new InvalidOperationException("Unrecognized detection artifact type");
		}
	}
}

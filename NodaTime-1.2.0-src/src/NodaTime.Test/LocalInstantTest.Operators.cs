// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NUnit.Framework;

namespace NodaTime.Test
{
    partial class LocalInstantTest
    {
        #region IEquatable.Equals
        [Test]
        public void IEquatableEquals_ToSelf_IsTrue()
        {
            Assert.True(LocalInstant.LocalUnixEpoch.Equals(LocalInstant.LocalUnixEpoch), "epoch == epoch (same object)");
        }

        [Test]
        public void IEquatableEquals_WithEqualTicks_IsTrue()
        {
            var first = new LocalInstant(100L);
            var second = new LocalInstant(100L);
            Assert.True(first.Equals(second), "100 == 100 (different objects)");
        }

        [Test]
        public void IEquatableEquals_WithDifferentTicks_IsFalse()
        {
            Assert.False(one.Equals(negativeOne), "1 == -1");
            Assert.False(one.Equals(threeMillion), "1 == 3,000,000");
            Assert.False(one.Equals(negativeFiftyMillion), "1 == -50,000,000");
            Assert.False(LocalInstant.MinValue.Equals(LocalInstant.MaxValue), "MinValue == MaxValue");
        }
        #endregion

        #region object.Equals
        [Test]
        public void ObjectEquals_ToNull_IsFalse()
        {
            object oOne = one;

            Assert.False(oOne.Equals(null), "1 == null");
        }

        [Test]
        public void ObjectEquals_ToSelf_IsTrue()
        {
            object oOne = one;

            Assert.True(oOne.Equals(oOne), "1 == 1 (same object)");
        }

        [Test]
        public void ObjectEquals_WithEqualTicks_IsTrue()
        {
            object oOne = one;
            object oOnePrime = onePrime;

            Assert.True(oOne.Equals(oOnePrime), "1 == 1 (different objects)");
        }

        [Test]
        public void ObjectEquals_WithDifferentTicks_IsFalse()
        {
            object oOne = one;
            object oNegativeOne = negativeOne;
            object oThreeMillion = threeMillion;
            object oNegativeFiftyMillion = negativeFiftyMillion;
            object oMinimum = LocalInstant.MinValue;
            object oMaximum = LocalInstant.MaxValue;

            Assert.False(oOne.Equals(oNegativeOne), "1 == -1");
            Assert.False(oOne.Equals(oThreeMillion), "1 == 3,000,000");
            Assert.False(oOne.Equals(oNegativeFiftyMillion), "1 == -50,000,000");
            Assert.False(oMinimum.Equals(oMaximum), "MinValue == MaxValue");
        }
        #endregion

        #region object.GetHashCode
        [Test]
        public void GetHashCode_Twice_IsEqual()
        {
            var test1 = new LocalInstant(123L);
            var test2 = new LocalInstant(123L);
            Assert.AreEqual(test1.GetHashCode(), test1.GetHashCode());
            Assert.AreEqual(test2.GetHashCode(), test2.GetHashCode());
        }

        [Test]
        public void GetHashCode_SameTicks_IsEqual()
        {
            var test1 = new LocalInstant(123L);
            var test2 = new LocalInstant(123L);
            Assert.AreEqual(test1.GetHashCode(), test2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentTicks_IsDifferent()
        {
            var test1 = new LocalInstant(123L);
            var test2 = new LocalInstant(123L);
            var test3 = new LocalInstant(321L);

            Assert.AreNotEqual(test1.GetHashCode(), test3.GetHashCode());
            Assert.AreNotEqual(test2.GetHashCode(), test3.GetHashCode());
        }
        #endregion

        #region CompareTo
        [Test]
        public void CompareTo_Self_IsEqual()
        {
            Assert.AreEqual(0, one.CompareTo(one), "1 == 1 (same object)");
        }

        [Test]
        public void CompareTo_WithEqualTicks_IsEqual()
        {
            Assert.AreEqual(0, one.CompareTo(onePrime), "1 == 1 (different objects)");
        }

        [Test]
        public void CompareTo_WithMoreTicks_IsGreater()
        {
            Assert.Greater(one.CompareTo(negativeFiftyMillion), 0, "1 > -50,000,000");
            Assert.Greater(threeMillion.CompareTo(one), 0, "3,000,000 > 1");
            Assert.Greater(negativeOne.CompareTo(negativeFiftyMillion), 0, "-1 > -50,000,000");
            Assert.Greater(LocalInstant.MaxValue.CompareTo(LocalInstant.MinValue), 0, "MaxValue > MinValue");
        }

        [Test]
        public void CompareTo_WithLessTicks_IsLess()
        {
            Assert.Less(negativeFiftyMillion.CompareTo(one), 0, "-50,000,000 < 1");
            Assert.Less(one.CompareTo(threeMillion), 0, "1 < 3,000,000");
            Assert.Less(negativeFiftyMillion.CompareTo(negativeOne), 0, "-50,000,000 > -1");
            Assert.Less(LocalInstant.MinValue.CompareTo(LocalInstant.MaxValue), 0, "MinValue < MaxValue");
        }
        #endregion

        [Test]
        public void IEquatableIComparable_Tests()
        {
            var value = new LocalInstant(12345);
            var equalValue = new LocalInstant(12345);
            var greaterValue = new LocalInstant(5432199);

            TestHelper.TestEqualsStruct(value, equalValue, greaterValue);
            TestHelper.TestCompareToStruct(value, equalValue, greaterValue);
            TestHelper.TestNonGenericCompareTo(value, equalValue, greaterValue);
            TestHelper.TestOperatorComparisonEquality(value, equalValue, greaterValue);
        }

        #region operator ==
        [Test]
        public void OperatorEquals_ToSelf_IsTrue()
        {
            // Warning CS1718: Comparison made to same variable; did you mean to compare something else?
            // This is intentional for testing
#pragma warning disable 1718
            Assert.True(one == one, "1 == 1 (same object)");
#pragma warning restore 1718
        }

        [Test]
        public void OperatorEquals_WithEqualTicks_IsTrue()
        {
            Assert.True(one == onePrime, "1 == 1 (different objects)");
        }

        [Test]
        public void OperatorEquals_WithDifferentTicks_IsFalse()
        {
            Assert.False(one == negativeOne, "1 == -1");
            Assert.False(one == threeMillion, "1 == 3,000,000");
            Assert.False(one == negativeFiftyMillion, "1 == -50,000,000");
            Assert.False(LocalInstant.MinValue == LocalInstant.MaxValue, "MinValue == MaxValue");
        }
        #endregion

        #region operator !=
        [Test]
        public void OperatorNotEquals_ToSelf_IsFalse()
        {
            // Warning CS1718: Comparison made to same variable; did you mean to compare something else?
            // This is intentional for testing
#pragma warning disable 1718
            Assert.False(one != one, "1 != 1 (same object)");
#pragma warning restore 1718
        }

        [Test]
        public void OperatorNotEquals_WithEqualTicks_IsFalse()
        {
            Assert.False(one != onePrime, "1 != 1 (different objects)");
        }

        [Test]
        public void OperatorNotEquals_WithDifferentTicks_IsTrue()
        {
            Assert.True(one != negativeOne, "1 != -1");
            Assert.True(one != threeMillion, "1 != 3,000,000");
            Assert.True(one != negativeFiftyMillion, "1 != -50,000,000");
            Assert.True(LocalInstant.MinValue != LocalInstant.MaxValue, "MinValue != MaxValue");
        }
        #endregion

        #region operator <
        [Test]
        public void OperatorLessThan_Self_IsFalse()
        {
            // Warning CS1718: Comparison made to same variable; did you mean to compare something else?
            // This is intentional for testing
#pragma warning disable 1718
            Assert.False(one < one, "1 < 1 (same object)");
#pragma warning restore 1718
        }

        [Test]
        public void OperatorLessThan_EqualTicks_IsFalse()
        {
            Assert.False(one < onePrime, "1 < 1 (different objects)");
        }

        [Test]
        public void OperatorLessThan_MoreTicks_IsTrue()
        {
            Assert.True(one < threeMillion, "1 < 3,000,000");
            Assert.True(negativeFiftyMillion < negativeOne, "-50,000,000 < -1");
            Assert.True(LocalInstant.MinValue < LocalInstant.MaxValue, "MinValue < MaxValue");
        }

        [Test]
        public void OperatorLessThan_LessTicks_IsFalse()
        {
            Assert.False(threeMillion < one, "3,000,000 < 1");
            Assert.False(negativeOne < negativeFiftyMillion, "-1 < -50,000,000");
            Assert.False(LocalInstant.MaxValue < LocalInstant.MinValue, "MaxValue < MinValue");
        }
        #endregion

        #region operator <=
        [Test]
        public void OperatorLessThanOrEqual_Self_IsTrue()
        {
            // Warning CS1718: Comparison made to same variable; did you mean to compare something else?
            // This is intentional for testing
#pragma warning disable 1718
            Assert.True(one <= one, "1 <= 1 (same object)");
#pragma warning restore 1718
        }

        [Test]
        public void OperatorLessThanOrEqual_EqualTicks_IsTrue()
        {
            Assert.True(one <= onePrime, "1 <= 1 (different objects)");
        }

        [Test]
        public void OperatorLessThanOrEqual_MoreTicks_IsTrue()
        {
            Assert.True(one <= threeMillion, "1 <= 3,000,000");
            Assert.True(negativeFiftyMillion <= negativeOne, "-50,000,000 <= -1");
            Assert.True(LocalInstant.MinValue <= LocalInstant.MaxValue, "MinValue <= MaxValue");
        }

        [Test]
        public void OperatorLessThanOrEqual_LessTicks_IsFalse()
        {
            Assert.False(threeMillion <= one, "3,000,000 <= 1");
            Assert.False(negativeOne <= negativeFiftyMillion, "-1 <= -50,000,000");
            Assert.False(LocalInstant.MaxValue <= LocalInstant.MinValue, "MaxValue <= MinValue");
        }
        #endregion

        #region operator >
        [Test]
        public void OperatorGreaterThan_Self_IsFalse()
        {
            // Warning CS1718: Comparison made to same variable; did you mean to compare something else?
            // This is intentional for testing
#pragma warning disable 1718
            Assert.False(one > one, "1 > 1 (same object)");
#pragma warning restore 1718
        }

        [Test]
        public void OperatorGreaterThan_EqualTicks_IsFalse()
        {
            Assert.False(one > onePrime, "1 > 1 (different objects)");
        }

        [Test]
        public void OperatorGreaterThan_MoreTicks_IsFalse()
        {
            Assert.False(one > threeMillion, "1 > 3,000,000");
            Assert.False(negativeFiftyMillion > negativeOne, "-50,000,000 > -1");
            Assert.False(LocalInstant.MinValue > LocalInstant.MaxValue, "MinValue > MaxValue");
        }

        [Test]
        public void OperatorGreaterThan_LessTicks_IsTrue()
        {
            Assert.True(threeMillion > one, "3,000,000 > 1");
            Assert.True(negativeOne > negativeFiftyMillion, "-1 > -50,000,000");
            Assert.True(LocalInstant.MaxValue > LocalInstant.MinValue, "MaxValue > MinValue");
        }
        #endregion

        #region operator >=
        [Test]
        public void OperatorGreaterThanOrEqual_Self_IsTrue()
        {
            // Warning CS1718: Comparison made to same variable; did you mean to compare something else?
            // This is intentional for testing
#pragma warning disable 1718
            Assert.True(one >= one, "1 >= 1 (same object)");
#pragma warning restore 1718
        }

        [Test]
        public void OperatorGreaterThanOrEqual_EqualTicks_IsTrue()
        {
            Assert.True(one >= onePrime, "1 >= 1 (different objects)");
        }

        [Test]
        public void OperatorGreaterThanOrEqual_MoreTicks_IsFalse()
        {
            Assert.False(one >= threeMillion, "1 >= 3,000,000");
            Assert.False(negativeFiftyMillion >= negativeOne, "-50,000,000 >= -1");
            Assert.False(LocalInstant.MinValue >= LocalInstant.MaxValue, "MinValue >= MaxValue");
        }

        [Test]
        public void OperatorGreaterThanOrEqual_LessTicks_IsTrue()
        {
            Assert.True(threeMillion >= one, "3,000,000 >= 1");
            Assert.True(negativeOne >= negativeFiftyMillion, "-1 >= -50,000,000");
            Assert.True(LocalInstant.MaxValue >= LocalInstant.MinValue, "MaxValue >= MinValue");
        }
        #endregion

        #region operator +
        [Test]
        public void OperatorPlus_DurationZero_IsNeutralElement()
        {
            Assert.AreEqual(LocalInstant.LocalUnixEpoch, LocalInstant.LocalUnixEpoch + Duration.Zero, "LocalUnixEpoch + Duration.Zero");
            Assert.AreEqual(one, one + Duration.Zero, "LocalInstant(1) + Duration.Zero");
            Assert.AreEqual(one, LocalInstant.LocalUnixEpoch + Duration.Epsilon, "LocalUnixEpoch + Duration.Epsilon");
        }

        [Test]
        public void OperatorPlus_DurationNonZero()
        {
            Assert.AreEqual(3000001L, (threeMillion + Duration.Epsilon).Ticks, "3,000,000 + 1");
            Assert.AreEqual(0L, (one + durationNegativeEpsilon).Ticks, "1 + (-1)");
            Assert.AreEqual(-49999999L, (negativeFiftyMillion + Duration.Epsilon).Ticks, "-50,000,000 + 1");
        }
        #endregion

        #region operator -
        [Test]
        public void OperatorMinusDuratino_Zero_IsNeutralElement()
        {
            Assert.AreEqual(0L, (LocalInstant.LocalUnixEpoch - LocalInstant.LocalUnixEpoch).Ticks, "0 - 0");
            Assert.AreEqual(1L, (one - LocalInstant.LocalUnixEpoch).Ticks, "1 - 0");
            Assert.AreEqual(-1L, (LocalInstant.LocalUnixEpoch - one).Ticks, "0 - 1");
        }

        [Test]
        public void OperatorMinusDuration_NonZero()
        {
            Assert.AreEqual(2999999L, (threeMillion - one).Ticks, "3,000,000 - 1");
            Assert.AreEqual(2L, (one - negativeOne).Ticks, "1 - (-1)");
            Assert.AreEqual(-50000001L, (negativeFiftyMillion - one).Ticks, "-50,000,000 - 1");
        }

        [Test]
        public void OperatorMinusOffset_Zero_IsNeutralElement()
        {
            Assert.AreEqual(NodaConstants.UnixEpoch, LocalInstant.LocalUnixEpoch.Minus(Offset.Zero), "LocalUnixEpoch - Offset.Zero");
            Assert.AreEqual(new Instant(1L), one.Minus(Offset.Zero), "LocalInstant(1) - Offset.Zero");
            Assert.AreEqual(new Instant(-NodaConstants.TicksPerHour), LocalInstant.LocalUnixEpoch.Minus(offsetOneHour), "LocalUnixEpoch - offsetOneHour");
        }
        #endregion
    }
}

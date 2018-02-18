// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Diagnostics.CodeAnalysis;

namespace NodaTime.Utility
{
    /// <summary>
    /// Provides method to help with generating hash codes for structures and classes. This handles
    /// value types, nullable type, and objects.
    /// </summary>
    /// <remarks>
    /// The basic usage pattern is:
    /// <example>
    /// <code>
    ///    public override int GetHashCode()
    ///    {
    ///        int hash = HashCodeHelper.Initialize();
    ///        hash = HashCodeHelper.Hash(hash, Field1);
    ///        hash = HashCodeHelper.Hash(hash, Field1);
    ///        hash = HashCodeHelper.Hash(hash, Field1);
    ///        ...
    ///        return hash;
    ///    }
    /// </code>
    /// </example>
    /// </remarks>
    internal static class HashCodeHelper
    {
        /// <summary>
        /// The multiplier for each value.
        /// </summary>
        private const int HashcodeMultiplier = 37;

        /// <summary>
        /// The initial hash value.
        /// </summary>
        private const int HashcodeInitializer = 17;

        /// <summary>
        /// Returns the initial value for a hash code.
        /// </summary>
        /// <returns>The initial interger value.</returns>
        internal static int Initialize()
        {
            return HashcodeInitializer;
        }

        /// <summary>
        /// Adds the hash value for the given value to the current hash and returns the new value.
        /// </summary>
        /// <typeparam name="T">The type of the value being hashed.</typeparam>
        /// <param name="code">The previous hash code.</param>
        /// <param name="value">The value to hash.</param>
        /// <returns>The new hash code.</returns>
        internal static int Hash<T>(int code, T value)
        {
            int hash = 0;
            if (value != null)
            {
                hash = value.GetHashCode();
            }
            return MakeHash(code, hash);
        }

        /// <summary>
        /// Adds the hash value for a int to the current hash value and returns the new value.
        /// </summary>
        /// <param name="code">The previous hash code.</param>
        /// <param name="value">The value to add to the hash code.</param>
        /// <returns>The new hash code.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", Justification = "Deliberately overflowing.")]
        private static int MakeHash(int code, int value)
        {
            unchecked
            {
                code = (code * HashcodeMultiplier) + value;
            }
            return code;
        }
    }
}
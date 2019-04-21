using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// This class allows comparing two nullable objects with each other.
    /// This usually requires the elements to implement IEquatable&lt;T&gt;, Equals and GetHashCode.
    /// Note that this is basically the IEqualityComparer pattern, but the class is static since it is used a lot.
    /// </summary>
    public static class ValueComparer<T>
    {
        /// <summary>
        /// Checks whether or not x and y are equal.
        /// X and y are considered equal if they are either both null, or both not null and their Equals implementation returns true.
        /// </summary>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        /// <returns>True if both objects are equal.</returns>
        public static bool Equals(T x, T y)
        {
            return
                x == null && y == null ||
                x != null && x.Equals(y);
        }
                
        /// <summary>
        /// Retrieves the hash code for the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The hash code.</returns>
        public static int GetHashCode(T obj)
        {
            return obj == null ? -1 : obj.GetHashCode();
        }
    }
}

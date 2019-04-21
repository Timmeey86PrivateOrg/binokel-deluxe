using System;
using System.Collections.Generic;
using System.Text;

namespace BinokelDeluxe.Common
{
    /// <summary>
    /// This class allows comparing two lists with each other.
    /// The lists are considered equal if they have the same order of elements, and the elements at each position are equal.
    /// This usually requires the elements to implement IEquatable&lt;T&gt;, Equals and GetHashCode.
    /// Note that this is basically the IEqualityComparer pattern, but the class is static since it is used a lot.
    /// </summary>
    public static class ListComparer<T>
    {
        /// <summary>
        /// Checks whether or not x is equal to y. 
        /// The lists are considered equal if they have the same order of elements, and the elements at each position are equal.
        /// </summary>
        /// <param name="x">the first list.</param>
        /// <param name="y">the second list.</param>
        /// <returns>True if the lists contain the same order of elements, and the elements at each position are equal.</returns>
        public static bool Equals(IList<T> x, IList<T> y)
        {
            return
                x == null && y == null ||
                x != null && y != null && System.Linq.Enumerable.SequenceEqual(x, y);
        }

        /// <summary>
        /// Retrieves the hash code for the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The hash code.</returns>
        public static int GetHashCode(IList<T> obj)
        {
            unchecked // overflow is fine, just wrap
            {
                int hash = 17;
                foreach (var value in obj)
                {
                    hash = hash * 29 + value.GetHashCode();
                }
                return hash;
            }
        }
    }
}

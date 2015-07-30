using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corvallis_Transit.Util
{
    /// <summary>
    /// Container for some convenient extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Null-safe check for if a collection has stuff.
        /// </summary>
        public static bool HasContent<T>(this IEnumerable<T> target) where T : class
        {
            return !target.IsNullOrEmpty();
        }

        /// <summary>
        /// Semantic wrapper for checking null and emptiness.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> target) where T : class
        {
            return target == null && !target.Any();
        }

        /// <summary>
        /// Semantic wrapper for string.IsNullOrWhiteSpace().
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string target)
        {
            return string.IsNullOrWhiteSpace(target);
        }
    }
}

#if NET45PLUS

namespace System
{
    using System.Collections;

    internal static class ArrayExtensions
    {
        public static T[] CastAsArray<T>(this IStructuralComparable comparable)
        {
            return (T[])comparable;
        }

        public static T[] CastAsArray<T>(this IStructuralEquatable equatable)
        {
            return (T[])equatable;
        }

        public static IStructuralComparable AsStructuralComparable<T>(this T[] array)
        {
            return array;
        }

        public static IStructuralEquatable AsStructuralEquatable<T>(this T[] array)
        {
            return array;
        }
    }
}

#endif

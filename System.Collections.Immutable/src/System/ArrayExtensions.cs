#if !NET45PLUS

namespace System
{
    using System.Collections;

    internal static class ArrayExtensions
    {
        public static Array AsArray(this IStructuralComparable comparable)
        {
#if NET40PLUS
            return comparable as Array;
#else
            ArrayWrapper wrapper = comparable as ArrayWrapper;
            if (wrapper == null)
                return null;

            return wrapper.Array;
#endif
        }

        public static Array AsArray(this IStructuralEquatable equatable)
        {
#if NET40PLUS
            return equatable as Array;
#else
            ArrayWrapper wrapper = equatable as ArrayWrapper;
            if (wrapper == null)
                return null;

            return wrapper.Array;
#endif
        }

        public static T[] CastAsArray<T>(this IStructuralComparable comparable)
        {
#if NET40PLUS
            return (T[])comparable;
#else
            return (T[])((ArrayWrapper)comparable).Array;
#endif
        }

        public static T[] CastAsArray<T>(this IStructuralEquatable equatable)
        {
#if NET40PLUS
            return (T[])equatable;
#else
            return (T[])((ArrayWrapper)equatable).Array;
#endif
        }

        public static IStructuralComparable AsStructuralComparable<T>(this T[] array)
        {
#if NET40PLUS
            return array;
#else
            if (array == null)
                return null;

            return new ArrayWrapper(array);
#endif
        }

        public static IStructuralEquatable AsStructuralEquatable<T>(this T[] array)
        {
#if NET40PLUS
            return array;
#else
            if (array == null)
                return null;

            return new ArrayWrapper(array);
#endif
        }

#if !NET40PLUS
        private sealed class ArrayWrapper : IStructuralComparable, IStructuralEquatable
        {
            private readonly Array _array;

            public ArrayWrapper(Array array)
            {
                _array = array;
            }

            public Array Array
            {
                get
                {
                    return _array;
                }
            }

            public int CompareTo(object other, IComparer comparer)
            {
                if (other == null) {
                    return 1;
                }

                Array o = other as Array;
                if (o == null)
                {
                    ArrayWrapper otherWrapper = other as ArrayWrapper;
                    if (otherWrapper != null)
                        o = otherWrapper.Array;
                }

                if (o == null || _array.Length != o.Length) {
                    throw new ArgumentException("Other Not Array Of Correct Length", "other");
                }

                int i = 0;
                int c = 0;

                while (i < o.Length && c == 0) {
                    object left = _array.GetValue(i);
                    object right = o.GetValue(i);

                    c = comparer.Compare(left, right);
                    i++;
                }
            
                return c;
            }

            public bool Equals(object other, IEqualityComparer comparer)
            {
                if (other == null) {
                    return false;
                }

                Array o = other as Array;
                if (o == null)
                {
                    ArrayWrapper otherWrapper = other as ArrayWrapper;
                    if (otherWrapper != null)
                        o = otherWrapper.Array;
                }

                if (Object.ReferenceEquals(_array, other)) {
                    return true;
                }

                if (o == null || o.Length != _array.Length) {
                    return false;
                }

                int i = 0;
                while (i < o.Length) {
                    object left = _array.GetValue(i);
                    object right = o.GetValue(i);

                    if (!comparer.Equals(left, right)) {
                        return false;
                    }
                    i++;
                }

                return true;
            }

            public int GetHashCode(IEqualityComparer comparer)
            {
                if (comparer == null)
                    throw new ArgumentNullException("comparer");

                int ret = 0;

                for (int i = (_array.Length >= 8 ? _array.Length - 8 : 0); i < _array.Length; i++) {
                    ret = CombineHashCodes(ret, comparer.GetHashCode(_array.GetValue(0)));
                }

                return ret;
            }

            // From System.Web.Util.HashCodeCombiner
            private static int CombineHashCodes(int h1, int h2) {
                return (((h1 << 5) + h1) ^ h2);
            }
        }
#endif
    }
}

#endif

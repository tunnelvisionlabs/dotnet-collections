#if !NET45PLUS

namespace System
{
    using System.Collections.Generic;

    internal static class ThrowHelper
    {
        internal static void ThrowArgumentNullException(ExceptionArgument argument)
        {
            throw new ArgumentNullException(argument.ToString());
        }

        internal static void ThrowArgumentException(string message)
        {
            throw new ArgumentException(message);
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
        {
            throw new ArgumentOutOfRangeException(argument.ToString());
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, string message)
        {
            throw new ArgumentOutOfRangeException(argument.ToString(), message);
        }

        internal static void ThrowInvalidOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }

        internal static void ThrowKeyNotFoundException()
        {
            throw new KeyNotFoundException();
        }

        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument argument)
        {
            if (default(T) != null)
                throw new ArgumentNullException(argument.ToString());
        }

        internal static void ThrowWrongValueTypeArgumentException(object value, Type type)
        {
            throw new ArgumentException();
        }

        internal static void ThrowWrongKeyTypeArgumentException(object key, Type type)
        {
            throw new ArgumentException();
        }

        internal static void ThrowNotSupportedException(string message)
        {
            throw new NotSupportedException(message);
        }
    }

    internal enum ExceptionArgument
    {
        array,
        arrayIndex,
        index,
        collection,
        dictionary,
        key,
        value
    }
}

#endif

#if !NET45PLUS

namespace System
{
    internal static class ExceptionResource
    {
        internal static readonly string ArgumentOutOfRange_NeedNonNegNum = "Need non-negative number";
        internal static readonly string Argument_AddingDuplicate = "Adding duplicate";
        internal static readonly string Argument_InvalidArrayType = "Invalid array type";
        internal static readonly string Arg_ArrayPlusOffTooSmall = "Array offset too small";
        internal static readonly string Arg_NonZeroLowerBound = "Non-zero lower bound";
        internal static readonly string Arg_RankMultiDimNotSupported = "Rank is not supported for multi-dimensional arrays";
        internal static readonly string InvalidOperation_EnumFailedVersion = "Enum failed version";
        internal static readonly string InvalidOperation_EnumOpCantHappen = "Enum op can't happen";
        internal static readonly string NotSupported_KeyCollectionSet = "Key collection set";
        internal static readonly string NotSupported_ValueCollectionSet = "Value collection set";

        public static string GetString(string value)
        {
            return value;
        }
    }
}

#endif

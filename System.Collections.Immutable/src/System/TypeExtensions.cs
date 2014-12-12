#if !NET45PLUS

namespace System
{
    internal static class TypeExtensions
    {
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
    }
}

#endif

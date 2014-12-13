#if NET40PLUS

using System.Collections;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(IStructuralComparable))]

#else

using System;

namespace System.Collections {

    public interface IStructuralComparable {
        Int32 CompareTo(Object other, IComparer comparer);
    }
}

#endif

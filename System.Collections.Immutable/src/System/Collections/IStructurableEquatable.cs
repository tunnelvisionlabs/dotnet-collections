#if NET40PLUS

using System.Collections;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(IStructuralEquatable))]

#else

namespace System.Collections {

    public interface IStructuralEquatable {
        Boolean Equals(Object other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }
}

#endif

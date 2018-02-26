# TunnelVisionLabs.Collections.Immutable

[![Build status](https://ci.appveyor.com/api/projects/status/o6sy4k8aaif2tel8/branch/master?svg=true)][1]

This project is a complete port of the [System.Collections.Immutable][2] assembly to support .NET 2.0 and newer.

[1]: https://ci.appveyor.com/project/sharwell/dotnet-collections/branch/master
[2]: https://www.nuget.org/packages/System.Collections.Immutable

## Releases

* [Binaries (NuGet)](https://www.nuget.org/packages/TunnelVisionLabs.Collections.Immutable)
* [Release Notes](https://github.com/tunnelvisionlabs/dotnet-collections/releases)
* [License](https://github.com/tunnelvisionlabs/dotnet-collections/blob/master/LICENSE)

Release version 1.1.36 and earlier can be found on NuGet in the [Rackspace.Collections.Immutable][3] library.

[3]: https://www.nuget.org/packages/Rackspace.Collections.Immutable

## Supported APIs

The following table shows the public APIs supported by this library. The type is defined in builds of this library where
the API is not part of the .NET Framework. In other cases, a `TypeForwardedTo` attribute is included which points to the
reference definition of the type.

* Immutable collections
  * `System.Collections.Immutable`
* Other collection interfaces
  * `System.Collections.IStructuralComparable`
  * `System.Collections.IStructuralEquatable`
  * `System.Collections.Generic.IReadOnlyCollection<T>`
  * `System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>`
  * `System.Collections.Generic.IReadOnlyList<T>`
  * `System.Collections.Generic.ISet<T>`¹
  * `System.Collections.Generic.SortedSet<T>`
* Threading
  * `System.Threading.Volatile`

¹ Even though a definition for `ISet<T>` is included in the .NET 3.5 build, the `HashSet<T>` class which is included in
the .NET Framework 3.5 does not implement this interface.

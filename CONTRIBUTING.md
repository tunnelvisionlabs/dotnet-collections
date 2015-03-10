### Steps to merge changes from dotnet/corefx

1. Clone the latest [dotnet/corefx](https://github.com/dotnet/corefx/) repository into **C:\BasePath\corefx**.

1. Clone the latest [rackerlabs/dotnet-collections](https://github.com/rackerlabs/dotnet-collections) repository into
   **C:\BasePath\dotnet-collections**.

1. From the **C:\BasePath\corefx** directory, execute the following command:

    ```
    git subtree split -P src/System.Collections.Immutable -b collections
    ```

1. From the **C:\BasePath\dotnet-collections** directory, execute the following command:

    ```
    git subtree pull -P System.Collections.Immutable --squash ../corefx collections
    ```

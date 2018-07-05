# .NET Core Publishing

*(command line examples are using Powershell)*

.NET Core projects doesn't generate exe files, only DLLs. Those can be run using dotnet.exe:

`& "C:\Program Files\dotnet\dotnet.exe" run`    
*(in the folder where `.csproj` is)*.

However, this requires .NET Core CLI installed on computer, so this is not for common user.

Another option is *.NET Core self-contained deployment*. By running 

`& "C:\Program Files\dotnet\dotnet.exe" publish -c release -r osx-x64`  
*in the folder where `.csproj` is* (more [here](http://ttu.github.io/dotnet-core-self-contained-deployments/))

This packs all the .NET Core files into single *publish* folder. To reduce packed .NET Core files to only necessary ones Trimming package can be used:

`& "C:\Program Files\dotnet\dotnet.exe" publish -c release -r osx-x64 /p:TrimUnusedDependencies=true`  
(read [here](https://ianqvist.blogspot.com/2018/01/reducing-size-of-self-contained-net.html)).

[RID Catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)


// TODO create publishing script for all platforms, with zipping and all.
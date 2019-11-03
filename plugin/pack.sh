dotnet build
dotnet logjoint.plugintool pack bin/Debug/netstandard2.0/manifest.xml bin/symphony.zip prod
dotnet logjoint.plugintool test bin/symphony.zip ~/Applications/logjoint.app/Contents/MonoBundle

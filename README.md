# Symphony plugin for LogJoint

Enables [LogJoint](https://github.com/sergey-su/logjoint) to open and visualize [Symphony](https://symphony.com/) logs.

## Development prerequisites
- [.NET Core SDK](https://dotnet.microsoft.com/download). Be sure to restart the Terminal after installation.
- [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/) (only for debugging on Mac, make sure to check `macOS (Cocoa)` component in the installer)

## Build
In repo's root dir:
```
dotnet build plugin
```

## Try locally (mac)

Have normal installation of LogJoint from the download link at https://github.com/sergey-su/logjoint. The installation may and may not have Symphony plugin added, it does not matter. However it must have all required plugins added - Chromium plugin.

In repo's root dir:
```
~/Applications/logjoint.app/Contents/MacOS/logjoint --plugin $PWD/plugin/bin/Debug/netstandard2.0
```


## Debug by logging (mac)

The 'Try locally' command above outputs log to the console.

You can output to the file also
```
~/Applications/logjoint.app/Contents/MacOS/logjoint --plugin $PWD/plugin/bin/Debug/netstandard2.0 --logging %HOME%/my-log.log
```

Specify full path or a path with `%HOME%` to refer user's `~` dir.

## Debug in debugger (mac)

The main app is built using Xamarin.Mac framework. There is no documented way to attach debugger to Xamarin.Mac process :(

The only way to debug is to build and run the main app from IDE:
1. Install [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/) (make sure to check `macOS (Cocoa)` component in the installer)
2. Checkout https://github.com/sergey-su/logjoint
3. Open trunk/platforms/osx/logjoint.mac.sln
4. Build the solution
5. Set logjoint.mac project as startup project (in right-click menu)
6. In logjoint.mac project's Properties, in section Run->Configurations->Default put Arguments `--plugin /path/to/logjoint-symphony-plugin/plugin/bin/Debug/netstandard2.0`
7. Run

## Publish

Publishing is the submission of the new binaries to the inbox. Once new binaries are verified and accepted, new update becomes available in the public updates channel for those who have your plugin installed.

In repo's root dir:
```
cd plugin
dotnet build
./pack.sh
dotnet logjoint.plugintool deploy bin/symphony.zip <secret inbox URL>
```

`<secret inbox URL>` has been allocated when plugin was registered. If it has to be shared, it must be done privately. It must never be committed to git.
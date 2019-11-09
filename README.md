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

## Integration testing

Plugin integration tests verify that plugin binaries work with the host application. Integration test cases are programmed into a netstandard2.0 library logjoint.symphony.plugin.integration.dll. That library is shipped together with other plugin binaries to allow logjoint backend run the tests whenever it needs to verify the plugin.

To run integration tests
```
dotnet build
dotnet logjoint.plugintool test bin/Debug/netstandard2.0 ~/Applications/logjoint.app/Contents/MonoBundle
```

That command
1. Finds tests library in your plugin. The plugin can be a packed plugin (ex, bin/symphony.zip) or a folder (ex, bin/Debug/netstandard2.0).
2. Finds host application binaries in specified installation location. That installation must have dependency plugins installed (chromium plugin).
3. For each test, creates a headless (no UI) instance of logjoint. Each instance is configured to work with temporary data directory so that tests won't interfere with your normal logjoint installation and its persistent state (logs history, caches, and so one).
4. Gets this instance to use your plugin under test.
5. Calls each test giving access to the headless logjoint's APIs via `context` argument.

Test structure:
```
[IntegrationTestFixture]
class MyFixture
{
    [IntegrationTest]
    public async Task MyTest(IContext context)
    {
        // use context.Model to access model-layer logjoint API
        // use context.Presentation to access presenation-layer logjoint API
        // use other context APIs
    }
}
```

Note that attributes `[IntegrationTestFixture]` and `[IntegrationTest]` are custom ones from `logjoint.testing.sdk`, not from any UT lib.

## Debug integration tests

When a test fails, it of course prints the failure details. If that's not enough, inspect log file that is written to test's temporary data directory which location is printed in case of failure.

To debug in debugger, use `Integration tests debug` task in vscode.

## Publish

Publishing is the submission of the new binaries to the inbox. Once new binaries are verified and accepted, new update becomes available in the public updates channel for those who have your plugin installed.

In repo's root dir:
```
cd plugin
./pack.sh # this builds, creates a zip, and runs integration tests
dotnet logjoint.plugintool deploy bin/symphony.zip <secret inbox URL>
```

`<secret inbox URL>` has been allocated when plugin was registered. If it has to be shared, it must be done privately. It must never be committed to git.
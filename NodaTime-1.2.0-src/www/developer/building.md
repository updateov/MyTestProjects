---
layout: developer
title: Building and testing
---

## Visual Studio (Windows)

Noda Time is mostly developed on Visual Studio 2010 and Visual Studio 2012.
All versions of Visual Studio 2010 and 2012 which support C#, including Express editions,
should be able to build Noda Time. If you're using Visual Studio Express, you *may* have
problems building the PCL version; it depends on other aspects of your computer. Visual
Studio Express itself doesn't have full support for portable class libraries, so while
it's important to us that developers with only Visual Studio Express can work on the
desktop build, we make no guarantees about the PCL build.
(Please contact the mailing list if you have problems not covered here.) 

You'll also need the [.NET Framework SDK][dotnetsdk]. This library supports
versions 3.5, 4 and 4.5 of the Framework. Install the appropriate
version or versions as defined by your needs.

[dotnetsdk]: http://msdn.microsoft.com/en-us/netframework/aa569263.aspx

To fetch the source code from the main Google Code repository, you'll need a
[Mercurial][] client. A good alternative for Microsoft Windows users is
[TortoiseHg][] which installs shell extensions so that Mercurial can be used
from the Windows Explorer.

[Mercurial]: http://mercurial.selenic.com/
[TortoiseHg]: http://tortoisehg.bitbucket.org/download/

To run the tests, you'll need [NUnit][] version 2.5.10 or higher.

[NUnit]: http://nunit.org/index.php?p=download

Unfortunately though we target .NET 3.5 for *running* Noda Time on the desktop, the unit test
library targets .NET 4. This is mostly so that we can easily run the tests against the PCL
build. We would like to run the desktop build tests against .NET 3.5 and the PCL build tests
against .NET 4, but that sort of subtlety seems to cause problems with the ability of
test runners to autodetect which version they should load. We don't use any .NET 4 features
in our tests though; if you hack at the project file to make it only target .NET 3.5, you should
be able to test the desktop build that way, if you really need to.

### Fetching and building

To fetch the source code, just clone the Google Code repository:

    > hg clone https://code.google.com/p/noda-time/

To build everything under Visual Studio, simply open the `src\NodaTime-All.sln` file.
To build with just the SDK and msbuild, run

    > msbuild src\NodaTime-All.sln /property:Configuration=Debug

Execute the tests using your favourite NUnit test runner. For example:

    > nunit-console src\NodaTime.Test\bin\Debug\NodaTime.Test.dll

(Include the other test DLLs should you wish to, of course.)

## Mono

*Note:* If you build Noda Time under Mono but execute it under the Microsoft
.NET 4 64-bit CLR, you may see exceptions around type initializers and
`RuntimeHelpers.InitializeArray`. We believe this to be due to a
[bug in .NET][ms-635365] which the Mono compiler happens to trigger. We
would recommend that you use a binary built by the Microsoft C# compiler if you
wish to run using this CLR.

[ms-635365]: http://connect.microsoft.com/VisualStudio/feedback/details/635365

We have tested Mono support using Mono 2.10.5 and 2.10.8.

To build Noda Time under Linux, you typically need to install the following
packages:

- mono-devel
- mono-xbuild

(These will add the other dependencies you need.)

Note that for Ubuntu specifically, you'll either need Ubuntu 11.10 (Oneiric) or
later, or work out how to install an unofficial backport; earlier versions of
Ubuntu [do not include a suitable version of Mono][MonoUbuntu].

[MonoUbuntu]: http://www.mono-project.com/DistroPackages/Ubuntu

To build Noda Time under OS X, [download][MonoDownload] the latest stable
release of Mono. Be sure to choose the developer package, not the smaller
runtime-only package.  To use the provided `Makefile`, you'll either need to
install [Xcode][xcode] or obtain `make` separately (for example, using
[osx-gcc-installer][] to install just the open-source parts of Xcode).

[MonoDownload]: http://www.mono-project.com/Download
[xcode]: https://developer.apple.com/xcode/
[osx-gcc-installer]: https://github.com/kennethreitz/osx-gcc-installer#readme

To fetch the source code from the main Google Code repository,
you'll need a [Mercurial][] client.

To run the tests, you'll need [NUnit][] version 2.5.10 or higher. (The version
that comes with stable builds of Mono at the time of this writing doesn't
support everything used by the unit tests of Noda Time.) Version 2.6.1 is
recommended on non-Windows platforms due to an [NUnit bug][nunit-993247] that
causes tests to fail with a "Too many open files" exception when running some
of the larger suites.

[nunit-993247]: https://bugs.launchpad.net/nunitv2/+bug/993247
  "NUnit Bug #993247: Tests fail with IOException: Too many open files"

### Fetching and building

To fetch the source code, just clone the Google Code repository:

    $ hg clone https://code.google.com/p/noda-time/

Building is performed with `make`, using the included Makefile. (If you don't
have a working `make`, you can also run `xbuild` by hand; see `Makefile` for
the commands you'll need to run.)

    $ cd noda-time
    $ make

This will build all the Noda Time main projects. The main assembly will be
written to `src/NodaTime/bin/Debug/NodaTime.dll`; this is all you need to use
Noda Time itself.

Other build targets are also available; again, see `Makefile` for documentation.
In particular, to build and run the tests, run:

    $ make check

to use the default (probably Mono-supplied) version of NUnit to run the tests,
or something like the following to override the location of the NUnit test
runner:

    $ make check NUNIT='mono ../NUnit-2.6.1/bin/nunit-console.exe'

### Source layout

All the source code is under the `src` directory. There are multiple projects:

- NodaTime: The main library to be distributed
- NodaTime.Benchmarks: Benchmarks run regularly to check the performance
- NodaTime.CheckTimeZones: Tool to verify that Joda Time and Noda Time interpret TZDB data in the same way
- NodaTime.Demo: Demonstration code written as unit tests. Interesting [Stack Overflow](http://stackoverflow.com) questions can lead to code in this project, for example.
- NodaTime.Serialization.JsonNet: Library to enable [Json.NET](http://json.net) serialization of NodaTime types.
- NodaTime.Serialization.Test: Tests for all serialization assemblies, under the assumption that at some point we'll support more than just Json.NET.
- NodaTime.Test: Tests for the main library
- NodaTime.Testing: Library to help users test code which depends on Noda Time. Also used within our own tests.
- NodaTime.Tools.SetVersion: Tool to set version numbers on appropriate assemblies as part of the release process
- NodaTime.TzdbCompiler: Tool to take a TZDB database and convert it into the NodaTime internal format
- NodaTime.TzdbCompiler.Test: Tests for NodaTime.TzdbCompiler

Additionally the `JodaDump` contains Java code to be used with NodaTime.CheckTimeZones.

The documentation is in the `www` directory with the rest of the website: `www/developer` for the developer guide, and `www/unstable/userguide` for the latest user guide. 
c
Additionally the `www` directory contains the source for the documentation, and `JodaDump` contains Java code to be used with NodaTime.CheckTimeZones.


There are four solutions, containing a variety of projects depending on the task at hand. The aim is to have everything you immediately need in one
place, without too many distractions.

- NodaTime-All.sln: Contains all .NET projects, but not the documentation source. Useful for continuous integration and refactoring.
- NodaTime-Core.sln: Contains NodaTime, NodaTime.Benchmarks, NodaTime.Serialization.JsonNet and NodaTime.Serialization.Test, NodaTime.Test, NodaTime.Testing.
 This is the "day to day development" solution, containing everything that's shipped and the primary tests for those projects. When we're updating NodaTime.TzdbCompiler,
 that project and its tests may temporarily live in this solution.
- NodaTime-Documentation.sln: Contains the documentation sources, NodaTime, NodaTime.Testing.SerializationJsonNet and NodaTime.Testing. Useful when writing documentation.
- NodaTime-Tools.sln: Contains NodaTime (so that project references work), NodaTime.CheckTimeZones, NodaTime.Tools.SetVersion, NodaTime.TzdbCompiler
 and NodaTime.TzdbCompiler.Test. Aimed at times when we need to change our supporting toolset.

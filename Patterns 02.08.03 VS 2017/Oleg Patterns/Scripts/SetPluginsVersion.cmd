@echo off
rem # Set version number from TFS build "TF_BUILD_BUILDNUMBER"
rem # First call without arguments, finds files and calls itself
rem # Second call with filename calls itself a third time for each line
rem # Third time processes each line to a temporary output file



if .%1. == .. goto firstcall
if .%1. == .@. goto thirdcall
goto secondcall

rem ==========================================================
:firstcall - start the whole thing running

rem set TF_BUILD_BUILDNUMBER=01.01.00.124

for /R %%d in (AssemblyInfo.cs) do if exist "%%d" call %0 "%%d"

goto finished

rem ==========================================================
:secondcall - called with filename

if exist %0.out del %0.out 

REM # Get Buildnumber and version
for /f "tokens=1-4 delims=." %%d in ("%TF_BUILD_BUILDNUMBER%") do (
	set MYVERSION=%%d.%%e.%%f
	set MYFILEVERSION=%%d.%%e.%%f.%%g
	set MYBUILDNUMBER=%%g
)

for /f "delims=" %%d in ('type %1') do (
    set INFOLINE=%%d
    call %0 @ 
)

REM # Get Date in string format 
set DATEYEAR=%date:~10,4%
set DATEMONTH=%date:~4,2%
set MONTHVALUE=%DATEMONTH%
if .%MONTHVALUE:~0,1%. == .0. set MONTHVALUE=%MONTHVALUE:~1,1%

set DATEDAY=%date:~7,2%
set DAYVALUE=%DATEDAY%
if .%DAYVALUE:~0,1%. == .0. set DAYVALUE=%DAYVALUE:~1,1%

set DATESTAMP=%DATEDAY%%DATEMONTH%%DATEYEAR%

attrib -r %1
copy %0.out %1

goto finished

rem ==========================================================
:thirdcall - called to process individual lines

for /f "tokens=1-3,* delims=() " %%d in ("%INFOLINE%") do (
	if .%%e. == .AssemblyVersion. (
	    echo was %%f setting %MYVERSION%
		set INFOLINE=%%d %%e^("%MYVERSION%"^) %%g
	) else if .%%e. == .AssemblyFileVersion. (
	    echo was %%f setting %MYFILEVERSION%
		set INFOLINE=%%d %%e^("%MYFILEVERSION%"^) %%g
	) else if .%%e. == .AssemblyDescription. (
		echo was %%f setting %MYBUILDNUMBER%.%DATESTAMP%
		set INFOLINE=%%d %%e^("%MYBUILDNUMBER%.%DATESTAMP%"^) %%g			
	)
)
echo %INFOLINE% >> %0.out

goto finished

rem ==========================================================
:finished

@ECHO OFF

ECHO TF_BUILD_BINARIESDIRECTORY = %TF_BUILD_BINARIESDIRECTORY% > output.log.txt
ECHO TF_BUILD_BINARIESLOCATION = %TF_BUILD_BINARIESLOCATION% >> output.log.txt
ECHO TF_BUILD_BUILDNUMBER = %TF_BUILD_BUILDNUMBER% >> output.log.txt
ECHO TF_BUILD_DROPLOCATION = %TF_BUILD_DROPLOCATION% >> output.log.txt
ECHO TF_BUILD_BUILDDEFINITIONNAME = %TF_BUILD_BUILDDEFINITIONNAME% >> output.log.txt
ECHO TF_BUILD_BUILDDIRECTORY = %TF_BUILD_BUILDDIRECTORY% >> output.log.txt
ECHO TF_BUILD_BUILDLOCATION = %TF_BUILD_BUILDLOCATION% >> output.log.txt
ECHO TF_BUILD_COLLECTIONURI = %TF_BUILD_COLLECTIONURI% >> output.log.txt
ECHO TF_BUILD_BUILDURI = %TF_BUILD_BUILDURI% >> output.log.txt
ECHO TF_BUILD_BUILDREASON = %TF_BUILD_BUILDREASON% >> output.log.txt
ECHO TF_BUILD_SOURCEGETVERSION = %TF_BUILD_SOURCEGETVERSION% >> output.log.txt
ECHO TF_BUILD_SOURCESDIRECTORY = %TF_BUILD_SOURCESDIRECTORY% >> output.log.txt
ECHO TF_BUILD_AGENT_ID = %TF_BUILD_AGENT_ID% >> output.log.txt
ECHO TF_BUILD_AGENT = %TF_BUILD_AGENT% >> output.log.txt
ECHO TF_BUILD_AGENTID = %TF_BUILD_AGENTID% >> output.log.txt
ECHO TF_AGENT = %TF_AGENT% >> output.log.txt

for /f "tokens=1-4 delims=." %%d in ("%TF_BUILD_BUILDNUMBER%") do (
 	set BUILDNUMBER=%%g
 	set FULLVERSION_WITH_COMMA=%%d,%%e,%%f
    set FULLVERSION_WITH_DOT=%%d.%%e.%%f
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

ECHO BUILDNUMBER = %BUILDNUMBER% >> output.log.txt
ECHO DATEYEAR = %DATEYEAR% >> output.log.txt
ECHO MONTHVALUE = %MONTHVALUE% >> output.log.txt
ECHO DAYVALUE = %DAYVALUE% >> output.log.txt
ECHO DATESTAMP = %DATESTAMP% >> output.log.txt
ECHO FULLVERSION_WITH_DOT = %FULLVERSION_WITH_DOT% >> output.log.txt

ECHO ---------------------------------------------------------------------- >> output.log.txt
dir /s "%TF_BUILD_DROPLOCATION%" >> output.log.txt
ECHO ---------------------------------------------------------------------- >> output.log.txt
dir /s "%TF_BUILD_BINARIESDIRECTORY%" >> output.log.txt

MD "%TF_BUILD_BINARIESDIRECTORY%\Setup2\bla\bla"

ECHO ---------------------------------------------------------------------- >> output.log.txt
dir /s "%TF_BUILD_BINARIESDIRECTORY%" >> output.log.txt

REM XCOPY "%TF_BUILD_BINARIESDIRECTORY%\PostBuildScriptTest.exe.config" "%TF_BUILD_DROPLOCATION%\Setup\PostBuildScriptTest.exe.config" /i
REM XCOPY /Y "%TF_BUILD_BINARIESDIRECTORY%\PostBuildScriptTest.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\Setup1\PostBuildScriptTest.exe.config" 
MOVE /Y "%TF_BUILD_BINARIESDIRECTORY%\PostBuildScriptTest.pdb" "%TF_BUILD_BINARIESDIRECTORY%\Setup2\bla\bla\PostBuildScriptTest %FULLVERSION_WITH_DOT% Build %BUILDNUMBER%.%DATESTAMP%.pdb" 

@echo off

REM # set shortcut paths

REM # Get Buildnumber and verion with commas
for /f "tokens=1-5 delims=." %%d in ("03.18.04.00.59") do (
 	set BUILDNUMBER=%%h
 	set FULLVERSION_WITH_COMMA=%%d,%%e,%%f,%%g
    set FULLVERSION_WITH_DOT=%%d.%%e.%%f.%%g
	set FULLVERSION_MESSAGE_VERSION=%%d%%e%%f
 )
echo %FULLVERSION_MESSAGE_VERSION%

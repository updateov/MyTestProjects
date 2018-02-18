@echo off
SET CONFIG_FILE=d:\Develop\TestProjects\BuildRunner\Batch\3\3.config.bat
CALL %CONFIG_FILE%
SET /a BUILDNUMBER=%PREVIOUSBUILD%+1

ReplaceAll %CONFIG_FILE% "SET PREVIOUSBUILD=%PREVIOUSBUILD%" "SET PREVIOUSBUILD=%BUILDNUMBER%"

ECHO Build=%BUILDNUMBER%

timeout 10
echo wait 10 seconds

echo finished
echo exiting with exitcode 1

REM exit 1;
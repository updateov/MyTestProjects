@echo off

timeout 10
echo wait 10 seconds
SET CONFIG=d:\Develop\TestProjects\BuildRunner\Batch\3\3.config.bat
CALL %CONFIG%

SET /a BUILDNUMBER=%PREVIOUSBUILD%+1


ReplaceAll "%CONFIG%" "SET PREVIOUSBUILD=%PREVIOUSBUILD%" "SET PREVIOUSBUILD=%BUILDNUMBER%"
ECHO Build=%BUILDNUMBER%

echo finished
echo exiting with exitcode 1

exit 1;
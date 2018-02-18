@echo off

SET CONFIG=d:\Develop\TestProjects\BuildRunner\Batch\2\2.config.bat
CALL %CONFIG%

SET /a BUILDNUMBER=%PREVIOUSBUILD%+1


ReplaceAll "%CONFIG%" "SET PREVIOUSBUILD=%PREVIOUSBUILD%" "SET PREVIOUSBUILD=%BUILDNUMBER%"
ECHO Build=%BUILDNUMBER%

echo 2.bat
timeout 10

exit 0;
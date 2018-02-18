@ECHO OFF

SET FOLDER=d:\Develop\TestProjects\BuildRunner\Batch\1\
SET CONFIG=1.config.bat

CALL %CONFIG%

SET /a BUILDNUMBER=%PREVIOUSBUILD%+1


ReplaceAll "%CONFIG%" "SET PREVIOUSBUILD=%PREVIOUSBUILD%" "SET PREVIOUSBUILD=%BUILDNUMBER%"

ECHO Build=%BUILDNUMBER%

ECHO Folder = %FOLDER%
ECHO Config = %CONFIG%

timeout 4
echo wait to end

rem exit 1;
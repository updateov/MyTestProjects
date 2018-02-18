@ECHO OFF

CALL d:\Develop\TestProjects\BuildRunner\Batch\1\1.config.bat

SET /a BUILDNUMBER=%PREVIOUSBUILD%+1

ReplaceAll "d:\Develop\TestProjects\BuildRunner\Batch\1\1.config.bat" "SET PREVIOUSBUILD=%PREVIOUSBUILD%" "SET PREVIOUSBUILD=%BUILDNUMBER%"

ECHO Build=%BUILDNUMBER%

timeout 4
echo wait to end
pause
rem exit 1;
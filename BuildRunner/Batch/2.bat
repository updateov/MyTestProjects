@echo off
CALL d:\Develop\TestProjects\BuildRunner\Batch\2\2.config.bat

SET /a BUILDNUMBER=%PREVIOUSBUILD%+1


ECHO Build=%BUILDNUMBER%

echo 2.bat
timeout 20

exit 0;
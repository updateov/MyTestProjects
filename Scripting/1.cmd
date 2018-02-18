@echo off
rem setlocal ENABLEDELAYEDEXPANSION
set PATH=D:
set TO_REPLACE=\\tfs


set BUILD="D:\Builds\1\MenyTest\PostBuildScriptTest"
set IBUILD="d:\Builds\1\MenyTest\PostBuildScriptTest"

Echo BUILD = %BUILD%
Echo IBUILD = %IBUILD%

call set BUILD=%%BUILD:%PATH%=%TO_REPLACE%%%
call set /I IBUILD=%%BUILD:%PATH%=%TO_REPLACE%%%
rem ReplaceAll.exe "%BUILD%" %PATH% %TO_REPLACE%

Echo BUILD = %BUILD%
Echo IBUILD = %IBUILD%
rem ReplaceAll.exe "1.txt" %PATH% %TO_REPLACE%
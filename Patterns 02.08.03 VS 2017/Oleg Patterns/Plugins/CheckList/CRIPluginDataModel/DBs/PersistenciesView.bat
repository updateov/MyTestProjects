@echo off
set outputFile=%date:~4,2%%date:~7,2%%date:~10,4%_%time:~0,2%%time:~3,2%%time:~6,2%.csv

SetLocal EnableDelayedExpansion
set startDateRange=%~1
set endDateRange=%~2

echo %startDateRange%| findstr /r "^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$">nul
echo %endDateRange%| findstr /r "^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$">nul
if errorlevel 1 (
    goto:USAGE 
)


set content=
for /F "delims=" %%i in (PersistenciesView.sql) do set content=!content! %%i



set outputFile=%outputFile: =0%


sqlite3.exe -header -csv EpisodeArchiveDB.db "%content%" >> %outputFile%

EndLocal

goto:EOF

:USAGE
echo.
echo Error. Invalid usage.
echo USAGE %~n0.bat [StartDateRange] [EndDateRange]
echo   [StartDateRange]	Low date limit (YYYY-MM-DD)
echo   [EndDateRange]	High date limit (YYYY-MM-DD)
echo.
echO Example: %~n0.bat 2000-12-01 2020-01-01
:EOF
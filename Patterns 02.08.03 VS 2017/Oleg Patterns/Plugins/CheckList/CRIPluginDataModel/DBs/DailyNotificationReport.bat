@echo off
set outputFile=%date:~4,2%%date:~7,2%%date:~10,4%_%time:~0,2%%time:~3,2%%time:~6,2%.html
set outputFile=%outputFile: =0%

SetLocal EnableDelayedExpansion
set startDateRange=%~1
set endDateRange=%~2
echo %startDateRange%| findstr /r "^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$">nul
echo %endDateRange%| findstr /r "^[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$">nul
if errorlevel 1 (
    goto:USAGE 
)


set content=
for /F "delims=" %%i in (DailyNotificationReport.sql) do set content=!content! %%i



echo ^<html^>^<head^>^<style^>table {border:none;border-collapse: collapse;}table th,td{width:150px;border: 1px solid #000;}^</style^>^</head^> > %outputFile%
echo ^<body^>^<h2^>PeriCALM CheckList™ Daily Notification Report >> %outputFile%
echo ^<body^>^<h2^>Generated on %date% %time:~0,2%:%time:~3,2%^</h2^>^<table^> >> %outputFile%
sqlite3.exe -header -html EpisodeArchiveDB.db "%content%" >> %outputFile%

EndLocal
echo ^</table^>^</body^>^</html^> >> %outputFile%

goto:EOF

:USAGE
echo.
echo Error: Invalid usage.
echo USAGE %~n0.bat [StartDateRange] [EndDateRange]
echo   [StartDateRange]	Low date limit (YYYY-MM-DD)
echo   [EndDateRange]	High date limit (YYYY-MM-DD)
echo.
echO Example: %~n0.bat 2000-12-01 2020-01-01
:EOF
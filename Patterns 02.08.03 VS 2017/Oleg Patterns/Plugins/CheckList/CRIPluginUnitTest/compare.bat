
REM NET STOP "CALM_PatternsPluginsService"

REM NET STOP "PeriCALMPatternsService"

DEL "DBs\*.db"

DEL "C:\patterns-data\*.db3"

PING -n 3 127.0.0.1>nul

START "CALM_PatternsPluginsService.exe /Console"

echo "***************"

--PING -n 5 127.0.0.1>nul

--start D:\Test-Itay\Feeder\MultiFeeder.exe 2 "D:\Test-Itay\XMLs\7025471.XML"

--PING -n 10 127.0.0.1>nul

--echo "wait for 00 to start Patterns "

--For /f "tokens=1-5 delims=/:." %%a in ("%TIME%") do (
    SET HH24=%%a
    SET MI=%%b
    SET SS=%%c
    SET FF=%%d
    
)

--SET /a delta=60-ss
--echo %delta%

--PING -n %delta% 127.0.0.1>nul

NET START "PeriCALMPatternsService"

--PING -n 20 127.0.0.1>nul

start D:\Test-Itay\Reader\CRIReaderTester.exe 
--15 60 "D:\Test-Itay\CRI Reader Results\Real1.csv"


Pause

--PING -n 3620 127.0.0.1>nul


--taskkill /im CRIReaderTester.exe
--taskkill /im MultiFeeder.exe

--start D:\Test-Itay\"CRI Reader Results"\CRIReaderCompareTool.exe "D:\Test-Itay\CRI Reader Results\M1.csv" "D:\Test-Itay\CRI Reader Results\M2.csv"

--PING -n 5 127.0.0.1>nul

--Pause

--Start D:\Test-Itay\"compare_start_00 - Copy1.bat"

--PING -n 10 127.0.0.1>nul

--taskkill /im cmd.exe

--PING -n 600 127.0.0.1>nul





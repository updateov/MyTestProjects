@echo off
ECHO BATCH FILE FOR PROCESSING AND COMPARING FAMOUS6 TRACINGS

if not exist ..\data\Famous6 unzip ..\data.zip -d ..

set consoleExe="..\..\debug\PatternsConsole.exe"
set compareExe="..\..\debug\PatternsCompare.exe"
set expertDir="..\data\Famous6"
set inDir="..\data\Famous6"
:: repDir contains text files detailing intervals in the FHR that were repaired - expert markings based on repaired version of tracing
set repDir="%inDir%\repairIntervals"
set BS=60
set DISABLE_REPAIR=1
set OP_LEVEL=0
set INT_OP_LEVEL=0
set NN_TRAIN=0

:: ************ THESE CAN BE MODIFIED BY THE USER ***********
set outDir=..\data\testOutput\Famous6
set outTag="famous6"
:: **********************************************************

mkdir %outDir%
%consoleExe% %inDir% %outDir% %BS% %OP_LEVEL% %INT_OP_LEVEL% %NN_TRAIN% %DISABLE_REPAIR% %repDir%
%compareExe% %expertDir% %outDir%\devOutput %outDir% %outTag% 0
%compareExe% %expertDir% %outDir%\devOutput %outDir% %outTag% 1
%compareExe% %expertDir% %outDir%\devOutput %outDir% %outTag% 2





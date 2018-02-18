@echo off
ECHO BATCH FILE FOR PROCESSING AND COMPARING FAMOUS6 TRACINGS

if not exist ..\data\C3D3 unzip ..\data.zip -d ..

set consoleExe="..\..\release\PatternsConsole.exe"
set compareExe="..\..\release\PatternsCompare.exe"
set expertDir1=..\data\C3D3\Group_C3
set expertDir2=..\data\C3D3\Group_D3
set inDir1=..\data\C3D3\Group_C3
set inDir2=..\data\C3D3\Group_D3
:: repDir contains text files detailing intervals in the FHR that were repaired - expert markings based on repaired version of tracing
set repDir1=%inDir1%\repairIntervals
set repDir2=%inDir2%\repairIntervals
set BS=60
set DISABLE_REPAIR=1
set OP_LEVEL=0
set INT_OP_LEVEL=0
set NN_TRAIN=0

:: ************ THESE CAN BE MODIFIED BY THE USER ***********
set outDirBase=..\data\testOutput\C3D3
set outDir1=%outDirBase%\Group_C3
set outDir2=%outDirBase%\Group_D3
set outTag="C3D3"
:: ***********************************************************
mkdir %outDirBase%
mkdir %outDir1%
mkdir %outDir2%

%consoleExe% "%inDir1%" "%outDir1%" %BS% %OP_LEVEL% %INT_OP_LEVEL% %NN_TRAIN% %DISABLE_REPAIR% "%repDir1%"
%consoleExe% "%inDir2%" "%outDir2%" %BS% %OP_LEVEL% %INT_OP_LEVEL% %NN_TRAIN% %DISABLE_REPAIR% "%repDir2%"
%compareExe% "%expertDir1%" "%outDir1%\devOutput" "%expertDir2%" "%outDir2%\devOutput" "%outDirBase%" %outTag% 0
%compareExe% "%expertDir1%" "%outDir1%\devOutput" "%expertDir2%" "%outDir2%\devOutput" "%outDirBase%" %outTag% 1
%compareExe% "%expertDir1%" "%outDir1%\devOutput" "%expertDir2%" "%outDir2%\devOutput" "%outDirBase%" %outTag% 2

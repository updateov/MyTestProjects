@echo off
ECHO BATCH FILE FOR COMPARING C3D3 batch detection to realtime detection

set compareExe=..\..\release\PatternsCompare.exe
set expertDir1=..\data\testOutput\C3D3\Group_C3\devOutput
set expertDir2=..\data\testOutput\C3D3\Group_D3\devOutput
set inDir1=..\data\testOutput\C3D3-batch\Group_C3\devOutput
set inDir2=..\data\testOutput\C3D3-batch\Group_D3\devOutput

:: ************ THESE CAN BE MODIFIED BY THE USER ***********
set outDirBase=..\data\testOutput\C3D3-batch-against-realtime
set outDir1=%outDirBase%\Group_C3
set outDir2=%outDirBase%\Group_D3
set outTag=C3D3

:: ***********************************************************
mkdir "%outDirBase%"
mkdir "%outDir1%"
mkdir "%outDir2%"

"%compareExe%" "%expertDir1%" "%inDir1%" "%expertDir2%" "%inDir2%" "%outDirBase%" "%outTag%" 0
"%compareExe%" "%expertDir1%" "%inDir1%" "%expertDir2%" "%inDir2%" "%outDirBase%" "%outTag%" 1
"%compareExe%" "%expertDir1%" "%inDir1%" "%expertDir2%" "%inDir2%" "%outDirBase%" "%outTag%" 2
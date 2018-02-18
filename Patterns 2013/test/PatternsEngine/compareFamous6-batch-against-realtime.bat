@echo off
ECHO BATCH FILE FOR COMPARING Famous 6 batch detection to realtime detection

set compareExe=..\..\release\PatternsCompare.exe
set expertDir=..\data\testOutput\Famous6\devOutput
set inDir=..\data\testOutput\Famous6-batch\devOutput

:: ************ THESE CAN BE MODIFIED BY THE USER ***********
set outDir=..\data\testOutput\Famous6-batch-against-realtime
set outTag=famous6

:: **********************************************************
mkdir "%outDir%"

"%compareExe%" "%expertDir%" "%inDir%" "%outDir%" "%outTag%" 0
"%compareExe%" "%expertDir%" "%inDir%" "%outDir%" "%outTag%" 1
"%compareExe%" "%expertDir%" "%inDir%" "%outDir%" "%outTag%" 2

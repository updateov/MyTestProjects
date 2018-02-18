@echo off
ECHO BATCH FILE FOR COMPARING FAMOUS6 PROCESSING WITH C++ engine (PatternsConsole.exe) and C# engine (PeriGen.Patterns.Engine.Processor.exe)

if not exist ..\data unzip ..\data.zip -d ..

MD "..\data\testOutput\PatternsConsoleVSPatternsProcessor\"
MD "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\"
MD "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\"
MD "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsProcessor\"

"..\..\Release\PatternsConsole.exe" ..\data\Famous6\2320968.xml ..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\ 600000 0 0
"..\..\Release\PatternsConsole.exe" ..\data\Famous6\5041108.xml ..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\ 600000 0 0
"..\..\Release\PatternsConsole.exe" ..\data\Famous6\6406557.xml ..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\ 600000 0 0
"..\..\Release\PatternsConsole.exe" ..\data\Famous6\6822506.xml ..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\ 600000 0 0
"..\..\Release\PatternsConsole.exe" ..\data\Famous6\7025471.xml ..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\ 600000 0 0
"..\..\Release\PatternsConsole.exe" ..\data\Famous6\7172174.xml ..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\ 600000 0 0

copy "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\devOutput\*.xml" "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\"
RMDIR /S /Q "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsConsole\TEMP\"

"..\..\Release\PeriGen.Patterns.Engine.Processor.exe" "..\data\Famous6\2320968.xml" "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsProcessor\2320968.xml"
"..\..\Release\PeriGen.Patterns.Engine.Processor.exe" "..\data\Famous6\5041108.xml" "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsProcessor\5041108.xml"
"..\..\Release\PeriGen.Patterns.Engine.Processor.exe" "..\data\Famous6\6406557.xml" "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsProcessor\6406557.xml"
"..\..\Release\PeriGen.Patterns.Engine.Processor.exe" "..\data\Famous6\6822506.xml" "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsProcessor\6822506.xml"
"..\..\Release\PeriGen.Patterns.Engine.Processor.exe" "..\data\Famous6\7025471.xml" "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsProcessor\7025471.xml"
"..\..\Release\PeriGen.Patterns.Engine.Processor.exe" "..\data\Famous6\7172174.xml" "..\data\testOutput\PatternsConsoleVSPatternsProcessor\PatternsProcessor\7172174.xml"

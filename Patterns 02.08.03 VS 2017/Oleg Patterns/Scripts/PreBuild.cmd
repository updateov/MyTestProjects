@echo off
rem # Set version number from TFS build "TF_BUILD_BUILDNUMBER"
rem # First call without arguments, finds files and calls itself
rem # Second call with filename calls itself a third time for each line
rem # Third time processes each line to a temporary output file

SET INSTALLER-CMD="\\LMSBUILD3\AdvInst\bin\x86\AdvancedInstaller.com"
SET REPLACE-ALL-CMD="%TF_BUILD_SOURCESDIRECTORY%\Patterns\Scripts\ReplaceAll.exe"
SET DEVENV-CMD="C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"

SET SET-PLUGIN-VERSION="%TF_BUILD_SOURCESDIRECTORY%\Patterns\Scripts\SetPluginsVersion.cmd"

SET PUBLISH-CMD="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" 
SET OBFUSCATE-CMD="\\tfs\Obfuscate\SmartAssembly.com"
SET ADVANCED-INSTALLER-FOLDER=\\lmsbuild3\Advanced Installer Builds\Patterns 02.08.03\
SET SIGNING-PATTERNS-SERVER=/S LMSBUILD3 /U eandcservice /P "E&C1908" /TN "Patterns 02.08.03 Signing"


ECHO *** TFS > build.log.txt
TIME /T >> build.log.txt

pushd Patterns\Plugins
CALL %SET-PLUGIN-VERSION%
popd

REM # Get Buildnumber and version
for /f "tokens=1-4 delims=." %%d in ("%TF_BUILD_BUILDNUMBER%") do (
	set MYVERSION=%%d.%%e.%%f
	set MYFILEVERSION=%%d.%%e.%%f.%%g
	set MYBUILDNUMBER=%%g
	set MYBUILDCOMAVERSION=%%d,%%e,%%f,%%g
)

REM # Get Date in string format 
set DATEYEAR=%date:~10,4%
set DATEMONTH=%date:~4,2%
set MONTHVALUE=%DATEMONTH%
if .%MONTHVALUE:~0,1%. == .0. set MONTHVALUE=%MONTHVALUE:~1,1%

set DATEDAY=%date:~7,2%
set DAYVALUE=%DATEDAY%
if .%DAYVALUE:~0,1%. == .0. set DAYVALUE=%DAYVALUE:~1,1%

set DATESTAMP=%DATEDAY%%DATEMONTH%%DATEYEAR%

ECHO ------------------------------ Stamping version >> build.log.txt

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Source\Patterns Application\PatternsVersionNumber.h"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Source\Patterns Application\PatternsVersionNumber.h" "01.00.00.00" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Source\Patterns Application\PatternsVersionNumber.h" "01,00,00,00" "%MYBUILDCOMAVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Source\Patterns Application\PatternsVersionNumber.h" "Unofficial build" "%MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PeriGen.Patterns.Service\PeriGen.Patterns.Service.xml"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PeriGen.Patterns.Service\PeriGen.Patterns.Service.xml" "01.00.00.00" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\PatternsOEMChartSetup\Patterns OEM Setup.vdproj"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\PatternsOEMChartSetup\Patterns OEM Setup.vdproj" "0.0.0.1" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCRIChart\PatternsCRIChartSetup\Patterns CRI Setup.vdproj"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCRIChart\PatternsCRIChartSetup\Patterns CRI Setup.vdproj" "0.0.0.1" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\LMSAssemblyInfo.cs"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\LMSAssemblyInfo.cs" "01.00.00.00" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\LMSAssemblyInfo.cs" "Unofficial build" "%MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PeriGen.Patterns.WPFLibrary\Properties\AssemblyInfo.cs"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PeriGen.Patterns.WPFLibrary\Properties\AssemblyInfo.cs" "1.0.0.0" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PeriGen.Patterns.WPFLibrary\Properties\AssemblyInfo.cs" "Unofficial build" "%MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
 
attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\Perigen.Patterns.NnetControls\Properties\AssemblyInfo.cs"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\Perigen.Patterns.NnetControls\Properties\AssemblyInfo.cs" "1.0.0.0" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\Perigen.Patterns.NnetControls\Properties\AssemblyInfo.cs" "Unofficial build" "%MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
 
attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\Retrospective Patterns Setup.nsi"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\Retrospective Patterns Setup.nsi" "9.9.9.9" "%MYFILEVERSION%"  >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\Retrospective Patterns Setup.nsi" "0.0.0.1" "%MYVERSION%"  >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\Retrospective Patterns Setup.nsi" "anonymous-build" "%MYBUILDNUMBER%.%DATESTAMP%"  >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\CALM Patterns Research Setup.nsi" "9.9.9.9"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\CALM Patterns Research Setup.nsi" "9.9.9.9" "%MYFILEVERSION%"  >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\CALM Patterns Research Setup.nsi" "0.0.0.1" "%MYVERSION%"  >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\Retrospective Patterns Setup\CALM Patterns Research Setup.nsi" "anonymous-build" "%MYBUILDNUMBER%.%DATESTAMP%"  >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\TestActiveX\TestActiveX.aip"
%INSTALLER-CMD% /edit "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\TestActiveX\TestActiveX.aip" /SetVersion %MYVERSION% -noprodcode >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\TestActiveX\TestActiveX.aip" "Unofficial build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\PeriCALM Patterns Service\PeriCALMPatternsService.aip"
%INSTALLER-CMD% /edit "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\PeriCALM Patterns Service\PeriCALMPatternsService.aip" /SetVersion "%MYVERSION%" -noprodcode >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\PeriCALM Patterns Service\PeriCALMPatternsService.aip" "Unofficial build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\PeriCALM Patterns Service\PeriWatchCuesService.aip"
%INSTALLER-CMD% /edit "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\PeriCALM Patterns Service\PeriWatchCuesService.aip" /SetVersion "%MYVERSION%" -noprodcode >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Setup\PeriCALM Patterns Service\PeriWatchCuesService.aip" "Unofficial build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns GE.html"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns GE.html" "0,0,0,1" "%MYBUILDCOMAVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns GE.html" "Unofficial build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns McKesson.html"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns McKesson.html" "0,0,0,1" "%MYBUILDCOMAVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns PeriCALM.html" "0,0,0,1" "%MYBUILDCOMAVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns McKesson.html" "Unofficial build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\Offline Patterns PeriCALM.html" "Unofficial build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\PeriGen.Patterns.Web\common\Default.aspx.cs"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsOEMChart\PeriGen.Patterns.Web\common\Default.aspx.cs" "0,0,0,1" "%MYBUILDCOMAVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\PeriCALM.Patterns.Curve.UI.Chart.csproj"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\PeriCALM.Patterns.Curve.UI.Chart.csproj" "2.1.0.%%2a" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\PageMain.xaml.cs"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\PageMain.xaml.cs" "01.00.00.00" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\AboutData.cs"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\AboutData.cs" "Unofficial Build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChartControl\AboutData.cs"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChartControl\AboutData.cs" "Unofficial Build" "%MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChartControl\CurveChartControl.nuspec"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChartControl\CurveChartControl.nuspec" "01.00.00.00" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

attrib -R "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\CLRPatternsUserControls\CLRPatternsUserControls.nuspec"
%REPLACE-ALL-CMD% "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\CLRPatternsUserControls\CLRPatternsUserControls.nuspec" "01.00.00.00" "%MYFILEVERSION%" >> build.log.txt
if %errorlevel% neq 0 GOTO BUILDFAILED

ECHO Build VDPROJ >> build.log.txt
%DEVENV-CMD% /build "Release" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Patterns.sln" /out build.log.txt



ECHO Check for "Patterns Engine" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsCompare.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsConsole.exe" GOTO BUILDFAILED  >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsDriver.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsTest.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsViewer.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.ConvertTracings.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Engine.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Engine.Data.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Engine.Processor.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Processor.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Engine.Registration.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Helper.dll" GOTO BUILDFAILED >> build.log.txt

ECHO Check for "Patterns OEM ActiveX" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Patterns OEM Chart.ocx" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriCALMPatternsOEMChart.CAB" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.ActiveXInterface.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.TestActiveX.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\PeriGen.Patterns.Web\bin\PeriGen.Patterns.Web.dll" GOTO BUILDFAILED >> build.log.txt

ECHO Check for "Patterns CRI ActiveX" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Patterns CRI Chart.ocx" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriCALMPatternsCRIChart.CAB" GOTO BUILDFAILED >> build.log.txt

ECHO Check for "Patterns for CALM" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Service.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.DecisionSupportAPI.TestTool.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PeriGen.Patterns.WebSite\bin\PeriGen.Patterns.WebSite.dll" GOTO BUILDFAILED >> build.log.txt

ECHO Check for "Patterns OEM" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Chalkboard.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Data.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Diagnostics.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Export.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Settings.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGen.Patterns.Settings.Tool.exe" GOTO BUILDFAILED >> build.log.txt

ECHO Check for "Retrospective Patterns" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\RetrospectivePatternsResearch.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\RetrospectivePatterns.exe" GOTO BUILDFAILED >> build.log.txt

ECHO Check for "Curve Control" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChartControl\Release\CurveChartControl.dll" GOTO BUILDFAILED >> build.log.txt

ECHO Check for "Plugins" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\CALMConnector.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\CommonLogger.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\CRIAlgorithm.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\CRIEntities.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\CRIPlugin.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\CRIPluginDataModel.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\CRIPluginDataModel.dll.config" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\EncDecSettings.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\EntityFramework.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\EntityFramework.SqlServer.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\EntityFramework.SqlServer.xml" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\EntityFramework.xml" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Export.Algorithm.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Export.Algorithm.dll.config" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\MessagingInterfaces.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\MessagingResponses.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Export.Entities.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Export.Plugin.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Export.Plugin.dll.config" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Export.PluginDataModel.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\Export.PluginDataModel.dll.config" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSArchiving.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSComm.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSFile.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSManagedModel.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSMessages.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSModel.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSPrinting.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSSite.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSTickerTape.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\LMSUtils.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsCALMMediator.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsCRIClient.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsCRIClient.exe.config" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsEntities.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsPluginsCommon.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsPluginsManager.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsPluginsService.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsPluginsService.exe.config" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PatternsPluginsService.psf" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriAuthCommon.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriAuthServiceProxy.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PeriGenSettingsManager.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PerigenSettingsTool.exe" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\PluginsAlgorithms.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\RestSharp.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\System.Data.SQLite.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\System.Data.SQLite.EF6.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\System.Data.SQLite.Linq.dll" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\System.Data.SQLite.xml" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Release\zlib1.dll" GOTO BUILDFAILED >> build.log.txt

ECHO *** PUBLISH >> build.log.txt
TIME /T >> build.log.txt
%PUBLISH-CMD% /t:publish /p:Configuration=release "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsCurveChart\PeriCALM.Patterns.Curve.UI.Chart.csproj"  
IF ERRORLEVEL 1 goto BUILDFAILED >> build.log.txt

ECHO Check for "CURVE XBAP"
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\bin\Release\PeriCALM.Patterns.Curve.UI.Chart.xbap" GOTO BUILDFAILED >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChart\bin\Release\app.publish\PeriCALM.Patterns.Curve.UI.Chart.xbap" GOTO BUILDFAILED >> build.log.txt
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsCurveChart\bin\Release\app.publish\*.*" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PeriGen.Patterns.WebSite\common\" /E /I /R /Y

ECHO *** INSTALLER >> build.log.txt
TIME /T >> build.log.txt

REM --------------------------------------------------------------------------------------------
REM ----                   NO NEED FOR RETROSPECTIVE INSTALLERS FOR NOW                     ----
REM --------------------------------------------------------------------------------------------

REM ECHO ------------------------------ Retrospective installers
REM %SIGN-CMD% "PATTERN\Release\RetrospectivePatterns.exe"
REM if %errorlevel% neq 0 GOTO BUILDFAILED
REM %SIGN-CMD% "PATTERN\Release\RetrospectivePatternsResearch.exe"
REM if %errorlevel% neq 0 GOTO BUILDFAILED

REM ECHO Retrospective Start
REM 
REM COPY "PATTERN\Source\Help\*.*" "PATTERN\Release\"
REM %NSIS-CMD% "PATTERN\Setup\Retrospective Patterns Setup\Retrospective Patterns Setup.nsi"
REM %NSIS-CMD% "PATTERN\Setup\Retrospective Patterns Setup\CALM Patterns Research Setup.nsi"
REM 
REM 
REM 
REM IF NOT EXIST "PATTERN\Release\PeriCALM Patterns Retrospective %PRODUCT_VERSION% Setup.exe" GOTO BUILDFAILED
REM IF NOT EXIST "PATTERN\Release\PeriCALM Patterns Research %PRODUCT_VERSION% Setup.exe" GOTO BUILDFAILED

REM --------------------------------------------------------------------------------------------
REM ----                   NO NEED FOR RETROSPECTIVE INSTALLERS FOR NOW                     ----
REM --------------------------------------------------------------------------------------------


ECHO ------------------------------ Obfuscation >> build.log.txt
ECHO PeriGen.Patterns.Engine.Processor...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.Engine.Processor.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Engine.Processor.exe" GOTO BUILDFAILED

ECHO PeriGen.Patterns.Engine.Registration...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.Engine.Registration.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Engine.Registration.exe" GOTO BUILDFAILED

ECHO PeriGen.Patterns.TestActiveX...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.TestActiveX.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.TestActiveX.exe" GOTO BUILDFAILED

ECHO PeriGen.Patterns.Settings.Tool...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.Settings.Tool.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Settings.Tool.exe" GOTO BUILDFAILED

ECHO PeriGen.Patterns.Chalkboard...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.Chalkboard.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Chalkboard.exe" GOTO BUILDFAILED

ECHO PeriGen.Patterns.Export...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.Export.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Export.exe" GOTO BUILDFAILED

ECHO PeriGen.Patterns.ConvertTracings...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.ConvertTracings.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.ConvertTracings.exe" GOTO BUILDFAILED

ECHO PeriGen.Patterns.Web...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.Web.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Web.dll" GOTO BUILDFAILED

ECHO PeriGen.Patterns.WebSite...
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.WebSite.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.WebSite.dll" GOTO BUILDFAILED

ECHO PeriGen.Patterns.Service.saproj
%OBFUSCATE-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriGen.Patterns.Service.saproj" >> build.log.txt
IF NOT EXIST "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Service.exe" GOTO BUILDFAILED

REM ECHO ------------------------------ Signing >> build.log.txt
REM %SIGN-CMD% "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\*.exe" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\*.dll" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\*.exe" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Perigen*.dll" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsCurveChartControl\Release\Curve*.dll" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Patterns*.dll" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\*.CAB" >> build.log.txt
REM if %errorlevel% neq 0 GOTO BUILDFAILED

ECHO --------------------------- Copy for Signing >> build.log.txt
IF EXIST "%ADVANCED-INSTALLER-FOLDER%" RMDIR "%ADVANCED-INSTALLER-FOLDER%" /s /q
MD "%ADVANCED-INSTALLER-FOLDER%"

XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\*.*" "%ADVANCED-INSTALLER-FOLDER%Wrapper\PatternsOEMChart\" /E /R /Y /I
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsCRIChart\*.*" "%ADVANCED-INSTALLER-FOLDER%Wrapper\PatternsCRIChart\" /E /R /Y /I
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\*.*" "%ADVANCED-INSTALLER-FOLDER%Release\" /E /R /Y /I

TIME /T >> build.log.txt

ECHO -------------- Run CAB Signing  >> build.log.txt
schtasks /Run %SIGNING-PATTERNS-SERVER% 
if errorlevel 1 goto failtask

ping -n 20 localhost
ECHO Sign successful
goto loop


:failtask
ECHO Task failed >> build.log.txt

:loop
for /f "tokens=2 delims=: " %%f in ('schtasks /Query %SIGNING-PATTERNS-SERVER% /FO LIST ^| find "Status:"' ) do (
    if "%%f"=="Running" (
        ping -n 10 localhost
        goto loop
    )
)



ECHO -------------------- Copy back for WIX Installer  >> build.log.txt
XCOPY "%ADVANCED-INSTALLER-FOLDER%Release\PeriCALMPatternsCRIChart.CAB" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriCALMPatternsCRIChart.CAB" /E /R /Y /I
XCOPY "%ADVANCED-INSTALLER-FOLDER%Release\PeriCALMPatternsOEMChart.CAB" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriCALMPatternsOEMChart.CAB" /E /R /Y /I



GOTO SUCCESS

:BUILDFAILED
ECHO ------------------------------------------------------------------- >> build.log.txt
ECHO SET VERSION FAILED >> build.log.txt
ECHO ------------------------------------------------------------------- >> build.log.txt
GOTO FINISH

:SUCCESS
ECHO ------------------------------------------------------------------- >> build.log.txt
ECHO SET VERSION SUCCEEDED >> build.log.txt
ECHO ------------------------------------------------------------------- >> build.log.txt

:FINISH
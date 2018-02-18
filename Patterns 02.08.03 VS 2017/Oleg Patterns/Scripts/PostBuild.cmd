@ECHO OFF

REM SET PUBLISH-CMD="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" 
SET NUGET-CMD="%TF_BUILD_SOURCESDIRECTORY%\Patterns\NuGet.exe"
REM SET SIGN-CMD="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\bin\signtool.exe" sign /q /s MY /n "PeriGen" /t http://timestamp.comodoca.com/authenticode
REM SET OBFUSCATE-CMD="\\tfs\Obfuscate\SmartAssembly.com"
REM SET INSTALLER-CMD="\\LMSBUILD3\AdvInst\bin\x86\AdvancedInstaller.com"
SET INSTALLER-PATTERNS-SERVER=/S LMSBUILD3 /U eandcservice /P "E&C1908" /TN "Patterns 02.08.03 Build"
REM SET SIGNING-PATTERNS-SERVER=/S LMSBUILD3 /U eandcservice /P "E&C1908" /TN "Patterns 02.08.03 Signing"
SET ADVANCED-INSTALLER-FOLDER=\\lmsbuild3\Advanced Installer Builds\Patterns 02.08.03\
SET RUN-STATISTIC-CMD="%TF_BUILD_SOURCESDIRECTORY%\Patterns\test\PatternsEngine\ALL.BAT"

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



ECHO ------------------------------  Copying sources for AdvancedInstaller

REM IF EXIST "%ADVANCED-INSTALLER-FOLDER%" RMDIR "%ADVANCED-INSTALLER-FOLDER%" /s /q
REM MD "%ADVANCED-INSTALLER-FOLDER%"
REM 
REM XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsCurveChart\*.*" "%ADVANCED-INSTALLER-FOLDER%Wrapper\PatternsCurveChart\" /E /R /Y /I
REM XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PeriGen.Patterns.WebSite\*.*" "%ADVANCED-INSTALLER-FOLDER%Wrapper\PeriGen.Patterns.WebSite\" /E /R /Y /I
REM XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\*.*" "%ADVANCED-INSTALLER-FOLDER%Wrapper\PatternsOEMChart\" /E /R /Y /I
REM XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsCRIChart\*.*" "%ADVANCED-INSTALLER-FOLDER%Wrapper\PatternsCRIChart\" /E /R /Y /I
REM XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\*.*" "%ADVANCED-INSTALLER-FOLDER%Setup\" /E /R /Y /I
REM XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\*.*" "%ADVANCED-INSTALLER-FOLDER%Release\" /E /R /Y /I
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\app.ico" "%ADVANCED-INSTALLER-FOLDER%app.ico" > nul
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\cues.ico" "%ADVANCED-INSTALLER-FOLDER%cues.ico" > nul
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\TestPatternsControlApp\obj\x86\Release\TestPatternsControlApp.exe" "%ADVANCED-INSTALLER-FOLDER%Release\TestPatternsControlApp.exe" /E /R /Y /I

REM ECHO -------------- Run CAB Signing
REM schtasks /Run %SIGNING-PATTERNS-SERVER% 
REM if errorlevel 1 goto failtask
REM 
REM ping -n 20 localhost
REM 
REM :loop
REM for /f "tokens=2 delims=: " %%f in ('schtasks /Query %SIGNING-PATTERNS-SERVER% /FO LIST ^| find "Status:"' ) do (
REM     if "%%f"=="Running" (
REM         ping -n 10 localhost
REM         goto loop
REM     )
REM )
REM 
REM ECHO --------------- Build Installers
REM schtasks /Run %INSTALLER-PATTERNS-SERVER% 
REM if errorlevel 1 goto failtask
REM 
REM ping -n 20 localhost
REM 
REM :loop
REM for /f "tokens=2 delims=: " %%f in ('schtasks /Query %INSTALLER-PATTERNS-SERVER% /FO LIST ^| find "Status:"' ) do (
REM     if "%%f"=="Running" (
REM         ping -n 10 localhost
REM         goto loop
REM     )
REM )
REM 
REM ECHO -------------------- Copy back for Plugins Installer
REM COPY /Y "%ADVANCED-INSTALLER-FOLDER%Release\PeriCALMPatternsCRIChart.CAB" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriCALMPatternsCRIChart.CAB" /E /R /Y /I
REM COPY /Y "%ADVANCED-INSTALLER-FOLDER%Release\PeriCALMPatternsOEMChart.CAB" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriCALMPatternsOEMChart.CAB" /E /R /Y /I
REM 

REM %INSTALLER-CMD% /build "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriCALM Patterns Service\PeriCALMPatternsService.aip"
REM IF NOT EXIST "%ADVANCED-INSTALLER-FOLDER%Setup\PeriCALM Patterns Service\PeriCALMPatternsService-SetupFiles\PeriCALM Patterns Service - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" GOTO BUILDFAILED
REM IF NOT EXIST "%ADVANCED-INSTALLER-FOLDER%Setup\PeriCALM Patterns Service\PeriWatchCuesService-SetupFiles\PeriWatch Cues Service - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" GOTO BUILDFAILED
REM IF NOT EXIST "%ADVANCED-INSTALLER-FOLDER%Setup\TestActiveX\TestActiveX-SetupFiles\PeriCALM Patterns ActiveX TestBed - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" GOTO BUILDFAILED

REM ECHO ------------------------------ Create NUGET

REM %NUGET-CMD% pack "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\PeriCALMPatternsCRIChart.nuspec" -IncludeReferencedProjects -Prop Configuration=Release -OutputDirectory "\\Tfs\nuget"

ECHO ----------------- Engine  >> build.log.txt
MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\"
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Source\Patterns Application\patterns.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\SerialShield.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsCompare.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsConsole.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsDriver.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsViewer.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.Engine.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.Engine.Data.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Engine.Processor.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.Engine.Processor.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Engine.Registration.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.Engine.Registration.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.Helper.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.ConvertTracings.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.ConvertTracings.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\" >> build.log.txt
REM 
ECHO ----------------- Active X  >> build.log.txt
MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\"
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\*.ocx" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\*.CAB" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.ActiveXInterface.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.NnetControls.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.NnetControls.tlb" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\" >> build.log.txt
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\TestActiveX\TestActiveX-SetupFiles\PeriCALM Patterns ActiveX TestBed - %PRODUCT_VERSION% Build %BUILD%.%BUILDDATE% Setup.msi" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\" > nul
REM COPY /Y "%ADVANCED-INSTALLER-FOLDER%Setup\TestActiveX\TestActiveX-SetupFiles\PeriCALM Patterns ActiveX TestBed - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\ActiveX\" > nul

REM 
ECHO ----------------- Marketing Demo  >> build.log.txt
MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\"
MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\GE\"
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Patterns OEM Chart.ocx" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\GE\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\register ocx.bat" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\GE\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\Offline Patterns GE.html" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\GE\" >> build.log.txt
MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\McKesson\"
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Patterns OEM Chart.ocx" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\McKesson\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\register ocx.bat" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\McKesson\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\Offline Patterns McKesson.html" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\McKesson\" >> build.log.txt

MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\PeriCALM\"
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Patterns OEM Chart.ocx" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\PeriCALM\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\register ocx.bat" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\PeriCALM\" >> build.log.txt

COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Patterns CRI Chart.ocx" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\PeriCALM\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsCRIChart\register ocx.bat" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\PeriCALM\" >> build.log.txt

COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Wrapper\PatternsOEMChart\Offline Patterns PeriCALM.html" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Demo\PeriCALM\" >> build.log.txt
REM 
REM ECHO ----------------- Retrospective Patterns >> build.log.txt
REM MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Retrospective\"
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriCALM Patterns Retrospective %PRODUCT_VERSION% Setup.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Retrospective\"
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriCALM Patterns Research %PRODUCT_VERSION% Setup.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Retrospective\"
REM 
ECHO ----------------- CALM >> build.log.txt
MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\"
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriCALM Patterns Service\PeriCALMPatternsService-SetupFiles\PeriCALM Patterns Service - %PRODUCT_VERSION% Build %BUILD%.%BUILDDATE% Setup.msi" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\" > nul
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Export.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.Export.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.DecisionSupportAPI.TestTool.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.DecisionSupportAPI.TestTool.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\" >> build.log.txt
REM COPY /Y "%ADVANCED-INSTALLER-FOLDER%Setup\PeriCALM Patterns Service\PeriCALMPatternsService-SetupFiles\PeriCALM Patterns Service - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\" > nul
REM COPY /Y "%ADVANCED-INSTALLER-FOLDER%Setup\PeriCALM Patterns Service\PeriWatchCuesService-SetupFiles\PeriWatch Cues Service - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\CALM\" > nul
REM 
ECHO ----------------- PLUGINS >> build.log.txt
MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\"
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Config\*.*" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\Config\" /E /R /Y /I >> build.log.txt
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\DBs\*.*" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\DBs\" /E /R /Y /I >> build.log.txt
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Help\*.*" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\Help\" /E /R /Y /I >> build.log.txt
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\res\*.*" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\res\" /E /R /Y /I >> build.log.txt
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\x64\*.*" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\x64\" /E /R /Y /I >> build.log.txt
XCOPY "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\x86\*.*" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\x86\" /E /R /Y /I >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Patterns CRI Chart.ocx" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIReaderCompareTool.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIReaderTester.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PerigenSettingsTool.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsCRIClient.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CALMConnector.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CommonLogger.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIAlgorithm.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIEntities.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIPlugin.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIPluginDataModel.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIPluginDataModelUnitTest.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\EncDecSettings.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\EntityFramework.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\EntityFramework.SqlServer.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.Algorithm.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.CALMManager.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\ExportConfig.xml" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.Entities.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.Plugin.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.PluginDataModel.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSArchiving.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSComm.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSFile.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSManagedModel.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSMessages.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSModel.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSPrinting.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSSite.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSTickerTape.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\LMSUtils.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsCALMMediator.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsEntities.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsPluginsCommon.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsPluginsManager.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriAuthCommon.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriAuthServiceProxy.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGenSettingsManager.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PluginsAlgorithms.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\RestSharp.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\System.Data.SQLite.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\System.Data.SQLite.EF6.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\System.Data.SQLite.Linq.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\zlib1.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsPluginsService.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsPluginsService.psf" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\EntityFramework.xml" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\EntityFramework.SqlServer.xml" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsPluginsService.xml" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\RestSharp.xml" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\System.Data.SQLite.xml" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIPluginDataModel.dll.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIReaderCompareTool.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\CRIReaderTester.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.Algorithm.dll.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.Plugin.dll.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Export.PluginDataModel.dll.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsCRIClient.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PatternsPluginsService.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PresentationFramework.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PresentationFramework.Aero.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PresentationCore.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\AboutBoxWrapper.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt
COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.WPFLibrary.dll" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\PLUGINS\" >> build.log.txt

REM ECHO ----------------- GE Service >> build.log.txt
REM MD "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\GE\"
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\PeriCALM Patterns GE Service\PeriCALMPatternsService-GE-SetupFiles\PeriCALM Patterns Service - GE - %PRODUCT_VERSION% Build %BUILD%.%BUILDDATE% Setup.msi" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\GE\" > nul
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Setup\GEInterfaceTest\GEInterfaceTest-SetupFiles\PeriCALM Patterns GE Interface Test - %PRODUCT_VERSION% Build %BUILD%.%BUILDDATE% Setup.msi" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\GE\" > nul
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\Obfuscation\PeriGen.Patterns.Export.exe" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\GE\" > nul
REM COPY /Y "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\PeriGen.Patterns.Export.exe.config" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\GE\" > nul
REM 

REM ECHO ------------------------------ Copying installers
REM IF NOT EXIST "BUILDS\CALM\" MKDIR "BUILDS\CALM\"
REM COPY /Y "%ADVANCED-INSTALLER-FOLDER%Setup\PeriCALM Patterns Service\PeriCALMPatternsService-SetupFiles\PeriCALM Patterns Service - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" "BUILDS\CALM\" > nul

REM IF NOT EXIST "BUILDS\ActiveX\" MKDIR "BUILDS\ActiveX\"
REM COPY /Y "%ADVANCED-INSTALLER-FOLDER%Setup\TestActiveX\TestActiveX-SetupFiles\PeriCALM Patterns ActiveX TestBed - %MYVERSION% Build %MYBUILDNUMBER%.%DATESTAMP% Setup.msi" "BUILDS\ActiveX\" > nul

REM XCOPY "%TF_BUILD_SOURCESDIRECTORY%\BUILDS\*.*" "%TF_BUILD_SOURCESDIRECTORY%\PATTERNS\Release\" /E /R /Y /I

ECHO ------------------ Create NuGet
ECHO ------------------ Create NuGet >> build.log.txt


%NUGET-CMD% pack "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\PatternsCurveChartControl\CurveChartControl.csproj" -Prop Configuration=Release -IncludeReferencedProjects -OutputDirectory "\\TFS\NUGET" >> build.log.txt
ECHO Created Curve control NuGet >> build.log.txt
%NUGET-CMD% pack "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Wrapper\CLRPatternsUserControls\CLRPatternsUserControls.nuspec" -Prop Configuration=Release -IncludeReferencedProjects -OutputDirectory "\\TFS\NUGET" >> build.log.txt
ECHO Create Patterns control NuGet >> build.log.txt

ECHO ----------------- Create BUILDS folder >> build.log.txt

ECHO ------------------------------ Running statistical validation tests >> build.log.txt
ECHO *** STATISTICS >> build.log.txt
TIME /T >> build.log.txt

CD %TF_BUILD_SOURCESDIRECTORY%\Patterns\test\PatternsEngine\
CALL %RUN-STATISTIC-CMD%

ECHO ---STATISTICS DONE--- >> build.log.txt

XCOPY "%TF_BUILD_SOURCESDIRECTORY%\Patterns\test\data\testOutput\*.*" "%TF_BUILD_BINARIESDIRECTORY%\BUILDS\Engine\ValidationTestOutput\" /S /I /Q /Y >> build.log.txt


REM --- DBUpdate Scripts For Export Plugin----
REM dir "%TF_BUILD_SOURCESDIRECTORY%\*.*" > "%TF_BUILD_BINARIESDIRECTORY%\filelist.log"
xcopy "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Plugins\Export\DBUpdate.Export\bin\Release" "%TF_BUILD_BINARIESDIRECTORY%\Setup\Server\DBUpdate.Export" /S /I /V
xcopy "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Plugins\Export\DBUpdate.Export\DBUpdate" "%TF_BUILD_BINARIESDIRECTORY%\Setup\Server\DBUpdate.Export\DBUPDATE"  /S /I /V
xcopy "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Plugins\Export\DBUpdate.Export\DBUpdate_Cues" "%TF_BUILD_BINARIESDIRECTORY%\Setup\Server\DBUpdate.Export\DBUPDATE_Cues"  /S /I /V

REM --- DBUpdate Scripts For CheckList Plugin----
xcopy "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Plugins\CheckList\DBUpdate.CheckList\bin\Release" "%TF_BUILD_BINARIESDIRECTORY%\Setup\Server\DBUpdate.CheckList" /S /I /V
xcopy "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Plugins\CheckList\DBUpdate.CheckList\DBUpdate" "%TF_BUILD_BINARIESDIRECTORY%\Setup\Server\DBUpdate.CheckList\DBUPDATE"  /S /I /V
xcopy "%TF_BUILD_SOURCESDIRECTORY%\Patterns\Plugins\CheckList\DBUpdate.CheckList\DBUpdate_Cues" "%TF_BUILD_BINARIESDIRECTORY%\Setup\Server\DBUpdate.CheckList\DBUPDATE_Cues"  /S /I /V




ECHO ------------------------------------------------------------------- >> build.log.txt
ECHO BUILD SUCCESS >> build.log.txt
ECHO ------------------------------------------------------------------- >> build.log.txt
GOTO CLOSE

:BUILDFAILED
ECHO ------------------------------------------------------------------- >> build.log.txt
ECHO BUILD FAILED >> build.log.txt
ECHO ------------------------------------------------------------------- >> build.log.txt
EXIT /B 1

:CLOSE

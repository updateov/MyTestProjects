SetCompressor /SOLID /FINAL lzma
SetCompressorDictSize 64

!include MUI2.nsh

; Define the application version informations
!define VERSION '0.0.0.1'
!define BUILD 'anonymous-build'

; Define your application name
!define APPNAME "PeriCALM Patterns Retrospective"
!define APPNAMEANDVERSION "${APPNAME} ${VERSION}"

; Main Install settings
Name "PeriCALM® Patterns™ Retrospective"
InstallDir "$PROGRAMFILES\PeriGen\${APPNAME}"
InstallDirRegKey HKLM "Software\${APPNAME}" ""
OutFile "..\..\Release\${APPNAMEANDVERSION} Setup.exe"
BrandingText /TRIMRIGHT "PeriGen Inc."
CRCCheck on
XPStyle on
ShowInstDetails nevershow
ShowUninstDetails nevershow
SetDateSave on
RequestExecutionLevel user

VIProductVersion 9.9.9.9
VIAddVersionKey ProductName "${APPNAMEANDVERSION}"
VIAddVersionKey ProductVersion "${VERSION} Build ${BUILD}"
VIAddVersionKey CompanyName "PeriGen Inc."
VIAddVersionKey CompanyWebsite "www.perigen.com"
VIAddVersionKey FileVersion "${VERSION}"
VIAddVersionKey FileDescription "${APPNAMEANDVERSION} Installer"
VIAddVersionKey LegalCopyright "Copyright 1997-2012. PeriGen Inc. All rights reserved."

; MUI Settings / Icons
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\orange-install.ico"
!define MUI_UNICON "uninstaller.ico"

; MUI Settings / Header
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_LEFT
!define MUI_HEADERIMAGE_BITMAP "header.bmp"
!define MUI_HEADERIMAGE_UNBITMAP "header-un.bmp"

; MUI Settings / Wizard
!define MUI_WELCOMEFINISHPAGE_BITMAP "wizard.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "wizard-un.bmp"

!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_RUN "$INSTDIR\RetrospectivePatterns.exe"
!define MUI_FINISHPAGE_TITLE_3LINES

!define MUI_LICENSEPAGE_CHECKBOX
!insertmacro MUI_PAGE_LICENSE "License.rtf"

!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_LINK "Visit PeriGen site"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.perigen.com/"

!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

; Set languages (first is default language)
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_RESERVEFILE_LANGDLL

Section "${APPNAME}" Section1

	SetShellVarContext all

	; Set Section properties
	SetOverwrite on

	; Set Section Files and Shortcuts
	SetOutPath "$INSTDIR\"
	File "..\..\Release\RetrospectivePatterns.exe"
	File "..\..\Release\RetrospectivePatternsInstallationGuide.pdf"
	File "..\..\Release\RetrospectivePatternsQuickStart.pdf"
	File "..\..\Release\CALMPatterns_UserGuide.pdf"
	File "..\..\Release\CALMPatterns.chm"
	File "..\..\Release\SerialShield.dll"
	File "..\..\third party\vcredist_x86.exe"

	CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\RetrospectivePatterns.exe"
	CreateShortCut "$STARTMENU\${APPNAME}.lnk" "$INSTDIR\RetrospectivePatterns.exe"
	CreateDirectory "$SMPROGRAMS\${APPNAME}"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\RetrospectivePatterns.exe"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME} Quick Start.lnk" "$INSTDIR\RetrospectivePatternsQuickStart.pdf"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME} Installation Guide.lnk" "$INSTDIR\RetrospectivePatternsInstallationGuide.pdf"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\PeriCALM Patterns User Guide.lnk" "$INSTDIR\CALMPatterns_UserGuide.pdf"
	CreateShortCut "$SMPROGRAMS\${APPNAME}\Uninstall ${APPNAME}.lnk" "$INSTDIR\uninstall.exe"
SectionEnd

Section -FinishSection

	WriteRegStr HKLM "Software\${APPNAME}" "" "$INSTDIR"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLInfoAbout" "http://www.perigen.com/"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "PeriCALM Patterns Retrospective"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\uninstall.exe"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "PeriGen Inc."
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "HelpLink" "http://www.perigen.com/ContactUs.aspx?Section=ContactUs"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "HelpTelephone" "1-877-263-2256"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSION}"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$INSTDIR\RetrospectivePatterns.exe"
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" "1"
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" "1"	
	WriteUninstaller "$INSTDIR\uninstall.exe"

	ExecWait '"$INSTDIR\vcredist_x86.exe" /q'
	Delete "$INSTDIR\vcredist_x86.exe"
SectionEnd

; Modern install component descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${Section1} ""
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;Uninstall section
Section Uninstall

	SetShellVarContext all
	
	;Remove from registry...
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
	DeleteRegKey HKLM "SOFTWARE\${APPNAME}"

	; Delete self
	Delete "$INSTDIR\uninstall.exe"

	; Delete Shortcuts
	Delete "$DESKTOP\${APPNAME}.lnk"
	Delete "$STARTMENU\${APPNAME}.lnk"
	Delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
	Delete "$SMPROGRAMS\${APPNAME}\${APPNAME} Quick Start.lnk"
	Delete "$SMPROGRAMS\${APPNAME}\${APPNAME} Installation Guide.lnk"
	Delete "$SMPROGRAMS\${APPNAME}\PeriCALM Patterns User Guide.lnk"
	Delete "$SMPROGRAMS\${APPNAME}\Uninstall ${APPNAME}.lnk"

	; Clean up RetrospectivePatterns
	Delete "$INSTDIR\RetrospectivePatterns.exe"
	Delete "$INSTDIR\RetrospectivePatternsInstallationGuide.pdf"
	Delete "$INSTDIR\RetrospectivePatternsQuickStart.pdf"
	Delete "$INSTDIR\CALMPatterns_UserGuide.pdf"
	Delete "$INSTDIR\CALMPatterns.chm"
	Delete "$INSTDIR\SerialShield.dll"
	Delete "$INSTDIR\vcredist_x86.exe"

	; Remove remaining directories
	RMDir /r "$SMPROGRAMS\${APPNAME}"
	RMDir /r "$INSTDIR\"

SectionEnd

Function .onInit
    push "RetrospectivePatterns.exe"
    processwork::KillProcess
    Sleep 2000

  ReadRegStr $R0 HKLM \
  "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" \
  "UninstallString"
  StrCmp $R0 "" done

;Run the uninstaller
uninst:
  ClearErrors

  ExecWait '$R0 _?=$INSTDIR' ;Do not copy the uninstaller to a temp file

  IfErrors no_remove_uninstaller
    ;You can either use Delete /REBOOTOK in the uninstaller or add some code
    ;here to remove the uninstaller. Use a registry key to check
    ;whether the user has chosen to uninstall. If you are using an uninstaller
    ;components page, make sure all sections are uninstalled.
  no_remove_uninstaller:

done:
FunctionEnd
Function un.onInit
    push "RetrospectivePatterns.exe"
    processwork::KillProcess
    Sleep 2000
FunctionEnd

; eof
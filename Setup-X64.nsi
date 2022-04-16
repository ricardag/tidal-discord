# Uses NSIS Installer 3.0
# http://nsis.sourceforge.net/
#
!include "MUI.nsh"
!include "nsProcess.nsh"

Name "TIDAL Rich Presence for Discord"

outFile "TIDAL-Discord-setup-X64.exe"
InstallDir "$PROGRAMFILES64\@ricardag\TIDAL Rich Presence for Discord"
RequestExecutionLevel admin ;Require admin rights on NT6+ (When UAC is turned on)

!define MUI_ICON "TidalDiscord\tidal.ico"
!define MUI_UNICON "TidalDiscord\tidal.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
 
!insertmacro MUI_LANGUAGE "English"


!macro TerminateApp
	${nsProcess::FindProcess} "TidalDiscord.exe" $R0

	${If} $R0 == 0
		DetailPrint "'TIDAL Rich Presence for Discord' is running. Closing it down"
		${nsProcess::CloseProcess} "TidalDiscord.exe" $R0
		DetailPrint "Waiting for 'TIDAL Rich Presence for Discord' to close"
		Sleep 2000  
	${Else}
		DetailPrint "'TIDAL Rich Presence for Discord' was not found to be running"        
	${EndIf}    

	${nsProcess::Unload}
!macroend



Section "install"
	!insertmacro TerminateApp

	setOutPath $INSTDIR
	writeUninstaller $INSTDIR\uninstaller.exe
	File "TidalDiscord\tidal.ico"
	File "TidalDiscord\bin\Release\net6.0-windows\publish\net6\*.*"
	
	# Startup
	CreateShortCut "$DESKTOP\TIDAL Rich Presence for Discord.lnk" "$INSTDIR\TidalDiscord.exe" "" "$INSTDIR\tidal.ico" 0
	
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TIDAL Rich Presence for Discord" "DisplayName" "TIDAL Rich Presence for Discord"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TIDAL Rich Presence for Discord" "UninstallString" "$INSTDIR\uninstaller.exe"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TIDAL Rich Presence for Discord" "Publisher" "@ricardag"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TIDAL Rich Presence for Discord" "DisplayIcon" "$INSTDIR\TidalDiscord.exe"
	
	
	# Runs app
	Exec '$INSTDIR\TidalDiscord.exe'
SectionEnd

Section "Uninstall"
	!insertmacro TerminateApp

	delete "$DESKTOP\TIDAL Rich Presence for Discord.lnk"
	
	delete "$INSTDIR\*.*"
	RmDir  $INSTDIR
	
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TIDAL Rich Presence for Discord"
SectionEnd

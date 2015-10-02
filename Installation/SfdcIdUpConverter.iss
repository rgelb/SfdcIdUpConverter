[Setup]
AppName=Sfdc Id Up Converter
AppVersion=1.0
AppCopyright=Robert Gelb 2015
AppPublisher=Robert Gelb
AppVerName=Sfdc Id Up Converter 1.0
AppPublisherURL=http://www.rejbrand.se
AppSupportURL=http://www.algosim.se
AppUpdatesURL=http://www.algosim.se


DefaultDirName={userpf}\Sfdc Id Up Converter

; Since no icons will be created in "{group}", we don't need the wizard
; to ask for a Start Menu folder name:
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\SfdcIdUpConverter.exe

[Files]
Source: "..\bin\debug\SfdcIdUpConverter.exe"; DestDir: "{app}"
Source: "..\bin\debug\SfdcIdUpConverter.exe.config"; DestDir: "{app}"

[Icons]
Name: "{userstartup}\SfdcId Up Converter"; Filename: "{app}\SfdcIdUpConverter.exe"

[Run]
Filename: "{app}\SfdcIdUpConverter.exe"
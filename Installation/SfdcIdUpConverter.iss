[Setup]
AppName=Sfdc Id Up Converter
AppVersion=1.2
AppCopyright=Robert Gelb 2015
AppPublisher=Robert Gelb
AppVerName=Sfdc Id Up Converter 1.2
DefaultDirName={userpf}\Sfdc Id Up Converter
DisableDirPage=yes
OutputBaseFilename=SfdcIdUpConverterSetup
AppId=Sfdc Id Up Converter
UsePreviousAppDir=yes

DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\SfdcIdUpConverter.exe

[Files]
Source: "..\bin\debug\SfdcIdUpConverter.exe"; DestDir: "{app}"
Source: "..\bin\debug\SfdcIdUpConverter.exe.config"; DestDir: "{app}"; Flags: onlyifdoesntexist

[Icons]
Name: "{userstartup}\SfdcId Up Converter"; Filename: "{app}\SfdcIdUpConverter.exe"

[Run]
Filename: "{app}\SfdcIdUpConverter.exe"; Flags: postinstall nowait
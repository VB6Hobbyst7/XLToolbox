{
; =====================================================================
; == custom-code.dist.pas
; == Part of VstoAddinInstaller
; == (https://github.com/bovender/VstoAddinInstaller)
; == (c) 2016 Daniel Kraus <bovender@bovender.de>
; == Published under the Apache License 2.0
; == See http://www.apache.org/licenses
; =====================================================================
; 
; DO NOT EDIT THIS FILE DIRECTLY.
; Changes may be lost if the file is updated from the Git repository.
; Make a copy of the file in the parent folder and name it 'code.iss'.
; Then, insert the information relevant to your addin.
; The code from this file will be included after all other code.
; DO NOT ADD A [Code] LINE
}

function IsLegacyInstalled(): Boolean;
var
  b: Boolean;
begin
  b := RegKeyExists(HKEY_CURRENT_USER,
    'Software\Microsoft\Windows\CurrentVersion\Uninstall\{BDE4805C-4A64-4C6D-8547-5B7DB885C65F}_is1');
  if b then
    Log('Found registry key for legacy uninstaller')
  else
    Log('Did not find registry key for legacy uninstaller');
  result := b;
end;

function LegacyUninstallerPath(Param: String): String;
var
  s: string;
begin
  RegQueryStringValue(HKEY_CURRENT_USER,
    'Software\Microsoft\Windows\CurrentVersion\Uninstall\{BDE4805C-4A64-4C6D-8547-5B7DB885C65F}_is1',
    'UninstallString', s);
  s := RemoveQuotes(s);
  Log('Legacy uninstaller path: ' + s);
  result := s;
end;

{ vim: set ft=pascal sw=2 sts=2 et : }

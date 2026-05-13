$ErrorActionPreference = "Stop"

if (!(Test-Path .\artifacts\win-x64)) {
  .\scripts\build-windows.ps1
}

New-Item -ItemType Directory -Force -Path .\release | Out-Null
Compress-Archive -Path .\artifacts\win-x64\* -DestinationPath .\release\WinShield-win-x64.zip -Force
Write-Host "Created .\release\WinShield-win-x64.zip"
Write-Host "Production path: add Velopack packaging/signing after code signing certificate and update feed are configured."

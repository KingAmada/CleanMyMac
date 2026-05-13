# Windows Integrations

## Microsoft Defender

`WindowsDefenderService` discovers `MpCmdRun.exe` under Defender Platform directories and supports quick, full, and custom path scans. Output is captured conservatively and surfaced as status text.

Validation on Windows:

```powershell
dotnet test .\WinShield.sln
dotnet run --project .\src\WinShield.App\WinShield.App.csproj
```

Use the Protection page to trigger Defender quick scan and inspect status output.

## AMSI

`WindowsAmsiService` P/Invokes `AmsiInitialize`, `AmsiScanBuffer`, and `AmsiUninitialize`. Non-Windows returns unsupported. AMSI verdicts are represented as provider findings when suspicious.

## Startup

`WindowsStartupProvider` reads HKCU/HKLM Run keys and Startup folder entries. Scheduled task inspection is intentionally left as a production expansion.

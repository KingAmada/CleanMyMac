# WinShield

**Clean. Protect. Accelerate.**

WinShield is a premium Windows desktop cleaner, optimizer, privacy cleaner, and security suite built with .NET 8 and Avalonia. The repository is designed so core product work can happen on macOS while Windows-only integrations are isolated behind platform service interfaces.

## Features

- Avalonia desktop app with dark premium UI, sidebar navigation, Smart Scan dashboard, status cards, and module pages.
- Cleaning engine for temp files, logs, crash dumps, cache-like files, large files, duplicate detection, and safe review-first cleanup inventory.
- Security engine with SHA-256 hashing, metadata inspection, entropy heuristics, JSON detection rules, severity scoring, scan sessions, and structured findings.
- Protection center scaffolding for quick/full/custom scans, browser hijacker indicators, suspicious startup inspection, AMSI handoff, Defender handoff, quarantine, and reporting.
- SQLite persistence for scan sessions, quarantine records, cleanup history, settings-ready schema, and audit-friendly exports.
- Quarantine service that moves files into an app-controlled directory with non-executable extension and stores restore metadata.
- Windows platform project for Microsoft Defender `MpCmdRun.exe`, AMSI P/Invoke, and Registry Run/startup-folder inspection.
- Cross-platform fallbacks so development UI and tests run on macOS.

## Architecture

```text
src/
  WinShield.App                Avalonia UI and MVVM shell
  WinShield.Application        Use cases and orchestration
  WinShield.Domain             Entities, enums, and service contracts
  WinShield.Infrastructure     SQLite, repositories, quarantine, real-time watcher, fallbacks
  WinShield.Platform.Windows   Defender, AMSI, registry startup provider
  WinShield.Security.Engine    Local scanning, rules, hashing, heuristics
  WinShield.Cleaning.Engine    Junk, large-file, and duplicate scanning
  WinShield.Shared             Shared utilities
tests/
docs/
rules/
native/
scripts/
```

## Why .NET 8 and Avalonia

.NET 8 is the conservative LTS choice for a Windows product foundation. Avalonia provides a real desktop UI stack that can be developed from macOS and shipped to Windows without putting business logic inside Windows-only UI code.

## Run on macOS

Install the .NET 8 SDK, then:

```bash
./scripts/test.sh
./scripts/build-mac.sh
```

The UI runs with mock startup data and unsupported Defender/AMSI fallbacks on macOS.

## Build for Windows

On Windows with .NET 8 SDK:

```powershell
.\scripts\build-windows.ps1
.\scripts\package-windows.ps1
```

This creates a self-contained `win-x64` publish folder and a ZIP package. The recommended commercial packaging path is Velopack after code signing, update channels, installer branding, and release signing are configured.

## Security Limitations

WinShield does not claim certified antivirus parity. The custom scanner is a local rules and heuristics engine. Defender and AMSI integrations delegate to installed Microsoft/antimalware providers. Kernel real-time protection is only documented as a future minifilter driver design and is not faked in this repository.

## Roadmap

See [docs/roadmap.md](docs/roadmap.md).

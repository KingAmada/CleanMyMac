# Architecture

WinShield uses clean layers:

- Domain: immutable models and contracts.
- Security.Engine: file enumeration, hashing, rule matching, heuristics, AMSI adapter calls.
- Cleaning.Engine: safe inventory scanning, duplicate detection, large files, cleanup categorization.
- Application: Smart Scan orchestration, report export, use cases.
- Infrastructure: SQLite repositories, quarantine file moves, user-mode file watcher, fallbacks.
- Platform.Windows: Defender, AMSI, Registry Run keys, startup folder.
- App: Avalonia UI and MVVM.

Windows APIs are intentionally behind interfaces so macOS development can continue without conditional compilation across the whole codebase.

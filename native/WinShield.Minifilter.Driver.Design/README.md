# WinShield Minifilter Driver Design

This folder is intentionally a design scaffold, not a fake completed driver.

A commercial real-time antivirus driver requires:

- Windows kernel engineering.
- Signed driver package.
- Microsoft driver attestation/HLK path as applicable.
- Extensive compatibility testing across Windows versions.
- Rollback and recovery strategy.
- User-mode service communication with strict authentication.

The MVP in this repository uses a user-mode `FileSystemWatcher` monitor for selected folders.

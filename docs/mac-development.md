# macOS Development

Install .NET 8 SDK:

```bash
brew install --cask dotnet-sdk
```

Run tests:

```bash
./scripts/test.sh
```

Run the desktop app:

```bash
./scripts/build-mac.sh
```

Expected macOS behavior:

- Avalonia UI runs normally.
- Startup data uses mock entries.
- Defender and AMSI report unavailable.
- File scanning, cleanup scanning, duplicate detection, rules, SQLite, reports, quarantine, and real-time watcher logic are cross-platform.

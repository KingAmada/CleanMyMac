# Security Model

WinShield follows evidence-based security UX:

- Findings include rule id, evidence, score, severity, category, and recommendation.
- Ambiguous detections are labeled suspicious, not confirmed malware.
- Cleanup and quarantine actions require user review.
- Quarantine changes file extension and stores original path/hash metadata in SQLite.
- Defender integration is a wrapper around supported Microsoft command tooling.
- AMSI integration scans content buffers through installed antimalware providers; AMSI is not a standalone antivirus engine.

Production hardening needs signed binaries, tamper protection, service isolation, updater signing, secure rule updates, telemetry privacy review, and rigorous false-positive handling.

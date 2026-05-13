# Threat Detection Design

The local engine is a rule and heuristic scanner:

1. Enumerate files from scan targets.
2. Extract metadata and bounded text previews.
3. Calculate SHA-256 for bounded files.
4. Calculate entropy for bounded files.
5. Match JSON rules by path regex, filename regex, extension, text pattern, hash, and startup context.
6. Add heuristic score for temp executable locations, missing trusted publisher metadata, high entropy, and startup context.
7. Map score to severity.
8. Store scan sessions and surface findings with explanation text.

Rules live in `/rules`. They are safe examples and intentionally do not contain real malware content.

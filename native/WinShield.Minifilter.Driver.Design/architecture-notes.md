# Architecture Notes

Future design:

- Kernel minifilter observes file create/write/rename/execute flows.
- User-mode WinShield service receives normalized events over a secure communication port.
- Service performs policy decisions and scanning using local engine, Defender/AMSI delegation, and cloud/rule updates.
- Driver must fail open for system stability unless enterprise policy explicitly chooses otherwise.
- Driver must never implement UI or high-level product logic.

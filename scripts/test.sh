#!/usr/bin/env bash
set -euo pipefail

dotnet test WinShield.sln -c Debug --logger "console;verbosity=normal"

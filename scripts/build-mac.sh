#!/usr/bin/env bash
set -euo pipefail

dotnet restore WinShield.sln
dotnet build WinShield.sln -c Debug
dotnet run --project src/WinShield.App/WinShield.App.csproj

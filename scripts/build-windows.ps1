$ErrorActionPreference = "Stop"

dotnet restore .\WinShield.sln
dotnet build .\WinShield.sln -c Release
dotnet publish .\src\WinShield.App\WinShield.App.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\win-x64

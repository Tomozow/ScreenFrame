# Suggested Commands
- Build Debug: `dotnet build` (run in repo root).
- Run Debug: `dotnet run` (starts overlay window).
- Publish Release: `dotnet publish -c Release -r win-x64 --self-contained` (creates standalone binaries; outputs under `bin/Release/net6.0-windows/win-x64/publish`).
- Clean: `dotnet clean`.
- Optional icon generation: `python create_icon.py` (requires Python + Pillow if installed).
- Git basics (Windows terminal): `git status`, `git diff`, `git commit`.
- File inspection: `Get-ChildItem`, `Get-Content` in PowerShell if not using Serena tools.
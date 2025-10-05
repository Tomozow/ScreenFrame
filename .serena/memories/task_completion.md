# Task Completion Checklist
- Ensure changes compile via `dotnet build`.
- If behavior affects overlay interaction, run the app with `dotnet run` for manual verification (drag, resize, tray menu, settings persistence).
- Update documentation (`README.md`) if user-visible behavior or shortcuts change.
- Include/refresh release artifacts in `release` folders only when explicitly requested.
- Confirm no unintended binary artifacts are introduced; keep icons/scripts synced.
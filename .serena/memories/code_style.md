# Coding Style
- Language: C# targeting .NET 6.0 WPF; nullable reference types enabled.
- Naming: Standard .NET PascalCase for types/methods/properties, camelCase with leading underscore for private fields.
- UI definitions in XAML; code-behind handles interaction logic and Win32 interop (P/Invoke).
- Uses explicit event wiring in constructors and helper methods; minimal comments except for Japanese inline notes.
- Persistence: JSON serialization via `System.Text.Json` with camelCase property names and `[JsonIgnore]` for computed members.
- Win32 interop constants and structs defined locally within classes; uses `DllImport` attributes.
- Keep resources embedded via `.csproj` definitions (`app.ico`).
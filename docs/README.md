# Bosch Media Browser - Developer Documentation

## Build Instructions

### Prerequisites
- Visual Studio 2022 or VS Code with C# extension
- .NET 7 SDK
- Rhino 8 for Windows

### Building the Solution

```bash
# Restore NuGet packages
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Build release version
dotnet build -c Release
```

### Project Structure

```
BoschMediaBrowser.sln
├── src/
│   ├── BoschMediaBrowser.Core/      # Business logic (no Rhino dependencies)
│   │   ├── Models/                   # Data entities
│   │   └── Services/                 # Core services (data, search, thumbnails)
│   └── BoschMediaBrowser.Rhino/     # Rhino plugin
│       ├── Commands/                 # Rhino commands
│       ├── Services/                 # Rhino-specific services (insert, layers)
│       └── UI/                       # Eto.Forms UI
└── tests/
    └── BoschMediaBrowser.Tests/     # Unit tests
```

### Installing the Plugin in Rhino

1. Build the solution in Release mode
2. Locate the `.rhp` file in `src/BoschMediaBrowser.Rhino/bin/Release/net7.0/`
3. Open Rhino 8
4. Run command: `PlugInManager`
5. Click "Install" and browse to the `.rhp` file
6. Restart Rhino
7. Run command: `ShowMediaBrowser` to open the panel

### Development Workflow

1. **Core First**: Implement models and services in `BoschMediaBrowser.Core`
2. **Test**: Write unit tests for Core services
3. **Plugin**: Build Rhino plugin UI and commands
4. **Iterate**: Test in Rhino, refine, repeat

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

### Debugging in Rhino

1. Set `BoschMediaBrowser.Rhino` as startup project
2. Project Properties → Debug → Launch → Executable
3. Set executable to Rhino 8 path: `C:\Program Files\Rhino 8\System\Rhino.exe`
4. F5 to start debugging (Rhino will launch)
5. In Rhino, run: `ShowMediaBrowser`
6. Breakpoints in Visual Studio will hit

### Key Dependencies

- **RhinoCommon** (8.0.23304.9001): Rhino SDK
- **Eto.Forms** (2.7.4): Cross-platform UI framework
- **System.Text.Json** (7.0.3): JSON serialization
- **xUnit** (2.5.0): Testing framework

### Troubleshooting

**Build Errors:**
- Ensure .NET 7 SDK is installed: `dotnet --version`
- Restore packages: `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`

**Plugin Not Loading:**
- Check Rhino version (must be Rhino 8)
- Verify .rhp is built for correct platform (Any CPU)
- Check Rhino console for error messages

**Tests Failing:**
- Ensure test data paths are correct
- Check for missing dependencies

---

**See Also:**
- [User Settings](./SETTINGS.md)
- [Architecture](../BoschMediaBrowserSpec/specs/001-rhino-media-browser/ARCHITECTURE.md)
- [Task List](../BoschMediaBrowserSpec/specs/001-rhino-media-browser/tasks.md)

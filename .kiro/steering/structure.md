# Project Structure & Organization

## Root Structure
```
GitHubify/
├── Program.cs              # Main entry point with CLI setup and command handling
├── GitHubify.csproj        # Project file with dependencies and build config
├── GitHubify.sln           # Solution file
├── README.md               # Project documentation
├── LICENSE.txt             # MIT license
├── .gitignore              # Git ignore patterns
├── .gitattributes          # Git attributes
└── Utilities/              # Core business logic and utilities
    ├── Config.cs           # Configuration model with JSON serialization
    └── MarkdownService.cs  # GitHub API integration and HTML generation
```

## Architecture Patterns

### Separation of Concerns
- **Program.cs**: CLI interface, command parsing, user interaction, and orchestration
- **Utilities/Config.cs**: Configuration management, validation, and path resolution
- **Utilities/MarkdownService.cs**: External API calls and HTML generation logic

### Configuration Management
- Uses record-like classes with JSON source generators for performance
- Supports both CLI arguments and `config.json` file
- CLI arguments override config file settings
- Path resolution and validation centralized in Config class

### Error Handling
- Specific exit codes for different error types:
  - `0`: Success
  - `1`: User cancellation
  - `2`: File not found
  - `3`: Configuration error
  - `4`: Network/API error
  - `-1`: Unexpected error

### Naming Conventions
- **Enums**: PascalCase (`Theme.Light`, `CssMode.CDN`)
- **Properties**: PascalCase with JsonPropertyName attributes for snake_case JSON
- **Methods**: PascalCase with descriptive names (`ConvertMarkdownToHtmlAsync`)
- **Constants**: PascalCase for readability (`GitHubApiUrl`)
- **Files**: PascalCase for C# files, following .NET conventions

### Code Organization
- Static HttpClient for efficient reuse
- Async/await throughout for non-blocking operations
- Nullable reference types enabled with proper null handling
- Source generators for JSON serialization performance
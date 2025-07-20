# Technology Stack & Build System

## Tech Stack
- **Runtime**: .NET 9.0
- **Language**: C# 13.0 with nullable reference types enabled
- **CLI Framework**: System.CommandLine (2.0.0-beta4.22272.1)
- **UI Library**: Spectre.Console (0.50.0) for interactive CLI
- **Serialization**: System.Text.Json with source generators
- **HTTP Client**: Built-in HttpClient for GitHub API calls

## Project Configuration
- **AOT Compilation**: Enabled (`PublishAot=true`)
- **Globalization**: Invariant (`InvariantGlobalization=true`)
- **Implicit Usings**: Enabled
- **Nullable**: Enabled throughout the project

## Common Commands

### Development
```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Run with arguments
dotnet run -- --input README.md --theme Dark
```

### Publishing
```bash
# Standard publish
dotnet publish -c Release

# AOT self-contained publish (Windows x64)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishAot=true

# Global tool installation (when packaged)
dotnet tool install --global GitHubify
```

### Testing
```bash
# Run tests (if test project exists)
dotnet test
```

## External Dependencies
- **GitHub Markdown API**: `https://api.github.com/markdown/raw`
- **GitHub CSS CDN**: Cloudflare CDN for github-markdown-css
  - Light: `https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.8.1/github-markdown-light.min.css`
  - Dark: `https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.8.1/github-markdown-dark.min.css`
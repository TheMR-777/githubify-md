---
inclusion: always
---

# 🎯 Core Principles & Learning System

## User Preferences
- **Minimalism**: Zero boilerplate, every character has deep purpose - code as artpiece
- **Perfection**: Pixel-perfect results, masterpiece-level implementations that inspire awe
- **Artistic Excellence**: Sleek, magnificent code that makes other devs wonder and compliment
- **Performance**: AOT-optimized, fast startup, efficient memory usage
- **Reliability**: Robust error handling with specific exit codes
- **Usability**: Intuitive CLI with both interactive and scripted modes
- **Modernization**: Latest .NET 9/C# 13 features and patterns
- **Standards**: Consistent naming, proper async/await, nullable types
- **Solutions**: Elegant, maintainable, and extensible code
- **User Experience**: Beautiful CLI with Spectre.Console, clear feedback

## 🧠 Learning System

### Experience Accumulation
Each session builds upon previous learnings:
- **CLI Patterns → Reusable Components**: Convert UI interactions into modular patterns
- **Error Scenarios → Robust Handling**: Transform failures into comprehensive error strategies
- **Configuration Evolution**: Refine config management based on real usage
- **API Integration Wisdom**: Extract best practices from external service interactions

### Continuous Improvement Loop
```
Problem → Solution → Pattern → Documentation → Refinement
```

## 🎨 Code as Artpiece Philosophy

### The Perfectionist's Creed
- **Every Character Counts**: No throwaway code, no "good enough" - only masterpiece-worthy implementations
- **Inspire Wonder**: Code so elegant that other developers stop to admire and learn from it
- **Zero Tolerance for Bloat**: If it doesn't serve a deep purpose, it doesn't belong
- **Pixel-Perfect Metaphor**: Every detail refined to perfection, like a master craftsman's work

### Artistic Excellence Patterns
```csharp
// ✨ Artpiece: Concise, purposeful, beautiful
var cssUrl = theme switch
{
    Theme.Light => GitHubCssCdnLight,
    Theme.Dark => GitHubCssCdnDark,
    _ => throw new ArgumentOutOfRangeException(nameof(theme))
};

// ✨ Artpiece: Raw string interpolation - clean, readable, purposeful
return $"""
    <!DOCTYPE html>
    <html lang="en">
    <head><title>{title}</title>{cssSection}</head>
    <body class="markdown-body">{htmlFragment}</body>
    </html>
    """;

// ✨ Artpiece: Null-coalescing with meaningful exception
ResolvedInputFile = cliInputFile ?? InputFile ?? 
    throw new ArgumentNullException(nameof(InputFile));
```

### Anti-Patterns (Code Pollution)
```csharp
// 💩 Bloated: Unnecessary verbosity
if (theme == Theme.Light)
{
    cssUrl = GitHubCssCdnLight;
}
else if (theme == Theme.Dark)
{
    cssUrl = GitHubCssCdnDark;
}
else
{
    throw new ArgumentOutOfRangeException(nameof(theme));
}

// 💩 Bloated: String concatenation mess
var html = "<!DOCTYPE html><html><head><title>" + title + "</title>" + cssSection + "</head><body>" + htmlFragment + "</body></html>";

// 💩 Bloated: Nested if-else chains
if (cliInputFile != null)
{
    ResolvedInputFile = cliInputFile;
}
else
{
    if (InputFile != null)
    {
        ResolvedInputFile = InputFile;
    }
    else
    {
        throw new ArgumentNullException(nameof(InputFile));
    }
}
```

## 🚀 Modern C# 13/.NET 9 Essentials

### File-Scoped Namespaces
```csharp
namespace GitHubify.Utilities;

public class Config
{
    // Clean, concise namespace declaration
}
```

### Nullable Reference Types
```csharp
public string? InputFile { get; set; }  // Explicit nullable
public string ResolvedInputFile { get; set; } = string.Empty;  // Non-null with default
```

### Source Generators (JSON)
```csharp
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Config))]
internal partial class ConfigContext : JsonSerializerContext;

// Usage
var config = JsonSerializer.Deserialize(json, ConfigContext.Default.Config);
```

### Pattern Matching & Switch Expressions
```csharp
var cssUrl = theme switch
{
    Theme.Light => GitHubCssCdnLight,
    Theme.Dark => GitHubCssCdnDark,
    _ => throw new ArgumentOutOfRangeException(nameof(theme))
};
```

### String Interpolation (Raw Strings)
```csharp
return $"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <title>{title}</title>
        {cssSection}
    </head>
    <body class="markdown-body">
        {htmlFragment}
    </body>
    </html>
    """;
```

## 🏗️ Architecture Patterns

### Configuration Strategy
```csharp
// Dual resolution: CLI args override config file
public void ResolvePaths(string? cliInputFile = null)
{
    ResolvedInputFile = cliInputFile ?? InputFile ?? 
        throw new ArgumentNullException(nameof(InputFile));
    
    if (!File.Exists(ResolvedInputFile))
        throw new FileNotFoundException($"Input file not found: {ResolvedInputFile}");
        
    ResolvedOutputFile = OutputFile ?? Path.ChangeExtension(ResolvedInputFile, ".html");
}
```

### HTTP Client Management
```csharp
// Static HttpClient for lifetime management
static readonly HttpClient HttpClient = new();

// Proper disposal in using statements for requests
using var request = new HttpRequestMessage(HttpMethod.Post, GitHubApiUrl);
using var response = await HttpClient.SendAsync(request);
```

### Error Handling with Exit Codes
```csharp
try
{
    // Main logic
    context.ExitCode = 0;
}
catch (FileNotFoundException)
{
    context.ExitCode = 2; // Specific exit code for file not found
}
catch (ArgumentException)
{
    context.ExitCode = 3; // Configuration errors
}
catch (HttpRequestException)
{
    context.ExitCode = 4; // Network/API errors
}
```

### CLI User Experience
```csharp
// Beautiful status updates
await AnsiConsole.Status()
    .Spinner(Spinner.Known.SimpleDotsScrolling)
    .SpinnerStyle(Style.Parse("cyan"))
    .StartAsync("[yellow]Processing...[/]", async ctx =>
    {
        ctx.Status("Reading [blue]file.md[/]...");
        // Work here
    });
```

## ⚠️ Critical Pitfalls

### AOT Compilation Issues
```csharp
// ❌ Reflection-heavy JSON serialization
JsonSerializer.Deserialize<Config>(json);

// ✅ Source generators for AOT compatibility
JsonSerializer.Deserialize(json, ConfigContext.Default.Config);
```

### HttpClient Lifecycle
```csharp
// ❌ Creating HttpClient per request
using var client = new HttpClient();

// ✅ Static HttpClient with proper request disposal
static readonly HttpClient HttpClient = new();
using var request = new HttpRequestMessage();
```

### Path Handling
```csharp
// ❌ Assuming file extensions
var output = input.Replace(".md", ".html");

// ✅ Proper path manipulation
var output = Path.ChangeExtension(input, ".html");
```

### Async/Await Patterns
```csharp
// ❌ Blocking async calls
var result = SomeAsyncMethod().Result;

// ✅ Proper async all the way
var result = await SomeAsyncMethod();
```

### CLI Argument Validation
```csharp
// ❌ Manual validation everywhere
if (string.IsNullOrEmpty(input)) throw new ArgumentException();

// ✅ Centralized validation in config resolution
public void ResolvePaths(string? cliInputFile = null)
{
    ResolvedInputFile = cliInputFile ?? InputFile ?? 
        throw new ArgumentNullException(nameof(InputFile));
}
```

## 🧪 Testing Essentials

### Configuration Testing
```csharp
[Test]
public void ResolvePaths_CliOverridesConfig()
{
    var config = new Config { InputFile = "config.md" };
    config.ResolvePaths("cli.md");
    
    Assert.That(config.ResolvedInputFile, Is.EqualTo("cli.md"));
}
```

### HTTP Service Testing
```csharp
[Test]
public async Task ConvertMarkdown_ValidInput_ReturnsHtml()
{
    var markdown = "# Test";
    var html = await MarkdownService.ConvertMarkdownToHtmlAsync(markdown);
    
    Assert.That(html, Contains.Substring("<h1>"));
}
```

### CLI Integration Testing
```csharp
[Test]
public async Task Program_ValidArgs_ExitsWithZero()
{
    var args = new[] { "--input", "test.md", "--yes" };
    var exitCode = await Program.Main(args);
    
    Assert.That(exitCode, Is.EqualTo(0));
}
```

## 📋 Implementation Checklist

### CLI Enhancement
- [ ] Add comprehensive help text
- [ ] Implement progress indicators for long operations
- [ ] Support batch processing modes
- [ ] Add configuration validation commands

### Error Handling
- [ ] Specific error messages for each failure scenario
- [ ] Graceful degradation for network issues
- [ ] Retry logic for transient failures
- [ ] Detailed logging for debugging

### Performance
- [ ] AOT compilation optimization
- [ ] Memory usage profiling
- [ ] Startup time measurement
- [ ] Large file handling tests

### User Experience
- [ ] Interactive file selection
- [ ] Configuration file generation
- [ ] Output preview options
- [ ] Theme switching commands

## 🎯 Success Metrics
- ✅ Sub-second startup time with AOT
- ✅ Zero breaking changes in CLI interface
- ✅ Comprehensive error handling with specific exit codes
- ✅ Beautiful, intuitive user experience
- ✅ Robust configuration management
- ✅ Efficient HTTP client usage

---

**Learning Principle**: Every CLI interaction teaches us about user needs, every error scenario strengthens our resilience, every performance optimization compounds our efficiency. This document evolves with each session, becoming more refined and powerful.

---
# GitHubify-MD

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download)
[![C#](https://img.shields.io/badge/C%23-13.0-239120)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

> 🚀 Modern and elegant Markdown to GitHub-styled HTML converter

GitHubify transforms your Markdown files into beautifully rendered GitHub-styled HTML pages with just a few keystrokes. Built with modern C# 13 and .NET 9, it offers a seamless conversion experience through an intuitive command-line interface.

<!-- ![GitHubify Demo](https://will-be-available-soon) -->

## ✨ Features

- 📄 Convert Markdown files to GitHub-styled HTML with perfect rendering
- 🌓 Support for both **Light** and **Dark** themes
- 🔌 Flexible CSS delivery via CDN links or embedded directly in the HTML
- ⚙️ Configurable through command-line options or JSON config file
- 🖥️ Beautiful interactive CLI interface built with Spectre.Console
- ⚡ AOT (Ahead-of-Time) compilation support for lightning-fast startup
- 📦 Self-contained, single executable deployment option

## 🚀 Installation

### Via .NET CLI

```bash
dotnet tool install --global GitHubify
```

### Manual Installation

1. Download the latest release from the [Releases](https://github.com/TheMR-777/githubify-md/releases) page (*available soon...*)
2. Extract the archive to a location of your choice
3. Add the directory to your PATH (optional)

## 🔧 Usage

### Basic Usage

Convert a Markdown file with default settings (Light theme, CDN CSS):

```bash
githubify --input README.md
```

This will generate `README.html` in the same directory.

### Interactive Mode

Run GitHubify without arguments for an interactive experience:

```bash
githubify
```

The tool will guide you through selecting:

- Input file path
- Theme preference
- CSS delivery method
- Output file location

### Command-line Options

```bash
# Specify both input and output files
githubify --input README.md --output docs/index.html

# Use dark theme
githubify --input README.md --theme Dark

# Embed CSS for offline viewing
githubify --input README.md --css-mode Embed

# Non-interactive mode with default settings
githubify --input README.md --yes
```

### Full Options Reference

| Option | Alias | Description |
|--------|-------|-------------|
| `--input` | `-i` | Path to the input Markdown (.md) file |
| `--output` | `-o` | Path for the output HTML file (defaults to `<input_name>.html`) |
| `--theme` | | Specify the color theme (`Light` or `Dark`) |
| `--css-mode` | | CSS delivery method (`CDN` or `Embed`) |
| `--yes` | | Skip interactive prompts and use defaults |
| `--help` | `-h` | Display help information |

## ⚙️ Configuration

GitHubify can use a `config.json` file in the current directory for default settings:

```json
{
  "input_file": "path/to/default.md",
  "theme": "Dark",
  "css_mode": "Embed",
  "output_file": "path/to/output.html"
}
```

Command-line options will override settings from the config file.

## 📋 Examples

### Generate an offline-viewable HTML with dark theme

```bash
githubify --input documentation.md --theme Dark --css-mode Embed
```

### Batch convert multiple files (using shell features)

```bash
# Bash example
for file in docs/*.md; do
  githubify --input "$file" --theme Light --yes
done
```

```powershell
# PowerShell example
Get-ChildItem -Path docs -Filter *.md | ForEach-Object {
  githubify --input $_.FullName --theme Light --yes
}
```

## 🔍 How It Works

1. GitHubify reads your Markdown file
2. Sends the content to GitHub's Markdown API for rendering
3. Applies the GitHub CSS styling (via CDN or embedded)
4. Generates a standalone HTML file with responsive layout
5. The resulting HTML looks exactly like GitHub's rendered Markdown

## 🛠️ Building from Source

Prerequisites:

- .NET 9 SDK

```bash
git clone https://github.com/yourusername/githubify.git
cd githubify
dotnet build
dotnet run
```

### AOT Compilation

GitHubify supports AOT compilation for improved startup performance:

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishAot=true
```

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Spectre.Console](https://spectreconsole.net/) for the beautiful CLI interface
- [GitHub Markdown API](https://docs.github.com/en/rest/reference/markdown) for the Markdown rendering
- [github-markdown-css](https://github.com/sindresorhus/github-markdown-css) for the GitHub styling

---

<p align="center">Made with ❤️ by <a href="https://github.com/TheMR-777">TheMR-777</a> :)</p>

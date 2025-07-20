using Spectre.Console;
using System.CommandLine;
using GitHubify.Utilities;

// ✨ Artpiece: Concise option definitions with meaningful defaults
var rootCommand = new RootCommand("Converts a Markdown file to a GitHub-styled HTML page.");

var inputOption = new Option<FileInfo?>(["--input", "-i"], "Path to the input Markdown (.md) file.");
var themeOption = new Option<Theme?>("--theme", "Specify the color theme (Light/Dark).");
var cssModeOption = new Option<CssMode?>("--css-mode", "Specify CSS delivery method (CDN/Embed).");
var outputOption = new Option<FileInfo?>(["--output", "-o"], "Path for the output HTML file (optional, defaults to <input_name>.html).");
var yesOption = new Option<bool>(["--yes", "-y"], "Skip interactive prompts and use defaults or provided options.");

rootCommand.AddOption(inputOption);
rootCommand.AddOption(themeOption);
rootCommand.AddOption(cssModeOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(yesOption);

rootCommand.SetHandler(async (input, theme, cssMode, output, yes) =>
{
    Environment.ExitCode = await CliHandler.ExecuteAsync(input, theme, cssMode, output, yes);
}, inputOption, themeOption, cssModeOption, outputOption, yesOption);

return await rootCommand.InvokeAsync(args);
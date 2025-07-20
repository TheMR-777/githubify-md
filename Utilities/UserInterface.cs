namespace GitHubify.Utilities;

using Spectre.Console;

internal static class UserInterface
{
    public static void ShowWelcome(bool skipPrompts)
    {
        if (skipPrompts) return;
        
        AnsiConsole.Clear();
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText("MD >> HTML").Centered().Color(Color.Cyan1));
        AnsiConsole.WriteLine();
    }

    public static bool ConfirmConfigFile(string configPath) =>
        AnsiConsole.Confirm($"[yellow]Configuration file '{configPath}' found. Use settings from this file?[/]");

    public static void ShowConfigLoaded() =>
        AnsiConsole.MarkupLine("[green]Loaded settings from config.json.[/]");

    public static void ShowConfigError(string configPath, string error)
    {
        AnsiConsole.MarkupLine($"[red]Error reading or parsing {configPath}: {error}[/]");
        AnsiConsole.MarkupLine("[yellow]Proceeding with manual configuration.[/]");
    }

    public static void ShowConfigIgnored() =>
        AnsiConsole.MarkupLine("[yellow]Ignoring config file. Proceeding with manual configuration.[/]");

    public static string PromptForInputFile() =>
        AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Enter the path to the Markdown file (.md):[/]")
                .Validate(path =>
                {
                    var trimmedPath = path.Trim('"');
                    return !string.IsNullOrWhiteSpace(trimmedPath) && File.Exists(trimmedPath)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Invalid path. Must be an existing file.[/]");
                })
        ).Trim('"');

    public static Theme PromptForTheme() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<Theme>()
                .Title("[green]Select the theme?[/]")
                .AddChoices(Enum.GetValues<Theme>())
                .UseConverter(t => t.ToString()));

    public static CssMode PromptForCssMode() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<CssMode>()
                .Title("[green]How to include GitHub CSS?[/]")
                .AddChoices(Enum.GetValues<CssMode>())
                .UseConverter(m => m switch
                {
                    CssMode.CDN => "CDN Link (Requires internet to view)",
                    CssMode.Embed => "Embed CSS (Downloads now, works offline)",
                    _ => m.ToString()
                }));

    public static void ShowDefaultUsed<T>(T value, string setting) =>
        AnsiConsole.MarkupLine($"[grey]No {setting} specified, defaulting to {value}.[/]");

    public static ConfigurationAction ConfirmExecution(Config config)
    {
        while (true)
        {
            // ✨ Artpiece: Clear screen for clean UI on each iteration
            AnsiConsole.Clear();
            ShowConfigurationTable(config);
            
            var action = PromptForConfigurationAction();
            
            if (action == ConfigurationAction.Proceed || action == ConfigurationAction.Cancel)
                return action;
            
            // Handle configuration changes with clean feedback
            ShowActionFeedback(action);
            
            return action;
        }
    }

    private static void ShowConfigurationTable(Config config)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText("Configuration").Centered().Color(Color.Cyan1));
        AnsiConsole.WriteLine();

        // ✨ Artpiece: Redesigned table with visually balanced columns
        var summary = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Title("[underline cyan]Current Settings[/]")
            .AddColumn(new TableColumn("[yellow]#[/]").Centered().Width(1)) // ✨ Minimal width for perfect balance
            .AddColumn(new TableColumn("[yellow]Setting[/]").LeftAligned().PadRight(2))
            .AddColumn(new TableColumn("[yellow]Value[/]").LeftAligned())
            .Expand()
            .AddRow("[cyan]1[/]", "[bold]Input File[/]", $"[blue]{Markup.Escape(Path.GetFileName(config.ResolvedInputFile))}[/]")
            .AddRow("[cyan]2[/]", "[bold]Output File[/]", $"[green]{Markup.Escape(Path.GetFileName(config.ResolvedOutputFile))}[/]")
            .AddRow("[cyan]3[/]", "[bold]Theme[/]", $"[violet]{config.Theme}[/]")
            .AddRow("[cyan]4[/]", "[bold]CSS Mode[/]", $"[orange3]{config.CssMode}[/]");

        AnsiConsole.Write(summary);
        AnsiConsole.WriteLine();
    }

    private static ConfigurationAction PromptForConfigurationAction()
    {
        // ✨ Artpiece: Store cursor position to enable clean section replacement
        var actionPromptStartLine = AnsiConsole.Console.Profile.Height - AnsiConsole.Console.Profile.Height + Console.CursorTop;
        
        ShowActionPrompt();

        while (true)
        {
            var keyInfo = AnsiConsole.Console.Input.ReadKey(true);
            if (!keyInfo.HasValue) continue;
            
            var key = keyInfo.Value.Key;
            var action = key switch
            {
                ConsoleKey.D1 or ConsoleKey.NumPad1 => ConfigurationAction.ChangeInputFile,
                ConsoleKey.D2 or ConsoleKey.NumPad2 => ConfigurationAction.ChangeOutputFile,
                ConsoleKey.D3 or ConsoleKey.NumPad3 => ConfigurationAction.ChangeTheme,
                ConsoleKey.D4 or ConsoleKey.NumPad4 => ConfigurationAction.ChangeCssMode,
                ConsoleKey.Enter => ConfigurationAction.Proceed,
                ConsoleKey.Escape => ConfigurationAction.Cancel,
                _ => (ConfigurationAction?)null
            };

            if (action.HasValue)
            {
                // ✨ Artpiece: Clear the action prompt section for clean UI
                ClearActionPromptSection();
                return action.Value;
            }
            
            // Invalid key - show error and continue
            AnsiConsole.MarkupLine("[red]>> Invalid key! Please use 1-4, Enter, or Esc[/]");
        }
    }

    private static void ShowActionPrompt()
    {
        // ✨ Artpiece: Clean action prompt display
        AnsiConsole.MarkupLine("[yellow]>> Choose an action:[/]");
        AnsiConsole.MarkupLine("[dim]   Press [cyan]1-4[/] to change settings[/]");
        AnsiConsole.MarkupLine("[dim]   Press [cyan]Enter[/] to proceed with conversion[/]");
        AnsiConsole.MarkupLine("[dim]   Press [cyan]Esc[/] to cancel[/]");
        AnsiConsole.WriteLine();
    }

    private static void ClearActionPromptSection()
    {
        // ✨ Artpiece: Clear the action prompt section (5 lines) for clean replacement
        Console.SetCursorPosition(0, Console.CursorTop - 5);
        for (int i = 0; i < 5; i++)
        {
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.WriteLine();
        }
        Console.SetCursorPosition(0, Console.CursorTop - 5);
    }

    private static void ShowActionFeedback(ConfigurationAction action)
    {
        // ✨ Artpiece: Clean feedback with proper spacing and Windows Terminal friendly symbols
        AnsiConsole.WriteLine();
        var feedback = action switch
        {
            ConfigurationAction.ChangeInputFile => "[cyan]>> Changing input file...[/]",
            ConfigurationAction.ChangeOutputFile => "[cyan]>> Changing output file...[/]",
            ConfigurationAction.ChangeTheme => "[cyan]>> Changing theme...[/]",
            ConfigurationAction.ChangeCssMode => "[cyan]>> Changing CSS mode...[/]",
            _ => ""
        };
        
        if (!string.IsNullOrEmpty(feedback))
        {
            AnsiConsole.MarkupLine(feedback);
            AnsiConsole.WriteLine(); // ✨ Artpiece: Extra spacing for clean separation
        }
    }

    public enum ConfigurationAction
    {
        Proceed,
        ChangeInputFile,
        ChangeOutputFile,
        ChangeTheme,
        ChangeCssMode,
        Cancel
    }

    public static void ShowSuccess(string outputPath, bool skipPrompts)
    {
        AnsiConsole.MarkupLine($"[bold green]Success![/] HTML file generated at: [underline cyan]{outputPath}[/]");
        AnsiConsole.WriteLine();
    }

    public static PostConversionAction ShowPostConversionOptions() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<PostConversionAction>()
                .Title("[yellow]>> Choose your next action:[/]")
                .AddChoices(
                    PostConversionAction.Exit,
                    PostConversionAction.ConvertAnother,
                    PostConversionAction.OpenOutput)
                .UseConverter(action => action switch
                {
                    PostConversionAction.ConvertAnother => "Convert another file",
                    PostConversionAction.OpenOutput => "Open output file",
                    PostConversionAction.Exit => "Exit application",
                    _ => action.ToString()
                }));

    public enum PostConversionAction
    {
        ConvertAnother,
        OpenOutput,
        Exit
    }

    public static string PromptForOutputFile(string defaultPath) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>($"[green]Enter the output file path:[/]")
                .DefaultValue(defaultPath)
                .Validate(path =>
                {
                    var trimmedPath = path.Trim('"');
                    if (string.IsNullOrWhiteSpace(trimmedPath))
                        return ValidationResult.Error("[red]Output path cannot be empty.[/]");
                    
                    var directory = Path.GetDirectoryName(trimmedPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        try
                        {
                            Directory.CreateDirectory(directory);
                            return ValidationResult.Success();
                        }
                        catch
                        {
                            return ValidationResult.Error("[red]Cannot create output directory.[/]");
                        }
                    }
                    return ValidationResult.Success();
                })
        ).Trim('"');

    public static void ShowGoodbye()
    {
        AnsiConsole.WriteLine();
        var goodbye = new FigletText("Goodbye!")
            .Centered()
            .Color(Color.Cyan1);
        
        AnsiConsole.Write(goodbye);
        AnsiConsole.MarkupLine("[dim]Thanks for using GitHubify! Have a great day![/]");
        AnsiConsole.WriteLine();
    }

    public static void OpenFileInDefaultApp(string filePath)
    {
        try
        {
            AnsiConsole.MarkupLine($"[green]Opening [cyan]{Path.GetFileName(filePath)}[/] in your default browser...[/]");
            
            if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                System.Diagnostics.Process.Start("open", filePath);
            }
            else if (OperatingSystem.IsLinux())
            {
                System.Diagnostics.Process.Start("xdg-open", filePath);
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Cannot auto-open file on this platform. Please open manually.[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Could not open file automatically: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[dim]You can manually open: {filePath}[/]");
        }
    }

    public static void ShowError(string title, string message, string? details = null, string? innerDetails = null)
    {
        AnsiConsole.MarkupLine($"[red]Error: {title}[/]");
        AnsiConsole.MarkupLine($"[red]{message}[/]");
        if (details != null) AnsiConsole.MarkupLine($"[red]{details}[/]");
        if (innerDetails != null) AnsiConsole.MarkupLine($"[red]Inner: {innerDetails}[/]");
    }

    public static void ShowUnexpectedError(Exception ex)
    {
        AnsiConsole.MarkupLine("[bold red]An unexpected error occurred![/]");
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
        if (ex.StackTrace != null)
        {
            AnsiConsole.MarkupLine("[grey]Stack Trace:[/]");
            AnsiConsole.WriteLine(ex.StackTrace);
        }
    }
}
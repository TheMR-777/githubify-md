using Spectre.Console;
using System.Text.Json;
using System.CommandLine;
using GitHubify.Utilities;

var rootCommand = new RootCommand("Converts a Markdown file to a GitHub-styled HTML page.");

// --- Define Command Line Options ---
var inputOption = new Option<FileInfo?>(
	aliases: ["--input", "-i"], // Use FileInfo for built-in validation
	description: "Path to the input Markdown (.md) file.")
{
	IsRequired = false // Not strictly required if config file or prompt is used
};


var themeOption = new Option<Theme?>( // Nullable for explicit detection
	name: "--theme",
	description: "Specify the color theme (Light/Dark).");

var cssModeOption = new Option<CssMode?>( // Nullable for explicit detection
	name: "--css-mode",
	description: "Specify CSS delivery method (CDN/Embed).");

var outputOption = new Option<FileInfo?>(
	aliases: ["--output", "-o"],
	description: "Path for the output HTML file (optional, defaults to <input_name>.html).")
{
	IsRequired = false
};

var yesOption = new Option<bool>( // For non-interactive mode / scripting
	aliases: ["--yes", "-y"],
	description: "Skip interactive prompts and use defaults or provided options.",
	getDefaultValue: () => false);


// --- Add Options to Root Command ---
rootCommand.AddOption(inputOption);
rootCommand.AddOption(themeOption);
rootCommand.AddOption(cssModeOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(yesOption);

// --- Set the Handler for the Command ---
rootCommand.SetHandler(async context =>
{
	// Get option values from the context
	var inputFileInfo = context.ParseResult.GetValueForOption(inputOption);
	var themeChoice = context.ParseResult.GetValueForOption(themeOption);
	var cssModeChoice = context.ParseResult.GetValueForOption(cssModeOption);
	var outputFileInfo = context.ParseResult.GetValueForOption(outputOption);
	var skipPrompts = context.ParseResult.GetValueForOption(yesOption);

	// Access the service (could use DI later if it grows)
	var config = new Config();
	var configFilePath = "config.json";
	var useConfigFile = false;

	if (!skipPrompts)
		AnsiConsole.Clear();

	// --- Configuration Loading / Interactive Setup ---
	AnsiConsole.WriteLine('\n');
	AnsiConsole.Write(new FigletText("MD >> HTML").Centered().Color(Color.Cyan1));
	AnsiConsole.WriteLine();

	// 1. Config File Check
	if (File.Exists(configFilePath) && !skipPrompts)
	{
		if (AnsiConsole.Confirm($"[yellow]Configuration file '{configFilePath}' found. Use settings from this file?[/]"))
		{
			useConfigFile = true;
			try
			{
				var json = await File.ReadAllTextAsync(configFilePath);
				config = JsonSerializer.Deserialize(json, ConfigContext.Default.Config) ?? throw new InvalidOperationException("Failed to deserialize config file.");
				AnsiConsole.MarkupLine("[green]Loaded settings from config.json.[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Error reading or parsing {configFilePath}: {ex.Message}[/]");
				AnsiConsole.MarkupLine("[yellow]Proceeding with manual configuration.[/]");
				useConfigFile = false;
				config = new(); // Reset config
			}
		}
		else
		{
			AnsiConsole.MarkupLine("[yellow]Ignoring config file. Proceeding with manual configuration.[/]");
		}
	}

	try
	{
		// 2. Determine Input File
		if (string.IsNullOrWhiteSpace(config.InputFile) && inputFileInfo == null && !useConfigFile)
		{
			if (skipPrompts) throw new InvalidOperationException("Input file must be specified via --input or config.json in non-interactive mode.");
			config.InputFile = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Enter the path to the Markdown file (.md):[/]")
					.Validate(path =>
					{
						var trimmedPath = path.Trim('"');
						return !string.IsNullOrWhiteSpace(trimmedPath) 
								// && trimmedPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase) 
								&& File.Exists(trimmedPath)
										? ValidationResult.Success()
										: ValidationResult.Error("[red]Invalid path. Must be an existing .md file.[/]");
					})
			).Trim('"');
		}
		// Resolve paths using CLI arg first, then config value
		config.ResolvePaths(inputFileInfo?.FullName);


		// 3. Determine Theme
		if (themeChoice.HasValue) // CLI option overrides config/default
		{
			config.Theme = themeChoice.Value;
		}
		else if (!useConfigFile || !Enum.IsDefined(config.Theme)) // Ask if not using valid config or no CLI override
		{
			if (skipPrompts)
			{
				AnsiConsole.MarkupLine($"[grey]No theme specified, defaulting to {config.Theme}.[/]");
			}
			else
			{
				config.Theme = AnsiConsole.Prompt(
					new SelectionPrompt<Theme>()
						.Title("[green]Select the theme?[/]")
						.AddChoices(Enum.GetValues<Theme>())
						.UseConverter(t => t.ToString()) // Display enum names nicely
				);
			}
		}

		// 4. Determine CSS Mode
		if (cssModeChoice.HasValue) // CLI option overrides config/default
		{
			config.CssMode = cssModeChoice.Value;
		}
		else if (!useConfigFile || !Enum.IsDefined(config.CssMode)) // Ask if not using valid config or no CLI override
		{
			if (skipPrompts)
			{
				AnsiConsole.MarkupLine($"[grey]No CSS mode specified, defaulting to {config.CssMode}.[/]");
			}
			else
			{
				config.CssMode = AnsiConsole.Prompt(
					new SelectionPrompt<CssMode>()
						.Title("[green]How to include GitHub CSS?[/]")
						.AddChoices(Enum.GetValues<CssMode>())
						.UseConverter(m => m switch
						{
							CssMode.CDN => "CDN Link (Requires internet to view)",
							CssMode.Embed => "Embed CSS (Downloads now, works offline)",
							_ => m.ToString()
						})
				);
			}
		}

		// 5. Determine Output File (Override config if specified via CLI)
		if (outputFileInfo != null)
		{
			config.ResolvedOutputFile = outputFileInfo.FullName;
			outputFileInfo.Directory?.Create();
		}
		// Ensure output directory exists if derived/from config
		else
		{
			var outputDir = Path.GetDirectoryName(config.ResolvedOutputFile);
			if (!string.IsNullOrEmpty(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}
		}

		// --- Display Summary (Table Format) ---
		if (!skipPrompts)
		{
			AnsiConsole.WriteLine();
			var summary = new Table()
				.RoundedBorder()
				.BorderColor(Color.Grey)
				.Title("[underline cyan]Configuration Summary[/]")
				.AddColumn(new TableColumn("[yellow]Setting[/]").RightAligned()) // Right align setting names
				.AddColumn("Value") // Left align values (default)
				.Expand()
				.AddRow("[bold]Input File[/]", $"[blue]{Markup.Escape(config.ResolvedInputFile)}[/]")
				.AddRow("[bold]Output File[/]", $"[green]{Markup.Escape(config.ResolvedOutputFile)}[/]")
				.AddRow("[bold]Theme[/]", $"[violet]{config.Theme}[/]")
				.AddRow("[bold]CSS Mode[/]", $"[orange3]{config.CssMode}[/]");

			AnsiConsole.Write(summary);
			AnsiConsole.WriteLine();

			if (!AnsiConsole.Confirm("[yellow]Proceed with conversion?[/]"))
			{
				AnsiConsole.MarkupLine("[red]Operation cancelled by user.[/]");
				context.ExitCode = 1;
				return;
			}
		}

		// --- Perform Conversion ---
		string? cssContent = null;
		var finalHtml = string.Empty;

		await AnsiConsole.Status()
			.Spinner(Spinner.Known.SimpleDotsScrolling)
			.SpinnerStyle(Style.Parse("cyan"))
			.StartAsync("[yellow]Processing...[/]", async ctx =>
			{
				// 1. Read Markdown File
				ctx.Status($"Reading [blue]{Path.GetFileName(config.ResolvedInputFile)}[/]...");
				var markdownContent = await File.ReadAllTextAsync(config.ResolvedInputFile);
				await Task.Delay(150); // Simulate work

				// 2. Get CSS if Embed mode
				if (config.CssMode == CssMode.Embed)
				{
					ctx.Status($"Downloading [blue]{config.Theme}[/] theme CSS...");
					cssContent = await MarkdownService.GetCssContentAsync(config.Theme);
				}

				// 3. Convert Markdown via GitHub API
				ctx.Status("Converting Markdown via GitHub API...");
				var htmlFragment = await MarkdownService.ConvertMarkdownToHtmlAsync(markdownContent);

				// 4. Generate Full HTML
				ctx.Status("Generating final HTML structure...");
				var title = Path.GetFileNameWithoutExtension(config.ResolvedInputFile);
				finalHtml = MarkdownService.GenerateFullHtml(title, htmlFragment, config.Theme, config.CssMode, cssContent);

				// 5. Write Output File
				ctx.Status($"Writing HTML to [blue]{Path.GetFileName(config.ResolvedOutputFile)}[/]...");
				await File.WriteAllTextAsync(config.ResolvedOutputFile, finalHtml);
			});

		AnsiConsole.MarkupLine($"[bold green]Success![/] HTML file generated at: [underline cyan]{config.ResolvedOutputFile}[/]");
		AnsiConsole.WriteLine();
		if (!skipPrompts)
			AnsiConsole.Console.Input.ReadKey(false);

		context.ExitCode = 0;
	}
	catch (FileNotFoundException ex)
	{
		AnsiConsole.MarkupLine($"[red]Error: Input file not found.[/]");
		AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
		context.ExitCode = 2; // Specific exit code for file not found
	}
	catch (ArgumentException ex) // Catches validation errors from ResolvePaths etc.
	{
		AnsiConsole.MarkupLine($"[red]Configuration Error: {ex.Message}[/]");
		context.ExitCode = 3;
	}
	catch (HttpRequestException ex)
	{
		AnsiConsole.MarkupLine($"[red]API Error: Failed to communicate with GitHub API or download CSS.[/]");
		AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
		if (ex.InnerException != null) AnsiConsole.MarkupLine($"[red]Inner: {ex.InnerException.Message}[/]");
		context.ExitCode = 4; // Specific exit code for network/API errors
	}
	catch (Exception ex) // Catch-all for unexpected errors
	{
		AnsiConsole.MarkupLine("[bold red]An unexpected error occurred![/]");
		// AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes);
		AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
		if (ex.StackTrace != null)
		{
			AnsiConsole.MarkupLine("[grey]Stack Trace:[/]");
			AnsiConsole.WriteLine(ex.StackTrace);
		}
		context.ExitCode = -1;
	}
});

// --- Run the Application ---
return await rootCommand.InvokeAsync(args);
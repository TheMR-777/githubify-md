namespace GitHubify.Utilities;

using Spectre.Console;

internal static class ConversionFlow
{
    public static async Task ExecuteAsync(Config config, bool skipPrompts)
    {
        string? cssContent = null;
        string finalHtml;

        if (skipPrompts)
        {
            finalHtml = await ExecuteConversionAsync(config, cssContent);
        }
        else
        {
            finalHtml = await AnsiConsole.Status()
                .Spinner(Spinner.Known.SimpleDotsScrolling)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("[yellow]Processing...[/]", async ctx =>
                {
                    ctx.Status($"Reading [blue]{Path.GetFileName(config.ResolvedInputFile)}[/]...");
                    var markdownContent = await File.ReadAllTextAsync(config.ResolvedInputFile);

                    if (config.CssMode == CssMode.Embed)
                    {
                        ctx.Status($"Downloading [blue]{config.Theme}[/] theme CSS...");
                        cssContent = await MarkdownService.GetCssContentAsync(config.Theme);
                    }

                    ctx.Status("Converting Markdown via GitHub API...");
                    var htmlFragment = await MarkdownService.ConvertMarkdownToHtmlAsync(markdownContent);

                    ctx.Status("Generating final HTML structure...");
                    var title = Path.GetFileNameWithoutExtension(config.ResolvedInputFile);
                    var html = MarkdownService.GenerateFullHtml(title, htmlFragment, config.Theme, config.CssMode, cssContent);

                    ctx.Status($"Writing HTML to [blue]{Path.GetFileName(config.ResolvedOutputFile)}[/]...");
                    await File.WriteAllTextAsync(config.ResolvedOutputFile, html);

                    return html;
                });
        }
    }

    private static async Task<string> ExecuteConversionAsync(Config config, string? cssContent)
    {
        var markdownContent = await File.ReadAllTextAsync(config.ResolvedInputFile);
        
        if (config.CssMode == CssMode.Embed)
            cssContent = await MarkdownService.GetCssContentAsync(config.Theme);

        var htmlFragment = await MarkdownService.ConvertMarkdownToHtmlAsync(markdownContent);
        var title = Path.GetFileNameWithoutExtension(config.ResolvedInputFile);
        var html = MarkdownService.GenerateFullHtml(title, htmlFragment, config.Theme, config.CssMode, cssContent);

        await File.WriteAllTextAsync(config.ResolvedOutputFile, html);
        return html;
    }
}
namespace GitHubify.Utilities;

using Spectre.Console;
using System.CommandLine;
using System.Text.Json;

internal static class CliHandler
{
    public static async Task<int> ExecuteAsync(
        FileInfo? inputFile,
        Theme? theme,
        CssMode? cssMode,
        FileInfo? outputFile,
        bool skipPrompts)
    {
        // ✨ Artpiece: Interactive loop for beautiful UX
        if (!skipPrompts)
        {
            return await ExecuteInteractiveLoop(inputFile, theme, cssMode, outputFile);
        }

        return await ExecuteSingleConversion(inputFile, theme, cssMode, outputFile, skipPrompts);
    }

    private static async Task<int> ExecuteInteractiveLoop(
        FileInfo? inputFile,
        Theme? theme,
        CssMode? cssMode,
        FileInfo? outputFile)
    {
        string? lastOutputPath = null;
        
        while (true)
        {
            try
            {
                var (result, outputPath) = await ExecuteSingleConversionWithOutput(inputFile, theme, cssMode, outputFile, false);
                
                if (result != ExitCodes.Success)
                    return result;

                lastOutputPath = outputPath;
                var postAction = UserInterface.ShowPostConversionOptions();
                
                switch (postAction)
                {
                    case UserInterface.PostConversionAction.ConvertAnother:
                        // Reset CLI args for next iteration to allow full reconfiguration
                        inputFile = null;
                        theme = null;
                        cssMode = null;
                        outputFile = null;
                        AnsiConsole.Clear();
                        continue;
                    
                    case UserInterface.PostConversionAction.OpenOutput:
                        if (!string.IsNullOrEmpty(lastOutputPath))
                        {
                            UserInterface.OpenFileInDefaultApp(lastOutputPath);
                        }
                        return ExitCodes.Success;
                    
                    case UserInterface.PostConversionAction.Exit:
                        UserInterface.ShowGoodbye();
                        return ExitCodes.Success;
                    
                    default:
                        return ExitCodes.Success;
                }
            }
            catch (OperationCanceledException)
            {
                UserInterface.ShowGoodbye();
                return ExitCodes.UserCancelled;
            }
            catch (Exception ex)
            {
                UserInterface.ShowUnexpectedError(ex);
                
                if (!AnsiConsole.Confirm("[yellow]Would you like to try again?[/]"))
                {
                    UserInterface.ShowGoodbye();
                    return ExitCodes.UnexpectedError;
                }
                
                // Reset for retry
                inputFile = null;
                theme = null;
                cssMode = null;
                outputFile = null;
            }
        }
    }

    private static async Task<int> ExecuteSingleConversion(
        FileInfo? inputFile,
        Theme? theme,
        CssMode? cssMode,
        FileInfo? outputFile,
        bool skipPrompts)
    {
        var (result, _) = await ExecuteSingleConversionWithOutput(inputFile, theme, cssMode, outputFile, skipPrompts);
        return result;
    }

    private static async Task<(int result, string? outputPath)> ExecuteSingleConversionWithOutput(
        FileInfo? inputFile,
        Theme? theme,
        CssMode? cssMode,
        FileInfo? outputFile,
        bool skipPrompts)
    {
        try
        {
            var config = await ConfigurationFlow.ResolveAsync(
                inputFile?.FullName, theme, cssMode, outputFile?.FullName, skipPrompts);

            await ConversionFlow.ExecuteAsync(config, skipPrompts);
            
            // ✨ Artpiece: Show success message only, post-conversion handling is done in interactive loop
            UserInterface.ShowSuccess(config.ResolvedOutputFile, skipPrompts);
            
            return (ExitCodes.Success, config.ResolvedOutputFile);
        }
        catch (OperationCanceledException)
        {
            if (!skipPrompts)
                UserInterface.ShowGoodbye();
            return (ExitCodes.UserCancelled, null);
        }
        catch (FileNotFoundException ex)
        {
            UserInterface.ShowError("Input file not found", ex.Message);
            return (ExitCodes.FileNotFound, null);
        }
        catch (ArgumentException ex)
        {
            UserInterface.ShowError("Configuration Error", ex.Message);
            return (ExitCodes.ConfigurationError, null);
        }
        catch (HttpRequestException ex)
        {
            UserInterface.ShowError("API Error", "Failed to communicate with GitHub API or download CSS", ex.Message, ex.InnerException?.Message);
            return (ExitCodes.NetworkError, null);
        }
        catch (Exception ex)
        {
            UserInterface.ShowUnexpectedError(ex);
            return (ExitCodes.UnexpectedError, null);
        }
    }
}

internal static class ExitCodes
{
    public const int Success = 0;
    public const int UserCancelled = 1;
    public const int FileNotFound = 2;
    public const int ConfigurationError = 3;
    public const int NetworkError = 4;
    public const int UnexpectedError = -1;
}
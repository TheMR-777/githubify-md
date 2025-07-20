namespace GitHubify.Utilities;

using System.Text.Json;

internal static class ConfigurationFlow
{
    private const string ConfigFilePath = "config.json";

    public static async Task<Config> ResolveAsync(
        string? cliInputFile,
        Theme? cliTheme,
        CssMode? cliCssMode,
        string? cliOutputFile,
        bool skipPrompts)
    {
        UserInterface.ShowWelcome(skipPrompts);

        var config = await LoadConfigFileAsync(skipPrompts);
        
        ResolveInputFileAsync(config, cliInputFile, skipPrompts);
        ResolveTheme(config, cliTheme, skipPrompts);
        ResolveCssMode(config, cliCssMode, skipPrompts);
        ResolveOutputFile(config, cliOutputFile);

        // ✨ Artpiece: Smart reconfiguration loop
        if (!skipPrompts)
        {
            config = HandleSmartReconfiguration(config);
        }

        return config;
    }

    private static Config HandleSmartReconfiguration(Config config)
    {
        while (true)
        {
            var action = UserInterface.ConfirmExecution(config);
            
            switch (action)
            {
                case UserInterface.ConfigurationAction.Proceed:
                    return config;
                
                case UserInterface.ConfigurationAction.ChangeInputFile:
                    config.InputFile = UserInterface.PromptForInputFile();
                    config.ResolvePaths();
                    break;
                
                case UserInterface.ConfigurationAction.ChangeOutputFile:
                    var newOutputPath = UserInterface.PromptForOutputFile(config.ResolvedOutputFile);
                    config.OutputFile = newOutputPath;
                    config.ResolvedOutputFile = newOutputPath;
                    break;
                
                case UserInterface.ConfigurationAction.ChangeTheme:
                    config.Theme = UserInterface.PromptForTheme();
                    break;
                
                case UserInterface.ConfigurationAction.ChangeCssMode:
                    config.CssMode = UserInterface.PromptForCssMode();
                    break;
                
                case UserInterface.ConfigurationAction.Cancel:
                    throw new OperationCanceledException("Configuration cancelled by user.");
                
                default:
                    return config;
            }
        }
    }

    private static async Task<Config> LoadConfigFileAsync(bool skipPrompts)
    {
        if (!File.Exists(ConfigFilePath) || skipPrompts)
            return new Config();

        if (!UserInterface.ConfirmConfigFile(ConfigFilePath))
        {
            UserInterface.ShowConfigIgnored();
            return new Config();
        }

        try
        {
            var json = await File.ReadAllTextAsync(ConfigFilePath);
            var config = JsonSerializer.Deserialize(json, ConfigContext.Default.Config) 
                ?? throw new InvalidOperationException("Failed to deserialize config file.");
            
            UserInterface.ShowConfigLoaded();
            return config;
        }
        catch (Exception ex)
        {
            UserInterface.ShowConfigError(ConfigFilePath, ex.Message);
            return new Config();
        }
    }

    private static void ResolveInputFileAsync(Config config, string? cliInputFile, bool skipPrompts)
    {
        if (cliInputFile != null || !string.IsNullOrWhiteSpace(config.InputFile))
        {
            config.ResolvePaths(cliInputFile);
            return;
        }

        if (skipPrompts)
            throw new InvalidOperationException("Input file must be specified via --input or config.json in non-interactive mode.");

        config.InputFile = UserInterface.PromptForInputFile();
        config.ResolvePaths();
    }

    private static void ResolveTheme(Config config, Theme? cliTheme, bool skipPrompts)
    {
        if (cliTheme.HasValue)
        {
            config.Theme = cliTheme.Value;
            return;
        }

        if (Enum.IsDefined(config.Theme))
            return;

        if (skipPrompts)
        {
            UserInterface.ShowDefaultUsed(config.Theme, "theme");
            return;
        }

        config.Theme = UserInterface.PromptForTheme();
    }

    private static void ResolveCssMode(Config config, CssMode? cliCssMode, bool skipPrompts)
    {
        if (cliCssMode.HasValue)
        {
            config.CssMode = cliCssMode.Value;
            return;
        }

        if (Enum.IsDefined(config.CssMode))
            return;

        if (skipPrompts)
        {
            UserInterface.ShowDefaultUsed(config.CssMode, "CSS mode");
            return;
        }

        config.CssMode = UserInterface.PromptForCssMode();
    }

    private static void ResolveOutputFile(Config config, string? cliOutputFile)
    {
        if (cliOutputFile != null)
        {
            config.ResolvedOutputFile = cliOutputFile;
            var outputDir = Path.GetDirectoryName(cliOutputFile);
            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);
        }
        else
        {
            var outputDir = Path.GetDirectoryName(config.ResolvedOutputFile);
            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);
        }
    }
}
namespace GitHubify.Utilities;

using System.Text.Json.Serialization;

public enum Theme { Light, Dark }
public enum CssMode { CDN, Embed }

public class Config
{
	[JsonPropertyName("input_file")]
	public string? InputFile { get; set; }

	[JsonPropertyName("theme")]
	[JsonConverter(typeof(JsonStringEnumConverter<Theme>))]
	public Theme Theme { get; set; } = Theme.Light;

	[JsonPropertyName("css_mode")]
	[JsonConverter(typeof(JsonStringEnumConverter<CssMode>))]
	public CssMode CssMode { get; set; } = CssMode.CDN;

	[JsonPropertyName("output_file")]
	public string? OutputFile { get; set; } // Optional, can be derived

	// --- Internal Helper Properties (Not serialized) ---
	[JsonIgnore]
	public string ResolvedInputFile { get; set; } = string.Empty;

	[JsonIgnore]
	public string ResolvedOutputFile { get; set; } = string.Empty;


	// Basic validation
	[JsonIgnore]
	public bool IsValid => !string.IsNullOrWhiteSpace(ResolvedInputFile) &&
						   !string.IsNullOrWhiteSpace(ResolvedOutputFile) &&
						   Enum.IsDefined(Theme) &&
						   Enum.IsDefined(CssMode);

	public void ResolvePaths(string? cliInputFile = null)
	{
		// ✨ Artpiece: Null-coalescing with meaningful exception
		ResolvedInputFile = cliInputFile ?? InputFile ?? 
			throw new ArgumentNullException(nameof(InputFile), "Input file path must be provided either via CLI argument or config file.");

		if (!File.Exists(ResolvedInputFile))
			throw new FileNotFoundException($"Input Markdown file not found: {ResolvedInputFile}");

		// ✨ Artpiece: Proper path manipulation
		ResolvedOutputFile = OutputFile ?? Path.ChangeExtension(ResolvedInputFile, ".html");
	}
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Config))]
internal partial class ConfigContext : JsonSerializerContext;
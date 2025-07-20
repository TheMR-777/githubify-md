namespace GitHubify.Utilities;

internal static class MarkdownService
{
	private static readonly HttpClient HttpClient = new();
	private const string GitHubApiUrl = "https://api.github.com/markdown/raw";
	private const string UserAgent = nameof(GitHubify);
	private const string GitHubCssCdnLight = "https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.8.1/github-markdown-light.min.css";
	private const string GitHubCssCdnDark = "https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.8.1/github-markdown-dark.min.css";

	public static async Task<string> ConvertMarkdownToHtmlAsync(string markdownContent)
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, GitHubApiUrl)
		{
			Content = new StringContent(markdownContent, System.Text.Encoding.UTF8, "text/plain")
		};
		
		request.Headers.Accept.Add(new("text/html"));
		request.Headers.UserAgent.ParseAdd(UserAgent);

		using var response = await HttpClient.SendAsync(request);
		
		if (!response.IsSuccessStatusCode)
		{
			var details = await response.Content.ReadAsStringAsync();
			throw new HttpRequestException($"GitHub API request failed: {response.StatusCode} - {response.ReasonPhrase}\nDetails: {details}");
		}
		
		return await response.Content.ReadAsStringAsync();
	}

	public static async Task<string> GetCssContentAsync(Theme theme)
	{
		var cssUrl = theme switch
		{
			Theme.Light => GitHubCssCdnLight,
			Theme.Dark => GitHubCssCdnDark,
			_ => throw new ArgumentOutOfRangeException(nameof(theme))
		};

		try
		{
			using var response = await HttpClient.GetAsync(cssUrl);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStringAsync();
		}
		catch (HttpRequestException ex)
		{
			throw new InvalidOperationException($"Failed to download CSS from '{cssUrl}'. Please check the URL or your internet connection.", ex);
		}
	}

	public static string GenerateFullHtml(string title, string htmlFragment, Theme theme, CssMode cssMode, string? cssContent = null)
	{
		var themeClass = theme switch { Theme.Light => "light", Theme.Dark => "dark", _ => "light" };

		var cssSection = cssMode switch
		{
			CssMode.Embed when !string.IsNullOrWhiteSpace(cssContent) => $"<style>{cssContent}</style>",
			CssMode.Embed => throw new ArgumentNullException(nameof(cssContent), "CSS content must be provided for embed mode."),
			CssMode.CDN => theme switch
			{
				Theme.Light => $"<link rel=\"stylesheet\" href=\"{GitHubCssCdnLight}\">",
				Theme.Dark => $"<link rel=\"stylesheet\" href=\"{GitHubCssCdnDark}\">",
				_ => throw new ArgumentOutOfRangeException(nameof(theme))
			},
			_ => throw new ArgumentOutOfRangeException(nameof(cssMode))
		};

		// ✨ Artpiece: Clean HTML generation with proper escaping
		return $@"<!DOCTYPE html>
<html lang=""en"" data-theme=""{themeClass}"">
<head>
	<meta charset=""UTF-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
	<title>{title}</title>
	{cssSection}
	<style>
		body {{
			box-sizing: border-box;
			min-width: 200px;
			max-width: 980px;
			margin: 0 auto;
			padding: 45px;
		}}
		.markdown-body {{
			box-sizing: border-box;
			min-width: 200px;
			max-width: 980px;
			margin: 0 auto;
			padding: 45px;
		}}
		@media (max-width: 767px) {{
			.markdown-body, body {{ padding: 15px; }}
		}}
	</style>
</head>
<body class=""markdown-body"">
	{htmlFragment}
</body>
</html>";
	}
}
namespace GitHubify.Utilities;

public class MarkdownService
{
	static readonly HttpClient HttpClient = new();
	const string GitHubApiUrl = "https://api.github.com/markdown/raw";
	const string UserAgent = nameof(GitHubify);

	const string GitHubCssCdnLight = "https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.8.1/github-markdown-light.min.css";
	const string GitHubCssCdnDark = "https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.8.1/github-markdown-dark.min.css";

	// Using Cloudflare CDN as an alternative/example. The original jsDelivr links are also fine:
	// https://cdn.jsdelivr.net/npm/github-markdown-css@5.8.1/github-markdown-light.css
	// https://cdn.jsdelivr.net/npm/github-markdown-css@5.8.1/github-markdown-dark.css

	public static async Task<string> ConvertMarkdownToHtmlAsync(string markdownContent)
	{
		using var request = new HttpRequestMessage(HttpMethod.Post, GitHubApiUrl);
		request.Headers.Accept.Add(new("text/html"));
		request.Headers.UserAgent.ParseAdd(UserAgent);
		request.Content = new StringContent(markdownContent, System.Text.Encoding.UTF8, "text/plain");

		using var response = await HttpClient.SendAsync(request);
		if (!response.IsSuccessStatusCode)
		{
			throw new HttpRequestException($"GitHub API request failed: {response.StatusCode} - {response.ReasonPhrase}\nDetails: {await response.Content.ReadAsStringAsync()}");
		}
		return await response.Content.ReadAsStringAsync();
	}

	public static async Task<string> GetCssContentAsync(Theme theme)
	{
		var cssUrl = theme == Theme.Light ? GitHubCssCdnLight : GitHubCssCdnDark;
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
		string cssSection;
		var themeClass = theme == Theme.Light ? "light" : "dark"; // Optional: for potential future JS theme switching

		if (cssMode == CssMode.Embed)
		{
			if (string.IsNullOrWhiteSpace(cssContent))
			{
				throw new ArgumentNullException(nameof(cssContent), "CSS content must be provided for embed mode.");
			}
			// Embed the CSS directly into a <style> tag
			cssSection = $"""
					<style>
					{cssContent}
					</style>
				""";
		}
		else // CssMode.CDN
		{
			string cssUrl = theme == Theme.Light ? GitHubCssCdnLight : GitHubCssCdnDark;
			// Link to the CDN
			cssSection = $"""
					<link rel="stylesheet" href="{cssUrl}">
				""";
		}

		// Note the 'markdown-body' class on the body tag, required by github-markdown-css
		return $$"""
			<!DOCTYPE html>
			<html lang="en" data-theme="{{themeClass}}">
			<head>
				<meta charset="UTF-8">
				<meta name="viewport" content="width=device-width, initial-scale=1.0">
				<title>{{title}}</title>

				{{cssSection}}

				<!-- Basic wrapper styles similar to GitHub's page structure -->
				<style>
					body {
						box-sizing: border-box;
						min-width: 200px;
						max-width: 980px;
						margin: 0 auto;
						padding: 45px;
					}
					/* Ensure the body takes the theme class for styling */
					.markdown-body {
					   box-sizing: border-box;
					   min-width: 200px;
					   max-width: 980px;
					   margin: 0 auto;
					   padding: 45px;
					}

					@media (max-width: 767px) {
						.markdown-body {
							padding: 15px;
						}
						/* Adjust body padding on mobile too if needed */
						body {
							 padding: 15px;
						}
					}

					/* Optional: Add basic dark/light mode background/text colors */
					/* This can be overridden/enhanced by the linked/embedded CSS */
					/* You might not need this if github-markdown-css handles it fully */
					/*
					@media (prefers-color-scheme: dark) {
					  body { background-color: #0d1117; color: #c9d1d9; }
					}
					@media (prefers-color-scheme: light) {
					   body { background-color: #ffffff; color: #24292f; }
					}
					html[data-theme='dark'] body { background-color: #0d1117; color: #c9d1d9; }
					html[data-theme='light'] body { background-color: #ffffff; color: #24292f; }
					*/
				</style>
			</head>
			<body class="markdown-body">
				{{htmlFragment}}
			</body>
			</html>
			""";
	}
}

namespace Kojinx.Trust.GitHub.Models;

public class GitHubAccessToken
{
    public string AccessToken { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
}

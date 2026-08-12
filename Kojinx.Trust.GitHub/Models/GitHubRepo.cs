namespace Kojinx.Trust.GitHub.Models;

public class GitHubRepo
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RepoUrl { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Platform { get; set; } = "GitHub";
}

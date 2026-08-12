using Kojinx.Trust.GitHub.Models;
using Kojinx.Trust.GitHub.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kojinx.Trust.GitHub;

public class GitHubRepoFetcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenEncryptor _tokenEncryptor;

    public GitHubRepoFetcher(IHttpClientFactory httpClientFactory, ITokenEncryptor tokenEncryptor)
    {
        _httpClientFactory = httpClientFactory;
        _tokenEncryptor = tokenEncryptor;
    }

    public async Task<List<GitHubRepo>?> FetchRepositoriesAsync(string protectedToken)
    {
        var token = _tokenEncryptor.UnprotectToken(protectedToken);
        if (string.IsNullOrEmpty(token)) return null;

        using var client = _httpClientFactory.CreateClient("GitHub");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (!client.DefaultRequestHeaders.UserAgent.TryParseAdd("KojinxApp"))
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("KojinxApp");
        }

        var allRepos = new List<GitHubRepo>();
        var page = 1;
        const int perPage = 100;

        while (true)
        {
            var url = $"https://api.github.com/user/repos?per_page={perPage}&sort=updated&type=all&page={page}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // if unauthorized, return null so caller knows token is invalid
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var reposArray = doc.RootElement;

            if (reposArray.GetArrayLength() == 0) break;

            foreach (var r in reposArray.EnumerateArray())
            {
                var name = r.GetProperty("name").GetString() ?? string.Empty;
                var fullName = r.GetProperty("full_name").GetString() ?? string.Empty;
                var cloneUrl = r.GetProperty("clone_url").GetString() ?? string.Empty;
                var isPrivate = r.TryGetProperty("private", out var p) && p.GetBoolean();
                var description = r.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null ? desc.GetString() ?? string.Empty : string.Empty;

                allRepos.Add(new GitHubRepo
                {
                    Name = name,
                    FullName = fullName,
                    RepoUrl = cloneUrl,
                    IsPrivate = isPrivate,
                    Description = description,
                    Platform = "GitHub"
                });
            }

            if (reposArray.GetArrayLength() < perPage) break;

            page++;
            if (page > 10) break; // max 1000 repos limit
        }

        return allRepos;
    }

    public async Task<(string? scopes, bool isValid, bool hasRepoScope)> DebugTokenAsync(string protectedToken)
    {
        var token = _tokenEncryptor.UnprotectToken(protectedToken);
        if (string.IsNullOrEmpty(token)) return (null, false, false);

        using var client = _httpClientFactory.CreateClient("GitHub");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (!client.DefaultRequestHeaders.UserAgent.TryParseAdd("KojinxApp"))
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("KojinxApp");
        }

        var response = await client.GetAsync("https://api.github.com/user");
        
        string? oauthScopes = null;
        if (response.Headers.TryGetValues("X-OAuth-Scopes", out var scopeVals))
        {
            oauthScopes = string.Join(", ", scopeVals);
        }

        return (oauthScopes, response.IsSuccessStatusCode, oauthScopes?.Contains("repo") ?? false);
    }
}

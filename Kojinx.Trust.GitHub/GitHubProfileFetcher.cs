using Kojinx.Trust.GitHub.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Kojinx.Trust.GitHub;

public class GitHubProfileFetcher
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubProfileFetcher(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<GitHubProfile?> FetchProfileAsync(string accessToken)
    {
        using var client = _httpClientFactory.CreateClient("GitHub");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (!client.DefaultRequestHeaders.UserAgent.TryParseAdd("KojinxApp"))
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("KojinxApp");
        }

        var userUrl = "https://api.github.com/user";
        var userRes = await client.GetAsync(userUrl);
        
        if (!userRes.IsSuccessStatusCode) return null;

        var userStr = await userRes.Content.ReadAsStringAsync();
        using var userDoc = JsonDocument.Parse(userStr);
        var userData = userDoc.RootElement;

        var providerId = userData.GetProperty("id").GetInt64().ToString();
        var login = userData.GetProperty("login").GetString() ?? "unknown";
        var name = userData.TryGetProperty("name", out var n) && n.ValueKind != JsonValueKind.Null ? (n.GetString() ?? login) : login;
        var avatarUrl = userData.TryGetProperty("avatar_url", out var a) && a.ValueKind != JsonValueKind.Null ? a.GetString() : null;

        var email = await FetchEmailAsync(accessToken, client);

        return new GitHubProfile
        {
            Id = providerId,
            Username = login,
            DisplayName = name,
            AvatarUrl = avatarUrl ?? string.Empty,
            Email = email
        };
    }

    private async Task<string?> FetchEmailAsync(string accessToken, HttpClient client)
    {
        var emailsUrl = "https://api.github.com/user/emails";
        var emailsRes = await client.GetAsync(emailsUrl);
        if (emailsRes.IsSuccessStatusCode)
        {
            var emailsStr = await emailsRes.Content.ReadAsStringAsync();
            using var emailsDoc = JsonDocument.Parse(emailsStr);
            var emailsData = emailsDoc.RootElement;
            foreach (var e in emailsData.EnumerateArray())
            {
                if (e.TryGetProperty("primary", out var primaryElem) && primaryElem.GetBoolean())
                {
                    return e.GetProperty("email").GetString();
                }
            }
        }
        return null;
    }
}

using Kojinx.Trust.GitHub.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kojinx.Trust.GitHub;

public class GitHubTokenExchange
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubTokenExchange(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<GitHubAccessToken?> ExchangeCodeForTokenAsync(string clientId, string clientSecret, string code)
    {
        using var client = _httpClientFactory.CreateClient("GitHub");
        var tokenUrl = "https://github.com/login/oauth/access_token";
        
        var tokenReq = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        tokenReq.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        tokenReq.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"code", code}
        });
        
        var tokenRes = await client.SendAsync(tokenReq);
        if (!tokenRes.IsSuccessStatusCode) return null;
        
        var tokenStr = await tokenRes.Content.ReadAsStringAsync();
        using var tokenDoc = JsonDocument.Parse(tokenStr);
        var tokenData = tokenDoc.RootElement;
        
        if (tokenData.TryGetProperty("access_token", out var accessTokenElem))
        {
            return new GitHubAccessToken
            {
                AccessToken = accessTokenElem.GetString() ?? string.Empty,
                Scope = tokenData.TryGetProperty("scope", out var scope) ? scope.GetString() ?? string.Empty : string.Empty,
                TokenType = tokenData.TryGetProperty("token_type", out var type) ? type.GetString() ?? string.Empty : string.Empty
            };
        }
        return null;
    }
}

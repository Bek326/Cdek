using Cdek.Services.Interfaces;
using Cdek.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Cdek.Services;

internal sealed class dekAccessTokenService : ICdekAccessTokenService
{
    private const string GrantTypeKey = "grant_type";
    private const string GrantTypeValue = "client_credentials";
    private const string ClientIdKey = "client_id";
    private const string ClientSecretKey = "client_secret";
    private readonly CdekSettings _cdekSettings;
    private readonly IHttpClientFactory _clientFactory;

    public CdekAccessTokenService(IHttpClientFactory clientFactory, IOptions<CdekSettings> cdekSettings)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _cdekSettings = cdekSettings.Value ?? throw new ArgumentNullException(nameof(cdekSettings));
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        using var client = CreateConfiguredClient();

        var response = await client.PostAsync(_cdekSettings.AuthUrl, CreateRequestContent(), cancellationToken);

        return await ParseTokenFromResponseAsync(response, cancellationToken);
    }

    private HttpClient CreateConfiguredClient()
    {
        return _clientFactory.CreateClient();
    }

    private FormUrlEncodedContent CreateRequestContent()
    {
        return new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>(GrantTypeKey, GrantTypeValue),
            new KeyValuePair<string, string>(ClientIdKey, _cdekSettings.ClientId),
            new KeyValuePair<string, string>(ClientSecretKey, _cdekSettings.ClientSecret)
        });
    }

    private static async Task<string> ParseTokenFromResponseAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JObject.Parse(content)["access_token"]!.ToString();
    }
}
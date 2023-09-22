using System.Text;
using Cdek.Requests;
using Cdek.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cdek.Services;

internal sealed class CdekDeliveryService : ICdekDeliveryService
{
    private const string CdekApiUrl = "https://api.edu.cdek.ru/v2/calculator/tarifflist";
    private const int CourierDeliveryType = 1;
    private readonly ICdekAccessTokenService _cdekAccessTokenService;
    private readonly IHttpClientFactory _clientFactory;

    public CdekDeliveryService(IHttpClientFactory clientFactory, ICdekAccessTokenService cdekAccessTokenService)
    {
        _clientFactory = clientFactory;
        _cdekAccessTokenService = cdekAccessTokenService;
    }

    public async Task<decimal> CalculateDeliveryCostAsync(CdekDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        var accessToken = await _cdekAccessTokenService.GetTokenAsync(cancellationToken);

        using var client = CreateConfiguredClient(accessToken);

        var requestContent = CreateRequestContent(request);

        var response = await client.PostAsync(CdekApiUrl, requestContent, cancellationToken);

        return await ParseResponseAsync(response, cancellationToken);
    }

    private HttpClient CreateConfiguredClient(string accessToken)
    {
        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        return client;
    }

    private static StringContent CreateRequestContent(CdekDeliveryRequest request)
    {
        var requestContent = new
        {
            type = CourierDeliveryType,
            from_location = new { code = request.SenderCityFias },
            to_location = new { code = request.ReceiverCityFias },
            packages = new[]
            {
                new
                {
                    weight = request.Weight / 1000.0m,
                    size = new
                    {
                        length = request.Length,
                        width = request.Width,
                        height = request.Height
                    }
                }
            }
        };

        return new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");
    }

    private static async Task<decimal> ParseResponseAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Ошибка при расчете стоимости: {content}");

        var json = JObject.Parse(content);
        return json["result"]!["price"]?.Value<decimal>() ?? 0;
    }
}
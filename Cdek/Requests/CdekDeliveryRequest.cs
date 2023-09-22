namespace Cdek.Requests;

public sealed class CdekDeliveryRequest
{
    public decimal Weight { get; set; }

    public int Length { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string? SenderCityFias { get; set; }

    public string? ReceiverCityFias { get; set; }
}
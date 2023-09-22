using Cdek.Requests;

namespace Cdek.Services.Interfaces;

public interface CdekDeliveryService
{
    Task<decimal> CalculateDeliveryCostAsync(CdekDeliveryRequest request,
        CancellationToken cancellationToken);
}
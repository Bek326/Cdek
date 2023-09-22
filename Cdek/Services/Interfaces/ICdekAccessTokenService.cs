namespace Cdek.Services.Interfaces;

public interface ICdekAccessTokenService
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);

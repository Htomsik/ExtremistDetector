namespace Inquisitor.Infrastructure;

public interface IOCRProvider
{
    public Task<string> GetTextFromUrl(string url, CancellationToken cancellationToken = default);
}
using IntegrationDistributed.API.Common;
using IntegrationDistributed.API.Persistence;
using Microsoft.EntityFrameworkCore;
using RedLockNet;

namespace IntegrationDistributed.API.Services;

public sealed class ItemIntegrationService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedLockFactory _lockFactory;

    public ItemIntegrationService(ApplicationDbContext context, IDistributedLockFactory lockFactory)
    {
        _context = context;
        _lockFactory = lockFactory;
    }

    public async Task<Result> SaveItemAsync(string itemContent, CancellationToken cancellationToken)
    {
        string resource = $"{nameof(SaveItemAsync)}-{itemContent}";
        var expiryTime = TimeSpan.FromSeconds(30);
        var waitTime = TimeSpan.FromSeconds(10);
        var retryTime = TimeSpan.FromSeconds(1);

        using (var redLock = await _lockFactory.CreateLockAsync(resource, expiryTime, waitTime, retryTime, cancellationToken))
        {
            if (redLock.IsAcquired)
            {
                if (await _context.Items.AnyAsync(item => item.Content == itemContent, cancellationToken))
                    return new Result(false, $"Duplicate item found in the backend with content {itemContent}.");

                var item = new Item();
                item.Content = itemContent;

                await _context.Items.AddAsync(item, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
            }
            else
            {
                return new Result(false, "We are currently unable to process your request due to system congestion. Please try again later.");
            }
        }
    }

    public async Task DeleteAsync(CancellationToken cancellationToken) =>
        await _context.Items.ExecuteDeleteAsync(cancellationToken);
}
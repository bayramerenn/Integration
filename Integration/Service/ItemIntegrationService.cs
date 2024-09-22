using Integration.Common;
using Integration.Backend;
using System.Collections.Concurrent;

namespace Integration.Service;

public sealed class ItemIntegrationService : IDisposable
{
    private readonly ConcurrentDictionary<string, DateTime> _processedContents = new ConcurrentDictionary<string, DateTime>();
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _entryLifetime = TimeSpan.FromMinutes(10);

    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

    public ItemIntegrationService()
    {
        _cleanupTimer = new Timer(CleanupProcessedContents, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public Result SaveItem(string itemContent)
    {
        var now = DateTime.UtcNow;

        var added = _processedContents.TryAdd(itemContent, now);

        if (!added)
        {
            return new Result(false, $"Duplicate item received with content {itemContent}.");
        }

        if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
        {
            return new Result(false, $"Duplicate item found in the backend with content {itemContent}.");
        }

        var item = ItemIntegrationBackend.SaveItem(itemContent);

        return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
    }

    private void CleanupProcessedContents(object state)
    {
        var now = DateTime.UtcNow;

        foreach (var kvp in _processedContents)
        {
            if (now - kvp.Value > _entryLifetime)
            {
                _processedContents.TryRemove(kvp.Key, out _);
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}
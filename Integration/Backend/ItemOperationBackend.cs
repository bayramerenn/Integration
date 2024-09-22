using System.Collections.Concurrent;
using Integration.Common;

namespace Integration.Backend;

public sealed class ItemOperationBackend
{
    private ConcurrentBag<Item> SavedItems { get; set; } = new();
    private int _identitySequence;

    public Item SaveItem(string itemContent)
    {
        Thread.Sleep(2_000);
        
        var item = new Item();
        item.Content = itemContent;
        item.Id = GetNextIdentity();
        SavedItems.Add(item);
        return item;
    }

    public List<Item> FindItemsWithContent(string itemContent)
    {
        return SavedItems.Where(x => x.Content == itemContent).ToList();
    }

    private int GetNextIdentity()
    {
        return Interlocked.Increment(ref _identitySequence);
    }

    public List<Item> GetAllItems()
    {
        return SavedItems.ToList();
    }
}
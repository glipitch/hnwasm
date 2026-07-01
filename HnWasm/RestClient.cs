using System.Net.Http.Json;

namespace HnWasm;

internal class RestClient
{
    readonly HttpClient client;
    readonly Dictionary<int, Task<ItemFetch>> itemTasks = new();

    public RestClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<int[]?> GetTopStories()
    {
        try
        {
            var result = await client.GetFromJsonAsync<int[]>("topstories.json");
            return result;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Item?> GetItem(int id) => (await GetItemFetch(id)).Item;

    public Task<ItemFetch> GetItemFetch(int id)
    {
        if (!itemTasks.TryGetValue(id, out var task))
        {
            task = FetchItem(id);
            itemTasks[id] = task;
        }
        return task;
    }

    public void PrefetchItems(IEnumerable<int> ids)
    {
        foreach (var id in ids)
        {
            _ = GetItem(id);
        }
    }

    async Task<ItemFetch> FetchItem(int id)
    {
        try
        {
            return new(await client.GetFromJsonAsync<Item>($"item/{id}.json"), Failed: false);
        }
        catch (OperationCanceledException)
        {
            itemTasks.Remove(id);
            return new(null, Failed: true);
        }
        catch
        {
            itemTasks.Remove(id);
            return new(null, Failed: true);
        }
    }
}

internal record ItemFetch(Item? Item, bool Failed);

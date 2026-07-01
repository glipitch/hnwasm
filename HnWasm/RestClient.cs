using System.Net.Http.Json;

namespace HnWasm;

internal class RestClient
{
    readonly HttpClient client;
    readonly ErrorState errorState;
    readonly Dictionary<int, Task<Item?>> itemTasks = new();

    public RestClient(HttpClient client, ErrorState errorState)
    {
        this.client = client;
        this.errorState = errorState;
    }

    public async Task<int[]?> GetTopStories()
    {
        try
        {
            var result = await client.GetFromJsonAsync<int[]>("topstories.json");
            return result;
        }
        catch
        {
            errorState.SetError(true);
            return null;
        }
    }

    public Task<Item?> GetItem(int id)
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

    async Task<Item?> FetchItem(int id)
    {
        try
        {
            return await client.GetFromJsonAsync<Item>($"item/{id}.json");
        }
        catch
        {
            itemTasks.Remove(id);
            errorState.SetError(true);
            return null;
        }
    }
}

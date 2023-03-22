using System.Net.Http.Json;

namespace HnWasm;

internal class RestClient
{
    readonly HttpClient client;
    private readonly ErrorState? errorState;

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
            errorState?.SetError(true);
            return null;
        }
    }

    public async Task<Item?> GetItem(int id)
    {
        try
        {
            var result = await client.GetFromJsonAsync<Item>($"item/{id}.json");
            return result;
        }
        catch
        {
            errorState?.SetError(true);
            return null;
        }
    }
}
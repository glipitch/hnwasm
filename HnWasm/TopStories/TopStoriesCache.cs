namespace HnWasm.TopStories;

internal class TopStoriesCache
{
    readonly RestClient rest;
    Task<IndexItem[]?>? storiesTask;

    public TopStoriesCache(RestClient rest)
    {
        this.rest = rest;
    }

    public Task<IndexItem[]?> Get()
    {
        storiesTask ??= Fetch();
        return storiesTask;
    }

    async Task<IndexItem[]?> Fetch()
    {
        var stories = await rest.GetTopStories();
        if (stories is null)
        {
            storiesTask = null;
            return null;
        }
        return stories.Select((id, ordinal) => new IndexItem(id, ordinal + 1)).ToArray();
    }
}

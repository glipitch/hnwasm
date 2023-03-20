namespace HnWasm.TopStories;

internal class TopStoriesCache
{
    readonly RestClient rest;
    IndexItem[]? stories;

    public TopStoriesCache(RestClient rest)
    {
        this.rest = rest;
    }

    internal async Task<IndexItem[]?> Get()
    {
        if (stories == null)
        {
            var newStories = await rest.GetTopStories();
            stories = newStories!.Select((id, ordinal) => new IndexItem(id, ordinal + 1)).ToArray();
        }
        return stories;
    }
}
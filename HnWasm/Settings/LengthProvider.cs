using Microsoft.JSInterop;

namespace HnWasm.Settings;

internal class LengthProvider
{
    readonly IJSRuntime js;

    public LengthProvider(IJSRuntime js) => this.js = js;

    const int defaultValue = 10;
    static readonly int[] values = new int[] { 10, 25, 50 };

    public async Task<int> Get()
    {
        var text = await js.InvokeAsync<string?>("localStorage.getItem", "length");
        if (text is not null && int.TryParse(text, out var number) && values.Contains(number))
        {
            return number;
        }
        return defaultValue;
    }

    public async Task Next(int current)
    {
        var index = Array.IndexOf(values, current);
        var next = index < values.Length - 1 ? values[index + 1] : values[0];
        await js.InvokeVoidAsync("localStorage.setItem", "length", next);
    }
}
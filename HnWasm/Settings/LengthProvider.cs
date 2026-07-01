using Microsoft.JSInterop;

namespace HnWasm.Settings;

internal class LengthProvider
{
    readonly IJSRuntime js;
    int? value;

    public LengthProvider(IJSRuntime js) => this.js = js;

    public event Action? Changed;

    const int defaultValue = 10;
    static readonly int[] values = new int[] { 10, 25, 50 };

    public async Task<int> Get()
    {
        if (value is not null)
        {
            return value.Value;
        }
        var text = await js.InvokeAsync<string?>("localStorage.getItem", "length");
        if (text is not null && int.TryParse(text, out var number) && values.Contains(number))
        {
            value = number;
            return value.Value;
        }
        value = defaultValue;
        return value.Value;
    }

    public async Task<int> Next(int current)
    {
        var index = Array.IndexOf(values, current);
        var next = index < values.Length - 1 ? values[index + 1] : values[0];
        value = next;
        await js.InvokeVoidAsync("localStorage.setItem", "length", next);
        return next;
    }

    public void NotifyChanged() => Changed?.Invoke();
}

using Microsoft.JSInterop;

namespace HnWasm.Settings;

internal class ThemeProvider
{
    readonly IJSRuntime js;

    public ThemeProvider(IJSRuntime js) => this.js = js;

    public async Task<string?> Get()
    {
        var theme = await js.InvokeAsync<string?>("localStorage.getItem", "theme");
        return theme == "light" ? "light" : "dark";
    }

    public async Task<string?> Next(string? original)
    {
        var next = original == "light" ? "dark" : "light";
        await js.InvokeVoidAsync("setThemeData", next);
        await js.InvokeVoidAsync("localStorage.setItem", "theme", next);
        return next;
    }
}
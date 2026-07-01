using Microsoft.JSInterop;

namespace HnWasm.Settings;

internal class ThemeProvider
{
    readonly IJSRuntime js;
    string? theme;

    public ThemeProvider(IJSRuntime js) => this.js = js;

    public async Task<string?> Get()
    {
        if (theme is not null)
        {
            return theme;
        }
        var stored = await js.InvokeAsync<string?>("localStorage.getItem", "theme");
        theme = stored == "light" ? "light" : "dark";
        return theme;
    }

    public async Task<string?> Next(string? original)
    {
        var next = original == "light" ? "dark" : "light";
        theme = next;
        await js.InvokeVoidAsync("setThemeData", next);
        await js.InvokeVoidAsync("localStorage.setItem", "theme", next);
        return next;
    }
}

using Microsoft.AspNetCore.Components;

namespace HnWasm.Navigation;

internal class NavigationService
{
    readonly NavigationManager navigationManager;

    string? previous, next;

    public NavigationService(NavigationManager navigationManager)
    {
        this.navigationManager = navigationManager;
    }

    internal event Action<string?, string?>? UrlsSet;

    internal void SetUrls(string? previous, string? next)
    {
        this.previous = previous;
        this.next = next;
        UrlsSet?.Invoke(previous, next);
    }

    internal void RightToLeft()
    {
        if (next != null)
        {
            navigationManager.NavigateTo(next);
            return;
        }
    }

    internal void LeftToRight()
    {
        if (previous != null)
        {
            navigationManager.NavigateTo(previous);
        }
    }
}
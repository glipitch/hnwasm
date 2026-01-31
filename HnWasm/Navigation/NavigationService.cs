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

    public event Action<string?, string?>? UrlsSet;

    public void SetUrls(string? previous, string? next)
    {
        this.previous = previous;
        this.next = next;
        UrlsSet?.Invoke(previous, next);
    }

    public void RightToLeft()
    {
        if (next != null)
        {
            navigationManager.NavigateTo(next);
            return;
        }
    }

    public void LeftToRight()
    {
        if (previous != null)
        {
            navigationManager.NavigateTo(previous);
        }
    }
}
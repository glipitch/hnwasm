namespace HnWasm.Helpers;

internal static class UrlExtensions
{
    public static string UrlToHuman(this string url)
    {
        var uri = new Uri(url.ToLowerInvariant());
        var host = uri.Host;
        if (host == "github.com" || host == "twitter.com")
        {
            var segments = uri.Segments;
            if (segments.Length < 2)
            {
                return host;
            }
            return $"{host}/{segments[1].Trim('/')}";
        }
        var parts = host.Split('.');
        if (parts.Length > 0 && parts[0] == "www")
        {
            parts = parts.Skip(1).ToArray();
        }
        return string.Join(".", parts);
    }
}
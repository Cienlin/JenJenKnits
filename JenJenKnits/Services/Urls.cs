namespace JenJenKnits.Services;

public static class Urls
{
    /// <summary>是否為 http:// 或 https:// 開頭的絕對網址（賣貨便連結的格式要求）。</summary>
    public static bool IsHttp(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

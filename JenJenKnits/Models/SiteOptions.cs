namespace JenJenKnits.Models;

/// <summary>appsettings.json 的 "Site" 區段：全站共用的對外連結。</summary>
public class SiteOptions
{
    public const string SectionName = "Site";

    /// <summary>空白 = 還沒有 IG，網站會自動隱藏相關按鈕。</summary>
    public string? InstagramUrl { get; set; }

    public string? Instagram => string.IsNullOrWhiteSpace(InstagramUrl) ? null : InstagramUrl.Trim();

    /// <summary>
    /// 網站對外的完整網址（不含結尾斜線），社群分享、canonical、sitemap 的絕對網址都由它組成。
    /// 目前是 GitHub Pages；換主機時只改這個設定。
    /// </summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>站內路徑（以 / 開頭）→ 對外的絕對網址。</summary>
    public string Absolute(string path) => BaseUrl.TrimEnd('/') + path;

    /// <summary>
    /// 頁面的對外網址（canonical、sitemap 用）：結尾一律加斜線。
    /// GitHub Pages 上每一頁是「資料夾/index.html」，不加斜線會被轉址；ASP.NET 兩種寫法都能開。
    /// </summary>
    public string PageUrl(string path) => Absolute(path.EndsWith('/') ? path : path + "/");
}

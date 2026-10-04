namespace JenJenKnits.Models;

/// <summary>appsettings.json 的 "Site" 區段：全站共用的對外連結。</summary>
public class SiteOptions
{
    public const string SectionName = "Site";

    /// <summary>空白 = 還沒有 IG，網站會自動隱藏相關按鈕。</summary>
    public string? InstagramUrl { get; set; }

    public string? Instagram => string.IsNullOrWhiteSpace(InstagramUrl) ? null : InstagramUrl.Trim();
}

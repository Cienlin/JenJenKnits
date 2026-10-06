namespace JenJenKnits.Data;

/// <summary>
/// 商品的文字資料（資料庫中的一列），在後台編輯。
/// 照片不在這裡：依 Slug 到 wwwroot/products/{slug}/ 找 cover、01、02…、colors。
/// </summary>
public class ProductRecord
{
    public int Id { get; set; }

    /// <summary>網址代稱，也是照片資料夾名稱；建立後不可修改。</summary>
    public required string Slug { get; set; }

    public required string Name { get; set; }
    public string? Category { get; set; }
    public int? Price { get; set; }
    public string? Material { get; set; }
    public string? Dimensions { get; set; }
    public string? ShortDescription { get; set; }

    /// <summary>賣貨便連結；必須是 http(s) 網址。</summary>
    public string? BuyUrl { get; set; }

    /// <summary>首頁精選排序，愈小愈前；null = 不上精選。</summary>
    public int? Featured { get; set; }

    /// <summary>配色模擬器的造型，目前只有 "puff-flower"。</summary>
    public string? ColorSimulator { get; set; }

    /// <summary>故事原文（Markdown），HTML 註解可當作者備註，前台不會顯示。</summary>
    public string? StoryMarkdown { get; set; }

    /// <summary>false = 草稿，前台看不到。</summary>
    public bool IsPublished { get; set; }

    /// <summary>最後修改時間（UTC）。</summary>
    public DateTime UpdatedAt { get; set; }
}

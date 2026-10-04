namespace JenJenKnits.Models;

/// <summary>一件商品 = wwwroot/products/{Slug}/ 一個資料夾。圖片路徑皆為網站根目錄起算的 URL。</summary>
public class Product
{
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public string? Category { get; init; }
    public int? Price { get; init; }
    public string? Material { get; init; }
    public string? Dimensions { get; init; }
    public string? ShortDescription { get; init; }

    /// <summary>賣貨便連結；null = 還沒上架賣貨便，商品照樣顯示。</summary>
    public string? BuyUrl { get; init; }

    /// <summary>首頁精選排序，愈小愈前；null = 不上精選。</summary>
    public int? Featured { get; init; }

    public required string CoverImagePath { get; init; }
    public IReadOnlyList<string> GalleryImagePaths { get; init; } = [];

    /// <summary>colors.* 色卡圖；有的話商品頁顯示「可選顏色」區塊。</summary>
    public string? ColorChartImagePath { get; init; }

    /// <summary>配色模擬器的造型，目前只有 "puff-flower"；null = 不顯示模擬器。</summary>
    public string? ColorSimulator { get; init; }

    public string? StoryHtml { get; init; }

    /// <summary>作品卡片滑鼠移上去時換上的照片（第一張 gallery）。</summary>
    public string? HoverImagePath => GalleryImagePaths.Count > 0 ? GalleryImagePaths[0] : null;

    /// <summary>作品卡片照片與商品頁大圖共用的 view-transition-name，換頁時照片會平滑放大。</summary>
    public string ViewTransitionName =>
        "product-" + string.Concat(Slug.Select(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '-'));
}

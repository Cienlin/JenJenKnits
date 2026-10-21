using JenJenKnits.Data;

namespace JenJenKnits.Services;

/// <summary>
/// 商品資料完成度：哪些欄位已填、哪些待補。後台列表與編輯頁共用同一份規則。
/// 沒填的欄位前台會省略，不影響發布；這裡只是提醒要補。
/// </summary>
public static class ProductCompleteness
{
    /// <param name="Label">畫面上的名稱。</param>
    /// <param name="Field">對應的表單欄位（Input.{Field}），編輯頁用來連到該欄位。</param>
    public record Item(string Label, string Field, bool IsFilled);

    public static IReadOnlyList<Item> Check(ProductRecord p) =>
        Check(p.Price, p.Material, p.Dimensions, p.ShortDescription, p.StoryMarkdown, p.BuyUrl);

    public static IReadOnlyList<Item> Check(int? price, string? material, string? dimensions, string? shortDescription, string? storyMarkdown, string? buyUrl) =>
    [
        new("價格", "Price", price is not null),
        new("材質", "Material", Filled(material)),
        new("尺寸", "Dimensions", Filled(dimensions)),
        new("簡介", "ShortDescription", Filled(shortDescription)),
        // 只有註解的故事在前台是空的，也算沒填
        new("故事", "StoryMarkdown", StoryRenderer.ToHtml(storyMarkdown) is not null),
        new("賣貨便連結", "BuyUrl", Filled(buyUrl)),
    ];

    private static bool Filled(string? value) => !string.IsNullOrWhiteSpace(value);
}

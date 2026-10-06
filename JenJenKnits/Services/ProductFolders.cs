using System.Text.RegularExpressions;

namespace JenJenKnits.Services;

/// <summary>一件商品的照片網址；沒有 cover 時 CoverPath 為 null。</summary>
public record ProductImages(string? CoverPath, IReadOnlyList<string> GalleryPaths, string? ColorChartPath);

/// <summary>
/// 商品照片資料夾：{Root}/{slug}/ 裡的 cover.*（主圖）、01.*、02.*…（gallery）、colors.*（色卡）。
/// Root 預設是 wwwroot/products；測試時指到暫存資料夾，不會動到真正的商品檔案。
/// </summary>
public partial class ProductFolders(string root)
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public string Root { get; } = root;

    public string PathOf(string slug) => Path.Combine(Root, slug);

    public ProductImages FindImages(string slug)
    {
        var dir = PathOf(slug);
        if (!Directory.Exists(dir))
        {
            return new ProductImages(null, [], null);
        }

        var images = Directory.EnumerateFiles(dir)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToList();

        var cover = images.FirstOrDefault(f => HasStem(f, "cover"));
        var colorChart = images.FirstOrDefault(f => HasStem(f, "colors"));

        // 01.jpg, 02.jpg ... 依數字排序；補零後比較，1 / 2 / 10 也能排對
        var gallery = images
            .Where(f => NumberedStem().IsMatch(Path.GetFileNameWithoutExtension(f)))
            .OrderBy(f => Path.GetFileNameWithoutExtension(f).PadLeft(10, '0'), StringComparer.Ordinal)
            .Select(f => UrlOf(slug, f))
            .ToList();

        return new ProductImages(
            cover is null ? null : UrlOf(slug, cover),
            gallery,
            colorChart is null ? null : UrlOf(slug, colorChart));
    }

    private static bool HasStem(string fileName, string stem) =>
        string.Equals(Path.GetFileNameWithoutExtension(fileName), stem, StringComparison.OrdinalIgnoreCase);

    // 網址固定是 /products/...，和照片實際放在哪個資料夾無關
    private static string UrlOf(string slug, string fileName) =>
        $"/products/{Uri.EscapeDataString(slug)}/{Uri.EscapeDataString(fileName)}";

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex NumberedStem();
}

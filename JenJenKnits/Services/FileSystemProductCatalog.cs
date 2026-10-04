using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JenJenKnits.Models;
using Markdig;

namespace JenJenKnits.Services;

/// <summary>
/// 掃描 wwwroot/products/{slug}/ 建立商品清單。資料夾名即 slug，底線開頭視為草稿。
/// 單一商品資料有誤只會跳過該商品並記 warning，不影響其他商品。
/// </summary>
public partial class FileSystemProductCatalog : IProductCatalog
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    // story.md 不渲染內嵌 HTML（HTML 註解會先被移除，可當作者備註用）；單一換行即換行，中文才不會多出空白
    private static readonly MarkdownPipeline StoryPipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    private readonly ILogger<FileSystemProductCatalog> _logger;
    private readonly IReadOnlyList<Product> _products;

    public FileSystemProductCatalog(IWebHostEnvironment env, ILogger<FileSystemProductCatalog> logger)
    {
        _logger = logger;
        _products = Scan(Path.Combine(env.WebRootPath, "products"));
    }

    public IReadOnlyList<Product> GetAll() => _products;

    public IReadOnlyList<Product> GetFeatured() => _products.Where(p => p.Featured is not null).ToList();

    public IReadOnlyList<Product> GetByCategory(string category) =>
        _products.Where(p => p.Category == category).ToList();

    public Product? GetBySlug(string slug) =>
        _products.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> GetAllCategories() =>
        _products.Select(p => p.Category).OfType<string>().Distinct().ToList();

    private IReadOnlyList<Product> Scan(string root)
    {
        if (!Directory.Exists(root))
        {
            _logger.LogWarning("找不到商品資料夾 {Root}", root);
            return [];
        }

        var products = new List<Product>();
        foreach (var dir in Directory.EnumerateDirectories(root))
        {
            var slug = Path.GetFileName(dir);
            if (slug.StartsWith('_') || slug.StartsWith('.'))
            {
                continue;
            }

            var product = TryLoad(dir, slug);
            if (product is not null)
            {
                products.Add(product);
            }
        }

        return products
            .OrderBy(p => p.Featured is null)
            .ThenBy(p => p.Featured)
            .ThenBy(p => p.Slug, StringComparer.Ordinal)
            .ToList();
    }

    private Product? TryLoad(string dir, string slug)
    {
        var metaPath = Path.Combine(dir, "meta.json");
        if (!File.Exists(metaPath))
        {
            _logger.LogWarning("略過商品 {Slug}：缺少 meta.json", slug);
            return null;
        }

        ProductMeta? meta;
        try
        {
            meta = JsonSerializer.Deserialize<ProductMeta>(File.ReadAllText(metaPath), JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("略過商品 {Slug}：meta.json 格式錯誤（{Error}）", slug, ex.Message);
            return null;
        }

        var name = Clean(meta?.Name);
        if (meta is null || name is null)
        {
            _logger.LogWarning("略過商品 {Slug}：meta.json 缺少 name", slug);
            return null;
        }

        var images = Directory.EnumerateFiles(dir)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToList();

        var cover = images.FirstOrDefault(f => HasStem(f, "cover"));
        if (cover is null)
        {
            _logger.LogWarning("略過商品 {Slug}：缺少 cover 圖（cover.jpg / .jpeg / .png / .webp）", slug);
            return null;
        }

        // 01.jpg, 02.jpg ... 依數字排序；補零後比較，1 / 2 / 10 也能排對
        var gallery = images
            .Where(f => NumberedStem().IsMatch(Path.GetFileNameWithoutExtension(f)))
            .OrderBy(f => Path.GetFileNameWithoutExtension(f).PadLeft(10, '0'), StringComparer.Ordinal)
            .Select(f => ImagePath(slug, f))
            .ToList();

        var colorChart = images.FirstOrDefault(f => HasStem(f, "colors"));

        var buyUrl = Clean(meta.BuyUrl);
        if (buyUrl is not null && !IsHttpUrl(buyUrl))
        {
            _logger.LogWarning("商品 {Slug} 的 buyUrl 不是有效網址，先當作沒有連結：{BuyUrl}", slug, buyUrl);
            buyUrl = null;
        }

        return new Product
        {
            Slug = slug,
            Name = name,
            Category = Clean(meta.Category),
            Price = meta.Price > 0 ? meta.Price : null,
            Material = Clean(meta.Material),
            Dimensions = Clean(meta.Dimensions),
            ShortDescription = Clean(meta.ShortDescription),
            BuyUrl = buyUrl,
            Featured = meta.Featured,
            CoverImagePath = ImagePath(slug, cover),
            GalleryImagePaths = gallery,
            ColorChartImagePath = colorChart is null ? null : ImagePath(slug, colorChart),
            ColorSimulator = Clean(meta.ColorSimulator),
            StoryHtml = RenderStory(Path.Combine(dir, "story.md")),
        };
    }

    private static string? RenderStory(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var markdown = HtmlComment().Replace(File.ReadAllText(path), string.Empty);
        return string.IsNullOrWhiteSpace(markdown) ? null : Markdown.ToHtml(markdown, StoryPipeline);
    }

    private static bool HasStem(string fileName, string stem) =>
        string.Equals(Path.GetFileNameWithoutExtension(fileName), stem, StringComparison.OrdinalIgnoreCase);

    private static string ImagePath(string slug, string fileName) =>
        $"/products/{Uri.EscapeDataString(slug)}/{Uri.EscapeDataString(fileName)}";

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlComment();

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex NumberedStem();

    private sealed class ProductMeta
    {
        public string? Name { get; init; }
        public string? Category { get; init; }
        public int? Price { get; init; }
        public string? Material { get; init; }
        public string? Dimensions { get; init; }
        public string? ShortDescription { get; init; }
        public string? BuyUrl { get; init; }
        public int? Featured { get; init; }
        public string? ColorSimulator { get; init; }
    }
}

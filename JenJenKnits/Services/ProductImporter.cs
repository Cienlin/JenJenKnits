using System.Text.Json;
using System.Text.Json.Serialization;
using JenJenKnits.Data;

namespace JenJenKnits.Services;

/// <summary>
/// 一次性匯入：資料庫還沒有任何商品時，把各資料夾的 meta.json + story.md 轉進資料庫，
/// 成功後刪除這兩種檔案，之後只在後台編輯（只留一份資料，才不會不同步）。
/// 底線開頭的資料夾是草稿，略過；單一資料夾有誤只略過它並記 warning。
/// </summary>
public class ProductImporter(SiteDbContext db, ProductFolders folders, ILogger<ProductImporter> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>回傳匯入的商品數；資料庫已有商品時不做任何事。</summary>
    public int ImportIfEmpty()
    {
        if (db.Products.Any() || !Directory.Exists(folders.Root))
        {
            return 0;
        }

        var importedDirs = new List<string>();
        foreach (var dir in Directory.EnumerateDirectories(folders.Root).Order(StringComparer.Ordinal))
        {
            var slug = Path.GetFileName(dir);
            if (slug.StartsWith('_') || slug.StartsWith('.') || !File.Exists(Path.Combine(dir, "meta.json")))
            {
                continue;
            }

            var record = TryRead(dir, slug);
            if (record is not null)
            {
                db.Products.Add(record);
                importedDirs.Add(dir);
            }
        }

        db.SaveChanges();

        foreach (var dir in importedDirs)
        {
            File.Delete(Path.Combine(dir, "meta.json"));
            File.Delete(Path.Combine(dir, "story.md"));
        }

        if (importedDirs.Count > 0)
        {
            logger.LogInformation("已從商品資料夾匯入 {Count} 件商品，並刪除其 meta.json 與 story.md", importedDirs.Count);
        }

        return importedDirs.Count;
    }

    private ProductRecord? TryRead(string dir, string slug)
    {
        ProductMeta? meta;
        try
        {
            meta = JsonSerializer.Deserialize<ProductMeta>(File.ReadAllText(Path.Combine(dir, "meta.json")), JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning("略過商品 {Slug}：meta.json 格式錯誤（{Error}）", slug, ex.Message);
            return null;
        }

        var name = Clean(meta?.Name);
        if (meta is null || name is null)
        {
            logger.LogWarning("略過商品 {Slug}：meta.json 缺少 name", slug);
            return null;
        }

        var buyUrl = Clean(meta.BuyUrl);
        if (buyUrl is not null && !Urls.IsHttp(buyUrl))
        {
            logger.LogWarning("商品 {Slug} 的 buyUrl 不是有效網址，匯入時略過：{BuyUrl}", slug, buyUrl);
            buyUrl = null;
        }

        var storyPath = Path.Combine(dir, "story.md");
        var story = File.Exists(storyPath) ? File.ReadAllText(storyPath) : null;

        return new ProductRecord
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
            ColorSimulator = Clean(meta.ColorSimulator),
            StoryMarkdown = string.IsNullOrWhiteSpace(story) ? null : story.Trim(),
            IsPublished = true,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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

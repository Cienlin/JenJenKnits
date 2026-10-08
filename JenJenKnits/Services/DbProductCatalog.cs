using JenJenKnits.Data;
using JenJenKnits.Models;
using Microsoft.EntityFrameworkCore;

namespace JenJenKnits.Services;

/// <summary>
/// 前台的商品來源：資料庫中已發布的商品，照片依 slug 到商品資料夾找。
/// 已發布但缺 cover 的商品不顯示並記 warning。每個請求建立一次（Scoped），第一次用到時才讀資料庫。
/// </summary>
public class DbProductCatalog(SiteDbContext db, ProductFolders folders, ILogger<DbProductCatalog> logger) : IProductCatalog
{
    private IReadOnlyList<Product>? _products;

    private IReadOnlyList<Product> Products => _products ??= Load();

    public IReadOnlyList<Product> GetAll() => Products;

    public IReadOnlyList<Product> GetFeatured() => Products.Where(p => p.Featured is not null).ToList();

    public IReadOnlyList<Product> GetByCategory(string category) =>
        Products.Where(p => p.Category == category).ToList();

    public Product? GetBySlug(string slug) =>
        Products.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> GetAllCategories() =>
        Products.Select(p => p.Category).OfType<string>().Distinct().ToList();

    private IReadOnlyList<Product> Load()
    {
        var products = new List<Product>();
        foreach (var record in db.Products.AsNoTracking().Where(p => p.IsPublished).ToList())
        {
            var images = folders.FindImages(record.Slug);
            if (images.CoverPath is null)
            {
                logger.LogWarning("商品 {Slug} 已發布但缺少 cover 圖，前台先不顯示", record.Slug);
                continue;
            }

            products.Add(new Product
            {
                Slug = record.Slug,
                Name = record.Name,
                Category = record.Category,
                Price = record.Price,
                Material = record.Material,
                Dimensions = record.Dimensions,
                ShortDescription = record.ShortDescription,
                BuyUrl = record.BuyUrl,
                Featured = record.Featured,
                CoverImagePath = images.CoverPath,
                GalleryImagePaths = images.GalleryPaths,
                ColorChartImagePath = images.ColorChartPath,
                ColorSimulator = record.ColorSimulator,
                StoryHtml = StoryRenderer.ToHtml(record.StoryMarkdown),
                UpdatedAt = record.UpdatedAt,
            });
        }

        return ProductOrder.Sort(products, p => p.Featured, p => p.Slug);
    }
}

/// <summary>商品排列順序：精選依 featured 升冪在前，其餘依 slug。前台與後台列表共用。</summary>
public static class ProductOrder
{
    public static List<T> Sort<T>(IEnumerable<T> items, Func<T, int?> featured, Func<T, string> slug) =>
        items
            .OrderBy(i => featured(i) is null)
            .ThenBy(featured)
            .ThenBy(slug, StringComparer.Ordinal)
            .ToList();
}

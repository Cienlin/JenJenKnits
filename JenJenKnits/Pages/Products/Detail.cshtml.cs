using JenJenKnits.Models;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace JenJenKnits.Pages.Products;

public class DetailModel : PageModel
{
    private readonly IProductCatalog _catalog;
    private readonly SiteOptions _site;

    public DetailModel(IProductCatalog catalog, IOptions<SiteOptions> site)
    {
        _catalog = catalog;
        _site = site.Value;
    }

    public Product Product { get; private set; } = default!;

    /// <summary>主圖在前，接著 01、02…，用於縮圖列。</summary>
    public IReadOnlyList<string> Images { get; private set; } = [];

    /// <summary>在全部商品中的序號（從 1 開始），顯示為 No. 01。</summary>
    public int Number { get; private set; }

    /// <summary>頁尾「下一件作品」：依全部商品的順序，最後一件之後接回第一件；只有一件商品時為 null。</summary>
    public Product? Next { get; private set; }

    public string? Instagram => _site.Instagram;

    public SiteOptions Site => _site;

    public IActionResult OnGet(string slug)
    {
        var product = _catalog.GetBySlug(slug);
        if (product is null)
        {
            return NotFound();
        }

        Product = product;
        Images = [product.CoverImagePath, .. product.GalleryImagePaths];

        var all = _catalog.GetAll();
        var index = all.ToList().FindIndex(p => p.Slug == product.Slug);
        Number = index + 1;
        Next = all.Count > 1 ? all[(index + 1) % all.Count] : null;

        return Page();
    }
}

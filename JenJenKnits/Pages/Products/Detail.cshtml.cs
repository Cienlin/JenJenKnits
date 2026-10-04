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

    public string? Instagram => _site.Instagram;

    public IActionResult OnGet(string slug)
    {
        var product = _catalog.GetBySlug(slug);
        if (product is null)
        {
            return NotFound();
        }

        Product = product;
        Images = [product.CoverImagePath, .. product.GalleryImagePaths];
        return Page();
    }
}

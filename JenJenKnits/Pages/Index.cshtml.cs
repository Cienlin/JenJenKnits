using JenJenKnits.Models;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace JenJenKnits.Pages;

public class IndexModel : PageModel
{
    // 精選少於 3 件時，用其他商品補到最多 6 件
    private const int MinFeatured = 3;
    private const int FeaturedFillTo = 6;

    private readonly IProductCatalog _catalog;
    private readonly SiteOptions _site;

    public IndexModel(IProductCatalog catalog, IOptions<SiteOptions> site)
    {
        _catalog = catalog;
        _site = site.Value;
    }

    public IReadOnlyList<Product> Featured { get; private set; } = [];

    /// <summary>至少一件商品有賣貨便連結 = 已開賣。</summary>
    public bool IsOnSale { get; private set; }

    public string? Instagram => _site.Instagram;

    public SiteOptions Site => _site;

    public void OnGet()
    {
        var featured = _catalog.GetFeatured();
        if (featured.Count < MinFeatured)
        {
            featured = featured
                .Concat(_catalog.GetAll().Where(p => p.Featured is null))
                .Take(FeaturedFillTo)
                .ToList();
        }

        Featured = featured;
        IsOnSale = _catalog.GetAll().Any(p => p.BuyUrl is not null);
    }
}

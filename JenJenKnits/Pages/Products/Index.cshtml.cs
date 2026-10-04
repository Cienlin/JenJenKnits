using JenJenKnits.Models;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JenJenKnits.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductCatalog _catalog;

    public IndexModel(IProductCatalog catalog)
    {
        _catalog = catalog;
    }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    public IReadOnlyList<string> Categories { get; private set; } = [];

    public IReadOnlyList<Product> Products { get; private set; } = [];

    public void OnGet()
    {
        Categories = _catalog.GetAllCategories();
        Products = string.IsNullOrEmpty(Category) ? _catalog.GetAll() : _catalog.GetByCategory(Category);
    }
}

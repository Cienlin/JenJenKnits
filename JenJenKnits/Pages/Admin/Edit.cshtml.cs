using System.ComponentModel.DataAnnotations;
using JenJenKnits.Data;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JenJenKnits.Pages.Admin;

public class EditModel(SiteDbContext db, ProductFolders folders) : PageModel
{
    /// <summary>配色模擬器目前支援的造型；空字串 = 不顯示。</summary>
    public static readonly IReadOnlyList<(string Value, string Label)> Simulators =
    [
        ("", "不顯示"),
        ("puff-flower", "泡芙小花"),
    ];

    [BindProperty]
    public ProductInput Input { get; set; } = new();

    public string Slug { get; private set; } = "";

    public ProductImages Images { get; private set; } = new(null, [], null);

    public IReadOnlyList<string> Categories { get; private set; } = [];

    public IActionResult OnGet(string slug)
    {
        var product = db.Products.AsNoTracking().FirstOrDefault(p => p.Slug == slug);
        if (product is null)
        {
            return NotFound();
        }

        Input = ProductInput.From(product);
        LoadPageData(slug);
        return Page();
    }

    public IActionResult OnPost(string slug)
    {
        var product = db.Products.FirstOrDefault(p => p.Slug == slug);
        if (product is null)
        {
            return NotFound();
        }

        var buyUrl = Clean(Input.BuyUrl);
        if (buyUrl is not null && !Urls.IsHttp(buyUrl))
        {
            ModelState.AddModelError("Input.BuyUrl", "賣貨便連結要是 http:// 或 https:// 開頭的網址");
        }

        if (!Simulators.Any(s => s.Value == (Input.ColorSimulator ?? "")))
        {
            ModelState.AddModelError("Input.ColorSimulator", "請從清單中選擇");
        }

        if (!ModelState.IsValid)
        {
            // 有錯就整份不存，保留已輸入的內容
            LoadPageData(slug);
            return Page();
        }

        product.Name = Input.Name.Trim();
        product.Category = Clean(Input.Category);
        product.Price = Input.Price;
        product.Material = Clean(Input.Material);
        product.Dimensions = Clean(Input.Dimensions);
        product.ShortDescription = Clean(Input.ShortDescription);
        product.BuyUrl = buyUrl;
        product.Featured = Input.Featured;
        product.ColorSimulator = Clean(Input.ColorSimulator);
        product.StoryMarkdown = Clean(Input.StoryMarkdown);
        product.IsPublished = Input.IsPublished;
        product.UpdatedAt = DateTime.UtcNow;
        db.SaveChanges();

        TempData["Notice"] = "已儲存";
        return RedirectToPage(new { slug });
    }

    /// <summary>故事預覽：用和前台相同的渲染器，只回傳 HTML 片段，不寫入資料庫。</summary>
    public IActionResult OnPostPreview([FromForm(Name = "Input.StoryMarkdown")] string? story) =>
        Content(StoryRenderer.ToHtml(story) ?? "<p class=\"hint\">故事是空的，前台不會顯示故事區塊。</p>", "text/html; charset=utf-8");

    private void LoadPageData(string slug)
    {
        Slug = slug;
        Images = folders.FindImages(slug);
        Categories = db.Products.Select(p => p.Category).OfType<string>().Distinct().OrderBy(c => c).ToList();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public class ProductInput
    {
        [Required(ErrorMessage = "請輸入名稱")]
        [StringLength(100, ErrorMessage = "最多 {1} 字")]
        public string Name { get; set; } = "";

        [StringLength(50, ErrorMessage = "最多 {1} 字")]
        public string? Category { get; set; }

        [Range(1, 1_000_000, ErrorMessage = "價格要是正整數")]
        public int? Price { get; set; }

        [StringLength(200, ErrorMessage = "最多 {1} 字")]
        public string? Material { get; set; }

        [StringLength(200, ErrorMessage = "最多 {1} 字")]
        public string? Dimensions { get; set; }

        [StringLength(500, ErrorMessage = "最多 {1} 字")]
        public string? ShortDescription { get; set; }

        [StringLength(500, ErrorMessage = "最多 {1} 字")]
        public string? BuyUrl { get; set; }

        [Range(1, 999, ErrorMessage = "精選排序要是正整數")]
        public int? Featured { get; set; }

        public string? ColorSimulator { get; set; }

        public string? StoryMarkdown { get; set; }

        public bool IsPublished { get; set; }

        public static ProductInput From(ProductRecord p) => new()
        {
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            Material = p.Material,
            Dimensions = p.Dimensions,
            ShortDescription = p.ShortDescription,
            BuyUrl = p.BuyUrl,
            Featured = p.Featured,
            ColorSimulator = p.ColorSimulator,
            StoryMarkdown = p.StoryMarkdown,
            IsPublished = p.IsPublished,
        };
    }
}

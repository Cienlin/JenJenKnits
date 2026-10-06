using JenJenKnits.Data;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JenJenKnits.Pages.Admin;

public class IndexModel(SiteDbContext db, ProductFolders folders) : PageModel
{
    public IReadOnlyList<Row> Rows { get; private set; } = [];

    public record Row(ProductRecord Product, ProductImages Images, IReadOnlyList<string> Missing);

    public void OnGet()
    {
        // 順序和前台相同，草稿也列出來
        var records = ProductOrder.Sort(db.Products.AsNoTracking().ToList(), p => p.Featured, p => p.Slug);
        Rows = records.Select(p => new Row(p, folders.FindImages(p.Slug), MissingFields(p))).ToList();
    }

    /// <summary>還沒填的欄位，列表上提醒要補。</summary>
    private static IReadOnlyList<string> MissingFields(ProductRecord p) =>
        new (string Label, bool IsMissing)[]
        {
            ("價格", p.Price is null),
            ("材質", p.Material is null),
            ("尺寸", p.Dimensions is null),
            ("簡介", p.ShortDescription is null),
            ("故事", StoryRenderer.ToHtml(p.StoryMarkdown) is null),
            ("賣貨便連結", p.BuyUrl is null),
        }
        .Where(f => f.IsMissing)
        .Select(f => f.Label)
        .ToList();
}

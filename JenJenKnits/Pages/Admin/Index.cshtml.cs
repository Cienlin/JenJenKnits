using JenJenKnits.Data;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JenJenKnits.Pages.Admin;

public class IndexModel(SiteDbContext db, ProductFolders folders) : PageModel
{
    public IReadOnlyList<Row> Rows { get; private set; } = [];

    public int PublishedCount => Rows.Count(r => r.Product.IsPublished);

    public int DraftCount => Rows.Count(r => !r.Product.IsPublished);

    /// <summary>有欄位待補的件數。</summary>
    public int IncompleteCount => Rows.Count(r => r.Missing.Count > 0);

    /// <param name="Missing">還沒填的欄位名稱，列表上提醒要補。</param>
    public record Row(ProductRecord Product, ProductImages Images, IReadOnlyList<string> Missing)
    {
        /// <summary>最後更新日期（台灣時間）；UpdatedAt 是 UTC。</summary>
        public string UpdatedOn => Product.UpdatedAt.AddHours(8).ToString("MM-dd");
    }

    public void OnGet()
    {
        // 順序和前台相同，草稿也列出來
        var records = ProductOrder.Sort(db.Products.AsNoTracking().ToList(), p => p.Featured, p => p.Slug);
        Rows = records
            .Select(p => new Row(p, folders.FindImages(p.Slug),
                ProductCompleteness.Check(p).Where(i => !i.IsFilled).Select(i => i.Label).ToList()))
            .ToList();
    }
}

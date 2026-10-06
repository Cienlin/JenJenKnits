using System.ComponentModel.DataAnnotations;
using JenJenKnits.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JenJenKnits.Pages.Admin;

public class CreateModel(SiteDbContext db) : PageModel
{
    [BindProperty]
    public CreateInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid && db.Products.Any(p => p.Slug == Input.Slug))
        {
            ModelState.AddModelError("Input.Slug", "這個網址代稱已經有商品在用了");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        db.Products.Add(new ProductRecord
        {
            Slug = Input.Slug,
            Name = Input.Name.Trim(),
            IsPublished = false,
            UpdatedAt = DateTime.UtcNow,
        });
        db.SaveChanges();

        TempData["Notice"] = $"已建立草稿。照片請放在 wwwroot/products/{Input.Slug}/（cover.jpg、01.jpg、02.jpg…）";
        return RedirectToPage("/Admin/Edit", new { slug = Input.Slug });
    }

    public class CreateInput
    {
        [Required(ErrorMessage = "請輸入名稱")]
        [StringLength(100, ErrorMessage = "名稱最多 {1} 字")]
        public string Name { get; set; } = "";

        // 網址代稱同時是照片資料夾名稱，只允許網址安全的字元
        [Required(ErrorMessage = "請輸入網址代稱")]
        [StringLength(80, ErrorMessage = "網址代稱最多 {1} 字")]
        [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "只能用小寫英文字母、數字和連字號，例如 puff-flower-net")]
        public string Slug { get; set; } = "";
    }
}

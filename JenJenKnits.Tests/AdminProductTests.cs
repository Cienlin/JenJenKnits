using System.Net;
using JenJenKnits.Data;

namespace JenJenKnits.Tests;

public class AdminProductTests
{
    private static ProductRecord Product(string slug, bool published = true) => new()
    {
        Slug = slug,
        Name = "網格托特包",
        Category = "提袋",
        IsPublished = published,
        UpdatedAt = DateTime.UtcNow,
    };

    private static Dictionary<string, string> Form(string name = "網格托特包", string price = "", string featured = "", string buyUrl = "", bool publish = true, string story = "") => new()
    {
        ["Input.Name"] = name,
        ["Input.Category"] = "提袋",
        ["Input.Price"] = price,
        ["Input.Material"] = "",
        ["Input.Dimensions"] = "",
        ["Input.ShortDescription"] = "",
        ["Input.BuyUrl"] = buyUrl,
        ["Input.Featured"] = featured,
        ["Input.ColorSimulator"] = "",
        ["Input.StoryMarkdown"] = story,
        ["Input.IsPublished"] = publish ? "true" : "false",
    };

    [Fact]
    public async Task List_shows_drafts_and_missing_fields()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "net-tote", "cover.jpg");
        site.Seed(Product("net-tote"), Product("secret-draft", published: false));
        var client = await SiteFactory.SignedInClientAsync(site);

        var html = await client.GetStringAsync("/admin");

        Assert.Contains("secret-draft", html);
        Assert.Contains("草稿", html);
        Assert.Contains("缺封面", html);
        Assert.Contains("價格", html);
    }

    [Fact]
    public async Task List_shows_cover_price_featured_and_summary()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "puff-flower-net", "cover.jpg", "01.jpg", "02.jpg", "colors.jpg");
        var complete = Product("puff-flower-net");
        complete.Price = 450;
        complete.Featured = 1;
        complete.Material = "純棉";
        complete.Dimensions = "寬 12 cm";
        complete.ShortDescription = "提著水壺出門";
        complete.StoryMarkdown = "鉤織的網狀提網。";
        complete.BuyUrl = "https://myship.7-11.com.tw/general/detail/GM0000";
        complete.UpdatedAt = new DateTime(2026, 10, 20, 17, 0, 0); // UTC，台灣時間已是 10-21
        site.Seed(complete, Product("secret-draft", published: false));
        var client = await SiteFactory.SignedInClientAsync(site);

        var html = await client.GetStringAsync("/admin");

        Assert.Contains("src=\"/products/puff-flower-net/cover.jpg\"", html);
        Assert.Contains("NT$ 450", html);
        Assert.Contains("精選 No. 1", html);
        Assert.Contains("封面＋2・色卡", html);
        Assert.Contains("10-21", html);
        Assert.Contains("資料齊全", html);
        Assert.Contains("未定價", html); // 草稿沒有價格
        Assert.Contains("1 件已發布・1 件草稿・1 件有欄位待補", html);
    }

    [Fact]
    public async Task Edit_shows_completeness_and_photos()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "net-tote", "cover.jpg", "01.jpg", "colors.jpg");
        var product = Product("net-tote");
        product.Price = 880;
        product.StoryMarkdown = "<!-- 只有註解，前台是空的 -->";
        site.Seed(product);
        var client = await SiteFactory.SignedInClientAsync(site);

        var html = await client.GetStringAsync("/admin/edit/net-tote");

        Assert.Contains("1 / 6", html);
        Assert.Contains("href=\"#field-Material\"", html);
        Assert.Contains("href=\"#field-StoryMarkdown\"", html); // 只有註解也算沒填
        Assert.DoesNotContain("href=\"#field-Price\"", html);
        Assert.Contains("src=\"/products/net-tote/cover.jpg\"", html);
        Assert.Contains("src=\"/products/net-tote/01.jpg\"", html);
        Assert.Contains("src=\"/products/net-tote/colors.jpg\"", html);
        Assert.Contains("封面＋1 張・色卡", html);
        Assert.DoesNotContain("還沒有封面照片", html);
    }

    [Fact]
    public async Task Edit_without_cover_explains_where_photos_go()
    {
        using var site = new SiteFactory();
        site.Seed(Product("net-tote"));
        var client = await SiteFactory.SignedInClientAsync(site);

        var html = await client.GetStringAsync("/admin/edit/net-tote");

        Assert.Contains("還沒有封面照片，發布後前台也不會顯示這件商品", html);
        Assert.Contains("wwwroot/products/net-tote/", html);
        Assert.Contains("0 / 6", html);
    }

    [Fact]
    public async Task Failed_save_counts_completeness_from_the_input()
    {
        using var site = new SiteFactory();
        site.Seed(Product("net-tote"));
        var client = await SiteFactory.SignedInClientAsync(site);

        // 價格與賣貨便連結格式錯，但都有填；其他欄位空白
        var response = await SiteFactory.PostFormAsync(client, "/admin/edit/net-tote", Form(price: "-5", buyUrl: "myship.7-11.com.tw"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("價格要是正整數", html);
        Assert.Contains("2 / 6", html);
        Assert.DoesNotContain("href=\"#field-BuyUrl\"", html);
        Assert.Contains("href=\"#field-Material\"", html);
    }

    [Fact]
    public async Task Create_page_previews_the_product_url()
    {
        using var site = new SiteFactory();
        var client = await SiteFactory.SignedInClientAsync(site);

        var html = await client.GetStringAsync("/admin/create");

        Assert.Contains("example.test/shop/products/<span data-slug-preview>代稱</span>", html);
        Assert.Contains("aria-current=\"step\"", html);
    }

    [Fact]
    public async Task Create_rejects_duplicate_and_invalid_slugs_then_creates_a_draft()
    {
        using var site = new SiteFactory();
        site.Seed(Product("net-tote"));
        var client = await SiteFactory.SignedInClientAsync(site);

        var duplicate = await SiteFactory.PostFormAsync(client, "/admin/create", new() { ["Input.Name"] = "新的", ["Input.Slug"] = "net-tote" });
        Assert.Contains("已經有商品在用了", await duplicate.Content.ReadAsStringAsync());

        var invalid = await SiteFactory.PostFormAsync(client, "/admin/create", new() { ["Input.Name"] = "新的", ["Input.Slug"] = "Net Tote 包" });
        Assert.Contains("只能用小寫英文字母", await invalid.Content.ReadAsStringAsync());

        var created = await SiteFactory.PostFormAsync(client, "/admin/create", new() { ["Input.Name"] = "新的", ["Input.Slug"] = "new-bag" });
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        Assert.Equal("/admin/edit/new-bag", created.Headers.Location!.OriginalString);
        Assert.False(site.Find("new-bag")!.IsPublished);
    }

    [Fact]
    public async Task Edit_validates_and_keeps_input_without_saving()
    {
        using var site = new SiteFactory();
        site.Seed(Product("net-tote"));
        var client = await SiteFactory.SignedInClientAsync(site);

        var response = await SiteFactory.PostFormAsync(client, "/admin/edit/net-tote",
            Form(name: "改過的名字", price: "-5", featured: "abc", buyUrl: "myship.7-11.com.tw"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("價格要是正整數", html);
        Assert.Contains("格式不正確", html);
        Assert.Contains("http:// 或 https://", html);
        Assert.Contains("改過的名字", html); // 已輸入的內容保留
        Assert.Equal("網格托特包", site.Find("net-tote")!.Name); // 整份都沒存
    }

    [Fact]
    public async Task Saving_updates_the_public_page_and_drafts_return_404()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "net-tote", "cover.jpg");
        site.Seed(Product("net-tote"));
        var client = await SiteFactory.SignedInClientAsync(site);

        var saved = await SiteFactory.PostFormAsync(client, "/admin/edit/net-tote",
            Form(price: "880", featured: "2", buyUrl: "https://myship.7-11.com.tw/general/detail/GM0000", story: "一個裝得下日常的網格托特包。"));
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);

        var page = await client.GetStringAsync("/products/net-tote");
        Assert.Contains("NT$ 880", page);
        Assert.Contains("裝得下日常的網格托特包", page);
        Assert.Contains("前往賣貨便", page);

        await SiteFactory.PostFormAsync(client, "/admin/edit/net-tote", Form(publish: false));
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/products/net-tote")).StatusCode);
        Assert.DoesNotContain("網格托特包", await client.GetStringAsync("/products"));
    }

    [Fact]
    public async Task Preview_renders_story_without_saving()
    {
        using var site = new SiteFactory();
        site.Seed(Product("net-tote"));
        var client = await SiteFactory.SignedInClientAsync(site);

        var response = await SiteFactory.PostFormAsync(client, "/admin/edit/net-tote?handler=preview",
            Form(story: "<!-- 備註 -->\n## 三色可選\n**紅**、黃、紫"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("<h2", html);
        Assert.Contains("<strong>紅</strong>", html);
        Assert.DoesNotContain("備註", html);
        Assert.Null(site.Find("net-tote")!.StoryMarkdown);
    }
}

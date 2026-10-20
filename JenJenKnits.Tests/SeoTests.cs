using System.Text.Json;
using System.Text.RegularExpressions;
using JenJenKnits.Data;
using JenJenKnits.Models;

namespace JenJenKnits.Tests;

public partial class SeoTests
{
    private const string Base = SiteFactory.BaseUrl;

    private static ProductRecord Product(string slug, int? price = null, bool published = true, DateTime? updatedAt = null) => new()
    {
        Slug = slug,
        Name = "泡芙小花提網",
        Category = "飲料提網",
        ShortDescription = "提著水壺出門 </script> 也不會壞",
        Price = price,
        IsPublished = published,
        UpdatedAt = updatedAt ?? new DateTime(2026, 10, 8, 3, 0, 0, DateTimeKind.Utc),
    };

    private static string Meta(string html, string property) =>
        Regex.Match(html, $"<meta (?:property|name)=\"{Regex.Escape(property)}\" content=\"([^\"]*)\"").Groups[1].Value;

    private static string Canonical(string html) => CanonicalLink().Match(html).Groups[1].Value;

    private static JsonElement JsonLd(string html) =>
        JsonDocument.Parse(JsonLdScript().Match(html).Groups[1].Value).RootElement;

    // @type 可能是字串，也可能是陣列（首頁同時是 WebPage 與 FAQPage）
    private static bool HasType(JsonElement node, string type) => node.GetProperty("@type") is var types
        && (types.ValueKind == JsonValueKind.Array ? types.EnumerateArray().Any(t => t.GetString() == type) : types.GetString() == type);

    private static JsonElement GraphNode(JsonElement jsonLd, string type) =>
        jsonLd.GetProperty("@graph").EnumerateArray().Single(node => HasType(node, type));

    [Fact]
    public async Task Product_page_has_share_preview_canonical_and_product_data()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "puff", "cover.jpg", "01.jpg");
        site.Seed(Product("puff", price: 450));
        var html = await site.Client().GetStringAsync("/products/puff");

        Assert.Equal($"{Base}/products/puff/cover.jpg", Meta(html, "og:image"));
        Assert.Equal("product", Meta(html, "og:type"));
        Assert.Equal($"{Base}/products/puff/", Canonical(html));
        Assert.Equal(Canonical(html), Meta(html, "og:url"));

        var product = GraphNode(JsonLd(html), "Product");
        Assert.Equal("提著水壺出門 </script> 也不會壞", product.GetProperty("description").GetString());
        Assert.Equal(2, product.GetProperty("image").GetArrayLength());
        Assert.Equal(450, product.GetProperty("offers").GetProperty("price").GetInt32());
        Assert.Equal("TWD", product.GetProperty("offers").GetProperty("priceCurrency").GetString());
    }

    [Fact]
    public async Task Product_without_price_has_no_offer()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "tote", "cover.jpg");
        site.Seed(Product("tote"));
        var html = await site.Client().GetStringAsync("/products/tote");

        Assert.False(GraphNode(JsonLd(html), "Product").TryGetProperty("offers", out _));
    }

    [Fact]
    public async Task Product_page_is_an_item_page_with_author_and_date()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "puff", "cover.jpg");
        site.Seed(Product("puff"));
        var html = await site.Client().GetStringAsync("/products/puff");

        var page = GraphNode(JsonLd(html), "ItemPage");
        Assert.Equal($"{Base}/products/puff/", page.GetProperty("url").GetString());
        Assert.Equal(GraphNode(JsonLd(html), "Product").GetProperty("@id").GetString(), page.GetProperty("mainEntity").GetProperty("@id").GetString());
        Assert.Equal($"{Base}/#organization", page.GetProperty("author").GetProperty("@id").GetString());
        Assert.Equal("織物製造所", Meta(html, "author"));

        // 結構化資料的日期和頁尾顯示的一樣（台灣時間）
        Assert.Equal("2026-10-08", page.GetProperty("dateModified").GetString());
        Assert.Matches("<time[^>]*datetime=\"2026-10-08\"[^>]*>2026-10-08</time>", html);
    }

    [Fact]
    public async Task Category_filter_points_canonical_at_the_list()
    {
        using var site = new SiteFactory();
        var list = await site.Client().GetStringAsync("/products?category=%E6%8F%90%E8%A2%8B");

        Assert.Equal($"{Base}/products/", Canonical(list));
        Assert.Equal($"{Base}/images/hero/puff-flower-lake.jpg", Meta(list, "og:image"));
    }

    [Fact]
    public async Task Home_has_brand_and_site_name_data()
    {
        using var site = new SiteFactory();
        site.UseSetting("Site:InstagramUrl", "https://www.instagram.com/example/");
        var home = await site.Client().GetStringAsync("/");
        Assert.Equal($"{Base}/", Canonical(home));

        var organization = GraphNode(JsonLd(home), "OnlineStore");
        Assert.Equal("織物製造所", organization.GetProperty("name").GetString());
        Assert.Equal("JH Craft Studio", organization.GetProperty("alternateName").GetString());
        Assert.Equal("https://www.instagram.com/example/", organization.GetProperty("sameAs")[0].GetString());

        var website = GraphNode(JsonLd(home), "WebSite");
        Assert.Equal("織物製造所", website.GetProperty("name").GetString());
        Assert.Equal("JH Craft Studio", website.GetProperty("alternateName").GetString());
        Assert.Equal(Canonical(home), website.GetProperty("url").GetString());
        Assert.Equal(organization.GetProperty("@id").GetString(), website.GetProperty("publisher").GetProperty("@id").GetString());
    }

    [Fact]
    public async Task Product_list_title_and_description_name_the_categories()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "puff", "cover.jpg");
        site.Seed(Product("puff"));
        var list = await site.Client().GetStringAsync("/products");

        Assert.Equal("作品：手鉤織飲料提網｜織物製造所", PageTitle().Match(list).Groups[1].Value);
        Assert.Contains("手鉤織的飲料提網等日常小物", Meta(list, "description"));
        Assert.Equal("織物製造所", Meta(list, "author"));
        Assert.DoesNotContain(">更新</dt>", list); // 版型的元素帶 CSS 隔離屬性（<dt b-…>），只比對結尾
    }

    [Fact]
    public async Task Home_cover_link_has_alt_text()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "puff", "cover.jpg");
        site.Seed(Product("puff"));
        var home = await site.Client().GetStringAsync("/");

        Assert.Matches("<img src=\"[^\"]*/products/puff/cover\\.jpg\" alt=\"泡芙小花提網\"", home);
    }

    [Fact]
    public async Task Home_has_faq_data_in_page_order()
    {
        using var site = new SiteFactory();
        site.UseFaq(
            new FaqItem("可以自己挑顏色嗎？", "可以，從色卡挑。"),
            new FaqItem("怎麼清洗？", "冷水手洗 </script> 不要擰乾。"));
        var home = await site.Client().GetStringAsync("/");

        var faq = GraphNode(JsonLd(home), "FAQPage");
        Assert.True(HasType(faq, "WebPage"), "常見問題就是首頁本身的頁面資料");
        Assert.Equal($"{Base}/#webpage", faq.GetProperty("@id").GetString());
        var questions = faq.GetProperty("mainEntity").EnumerateArray().ToList();
        Assert.Equal(2, questions.Count);
        Assert.All(questions, question => Assert.Equal("Question", question.GetProperty("@type").GetString()));
        Assert.Equal("可以自己挑顏色嗎？", questions[0].GetProperty("name").GetString());
        Assert.Equal("怎麼清洗？", questions[1].GetProperty("name").GetString());

        var answer = questions[1].GetProperty("acceptedAnswer");
        Assert.Equal("Answer", answer.GetProperty("@type").GetString());
        Assert.Equal("冷水手洗 </script> 不要擰乾。", answer.GetProperty("text").GetString());
    }

    [Fact]
    public async Task Home_without_faq_has_no_faq_data()
    {
        using var site = new SiteFactory();
        site.UseFaq();
        var home = await site.Client().GetStringAsync("/");

        var graph = JsonLd(home).GetProperty("@graph").EnumerateArray().ToList();
        Assert.All(graph, node => Assert.Equal(JsonValueKind.Object, node.ValueKind));
        Assert.DoesNotContain(graph, node => HasType(node, "FAQPage"));
        Assert.False(GraphNode(JsonLd(home), "WebPage").TryGetProperty("mainEntity", out _));
    }

    [Fact]
    public async Task Home_page_has_author_and_the_latest_date()
    {
        using var site = new SiteFactory();
        site.UseFaq(new SiteFaq([new FaqItem("Q", "A")]) { UpdatedOn = new DateOnly(2026, 10, 12) }); // Seed 會啟動網站，要先設定
        ProductFiles.Folder(site.ProductsPath, "puff", "cover.jpg");
        site.Seed(Product("puff"));
        var home = await site.Client().GetStringAsync("/");

        var page = GraphNode(JsonLd(home), "WebPage");
        Assert.Equal($"{Base}/", page.GetProperty("url").GetString());
        Assert.Equal($"{Base}/#website", page.GetProperty("isPartOf").GetProperty("@id").GetString());
        Assert.Equal(GraphNode(JsonLd(home), "OnlineStore").GetProperty("@id").GetString(), page.GetProperty("author").GetProperty("@id").GetString());
        Assert.Equal("織物製造所", Meta(home, "author"));

        // 商品 10-08、常見問題 10-12：取最新的，頁尾顯示同一天
        Assert.Equal("2026-10-12", page.GetProperty("dateModified").GetString());
        Assert.Matches("<time[^>]*datetime=\"2026-10-12\"[^>]*>2026-10-12</time>", home);
    }

    [Fact]
    public async Task Home_title_names_the_brand_and_description_the_works()
    {
        using var site = new SiteFactory();
        var home = await site.Client().GetStringAsync("/");

        var title = PageTitle().Match(home).Groups[1].Value;
        Assert.StartsWith("織物製造所 JH Craft Studio｜", title);
        Assert.Contains("顏色可客製", title);
        Assert.Equal(title, Meta(home, "og:title"));

        var description = Meta(home, "description");
        Assert.Contains("泡芙小花提網", description);
        Assert.Contains("花花杯墊", description);
        Assert.Contains($"{YarnPalette.Colors.Count} 色", description);
        Assert.Equal(description, Meta(home, "og:description"));
    }

    [Fact]
    public async Task Home_without_instagram_has_no_social_profile()
    {
        using var site = new SiteFactory();
        site.UseSetting("Site:InstagramUrl", "");
        var home = await site.Client().GetStringAsync("/");

        Assert.False(GraphNode(JsonLd(home), "OnlineStore").TryGetProperty("sameAs", out _));
    }

    [Fact]
    public void Serialize_drops_null_fields_inside_arrays()
    {
        var json = PageSeo.Serialize(new Dictionary<string, object?>
        {
            ["@graph"] = new object[]
            {
                new Dictionary<string, object?> { ["@type"] = "Organization", ["sameAs"] = null },
            },
        });

        Assert.Equal("{\"@graph\":[{\"@type\":\"Organization\"}]}", json);
    }

    [Fact]
    public async Task Sitemap_lists_only_visible_products_and_robots_blocks_admin()
    {
        using var site = new SiteFactory();
        ProductFiles.Folder(site.ProductsPath, "visible", "cover.jpg");
        ProductFiles.Folder(site.ProductsPath, "draft", "cover.jpg");
        site.Seed(Product("visible"), Product("draft", published: false), Product("no-cover"));
        var client = site.Client();

        var sitemap = await client.GetStringAsync("/sitemap.xml");
        Assert.Contains($"<loc>{Base}/products/visible/</loc>", sitemap);
        Assert.Contains("<lastmod>2026-10-08</lastmod>", sitemap);
        Assert.Contains($"<loc>{Base}/</loc>", sitemap);
        Assert.DoesNotContain("draft", sitemap);
        Assert.DoesNotContain("no-cover", sitemap);

        // UTC 晚上 8 點以後，在台灣已經是隔天
        site.Seed(Product("late", updatedAt: new DateTime(2026, 10, 8, 16, 30, 0, DateTimeKind.Utc)));
        ProductFiles.Folder(site.ProductsPath, "late", "cover.jpg");
        Assert.Contains("<lastmod>2026-10-09</lastmod>", await client.GetStringAsync("/sitemap.xml"));

        var robots = await client.GetStringAsync("/robots.txt");
        Assert.Contains("Disallow: /admin", robots);
        Assert.Contains($"Sitemap: {Base}/sitemap.xml", robots);
    }

    [GeneratedRegex("<title>([^<]*)</title>")]
    private static partial Regex PageTitle();

    [GeneratedRegex("<link rel=\"canonical\" href=\"([^\"]*)\"")]
    private static partial Regex CanonicalLink();

    [GeneratedRegex("<script type=\"application/ld\\+json\">(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex JsonLdScript();
}

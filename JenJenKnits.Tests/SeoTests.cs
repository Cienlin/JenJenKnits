using System.Text.Json;
using System.Text.RegularExpressions;
using JenJenKnits.Data;
using JenJenKnits.Models;

namespace JenJenKnits.Tests;

public partial class SeoTests
{
    private const string Base = SiteFactory.BaseUrl;

    private static ProductRecord Product(string slug, int? price = null, bool published = true) => new()
    {
        Slug = slug,
        Name = "泡芙小花提網",
        Category = "飲料提網",
        ShortDescription = "提著水壺出門 </script> 也不會壞",
        Price = price,
        IsPublished = published,
        UpdatedAt = new DateTime(2026, 10, 8, 3, 0, 0, DateTimeKind.Utc),
    };

    private static string Meta(string html, string property) =>
        Regex.Match(html, $"<meta (?:property|name)=\"{Regex.Escape(property)}\" content=\"([^\"]*)\"").Groups[1].Value;

    private static string Canonical(string html) => CanonicalLink().Match(html).Groups[1].Value;

    private static JsonElement JsonLd(string html) =>
        JsonDocument.Parse(JsonLdScript().Match(html).Groups[1].Value).RootElement;

    private static JsonElement GraphNode(JsonElement jsonLd, string type) =>
        jsonLd.GetProperty("@graph").EnumerateArray().Single(node => node.GetProperty("@type").GetString() == type);

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

        var product = JsonLd(html);
        Assert.Equal("Product", product.GetProperty("@type").GetString());
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

        Assert.False(JsonLd(html).TryGetProperty("offers", out _));
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

        var organization = GraphNode(JsonLd(home), "Organization");
        Assert.Equal("織物製造所", organization.GetProperty("name").GetString());
        Assert.Equal("https://www.instagram.com/example/", organization.GetProperty("sameAs")[0].GetString());

        var website = GraphNode(JsonLd(home), "WebSite");
        Assert.Equal("織物製造所", website.GetProperty("name").GetString());
        Assert.Equal("JH Craft Studio", website.GetProperty("alternateName").GetString());
        Assert.Equal(Canonical(home), website.GetProperty("url").GetString());
        Assert.Equal(organization.GetProperty("@id").GetString(), website.GetProperty("publisher").GetProperty("@id").GetString());
    }

    [Fact]
    public async Task Home_without_instagram_has_no_social_profile()
    {
        using var site = new SiteFactory();
        site.UseSetting("Site:InstagramUrl", "");
        var home = await site.Client().GetStringAsync("/");

        Assert.False(GraphNode(JsonLd(home), "Organization").TryGetProperty("sameAs", out _));
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

        var robots = await client.GetStringAsync("/robots.txt");
        Assert.Contains("Disallow: /admin", robots);
        Assert.Contains($"Sitemap: {Base}/sitemap.xml", robots);
    }

    [GeneratedRegex("<link rel=\"canonical\" href=\"([^\"]*)\"")]
    private static partial Regex CanonicalLink();

    [GeneratedRegex("<script type=\"application/ld\\+json\">(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex JsonLdScript();
}

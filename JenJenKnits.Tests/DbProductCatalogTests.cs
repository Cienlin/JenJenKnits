using JenJenKnits.Data;
using JenJenKnits.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace JenJenKnits.Tests;

public class DbProductCatalogTests
{
    private static ProductRecord Record(string slug, bool published = true, int? featured = null, string? story = null) => new()
    {
        Slug = slug,
        Name = slug,
        Featured = featured,
        StoryMarkdown = story,
        IsPublished = published,
        UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public void Shows_only_published_products_with_a_cover_in_front_order()
    {
        using var folder = new TempFolder();
        using var db = TestDatabase.Create(folder);
        var products = folder.Combine("products");
        ProductFiles.Folder(products, "b-plain", "cover.jpg");
        ProductFiles.Folder(products, "a-featured-2", "cover.jpg");
        ProductFiles.Folder(products, "z-featured-1", "cover.jpg", "02.jpg", "01.jpg", "10.jpg", "colors.jpg");
        ProductFiles.Folder(products, "draft", "cover.jpg");
        ProductFiles.Folder(products, "no-cover", "01.jpg");
        db.Products.AddRange(
            Record("b-plain"),
            Record("a-featured-2", featured: 2),
            Record("z-featured-1", featured: 1, story: "<!-- 只有備註 -->"),
            Record("draft", published: false),
            Record("no-cover"));
        db.SaveChanges();

        var catalog = new DbProductCatalog(db, new ProductFolders(products), NullLogger<DbProductCatalog>.Instance);

        Assert.Equal(["z-featured-1", "a-featured-2", "b-plain"], catalog.GetAll().Select(p => p.Slug));
        Assert.Null(catalog.GetBySlug("draft"));
        Assert.Null(catalog.GetBySlug("no-cover"));

        var first = catalog.GetBySlug("z-featured-1")!;
        Assert.Equal("/products/z-featured-1/cover.jpg", first.CoverImagePath);
        Assert.Equal(["/products/z-featured-1/01.jpg", "/products/z-featured-1/02.jpg", "/products/z-featured-1/10.jpg"], first.GalleryImagePaths);
        Assert.Equal("/products/z-featured-1/colors.jpg", first.ColorChartImagePath);
        Assert.Null(first.StoryHtml);
    }

    [Fact]
    public void Renders_story_markdown_without_html_comments()
    {
        var html = StoryRenderer.ToHtml("<!-- 作者備註 -->\n\n第一段\n第二行\n\n## 自己挑配色");

        Assert.NotNull(html);
        Assert.DoesNotContain("作者備註", html);
        Assert.Contains("<br />", html);
        Assert.Contains("<h2", html);
    }
}

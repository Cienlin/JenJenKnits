using JenJenKnits.Data;
using JenJenKnits.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace JenJenKnits.Tests;

public class ProductImporterTests
{
    private static ProductImporter Importer(SiteDbContext db, TempFolder folder) =>
        new(db, new ProductFolders(folder.Combine("products")), NullLogger<ProductImporter>.Instance);

    private static void WriteMeta(string dir, string json) => File.WriteAllText(Path.Combine(dir, "meta.json"), json);

    [Fact]
    public void Imports_meta_and_story_then_deletes_them_but_keeps_photos()
    {
        using var folder = new TempFolder();
        using var db = TestDatabase.Create(folder);
        var dir = ProductFiles.Folder(folder.Combine("products"), "puff-flower-net", "cover.jpg", "01.jpg");
        WriteMeta(dir, """{ "name": "泡芙小花提網", "category": "飲料提網", "price": 450, "material": "棉線", "dimensions": "", "featured": 1, "colorSimulator": "puff-flower", "buyUrl": "" }""");
        File.WriteAllText(Path.Combine(dir, "story.md"), "<!-- 備註 -->\n\n鉤織的網狀提網。\n");

        var count = Importer(db, folder).ImportIfEmpty();

        Assert.Equal(1, count);
        var record = Assert.Single(db.Products);
        Assert.Equal("puff-flower-net", record.Slug);
        Assert.Equal("泡芙小花提網", record.Name);
        Assert.Equal("飲料提網", record.Category);
        Assert.Equal(450, record.Price);
        Assert.Equal("棉線", record.Material);
        Assert.Null(record.Dimensions);
        Assert.Null(record.BuyUrl);
        Assert.Equal(1, record.Featured);
        Assert.Equal("puff-flower", record.ColorSimulator);
        Assert.Contains("鉤織的網狀提網", record.StoryMarkdown);
        Assert.True(record.IsPublished);
        Assert.False(File.Exists(Path.Combine(dir, "meta.json")));
        Assert.False(File.Exists(Path.Combine(dir, "story.md")));
        Assert.True(File.Exists(Path.Combine(dir, "cover.jpg")));
    }

    [Fact]
    public void Skips_broken_json_missing_name_and_draft_folders()
    {
        using var folder = new TempFolder();
        using var db = TestDatabase.Create(folder);
        var products = folder.Combine("products");
        var broken = ProductFiles.Folder(products, "broken", "cover.jpg");
        WriteMeta(broken, "{ not json");
        var noName = ProductFiles.Folder(products, "no-name", "cover.jpg");
        WriteMeta(noName, """{ "name": "  " }""");
        var draft = ProductFiles.Folder(products, "_wip", "cover.jpg");
        WriteMeta(draft, """{ "name": "草稿" }""");
        var ok = ProductFiles.Folder(products, "ok", "cover.jpg");
        WriteMeta(ok, """{ "name": "好的", "buyUrl": "not-a-url", "price": 0 }""");

        Importer(db, folder).ImportIfEmpty();

        var record = Assert.Single(db.Products);
        Assert.Equal("ok", record.Slug);
        Assert.Null(record.BuyUrl);
        Assert.Null(record.Price);
        // 略過的資料夾檔案保持原樣
        Assert.True(File.Exists(Path.Combine(broken, "meta.json")));
        Assert.True(File.Exists(Path.Combine(noName, "meta.json")));
        Assert.True(File.Exists(Path.Combine(draft, "meta.json")));
    }

    [Fact]
    public void Does_nothing_when_database_already_has_products()
    {
        using var folder = new TempFolder();
        using var db = TestDatabase.Create(folder);
        db.Products.Add(new ProductRecord { Slug = "existing", Name = "既有", UpdatedAt = DateTime.UtcNow });
        db.SaveChanges();
        var dir = ProductFiles.Folder(folder.Combine("products"), "new-one", "cover.jpg");
        WriteMeta(dir, """{ "name": "新的" }""");

        var count = Importer(db, folder).ImportIfEmpty();

        Assert.Equal(0, count);
        Assert.Single(db.Products);
        Assert.True(File.Exists(Path.Combine(dir, "meta.json")));
    }
}

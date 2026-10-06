using JenJenKnits.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JenJenKnits.Tests;

/// <summary>測試用的暫存資料夾：每個測試一份，用完刪除。</summary>
public sealed class TempFolder : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "jjk-tests", Guid.NewGuid().ToString("N"));

    public TempFolder() => Directory.CreateDirectory(Path);

    public string Combine(params string[] parts) => System.IO.Path.Combine([Path, .. parts]);

    public void Dispose()
    {
        // SQLite 連線池會占住檔案，刪資料夾前先釋放
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(Path, recursive: true);
        }
        catch (IOException)
        {
            // 暫存檔刪不掉不影響測試結果
        }
    }
}

/// <summary>商品照片資料夾的測試資料：只建出檔名，內容不重要。</summary>
public static class ProductFiles
{
    public static string Folder(string productsRoot, string slug, params string[] files)
    {
        var dir = Path.Combine(productsRoot, slug);
        Directory.CreateDirectory(dir);
        foreach (var file in files)
        {
            File.WriteAllText(Path.Combine(dir, file), file.EndsWith(".json") || file.EndsWith(".md") ? "" : "img");
        }

        return dir;
    }
}

public static class TestDatabase
{
    public static string ConnectionString(TempFolder folder) => $"Data Source={folder.Combine("site.db")}";

    /// <summary>建立已套用遷移的暫存資料庫。</summary>
    public static SiteDbContext Create(TempFolder folder)
    {
        var options = new DbContextOptionsBuilder<SiteDbContext>().UseSqlite(ConnectionString(folder)).Options;
        var db = new SiteDbContext(options);
        db.Database.Migrate();
        return db;
    }
}

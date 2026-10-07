using JenJenKnits.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JenJenKnits.Tests;

public class DatabaseTests
{
    [Fact]
    public void Migrations_create_products_table_with_unique_slug()
    {
        using var folder = new TempFolder();
        using var db = TestDatabase.Create(folder);

        db.Products.Add(new ProductRecord { Slug = "net-tote", Name = "網格托特包", UpdatedAt = DateTime.UtcNow });
        db.SaveChanges();
        db.Products.Add(new ProductRecord { Slug = "net-tote", Name = "重複", UpdatedAt = DateTime.UtcNow });

        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void Site_keeps_the_database_in_a_single_file_for_git()
    {
        using var site = new SiteFactory();
        site.Client(); // 啟動網站

        using var scope = site.Services.CreateScope();
        var connection = scope.ServiceProvider.GetRequiredService<SiteDbContext>().Database.GetDbConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode";

        // WAL 會把資料放在被 .gitignore 忽略的 -wal 檔，資料就進不了 git
        Assert.Equal("delete", command.ExecuteScalar() as string);
    }

    [Fact]
    public void Starting_the_site_again_does_not_touch_the_database_file()
    {
        using var first = new SiteFactory();
        first.Client(); // 第一次啟動：建立資料庫
        var dbPath = first.Folder.Combine("site.db");
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        var before = File.ReadAllBytes(dbPath);

        // 用同一個資料庫再啟動一次（像是隔天再打開網站）
        using var second = new SiteFactory();
        second.UseDatabase(dbPath);
        second.Client().GetAsync("/products").Wait();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        // 只是啟動和瀏覽，資料庫檔一個位元組都不該變，git 才不會顯示它被修改
        Assert.Equal(before, File.ReadAllBytes(dbPath));
    }
}

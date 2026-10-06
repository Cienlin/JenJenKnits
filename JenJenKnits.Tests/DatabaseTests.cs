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
}

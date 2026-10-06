using System.Net;
using System.Text.RegularExpressions;
using JenJenKnits.Admin;
using JenJenKnits.Data;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace JenJenKnits.Tests;

/// <summary>
/// 用暫存資料庫與暫存商品資料夾啟動整個網站。環境設為 Testing，
/// 所以不會讀到本機 user-secrets 裡真正的管理員帳號。
/// </summary>
public sealed partial class SiteFactory : WebApplicationFactory<Program>
{
    public const string UserName = "admin";
    public const string Password = "correct horse battery";

    public TempFolder Folder { get; } = new();

    public string ProductsPath => Folder.Combine("products");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(ProductsPath);
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Site", TestDatabase.ConnectionString(Folder));
        builder.UseSetting("Catalog:ProductsPath", ProductsPath);
        builder.UseSetting("Admin:UserName", UserName);
        builder.UseSetting("Admin:PasswordHash", AdminPassword.Hash(Password));
    }

    /// <summary>不自動跟隨轉址，才能檢查導向登入頁；cookie 會保留在這個 client。</summary>
    public HttpClient Client()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        // 保險：確認網站真的用了暫存資料，否則測試會動到真正的商品資料
        if (Services.GetRequiredService<ProductFolders>().Root != ProductsPath)
        {
            throw new InvalidOperationException("測試網站沒有使用暫存商品資料夾");
        }

        return client;
    }

    public void Seed(params ProductRecord[] products)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SiteDbContext>();
        db.Products.AddRange(products);
        db.SaveChanges();
    }

    public ProductRecord? Find(string slug)
    {
        using var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SiteDbContext>().Products.FirstOrDefault(p => p.Slug == slug);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Folder.Dispose();
        }
    }

    /// <summary>先 GET 表單頁（或 tokenFrom 指定的頁面）拿 antiforgery token，再 POST。</summary>
    public static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client, string url, Dictionary<string, string> fields, string? tokenFrom = null)
    {
        var page = await client.GetStringAsync(tokenFrom ?? url.Split('?')[0]);
        var token = AntiforgeryToken().Match(page).Groups[1].Value;
        return await client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>(fields)
        {
            ["__RequestVerificationToken"] = token,
        }));
    }

    public static async Task<HttpClient> SignedInClientAsync(SiteFactory site)
    {
        var client = site.Client();
        var response = await PostFormAsync(client, "/admin/login", new()
        {
            ["Input.UserName"] = UserName,
            ["Input.Password"] = Password,
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return client;
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryToken();
}

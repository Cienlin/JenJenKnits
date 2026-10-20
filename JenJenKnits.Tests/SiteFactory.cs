using System.Net;
using System.Text.RegularExpressions;
using JenJenKnits.Admin;
using JenJenKnits.Data;
using JenJenKnits.Models;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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

    /// <summary>測試用的對外網址；刻意帶子路徑，換正式網域也不用改測試。</summary>
    public const string BaseUrl = "https://example.test/shop";

    public TempFolder Folder { get; } = new();

    public string ProductsPath => Folder.Combine("products");

    private string? _databasePath;

    private readonly Dictionary<string, string> _settings = [];

    private SiteFaq? _faq;

    /// <summary>改用指定的資料庫檔（預設是暫存資料夾裡的新資料庫），要在第一次 Client() 之前呼叫。</summary>
    public void UseDatabase(string path) => _databasePath = path;

    /// <summary>覆寫網站設定（例如 Site:InstagramUrl 給空字串 = 沒設定），要在第一次 Client() 之前呼叫。</summary>
    public void UseSetting(string key, string value) => _settings[key] = value;

    /// <summary>
    /// 換成測試自己的常見問題（不給題目 = 沒有常見問題），要在第一次 Client() 之前呼叫。
    /// 沒呼叫時用正式網站的清單，所以要測常見問題的測試都要自己指定。
    /// </summary>
    public void UseFaq(params FaqItem[] items) => _faq = new SiteFaq(items);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(ProductsPath);
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Site",
            _databasePath is null ? TestDatabase.ConnectionString(Folder) : $"Data Source={_databasePath}");
        builder.UseSetting("Catalog:ProductsPath", ProductsPath);
        builder.UseSetting("Site:BaseUrl", BaseUrl);
        builder.UseSetting("Admin:UserName", UserName);
        builder.UseSetting("Admin:PasswordHash", AdminPassword.Hash(Password));
        foreach (var (key, value) in _settings)
        {
            builder.UseSetting(key, value);
        }

        if (_faq is SiteFaq faq)
        {
            // 在網站自己的註冊之後執行，後註冊的會被取用
            builder.ConfigureTestServices(services => services.AddSingleton(faq));
        }
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

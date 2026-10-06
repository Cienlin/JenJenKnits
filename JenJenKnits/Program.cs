using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using JenJenKnits.Admin;
using JenJenKnits.Data;
using JenJenKnits.Models;
using JenJenKnits.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// `dotnet run -- hash-password`：產生管理員密碼的雜湊值，不啟動網站
if (args is ["hash-password", ..])
{
    AdminPassword.PromptAndPrintHash();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// 資料庫：預設 App_Data/jenjenknits.db（進 git，兩台電腦靠 git 同步）；測試用 ConnectionStrings:Site 指到暫存檔
// SQLite 的相對路徑是相對於「目前工作目錄」，所以用 ContentRoot 組成絕對路徑
var appData = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appData);
var connectionString = builder.Configuration.GetConnectionString("Site")
    ?? $"Data Source={Path.Combine(appData, "jenjenknits.db")}";
builder.Services.AddDbContext<SiteDbContext>(options => options.UseSqlite(connectionString));

builder.Services
    .AddRazorPages(options =>
    {
        // 後台整個資料夾都要登入，登入頁除外
        options.Conventions.AuthorizeFolder("/Admin");
        options.Conventions.AllowAnonymousToPage("/Admin/Login");
    })
    .AddMvcOptions(options =>
    {
        // 表單欄位格式錯誤時的訊息改成中文
        var messages = options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((value, _) => $"「{value}」格式不正確");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"「{value}」格式不正確");
        messages.SetValueMustBeANumberAccessor(_ => "請輸入數字");
    });
// 中文直接輸出，不轉成 &#x...; 編碼
builder.Services.AddWebEncoders(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));

// 後台登入：Cookie 驗證 + 單一管理員帳號（不使用 Identity，見 add-admin-backend design D6）
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "jjk_admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// 登入次數限制：同一個 IP 每分鐘最多送出 5 次登入
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        HttpMethods.IsPost(context.Request.Method)
        && context.Request.Path.StartsWithSegments("/admin/login", StringComparison.OrdinalIgnoreCase)
            ? RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) })
            : RateLimitPartition.GetNoLimiter("not-login"));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("登入嘗試太多次，請一分鐘後再試。", token);
    };
});

// 商品文字資料在資料庫；照片在 wwwroot/products/{slug}/（測試用 Catalog:ProductsPath 指到暫存資料夾）
builder.Services.AddSingleton(new ProductFolders(
    builder.Configuration["Catalog:ProductsPath"] ?? Path.Combine(builder.Environment.WebRootPath, "products")));
builder.Services.AddScoped<ProductImporter>();
builder.Services.AddScoped<IProductCatalog, DbProductCatalog>();

var app = builder.Build();

// 啟動時套用資料庫遷移；資料庫還沒有商品時，從舊的 meta.json / story.md 匯入一次
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SiteDbContext>();
    db.Database.Migrate();
    // EF Core 建立 SQLite 時預設開 WAL，寫入的資料會先留在旁邊的 -wal 檔；
    // 切回單一檔案模式（切換時會把 -wal 寫回主檔），資料庫檔才能直接進 git（design D2）
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
    scope.ServiceProvider.GetRequiredService<ProductImporter>().ImportIfEmpty();
}

if (!app.Services.GetRequiredService<IOptions<AdminOptions>>().Value.IsConfigured)
{
    app.Logger.LogWarning("後台尚未設定管理員帳號（Admin:UserName、Admin:PasswordHash），登入一律失敗");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

// 後台不讓搜尋引擎收錄
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    }

    await next();
});

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// 讓測試專案的 WebApplicationFactory<Program> 找得到進入點
public partial class Program;

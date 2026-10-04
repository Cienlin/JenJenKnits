using System.Text.Encodings.Web;
using System.Text.Unicode;
using JenJenKnits.Models;
using JenJenKnits.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
// 中文直接輸出，不轉成 &#x...; 編碼
builder.Services.AddWebEncoders(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));

// Development 每個 request 重掃商品資料夾，改完重新整理就看得到；Production 啟動時掃一次
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IProductCatalog, FileSystemProductCatalog>();
}
else
{
    builder.Services.AddSingleton<IProductCatalog, FileSystemProductCatalog>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.Services.GetRequiredService<IProductCatalog>();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace JenJenKnits.Models;

/// <summary>
/// 頁面給搜尋引擎與社群分享用的資訊。各頁放進 ViewData["Seo"]，由 _Layout 輸出 meta、canonical 與 JSON-LD。
/// 圖片與網址都用站內路徑（以 / 開頭），_Layout 依 Site:BaseUrl 轉成絕對網址。
/// </summary>
public class PageSeo
{
    /// <summary>沒有指定分享照片時用首頁主照片。</summary>
    public const string DefaultImage = "/images/hero/puff-flower-lake.jpg";

    public string? Image { get; init; }

    /// <summary>Open Graph 類型：website 或 product。</summary>
    public string Type { get; init; } = "website";

    /// <summary>canonical 用的站內路徑；null = 目前請求的路徑（不含查詢字串）。</summary>
    public string? CanonicalPath { get; init; }

    /// <summary>schema.org 結構化資料，由 _Layout 序列化成 JSON-LD。</summary>
    public object? JsonLd { get; init; }

    // 中文直接輸出；<、>、& 仍會被跳脫，商品文字裡就算有 </script> 也不會跳出 script 區塊
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(object value) => JsonSerializer.Serialize(WithoutNulls(value), JsonOptions);

    // 沒填的欄位（例如沒有價格時的 offers）整個拿掉，不輸出 "offers": null；
    // JsonIgnoreCondition 只管物件屬性，管不到字典裡的值
    private static object? WithoutNulls(object? value) => value is IDictionary<string, object?> dictionary
        ? dictionary.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => WithoutNulls(pair.Value))
        : value;
}

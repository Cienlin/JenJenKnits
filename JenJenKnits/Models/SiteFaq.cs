namespace JenJenKnits.Models;

/// <summary>常見問題的一題。答案是純文字，不放連結或格式（add-faq design D4）。</summary>
public record FaqItem(string Question, string Answer);

/// <summary>
/// 首頁的常見問題：「03 常見問題」章節與 FAQPage 結構化資料都從這份清單產生，兩邊文字一定一致。
/// 啟動時註冊成服務；測試用 SiteFactory.UseFaq 換成自己的題目。
/// </summary>
public sealed class SiteFaq(IReadOnlyList<FaqItem> items)
{
    public IReadOnlyList<FaqItem> Items { get; } = items;

    /// <summary>
    /// 題目或答案最後修改的日期，首頁的更新日期會參考它（add-page-dates-author design D3）。
    /// 改 Default 的題目或答案時，一起改這個日期。沒有值時首頁的日期只看商品。
    /// </summary>
    public DateOnly? UpdatedOn { get; init; }

    /// <summary>
    /// 正式網站的題目，依買家下單前後會想知道的順序。只放確認過的答案，
    /// 確認的經過記在 openspec/changes/archive/2026-10-20-add-faq/design.md D6。清單是空的時候首頁不顯示這個章節。
    /// </summary>
    public static SiteFaq Default { get; } = new([
        new("可以自己挑顏色嗎？",
            "可以。每件作品都能從 22 色線材裡挑：泡芙小花提網挑主色和配色兩個顏色；花花杯墊可以選純色（1 色）或雙色混織（2 色）。商品頁有色卡，提網還能用配色模擬先看配起來的樣子。已經有想要的色號，也可以先私訊 Instagram 詢問。"),
        new("泡芙小花提網可以裝多大的水壺或杯子？",
            "從一般寶特瓶、超商咖啡杯，到 Stanley 1180 ml（40 oz）這種大容量保溫杯都裝得下。"),
        new("下單後多久可以拿到？",
            "每一件都是收到訂單後才開始手工鉤織，大約一週內完成，完成當天或隔天寄出，再經 7-11 店到店約 2～3 天送到門市。"),
        new("運費多少？怎麼取貨？",
            "在 7-11 賣貨便下單付款，到 7-11 門市取貨，運費 60 元。賣場開賣前，可以私訊 Instagram 訂購。"),
        new("怎麼清洗？可以丟洗衣機嗎？",
            "建議用冷水手洗，輕輕按壓、不要用力擰乾，攤平放在陰涼處晾乾。不建議丟洗衣機或烘衣機，容易變形；也不要用漂白水。"),
        new("可以退換貨嗎？",
            "每件作品都是依你選的顏色接單後才鉤織，屬於客製商品，不適用七天鑑賞期，下單前請先確認好顏色。收到時如果有瑕疵或寄錯，請在收到後 7 天內私訊 Instagram，我們會協助處理。"),
    ])
    {
        UpdatedOn = new(2026, 10, 20),
    };
}

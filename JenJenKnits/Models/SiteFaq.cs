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
    /// 正式網站的題目，依買家下單前後會想知道的順序。只放跟貞貞確認過的答案，
    /// 草稿在 openspec/changes/add-faq/design.md D6。清單是空的時候首頁不顯示這個章節。
    /// </summary>
    public static SiteFaq Default { get; } = new([]);
}

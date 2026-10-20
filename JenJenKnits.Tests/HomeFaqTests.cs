using JenJenKnits.Models;

namespace JenJenKnits.Tests;

/// <summary>首頁「03 常見問題」章節；結構化資料的部分在 SeoTests。</summary>
public class HomeFaqTests
{
    [Fact]
    public async Task Home_shows_every_question_and_answer_after_how_to_buy()
    {
        using var site = new SiteFactory();
        site.UseFaq(
            new FaqItem("可以自己挑顏色嗎？", "可以，從色卡挑。"),
            new FaqItem("怎麼清洗？", "冷水手洗 <b>不要</b> 擰乾。"));
        var home = await site.Client().GetStringAsync("/");

        Assert.Contains("<a href=\"#faq\">", home);
        Assert.True(home.IndexOf("id=\"buy\"") < home.IndexOf("id=\"faq\""), "常見問題要在購買方式後面");

        // 從章節開始找，避開 <head> 裡同樣文字的 JSON-LD
        var section = home[home.IndexOf("id=\"faq\"")..];
        Assert.Contains("<h3>可以自己挑顏色嗎？</h3>", section);
        var positions = new[] { "可以自己挑顏色嗎？", "可以，從色卡挑。", "怎麼清洗？", "冷水手洗" }
            .Select(text => section.IndexOf(text))
            .ToList();
        Assert.DoesNotContain(-1, positions);
        Assert.Equal(positions.Order(), positions);

        // 答案是純文字：寫了 HTML 也照字面顯示
        Assert.Contains("冷水手洗 &lt;b&gt;不要&lt;/b&gt; 擰乾。", section);
    }

    [Fact]
    public async Task Home_without_faq_has_no_section_or_contents_entry()
    {
        using var site = new SiteFactory();
        site.UseFaq();
        var home = await site.Client().GetStringAsync("/");

        Assert.DoesNotContain("id=\"faq\"", home);
        Assert.DoesNotContain("href=\"#faq\"", home);
    }
}

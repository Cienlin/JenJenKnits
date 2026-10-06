using System.Text.RegularExpressions;
using Markdig;

namespace JenJenKnits.Services;

/// <summary>商品故事 Markdown → HTML。前台與後台預覽共用，預覽看到的就是上線後的樣子。</summary>
public static partial class StoryRenderer
{
    // 不渲染內嵌 HTML（HTML 註解會先被移除，可當作者備註用）；單一換行即換行，中文才不會多出空白
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    /// <summary>空白或只有 HTML 註解時回傳 null，前台就不顯示故事區塊。</summary>
    public static string? ToHtml(string? markdown)
    {
        if (markdown is null)
        {
            return null;
        }

        var text = HtmlComment().Replace(markdown, string.Empty);
        return string.IsNullOrWhiteSpace(text) ? null : Markdown.ToHtml(text, Pipeline);
    }

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex HtmlComment();
}

namespace JenJenKnits.Models;

public record YarnColor(int Number, string Hex);

/// <summary>
/// 泡芙小花提網色卡（Excel LOTTO EX088）的 22 色。色值從色卡照片取樣，只用於螢幕預覽，
/// 實際線色以色卡照片為準。首頁色點與配色模擬器共用。
/// </summary>
public static class YarnPalette
{
    public static IReadOnlyList<YarnColor> Colors { get; } =
    [
        new(2, "#edb3c9"), new(4, "#a60d20"), new(6, "#dc9c27"), new(7, "#f0df47"),
        new(8, "#afab46"), new(9, "#3a9d81"), new(10, "#1d74af"), new(11, "#112b74"),
        new(12, "#0b102c"), new(13, "#d2b1c5"), new(14, "#652e64"), new(15, "#e6dfc7"),
        new(16, "#be9e6f"), new(17, "#885d2a"), new(18, "#452c1f"), new(19, "#3e3e3e"),
        new(20, "#02060a"), new(21, "#8a9cb7"), new(22, "#cbccc7"), new(23, "#e9ced1"),
        new(24, "#ede59d"), new(25, "#81988b"),
    ];
}

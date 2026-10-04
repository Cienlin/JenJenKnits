using System.Reflection;

namespace JenJenKnits;

/// <summary>
/// 頁尾的版本號，例如 v1.0.0 (3f2a1c9)。
/// 版號來自 csproj 的 &lt;Version&gt;；括號內是建置時的 git commit 前 7 碼，由 .NET SDK 自動帶入。
/// </summary>
public static class AppVersion
{
    public static string Display { get; } = Build();

    private static string Build()
    {
        var informational = typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

        var parts = informational.Split('+', 2);
        var version = $"v{parts[0]}";
        return parts.Length == 2 && parts[1].Length >= 7 ? $"{version} ({parts[1][..7]})" : version;
    }
}

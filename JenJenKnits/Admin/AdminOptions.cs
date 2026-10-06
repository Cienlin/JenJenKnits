namespace JenJenKnits.Admin;

/// <summary>
/// appsettings 的 "Admin" 區段：唯一的管理員帳號。
/// 密碼只存雜湊值（用 `dotnet run -- hash-password` 產生），本機放 user-secrets，不寫進 appsettings.json。
/// </summary>
public class AdminOptions
{
    public const string SectionName = "Admin";

    public string? UserName { get; set; }

    public string? PasswordHash { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(PasswordHash);
}

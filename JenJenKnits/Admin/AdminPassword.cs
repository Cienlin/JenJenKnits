using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace JenJenKnits.Admin;

/// <summary>
/// 管理員密碼的雜湊與驗證。只借用 Identity 的 PasswordHasher（PBKDF2，含 salt 與版本資訊），
/// 不使用 Identity 的資料表與頁面。
/// </summary>
public static class AdminPassword
{
    private static readonly PasswordHasher<AdminUser> Hasher = new();
    private static readonly AdminUser Admin = new();

    public static string Hash(string password) => Hasher.HashPassword(Admin, password);

    /// <summary>帳號與密碼都正確才回傳 true；未設定管理員時一律 false。</summary>
    public static bool Verify(AdminOptions options, string? userName, string? password)
    {
        if (!options.IsConfigured || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        // 帳號用固定時間比較，避免從回應時間猜出帳號
        var userMatches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(userName), Encoding.UTF8.GetBytes(options.UserName!));

        PasswordVerificationResult result;
        try
        {
            result = Hasher.VerifyHashedPassword(Admin, options.PasswordHash!, password);
        }
        catch (FormatException)
        {
            // 設定裡的雜湊值格式不對（例如貼成明文密碼）
            return false;
        }

        return userMatches && result != PasswordVerificationResult.Failed;
    }

    /// <summary>`dotnet run -- hash-password`：輸入密碼（不顯示在畫面上），印出可放進設定的雜湊值。</summary>
    public static void PromptAndPrintHash()
    {
        var password = ReadPassword("輸入新的管理員密碼：");
        if (password.Length < 12)
        {
            Console.Error.WriteLine("密碼至少要 12 個字元。");
            return;
        }

        if (!Console.IsInputRedirected && ReadPassword("再輸入一次：") != password)
        {
            Console.Error.WriteLine("兩次輸入不一樣。");
            return;
        }

        Console.Error.WriteLine("把下面這行設成 Admin:PasswordHash，例如：");
        Console.Error.WriteLine("  dotnet user-secrets set \"Admin:PasswordHash\" \"<雜湊值>\"");
        Console.WriteLine(Hash(password));
    }

    private static string ReadPassword(string prompt)
    {
        Console.Error.Write(prompt);
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? "";
        }

        var text = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.Error.WriteLine();
                return text.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (text.Length > 0)
                {
                    text.Length--;
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                text.Append(key.KeyChar);
            }
        }
    }

    private sealed class AdminUser;
}

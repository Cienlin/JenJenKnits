using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using JenJenKnits.Admin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace JenJenKnits.Pages.Admin;

public class LoginModel(IOptions<AdminOptions> admin, ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    /// <summary>登入後回到原本要去的後台頁面（Cookie 驗證導來登入頁時會帶上）。</summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var options = admin.Value;
        if (!options.IsConfigured)
        {
            logger.LogWarning("有人嘗試登入後台，但尚未設定管理員帳號");
        }

        // 不透露是帳號還是密碼錯
        if (!AdminPassword.Verify(options, Input.UserName, Input.Password))
        {
            ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
            return Page();
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, options.UserName!)],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        // 只導向站內網址，避免被拿來轉址到外部網站
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/admin");
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "請輸入帳號")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}

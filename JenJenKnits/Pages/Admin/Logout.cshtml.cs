using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JenJenKnits.Pages.Admin;

/// <summary>登出只接受 POST（帶 antiforgery token），避免別的網站用一個連結把管理員登出。</summary>
public class LogoutModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Admin/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Admin/Login");
    }
}

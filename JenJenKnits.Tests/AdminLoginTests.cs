using System.Net;

namespace JenJenKnits.Tests;

public class AdminLoginTests
{
    [Fact]
    public async Task Admin_pages_redirect_to_login_and_back_after_signing_in()
    {
        using var site = new SiteFactory();
        var client = site.Client();

        var response = await client.GetAsync("/admin/create");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/admin/login?ReturnUrl=%2Fadmin%2Fcreate", response.Headers.Location!.PathAndQuery, StringComparison.OrdinalIgnoreCase);

        var login = await SiteFactory.PostFormAsync(client, response.Headers.Location!.PathAndQuery, new()
        {
            ["Input.UserName"] = SiteFactory.UserName,
            ["Input.Password"] = SiteFactory.Password,
            ["ReturnUrl"] = "/admin/create",
        });
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/admin/create", login.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/admin")).StatusCode);
    }

    [Fact]
    public async Task Wrong_password_shows_a_generic_error()
    {
        using var site = new SiteFactory();
        var client = site.Client();

        var response = await SiteFactory.PostFormAsync(client, "/admin/login", new()
        {
            ["Input.UserName"] = SiteFactory.UserName,
            ["Input.Password"] = "wrong password",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("帳號或密碼錯誤", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/admin")).StatusCode);
    }

    [Fact]
    public async Task Login_does_not_redirect_to_other_sites()
    {
        using var site = new SiteFactory();
        var client = site.Client();

        var response = await SiteFactory.PostFormAsync(client, "/admin/login", new()
        {
            ["Input.UserName"] = SiteFactory.UserName,
            ["Input.Password"] = SiteFactory.Password,
            ["ReturnUrl"] = "https://evil.example/",
        });

        Assert.Equal("/admin", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Logout_requires_signing_in_again()
    {
        using var site = new SiteFactory();
        var client = await SiteFactory.SignedInClientAsync(site);

        // 登出鈕在後台每一頁的頁首，token 從列表頁拿
        var signOut = await SiteFactory.PostFormAsync(client, "/admin/logout", new(), tokenFrom: "/admin");

        Assert.Equal(HttpStatusCode.Redirect, signOut.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/admin")).StatusCode);
    }

    [Fact]
    public async Task Too_many_login_attempts_are_rejected()
    {
        using var site = new SiteFactory();
        var client = site.Client();
        HttpResponseMessage? last = null;

        for (var i = 0; i < 6; i++)
        {
            last = await SiteFactory.PostFormAsync(client, "/admin/login", new()
            {
                ["Input.UserName"] = SiteFactory.UserName,
                ["Input.Password"] = "wrong password",
            });
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
    }

    [Fact]
    public async Task Admin_is_hidden_from_search_engines_and_public_pages()
    {
        using var site = new SiteFactory();
        var client = site.Client();

        var login = await client.GetAsync("/admin/login");
        Assert.Contains("noindex", string.Join(",", login.Headers.GetValues("X-Robots-Tag")));
        Assert.Contains("name=\"robots\" content=\"noindex", await login.Content.ReadAsStringAsync());

        var home = await client.GetStringAsync("/");
        Assert.DoesNotContain("/admin", home, StringComparison.OrdinalIgnoreCase);
    }
}

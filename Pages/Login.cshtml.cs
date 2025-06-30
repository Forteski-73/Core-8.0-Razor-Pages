using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace OXF.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Username == "admin" && Password == "123456")
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Username)
        };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);

            return Redirect("/Index"); // redireciona explicitamente
        }

        ErrorMessage = "Usuário ou senha inválidos";
        return Page();
    }
}
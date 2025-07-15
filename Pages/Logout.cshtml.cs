using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OXF.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            // Limpa a sessão
            HttpContext.Session.Clear();

            // Desloga do esquema de autenticação
            await HttpContext.SignOutAsync("CookieAuth");

            // Redireciona para a página inicial
            return RedirectToPage("/Login");
        }
    }
}

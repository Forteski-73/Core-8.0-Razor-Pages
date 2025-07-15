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
            // Limpa a sess�o
            HttpContext.Session.Clear();

            // Desloga do esquema de autentica��o
            await HttpContext.SignOutAsync("CookieAuth");

            // Redireciona para a p�gina inicial
            return RedirectToPage("/Login");
        }
    }
}

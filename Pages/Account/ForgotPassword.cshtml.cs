using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OXF.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        public string ErrorMessage { get; set; }
        public string Message { get; set; }

        public void OnGet()
        {
            // Apenas exibe a p�gina
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                ErrorMessage = "Informe um e-mail v�lido.";
                return Page();
            }

            // TODO: Chamar o servi�o que gera token de reset, envia email, etc.
            // Simula��o de delay
            await Task.Delay(500);

            // Simule o sucesso
            Message = $"Se o e-mail {Email} estiver cadastrado, um link de recupera��o foi enviado.";

            // N�o exp�e se o email existe ou n�o por seguran�a

            // Limpa o campo
            ModelState.Clear();

            return Page();
        }
    }
}

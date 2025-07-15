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
            // Apenas exibe a página
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                ErrorMessage = "Informe um e-mail válido.";
                return Page();
            }

            // TODO: Chamar o serviço que gera token de reset, envia email, etc.
            // Simulação de delay
            await Task.Delay(500);

            // Simule o sucesso
            Message = $"Se o e-mail {Email} estiver cadastrado, um link de recuperação foi enviado.";

            // Não expõe se o email existe ou não por segurança

            // Limpa o campo
            ModelState.Clear();

            return Page();
        }
    }
}

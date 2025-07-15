using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Models;
using OXF.Services;
using OXF.Constants;

namespace OXF.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILoginAttemptService _loginAttemptService;
        private readonly IClientIpProvider _clientIpProvider;

        public RegisterModel(
            IAuthService authService,
            ILoginAttemptService loginAttemptService,
            IClientIpProvider clientIpProvider)
        {
            _authService = authService;
            _loginAttemptService = loginAttemptService;
            _clientIpProvider = clientIpProvider;
        }

        [BindProperty]
        public ApiUser ApiUser { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        public void OnGet()
        {
            if (TempData.ContainsKey("Username"))
            {
                ApiUser.User = TempData["Username"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Message = Messages.Login.FormInvalid;
                return Page();
            }

            ApiUser.User = ApiUser.User?.Trim();
            ApiUser.Password = ApiUser.Password?.Trim();

            if (string.IsNullOrWhiteSpace(ApiUser.User) || string.IsNullOrWhiteSpace(ApiUser.Password))
            {
                Message = Messages.Login.InvalidCredentials; // Ou outra mensagem apropriada
                return Page();
            }

            try
            {
                var ip = _clientIpProvider.GetClientIp();
                var normalizedUser = ApiUser.User.ToLowerInvariant();
                var key = $"register_attempts_{normalizedUser}_{ip}";

                int tentativas = _loginAttemptService.GetAttempts(key);
                if (tentativas >= 3)
                {
                    Message = Messages.Login.MaxAttempts;
                    return Page();
                }

                var (success, errorMessage) = await _authService.RegisterAsync(ApiUser);

                if (success)
                {
                    _loginAttemptService.ResetAttempts(key);
                    Message = Messages.Login.Success;
                    return Page();
                }
                else
                {
                    _loginAttemptService.IncrementAttempts(key);
                    await Task.Delay(1000);
                    Message = errorMessage;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Message = Messages.ErrorProcess(ex.Message);
                return Page();
            }
        }
    }
}
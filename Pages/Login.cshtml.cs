using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Constants;
using OXF.Models;
using OXF.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OXF.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly IClientIpProvider _clientIpProvider;

    public LoginModel(IAuthService authService, ILoginAttemptService loginAttemptService, IClientIpProvider clientIpProvider )
    {
        _authService         = authService;
        _loginAttemptService = loginAttemptService;
        _clientIpProvider    = clientIpProvider;
    }

    [BindProperty, Required(ErrorMessage = Messages.Login.UsernameRequired)]
    public string Username { get; set; }

    [BindProperty, Required(ErrorMessage = Messages.Login.PasswordRequired)]
    public string Password { get; set; }

    [BindProperty]
    public bool RememberMe { get; set; }

    public string ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var ip = _clientIpProvider.GetClientIp();
            var normalizedUser = Username.ToLowerInvariant();
            var key = $"register_attempts_{normalizedUser}_{ip}";
            //var key = $"login_attempts_{Username}_{ip}";

            int tentativas = _loginAttemptService.GetAttempts(key);
            if (tentativas >= 3)
            {
                ErrorMessage = Messages.Login.MaxAttemptsReached;
                return Page();
            }

            var token = await _authService.AuthenticateAsync(Username, Password);

            if (string.IsNullOrEmpty(token))
            {
                _loginAttemptService.IncrementAttempts(key);
                await Task.Delay(1000);
                ErrorMessage = Messages.Login.InvalidCredentials;
                return Page();
            }

            // Zera tentativas
            _loginAttemptService.ResetAttempts(key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username),
                new Claim("JwtToken", token)
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties // tempo que o token fica salvo no Claim
            {
                IsPersistent = RememberMe,
                ExpiresUtc = RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync("CookieAuth", principal, authProps);

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            // Log the exception (ex) if needed
            ErrorMessage = Messages.ErrorProcess(ex.Message);
            return Page();
        }
    }
}

using Microsoft.AspNetCore.HttpOverrides;
using OXF.Models;
using OXF.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // habilita validação de asp-for
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Register");
});

builder.Services.AddSession();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login";
        options.Cookie.Name = "OxfordOnlineAuth";
        options.Cookie.Path = "/";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    });

builder.Services.AddAntiforgery(options => { options.HeaderName = "RequestToken"; });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient<IAuthService, AuthService>();

builder.Services.AddMemoryCache(); // usa para limitar tentativas d login e registro
builder.Services.AddSingleton<ILoginAttemptService, LoginAttemptService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClientIpProvider, ClientIpProvider>();
builder.Services.AddHttpClient<IProductService, ProductService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run("http://localhost:5001");
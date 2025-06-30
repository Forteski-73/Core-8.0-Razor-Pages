using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Adiciona Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/"); // tudo dentro da pasta / (Pages) exige login
    options.Conventions.AllowAnonymousToPage("/Login"); //Exceção para não entrar em looping solicitando login
});

// Adiciona autenticação com cookies
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login";
        options.Cookie.Name = "OxfordOnlineAuth";    // opcional: nome personalizado
        options.Cookie.Path = "/";                 // restringe cookie à subpasta /
        options.Cookie.HttpOnly = true;               // recomendado
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // só envia cookie em HTTPS
        options.Cookie.SameSite = SameSiteMode.None;  // necessário para proxy reverso HTTPS
    });

// Adiciona autorização (obrigatório junto com autenticação)
builder.Services.AddAuthorization();

var app = builder.Build();

// Configuração de erro para produção
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Configura para entender proxy reverso (Nginx)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Autenticação vem antes de autorização
app.UseAuthentication();
app.UseAuthorization();

// Mapeia as páginas Razor
app.MapRazorPages();

// Executa na porta 5001 (localhost)
app.Run("http://localhost:5001");

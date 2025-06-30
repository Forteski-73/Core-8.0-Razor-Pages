using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Adiciona Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/"); // tudo dentro da pasta / (Pages) exige login
    options.Conventions.AllowAnonymousToPage("/Login"); //Exce��o para n�o entrar em looping solicitando login
});

// Adiciona autentica��o com cookies
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login";
        options.Cookie.Name = "OxfordOnlineAuth";    // opcional: nome personalizado
        options.Cookie.Path = "/";                 // restringe cookie � subpasta /
        options.Cookie.HttpOnly = true;               // recomendado
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // s� envia cookie em HTTPS
        options.Cookie.SameSite = SameSiteMode.None;  // necess�rio para proxy reverso HTTPS
    });

// Adiciona autoriza��o (obrigat�rio junto com autentica��o)
builder.Services.AddAuthorization();

var app = builder.Build();

// Configura��o de erro para produ��o
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

// Autentica��o vem antes de autoriza��o
app.UseAuthentication();
app.UseAuthorization();

// Mapeia as p�ginas Razor
app.MapRazorPages();

// Executa na porta 5001 (localhost)
app.Run("http://localhost:5001");

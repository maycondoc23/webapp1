using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configura o logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders(); // Limpa os provedores de log padrões
    logging.AddConsole(); // Adiciona a saída no console
    logging.AddDebug();   // Adiciona o log para o modo Debug (útil para depuração)
});

// Adiciona o serviço de autenticação com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // Defina o caminho para a página de login
        options.LogoutPath = "/Login/Logout"; // Defina o caminho para o logout
        options.AccessDeniedPath = "/Home/AccessDenied"; // Defina o caminho para acesso negado
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuração do pipeline de requisição
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// **Adiciona a autenticação ao pipeline**


app.UseRouting();
app.UseStaticFiles();  // <-- Certifique-se de que isso está presente

// Define o roteamento padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");



app.UseAuthentication();  // Adiciona autenticação ao pipeline
app.UseAuthorization();   // Adiciona autorização ao pipeline


app.Run();

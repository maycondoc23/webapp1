using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o servi�o de autentica��o com cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // Defina o caminho para a p�gina de login
        options.LogoutPath = "/Login/Logout"; // Defina o caminho para o logout
        options.AccessDeniedPath = "/Home/AccessDenied"; // Defina o caminho para acesso negado
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configura��o do pipeline de requisi��o
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// **Adiciona a autentica��o ao pipeline**


app.UseRouting();
app.UseStaticFiles();  // <-- Certifique-se de que isso est� presente

// Define o roteamento padr�o
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");



app.UseAuthentication();  // Adiciona autentica��o ao pipeline
app.UseAuthorization();   // Adiciona autoriza��o ao pipeline


app.Run();

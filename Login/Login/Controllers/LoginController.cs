using System.Security.Claims;
using Login.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace Login.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Homepage", "Pages");  // Corrigido aqui para o controlador correto
                return Json(new { Msg = "usuario ja logado" });
            }
            return View("~/Views/Login/Login.cshtml");
        }

        public IActionResult Signup()
        {
            return View("~/Views/Login/Cadastrar.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha)
        {
            MySqlConnection MySqlConnection = new MySqlConnection("server=localhost;database=webapp2;uid=root;password=1234");
            await MySqlConnection.OpenAsync();
            MySqlCommand MySqlCommand = MySqlConnection.CreateCommand();
            MySqlCommand.CommandText = $"SELECT * FROM apptable WHERE username = '{username}' AND senha = '{senha}'";

            MySqlDataReader reader = MySqlCommand.ExecuteReader();

            if (await reader.ReadAsync())
            {
                int usuarioId = reader.GetInt32(0);
                string nome = reader.GetString(1);

                List<Claim> direitosacesso = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                    new Claim(ClaimTypes.Name, nome)
                };

                var identity = new ClaimsIdentity(direitosacesso, "Identity.login");
                var userprincipal = new ClaimsPrincipal(new[] { identity });

                await HttpContext.SignInAsync(userprincipal, new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTime.Now.AddHours(1)
                });

                ViewData["UserId"] = usuarioId;

                return RedirectToAction("Homepage", "Pages");  // Corrigido aqui para o controlador correto
                return View("~/Views/Paginas/homepage.cshtml");
                return Json(new { Msg = $"Usuario Logado com sucesso!" });
            }

            return Json(new { Msg = $"Usuario {username} com senha {senha} nao cadastrado" });
        }

        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> cadastro(string nome, string username, string senha)
        {
            MySqlConnection MySqlConnection = new MySqlConnection("server=localhost;database=webapp2;uid=root;password=1234");
            MySqlCommand MySqlCommand = MySqlConnection.CreateCommand();
            await MySqlConnection.OpenAsync();
            MySqlCommand.CommandText = $"INSERT INTO apptable (nome, username, senha) VALUES ('{nome}', '{username}', '{senha}')";
            MySqlDataReader reader = MySqlCommand.ExecuteReader();

            return Json(new { Msg = $"Usuario {username} com nome {nome} cadastrado com senha {senha}" });
        }


        [Authorize]
        public async Task<IActionResult> Usuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (var connection = new MySqlConnection("server=localhost;database=webapp2;uid=root;password=1234"))
            {
                await connection.OpenAsync();

                string query = "SELECT id, nome, username, senha, file_path FROM apptable";  // Incluindo o campo file_path na consulta
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var filePath = reader.IsDBNull(4) ? null : reader.GetString(4);  // Verifica se o file_path é NULL

                            usuarios.Add(new Usuario
                            {
                                Id = reader.GetInt32(0),
                                nome = reader.GetString(1),
                                username = reader.GetString(2),
                                senha = reader.GetString(3),
                                image = filePath  // Mapeando o campo file_path para a propriedade image
                            });
                        }
                    }
                }
            }

            return View(usuarios);  // Passando os dados para a View
        }
    }
}

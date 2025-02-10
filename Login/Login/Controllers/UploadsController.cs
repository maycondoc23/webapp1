using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Login.Controllers
{
    public class UploadsController : Controller
    {
        private readonly string _diretorioFotos = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "fotos");


        [HttpPost]
        public async Task<IActionResult> UploadSelectedFoto(IFormFile foto)
        {
            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(usuarioId, out var id))
            {
                if (foto != null && foto.Length > 0)
                {
                    //var filePath = Path.Combine(_diretorioFotos, id + Path.GetExtension(foto.FileName));
                    var filePath = Path.Combine(_diretorioFotos, id + Path.GetExtension(foto.FileName));
                    var usuarioUsername = @User.Identity.Name;
                    // Cria o diretório, se necessário
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }

                    // Salvar o caminho no banco de dados
                    using (var connection = new MySqlConnection("server=localhost;database=webapp2;uid=root;password=1234"))
                    {
                        await connection.OpenAsync();

                        string query = $"INSERT INTO apptable (user_id, file_path, username) WHERE user_id=={id} VALUES (@UserId, @FilePath, @usuarioUsername)";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UserId", id);
                            command.Parameters.AddWithValue("@FilePath", $"{id}_.{usuarioUsername}_{Path.GetExtension(foto.FileName)}");
                            command.Parameters.AddWithValue("@usuarioUsername", usuarioUsername);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    return Json(new { Msg = $"Codigo executado / {id}. {filePath}" });

                    return RedirectToAction("Profile", "User");
                }
                else
                { 
                    ViewBag.Error = "Nenhuma foto foi selecionada.";
                }
            }
            else
            {
                ViewBag.Error = "Usuário não autenticado.";
            }
            return Json(new { Msg = "Codigo executado 2" });

        }
    }
}

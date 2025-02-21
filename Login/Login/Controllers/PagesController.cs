using Login.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Login.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult Homepage()
        {
            return View("~/Views/Paginas/homepage.cshtml");
        }

        [Authorize]
        public  IActionResult UploadFoto()
        {
            return View("~/Views/Paginas/DashboardAsusFT.cshtml");
        }
        
        [Authorize]
        public  IActionResult DataResult()
        {
            return View("~/Views/Paginas/dataresult_pictures.cshtml");
        }
                
        [Authorize]
        public  IActionResult Report()
        {
            return View("~/Views/Paginas/report.cshtml");
        }




        private readonly string _diretorioFotos = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "fotos");

        [Authorize, HttpPost]
        public async Task<IActionResult> SubmitReport(string nome, string report, string detalhe, IEnumerable<IFormFile> images)
        {
            // Verifica se há imagens para serem processadas
            if (images != null && images.Any())
            {
                // Abrindo a conexão com o banco de dados
                using (var connection = new MySqlConnection("server=localhost;database=webapp2;uid=root;password=12345"))
                {
                    await connection.OpenAsync();

                    foreach (var image in images)
                    {
                        // Definir o nome da imagem
                        string imageName = null;

                        // Verificar se a imagem foi enviada
                        if (image.Length > 0)
                        {
                            // Gerar o nome da imagem
                            imageName = nome + "_" + Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            var filePath = Path.Combine(_diretorioFotos, imageName);

                            // Cria o diretório se não existir
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            // Salvar a imagem no diretório
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            // Comando para inserir os dados na tabela (uma linha por foto)
                            var query = "INSERT INTO apptable2 (nome, report, detalhe, image) VALUES (@nome, @report, @detalhe, @image)";
                            using (var command = new MySqlCommand(query, connection))
                            {
                                // Adicionando parâmetros para prevenir injeção de SQL
                                command.Parameters.AddWithValue("@nome", nome);
                                command.Parameters.AddWithValue("@report", report);
                                command.Parameters.AddWithValue("@detalhe", detalhe);
                                command.Parameters.AddWithValue("@image", imageName);

                                // Executando o comando
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }

            // Retornar resposta JSON com dados inseridos
            return Json(new { Msg = nome, Report = report, Detalhe = detalhe, Images = images.Select(i => i.FileName) });
        }




        //[Authorize]
        public async Task<IActionResult> ReportTable()
        {
            List<Reportclass> reporttable = new List<Reportclass>();

            using (var connection = new MySqlConnection("server=localhost;database=webapp2;uid=root;password=12345"))
            {
                await connection.OpenAsync();

                string query = "SELECT id, nome, report, detalhe, image FROM apptable2";  // Incluindo o campo file_path na consulta
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var filePath = reader.IsDBNull(4) ? null : reader.GetString(4);  // Verifica se o file_path é NULL

                            reporttable.Add(new Reportclass
                            {
                                Id = reader.GetInt32(0),
                                nome = reader.GetString(1),
                                report = reader.GetString(2),
                                detalhe = reader.GetString(3),
                                image = filePath  // Mapeando o campo file_path para a propriedade image
                            });
                        }
                    }
                }
            }

            return View("~/Views/Paginas/reporttable.cshtml", reporttable);
        }




    }
}

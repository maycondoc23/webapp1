using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Login.Models;
using System;
using static System.Collections.Specialized.BitVector32;
using System.Data;

namespace Login.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly string _connectionString = "server=localhost;database=webapp2;uid=root;password=12345";

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        // Método para buscar os dados e passá-los para a view
        [HttpGet]
        public async Task<ActionResult> Index(DateTime? date, string? model, string? customer)
        {
            try
            {
                _logger.LogInformation($"Iniciando a consulta para obter os dados de estações. Data: {date}");

                // Chama os métodos para obter os dados da estação e os dados dos clientes
                var stationDataRecords = await GetStationDataAsync(date, model, customer);
                var customers = await GetCustomersasync(); // Chama o método para obter os clientes
                var Models = await GetModelsasync(); // Chama o método para obter os clientes

                if (stationDataRecords.Count == 0)
                {
                    _logger.LogWarning("Nenhum dado encontrado na consulta ao banco de dados.");
                }

                // Passa os dados tanto de estação quanto de clientes para a View
                var viewModel = new DashboardViewModel
                {
                    StationDataList = stationDataRecords,
                    CustomersList = customers,
                    ModelsList = Models
                };

                return View(viewModel); // Passa o viewModel para a View
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar dados no banco de dados: {ex.Message}");
                return View("Error"); // Retorna uma view de erro, se necessário
            }
        }

        private async Task<List<Customers>> GetCustomersasync()
        {
            var customers = new List<Customers>();  // Lista de clientes

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Query para buscar os dados dos clientes
                    string query = "SELECT * FROM webapp2.foxconncustomers";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // Adiciona cada cliente na lista
                                customers.Add(new Customers
                                {
                                    Id = reader.GetInt32("id"),  // ID do cliente
                                    Customername = reader.GetString("customer")  // Nome do cliente
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log de erro caso algo falhe
                Console.WriteLine($"Erro ao buscar clientes: {ex.Message}");
            }

            return customers;  // Retorna a lista de clientes
        }
        
        private async Task<List<Modelos>> GetModelsasync()
        {
            var customers = new List<Modelos>();  // Lista de clientes

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Query para buscar os dados dos clientes
                    string query = "SELECT * FROM webapp2.foxconnmodels";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                // Adiciona cada cliente na lista
                                customers.Add(new Modelos
                                {
                                    Id = reader.GetInt32("id"),  // ID do cliente
                                    Modelname = reader.GetString("model")  // Nome do cliente
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log de erro caso algo falhe
                Console.WriteLine($"Erro ao buscar clientes: {ex.Message}");
            }

            return customers;  // Retorna a lista de clientes
        }

        // Função assíncrona para obter os dados da tabela no banco de dados
        private async Task<List<StationData>> GetStationDataAsync(DateTime? date,  string? model, string? customer)
        {
            var stationDataList = new List<StationData>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Se uma data foi fornecida, usa ela no filtro da query
                    string query = "SELECT STATION, COUNT(*) AS PASS_COUNT " +
                                   "FROM webapp2.asusft " +
                                   "WHERE STATUS = 'PASS' ";

                    if (date.HasValue)
                    {
                        query += $"AND DATE = '{date.Value.ToString("dd-MM-yyyy").Replace('-', '/')}' ";
                    }

                    // Aqui, o modelo é passado para o filtro SQL
                    query += $"AND MODEL = '{model}' and CUSTOMER = '{customer}' ";

                    // Resto da query
                    query += "GROUP BY STATION ORDER BY PASS_COUNT DESC;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Se a data foi fornecida, adiciona o parâmetro para a query
                        if (date.HasValue)
                        {
                            command.Parameters.AddWithValue("@Date", date.Value.ToString("yyyy-MM-dd"));
                        }

                        _logger.LogInformation($"----Consultando os dados: {query}");
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                 stationDataList.Add(new StationData
                                {
                                    Station = reader["STATION"].ToString(),
                                    Count = Convert.ToInt32(reader["PASS_COUNT"])
                                });
                            }
                        }

                    }
                }

                _logger.LogInformation("Dados de estações obtidos com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao executar a consulta: {ex.Message}");
            }

            return stationDataList;
        }


        // Nova ação para retornar os dados de STATION em formato JSON
        [HttpGet]
        public async Task<JsonResult> GetStationDataJson(DateTime? date, string? model, string? customer)
        {
            try
            {
                _logger.LogInformation("Iniciando a consulta para obter os dados de estações em formato JSON.2");

                var records = await GetStationDataAsync(date, model, customer);

                if (records.Count == 0)
                {
                    _logger.LogWarning("Nenhum dado encontrado na consulta ao banco de dados.");
                }

                // Retorna os dados em formato JSON
                return Json(records);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar dados no banco de dados: {ex.Message}");
                return Json(new { error = "Erro ao buscar os dados" });  // Retorna um erro em formato JSON
            }
        }
    }
}

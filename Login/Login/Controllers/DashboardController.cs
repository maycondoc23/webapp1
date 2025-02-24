using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Login.Models;
using System;

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
        public async Task<ActionResult> Index(DateTime? date)
        {
            try
            {
                // Log the incoming date parameter
                _logger.LogInformation($"Iniciando a consulta para obter os dados de estações. Data: {date}");

                // Use the provided date or null if not provided
                var records = await GetStationDataAsync(date);

                if (records.Count == 0)
                {
                    _logger.LogWarning("Nenhum dado encontrado na consulta ao banco de dados.");
                }

                return View(records);  // Passa os dados para a View
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar dados no banco de dados: {ex.Message}");
                return View("Error");  // Retorna uma view de erro, se necessário
            }
        }

        // Função assíncrona para obter os dados da tabela no banco de dados
        private async Task<List<StationData>> GetStationDataAsync(DateTime? date)
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

                    // Adiciona o filtro de data apenas se a data não for nula
                    if (date.HasValue)
                    {
                        query += $"AND DATE = '{date.Value.ToString("dd-MM-yyyy").Replace('-','/')}' ";
                    }

                    // Adiciona o agrupamento e a ordenação
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
        public async Task<JsonResult> GetStationDataJson(DateTime? date)
        {
            try
            {
                _logger.LogInformation("Iniciando a consulta para obter os dados de estações em formato JSON.2");

                var records = await GetStationDataAsync(date);

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
 
﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly string _connectionString = "server=localhost;database=webapp2;uid=root;password=1234";

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        // Método para buscar os dados e passá-los para a view
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Iniciando a consulta para obter os dados de estações.");

                var records = await GetStationDataAsync();

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
        private async Task<List<StationData>> GetStationDataAsync()
        {
            var stationDataList = new List<StationData>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT STATION, COUNT(*) AS PASS_COUNT\r\nFROM webapp2.asusft\r\nWHERE STATUS = \"PASS\"\r\nGROUP BY STATION\r\nORDER BY PASS_COUNT DESC;";

                    using (var command = new MySqlCommand(query, connection))
                    {
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
        public async Task<JsonResult> GetStationDataJson()
        {
            try
            {
                _logger.LogInformation("Iniciando a consulta para obter os dados de estações em formato JSON.");

                var records = await GetStationDataAsync();

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
 
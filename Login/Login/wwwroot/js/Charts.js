var chart;  // Variável global para armazenar o gráfico

// Definir o valor da data para o dia de hoje
document.addEventListener("DOMContentLoaded", function () {
    var today = new Date();

    // Ajustar a data para o formato "yyyy-mm-dd" no fuso horário local
    var day = today.getDate();
    var month = today.getMonth() + 1;  // Janeiro é 0!
    var year = today.getFullYear();

    // Adicionar zero à esquerda para dia e mês, se necessário
    if (day < 10) day = '0' + day;
    if (month < 10) month = '0' + month;

    // Formatar a data como "yyyy-mm-dd"
    var formattedToday = year + '-' + month + '-' + day;

    // Definir o valor do campo de data como a data de hoje
    document.getElementById("date").value = formattedToday;
});

// Captura o evento de envio do formulário
document.getElementById("filterForm").addEventListener("submit", function (event) {
    event.preventDefault();  // Impede o envio padrão do formulário

    var selectedDate = document.getElementById("date").value;  // Data selecionada no campo de data
    var selectedModel = document.getElementById("modelinput").value;  // Data selecionada no campo de data
    var selectedcustomer = document.getElementById("customerinput").value;  // Data selecionada no campo de data


    console.log('==============================================')
    console.log(selectedModel)
    // Fazendo a requisição AJAX para obter os dados com a data selecionada
    fetch("/Dashboard/GetStationDataJson?date=" + selectedDate + "&model=" + selectedModel + "&customer=" + selectedcustomer)
        .then(response => response.json())
        .then(data => {
            // Processamento dos dados para o gráfico
            var stations = data.map(item => item.station);
            var passCounts = data.map(item => item.count);

            // Se já existe um gráfico, destrua-o antes de criar um novo
            if (chart) {
                chart.destroy();
            }

            // Configuração do gráfico
            const config = {
                type: 'bar',
                data: {
                    labels: stations,  // Labels (estações)
                    datasets: [{
                        label: 'Quantidade de PASS',
                        data: passCounts,  // Quantidade de PASS
                        borderColor: randomColor(),
                        backgroundColor: "#cde5b3",
                        fill: false
                    }]
                },
                options: {
                    plugins: {
                        datalabels: {
                            anchor: 'end',
                            align: 'end',
                            color: 'black',
                            font: {
                                size: "30",
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            suggestedMax: Math.max(...passCounts) + 2,
                            ticks: {
                                padding: 10
                            }
                        }
                    },
                    // Adicionando o evento de clique
                    onClick: (e, elements) => {
                        if (elements.length > 0) {
                            var index = elements[0].index;  // Índice do item clicado
                            var station = stations[index];   // Obter o nome da estação
                            var passCount = passCounts[index]; // Obter a quantidade de PASS

                            // Exibir o modal com as informações da estação
                            showModal(station, passCount);
                        }
                    }
                },
                plugins: [ChartDataLabels],
            };

            // Função para gerar cores aleatórias
            function randomColor() {
                return 'rgb(' + Math.floor(Math.random() * 256) + ',' + Math.floor(Math.random() * 256) + ',' + Math.floor(Math.random() * 256) + ')';
            }

            // Inicializar o gráfico
            const ctx = document.getElementById('myChart').getContext('2d');
            chart = new Chart(ctx, config);  // Agora, o gráfico é armazenado na variável global
        })
        .catch(error => {
            console.error("Erro ao carregar dados: ", error);
        });
});

// Função para mostrar o modal com detalhes da estação
function showModal(station, passCount) {
    var modal = document.getElementById("myModal");
    var stationDetails = document.getElementById("stationDetails");

    // Atualizando o conteúdo do modal
    stationDetails.innerHTML = "Estação: " + station + "<br>Quantidade de PASS: " + passCount;

    // Exibindo o modal
    modal.style.display = "block";
}

// Função para fechar o modal
document.getElementById("closeModalBtn").onclick = function () {
    var modal = document.getElementById("myModal");
    modal.style.display = "none";
}

// Fechar o modal se o usuário clicar fora dele
window.onclick = function (event) {
    var modal = document.getElementById("myModal");
    if (event.target == modal) {
        modal.style.display = "none";
    }
}
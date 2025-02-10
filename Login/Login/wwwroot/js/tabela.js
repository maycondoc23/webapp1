// Função para obter o valor da célula com base no índice da coluna (idx)
const getCellValue = (tr, idx) => tr.children[idx].innerText || tr.children[idx].textContent;

// Função para comparar as células de uma linha para ordenação
const comparer = (idx, asc) => (a, b) => {
    // Obtemos os valores das células das duas linhas a serem comparadas
    const v1 = getCellValue(asc ? a : b, idx);
    const v2 = getCellValue(asc ? b : a, idx);

    // Se os valores não são vazios e são números, realizamos a comparação numérica
    return v1 !== '' && v2 !== '' && !isNaN(v1) && !isNaN(v2) ? v1 - v2
        : v1.toString().localeCompare(v2); // Caso contrário, usamos comparação lexicográfica
};

// Adiciona um evento de clique em cada cabeçalho de tabela para ordenação
document.querySelectorAll('th').forEach((th, index) => {
    th.addEventListener('click', () => {
        const table = th.closest('table'); // Encontra a tabela mais próxima do cabeçalho
        const rows = Array.from(table.querySelectorAll('tbody tr')); // Pega todas as linhas de dados da tabela (sem o cabeçalho)

        // Ordena as linhas com base na coluna que foi clicada
        const sortedRows = rows.sort(comparer(index, this.asc = !this.asc)); // Alterna a direção da ordenação

        // Reaplica as linhas ordenadas de volta ao corpo da tabela (<tbody>)
        const tbody = table.querySelector('tbody');
        sortedRows.forEach(row => tbody.appendChild(row)); // Anexa as linhas ordenadas de volta ao <tbody>
    });
});


// Função para abrir o modal e exibir a imagem em tamanho grande
function openModal(imageSrc) {
    var modal = document.getElementById("myModal"); // Obtém o modal
    var modalImg = document.getElementById("modalImage"); // Obtém o elemento da imagem dentro do modal
    var captionText = document.getElementById("caption"); // Obtém o texto da legenda do modal

    modal.style.display = "block";  // Exibe o modal
    modalImg.src = imageSrc;       // Define o src da imagem para a imagem clicada
    captionText.innerHTML = "Foto em tamanho grande"; // Define uma legenda para a imagem
}

// Função para fechar o modal
function closeModal() {
    var modal = document.getElementById("myModal"); // Obtém o modal
    modal.style.display = "none"; // Esconde o modal
}

// Função para filtrar as linhas da tabela com base no que é digitado na caixa de pesquisa
function filterTable() {
    var input, filter, table, tr, td, i, j, txtValue;

    input = document.getElementById("searchInput"); // Obtém o campo de pesquisa
    filter = input.value.toUpperCase(); // Converte o valor da pesquisa para maiúsculo para facilitar a comparação
    table = document.querySelector("Table"); // Obtém a tabela
    tr = table.getElementsByTagName("tr"); // Obtém todas as linhas da tabela (incluindo o cabeçalho)

    // Loop pelas linhas da tabela, começando pela linha 1 (pois a linha 0 é o cabeçalho)
    for (i = 1; i < tr.length; i++) {
        tr[i].style.display = "none"; // Inicialmente escondemos a linha
        td = tr[i].getElementsByTagName("td"); // Obtém as células da linha

        // Loop pelas células da linha para procurar o texto correspondente à pesquisa
        for (j = 0; j < td.length; j++) {
            if (td[j]) { // Se a célula existe
                txtValue = td[j].textContent || td[j].innerText; // Pega o texto da célula
                // Se o texto da célula contém o texto de pesquisa, mostra a linha
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    tr[i].style.display = ""; // Exibe a linha
                    break; // Para o loop interno após encontrar a célula correspondente
                }
            }
        }
    }
}


// Adiciona um evento de tecla para capturar o pressionamento da tecla "Esc"
document.addEventListener('keydown', function (event) {
    if (event.key === "Escape" || event.keyCode === 27) { // Se a tecla pressionada for Esc (tecla 27)
        closeModal(); // Fecha o modal
    }
});
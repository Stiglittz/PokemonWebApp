using ClosedXML.Excel;
using PokemonWebApp.Models.Pokemon;

namespace PokemonWebApp.Services
{
    /// <summary>
    /// Servicio para generar archivos Excel con datos de Pokémon
    /// Utiliza ClosedXML para crear hojas de cálculo profesionales
    /// </summary>
    public class ExcelService : IExcelService
    {
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger;
            // ClosedXML no requiere configuración de licencia especial
        }

        /// <summary>
        /// Exporta múltiples Pokémon a Excel con formato profesional
        /// </summary>
        public async Task<byte[]> ExportPokemonToExcelAsync(List<Pokemon> pokemonList, string fileName = "Pokemon_Export")
        {
            try
            {
                _logger.LogInformation($"Iniciando exportación de {pokemonList.Count} Pokémon a Excel con ClosedXML");

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Lista de Pokémon");

                    // CONFIGURAR ENCABEZADOS
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Nombre";
                    worksheet.Cell(1, 3).Value = "Altura (dm)";
                    worksheet.Cell(1, 4).Value = "Peso (hg)";
                    worksheet.Cell(1, 5).Value = "Tipos";
                    worksheet.Cell(1, 6).Value = "Experiencia Base";
                    worksheet.Cell(1, 7).Value = "Fecha de Exportación";

                    // ESTILO DE ENCABEZADOS
                    var headerRange = worksheet.Range(1, 1, 1, 7);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.Blue;
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

                    // LLENAR DATOS
                    for (int i = 0; i < pokemonList.Count; i++)
                    {
                        var pokemon = pokemonList[i];
                        var row = i + 2; // Empezar en fila 2

                        worksheet.Cell(row, 1).Value = pokemon.Id;
                        worksheet.Cell(row, 2).Value = CapitalizeName(pokemon.Name);
                        worksheet.Cell(row, 3).Value = pokemon.Height;
                        worksheet.Cell(row, 4).Value = pokemon.Weight;
                        worksheet.Cell(row, 5).Value = string.Join(", ", pokemon.Types.Select(t => CapitalizeName(t.Type.Name)));
                        worksheet.Cell(row, 6).Value = pokemon.BaseExperience;
                        worksheet.Cell(row, 7).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                        // ESTILO ALTERNADO DE FILAS
                        if (i % 2 == 0)
                        {
                            var rowRange = worksheet.Range(row, 1, row, 7);
                            rowRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        // BORDES
                        var cellRange = worksheet.Range(row, 1, row, 7);
                        cellRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    // AJUSTAR ANCHO DE COLUMNAS
                    worksheet.Column(1).Width = 8;  // ID
                    worksheet.Column(2).Width = 20; // Nombre
                    worksheet.Column(3).Width = 12; // Altura
                    worksheet.Column(4).Width = 12; // Peso
                    worksheet.Column(5).Width = 25; // Tipos
                    worksheet.Column(6).Width = 18; // Experiencia
                    worksheet.Column(7).Width = 22; // Fecha

                    // INFORMACIÓN ADICIONAL
                    var lastRow = pokemonList.Count + 3;
                    worksheet.Cell(lastRow, 1).Value = $"Total de Pokémon exportados: {pokemonList.Count}";
                    worksheet.Cell(lastRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(lastRow, 1).Style.Font.Italic = true;

                    worksheet.Cell(lastRow + 1, 1).Value = $"Exportado desde: Pokémon Web App";
                    worksheet.Cell(lastRow + 1, 1).Style.Font.FontSize = 10;
                    worksheet.Cell(lastRow + 1, 1).Style.Font.FontColor = XLColor.Gray;

                    _logger.LogInformation("Exportación a Excel completada exitosamente con ClosedXML");

                    // CONVERTIR A BYTES
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return await Task.FromResult(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar Pokémon a Excel con ClosedXML");
                throw new Exception("Error al generar el archivo Excel", ex);
            }
        }

        /// <summary>
        /// Exporta un solo Pokémon con información detallada
        /// </summary>
        public async Task<byte[]> ExportSinglePokemonToExcelAsync(Pokemon pokemon, string? fileName = null)
        {
            try
            {
                fileName ??= $"Pokemon_{CapitalizeName(pokemon.Name)}";
                _logger.LogInformation($"Exportando Pokémon individual: {pokemon.Name} con ClosedXML");

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add($"Pokémon - {CapitalizeName(pokemon.Name)}");

                    // TÍTULO PRINCIPAL
                    worksheet.Range(1, 1, 1, 4).Merge();
                    worksheet.Cell(1, 1).Value = $"INFORMACIÓN DE {CapitalizeName(pokemon.Name).ToUpper()}";
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.Green;
                    worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;

                    // ENCABEZADOS DE INFORMACIÓN
                    var row = 3;
                    worksheet.Cell(row, 1).Value = "Campo";
                    worksheet.Cell(row, 2).Value = "Valor";
                    
                    // Estilo de encabezados de campos
                    var headerFieldRange = worksheet.Range(row, 1, row, 2);
                    headerFieldRange.Style.Font.Bold = true;
                    headerFieldRange.Style.Fill.BackgroundColor = XLColor.DarkGray;
                    headerFieldRange.Style.Font.FontColor = XLColor.White;

                    // DATOS DEL POKÉMON
                    var fields = new Dictionary<string, string>
                    {
                        { "ID", pokemon.Id.ToString() },
                        { "Nombre", CapitalizeName(pokemon.Name) },
                        { "Altura", $"{pokemon.Height} decímetros" },
                        { "Peso", $"{pokemon.Weight} hectogramos" },
                        { "Experiencia Base", pokemon.BaseExperience.ToString() },
                        { "Tipos", string.Join(", ", pokemon.Types.Select(t => CapitalizeName(t.Type.Name))) },
                        { "Exportado el", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                    };

                    row = 4;
                    foreach (var field in fields)
                    {
                        worksheet.Cell(row, 1).Value = field.Key;
                        worksheet.Cell(row, 2).Value = field.Value;
                        
                        // Estilo alternado
                        if ((row - 4) % 2 == 0)
                        {
                            var fieldRange = worksheet.Range(row, 1, row, 2);
                            fieldRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        }
                        
                        row++;
                    }

                    // AJUSTAR COLUMNAS
                    worksheet.Column(1).Width = 20;
                    worksheet.Column(2).Width = 30;

                    // BORDES PARA TODA LA TABLA
                    var tableRange = worksheet.Range(3, 1, row - 1, 2);
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return await Task.FromResult(stream.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al exportar Pokémon individual: {pokemon.Name} con ClosedXML");
                throw new Exception("Error al generar el archivo Excel individual", ex);
            }
        }

        /// <summary>
        /// Capitaliza la primera letra de cada palabra
        /// </summary>
        private string CapitalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            
            return string.Join(" ", name.Split('-', ' ')
                .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        }
    }
}
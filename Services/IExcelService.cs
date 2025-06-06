using PokemonWebApp.Models.Pokemon;

namespace PokemonWebApp.Services
{
    /// <summary>
    /// Interfaz para el servicio de exportación a Excel
    /// Define los métodos para generar archivos Excel con datos de Pokémon
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// Exporta una lista de Pokémon a un archivo Excel
        /// </summary>
        /// <param name="pokemonList">Lista de Pokémon a exportar</param>
        /// <param name="fileName">Nombre del archivo (opcional)</param>
        /// <returns>Array de bytes del archivo Excel generado</returns>
        Task<byte[]> ExportPokemonToExcelAsync(List<Pokemon> pokemonList, string fileName = "Pokemon_Export");
        
        /// <summary>
        /// Exporta un solo Pokémon a Excel con información detallada
        /// </summary>
        /// <param name="pokemon">Pokémon individual a exportar</param>
        /// <param name="fileName">Nombre del archivo (opcional)</param>
        /// <returns>Array de bytes del archivo Excel generado</returns>
        Task<byte[]> ExportSinglePokemonToExcelAsync(Pokemon pokemon, string? fileName = null);
    }
}
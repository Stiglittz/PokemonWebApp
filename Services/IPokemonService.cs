using PokemonWebApp.Models.Pokemon;
using PokemonWebApp.Models.ViewModels;

namespace PokemonWebApp.Services
{
    /// <summary>
    /// Interfaz para el servicio de Pokémon que maneja todas las operaciones con la PokeAPI
    /// </summary>
    public interface IPokemonService
    {
        /// <summary>
        /// Obtiene una lista paginada de Pokémon con filtros aplicados
        /// </summary>
        /// <param name="pageNumber">Número de página (base 1)</param>
        /// <param name="pageSize">Cantidad de elementos por página</param>
        /// <param name="nameFilter">Filtro por nombre (opcional)</param>
        /// <param name="typeFilter">Filtro por tipo (opcional)</param>
        /// <param name="minHeight">Altura mínima en decímetros (opcional)</param>
        /// <param name="maxHeight">Altura máxima en decímetros (opcional)</param>
        /// <returns>ViewModel con los Pokémon filtrados y información de paginación</returns>
        Task<PokemonIndexViewModel> GetPokemonListAsync(
            int pageNumber = 1, 
            int pageSize = 20, 
            string? nameFilter = null, 
            string? typeFilter = null, 
            int? minHeight = null, 
            int? maxHeight = null);

        /// <summary>
        /// Obtiene la lista de todos los tipos de Pokémon disponibles
        /// Utiliza cache local para evitar llamadas repetidas a la API
        /// </summary>
        /// <returns>Lista de opciones de tipos para filtros</returns>
        Task<List<PokemonTypeOption>> GetPokemonTypesAsync();

        /// <summary>
        /// Obtiene los detalles completos de un Pokémon específico por su ID
        /// </summary>
        /// <param name="id">ID del Pokémon</param>
        /// <returns>Modelo completo del Pokémon o null si no se encuentra</returns>
        Task<Models.Pokemon.Pokemon?> GetPokemonDetailsAsync(int id);

        /// <summary>
        /// Obtiene los detalles completos de un Pokémon específico por su nombre
        /// </summary>
        /// <param name="name">Nombre del Pokémon</param>
        /// <returns>Modelo completo del Pokémon o null si no se encuentra</returns>
        Task<Models.Pokemon.Pokemon?> GetPokemonDetailsByNameAsync(string name);

        /// <summary>
        /// Obtiene la información de especies de un Pokémon
        /// Incluye descripción, habitat, si es legendario, etc.
        /// </summary>
        /// <param name="id">ID del Pokémon</param>
        /// <returns>Información de especies o null si no se encuentra</returns>
        Task<PokemonSpecies?> GetPokemonSpeciesAsync(int id);

        /// <summary>
        /// Obtiene el ViewModel completo para el modal de detalles
        /// Combina información básica del Pokémon con datos de especies
        /// </summary>
        /// <param name="id">ID del Pokémon</param>
        /// <returns>ViewModel para el modal de detalles o null si no se encuentra</returns>
        Task<PokemonDetailsViewModel?> GetPokemonDetailsViewModelAsync(int id);

        /// <summary>
        /// Obtiene múltiples Pokémon por sus IDs
        /// Útil para exportaciones y operaciones en lote
        /// </summary>
        /// <param name="ids">Lista de IDs de Pokémon</param>
        /// <returns>Lista de Pokémon encontrados</returns>
        Task<List<Models.Pokemon.Pokemon>> GetMultiplePokemonAsync(List<int> ids);

        /// <summary>
        /// MÉTODO LEGACY - Mantener compatibilidad con controlador existente
        /// </summary>
        Task<Models.Pokemon.Pokemon?> GetPokemonByIdAsync(int id);
        
        /// <summary>
        /// Limpia todos los caches del servicio
        /// Útil para refrescar datos cuando sea necesario
        /// </summary>
        void ClearCache();
    }
}
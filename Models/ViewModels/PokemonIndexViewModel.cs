using PokemonWebApp.Models.Pokemon;

namespace PokemonWebApp.Models.ViewModels
{
    /// <summary>
    /// ViewModel mejorado para la página principal de Pokémon
    /// Incluye filtros avanzados y mejor manejo de paginación
    /// </summary>
    public class PokemonIndexViewModel
    {
        // Lista de Pokémon a mostrar
        public List<Pokemon.Pokemon> Pokemons { get; set; } = new();

        // Información de paginación mejorada
        public int CurrentPage { get; set; } = 1;
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 20;
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }

        // Filtros expandidos
        public string? NameFilter { get; set; }
        public string? TypeFilter { get; set; }  // ¡NUEVO!
        public int? MinHeight { get; set; }      // ¡NUEVO!
        public int? MaxHeight { get; set; }      // ¡NUEVO!

        // ¡MANTENER COMPATIBILIDAD CON VISTA ANTERIOR!
        public string? SpeciesFilter { get; set; }
        public List<string> AvailableSpecies { get; set; } = new();

        // Opciones para dropdowns
        public List<string> AvailableTypes { get; set; } = new();      // ¡NUEVO!
        public List<PokemonTypeOption> TypeOptions { get; set; } = new(); // ¡NUEVO!

        // Estado de la aplicación
        public bool IsLoading { get; set; }
        public string? ErrorMessage { get; set; }
        public bool HasFilters => !string.IsNullOrEmpty(NameFilter) || 
                                 !string.IsNullOrEmpty(TypeFilter) || 
                                 !string.IsNullOrEmpty(SpeciesFilter) ||
                                 MinHeight.HasValue || 
                                 MaxHeight.HasValue;

        // Propiedades calculadas para paginación
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public int StartIndex => (CurrentPage - 1) * PageSize + 1;
        public int EndIndex => Math.Min(CurrentPage * PageSize, TotalCount);
        
        // Propiedades para mejorar la UX de paginación
        public bool ShowFirstPage => CurrentPage > 3;
        public bool ShowLastPage => CurrentPage < TotalPages - 2;
        public int StartPageNumber => Math.Max(1, CurrentPage - 2);
        public int EndPageNumber => Math.Min(TotalPages, CurrentPage + 2);
    }

    /// <summary>
    /// Clase para las opciones de tipo con información adicional
    /// </summary>
    public class PokemonTypeOption
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
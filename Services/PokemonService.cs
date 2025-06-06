using Newtonsoft.Json;
using PokemonWebApp.Models.Pokemon;
using PokemonWebApp.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace PokemonWebApp.Services
{
    /// <summary>
    /// Servicio que consume la PokeAPI para obtener información de Pokémon
    /// CON SISTEMA DE CACHE MEJORADO usando IMemoryCache
    /// </summary>
    public class PokemonService : IPokemonService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PokemonService> _logger;
        private readonly IMemoryCache _memoryCache;

        // Claves de cache constantes para consistencia
        private const string CACHE_KEY_TYPES = "pokemon_types";
        private const string CACHE_KEY_POKEMON_DETAILS = "pokemon_details_{0}";
        private const string CACHE_KEY_POKEMON_SPECIES = "pokemon_species_{0}";
        private const string CACHE_KEY_SEARCH_RESULTS = "search_results_{0}";

        // Tiempos de expiración configurables
        private static readonly TimeSpan CacheExpirationTypes = TimeSpan.FromHours(6);      // Tipos: 6 horas
        private static readonly TimeSpan CacheExpirationDetails = TimeSpan.FromMinutes(30); // Detalles: 30 minutos
        private static readonly TimeSpan CacheExpirationSpecies = TimeSpan.FromHours(1);    // Especies: 1 hora
        private static readonly TimeSpan CacheExpirationSearch = TimeSpan.FromMinutes(15);  // Búsquedas: 15 minutos

        // Constructor: recibe HttpClient, Logger e IMemoryCache por inyección de dependencias
        public PokemonService(HttpClient httpClient, ILogger<PokemonService> logger, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _memoryCache = memoryCache;

        }

        // ====================================
        // MÉTODOS DE CACHE HELPERS
        // ====================================

        /// <summary>
        /// Obtiene o crea un elemento en cache con logging mejorado Y MÉTRICAS
        /// </summary>
        private async Task<T?> GetOrCreateCacheAsync<T>(
            string cacheKey,
            Func<Task<T?>> factory,
            TimeSpan expiration,
            string itemDescription = "item") where T : class
        {
            // Intentar obtener del cache primero
            if (_memoryCache.TryGetValue(cacheKey, out T? cachedItem) && cachedItem != null)
            {
                _logger.LogInformation("✅ Cache HIT: {ItemDescription} obtenido desde cache (clave: {CacheKey})",
                    itemDescription, cacheKey);


                return cachedItem;
            }

            _logger.LogInformation("⚡ Cache MISS: Obteniendo {ItemDescription} desde API (clave: {CacheKey})",
                itemDescription, cacheKey);


            try
            {
                // Obtener desde la fuente original
                var item = await factory();

                if (item != null)
                {
                    // Configurar opciones de cache
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration,
                        SlidingExpiration = TimeSpan.FromMinutes(5),
                        Priority = CacheItemPriority.Normal
                    };

                    // Guardar en cache
                    _memoryCache.Set(cacheKey, item, cacheOptions);

                    _logger.LogInformation("💾 Cache STORED: {ItemDescription} guardado en cache por {Expiration} (clave: {CacheKey})",
                        itemDescription, expiration, cacheKey);
                }
                else
                {
                    _logger.LogWarning("⚠️ Cache SKIP: {ItemDescription} es null, no se guardó en cache", itemDescription);
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cache ERROR: Error al obtener {ItemDescription} para cache", itemDescription);
                return null;
            }
        }

        /// <summary>
        /// Invalida un elemento específico del cache
        /// </summary>
        private void InvalidateCache(string cacheKey, string itemDescription = "item")
        {
            _memoryCache.Remove(cacheKey);
            _logger.LogInformation("🗑️ Cache INVALIDATED: {ItemDescription} removido del cache (clave: {CacheKey})",
                itemDescription, cacheKey);
        }

        /// <summary>
        /// Obtiene estadísticas del cache para debugging
        /// </summary>
        public CacheStats GetCacheStats()
        {
            // Verificar qué elementos están en cache
            var stats = new CacheStats();

            if (_memoryCache.TryGetValue(CACHE_KEY_TYPES, out _))
                stats.TypesCached = true;

            // Verificar algunos Pokémon populares (IDs 1-10)
            for (int i = 1; i <= 10; i++)
            {
                if (_memoryCache.TryGetValue(string.Format(CACHE_KEY_POKEMON_DETAILS, i), out _))
                    stats.PokemonDetailsCached++;

                if (_memoryCache.TryGetValue(string.Format(CACHE_KEY_POKEMON_SPECIES, i), out _))
                    stats.SpeciesCached++;
            }

            _logger.LogInformation("📊 Cache Stats: Types={TypesCached}, Details={PokemonDetailsCached}, Species={SpeciesCached}",
                stats.TypesCached, stats.PokemonDetailsCached, stats.SpeciesCached);

            return stats;
        }

        /// <summary>
        /// Clase para estadísticas del cache
        /// </summary>
        public class CacheStats
        {
            public bool TypesCached { get; set; }
            public int PokemonDetailsCached { get; set; }
            public int SpeciesCached { get; set; }
        }

        // ====================================
        // MÉTODOS PRINCIPALES CON CACHE MEJORADO
        // ====================================

        /// <summary>
        /// Obtiene los detalles completos de un Pokémon específico por su ID
        /// CON CACHE MEJORADO usando IMemoryCache
        /// </summary>
        public async Task<Models.Pokemon.Pokemon?> GetPokemonDetailsAsync(int id)
        {
            var cacheKey = string.Format(CACHE_KEY_POKEMON_DETAILS, id);
            var itemDescription = $"Pokémon details para ID {id}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                async () => await FetchPokemonDetailsFromApiAsync(id),
                CacheExpirationDetails,
                itemDescription
            );
        }

        /// <summary>
        /// Obtiene la información de especies de un Pokémon CON CACHE MEJORADO
        /// </summary>
        public async Task<PokemonSpecies?> GetPokemonSpeciesAsync(int id)
        {
            var cacheKey = string.Format(CACHE_KEY_POKEMON_SPECIES, id);
            var itemDescription = $"Pokémon species para ID {id}";

            return await GetOrCreateCacheAsync(
                cacheKey,
                async () => await FetchPokemonSpeciesFromApiAsync(id),
                CacheExpirationSpecies,
                itemDescription
            );
        }

        /// <summary>
        /// Obtiene la lista de tipos de Pokémon CON CACHE MEJORADO
        /// </summary>
        public async Task<List<PokemonTypeOption>> GetPokemonTypesAsync()
        {
            return await GetOrCreateCacheAsync(
                CACHE_KEY_TYPES,
                async () => await FetchPokemonTypesFromApiAsync(),
                CacheExpirationTypes,
                "Tipos de Pokémon"
            ) ?? new List<PokemonTypeOption>();
        }

        /// <summary>
        /// Busca nombres de Pokémon para autocompletado CON CACHE
        /// </summary>
        public async Task<List<string>> SearchPokemonNamesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return new List<string>();

            var cacheKey = string.Format(CACHE_KEY_SEARCH_RESULTS, query.ToLower());
            var itemDescription = $"Búsqueda para '{query}'";

            return await GetOrCreateCacheAsync(
                cacheKey,
                async () => await FetchPokemonSearchFromApiAsync(query),
                CacheExpirationSearch,
                itemDescription
            ) ?? new List<string>();
        }

        /// <summary>
        /// Limpia todos los caches del servicio - MEJORADO
        /// </summary>
        public void ClearCache()
        {
            try
            {
                // Limpiar tipos
                InvalidateCache(CACHE_KEY_TYPES, "Tipos de Pokémon");

                // Limpiar detalles de Pokémon (IDs 1-1000, común)
                for (int i = 1; i <= 1000; i++)
                {
                    var detailsKey = string.Format(CACHE_KEY_POKEMON_DETAILS, i);
                    var speciesKey = string.Format(CACHE_KEY_POKEMON_SPECIES, i);

                    if (_memoryCache.TryGetValue(detailsKey, out _))
                        InvalidateCache(detailsKey, $"Pokémon details {i}");

                    if (_memoryCache.TryGetValue(speciesKey, out _))
                        InvalidateCache(speciesKey, $"Pokémon species {i}");
                }

                _logger.LogInformation("🧹 CACHE CLEARED: Todos los caches han sido limpiados completamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar cache");
            }
        }

        // ====================================
        // MÉTODOS PRIVADOS PARA API (SIN CACHE)
        // ====================================

        /// <summary>
        /// Obtiene detalles del Pokémon desde la API (sin cache)
        /// </summary>
        private async Task<Models.Pokemon.Pokemon?> FetchPokemonDetailsFromApiAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"pokemon/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned status {StatusCode} for Pokémon {Id}",
                        response.StatusCode, id);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var pokemon = JsonConvert.DeserializeObject<Models.Pokemon.Pokemon>(content);

                if (pokemon != null)
                {
                    _logger.LogInformation("✅ API SUCCESS: Pokémon {Name} (ID: {Id}) obtenido desde API",
                        pokemon.Name, pokemon.Id);
                }

                return pokemon;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error fetching Pokémon {Id}", id);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON error deserializing Pokémon {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching Pokémon {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene especies del Pokémon desde la API (sin cache)
        /// </summary>
        private async Task<PokemonSpecies?> FetchPokemonSpeciesFromApiAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"pokemon-species/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned status {StatusCode} for Pokémon species {Id}",
                        response.StatusCode, id);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var species = JsonConvert.DeserializeObject<PokemonSpecies>(content);

                if (species != null)
                {
                    _logger.LogInformation("✅ API SUCCESS: Species {Name} (ID: {Id}) obtenido desde API",
                        species.Name, species.Id);
                }

                return species;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching species for Pokémon {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene tipos de Pokémon desde la API (sin cache)
        /// </summary>
        private async Task<List<PokemonTypeOption>?> FetchPokemonTypesFromApiAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("type?limit=20");
                var typesResponse = JsonConvert.DeserializeObject<PokemonListResponse>(response);

                if (typesResponse?.Results != null)
                {
                    var typeOptions = typesResponse.Results.Select(t => new PokemonTypeOption
                    {
                        Name = t.Name,
                        DisplayName = char.ToUpper(t.Name[0]) + t.Name.Substring(1),
                        ColorClass = GetTypeColorClass(t.Name),
                        Count = 0
                    }).OrderBy(t => t.DisplayName).ToList();

                    _logger.LogInformation("✅ API SUCCESS: {Count} tipos obtenidos desde API", typeOptions.Count);
                    return typeOptions;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Pokémon types from API");
                return null;
            }
        }

        /// <summary>
        /// Busca nombres de Pokémon en la API (sin cache)
        /// </summary>
        private async Task<List<string>?> FetchPokemonSearchFromApiAsync(string query)
        {
            try
            {
                var response = await _httpClient.GetStringAsync("pokemon?limit=1000");
                var pokemonList = JsonConvert.DeserializeObject<PokemonListResponse>(response);

                if (pokemonList?.Results != null)
                {
                    var matchingNames = pokemonList.Results
                        .Where(p => p.Name.StartsWith(query.ToLower(), StringComparison.OrdinalIgnoreCase))
                        .Select(p => char.ToUpper(p.Name[0]) + p.Name.Substring(1))
                        .Take(10)
                        .ToList();

                    _logger.LogInformation("✅ API SUCCESS: {Count} resultados de búsqueda para '{Query}'",
                        matchingNames.Count, query);
                    return matchingNames;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Pokémon names for query '{Query}'", query);
                return null;
            }
        }

        // ====================================
        // MÉTODOS PRINCIPALES MANTENIDOS
        // ====================================

        /// <summary>
        /// Obtiene una lista paginada de Pokémon con filtros avanzados
        /// MÉTODO PRINCIPAL ACTUALIZADO para compatibilidad con interfaz
        /// </summary>
        public async Task<PokemonIndexViewModel> GetPokemonListAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? nameFilter = null,
            string? typeFilter = null)
        {
            var viewModel = new PokemonIndexViewModel
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                NameFilter = nameFilter,
                TypeFilter = typeFilter
            };

            try
            {
                _logger.LogInformation("Obteniendo Pokémon - Página: {Page}, Filtros: Nombre={NameFilter}, Tipo={TypeFilter}",
                    pageNumber, nameFilter, typeFilter);

                // Calcular offset para paginación
                var offset = (pageNumber - 1) * pageSize;

                if (!string.IsNullOrEmpty(typeFilter))
                {
                    _logger.LogInformation("🌐 Usando búsqueda global por tipo: {TypeFilter}", typeFilter);
                    return await GetPokemonByTypeGlobalAsync(typeFilter, pageNumber, pageSize, nameFilter);
                }

                // Si hay filtro por nombre específico, buscar ese Pokémon
                if (!string.IsNullOrEmpty(nameFilter))
                {
                    var filteredPokemon = await GetPokemonByNameAsync(nameFilter);
                    if (filteredPokemon != null)
                    {
                        viewModel.Pokemons = new List<Models.Pokemon.Pokemon> { filteredPokemon };
                        viewModel.TotalCount = 1;
                        viewModel.HasNext = false;
                        viewModel.HasPrevious = false;
                        return viewModel;
                    }
                    else
                    {
                        viewModel.Pokemons = new List<Models.Pokemon.Pokemon>();
                        viewModel.TotalCount = 0;
                        viewModel.HasNext = false;
                        viewModel.HasPrevious = false;
                        return viewModel;
                    }
                }

                // Obtener lista paginada de la API
                var url = $"pokemon?limit={pageSize}&offset={offset}";
                var response = await _httpClient.GetStringAsync(url);
                var pokemonList = JsonConvert.DeserializeObject<PokemonListResponse>(response);

                if (pokemonList?.Results != null)
                {
                    // Obtener detalles de cada Pokémon en paralelo
                    var pokemonTasks = pokemonList.Results.Select(async item =>
                    {
                        return await GetPokemonByIdAsync(item.Id);
                    });

                    var pokemonDetails = await Task.WhenAll(pokemonTasks);

                    // Filtrar los que se obtuvieron correctamente
                    var allPokemons = pokemonDetails.Where(p => p != null).Cast<Models.Pokemon.Pokemon>().ToList();

                    // Aplicar filtros avanzados
                    var filteredPokemons = ApplyAdvancedFilters(allPokemons, typeFilter);

                    viewModel.Pokemons = filteredPokemons;
                    viewModel.TotalCount = pokemonList.Count; // Nota: Este es el total sin filtros
                    viewModel.HasNext = !string.IsNullOrEmpty(pokemonList.Next);
                    viewModel.HasPrevious = !string.IsNullOrEmpty(pokemonList.Previous);
                }

                _logger.LogInformation("Se obtuvieron {Count} Pokémon exitosamente", viewModel.Pokemons.Count);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener Pokémon");
                viewModel.ErrorMessage = "Error de conexión con la API. Por favor, intenta de nuevo.";
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout al obtener Pokémon");
                viewModel.ErrorMessage = "La solicitud tardó demasiado. Por favor, intenta de nuevo.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener Pokémon");
                viewModel.ErrorMessage = "Ocurrió un error inesperado. Por favor, intenta de nuevo.";
            }

            return viewModel;
        }

        /// <summary>
        /// NUEVO: Obtiene todos los Pokémon de un tipo específico con paginación
        /// </summary>
        public async Task<PokemonIndexViewModel> GetPokemonByTypeGlobalAsync(
            string typeName,
            int pageNumber = 1,
            int pageSize = 20,
            string? nameFilter = null,
            int? minHeight = null,
            int? maxHeight = null)
        {
            var viewModel = new PokemonIndexViewModel
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                NameFilter = nameFilter,
                TypeFilter = typeName,
                MinHeight = minHeight,
                MaxHeight = maxHeight
            };

            try
            {
                _logger.LogInformation("🔍 Búsqueda global por tipo: {TypeName}", typeName);

                var typeResponse = await _httpClient.GetStringAsync($"type/{typeName.ToLower()}");
                var typeData = JsonConvert.DeserializeObject<PokemonTypeResponse>(typeResponse);

                if (typeData?.Pokemon != null)
                {
                    var allPokemonTasks = typeData.Pokemon.Take(200).Select(async p => // Limitar a 200 para rendimiento
                    {
                        try
                        {
                            var pokemonId = ExtractIdFromUrl(p.Pokemon.Url);
                            return await GetPokemonByIdAsync(pokemonId);
                        }
                        catch { return null; }
                    });

                    var allPokemons = (await Task.WhenAll(allPokemonTasks))
                        .Where(p => p != null)
                        .Cast<Models.Pokemon.Pokemon>()
                        .ToList();

                    // Aplicar filtros
                    var filteredPokemons = allPokemons;

                    if (!string.IsNullOrEmpty(nameFilter))
                    {
                        filteredPokemons = filteredPokemons
                            .Where(p => p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    filteredPokemons = ApplyHeightFilters(filteredPokemons, minHeight, maxHeight);

                    // Paginación
                    var totalCount = filteredPokemons.Count;
                    var skipCount = (pageNumber - 1) * pageSize;
                    var pagedPokemons = filteredPokemons.Skip(skipCount).Take(pageSize).ToList();

                    viewModel.Pokemons = pagedPokemons;
                    viewModel.TotalCount = totalCount;
                    viewModel.HasNext = skipCount + pageSize < totalCount;
                    viewModel.HasPrevious = pageNumber > 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en búsqueda global por tipo {TypeName}", typeName);
                viewModel.ErrorMessage = $"Error al buscar Pokémon de tipo {typeName}";
            }

            return viewModel;
        }

        /// <summary>
        /// NUEVO: Extrae ID de la URL de la API
        /// </summary>
        private int ExtractIdFromUrl(string url)
        {
            var segments = url.TrimEnd('/').Split('/');
            return int.Parse(segments.Last());
        }

        /// <summary>
        /// NUEVO: Aplica filtros de altura corregidos
        /// </summary>
        private List<Models.Pokemon.Pokemon> ApplyHeightFilters(
            List<Models.Pokemon.Pokemon> pokemons,
            int? minHeight,
            int? maxHeight)
        {
            return pokemons.Where(p => MatchesHeightFilters(p, minHeight, maxHeight)).ToList();
        }

        /// <summary>
        /// NUEVO: Verifica filtros de altura en decímetros
        /// </summary>
        private bool MatchesHeightFilters(Models.Pokemon.Pokemon pokemon, int? minHeight, int? maxHeight)
        {
            var pokemonHeightInDm = pokemon.Height; // Ya está en decímetros

            if (minHeight.HasValue && pokemonHeightInDm < minHeight.Value)
                return false;

            if (maxHeight.HasValue && pokemonHeightInDm > maxHeight.Value)
                return false;

            return true;
        }

        /// <summary>
        /// MÉTODO LEGACY: Mantener compatibilidad con código existente
        /// </summary>
        public async Task<PokemonIndexViewModel> GetPokemonsAsync(
            int page = 1,
            int pageSize = 20,
            string? nameFilter = null,
            string? typeFilter = null,
            int? minHeight = null,
            int? maxHeight = null)
        {
            // Redirigir al método nuevo
            return await GetPokemonListAsync(page, pageSize, nameFilter, typeFilter);
        }

        /// <summary>
        /// Obtiene un Pokémon específico por su ID
        /// </summary>
        public async Task<Models.Pokemon.Pokemon?> GetPokemonByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"pokemon/{id}");
                var pokemon = JsonConvert.DeserializeObject<Models.Pokemon.Pokemon>(response);
                return pokemon;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el Pokémon con ID: {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene los detalles completos de un Pokémon específico por su nombre
        /// </summary>
        public async Task<Models.Pokemon.Pokemon?> GetPokemonDetailsByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogWarning("Nombre de Pokémon vacío o nulo");
                    return null;
                }

                var normalizedName = name.ToLower().Trim();
                _logger.LogInformation("Obteniendo detalles del Pokémon con nombre: {Name}", normalizedName);

                var response = await _httpClient.GetAsync($"pokemon/{normalizedName}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudo obtener el Pokémon con nombre {Name}. Status: {StatusCode}",
                        normalizedName, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var pokemon = JsonConvert.DeserializeObject<Models.Pokemon.Pokemon>(content);

                if (pokemon != null)
                {
                    // Guardar en cache también
                    var cacheKey = string.Format(CACHE_KEY_POKEMON_DETAILS, pokemon.Id);
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = CacheExpirationDetails,
                        SlidingExpiration = TimeSpan.FromMinutes(5),
                        Priority = CacheItemPriority.Normal
                    };
                    _memoryCache.Set(cacheKey, pokemon, cacheOptions);

                    _logger.LogInformation("Pokémon {Name} (ID: {Id}) obtenido exitosamente",
                        pokemon.Name, pokemon.Id);
                }

                return pokemon;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al obtener detalles del Pokémon {Name}", name);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar detalles del Pokémon {Name}", name);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener detalles del Pokémon {Name}", name);
                return null;
            }
        }

        /// <summary>
        /// Obtiene el ViewModel completo para el modal de detalles
        /// Combina información básica del Pokémon con datos de especies
        /// </summary>
        public async Task<PokemonDetailsViewModel?> GetPokemonDetailsViewModelAsync(int id)
        {
            try
            {
                _logger.LogInformation("Creando ViewModel de detalles para el Pokémon {Id}", id);

                // Obtener datos básicos del Pokémon (obligatorio)
                var pokemon = await GetPokemonDetailsAsync(id);
                if (pokemon == null)
                {
                    _logger.LogWarning("No se pudo obtener información básica del Pokémon {Id}", id);
                    return null;
                }

                // Crear el ViewModel con los datos básicos
                var viewModel = new PokemonDetailsViewModel
                {
                    Id = pokemon.Id,
                    Name = pokemon.Name,
                    Height = pokemon.Height,
                    Weight = pokemon.Weight,
                    BaseExperience = pokemon.BaseExperience,
                    Types = pokemon.Types,
                    Abilities = pokemon.Abilities,
                    Stats = pokemon.Stats,
                    Sprites = pokemon.Sprites
                };

                // Intentar obtener datos de especies (opcional, no crítico)
                try
                {
                    var species = await GetPokemonSpeciesAsync(id);
                    if (species != null)
                    {
                        viewModel.Species = species;
                        _logger.LogInformation("Datos de especies agregados al ViewModel del Pokémon {Id}", id);
                    }
                    else
                    {
                        _logger.LogInformation("No se pudieron obtener datos de especies para el Pokémon {Id}, continuando sin ellos", id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al obtener especies del Pokémon {Id}, continuando sin datos de especies", id);
                    // No retornamos null, continuamos sin los datos de especies
                }

                _logger.LogInformation("ViewModel de detalles creado exitosamente para el Pokémon {Name} (ID: {Id})",
                    viewModel.Name, viewModel.Id);

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear ViewModel de detalles para el Pokémon {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Obtiene múltiples Pokémon por sus IDs
        /// Útil para exportaciones y operaciones en lote
        /// </summary>
        public async Task<List<Models.Pokemon.Pokemon>> GetMultiplePokemonAsync(List<int> ids)
        {
            var pokemonList = new List<Models.Pokemon.Pokemon>();

            if (ids == null || !ids.Any())
            {
                _logger.LogWarning("Lista de IDs vacía o nula");
                return pokemonList;
            }

            _logger.LogInformation("Obteniendo {Count} Pokémon por IDs", ids.Count);

            // Usar Task.WhenAll para paralelizar las llamadas
            var tasks = ids.Select(async id =>
            {
                try
                {
                    var pokemon = await GetPokemonDetailsAsync(id);
                    return pokemon;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener Pokémon con ID {Id}", id);
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);

            // Filtrar resultados nulos
            pokemonList.AddRange(results.Where(p => p != null)!);

            _logger.LogInformation("Se obtuvieron {Count} de {Total} Pokémon solicitados",
                pokemonList.Count, ids.Count);

            return pokemonList;
        }

        // ====================================
        // MÉTODOS AUXILIARES MANTENIDOS
        // ====================================

        /// <summary>
        /// Busca un Pokémon por nombre exacto
        /// </summary>
        private async Task<Models.Pokemon.Pokemon?> GetPokemonByNameAsync(string name)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"pokemon/{name.ToLower()}");
                var pokemon = JsonConvert.DeserializeObject<Models.Pokemon.Pokemon>(response);
                return pokemon;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener el Pokémon con nombre: {Name}", name);
                return null;
            }
        }

        /// <summary>
        /// Aplica filtros avanzados a una lista de Pokémon
        /// </summary>
        private List<Models.Pokemon.Pokemon> ApplyAdvancedFilters(
            List<Models.Pokemon.Pokemon> pokemons,
            string? typeFilter)
        {
            return pokemons.Where(p => MatchesTypeFilter(p, typeFilter)).ToList();
        }

        /// <summary>
        /// Verifica si un Pokémon cumple con los filtros especificados
        /// </summary>
        private bool MatchesTypeFilter(Models.Pokemon.Pokemon pokemon, string? typeFilter)
        {
            if (string.IsNullOrEmpty(typeFilter)) return true;

            return pokemon.Types.Any(t =>
                t.Type.Name.Equals(typeFilter, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Obtiene la clase CSS para el color de un tipo de Pokémon
        /// </summary>
        private string GetTypeColorClass(string typeName)
        {
            return typeName.ToLower() switch
            {
                "normal" => "bg-secondary",
                "fire" => "bg-danger",
                "water" => "bg-primary",
                "electric" => "bg-warning",
                "grass" => "bg-success",
                "ice" => "bg-info",
                "fighting" => "bg-dark",
                "poison" => "bg-purple",
                "ground" => "bg-brown",
                "flying" => "bg-light",
                "psychic" => "bg-pink",
                "bug" => "bg-green",
                "rock" => "bg-gray",
                "ghost" => "bg-indigo",
                "dragon" => "bg-violet",
                "dark" => "bg-dark",
                "steel" => "bg-secondary",
                "fairy" => "bg-pink",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// COMPATIBILIDAD: Obtiene especies (redirige a tipos para mantener compatibilidad)
        /// </summary>
        public async Task<List<string>> GetSpeciesAsync()
        {
            try
            {
                var types = await GetPokemonTypesAsync();
                return types.Select(t => t.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener especies (compatibilidad)");
                return new List<string>();
            }
        }
        
        
        /// <summary>
        /// Extrae el tipo de cache de la clave para métricas
        /// </summary>
        private string GetCacheType(string cacheKey)
        {
            if (cacheKey.Contains("pokemon_types")) return "Types";
            if (cacheKey.Contains("pokemon_details")) return "PokemonDetails";
            if (cacheKey.Contains("pokemon_species")) return "PokemonSpecies";
            if (cacheKey.Contains("search_results")) return "SearchResults";
            return "Unknown";
        }
    }
}
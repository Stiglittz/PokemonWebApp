using Microsoft.AspNetCore.Mvc;
using PokemonWebApp.Services;
using PokemonWebApp.Models.Pokemon;
using PokemonWebApp.Models.ViewModels;

namespace PokemonWebApp.Controllers
{
    /// <summary>
    /// Controlador principal para manejar las páginas de Pokémon
    /// CON LOGGING MEJORADO SERILOG (SIN MÉTRICAS)
    /// </summary>
    public class PokemonController : Controller
    {
        private readonly IPokemonService _pokemonService;
        private readonly IExcelService _excelService;
        private readonly ILogger<PokemonController> _logger;
        private readonly IEmailService _emailService;

        // Constructor: recibe servicios por inyección de dependencias
        public PokemonController(
            IPokemonService pokemonService, 
            IExcelService excelService, 
            IEmailService emailService, 
            ILogger<PokemonController> logger)
        {
            _pokemonService = pokemonService;
            _excelService = excelService;
            _logger = logger;
            _emailService = emailService;
        }

        /// <summary>
        /// Página principal que muestra la lista de Pokémon
        /// CON LOGGING MEJORADO
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="nameFilter">Filtro por nombre</param>
        /// <param name="typeFilter">Filtro por tipo</param>
        /// <param name="minHeight">Altura mínima</param>
        /// <param name="maxHeight">Altura máxima</param>
        /// <returns>Vista con lista de Pokémon</returns>
        public async Task<IActionResult> Index(
            int page = 1,
            string? nameFilter = null,
            string? typeFilter = null,
            int? minHeight = null,
            int? maxHeight = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("🎯 Cargando página de Pokémon - Página: {Page}, Filtros aplicados", page);

                // Validar parámetros
                if (page < 1) page = 1;
                if (minHeight < 0) minHeight = null;
                if (maxHeight < 0) maxHeight = null;
                if (minHeight > maxHeight && maxHeight.HasValue)
                {
                    (minHeight, maxHeight) = (maxHeight, minHeight); // Intercambiar si están al revés
                }

                // Obtener datos del servicio
                var viewModel = await _pokemonService.GetPokemonListAsync(
                    page, 20, nameFilter, typeFilter, minHeight, maxHeight);

                // Obtener tipos para el dropdown
                if (!viewModel.TypeOptions.Any())
                {
                    viewModel.TypeOptions = await _pokemonService.GetPokemonTypesAsync();
                }

                // Agregar información de debugging en desarrollo
                if (HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                {
                    ViewBag.DebugInfo = new
                    {
                        TotalPokemons = viewModel.Pokemons.Count,
                        FiltersApplied = viewModel.HasFilters,
                        PageInfo = $"{viewModel.CurrentPage}/{viewModel.TotalPages}"
                    };
                }

                stopwatch.Stop();
                _logger.LogInformation("✅ Página de Pokémon cargada exitosamente en {ElapsedMs}ms - {Count} resultados", 
                    stopwatch.ElapsedMilliseconds, viewModel.Pokemons.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al cargar la página de Pokémon");

                var errorViewModel = new Models.ViewModels.PokemonIndexViewModel
                {
                    ErrorMessage = "No se pudieron cargar los Pokémon. Por favor, intenta de nuevo más tarde.",
                    TypeOptions = await _pokemonService.GetPokemonTypesAsync(),
                    CurrentPage = page,
                    NameFilter = nameFilter,
                    TypeFilter = typeFilter,
                    MinHeight = minHeight,
                    MaxHeight = maxHeight
                };

                return View(errorViewModel);
            }
        }

        /// <summary>
        /// Endpoint AJAX para obtener los detalles completos de un Pokémon
        /// CON LOGGING MEJORADO
        /// </summary>
        /// <param name="id">ID del Pokémon</param>
        /// <returns>JSON con PokemonDetailsViewModel o error</returns>
        [HttpGet]
        public async Task<IActionResult> GetPokemonDetails(int id)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("🔍 Solicitados detalles del Pokémon con ID: {Id}", id);

                if (id <= 0)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("⚠️ ID de Pokémon inválido: {Id}", id);
                    return Json(new { success = false, message = "ID de Pokémon inválido" });
                }

                var pokemonDetails = await _pokemonService.GetPokemonDetailsViewModelAsync(id);

                if (pokemonDetails == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarning("❌ No se encontró el Pokémon con ID: {Id}", id);
                    return Json(new { success = false, message = "Pokémon no encontrado" });
                }

                // Retornar datos estructurados para el frontend
                var response = new
                {
                    success = true,
                    pokemon = new
                    {
                        // Información básica
                        id = pokemonDetails.Id,
                        name = pokemonDetails.FormattedName,
                        height = pokemonDetails.FormattedHeight,
                        weight = pokemonDetails.FormattedWeight,
                        baseExperience = pokemonDetails.BaseExperience,

                        // Imágenes
                        mainImage = pokemonDetails.MainImageUrl,
                        backImage = pokemonDetails.BackImageUrl,

                        // Tipos y habilidades
                        types = pokemonDetails.Types.Select(t => new
                        {
                            name = char.ToUpper(t.Type.Name[0]) + t.Type.Name.Substring(1),
                            slot = t.Slot
                        }).ToList(),

                        abilities = pokemonDetails.Abilities.Select(a => new
                        {
                            name = char.ToUpper(a.Ability.Name[0]) + a.Ability.Name.Substring(1),
                            isHidden = a.IsHidden,
                            slot = a.Slot
                        }).ToList(),

                        // Stats
                        stats = pokemonDetails.Stats.Select(s => new
                        {
                            name = s.Stat.Name,
                            baseStat = s.BaseStat,
                            effort = s.Effort
                        }).ToList(),

                        totalStats = pokemonDetails.TotalStats,

                        // Información de especies (si está disponible)
                        species = pokemonDetails.Species != null ? new
                        {
                            description = pokemonDetails.Description,
                            category = pokemonDetails.Category,
                            habitat = pokemonDetails.Habitat,
                            generation = pokemonDetails.Generation,
                            growthRate = pokemonDetails.GrowthRate,
                            isLegendary = pokemonDetails.Species.IsLegendary,
                            isMythical = pokemonDetails.Species.IsMythical,
                            captureRate = pokemonDetails.Species.CaptureRate,
                            baseHappiness = pokemonDetails.Species.BaseHappiness
                        } : null,

                        // Datos adicionales
                        isSpecial = pokemonDetails.IsSpecialPokemon,
                        specialLabel = pokemonDetails.SpecialLabel,
                        typeColor = pokemonDetails.GetTypeColor()
                    }
                };

                stopwatch.Stop();
                _logger.LogInformation("✅ Detalles del Pokémon {Name} (ID: {Id}) obtenidos exitosamente en {ElapsedMs}ms",
                    pokemonDetails.Name, pokemonDetails.Id, stopwatch.ElapsedMilliseconds);

                return Json(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error inesperado al obtener detalles del Pokémon {Id}", id);

                return Json(new
                {
                    success = false,
                    message = "Error interno del servidor. Por favor, inténtalo de nuevo."
                });
            }
        }

        /// <summary>
        /// Exporta Pokémon seleccionados a Excel
        /// CON LOGGING MEJORADO
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExportSelectedToExcel([FromBody] List<int> selectedIds)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (selectedIds == null || !selectedIds.Any())
                {
                    return BadRequest("No se han seleccionado Pokémon para exportar");
                }

                _logger.LogInformation("📋 Iniciando exportación de {Count} Pokémon seleccionados", selectedIds.Count);

                // Obtener los Pokémon seleccionados
                var selectedPokemon = new List<Models.Pokemon.Pokemon>();
                foreach (var id in selectedIds)
                {
                    var pokemon = await _pokemonService.GetPokemonByIdAsync(id);
                    if (pokemon != null)
                    {
                        selectedPokemon.Add(pokemon);
                    }
                }

                if (!selectedPokemon.Any())
                {
                    return NotFound("No se encontraron los Pokémon seleccionados");
                }

                // Generar archivo Excel
                var excelBytes = await _excelService.ExportPokemonToExcelAsync(selectedPokemon, "Pokemon_Seleccionados");
                var fileName = $"Pokemon_Seleccionados_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                stopwatch.Stop();
                _logger.LogInformation("✅ Exportación Excel completada: {Count} Pokémon en {ElapsedMs}ms - {FileSize} bytes", 
                    selectedPokemon.Count, stopwatch.ElapsedMilliseconds, excelBytes.Length);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al exportar Pokémon seleccionados");
                return StatusCode(500, $"Error al exportar: {ex.Message}");
            }
        }

        /// <summary>
        /// Exporta un Pokémon individual a Excel
        /// CON LOGGING MEJORADO
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportSingleToExcel(int id)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📋 Iniciando exportación individual del Pokémon ID: {Id}", id);

                var pokemon = await _pokemonService.GetPokemonByIdAsync(id);
                if (pokemon == null)
                {
                    return NotFound($"No se encontró el Pokémon con ID {id}");
                }

                var excelBytes = await _excelService.ExportSinglePokemonToExcelAsync(pokemon);
                var fileName = $"Pokemon_{pokemon.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                stopwatch.Stop();
                _logger.LogInformation("✅ Exportación individual completada: {Name} en {ElapsedMs}ms", 
                    pokemon.Name, stopwatch.ElapsedMilliseconds);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al exportar Pokémon individual");
                return StatusCode(500, $"Error al exportar: {ex.Message}");
            }
        }

        /// <summary>
        /// Exporta todos los Pokémon de la página actual a Excel
        /// CON LOGGING MEJORADO
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ExportCurrentPageToExcel([FromBody] ExportPageRequest request)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📋 Iniciando exportación de página actual: {Page}", request.Page);

                // Obtener los mismos Pokémon que se muestran en la página actual
                var result = await _pokemonService.GetPokemonListAsync(
                    request.Page,
                    20, // pageSize
                    request.Name,
                    request.TypeName,
                    request.MinHeight,
                    request.MaxHeight
                );

                if (!result.Pokemons.Any())
                {
                    return NotFound("No hay Pokémon en la página actual para exportar");
                }

                var excelBytes = await _excelService.ExportPokemonToExcelAsync(result.Pokemons, "Pokemon_Pagina_Actual");
                var fileName = $"Pokemon_Pagina_{request.Page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                stopwatch.Stop();
                _logger.LogInformation("✅ Exportación de página completada: {Count} Pokémon en {ElapsedMs}ms", 
                    result.Pokemons.Count, stopwatch.ElapsedMilliseconds);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al exportar página actual");
                return StatusCode(500, $"Error al exportar página: {ex.Message}");
            }
        }

        /// <summary>
        /// Envía un email con información de un Pokémon individual
        /// CON LOGGING MEJORADO
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendSinglePokemonEmail([FromBody] EmailFormViewModel model)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Datos del formulario inválidos" });
                }

                if (!_emailService.IsConfigurationValid())
                {
                    return Json(new { success = false, message = "Configuración de email no disponible" });
                }

                var pokemonId = model.PokemonIds.FirstOrDefault();
                if (pokemonId <= 0)
                {
                    return Json(new { success = false, message = "ID de Pokémon inválido" });
                }

                _logger.LogInformation("📧 Iniciando envío de email individual - Pokémon ID: {Id} a {Email}", 
                    pokemonId, model.RecipientEmail);

                // Obtener datos del Pokémon
                var pokemon = await _pokemonService.GetPokemonByIdAsync(pokemonId);
                if (pokemon == null)
                {
                    return Json(new { success = false, message = "Pokémon no encontrado" });
                }

                // Enviar email
                var success = await _emailService.SendPokemonEmailAsync(pokemon, model.RecipientEmail, model.RecipientName);

                stopwatch.Stop();
                if (success)
                {
                    _logger.LogInformation("✅ Email enviado exitosamente: {Name} a {Email} en {ElapsedMs}ms", 
                        pokemon.Name, model.RecipientEmail, stopwatch.ElapsedMilliseconds);

                    return Json(new
                    {
                        success = true,
                        message = $"Email enviado exitosamente con información de {pokemon.Name} a {model.RecipientEmail}"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Error al enviar el email. Revisa la configuración SMTP." });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al enviar email individual");
                return Json(new { success = false, message = $"Error interno: {ex.Message}" });
            }
        }

        /// <summary>
        /// Envía un email con información de múltiples Pokémon seleccionados
        /// CON LOGGING MEJORADO
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMultiplePokemonEmails([FromBody] EmailFormViewModel model)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Datos del formulario inválidos" });
                }

                if (!_emailService.IsConfigurationValid())
                {
                    return Json(new { success = false, message = "Configuración de email no disponible" });
                }

                if (!model.PokemonIds.Any())
                {
                    return Json(new { success = false, message = "No se seleccionaron Pokémon" });
                }

                _logger.LogInformation("📧 Iniciando envío de email masivo - {Count} Pokémon a {Email}", 
                    model.PokemonIds.Count, model.RecipientEmail);

                // Obtener datos de todos los Pokémon seleccionados
                var pokemons = new List<Models.Pokemon.Pokemon>();
                foreach (var id in model.PokemonIds)
                {
                    var pokemon = await _pokemonService.GetPokemonByIdAsync(id);
                    if (pokemon != null)
                    {
                        pokemons.Add(pokemon);
                    }
                }

                if (!pokemons.Any())
                {
                    return Json(new { success = false, message = "No se pudieron obtener los datos de los Pokémon" });
                }

                // Enviar email masivo
                var successCount = await _emailService.SendMultiplePokemonEmailsAsync(pokemons, model.RecipientEmail, model.RecipientName);

                stopwatch.Stop();
                if (successCount > 0)
                {
                    _logger.LogInformation("✅ Email masivo enviado exitosamente: {Count} Pokémon a {Email} en {ElapsedMs}ms", 
                        pokemons.Count, model.RecipientEmail, stopwatch.ElapsedMilliseconds);

                    return Json(new
                    {
                        success = true,
                        message = $"Email enviado exitosamente con información de {pokemons.Count} Pokémon a {model.RecipientEmail}"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Error al enviar el email. Revisa la configuración SMTP." });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al enviar email masivo");
                return Json(new { success = false, message = $"Error interno: {ex.Message}" });
            }
        }

        /// <summary>
        /// Verifica si la configuración de email está disponible
        /// </summary>
        [HttpGet]
        public IActionResult CheckEmailConfiguration()
        {
            var isValid = _emailService.IsConfigurationValid();
            return Json(new { isConfigured = isValid });
        }

        /// <summary>
        /// Endpoint para limpiar todos los caches del servicio
        /// Útil para desarrollo y testing
        /// </summary>
        [HttpPost]
        public IActionResult ClearCache()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                _pokemonService.ClearCache();
                stopwatch.Stop();
                
                _logger.LogInformation("🧹 Cache limpiado por solicitud del usuario en {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                return Json(new
                {
                    success = true,
                    message = "Cache limpiado exitosamente"
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al limpiar el cache");
                return Json(new
                {
                    success = false,
                    message = "Error al limpiar el cache"
                });
            }
        }

        /// <summary>
        /// Endpoint para verificar el estado del servicio
        /// Útil para debugging y monitoreo
        /// </summary>
        [HttpGet]
        public IActionResult ServiceStatus()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var status = new
                {
                    success = true,
                    timestamp = DateTime.UtcNow,
                    cacheInfo = new
                    {
                        message = "Información de cache disponible en logs"
                    }
                };

                stopwatch.Stop();
                return Json(status);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "❌ Error al obtener estado del servicio");
                return Json(new
                {
                    success = false,
                    message = "Error al obtener estado del servicio"
                });
            }
        }
    }
}

// CLASE AUXILIAR PARA LA EXPORTACIÓN DE PÁGINA
public class ExportPageRequest
{
    public int Page { get; set; }
    public string? Name { get; set; }
    public string? TypeName { get; set; }
    public int? MinHeight { get; set; }
    public int? MaxHeight { get; set; }
}
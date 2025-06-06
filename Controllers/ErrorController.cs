using Microsoft.AspNetCore.Mvc;
using PokemonWebApp.Models.ViewModels;

namespace PokemonWebApp.Controllers
{
    /// <summary>
    /// Controlador para manejo de páginas de error personalizadas
    /// Proporciona una experiencia de usuario elegante cuando ocurren errores
    /// </summary>
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Página principal de errores con códigos de estado específicos
        /// </summary>
        /// <param name="statusCode">Código de estado HTTP (404, 500, etc.)</param>
        [Route("Error/{statusCode?}")]
        public IActionResult Index(int? statusCode = null)
        {
            var viewModel = new ErrorViewModel();

            // Configurar mensaje basado en el código de estado
            switch (statusCode)
            {
                case 404:
                    viewModel.Title = "🔍 Página no encontrada";
                    viewModel.Message = "Lo sentimos, la página que buscas no existe.";
                    viewModel.Description = "Es posible que la página haya sido movida, eliminada o que hayas escrito mal la dirección.";
                    viewModel.SuggestedActions = new List<string>
                    {
                        "Verifica la dirección URL",
                        "Regresa a la página principal",
                        "Usa el botón de búsqueda"
                    };
                    viewModel.StatusCode = 404;
                    viewModel.ShowBackButton = true;
                    viewModel.ShowHomeButton = true;
                    break;

                case 500:
                    viewModel.Title = "⚠️ Error interno del servidor";
                    viewModel.Message = "Algo salió mal en nuestro servidor.";
                    viewModel.Description = "Estamos trabajando para solucionar este problema. Por favor, intenta de nuevo en unos minutos.";
                    viewModel.SuggestedActions = new List<string>
                    {
                        "Espera unos minutos e intenta de nuevo",
                        "Verifica tu conexión a internet",
                        "Contacta al soporte si el problema persiste"
                    };
                    viewModel.StatusCode = 500;
                    viewModel.ShowBackButton = true;
                    viewModel.ShowHomeButton = true;
                    viewModel.ShowRetryButton = true;
                    break;

                case 403:
                    viewModel.Title = "🚫 Acceso denegado";
                    viewModel.Message = "No tienes permisos para acceder a este recurso.";
                    viewModel.Description = "Esta página requiere permisos especiales que no posees actualmente.";
                    viewModel.SuggestedActions = new List<string>
                    {
                        "Verifica tus credenciales",
                        "Contacta al administrador",
                        "Regresa a la página principal"
                    };
                    viewModel.StatusCode = 403;
                    viewModel.ShowBackButton = true;
                    viewModel.ShowHomeButton = true;
                    break;

                case 408:
                    viewModel.Title = "⏱️ Tiempo de espera agotado";
                    viewModel.Message = "La solicitud tardó demasiado tiempo en procesarse.";
                    viewModel.Description = "El servidor tardó demasiado en responder. Esto puede deberse a problemas de conectividad.";
                    viewModel.SuggestedActions = new List<string>
                    {
                        "Verifica tu conexión a internet",
                        "Intenta de nuevo en unos momentos",
                        "Reduce el número de filtros aplicados"
                    };
                    viewModel.StatusCode = 408;
                    viewModel.ShowBackButton = true;
                    viewModel.ShowHomeButton = true;
                    viewModel.ShowRetryButton = true;
                    break;

                case 503:
                    viewModel.Title = "🔧 Servicio no disponible";
                    viewModel.Message = "El servicio está temporalmente no disponible.";
                    viewModel.Description = "Estamos realizando mantenimiento o hay problemas técnicos temporales.";
                    viewModel.SuggestedActions = new List<string>
                    {
                        "Intenta de nuevo en unos minutos",
                        "Verifica nuestro estado de servicio",
                        "Contacta al soporte si es urgente"
                    };
                    viewModel.StatusCode = 503;
                    viewModel.ShowBackButton = true;
                    viewModel.ShowHomeButton = true;
                    viewModel.ShowRetryButton = true;
                    break;

                default:
                    viewModel.Title = "❌ Error inesperado";
                    viewModel.Message = "Ha ocurrido un error inesperado.";
                    viewModel.Description = "Algo no funcionó como esperábamos. Nuestro equipo ha sido notificado.";
                    viewModel.SuggestedActions = new List<string>
                    {
                        "Intenta recargar la página",
                        "Regresa a la página principal",
                        "Contacta al soporte si el problema persiste"
                    };
                    viewModel.StatusCode = statusCode ?? 500;
                    viewModel.ShowBackButton = true;
                    viewModel.ShowHomeButton = true;
                    viewModel.ShowRetryButton = true;
                    break;
            }

            // Log del error para seguimiento
            _logger.LogWarning("🎯 Página de error mostrada | StatusCode: {StatusCode} | Title: {Title}", 
                viewModel.StatusCode, viewModel.Title);

            return View(viewModel);
        }

        /// <summary>
        /// Página de error para problemas de API externa
        /// </summary>
        [Route("Error/ApiUnavailable")]
        public IActionResult ApiUnavailable()
        {
            var viewModel = new ErrorViewModel
            {
                Title = "🌐 API no disponible",
                Message = "La API de Pokémon no está disponible en este momento.",
                Description = "Estamos experimentando problemas de conectividad con el servicio externo de datos de Pokémon.",
                SuggestedActions = new List<string>
                {
                    "Intenta recargar la página en unos minutos",
                    "Verifica tu conexión a internet",
                    "Los datos en cache pueden estar disponibles"
                },
                StatusCode = 503,
                ShowBackButton = true,
                ShowHomeButton = true,
                ShowRetryButton = true
            };

            _logger.LogWarning("🌐 API no disponible - Página de error mostrada");

            return View("Index", viewModel);
        }

        /// <summary>
        /// Página de error para problemas de configuración
        /// </summary>
        [Route("Error/Configuration")]
        public IActionResult Configuration()
        {
            var viewModel = new ErrorViewModel
            {
                Title = "⚙️ Error de configuración",
                Message = "Hay un problema con la configuración de la aplicación.",
                Description = "Algunos servicios no están configurados correctamente. Contacta al administrador del sistema.",
                SuggestedActions = new List<string>
                {
                    "Contacta al administrador del sistema",
                    "Verifica la configuración de la aplicación",
                    "Intenta de nuevo más tarde"
                },
                StatusCode = 500,
                ShowBackButton = false,
                ShowHomeButton = true,
                ShowRetryButton = false
            };

            _logger.LogError("⚙️ Error de configuración - Página de error mostrada");

            return View("Index", viewModel);
        }

        /// <summary>
        /// Endpoint AJAX para reportar errores del cliente
        /// </summary>
        [HttpPost]
        [Route("Error/ReportClientError")]
        public IActionResult ReportClientError([FromBody] ClientErrorReport report)
        {
            if (report == null)
            {
                return BadRequest("Reporte de error inválido");
            }

            _logger.LogError("🖥️ ERROR DEL CLIENTE | URL: {Url} | Error: {ErrorMessage} | UserAgent: {UserAgent} | Timestamp: {Timestamp}",
                report.Url, report.ErrorMessage, report.UserAgent, report.Timestamp);

            return Json(new { success = true, message = "Error reportado exitosamente" });
        }
    }

    /// <summary>
    /// Modelo para reportes de errores del cliente (JavaScript)
    /// </summary>
    public class ClientErrorReport
    {
        public string Url { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? StackTrace { get; set; }
    }
}
namespace PokemonWebApp.Models.ViewModels
{
    /// <summary>
    /// ViewModel para páginas de error personalizadas
    /// Proporciona información estructurada para mostrar errores elegantes
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Título principal del error (ej: "Página no encontrada")
        /// </summary>
        public string Title { get; set; } = "Error";

        /// <summary>
        /// Mensaje principal del error
        /// </summary>
        public string Message { get; set; } = "Ha ocurrido un error inesperado.";

        /// <summary>
        /// Descripción detallada del error
        /// </summary>
        public string Description { get; set; } = "Por favor, intenta de nuevo más tarde.";

        /// <summary>
        /// Lista de acciones sugeridas para el usuario
        /// </summary>
        public List<string> SuggestedActions { get; set; } = new List<string>();

        /// <summary>
        /// Código de estado HTTP del error
        /// </summary>
        public int StatusCode { get; set; } = 500;

        /// <summary>
        /// Indica si mostrar el botón de "Volver atrás"
        /// </summary>
        public bool ShowBackButton { get; set; } = true;

        /// <summary>
        /// Indica si mostrar el botón de "Ir al inicio"
        /// </summary>
        public bool ShowHomeButton { get; set; } = true;

        /// <summary>
        /// Indica si mostrar el botón de "Reintentar"
        /// </summary>
        public bool ShowRetryButton { get; set; } = false;

        /// <summary>
        /// ID de correlación para tracking (opcional)
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Timestamp del error
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Información adicional para desarrollo (solo en modo debug)
        /// </summary>
        public string? TechnicalDetails { get; set; }

        /// <summary>
        /// Obtiene el icono apropiado basado en el código de estado
        /// </summary>
        public string GetStatusIcon()
        {
            return StatusCode switch
            {
                404 => "🔍",
                403 => "🚫",
                408 => "⏱️",
                500 => "⚠️",
                503 => "🔧",
                _ => "❌"
            };
        }

        /// <summary>
        /// Obtiene la clase CSS de color basada en el código de estado
        /// </summary>
        public string GetStatusColorClass()
        {
            return StatusCode switch
            {
                404 => "text-info",
                403 => "text-danger",
                408 => "text-warning",
                500 => "text-danger",
                503 => "text-warning",
                _ => "text-secondary"
            };
        }

        /// <summary>
        /// Obtiene la clase CSS de fondo basada en el código de estado
        /// </summary>
        public string GetStatusBackgroundClass()
        {
            return StatusCode switch
            {
                404 => "bg-info-subtle",
                403 => "bg-danger-subtle",
                408 => "bg-warning-subtle",
                500 => "bg-danger-subtle",
                503 => "bg-warning-subtle",
                _ => "bg-secondary-subtle"
            };
        }
    }
}
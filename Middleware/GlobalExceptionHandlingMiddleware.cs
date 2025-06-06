using System.Net;
using System.Text.Json;
using Serilog;

namespace PokemonWebApp.Middleware
{
    /// <summary>
    /// Middleware para manejo centralizado de excepciones
    /// Captura todas las excepciones no manejadas y las procesa elegantemente
    /// SIMPLIFICADO PARA EVITAR PROBLEMAS DE INYECCIÓN DE DEPENDENCIAS
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log detallado de la excepción usando Serilog directamente
            var correlationId = Guid.NewGuid().ToString();
            var requestPath = context.Request.Path.Value ?? "Unknown";
            var requestMethod = context.Request.Method;
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            Log.Error(exception, 
                "🚨 EXCEPCIÓN NO MANEJADA | CorrelationId: {CorrelationId} | Path: {RequestPath} | Method: {RequestMethod} | IP: {ClientIp} | UserAgent: {UserAgent}",
                correlationId, requestPath, requestMethod, clientIp, userAgent);

            // Configurar respuesta HTTP
            context.Response.ContentType = "application/json";
            
            var (statusCode, message, details) = GetErrorResponse(exception, context);
            context.Response.StatusCode = (int)statusCode;

            // Crear respuesta de error
            var errorResponse = new ErrorResponse
            {
                CorrelationId = correlationId,
                Message = message,
                Details = IsProduction(context) ? null : details,
                Timestamp = DateTime.UtcNow,
                Path = requestPath,
                StatusCode = (int)statusCode
            };

            // Serializar y enviar respuesta
            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);

            // Log adicional para métricas
            LogErrorMetrics(exception, statusCode, requestPath, requestMethod);
        }

        /// <summary>
        /// Determina el código de estado y mensaje basado en el tipo de excepción
        /// </summary>
        private (HttpStatusCode statusCode, string message, string details) GetErrorResponse(Exception exception, HttpContext context)
        {
            var isDevelopment = !IsProduction(context);
            
            return exception switch
            {
                ArgumentNullException => (
                    HttpStatusCode.BadRequest, 
                    "Parámetro requerido no proporcionado", 
                    exception.Message),
                
                ArgumentException => (
                    HttpStatusCode.BadRequest, 
                    "Parámetro inválido", 
                    exception.Message),
                
                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized, 
                    "Acceso no autorizado", 
                    exception.Message),
                
                FileNotFoundException => (
                    HttpStatusCode.NotFound, 
                    "Recurso no encontrado", 
                    exception.Message),
                
                HttpRequestException => (
                    HttpStatusCode.ServiceUnavailable, 
                    "Servicio externo no disponible", 
                    exception.Message),
                
                TaskCanceledException => (
                    HttpStatusCode.RequestTimeout, 
                    "La solicitud tardó demasiado tiempo", 
                    "Timeout del servidor"),
                
                JsonException => (
                    HttpStatusCode.BadRequest, 
                    "Formato de datos inválido", 
                    exception.Message),
                
                InvalidOperationException => (
                    HttpStatusCode.Conflict, 
                    "Operación no válida en el estado actual", 
                    exception.Message),
                
                NotSupportedException => (
                    HttpStatusCode.NotImplemented, 
                    "Operación no soportada", 
                    exception.Message),
                
                _ => (
                    HttpStatusCode.InternalServerError, 
                    "Error interno del servidor", 
                    isDevelopment ? exception.ToString() : "Un error inesperado ocurrió")
            };
        }

        /// <summary>
        /// Log adicional para métricas y monitoreo
        /// </summary>
        private void LogErrorMetrics(Exception exception, HttpStatusCode statusCode, string path, string method)
        {
            var errorType = exception.GetType().Name;
            var severity = GetErrorSeverity(statusCode);

            Log.Warning(
                "📊 ERROR METRICS | Type: {ErrorType} | Severity: {Severity} | StatusCode: {StatusCode} | Path: {Path} | Method: {Method}",
                errorType, severity, (int)statusCode, path, method);
        }

        /// <summary>
        /// Determina la severidad del error basada en el código de estado
        /// </summary>
        private string GetErrorSeverity(HttpStatusCode statusCode)
        {
            return (int)statusCode switch
            {
                >= 500 => "CRITICAL",
                >= 400 and < 500 => "WARNING",
                _ => "INFO"
            };
        }

        /// <summary>
        /// Verifica si estamos en producción
        /// </summary>
        private bool IsProduction(HttpContext context)
        {
            var environment = context.RequestServices.GetService<IWebHostEnvironment>();
            return environment?.IsProduction() ?? false;
        }
    }

    /// <summary>
    /// Modelo para respuestas de error estructuradas
    /// </summary>
    public class ErrorResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }
}
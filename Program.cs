using PokemonWebApp.Services;
using PokemonWebApp.Models;
using PokemonWebApp.Middleware;
using Serilog;
using Serilog.Events;

// Configurar Serilog ANTES de crear la aplicación
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/pokemon-app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog como el proveedor de logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar HttpClient para consumir la API de Pokémon
builder.Services.AddHttpClient<IPokemonService, PokemonService>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar servicios de la aplicación
builder.Services.AddScoped<IExcelService, ExcelService>();

// Configurar y registrar servicio de Email
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Configurar cache en memoria
builder.Services.AddMemoryCache();

// NO registrar el middleware como servicio - se usa directamente
// builder.Services.AddScoped<GlobalExceptionHandlingMiddleware>(); ← REMOVER ESTA LÍNEA

var app = builder.Build();

// Usar Serilog para logging de requests HTTP
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "🌐 HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null 
        ? LogEventLevel.Error 
        : httpContext.Response.StatusCode > 499 
            ? LogEventLevel.Error 
            : LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown");
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");
    };
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Usar nuestro middleware personalizado de manejo de errores
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    app.UseHsts();
}
else
{
    // En desarrollo, también usar nuestro middleware pero con más detalle
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
}

// Configurar manejo de códigos de estado personalizados
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Configurar rutas personalizadas para páginas de error
app.MapControllerRoute(
    name: "error",
    pattern: "Error/{statusCode?}",
    defaults: new { controller = "Error", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pokemon}/{action=Index}/{id?}");

// Log de inicio de aplicación
Log.Information("🚀 PokemonWebApp iniciada en {Environment} en {MachineName}", 
    app.Environment.EnvironmentName, Environment.MachineName);

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💀 La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
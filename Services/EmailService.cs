using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using PokemonWebApp.Models;
using PokemonWebApp.Models.Pokemon;

namespace PokemonWebApp.Services
{
    /// <summary>
    /// Servicio para envío de emails con información de Pokémon
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Envía un email con información de un Pokémon individual
        /// </summary>
        public async Task<bool> SendPokemonEmailAsync(Pokemon pokemon, string recipientEmail, string recipientName = "")
        {
            try
            {
                if (!IsConfigurationValid())
                {
                    _logger.LogError("Configuración SMTP inválida");
                    return false;
                }

                var subject = $"Información del Pokémon: {pokemon.Name}";
                var htmlBody = GeneratePokemonEmailTemplate(pokemon, recipientName);

                return await SendEmailAsync(recipientEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email individual para Pokémon {PokemonName}", pokemon.Name);
                return false;
            }
        }

        /// <summary>
        /// Envía emails masivos con información de múltiples Pokémon
        /// </summary>
        public async Task<int> SendMultiplePokemonEmailsAsync(List<Pokemon> pokemons, string recipientEmail, string recipientName = "")
        {
            var successCount = 0;

            if (!IsConfigurationValid())
            {
                _logger.LogError("Configuración SMTP inválida para envío masivo");
                return 0;
            }

            // Enviar un email con todos los Pokémon seleccionados
            try
            {
                var subject = $"Información de {pokemons.Count} Pokémon seleccionados";
                var htmlBody = GenerateMultiplePokemonEmailTemplate(pokemons, recipientName);

                var success = await SendEmailAsync(recipientEmail, subject, htmlBody);
                if (success)
                {
                    successCount = 1;
                    _logger.LogInformation("Email masivo enviado exitosamente con {Count} Pokémon", pokemons.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email masivo con {Count} Pokémon", pokemons.Count);
            }

            return successCount;
        }

        /// <summary>
        /// Verifica si la configuración SMTP es válida
        /// </summary>
        public bool IsConfigurationValid()
        {
            return !string.IsNullOrEmpty(_emailSettings.SmtpServer) &&
                   _emailSettings.SmtpPort > 0 &&
                   !string.IsNullOrEmpty(_emailSettings.SmtpUsername) &&
                   !string.IsNullOrEmpty(_emailSettings.SmtpPassword) &&
                   !string.IsNullOrEmpty(_emailSettings.FromEmail);
        }

        /// <summary>
        /// Método privado para enviar el email usando SMTP
        /// </summary>
        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                smtpClient.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                smtpClient.EnableSsl = _emailSettings.EnableSsl;

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = htmlBody;
                mailMessage.IsBodyHtml = true;

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email enviado exitosamente a {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// Genera el template HTML para un Pokémon individual
        /// </summary>
        private string GeneratePokemonEmailTemplate(Pokemon pokemon, string recipientName)
        {
            var typeColors = GetTypeColor(pokemon.Types?.FirstOrDefault()?.Type?.Name ?? "normal");
            
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='es'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Información Pokémon</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
            html.AppendLine("        .container { max-width: 600px; margin: 0 auto; background: white; border-radius: 15px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1); }");
            html.AppendLine($"        .header {{ background: linear-gradient(135deg, {typeColors.primary}, {typeColors.secondary}); color: white; padding: 30px; text-align: center; }}");
            html.AppendLine("        .content { padding: 30px; }");
            html.AppendLine("        .pokemon-image { text-align: center; margin: 20px 0; }");
            html.AppendLine("        .pokemon-image img { max-width: 200px; height: auto; }");
            html.AppendLine("        .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin: 20px 0; }");
            html.AppendLine("        .info-item { background: #f8f9fa; padding: 15px; border-radius: 8px; }");
            html.AppendLine("        .info-label { font-weight: bold; color: #666; margin-bottom: 5px; }");
            html.AppendLine("        .info-value { font-size: 18px; color: #333; }");
            html.AppendLine("        .types { text-align: center; margin: 20px 0; }");
            html.AppendLine("        .type-badge { display: inline-block; padding: 5px 15px; margin: 0 5px; border-radius: 20px; color: white; font-weight: bold; text-transform: capitalize; }");
            html.AppendLine("        .footer { background: #f8f9fa; padding: 20px; text-align: center; color: #666; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container'>");
            html.AppendLine("        <div class='header'>");
            html.AppendLine($"            <h1>¡Hola{(!string.IsNullOrEmpty(recipientName) ? $" {recipientName}" : "")}!</h1>");
            html.AppendLine($"            <h2>Información del Pokémon: {pokemon.Name?.ToUpperInvariant()}</h2>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='content'>");
            
            if (!string.IsNullOrEmpty(pokemon.Sprites?.FrontDefault))
            {
                html.AppendLine("            <div class='pokemon-image'>");
                html.AppendLine($"                <img src='{pokemon.Sprites.FrontDefault}' alt='{pokemon.Name}' />");
                html.AppendLine("            </div>");
            }

            html.AppendLine("            <div class='info-grid'>");
            html.AppendLine("                <div class='info-item'>");
            html.AppendLine("                    <div class='info-label'>ID</div>");
            html.AppendLine($"                    <div class='info-value'>#{pokemon.Id:000}</div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class='info-item'>");
            html.AppendLine("                    <div class='info-label'>Altura</div>");
            html.AppendLine($"                    <div class='info-value'>{pokemon.Height / 10.0:F1} m</div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class='info-item'>");
            html.AppendLine("                    <div class='info-label'>Peso</div>");
            html.AppendLine($"                    <div class='info-value'>{pokemon.Weight / 10.0:F1} kg</div>");
            html.AppendLine("                </div>");
            html.AppendLine("                <div class='info-item'>");
            html.AppendLine("                    <div class='info-label'>Experiencia Base</div>");
            html.AppendLine($"                    <div class='info-value'>{pokemon.BaseExperience}</div>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");

            if (pokemon.Types?.Any() == true)
            {
                html.AppendLine("            <div class='types'>");
                html.AppendLine("                <h3>Tipos:</h3>");
                foreach (var type in pokemon.Types)
                {
                    var colors = GetTypeColor(type.Type?.Name ?? "normal");
                    html.AppendLine($"                <span class='type-badge' style='background: {colors.primary};'>{type.Type?.Name}</span>");
                }
                html.AppendLine("            </div>");
            }

            html.AppendLine("        </div>");
            html.AppendLine("        <div class='footer'>");
            html.AppendLine("            <p>Email generado por PokemonWebApp</p>");
            html.AppendLine($"            <p>Enviado el {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// Genera el template HTML para múltiples Pokémon
        /// </summary>
        private string GenerateMultiplePokemonEmailTemplate(List<Pokemon> pokemons, string recipientName)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='es'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Múltiples Pokémon</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }");
            html.AppendLine("        .container { max-width: 800px; margin: 0 auto; background: white; border-radius: 15px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1); }");
            html.AppendLine("        .header { background: linear-gradient(135deg, #667eea, #764ba2); color: white; padding: 30px; text-align: center; }");
            html.AppendLine("        .content { padding: 30px; }");
            html.AppendLine("        .pokemon-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 20px; }");
            html.AppendLine("        .pokemon-card { border: 1px solid #ddd; border-radius: 10px; padding: 15px; background: #f8f9fa; }");
            html.AppendLine("        .pokemon-image { text-align: center; margin-bottom: 10px; }");
            html.AppendLine("        .pokemon-image img { max-width: 80px; height: auto; }");
            html.AppendLine("        .pokemon-name { font-size: 18px; font-weight: bold; text-align: center; margin-bottom: 10px; text-transform: capitalize; }");
            html.AppendLine("        .pokemon-info { font-size: 12px; color: #666; }");
            html.AppendLine("        .type-badge { display: inline-block; padding: 2px 8px; margin: 2px; border-radius: 12px; color: white; font-size: 10px; font-weight: bold; text-transform: capitalize; }");
            html.AppendLine("        .footer { background: #f8f9fa; padding: 20px; text-align: center; color: #666; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container'>");
            html.AppendLine("        <div class='header'>");
            html.AppendLine($"            <h1>¡Hola{(!string.IsNullOrEmpty(recipientName) ? $" {recipientName}" : "")}!</h1>");
            html.AppendLine($"            <h2>Información de {pokemons.Count} Pokémon Seleccionados</h2>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='content'>");
            html.AppendLine("            <div class='pokemon-grid'>");

            foreach (var pokemon in pokemons)
            {
                html.AppendLine("                <div class='pokemon-card'>");
                
                if (!string.IsNullOrEmpty(pokemon.Sprites?.FrontDefault))
                {
                    html.AppendLine("                    <div class='pokemon-image'>");
                    html.AppendLine($"                        <img src='{pokemon.Sprites.FrontDefault}' alt='{pokemon.Name}' />");
                    html.AppendLine("                    </div>");
                }

                html.AppendLine($"                    <div class='pokemon-name'>{pokemon.Name}</div>");
                html.AppendLine("                    <div class='pokemon-info'>");
                html.AppendLine($"                        <strong>ID:</strong> #{pokemon.Id:000}<br>");
                html.AppendLine($"                        <strong>Altura:</strong> {pokemon.Height / 10.0:F1} m<br>");
                html.AppendLine($"                        <strong>Peso:</strong> {pokemon.Weight / 10.0:F1} kg<br>");
                
                if (pokemon.Types?.Any() == true)
                {
                    html.AppendLine("                        <strong>Tipos:</strong><br>");
                    foreach (var type in pokemon.Types)
                    {
                        var colors = GetTypeColor(type.Type?.Name ?? "normal");
                        html.AppendLine($"                        <span class='type-badge' style='background: {colors.primary};'>{type.Type?.Name}</span>");
                    }
                }

                html.AppendLine("                    </div>");
                html.AppendLine("                </div>");
            }

            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class='footer'>");
            html.AppendLine("            <p>Email generado por PokemonWebApp</p>");
            html.AppendLine($"            <p>Enviado el {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// Obtiene los colores asociados a cada tipo de Pokémon
        /// </summary>
        private (string primary, string secondary) GetTypeColor(string typeName)
        {
            return typeName?.ToLower() switch
            {
                "fire" => ("#FF6B35", "#FF8E53"),
                "water" => ("#4A90E2", "#6FA8F5"),
                "grass" => ("#7AC142", "#8FD255"),
                "electric" => ("#F7D358", "#F9E579"),
                "psychic" => ("#F85888", "#FA7FA7"),
                "ice" => ("#96D9D6", "#B2E5E2"),
                "dragon" => ("#7038F8", "#8E59F9"),
                "dark" => ("#705848", "#8B6F47"),
                "fairy" => ("#EE99AC", "#F2B3C1"),
                "normal" => ("#A8A878", "#B8B888"),
                "fighting" => ("#C03028", "#D0504A"),
                "poison" => ("#A040A0", "#B159B1"),
                "ground" => ("#E0C068", "#E6D080"),
                "flying" => ("#A890F0", "#BBA7F3"),
                "bug" => ("#A8B820", "#B8C838"),
                "rock" => ("#B8A038", "#C8B050"),
                "ghost" => ("#705898", "#8270AB"),
                "steel" => ("#B8B8D0", "#C8C8D8"),
                _ => ("#68A090", "#78B0A0")
            };
        }
    }
}
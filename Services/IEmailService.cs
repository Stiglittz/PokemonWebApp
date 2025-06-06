using PokemonWebApp.Models.Pokemon;

namespace PokemonWebApp.Services
{
    /// <summary>
    /// Interfaz para el servicio de envío de emails
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía un email con información de un Pokémon individual
        /// </summary>
        /// <param name="pokemon">Datos del Pokémon</param>
        /// <param name="recipientEmail">Email del destinatario</param>
        /// <param name="recipientName">Nombre del destinatario (opcional)</param>
        /// <returns>True si el envío fue exitoso</returns>
        Task<bool> SendPokemonEmailAsync(Pokemon pokemon, string recipientEmail, string recipientName = "");

        /// <summary>
        /// Envía emails masivos con información de múltiples Pokémon
        /// </summary>
        /// <param name="pokemons">Lista de Pokémon</param>
        /// <param name="recipientEmail">Email del destinatario</param>
        /// <param name="recipientName">Nombre del destinatario (opcional)</param>
        /// <returns>Número de emails enviados exitosamente</returns>
        Task<int> SendMultiplePokemonEmailsAsync(List<Pokemon> pokemons, string recipientEmail, string recipientName = "");

        /// <summary>
        /// Verifica si la configuración SMTP es válida
        /// </summary>
        /// <returns>True si la configuración es válida</returns>
        bool IsConfigurationValid();
    }
}
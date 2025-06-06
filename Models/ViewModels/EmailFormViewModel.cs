using System.ComponentModel.DataAnnotations;

namespace PokemonWebApp.Models.ViewModels
{
    /// <summary>
    /// ViewModel para el formulario de envío de emails
    /// </summary>
    public class EmailFormViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Email del destinatario")]
        public string RecipientEmail { get; set; } = string.Empty;

        [Display(Name = "Nombre del destinatario (opcional)")]
        public string RecipientName { get; set; } = string.Empty;

        [Display(Name = "IDs de Pokémon seleccionados")]
        public List<int> PokemonIds { get; set; } = new List<int>();

        [Display(Name = "Tipo de envío")]
        public EmailType EmailType { get; set; }
    }

    /// <summary>
    /// Tipos de envío de email disponibles
    /// </summary>
    public enum EmailType
    {
        Single,     // Un solo Pokémon
        Multiple    // Múltiples Pokémon
    }
}
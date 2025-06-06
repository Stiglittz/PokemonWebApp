using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    /// <summary>
    /// Representa las imágenes de un Pokémon
    /// Expandido para incluir imágenes oficiales de alta calidad
    /// </summary>
    public class PokemonSprites
    {
        [JsonProperty("front_default")]
        public string? FrontDefault { get; set; }

        [JsonProperty("front_shiny")]
        public string? FrontShiny { get; set; }

        [JsonProperty("back_default")]
        public string? BackDefault { get; set; }

        [JsonProperty("back_shiny")]
        public string? BackShiny { get; set; }

        /// <summary>
        /// Sprites adicionales de mejor calidad
        /// </summary>
        [JsonProperty("other")]
        public OtherSprites? Other { get; set; }
    }

    /// <summary>
    /// Sprites adicionales de otras fuentes
    /// </summary>
    public class OtherSprites
    {
        /// <summary>
        /// Artwork oficial de alta calidad
        /// </summary>
        [JsonProperty("official-artwork")]
        public OfficialArtwork? OfficialArtwork { get; set; }

        /// <summary>
        /// Sprites del juego Home
        /// </summary>
        [JsonProperty("home")]
        public HomeSprites? Home { get; set; }
    }

    /// <summary>
    /// Artwork oficial de alta calidad
    /// </summary>
    public class OfficialArtwork
    {
        [JsonProperty("front_default")]
        public string? FrontDefault { get; set; }

        [JsonProperty("front_shiny")]
        public string? FrontShiny { get; set; }
    }

    /// <summary>
    /// Sprites del Pokémon Home
    /// </summary>
    public class HomeSprites
    {
        [JsonProperty("front_default")]
        public string? FrontDefault { get; set; }

        [JsonProperty("front_shiny")]
        public string? FrontShiny { get; set; }
    }
}
using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    /// <summary>
    /// Referencia a la especie del Pokémon
    /// </summary>
    public class PokemonSpeciesReference
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }
}
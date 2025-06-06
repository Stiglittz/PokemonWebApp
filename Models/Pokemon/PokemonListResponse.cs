using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    /// <summary>
    /// Respuesta de la API cuando pedimos una lista de Pokémon
    /// </summary>
    public class PokemonListResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public string? Next { get; set; }

        [JsonProperty("previous")]
        public string? Previous { get; set; }

        [JsonProperty("results")]
        public List<PokemonListItem> Results { get; set; } = new();
    }

    /// <summary>
    /// Elemento individual en la lista de Pokémon
    /// </summary>
    public class PokemonListItem
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        // Propiedad calculada para extraer el ID de la URL
        public int Id
        {
            get
            {
                var segments = Url.TrimEnd('/').Split('/');
                return int.TryParse(segments.Last(), out var id) ? id : 0;
            }
        }
    }
}
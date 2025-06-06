using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    public class PokemonTypeResponse
    {
        [JsonProperty("pokemon")]
        public List<PokemonTypeSlot> Pokemon { get; set; } = new();
    }

    public class PokemonTypeSlot
    {
        [JsonProperty("pokemon")]
        public PokemonReference Pokemon { get; set; } = new();
    }

    public class PokemonReference
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        public int Id => ExtractIdFromUrl(Url);

        private int ExtractIdFromUrl(string url)
        {
            var segments = url.TrimEnd('/').Split('/');
            return int.TryParse(segments.Last(), out int id) ? id : 0;
        }
    }
}
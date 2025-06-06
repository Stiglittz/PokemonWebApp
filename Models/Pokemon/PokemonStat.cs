using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    /// <summary>
    /// Representa una estadística de un Pokémon (HP, Attack, Defense, etc.)
    /// </summary>
    public class PokemonStat
    {
        [JsonProperty("base_stat")]
        public int BaseStat { get; set; }

        [JsonProperty("effort")]
        public int Effort { get; set; }

        [JsonProperty("stat")]
        public StatInfo Stat { get; set; } = new();
    }

    /// <summary>
    /// Información de la estadística
    /// </summary>
    public class StatInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }
}
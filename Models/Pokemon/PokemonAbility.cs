using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    /// <summary>
    /// Representa una habilidad de un Pokémon
    /// </summary>
    public class PokemonAbility
    {
        [JsonProperty("is_hidden")]
        public bool IsHidden { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("ability")]
        public AbilityInfo Ability { get; set; } = new();
    }

    /// <summary>
    /// Información de la habilidad
    /// </summary>
    public class AbilityInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }
}
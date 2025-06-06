using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    /// <summary>
    /// Modelo que representa un Pokémon obtenido de la PokeAPI
    /// </summary>
    public class Pokemon
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("base_experience")]
        public int BaseExperience { get; set; }

        [JsonProperty("sprites")]
        public PokemonSprites Sprites { get; set; } = new();

        [JsonProperty("types")]
        public List<PokemonType> Types { get; set; } = new();

        [JsonProperty("abilities")]
        public List<PokemonAbility> Abilities { get; set; } = new();

        [JsonProperty("stats")]
        public List<PokemonStat> Stats { get; set; } = new();

        [JsonProperty("species")]
        public PokemonSpeciesReference Species { get; set; } = new();

        // Propiedades calculadas para mostrar en la vista
        public string DisplayName => char.ToUpper(Name[0]) + Name.Substring(1);
        
        public string ImageUrl => Sprites.Other?.OfficialArtwork?.FrontDefault ?? 
                                  Sprites.FrontDefault ?? 
                                  "/images/pokemon-placeholder.png";
        
        public string TypesAsString => string.Join(", ", Types.Select(t => t.Type.Name));

        /// <summary>
        /// Obtiene el color basado en el tipo principal del Pokémon
        /// Retorna string hexadecimal para compatibilidad
        /// </summary>
        public string GetTypeColor()
        {
            if (!Types.Any()) return "#68D391"; // Verde por defecto

            var primaryType = Types.First().Type.Name.ToLower();
            
            return primaryType switch
            {
                "normal" => "#A8A878",
                "fire" => "#F08030",
                "water" => "#6890F0",
                "electric" => "#F8D030",
                "grass" => "#78C850",
                "ice" => "#98D8D8",
                "fighting" => "#C03028",
                "poison" => "#A040A0",
                "ground" => "#E0C068",
                "flying" => "#A890F0",
                "psychic" => "#F85888",
                "bug" => "#A8B820",
                "rock" => "#B8A038",
                "ghost" => "#705898",
                "dragon" => "#7038F8",
                "dark" => "#705848",
                "steel" => "#B8B8D0",
                "fairy" => "#EE99AC",
                _ => "#68D391"
            };
        }
    }
}
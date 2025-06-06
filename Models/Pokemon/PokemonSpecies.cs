using Newtonsoft.Json;

namespace PokemonWebApp.Models.Pokemon
{
    /// <summary>
    /// Modelo COMPLETO para la información de especies de Pokémon desde /pokemon-species/{id}
    /// DIFERENTE de PokemonSpeciesReference que solo tiene name + url
    /// Contiene datos adicionales como descripción, habitat, color, etc.
    /// </summary>
    public class PokemonSpecies
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("color")]
        public PokemonColor Color { get; set; } = new();

        [JsonProperty("habitat")]
        public PokemonHabitat? Habitat { get; set; }

        [JsonProperty("generation")]
        public PokemonGeneration Generation { get; set; } = new();

        [JsonProperty("is_legendary")]
        public bool IsLegendary { get; set; }

        [JsonProperty("is_mythical")]
        public bool IsMythical { get; set; }

        [JsonProperty("capture_rate")]
        public int CaptureRate { get; set; }

        [JsonProperty("base_happiness")]
        public int BaseHappiness { get; set; }

        [JsonProperty("growth_rate")]
        public PokemonGrowthRate GrowthRate { get; set; } = new();

        [JsonProperty("flavor_text_entries")]
        public List<FlavorTextEntry> FlavorTextEntries { get; set; } = new();

        [JsonProperty("genera")]
        public List<Genus> Genera { get; set; } = new();

        /// <summary>
        /// Obtiene la descripción en español, o inglés como fallback
        /// </summary>
        public string GetDescription()
        {
            // Buscar descripción en español primero
            var spanishEntry = FlavorTextEntries
                .FirstOrDefault(x => x.Language.Name == "es");
            
            if (spanishEntry != null)
                return spanishEntry.FlavorText.Replace("\n", " ").Replace("\f", " ").Trim();

            // Fallback a inglés
            var englishEntry = FlavorTextEntries
                .FirstOrDefault(x => x.Language.Name == "en");
            
            return englishEntry?.FlavorText.Replace("\n", " ").Replace("\f", " ").Trim() ?? "Sin descripción disponible";
        }

        /// <summary>
        /// Obtiene la categoría del Pokémon (ej: "Pokémon Semilla")
        /// </summary>
        public string GetCategory()
        {
            var spanishGenus = Genera.FirstOrDefault(x => x.Language.Name == "es");
            if (spanishGenus != null)
                return spanishGenus.GenusText;

            var englishGenus = Genera.FirstOrDefault(x => x.Language.Name == "en");
            return englishGenus?.GenusText ?? "Desconocido";
        }
    }

    /// <summary>
    /// Color principal del Pokémon
    /// </summary>
    public class PokemonColor
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Habitat donde se encuentra el Pokémon
    /// </summary>
    public class PokemonHabitat
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Traduce el habitat al español
        /// </summary>
        public string GetSpanishName()
        {
            return Name switch
            {
                "cave" => "Cueva",
                "forest" => "Bosque",
                "grassland" => "Pradera",
                "mountain" => "Montaña",
                "rare" => "Raro",
                "rough-terrain" => "Terreno Accidentado",
                "sea" => "Mar",
                "urban" => "Urbano",
                "waters-edge" => "Orilla del Agua",
                _ => Name
            };
        }
    }

    /// <summary>
    /// Generación del Pokémon
    /// </summary>
    public class PokemonGeneration
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene el número de generación (ej: "generation-i" -> "I")
        /// </summary>
        public string GetGenerationNumber()
        {
            return Name switch
            {
                "generation-i" => "I",
                "generation-ii" => "II",
                "generation-iii" => "III",
                "generation-iv" => "IV",
                "generation-v" => "V",
                "generation-vi" => "VI",
                "generation-vii" => "VII",
                "generation-viii" => "VIII",
                "generation-ix" => "IX",
                _ => Name.Replace("generation-", "").ToUpper()
            };
        }
    }

    /// <summary>
    /// Velocidad de crecimiento del Pokémon
    /// </summary>
    public class PokemonGrowthRate
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Traduce la velocidad de crecimiento al español
        /// </summary>
        public string GetSpanishName()
        {
            return Name switch
            {
                "slow" => "Lento",
                "medium" => "Medio",
                "fast" => "Rápido",
                "medium-slow" => "Medio-Lento",
                "slow-then-very-fast" => "Lento luego Muy Rápido",
                "fast-then-very-slow" => "Rápido luego Muy Lento",
                _ => Name
            };
        }
    }

    /// <summary>
    /// Entrada de texto descriptivo del Pokémon
    /// </summary>
    public class FlavorTextEntry
    {
        [JsonProperty("flavor_text")]
        public string FlavorText { get; set; } = string.Empty;

        [JsonProperty("language")]
        public LanguageReference Language { get; set; } = new();
    }

    /// <summary>
    /// Categoría/Género del Pokémon (ej: "Pokémon Semilla")
    /// </summary>
    public class Genus
    {
        [JsonProperty("genus")]
        public string GenusText { get; set; } = string.Empty;

        [JsonProperty("language")]
        public LanguageReference Language { get; set; } = new();
    }

    /// <summary>
    /// Referencia a un idioma
    /// </summary>
    public class LanguageReference
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }
}
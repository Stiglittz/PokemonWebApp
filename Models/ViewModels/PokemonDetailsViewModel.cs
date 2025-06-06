using PokemonWebApp.Models.Pokemon;

namespace PokemonWebApp.Models.ViewModels
{
    /// <summary>
    /// ViewModel para el modal de detalles del Pokémon
    /// Compatible con tu estructura de modelos existente
    /// </summary>
    public class PokemonDetailsViewModel
    {
        // Información básica del Pokémon
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Height { get; set; }
        public int Weight { get; set; }
        public int BaseExperience { get; set; }
        public List<PokemonType> Types { get; set; } = new();
        public List<PokemonAbility> Abilities { get; set; } = new();
        public List<PokemonStat> Stats { get; set; } = new();
        public PokemonSprites Sprites { get; set; } = new();

        // Información de especies (opcional, puede ser null si falla la carga)
        public PokemonSpecies? Species { get; set; }

        /// <summary>
        /// Nombre formateado para mostrar (primera letra mayúscula)
        /// </summary>
        public string FormattedName => 
            string.IsNullOrEmpty(Name) ? "Desconocido" : 
            char.ToUpper(Name[0]) + Name.Substring(1).ToLower();

        /// <summary>
        /// Altura formateada en metros (la API devuelve en decímetros)
        /// </summary>
        public string FormattedHeight => $"{Height / 10.0:F1} m";

        /// <summary>
        /// Peso formateado en kg (la API devuelve en hectogramos)
        /// </summary>
        public string FormattedWeight => $"{Weight / 10.0:F1} kg";

        /// <summary>
        /// Lista de tipos como string separado por comas
        /// </summary>
        public string TypesString => string.Join(", ", 
            Types.Select(t => char.ToUpper(t.Type.Name[0]) + t.Type.Name.Substring(1)));

        /// <summary>
        /// Lista de habilidades como string separado por comas
        /// </summary>
        public string AbilitiesString => string.Join(", ", 
            Abilities.Select(a => char.ToUpper(a.Ability.Name[0]) + a.Ability.Name.Substring(1)));

        /// <summary>
        /// URL de la imagen principal (usa artwork oficial si está disponible)
        /// </summary>
        public string MainImageUrl => 
            Sprites.Other?.OfficialArtwork?.FrontDefault ?? 
            Sprites.FrontDefault ?? 
            "/images/pokemon-placeholder.png";

        /// <summary>
        /// URL de la imagen trasera (opcional)
        /// </summary>
        public string? BackImageUrl => Sprites.BackDefault;

        /// <summary>
        /// Indica si el Pokémon es legendario o mítico
        /// </summary>
        public bool IsSpecialPokemon => Species?.IsLegendary == true || Species?.IsMythical == true;

        /// <summary>
        /// Etiqueta para Pokémon especiales
        /// </summary>
        public string? SpecialLabel
        {
            get
            {
                if (Species?.IsLegendary == true) return "Legendario";
                if (Species?.IsMythical == true) return "Mítico";
                return null;
            }
        }

        /// <summary>
        /// Descripción del Pokémon (de especies, o mensaje por defecto)
        /// </summary>
        public string Description => Species?.GetDescription() ?? "Información de especies no disponible";

        /// <summary>
        /// Categoría del Pokémon (ej: "Pokémon Semilla")
        /// </summary>
        public string Category => Species?.GetCategory() ?? "Desconocido";

        /// <summary>
        /// Habitat del Pokémon traducido al español
        /// </summary>
        public string Habitat => Species?.Habitat?.GetSpanishName() ?? "Desconocido";

        /// <summary>
        /// Generación del Pokémon
        /// </summary>
        public string Generation => Species?.Generation?.GetGenerationNumber() ?? "?";

        /// <summary>
        /// Velocidad de crecimiento traducida
        /// </summary>
        public string GrowthRate => Species?.GrowthRate?.GetSpanishName() ?? "Desconocido";

        /// <summary>
        /// Obtiene un stat específico por nombre
        /// </summary>
        /// <param name="statName">Nombre del stat (hp, attack, defense, etc.)</param>
        /// <returns>Valor del stat o 0 si no se encuentra</returns>
        public int GetStatValue(string statName)
        {
            var stat = Stats.FirstOrDefault(s => s.Stat.Name.Equals(statName, StringComparison.OrdinalIgnoreCase));
            return stat?.BaseStat ?? 0;
        }

        /// <summary>
        /// Calcula el total de stats base
        /// </summary>
        public int TotalStats => Stats.Sum(s => s.BaseStat);

        /// <summary>
        /// Obtiene el color CSS basado en el tipo principal del Pokémon
        /// Compatible con tu método existente GetTypeColor()
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
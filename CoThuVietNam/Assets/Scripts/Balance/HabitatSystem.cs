using UnityEngine;
using System.Collections.Generic;

public class HabitatSystem : MonoBehaviour
{
    public static HabitatSystem Instance { get; private set; }

    public enum Habitat
    {
        Sky,    // Flying creatures
        Land,   // Ground-based creatures
        Sea,    // Aquatic creatures
        Hybrid  // Creatures that can exist in multiple domains
    }

    [System.Serializable]
    public class HabitatAdvantage
    {
        public Habitat attacker;
        public Habitat defender;
        public float damageMultiplier = 1.3f;
        public float movementAdvantage = 1.2f;
    }

    [System.Serializable]
    public class AnimalHabitat
    {
        public AnimalType animalType;
        public Habitat primaryHabitat;
        public Habitat[] secondaryHabitats;
        public float[] habitatBonuses; // Stat bonuses when in preferred habitat
    }

    [Header("Habitat Settings")]
    [SerializeField] private HabitatAdvantage[] habitatAdvantages;
    [SerializeField] private AnimalHabitat[] animalHabitats;
    [SerializeField] private float maxHabitatBonus = 0.3f; // 30% max bonus

    private Dictionary<AnimalType, AnimalHabitat> habitatDict;
    private Dictionary<(Habitat, Habitat), HabitatAdvantage> advantageDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        InitializeHabitats();
        InitializeAdvantages();
    }

    private void InitializeHabitats()
    {
        habitatDict = new Dictionary<AnimalType, AnimalHabitat>();

        // Default habitat assignments if none are set
        if (animalHabitats.Length == 0)
        {
            animalHabitats = new AnimalHabitat[]
            {
                // Sky Domain (Flying creatures)
                CreateHabitat(AnimalType.Dragon, Habitat.Sky, new[] { Habitat.Land }),
                CreateHabitat(AnimalType.Phoenix, Habitat.Sky),
                CreateHabitat(AnimalType.Griffin, Habitat.Sky, new[] { Habitat.Land }),
                CreateHabitat(AnimalType.Pegasus, Habitat.Sky),

                // Land Domain (Ground creatures)
                CreateHabitat(AnimalType.Tiger, Habitat.Land),
                CreateHabitat(AnimalType.Lion, Habitat.Land),
                CreateHabitat(AnimalType.Elephant, Habitat.Land),
                CreateHabitat(AnimalType.Wolf, Habitat.Land),
                CreateHabitat(AnimalType.Dog, Habitat.Land),
                CreateHabitat(AnimalType.Fox, Habitat.Land),
                CreateHabitat(AnimalType.Unicorn, Habitat.Land),
                CreateHabitat(AnimalType.Chimera, Habitat.Land, new[] { Habitat.Sky }),

                // Sea Domain (Aquatic creatures)
                CreateHabitat(AnimalType.Kraken, Habitat.Sea),
                CreateHabitat(AnimalType.Mouse, Habitat.Land, new[] { Habitat.Sea }), // Mice can swim
                CreateHabitat(AnimalType.Hydra, Habitat.Sea, new[] { Habitat.Land }),

                // Hybrid Domain (Multi-domain creatures)
                CreateHabitat(AnimalType.Dragon, Habitat.Sky, new[] { Habitat.Land, Habitat.Sea }),
                CreateHabitat(AnimalType.Chimera, Habitat.Land, new[] { Habitat.Sky }),
                CreateHabitat(AnimalType.Hydra, Habitat.Sea, new[] { Habitat.Land })
            };
        }

        foreach (var habitat in animalHabitats)
        {
            habitatDict[habitat.animalType] = habitat;
        }
    }

    private void InitializeAdvantages()
    {
        advantageDict = new Dictionary<(Habitat, Habitat), HabitatAdvantage>();

        // Default habitat advantages if none are set
        if (habitatAdvantages.Length == 0)
        {
            habitatAdvantages = new HabitatAdvantage[]
            {
                // Implement Sky > Land > Sea > Sky relationship
                new HabitatAdvantage { 
                    attacker = Habitat.Sky, 
                    defender = Habitat.Land, 
                    damageMultiplier = 1.3f,
                    movementAdvantage = 1.2f
                },
                new HabitatAdvantage { 
                    attacker = Habitat.Land, 
                    defender = Habitat.Sea, 
                    damageMultiplier = 1.3f,
                    movementAdvantage = 1.2f
                },
                new HabitatAdvantage { 
                    attacker = Habitat.Sea, 
                    defender = Habitat.Sky, 
                    damageMultiplier = 1.3f,
                    movementAdvantage = 1.2f
                }
            };
        }

        foreach (var advantage in habitatAdvantages)
        {
            advantageDict[(advantage.attacker, advantage.defender)] = advantage;
        }
    }

    private AnimalHabitat CreateHabitat(AnimalType type, Habitat primary, Habitat[] secondary = null)
    {
        return new AnimalHabitat
        {
            animalType = type,
            primaryHabitat = primary,
            secondaryHabitats = secondary ?? new Habitat[0],
            habitatBonuses = new float[] { 0.2f, 0.1f } // 20% bonus in primary, 10% in secondary
        };
    }

    public Habitat GetPrimaryHabitat(AnimalType type)
    {
        return habitatDict.TryGetValue(type, out AnimalHabitat habitat) ? habitat.primaryHabitat : Habitat.Land;
    }

    public bool HasHabitatAdvantage(Animal attacker, Animal defender)
    {
        Habitat attackerHabitat = GetPrimaryHabitat(attacker.type);
        Habitat defenderHabitat = GetPrimaryHabitat(defender.type);

        return advantageDict.ContainsKey((attackerHabitat, defenderHabitat));
    }

    public float GetHabitatDamageMultiplier(Animal attacker, Animal defender)
    {
        Habitat attackerHabitat = GetPrimaryHabitat(attacker.type);
        Habitat defenderHabitat = GetPrimaryHabitat(defender.type);

        if (advantageDict.TryGetValue((attackerHabitat, defenderHabitat), out HabitatAdvantage advantage))
        {
            return advantage.damageMultiplier;
        }

        return 1.0f;
    }

    public float GetMovementBonus(Animal animal, Vector2Int position)
    {
        // Get the terrain type at the position from GameBoard
        TerrainType terrain = GameBoard.Instance.GetTerrainType(position);
        Habitat habitatAtPosition = TerrainToHabitat(terrain);

        if (!habitatDict.TryGetValue(animal.type, out AnimalHabitat habitat))
            return 1.0f;

        // Check if this is the animal's primary or secondary habitat
        if (habitat.primaryHabitat == habitatAtPosition)
            return 1.0f + Mathf.Min(habitat.habitatBonuses[0], maxHabitatBonus);
        
        if (System.Array.IndexOf(habitat.secondaryHabitats, habitatAtPosition) != -1)
            return 1.0f + Mathf.Min(habitat.habitatBonuses[1], maxHabitatBonus);

        return 1.0f;
    }

    private Habitat TerrainToHabitat(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Water:
                return Habitat.Sea;
            case TerrainType.Mountain:
                return Habitat.Sky;
            default:
                return Habitat.Land;
        }
    }

    public bool CanMoveInHabitat(Animal animal, Vector2Int position)
    {
        TerrainType terrain = GameBoard.Instance.GetTerrainType(position);
        Habitat habitatAtPosition = TerrainToHabitat(terrain);

        if (!habitatDict.TryGetValue(animal.type, out AnimalHabitat habitat))
            return false;

        // Can always move in primary habitat
        if (habitat.primaryHabitat == habitatAtPosition)
            return true;

        // Check secondary habitats
        return System.Array.IndexOf(habitat.secondaryHabitats, habitatAtPosition) != -1;
    }

    public string GetHabitatDescription(AnimalType type)
    {
        if (!habitatDict.TryGetValue(type, out AnimalHabitat habitat))
            return "No habitat information";

        string description = $"Primary Habitat: {habitat.primaryHabitat}\n";
        
        if (habitat.secondaryHabitats.Length > 0)
        {
            description += "Secondary Habitats: " + string.Join(", ", habitat.secondaryHabitats);
        }

        return description;
    }

    public List<AnimalType> GetAnimalsInHabitat(Habitat habitat)
    {
        List<AnimalType> animals = new List<AnimalType>();
        
        foreach (var pair in habitatDict)
        {
            if (pair.Value.primaryHabitat == habitat || 
                System.Array.IndexOf(pair.Value.secondaryHabitats, habitat) != -1)
            {
                animals.Add(pair.Key);
            }
        }

        return animals;
    }
}
